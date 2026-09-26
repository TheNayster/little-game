# /// script
# dependencies = ["cryptography"]
# ///
"""One home destination and an unobscured real character above the chooser."""
import argparse
import importlib.util
import time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'home-navigation';folder.mkdir();checks=[]
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2');home.ready(a);home.ready(b)
        def player(c):return next(p for p in server.state()['view']['players'] if p['id']==c.profile)
        def settled():return wait(lambda:not a.input('inspect')['pending'],'settled')
        for w,h,label in [(1280,591,'phone'),(1024,768,'tablet')]:
            a.input('resize',x=w,y=h);home.ready(a)
            for x,end in [(-4450,'house'),(3500,'backyard')]:
                require(home.command(a,0,x=x,y=35)['accepted'],'Fixture position rejected');time.sleep(.6)
                before=dict(player(a));sibling=dict(player(b));toys=server.state()['view']['toys']
                a.input('touchButton',text='Characters');time.sleep(.2)
                s=home.capture(a,folder,label+'-'+end+'-bluey')
                require(s['charactersOpen'] and s['activeCharacterVisible'],'World character hidden by chooser')
                require(not any(c['name']=='World Garden' for c in s['controls']),'Duplicate backyard destination')
                a.input('touchButton',text='Bingo');settled();time.sleep(.2)
                s=home.capture(a,folder,label+'-'+end+'-bingo')
                require(s['character']=='bingo' and s['activeCharacterVisible'],'Bingo hidden after selection')
                after=player(a)
                require({k:v for k,v in before.items() if k!='avatar'}=={k:v for k,v in after.items() if k!='avatar'},'Chooser moved or reset player')
                a.input('touchButton',text='World Heeler Home');settled()
                s=a.input('inspect');require(not s['worldLoading'] and not s['charactersOpen'],'Current property should resume directly')
                require(player(a)==after and player(b)==sibling and server.state()['view']['toys']==toys,'Home button changed persistent state')
                require(home.command(a,1,value='blue-pup')['accepted'],'Reset fixture avatar')
            checks.append(label+': one home destination, bottom-edge Bluey/Bingo visible, selection and close preserve state')
        home.scenic.travel(a,'creek');home.ready(a)
        a.input('touchButton',text='Characters')
        for _ in range(12):
            s=a.input('inspect');c=next((c for c in s['controls'] if c['name']=='World Heeler Home'),None)
            if c and c['bounds']['height']>=c['bounds']['width']*.97:break
            home.scenic.swipe(a,False)
        a.input('touchButton',text='World Heeler Home');s=home.ready(a)
        require(s['zone']=='garden' and s['place']=='home' and 'scenery-ready' in s['travelStages'],'Return from another world failed')
        checks.append('Creek to single home entry retains real loading and scenery readiness')
        require(home.command(a,0,x=-60,y=150)['accepted'],'Boundary fixture');time.sleep(.4)
        before=dict(player(a));a.input('touchButton',text='Tap to walk');a.input('touch-begin',role='stick',x=55,finger=77)
        wait(lambda:player(a)['x']>60,'walk directly into backyard');a.input('touch-end',role='stick',finger=77);settled()
        require(player(a)['zone']==before['zone'] and player(a)['visit']==before['visit'] and not a.input('inspect')['worldLoading'],'House/backyard introduced a trip')
        checks.append('real joystick crosses house/backyard boundary without travel or a new visit')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,liveFamilyTouched=False))
        print('RESULT '+str(folder/'result.json'),flush=True)
    finally:run.close()

if __name__=='__main__':main()
