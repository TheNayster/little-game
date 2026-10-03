# /// script
# dependencies = ["cryptography"]
# ///
"""Four disposable release clients: actual touch cooking and illustrated stages."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build);folder=run.path/'cake-flow';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),'authority')['view']
        def food(i=0):return next(t for t in world()['toys'] if t['id']=='cookware-'+str(i))['kitchen']['dish']
        def button(v,name):
            wait(lambda:any(x['name']==name and x['enabled'] for x in v.input('inspect')['controls']),'enabled '+name)
            v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
        def stage(value,i=0):
            wait(lambda:food(i)['stage']==value,value,18)
            wait(lambda:next(t for t in clients[i].state()['view']['toys'] if t['id']=='cookware-'+str(i))['kitchen']['dish']['stage']==value,'visible '+value)
            time.sleep(.35)
        def record(name):checks.append(name);print('PASS '+name,flush=True)
        def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
        for v in clients:home.ready(v);cmd(v,0,x=-2110,y=200);time.sleep(.65);button(v,'Cook');button(v,'Cakes')
        for i,v in enumerate(clients):button(v,'Recipe CAK-02');stage('ingredients',i)
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'01-cake-ingredients-phone')
        controls=a.input('inspect')['controls'];names=[x['name'] for x in controls]
        require('Add patty' not in names and 'Add cheese' not in names and 'Add icing' not in names,'Unrelated or early ingredients visible')
        require(len({food(i)['id'] for i in range(4)})==4,'Shared tray collision')
        record('four real recipe pickers create distinct cakes and offer only batter ingredients')
        for v in clients:
            for ingredient in ['egg','milk','chocolate']:button(v,'Add '+ingredient)
        stage('mix');a.input('touch-begin',role='ui:Cake activity',finger=41)
        a.input('touch-move',role='ui:Cake activity',x=65,y=32,finger=41)
        a.input('touch-end',role='ui:Cake activity',x=65,y=32,finger=41);time.sleep(.4)
        require(0<food()['mixed']<1 and food()['stage']=='mix','Stroke did not preserve partial mixing')
        home.capture(a,folder,'02-partially-mixed-phone')
        button(a,'Cooking action');stage('pour');home.capture(a,folder,'03-pour-tins-phone')
        button(b,'Cooking action');stage('pour',1)
        button(a,'Cooking action');time.sleep(.65);button(a,'Back');time.sleep(.4)
        partial=food()['poured'];require(0<partial<1 and food()['stage']=='pour','Closing did not preserve partial transfer')
        button(a,'Cook');home.capture(a,folder,'04-partially-poured-phone')
        button(a,'Cooking action');stage('bake');require(food()['poured']==1,'Full batter not transferred')
        button(c,'Cooking action');stage('pour',2);button(c,'Cooking action');stage('bake',2);button(c,'Cooking action')
        button(c,'Back');cmd(c,7,value='park')
        button(a,'Cooking action');time.sleep(2);home.capture(a,folder,'05-cake-rising-phone');stage('filling');stage('filling',2)
        require(food(1)['stage']=='pour' and food(3)['stage']=='mix','Sibling cooking changed')
        require(not any(x['ingredient']=='icing' for x in food()['ingredients']),'Icing required before baking')
        record('actual stroke, assisted mix and interrupted pour persist; two bakes finish while another cook leaves')
        home.capture(a,folder,'06-baked-layer-phone');button(a,'Add icing');button(a,'Cooking action');stage('stack')
        home.capture(a,folder,'07-layer-assembly-phone');button(a,'Cooking action');stage('ice')
        button(a,'Cooking action');stage('decorate');require(food()['layers']==2 and food()['icingMask']==511,'Layer or icing missing')
        for name in ['chocolate','strawberry','sprinkles']:button(a,'Add '+name)
        home.capture(a,folder,'08-decorated-cake-phone');a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'09-decorated-cake-tablet')
        button(a,'Cooking action');stage('cut');button(a,'Cooking action');stage('serve');home.capture(a,folder,'10-sliced-cake-tablet')
        ingredients=food()['ingredients']
        for i in range(4):button(a,'Plate '+str(i+1))
        require(food()['portions']==0,'Whole cake left under portions')
        plates=[next(t for t in world()['toys'] if t['id']=='plate-'+str(i))['kitchen']['dish'] for i in range(4)]
        require([x['portions'] for x in plates]==[1,2,4,8] and all(x['ingredients']==ingredients and x['layers']==2 for x in plates),'Served creation changed')
        button(a,'Back');cmd(a,0,x=-470,y=200);time.sleep(.7);home.capture(a,folder,'11-four-real-cake-portions')
        record('filling, layer assembly, icing, decoration and slicing keep the same food across four unique served portions')
        for v in clients:require('Exception' not in (v.out/'player.log').read_text(errors='replace'),'Native runtime exception')
        passed=True
    finally:
        run.close();write(folder/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(folder/'results.json'),flush=True)
if __name__=='__main__':main()
