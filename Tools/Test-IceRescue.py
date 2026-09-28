# /// script
# dependencies = ["cryptography"]
# ///
"""Native touch, migration, four-player authority and restart; disposable worlds only."""
import argparse, importlib.util, json, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=187);args=p.parse_args()
    old=Run(args.previous);run=None;checks=[];passed=False;folder=old.path/'ice-rescue';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-6590,y=200)['accepted'],'legacy position')
        require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'legacy coloring')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==19 and all(len(w['ice'][0]['current']['cells'])==24 for w in after['discovery']),'Missing rescue migration')
        for w in normal['discovery']:w.pop('ice')
        normal['schema']=before['schema'];normal['revision']-=1;require(normal==before,'Migration changed earlier records')
        record('actual schema18 save adds four rescue trays and preserves every earlier record')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def ws(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        def tray(v):return ws(v)['ice'][0]
        def state(v):return tray(v)['current']
        def button(v,name):v.input('touchButton',text=name);time.sleep(.16);home.ready(v)
        def tap(v,x,y,finger=61):
            v.input('touch-begin',role='ice',x=x,y=y,finger=finger);v.input('touch-end',role='ice',x=x,y=y,finger=finger);home.ready(v)
        for v in clients:
            home.ready(v);require(home.command(v,0,x=-6590,y=200)['accepted'],'science position');time.sleep(.6);button(v,'Science bench');button(v,'Dinosaur rescue')
        a.input('resize',x=1024,y=768);home.ready(a);time.sleep(1);home.capture(a,folder,'ice-tablet-start')
        for v in clients:tap(v,316,185);require(state(v)['cells'][0]<.4 and state(v)['cells'][23]==1,'Spatial hammer tap failed')
        home.capture(a,folder,'ice-tablet-cracked');record('all four release clients use direct localized hammer taps')
        siblings=[deepcopy(tray(v)) for v in [b,c,d]];before_reset=deepcopy(state(a));button(a,'Ice Again');button(a,'Ice Undo');require(state(a)==before_reset,'Reset undo lost rescue');require([tray(v) for v in [b,c,d]]==siblings,'Reset changed siblings')
        record('Again and Undo preserve one rescue without touching siblings')
        for _ in range(70):
            if state(a)['freed']:break
            button(a,'Ice Chip')
        require(state(a)['freed'],'Chip button did not finish rescue')
        a.input('touch-begin',role='ice',x=500,y=330,finger=63)
        a.input('touch-begin',role='ice',x=510,y=340,finger=64)
        a.input('touch-end',role='ice',x=800,y=420,finger=64)
        require(state(a)['x']==500,'Second finger stole dinosaur')
        a.input('touch-move',role='ice',x=760,y=420,finger=63);a.input('touch-end',role='ice',x=760,y=420,finger=63);home.ready(a)
        require(abs(state(a)['x']-735)<2,'Freed toy drag failed');home.capture(a,folder,'ice-tablet-rescued')
        record('large Chip button completes rescue and second finger cannot steal freed dinosaur drag')
        button(b,'Ice Again');button(b,'Ice Dinosaur');button(b,'Ice Water');require(state(b)['toy']==1 and any(state(b)['energy']),'Water did not start')
        initial=state(b)['cells'][0];button(b,'Floating boats');wait(lambda:state(b)['cells'][0]<initial-.1,'authority melt while another station open')
        button(c,'Back to Home');require(home.command(c,7,value='park')['accepted'],'sibling travel');button(d,'Ice Chip')
        require(a.input('inspect')['discoveryOpen'] and not c.input('inspect')['discoveryOpen'],'Activity not independent')
        record('authority melts water while sibling changes station another travels and another chips')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'ice-phone-rescued')
        button(a,'Ice More');button(a,'Ice Sound');button(a,'Ice Calm');home.capture(a,folder,'ice-phone-options');button(a,'Ice Done')
        a.input('network-pause');time.sleep(.25);a.input('network-resume');home.ready(a);button(a,'Science bench');button(a,'Dinosaur rescue')
        require(state(a)['freed'] and abs(state(a)['x']-735)<2,'Lifecycle lost freed dinosaur')
        button(a,'Ice More');info=a.input('inspect');require('Sound off' in info['visibleText'] and 'Calm effects' in info['visibleText'],'Preferences lost');button(a,'Ice Done')
        record('phone and tablet views render and lifecycle retains toy plus local sound and calm preferences')
        wait(lambda:all(not any(w['ice'][0]['current']['energy']) for w in world()['discovery']),'settled water',seconds=15)
        expected=deepcopy(world()['discovery']);run.close();run=Run(args.build,resume=old.run_id);server=run.start('server');require(saved(run)['discovery']==expected,'Server restart changed rescues')
        reopened=run.start('client',run.slots[0]['profile']);home.ready(reopened);button(reopened,'Science bench');button(reopened,'Dinosaur rescue');time.sleep(.5);home.capture(reopened,folder,'ice-rejoined')
        require(ws(reopened)==expected[0],'Rejoin changed saved rescue');record('authority restart and client rejoin preserve all four rescues and previous activities exactly')
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)
if __name__=='__main__':main()
