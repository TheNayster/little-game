# /// script
# dependencies = ["cryptography"]
# ///
"""Actual build-155 migration and food persistence through private reopen and authoritative rejoin."""
import argparse, json, time, importlib.util
from copy import deepcopy
from pathlib import Path
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint, Recovery, digest
import server_recovery
from shared_garden_runtime import wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    f=RecoveryFixture(155);checks=[];passed=False;save=f.path/'server-world/world.save'
    server_recovery.QUALIFIED_BUILDS|={155,args.build};server_recovery.MAX_QUALIFIED_BUILD=max(args.build,server_recovery.MAX_QUALIFIED_BUILD)
    print('EVIDENCE '+str(f.path/'kitchen-continuity.json'),flush=True)
    try:
        f.controller.start();a=f.join(1);home.ready(a);f.stop()
        backup=Recovery(f.run_id,155).backup();before=checkpoint(save.read_bytes());keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);server=f.launch(0)
        wait(lambda:server.status() and server.status()['status']=='listening','upgraded authority')
        after=checkpoint(save.read_bytes());normalized=deepcopy(after)
        require(after['schema']==13 and len(after['toys'])==106,'Kitchen migration missing')
        normalized['schema']=12;normalized['revision']-=1;normalized['toys']=normalized['toys'][:53];normalized.pop('kitchen')
        for t in normalized['toys']:t.pop('kitchen',None)
        require(normalized==before,'Kitchen migration changed old fields')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        a=f.join(1);others=[f.join(i) for i in (2,3,4)];home.ready(a)
        for v in others:v.close()
        checks.append('Actual 155 checkpoint upgrades additively: all prior objects, rooms, balloon and receipts identical; 53 kitchen objects added once; four original profiles rejoin')
        def world():return wait(lambda:server.state(),'fresh authority')['view']
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def actor():return next(p for p in world()['players'] if p['id']==a.profile)
        def cmd(action,**kw):
            r=home.command(a,action,**kw);require(r['accepted'],str(r));home.ready(a)
        def cook(op,item='',target=''):cmd(18,item=item,target=target,value=op)
        def move(x,y=420):cmd(0,x=x,y=y);time.sleep(.6);home.ready(a)
        def button(name):a.input('touchButton',text=name);time.sleep(.3);home.ready(a)
        cook('door',target='fridge');cook('readybase','cookware-0','PIZ-01');cook('cut','cookware-0');cook('serve','cookware-0','plate-0')
        cmd(2,item='plate-0');move(-2980);button('Stair entry');wait(lambda:actor()['zone']=='home-upstairs' and actor()['stairs']==0,'upstairs');home.ready(a)
        move(710);button('Enter bedroom 1');wait(lambda:actor()['zone']=='home-bedroom-1','bedroom');home.ready(a)
        cmd(3,item='plate-0',x=1800,y=60);require(toy('plate-0')['kitchen']['dish']['portions']==1,'Carried portion changed')
        move(1500,200);time.sleep(1.5);server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'private kitchen continuation',40);home.ready(a)
        a.input('touch-begin',role='plate-0',finger=52);a.input('touch-end',role='plate-0',finger=52);button('Taste');button('Close');button('Menu')
        path=Path(a.input('inspect')['savePath']);local=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(next(t for t in local['toys'] if t['id']=='plate-0')['kitchen']['dish']['portions']==0,'Private taste not saved')
        a.close();a=f.launch(1);home.ready(a);require(a.input('inspect')['zone']=='home-bedroom-1','Private room lost')
        button('Menu');reopened=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(reopened['toys']==local['toys'] and reopened['kitchen']==local['kitchen'],'Private food or appliances lost');button('Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'authoritative rejoin',45)
        require(toy('plate-0')['kitchen']['dish']['portions']==1,'Private taste imported into family');require(path.exists(),'Private save removed')
        checks.append('Real plate survives stairs and bedroom travel; outage/private taste/cold reopen retain food; rejoin loads family portion without importing private edits')
        f.stop();f.controller.start();f.stop();require(Recovery(f.run_id,args.build).backup()['verified'],'Current backup failed')
        Recovery(f.run_id,args.build).restore(Path(backup['path']),digest(save.read_bytes()));f.controller.start();restored=checkpoint(save.read_bytes())
        require(restored['toys']==after['toys'] and restored['kitchen']==after['kitchen'],'Old backup migration changed');f.stop()
        checks.append('Current food backup validates; restoring actual 155 backup repeats identical additive kitchen migration')
        passed=True
    finally:
        f.cleanup();write(f.path/'kitchen-continuity.json',dict(passed=passed,build=args.build,previousBuild=155,checks=checks,liveFamilyTouched=False));print('RESULT '+str(f.path/'kitchen-continuity.json'),flush=True)
if __name__=='__main__':main()
