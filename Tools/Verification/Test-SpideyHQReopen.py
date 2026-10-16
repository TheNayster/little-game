# /// script
# dependencies = ["cryptography"]
# ///
"""Check the saved-HQ startup repair without repeating accepted activity loops."""
import argparse,hashlib,importlib.util,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,wait,require,write,read
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);p.add_argument('resume');a=p.parse_args()
    run=Run(a.build,resume=a.resume,graphics_api='d3d11');out=run.path/f'spidey-hq-reopen-{a.build}';out.mkdir();passed=False;checks=[]
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in read(run.folder/'source-manifest.json')['files'] if f['path'].startswith('Unity/')),'Unity source differs from candidate')
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots]
        for c in clients:home.ready(c)
        wait(lambda:all(c.input('inspect')['visibleSpideyPlayers']==4 for c in clients),'four saved HQ views')
        h=server.state()['view']['spidey'];require(h['puzzles']==1 and h['bricks'][0]['variant']==25,'Saved common creation/progress lost')
        require(all(c.input('inspect')['spidey']['bricks'][0]['id']==h['bricks'][0]['id'] for c in clients),'Late client catch-up lost object identity')
        checks.append('Four fresh clients load the saved HQ, preserve original brick identity and common puzzle completion')
        c=clients[0];c.input('resize',x=1280,y=591);home.ready(c);home.capture(c,out,'phone-shaped-hq');c.input('resize',x=1024,y=768);home.ready(c);home.capture(c,out,'tablet-shaped-hq')
        last=clients.pop();last.close();wait(lambda:len(server.state()['connected'])==3,'independent disconnect');clients.append(run.start('client',last.profile));home.ready(clients[-1]);wait(lambda:clients[0].input('inspect')['visibleSpideyPlayers']==4,'late reconnect')
        require(server.state()['view']['spidey']['bricks'][0]['id']==h['bricks'][0]['id'],'Reconnect changed shared creation');checks.append('Independent disconnection and fresh rejoining preserve siblings and saved creations');passed=True
    finally:
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,runtimeErrors=errors,physicalDevices=False,installedServerTouched=False));require(not errors,'Native runtime exception; see result')


if __name__=='__main__':main()
