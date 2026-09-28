# /// script
# dependencies = ["cryptography"]
# ///
"""Real-time five-minute cleanup with four isolated native release clients."""
import argparse, importlib.util, json, time
from copy import deepcopy
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def saved(run):return json.loads((run.path/'server-world/world.save').read_bytes().split(b'\n',2)[2])

def same_clocks(a,b):
    return len(a)==len(b) and all(x['key']==y['key'] and abs(x['seconds']-y['seconds'])<1e-9 for x,y in zip(a,b))

def finish_restart(build,run_id):
    run=Run(build,resume=run_id,extended_test_lifetime=True);folder=run.path/'home-tidying'
    prior=json.loads((folder/'results.json').read_text());require(prior['build']==build and len(prior['checks'])==4 and not prior['passed'],'Expected completed real-time checks')
    write(folder/'initial-timer-results.json',prior);checks=prior['checks'];passed=False
    before=saved(run);profile=run.slots[1]['profile'];expected=next(w for w in before['discovery'] if w['owner']==profile)['liquid']
    try:
        require(any(t['key']=='item/living-book-hello' for t in before['homeIdleTimers']),'No released book clock')
        server=run.start('server');time.sleep(2);require(same_clocks(saved(run)['homeIdleTimers'],before['homeIdleTimers']),'Empty authority aged clocks')
        client=run.start('client',profile);home.ready(client)
        require(next(w for w in server.state()['view']['discovery'] if w['owner']==profile)['liquid']==expected,'Restart lost mixture')
        checks.append('saved inactivity clocks survive authority restart and pause without connected players');print('PASS '+checks[-1],flush=True)
        run.close()
        for log in run.path.rglob('player.log'):
            text=log.read_text(errors='replace');require('Exception:' not in text and 'Send queue is full' not in text,'Runtime error '+str(log))
        checks.append('all candidate processes close cleanly and final logs contain no exceptions or queue overflow');print('PASS '+checks[-1],flush=True);passed=True
    finally:
        run.close();write(folder/'results.json',dict(passed=passed,build=build,previous=prior['previous'],checks=checks,realTimer=True,continuedFromSameStoppedWorld=True,clockRoundTripToleranceSeconds=1e-9,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=198);p.add_argument('--finish-run');args=p.parse_args()
    if args.finish_run:return finish_restart(args.build,args.finish_run)
    old=Run(args.previous);run=None;checks=[];passed=False;folder=old.path/'home-tidying';folder.mkdir();print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        old.start('server');legacy=old.start('client',old.slots[0]['profile']);home.ready(legacy)
        require(home.command(legacy,0,x=-6590,y=200)['accepted'],'old placement')
        require(home.command(legacy,19,item=legacy.profile,target='0@0',value='fill:0:5')['accepted'],'old coloring')
        old.close();before=saved(old)
        run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');after=saved(run);normal=deepcopy(after)
        require(after['schema']==22 and after['homeIdleTimers']==[],'Missing safe migration')
        normal.pop('homeIdleTimers');normal.pop('homeTidyCues',None);normal['schema']=before['schema'];normal['revision']-=1
        require(normal==before,'Migration altered old world content')
        record('actual 198 world upgrades with placements artwork rooms food and enrollment unchanged')
        clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def world():return server.state()['view']
        def work(v):return next(w for w in world()['discovery'] if w['owner']==v.profile)
        def parts(v):return work(v)['liquid'][0]['current']['parts']
        def button(v,text):
            wait(lambda:any(c['name']==text and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+text)
            v.input('touchButton',text=text);time.sleep(.25);home.ready(v)
            if text=='Science bench':wait(lambda:v.input('inspect')['discoveryOpen'],'science opened')
        idle_started={}
        for v in clients:
            require(home.command(v,0,x=-6590,y=200)['accepted'],'science placement');time.sleep(.6);home.ready(v)
            button(v,'Science bench');button(v,'Liquid colors');button(v,'Colors Red');idle_started[v.profile]=time.monotonic()
        a.input('resize',x=1280,y=591);home.ready(a)
        artwork=deepcopy([w['pages'] for w in world()['discovery']]);rooms=deepcopy(world()['bedrooms'])
        button(d,'Back to Home');require(home.command(d,2,item='living-book-hello')['accepted'],'hold book')
        button(c,'Back to Home');require(home.command(c,18,target='fridge',value='door')['accepted'],'open fridge');require(home.command(c,2,item='ingredient-cheese')['accepted'],'hold ingredient')
        require(home.command(c,3,item='ingredient-cheese',x=-6500,y=130)['accepted'],'loose ingredient')
        stock=deepcopy(next(t['kitchen'] for t in world()['toys'] if t['id']=='ingredient-cheese'))
        record('four native players prepare independent experiments with loose stock and a held shared book')
        started=time.monotonic();last_refresh=started;saw_cue=False;captured=False
        # This uses the real authority clock, not a test acceleration or seeded deadline.
        while time.monotonic()-started<335:
            now=time.monotonic()
            if now-last_refresh>=40:
                button(b,'Colors Again');button(b,'Colors Blue');last_refresh=time.monotonic()
                print('PROGRESS real-time idle check '+str(round(last_refresh-started))+' seconds',flush=True)
            info=a.input('inspect')
            if any('Tidying soon' in line for line in info['visibleText']):
                saw_cue=True
                if not captured:home.capture(a,folder,'science-tidying-cue-phone');captured=True
            if parts(a)==[0,0,0] and parts(c)==[0,0,0] and parts(d)==[0,0,0]:break
            if time.monotonic()-idle_started[a.profile]<299:require(parts(a)==[1,0,0],'Experiment reset early')
            time.sleep(.35)
        elapsed=time.monotonic()-started
        require(saw_cue and parts(a)==[0,0,0] and parts(c)==[0,0,0] and parts(d)==[0,0,0],'Idle trays or cue missing')
        require(sum(parts(b))>0 and b.input('inspect')['discoveryOpen'],'Active sibling interrupted')
        record('real five-minute inactivity plus five-second cue resets three trays while active sibling continues')
        home.capture(a,folder,'science-clean-tray-phone');home.capture(b,folder,'science-active-sibling')
        wait(lambda:next(t for t in world()['toys'] if t['id']=='ingredient-cheese')['container']=='kitchen/fridge/2','stock returned',15)
        cheese=next(t for t in world()['toys'] if t['id']=='ingredient-cheese');require(cheese['kitchen']==stock,'Ingredient quantities changed')
        require(next(t for t in world()['toys'] if t['id']=='living-book-hello')['holder']==d.profile,'Held book returned')
        wait(lambda:not world()['kitchen']['fridgeOpen'],'fridge closed',15)
        require([w['pages'] for w in world()['discovery']]==artwork and world()['bedrooms']==rooms,'Personal creations erased')
        record('unused kitchen stock returns and fridge closes while held book paintings and bedrooms remain')
        require(home.command(d,4,item='living-book-hello')['accepted'],'release book')
        expected=deepcopy(work(b)['liquid']);run.close();checkpoint=saved(run)
        require(any(t['key']=='item/living-book-hello' for t in checkpoint['homeIdleTimers']),'Released book clock not persisted')
        run=Run(args.build,resume=old.run_id,extended_test_lifetime=True);server=run.start('server');time.sleep(2)
        require(same_clocks(saved(run)['homeIdleTimers'],checkpoint['homeIdleTimers']),'Empty authority aged clocks')
        reopened=run.start('client',b.profile);home.ready(reopened);require(work(reopened)['liquid']==expected,'Restart lost active mixture')
        record('saved inactivity clocks survive authority restart and pause without connected players')
        run.close();old.close()
        for log in old.path.rglob('player.log'):
            text=log.read_text(errors='replace');require('Exception:' not in text and 'Send queue is full' not in text,'Runtime error '+str(log))
        record('all candidate processes close cleanly and final logs contain no exceptions or queue overflow')
        passed=True
    finally:
        if run:run.close()
        old.close();write(folder/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,realTimer=True,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(folder/'results.json'),flush=True)

if __name__=='__main__':main()
