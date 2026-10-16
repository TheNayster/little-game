# /// script
# dependencies = ["cryptography"]
# ///
"""Reopen the disposable saved Markets authority with four fresh native clients."""
import argparse,hashlib,importlib.util,sys,uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('--run-id');a=p.parse_args()
    record=read(ROOT/'LocalData/markets-last-checked-run.json')
    require(record and record['build']==a.build,'Matching completed Markets run required')
    require(not a.run_id or a.run_id==record['runId'],'Explicit run differs from saved evidence')
    expected=read(Path(record['expected']));require(expected and len(expected['bricks'])==4,'Saved shared creation evidence missing')
    run=Run(a.build,resume=record['runId'],extended_test_lifetime=True,graphics_api='d3d11')
    out=run.path/('markets-reopen-'+uuid.uuid4().hex[:8]);out.mkdir();checks=[];passed=False
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source differs from candidate')
    print('EVIDENCE '+str(out),flush=True)
    def same(view):
        require(view['bricks']==expected['bricks'],'Saved brick identity/ownership/placement/rotation lost')
        require(view['ponies']==expected['ponies'],'Saved pony care/appearance lost')
        fields=('id','kind','choice','station')
        require([{k:i[k] for k in fields} for i in view['items']]==[{k:i[k] for k in fields} for i in expected['items']],'Finished products lost or duplicated')
        require(all(not i['holder'] for i in view['items']),'Transient product holder survived process reopening')
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for c in clients:home.ready(c)
        wait(lambda:len(server.state()['connected'])==4,'four fresh saved-family clients')
        same(server.state()['view']['markets'])
        for c in clients:wait(lambda:c.input('inspect').get('markets',{}).get('bricks')==expected['bricks'],'fresh client saved creation catch-up')
        checks.append('Actual saved-authority process reopening and four fresh clients preserve exact shared creations, both ponies and finished products')
        last=clients[-1];last.close();wait(lambda:len(server.state()['connected'])==3,'independent disconnect')
        same(server.state()['view']['markets']);clients[-1]=run.start('client',last.profile);home.ready(clients[-1])
        wait(lambda:len(server.state()['connected'])==4,'fresh late reconnect');same(server.state()['view']['markets'])
        wait(lambda:clients[-1].input('inspect')['markets']['bricks']==expected['bricks'],'late identity catch-up')
        checks.append('Independent disconnect and fresh rejoin retain siblings and exact durable state')
        home.capture(clients[0],out,'saved-markets-reopened');passed=True
    finally:
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,runId=run.run_id,checks=checks,runtimeErrors=errors,physicalDevices=False,installedServerTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True);require(not errors,'Native exception; see result')


if __name__=='__main__':main()
