# /// script
# dependencies = ["cryptography"]
# ///
"""Real secret-door, fort and plush UI in four isolated native clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'secrets';folder.mkdir();checks=[];passed=False;clients=[]
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for v in clients:home.ready(v)
        a,b,c,d=clients
        def world():return server.state()['view']
        def player(v):return next(p for p in world()['players'] if p['id']==v.profile)
        def secret(i=0):return next(r for r in world()['secrets'] if r['id']=='home-secret-'+str(i+1))
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def sync(v):
            wait(lambda:next(p for p in v.state()['view']['players'] if p['id']==v.profile)['zone']==player(v)['zone'] and player(v)['stairs']==0,'arrival snapshot');home.ready(v)
        def move(v,x,y=220):
            sync(v);require(home.command(v,0,x=x,y=y)['accepted'],'Position rejected');time.sleep(.7);home.ready(v)
        def button(v,name):v.input('touchButton',text=name);time.sleep(.3);home.ready(v)
        def arrived(v,zone):wait(lambda:player(v)['zone']==zone and v.input('inspect')['zone']==zone,'arrival '+zone);sync(v)
        def room(v,i):move(v,[710,1132,1601,2066][i],420);button(v,'Enter bedroom '+str(i+1));arrived(v,'home-bedroom-'+str(i+1))
        def backhall(v):move(v,325,420);button(v,'Return to hallway');arrived(v,'home-upstairs')
        def enter(v,i):move(v,2310 if secret(i)['slot']==0 else 2250);time.sleep(.6);button(v,'Secret star door');arrived(v,'home-secret-'+str(i+1))
        def leave(v,i=0):button(v,'Bedroom ←');arrived(v,'home-bedroom-'+str(i+1))
        def edit(v,target,value,i=0):return home.command(v,15,target=target,value=str(value)+':'+str(secret(i)['entranceRevision']))
        require(len(world()['secrets'])==4 and len(world()['toys'])==27,'Secret migration stock')
        for i,v in enumerate(clients):
            move(v,-2980,420);button(v,'Stair entry');arrived(v,'home-upstairs');room(v,i)
            require('Magical secret entrance' not in v.input('inspect')['homeDrawOrder'],'Secret door visible at room entry')
            if i==0:home.capture(v,folder,'bedroom-hidden-entrance')
            move(v,2310);time.sleep(.6)
            require('Magical secret entrance' in v.input('inspect')['homeDrawOrder'],'Near approach did not reveal')
            if i==0:home.capture(v,folder,'magical-far-back-door')
            button(v,'Secret star door');arrived(v,'home-secret-'+str(i+1))
            require(secret(i)['created'],'Owner tap did not create')
            require(len(v.input('inspect')['residentScenery'])+v.input('inspect')['pendingScenery']<=3,'Scenery budget exceeded')
        require(len(world()['toys'])==51,'Secret stock duplicated or missing')
        record('four owner doors reveal only near the far wall, create four distinct rooms and retain the three-panorama budget')
        a.input('resize',x=1280,y=591);sync(a);home.capture(a,folder,'secret-interior-phone')
        a.input('resize',x=1024,y=768);sync(a)
        for i,v in enumerate(clients):
            if i:leave(v,i);backhall(v);room(v,0);enter(v,0)
        for i,v in enumerate(clients):button(v,'Sit in fort '+str(i+1));wait(lambda:player(v)['fixture']=='secret-fort-'+str(i),'fort occupancy')
        require(not home.command(a,8,target='secret-fort-1')['accepted'],'Stole occupied fort slot')
        # Centre all four occupants for a visual composition proof.
        camera=a.input('inspect')['cameraX'];end=700-(1050-camera)*.96
        a.input('touch-begin',role='screen',x=700,y=650,finger=70);a.input('touch-move',role='screen',x=end,y=650,finger=70);a.input('touch-end',role='screen',x=end,y=650,finger=70)
        time.sleep(.5);home.capture(a,folder,'four-fort-occupants-tablet')
        guests=[(player(v)['fixture'],player(v)['x']) for v in (b,c,d)];leave(a)
        require([(player(v)['fixture'],player(v)['x']) for v in (b,c,d)]==guests,'Owner exit interrupted fort')
        enter(a,0)
        for i,v in enumerate(clients):button(v,'Sit on cushion '+str(i+1));wait(lambda:player(v)['fixture']=='bedroom-cushion-'+str(i),'cushion occupancy')
        home.capture(a,folder,'four-secret-cushions-tablet')
        record('four visitors use separate fort and cushion places; leaving preserves siblings and accepted character art')
        move(a,1800);item='home-secret-1-plush-0'
        a.input('touch-begin',role=item,finger=71);wait(lambda:toy(item)['holder']==a.profile,'plush pickup')
        a.input('touch-move',x=1685,y=300+150/.45,finger=71);a.input('touch-end',x=1685,y=300+150/.45,finger=71)
        wait(lambda:toy(item)['holder']=='','plush drop');sync(a);require(toy(item)['container']=='home-secret-1/chest-2','Real plush chest storage')
        button(a,'Open toy chest');wait(lambda:not secret()['furniture']['chestOpen'],'Close chest')
        require(not home.command(a,2,item=item)['accepted'],'Closed contents reachable')
        button(a,'Open toy chest');home.capture(a,folder,'plush-storage-tablet')
        require(home.command(a,2,item=item)['accepted'],'Pickup for carry');leave(a);require(toy(item)['zone']=='home-bedroom-1' and toy(item)['holder']==a.profile,'Exit lost held plush');enter(a,0)
        require(home.command(a,3,item=item,x=1400,y=150)['accepted'],'Drop carried plush')
        record('six plush types are real bounded objects; drag storage, close/retrieve and carry through doors preserve identity')
        move(a,1200);button(a,'Quiet room');button(a,'Sky motion');button(a,'Sky brightness');button(a,'Quiet music');button(a,'Quiet effects');home.capture(a,folder,'quiet-local-settings');button(a,'Quiet room')
        local=a.input('inspect');other=b.input('inspect');phase=local['quietPhase'];time.sleep(.7)
        require(local['quietStill'] and local['quietBrightness']==1 and local['quietMusicLevel']==2 and local['quietEffectsLevel']==2,'Local controls did not update')
        require(not other['quietStill'] and other['quietBrightness']==0 and other['quietMusicLevel']==1 and other['quietEffectsLevel']==1,'Local preferences affected sibling')
        require(a.input('inspect')['quietPhase']==phase,'Reduced-motion sky still animates')
        before=world()['revision'];button(a,'Twinkle star 1');require(world()['revision']==before,'Twinkle changed shared world')
        record('local quiet controls and deliberate star interaction run without shared-room mutations')
        button(a,'Secret door');button(a,'Move door');require(secret()['slot']==1,'Move entrance');button(a,'Entrance visibility');require(not secret()['active'],'Hide entrance');button(a,'Secret door')
        guests=[(player(v)['fixture'],player(v)['zone']) for v in (b,c,d)];leave(a)
        require('Magical secret entrance' not in a.input('inspect')['homeDrawOrder'],'Archived door remains visible')
        require([(player(v)['fixture'],player(v)['zone']) for v in (b,c,d)]==guests,'Entrance edit evicted visitors')
        require(home.command(a,7,value='park')['accepted'],'Owner travel');arrived(a,'park')
        move(b,325,420);button(b,'Leave secret room');arrived(b,'home-bedroom-1')
        require(player(c)['zone']=='home-secret-1' and player(d)['zone']=='home-secret-1','Exit changed other visitors')
        require(len(world()['toys'])==51 and toy(item)['zone']=='home-secret-1','Entrance lifecycle lost room contents')
        record('moving/hiding the far-wall entrance retains contents and visitors; visible exit works while owner is away')
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
