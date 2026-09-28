# /// script
# dependencies = ["cryptography"]
# ///
"""Disposable headless authority and four native release clients, real UGUI input."""
import argparse, importlib.util, time, json
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'hide-and-seek';out.mkdir();checks=[];trace=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def info(v):return v.input('inspect')
    def button(v,name):v.input('touchButton',text=name);time.sleep(.15);home.ready(v)
    def cmd(v,action,**kw):
        result=home.command(v,action,**kw);require(result['accepted'],'Command rejected: '+str(result));home.ready(v);return result
    def state():
        s=server.state()['view']['hideAndSeek']
        signature=(s['phase'],s['target'],tuple((h['mode'],h['slot'],h['cycle']) for h in s['hiders']))
        if not trace or trace[-1]['signature']!=signature:trace.append(dict(signature=signature,observed=time.monotonic(),state=s))
        return s
    def hider(v):return next(h for h in state()['hiders'] if h['actor']==v.profile)
    def position(v,slot):cmd(v,0,x=[-4900,-4050,-3870,-3560,-3430,-3150,-7040,-470,2910,4310][slot],y=50);time.sleep(.85)
    def join(v):button(v,'Hide & seek');button(v,'Play hide and seek');wait(lambda:hider(v)['mode']==1,'joined hider')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        for v in clients:home.ready(v);cmd(v,0,x=-3800,y=50)
        a.input('resize',x=1280,y=591);time.sleep(1)
        button(a,'Hide & seek');home.capture(a,out,'invitation-phone');button(a,'Play hide and seek')
        start=state()['count'];require(8<start<=10,'Not a ten-second count');home.capture(a,out,'countdown-phone');time.sleep(1.3);require(0<state()['count']<start-.8,'Headless countdown stalled')
        for v in (b,c):join(v)
        d.input('resize',x=1024,y=768);join(d);home.capture(d,out,'countdown-tablet')
        require(len(server.state()['view']['players'])==4 and len(state()['hiders'])==4,'NPC consumed a player slot')
        record('pictured start card and headless countdown; four independent participants and a separate parent')
        slots=[9,6,7,8]
        for v,slot in zip(clients,slots):
            position(v,slot);button(v,'Hide '+['Curtain','Sofa left','Sofa right','Wardrobe left','Wardrobe right','Tent','Folding screen','Dining table','Blanket bench','Garden bush'][slot]);wait(lambda:hider(v)['mode']==2,'hidden at '+str(slot))
            home.capture(v,out,'hidden-'+str(slot))
        all_hidden=state();require(len({h['slot'] for h in all_hidden['hiders']})==4,'Duplicate hiding slots')
        # Scene visibility, not merely authoritative slot flags.
        for v in clients:
            s=info(v);hidden_others={h['actor'] for h in s['hideAndSeek']['hiders'] if h['mode']==2 and h['actor']!=v.profile}
            require(all(not p['visible'] for p in s['players'] if p['id'] in hidden_others),'A hidden sibling is visible')
        record('four covers accept physical taps and hide sibling characters while showing local status')
        # One sibling leaves while the other three keep their own roles.
        prior={h['actor']:h['cycle'] for h in state()['hiders']};button(b,'All done');require(hider(b)['mode']==0,'Leave failed')
        require(all(hider(v)['cycle']==prior[v.profile] and hider(v)['mode'] in (2,3) for v in (a,c,d)),'Leaving restarted siblings')
        wait(lambda:all(hider(v)['mode']==3 for v in (a,c,d)),'three friendly finds',110)
        home.capture(a,out,'found-phone');record('Bandit searches across the first level and finds; one child leaving preserves siblings')
        button(a,'Come out') # stable control name; label becomes Hide again after a find
        wait(lambda:hider(a)['mode']==1,'hide again');require(state()['round']==2,'Chilli did not take second turn');home.capture(a,out,'chilli-count')
        join(b);position(a,2);cmd(a,2,item='bucket-1');button(a,'Hide Sofa right');wait(lambda:hider(a)['mode']==2,'hide holding bucket')
        position(b,2);rejected=home.command(b,20,target='2',value='hide');require(not rejected['accepted'] and rejected['outcome']=='hide-space-busy','Occupied cover stolen')
        require(next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']==a.profile,'Hiding dropped possession')
        cmd(a,1,value='orange-pup');require(hider(a)['mode']==2,'Avatar change released cover')
        button(a,'Come out');require(hider(a)['mode']==1 and next(t for t in server.state()['view']['toys'] if t['id']=='bucket-1')['holder']==a.profile,'Come out lost possession')
        record('same-slot rejection, held object retention, avatar change and immediate Come out')
        position(a,0);button(a,'Hide Curtain');wait(lambda:hider(a)['mode']==2,'re-entry')
        a.input('application-pause');wait(lambda:hider(a)['mode']==0,'background releases own participation')
        require(hider(b)['mode']==1,'Backgrounding interrupted sibling');a.input('application-resume');home.ready(a)
        cmd(b,7,value='park');wait(lambda:hider(b)['mode']==0,'world travel withdraws');record('background and world travel release only their own player')
        # Native 4:3 composition and cold authoritative restore.
        position(c,4);button(c,'Come out');wait(lambda:hider(c)['mode']==1,'new round for restore');button(c,'Hide Wardrobe right');wait(lambda:hider(c)['mode']==2,'restored-slot setup')
        c.input('resize',x=1024,y=768);time.sleep(.6);home.capture(c,out,'hidden-tablet')
        time.sleep(5.5);saved_inventory=sorted(t["id"] for t in server.state()["view"]["toys"]);server.close()
        for v in clients:v.close()
        server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a)
        require(all(h['mode']==0 and h['slot']==-1 for h in state()['hiders']),'Stale roles resumed after restart')
        require(sorted(t['id'] for t in server.state()['view']['toys'])==saved_inventory,'Restart altered bounded inventory')
        record('tablet composition and native cold restore suspend stale roles without removing inventory')
        # Every authored button is used in the release UI, with both parent atlases.
        names=['Curtain','Sofa left','Sofa right','Wardrobe left','Wardrobe right','Tent','Folding screen','Dining table','Blanket bench','Garden bush']
        for slot,name in enumerate(names):
            position(a,slot);join(a);button(a,'Hide '+name);wait(lambda:hider(a)['mode']==2,'cover tour '+name)
            sample=info(a);require(sample['hideAndSeek']['hiders'][0]['slot']==slot,'Wrong pictured cover')
            if slot>=6:home.capture(a,out,'tour-'+str(slot))
            button(a,'Come out');require(hider(a)['slot']==-1,'No safe exit from '+name);button(a,'All done')
        record('all ten first-level picture buttons enter and exit real covers, including four new places')
        # Follow the seeker as a nonparticipant to record the actual stationary look.
        position(a,5);join(a);position(a,9);button(a,'Hide Garden bush');wait(lambda:hider(a)['mode']==2,'long-route setup')
        wait(lambda:state()['phase']==6 and state()['target']>=0,'mid-walk look pause',70)
        look=state();time.sleep(.15);after=state();require(after['phase']==6 and look['x']==after['x'],'Parent did not stop to look')
        record('headless seeker visibly pauses during long walks without advancing its floor position')

        passed=True
    finally:
        if not passed:
            for v in locals().get("clients",[]):
                try:home.capture(v,out,"failure-"+v.profile)
                except Exception:pass
        run.close();write(out/'phase-trace.json',trace);write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
