# /// script
# dependencies = ["cryptography"]
# ///
"""Four disposable native cooks exercise pizza touch stages, servings and retained creations."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from pathlib import Path
from copy import deepcopy
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args();run=Run(args.build,extended_test_lifetime=True);runid=run.path.name;out=run.path/'pizza-preparation';out.mkdir();checks=[];passed=False
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
        def stroke(v,x,y):
            v.input('touch-begin',role='ui:Cake activity',finger=41);v.input('touch-move',role='ui:Cake activity',x=x,y=y,finger=41);v.input('touch-end',role='ui:Cake activity',x=x,y=y,finger=41);time.sleep(.4);home.ready(v)
        for i,v in enumerate(clients):
            home.ready(v);cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'Recipe PIZ-0'+str(i+1));stage(i,'knead')
        a=clients[0];a.input('resize',x=1280,y=591);home.ready(a);stroke(a,75,32);require(0<food(0)['mixed']<1,'No partial kneading');home.capture(a,out,'01-partial-dough-phone')
        record('four independent pizza trays and a real partial kneading stroke')
        for i,v in enumerate(clients):
            button(v,'Cooking action');stage(i,'roll');button(v,'Cooking action');stage(i,'vegetables' if i==2 else 'sauce')
            if i==2:
                for ingredient in ['capsicum','mushroom','tomato']:button(v,'Add '+ingredient)
                stage(i,'chop');home.capture(v,out,'02-whole-vegetables');stroke(v,145,0);require(food(i)['step']==1,'Chop stroke did not preserve first cut');home.capture(v,out,'03-partial-chop');button(v,'Cooking action');stage(i,'sauce')
            button(v,'Add sauce');stage(i,'spread')
        stroke(a,55,45);require(0<food(0)['icingMask']<511,'No partial sauce coverage');saved=deepcopy(food(0));button(a,'Back');button(a,'Cook');require(food(0)==saved,'Closing lost sauce');home.capture(a,out,'04-partial-sauce-phone')
        for i,v in enumerate(clients):
            button(v,'Cooking action');stage(i,'toppings');names=[c['name'] for c in v.input('inspect')['controls']];require('Add icing' not in names and 'Add patty' not in names,'Wrong recipe palette')
            for ingredient in [['cheese'],['cheese','pepperoni'],[],['cheese','ham','pineapple']][i]:button(v,'Add '+ingredient)
            home.capture(v,out,'05-pizza-'+str(i+1)+'-toppings');button(v,'Cooking action');stage(i,'bake')
        record('roll, whole-to-chopped vegetables, touched sauce coverage and per-recipe toppings retain partial work')
        for v in clients:button(v,'Cooking action')
        time.sleep(1);home.capture(a,out,'06-pizza-baking-phone');button(clients[3],'Back');cmd(clients[3],7,value='park')
        for i in range(4):stage(i,'cut')
        cmd(clients[3],7,value='home');cmd(clients[3],0,x=-2300,y=200);time.sleep(2);button(clients[3],'Cook')
        for i,v in enumerate(clients):
            button(v,'Cooking action');stage(i,'serve');ingredients=deepcopy(food(i)['ingredients']);button(v,'Plate '+str(i*2+1));button(v,'Plate '+str(i*2+2));require(food(i)['portions']==12,'Remaining portions incorrect')
            for j in range(2):
                d=next(t for t in world()['toys'] if t['id']=='plate-'+str(i*2+j))['kitchen']['dish'];require(d['portions']==1<<j and d['ingredients']==ingredients,'Served topping identity changed')
            button(v,'Put food away')
        record('four safe authority bakes, independent departure, eight distinct portions and four saved remainders')
        button(a,'Recipe PIZ-05');stage(0,'knead')
        for next_stage in ['roll','sauce']:button(a,'Cooking action');stage(0,next_stage)
        button(a,'Add sauce');stage(0,'spread');button(a,'Cooking action');stage(0,'toppings');button(a,'Add cheese')
        for ingredient,x,y in [('tomato',-75,48),('tomato',75,48),('capsicum',0,-70)]:
            before=len(food(0)['ingredients']);a.input('touch-begin',role='ui:Add '+ingredient,finger=49);a.input('touch-move',role='ui:Cake activity',x=x,y=y,finger=49);a.input('touch-end',role='ui:Cake activity',x=x,y=y,finger=49);wait(lambda:len(food(0)['ingredients'])==before+1,'placed topping');home.ready(a)
        positions=deepcopy(food(0)['ingredients']);tomatoes=[v for v in positions if v['ingredient']=='tomato'];require(tomatoes[0]['x']<0<tomatoes[1]['x'],'Touch positions not preserved');home.capture(a,out,'07-silly-face-phone')
        button(a,'Cooking action');stage(0,'bake');button(a,'Cooking action');stage(0,'cut');require(food(0)['ingredients']==positions,'Baking rearranged toppings');button(a,'Cooking action');stage(0,'serve');a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,out,'08-silly-face-served-tablet');button(a,'Put food away')
        expected=deepcopy(world()['homeCreations']);record('fifth recipe uses actual topping drags and retains the child arrangement through baking and storage')
        for v in clients:require('Exception' not in (v.out/'player.log').read_text(errors='replace'),'Native exception')
        run.close();run=Run(args.build,resume=runid,extended_test_lifetime=True);server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for v in clients:home.ready(v)
        require(world()['homeCreations']==expected,'Archived pizzas changed after restart')
        for i,v in enumerate(clients):
            cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'My saved food');home.capture(v,out,'09-saved-pizza-'+str(i+1));button(v,'Use saved creation')
        restored=[t['kitchen']['dish'] for t in world()['toys'] if t['id'].startswith('cookware-')];require(all(d['recipeVersion']==3 for d in restored) and len({d['id'] for d in restored})==4,'Pizza restore identity error')
        record('cold restart and four clients retrieve retained staged pizzas without duplicating servings')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
