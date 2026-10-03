# /// script
# dependencies = ["cryptography"]
# ///
"""Disposable enrolled upgrade and mid-stair private continuation/rejoin."""
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
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=132);args=p.parse_args()
    f=RecoveryFixture(args.previous);checks=[];passed=False
    print('EVIDENCE '+str(f.path/'upstairs-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old)
        require(home.command(old,10,target='radio-living',value='on')['accepted'],'Legacy radio fixture')
        f.stop();save=f.path/'server-world/world.save';before=checkpoint(save.read_bytes())
        keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build)
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','upgraded authority')
        after=checkpoint(save.read_bytes());expected=deepcopy(before)
        expected['schema']=6;expected['revision']+=1
        for v in expected['players']:v['stairs']=0
        require(after==expected,'Schema-5 migration altered existing state')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        a=f.join(1);home.ready(a)
        checks.append('Schema 5 to 6 preserves complete world, props, fixture/balloon state, receipts and enrollment; adds only transit defaults and one revision')
        require(home.command(a,0,x=-2980,y=420)['accepted'],'Stair setup')
        require(home.command(a,2,item='ball-1')['accepted'],'Carry setup')
        time.sleep(.5);a.input('touchButton',text='Stair entry')
        def actor():return next(v for v in server.state()['view']['players'] if v['id']==a.profile)
        wait(lambda:actor().get('stairs',0)>.15,'mid-stair authority')
        # Kill only this newly created disposable process, identified by its handle.
        server.process.kill();server.process.wait(timeout=10)
        solo=wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'private stair continuation',40)
        require(solo['zone']=='garden','Interrupted stair did not settle to source')
        home.ready(a)
        a.input('touch-begin',role='ball-1',finger=71)
        a.input('touch-move',x=-2800,y=200,finger=71);a.input('touch-end',x=-2800,y=200,finger=71)
        a.input('touchButton',text='Stair entry')
        wait(lambda:(s if (s:=a.input('inspect'))['zone']=='home-upstairs' and s['sceneryReady'] else None),'private upstairs remains playable')
        a.input('touchButton',text='Menu');time.sleep(.2);private=a.input('inspect')
        raw=Path(private['savePath']).read_text(encoding='utf-8-sig').split('\n',2)[2]
        local=json.loads(raw);local_snapshot=local['snapshot']
        require(next(t for t in local_snapshot['toys'] if t['id']=='ball-1')['x']==-2800,'Private edit not durable')
        require(next(v for v in local_snapshot['players'] if v['id']==a.profile)['zone']=='home-upstairs','Private room not durable')
        a.input('touchButton',text='Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'authoritative rejoin',45)
        require(next(v for v in server.state()['view']['players'] if v['id']==a.profile)['zone']=='garden','Offline room imported')
        require(next(t for t in server.state()['view']['toys'] if t['id']=='ball-1')['x']!=-2800,'Offline item imported')
        require(Path(private['savePath']).exists(),'Private save lost on rejoin')
        checks.append('Mid-stair authority loss settles private view safely; offline stairs/item edit save; reconnect loads server world and retains private save separately')
        passed=True
    finally:
        f.cleanup();write(f.path/'upstairs-continuity.json',dict(passed=passed,build=args.build,previousBuild=args.previous,checks=checks,liveFamilyTouched=False))
        print('RESULT '+str(f.path/'upstairs-continuity.json'),flush=True)

if __name__=='__main__':main()
