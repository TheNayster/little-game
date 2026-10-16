# /// script
# dependencies = ["cryptography"]
# ///
"""Open the verified local Peppa Funfair world with a separate saved preview family."""
import argparse,hashlib,msvcrt,os,shutil,sys,time,uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--players',type=int,choices=(1,2,3,4));p.add_argument('--verify',action='store_true',help='Open hidden disposable clients, confirm readiness, then close them.');args=p.parse_args()
    record=read(ROOT/'LocalData/peppa-funfair-preview.json');require(record,'No checked Peppa Funfair preview is selected')
    pack=(ROOT/record['artRoot']).resolve();require(pack.is_relative_to(ROOT),'Preview content must stay inside this project')
    receipt=read(ROOT/record['assetReceipt']);require(receipt,'Checked content receipt is missing')
    for row in receipt:
        file=(pack/row['path']).resolve();require(file.is_relative_to(pack) and file.is_file() and hashlib.sha256(file.read_bytes()).hexdigest()==row['sha256'],'Peppa Funfair content differs from the checked pack: '+row['path'])
    if record.get('packaged'):os.environ.pop('LITTLE_WEEPS_FUNFAIR_ART',None)
    else:os.environ['LITTLE_WEEPS_FUNFAIR_ART']=str(pack)
    lock=(ROOT/'LocalData/peppa-funfair-preview.lock').open('a+b')
    if lock.tell()==0:lock.write(b'1');lock.flush()
    lock.seek(0)
    try:msvcrt.locking(lock.fileno(),msvcrt.LK_NBLCK,1)
    except OSError:
        require(not args.verify,'An existing Peppa Funfair preview owns the launcher')
        write(ROOT/'LocalData/peppa-funfair-preview-reopen.json',dict(nonce=uuid.uuid4().hex,players=args.players or 2));lock.close();return
    previous=None if args.verify else read(ROOT/'LocalData/peppa-funfair-preview-session.json')
    count=args.players or (previous or {}).get('players',2)
    run=Run(record['buildNumber'],interactive=not args.verify,review_controls=True,extended_test_lifetime=True,resume=previous['runId'] if previous else None,graphics_api='d3d11')
    try:
        server=run.start('server');clients={}
        def open_player(index):
            profile='player-'+str(index);client=run.start('client',profile)
            wait(lambda:client.input('inspect')['ready'],'Peppa Funfair preview ready')
            if previous is None:
                client.input('fixtureTravel',text='peppa-funfair');wait(lambda:client.input('inspect')['zone']=='peppa-funfair','Peppa Funfair preview entry')
            clients[profile]=client
        for i in range(1,count+1):open_player(i)
        if args.verify:
            time.sleep(3);require(len(server.state()['connected'])==count and all(c.input('inspect')['ready'] for c in clients.values()),'Preview did not keep a durable ready state')
            require(all(c.input('inspect')['visibleFunfairPlayers']==count for c in clients.values()),'Packaged Funfair characters are not visible')
            first=next(iter(clients.values()));first.input('capture');wait(lambda:(first.out/'garden.png').exists(),'packaged launcher capture');shutil.copyfile(first.out/'garden.png',run.path/'peppa-funfair-launcher.png')
            write(ROOT/'LocalData/peppa-funfair-launcher-check.json',dict(passed=True,build=run.build,runId=run.run_id,players=count,installedServerTouched=False));return
        write(ROOT/'LocalData/peppa-funfair-preview-session.json',dict(runId=run.run_id,build=run.build,players=count))
        seen=read(ROOT/'LocalData/peppa-funfair-preview-reopen.json')
        while any(c.process.poll() is None for c in clients.values()):
            request=read(ROOT/'LocalData/peppa-funfair-preview-reopen.json')
            if request and request!=seen:
                seen=request;count=max(count,request['players'])
                for i in range(1,count+1):
                    profile='player-'+str(i);client=clients.get(profile)
                    if client is None or client.process.poll() is not None:open_player(i)
                write(ROOT/'LocalData/peppa-funfair-preview-session.json',dict(runId=run.run_id,build=run.build,players=count))
            time.sleep(.5)
    finally:run.close();lock.close()

if __name__=='__main__':
    try:main()
    except Exception as error:
        (ROOT/'LocalData/peppa-funfair-preview-error.txt').write_text(str(error),encoding='utf-8');raise
