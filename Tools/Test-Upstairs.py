# /// script
# dependencies = ["cryptography"]
# ///
"""Working stairs in four isolated native clients; never the live family world."""
import argparse
from copy import deepcopy
import importlib.util
from pathlib import Path
import time
import uuid
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'upstairs';folder.mkdir();checks=[];passed=False
    # Exercise production-length profile IDs in four simultaneous motion packets.
    for slot in run.slots:slot['profile']=uuid.uuid4().hex
    write(run.path/'slots.json',run.slots)
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots]
        for c in clients:home.ready(c)
        a,b,c,d=clients
        def player(v):return next(p for p in server.state()['view']['players'] if p['id']==v.profile)
        def place(v,x,y=420):
            require(home.command(v,0,x=x,y=y)['accepted'],'Fixture position')
            time.sleep(.3);home.ready(v)
        def entry(v):
            place(v,-2980 if player(v)['zone']=='garden' else 540,420 if player(v)['zone']=='garden' else 400)
        def start(v):
            v.input('touchButton',text='Stair entry')
            wait(lambda:player(v).get('stairs',0)>0,'visible stair traversal starts')
        def arrived(v,zone):
            wait(lambda:player(v)['zone']==zone and player(v).get('stairs',0)==0,'stairs arrive')
            state=home.ready(v);require(state['zone']==zone,'Client presents destination');return state
        def record(text):checks.append(text);print('PASS '+text,flush=True)
        entry(a);sibling=deepcopy(player(b));a.input('resize',x=1280,y=591);home.ready(a)
        home.capture(a,folder,'stairs-source-phone');start(a)
        time.sleep(.65);home.capture(a,folder,'stairs-up-contact-phone')
        state=arrived(a,'home-upstairs')
        require(player(b)==sibling,'Stairs moved downstairs sibling')
        require(state['place']=='home' and state['visiblePlayers']==1,'Home destination or room filtering')
        require(len(state['residentScenery'])+state['pendingScenery']<=3,'Scenery residency')
        home.capture(a,folder,'landing-phone')
        record('real touch starts visible ascent; local landing keeps sibling downstairs and stays within scenery budget')
        a.input('touchButton',text='Characters');time.sleep(.2)
        before=deepcopy(player(a));a.input('touchButton',text='World Heeler Home');home.ready(a)
        require(player(a)==before,'Home chooser teleported upstairs player')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'landing-tablet')
        record('one Home destination resumes upstairs; phone and tablet landing captures')
        for v in clients:entry(v)
        start(a)
        for v in (b,c,d):start(v)
        arrived(a,'garden')
        for v in (b,c,d):arrived(v,'home-upstairs')
        require(len({player(v)['x'] for v in (b,c,d)})==3,'Overlapping arrival anchors')
        require(b.input('inspect')['visiblePlayers']==3 and a.input('inspect')['visiblePlayers']==1,'Room visibility')
        record('four simultaneous opposite-direction trips; separate safe arrivals and local avatar visibility')
        entry(a);require(home.command(a,2,item='bucket-1')['accepted'],'Carry fixture')
        start(a);arrived(a,'home-upstairs')
        toy=next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')
        require(toy['holder']==a.profile and toy['zone']=='home-upstairs','Carried identity lost')
        require(home.command(a,3,item='bucket-1',x=750,y=150)['accepted'],'Upstairs drop')
        time.sleep(.3);home.capture(a,folder,'four-upstairs-and-carried-bucket')
        require(a.input('inspect')['visiblePlayers']==4,'Four visitors not visible')
        record('all four share landing; borrowed item transfers with same ID and can be dropped upstairs')
        # A native drag in the room with no water stations must still drop safely.
        a.input('touch-begin',role='bucket-1',finger=41)
        wait(lambda:next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']==a.profile,'upstairs pickup')
        a.input('touch-move',x=850,y=200,finger=41)
        a.input('touch-end',x=850,y=200,finger=41)
        wait(lambda:next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']=='','upstairs drop UI')
        record('real upstairs pickup/drop works without downstairs interaction targets')
        # Walk onto the real downstairs stair entry using the unchanged joystick.
        entry(a);start(a);arrived(a,'garden');place(a,-2980,350)
        if not a.input('inspect')['joystickVisible']:a.input('touchButton',text='Tap to walk')
        a.input('touch-begin',role='stick',x=0,y=55,finger=61)
        wait(lambda:player(a).get('stairs',0)>0,'joystick stair entry')
        a.input('touch-end',role='stick',finger=61);arrived(a,'home-upstairs')
        record('joystick entry uses the same stair route as tapping')
        # Cancellation of an in-flight per-player trip never rewinds other actors.
        entry(a);start(a);require(home.command(a,12)['accepted'],'Cancel traversal')
        time.sleep(2.7);require(player(a)['zone']=='home-upstairs' and player(a)['stairs']==0,'Cancelled trip left source')
        others=[deepcopy(player(v)) for v in (b,c,d)]
        entry(a);start(a);a.close();time.sleep(.6)
        require(player(a)['stairs']==0 and player(a)['zone']=='home-upstairs','Detach left transit active')
        require([player(v) for v in (b,c,d)]==others,'Detach moved siblings')
        record('cancel and disconnect settle the traveler at source and preserve all other players')
        passed=True
    finally:
        write(folder/'result.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,
            limits='Native Windows release clients. Bedrooms/furnishings, physical devices and visual user acceptance remain pending.'))
        run.close()
        print('RESULT '+str(folder/'result.json'),flush=True)

if __name__=='__main__':main()
