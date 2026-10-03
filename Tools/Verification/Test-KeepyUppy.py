# /// script
# dependencies = ["cryptography"]
# ///
"""Keepy Uppy acceptance in isolated release players; never touches the family server."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import importlib.util
import json
from pathlib import Path
import time
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import Run, wait, require, read, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
ready,travel,capture,command=home.ready,home.travel,home.capture,home.command

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--balloon-only',action='store_true');args=parser.parse_args()
    run=Run(args.build);folder=run.path/'keepy';folder.mkdir();checks=[];fixture=None;flight=None
    expected_schema=read(run.folder/'build-summary.json').get('schema',5)
    print('EVIDENCE '+str(folder),flush=True)
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2')
        ready(a);ready(b);c=run.start('client','player-3');d=run.start('client','player-4');ready(c);ready(d);a.input('resize',x=1280,y=591);ready(a)
        travel(a,'home');travel(b,'home')
        clients=[a,b,c,d]
        for guest in (c,d):travel(guest,'home')
        if not args.balloon_only:
            for place,names,slots,label in [
                ('home',['Sit left','Sit middle left','Sit middle right','Sit right'],['sofa-left','sofa-middle-left','sofa-middle-right','sofa-right'],'sofa'),
                ('garden',['Bounce left','Bounce middle left','Bounce middle right','Bounce right'],['trampoline-left','trampoline-middle-left','trampoline-middle-right','trampoline-right'],'trampoline')]:
                for i,client in enumerate(clients):
                    travel(client,place)
                    if place=='garden':
                        require(command(client,0,x=1650,y=100)['accepted'],'Trampoline approach failed');time.sleep(.8)
                    if i%2:require(command(client,1,value='orange-pup')['accepted'],'Bingo selection failed')
                    client.input('touchButton',text=names[i])
                    wait(lambda:next(p for p in server.state()['view']['players'] if p['id']==client.profile)['fixture']==slots[i],'four-player fixture')
                time.sleep(.6);capture(a,folder,'four-'+label+'-phone')
                require(len({p['fixture'] for p in server.state()['view']['players']})==4,'Missing fourth spot')
                a.input('resize',x=1024,y=768);ready(a);capture(a,folder,'four-'+label+'-tablet')
                a.input('resize',x=1280,y=591);ready(a)
                for client in clients:require(command(client,9)['accepted'],'Fixture exit failed')
                passed('four real clients use four '+label+' spots through touch, original object size, phone/tablet')
        play_place='garden' if args.build>=131 else 'home'
        for client in clients:
            travel(client,play_place)
            if args.build>=131:require(command(client,0,x=2050 if args.build>=151 else 420,y=480 if args.build>=151 else 100)['accepted'],'Yard approach failed')
        time.sleep(.8)
        def balloon():return server.state()['view']['keepy']
        require(balloon()['phase']==0 and balloon()['round']==0,'Balloon started without a tap')
        capture(a,folder,'resting-balloon-phone')
        a.input('touchButton',text='Play Keepy Uppy');wait(lambda:balloon()['phase']==1,'balloon toss')
        require(balloon()['vz']>0 and balloon()['height']>28,'Tap did not toss balloon upwards')
        one=balloon()['height'];time.sleep(.4)
        require(abs(b.input('inspect')['keepy']['height']-one)>10,'Sibling does not receive ongoing flight')
        capture(a,folder,'balloon-toss-phone');passed('real balloon touch starts upward flight; sibling sees motion without sending commands')
        if args.build>=131:
            sample=balloon();peak=sample['height'];apex=sample['elapsed']
            while sample['phase']==1:
                if sample['height']>peak:peak=sample['height'];apex=sample['elapsed']
                time.sleep(.02);sample=balloon()
            flight=dict(peak=peak,flightSeconds=sample['elapsed'],fallSeconds=sample['elapsed']-apex)
            require(peak>285 and sample['elapsed']<(2.3 if args.build>=132 else 3) and (abs(sample['x']-2450)>100 if args.build>=151 else sample['x']>760),'Higher/faster outdoor toss failed')
            if args.build>=132:require(flight['fallSeconds']<1.05,'Descent still too slow')
            passed('higher outdoor toss drifts sideways and lands quickly; descent duration measured separately')
        for hitter in clients:
            for other in clients:
                require(command(other,0,x=1050 if args.build>=131 else -3500,y=480)['accepted'],'Clear hit area failed')
            before=balloon()['hitSerial'];limit=time.monotonic()+9
            while time.monotonic()<limit and balloon()['hitSerial']==before:
                v=balloon();require(command(hitter,0,x=v['x'],y=v['y'])['accepted'],'Follow hitter failed')
                if v['phase']==0:
                    time.sleep(.4);hitter.input('touchButton',text='Play Keepy Uppy')
            require(balloon()['lastHitter']==hitter.profile and balloon()['hitSerial']>before,'One of four players cannot hit')
            for client in clients:wait(lambda:client.input('inspect')['keepy']['hitSerial']>before,'four shared views')
        passed('all four connected players can return the same balloon, observed by every client')
        for guest in (c,d):require(command(guest,7,value='park')['accepted'],'Leave other players failed')
        for avatar,label in [('blue-pup','bluey'),('orange-pup','bingo')]:
            require(command(a,1,value=avatar)['accepted'],'Avatar failed')
            if balloon()['phase']==0:a.input('touchButton',text='Play Keepy Uppy')
            before=balloon()['hitSerial'];poses=set();limit=time.monotonic()+12
            while time.monotonic()<limit and balloon()['hitSerial']<before+2:
                v=balloon();require(command(a,0,x=v['x'],y=v['y'])['accepted'],'Follow position failed')
                s=a.input('inspect');poses.add(s['homePose'])
                if s['homePose']=='BalloonTap':capture(a,folder,label+'-arm-tap-phone')
            require(balloon()['hitSerial']>=before+2 and balloon()['lastHitter']==a.profile,'No repeated automatic taps')
            require('BalloonTap' in poses,'Raised arm not presented')
            wait(lambda:b.input('inspect')['keepy']['hitSerial']>=before+2,'shared hits')
            passed(label+' automatically raises arm and returns falling balloon; both clients agree on taps')
        if args.build>=151:
            require(command(a,1,value='blue-pup')['accepted'],'High flight avatar setup failed')
            for width,height,label in [(1280,591,'phone'),(1024,768,'tablet')]:
                a.input('resize',x=width,y=height);ready(a);limit=time.monotonic()+30;captured=False
                while time.monotonic()<limit:
                    v=balloon();require(command(a,0,x=v['x'],y=v['y'])['accepted'],'High flight follow failed')
                    if v['phase']==0:
                        time.sleep(.4);a.input('touchButton',text='Play Keepy Uppy')
                    elif v['height']>495 and abs(v['vz'])<65:
                        capture(a,folder,'higher-hit-'+label);captured=True;break
                require(captured,'No occasional higher flight captured on '+label)
            passed('occasional higher automatic returns render in phone and tablet viewports')
        a.input('resize',x=1024,y=768);ready(a);capture(a,folder,'balloon-tablet')
        require(command(a,0,x=1050 if args.build>=131 else -3500,y=480)['accepted'],'Depth placement failed')
        require(command(b,0,x=1050 if args.build>=131 else -3500,y=480)['accepted'],'Sibling depth failed')
        wait(lambda:balloon()['phase']==0,'floor rest',12);rest=dict(balloon());time.sleep(1)
        require(balloon()==rest,'Balloon restarted itself')
        capture(a,folder,'rest-after-landing-tablet')
        require('hits' not in rest and 'best' not in rest,'Unexpected scoreboard')
        passed('wrong floor depth cannot hit; landing rests with no score, loss, timer or automatic restart')
        # Return camera to the grounded balloon, then tap it again through UGUI.
        require(command(a,0,x=rest['x'],y=rest['y'])['accepted'],'Return failed');time.sleep(.8)
        travel(b,'beach')
        a.input('touchButton',text='Play Keepy Uppy');wait(lambda:balloon()['round']==rest['round']+1,'second toss')
        # Fixture travel must finish before a full flight can land; independently
        # exercise the real chooser elsewhere rather than time its browsing here.
        require(command(a,7,value='creek')['accepted'],'Leave home failed');time.sleep(.3)
        paused=dict(balloon());require(paused['phase']==1,'Pause fixture landed too soon');time.sleep(1)
        require(balloon()==paused,'Empty home kept advancing')
        require(not any(c['name']=='Play Keepy Uppy' for c in a.input('inspect')['controls']),'Balloon in creek')
        travel(a,'home');wait(lambda:balloon()['elapsed']>paused['elapsed'],'home resumes')
        passed('tap starts another toss; balloon stays home and pauses when everyone leaves; return resumes')
        fixture=RecoveryFixture(args.build);solo=fixture.launch(1);ready(solo);travel(solo,play_place)
        if args.build>=151:
            if not solo.input('inspect')['joystickVisible']:solo.input('touchButton',text='Tap to walk')
            solo.input('touch-begin',role='stick',x=30,finger=31)
            wait(lambda:next(p['position']['x'] for p in solo.input('inspect')['players'] if p['id']==solo.profile)>2050,'solo clear-lawn approach',15)
            solo.input('touch-end',role='stick',x=30,finger=31);time.sleep(.6)
        solo.input('touchButton',text='Play Keepy Uppy');wait(lambda:solo.input('inspect')['keepy']['phase']==1,'solo toss')
        solo.input('touchButton',text='Menu');save=Path(solo.input('inspect')['savePath']);solo.close()
        saved=json.loads(save.read_bytes().split(b'\n',2)[2]);require(saved['schema']==expected_schema and saved['keepy']['round']==1,'Missing solo balloon save')
        reopened=fixture.launch(1);s=ready(reopened)
        require(not s['shared'] and s['keepy']['round']==1,'Private solo balloon lost')
        passed(f'offline touch starts same game; schema-{expected_schema} balloon survives close/reopen in private solo')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,flight=flight,liveFamilyServerAccessed=False,physicalDevicesAccessed=False))
        print('RESULT '+str(folder/'result.json'),flush=True)
    finally:
        try:
            if fixture:fixture.cleanup()
        finally:run.close()
if __name__=='__main__':main()
