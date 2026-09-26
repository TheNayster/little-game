# /// script
# dependencies = ["cryptography"]
# ///
"""Isolated release-player acceptance for scenic travel, camera and retained state."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import time
import uuid
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import Run, wait, require, write, read

NAMES={'home':'World Heeler Home','garden':'World Garden','park':'World Playground & Park','creek':'World Creek','beach':'World The Beach','daycare':'World Daycare'}

def ready(client):
    return wait(lambda: (s if (s:=client.input('inspect'))['ready'] and s.get('sceneryReady') and not s['worldLoading'] and not s['pending'] else None),'scenic readiness',30)

def swipe(client,up=True):
    s=client.input('inspect');w,h=s['screenWidth'],s['screenHeight'];x=w-100*h/800
    a,b=(h*.55,h*.81) if up else (h*.81,h*.55)
    client.input('touch-begin',role='screen',x=x,y=a,finger=52)
    for f in (.15,.4,.7,1):client.input('touch-move',role='screen',x=x,y=a+(b-a)*f,finger=52)
    time.sleep(.2);client.input('touch-end',role='screen',x=x,y=b,finger=52);time.sleep(.25)

def travel(client,place):
    client.input('touchButton',text='Characters')
    name=NAMES[place]
    # Home/Garden are toward the top. Later destinations are below them.
    for _ in range(16):
        s=client.input('inspect');control=next((c for c in s['controls'] if c['name']==name),None)
        if control and control['bounds']['height']>=control['bounds']['width']*.97:break
        swipe(client,place not in ('home','garden'))
    else:raise AssertionError('Could not reveal '+place)
    require(control['enabled'],'Destination disabled')
    client.input('touchButton',text=name)
    s=ready(client);require(s['place']==place,'Wrong place')
    require('scenery-ready' in s['travelStages'],'No scenic readiness barrier')
    return s

def capture(client,folder,name):
    target=client.out/'garden.png';old=target.stat().st_mtime_ns if target.exists() else 0
    s=client.input('capture');wait(lambda:target.exists() and target.stat().st_mtime_ns!=old,'new capture')
    shutil.copyfile(target,folder/(name+'.png'));write(folder/(name+'.json'),s)
    require(s['controlsInSafeArea'],'Controls outside screen '+name)
    require(len(s['residentScenery'])+s['pendingScenery']<=3,'Scenery cache exceeded')
    return s

def command(client,action,**kw):
    state=client.state();p=next(p for p in state['view']['players'] if p['id']==client.profile);rid=uuid.uuid4().hex
    data=dict(requestId=rid,actor=client.profile,expectedRevision=state['view']['revision'],action=action,zone=p['zone'],visit=p['visit'],item='',target='',value='',x=0,y=0);data.update(kw)
    client.serial+=1;write(client.out/'control.json',dict(serial=client.serial,kind='command',request=dict(requestId=rid,protocol=3,command=data)))
    return wait(lambda:read(client.out/('reply-'+rid+'.json')),'test transaction')

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'scenery';folder.mkdir();checks=[];fixture=None
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2');ready(a);ready(b)
        a.input('resize',x=1280,y=591);ready(a)
        def player(client):return next(p for p in server.state()['view']['players'] if p['id']==client.profile)
        b.input('touch-begin',role='bucket-1',finger=32);b.input('touch-end',x=150,y=340,finger=32)
        wait(lambda:not b.input('inspect')['pending'] and not b.input('inspect')['dragging'],'watering settles')
        require(next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['water']==3,'Scenic bucket no longer fills')
        b.input('touch-begin',role='bucket-1',finger=33)
        wait(lambda:not b.input('inspect')['pending'],'sibling holds filled bucket')
        passed('existing bucket interaction works through the scenic coordinate mapping')
        sibling=dict(player(b));original_toys=server.state()['view']['toys']
        for place in ('home','garden','park','creek','beach','daycare'):
            travel(a,place);capture(a,folder,place+'-arrival-phone')
            require(player(b)==sibling,'Travel moved sibling')
            require(server.state()['view']['toys']==original_toys,'Travel changed props')
        passed('six touch-selected destinations with scenery readiness, bounded assets, sibling and item continuity')
        # Reach each illustrated segment using authoritative test transactions;
        # real touch walking is separately checked across the house boundary.
        for place,x,label in [('home',-3500,'home-living'),('home',-1200,'home-kitchen'),('garden',1200,'garden-tree'),('garden',3500,'garden-shed'),('park',1200,'park-playground'),('park',3500,'park-picnic'),('creek',1200,'creek-bank'),('creek',3500,'creek-crossing'),('beach',1200,'beach-dunes'),('beach',3500,'beach-rockpools'),('daycare',1200,'daycare-playroom'),('daycare',3500,'daycare-garden')]:
            if a.input('inspect')['place']!=place:travel(a,place)
            require(command(a,0,x=x,y=160)['accepted'],'Test placement rejected');time.sleep(.8);ready(a);capture(a,folder,label+'-phone')
        passed('all twelve installed panorama segments render in the real release player')
        a.input('touch-begin',x=3750,y=160,finger=34);a.input('touch-end',x=3750,y=160,finger=34)
        wait(lambda:abs(player(a)['x']-3750)<2,'tap walking beyond the old floor limit',10)
        require(player(b)==sibling,'Long walk affected sibling')
        passed('real tap walking reaches a destination beyond the old 1000-unit board')
        travel(a,'garden');command(a,0,x=20,y=180);time.sleep(.8)
        before=dict(player(a));a.input('touchButton',text='Tap to walk')
        a.input('touch-begin',role='stick',x=-45,finger=41)
        wait(lambda:player(a)['x']<-50,'walk into house');a.input('touch-end',role='stick',finger=41);time.sleep(.5)
        require(player(a)['zone']=='garden' and player(a)['visit']==before['visit'],'House boundary traveled or cloned area')
        require(player(b)==sibling,'House walk moved sibling')
        capture(a,folder,'house-yard-boundary')
        passed('joystick crosses house and backyard as one continuous property without changing visit or siblings')
        # Pan the ground through native touch; this must not issue a walk.
        before=dict(player(a));s=a.input('inspect');start=s['cameraX']
        a.input('touch-begin',role='screen',x=760,y=310,finger=42)
        for x in (690,570,430,300):a.input('touch-move',role='screen',x=x,y=310,finger=42)
        a.input('touch-end',role='screen',x=300,y=310,finger=42);time.sleep(.4)
        require(abs(a.input('inspect')['cameraX']-start)>300 and player(a)==before,'Pan moved player or failed')
        a.input('touchButton',text='Characters');capture(a,folder,'phone-chooser');a.input('touchButton',text='Close characters')
        a.input('resize',x=1024,y=768);ready(a);capture(a,folder,'tablet-property')
        a.input('touchButton',text='Characters');capture(a,folder,'tablet-chooser');a.input('touchButton',text='Close characters')
        passed('independent touch camera panning and phone/tablet controls remain inside safe area')
        fixture=RecoveryFixture(args.build);solo=fixture.launch(1);s=ready(solo);require(not s['shared'],'Expected isolated offline player')
        travel(solo,'home');s=ready(solo);save_path=Path(s['savePath']);solo.input('touchButton',text='Menu')
        raw=save_path.read_bytes();header,digest,payload=raw.split(b'\n',2);require(hashlib.sha256(payload).hexdigest().encode()==digest,'Bad saved checkpoint')
        saved=json.loads(payload);solo.close();reopened=fixture.launch(1);s=ready(reopened)
        require(s['place']=='home' and saved['schema']==3 and saved['players'][0]['x']<0,'Offline home not retained')
        capture(reopened,folder,'offline-home-reopened')
        passed('offline travel saves schema 3 with negative home position and reopens in the same property')
        result=dict(passed=True,build=args.build,checks=checks,evidence=str(folder),physicalDevicesAccessed=False,liveFamilyServerAccessed=False)
        write(folder/'result.json',result);print(json.dumps(result),flush=True)
    finally:
        try:
            if fixture:fixture.cleanup()
        finally:run.close()

if __name__=='__main__':main()
