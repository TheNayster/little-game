# /// script
# dependencies = ["cryptography"]
# ///
"""Four native room destinations and ownership; disposable loopback world only."""
import argparse
from copy import deepcopy
import importlib.util
from pathlib import Path
import time
import uuid
from shared_garden_runtime import Run,wait,require,write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
HALL_X=[710,1132,1601,2066]

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'bedrooms';folder.mkdir();checks=[];passed=False
    for slot in run.slots:slot['profile']=uuid.uuid4().hex
    write(run.path/'slots.json',run.slots)
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for c in clients:home.ready(c)
        a,b,c,d=clients
        def world():return server.state()['view']
        def player(v):return next(p for p in world()['players'] if p['id']==v.profile)
        def move(v,x,y=420):
            require(home.command(v,0,x=x,y=y)['accepted'],'Fixture positioning rejected');time.sleep(.4);home.ready(v)
        def room(v,index):
            target='home-bedroom-'+str(index+1);move(v,HALL_X[index]);v.input('touchButton',text='Enter bedroom '+str(index+1))
            wait(lambda:player(v)['zone']==target,'bedroom door arrival');return home.ready(v)
        def exit(v):
            move(v,325);v.input('touchButton',text='Return to hallway')
            wait(lambda:player(v)['zone']=='home-upstairs','bedroom exit');home.ready(v)
        owners=deepcopy(world()['bedrooms'])
        require(len(owners)==4 and [r['owner'] for r in owners]==[s['profile'] for s in run.slots],'Initial owner mapping')
        require(home.command(a,2,item='ball-1')['accepted'],'Carry ball setup')
        for v in clients:
            move(v,-2980);v.input('touchButton',text='Stair entry')
            wait(lambda:player(v)['zone']=='home-upstairs','Upstairs setup');home.ready(v)
        a.input('resize',x=1280,y=591);home.ready(a);move(a,710)
        home.capture(a,folder,'four-owned-doors-phone')
        for i,v in enumerate(clients):
            s=room(v,i);require(s['place']=='home' and s['visiblePlayers']==1,'Independent room visibility')
            require(len(s['residentScenery'])+s['pendingScenery']<=3,'Room art residency budget')
        require(len({player(v)['zone'] for v in clients})==4 and world()['bedrooms']==owners,'Separate rooms or owners changed')
        record('four real door buttons enter four distinct owned bedrooms independently with bounded local art')
        ball=next(t for t in world()['toys'] if t['id']=='ball-1')
        require(ball['holder']==a.profile and ball['zone']==player(a)['zone'],'Door lost carried identity')
        require(home.command(a,3,item='ball-1',x=850,y=200)['accepted'],'Bedroom ball drop')
        time.sleep(.3);home.capture(a,folder,'bedroom-phone')
        a.input('touch-begin',role='ball-1',finger=51)
        wait(lambda:next(t for t in world()['toys'] if t['id']=='ball-1')['holder']==a.profile,'Bedroom real pickup')
        a.input('touch-move',x=980,y=170,finger=51);a.input('touch-end',x=980,y=170,finger=51)
        wait(lambda:next(t for t in world()['toys'] if t['id']=='ball-1')['holder']=='','Bedroom real drop')
        record('carried ball keeps one identity through stairs and room door; real pickup/drop works inside')
        for v in clients:
            require(home.command(v,1,value='orange-pup')['accepted'],'Avatar change')
        require(world()['bedrooms']==owners,'Avatar changed ownership')
        a.input('touchButton',text='Characters');time.sleep(.2);before=deepcopy(player(a))
        a.input('touchButton',text='World Heeler Home');home.ready(a)
        require(player(a)==before,'Home chooser moved bedroom occupant')
        record('same favorite avatar on all four profiles preserves owners; Home chooser stays in current bedroom')
        for v in (b,c,d):exit(v);room(v,0)
        require(len({player(v)['x'] for v in clients})==4,'Four arrivals overlap')
        wait(lambda:a.input('inspect')['visiblePlayers']==4,'Four visitors appear')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-bedroom-visitors-tablet')
        record('all four can gather inside any owned bedroom with distinct safe arrival anchors')
        guests=[deepcopy(player(v)) for v in (b,c,d)];exit(a);home.travel(a,'park')
        require([player(v) for v in (b,c,d)]==guests and world()['bedrooms']==owners,'Owner departure changed visitors or room')
        a.close();time.sleep(.6);require([player(v) for v in (b,c,d)]==guests,'Owner disconnect moved visitors')
        require(b.input('inspect')['visiblePlayers']==3,'Guests disappeared with owner')
        record('owner can leave for another world and disconnect while three visitors continue in the same saved room')
        exit(b);room(b,0)
        require(next(t for t in world()['toys'] if t['id']=='ball-1')['zone']=='home-bedroom-1','Room item lost on revisit')
        require(world()['bedrooms']==owners and len(world()['toys'])==(27 if world()['schema']>=8 else 11),'Visit duplicated rooms or stock')
        record('leaving and revisiting uses the existing room and retained item without allocating a duplicate')
        passed=True
    finally:
        run.close();write(folder/'result.json',dict(passed=passed,build=args.build,checks=checks,profileIdLength=32,liveFamilyTouched=False,
            limits='Windows native destinations/ownership pass. Furniture/decor/storage have separate tests; physical device qualification remains separate.'))
        print('RESULT '+str(folder/'result.json'),flush=True)

if __name__=='__main__':main()
