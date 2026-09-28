"""Release authority and real touch acceptance for bounded creation storage."""
import argparse, base64, importlib.util, io, json, struct, time, zlib
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def decode(encoded):
    if not encoded:return dict(serial=0,foods=[],pictures=[],removed=[])
    f=io.BytesIO(zlib.decompress(base64.b64decode(encoded),-15))
    def n(fmt):return struct.unpack('<'+fmt,f.read(struct.calcsize('<'+fmt)))[0]
    def s():
        size=n('H');return None if size==65535 else f.read(size).decode('utf-8')
    def dish():
        d=dict(id=s(),recipe=s(),step=n('i'),portions=n('i'),heated=n('?'),assisted=n('?'),guided=n('?'),experiment=n('?'),recipeVersion=n('i'),layers=n('i'),icingMask=n('i'),cutMask=n('i'),stage=s(),mixed=n('f'),poured=n('f'),heat=n('d'))
        d['ingredients']=[dict(unit=s(),ingredient=s(),by=s(),phase=s(),x=n('f'),y=n('f')) for _ in range(n('B'))];return d
    def pictures():
        out=[]
        for _ in range(n('B')):
            p=dict(key=n('i'),owner=s(),page=n('B'),displayed=n('?'),drawing=dict(revision=n('q')))
            for field in ['colors','undo','redo']:p['drawing'][field]=[n('H') for _ in range(n('B'))]
            out.append(p)
        return out
    require(n('B')==1,'archive version');data=dict(serial=n('i'))
    data['foods']=[dict(key=n('i'),owner=s(),dish=dish()) for _ in range(n('B'))]
    data['pictures']=pictures();data['removed']=pictures();require(f.read()==b'','archive trailing bytes');return data

def same_food(a,b,key=''):
    # Unity's public JSON writes null strings as empty strings and rounds float
    # coordinates. The archive intentionally retains the original CLR values.
    if key in ('stage','phase'):return (a or '')==(b or '')
    if isinstance(a,dict):return isinstance(b,dict) and a.keys()==b.keys() and all(same_food(v,b[k],k) for k,v in a.items())
    if isinstance(a,list):return isinstance(b,list) and len(a)==len(b) and all(same_food(x,y) for x,y in zip(a,b))
    if key in ('x','y','mixed','poured'):return abs(a-b)<2e-7
    return a==b

