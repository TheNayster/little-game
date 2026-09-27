# /// script
# dependencies = ["cryptography"]
# ///
"""Schema-eight upgrade and secret-room private save/rejoin, using disposable enrollment."""
import argparse,json,time,importlib.util
from copy import deepcopy
from pathlib import Path
from parent_server import ParentServer
from recovery_fixture import RecoveryFixture
from server_recovery import checkpoint
from shared_garden_runtime import wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=142);args=p.parse_args()
    f=RecoveryFixture(args.previous);checks=[];passed=False
    print('EVIDENCE '+str(f.path/'secret-continuity.json'),flush=True)
    try:
        f.controller.start();old=f.join(1);home.ready(old);f.stop()
        save=f.path/'server-world/world.save';before=checkpoint(save.read_bytes());keys={x.name:x.read_bytes() for x in f.path.glob('*.pairing')}
        f.build=args.build;f.controller=ParentServer(f.run_id,args.build);server=f.launch(0)
        wait(lambda:server.status() and server.status()['status']=='listening','Upgraded authority')
        after=checkpoint(save.read_bytes());normalized=deepcopy(after)
        require(after['schema']==9 and len(after['toys'])==27 and len(after['secrets'])==4 and all(not r['created'] for r in after['secrets']),'Secret migration missing')
        normalized['schema']=8;normalized['revision']-=1;normalized.pop('secrets')
        require(normalized==before,'Secret migration altered prior furnished world');require(all((f.path/n).read_bytes()==v for n,v in keys.items()),'Enrollment changed')
        checks.append('Schema 8 to 9 adds four reserved secrets only; all prior furnished state, 27 objects, profiles, fixtures, balloon, receipts and enrollment remain unchanged')
        a=f.join(1);home.ready(a)
        def actor():return next(v for v in server.state()['view']['players'] if v['id']==a.profile)
        def move(x,y=420):
            wait(lambda:next(p for p in a.state()['view']['players'] if p['id']==a.profile)['zone']==actor()['zone'] and actor()['stairs']==0,'Arrival snapshot')
            require(home.command(a,0,x=x,y=y)['accepted'],'Room setup');time.sleep(.6);home.ready(a)
        move(-2980);a.input('touchButton',text='Stair entry');wait(lambda:actor()['zone']=='home-upstairs','Upstairs');home.ready(a)
        move(710);a.input('touchButton',text='Enter bedroom 1');wait(lambda:actor()['zone']=='home-bedroom-1','Bedroom');home.ready(a)
        move(2310,220);time.sleep(.6);a.input('touchButton',text='Secret star door');wait(lambda:actor()['zone']=='home-secret-1','Secret entry');home.ready(a)
        move(1500,180);item='home-secret-1-plush-0'
        require(home.command(a,14,target='theme',value='1:1')['accepted'],'Shared secret decoration')
        require(home.command(a,2,item=item)['accepted'],'Shared plush pickup');require(home.command(a,3,item=item,target='home-secret-1/chest-2',x=1685,y=300)['accepted'],'Shared plush storage')
        require(home.command(a,15,target='active',value='0:2')['accepted'],'Hide entrance with owner inside')
        time.sleep(1.5);server.process.kill();server.process.wait(timeout=10)
        wait(lambda:(s if not (s:=a.input('inspect'))['shared'] and s['ready'] and s['adventure'] else None),'Private secret continuation',40)
        home.ready(a);require(a.input('inspect')['zone']=='home-secret-1','Private fork lost latest secret')
        a.input('touchButton',text='Decorate');a.input('touchButton',text='Colours');home.ready(a);a.input('touchButton',text='Arrange');home.ready(a);a.input('touchButton',text='Decorate')
        a.input('touch-begin',role=item,finger=81);a.input('touch-move',x=1300,y=180,finger=81);a.input('touch-end',x=1300,y=180,finger=81)
        a.input('touchButton',text='Sit in fort 1');home.ready(a)
        a.input('touchButton',text='Quiet room');a.input('touchButton',text='Sky motion');a.input('touchButton',text='Sky brightness');a.input('touchButton',text='Quiet music');a.input('touchButton',text='Quiet effects');a.input('touchButton',text='Quiet room')
        prefs=a.input('inspect');a.input('touchButton',text='Menu');time.sleep(.3);path=Path(a.input('inspect')['savePath'])
        local=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot'];r=local['secrets'][0]
        require(r['furniture']['theme']==2 and r['furniture']['layout']==1 and not r['active'],'Private secret save differs')
        require(next(t for t in local['toys'] if t['id']==item)['x']==1300,'Private plush not saved')
        a.close();a=f.launch(1);home.ready(a);reopen=a.input('inspect')
        require(reopen['zone']=='home-secret-1' and reopen['homePose']!='Sit','Private reopen lost room or retained fort lease')
        require(all(reopen[k]==prefs[k] for k in ('quietStill','quietBrightness','quietMusicLevel','quietEffectsLevel')),'Local quiet preferences did not survive reopen')
        a.input('touchButton',text='Menu');time.sleep(.2);loaded=json.loads(path.read_text(encoding='utf-8-sig').split('\n',2)[2])['snapshot']
        require(loaded['secrets']==local['secrets'] and loaded['toys']==local['toys'],'Reopen lost secret or plush state');a.input('touchButton',text='Back to play')
        a.input('touchButton',text='Bedroom ←');home.ready(a);require(a.input('inspect')['zone']=='home-bedroom-1','Archived room trapped player after private reopen')
        server=f.launch(0);wait(lambda:server.status() and server.status()['status']=='listening','Restarted authority')
        wait(lambda:a.input('inspect')['shared'] and a.input('inspect')['connected'],'Shared rejoin',45)
        require(all(a.input('inspect')[k]==prefs[k] for k in ('quietStill','quietBrightness','quietMusicLevel','quietEffectsLevel')),'Rejoin reset local quiet preferences')
        shared=server.state()['view'];r=shared['secrets'][0]
        require(actor()['zone']=='home-secret-1' and r['furniture']['theme']==1 and r['furniture']['layout']==0 and not r['active'],'Private secret imported')
        require(next(t for t in shared['toys'] if t['id']==item)['container']=='home-secret-1/chest-2','Private plush edits imported')
        require(path.exists() and len(shared['toys'])==33,'Private save lost or creation refilled stock')
        a.input('touchButton',text='Bedroom ←');home.ready(a);wait(lambda:actor()['zone']=='home-bedroom-1','Archived shared escape after restart')
        checks.append('Hidden entrance and occupied secret survive outage/cold reopen; furniture, plush and local calm preferences persist privately; shared reconnect imports no offline edits and always permits exit')
        passed=True
    finally:
        f.cleanup();write(f.path/'secret-continuity.json',dict(passed=passed,build=args.build,previousBuild=args.previous,checks=checks,liveFamilyTouched=False));print('RESULT '+str(f.path/'secret-continuity.json'),flush=True)
if __name__=='__main__':main()
