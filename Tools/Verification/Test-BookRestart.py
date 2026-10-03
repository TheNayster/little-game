"""Focused native check of whole-book restart and dinosaur call controls."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, hashlib, importlib.util, json, time, wave
from pathlib import Path
from shared_garden_runtime import ROOT, Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
TITLES=['hello-dinosaurs','little-bridge','rocket-moon','fairy-garden','princess-star','mermaid-shell']

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);out=run.path/'book-restart';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        run.start('server');c=run.start('client','player-1');home.ready(c)
        require(home.command(c,0,x=-4500,y=200)['accepted'],'Book rack placement');time.sleep(.6)
        def info():return c.input('inspect')
        def button(name):c.input('touchButton',text=name)
        def ready():return wait(lambda:info()['bookReady'],'page media',25)
        def openbook(title):
            button('Our books' if info()['bookOpen'] else 'Read shared books')
            button('Choose '+title);ready()
        def speaking():return wait(lambda:info()['bookSpeaking'],'narration starts')
        for title in TITLES:
            openbook(title);button('>');button('>');ready();require(info()['bookPage']==2,'Nonzero page missing')
            button('Start again');ready();speaking();require(info()['bookPage']==0,'Restart did not return to page one')
            button('Read to me')
        record('Start again returns all six books to page one and starts narration')
        openbook('hello-dinosaurs')
        button('Start again');ready();speaking();button('Read to me')
        for species in (0,7,8):
            button('Animal choice '+str(species))
            wait(lambda:info()['bookEffect'],'name followed by dinosaur call',12)
            require(not info()['bookSpeaking'],'Call overlaps narration')
            wait(lambda:not info()['bookEffect'] and not info()['bookEffectPending'],'call finishes',8)
        record('T-Rex Spinosaurus and Pteranodon taps say their names then play calls')
        button('Hear sound');wait(lambda:info()['bookEffect'],'direct dinosaur call')
        button('Start again');ready();speaking();require(info()['bookPage']==0 and not info()['bookEffect'],'Restart left stale call playing')
        button('Read to me');button('>');ready()
        button('More reading options');button('Voice on');button('Reading options done')
        button('Start again');ready();s=info();require(s['bookPage']==0 and not s['bookPlaying'],'Muted restart starts voice')
        button('Animal choice 0');wait(lambda:info()['bookEffect'],'voice-off dinosaur call')
        button('Close');time.sleep(.6);s=info();require(not s['bookOpen'] and not s['bookEffect'] and not s['bookPlaying'],'Closing left reader playing')
        record('Restart cancels old calls; voice-off restart remains quiet; closing cancels playback')
        openbook('little-bridge');button('More reading options');button('Voice on');button('Reading options done')
        button('Start again');ready();speaking();button('Read to me')
        for _ in range(7):button('>')
        ready();button('Read to me');speaking()
        wait(lambda:not info()['bookPlaying'],'last-page narration finishes',35)
        require('Read book again' in info()['visibleText'],'Finished-book label absent')
        button('Read to me');ready();speaking();require(info()['bookPage']==0,'Finished-book read did not restart')
        button('Read to me');c.input('resize',x=1280,y=591);home.capture(c,out,'restart-phone')
        record('Completed narration offers Read book again and restarts the entire book')
        calls=[]
        for species in range(12):
            source=ROOT/f'SourceAudio/Home/Books/hello-dinosaurs/effect-{species}.wav'
            runtime=ROOT/f'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/Books/hello-dinosaurs/audio/effect-{species}.wav'
            digest=hashlib.sha256(source.read_bytes()).hexdigest()
            require(source.read_bytes()==runtime.read_bytes(),'Runtime call differs from generated source')
            with wave.open(str(source)) as wav:
                require(wav.getnchannels()==1 and wav.getframerate()==32000 and wav.getsampwidth()==2,'Wrong call encoding')
                expected=json.loads(source.with_suffix('.json').read_text())['seconds']
                require(abs(wav.getnframes()/wav.getframerate()-expected)<.03,'Wrong call duration')
            calls.append(dict(species=species,sha256=digest))
        require(len({v['sha256'] for v in calls})==12,'Calls are duplicated')
        write(out/'calls.json',calls);record('All twelve distinct calls match their recorded source assets')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False,physicalAudioVerified=False))
        print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
