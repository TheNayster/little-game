# /// script
# dependencies = ["cryptography"]
# ///
"""Schema-7 furniture migration, private edits and authoritative rejoin in an isolated family."""
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
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=137);args=p.parse_args()
    f=RecoveryFixture(args.previous);checks=[];passed=False
    print('EVIDENCE '+str(f.path/'furniture-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old)
        require(home.command(old,10,target='radio-living',value='on')['accepted'],'Prior radio fixture')
        f.stop();save=f.path/'server-world/world.save';before=checkpoint(save.read_bytes())
        keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build)
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','Upgraded authority')
        after=checkpoint(save.read_bytes());normalized=deepcopy(after)
        require(after['schema']==8 and len(after['toys'])==27,'Furniture migration missing')
        require(all(r['theme']==i and r['roomRevision']==1 and r['layout']==0 for i,r in enumerate(after['bedrooms'])),'Room defaults')
        normalized['schema']=before['schema'];normalized['revision']-=1
        normalized['toys']=normalized['toys'][:11]
        for t in normalized['toys']:t.pop('personalRoom')
        normalized['bedrooms']=[{k:r[k] for k in ('id','owner')} for r in normalized['bedrooms']]
        require(normalized==before,'Furniture migration altered prior world')
        require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        owners=[r['owner'] for r in after['bedrooms']]
        checks.append('Schema 7 to 8 adds bounded furniture state and 16 toys; prior world, all 11 items, owners, fixtures, balloon, receipts and enrollment remain unchanged')
        a=f.join(1);home.ready(a)
        def actor():return next(v for v in server.state()['view']['players'] if v['id']==a.profile)
        def move(x,y=420):require(home.command(a,0,x=x,y=y)['accepted'],'Room setup');time.sleep(.5);home.ready(a)
        move(-2980);a.input('touchButton',text='Stair entry');wait(lambda:actor()['zone']=='home-upstairs','Upstairs');home.ready(a)
        move(710);a.input('touchButton',text='Enter bedroom 1');wait(lambda:actor()['zone']=='home-bedroom-1','First room');home.ready(a)
        move(1500,180);item='home-bedroom-1-toy-0'
        require(home.command(a,14,target='theme',value='1:1')['accepted'],'Shared decor')
        require(home.command(a,10,target='bedroom-chest',value='on')['accepted'],'Shared chest')
        require(home.command(a,2,item=item)['accepted'],'Shared toy pickup')
        require(home.command(a,3,item=item,target='home-bedroom-1/chest-0',x=1545,y=300)['accepted'],'Shared toy storage')
        time.sleep(1.5);server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'Private room continuation',40)
        home.ready(a);require(a.input('inspect')['zone']=='home-bedroom-1','Private fork lost latest room')
        a.input('touchButton',text='Decorate');a.input('touchButton',text='Colours');home.ready(a);a.input('touchButton',text='Arrange');home.ready(a);a.input('touchButton',text='Decorate')
        a.input('touch-begin',role=item,finger=91);a.input('touch-move',x=1300,y=180,finger=91);a.input('touch-end',x=1300,y=180,finger=91)
        a.input('touchButton',text='Rest on bed');home.ready(a)
        a.input('touchButton',text='Menu');time.sleep(.2);private=a.input('inspect');path=Path(private['savePath'])
        local=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require([r['owner'] for r in local['bedrooms']]==owners and local['bedrooms'][0]['theme']==2 and local['bedrooms'][0]['layout']==1,'Private decor/save differs')
        require(next(t for t in local['toys'] if t['id']==item)['x']==1300,'Private item not saved')
        a.close();a=f.launch(1);home.ready(a);require(a.input('inspect')['zone']=='home-bedroom-1' and a.input('inspect')['homePose']!='Rest','Private reopen lost room or retained temporary lease')
        a.input('touchButton',text='Menu');time.sleep(.2)
        reopened=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(reopened['bedrooms']==local['bedrooms'] and reopened['toys']==local['toys'],'Private reopen lost furniture/items')
        a.input('touchButton',text='Back to play')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','Restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'Shared rejoin',45)
        shared=server.state()['view']
        require(actor()['zone']=='home-bedroom-1' and shared['bedrooms'][0]['theme']==1 and shared['bedrooms'][0]['layout']==0,'Private decor imported')
        require(next(t for t in shared['toys'] if t['id']==item)['container']=='home-bedroom-1/chest-0','Private toy edits imported')
        require(path.exists() and len(shared['toys'])==27,'Private save lost or migration duplicated stock')
        checks.append('Latest furniture continues privately after outage; local decor/items survive cold reopen with safe lease release; reconnect loads shared furniture without importing offline edits or duplicating stock')
        passed=True
    finally:
        f.cleanup();write(f.path/'furniture-continuity.json',dict(passed=passed,build=args.build,previousBuild=args.previous,checks=checks,liveFamilyTouched=False))
        print('RESULT '+str(f.path/'furniture-continuity.json'),flush=True)

if __name__=='__main__':main()
