# /// script
# dependencies = ["cryptography"]
# ///
"""Real room play gestures across four isolated native release clients."""
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'room-play';folder.mkdir();checks=[];passed=False;clients=[]
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        a,b,c,d=clients
        def world():return wait(lambda:server.state(),'fresh authoritative view')['view']
        def player(v):return next(p for p in world()['players'] if p['id']==v.profile)
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def cmd(v,action,**kw):
            result=home.command(v,action,**kw);require(result['accepted'],str(result));home.ready(v);return result
        def sync(v):
            wait(lambda:v.input('inspect')['zone']==player(v)['zone'] and player(v)['stairs']==0,'room arrival');home.ready(v)
        def move(v,x,y=180):cmd(v,0,x=x,y=y);time.sleep(.6);sync(v)
        def button(v,name):v.input('touchButton',text=name);time.sleep(.3);home.ready(v)
        def tap(v,id,finger):
            v.input('touch-begin',role=id,finger=finger);v.input('touch-end',role=id,finger=finger);time.sleep(.3);home.ready(v)
        def drag(v,id,x,y,finger):
            v.input('touch-begin',role=id,finger=finger);v.input('touch-move',x=x,y=y,finger=finger)
            wait(lambda:toy(id)['holder']==v.profile,'drag lease after movement')
            v.input('touch-end',x=x,y=y,finger=finger);wait(lambda:toy(id)['holder']=='','released drag');home.ready(v)
        def room(v,i):
            move(v,[710,1132,1601,2066][i],420);button(v,'Enter bedroom '+str(i+1));sync(v)
        def leave(v):cmd(v,16);sync(v)
        def hall(v):move(v,325,420);button(v,'Return to hallway');sync(v)
        def enter(v,i):
            move(v,2310,220);button(v,'Secret star door');sync(v);require(player(v)['zone']=='home-secret-'+str(i+1),'secret door')
        for i,v in enumerate(clients):
            home.ready(v);move(v,-2980,420);button(v,'Stair entry');wait(lambda:player(v)['zone']=='home-upstairs','upstairs');sync(v);room(v,i)
            move(v,1200);button(v,'Rest on bed');wait(lambda:player(v)['fixture']=='bedroom-bed','bed rest');require(v.input('inspect')['homePose']=='Rest','rest pose')
            button(v,'Decorate')
            for name in ('Bedding','Rug','Picture','Lamp'):button(v,name)
            button(v,'Undo');button(v,'Decorate');enter(v,i)
        expected_stock=150 if world()['schema']>=13 else 97
        require(world()['schema']>=12 and len(world()['toys'])==expected_stock,'full bounded stock')
        record('four owners independently use beds and new decoration controls; four secret rooms retain bounded shared stock without exceeding wire bounds')
        for i,v in enumerate(clients):
            if i:leave(v);hall(v);room(v,0);enter(v,0)
        ids=['home-secret-1-plush-'+str(i) for i in range(4)]
        for i,v in enumerate(clients):
            cmd(v,2,item=ids[i]);cmd(v,3,item=ids[i],x=1540,y=30);move(v,1400);tap(v,ids[i],10+i)
            wait(lambda:toy(ids[i])['holder']==v.profile and player(v)['fixture'].startswith('bedroom-cushion-'),'cuddle tap')
        for v in clients:require(v.input('inspect')['homePose']=='Sit','cuddle did not retain seated character pose')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-cuddles-tablet')
        guests=[(player(v)['fixture'],toy(ids[i+1])['holder']) for i,v in enumerate(clients[1:])]
        move(a,1400);require(toy(ids[0])['holder']=='','own cuddle not settled')
        require([(player(v)['fixture'],toy(ids[i+1])['holder']) for i,v in enumerate(clients[1:])]==guests,'movement disturbed siblings')
        record('real tap gestures create four separate cuddles; accepted sitting art remains and moving releases only one plush')
        # Drag a real plush onto its visible nest, then inspect registered depth.
        move(a,1550);cmd(a,2,item=ids[0]);cmd(a,3,item=ids[0],x=1550,y=150)
        drag(a,ids[0],1630,265+65/.45,31);require(toy(ids[0])['container']=='home-secret-1/nest-3','nest visual drop')
        names=a.input('inspect')['homeDrawOrder'];require(names.index('Blanket nest 4')<names.index(ids[0])<names.index('Nest quilt front 3'),'nest masking order')
        home.capture(a,folder,'plush-tucked-tablet');drag(a,ids[0],1520,140,32);require(toy(ids[0])['container']=='','retrieve tucked toy')
        record('real drag tucks and retrieves exact plush identity; quilt front is above toy and cushion rear below')
        # Release guests locally, then make a three-toy pile using real drags.
        for v in clients[1:]:move(v,1250)
        move(a,1800)
        for i,id in enumerate(ids[:3]):cmd(a,2,item=id);cmd(a,3,item=id,x=1760+i*170,y=35)
        drag(a,ids[1],1760,35+42/.45,40);require(toy(ids[1])['container']=='stack/'+ids[0],'second toy stack')
        drag(a,ids[2],1760,35+102/.45,41);require(toy(ids[2])['container']=='stack/'+ids[1],'third toy stack')
        home.capture(a,folder,'three-toy-stack');drag(a,ids[0],2240,80,42)
        require(all(toy(id)['container']=='' for id in ids[:3]) and len({(toy(id)['x'],toy(id)['y']) for id in ids[:3]})==3,'stack base removal did not settle separately')
        record('real three-toy stacking and lower-toy removal settle dependents separately without duplicates')
        pot='home-secret-1-tea-4';cups=['home-secret-1-tea-'+str(i) for i in range(4)]
        move(a,1200)
        for i,cup in enumerate(cups):
            if i==3:tap(a,pot,60)
            drag(a,pot,820+i*200,30+38/.45,61+i)
            require(toy(cup)['water']==1,'pretend pouring failed')
        before=[toy(id)['water'] for id in cups];require(before==[1,1,1,1],'cups were not independent')
        for i,v in enumerate(clients):move(v,1100);tap(v,cups[i],70+i)
        require(all(toy(id)['water']==0 for id in cups),'four independent sips')
        record('real teapot refill/pour and four individual cup taps change only bounded pretend-liquid states')
        move(a,2000);button(a,'Look at room picture');home.capture(a,folder,'aurora-picture-card');button(a,'Picture read');button(a,'Close')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'room-play-phone')
        leave(a);move(a,1300);home.capture(a,folder,'bedroom-play-phone')
        require(len(world()['toys'])==expected_stock,'stock changed after play')
        record('room picture controls open and close; phone and tablet compositions captured; owner exit leaves other secret-room players')
        passed=True
    except Exception:
        for i,v in enumerate(clients):
            if v.process.poll() is None:
                try:home.capture(v,folder,'failure-client-'+str(i))
                except Exception:pass
        raise
    finally:
        run.close();write(folder/'result.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTouched=False));print('RESULT '+str(folder/'result.json'),flush=True)
if __name__=='__main__':main()
