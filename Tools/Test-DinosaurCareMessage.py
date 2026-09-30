# /// script
# dependencies = ["cryptography"]
# ///
"""Focused real picture tap: a dinosaur with care reserved shows a child-friendly prompt."""
import importlib.util, argparse, time
from pathlib import Path
from shared_garden_runtime import Run, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
run=Run(args.build,extended_test_lifetime=True);out=run.path/'dinosaur-care-message';out.mkdir();passed=False
try:
    server=run.start('server');v=run.start('client',run.slots[0]['profile']);home.ready(v)
    def cmd(action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
    cmd(7,value='dinosaur-world');cmd(0,x=900,y=100);cmd(23,value='take',target='tyrannosaurus');cmd(0,x=600,y=100);time.sleep(.65)
    v.input('touchButton',text='Ride tyrannosaurus');s=home.ready(v)
    require(s['feedback']=='This dinosaur is busy. Choose another one.','Raw care outcome exposed: '+s['feedback'])
    home.capture(v,out,'busy-dinosaur-friendly-message');passed=True;print('PASS child-friendly busy prompt from a real dinosaur picture tap',flush=True)
finally:
    run.close();write(out/'results.json',dict(passed=passed,build=args.build,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(out/'results.json'),flush=True)
