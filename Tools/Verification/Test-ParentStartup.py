"""Real Windows shortcut/helper/recovery proof inside a new isolated family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import importlib.util
import json
import os
import subprocess
import sys
import threading
import time
from urllib.error import HTTPError
from urllib.parse import urlsplit
from urllib.request import Request, urlopen
from unittest.mock import patch

import psutil
from parent_bootstrap import ensure_dashboard, ready_dashboard
from parent_startup import ParentStartup, state_folder, powershell, process_alive
from parent_server import OperationError
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint
from shared_garden_runtime import ROOT, read, write, wait, require


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    f=RecoveryFixture(args.build);controller=f.controller;folder=state_folder(controller);folder.mkdir()
    write(folder/'settings.json',dict(family=f.run_id,build=f.build))
    spec=importlib.util.spec_from_file_location('parent_http',ROOT/'Tools/Parent-Server.py')
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    checks=[];http=None;thread=None;success=False;observations={};helpers=set()
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def api(route,payload=None,token=True,origin=None):
        if http:
            url=http.origin;cap=http.token
        else:
            record=read(folder/'dashboard.json');u=urlsplit(record['url']);url=f'http://127.0.0.1:{u.port}';cap=u.fragment
        headers={'X-Little-Weeps':cap} if token else {}
        if origin:headers['Origin']=origin
        if payload is not None:headers['Content-Type']='application/json'
        request=Request(url+'/api/'+route,data=json.dumps(payload).encode() if payload is not None else None,headers=headers)
        try:
            with urlopen(request,timeout=60) as response:return response.status,json.load(response)
        except HTTPError as error:return error.code,json.load(error)
    def kill_helper():
        record=read(folder/'dashboard.json')
        if not record:return
        require(record['family']==f.run_id,'Wrong helper scope')
        if not process_alive(record['pid']):return
        process=psutil.Process(record['pid']);command=process.cmdline()
        require(str(ROOT/'Tools/Parent-Server.py') in command and f.run_id in command and '--isolated' in command,'Wrong helper process')
        helpers.add(record['pid']);process.terminate();process.wait(15)
    def launch_link():
        os.startfile(str(startup.path))
        record=wait(lambda:ready_dashboard(folder,controller),'real shortcut launches matching helper',40)
        helpers.add(record['pid']);return record
    def close_http():
        nonlocal http,thread
        if http:http.shutdown();http.server_close();thread.join(5);http=None
    try:
        controller.start();clients=[f.join(i) for i in (1,2,3,4)]
        http=module.ParentHTTP(controller);thread=threading.Thread(target=http.serve_forever,daemon=True);thread.start()
        startup=http.startup
        require(startup.path.is_relative_to(f.path),'Real Windows Startup folder selected')
        require(api('startup-enable',{},token=False)[0]==403 and api('startup-disable',{},origin='https://invalid.example')[0]==403,'Startup access boundary failed')
        require(api('startup-enable',{'path':'unwanted'})[0]==400 and api('startup-enable',{})[0]==409,'Enable accepted arbitrary data or missing recovery')
        require(api('startup-enable')[0]==404,'GET mutated registration')
        require(not startup.path.exists(),'Startup created before opt-in')
        passed('startup actions require local authorization, empty payload, healthy selected build and recovery; test registration never uses the real Startup folder')
        require(api('recovery-enable',{})[0]==200,'Recovery enable failed')
        wait(lambda:api('status')[1]['recovery']['status']=='healthy','healthy recovery')
        with patch.object(controller,'build',85):require(api('startup-enable',{})[0]==409,'Mismatched build enabled startup')
        with patch.object(controller,'isolated',False),patch.object(controller,'network_prepared',return_value=False):
            require(api('startup-enable',{})[0]==409,'Unprepared network enabled startup')
        instance=controller.snapshot()['instanceId']
        code,result=api('startup-enable',{});require(code==200 and result['status']['startup']['state']=='configured','Native .lnk creation failed: '+str(result))
        original=startup.path.read_bytes()
        require(api('startup-enable',{})[0]==200 and startup.path.read_bytes()==original,'Repeated enable replaced shortcut')
        require(api('startup-disable',{})[0]==200 and not startup.path.exists(),'Remove failed')
        require(controller.snapshot()['instanceId']==instance and controller.snapshot()['players']==4 and api('status')[1]['recovery']['enabled'],'Registration interrupted play/recovery')
        require(api('startup-enable',{})[0]==200,'Re-add failed')
        original=startup.path.read_bytes();startup.path.write_bytes(b'unrelated shortcut content')
        require(api('startup-disable',{})[0]==409 and api('startup-enable',{})[0]==409 and startup.path.read_bytes()==b'unrelated shortcut content','Changed shortcut overwritten/deleted')
        startup.path.write_bytes(original)
        passed('native Windows shortcut create/read/remove is idempotent, preserves four active players and recovery, and refuses altered or unprepared registrations')
        close_http();first=launch_link()
        require(controller.snapshot()['instanceId']==instance and controller.snapshot()['players']==4,'Shortcut restarted active authority')
        with ThreadPoolExecutor(max_workers=2) as pool:
            results=list(pool.map(lambda _:ensure_dashboard(controller,True),range(2)))
        require(all(r['pid']==first['pid'] for r in results),'Duplicate helper created')
        direct=subprocess.run([sys.executable,str(ROOT/'Tools/Parent-Server.py'),'--family',f.run_id,'--build',str(f.build),'--isolated','--no-browser'],capture_output=True,timeout=15,creationflags=subprocess.CREATE_NO_WINDOW)
        require(direct.returncode!=0 and ready_dashboard(folder,controller)['pid']==first['pid'],'Lifetime helper lock failed')
        record=read(folder/'dashboard.json');write(folder/'dashboard.json',dict(record,build=85))
        try:
            try:ensure_dashboard(controller)
            except OperationError:pass
            else:raise AssertionError('Stale helper metadata accepted')
            require(process_alive(first['pid']),'Stale helper killed')
        finally:write(folder/'dashboard.json',record)
        passed('actual shortcut invokes the offline launcher; concurrent launches reuse one verified helper, duplicate direct service is rejected and stale live helper is left untouched')
        for client in clients:client.close()
        wait(lambda:controller.snapshot()['players']==0,'empty fixture')
        require(api('stop',{'instanceId':instance})[0]==200,'Parent stop failed')
        stopped=(f.path/'server-world/world.save').read_bytes();kill_helper();launch_link()
        time.sleep(12)
        require(controller.snapshot()['state']=='stopped' and api('status')[1]['recovery']['status']=='paused','Sign-in undid parent Stop')
        require((f.path/'server-world/world.save').read_bytes()==stopped,'Stopped checkpoint changed')
        passed('deliberate Stop survives helper exit and actual shortcut relaunch without changing the stopped save')
        code,result=api('start',{});require(code==200,'Restart failed: '+str(result))
        clients=[f.join(i) for i in (1,2,3,4)]
        wait(lambda:api('status')[1]['recovery']['status']=='healthy','recovery active')
        native=controller.processes()[0];before=checkpoint((f.path/'server-world/world.save').read_bytes())
        kill_helper();require(native['output'].parent==f.path,'Wrong authority scope')
        process=psutil.Process(native['pid']);process.terminate();process.wait(15)
        started=time.monotonic();launch_link()
        wait(lambda:controller.snapshot()['state']=='ready' and controller.snapshot()['players']==4 and controller.snapshot()['instanceId']!=native['instanceId'],'sign-in recovery and four retained clients',75)
        after=checkpoint((f.path/'server-world/world.save').read_bytes())
        require(all(after[k]==before[k] for k in ('players','toys','receipts')),'Sign-in recovery changed durable play')
        observations['recoverySeconds']=round(time.monotonic()-started,2)
        passed('saved recovery preference resumes after both helper and authority loss; guarded restart restores durable play and four original clients rejoin')
        current=controller.snapshot()['instanceId'];require(api('recovery-pause',{})[0]==200,'Pause failed')
        kill_helper();require(ensure_dashboard(controller,True) is None,'Paused recovery launched at sign-in')
        require(controller.snapshot()['instanceId']==current and controller.snapshot()['players']==4,'Pause disturbed active game')
        # Manual shortcut remains available after Pause and does not undo it.
        ensure_dashboard(controller);helpers.add(read(folder/'dashboard.json')['pid'])
        require(not api('status')[1]['recovery']['enabled'],'Manual launch overrode Pause')
        require(api('startup-disable',{})[0]==200,'Final shortcut removal failed')
        kill_helper();require(ensure_dashboard(controller,True) is None,'Removed startup still launched')
        passed('Pause and removal suppress sign-in launch, while manual reopening and active players remain available without changing saved intent')
        success=True
    finally:
        close_http();kill_helper();f.cleanup()
        write(f.path/'startup-result.json',dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,observations=observations,
              scope='Isolated native Windows authority, four clients, real .lnk execution in a test folder and fresh helper processes. No actual account startup entry, Windows sign-out/reboot, live family change or physical mobile acceptance.'))
        print('Evidence:',f.path/'startup-result.json',flush=True)


if __name__=='__main__':main()
