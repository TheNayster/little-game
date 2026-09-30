# /// script
# dependencies = ["cryptography"]
# ///
"""Focused four-client dinosaur riding check through isolated native UGUI inputs."""
import argparse, importlib.util, time, hashlib, json, uuid
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'dinosaur-world';out.mkdir();checks=[];passed=False
    for slot in run.slots:slot["profile"]=uuid.uuid4().hex
    names=['tyrannosaurus','triceratops','brachiosaurus','parasaurolophus']
    home.scenic.NAMES['dinosaur-world']='World Dinosaur World'
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def player(v):return next(p for p in server.state()['view']['players'] if p['id']==v.profile)
    def state():return server.state()['view']['dinosaurWorld']
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for i,v in enumerate(clients):
            home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
        a,b,c,d=clients
        home.scenic.travel(a,'dinosaur-world');home.capture(a,out,'dinosaur-arrival-phone')
        for v in clients[1:]:cmd(v,7,value='dinosaur-world')
        for i,v in enumerate(clients):
            cmd(v,1,value='orange-pup' if i%2 else 'blue-pup')
            target=state()['animals'][i];cmd(v,0,x=target['x'],y=target['y']);time.sleep(.65) # Let camera/anticipation settle after fixture teleport.
            v.input('touchButton',text='Ride '+names[i]);wait(lambda:player(v)['fixture']=='dinosaur-'+names[i],'native mount')
            wait(lambda:v.input('inspect')['dinosaurSoundPlaying'],'approved call playback',8)
            v.input('touchButton',text='Ride '+names[i]);require(player(v)['fixture']=='dinosaur-'+names[i],'repeat tap dismounted')
            home.capture(v,out,names[i]+'-mounted')
        require(len({player(v)['fixture'] for v in clients})==4,'not four simultaneous riders')
        record('new picture destination loads; four distinct dinosaurs mounted via real touches; repeat taps retain rides; each approved call plays')
        before=player(a)['x'];inspect=a.input('inspect')
        if not inspect['joystickVisible']:a.input('touchButton',text='Tap to walk')
        a.input('touch-begin',role='stick',x=-45,finger=60)
        wait(lambda:player(a)['x']<before-80,'steering left');a.input('touch-end',role='stick',finger=60)
        require(player(a)['fixture']=='dinosaur-tyrannosaurus' and state()['animals'][0]['left'],'moving lost mount/facing')
        home.capture(a,out,'tyrannosaurus-riding-left')
        a.input('touch-begin',role='stick',x=45,finger=61)
        wait(lambda:player(a)['x']>before+60,'steering right');a.input('touch-end',role='stick',finger=61)
        require(not state()['animals'][0]['left'],'facing did not follow turn')
        home.capture(a,out,'tyrannosaurus-riding-right')
        for v in clients:
            p=player(v);animal=next(x for x in state()['animals'] if 'dinosaur-'+x['species']==p['fixture']);require(abs(p['x']-animal['x'])<.01 and abs(p['y']-animal['y'])<.01,'authority rider/body differ')
        record('real joystick steers dinosaur both directions without dismount; all four authority body/rider positions agree')
        # Bring the four riders into one shared camera for assembly/depth review.
        for i,v in enumerate(clients):cmd(v,0,x=1900+i*240,y=130+i*15)
        time.sleep(.5);home.capture(b,out,'four-riders-together-tablet')
        wait(lambda:state()['clock']-state()['animals'][1]['lastCall']>3,'call cooldown')
        before=state()['animals'][1]['calls'];b.input('touchButton',text='Dinosaur call')
        wait(lambda:state()['animals'][1]['calls']==before+1,'replicated call')
        wait(lambda:a.input('inspect')['dinosaurSoundPlaying'],'sibling hears nearby call')
        b.input('touchButton',text='Dinosaur call');require(state()['animals'][1]['calls']==before+1,'rapid taps stacked shared calls')
        record('four riders share the valley; roar is replicated to siblings and rapid call taps are bounded')
        cmd(c,1,value='bandit');home.capture(c,out,'bandit-brachiosaurus-fallback');require(player(c)['fixture']=='dinosaur-brachiosaurus','character swap dropped mount')
        original=player(c)['fixture'];a.input('touchButton',text='Dinosaur get off');wait(lambda:player(a)['fixture']=='','get off')
        require(player(c)['fixture']==original,'get off disrupted sibling')
        a.input('touchButton',text='Ride brachiosaurus');home.ready(a);require(player(a)['fixture']=='' and player(c)['fixture']==original,'occupied mount stolen')
        cmd(a,7,value='creek');require(a.input('inspect')['dinosaurTextures']==0,'travel retained world atlases')
        cmd(b,7,value='home');require(player(c)['fixture']==original and player(d)['fixture']=='dinosaur-parasaurolophus','travel disrupted sibling')
        c.close();wait(lambda:player(c)['fixture']=='','disconnect releases only own dinosaur')
        require(player(d)['fixture']=='dinosaur-parasaurolophus','disconnect disrupted last rider')
        cmd(d,0,x=2500,y=150);home.capture(d,out,'remaining-rider-after-departures')
        record('explicit get-off, occupied-mount rejection, world unloading, independent travel and disconnect preserve remaining rides')
        d.input('application-pause');wait(lambda:player(d)['fixture']=='','pause releases own mount');require(not d.input('inspect')['dinosaurSoundPlaying'],'paused call continued');d.input('application-resume');home.ready(d);d.input('touchButton',text='Ride parasaurolophus');wait(lambda:player(d)['fixture']=='dinosaur-parasaurolophus','resume can remount');record('application pause releases its rider and stops dinosaur audio; resume can ride again')
        hashes={}
        root=Path(__file__).resolve().parents[1]/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources'
        for name,index in zip(names,[0,1,3,10]):
            one=hashlib.sha256((root/'DinosaurWorldAudio'/(name+'.wav')).read_bytes()).hexdigest();two=hashlib.sha256((root/'Books/hello-dinosaurs/audio'/f'effect-{index}.wav').read_bytes()).hexdigest();require(one==two,'book sound changed');hashes[name]=dict(bookEffect=index,sha256=one)
        write(out/'audio-hashes.json',hashes);record('all four world WAVs match approved runtime Home-book WAVs byte-for-byte')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
