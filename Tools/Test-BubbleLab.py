# /// script
# dependencies = ["cryptography"]
# ///
"""Native bubble preparation, direct touch, four-player authority, migration and restart."""
import argparse, importlib.util, json, math, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def equivalent(a,b,key=''):
    # Unity JSON round-trips doubles by a few ULPs. Only analytic clocks/birth
    # times allow 1e-12; IDs, quantities, history and other records stay exact.
    if isinstance(a,dict):return isinstance(b,dict) and a.keys()==b.keys() and all(equivalent(v,b[k],k) for k,v in a.items())
    if isinstance(a,list):return isinstance(b,list) and len(a)==len(b) and all(equivalent(x,y,key) for x,y in zip(a,b))
    if key in ('clock','born') and isinstance(a,(int,float)) and isinstance(b,(int,float)):return abs(a-b)<=1e-12
    return a==b

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=191);args=p.parse_args()
    old=Run(args.previous);run=None;checks=[];passed=False;folder=old.path/'bubble-lab';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-6590,y=200)['accepted'],'legacy position')
        require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'legacy coloring')
        require(home.command(legacy,19,item=legacy.profile,target='ice@0',value='ice:chip',x=316,y=185)['accepted'],'legacy ice')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==20 and all(len(w['bubbles'])==1 for w in after['discovery']),'Missing bubble migration')
        for w in normal['discovery']:w.pop('bubbles')
        normal['schema']=before['schema'];normal['revision']-=1;require(normal==before,'Migration changed earlier records')
        record('actual schema19 save gains four bubble trays while every previous record stays exact')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def tray(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)['bubbles'][0]
        def state(v):return tray(v)['current']
        def button(v,name):v.input('touchButton',text=name);time.sleep(.16);home.ready(v)
        def tap(v,x,y,finger=61):
            v.input('touch-begin',role='bubbles',x=x,y=y,finger=finger);v.input('touch-end',role='bubbles',x=x,y=y,finger=finger);home.ready(v)
        for v in clients:
            home.ready(v);require(home.command(v,0,x=-6590,y=200)['accepted'],'science position');time.sleep(.4);button(v,'Science bench');button(v,'Bubble lab')
        a.input('resize',x=1024,y=768);home.ready(a);time.sleep(.4);home.capture(a,folder,'bubbles-tablet-start')
        for v in clients:
            tap(v,200,170);require(state(v)['water'] and not state(v)['soap'],'Direct water failed')
            tap(v,195,330);require(state(v)['soap'] and not state(v)['mixed'],'Direct soap failed')
            tap(v,490,365);require(state(v)['mixed'] and state(v)['film']==0,'Direct stirring failed')
            tap(v,790,245);require(state(v)['film']==4 and state(v)['solution']==15,'Direct wand dip failed')
        record('four release clients prepare their own water soap mixture and dip their own wand through direct touch')
        siblings=[deepcopy(tray(v)) for v in [b,c,d]];previous=deepcopy(state(a));button(a,'Bubbles Again');button(a,'Bubbles Undo');require(state(a)==previous,'Reset undo lost mixture');require([tray(v) for v in [b,c,d]]==siblings,'Reset changed siblings')
        record('Again and Undo preserve one mixture without altering siblings')
        button(a,'Bubbles Shape');button(a,'Bubbles Size');button(a,'Bubbles More');button(a,'Bubbles Air');button(a,'Bubbles Done');button(a,'Bubbles Next');require(state(a)['square'] and state(a)['floating'][0]['radius']==86 and state(a)['film']==2,'Big bubble failed');home.capture(a,folder,'bubbles-tablet-big')
        particle=state(a)['floating'][0];age=state(a)['clock']-particle['born'];x=785-38*particle['air']*(1-math.exp(-.2*age))/.2;y=245-22*age-age*age
        tap(a,x,y);require(not state(a)['floating'],'Direct bubble pop failed')
        for v in [b,c,d]:button(v,'Bubbles Next');require(tray(v)['serial']==3,'Little bubbles were not created')
        record('large and little bubbles consume film square wand releases round bubbles and direct tap pops a real bubble')
        a.input('touch-begin',role='bubbles',x=790,y=300,finger=63);a.input('touch-begin',role='bubbles',x=790,y=300,finger=64);a.input('touch-end',role='bubbles',x=790,y=300,finger=64)
        require(state(a)['film']==2,'Second finger stole wand');a.input('touch-end',role='bubbles',x=790,y=300,finger=63);home.ready(a);require(state(a)['film']==0,'First finger did not blow')
        button(b,'Floating boats');button(c,'Back to Home');require(home.command(c,7,value='park')['accepted'],'Independent travel');button(d,'Bubbles Next')
        require(a.input('inspect')['discoveryOpen'] and not c.input('inspect')['discoveryOpen'],'Activity not independent')
        wait(lambda:all(not state(v)['floating'] for v in clients),'authority bubble expiry under other activities',seconds=16)
        record('second finger cannot steal wand and authority expires bubbles while siblings change stations or travel')
        a.input('resize',x=1280,y=591);home.ready(a);button(a,'Bubbles Next');button(a,'Bubbles Next');home.capture(a,folder,'bubbles-phone-big')
        button(a,'Bubbles More');button(a,'Bubbles Sound');button(a,'Bubbles Calm');home.capture(a,folder,'bubbles-phone-options');button(a,'Bubbles Done')
        a.input('network-pause');time.sleep(.25);a.input('network-resume');home.ready(a);button(a,'Science bench');button(a,'Bubble lab');button(a,'Bubbles More');info=a.input('inspect');require('Sound off' in info['visibleText'] and 'Calm effects' in info['visibleText'],'Preferences lost');button(a,'Bubbles Done')
        record('native phone and tablet layouts render and lifecycle keeps local sound and calm preferences')
        wait(lambda:all(not w['bubbles'][0]['current']['floating'] for w in world()['discovery']),'all bubbles settled',seconds=16)
        expected=deepcopy(world()['discovery']);run.close();run=Run(args.build,resume=old.run_id);server=run.start('server');require(equivalent(saved(run)['discovery'],expected),'Server restart changed discovery')
        reopened=run.start('client',run.slots[0]['profile']);home.ready(reopened);button(reopened,'Science bench');button(reopened,'Bubble lab');home.capture(reopened,folder,'bubbles-rejoined');require(equivalent(tray(reopened),expected[0]['bubbles'][0]),'Rejoin lost bubble mixture')
        record('authority restart and rejoin retain all four mixtures undo histories and every earlier discovery record')
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)
if __name__=='__main__':main()
