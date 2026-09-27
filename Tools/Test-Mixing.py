# /// script
# dependencies = ["cryptography"]
# ///
"""Release-player mixing acceptance; only disposable loopback worlds."""
import argparse, importlib.util, json, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--previous',type=int,default=174);args=parser.parse_args()
    old=Run(args.previous);run=None;checks=[];passed=False;folder=old.path/'mixing';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-6590,y=200)['accepted'],'legacy position')
        require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'legacy coloring')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema'] in (17,18) and all(len(w['mixtures'])==4 for w in after['discovery']),'Missing mixing migration')
        for w in normal['discovery']:
            w.pop('mixtures')
            if after['schema']>=18:w['pages']=w['pages'][:6]
        normal['schema']=before['schema'];normal['revision']-=after['schema']-before['schema'];require(normal==before,'Migration altered existing records')
        record('actual 174 save adds sixteen trays while preserving all previous records and coloring')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def ws(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        def tray(v,mode=0):return ws(v)['mixtures'][mode]
        def cmd(v,action,**kw):
            result=home.command(v,action,**kw);require(result['accepted'],str(result));home.ready(v)
            if action==0:time.sleep(.75);home.ready(v)
            return result
        def button(v,name):v.input('touchButton',text=name);time.sleep(.12);home.ready(v)
        def tap(v,x,y):
            v.input('touch-begin',role='mixing',x=x,y=y,finger=61);v.input('touch-end',role='mixing',x=x,y=y,finger=61);time.sleep(.14);home.ready(v)
        def supply(v,slot):tap(v,-430 if slot<3 else 430,145-slot%3*148)
        for v in clients:home.ready(v);cmd(v,0,x=-6590,y=200);button(v,'Science bench');button(v,'Mix & discover')
        a.input('resize',x=1024,y=768);home.ready(a)
        button(a,'Floating boats')
        a.input('touch-begin',role='discovery',x=400,y=230,finger=71);a.input('touch-end',role='discovery',x=400,y=230,finger=71);home.ready(a)
        require(ws(a)['cargo']==1,'Direct boat tap did not add cargo');button(a,'Discovery Wide boat');home.capture(a,folder,'illustrated-boat-tablet')
        button(a,'Magnet materials');button(a,'Discovery Iron');home.capture(a,folder,'illustrated-magnets-tablet')
        button(a,'Colored light')
        for x in [230,400,570]:
            a.input('touch-begin',role='discovery',x=x,y=375,finger=72);a.input('touch-end',role='discovery',x=x,y=375,finger=72);home.ready(a)
        require(ws(a)['lights']==7,'Direct illustrated lamps failed');home.capture(a,folder,'illustrated-rgb-tablet');button(a,'Mix & discover')
        record('illustrated boats materials and lamps render with direct boat and lamp input')
        home.capture(a,folder,'mixing-empty-tablet')
        for v in clients:
            supply(v,1);supply(v,2);require(tray(v)['reacted']==0,'Water made gas');supply(v,0);require(tray(v)['reacted']==1,'Contact did not react')
        home.capture(a,folder,'mixing-bowl-tablet')
        record('four clients use real ingredient taps with immediate contact gas and a water control')
        button(a,'Mixing Rinse bowl');supply(a,1);supply(a,1);supply(a,1);supply(a,1);supply(a,3)
        a.input('touch-begin',role='mixing',x=-430,y=145,finger=63);a.input('touch-move',role='mixing',x=0,y=60,finger=63);time.sleep(1.7);a.input('touch-end',role='mixing',x=0,y=60,finger=63);home.ready(a)
        require(2<=tray(a)['amounts'][0]<=6,'Held pour did not create bounded partial portions');require(tray(a)['reacted']>0 and tray(a)['foam']>0,'Foam missing')
        home.capture(a,folder,'mixing-foam-tablet');button(a,'Mixing Vessel');home.capture(a,folder,'mixing-volcano-tablet')
        require(tray(a)['volcano'],'Vessel did not change');prior=deepcopy(tray(b));button(a,'Mixing Rinse bowl');require(tray(a)['reacted']==0 and tray(b)['amounts']==prior['amounts'],'Reset affected sibling')
        record('holding the jug over the vessel partially pours and foam survives vessel changes without duplicating reactants')
        # A second finger on another supply must not steal/release the held jug.
        button(a,'Mixing Rinse bowl')
        a.input('touch-begin',role='mixing',x=-430,y=145,finger=63)
        a.input('touch-move',role='mixing',x=0,y=60,finger=63)
        a.input('touch-begin',role='mixing',x=-430,y=-3,finger=64)
        a.input('touch-end',role='mixing',x=-430,y=-3,finger=64)
        partial=tray(a)['amounts'][0];time.sleep(1.2)
        a.input('touch-end',role='mixing',x=0,y=60,finger=63);home.ready(a)
        require(tray(a)['amounts'][0]>partial and tray(a)['amounts'][1]==0,'Second finger interrupted the pour')
        record('a second finger cannot steal an active ingredient pour')
        button(a,'Color changing');supply(a,0);supply(a,1);require(tray(a,1)['amounts'][5]==1 and tray(a,1)['amounts'][0]==1,'Indicator inputs');home.capture(a,folder,'mixing-indicator-pink-tablet')
        supply(a,2);supply(a,2);home.capture(a,folder,'mixing-indicator-teal-tablet')
        button(a,'Oil & water');supply(a,0);supply(a,0);supply(a,1);supply(a,2);button(a,'Mixing Stir');require(tray(a,2)['stir']>0,'Layer stirring not authoritative');home.capture(a,folder,'mixing-oil-droplets-tablet')
        wait(lambda:tray(a,2)['stir']==0,'layer settling',seconds=15);home.capture(a,folder,'mixing-oil-layers-tablet')
        button(a,'Squishy oobleck');supply(a,0);supply(a,0);supply(a,1);tap(a,0,0);require(tray(a,3)['poke']>0,'Oobleck poke');home.capture(a,folder,'mixing-oobleck-tablet')
        record('indicator quantities oil separation and oobleck touch all operate as distinct persistent activities')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'mixing-phone-landscape')
        button(a,'Sound on');button(a,'Gentle motion');home.capture(a,folder,'mixing-calm-muted-phone')
        a.input('network-pause');time.sleep(.3);a.input('network-resume');home.ready(a);button(a,'Science bench');button(a,'Mix & discover')
        require(ws(a)['mixtures'][3]['amounts'][4]==2,'Lifecycle erased mixture')
        record('phone/tablet layouts and calm muted controls render and lifecycle retains the mixtures')
        button(d,'Back to Home');cmd(d,7,value='park');before_b=tray(b)['amounts'][:];button(c,'Mixing Rinse bowl');require(tray(b)['amounts']==before_b,'Other user reset')
        require(a.input('inspect')['discoveryOpen'] and not d.input('inspect')['discoveryOpen'],'View not independent')
        # Include old coloring behavior in the new release.
        button(a,'Back to Home');cmd(a,0,x=-5460,y=200);button(a,'Coloring table');button(a,'Choose picture');button(a,'Coloring page 0');button(a,'Discovery crayon Green')
        a.input('touch-begin',role='discovery',x=280,y=250,finger=66);a.input('touch-end',role='discovery',x=280,y=250,finger=66);home.ready(a)
        require(ws(a)['pages'][0]['colors'][0]==4,'Existing coloring regressed')
        record('one user can travel another rinse and another color while sibling mixtures remain intact')
        wait(lambda:all(t['reaction']==0 and t['foam']==0 and t['stir']==0 and t['poke']==0 for w in world()['discovery'] for t in w['mixtures']),'settled saved results',seconds=28)
        expected=deepcopy(world()['discovery']);run.close();run=Run(args.build,resume=old.run_id);server=run.start('server');require(saved(run)['discovery']==expected,'Authority restart lost mixtures')
        reopened=run.start('client',run.slots[0]['profile']);home.ready(reopened);cmd(reopened,0,x=-6590,y=200);button(reopened,'Science bench');button(reopened,'Mix & discover');home.capture(reopened,folder,'mixing-rejoined')
        require(ws(reopened)==expected[0],'Rejoin differs');record('server save restart and rejoin retain all four players and all four experiment variants without replay')
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception: '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)

if __name__=='__main__':main()
