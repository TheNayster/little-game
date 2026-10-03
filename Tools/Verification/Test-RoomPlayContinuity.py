# /// script
# dependencies = ["cryptography"]
# ///
"""Upgrade an actual build-151 save and retain room play through private reopen/rejoin."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,json,time,importlib.util
from copy import deepcopy
from pathlib import Path
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint,Recovery,digest
import server_recovery
from shared_garden_runtime import wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    f=RecoveryFixture(151);checks=[];passed=False;save=f.path/'server-world/world.save'
    server_recovery.QUALIFIED_BUILDS|={151,args.build};server_recovery.MAX_QUALIFIED_BUILD=max(args.build,server_recovery.MAX_QUALIFIED_BUILD)
    print('EVIDENCE '+str(f.path/'room-play-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old);f.stop()
        backup=Recovery(f.run_id,151).backup();before=checkpoint(save.read_bytes());keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);server=f.launch(0)
        wait(lambda:server.status() and server.status()['status']=='listening','Upgraded authority')
        after=checkpoint(save.read_bytes());normalized=deepcopy(after)
        require(after['schema']==12 and len(after['toys'])==53,'Room play migration missing')
        normalized['schema']=11;normalized['revision']-=1;normalized['toys']=normalized['toys'][:33]
        for r in normalized['bedrooms']+[s['furniture'] for s in normalized['secrets']]:
            for field in ('bedding','rug','lamp','picture'):r.pop(field)
        require(normalized==before,'Room play migration altered prior state')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        a=f.join(1);others=[f.join(i) for i in (2,3,4)];home.ready(a)
        for v in others:v.close()
        checks.append('Build 151 to current preserves every old object, room, balloon, receipt and enrollment field; adds 20 bounded tea objects; all four original profiles rejoin')
        def world():return wait(lambda:server.state(),'fresh authoritative view')['view']
        def actor():return next(v for v in world()['players'] if v['id']==a.profile)
        def cmd(action,**kw):
            result=home.command(a,action,**kw);require(result['accepted'],str(result));home.ready(a)
        def move(x,y=420):
            wait(lambda:next(p for p in wait(lambda:a.state(),'fresh client view')['view']['players'] if p['id']==a.profile)['zone']==actor()['zone'] and actor()['stairs']==0,'arrival snapshot')
            cmd(0,x=x,y=y);time.sleep(.6);home.ready(a)
        move(-2980);a.input('touchButton',text='Stair entry');wait(lambda:actor()['zone']=='home-upstairs','upstairs');home.ready(a)
        move(710);a.input('touchButton',text='Enter bedroom 1');wait(lambda:actor()['zone']=='home-bedroom-1','bedroom');home.ready(a)
        move(2310,220);a.input('touchButton',text='Secret star door');wait(lambda:actor()['zone']=='home-secret-1','secret');home.ready(a)
        move(1400,180);item='home-secret-1-plush-0'
        cmd(2,item=item);cmd(3,item=item,target='home-secret-1/nest-3',x=1630,y=265)
        cmd(14,target='bedding',value='1:1');cmd(15,target='active',value='0:2')
        time.sleep(1.5);server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'private room continuation',40);home.ready(a)
        require(a.input('inspect')['zone']=='home-secret-1','Private fork lost current room')
        a.input('touchButton',text='Decorate');a.input('touchButton',text='Bedding');home.ready(a);a.input('touchButton',text='Decorate')
        a.input('touch-begin',role=item,finger=51);a.input('touch-move',x=1400,y=160,finger=51);a.input('touch-end',x=1400,y=160,finger=51);home.ready(a)
        a.input('touchButton',text='Menu');time.sleep(.4);path=Path(a.input('inspect')['savePath'])
        local=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(local['secrets'][0]['furniture']['bedding']==2 and not local['secrets'][0]['active'],'Private decor/entrance lost')
        require(next(t for t in local['toys'] if t['id']==item)['container']=='','Private tuck retrieval not saved')
        a.close();a=f.launch(1);home.ready(a);require(a.input('inspect')['zone']=='home-secret-1','Private cold reopen lost room')
        a.input('touchButton',text='Menu');time.sleep(.3);reopened=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(reopened['secrets']==local['secrets'] and reopened['toys']==local['toys'],'Private cold reopen lost room play');a.input('touchButton',text='Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'authoritative rejoin',45)
        require(world()['secrets'][0]['furniture']['bedding']==1 and next(t for t in world()['toys'] if t['id']==item)['container']=='home-secret-1/nest-3','Private edits imported into family')
        require(path.exists(),'Private save lost');a.input('touchButton',text='Bedroom ←');wait(lambda:actor()['zone']=='home-bedroom-1','hidden room exit')
        checks.append('New tucks and bedding survive an outage; private edits retain exact toys/decor through cold reopen; rejoin restores server state without importing private edits; hidden entrance retains usable exit')
        f.stop();f.controller.start();f.stop();current=Recovery(f.run_id,args.build).backup();require(current['verified'],'Current room-play backup not verified')
        Recovery(f.run_id,args.build).restore(Path(backup['path']),digest(save.read_bytes()));f.controller.start()
        upgraded=checkpoint(save.read_bytes());require(upgraded['toys']==after['toys'] and upgraded['bedrooms']==after['bedrooms'],'Restored legacy backup migrated differently');f.stop()
        checks.append('Current backup validates, and restoring the real build-151 backup repeats the same additive room-play migration')
        passed=True
    finally:
        f.cleanup();write(f.path/'room-play-continuity.json',dict(passed=passed,build=args.build,previousBuild=151,checks=checks,liveFamilyTouched=False));print('RESULT '+str(f.path/'room-play-continuity.json'),flush=True)
if __name__=='__main__':main()
