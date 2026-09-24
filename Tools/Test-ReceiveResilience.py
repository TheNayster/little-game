"""Native receive-pool and hard-departure regression, isolated to a fresh loopback server."""
import argparse
from datetime import datetime,timezone
import socket,time,uuid
from shared_garden_runtime import Run,read,write,wait,require

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);parser.add_argument('--cycles',type=int,default=8)
    args=parser.parse_args();require(1<=args.cycles<=20,'Use 1..20 bounded crash cycles')
    run=Run(args.build);checks=[];server=None;clients=[]
    try:
        server=run.start('server');clients=[run.start('client','player-'+str(i)) for i in range(1,5)]
        initial=server.state()['view'];server_pid=server.process.pid;epoch=server.state()['epoch']
        def command(client,x):
            state=server.state();p=next(p for p in state['view']['players'] if p['id']==client.profile)
            rid=uuid.uuid4().hex;client.serial+=1
            cmd=dict(requestId=rid,actor=client.profile,expectedRevision=state['view']['revision'],zone=p['zone'],visit=p['visit'],action=0,x=x,y=100)
            write(client.out/'control.json',dict(serial=client.serial,kind='command',request=dict(requestId=rid,protocol=run.protocol,command=cmd)))
            result=wait(lambda:read(client.out/('reply-'+rid+'.json')),'fresh survivor command acknowledged',seconds=5)
            require(result['accepted'] and result['durable'],'Fresh command not committed')
            return result
        def healthy(count):
            state=server.state()
            require(server.process.pid==server_pid and state['epoch']==epoch,'Test restarted the authority')
            require(len(state['connected'])==count,'A surviving profile was lost')
            for c in clients:
                require(c.status()['status']=='connected','A surviving client disconnected')
        # All sends are to the explicitly created loopback test server only.
        # Empty/truncated datagrams complete receives but are discarded by UTP.
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as probe:
            for batch in range(16):
                for _ in range(64):
                    probe.sendto(b'',('127.0.0.1',run.port))
                    probe.sendto(b'x'*4096,('127.0.0.1',run.port))
                time.sleep(.025)
        require(server.state()['view']==initial,'Discarded packets mutated the world')
        for i,c in enumerate(clients):command(c,250+i*130)
        time.sleep(3);healthy(4)
        checks.append(dict(check='1024 empty and 1024 oversized datagrams do not exhaust receives or disconnect players',passed=True))
        print('PASS discarded datagrams leave all four clients responsive',flush=True)
        for cycle in range(args.cycles):
            victim=clients.pop(cycle%len(clients));profile=victim.profile
            victim.process.kill();victim.process.wait(timeout=8);start=time.monotonic()
            # Exercise a survivor throughout failure detection, not only before
            # its timeout. Check every other original connection on each step.
            commands=0
            while time.monotonic()-start<4:
                command(clients[0],300+commands%20);commands+=1
                for c in clients:require(c.status()['status']=='connected','Crash interrupted a survivor')
                time.sleep(.15)
            wait(lambda:profile not in server.state()['connected'],'departed profile released',seconds=4)
            healthy(3)
            return_started=time.monotonic();returning=run.start('client',profile);clients.append(returning)
            healthy(4);command(returning,450)
            checks.append(dict(check='hard departure and current-world rejoin '+str(cycle+1),passed=True,survivorCommands=commands,rejoinSeconds=round(time.monotonic()-return_started,3),serverPid=server_pid))
            print('PASS hard departure/rejoin '+str(cycle+1)+'; survivors never disconnected',flush=True)
        for c in clients:c.close()
        wait(lambda:not server.state()['connected'],'clients left');before=server.state()['view'];server.close()
        server=run.start('server');require(server.state()['view']==before,'Final complete world did not survive restart')
        checks.append(dict(check='complete world survives final graceful authority restart',passed=True))
        write(run.path/'receive-result.json',dict(passed=True,build=args.build,runId=run.run_id,utc=datetime.now(timezone.utc).isoformat(),checks=checks,physicalDevicesAccessed=False,serverRestartsDuringCrashCycles=0))
        print('PASS receive resilience. Evidence: '+str(run.path),flush=True)
    except Exception as error:
        write(run.path/'receive-result.json',dict(passed=False,build=args.build,runId=run.run_id,checks=checks,error=str(error),connectionEvidence=read(server.out/'connection-evidence.json') if server else None))
        print('FAIL '+str(error)+'. Evidence: '+str(run.path),flush=True);raise
    finally:run.close()

if __name__=='__main__':main()
