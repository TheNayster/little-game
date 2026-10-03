# /// script
# dependencies = ["cryptography"]
# ///
"""One focused four-client beach flock check in a disposable family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time, shutil
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'seagulls';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
    def capture(v,name):
        # Bird touch targets may extend beyond the camera clip while offscreen;
        # inspect the visible scene and its bounded cache, not unrelated chrome.
        file=v.out/'garden.png';old=file.stat().st_mtime_ns if file.exists() else 0
        s=v.input('capture');wait(lambda:file.exists() and file.stat().st_mtime_ns!=old,'new screenshot')
        shutil.copyfile(file,out/(name+'.png'));write(out/(name+'.json'),s)
        require(s['visibleSeagulls']==6 and s['sceneryReady'],'Missing beach birds/scenery')
        require(len(s['residentScenery'])+s['pendingScenery']<=3,'Scenery cache grew')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def flock():return server.state()['view']['seagulls']
        home.travel(a,'beach')
        for v in (b,c,d):cmd(v,7,value='beach')
        for i,v in enumerate(clients):cmd(v,0,x=550+i*15,y=235)
        a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);time.sleep(.5)
        require(all(v.input('inspect')['visibleSeagulls']==6 for v in clients),'Four clients cannot see same flock')
        capture(a,'01-resting-phone');record('beach picture travel and six resting birds visible to four clients')
        for i,v in enumerate(clients):cmd(v,0,x=830+i*16,y=235)
        wait(lambda:flock()['phase']>0,'approach starts shared flight')
        require(flock()['flights']==1,'Four approaches created separate flights')
        route=(flock()['from'],flock()['to'])
        wait(lambda:flock()['phase']==3,'flying phase',12)
        samples=[v.input('inspect')['seagulls'] for v in clients]
        require(all(g['flights']==1 and (g['from'],g['to'])==route for g in samples),'Clients disagree on flock route')
        capture(d,'02-shared-flight-tablet');record('one shared approach/takeoff/flight across four clients')
        cmd(a,7,value='creek');b.close()
        wait(lambda:flock()['phase']==0,'remaining siblings see landing',15)
        require(flock()['from']==route[1] and flock()['flights']==1,'Departure reset flock')
        wait(lambda:c.input('inspect')['seagulls']['phase']==0,'landing reaches client')
        cmd(c,0,x=[900,1580,2220][route[1]]-230,y=235)
        cmd(d,0,x=[900,1580,2220][route[1]]-90,y=235)
        time.sleep(4)
        require(flock()['phase']==0 and flock()['flights']==1,'Nearby player repeatedly scares newly landed birds')
        capture(c,'03-landed-phone');require(c.input('inspect')['visibleGullTracks']==16,'Missing footprint trail')
        record('independent world travel/disconnect, landing, footprints and calm with nearby sibling')
        c.input('touchButton',text='Say hello to seagull 1')
        wait(lambda:flock()['flights']==2,'real touch starts second encounter')
        require(flock()['phase']>0,'Tap did not start encounter')
        record('nearby real UGUI bird tap starts a new shared encounter')
        require(a.input('inspect')['visibleSeagulls']==0,'Beach birds remain visible in creek')
        record('travel hides beach birds independently')
        passed=True
    finally:
        if not passed:
            for v in locals().get('clients',[]):
                if v.process.poll() is None:
                    try:v.input('capture');shutil.copyfile(v.out/'garden.png',out/('failure-'+v.profile+'.png'))
                    except Exception:pass
        run.close();write(out/'result.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
