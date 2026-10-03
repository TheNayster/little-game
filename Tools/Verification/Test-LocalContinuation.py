"""Exercise native outage continuation in a disposable enrolled world, never the family server."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
from pathlib import Path
import time
import json
import hashlib
import psutil
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import read, wait, require, write
from parent_server import checkpoint_bytes

def payload(path):
    raw=checkpoint_bytes(path)[0];header,digest,body=raw.split(b'\n',2)
    require(header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode()==digest,'Invalid save envelope')
    return json.loads(body)

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    f=RecoveryFixture(args.build);checks=[];success=False;observations={}
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def inspect(c):return c.input('inspect')
    def toy(state,name):return next(t for t in state['toys'] if t['id']==name)
    def replica(c):return f.path/'client-recovery'/c.profile/'world.save'
    def drag(c,role,x,y):c.input('press',role=role);c.input('release',x=x,y=y)
    def shared(c):return wait(lambda:(v if (v:=inspect(c))['shared'] else None),'family presentation',55)
    def local(c):return wait(lambda:(v if not (v:=inspect(c))['shared'] and v.get('adventure') else None),'local adventure',55)
    def save(c,predicate):return wait(lambda:(v if (v:=payload(Path(inspect(c)['savePath']))) and predicate(v) else None),'local durable save',15)
    try:
        # A new offline install must not fabricate a shared checkpoint.
        first=f.launch(1)
        wait(lambda:inspect(first)['ready'],'new offline solo',25);time.sleep(12)
        state=inspect(first);require(not state['shared'] and not state['adventure'],'New install invented recovery state')
        solo=Path(state['savePath']);drag(first,'bucket-1',240,180)
        save(first,lambda v:abs(toy(v,'bucket-1')['x']-240)<.1)
        original_solo=payload(solo)
        passed('offline first launch remains playable without inventing a shared checkpoint')
        f.controller.start();shared(first);original_solo=payload(solo)
        write(f.path/'original-solo-before-outage.json',original_solo)
        clients=[first]+[f.join(i) for i in range(2,5)]
        clients[1].input('button',text='Creek')
        wait(lambda:(v:=inspect(clients[1]))['zone']=='creek' and not v['pending'],'second actor at creek')
        clients[1].input('press',role='sponge-creek')
        wait(lambda:toy(read(f.controller.processes()[0]['output']/'view.json')['view'],'sponge-creek')['holder']==clients[1].profile,'remote shared hold')
        for c in clients:
            wait(lambda:replica(c).exists() and toy(payload(replica(c))['snapshot'],'sponge-creek')['holder']==clients[1].profile,'replicated held toy',50)
        native=f.controller.processes()[0];psutil.Process(native['pid']).terminate();lost=time.monotonic()
        wait(lambda:not f.controller.processes(),'test server exit')
        # Release physical input so the pending gesture can settle at the loss boundary.
        clients[1].input('release',role='sponge-creek')
        states=[local(c) for c in clients];observations['allFourContinuationSecondsAfterKill']=round(time.monotonic()-lost,3)
        paths=[Path(s['savePath']) for s in states];records=[payload(p) for p in paths]
        require(len({r['id'] for r in records})==4,'Clients reused a branch identity')
        for c,r in zip(clients,records):
            require(r['actor']==c.profile and r['snapshot']['worldId']==r['id'],'Actor or branch identity mismatch')
            require(len(r['snapshot']['toys'])==10 and len(r['snapshot']['players'])==4,'Incomplete local world')
            require(all(not t['holder'] for t in r['snapshot']['toys']),'Stale remote hold survived')
            require(r['origin']['world']==f.run_id,'Lost common base')
        require(payload(solo)==original_solo,'Outage replaced the old solo draft')
        passed('four native clients automatically continue after server loss with unique saved branches, both areas and released item holds')
        # Exercise a non-first roster actor in both areas through real UI input.
        second=clients[1];drag(second,'sponge-creek',820,170)
        save(second,lambda r:abs(toy(r['snapshot'],'sponge-creek')['x']-820)<.1)
        second.input('button',text='Garden');wait(lambda:inspect(second)['zone']=='garden','local travel to garden')
        second.input('button',text='Creek');wait(lambda:inspect(second)['zone']=='creek','local travel back')
        first=clients[0];drag(first,'bucket-1',730,220)
        saved=save(first,lambda r:abs(toy(r['snapshot'],'bucket-1')['x']-730)<.1)
        branch_id=saved['id'];base=saved['origin'];first.input('press',role='bucket-1')
        save(first,lambda r:toy(r['snapshot'],'bucket-1')['holder']==first.profile)
        first.process.kill();first.process.wait(timeout=10)
        clients[0]=first=f.launch(1);resumed=local(first)
        require(resumed['adventure']==branch_id and abs(toy(payload(Path(resumed['savePath']))['snapshot'],'bucket-1')['x']-730)<.1,'Cold launch lost adventure')
        require(not toy(payload(Path(resumed['savePath']))['snapshot'],'bucket-1')['holder'],'Cold launch retained crashed pointer hold')
        first.input('capture');wait(lambda:(first.out/'garden.png').exists(),'continuation screenshot')
        write(f.path/'original-solo-after-cold-resume.json',payload(solo))
        require(payload(solo)==original_solo,'Cold resume touched old solo')
        passed('local interactions and travel use the correct profile; cold restart resumes the changed adventure while the older solo save remains intact')
        # Reunion waits for a real pointer gesture and saves it before binding.
        first.input('press',role='bucket-1');f.controller.start()
        wait(lambda:inspect(first)['connected'],'client rediscovers restarted authority',55)
        require(not inspect(first)['shared'],'Reunion interrupted a held gesture')
        shared(second);first.input('release',x=650,y=210);shared(first)
        archived=payload(paths[0]);require(archived['origin']==base,'Reunion modified common base')
        require(abs(toy(archived['snapshot'],'bucket-1')['x']-650)<.1 and not toy(archived['snapshot'],'bucket-1')['holder'],'Reunion discarded local drop')
        live=read(f.controller.processes()[0]['output']/'view.json')['view']
        require(toy(live,'bucket-1')['x']!=650,'Local branch silently overwrote authority')
        require(payload(solo)==original_solo,'Reunion replaced solo')
        passed('automatic reunion waits for gesture completion, saves local changes separately and leaves the authoritative family world intact')
        # Saved work remains accessible through the game, not just a hidden file.
        first.input('button',text='Menu');first.input('button',text='Saved adventures')
        first.input('capture');time.sleep(.5)
        (f.path/'saved-adventures-menu.png').write_bytes((first.out/'garden.png').read_bytes())
        first.input('button',text='Adventure 1')
        reopened=local(first);require(reopened['adventure']==branch_id,'Saved adventure menu opened wrong branch')
        require(abs(toy(payload(Path(reopened['savePath']))['snapshot'],'bucket-1')['x']-650)<.1,'Reopened adventure lost drop')
        first.input('button',text='Menu');first.input('button',text='Saved adventures');first.input('button',text='My solo play')
        old=wait(lambda:(s if not(s:=inspect(first))['shared'] and not s['adventure'] else None),'original solo selection')
        require(Path(old['savePath'])==solo and payload(solo)==original_solo,'Original solo is no longer accessible')
        require(inspect(second)['shared'] and inspect(second)['connected'],'Selecting saved adventures interrupted sibling')
        passed('in-game saved-adventure and solo selection restore their distinct progress without disrupting a connected sibling')
        first.input('button',text='Menu');first.input('button',text='Find my family');shared(first)
        for c in clients:shared(c)
        native=f.controller.processes()[0];current_epoch=read(native['output']/'view.json')['epoch']
        wait(lambda:payload(replica(first))['epoch']==current_epoch,'replica from recovered authority',50)
        retained=checkpoint_bytes(paths[0])[0];psutil.Process(native['pid']).terminate()
        again=local(first)
        require(again['adventure']!=branch_id and checkpoint_bytes(paths[0])[0]==retained,'Second outage reused or replaced the prior adventure')
        require(payload(Path(again['savePath']))['origin']['epoch']==current_epoch,'Second outage used wrong authority base')
        passed('a second outage creates a new branch from the recovered authority and preserves the earlier adventure byte for byte')
        success=True
    finally:
        f.cleanup()
        result=dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,observations=observations,
                    scope='Four native Windows clients, isolated DTLS authority crash/restart, input and storage. No family server, mobile qualification, host election or divergent-world merging.')
        write(f.path/'local-continuation-result.json',result);print('Evidence:',f.path/'local-continuation-result.json',flush=True)

if __name__=='__main__':main()
