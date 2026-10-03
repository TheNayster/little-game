"""Current illustrated rendering of legacy coordinates/props in owned lab data.
Only accepts a completed Stage 5 synthetic run; preserves its original checkpoint.
"""
import argparse, copy, hashlib, importlib.util, json, sys
from pathlib import Path
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from shared_garden_runtime import Run,read,write,require,wait
from project_paths import ROOT
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);ap.add_argument('--of',dest='owned',required=True);a=ap.parse_args()
    run=Run(a.build,resume=a.owned,extended_test_lifetime=True)
    prior=read(run.path/'sandpit-stage-five/result.json');require(prior and prior['passed'] and prior['build']==a.build,'completed owned current-source fixture required')
    for f in read(run.folder/'source-manifest.json')['files']:
        if f['path'].startswith('Unity/'):require(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'],'stale '+f['path'])
    out=run.path/'sandpit-legacy-presentation';out.mkdir();p=run.path/'server-world/world.save';data=p.read_bytes();magic,digest,payload=data.split(b'\n',2)
    require(magic==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(payload).hexdigest().encode()==digest,'synthetic checkpoint checksum')
    require(not run.instances,'no owned writer running');(out/'before.save').write_bytes(data)
    fixture=json.loads(payload);g=fixture['sandpit'];fixture['schema']=49;g.update(format=0,pieceLimit=0,scoopCapacity=0,phase=2,round=7);g.pop('toy',None);g.pop('reset',None)
    g['moulds']=[dict(scoops=2,wet=True,built=True,decoration=1),dict(scoops=3,wet=True,built=True,decoration=2),dict(scoops=1,wet=False,built=False,decoration=0),dict(scoops=2,wet=True,built=False,decoration=0)]
    body=json.dumps(fixture,separators=(',',':')).encode();p.write_bytes(magic+b'\n'+hashlib.sha256(body).hexdigest().encode()+b'\n'+body)
    passed=False
    try:
        server=run.start('server');m=server.state()['view']['sandpit']['moulds']
        for i,v in enumerate(m):require(v['id']=='legacy-'+str(i) and v['x']==4210+i*160 and v['y']==440 and v['capacity']==2+i%2,'retained legacy coordinate/capacity')
        require(m[0]['attachments']==[dict(slot=0,kind='flag')] and m[1]['attachments']==[dict(slot=2,kind='shell')],'retained migrated props')
        for i,slot in enumerate(run.slots[:2]):
            client=run.start('client',slot['profile']);home.ready(client);client.input('resize',x=1280 if i==0 else 1024,y=591 if i==0 else 768)
            require(home.command(client,32,value='start')['accepted'],'legacy start')
            wait(lambda:not client.input('inspect')['pending'],'settled legacy')
            client.input('touchButton',text='Sand piece '+str(i+1));client.input('touchButton',text='Decorate')
            info=client.input('inspect');require(info['sandIllustrated'] and not any(info['sandpitTipEffects']) and info['sandActiveEffects']==0,'illustrated legacy with no replay')
            home.capture(client,out,'phone-legacy-props' if i==0 else 'tablet-legacy-props')
        passed=True
    finally:
        run.close();errors=[]
        for v in run.instances:errors.extend(line for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception' in line)
        p.write_bytes(data) # restore only this stopped synthetic checkpoint
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,clients=2,legacyPositionsAndProps=True,noReplay=True,originalSyntheticCheckpointRestored=True,errors=errors))
        require(not errors,'native errors')
    print('PASS current illustrated legacy positions, capacities, progress and migrated flags/shells; original lab checkpoint restored',flush=True)
if __name__=='__main__':main()
