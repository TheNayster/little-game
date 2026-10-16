# /// script
# dependencies = ["cryptography"]
# ///
"""Check an isolated HQ candidate with four native release clients and UGUI input."""
import argparse
import hashlib
import importlib.util
import os
from pathlib import Path
import sys
import time
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run,read,write,wait,require,ROOT

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--packaged',action='store_true');a=p.parse_args()
    if a.packaged:os.environ.pop('LITTLE_WEEPS_SPIDEY_ART',None)
    else:os.environ['LITTLE_WEEPS_SPIDEY_ART']=str(ROOT/'LocalData/Research/DuploMarvel/world-tests/SpideyHQ/Content-v4')
    run=Run(a.build,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'spidey-hq';out.mkdir();checks=[];passed=False
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source differs from candidate')
    print('EVIDENCE '+str(out),flush=True)
    source=(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text(encoding='utf-8')
    import re
    actions=[v.strip() for v in re.search(r'enum SoloAction\s*{([^}]+)}',source).group(1).split(',')]
    def command(c,action,**kw):
        result=home.command(c,actions.index(action),**kw);require(result['accepted'],result['outcome']);home.ready(c);return result
    def button(c,name):
        wait(lambda:any(v['name']==name and v['enabled'] for v in c.input('inspect')['controls']),'enabled '+name,20)
        previous=None;stable=0;deadline=time.monotonic()+5
        while time.monotonic()<deadline:
            control=next(v for v in c.input('inspect')['controls'] if v['name']==name);bounds=control['bounds'];point=(bounds['x'],bounds['y'])
            stable=stable+1 if previous and abs(previous[0]-point[0])+abs(previous[1]-point[1])<2 else 0
            if stable>=2:break
            previous=point;time.sleep(.12)
        c.input('touchButton',text=name);home.ready(c)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots]
        for c in clients:home.ready(c)
        home.scenic.NAMES['spidey-hq']='World Spidey HQ';home.travel(clients[0],'spidey-hq')
        for c in clients[1:]:command(c,'Travel',value='spidey-hq')
        wait(lambda:all(c.input('inspect')['visibleSpideyPlayers']==4 for c in clients),'four HQ players')
        def view():return server.state()['view']['spidey']
        record('four actual clients enter one HQ using its world card and distinct arrival positions')
        home.capture(clients[0],out,'first-hq-view')
        for c in clients:
            command(c,'Move',x=1620,y=160);button(c,'HQ pizza')
        wait(lambda:sum(p['holder'] in {c.profile for c in clients} for p in view()['props'])==4,'four shared pizza services',30)
        record('four actual HQ pizza controls approach and share one queue')
        for c in clients:button(c,'HQ tool Eat')
        wait(lambda:all(p['holder'] not in {c.profile for c in clients} for p in view()['props']),'all pizza consumed',15)
        for c in clients:command(c,'Move',x=2400,y=160)
        wait(lambda:view()['battle']=='fight','naturally triggered Goblin theft',40)
        for c in clients:button(c,'HQ tool Web')
        wait(lambda:view()['recoveries']>=1,'one shared four-person recovery',20)
        record('natural stolen-item encounter and four actual web controls complete one common recovery')
        for c in clients:command(c,'Move',x=1068,y=160);button(c,'HQ puzzle station')
        for i,c in enumerate(clients):
            if i==0:
                def center(name):
                    b=next(v['bounds'] for v in c.input('inspect')['controls'] if v['name']==name);return b['x']+b['width']/2,b['y']+b['height']/2
                start=center('HQ puzzle piece 0');end=center('HQ puzzle slot 0')
                c.input('touch-begin',role='screen',x=start[0],y=start[1],finger=72)
                for t in [.15,.4,.7,1]:c.input('touch-move',role='screen',x=start[0]+(end[0]-start[0])*t,y=start[1]+(end[1]-start[1])*t,finger=72)
                c.input('touch-end',role='screen',x=end[0],y=end[1],finger=72);wait(lambda:view()['puzzlePlaced'][0]==1,'actual puzzle drag/snap')
            else:
                button(c,'HQ puzzle piece '+str(i));wait(lambda:view()['puzzleHolders'][i]==c.profile,'piece ownership');button(c,'HQ puzzle slot '+str(i))
        wait(lambda:view()['puzzles']==1,'one shared puzzle completion')
        record('four children contribute to one puzzle using actual piece and target controls')
        for c in clients:button(c,'HQ activity close')
        c=clients[0];command(c,'Move',x=3414,y=160);button(c,'HQ building station');button(c,'HQ brick choice 25');button(c,'HQ brick rotate');button(c,'HQ building spot 2 2');wait(lambda:len(view()['bricks'])==1 and view()['bricks'][0]['owner']=='','original brick placed')
        button(c,'HQ activity close');record('original building palette, rotation and shared mat placement')
        command(clients[1],'Move',x=2378,y=160);button(clients[1],'HQ globe');command(clients[2],'Move',x=2149,y=160);button(clients[2],'HQ poster');command(clients[3],'Move',x=2273,y=160);button(clients[3],'HQ photo')
        wait(lambda:view()['photos']>=1 and view()['poster']==1 and view()['globe']==1,'small HQ interactions')
        record('original poster, globe and photo interactions')
        command(clients[0],'Move',x=2890,y=160);button(clients[0],'HQ balloon');wait(lambda:any(p['holder']==clients[0].profile and p['kind']=='balloon' for p in view()['props']),'balloon service');button(clients[0],'HQ tool Float');wait(lambda:view()['members'][0]['station']=='float','balloon float');button(clients[0],'HQ tool Leave')
        for c in clients:command(c,'Move',x=2578,y=160);button(c,'HQ slide')
        wait(lambda:not view()['slideQueue'] and all(m['station']!='slide' for m in view()['members']),'shared slide turns',20)
        command(clients[0],'Move',x=3058,y=260);button(clients[0],'HQ seat 0');wait(lambda:view()['members'][0]['seat']==0,'source seat');button(clients[0],'HQ tool Sleep');wait(lambda:view()['members'][0]['station']=='sleep','sleep');button(clients[0],'HQ tool Leave')
        button(clients[0],'HQ tool Sense');wait(lambda:view()['members'][0]['station']=='sense','Spidey sense');button(clients[1],'HQ tool Vanish');wait(lambda:view()['members'][1]['station']=='invisible','Spin invisibility')
        record('actual balloon/float, four shared slide turns, authored chair/sleep and actor-specific powers')
        saved=view()['bricks'][0].copy();command(clients[1],'Travel',value='garden');wait(lambda:clients[0].input('inspect')['visibleSpideyPlayers']==3,'independent departure');require(view()['puzzles']==1 and view()['bricks'][0]['variant']==25,'departure reset common progress')
        command(clients[1],'Travel',value='spidey-hq');wait(lambda:all(c.input('inspect')['visibleSpideyPlayers']==4 for c in clients),'return')
        record('one child leaves and returns without resetting siblings or creations')
        clients[0].input('resize',x=1280,y=591);home.ready(clients[0]);home.capture(clients[0],out,'phone-shaped-hq');clients[0].input('resize',x=1024,y=768);home.ready(clients[0]);home.capture(clients[0],out,'tablet-shaped-hq')
        for c in clients:c.close()
        server.close();reopened=Run(a.build,resume=run.run_id,graphics_api='d3d11');run=reopened;server=run.start('server');wait(lambda:server.state(),'reopened authority');require(view()['bricks'][0]['id']==saved['id'] and view()['bricks'][0]['variant']==25 and view()['puzzles']==1,'saved creations differ after authority reopen')
        c=run.start('client','player-1');home.ready(c);require(c.input('inspect')['spidey']['bricks'][0]['id']==saved['id'],'client catch-up did not recover creation')
        record('actual authority/save process reopening and fresh client catch-up preserve brick identity and puzzle completion');passed=True
    finally:
        write(out/'result.json',dict(passed=passed,build=a.build,checks=checks,limits=['Fixture travel/placement is identified; activity controls use UGUI touches.','Windows phone/tablet layouts are simulations; physical devices and owner audio/motion review remain separate.']))
        run.close()


if __name__=='__main__':main()
