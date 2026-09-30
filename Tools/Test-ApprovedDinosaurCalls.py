"""One focused asset/loading/cancellation check for the approved book calls."""
import argparse, hashlib, importlib.util, json, time, wave
from pathlib import Path
from shared_garden_runtime import ROOT, Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    manifest=json.loads((ROOT/'SourceAudio/Books/ApprovedDinosaurCalls/2026-09-30/manifest.json').read_text())
    run=Run(args.build);out=run.path/'approved-dinosaur-calls';out.mkdir();passed=False
    print('EVIDENCE '+str(out),flush=True)
    try:
        for entry in manifest['approved']:
            source=ROOT/f'SourceAudio/Books/hello-dinosaurs/effect-{entry["index"]}.wav'
            runtime=ROOT/f'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Books/hello-dinosaurs/audio/effect-{entry["index"]}.wav'
            require(source.read_bytes()==runtime.read_bytes(),'Runtime differs from approved source')
            require(hashlib.sha256(source.read_bytes()).hexdigest()==entry['sha256'],'Approved call changed')
            with wave.open(str(source)) as wav:
                require((wav.getnchannels(),wav.getframerate(),wav.getsampwidth())==(1,32000,2),'Wrong encoding')
                require(abs(wav.getnframes()/wav.getframerate()-entry['seconds'])<.03,'Wrong edited duration')
        run.start('server');c=run.start('client','player-1');home.ready(c)
        require(home.command(c,0,x=-4500,y=200)['accepted'],'Book rack placement');time.sleep(.6)
        def info():return c.input('inspect')
        def button(name):c.input('touchButton',text=name)
        button('Read shared books');button('Choose hello-dinosaurs');wait(lambda:info()['bookReady'],'book media',25)
        for entry in manifest['approved']:
            button('Animal choice '+str(entry['index']))
            wait(lambda:info()['bookEffect'],'approved call starts',12)
            require(not info()['bookSpeaking'],'Call overlaps page narration')
            # The trimmed longer calls must finish naturally, not be truncated
            # by the earlier generated-call duration or a page timer.
            if entry['index'] in (2,10):
                wait(lambda:not info()['bookEffect'] and not info()['bookEffectPending'],'trimmed call completes',entry['seconds']+3)
            print('PASS '+entry['species'],flush=True)
        button('Close');time.sleep(.4)
        require(not info()['bookOpen'] and not info()['bookEffect'],'Close leaves audio playing')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,approved_species=[v['species'] for v in manifest['approved']],
            pending_indices=manifest['pending_indices'],liveFamilyTouched=False,physicalDevicesTested=False,userListeningApproved=True))
        print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
