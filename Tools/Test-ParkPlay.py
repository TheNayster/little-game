# /// script
# dependencies = ["cryptography"]
# ///
"""Focused native four-player park check; uses an isolated server and saves."""
import argparse
import importlib.util
import time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build);folder=run.path/'park';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def player(client):return next(v for v in server.state()['view']['players'] if v['id']==client.profile)
        def settled(client):home.ready(client);wait(lambda:not client.input('inspect')['menuOpen'],'scenery settled')
        def place(client,x):require(home.command(client,0,x=x,y=100)['accepted'],'placement');settled(client);time.sleep(.5)
        for client in clients:
            home.ready(client);require(home.command(client,7,value='park')['accepted'],'park travel');settled(client)
        require(home.command(b,1,value='orange-pup')['accepted'],'Bingo selection')
        a.input('resize',x=1280,y=591);settled(a)
        place(a,650);a.input('touchButton',text='Climb and slide')
        wait(lambda:player(a)['fixture'].startswith('park-slide'),'slide started');time.sleep(1)
        home.capture(a,folder,'climb-phone')
        wait(lambda:player(a)['fixture']=='','automatic slide landing');require(abs(player(a)['x']-944)<2,'slide landing position')
        record('native tap starts climb/platform/slide and lands automatically')
        for i,client in enumerate(clients):
            place(client,1510);client.input('touchButton',text='Ride swing '+str(i+1));wait(lambda:player(client)['fixture']=='park-swing-'+str(i),'swing lease')
        home.capture(a,folder,'four-swings-phone')
        a.input('resize',x=1024,y=768);settled(a);home.capture(a,folder,'four-swings-ipad')
        require(not home.command(b,8,target='park-swing-0')['accepted'],'same seat admitted twice')
        poses=[]
        for _ in range(4):
            state=a.input('capture');poses.append(state.get('homePose'));time.sleep(.12)
        record('four native swing targets, exclusive seats and phone/iPad composition')
        for client in clients:
            place(client,2440);client.input('touchButton',text='Ride merry-go-round');wait(lambda:player(client)['fixture'].startswith('park-roundabout'),'roundabout boarding')
        wait(lambda:server.state()['view']['park']['speed']>.4,'roundabout acceleration')
        home.capture(a,folder,'four-roundabout-ipad')
        angles=[]
        for _ in range(4):angles.append(server.state()['view']['park']['angle']);time.sleep(.15)
        require(max(angles)-min(angles)>.1,'roundabout stalled')
        a.input('touchButton',text='Roundabout speed');wait(lambda:server.state()['view']['park']['targetSpeed']==0,'shared stop')
        a.input('touchButton',text='Roundabout speed');wait(lambda:server.state()['view']['park']['targetSpeed']>0,'shared turn')
        d.close();wait(lambda:player(d)['fixture']=='','departed ride released')
        require(all(player(v)['fixture'].startswith('park-roundabout') for v in (a,b,c)),'departure disturbed other riders')
        record('one shared moving roundabout, shared stop/restart and independent departure')
        a.input('touchButton',text='Get off');wait(lambda:player(a)['fixture']=='','get off')
        place(a,3070);a.input('touchButton',text='Fountain water');wait(lambda:server.state()['view']['park']['waterUntil']>server.state()['view']['park']['clock'],'water arc')
        a.input('touch-begin',role='bucket-park',finger=22);wait(lambda:next(t for t in server.state()['view']['toys'] if t['id']=='bucket-park')['holder']==a.profile,'bucket pickup')
        a.input('touch-end',x=3070,y=250,finger=22);wait(lambda:next(t for t in server.state()['view']['toys'] if t['id']=='bucket-park')['water']==3,'fountain fills bucket')
        home.capture(a,folder,'fountain-ipad');record('native fountain tap and shared bucket water transfer')
        for station,x,label in [('picnic',3630,'Sit for picnic'),('bench',4230,'Sit on bench')]:
            place(a,x);a.input('touchButton',text=label);wait(lambda:player(a)['fixture'].startswith('park-'+station),'sit '+station);home.capture(a,folder,station+'-ipad')
        require(home.command(a,7,value='creek')['accepted'],'independent travel');settled(a)
        require(player(b)['fixture'].startswith('park-roundabout'),'traveller disturbed sibling')
        record('bench/picnic seating and independent major-world travel')
        passed=True
    finally:
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)

if __name__=='__main__':main()
