"""Open two to four Windows gardens with one saved private local server."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import msvcrt
import sys
import time
import uuid
from shared_garden_runtime import ROOT, Run, read, write, wait


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--players',type=int,choices=(2,3,4))
    args=parser.parse_args()
    record = read(ROOT/'LocalData/latest-shared-preview.json')
    if not record:
        raise RuntimeError('No verified shared garden build is selected.')
    lock = (ROOT/'LocalData/shared-preview.lock').open('a+b')
    if lock.tell() == 0:
        lock.write(b'1'); lock.flush()
    lock.seek(0)
    try:
        msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
    except OSError:
        # A live controller owns the lock. Ask it to reopen only missing clients;
        # never launch a second server for the same saved preview world.
        request=dict(nonce=uuid.uuid4().hex)
        if args.players is not None:request['players']=args.players
        write(ROOT/'LocalData/shared-preview-reopen.json',request)
        lock.close();return
    previous = read(ROOT/'LocalData/shared-preview-session.json')
    count=args.players or (previous or {}).get('players',2)
    if count not in (2,3,4):raise ValueError('Saved preview player count must be 2, 3 or 4.')
    run = Run(record['buildNumber'], interactive=True, resume=previous['runId'] if previous else None)
    try:
        server=run.start('server')
        clients={profile:run.start('client',profile) for profile in ('player-'+str(i) for i in range(1,count+1))}
        write(ROOT/'LocalData/shared-preview-session.json',dict(runId=run.run_id,build=run.build,players=count))
        seen=read(ROOT/'LocalData/shared-preview-reopen.json')
        while any(c.process.poll() is None for c in clients.values()):
            request=read(ROOT/'LocalData/shared-preview-reopen.json')
            if request and request!=seen:
                seen=request
                desired=request.get('players',count)
                if desired not in (2,3,4):raise ValueError('Requested player count must be 2, 3 or 4.')
                # Expanding a live preview never closes a sibling's window or
                # restarts the authority. Ignore smaller requests while active.
                count=max(count,desired)
                for i in range(1,count+1):
                    profile='player-'+str(i)
                    if profile not in clients:clients[profile]=run.start('client',profile)
                for profile,client in list(clients.items()):
                    status=client.status()
                    if client.process.poll() is not None or (status and status['status']=='stopped'):
                        # Unity reports normal shutdown before its process exits.
                        # Keep this reopen request through that short interval.
                        if client.process.poll() is None:client.process.wait(timeout=12)
                        wait(lambda:profile not in server.state()['connected'],'previous client departure')
                        clients[profile]=run.start('client',profile)
                write(ROOT/'LocalData/shared-preview-session.json',dict(runId=run.run_id,build=run.build,players=count))
            time.sleep(.5)
    finally:
        run.close(); lock.close()


if __name__ == '__main__':
    try:
        main()
    except Exception as error:
        (ROOT/'LocalData/shared-preview-error.txt').write_text(str(error),encoding='utf-8')
        sys.exit(1)
