# /// script
# dependencies = ["cryptography"]
# ///
"""Schema-6 migration and room private-continuation/rejoin in an isolated family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from copy import deepcopy
import importlib.util
import json
from pathlib import Path
import time
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint
from shared_garden_runtime import wait,require,write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=135);args=p.parse_args()
    f=RecoveryFixture(args.previous);checks=[];passed=False
    print('EVIDENCE '+str(f.path/'bedroom-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old)
        require(home.command(old,10,target='radio-living',value='on')['accepted'],'Prior radio fixture')
        f.stop();save=f.path/'server-world/world.save';before=checkpoint(save.read_bytes())
        keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build)
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','Upgraded room authority')
        after=checkpoint(save.read_bytes());expected=deepcopy(before)
        expected['schema']=7;expected['revision']+=1
        expected['bedrooms']=[dict(id='home-bedroom-'+str(i+1),owner=v['id']) for i,v in enumerate(before['players'])]
        require(after==expected,'Room migration altered prior world')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        owners=deepcopy(after['bedrooms'])
        checks.append('Schema 6 to 7 adds four persistent owners once; complete prior world, fixtures, balloon, receipts and enrollment remain unchanged')
        a=f.join(1);home.ready(a)
        def move(x):require(home.command(a,0,x=x,y=420)['accepted'],'Room setup');time.sleep(.5);home.ready(a)
        def actor():return next(v for v in server.state()['view']['players'] if v['id']==a.profile)
        move(-2980);require(home.command(a,2,item='ball-1')['accepted'],'Carried room fixture')
        a.input('touchButton',text='Stair entry');wait(lambda:actor()['zone']=='home-upstairs','Upstairs');home.ready(a)
        move(710);a.input('touchButton',text='Enter bedroom 1');wait(lambda:actor()['zone']=='home-bedroom-1','First room');home.ready(a)
        require(home.command(a,3,item='ball-1',x=700,y=180)['accepted'],'Shared room placement');time.sleep(1.5)
        server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'Private room continuation',40)
        home.ready(a);require(a.input('inspect')['zone']=='home-bedroom-1','Private fork lost latest room')
        a.input('touch-begin',role='ball-1',finger=91);a.input('touch-move',x=980,y=180,finger=91);a.input('touch-end',x=980,y=180,finger=91)
        a.input('touchButton',text='Return to hallway');wait(lambda:a.input('inspect')['zone']=='home-upstairs','Private room exit');home.ready(a)
        a.input('touchButton',text='Enter bedroom 2');wait(lambda:a.input('inspect')['zone']=='home-bedroom-2','Private second room');home.ready(a)
        a.input('touchButton',text='Menu');time.sleep(.2);private=a.input('inspect');path=Path(private['savePath'])
        local=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(local['bedrooms']==owners and next(v for v in local['players'] if v['id']==a.profile)['zone']=='home-bedroom-2','Private room ownership/save differs')
        require(next(t for t in local['toys'] if t['id']=='ball-1')['x']==980,'Private item not saved')
        a.input('touchButton',text='Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','Restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'Shared room rejoin',45)
        require(actor()['zone']=='home-bedroom-1' and server.state()['view']['bedrooms']==owners,'Offline room imported or owners changed')
        require(next(t for t in server.state()['view']['toys'] if t['id']=='ball-1')['x']==700,'Private item imported')
        require(path.exists(),'Private room save lost')
        checks.append('Authority loss inside bedroom preserves latest room and ownership privately; offline item/room progress saves separately; rejoin restores shared room without importing local edits')
        passed=True
    finally:
        f.cleanup();write(f.path/'bedroom-continuity.json',dict(passed=passed,build=args.build,previousBuild=args.previous,checks=checks,liveFamilyTouched=False))
        print('RESULT '+str(f.path/'bedroom-continuity.json'),flush=True)

if __name__=='__main__':main()
