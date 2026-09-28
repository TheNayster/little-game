# /// script
# dependencies = ["cryptography"]
# ///
"""Native liquid-color touch flow, four-player isolation, additive migration and restart."""
import argparse,importlib.util,json,time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=192);args=p.parse_args()
    old=Run(args.previous);run=None;checks=[];passed=False;folder=old.path/'liquid-colors';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-6590,y=200)['accepted'],'legacy position')
        require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'legacy coloring')
        for revision,op in enumerate(['water','soap','stir','dip']):require(home.command(legacy,19,item=legacy.profile,target=f'bubbles@{revision}',value='bubble:'+op)['accepted'],'legacy bubbles')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==21 and all(len(w['liquid'])==1 for w in after['discovery']),'Missing color migration')
        for w in normal['discovery']:w.pop('liquid')
        normal['schema']=before['schema'];normal['revision']-=1;require(normal==before,'Migration changed earlier records')
        record('actual schema20 save gains four color trays without altering any earlier record')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def tray(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)['liquid'][0]
        def state(v):return tray(v)['current']
        def button(v,name):v.input('touchButton',text=name);time.sleep(.16);home.ready(v)
        def tap(v,color,finger=81):
            x,y=[(180,280),(500,150),(820,280)][color]
            v.input('touch-begin',role='colors',x=x,y=y,finger=finger);v.input('touch-end',role='colors',x=x,y=y,finger=finger);home.ready(v)
        for v in clients:
            home.ready(v);require(home.command(v,0,x=-6590,y=200)['accepted'],'science position');time.sleep(.4);button(v,'Science bench');button(v,'Liquid colors')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'colors-tablet-start')
        for v,colors,name in [(a,[0,1],'Orange'),(b,[1,2],'Green'),(c,[0,2],'Purple'),(d,[0,1,2],'Earthy brown')]:
            for color in colors:tap(v,color)
            require(state(v)['parts']==[colors.count(i) for i in range(3)],'Wrong color portions')
            require(name in v.input('inspect')['visibleText'],'Wrong visible color result '+name)
        time.sleep(1);home.capture(a,folder,'colors-tablet-orange');home.capture(b,folder,'colors-green');home.capture(c,folder,'colors-purple');home.capture(d,folder,'colors-brown')
        record('four native clients use real bottle touches to independently make orange green purple and brown')
        siblings=[deepcopy(tray(v)) for v in [b,c,d]];before=deepcopy(state(a));button(a,'Colors More');button(a,'Colors Water');button(a,'Colors Done');require(state(a)['water']==1 and state(a)['parts']==before['parts'],'Water replaced color quantities')
        button(a,'Colors Again');button(a,'Colors Undo');require(state(a)['water']==1 and state(a)['parts']==before['parts'],'Undo lost diluted mixture');require([tray(v) for v in [b,c,d]]==siblings,'A player changed siblings')
        record('water keeps original color quantities and reset undo affects only the owner')
        button(a,'Colors Again');a.input('touch-begin',role='colors',x=180,y=280,finger=83);a.input('touch-begin',role='colors',x=820,y=280,finger=84);a.input('touch-end',role='colors',x=820,y=280,finger=84);require(state(a)['parts']==[0,0,0],'Second finger stole pour');a.input('touch-end',role='colors',x=180,y=280,finger=83);home.ready(a);require(state(a)['parts']==[1,0,0],'First touch did not pour')
        for i in range(11):button(a,['Colors Red','Colors Yellow','Colors Blue'][i%3])
        require(sum(state(a)['parts'])==12,'Bowl not full');before=deepcopy(tray(a));tap(a,2);button(a,'Colors Red');require(tray(a)==before,'Full bowl accepted extra portion');time.sleep(1);home.capture(a,folder,'colors-tablet-full')
        record('one-pointer touch protection works and twelve portions disable extra pours without overflow')
        button(b,'Bubble lab');button(c,'Back to Home');require(home.command(c,7,value='park')['accepted'],'Independent travel');button(d,'Colors Blue');require(tray(b)==siblings[0] and tray(c)==siblings[1],'Station change or travel altered mixtures');require(a.input('inspect')['discoveryOpen'] and not c.input('inspect')['discoveryOpen'],'Activity not independent')
        record('one player changes station another travels and another pours without interrupting siblings')
        button(a,'Colors Again');button(a,'Colors Red');button(a,'Colors Yellow');a.input('resize',x=1280,y=591);home.ready(a);time.sleep(1);home.capture(a,folder,'colors-phone-orange')
        button(a,'Colors More');button(a,'Colors Water');button(a,'Colors Sound');button(a,'Colors Calm');home.capture(a,folder,'colors-phone-options');button(a,'Colors Done')
        a.input('network-pause');time.sleep(.25);a.input('network-resume');home.ready(a);button(a,'Science bench');button(a,'Liquid colors');button(a,'Colors More');info=a.input('inspect');require('Sound off' in info['visibleText'] and 'Calm effects' in info['visibleText'],'Preferences lost');button(a,'Colors Done')
        record('phone and tablet layouts render and lifecycle preserves local sound and calm settings')
        expected=deepcopy(world()['discovery']);run.close();run=Run(args.build,resume=old.run_id);server=run.start('server');require(saved(run)['discovery']==expected,'Authority restart changed discovery')
        reopened=run.start('client',run.slots[0]['profile']);home.ready(reopened);button(reopened,'Science bench');button(reopened,'Liquid colors');home.capture(reopened,folder,'colors-rejoined');require(tray(reopened)==expected[0]['liquid'][0],'Rejoin lost color state')
        record('authority restart and rejoin preserve all four mixtures undo history and previous science records exactly')
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)
if __name__=='__main__':main()
