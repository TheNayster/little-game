# /// script
# dependencies = ["cryptography"]
# ///
"""Disposable four-cook native qualification for version-four pan/pot meals."""
import argparse, base64, importlib.util, struct, time
from pathlib import Path
from copy import deepcopy
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def progress(d):
    raw=base64.b64decode(d['preparation']);require(raw[0]==1,'Progress version')
    return dict(zip(['mix','pour','water','stir','drain','coat','plate','combine','chop','flip','stack','heat','started'],struct.unpack('<8f5i',raw[1:])))

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--resume-visual');args=p.parse_args();run=Run(args.build,resume=args.resume_visual,extended_test_lifetime=True);runid=run.path.name;out=run.path/('meal-presentation-'+str(args.build) if args.resume_visual else 'meal-preparation');out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        def world():return wait(lambda:server.state(),'authority')['view']
        def food(i,client=False):
            state=clients[i].state()['view'] if client else world()
            return next(t for t in state['toys'] if t['id']=='cookware-'+str(i))['kitchen']['dish']
        def button(v,name):
            wait(lambda:any(x['name']==name and x['enabled'] for x in v.input('inspect')['controls']),'enabled '+name)
            v.input('touchButton',text=name);time.sleep(.3);home.ready(v)
        def stage(i,value):
            wait(lambda:food(i)['stage']==value,'server '+value,20)
            wait(lambda:food(i,True)['stage']==value,'visible '+value);time.sleep(.35)
        def cmd(v,action,**kw):
            r=home.command(v,action,**kw);require(r['accepted'],r.get('outcome'));home.ready(v);return r
        def record(name):checks.append(name);print('PASS '+name,flush=True)
        def stroke(v,x,y):
            v.input('touch-begin',role='ui:Cake activity',finger=41);v.input('touch-move',role='ui:Cake activity',x=x,y=y,finger=41);v.input('touch-end',role='ui:Cake activity',x=x,y=y,finger=41);time.sleep(.4);home.ready(v)
        def action(i,next_stage):button(clients[i],'Cooking action');stage(i,next_stage)
        def add(i,names):
            for name in names:button(clients[i],'Add '+name)
        if args.resume_visual:
            for v in clients:home.ready(v)
            before=[deepcopy(food(i)) for i in range(4)]
            for i,v in enumerate(clients):
                cmd(v,0,x=-2300,y=200);time.sleep(2);v.input('resize',x=1280 if i==0 else 1024,y=591 if i==0 else 768);home.ready(v);button(v,'Cook');home.capture(v,out,'01-retained-meal-'+str(i+1))
            require(before==[food(i) for i in range(4)],'Presentation changed restored meals')
            record('final candidate presents four retained meal portions without changing their saved preparation')
            soup=clients[2];button(soup,'Put food away');button(soup,'Meals');button(soup,'Make from the beginning');button(soup,'Recipe MEAL-03');stage(2,'ladle');action(2,'serve');home.capture(soup,out,'02-full-soup-tablet')
            record('soup sits within the illustrated bowl at full and partial serving amounts')
            burger=clients[0];button(burger,'Put food away');button(burger,'Meals');button(burger,'Make from the beginning');button(burger,'Recipe MEAL-01');stage(0,'assembly');add(0,['bun','lettuce']);action(0,'stack');burger.input('touchButton',text='Cooking action');home.capture(burger,out,'03-burger-layer-moving-phone');wait(lambda:progress(food(0))['stack']==1,'first burger layer');home.ready(burger)
            record('pictured stack assistance moves the layer and preserves one committed assembly step')
            passed=True;return
        for i,v in enumerate(clients):
            home.ready(v);cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'Meals');button(v,'Recipe MEAL-0'+str(i+1))
        a=clients[0];a.input('resize',x=1280,y=591);home.ready(a)
        stroke(a,90,15);require(0<progress(food(0))['pour']<1,'No partial patty transfer');home.capture(a,out,'01-patty-into-pan-phone');action(0,'cook-first')
        stroke(clients[1],90,20);require(0<progress(food(1))['water']<1,'No partial water');saved=deepcopy(food(1));button(clients[1],'Back');button(clients[1],'Cook');require(food(1)==saved,'Closing lost pot fill');home.capture(clients[1],out,'02-pasta-water')
        action(1,'stir');action(1,'boil')
        add(2,['carrot','tomato','capsicum']);stage(2,'chop');stroke(clients[2],145,0);require(progress(food(2))['chop']==1,'Chop first direction missing');home.capture(clients[2],out,'03-soup-chopping');action(2,'pour');action(2,'stir');action(2,'simmer')
        names=[c['name'] for c in clients[3].input('inspect')['controls']];require('Add banana' not in names and 'Add patty' not in names,'Fruit or patty in raw batter palette')
        add(3,['egg','milk']);stage(3,'mix');stroke(clients[3],100,20);require(0<progress(food(3))['mix']<1,'No partial batter');home.capture(clients[3],out,'04-pancake-mixing');action(3,'pour');action(3,'cook-first')
        record('four matching palettes and direct partial patty, water, chopping and batter gestures retain work')
        for v in clients:button(v,'Cooking action')
        time.sleep(.4);view=world();require(sum(t.get('container','').startswith('kitchen/hob/') for t in view['toys'])==4,'Four real hobs missing')
        home.capture(a,out,'05-first-side-countdown-phone');button(clients[3],'Back');cmd(clients[3],7,value='park')
        for i,value in enumerate(['flip','drain','ladle','flip']):stage(i,value)
        record('four authority hobs complete first passes independently while one cook leaves')
        stroke(a,100,0);stage(0,'cheese');add(0,['cheese']);action(0,'cook-second');require(progress(food(0))['started']==1,'Second side started before Cook')
        button(a,'Cooking action');time.sleep(.4);visible=food(0,True);require(visible['stage']=='cook-second' and 0<visible['heat']<4 and progress(visible)['started']==2,'Second-side countdown skipped or reused first-side heat')
        home.capture(a,out,'06-second-side-countdown-phone');stage(0,'assembly');add(0,['bun','lettuce','tomato']);action(0,'stack');home.capture(a,out,'07-burger-layers-phone')
        for n in range(4):button(a,'Cooking action')
        stage(0,'serve');home.capture(a,out,'08-burger-ready-phone')
        stroke(clients[1],110,20);require(0<progress(food(1))['drain']<1,'No direct drain');home.capture(clients[1],out,'09-draining-pasta');action(1,'sauce');add(1,['sauce']);stage(1,'coat');action(1,'plate');action(1,'cheese');add(1,['cheese']);action(1,'serve');home.capture(clients[1],out,'10-pasta-ready')
        action(2,'serve');home.capture(clients[2],out,'11-soup-ready')
        cmd(clients[3],7,value='home');cmd(clients[3],0,x=-2300,y=200);time.sleep(2);button(clients[3],'Cook');stroke(clients[3],100,0);stage(3,'cook-second');button(clients[3],'Cooking action');time.sleep(.4);require(0<food(3,True)['heat']<4,'Pancake second countdown skipped');home.capture(clients[3],out,'12-pancake-second-side');stage(3,'stack')
        for n in range(3):button(clients[3],'Cooking action')
        stage(3,'fruit');add(3,['banana','strawberry']);action(3,'serve');home.capture(clients[3],out,'13-pancakes-ready')
        record('separate live second-side timers, actual flips, draining, sauce coating, ladling and layered assembly')
        for i,v in enumerate(clients):
            ingredients=deepcopy(food(i)['ingredients']);button(v,'Plate '+str(i*2+1));button(v,'Plate '+str(i*2+2));require(food(i)['portions']==12,'Remaining portion mask')
            for j in range(2):
                d=next(t for t in world()['toys'] if t['id']=='plate-'+str(i*2+j))['kitchen']['dish'];require(d['portions']==1<<j and d['ingredients']==ingredients,'Served ingredients or portion identity changed')
            home.capture(v,out,'14-meal-remainder-'+str(i+1));button(v,'Put food away')
        record('eight unique servings and four preserved food remainders release the same reusable cookware')
        button(a,'Recipe MEAL-05');stage(0,'water');action(0,'boil');button(a,'Cooking action');time.sleep(1);home.capture(a,out,'15-rice-absorbing-water-phone');stage(0,'vegetables');add(0,['carrot','capsicum','mushroom']);stage(0,'chop');action(0,'stir');action(0,'pan-cook');button(a,'Cooking action');time.sleep(.4);require(0<food(0,True)['heat']<4,'Rice vegetable pass skipped');home.capture(a,out,'16-vegetable-pan-phone');stage(0,'combine');action(0,'scoop');action(0,'serve');a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,out,'17-rice-ready-tablet');button(a,'Put food away')
        expected=deepcopy(world()['homeCreations']);record('rice absorbs water before separate chopped vegetable pan cooking and final combination')
        for v in clients:require('Exception' not in (v.out/'player.log').read_text(errors='replace'),'Native exception')
        run.close();run=Run(args.build,resume=runid,extended_test_lifetime=True);server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for v in clients:home.ready(v)
        require(world()['homeCreations']==expected,'Archived meals changed on restart')
        for i,v in enumerate(clients):
            cmd(v,0,x=-2300,y=200);time.sleep(2);button(v,'Cook');button(v,'My saved food');home.capture(v,out,'18-saved-meal-'+str(i+1));button(v,'Use saved creation')
        restored=[t['kitchen']['dish'] for t in world()['toys'] if t['id'].startswith('cookware-')];require(all(d['recipeVersion']==4 for d in restored) and len({d['id'] for d in restored})==4,'Meal restore identity error')
        record('cold server restart and four clients restore the exact staged meals without duplicating portions')
        passed=True
    finally:
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
