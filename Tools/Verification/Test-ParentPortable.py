"""Isolated HTTP/native acceptance for portable parent-page backups."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import base64
from datetime import datetime, timezone
import importlib.util
import json
import secrets
import threading
import time
from unittest.mock import patch
from urllib.error import HTTPError
from urllib.request import Request, urlopen

import parent_portable
import portable_recovery
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int); parser.add_argument('--hold-browser', action='store_true')
    args = parser.parse_args(); f = RecoveryFixture(args.build)
    spec = importlib.util.spec_from_file_location('portable_parent_http', ROOT / 'Tools/Parent-Server.py')
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    server = None; checks = []; success = False
    password = secrets.token_urlsafe(36)
    def passed(name): checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def api(route, data=None, token=True, origin=None, raw=None, length=None):
        headers = {'X-Little-Weeps':server.token} if token else {}
        if origin: headers['Origin'] = origin
        if data is not None or raw is not None: headers['Content-Type'] = 'application/json'
        if length is not None: headers['Content-Length'] = str(length)
        request = Request(server.origin + '/api/' + route,
                          data=raw if raw is not None else json.dumps(data).encode() if data is not None else None,
                          headers=headers)
        try:
            with urlopen(request, timeout=90) as r:
                value = r.read()
                return r.status, value if r.headers.get_content_type() == 'application/octet-stream' else json.loads(value), dict(r.headers)
        except HTTPError as e: return e.code, json.load(e), dict(e.headers)
    export_body = dict(password=password, confirmation=password)
    try:
        f.controller.start(); clients = [f.join(i) for i in range(1,5)]
        clients[0].input('press', role='bucket-1'); clients[0].input('release', x=715, y=190)
        clients[1].input('button', text='Creek')
        wait(lambda: all(not c.input('inspect')['pending'] for c in clients), 'settled shared edits')
        server = module.ParentHTTP(f.controller)
        thread = threading.Thread(target=server.serve_forever, daemon=True); thread.start()
        baseline = checkpoint((f.path/'server-world/world.save').read_bytes())
        native = f.controller.snapshot()
        code, raw, headers = api('portable-export', export_body)
        require(code==200 and headers['Cache-Control']=='no-store' and '.lwportable' in headers['Content-Disposition'], 'Download headers')
        bundle, files, credentials, body = portable_recovery.validate_payload(portable_recovery.open_sealed(raw,password))
        require(body['players']==baseline['players'] and body['toys']==baseline['toys'] and body['receipts']==baseline['receipts'], 'Downloaded progress differs')
        require(bundle['world']==f.run_id and len(body['players'])==4 and {p['zone'] for p in body['players']}=={'garden','creek'}, 'Wrong or incomplete family')
        require(f.controller.snapshot()['instanceId']==native['instanceId'] and f.controller.snapshot()['players']==4, 'Export interrupted players')
        require(api('status')[1]['backup']['state']=='verified', 'Export did not update local-backup status')
        secrets_to_hide=[password, f.run_id]+[v[k] for v in credentials.values() for k in ('privateKey','credential') if k in v]
        require(all(v.encode() not in raw for v in secrets_to_hide), 'Plaintext in downloaded ciphertext')
        passed('authenticated binary download preserves the complete two-area four-player world and enrollment without stopping play; ciphertext and no-store attachment headers verified')

        encoded=base64.b64encode(raw).decode(); upload=dict(password=password,file=encoded)
        before=checkpoint((f.path/'server-world/world.save').read_bytes())
        code,result,_=api('portable-verify',upload)
        require(code==200 and result['profiles']==4 and result['revision']==body['revision'] and result['independentRecoveryQualified'] is False, 'Read-only verification result')
        after=checkpoint((f.path/'server-world/world.save').read_bytes())
        # The live authority legitimately checkpoints advancing idle clocks.
        # Compare gameplay here; test exact bytes below with the writer stopped.
        require(set(result)=={'result','build','revision','profiles','independentRecoveryQualified'}
                and {k:v for k,v in after.items() if k!='idleTimers'}=={k:v for k,v in before.items() if k!='idleTimers'},
                'Verification leaked data or edited game progress')
        clients[2].input('button',text='Orange pup')
        wait(lambda:not clients[2].input('inspect')['pending'],'ongoing play after download')
        require(all(c.input('inspect')['connected'] for c in clients),'Sibling lost admission')
        passed('selected encrypted-file upload verifies the expected family using only safe metadata; world is unchanged and all four clients remain usable')

        for payload in (dict(password='wrong password for this test',file=encoded),
                        dict(password=password,file=base64.b64encode(raw[:-1]+bytes([raw[-1]^1])).decode()),
                        dict(password=password,file='not base64'),dict(password=password,file='')):
            code,error,_=api('portable-verify',payload)
            require(code==409 and not any(v in json.dumps(error) for v in secrets_to_hide),'Unsafe verification error')
        with patch.object(server.portable.controller,'family','0'*32):
            require(api('portable-verify',upload)[0]==409,'Cross-family verification accepted')
        require(api('portable-export',dict(password=password,confirmation='a different temporary passphrase'))[0]==409,'Mismatched passphrases accepted')
        require(api('portable-export',dict(password='short',confirmation='short'))[0]==409,'Short passphrase accepted')
        passed('wrong passwords, damaged/empty/malformed files, another family and invalid export passphrases are rejected without restoration or secret-bearing errors')

        for route,data in [('portable-export',export_body),('portable-verify',upload)]:
            require(api(route,data,token=False)[0]==403 and api(route,data,origin='https://example.invalid')[0]==403,'Access control bypass')
            require(api(route)[0]==404,'GET mutation exposed')
        require(api('portable-verify',raw=b'{}',length=parent_portable.REQUEST_LIMIT+1)[0]==400,'Upload limit bypass')
        require(api('portable-export',raw=b'{}',length=16385)[0]==400,'Export limit bypass')
        require(api('portable-verify',raw=b'[]')[0]==400 and api('portable-export',raw=b'broken')[0]==400,'Invalid JSON accepted')
        require(api('portable-restore',{})[0]==404 and api('restore',{})[0]==404,'Restore exposed')
        passed('missing session, foreign origin, oversized and invalid bodies, GET mutations and restore routes are refused')

        entered=threading.Event(); release=threading.Event(); response=[]
        original=portable_recovery.verify_bytes
        def delayed(*a): entered.set(); require(release.wait(15),'test concurrency release'); return original(*a)
        with patch.object(portable_recovery,'verify_bytes',side_effect=delayed):
            worker=threading.Thread(target=lambda:response.append(api('portable-verify',upload))); worker.start()
            try:
                require(entered.wait(8),'first protected operation missing')
                require(api('portable-verify',upload)[0]==409 and api('portable-export',export_body)[0]==409,'Competing KDF queued')
                require(api('status')[0]==200,'Status blocked by protected backup')
            finally: release.set(); worker.join(timeout=30)
        require(response and response[0][0]==200,'Original operation failed')
        passed('one protected operation at a time bounds KDF memory; competing export/check requests fail promptly while status remains responsive')

        # Backend recreation cannot retain a passphrase or uploaded file: no
        # such fields are stored, and verification is performed from bytes.
        server.shutdown(); server.server_close(); thread.join(timeout=5)
        server=module.ParentHTTP(f.controller);thread=threading.Thread(target=server.serve_forever,daemon=True);thread.start()
        require(api('portable-verify',upload)[0]==200 and f.controller.snapshot()['instanceId']==native['instanceId'],'Reopen check lost world')
        f.stop(); before=(f.path/'server-world/world.save').read_bytes()
        require(api('portable-verify',upload)[0]==200 and (f.path/'server-world/world.save').read_bytes()==before,
                'Read-only check changed stopped world bytes')
        passed('a new helper verifies without disturbing the live authority or players; a stopped-world check leaves the exact checkpoint bytes unchanged')
        if args.hold_browser:
            f.controller.start(); clients=[f.join(i) for i in range(1,5)]
            # Public synthetic UI-test phrase, never an actual family password.
            browser_password='Temporary browser acceptance phrase.'
            code,raw,_=api('portable-export',dict(password=browser_password,confirmation=browser_password))
            require(code==200,'Browser fixture export')
            file=f.path/'browser-check.lwportable';file.write_bytes(raw)
            write(ROOT/'LocalData/ParentServer/portable-test-dashboard.json',dict(url=server.url,fixture=str(f.path),file=str(file)))
            print('READY for browser acceptance; isolated fixture only.',flush=True)
            deadline=time.monotonic()+1500
            while not (f.path/'browser-done.json').exists() and time.monotonic()<deadline: time.sleep(1)
            require(read(f.path/'browser-done.json')=={'passed':True},'Browser acceptance incomplete')
        success=True
    finally:
        if server: server.shutdown();server.server_close()
        f.cleanup()
        write(f.path/'parent-portable-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
              scope='Isolated Windows HTTP/native test world only. Does not establish off-PC storage, another-account recovery, physical-device acceptance or actual parent-helper deployment.'))
        print('Evidence:',f.path/'parent-portable-result.json',flush=True)


if __name__=='__main__':main()
