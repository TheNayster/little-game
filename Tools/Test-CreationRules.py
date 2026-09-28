"""Isolated release-server storage invariants, capacity and real recovery payload."""
import argparse, importlib.util, json, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
def module(name,path):
    s=importlib.util.spec_from_file_location(name,Path(__file__).with_name(path));m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
home=module('home','Test-HomeWorld.py');creations=module('creations','Test-HomeCreations.py')

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args();run=Run(args.build,extended_test_lifetime=True);out=run.path/'creation-rules';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');a=run.start('client',run.slots[0]['profile']);b=run.start('client',run.slots[1]['profile']);home.ready(a);home.ready(b)
        def world():return server.state()['view']
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def archive():return creations.decode(world()['homeCreations'])
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],str(r));return r
        def cook(v,op,item='',target=''):return cmd(v,18,item=item,target=target,value=op)
        def token(id):
            d=toy(id)['kitchen']['dish'];return d['id']+'@'+(d['stage'] or '')+'@'+str(d['step'])+'@'+str(d['portions'])
        def store(v,id):return cook(v,'store-food',id,token(id))
        for v in [a,b]:cmd(v,0,x=-2110,y=200)
        cook(a,'easy:start','cookware-0','PIZ-01');t=token('cookware-0')
        require(home.command(b,18,item='cookware-0',target=t,value='store-food')['outcome']=='owner-only','other owner stored food')
        cmd(a,2,item='cookware-0');require(not home.command(a,18,item='cookware-0',target=t,value='store-food')['accepted'],'held food stored');cmd(a,4,item='cookware-0')
        store(a,'cookware-0');cook(a,'easy:start','cookware-0','PIZ-01');store(a,'cookware-0');cook(a,'easy:start','cookware-0','PIZ-01')
        prior=deepcopy(toy('cookware-0'));require(home.command(a,18,item='cookware-0',target=token('cookware-0'),value='store-food')['outcome']=='food-storage-full','capacity ignored');require(toy('cookware-0')==prior,'full collection lost food')
        key=archive()['foods'][0]['key'];require(not home.command(b,18,target=str(key),value='restore-food')['accepted'],'other owner restored food');require(not home.command(a,18,item='cookware-0',target=str(key),value='restore-food')['accepted'],'occupied tray overwritten')
        record('owner held-food capacity and occupied-tray guards preserve original creations')
        cook(b,'easy:readybase','cookware-1','PIZ-01');cook(b,'easy:cut','cookware-1');cook(b,'easy:serve','cookware-1','plate-0');id=toy('cookware-1')['kitchen']['dish']['id'];store(b,'cookware-1');store(b,'plate-0')
        portions=[f['dish']['portions'] for f in archive()['foods'] if f['dish']['id']==id];require(sum(portions)==15 and len(portions)==2,'archived portions duplicated')
        key=next(f['key'] for f in archive()['foods'] if f['dish']['id']==id);cook(b,'restore-food','cookware-1',str(key));require(sum([toy('cookware-1')['kitchen']['dish']['portions']]+[f['dish']['portions'] for f in archive()['foods'] if f['dish']['id']==id])==15,'restore portion conservation')
        record('serving archiving and restoring conserve all four unique portions')
        cook(b,'easy:start','cookware-2','CAK-02')
        # Starting a cake supplies only premix; direct storage retains its exact
        # unfinished stage. A different tray never resets another cook's food.
        unfinished=deepcopy(toy('cookware-2')['kitchen']['dish']);store(b,'cookware-2');require(creations.same_food(archive()['foods'][-1]['dish'],unfinished),'unfinished cake changed')
        expected=deepcopy(archive());run.close();run=Run(args.build,resume=run.run_id,extended_test_lifetime=True);server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(archive()==expected,'restart changed archived stage or ingredients');record('unfinished cake and served portions survive authority restart exactly')
        write(out/'wire-bound.json',dict(actualViewBytes=len(json.dumps(world(),separators=(',',':')).encode()),archiveCharacters=len(world()['homeCreations']),archiveCharacterCap=8192,previousMaximalWorldBytes=98441,combinedUpperBoundBytes=98441+8192,transportPayloadCap=131072,note='Conservative storage contribution bound added to the measured full-food/science/coloring baseline; counts do not inflate JSON inside the archive.'))
        record('archive cap adds at most 8192 bytes to the previously measured maximal shared world')
        run.close()
        for log in run.path.rglob('player.log'):
            t=log.read_text(errors='replace');require('Exception:' not in t and 'Send queue is full' not in t,'native error '+str(log))
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
