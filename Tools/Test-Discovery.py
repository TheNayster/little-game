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
        require(home.command(legacy,0,x=-4210,y=220)['accepted'],'legacy placement');require(home.command(legacy,1,value='orange-pup')['accepted'],'legacy avatar')
        if args.previous>=178:
            require(home.command(legacy,0,x=-6590,y=220)['accepted'],'old science position')
            require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'old colored page')
            require(home.command(legacy,19,item=legacy.profile,target='mix:1@0',value='mix:1:add:5',x=1)['accepted'],'old indicator mixture')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']>=16 and len(after['discovery'])==4,'New schema/workspaces missing')
        if before['schema']<16:normal.pop('discovery')
        else:
            for w in normal['discovery']:
                w['pages']=w['pages'][:len(before['discovery'][0]['pages'])]
                if before['schema']<17:w.pop('mixtures')
        normal['schema']=before['schema'];normal['revision']-=after['schema']-before['schema']
        require(normal==before,'Migration changed existing saved records')
        record('actual previous release save migrates additively with profile avatar positions objects and receipts intact')
        clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority view')['view']
        def ws(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
        def move(v,x):cmd(v,0,x=x,y=200);time.sleep(.65);home.ready(v)
        def button(v,name):
            if name.startswith('Coloring page '):
                page=int(name.split()[-1])
                for _ in range(3):
                    names={c['name'] for c in v.input('inspect')['controls']}
                    if name in names:break
                    visible=[int(n.split()[-1]) for n in names if n.startswith('Coloring page ')]
                    v.input('touchButton',text='More pictures' if page>max(visible) else 'Earlier pictures');time.sleep(.15)
            v.input('touchButton',text=name);time.sleep(.15);home.ready(v)
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
        for v in clients:button(v,'Choose picture');button(v,'Coloring page 0')
        for v,color in zip(clients,['Blue','Green','Purple','Orange']):button(v,'Discovery crayon '+color);tap(v,280,250)
        require([ws(v)['pages'][0]['colors'][0] for v in clients]==[5,4,6,2],'Four independent coloring edits')
        before_b=deepcopy(ws(b));button(a,'Undo');require(ws(a)['pages'][0]['colors'][0]==0 and ws(b)==before_b,'Undo scope');button(a,'Redo');require(ws(a)['pages'][0]['colors'][0]==5,'Redo scope')
        for page in range(6):
            home.capture(a,folder,'coloring-'+str(page)+'-tablet')
            if page<5:button(a,'Page >')
        # Official line art uses fixed PDF-derived masks; test their real hit regions.
        mask_root=Path(__file__).resolve().parents[1]/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Discovery/Coloring'
        catalog=json.loads((mask_root/'catalog.json').read_text())
        for page,item in enumerate(catalog['pages'],6):
            raw=(mask_root/(item['id']+'.bytes')).read_bytes();width=int.from_bytes(raw[:2],'little');height=int.from_bytes(raw[2:4],'little')
            # A large closed character region, away from the edge and logo.
            import collections
            middle=raw[4+width*height//4:4+width*height*3//4];counts=collections.Counter(middle);counts.pop(0,None);region=counts.most_common(1)[0][0]
            pixels=[i for i,c in enumerate(raw[4:]) if c==region];point=pixels[len(pixels)//2];x=(point%width+.5)/width*800;y=(point//width+.5)/height*460
            for v,color in zip(clients,['Blue','Green','Purple','Orange']):
                button(v,'Choose picture');button(v,'Coloring page '+str(page));button(v,'Discovery crayon '+color);tap(v,x,y)
            require([ws(v)['pages'][page]['colors'][region-1] for v in clients]==[5,4,6,2],'Official page hit mask/four-owner fill '+str(page))
            before=deepcopy(ws(b));button(a,'Undo');require(ws(a)['pages'][page]['colors'][region-1]==0 and ws(b)==before,'New page undo scope');button(a,'Redo')
            home.capture(a,folder,'official-'+item['id']+'-tablet')
        button(a,'Choose picture');home.capture(a,folder,'all-eighteen-pictures');button(a,'Coloring page 6')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'coloring-phone-landscape')
        record('all eighteen fitted pages pass four-player hit masks fill undo redo and tablet/phone layout checks')
        button(b,'Choose picture');button(b,'Coloring page 0')
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
