# /// script
# dependencies = ["cryptography"]
# ///
"""Native ramp gestures, migration, four owned courses and retained meal storage."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,importlib.util,json,time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=212);args=p.parse_args()
    old=Run(args.previous,extended_test_lifetime=True);run=None;checks=[];passed=False;out=old.path/'marble-ramps';out.mkdir();print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def button(v,name):
        wait(lambda:any(x['name']==name and x['enabled'] for x in v.input('inspect')['controls']),'enabled '+name)
        v.input('touchButton',text=name);time.sleep(.25);home.ready(v)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v);return r
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        cmd(legacy,0,x=-6590,y=200);cmd(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5');old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==27 and all(len(w['ramps'])==1 for w in after['discovery']),'Missing ramp migration')
        for w in normal['discovery']:w.pop('ramps')
        normal['schema']=before['schema'];normal['revision']-=1;require(normal==before,'Migration changed earlier records')
        record('actual schema26 save gains four ramp trays without altering prior records')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def tray(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)['ramps'][0]
        def tap(v,x,y,finger=71):
            v.input('touch-begin',role='ramps',x=x,y=y,finger=finger);v.input('touch-end',role='ramps',x=x,y=y,finger=finger);home.ready(v)
        def drag(v,x,y,xx,yy):
            v.input('touch-begin',role='ramps',x=x,y=y,finger=72)
            for n in range(1,5):v.input('touch-move',role='ramps',x=x+(xx-x)*n/4,y=y+(yy-y)*n/4,finger=72)
            v.input('touch-end',role='ramps',x=xx,y=yy,finger=72);home.ready(v)
        for v in clients:
            home.ready(v);cmd(v,0,x=-6590,y=200);time.sleep(.5);button(v,'Science bench');button(v,'Marble ramps')
        a.input('resize',x=1280,y=591);b.input('resize',x=1024,y=768);home.ready(a);home.ready(b)
        for v,width,height,name in [(a,1280,591,'01-starter-phone'),(b,1024,768,'02-starter-tablet')]:
            info=home.capture(v,out,name);controls=[x for x in info['controls'] if x['name'].startswith('Ramps ') or x['name']=='Marble ramps']
            for control in controls:
                r=control['bounds'];require(r['width']>=44 and r['height']>=44 and r['x']>=0 and r['y']>=0 and r['x']+r['width']<=width and r['y']+r['height']<=height,'Clipped or small '+control['name'])
            for i,control in enumerate(controls):
                r=control['bounds']
                for other in controls[i+1:]:
                    t=other['bounds'];require(min(r['x']+r['width'],t['x']+t['width'])<=max(r['x'],t['x']) or min(r['y']+r['height'],t['y']+t['height'])<=max(r['y'],t['y']),'Overlapping ramp controls')
        drag(a,210,150,250,100);require(tray(a)['current']['course']['points'][:2]==[250,100],'Drag missed handle')
        tap(b,860,280);tap(b,850,240);require(tray(b)['current']['course']['points'][4:6]==[850,240],'Tap then place missed')
        drag(c,700,470,680,440);drag(d,210,150,240,130)
        button(b,'Ramps Surface');button(c,'Ramps Surface');button(c,'Ramps Surface')
        for v in clients:button(v,'Ramps Keep')
        require(len({json.dumps(tray(v)['kept']) for v in clients})==4,'Kept courses not independent')
        record('four clients drag or tap-place real handles and keep distinct surface and geometry choices')
        baseline=deepcopy(tray(a));a.input('touch-begin',role='ramps',x=250,y=100,finger=73);a.input('touch-begin',role='ramps',x=760,y=220,finger=74);a.input('touch-move',role='ramps',x=500,y=180,finger=74);a.input('touch-end',role='ramps',x=500,y=180,finger=74)
        require(tray(a)==baseline,'Second finger changed ramp');a.input('touch-move',role='ramps',x=260,y=90,finger=73);a.input('touch-end',role='ramps',x=260,y=90,finger=73);home.ready(a);require(tray(a)['current']['course']['points'][:2]==[260,90],'First pointer lost ramp')
        button(a,'Ramps Undo');require(tray(a)['current']==baseline['current'],'Undo lost course');button(a,'Ramps More');button(a,'Ramps Starter');require(tray(a)['kept']==baseline['kept'],'Starter erased kept layout');button(a,'Ramps Restore');button(a,'Ramps Sound');button(a,'Ramps Calm');home.capture(a,out,'03-options-phone');button(a,'Ramps Done')
        require(tray(a)['current']==baseline['current'],'Restore changed saved geometry');record('one pointer owns the gesture and undo starter restore preserve deliberately kept courses')
        for v in clients:button(v,'Ramps Roll')
        time.sleep(2);require(all(tray(v)['current']['released'] and tray(v)['current']['elapsed']>0 for v in clients),'Releases not advancing');home.capture(a,out,'04-rolling-phone');home.capture(b,out,'05-rolling-tablet')
        sibling_kept=[deepcopy(tray(v)['kept']) for v in clients];button(d,'Back to Home');cmd(d,7,value='park');button(c,'Ramps More');button(c,'Ramps Starter');button(c,'Ramps Restore');button(c,'Ramps Done')
        require([tray(v)['kept'] for v in clients]==sibling_kept and tray(a)['current']['released'] and tray(b)['current']['released'],'Independent reset/departure changed siblings')
        record('authority advances four releases and departure or reset leaves siblings and kept courses intact')
        button(a,'Ramps More');button(a,'Ramps Starter');button(a,'Ramps Done');button(a,'Ramps Roll');wait(lambda:any('In the basket!' in text for text in a.input('inspect')['visibleText']),'starter visibly reaches basket',20);home.capture(a,out,'06-caught-phone')
        button(a,'Ramps More');button(a,'Ramps Restore');button(a,'Ramps Done');button(a,'Ramps Roll');time.sleep(.6)
        require(server.status()['status']=='listening','Authority stopped before checkpoint');run.close();checkpoint=saved(run);expected=[deepcopy(w['ramps'][0]) for w in checkpoint['discovery']];require(expected[0]['current']['released'] and expected[0]['current']['elapsed']<14,'No partial release persisted')
        restarted_at=time.monotonic();run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');restored=saved(run)
        restored_ramps=[deepcopy(w['ramps'][0]) for w in restored['discovery']]
        write(out/'restart-comparison.json',dict(before=expected,after=restored_ramps))
        for previous,current in zip(expected,restored_ramps):
            # The resumed server is already ticking when its listening status
            # is observed. Only current release time may advance; geometry,
            # revisions, undo and deliberately kept courses must remain exact.
            delta=current['current']['elapsed']-previous['current']['elapsed']
            require(-1e-9<=delta<=time.monotonic()-restarted_at+.1,'Restart rewound or jumped release time')
            current['current']['elapsed']=previous['current']['elapsed']
            for old_state,new_state in zip(previous['previous'],current['previous']):
                require(abs(old_state['elapsed']-new_state['elapsed'])<=1e-9,'Undo release time changed')
                new_state['elapsed']=old_state['elapsed']
        require(restored_ramps==expected,'Restart changed ramp checkpoint')
        a=run.start('client',run.slots[0]['profile']);home.ready(a);button(a,'Science bench');button(a,'Marble ramps');home.capture(a,out,'07-rejoined-course');button(a,'Ramps More');require('Sound off' in a.input('inspect')['visibleText'] and 'Calm effects' in a.input('inspect')['visibleText'],'Local preferences lost');button(a,'Ramps Done')
        record('cold authority restart preserves partial release histories and four kept courses and rejoin keeps local preferences')
        button(a,'Back to Home');cmd(a,0,x=-2300,y=200);time.sleep(2);button(a,'Cook');button(a,'Meals');button(a,'Recipe MEAL-01');button(a,'Cooking action');button(a,'Cooking action')
        def dish():return next(t for t in world()['toys'] if t['id']=='cookware-0')['kitchen']['dish']
        wait(lambda:dish()['stage']=='flip','safe first-side finish',20);home.ready(a);before_food=deepcopy(dish());button(a,'Put food away');button(a,'My saved food');home.capture(a,out,'08-waiting-flip-saved');button(a,'Use saved creation');require(dish()==before_food,'Waiting-flip storage changed food');home.capture(a,out,'09-waiting-flip-restored')
        record('Put away and Bring out preserve the same burger waiting for its flip through actual controls')
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(out/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
