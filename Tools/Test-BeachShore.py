"""One native four-client beach check in a disposable loopback family."""
import argparse, importlib.util, shutil, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--visual-floor',action='store_true');args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'shore';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
    def capture(v,name):
        file=v.out/'garden.png';old=file.stat().st_mtime_ns if file.exists() else 0
        s=v.input('capture');wait(lambda:file.exists() and file.stat().st_mtime_ns!=old,'new screenshot')
        shutil.copyfile(file,out/(name+'.png'));write(out/(name+'.json'),s)
        require(s['sceneryReady'] and len(s['residentScenery'])+s['pendingScenery']<=3,'Scenery/cache issue')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def shore():return server.state()['view']['shore']
        home.travel(a,'beach')
        for v in (b,c,d):cmd(v,7,value='beach')
        a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768)
        for k,v in enumerate(clients):
            cmd(v,0,x=600+k*170,y=200+k*45)
            for i in range(1,8):cmd(v,0,x=600+k*170+i*35,y=200+k*45)
        wait(lambda:all(len(v.input('inspect')['shore']['prints'])>=20 for v in clients),'four replicated dry trails')
        require(len({p['actor'] for p in shore()['prints']})==4,'Missing child trail')
        capture(d,'01-four-trails-tablet');record('four children create and see one shared dry-sand trail field')
        # Exercise the real walking stream too, not only durable placement commands.
        if not a.input('inspect')['joystickVisible']:a.input('touchButton',text='Tap to walk')
        before=len(shore()['prints']);a.input('touch-begin',role='stick',x=45,finger=21);time.sleep(.65);a.input('touch-end',role='stick',finger=21)
        wait(lambda:len(shore()['prints'])>before,'real joystick produces footsteps')
        record('native joystick walking emits shared footsteps without growing motion datagrams')
        cmd(c,0,x=1100,y=480)
        # Wait for retreat so the new wet prints can be captured before washing.
        wait(lambda:shore()['clock']%9<1,'retreated wave',12)
        for i in range(1,6):cmd(c,0,x=1100+i*35,y=480)
        capture(c,'02-wet-prints-phone')
        require(any(p['y']>=430 for p in shore()['prints']),'Missing wet prints')
        wait(lambda:not any(p['y']>=430 for p in shore()['prints']),'wave washes wet trail',12)
        require(any(p['y']<430 for p in shore()['prints']),'Wave erased dry trail')
        capture(c,'03-washed-wet-strip-phone');record('moving wave washes wet prints; dry sand and sibling trails remain')
        cmd(d,0,x=1250,y=410)
        d.input('touchButton',text='Splash water 6')
        wait(lambda:any(p['actor']==d.profile for p in shore()['ripples']),'real water tap ripple')
        capture(d,'04-shared-ripple-tablet');record('real native water tap creates a bounded shared ripple')
        if args.visual_floor:
            passed=True
            record('focused beach floor correction: four trails, wet washing, walking and phone/tablet captures')
            return
        wait(lambda:shore()['visits']>0 and shore()['clock']-shore()['visitStarted']<2.5,'random first sea sighting',48)
        event=shore();number=event['visits'];kind=event['visitor'];seen={kind};types={0:'whale',1:'dolphin',2:'mermaid'}
        wait(lambda:all(v.input('inspect')['shore']['visits']==number for v in clients),'shared visitor replicated')
        require(all(v.input('inspect')['shore']['visitor']==kind for v in clients),'Separate per-client random events')
        time.sleep(.55);capture(d,'05-'+types[kind]+'-tablet')
        cmd(a,7,value='creek');b.close();time.sleep(.3)
        require(shore()['visits']==number and shore()['visitX']==event['visitX'],'Departure reset event')
        record('one common random sighting for four players; independent travel/disconnect preserve it')
        require(a.input('inspect')['visibleSandPrints']==0 and not a.input('inspect')['visibleSeaVisitor'],'Beach art remains visible in creek')
        deadline=time.monotonic()+220
        while len(seen)<3 and time.monotonic()<deadline:
            wait(lambda:shore()['visits']>number,'next randomized sea visitor',52)
            event=shore();number=event['visits'];kind=event['visitor']
            if kind in seen:continue
            seen.add(kind);time.sleep(1.2);capture(c,'06-'+types[kind]+'-phone');print('CAPTURE '+types[kind],flush=True)
        require(len(seen)==3,'All requested visitors were not observed in the bounded native pass')
        record('whale, dolphin and mermaid each jump from the offshore water in native phone/tablet scene')
        passed=True
    finally:
        if not passed:
            for v in locals().get('clients',[]):
                if v.process.poll() is None:
                    try:v.input('capture');shutil.copyfile(v.out/'garden.png',out/('failure-'+v.profile+'.png'))
                    except Exception:pass
        run.close();write(out/'result.json',dict(passed=passed,build=args.build,checks=checks,visualFloorOnly=args.visual_floor,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
