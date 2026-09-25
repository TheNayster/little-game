"""Serve the parent control page on this PC only; never exposes game credentials."""
import argparse
import hmac
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
from pathlib import Path
import secrets
import webbrowser

from parent_server import ParentServer, OperationError
from parent_operations import ParentOperations
from parent_portable import ParentPortable, REQUEST_LIMIT
from portable_recovery import FILE_LIMIT
from shared_garden_runtime import ROOT, write


class ParentHTTP(ThreadingHTTPServer):
    daemon_threads = True
    def __init__(self, controller, port=0):
        super().__init__(('127.0.0.1', port), Handler)
        self.controller = controller
        self.operations = ParentOperations(controller)
        self.portable = ParentPortable(controller, self.operations.remember_backup)
        self.token = secrets.token_urlsafe(32)
        self.authority = f'127.0.0.1:{self.server_port}'
        self.origin = 'http://' + self.authority

    @property
    def url(self):
        return self.origin + '/#' + self.token

    def status(self):
        native = self.controller.snapshot()
        native.update(self.operations.snapshot(native))
        native['portable'] = dict(available=True, maxBytes=FILE_LIMIT)
        return native

    def server_close(self):
        self.operations.close()
        super().server_close()


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass  # Do not put the local session capability in access logs.

    def reply(self, code, value, content_type='application/json; charset=utf-8'):
        payload = json.dumps(value).encode() if content_type.startswith('application/json') else value
        self.send_response(code)
        self.send_header('Content-Type', content_type)
        self.send_header('Content-Length', str(len(payload)))
        self.send_header('Cache-Control', 'no-store')
        self.send_header('X-Content-Type-Options', 'nosniff')
        self.send_header('Referrer-Policy', 'no-referrer')
        if content_type == 'application/octet-stream':
            self.send_header('Content-Disposition', 'attachment; filename="Little-Weeps-family.lwportable"')
        self.send_header('Content-Security-Policy', "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'")
        self.end_headers()
        try:
            self.wfile.write(payload)
        except (BrokenPipeError, ConnectionResetError):
            pass

    def allowed(self, require_token=True):
        # Explicit Host/Origin checks also reject DNS rebinding and cross-site
        # requests. No CORS, GET mutations, filesystem browsing or remote bind.
        if self.headers.get('Host') != self.server.authority or self.headers.get('Origin', self.server.origin) != self.server.origin:
            self.reply(403, dict(error='Open the parent page from its local shortcut.'))
            return False
        if require_token and not hmac.compare_digest(self.headers.get('X-Little-Weeps', ''), self.server.token):
            self.reply(403, dict(error='Reopen the parent page from its shortcut.'))
            return False
        return True

    def do_GET(self):
        if not self.allowed(require_token=self.path == '/api/status'):
            return
        assets = {'/': ('index.html', 'text/html; charset=utf-8'),
                  '/app.js': ('app.js', 'text/javascript; charset=utf-8'),
                  '/style.css': ('style.css', 'text/css; charset=utf-8')}
        if self.path in assets:
            name, kind = assets[self.path]
            self.reply(200, (Path(__file__).parent / 'parent-ui' / name).read_bytes(), kind)
        elif self.path == '/api/status':
            self.reply(200, self.server.status())
        else:
            self.reply(404, dict(error='Not found.'))

    def do_POST(self):
        if not self.allowed():
            return
        try:
            length = int(self.headers.get('Content-Length', '0'))
            limit = REQUEST_LIMIT if self.path == '/api/portable-verify' else 16384 if self.path == '/api/portable-export' else 2048
            if not 0 < length <= limit or self.headers.get('Content-Type') != 'application/json':
                self.reply(400, dict(error='Invalid request.')); return
            self.connection.settimeout(15)
            data = json.loads(self.rfile.read(length))
            if not isinstance(data, dict):
                raise ValueError('Expected object')
            if self.path == '/api/portable-export':
                self.reply(200, self.server.portable.export(data), 'application/octet-stream'); return
            elif self.path == '/api/portable-verify':
                self.reply(200, self.server.portable.verify(data)); return
            elif self.path == '/api/start':
                result = self.server.controller.start()
            elif self.path == '/api/stop':
                result = self.server.controller.stop(data.get('instanceId'))
            elif self.path == '/api/backup':
                result = self.server.operations.backup()
            elif self.path == '/api/recovery-enable':
                result = self.server.operations.enable()
            elif self.path == '/api/recovery-pause':
                result = self.server.operations.pause()
            else:
                self.reply(404, dict(error='Not found.')); return
            result['status'] = self.server.status()
            self.reply(200, result)
        except (ValueError, TypeError):
            self.reply(400, dict(error='Invalid request.'))
        except OperationError as error:
            self.reply(409, dict(error=str(error)))
        except Exception:
            self.reply(503, dict(error='The operation could not be verified. Refresh status; no force-stop or reset was attempted.'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--family', required=True)
    parser.add_argument('--build', type=int, default=84)
    parser.add_argument('--no-browser', action='store_true')
    args = parser.parse_args()
    server = ParentHTTP(ParentServer(args.family, args.build))
    folder = ROOT / 'LocalData/ParentServer'; folder.mkdir(parents=True, exist_ok=True)
    # Launcher-only private file, ignored by Git; capability stays out of paths,
    # query strings, public docs and the game protocol.
    write(folder / 'dashboard.json', dict(url=server.url, family=args.family, pid=__import__('os').getpid()))
    if not args.no_browser:
        webbrowser.open(server.url)
    try:
        server.serve_forever()
    finally:
        server.server_close()  # Closing this panel never stops the game server.


if __name__ == '__main__':
    main()
