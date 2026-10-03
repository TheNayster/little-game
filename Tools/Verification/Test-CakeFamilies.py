# /// script
# dependencies = ["cryptography"]
# ///
"""Four disposable release cooks: actual controls, recipe-specific stages and cold recovery."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time, json
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True);runid=run.path.name;out=run.path/'cake-families';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        def world():return wait(lambda:server.state(),'authority')['view']
        def food(i):return next(t for t in world()['toys'] if t['id']=='cookware-'+str(i))['kitchen']['dish']
        def button(v,name):
            wait(lambda:any(x['name']==name and x['enabled'] for x in v.input('inspect')['controls']),'enabled '+name)
            v.input('touchButton',text=name);time.sleep(.3);home.ready(v)
        def stage(i,value):
            wait(lambda:food(i)['stage']==value,'server '+value,20)
            wait(lambda:next(t for t in clients[i].state()['view']['toys'] if t['id']=='cookware-'+str(i))['kitchen']['dish']['stage']==value,'visible '+value);time.sleep(.35)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v);return r
        def record(name):checks.append(name);print('PASS '+name,flush=True)
        def header(v):
            info=v.input('inspect');controls={c['name']:c for c in info['controls']}
            boxes=[controls[n]['bounds'] for n in ['Back','Help','My saved food','Put food away']]
            for i,a in enumerate(boxes):
                require(a['width']>=44 and a['height']>=44,'small header target')
                for b in boxes[i+1:]:
                    require(min(a['x']+a['width'],b['x']+b['width'])<=max(a['x'],b['x']) or min(a['y']+a['height'],b['y']+b['height'])<=max(a['y'],b['y']),'overlapping cooking header targets')
        recipes=['CAK-01','CAK-03','CAK-04','CAK-05']
        for i,v in enumerate(clients):
            home.ready(v);cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'Cakes');button(v,'Recipe '+recipes[i]);stage(i,'ingredients')
        require(len({food(i)['id'] for i in range(4)})==4,'Tray collision')
        for i,v in enumerate(clients):
            names=[c['name'] for c in v.input('inspect')['controls']];require('Add patty' not in names and 'Add cheese' not in names and 'Add icing' not in names,'Wrong stage ingredient')
            for name in ['egg','milk']+(['carrot'] if i==3 else []):button(v,'Add '+name)
        stage(3,'chop');home.capture(clients[3],out,'01-carrot-chopping')
        button(clients[3],'Cooking action');stage(3,'mix');record('four actual recipe choices select distinct counted batters with only stage ingredients')
        for i,v in enumerate(clients):button(v,'Cooking action');stage(i,'colors' if i==2 else 'pour')
        colors=clients[2];button(colors,'Color Purple');home.capture(colors,out,'02-rainbow-first-color');partial=deepcopy(food(2));button(colors,'Back');button(colors,'Cook');require(food(2)==partial,'Color lost on close');button(colors,'Color Orange');button(colors,'Color Blue');stage(2,'pour')
        require(food(2)['step']==313,'Color order not retained')
        for i,v in enumerate(clients):
            v.input('resize',x=1280,y=591);home.ready(v);home.capture(v,out,'03-'+recipes[i]+'-molds-phone');header(v);button(v,'Cooking action');stage(i,'bake')
        record('rainbow colors survive closing; heart body-head round and colored molds are reachable on phone')
        for v in clients:button(v,'Cooking action')
        time.sleep(1);home.capture(clients[0],out,'04-shared-oven-countdown');button(clients[3],'Back');cmd(clients[3],7,value='park')
        for i in range(4):stage(i,'stack' if i in [0,2] else 'ice')
        require(all(food(i)['heated'] and food(i)['heat']==8 for i in range(4)),'Shared heat did not finish')
        record('four server-driven bakes settle safely, including the cook who leaves Home')
        cmd(clients[3],7,value='home');cmd(clients[3],0,x=-2300,y=200);time.sleep(2);button(clients[3],'Cook')
        for i,v in enumerate(clients):
            if i in [0,2]:
                home.capture(v,out,'05-'+recipes[i]+'-assembly');button(v,'Cooking action')
                if i==2:wait(lambda:food(i)['layers']==2,'second rainbow layer');time.sleep(.5);button(v,'Cooking action')
                stage(i,'ice')
            button(v,'Add icing');button(v,'Cooking action');stage(i,'features' if i==0 else 'decorate')
            if i==0:button(v,'Add duck beak');button(v,'Add duck eyes');stage(i,'decorate')
            button(v,'Add '+['popcorn','strawberry','sprinkles','carrot'][i]);home.capture(v,out,'06-'+recipes[i]+'-decorated-phone')
            v.input('resize',x=1024,y=768);home.ready(v);home.capture(v,out,'07-'+recipes[i]+'-decorated-tablet');header(v)
            button(v,'Cooking action');stage(i,'cut');button(v,'Cooking action');stage(i,'serve')
        record('duck assembly and icing features, rainbow stacking, heart berries and carrot decoration use actual controls')
        for i,v in enumerate(clients):
            ingredients=deepcopy(food(i)['ingredients']);button(v,'Plate '+str(i*2+1));button(v,'Plate '+str(i*2+2))
            plates=[next(t for t in world()['toys'] if t['id']=='plate-'+str(i*2+j))['kitchen']['dish'] for j in range(2)]
            require([p['portions'] for p in plates]==[1,2] and food(i)['portions']==12,'Portion duplication');require(all(p['ingredients']==ingredients for p in plates),'Serving changed counted ingredients')
            button(v,'Put food away')
        expected=deepcopy(world()['homeCreations']);require(expected,'Missing archived cakes');record('eight served portions and four saved remainders conserve food identities and preparation')
        for v in clients:require('Exception' not in (v.out/'player.log').read_text(errors='replace'),'Native exception')
        run.close();run=Run(args.build,resume=runid,extended_test_lifetime=True);server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for v in clients:home.ready(v)
        require(world()['homeCreations']==expected,'Stored cakes changed after cold restart')
        for i,v in enumerate(clients):
            cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'My saved food');home.capture(v,out,'08-'+recipes[i]+'-stored-remainder');button(v,'Use saved creation');time.sleep(.4)
        # The actual cookware identities may change when retrieving; food IDs do not.
        restored=[t['kitchen']['dish'] for t in world()['toys'] if t.get('kitchen') and t['id'].startswith('cookware-')]
        require(len({d['id'] for d in restored})==4 and all(d['portions']==12 for d in restored),'Retrieved remainder duplicated or changed')
        require(next(d for d in restored if d['recipe']=='CAK-04')['step']==313,'Archived rainbow color order changed')
        record('cold server restart and four rejoining cooks retrieve the same saved cakes and color order')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
