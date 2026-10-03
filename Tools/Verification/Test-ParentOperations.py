"""Real HTTP/native acceptance for parent backup and recovery controls."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import importlib.util
import json
import os
from pathlib import Path
import signal
import threading
from unittest.mock import patch
from urllib.error import HTTPError
from urllib.request import Request, urlopen

from recovery_fixture import RecoveryFixture
from server_recovery import unpack, checkpoint
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    fixture=RecoveryFixture(args.build);controller=fixture.controller;checks=[];success=False
    spec=importlib.util.spec_from_file_location('parent_http',ROOT/'Tools/Parent-Server.py')
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    http=None;thread=None
    def open_http():
        nonlocal http,thread
        http=module.ParentHTTP(controller);thread=threading.Thread(target=http.serve_forever,daemon=True);thread.start()
    def close_http():
        http.shutdown();http.server_close();thread.join(timeout=5)
    def api(route,payload=None,token=True,origin=None):
        headers={'X-Little-Weeps':http.token} if token else {}
        if origin:headers['Origin']=origin
        if payload is not None:headers['Content-Type']='application/json'
        request=Request(http.origin+'/api/'+route,data=json.dumps(payload).encode() if payload is not None else None,headers=headers)
        try:
            with urlopen(request,timeout=90) as response:return response.status,json.load(response)
        except HTTPError as error:return error.code,json.load(error)
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    try:
        controller.start();clients=[fixture.join(i) for i in range(1,5)];open_http()
        native=controller.processes()[0];before=checkpoint((fixture.path/'server-world/world.save').read_bytes())
        require(api('backup',{},token=False)[0]==403 and api('recovery-enable',{},origin='https://example.invalid')[0]==403,'New action bypassed access checks')
        require(api('restore',{})[0]==404,'Destructive restore exposed on routine page')
        code,result=api('backup',{});require(code==200 and result['status']['backup']['state']=='verified','Backup API did not verify file')
        record=read(http.operations.path);backup=ROOT/'LocalData/ServerBackups'/record['backup']['name']
        bundle,files,body=unpack(backup)
        require(body['players']==before['players'] and body['toys']==before['toys'] and body['receipts']==before['receipts'],'Backup omitted existing play')
        require(controller.snapshot()['players']==4 and controller.snapshot()['instanceId']==native['instanceId'],'Backup interrupted play')
        require(all(secret not in json.dumps(result) for secret in ('credentialHash','privateKey','caCertificate','familyId')),'Private enrollment leaked into HTTP response')
        passed('protected local HTTP backup creates a verified complete bundle while all four clients remain connected; routine page exposes no restore or enrollment secrets')
        with patch.object(controller,'build',84):require(api('recovery-enable',{})[0]==409,'Wrong runtime activated')
        with patch.object(controller,'isolated',False),patch.object(controller,'network_prepared',return_value=False):
            require(api('recovery-enable',{})[0]==409,'Unconfigured network activated')
        require(api('recovery-enable',{})[0]==200,'Enable failed')
        wait(lambda: api('status')[1]['recovery']['status']=='healthy','parent-managed supervisor ready')
        native=controller.processes()[0];require(native['output'].parent==fixture.path,'Not a test authority')
        os.kill(native['pid'],signal.SIGTERM)
        wait(lambda: not controller.processes(),'test authority exits')
        wait(lambda: (s:=api('status')[1])['state']=='ready' and s['instanceId']!=native['instanceId'] and s['players']==4,'parent helper recovers four existing clients',70)
        passed('enable refuses wrong build/unprepared network; parent-managed helper then recovers an actual test crash and all four clients rejoin')
        active=controller.snapshot()['instanceId']
        require(api('recovery-pause',{})[0]==200 and not api('status')[1]['recovery']['enabled'],'Pause failed')
        require(controller.snapshot()['instanceId']==active and all(c.input('inspect')['connected'] for c in clients),'Pause disconnected a player')
        require(api('recovery-enable',{})[0]==200,'Re-enable failed')
        require(api('stop',dict(instanceId=active))[0]==409 and api('status')[1]['recovery']['enabled'],'Occupied stop changed recovery preference')
        for c in clients:c.close()
        wait(lambda:controller.snapshot()['players']==0,'empty test server')
        require(api('stop',dict(instanceId=active))[0]==200,'Empty stop failed')
        require(api('status')[1]['recovery']['status']=='paused','Stop preference not reflected immediately')
        close_http();open_http()
        require(api('status')[1]['state']=='stopped' and api('status')[1]['recovery']['status']=='paused','Reopening parent helper undid deliberate stop')
        require(api('start',{})[0]==200,'Manual restart failed')
        wait(lambda:api('status')[1]['recovery']['status']=='healthy','supervision follows manual start')
        passed('pause leaves current players alone; occupied stop stays protected; deliberate stop survives control-helper recreation and manual Start resumes supervision')
        require(api('status')[1]['backup']['state']=='verified','Verified backup record lost across helper recreation')
        original=backup.read_bytes()
        try:
            backup.write_bytes(original+b'changed')
            require(api('status')[1]['backup']['state']=='changed','Altered backup still reported verified')
        finally:backup.write_bytes(original)
        require(api('status')[1]['backup']['state']=='verified','Restored backup hash not observed')
        passed('last-backup status persists and a subsequently altered bundle is no longer displayed as verified')
        api('recovery-pause',{});api('stop',dict(instanceId=controller.snapshot()['instanceId']))
        success=True
    finally:
        if http:close_http()
        fixture.cleanup()
        write(fixture.path/'parent-operations-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,
              scope='Real localhost HTTP, parent-owned supervisor thread, actual isolated crash and four native Windows clients. Not actual family deployment, physical-device or OS-service qualification.'))
        print('Evidence:',fixture.path/'parent-operations-result.json',flush=True)


if __name__=='__main__':main()
