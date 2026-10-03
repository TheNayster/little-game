# /// script
# dependencies = ["cryptography"]
# ///
"""Closed-door first-use cooking through real UI in four disposable clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse,importlib.util,time,json
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
spec=importlib.util.spec_from_file_location('kitchen',Path(__file__).with_name('Test-Kitchen.py'))
kitchen=importlib.util.module_from_spec(spec);spec.loader.exec_module(kitchen)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--preview',action='store_true');p.add_argument('--catalog-group',type=int,choices=[0,1,2]);args=p.parse_args()
    run=Run(args.build);folder=run.path/'easy-kitchen';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
        def cook(v,op,item='',target=''):return cmd(v,18,item=item,target=target,value=op)
        def move(v,x,y=200):cmd(v,0,x=x,y=y);time.sleep(.5)
        def button(v,name):v.input('touchButton',text=name);time.sleep(.25);home.ready(v)
        def tap(v,role,finger=31):
            v.input('touch-begin',role=role,finger=finger);v.input('touch-end',role=role,finger=finger);time.sleep(.3);home.ready(v)
        def record(name):checks.append(name);print('PASS '+name,flush=True)
        def wash(v,id):
            while toy(id)['kitchen']['dish']['portions']:cook(v,'easy:taste',id)
            cook(v,'easy:wash',id)
        if args.catalog_group is None:
            for v in clients:home.ready(v);move(v,-2110)
            a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'kitchen-new-layout-phone')
            require(not world()['kitchen']['fridgeOpen'] and not any(world()['kitchen']['cupboards']),'Expected ordinary closed kitchen')
            # Every picker opens before any recipe is committed. Authority must choose
            # separate trays, even though all four saw the same initial availability.
            for v in clients:button(v,'Cook')
            home.capture(a,folder,'recipe-pictures-phone')
            for i,v in enumerate(clients):
                button(v,'Recipe PIZ-01');wait(lambda:toy('cookware-'+str(i))['kitchen']['dish'].get('id'),'UI recipe creates persistent food')
                require(toy('cookware-'+str(i))['kitchen']['cook']==v.profile,'Automatic tray assignment')
            record('four simultaneous recipe pickers create four independent trays without opening storage')
            for v in clients:button(v,'Add sauce');button(v,'Add cheese')
            require(all(len(toy('cookware-'+str(i))['kitchen']['dish']['ingredients'])==3 for i in range(4)),'Ingredient taps failed')
            home.capture(a,folder,'central-pizza-phone')
            a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'central-pizza-tablet');a.input('resize',x=1280,y=591);home.ready(a)
            if args.preview:passed=True;return
            # A broad worktop drag followed by a tap must count once each.
            a.input('touch-begin',role='ui:Food work surface',finger=35)
            a.input('touch-move',role='ui:Food work surface',x=65,y=30,finger=35)
            a.input('touch-end',role='ui:Food work surface',x=65,y=30,finger=35);home.ready(a)
            require(toy('cookware-0')['kitchen']['dish']['step']==1,'Shape gesture')
            button(a,'Cooking action');require(toy('cookware-0')['kitchen']['dish']['step']==2,'Tap after drag')
            for v in clients[1:]:button(v,'Cooking action');button(v,'Cooking action')
            before=toy('cookware-0')['kitchen']['dish']['ingredients']
            for v in clients:button(v,'Cooking action')
            button(d,'Back');cmd(d,7,value='park')
            wait(lambda:all(toy('cookware-'+str(i))['kitchen']['dish']['heated'] for i in range(4)),'four assisted bakes',15)
            require(not world()['kitchen']['ovenOpen'],'Assistance changed shared door state')
            for v in clients[:3]:button(v,'Cooking action')
            cmd(d,7,value='home');cook(d,'easy:cut','cookware-3')
            home.capture(a,folder,'baked-pizza-phone')
            for i in range(4):button(a,'Plate '+str(i+1))
            require(toy('cookware-0')['kitchen']['dish']['portions']==0,'Four portions not transferred')
            require(all(toy('plate-'+str(i))['kitchen']['dish']['ingredients']==before for i in range(4)),'Decorations changed in slicing')
            record('tap-only ingredients/bake/serve and optional gesture retain decorations; departing cook does not stop siblings')
            button(a,'Wash & reuse');button(a,'Back')
            for v in clients[1:3]:button(v,'Back')
            for i,v in enumerate(clients):
                move(v,-470);button(v,'Dining seat '+str(i+1))
            a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-diners-clear-appliances-tablet')
            move(a,-1500);require(all(next(p for p in world()['players'] if p['id']==v.profile)['fixture'] for v in clients[1:]),'One departure cleared another seat')
            for i,v in enumerate(clients):wash(v,'plate-'+str(i))
            for i,v in enumerate(clients[1:],1):wash(v,'cookware-'+str(i))
            for v in clients:move(v,-2110)
            # Direct empty-plate entry must have a real Make food route.
            cmd(a,2,item='plate-0');cmd(a,3,item='plate-0',x=-2110,y=180);tap(a,'plate-0');button(a,'Cooking action');button(a,'Recipe PIZ-02')
            require(any(t['kitchen']['dish'].get('recipe')=='PIZ-02' for t in world()['toys'] if t['kind']==13),'Empty plate Make food dead end')
            home.capture(a,folder,'plate-to-cooking');button(a,'Back')
            record('four visible dining places remain independent and an empty plate leads directly to working cooking')
            passed=True;return
        # Finish and reuse that dish, then exercise every recipe without changing doors.
        def finish(v,id):
            food=toy(id)['kitchen']['dish'];r=food['recipe'];idx=list(kitchen.RECIPES).index(r)
            for ingredient in kitchen.TOPPINGS[idx]:
                if ingredient not in [x['ingredient'] for x in toy(id)['kitchen']['dish']['ingredients']]:
                    if not toy('ingredient-'+ingredient)['kitchen']['amount']:cook(v,'easy:restock','ingredient-'+ingredient)
                    cook(v,'easy:add','ingredient-'+ingredient,id)
            for step in kitchen.RECIPES[r][food['step']:]:
                cook(v,'easy:'+('bake' if step=='heat' else step),id)
                if step=='heat':wait(lambda:toy(id)['kitchen']['dish']['heated'],'recipe bake',15)
        for idx,r in enumerate(list(kitchen.RECIPES)[args.catalog_group*5:args.catalog_group*5+5]):
            v=clients[idx%4];base='dough' if r.startswith('PIZ') else 'flour' if r.startswith('CAK') or r=='MEAL-04' else {'MEAL-01':'bun','MEAL-02':'pasta','MEAL-03':'broth','MEAL-05':'rice'}[r]
            if not toy('ingredient-'+base)['kitchen']['amount']:cook(v,'easy:restock','ingredient-'+base)
            cook(v,'easy:start',target=r);id=next(t['id'] for t in world()['toys'] if t['kind']==13 and t['kitchen']['dish'].get('recipe')==r)
            finish(v,id);cook(v,'easy:serve',id,'plate-4');require(toy('plate-4')['kitchen']['dish']['recipe']==r,'Wrong served recipe');wash(v,'plate-4');wash(v,id)
        record('five '+['pizza','cake','meal'][args.catalog_group]+' recipes prepare heat serve taste and wash without drawer setup or extra world objects')
        require(len(world()['toys'])==106,'Extra stock spawned')
        require(not world()['kitchen']['fridgeOpen'] and not world()['kitchen']['ovenOpen'] and not any(world()['kitchen']['cupboards']),'Shared door state changed')
        for v in clients:
            require('Exception' not in (v.out/'player.log').read_text(errors='replace'),'Native runtime exception')
        passed=True
    finally:
        run.close();write(folder/'results.json',dict(passed=passed,build=args.build,checks=checks,previewOnly=args.preview,catalogGroup=args.catalog_group,liveFamilyTouched=False));print('RESULT '+str(folder/'results.json'),flush=True)

if __name__=='__main__':main()
