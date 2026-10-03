"""Native four-player recovery replication; only disposable enrolled Windows worlds."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import time
import uuid
import ctypes
from recovery_fixture import RecoveryFixture
from parent_server import checkpoint_bytes
from shared_garden_runtime import read, write, wait, require


def payload(path):
    raw = checkpoint_bytes(path)[0]
    header, digest, body = raw.split(b'\n', 2)
    require(header == b'LITTLEWEEPS-SOLO-1' and hashlib.sha256(body).hexdigest().encode() == digest, 'Invalid native checkpoint')
    return json.loads(body), body


def main():
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('build', type=int)
    args = parser.parse_args(); fixture = RecoveryFixture(args.build); checks = []; success = False; metrics = []
    def passed(name): checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)
    def replica(c): return fixture.path / 'client-recovery' / c.profile / 'world.save'
    def durable(c, minimum=0):
        return wait(lambda: (v if (v := read(c.out/'recovery-evidence.json')) and v['status']=='durable' and v['checkpoint']>minimum else None), 'durable client replica', 50)
    def command(c, action=0, **kw):
        state=read(out/'view.json')['view']; player=next(p for p in state['players'] if p['id']==c.profile); identity=uuid.uuid4().hex
        data=dict(requestId=identity,actor=c.profile,action=action,expectedRevision=state['revision'],zone=player['zone'],visit=player['visit'],item='',target='',value='',x=0,y=0)
        data.update(kw);request=dict(requestId=identity, protocol=3, command=data)
        c.serial+=1;write(c.out/'control.json',dict(serial=c.serial,kind='command',request=request))
        response=wait(lambda:read(c.out/('reply-'+identity+'.json')), 'setup action')
        require(response['accepted'], 'Native setup action rejected: '+response['outcome'])
    try:
        fixture.controller.start(); native=fixture.controller.processes()[0];out=native['output']
        clients=[fixture.join(i) for i in range(1,5)]
        for c in clients: durable(c)
        for i in range(130): command(clients[0],x=300+i,y=100)
        command(clients[0],action=2,item='bucket-1');command(clients[0],action=3,item='bucket-1',target='tap-1',x=150,y=340)
        command(clients[1],action=7,value='creek');command(clients[1],action=2,item='sponge-creek')
        expected=payload(fixture.path/'server-world/world.save')[0]
        for c in clients:
            wait(lambda: replica(c).exists() and payload(replica(c))[0]['snapshot']['revision']>=expected['revision'], 'full changed replica', 50)
            record, raw=payload(replica(c)); state=record['snapshot']
            require(record['world']==fixture.run_id and record['authority']==read(fixture.path/'family.json')['authorityId'], 'Recovery lineage mismatch')
            require(all(state[k]==expected[k] for k in ('worldId','revision','players','toys','receipts')), 'Recovery state lost a field')
            require(len(state['receipts'])==128 and len(state['idleTimers'])>0 and len(raw)>16384, 'Did not exercise complete multi-message checkpoint')
            require(next(t for t in state['toys'] if t['id']=='sponge-creek')['holder']==clients[1].profile, 'Recovery storage prematurely released a live hold')
            expected_hash=hashlib.sha256(raw).hexdigest()
            wait(lambda: any(p['profile']==c.profile and p['checkpoint']>=record['checkpoint'] for p in (read(out/'recovery-evidence.json') or {}).get('peers',[])), 'authority durable acknowledgement')
            evidence=durable(c);metrics.append(dict(bytes=len(raw),receipts=len(state['receipts']),timers=len(state['idleTimers']),writeMilliseconds=evidence['writeMilliseconds']))
        passed('four enrolled native clients durably receive multi-chunk snapshots containing both areas, four players, 128 receipts, idle clocks and the live held prop')

        # Lock one test replica without granting delete sharing. A failed write
        # must not be counted as durable, and the other players keep receiving.
        c=clients[3]; prior=payload(replica(c))[0]; before=checkpoint_bytes(replica(c))[0]
        kernel=ctypes.WinDLL('kernel32',use_last_error=True)
        kernel.CreateFileW.argtypes=[ctypes.c_wchar_p,ctypes.c_ulong,ctypes.c_ulong,ctypes.c_void_p,ctypes.c_ulong,ctypes.c_ulong,ctypes.c_void_p]
        kernel.CreateFileW.restype=ctypes.c_void_p;kernel.CloseHandle.argtypes=[ctypes.c_void_p]
        handle=kernel.CreateFileW(str(replica(c)),0x80000000,0,None,3,0x80,None)
        require(handle!=ctypes.c_void_p(-1).value,'Cannot lock isolated replica')
        try:
            command(clients[0],x=720,y=140)
            wait(lambda:(read(c.out/'recovery-evidence.json') or {}).get('status')=='checkpoint-rejected','write failure reported',30)
            require(all(v.input('inspect')['connected'] for v in clients), 'Replica disk error disrupted family play')
            peer=next(p for p in read(out/'recovery-evidence.json')['peers'] if p['profile']==c.profile)
            require(peer['checkpoint']==prior['checkpoint'], 'Failed write reported durable coverage')
        finally: kernel.CloseHandle(handle)
        require(checkpoint_bytes(replica(c))[0]==before,'Failed write changed committed replica')
        c.close();clients[3]=fixture.join(4);durable(clients[3])
        require(payload(replica(clients[3]))[0]['snapshot']['players'][0]['x']==720,'Restart did not recover replication')
        passed('one client disk denial preserves its old checkpoint and receives no false durable credit; siblings stay connected and client restart resumes replication')

        # Closing the authority leaves the committed replica intact. Automatic
        # branching is deliberately a later task, not faked by reading the view.
        copies={c.profile:checkpoint_bytes(replica(c))[0] for c in clients}
        import psutil
        psutil.Process(native['pid']).terminate()
        wait(lambda: not fixture.controller.processes(), 'isolated authority exit')
        time.sleep(4)
        require(all(checkpoint_bytes(replica(c))[0]==copies[c.profile] for c in clients), 'Loss rewrote client recovery data')
        for c in clients: c.close()
        fixture.controller.start();clients=[fixture.join(i) for i in range(1,5)]
        for c in clients: durable(c)
        current_epoch=read(fixture.controller.processes()[0]['output']/'view.json')['epoch']
        require(all(payload(replica(c))[0]['epoch']==current_epoch for c in clients),'New authority epoch not accepted after restart')
        passed('hard loss preserves committed replicas; reopening the same four profiles and a new authority epoch resumes complete replication')

        # Use the existing manual solo branch unchanged, then prove replication
        # does not replace it with the server recovery file.
        c=clients[0];c.input('button',text='Menu');c.input('button',text='Play by myself')
        local=wait(lambda:(v if not (v:=c.input('inspect'))['shared'] else None), 'solo presentation')
        solo=Path(local['savePath']);require(solo.exists() and solo!=replica(c),'Separate solo save missing')
        before_solo=checkpoint_bytes(solo)[0]
        c.input('button',text='Menu');c.input('button',text='Find my family')
        wait(lambda:c.input('inspect')['shared'],'shared presentation');durable(c)
        require(checkpoint_bytes(solo)[0]==before_solo,'Shared recovery overwrote solo draft')
        passed('manual solo draft remains a distinct unchanged save while shared checkpoint replication resumes')
        success=True
    finally:
        fixture.cleanup()
        result=dict(passed=success,build=args.build,utc=datetime.now(timezone.utc).isoformat(),checks=checks,clientObservations=metrics,
                    scope='Four native Windows clients, actual enrolled DTLS transport, client storage failure and isolated authority termination. No family world, phone, iPad or Mac. Not automatic outage continuation, host migration or branch reconciliation.')
        write(fixture.path/'client-recovery-result.json',result);print('Evidence:',fixture.path/'client-recovery-result.json',flush=True)


if __name__=='__main__':main()
