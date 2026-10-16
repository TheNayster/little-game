# /// script
# dependencies = ["cryptography"]
# ///
"""Exercise one shared trip with four isolated release clients and real UGUI input."""
import argparse
import hashlib
import importlib.util
import os
from pathlib import Path
import re
import sys
import time
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--packaged',action='store_true');p.add_argument('--resume');p.add_argument('--continue-roadworks',action='store_true');p.add_argument('--continue-window',action='store_true');a=p.parse_args()
    if a.packaged:os.environ.pop('LITTLE_WEEPS_DRIVE_ART',None)
    else:os.environ['LITTLE_WEEPS_DRIVE_ART']=str(ROOT/'LocalData/Research/GoForADrive/Content-fixtures')
    run=Run(a.build,resume=a.resume,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/('drive-window-continue' if a.continue_window else 'drive-continue' if a.continue_roadworks else 'drive-reopen' if a.resume else 'drive');base_out=out
    for attempt in range(1,100):
        if not out.exists():break
        out=base_out.with_name(base_out.name+'-'+str(attempt))
    out.mkdir();checks=[];passed=False
    concurrent=[]
    for f in read(run.folder/'source-manifest.json')['files']:
        if not f['path'].startswith('Unity/'):continue
        if hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()!=f['sha256']:
            require(any('/Worlds/'+world+'/' in f['path'] for world in ('Markets','PeppaFunfair')),'Drive/shared input differs from candidate: '+f['path']);concurrent.append(f['path'])
    before=ROOT/f'LocalData/GoForADrive/prebuild-{a.build}.json'
    if before.exists():
        for f in read(before):
            if hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()!=f['sha256']:
                require(any('/Worlds/'+world+'/' in f['path'] for world in ('Markets','PeppaFunfair')),'Drive/shared input changed during build: '+f['path']);concurrent.append(f['path'])
    actions=[v.strip() for v in re.search(r'enum SoloAction\s*{([^}]+)}',(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text()).group(1).split(',')]
    print('EVIDENCE '+str(out),flush=True)
    def command(c,action,**kw):
        r=home.command(c,actions.index(action),**kw);require(r['accepted'],r['outcome']);home.ready(c);return r
    def control(c,name):return next((v for v in c.input('inspect')['controls'] if v['name']==name and v['enabled']),None)
    def button(c,name):
        wait(lambda:control(c,name),'enabled '+name,20);c.input('touchButton',text=name);home.ready(c)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def view():return server.state()['view']['drive']
    def center(c,name):
        b=wait(lambda:control(c,name),'control '+name)['bounds'];return b['x']+b['width']/2,b['y']+b['height']/2
    def assert_views():
        d=view();wait(lambda:all(c.input('inspect')['drive']['stage']==d['stage'] for c in clients),'common stage')
    def build_statue():
        if view()['freeBuild']:button(clients[0],'Drive free-build');wait(lambda:not view()['freeBuild'],'matching statue mode')
        for i,b in enumerate(view()['bricks']):
            if b['variant']>=12 or b['snapped']:continue
            c=clients[i%4]
            # The generic bounds catalog drops rotated rectangles. The native
            # touch tool still presses this known Button's actual screen center.
            c.input('touchButton',text='Drive building brick '+b['id']);home.ready(c)
            wait(lambda:next(v for v in view()['bricks'] if v['id']==b['id'])['holder']==c.profile,'source brick owner')
            button(c,'Drive statue place '+str(b['variant']));wait(lambda:next(v for v in view()['bricks'] if v['id']==b['id'])['snapped'],'rebuild contact')
        wait(lambda:view()['statueBuilt'],'shared original statue')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for c in clients:home.ready(c)
        if a.resume and not (a.continue_roadworks or a.continue_window):
            require(all(c.input('inspect')['zone']=='go-for-a-drive' for c in clients),'Saved world arrival lost')
            wait(lambda:all(c.input('inspect')['visibleDrivePlayers']==4 for c in clients),'saved cast')
            d=view();require(d['stage']==3 and d['statues']>=1 and d['stars']>=10 and len(d['bricks'])>=12,'Saved trip progress lost')
            if not d['statueBuilt']:build_statue();record('four fresh clients reopen the saved tumble and rebuild the original shared pieces through actual rotated controls')
            record('four fresh clients reopen the same trip, original brick IDs and common window rewards')
        else:
            if not (a.continue_roadworks or a.continue_window):
                home.scenic.NAMES['go-for-a-drive']='World Go for a Drive';home.travel(clients[0],'go-for-a-drive')
                for c in clients[1:]:command(c,'Travel',value='go-for-a-drive')
                wait(lambda:all(c.input('inspect')['visibleDrivePlayers']==4 for c in clients),'four distinct Drive characters')
                button(clients[0],'Drive start');wait(lambda:all(m['joined'] for m in view()['members']),'one invitation joins all four')
                record('world picture opens recovered scenery and one Start joins four actual clients')
                home.capture(clients[0],out,'packing-first-view')
                for i in range(3):
                    c=clients[i];button(c,'Drive luggage luggage-'+str(i));wait(lambda:view()['items'][i]['holder']==c.profile,'one original object owner');button(c,'Drive car peg '+str(i))
                wait(lambda:view()['packingClear'],'shared balanced packing',10)
                record('real picture controls carry the three original items to separate car pegs and finish shared packing')
                button(clients[3],'Drive next');wait(lambda:view()['stage']==1,'common drive');assert_views()
                for stop in range(3):
                    while not view()['stop']:
                        d=view();button(clients[d['roadStep']%4],'Drive lane-'+str((d['nomadLane']+1)%3));button(clients[(d['roadStep']+1)%4],'Drive boost')
                        wait(lambda:view()['stop']!='' or not view()['nomadPassed'],'next source vehicle',5)
                    require(view()['stop']==['toilet','petrol','roadworks'][stop],'Wrong source stop order')
                    home.capture(clients[0],out,'stop-'+view()['stop'])
                    if stop==0:button(clients[0],'Drive toilet');wait(lambda:view()['stopDone'],'shared toilet stop',8)
                    elif stop==1:
                        button(clients[1],'Drive fuel');wait(lambda:view()['fuel']>.15,'partial nozzle fill')
                        command(clients[1],'Travel',value='garden');wait(lambda:view()['hoseHolder']=='','nozzle release on independent departure')
                        partial=view()['fuel'];button(clients[2],'Drive fuel');wait(lambda:view()['stopDone'],'remaining child fills the same tank',10)
                        require(view()['fuel']>=partial,'Partial fuel was lost');command(clients[1],'Travel',value='go-for-a-drive')
                    else:
                        for c in clients:button(c,'Drive rubble')
                        wait(lambda:view()['clock']>=view()['members'][3]['nextAction'],'rubble action finishes');button(clients[3],'Drive light');wait(lambda:view()['stopDone'],'shared clear road and green light')
                    button(clients[0],'Drive cow');button(clients[0],'Drive continue');wait(lambda:view()['stop']=='','return to the same car')
                record('four people guide one car through source traffic and all three stops; departure preserves siblings and partial fuel')
            elif a.continue_roadworks:
                require(view()['stage']==1 and view()['stop']=='roadworks' and view()['stopProgress']==4,'Interrupted roadworks checkpoint lost')
                wait(lambda:all(m['joined'] for m in view()['members']),'resumed four-person trip')
                if not view()['trafficGreen']:button(clients[3],'Drive light')
                wait(lambda:view()['stopDone'],'resumed roadworks complete');button(clients[0],'Drive continue');wait(lambda:view()['stop']=='','saved road checkpoint continues')
                record('four fresh clients resume the interrupted roadworks checkpoint and finish the common stop')
            if not a.continue_window:button(clients[2],'Drive next');wait(lambda:view()['stage']==2,'shared window')
            else:require(view()['stage']==2 and view()['stars']>0,'Saved window rewards changed')
            assert_views();home.capture(clients[0],out,'window-first-view');peak_speed=view()['windowSpeed'];start_cycle=view()['stars']//10
            patterns=read(ROOT/'LocalData/Research/GoForADrive/Content-fixtures/pattern-content.json')['patterns']
            for i in range(45):
                c=clients[i%4];d=view();serial=d['obstacle'];pattern=patterns[d['obstacleKind']]
                # Lift the real shared source hand, then release it after the
                # obstacle. Input travels through Touchscreen + UGUI drag events.
                name='Drive window hand '+c.profile;start=center(c,name);surface=control(c,'Drive recovered scenery')['bounds']
                end=(start[0]+40,surface['y']+surface['height']*.70)
                c.input('touch-begin',role='screen',x=start[0],y=start[1],finger=81)
                c.input('touch-move',role='screen',x=end[0],y=end[1],finger=81)
                wait(lambda:view()['armHolder']==c.profile,'shared hand drag lease')
                # The actual source obstacle is the sole semantic collider
                # other than root input and reward; its right edge defines clear.
                pic=next(v for v in read(ROOT/'LocalData/Research/GoForADrive/Content-fixtures/pattern-content.json')['pictures'] if v['key']==pattern['key'])
                obstacle=next(v for v in pattern['colliders'] if v['path'] and v['path']!=pattern['reward']['path']);b=obstacle['bounds']
                right=((b['x']+b['width'])*pattern['width']-pic['pivot']['x'])*pic['unitsPerPixel']*80
                clear=pattern['sourceSpan']*40+right+8;deadline=time.monotonic()+12
                while view()['obstacle']==serial and view()['windowDistance']<clear:
                    require(time.monotonic()<deadline,'Window pattern did not advance');require(view()['armHolder']==c.profile,'Stationary held drag lost ownership');peak_speed=max(peak_speed,view()['windowSpeed']);time.sleep(.12)
                wait(lambda:not c.input('inspect')['pending'],'release after current hand acknowledgement')
                c.input('touch-end',role='screen',x=end[0],y=end[1],finger=81)
                wait(lambda:view()['obstacle']!=serial,'original next window pattern',10);peak_speed=max(peak_speed,view()['windowSpeed'])
                if i>=14 and view()['stars']//10>start_cycle:break
            require(view()['stars']>=10,'Source obstacle/reward path did not complete a sticker cycle');require(peak_speed>3,'Full sticker cycle did not increase scrolling speed')
            record('all fifteen actual source patterns scroll past one shared hand; four contributors collect a looping sticker cycle through real drags')
            button(clients[0],'Drive leave');wait(lambda:not view()['members'][0]['joined'],'explicit independent leave');require(all(m['joined'] for m in view()['members'][1:]),'Sibling participation lost');button(clients[0],'Drive join')
            home.capture(clients[0],out,'window-shared-rewards')
            button(clients[1],'Drive next');wait(lambda:view()['stage']==3,'shared destination');assert_views()
            for i,b in enumerate(view()['bricks']):
                c=clients[i%4];button(c,'Drive building brick '+b['id']);wait(lambda:next(v for v in view()['bricks'] if v['id']==b['id'])['holder']==c.profile,'one original brick editor');button(c,'Drive statue place '+str(b['variant']));wait(lambda:next(v for v in view()['bricks'] if v['id']==b['id'])['snapped'],'piece snapped at source anchor')
            wait(lambda:view()['statueBuilt'],'twelve original pieces assembled');require(view()['statues']==1,'Duplicate completion')
            home.capture(clients[0],out,'big-peanut-complete');record('four contributors build one exact original twelve-piece Peanut at recovered camera anchors')
            button(clients[2],'Drive free-build');button(clients[2],'Drive palette item 11');wait(lambda:any(b['holder']==clients[2].profile for b in view()['bricks']),'original palette brick')
            # An actual background tap places the held source brick on the free board.
            s=control(clients[2],'Drive recovered scenery')['bounds'];point=dict(role='screen',x=s['x']+s['width']*.82,y=s['y']+s['height']*.30);clients[2].input('press',**point);clients[2].input('release',**point);home.ready(clients[2])
            wait(lambda:all(b['holder']=='' for b in view()['bricks']),'free building placement')
            record('the original palette and normal free-board tap retain a separately identified shared brick')
        last=clients.pop();last.close();wait(lambda:len(server.state()['connected'])==3,'independent native disconnect');clients.append(run.start('client',last.profile));home.ready(clients[-1]);wait(lambda:clients[0].input('inspect')['visibleDrivePlayers']==4,'native reconnect catch-up')
        require(view()['statueBuilt'] and view()['stars']>=10,'Reconnect changed shared progress');record('native disconnect/reconnect catches up without resetting siblings or their creation')
        c=clients[0];c.input('resize',x=1280,y=591);home.ready(c);home.capture(c,out,'phone-shaped-destination');c.input('resize',x=1024,y=768);home.ready(c);home.capture(c,out,'tablet-shaped-destination')
        write(out/'checkpoint-drive.json',view())
        if a.resume and not (a.continue_roadworks or a.continue_window):
            count=len(view()['bricks']);button(clients[0],'Drive smash');wait(lambda:view()['smashes']>=1 and not view()['smashing'],'source car tumble')
            require(len(view()['bricks'])==count and not view()['statueBuilt'],'Tumble erased original pieces or left completed statue')
            home.capture(clients[0],out,'car-tumble-preserved-pieces');record('actual moving car tumbles the original statue and preserves every shared piece')
            build_statue()
            wait(lambda:view()['statueBuilt'],'shared rebuild');trip=view()['trip']
            for c in clients[:-1]:button(c,'Drive replay');require(view()['trip']==trip,'Replay reset before family consent')
            button(clients[-1],'Drive replay');wait(lambda:view()['trip']==trip+1 and view()['stage']==0,'one common next trip')
            require(len(view()['bricks'])==count,'Another trip erased the family creation');record('four real replay choices start one next packing trip while retaining the creation')
        passed=True
    finally:
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,runId=run.run_id,checks=checks,runtimeErrors=errors,sourceScope='Go for a Drive and shared integration',concurrentWorldInputs=sorted(set(concurrent)),physicalDevices=False,ownerAcceptance=False,audioListening=False,installedServerTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True);require(not errors,'Runtime exception; inspect result')

if __name__=='__main__':main()
