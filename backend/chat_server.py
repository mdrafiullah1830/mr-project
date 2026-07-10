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
import os
import email.parser
from datetime import datetime
import time
from collections import defaultdict

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

# Allowed origins for CORS
ALLOWED_ORIGINS = [
    'http://localhost:8000',
    'http://localhost:3000',
    'http://127.0.0.1:8000',
    'http://127.0.0.1:3000',
]

# Rate limiting
rate_limit_data = defaultdict(list)
RATE_LIMIT_WINDOW = 60  # seconds
RATE_LIMIT_MAX_REQUESTS = 30  # max requests per window

def check_rate_limit(ip):
    """Check if IP has exceeded rate limit"""
    now = time.time()
    # Remove old entries
    rate_limit_data[ip] = [t for t in rate_limit_data[ip] if now - t < RATE_LIMIT_WINDOW]
    if len(rate_limit_data[ip]) >= RATE_LIMIT_MAX_REQUESTS:
        return False
    rate_limit_data[ip].append(now)
    return True

def sanitize_input(text, max_length=500):
    """Sanitize and validate input text"""
    if not isinstance(text, str):
        return ''
    # Remove null bytes and control characters
    text = ''.join(c for c in text if c.isprintable() or c in '\n\r\t')
    # Truncate to max length
    text = text[:max_length].strip()
    return text

class Handler(http.server.SimpleHTTPRequestHandler):
    server_version = 'ChatServer/0.2'

    def _get_allowed_origin(self):
        """Check if request origin is allowed"""
        origin = self.headers.get('Origin', '')
        if origin in ALLOWED_ORIGINS:
            return origin
        return ALLOWED_ORIGINS[0]  # Default to localhost

    def _set_json_response(self, code=200):
        self.send_response(code)
        self.send_header('Content-type', 'application/json; charset=utf-8')
        self.send_header('Access-Control-Allow-Origin', self._get_allowed_origin())
        self.send_header('Access-Control-Allow-Credentials', 'true')
        self.end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', self._get_allowed_origin())
        self.send_header('Access-Control-Allow-Methods', 'POST, GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.send_header('Access-Control-Allow-Credentials', 'true')
        self.end_headers()

    def do_POST(self):
        # Rate limiting check
        client_ip = self.client_address[0]
        if not check_rate_limit(client_ip):
            self._set_json_response(429)
            self.wfile.write(json.dumps({'error': 'Rate limit exceeded. Please try again later.'}).encode('utf-8'))
            return

        if self.path == '/api/query':
            length = int(self.headers.get('Content-Length', 0))
            # Input size validation
            if length > 1024:  # Max 1KB for query
                self._set_json_response(413)
                self.wfile.write(json.dumps({'error': 'Request too large'}).encode('utf-8'))
                return
            
            body = self.rfile.read(length)
            try:
                data = json.loads(body.decode('utf-8'))
                question = sanitize_input(data.get('question', ''), max_length=500)
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
                content_type = self.headers.get('Content-Type', '')
                content_length = int(self.headers.get('Content-Length', 0))
                
                # Input size validation
                if content_length > 10 * 1024 * 1024:  # Max 10MB
                    self._set_json_response(413)
                    self.wfile.write(json.dumps({'error': 'File too large (max 10MB)'}).encode('utf-8'))
                    return
                
                body = self.rfile.read(content_length)
                
                saved = []
                message = ''
                reports_dir = os.path.join(os.path.dirname(__file__), 'reports')
                os.makedirs(reports_dir, exist_ok=True)
                
                if 'multipart/form-data' in content_type:
                    # Parse multipart form data manually
                    boundary = content_type.split('boundary=')[1].strip()
                    if boundary.startswith('"'):
                        boundary = boundary[1:-1]
                    
                    parts = body.split(('--' + boundary).encode())
                    for part in parts:
                        if part.strip() and b'\r\n\r\n' in part:
                            header_end = part.find(b'\r\n\r\n')
                            headers_raw = part[:header_end].decode('utf-8', errors='replace')
                            data = part[header_end + 4:]
                            if data.endswith(b'\r\n'):
                                data = data[:-2]
                            
                            # Extract name and filename from Content-Disposition
                            name = ''
                            filename = ''
                            for line in headers_raw.split('\r\n'):
                                if 'Content-Disposition' in line:
                                    for item in line.split(';'):
                                        item = item.strip()
                                        if item.startswith('name='):
                                            name = item.split('=', 1)[1].strip('"')
                                        elif item.startswith('filename='):
                                            filename = item.split('=', 1)[1].strip('"')
                            
                            if filename:
                                # Sanitize filename
                                filename = os.path.basename(filename)
                                # Only allow safe characters
                                filename = ''.join(c for c in filename if c.isalnum() or c in '.-_')
                                if not filename:
                                    filename = 'upload'
                                ts = datetime.utcnow().strftime('%Y%m%d%H%M%S')
                                safe_name = f"{ts}_{filename}"
                                out_path = os.path.join(reports_dir, safe_name)
                                try:
                                    with open(out_path, 'wb') as out_f:
                                        out_f.write(data)
                                    saved.append({'field': name, 'filename': filename, 'path': os.path.relpath(out_path, os.path.dirname(__file__))})
                                except Exception as e:
                                    print('Failed to save upload', e)
                            elif name == 'message':
                                message = sanitize_input(data.decode('utf-8', errors='replace'), max_length=1000)
            except Exception as e:
                self._set_json_response(400)
                self.wfile.write(json.dumps({'error': 'failed to parse upload', 'detail': str(e)}).encode('utf-8'))
                return

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
    # Serve static files from the project root (parent of backend/)
    os.chdir(cw.ROOT)
    print(f"Serving static files from: {cw.ROOT}")
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