def finish_run(build,run_id):
    run=Run(build,resume=run_id,extended_test_lifetime=True);out=run.path/'home-creations';prior=json.loads((out/'results.json').read_text())
    require(prior['build']==build and len(prior['checks'])==5 and not prior['passed'],'Expected completed food and picture UI checks')
    write(out/'initial-results.json',prior);checks=prior['checks'];passed=False
    expected=decode(json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])['homeCreations'])
    try:
        server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(decode(server.state()['view']['homeCreations'])==expected,'first restart changed pictures')
        require(home.command(a,0,x=710,y=420)['accepted'],'landing placement')
        require(home.command(a,13,target='home-bedroom-1')['accepted'],'bedroom entry');home.ready(a)
        wait(lambda:a.input('inspect')['zone']=='home-bedroom-1','client bedroom scene')
        require(home.command(a,0,x=1000,y=200)['accepted'],'picture position');time.sleep(2);home.ready(a)
        home.capture(a,out,'bedroom-saved-picture');a.input('touchButton',text='Look at room picture');time.sleep(.5)
        require(any('Bedroom pictures' in t for t in a.input('inspect')['visibleText']),'bedroom frame does not open saved picture')
        home.capture(a,out,'bedroom-folder');a.input('touchButton',text='Close creations');time.sleep(.5);home.capture(a,out,'bedroom-after-folder')
        run.close();run=Run(build,resume=run_id,extended_test_lifetime=True);server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(decode(server.state()['view']['homeCreations'])==expected,'second restart lost collection')
        checks.append('bedroom frame opens saved art and closing its folder preserves the frame; two authority restarts retain all four collections');print('PASS '+checks[-1],flush=True)
        run.close()
        for log in run.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'native exception '+str(log))
        checks.append('native authority and clients close without exceptions');print('PASS '+checks[-1],flush=True);passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=build,previous=prior['previous'],checks=checks,continuedSameStoppedWorld=True,physicalDevicesTested=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=202);p.add_argument('--finish-run');args=p.parse_args()
    if args.finish_run:return finish_run(args.build,args.finish_run)
    old=Run(args.previous,extended_test_lifetime=True);run=None;out=old.path/'home-creations';out.mkdir();checks=[];passed=False
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def button(v,name):
        wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name)
        v.input('touchButton',text=name);time.sleep(.3);home.ready(v)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
    def move(v,x):cmd(v,0,x=x,y=200);time.sleep(2);home.ready(v)
    print('EVIDENCE '+str(out),flush=True)
    try:
        old.start('server');a=old.start('client',old.slots[0]['profile']);home.ready(a);move(a,-1700)
        cmd(a,18,item='cookware-0',target='PIZ-02',value='easy:start');cmd(a,18,item='ingredient-sauce',target='cookware-0',value='easy:add')
        move(a,-5460);cmd(a,19,item=a.profile,target='0@0',value='fill:0:5');old.close()
        before=json.loads((old.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])
        run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def archive():return decode(world()['homeCreations'])
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def work(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        require(world()['schema']==23 and world()['discovery']==before['discovery'],'migration coloring')
        require(toy('cookware-0')['kitchen']==next(t for t in before['toys'] if t['id']=='cookware-0')['kitchen'],'migration food')
        record('202 coloring and food migrate unchanged into schema 23')
        dishes=[]
        for i,v in enumerate(clients):
            home.ready(v);move(v,-2300)
            if i:cmd(v,18,item='cookware-'+str(i),target='PIZ-02',value='easy:start')
            v.input('touch-begin',role='cookware-'+str(i),finger=49);v.input('touch-end',role='cookware-'+str(i),finger=49);time.sleep(.5);home.ready(v)
            dishes.append(deepcopy(toy('cookware-'+str(i))['kitchen']['dish']))
            button(v,'Put food away')
            require(not toy('cookware-'+str(i))['kitchen']['dish']['id'],'tray not freed')
            require(same_food(archive()['foods'][-1]['dish'],dishes[-1]),'stored food changed')
            button(v,'My saved food')
        record('four real Put away controls preserve food and free four trays')
        for width,height,label in [(1280,591,'phone'),(1024,768,'tablet')]:
            a.input('resize',x=width,y=height);time.sleep(.4);home.capture(a,out,'saved-food-'+label)
        for i,v in enumerate(clients):
            button(v,'Use saved creation');require(same_food(toy('cookware-'+str(i))['kitchen']['dish'],dishes[i]),'restore food changed');button(v,'Back')
        require(not archive()['foods'],'restoration copied rather than moved');record('four Bring out controls restore original identities ingredients and preparation')
        for v in clients:
            move(v,-5460);button(v,'Coloring table')
            button(v,'Choose picture')
            for _ in range(3):
                visible=[int(c['name'].split()[-1]) for c in v.input('inspect')['controls'] if c['name'].startswith('Coloring page ')]
                if 6 in visible:break
                button(v,'More pictures' if max(visible)<6 else 'Earlier pictures')
            button(v,'Coloring page 6')
            page=work(v)['pages'][6];cmd(v,19,item=v.profile,target='6@'+str(page['revision']),value='fill:0:4')
            button(v,'Keep picture');button(v,'Keep picture');button(v,'My pictures');button(v,'Use saved creation')
        require(len(archive()['pictures'])==4 and all(p['displayed'] for p in archive()['pictures']),'four displays or duplicate save')
        record('four independent picture folders save once and hang one picture per owner')
        for width,height,label in [(1280,591,'phone'),(1024,768,'tablet')]:
            a.input('resize',x=width,y=height);time.sleep(.4);home.capture(a,out,'saved-picture-'+label)
            info=a.input('inspect');controls={c['name']:c for c in info['controls']}
            for name in ['Close creations','Use saved creation','Put picture away','Undo picture removal']:
                r=controls[name]['bounds'];require(r['width']>=44 and r['height']>=44,'small '+name);require(r['x']>=0 and r['y']>=0 and r['x']+r['width']<=info['screenWidth']+1 and r['y']+r['height']<=info['screenHeight']+1,'clipped '+name)
        button(a,'Put picture away');require(len(archive()['pictures'])==3,'remove');button(a,'Undo picture removal');require(len(archive()['pictures'])==4,'undo remove')
        button(a,'Close creations');page=work(a)['pages'][6];cmd(a,19,item=a.profile,target='6@'+str(page['revision']),value='fill:0:1')
        require(next(p for p in archive()['pictures'] if p['owner']==a.profile)['drawing']['colors'][0]==4,'saved picture mutated');home.capture(a,out,'coloring-storage-controls')
        record('native collection targets fit phone and tablet; picture undo and immutable saved copy work')
        button(a,'Back to Home');move(a,-2980);cmd(a,0,x=-2980,y=420);cmd(a,11)
        wait(lambda:any(p['id']==a.profile and p['zone']=='home-upstairs' and p['stairs']==0 for p in a.state()['view']['players']),'client stairs');home.ready(a)
        cmd(a,0,x=710,y=420);cmd(a,13,target='home-bedroom-1');home.ready(a)
        wait(lambda:any(p['id']==a.profile and p['zone']=='home-bedroom-1' for p in a.state()['view']['players']),'client bedroom');home.ready(a)
        move(a,1000);home.capture(a,out,'bedroom-saved-picture');button(a,'Look at room picture')
        require(any('Bedroom pictures' in t for t in a.input('inspect')['visibleText']),'room frame did not open picture')
        home.capture(a,out,'bedroom-folder');button(a,'Close creations');home.capture(a,out,'bedroom-after-folder')
        require(b.input('inspect')['discoveryOpen']==False,'sibling gallery unexpectedly switched')
        expected=deepcopy(archive());run.close();run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(archive()==expected,'server restart lost collection');record('bedroom display and authority restart preserve all four personal collections')
        run.close()
        for log in old.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'native exception '+str(log))
        record('native authority and clients close without exceptions');passed=True
    finally:
        if run:run.close()
        old.close();write(out/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,physicalDevicesTested=False,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
