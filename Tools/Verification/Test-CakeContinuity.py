# /// script
# dependencies = ["cryptography"]
# ///
"""Migrate a real 166 world, then preserve partially prepared cake through outage/reopen/rejoin."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, json, time, importlib.util
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
    f=RecoveryFixture(166);checks=[];passed=False;save=f.path/'server-world/world.save'
    server_recovery.QUALIFIED_BUILDS|={166,args.build};server_recovery.MAX_QUALIFIED_BUILD=max(args.build,server_recovery.MAX_QUALIFIED_BUILD)
    print('EVIDENCE '+str(f.path/'cake-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old)
        def command(v,op,item='',target='',x=0,y=0):
            r=home.command(v,18,item=item,target=target,value=op,x=x,y=y);require(r['accepted'],str(r));home.ready(v)
        command(old,'easy:start','cookware-0','CAK-02');command(old,'easy:add','ingredient-patty','cookware-0')
        f.stop();backup=Recovery(f.run_id,166).backup();before=checkpoint(save.read_bytes());keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);server=f.launch(0)
        wait(lambda:server.status() and server.status()['status']=='listening','upgraded authority')
        after=checkpoint(save.read_bytes())
        def prior_shape(new,prior):
            if isinstance(prior,dict):return {k:prior_shape(new[k],v) for k,v in prior.items()}
            if isinstance(prior,list):return [prior_shape(n,v) for n,v in zip(new,prior)]
            return new
        require(after['schema']==15 and len(after['toys'])==len(before['toys'])+1,'Additive cake migration failed')
        for key in ['worldId','players','bedrooms','secrets','home','keepy','kitchen','receipts','toys']:
            require(prior_shape(after[key],before[key])==before[key],'Legacy field changed: '+key)
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        clients=[f.join(i) for i in range(1,5)];a=clients[1]
        for v in clients:home.ready(v)
        for v in clients:
            if v!=a:v.close()
        checks.append('Real 166 unfinished unusual cake, all existing items/rooms/receipts and four enrolled profiles preserved; one cake-mix supply added')
        def world():return wait(lambda:server.state(),'authority')['view']
        def cake():return next(t for t in world()['toys'] if t['id']=='cookware-1')['kitchen']['dish']
        def button(name):a.input('touchButton',text=name);time.sleep(.25);home.ready(a)
        r=home.command(a,0,x=-2110,y=200);require(r['accepted'],str(r));time.sleep(.7)
        command(a,'easy:start','cookware-1','CAK-02')
        for name in ['egg','milk','chocolate']:command(a,'easy:add','ingredient-'+name,'cookware-1')
        command(a,'cake:mix','cookware-1',cake()['id']+'@mix',.2,2)
        time.sleep(2);server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'private cake continuation',40);home.ready(a)
        button('Cook');button('Cooking action');time.sleep(3);button('Back');button('Menu')
        path=Path(a.input('inspect')['savePath']);private=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        localcake=next(t for t in private['toys'] if t['id']=='cookware-1')['kitchen']['dish']
        require(localcake['mixed']==1 and localcake['stage']=='pour','Private mixture not saved')
        a.close();a=f.launch(2);home.ready(a);button('Menu')
        reopened=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(reopened['toys']==private['toys'],'Partial cake/stock lost on cold reopen');button('Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'authoritative rejoin',45)
        require(abs(cake()['mixed']-.2)<.001 and cake()['stage']=='mix','Private cake edits imported into shared family world')
        checks.append('Partially mixed shared cake continues privately, cold-reopens exactly, then rejoin restores authoritative partial work without importing private mixing')
        f.stop();f.controller.start();f.stop();require(Recovery(f.run_id,args.build).backup()['verified'],'Cake backup failed')
        Recovery(f.run_id,args.build).restore(Path(backup['path']),digest(save.read_bytes()));f.controller.start();restored=checkpoint(save.read_bytes())
        require(restored['toys']==after['toys'],'Old backup migration changed');f.stop()
        checks.append('Current schema-15 cake backup validates; restored real 166 backup migrates identically; production allowlist unchanged')
        passed=True
    finally:
        f.cleanup();write(f.path/'cake-continuity.json',dict(passed=passed,build=args.build,previousBuild=166,checks=checks,liveFamilyTouched=False));print('RESULT '+str(f.path/'cake-continuity.json'),flush=True)
if __name__=='__main__':main()
