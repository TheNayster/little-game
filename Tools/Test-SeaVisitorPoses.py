"""Focused native phone/tablet presentation check; shared rules stay unchanged."""
import argparse,importlib.util,shutil,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'visitor-poses';out.mkdir();passed=False;seen=set();checks=[]
    print('EVIDENCE '+str(out),flush=True)
    def capture(v,name):
        file=v.out/'garden.png';old=file.stat().st_mtime_ns if file.exists() else 0
        s=v.input('capture');wait(lambda:file.exists() and file.stat().st_mtime_ns!=old,'new image')
        shutil.copyfile(file,out/(name+'.png'));write(out/(name+'.json'),s)
        require(s['visibleSeaVisitor'] and s['sceneryReady'],'Visitor/scenery not rendered')
        return s
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2')
        home.travel(a,'beach');require(home.command(b,7,value='beach')['accepted'],'second beach arrival')
        a.input('resize',x=1280,y=591);b.input('resize',x=1024,y=768)
        for v in (a,b):require(home.command(v,0,x=1050,y=300)['accepted'],'camera placement')
        def shore():return server.state()['view']['shore']
        number=0;deadline=time.monotonic()+240;types=('whale','dolphin','mermaid');poses=('emerging','airborne','reentry')
        while len(seen)<3 and time.monotonic()<deadline:
            g=wait(lambda:(s if (s:=shore())['visits']>number else None),'next real shared event',50);number=g['visits'];kind=g['visitor']
            if kind in seen:continue
            for pose in range(3):
                for v in (a,b):
                    s=wait(lambda:(s if (s:=v.input('inspect'))['visibleSeaVisitor'] and s['seaVisitorPose']==pose else None),types[kind]+' '+poses[pose],5)
                    require(s['shore']['visits']==number and s['shore']['visitor']==kind,'Wrong shared event')
                capture(b if pose==1 else a,types[kind]+'-'+poses[pose])
            seen.add(kind);checks.append(types[kind]+' uses three distinct stage drawings in a real shared event')
            print('PASS '+checks[-1],flush=True)
        require(len(seen)==3,'Not all visitor pose sets observed')
        passed=True
    finally:
        run.close();write(out/'result.json',dict(passed=passed,build=args.build,checks=checks,layouts=['phone 1280x591','tablet 1024x768'],liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
