# /// script
# dependencies = ["cryptography"]
# ///
"""Kitchen acceptance in four disposable native release players, including real UI input."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time, json
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
RECIPES={
 'PIZ-01':['shape','spread','heat','cut'], 'PIZ-02':['shape','spread','heat','cut'],
 'PIZ-03':['shape','chop','spread','heat','cut'], 'PIZ-04':['shape','spread','heat','cut'],
 'PIZ-05':['shape','spread','decorate','heat','cut'], 'CAK-01':['mix','shape-duck','heat','decorate','cut'],
 'CAK-02':['mix','heat','stack','ice','cut'], 'CAK-03':['pour','shape-heart','heat','ice','cut'],
 'CAK-04':['mix','layer','heat','decorate','cut'], 'CAK-05':['chop','mix','heat','ice','cut'],
 'MEAL-01':['heat','stack','plate'], 'MEAL-02':['pour','stir','heat','scoop'],
 'MEAL-03':['chop','pour','stir','heat','ladle'], 'MEAL-04':['mix','pour','heat','flip','stack'],
 'MEAL-05':['chop','stir','heat','scoop']}
TOPPINGS=[['sauce','cheese'],['sauce','cheese','pepperoni'],['capsicum','mushroom','tomato'],['ham','pineapple','cheese'],['tomato','cheese','capsicum'],['egg','milk','icing','popcorn'],['egg','chocolate','icing'],['milk','icing','strawberry'],['milk','sprinkles','icing'],['carrot','egg','icing'],['patty','cheese','lettuce'],['sauce','cheese'],['carrot','tomato','capsicum'],['egg','milk','banana'],['carrot','capsicum','mushroom']]

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--ui-only',action='store_true');args=p.parse_args()
    run=Run(args.build);folder=run.path/'kitchen';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'fresh authority')['view']
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def player(v):return next(p for p in world()['players'] if p['id']==v.profile)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
        def cook(v,op,item='',target=''):return cmd(v,18,item=item,target=target,value=op)
        def move(v,x,y=200):cmd(v,0,x=x,y=y);time.sleep(.5)
        def button(v,name):v.input('touchButton',text=name);time.sleep(.25);home.ready(v)
        def tap(v,id,finger=31):
            v.input('touch-begin',role=id,finger=finger);v.input('touch-end',role=id,finger=finger);time.sleep(.25);home.ready(v)
        def drop(v,id,group,slot):
            x,y={'counter':(-2040+slot*230,430),'oven':(-610+slot%2*95,340-slot//2*160),'sink':(-2270+slot*44,430),'dining':(-1020+slot*235,180)}[group]
            cmd(v,2,item=id);cmd(v,3,item=id,target=f'kitchen/{group}/{slot}',x=x,y=y)
        def rinse(v,id,slot):
            while toy(id)['kitchen']['dish']['portions']:cook(v,'taste',id)
            drop(v,id,'sink',slot);cook(v,'wash',id)
        for v in clients:home.ready(v);move(v,-1700)
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'kitchen-closed-phone')
        require(not home.command(a,2,item='ingredient-dough')['accepted'],'Closed fridge allowed pickup')
        move(a,-960);button(a,'Fridge handle');require(world()['kitchen']['fridgeOpen'],'Fridge handle')
        move(a,-1700)
        for i in range(4):button(a,'Cupboard handle '+str(i+1))
        move(a,-565);button(a,'Oven handle');move(a,-1800);button(a,'Kitchen tap')
        require(world()['kitchen']['ovenOpen'] and world()['kitchen']['waterOn'] and all(world()['kitchen']['cupboards']),'Appliance toggles')
        home.capture(a,folder,'kitchen-open-phone')
        record('real appliance handles open fridge, four cupboards and oven; tap runs; closed contents refuse pickup')
        tap(a,'cookware-0');home.capture(a,folder,'fifteen-recipes-phone');button(a,'Recipe PIZ-01')
        button(a,'Add sauce');button(a,'Add cheese');require(len(toy('cookware-0')['kitchen']['dish']['ingredients'])==3,'Real ingredient taps')
        button(a,'Mix');require(toy('cookware-0')['kitchen']['dish']['step']==1,'Real shape button')
        # Real worktop drag uses the same operation once, then the next tap must still work.
        a.input('touch-begin',role='ui:Food work surface',finger=35)
        a.input('touch-move',role='ui:Food work surface',x=60,y=25,finger=35)
        a.input('touch-end',role='ui:Food work surface',x=60,y=25,finger=35);home.ready(a)
        require(toy('cookware-0')['kitchen']['dish']['step']==2,'Worktop spread gesture')
        home.capture(a,folder,'pizza-worktop-phone');button(a,'Mix')
        wait(lambda:toy('cookware-0')['kitchen']['dish']['heated'],'safe bake',15)
        button(a,'Mix');button(a,'Serve');home.capture(a,folder,'pizza-serving-phone')
        for i in range(4):button(a,'Plate '+str(i+1))
        require(toy('cookware-0')['kitchen']['dish']['portions']==0 and sum(toy('plate-'+str(i))['kitchen']['dish']['portions'] for i in range(4))==15,'Four unique portions')
        button(a,'Close');record('real recipe, ingredient taps, spread gesture, oven, cut and Serve controls make four unique pizza portions')
        for i,v in enumerate(clients):move(v,-670);button(v,'Dining seat '+str(i+1));wait(lambda:player(v)['fixture']=='kitchen-seat-'+str(i),'real dining seat')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'four-diners-tablet')
        move(a,-1600);require(all(player(v)['fixture']=='kitchen-seat-'+str(i+1) for i,v in enumerate(clients[1:])),'One exit disturbed other diners')
        for i,v in enumerate(clients):rinse(v,'plate-'+str(i),i);drop(v,'plate-'+str(i),'dining',i)
        rinse(a,'cookware-0',0);drop(a,'cookware-0','counter',0)
        record('four dining places coexist; leaving releases only one seat; each original plate is independently tasted and washed')
        if args.ui_only:passed=True;return
        for i,v in enumerate(clients):
            cook(v,'start','cookware-'+str(i),'PIZ-01')
            for ingredient in ('sauce','cheese'):cook(v,'add','ingredient-'+ingredient,'cookware-'+str(i))
            cook(v,'shape','cookware-'+str(i));cook(v,'spread','cookware-'+str(i));drop(v,'cookware-'+str(i),'oven',i)
        cmd(a,7,value='park');wait(lambda:all(toy('cookware-'+str(i))['kitchen']['dish']['heated'] for i in range(4)),'four independent bakes',15)
        require(all(toy('cookware-'+str(i))['kitchen']['dish']['heat']==8 for i in range(4)),'Baking failed when a cook left')
        cmd(a,7,value='home')
        for i,v in enumerate(clients):cook(v,'cut','cookware-'+str(i));rinse(v,'cookware-'+str(i),i);drop(v,'cookware-'+str(i),'counter',i)
        record('four trays bake independently; one player travels away and every dish reaches a stable ready state')
        # Distinct complete recipe paths in the actual authority, in four-tray batches.
        recipe_ids=list(RECIPES)
        for start in range(0,15,4):
            batch=recipe_ids[start:start+4]
            for i,recipe in enumerate(batch):
                v=clients[i];id='cookware-'+str(i);cook(v,'start',id,recipe)
                for ingredient in TOPPINGS[start+i]:
                    if not toy('ingredient-'+ingredient)['kitchen']['amount']:cook(v,'restock','ingredient-'+ingredient)
                    cook(v,'add','ingredient-'+ingredient,id)
                for op in RECIPES[recipe][:RECIPES[recipe].index('heat')]:cook(v,op,id)
                drop(v,id,'oven',i)
            wait(lambda:all(toy('cookware-'+str(i))['kitchen']['dish']['heated'] for i in range(len(batch))),'recipe batch baked',15)
            for i,recipe in enumerate(batch):
                v=clients[i];id='cookware-'+str(i)
                for op in RECIPES[recipe][RECIPES[recipe].index('heat')+1:]:cook(v,op,id)
                cook(v,'serve',id,'plate-'+str(i));require(toy('plate-'+str(i))['kitchen']['dish']['recipe']==recipe,'Recipe identity changed')
                rinse(v,'plate-'+str(i),i);drop(v,'plate-'+str(i),'dining',i)
                rinse(v,id,i);drop(v,id,'counter',i)
        record('all fifteen native recipe sequences consume real ingredients, prepare, heat, portion, taste and wash using bounded stock')
        require(len(world()['toys'])==106,'Cooking spawned extra stock')
        for i,v in enumerate(clients):
            move(v,-2980,420);cmd(v,11);wait(lambda:player(v)['zone']=='home-upstairs' and player(v)['stairs']==0,'upstairs');home.ready(v)
            move(v,[710,1132,1601,2066][i],420);cmd(v,13,target='home-bedroom-'+str(i+1));home.ready(v)
            move(v,2310,220);cmd(v,15,target='create',value='1:1');cmd(v,7,value='park');cmd(v,7,value='home')
        require(len(world()['toys'])==150,'Four-secret kitchen stock')
        for i,v in enumerate(clients):
            id='cookware-'+str(i)
            for ingredient in ('dough','sauce','cheese'):
                if not toy('ingredient-'+ingredient)['kitchen']['amount']:cook(v,'restock','ingredient-'+ingredient)
            cook(v,'readybase',id,'PIZ-01')
            for j in range(21):
                if not toy('ingredient-cheese')['kitchen']['amount']:cook(v,'restock','ingredient-cheese')
                cook(v,'add','ingredient-cheese',id)
            cook(v,'cut',id)
            for plate in (i*2,i*2+1):cook(v,'serve',id,'plate-'+str(plate))
        revision=world()['revision']
        for v in clients:
            state=wait(lambda:(s if (s:=v.state()) and s['view']['revision']>=revision else None),'maximum food snapshot')['view']
            require(len(state['toys'])==150 and sum(len(t.get('kitchen',{}).get('dish',{}).get('ingredients',[])) for t in state['toys'])==288,'Native maximum food snapshot incomplete')
        write(folder/'wire-bound.json',dict(compactNativeViewBytes=len(json.dumps(world(),separators=(',',':')).encode()),reliableCap=131072,toys=150,foodContainers=12,ingredientRecords=288))
        record('all four clients receive maximum native food snapshot: four created secrets, 150 objects and twelve dishes with 24 additions each')
        passed=True
    finally:
        run.close();write(folder/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(folder/'results.json'),flush=True)

if __name__=='__main__':main()
