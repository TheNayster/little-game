# /// script
# dependencies = ["cryptography"]
# ///
"""Four riders on isolated PC authority; never touches the live family server."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build);folder=run.path/'park-wheels';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def player(client):return next(v for v in server.state()['view']['players'] if v['id']==client.profile)
        def fixture(client,id):
            wait(lambda:player(client)['fixture']==id and next(v for v in client.state()['view']['players'] if v['id']==client.profile)['fixture']==id,'presented fixture '+id)
            settled(client);time.sleep(.2)
        def settled(client):home.ready(client);wait(lambda:not client.input('inspect')['menuOpen'],'scenery settled')
        def place(client,x):require(home.command(client,0,x=x,y=100)['accepted'],'placement');settled(client);time.sleep(.55)
        for client in clients:
            require(home.command(client,7,value='park')['accepted'],'park travel');settled(client)
        require(home.command(b,1,value='orange-pup')['accepted'],'Bingo selection')
        a.input('resize',x=1024,y=768);settled(a)
        place(a,950);home.capture(a,folder,'clear-slide-swings-ipad')
        a.input('touchButton',text='Climb and slide');wait(lambda:player(a)['fixture'].startswith('park-slide-'),'unblocked slide entrance')
        wait(lambda:player(a)['fixture']=='','slide landing',seconds=8)
        place(a,1510);a.input('touchButton',text='Ride swing 2');fixture(a,'park-swing-1')
        a.input('touchButton',text='Get off');wait(lambda:player(a)['fixture']=='','swing exit')
        place(a,3500);home.capture(a,folder,'parking-bays-ipad')
        record('parked vehicles clear the playhouse, slide landing and swing approaches; native equipment touches work')
        for kind,base in [('bike',0),('scooter',4)]:
            for i,client in enumerate(clients):
                place(client,2500+(base+i)*280);client.input('touchButton',text='Ride '+kind+' '+str(i+1))
                fixture(client,'park-wheels-'+kind+'-'+str(i))
            require(not home.command(b,8,target='park-wheels-'+kind+'-0')['accepted'],'duplicate vehicle lease')
            time.sleep(.8);home.capture(a,folder,'four-'+kind+'-ipad')
            # Keep both destinations in the tablet viewport while its camera follows.
            start=player(a)['x'];right=min(4500,start+180);left=start-120
            # Steer through empty scenery, above fixture and riding-HUD hitboxes.
            # Riding projects the tap onto the existing ground lane.
            a.input('touch-begin',x=right,y=900,finger=32);a.input('touch-end',x=right,y=900,finger=32)
            wait(lambda:abs(player(a)['x']-right)<1,'ride right',seconds=7);time.sleep(.6)
            require(player(a)['y']==85 and player(a)['fixture'].endswith(kind+'-0'),'tap left lane')
            a.input('touch-begin',x=left,y=900,finger=32);a.input('touch-end',x=left,y=900,finger=32)
            wait(lambda:abs(player(a)['x']-left)<1,'ride left',seconds=7);time.sleep(.6)
            home.capture(a,folder,kind+'-left-ipad')
            if not b.input('inspect')['joystickVisible']:b.input('button',text='Tap to walk')
            before=player(b)['x'];b.input('touch-begin',role='stick',x=45,finger=31);time.sleep(.5);b.input('touch-end',role='stick',finger=31)
            require(player(b)['x']>before+40,'joystick ride did not advance')
            require(player(b)['fixture']=='park-wheels-'+kind+'-1','joystick dismounted')
            record('four native '+kind+' mounts, exclusive leases, tap both ways and joystick')
            for client in clients:client.input('touchButton',text='Get off');wait(lambda:player(client)['fixture']=='','native dismount')
        a.input('resize',x=1280,y=591);settled(a);place(a,3900);home.capture(a,folder,'parking-bays-phone')
        a.input('touchButton',text='Ride scooter 2');wait(lambda:player(a)['fixture']=='park-wheels-scooter-1','phone mount')
        place(b,2780)
        b.input('touchButton',text='Ride bike 2');wait(lambda:player(b)['fixture']=='park-wheels-bike-1','bike sibling')
        home.capture(a,folder,'scooter-phone')
        place(d,4460)
        d.input('touchButton',text='Ride scooter 4');wait(lambda:player(d)['fixture']=='park-wheels-scooter-3','departing mount')
        d.close();wait(lambda:player(d)['fixture']=='','departure releases vehicle')
        require(player(a)['fixture']=='park-wheels-scooter-1' and player(b)['fixture']=='park-wheels-bike-1','sibling rides disturbed')
        require(home.command(a,7,value='creek')['accepted'],'independent travel');settled(a)
        require(player(a)['fixture']=='' and player(b)['fixture']=='park-wheels-bike-1','area departure disturbed sibling')
        record('native get-off, phone layout, disconnect and independent area departure')
        require(home.command(a,7,value='park')['accepted'],'preview return');settled(a)
        for kind,base in [('bike',0),('scooter',4)]:
            if player(a)['fixture']:a.input('touchButton',text='Get off')
            if player(b)['fixture']:b.input('touchButton',text='Get off')
            require(home.command(a,8,target='park-wheels-'+kind+'-0')['accepted'],'preview mount a')
            require(home.command(b,8,target='park-wheels-'+kind+'-1')['accepted'],'preview mount b')
            place(a,2800);place(b,3070);a.input('resize',x=1280,y=800);settled(a);time.sleep(.6)
            home.capture(a,folder,kind+'-bluey-bingo')

        passed=True
    finally:
        if not passed:
            for client in clients:
                if client.process.poll() is None:
                    try: home.capture(client,folder,'failure-'+client.profile)
                    except Exception as exc: print('Capture unavailable: '+str(exc),flush=True)
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)

if __name__=='__main__':main()
