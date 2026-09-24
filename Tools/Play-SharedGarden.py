"""Open two playable Windows gardens and a private local server, then clean up."""
import msvcrt
import sys
import time
import uuid
from shared_garden_runtime import ROOT, Run, read, write, wait


def main():
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
        write(ROOT/'LocalData/shared-preview-reopen.json',dict(nonce=uuid.uuid4().hex))
        lock.close();return
    previous = read(ROOT/'LocalData/shared-preview-session.json')
    run = Run(record['buildNumber'], interactive=True, resume=previous['runId'] if previous else None)
    try:
        server=run.start('server')
        clients={profile:run.start('client',profile) for profile in ('player-1','player-2')}
        write(ROOT/'LocalData/shared-preview-session.json',dict(runId=run.run_id,build=run.build))
        seen=read(ROOT/'LocalData/shared-preview-reopen.json')
        while any(c.process.poll() is None for c in clients.values()):
            request=read(ROOT/'LocalData/shared-preview-reopen.json')
            if request and request!=seen:
                seen=request
                for profile,client in list(clients.items()):
                    status=client.status()
                    if client.process.poll() is not None or (status and status['status']=='stopped'):
                        # Unity reports normal shutdown before its process exits.
                        # Keep this reopen request through that short interval.
                        if client.process.poll() is None:client.process.wait(timeout=12)
                        wait(lambda:profile not in server.state()['connected'],'previous client departure')
                        clients[profile]=run.start('client',profile)
            time.sleep(.5)
    finally:
        run.close(); lock.close()


if __name__ == '__main__':
    try:
        main()
    except Exception as error:
        (ROOT/'LocalData/shared-preview-error.txt').write_text(str(error),encoding='utf-8')
        sys.exit(1)
