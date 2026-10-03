# /// script
# dependencies = ["cryptography"]
# ///
"""All sixteen Zoo exhibits, actual bucket touches and one shared family queue."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,importlib.util,json,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
TRAILS=['zoo-savanna','zoo-dinosaurs','zoo-reptiles','zoo-aquarium']
GROUPS=[['elephant','giraffe','zebra','lion'],['brachiosaurus','triceratops','stegosaurus','tyrannosaurus'],['tortoise','gecko','iguana','crocodile'],['clownfish','blue-tang','zebra-shark','penguin']]
FOODS=dict(elephant='leaves',giraffe='leaves',zebra='hay',lion='meat',brachiosaurus='leaves',triceratops='leaves',stegosaurus='leaves',tyrannosaurus='meat',tortoise='leaves',gecko='insects',iguana='leaves',crocodile='fish',clownfish='pellets',**{'blue-tang':'seaweed','zebra-shark':'fish','penguin':'fish'})

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--menu-only',action='store_true');parser.add_argument('--contact-only',action='store_true');args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'zoo-full';out.mkdir();checks=[];passed=False;seen=[]
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def cmd(v,action,**kw):
        result=home.command(v,action,**kw);require(result['accepted'],str(result));home.ready(v);return result
    def state():return server.state()['view']['zoo']
    def animal(id):return next(a for a in state()['animals'] if a['species']==id)
    def portion(v):return next(f for f in state()['food'] if f['actor']==v.profile)
    def enter(v,area):
        zone=v.input('inspect')['zone']
        if zone in TRAILS:cmd(v,0,x=200,y=100);cmd(v,22,value='gate',target='zoo')
        elif zone!='zoo':cmd(v,7,value='zoo')
        cmd(v,0,x=650 if TRAILS.index(area)%2==0 else 1750,y=100);cmd(v,22,value='gate',target=area)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
        a,b,c,d=clients;home.scenic.NAMES['zoo']='World Zoo';home.travel(a,'zoo');time.sleep(.8);home.capture(a,out,'four-trail-entrance')
        a.input('resize',x=1024,y=768);time.sleep(.5);home.capture(a,out,'four-trail-entrance-tablet')
        for area,name in [('zoo-savanna','Visit the savanna'),('zoo-dinosaurs','Visit Dinosaur Valley'),('zoo-reptiles','Visit Reptile Garden'),('zoo-aquarium','Visit Aquarium')]:
            cmd(a,0,x=650 if TRAILS.index(area)%2==0 else 1750,y=100);time.sleep(.3);a.input('touchButton',text=name)
            wait(lambda:a.input('inspect')['zone']==area,'picture circle opens '+area,12)
            cmd(a,0,x=200,y=100);cmd(a,22,value='gate',target='zoo');time.sleep(.5)
        a.input('resize',x=1280,y=591);home.ready(a)
        record('four illustrated circle choices visible on phone/tablet; each opens its own trail')
        if args.menu_only:passed=True;return
        for round in range(4):
            batch=[]
            for i,v in enumerate(clients):
                id=GROUPS[i][round];area=TRAILS[i];enter(v,area);center=1200+round*2400;cmd(v,0,x=center+360,y=100);batch.append((v,id,animal(id)['fed']))
            time.sleep(1.4)
            for v,id,_ in batch:
                inspected=v.input('inspect');require(1<=inspected['zooTextures']<=3,'Unbounded/missing animal textures '+id)
                v.input('touchButton',text='Take '+FOODS[id]+' for '+id)
            wait(lambda:all(portion(v)['offered'] for v,_,_ in batch),'automatic offers for batch '+str(round),35)
            captured=set();until=time.monotonic()+80
            while len(captured)<4 and time.monotonic()<until:
                for v,id,_ in batch:
                    if id not in captured and animal(id)['phase']==6:
                        home.capture(v,out,id+'-feeding');captured.add(id);seen.append(id)
                time.sleep(.25)
            require(len(captured)==4,'Missed feeding pose: '+str(captured))
            wait(lambda:all(animal(id)['fed']==fed+1 and portion(v)['species']=='' for v,id,fed in batch),'one exact portion for each animal',25)
            for v,id,_ in batch:
                v.input('touchButton',text='Hear '+id);require(v.input('inspect')['zooSoundPlaying'],'No playable sound for '+id)
            record('four trail exhibits render, feed once and play sound: '+', '.join(id for _,id,_ in batch))
            if args.contact_only:passed=True;return
        for v in clients:enter(v,'zoo-dinosaurs');cmd(v,0,x=8760,y=100)
        time.sleep(1.4);before=animal('tyrannosaurus')['fed']
        for v in clients:v.input('touchButton',text='Take meat for tyrannosaurus')
        wait(lambda:all(portion(v)['offered'] for v in clients),'four shared T. rex offerings',35)
        require(len({portion(v)['slot'] for v in clients})==4,'Shared places overlap')
        home.capture(d,out,'four-player-dinosaur-queue')
        cmd(b,7,value='creek');require(portion(b)['species']=='','Departure left stale offer')
        require(all(portion(v)['species']=='tyrannosaurus' for v in (a,c,d)),'Departure canceled siblings')
        wait(lambda:animal('tyrannosaurus')['fed']==before+3 and all(portion(v)['species']=='' for v in (a,c,d)),'remaining three finish once',110)
        record('four-player dinosaur queue keeps siblings feeding when one leaves')
        for area in TRAILS:
            enter(a,area);cmd(a,0,x=9200,y=100);target=TRAILS[(TRAILS.index(area)+1)%4];cmd(a,22,value='gate',target=target)
            require(a.input('inspect')['zone']==target,'Next gate failed');cmd(a,0,x=200,y=100);cmd(a,22,value='gate',target=area)
            require(a.input('inspect')['zone']==area,'Reverse gate failed')
        cmd(a,0,x=9200,y=490);require(next(p for p in server.state()['view']['players'] if p['id']==a.profile)['y']==120,'Visitor entered habitat')
        cmd(a,7,value='park');wait(lambda:a.input('inspect')['zooTextures']==0 and a.input('inspect')['zooAudioClips']==0,'Zoo cache release')
        record('all bidirectional trail links, visitor boundary and texture/audio departure release')
        passed=True
    finally:
        if not passed:
            for v in locals().get('clients',[]):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+v.profile)
                    except Exception:pass
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,speciesSeen=seen,liveFamilyTouched=False,physicalDevicesTested=False,menuOnly=args.menu_only,contactOnly=args.contact_only))
        print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
