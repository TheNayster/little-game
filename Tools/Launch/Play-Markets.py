# /// script
# dependencies = ["cryptography"]
# ///
"""Open the checked Markets candidate in its own saved one-to-four-player family."""
import argparse,msvcrt,sys,time,uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--players',type=int,choices=(1,2,3,4));parser.add_argument('--verify',action='store_true');args=parser.parse_args()
    selected=read(ROOT/'LocalData/markets-preview.json');require(selected,'No checked Markets candidate selected')
    lock=(ROOT/'LocalData/markets-preview.lock').open('a+b')
    if lock.tell()==0:lock.write(b'1');lock.flush()
    lock.seek(0)
    try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
    except OSError:
        require(not args.verify,'An existing Markets preview owns the launcher');write(ROOT/'LocalData/markets-preview-reopen.json',dict(nonce=uuid.uuid4().hex,players=args.players or 2));lock.close();return
    previous=None if args.verify else read(ROOT/'LocalData/markets-preview-session.json');count=args.players or (previous or {}).get('players',2)
    run=Run(selected['buildNumber'],interactive=not args.verify,review_controls=True,extended_test_lifetime=True,resume=previous['runId'] if previous else None,graphics_api='d3d11')
    try:
        server=run.start('server');clients={}
        def open_player(index):
            profile='player-'+str(index)
            # Verification isolates its synthetic pointer from the physical
            # mouse. Use it only for hidden setup, then reopen ordinary play.
            run.interactive=False;run.review_controls=True
            setup=run.start('client',profile)
            wait(lambda:(v:=setup.input('inspect'))['ready'] and not v['pending'] and not v['worldLoading'],'Markets setup ready')
            if previous is None:
                setup.input('fixtureTravel',text='markets')
                wait(lambda:(v:=setup.input('inspect'))['ready'] and v['zone']=='markets' and not v['pending'] and not v['worldLoading'],'Markets preview entry')
            if args.verify:
                clients[profile]=setup;return
            setup.close();wait(lambda:profile not in server.state()['connected'],'setup profile disconnected')
            run.interactive=True;run.review_controls=False
            client=run.start('client',profile)
            wait(lambda:client.status()['status']=='connected' and any(p['id']==profile for p in client.state()['view']['players']),'Markets mouse player connected')
            clients[profile]=client
        for index in range(1,count+1):open_player(index)
        if args.verify:
            time.sleep(3);require(len(server.state()['connected'])==count and all(c.input('inspect')['ready'] and c.input('inspect').get('markets') is not None for c in clients.values()),'Markets preview did not stay ready')
            write(ROOT/'LocalData/markets-launcher-check.json',dict(passed=True,build=run.build,runId=run.run_id,players=count,installedServerTouched=False));return
        write(ROOT/'LocalData/markets-preview-session.json',dict(runId=run.run_id,build=run.build,players=count,nativeControls=True))
        seen=read(ROOT/'LocalData/markets-preview-reopen.json')
        while any(c.process.poll() is None for c in clients.values()):
            request=read(ROOT/'LocalData/markets-preview-reopen.json')
            if request and request!=seen:
                seen=request;count=max(count,request['players'])
                for index in range(1,count+1):
                    profile='player-'+str(index);client=clients.get(profile)
                    if client is None or client.process.poll() is not None:open_player(index)
                write(ROOT/'LocalData/markets-preview-session.json',dict(runId=run.run_id,build=run.build,players=count,nativeControls=True))
            time.sleep(.5)
    finally:run.close();lock.close()
if __name__=='__main__':
    try:main()
    except Exception as error:(ROOT/'LocalData/markets-preview-error.txt').write_text(str(error),encoding='utf-8');raise
