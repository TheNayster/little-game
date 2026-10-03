"""Measure real native canvas motion; optional receive-side motion delay/loss."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import ctypes
from ctypes import wintypes
import hashlib
from pathlib import Path
from datetime import datetime, timezone
import math
import time
import uuid
from shared_garden_runtime import Run, read, write, wait, require


def percentile(values, fraction):
    ordered=sorted(values);return ordered[min(len(ordered)-1,int((len(ordered)-1)*fraction))]


def analyze(samples):
    pairs=[(a,b) for a,b in zip(samples,samples[1:]) if 170<a['authority']['x']<740 and 170<b['authority']['x']<740]
    require(len(pairs)>40,'Insufficient moving canvas samples')
    dt=[b['time']-a['time'] for a,b in pairs]
    visual=[b['visual']['x']-a['visual']['x'] for a,b in pairs]
    raw=[b['authority']['x']-a['authority']['x'] for a,b in pairs]
    require(all(t>0 for t in dt),'Bad sample clock')
    result=dict(sampleCount=len(pairs),medianFrameMs=round(percentile(dt,.5)*1000,2),
                visualMovingFrameFraction=round(sum(d>.05 for d in visual)/len(visual),3),
                rawMovingFrameFraction=round(sum(d>.05 for d in raw)/len(raw),3),
                p95VisualSpeed=round(percentile([abs(d)/t for d,t in zip(visual,dt)],.95),2),
                p95RawSnapSpeed=round(percentile([abs(d)/t for d,t in zip(raw,dt)],.95),2),
                worstBackwardStep=round(min(visual),3),
                p95VisualLagBehindReceivedSeconds=round(percentile([(b['authority']['x']-b['visual']['x'])/210 for a,b in pairs],.95),3))
    require(result['visualMovingFrameFraction']>.85,'Visible movement still pauses on too many render frames: '+str(result))
    require(result['p95VisualSpeed']<420 and result['worstBackwardStep']>-.5,'Visible jitter/correction is too large: '+str(result))
    require(result['p95VisualLagBehindReceivedSeconds']<.4,'Visual lag behind received state exceeds prototype budget')
    return result


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);parser.add_argument('--impaired',action='store_true');parser.add_argument('--players',type=int,choices=(2,4),default=2);args=parser.parse_args()
    conditions=dict(testMotionDelayMs=80,testMotionJitterMs=25,testMotionDropEvery=10) if args.impaired else {}
    harnessHashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (Path(__file__),Path(__file__).with_name('shared_garden_runtime.py'))}
    run=Run(args.build,motion_conditions=conditions);checks=[]
    try:
        server=run.start('server');first=run.start('client','player-1');second=run.start('client','player-2')
        extras=[run.start('client','player-'+str(i)) for i in range(3,args.players+1)]
        if args.build>=86:
            # Fill the real receipt window before measuring. This makes the
            # new recovery stream span multiple bounded messages during motion.
            for i in range(130):
                state=server.state()['view'];p=next(p for p in state['players'] if p['id']==first.profile);rid=uuid.uuid4().hex
                command=dict(requestId=rid,actor=first.profile,expectedRevision=state['revision'],zone=p['zone'],visit=p['visit'],action=1,
                             item='',target='',value='blue-pup' if i%2==0 else 'orange-pup',x=0,y=0)
                first.serial+=1;write(first.out/'control.json',dict(serial=first.serial,kind='command',request=dict(requestId=rid,protocol=3,command=command)))
                reply=wait(lambda:read(first.out/('reply-'+rid+'.json')),'receipt stress setup')
                require(reply['accepted'],'Receipt stress command failed')
        def player(c):return next(p for p in server.state()['view']['players'] if p['id']==c.profile)
        def walk_to(x,y,check_others=False):
            first.input('press',x=x,y=y);first.input('release',x=x,y=y)
            proof=None
            if check_others:
                def concurrent_positions():
                    positions=server.state()['view']['players']
                    return positions if all(170<p['x']<740 for p in positions) else None
                proof=wait(concurrent_positions,'four characters walking concurrently',seconds=5)
            wait(lambda:math.hypot(player(first)['x']-x,player(first)['y']-y)<1,'arrival',seconds=12)
            return proof
        walk_to(50,70);time.sleep(.6)
        # In the four-player case, keep the other three clients walking diagonally
        # while the measured player crosses the floor; all use native input.
        movers=extras+[second] if extras else []
        for index,client in enumerate(movers):
            client.input('press',x=50,y=55+index*25);client.input('release',x=50,y=55+index*25)
        for index,client in enumerate(movers):
            wait(lambda:math.hypot(player(client)['x']-50,player(client)['y']-(55+index*25))<1,'extra client start position',seconds=12)
        if args.build>=86:
            # Evidence is written on durable completion, not every chunk, and
            # each peer then has a five-second offer cooldown. Let all idle
            # clients catch up and exhaust that cooldown before starting the
            # measured walk; otherwise a valid short walk can fit between
            # transfers and prove nothing about concurrent recovery traffic.
            stable_checkpoint=None;stable_since=None
            def recovery_ready():
                nonlocal stable_checkpoint,stable_since
                evidence=read(server.out/'recovery-evidence.json') or {}
                peers=evidence.get('peers',[])
                checkpoints={p['checkpoint'] for p in peers}
                if evidence.get('status')!='replicated' or len(peers)!=args.players or len(checkpoints)!=1 or min(checkpoints)<=0:
                    stable_checkpoint=None;stable_since=None;return False
                checkpoint=next(iter(checkpoints))
                if checkpoint!=stable_checkpoint:
                    stable_checkpoint=checkpoint;stable_since=time.monotonic()
                return evidence if time.monotonic()-stable_since>=5.2 else False
            ready=wait(recovery_ready,'idle recovery convergence and offer readiness',seconds=40)
            write(run.path/'recovery-trace-readiness.json',ready)
        metrics_before=read(server.out/'motion-stats.json')['checkpointWrites']
        recovery_before=read(server.out/'recovery-evidence.json') or {}
        first.input('traceStart',role=first.profile);second.input('traceStart',role=first.profile)
        for client in movers:
            client.input('press',x=920,y=450);client.input('release',x=920,y=450)
        start=time.monotonic();concurrent=walk_to(920,70,bool(movers));elapsed=time.monotonic()-start;time.sleep(.6)
        first.input('traceStop');second.input('traceStop')
        metrics_after=read(server.out/'motion-stats.json')['checkpointWrites'];writes=metrics_after-metrics_before
        recovery_after=read(server.out/'recovery-evidence.json') or {}
        recovery_chunks=sum(p['sent'] for p in recovery_after.get('peers',[]))-sum(p['sent'] for p in recovery_before.get('peers',[]))
        write(run.path/'recovery-trace-measurement.json',dict(before=recovery_before,after=recovery_after,checkpointWrites=writes,recoveryChunks=recovery_chunks))
        local=analyze(read(first.out/'motion-trace.json')['samples']);remote=analyze(read(second.out/'motion-trace.json')['samples'])
        require(remote['visualMovingFrameFraction']>remote['rawMovingFrameFraction']+.2,'Interpolation did not fill missing display frames')
        require(3.8<elapsed<5.6,'Walking speed depends on acknowledgements: '+str(elapsed))
        if args.build>=86:
            require(recovery_chunks>0,'No measured recovery traffic during walk')
        require(writes<=8,'Motion still causes per-step disk checkpoints: '+str(writes))
        checks.append(dict(check='continuous local and remote canvas motion',passed=True,local=local,remote=remote,walkSeconds=round(elapsed,3),checkpointWritesDuringWalk=writes,concurrentAuthoritativePositions=concurrent,
                           recoveryChunksDuringTrace=recovery_chunks,receiptStress=args.build>=86))
        print('PASS motion '+str(remote),flush=True)
        first.input('button',text='Tap to walk');first.input('touch-begin',role='stick',x=-45,finger=31)
        time.sleep(.45);first.input('touchButton',text='Menu');time.sleep(.65)
        stopped=(player(first)['x'],player(first)['y']);time.sleep(.6)
        require(stopped==(player(first)['x'],player(first)['y']),'Menu left runaway walking')
        first.input('touch-cancel',role='stick',finger=31);first.input('touchButton',text='Back to play')
        checks.append(dict(check='menu cancels continuous joystick input and it remains stopped',passed=True))
        print('PASS menu stops authoritative walking',flush=True)
        # Hold only a disposable observation file, denying Windows rename.
        # This used to stop the server while a test reader was inspecting it.
        kernel=ctypes.WinDLL('kernel32',use_last_error=True)
        kernel.CreateFileW.argtypes=[wintypes.LPCWSTR,wintypes.DWORD,wintypes.DWORD,wintypes.LPVOID,wintypes.DWORD,wintypes.DWORD,wintypes.HANDLE]
        kernel.CreateFileW.restype=wintypes.HANDLE
        kernel.CloseHandle.argtypes=[wintypes.HANDLE];kernel.CloseHandle.restype=wintypes.BOOL
        handle=kernel.CreateFileW(str(server.out/'view.json'),0x80000000,3,None,3,0,None)
        require(handle!=ctypes.c_void_p(-1).value,'Could not open isolated evidence lock')
        try:
            time.sleep(.35);require(server.status()['status']=='listening','An evidence reader stopped the game server')
        finally:kernel.CloseHandle(handle)
        wait(lambda:read(server.out/'motion-stats.json')['diagnosticWriteConflicts']>0,'observed diagnostic lock handling')
        checks.append(dict(check='locked diagnostic observation does not stop authoritative play',passed=True))
        # The exact stopped position, not only a data directory, must survive.
        first.close();second.close()
        for client in extras:client.close()
        wait(lambda:not server.state()['connected'],'clients left');before=server.state()['view'];server.close()
        server=run.start('server');first=run.start('client','player-1')
        require(server.state()['view']==before,'Stopped movement did not persist exactly')
        checks.append(dict(check='stopped positions survive server restart exactly',passed=True))
        write(run.path/'motion-result.json',dict(passed=True,build=args.build,runId=run.run_id,utc=datetime.now(timezone.utc).isoformat(),
              players=args.players,concurrentWalkers=1+len(movers),harnessHashes=harnessHashes,conditions=conditions,impairmentScope='Client receive-side motion stream only; not whole-network impairment.',checks=checks,
              comparison='Smoothed native canvas vs raw authoritative snap positions sampled on the same render frames; not a historical-build benchmark.',physicalDevicesAccessed=False))
        print('PASS smooth movement. Evidence: '+str(run.path),flush=True)
    except Exception as e:
        write(run.path/'motion-result.json',dict(passed=False,build=args.build,runId=run.run_id,players=args.players,conditions=conditions,checks=checks,error=str(e)))
        print('FAIL '+str(e)+' '+str(run.path),flush=True);raise
    finally:run.close()


if __name__=='__main__':main()
