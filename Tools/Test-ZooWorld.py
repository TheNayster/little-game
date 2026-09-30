# /// script
# dependencies = ["cryptography"]
# ///
"""Historical first-slice (Windows 253) Zoo acceptance. Test-ZooSpecies.py verifies the expanded four-trail Zoo."""
import argparse, importlib.util, time
from pathlib import Path
import shared_garden_runtime
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--root',type=Path);args=parser.parse_args()
    if args.root:shared_garden_runtime.ROOT=args.root.resolve()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'zoo';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v);return r
    def zoo():return server.state()['view']['zoo']
    def portion(v):return next(f for f in zoo()['food'] if f['actor']==v.profile)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        for v in clients:home.ready(v)
        home.scenic.NAMES['zoo']='World Zoo';home.travel(a,'zoo');home.capture(a,out,'entrance-phone')
        a.input('touchButton',text='Visit the savanna');wait(lambda:a.input('inspect')['zone']=='zoo-savanna','walk through savanna gate',25);home.ready(a)
        for v in (b,c,d):
            cmd(v,7,value='zoo');cmd(v,0,x=1200,y=100);cmd(v,22,value='gate',target='zoo-savanna')
        cmd(a,0,x=1200,y=100);a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);time.sleep(2)
        require(a.input('inspect')['visibleZooAnimals']==2,'Missing animated animals');home.capture(a,out,'elephant-phone')
        record('Zoo sixth picture menu destination, entrance and touch-controlled savanna gate')
        before=zoo()['animals'][0]['fed']
        for v in clients:
            cmd(v,0,x=1200,y=100)
        for v in clients:v.input('touchButton',text='Take leaves for elephant')
        wait(lambda:all(portion(v)['offered'] for v in clients),'four automatic walks and food offers',25)
        require(len({f['slot'] for f in zoo()['food']})==4,'Food slots overlap')
        samples=[v.input('inspect')['zoo'] for v in clients]
        require(all(s['animals'][0]['owner']==samples[0]['animals'][0]['owner'] for s in samples),'Clients disagree on animal feeding owner')
        home.capture(d,out,'four-feeders-tablet');record('four clients see one animal queue and four distinct offered portions')
        cmd(b,7,value='creek');require(portion(b)['species']=='','Departed player retained food');require(all(portion(v)['species']=='elephant' for v in (a,c,d)),'Sibling offers canceled')
        wait(lambda:zoo()['animals'][0]['fed']==before+3 and all(f['species']=='' for f in zoo()['food']),'three remaining portions eaten once',100)
        record('independent departure cancels only that portion; remaining three are eaten once')
        cmd(a,0,x=3600,y=100);home.ready(a);time.sleep(1.2);a.input('touchButton',text='Take leaves for giraffe');wait(lambda:portion(a)['offered'],'giraffe offer',20)
        wait(lambda:zoo()['animals'][1]['phase']==6,'giraffe eating',35);home.capture(a,out,'giraffe-feed-phone')
        wait(lambda:portion(a)['species']=='','giraffe finishes',10)
        require(zoo()['animals'][1]['fed']==1,'Giraffe portion duplicated');cmd(a,0,x=2500,y=490);require(next(p for p in server.state()['view']['players'] if p['id']==a.profile)['y']==120,'Visitor escaped enclosure boundary')
        cmd(a,7,value='park');wait(lambda:a.input('inspect')['visibleZooAnimals']==0,'Zoo art unloaded');record('giraffe has its own feeding pose; visitor boundary and world departure work')
        passed=True
    finally:
        if not passed:
            for v in locals().get('clients',[]):
                if v.process.poll() is None:
                    try:home.capture(v,out,'failure-'+v.profile)
                    except Exception:pass
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
