# /// script
# dependencies = ["cryptography"]
# ///
"""One disposable authority: a broadcast hiding window and opt-in cover membership."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'hide-window';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def info(v):return v.input('inspect')
    def button(v,name):v.input('touchButton',text=name);time.sleep(.15);home.ready(v)
    def cmd(v,action,**kw):
        result=home.command(v,action,**kw);require(result['accepted'],'Command rejected: '+str(result));home.ready(v);return result
    def state():return server.state()['view']['hideAndSeek']
    def hider(v):return next(h for h in state()['hiders'] if h['actor']==v.profile)
    def position(v,slot):cmd(v,0,x=[-4900,-4050,-3870,-3560,-3430,-3150,-7040,-470,2910,4310][slot],y=50)
    def start(v):button(v,'Hide & seek');button(v,'Play hide and seek');wait(lambda:state()['phase']==1,'shared count')
    def hide(v,slot):
        button(v,'Hide '+['Curtain','Sofa left','Sofa right','Wardrobe left','Wardrobe right','Tent','Folding screen','Dining table','Blanket bench','Garden bush'][slot]);wait(lambda:hider(v)['mode']==2,'hidden player')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        for v in clients:home.ready(v);cmd(v,0,x=-3800,y=50)
        cmd(b,7,value='park');position(c,9);position(d,8)
        a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);time.sleep(3)
        start(a);round_id=state()['round'];samples=[info(v) for v in clients]
        require(all(s['hideAndSeek']['phase']==1 and s['hideAndSeek']['round']==round_id for s in samples),'Different shared rounds')
        require(all(any(t.isdigit() and 1<=int(t)<=15 for t in s['visibleText']) for s in samples),'Broadcast countdown missing, including other world')
        require(all(h['mode']==0 and h['preparation']==0 for h in state()['hiders']),'Starting forced participation')
        require(next(p for p in server.state()['view']['players'] if p['id']==b.profile)['zone']=='park','Broadcast moved nonparticipant')
        hide(c,9);hide(d,8);home.capture(a,out,'broadcast-phone');home.capture(b,out,'broadcast-other-world');home.capture(d,out,'hidden-tablet')
        wait(lambda:state()['phase']!=1,'countdown end',20)
        require(hider(a)['mode']==0 and hider(b)['mode']==0,'Nonhiders enrolled at deadline')
        require(not home.command(a,20,target='1',value='hide')['accepted'],'Late hide enrolled in search')
        require(hider(c)['mode']==2 and hider(d)['mode']==2,'Hidden participants missing')
        record('one start broadcasts fifteen seconds to all four; hiding opts in and nonhiders stay ignored')
        button(c,'Come out');require(hider(c)['mode']==0 and hider(d)['mode']==2 and state()['round']==round_id,'Coming out changed sibling round')
        wait(lambda:hider(d)['mode']==3,'remaining hider found',110)
        require(hider(a)['mode']==0 and hider(b)['mode']==0 and hider(c)['mode']==0,'Parent found a nonparticipant')
        record('coming out after zero withdraws only that player; parent finds only the remaining hider')
        # All four can choose to hide in the same next round; no readiness lobby.
        for v,slot in zip((a,c,d),(6,7,8)):position(v,slot)
        start(a);require(state()['round']==round_id+1,'Parent turn did not advance')
        button(b,'Join family hide and seek');button(b,'Play hide and seek');wait(lambda:hider(b)['mode']==1,'optional trip downstairs');position(b,9)
        for v,slot in zip(clients,(6,9,7,8)):hide(v,slot)
        require(all(h['mode']==2 for h in state()['hiders']) and len({h['slot'] for h in state()['hiders']})==4,'Not four shared hiders')
        require(all(h['preparation']==0 for h in state()['hiders']),'Personal countdown added')
        wait(lambda:state()['phase']!=1,'second countdown end',20)
        cmd(b,0,x=4100,y=50);button(a,'All done')
        require(hider(b)['mode']==0 and hider(a)['mode']==0 and all(hider(v)['mode'] in (2,3) for v in (c,d)),'Walking/leaving affected siblings')
        require(not home.command(b,20,target=str(state()['round']),value='join')['accepted'],'Late join restarted count')
        home.capture(d,out,'shared-search-tablet')
        wait(lambda:all(hider(v)['mode']==3 for v in (c,d)),'shared remaining hiders found',110)
        record('all four hide together with one alternating parent; walking and leaving preserve the shared search')
        # Empty rounds wait the full hiding window, then close without seeking.
        position(a,1);start(a);time.sleep(2);require(state()['phase']==1,'Empty hiding window closed early')
        wait(lambda:state()['phase']==0,'empty round end',20);require(all(h['mode']==0 for h in state()['hiders']),'Empty round retained roles')
        record('no hiders at zero ends gently without searching or personal timers')
        position(c,4);start(a);hide(c,4);time.sleep(2)
        inventory=sorted(t['id'] for t in server.state()['view']['toys']);server.close()
        for v in clients:v.close()
        server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(state()['phase']==0 and all(h['mode']==0 and h['slot']==-1 for h in state()['hiders']),'Cold restore retained stale leases')
        require(sorted(t['id'] for t in server.state()['view']['toys'])==inventory,'Cold restore lost inventory')
        record('cold restore clears temporary roles and preserves inventory')
        passed=True
    finally:
        if not passed:
            for v in locals().get('clients',[]):
                if v.process.poll() is not None:continue
                try:home.capture(v,out,'failure-'+v.profile)
                except Exception:pass
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
