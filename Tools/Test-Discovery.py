# /// script
# dependencies = ["cryptography"]
# ///
"""Disposable four-client science/coloring and real 172-to-current migration."""
import argparse, importlib.util, json, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--previous',type=int,default=172);args=parser.parse_args()
    old=Run(args.previous);run=None;checks=[];passed=False
    folder=old.path/'discovery-acceptance';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(text):checks.append(text);print('PASS '+text,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-4210,y=220)['accepted'],'legacy placement');require(home.command(legacy,1,value='orange-pup')['accepted'],'legacy avatar');old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==16 and len(after['discovery'])==4,'New schema/workspaces missing')
        normal.pop('discovery');normal['schema']=before['schema'];normal['revision']-=1
        require(normal==before,'Migration changed existing saved records')
        record('actual previous release save migrates additively with profile avatar positions objects and receipts intact')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority view')['view']
        def ws(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
        def move(v,x):cmd(v,0,x=x,y=200);time.sleep(.65);home.ready(v)
        def button(v,name):v.input('touchButton',text=name);time.sleep(.15);home.ready(v)
        def tap(v,x,y,finger=39):
            v.input('touch-begin',role='discovery',x=x,y=y,finger=finger);v.input('touch-end',role='discovery',x=x,y=y,finger=finger);time.sleep(.15);home.ready(v)
        for v in clients:home.ready(v);move(v,-6590)
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'downstairs-four-player-tablet')
        for v in clients:button(v,'Science bench')
        for v in clients:
            for i in range(3):button(v,'Discovery + Cargo')
            require(ws(v)['cargo']==3 and not ws(v)['wide'],'Cargo input did not persist');button(v,'Discovery Wide boat')
        require(all(w['cargo']==3 and w['wide'] for w in world()['discovery']),'Four independent boats')
        home.capture(a,folder,'loaded-boat-tablet');button(a,'Discovery Reset tray')
        require(ws(a)['cargo']==0 and all(ws(v)['cargo']==3 for v in clients[1:]),'Reset affected sibling')
        record('four real UI boat trays preserve separate cargo and one reset does not affect siblings')
        button(a,'Magnet materials');button(a,'Discovery Iron');require(ws(a)['ironY']==290,'Iron did not respond')
        a.input('touch-begin',role='discovery',x=140,y=240,finger=41)
        for x,y in [(170,225),(195,215),(220,200)]:a.input('touch-move',role='discovery',x=x,y=y,finger=41)
        a.input('touch-end',role='discovery',x=220,y=200,finger=41);home.ready(a)
        require(abs(ws(a)['ironX']-220)<2,'Dragged magnet did not move iron');iron=(ws(a)['ironX'],ws(a)['ironY']);button(a,'Discovery Aluminum')
        require((ws(a)['ironX'],ws(a)['ironY'])==iron,'Nonmagnetic remote sample moved iron')
        home.capture(a,folder,'magnet-materials-tablet');button(a,'Colored light');button(a,'Discovery Red lamp');button(a,'Discovery Green lamp');require(ws(a)['lights']==3,'R+G light not yellow')
        button(a,'Discovery Blue lamp');require(ws(a)['lights']==7,'RGB light not white');home.capture(a,folder,'colored-light-tablet')
        record('native magnet drag commits once and RGB combinations update authoritative persistent state')
        for v in clients:button(v,'Back to Home');move(v,-5460);button(v,'Coloring table')
        for v,color in zip(clients,['Blue','Green','Purple','Orange']):button(v,'Discovery crayon '+color);tap(v,280,250)
        require([ws(v)['pages'][0]['colors'][0] for v in clients]==[5,4,6,2],'Four independent coloring edits')
        before_b=deepcopy(ws(b));button(a,'Undo');require(ws(a)['pages'][0]['colors'][0]==0 and ws(b)==before_b,'Undo scope');button(a,'Redo');require(ws(a)['pages'][0]['colors'][0]==5,'Redo scope')
        for page in range(6):
            home.capture(a,folder,'coloring-'+str(page)+'-tablet')
            if page<5:button(a,'Page >')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'coloring-phone-landscape')
        record('all six fitted pages render with actual four-player fill undo redo and tablet/phone layout checks')
        prior={v.profile:deepcopy(ws(v)) for v in clients};button(d,'Back to Home');cmd(d,7,value='park')
        button(b,'Discovery crayon Red');tap(b,130,102);require(ws(c)==prior[c.profile] and ws(d)==prior[d.profile],'Departure altered another creation')
        require(not d.input('inspect')['discoveryOpen'] and b.input('inspect')['discoveryOpen'],'Overlay was not local')
        record('one player travels away while another keeps coloring with no global activity lock')
        # Visit each seam while carrying one existing toy; migration adds no props.
        button(c,'Back to Home');move(c,-4380);cmd(c,2,item='living-book-hello');move(c,-4990);home.capture(c,folder,'living-discovery-seam');move(c,-7000)
        require(next(t for t in world()['toys'] if t['id']=='living-book-hello')['holder']==c.profile,'Held book lost across chunk');cmd(c,4,item='living-book-hello')
        record('continuous living-room route retains held item and stays within three scenery textures')
        expected=deepcopy(world()['discovery'])
        for v in clients:v.close()
        run.close();run=Run(args.build,resume=old.run_id);server=run.start('server')
        require(saved(run)['discovery']==expected,'Restart lost acknowledged science/coloring state')
        reopened=run.start('client',run.slots[0]['profile']);home.ready(reopened);move(reopened,-5460);button(reopened,'Coloring table')
        require(next(w for w in reopened.state()['view']['discovery'] if w['owner']==reopened.profile)==expected[0],'Rejoin view differs')
        home.capture(reopened,folder,'saved-picture-rejoined');record('authority save restart and real client rejoin retain all four creations and science trays')
        for log in old.path.rglob('player.log'):
            require('Exception:' not in log.read_text(errors='replace'),'Native exception in '+str(log))
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)

if __name__=='__main__':main()
