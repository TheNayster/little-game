"""Focused four-client test in a disposable authority, using actual menu/prop touches."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--checkpoint-run');args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True,resume=args.checkpoint_run);out=run.path/'kingdom';out.mkdir(exist_ok=bool(args.checkpoint_run));checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def state():return server.state()['view']['kingdom']
    def cmd(v,action,**kw):
        result=home.command(v,action,**kw);require(result['accepted'],result.get('outcome','command rejected'));home.ready(v);return result
    def button(v,name):
        wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'visible '+name,15)
        v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
    def menu(v):button(v,'Games');button(v,'The Adventure');wait(lambda:next(m for m in state()['members'] if m['actor']==v.profile)['attending'],'member joined');home.ready(v)
    def prop(v,name,mask,key):
        phase=state()['phase'];wait(lambda:v.input('inspect')['kingdom']['phase']==phase,'client story phase',15)
        button(v,'Next adventure task');wait(lambda:state()[key]&mask,'shared '+name,14)
    try:
        server=run.start('server')
        if args.checkpoint_run:
            a=run.start('client',run.slots[0]['profile']);home.ready(a);saved=state();require(saved['phase']==6 and saved['rescued']==7,'Saved feast checkpoint lost');round_id=saved['round'];clock=saved['clock'];time.sleep(1);require(state()['clock']==clock,'Empty reopened story clock ran');menu(a);require(state()['round']==round_id and state()['rescued']==7,'Joining reset checkpoint');wait(lambda:state()['clock']-state()['started']>=6,'replay ready');button(a,'Play again');wait(lambda:state()['round']==round_id+1 and state()['supplies']==0,'shared replay');record('empty suspension, native saved checkpoint reopening and explicit shared replay');passed=True;return
        clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        for v in clients:home.ready(v);cmd(v,7,value='daycare')
        a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);home.ready(a);home.ready(d)
        button(a,'Games');info=a.input('inspect');require(any(t=='The Adventure' for t in info['visibleText']) and not any(t=='Fishing' for t in info['visibleText']),'Wrong Daycare game menu');home.capture(a,out,'daycare-games-phone');button(a,'The Adventure');home.ready(a)
        round_id=state()['round'];require(next(p for p in server.state()['view']['players'] if p['id']==b.profile)['zone']=='daycare','Start teleported sibling')
        menu(b);wait(lambda:state()['phase']==2,'shared supply phase');prop(a,'Gather fruit 1',1,'supplies')
        menu(c);menu(d);require(state()['round']==round_id and state()['supplies']==1,'Late joins reset story')
        samples=[v.input('inspect') for v in clients];require(all(s['kingdom']['round']==round_id and s['visibleKingdomNpcs']==9 for s in samples),'Shared round/cast missing')
        home.capture(d,out,'four-player-story-tablet');record('Daycare menu starts one shared story; four players join current progress with nine prepared NPCs')
        prop(b,'Gather fruit 2',2,'supplies');prop(c,'Gather fruit 3',4,'supplies');wait(lambda:state()['phase']==3,'bridge phase')
        prop(d,'Place bridge plank 1',1,'boards');button(a,'Return to daycare');require(state()['boards']==1 and sum(m['attending'] for m in state()['members'])==3,'Exit reset siblings')
        prop(b,'Place bridge plank 2',2,'boards');prop(c,'Place bridge plank 3',4,'boards');wait(lambda:state()['phase']==4,'queen phase')
        button(b,'Next adventure task');wait(lambda:state()['distractedUntil']>state()['clock'],'queen watches ball')
        wait(lambda:d.input('inspect')['kingdom']['distractedUntil']>d.input('inspect')['kingdom']['clock'],'client sees ball distraction',15);button(d,'Next adventure task');wait(lambda:state()['phase']==5,'rescue phase');home.capture(d,out,'magic-wand-rescue-tablet')
        prop(b,'Wake friend 1',1,'rescued');d.close();wait(lambda:not next(m for m in state()['members'] if m['actor']==d.profile)['attending'],'departed membership')
        menu(a);require(state()['rescued']==1 and state()['round']==round_id,'Rejoining restarted rescue')
        prop(c,'Wake friend 2',2,'rescued');prop(a,'Wake friend 3',4,'rescued');wait(lambda:state()['phase']==6,'shared feast');time.sleep(5);home.capture(c,out,'kingdom-saved-phone');record('four shared contributions, independent return/disconnect, join during rescue and common completion')
        for v in (a,b,c):button(v,'Return to daycare')
        clock=state()['clock'];time.sleep(1);require(state()['clock']==clock and state()['rescued']==7,'Empty story did not suspend')
        for v in (a,b,c):v.close()
        server.close()
        server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a);require(next(p for p in server.state()['view']['players'] if p['id']==a.profile)['zone']=='daycare','Saved player location changed');menu(a)
        require(state()['round']==round_id and state()['rescued']==7 and state()['phase']==6,'Saved checkpoint lost');wait(lambda:state()['clock']-state()['started']>=6,'replay ready');button(a,'Play again');require(state()['round']==round_id+1 and state()['supplies']==0,'Replay did not start one new story');record('empty suspension, native saved checkpoint reopening and explicit shared replay')
        passed=True
    finally:
        write(out/('checkpoint-result.json' if args.checkpoint_run else 'result.json'),dict(build=args.build,passed=passed,checks=checks,checkpointOnly=bool(args.checkpoint_run),scope='disposable release server and four native clients; no physical-device or live-server update'))
        run.close()
    print('PASS ALL '+str(len(checks))+' focused groups',flush=True)
if __name__=='__main__':main()
