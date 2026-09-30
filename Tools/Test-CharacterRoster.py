"""Focused native chooser checks with an isolated four-player family."""
import argparse
import importlib.util
from pathlib import Path
import time
import re
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
ROOT=Path(__file__).resolve().parents[1]
CAST=[(name,avatar,art) for avatar,art,name in re.findall(
    r'new Entry\("([^"]+)","([^"]+)","([^"]+)"',
    (ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/PlayableCharacters.cs').read_text())]
assert len(CAST)==37


def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int)
    parser.add_argument('--departure-only',action='store_true')
    parser.add_argument('--visual-only',action='store_true');args=parser.parse_args()
    run=Run(args.build);folder=run.path/'character-roster';folder.mkdir();checks=[]
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client','player-'+str(i)) for i in range(1,5)]
        a=clients[0]
        # Windows can briefly deny the diagnostic file during replacement.
        # Retry a fresh read, without inventing or retaining cached state.
        def authority():return wait(lambda:server.state(),'read authoritative diagnostic')
        def player(c):return next(p for p in authority()['view']['players'] if p['id']==c.profile)
        def reveal(name):
            for _ in range(15):
                state=a.input('inspect');control=next((c for c in state['controls'] if c['name']==name),None)
                if control and control['bounds']['width']>state['screenHeight']*.20:return state
                w,h=state['screenWidth'],state['screenHeight'];y=h*.14
                a.input('touch-begin',role='screen',x=w*.73,y=y,finger=91)
                for f in (.2,.4,.6,.8,1):a.input('touch-move',role='screen',x=w*(.73-.47*f),y=y,finger=91)
                a.input('touch-end',role='screen',x=w*.26,y=y,finger=91);time.sleep(.18)
            raise AssertionError('Cannot reveal '+name)
        for width,height,label in ([] if args.departure_only else [(1280,591,'phone'),(1024,768,'tablet')]):
            a.input('resize',x=width,y=height);home.ready(a)
            a.input('touchButton',text='Characters')
            for name,avatar,art in (CAST if not args.visual_only else [c for c in CAST if c[0] in {'Bluey','Bandit','Judo','Hercules','Mia','Captain'}]):
                state=reveal(name);before=dict(player(a));toys=authority()['view']['toys']
                a.input('touchButton',text=name)
                wait(lambda:player(a)['avatar']==avatar,'selected '+name)
                state=home.ready(a)
                require(state['character']==art,'Wrong artwork for '+name)
                require({k:v for k,v in player(a).items() if k!='avatar'}=={k:v for k,v in before.items() if k!='avatar'},'Selection changed player state')
                require(authority()['view']['toys']==toys,'Selection changed belongings')
                home.capture(a,folder,label+'-'+art)
            a.input('touchButton',text='Close characters')
            checks.append(label+(': representative tall portraits and final entries fit the shelf' if args.visual_only else ': horizontal browsing and all 37 pictured selections preserve state'))
        if args.visual_only:
            write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,liveFamilyTouched=False))
            print('PASS '+str(folder/'result.json'),flush=True);return
        for c in clients:
            require(home.command(c,1,value='captain')['accepted'],'Duplicate favorite rejected')
        wait(lambda:all(p['avatar']=='captain' for p in authority()['view']['players']),'four favorites')
        wait(lambda:all(c.input('inspect')['character']=='captain' for c in clients),'four rendered favorites')
        try:clients[0].close()
        except Exception:
            print('Departure exit code: '+str(clients[0].process.returncode),flush=True);raise
        wait(lambda:len(authority()['connected'])==3,'independent departure')
        require(home.command(clients[1],1,value='bandit')['accepted'],'Sibling cannot change after departure')
        wait(lambda:clients[1].input('inspect')['character']=='bandit','remaining sibling artwork')
        checks.append('four clients choose the same favorite; one exits and siblings continue changing characters')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,liveFamilyTouched=False))
        print('PASS '+str(folder/'result.json'),flush=True)
    finally:run.close()


if __name__=='__main__':main()
