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
        else:
            # fallback to file handling (unlikely for POST)
            return http.server.SimpleHTTPRequestHandler.do_POST(self)


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
