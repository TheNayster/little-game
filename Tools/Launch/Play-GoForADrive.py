# /// script
# dependencies = ["cryptography"]
# ///
"""Open a checked Go for a Drive candidate in its own saved one-to-four-player family."""
import argparse
import hashlib
import msvcrt
import os
from pathlib import Path
import sys
import time
import uuid
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--players',type=int,choices=(1,2,3,4));p.add_argument('--verify',action='store_true');args=p.parse_args()
    record=read(ROOT/'LocalData/go-for-a-drive-preview.json');require(record,'No checked Go for a Drive preview is selected')
    art=(ROOT/record['artRoot']).resolve();require(art.is_relative_to(ROOT),'Preview content must remain in this project')
    receipt=read(ROOT/record['assetReceipt']);require(receipt,'Missing checked content receipt')
    for row in receipt:
        path=(art/row['path']).resolve();require(path.is_relative_to(art) and path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest()==row['sha256'],'Drive content changed: '+row['path'])
    if record.get('packaged'):os.environ.pop('LITTLE_WEEPS_DRIVE_ART',None)
    else:os.environ['LITTLE_WEEPS_DRIVE_ART']=str(art)
    lock=(ROOT/'LocalData/go-for-a-drive-preview.lock').open('a+b')
    if lock.tell()==0:lock.write(b'1');lock.flush()
    lock.seek(0)
    try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
    except OSError:
        require(not args.verify,'An existing Drive preview owns this launcher')
        write(ROOT/'LocalData/go-for-a-drive-preview-reopen.json',dict(nonce=uuid.uuid4().hex,players=args.players or 2));lock.close();return
    previous=None if args.verify else read(ROOT/'LocalData/go-for-a-drive-preview-session.json')
    count=args.players or (previous or {}).get('players',2)
    run=Run(record['buildNumber'],interactive=not args.verify,review_controls=True,extended_test_lifetime=True,resume=previous['runId'] if previous else None,graphics_api='d3d11')
    try:
        server=run.start('server');clients={}
        def open_player(index):
            profile='player-'+str(index)
            # Automated pointer isolation is only for hidden setup/checks.
            # Owner windows must start without FamilyGameVerification, which
            # disables the physical mouse to protect automated test gestures.
            run.interactive=False;run.review_controls=True
            setup=run.start('client',profile)
            wait(lambda:(v:=setup.input('inspect'))['ready'] and not v['pending'] and not v['worldLoading'],'Drive setup ready')
            if previous is None:
                setup.input('fixtureTravel',text='go-for-a-drive')
                wait(lambda:(v:=setup.input('inspect'))['ready'] and v['zone']=='go-for-a-drive' and not v['pending'] and not v['worldLoading'],'Drive entry')
            if args.verify:
                clients[profile]=setup;return
            setup.close();wait(lambda:profile not in server.state()['connected'],'setup profile disconnected')
            run.interactive=True;run.review_controls=False
            client=run.start('client',profile)
            wait(lambda:client.status()['status']=='connected' and any(p['id']==profile for p in client.state()['view']['players']),'native mouse player connected')
            clients[profile]=client
        for i in range(1,count+1):open_player(i)
        if args.verify:
            time.sleep(3);require(len(server.state()['connected'])==count and all(c.input('inspect')['ready'] and c.input('inspect')['visibleDrivePlayers']==count for c in clients.values()),'Packaged Drive view did not remain ready')
            write(ROOT/'LocalData/go-for-a-drive-launcher-check.json',dict(passed=True,build=run.build,runId=run.run_id,players=count,installedServerTouched=False));return
        write(ROOT/'LocalData/go-for-a-drive-preview-session.json',dict(runId=run.run_id,build=run.build,players=count,nativeControls=True))
        seen=read(ROOT/'LocalData/go-for-a-drive-preview-reopen.json')
        while any(c.process.poll() is None for c in clients.values()):
            request=read(ROOT/'LocalData/go-for-a-drive-preview-reopen.json')
            if request and request!=seen:
                seen=request;count=max(count,request['players'])
                for i in range(1,count+1):
                    profile='player-'+str(i);client=clients.get(profile)
                    if client is None or client.process.poll() is not None:open_player(i)
                write(ROOT/'LocalData/go-for-a-drive-preview-session.json',dict(runId=run.run_id,build=run.build,players=count,nativeControls=True))
            time.sleep(.5)
    finally:run.close();lock.close()

if __name__=='__main__':
    try:main()
    except Exception as error:(ROOT/'LocalData/go-for-a-drive-preview-error.txt').write_text(str(error),encoding='utf-8');raise
