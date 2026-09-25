"""Verify last-visible outage continuity in disposable worlds, never the family server."""
import argparse
from datetime import datetime, timezone
from pathlib import Path
import time
import json
import hashlib
import psutil
from recovery_fixture import RecoveryFixture
from parent_server import ParentServer
from shared_garden_runtime import read, wait, require, write
from parent_server import checkpoint_bytes


def payload(path):
    raw=checkpoint_bytes(path)[0];header,digest,body=raw.split(b'\n',2)
    require(header==b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode()==digest,'Invalid save')
    return json.loads(body)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int)
    p.add_argument('--observe',action='store_true');p.add_argument('--scenario',choices=('current','no-checkpoint'))
    args=p.parse_args();failed=False
    for scenario in ([args.scenario] if args.scenario else ('current','no-checkpoint')):
        f=RecoveryFixture(args.build);error=None;observations={}
        if scenario=='no-checkpoint':f.controller=ParentServer(f.run_id,85)
        try:
            f.controller.start();c=f.join(1);sibling=f.join(2)
            def inspect():return c.input('inspect')
            original_path=Path(inspect()['savePath']);original=checkpoint_bytes(original_path)[0]
            c.input('button',text='Creek');wait(lambda:inspect()['zone']=='creek' and not inspect()['pending'],'creek visit')
            c.input('press',x=805,y=170);c.input('release',x=805,y=170)
            wait(lambda:abs(next(p for p in inspect()['players'] if p['id']==c.profile)['position']['x']-805)<2,'walking destination')
            c.input('press',role='bucket-creek');c.input('release',x=730,y=190)
            wait(lambda:not inspect()['pending'] and not inspect()['dragging'],'accepted bucket drop')
            if scenario=='current':
                wait(lambda:(f.path/'client-recovery'/c.profile/'world.save').exists(),'full recovery checkpoint',40)
            c.input('press',role='bucket-creek');wait(lambda:not inspect()['pending'],'accepted pickup')
            c.input('move',x=660,y=230)
            before=inspect();before_player=next(p for p in before['players'] if p['id']==c.profile)['position']
            native=f.controller.processes()[0];psutil.Process(native['pid']).terminate();start=time.monotonic()
            # Keep the pointer down across the real transport loss.
            after=wait(lambda:(s if not (s:=inspect())['shared'] else None),'automatic local presentation',12)
            observations['lossToLocalSeconds']=round(time.monotonic()-start,3)
            require(after['zone']=='creek','Loss rewound the current area to a previous solo/checkpoint area')
            require(bool(after['adventure']),'Loss restored old solo instead of a private visible-state adventure')
            record=payload(Path(after['savePath']));own=next(p for p in record['snapshot']['players'] if p['id']==c.profile)
            toy=next(t for t in record['snapshot']['toys'] if t['id']=='bucket-creek')
            require(record['version']==2 and not (record.get('basis') or {}).get('version') and record['origin']['epoch'],'Local copy impersonates a recovery checkpoint')
            require(abs(own['x']-before_player['x'])<2 and abs(own['y']-before_player['y'])<2,'Loss changed the visible character position')
            require(abs(toy['x']-660)<2 and abs(toy['y']-230)<2 and not toy['holder'],'Held toy did not remain at its visible position with its lease released')
            require(checkpoint_bytes(original_path)[0]==original,'Loss overwrote original solo')
            observations['visiblePoseRetained']=True;observations['heldToyRetainedAndReleased']=True
            if scenario=='no-checkpoint':require(not (f.path/'client-recovery'/c.profile/'world.save').exists(),'Older server unexpectedly provided a full checkpoint')
            c.input('release',x=660,y=230)
            branch=after['adventure'];origin=record['origin'];path=Path(after['savePath'])
            c.process.kill();c.process.wait(timeout=10);c=f.launch(1)
            wait(lambda:(s:=inspect())['ready'] and s['adventure']==branch,'cold reopen private copy',30)
            require(payload(path)['origin']==origin,'Cold reopening changed provenance')
            f.controller.start()
            wait(lambda:inspect()['shared'] and inspect()['connected'],'server-authoritative reunion',55)
            wait(lambda:sibling.input('inspect')['shared'],'sibling reunion',55)
            live=read(f.controller.processes()[0]['output']/'view.json')['view']
            require(next(t for t in live['toys'] if t['id']=='bucket-creek')['x']!=660,'Private visual drop uploaded to server')
            require(payload(path)['origin']==origin and checkpoint_bytes(original_path)[0]==original,'Reunion changed saved origin or original solo')
            observations['coldReopenAndAuthoritativeReunion']=True
        except Exception as exc:error=str(exc);failed=True
        finally:
            f.cleanup()
            result=dict(build=args.build,scenario=scenario,passed=error is None,error=error,observations=observations,
                        utc=datetime.now(timezone.utc).isoformat(),scope='Native Windows UGUI, real isolated authority loss, held pointer, old-server/no-replica, cold reopen and no-upload reunion. Physical mobile qualification remains separate.')
            write(f.path/'visible-continuation-result.json',result);print(json.dumps(result), 'Evidence:',f.path/'visible-continuation-result.json',flush=True)
    if failed and not args.observe:raise SystemExit(1)


if __name__=='__main__':main()
