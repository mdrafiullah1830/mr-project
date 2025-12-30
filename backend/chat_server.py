#!/usr/bin/env python3
"""
Lightweight HTTP server exposing the local site QA assistant via JSON API.

Run:
  python3 chat_server.py 8000

Then open: http://localhost:8000/chat.html

Endpoints:
  POST /api/query  -> JSON {"question": "..."}  returns JSON {results: [{path, score, snippet}]}
  POST /api/reindex -> triggers reindexing (no body)

This server serves static files from the project directory and adds the API.
"""

import http.server
import socketserver
import json
import urllib
import sys
import threading
from io import BytesIO
import cgi
import os
from datetime import datetime

# import the index building/query functions from cjhatwithus.py
try:
    import chatwithus as cw
except Exception:
    # fallback: try to import by path
    import importlib.util, os
    spec = importlib.util.spec_from_file_location('chatwithus', os.path.join(os.path.dirname(__file__), 'chatwithus.py'))
    chatwithus = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(chatwithus)
    cw = chatwithus

PORT = 8000

class Handler(http.server.SimpleHTTPRequestHandler):
    server_version = 'ChatServer/0.1'

    def _set_json_response(self, code=200):
        self.send_response(code)
        self.send_header('Content-type', 'application/json; charset=utf-8')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'POST, GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()

    def do_POST(self):
        # Admin product upload (multipart) -> save image to ./assets/images and create product entry
        if self.path == '/api/admin/product':
            try:
                env = {'REQUEST_METHOD': 'POST', 'CONTENT_TYPE': self.headers.get('Content-Type'), 'CONTENT_LENGTH': self.headers.get('Content-Length')}
                fs = cgi.FieldStorage(fp=self.rfile, headers=self.headers, environ=env)
            except Exception as e:
                self._set_json_response(400)
                self.wfile.write(json.dumps({'error': 'failed to parse upload', 'detail': str(e)}).encode('utf-8'))
                return

            proj_root = os.path.dirname(os.path.dirname(__file__)) if os.path.basename(os.path.dirname(__file__)) == 'backend' else os.path.dirname(__file__)
            assets_dir = os.path.join(proj_root, 'assets', 'images')
            os.makedirs(assets_dir, exist_ok=True)

            # gather fields
            product = {'id': None, 'name': '', 'price': 0, 'description': '', 'category': '', 'image': ''}
            for key in fs.keys() if hasattr(fs, 'keys') else []:
                field = fs[key]
                if not field:
                    continue
                if field.filename:
                    filename = os.path.basename(field.filename)
                    safe_name = filename
                    out_path = os.path.join(assets_dir, safe_name)
                    # if exists, append timestamp
                    if os.path.exists(out_path):
                        ts = datetime.utcnow().strftime('%Y%m%d%H%M%S')
                        out_path = os.path.join(assets_dir, f"{ts}_{filename}")
                    try:
                        with open(out_path, 'wb') as out_f:
                            data = field.file.read()
                            out_f.write(data)
                        product['image'] = os.path.relpath(out_path, proj_root).replace('\\', '/')
                    except Exception as e:
                        print('Failed to save uploaded image', e)
                else:
                    val = getattr(field, 'value', '')
                    if key in product:
                        # cast price
                        product[key] = float(val) if key == 'price' and val else val

            # store product in data/products.json
            data_dir = os.path.join(proj_root, 'data')
            os.makedirs(data_dir, exist_ok=True)
            products_file = os.path.join(data_dir, 'products.json')
            try:
                if os.path.exists(products_file):
                    with open(products_file, 'r', encoding='utf-8') as pf:
                        products = json.load(pf)
                else:
                    products = []
            except Exception:
                products = []

            # assign id
            max_id = max((p.get('id', 0) for p in products), default=200)
            product['id'] = int(max_id) + 1
            products.append(product)
            with open(products_file, 'w', encoding='utf-8') as pf:
                json.dump(products, pf, indent=2, ensure_ascii=False)

            self._set_json_response(201)
            self.wfile.write(json.dumps(product).encode('utf-8'))
            return

        if self.path == '/api/query':
            length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(length)
            try:
                data = json.loads(body.decode('utf-8'))
                question = data.get('question', '')
            except Exception:
                question = ''
            results = []
            if question:
                scores = cw.query_index(cw_index, question, top_k=6)
                for score, chunk in scores:
                    results.append({'path': chunk['path'], 'score': float(score), 'snippet': chunk['text']})
            self._set_json_response(200)
            self.wfile.write(json.dumps({'results': results}).encode('utf-8'))
            return
        elif self.path == '/api_reindex':
            # legacy fallback if called without slash
            threading.Thread(target=reindex).start()
            self._set_json_response(200)
            self.wfile.write(json.dumps({'status': 'reindexing'}).encode('utf-8'))
            return
        elif self.path == '/api/reindex':
            # reindex in a background thread
            threading.Thread(target=reindex).start()
            self._set_json_response(200)
            self.wfile.write(json.dumps({'status': 'reindexing'}).encode('utf-8'))
            return
        elif self.path == '/api/report':
            # accept multipart/form-data uploads: fields: message (optional), files...
            try:
                # prepare environ for cgi
                env = {'REQUEST_METHOD': 'POST', 'CONTENT_TYPE': self.headers.get('Content-Type'), 'CONTENT_LENGTH': self.headers.get('Content-Length')}
                fs = cgi.FieldStorage(fp=self.rfile, headers=self.headers, environ=env)
            except Exception as e:
                self._set_json_response(400)
                self.wfile.write(json.dumps({'error': 'failed to parse upload', 'detail': str(e)}).encode('utf-8'))
                return

            saved = []
            message = ''
            reports_dir = os.path.join(os.path.dirname(__file__), 'reports')
            os.makedirs(reports_dir, exist_ok=True)

            # FieldStorage may present single or multiple fields
            for key in fs.keys() if hasattr(fs, 'keys') else []:
                field = fs[key]
                if not field:
                    continue
                if field.filename:
                    # it's a file upload
                    filename = os.path.basename(field.filename)
                    ts = datetime.utcnow().strftime('%Y%m%d%H%M%S')
                    safe_name = f"{ts}_{filename}"
                    out_path = os.path.join(reports_dir, safe_name)
                    try:
                        with open(out_path, 'wb') as out_f:
                            data = field.file.read()
                            out_f.write(data)
                        saved.append({'field': key, 'filename': filename, 'path': os.path.relpath(out_path, os.path.dirname(__file__))})
                    except Exception as e:
                        print('Failed to save upload', e)
                else:
                    # regular form field
                    try:
                        val = field.value
                    except Exception:
                        val = ''
                    if key == 'message':
                        message = val

            # respond with saved file list
            self._set_json_response(200)
            self.wfile.write(json.dumps({'status': 'ok', 'message': message, 'files': saved}).encode('utf-8'))
            return
        # create category via JSON POST
        elif self.path == '/api/admin/category':
            length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(length)
            try:
                data = json.loads(body.decode('utf-8'))
            except Exception:
                self._set_json_response(400)
                self.wfile.write(json.dumps({'error': 'invalid json'}).encode('utf-8'))
                return
            name = data.get('name')
            if not name:
                self._set_json_response(400)
                self.wfile.write(json.dumps({'error': 'missing name'}).encode('utf-8'))
                return
            proj_root = os.path.dirname(os.path.dirname(__file__)) if os.path.basename(os.path.dirname(__file__)) == 'backend' else os.path.dirname(__file__)
            data_dir = os.path.join(proj_root, 'data')
            os.makedirs(data_dir, exist_ok=True)
            cats_file = os.path.join(data_dir, 'categories.json')
            try:
                if os.path.exists(cats_file):
                    with open(cats_file, 'r', encoding='utf-8') as cf:
                        cats = json.load(cf)
                else:
                    cats = []
            except Exception:
                cats = []
            cats.append({'name': name})
            with open(cats_file, 'w', encoding='utf-8') as cf:
                json.dump(cats, cf, indent=2, ensure_ascii=False)
            self._set_json_response(201)
            self.wfile.write(json.dumps({'name': name}).encode('utf-8'))
            return
        else:
            # fallback to file handling (unlikely for POST)
            return http.server.SimpleHTTPRequestHandler.do_POST(self)

    def do_GET(self):
        # list products
        if self.path == '/api/products':
            proj_root = os.path.dirname(os.path.dirname(__file__)) if os.path.basename(os.path.dirname(__file__)) == 'backend' else os.path.dirname(__file__)
            products_file = os.path.join(proj_root, 'data', 'products.json')
            try:
                with open(products_file, 'r', encoding='utf-8') as pf:
                    products = json.load(pf)
            except Exception:
                products = []
            self._set_json_response(200)
            self.wfile.write(json.dumps(products).encode('utf-8'))
            return

        # single product by id: /api/products/<id>
        if self.path.startswith('/api/products/'):
            parts = self.path.split('/')
            pid = parts[-1]
            proj_root = os.path.dirname(os.path.dirname(__file__)) if os.path.basename(os.path.dirname(__file__)) == 'backend' else os.path.dirname(__file__)
            products_file = os.path.join(proj_root, 'data', 'products.json')
            try:
                with open(products_file, 'r', encoding='utf-8') as pf:
                    products = json.load(pf)
            except Exception:
                products = []
            found = None
            for p in products:
                if str(p.get('id')) == pid:
                    found = p; break
            if found:
                self._set_json_response(200)
                self.wfile.write(json.dumps(found).encode('utf-8'))
            else:
                self._set_json_response(404)
                self.wfile.write(json.dumps({'error': 'not found'}).encode('utf-8'))
            return

        # categories list
        if self.path == '/api/categories':
            proj_root = os.path.dirname(os.path.dirname(__file__)) if os.path.basename(os.path.dirname(__file__)) == 'backend' else os.path.dirname(__file__)
            cats_file = os.path.join(proj_root, 'data', 'categories.json')
            try:
                with open(cats_file, 'r', encoding='utf-8') as cf:
                    cats = json.load(cf)
            except Exception:
                cats = []
            self._set_json_response(200)
            self.wfile.write(json.dumps(cats).encode('utf-8'))
            return

        # default serve static
        return http.server.SimpleHTTPRequestHandler.do_GET(self)


def reindex():
    global cw_index
    print('Re-indexing project files...')
    docs = cw.load_documents(cw.ROOT)
    cw_index = cw.build_index(docs)
    print(f'Re-indexed {len(cw_index["chunks"])} chunks')


def run(port):
    global cw_index
    docs = cw.load_documents(cw.ROOT)
    cw_index = cw.build_index(docs)
    print(f'Index built with {len(cw_index["chunks"])} chunks')
    with socketserver.TCPServer(("", port), Handler) as httpd:
        print(f"Serving at http://localhost:{port}")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print('\nShutting down')
            httpd.server_close()

if __name__ == '__main__':
    port = PORT
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except Exception:
            pass
    run(port)
