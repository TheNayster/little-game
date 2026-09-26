# /// script
# dependencies = ["cryptography"]
# ///
"""Home interaction acceptance using isolated native release players and real UGUI touches."""
import argparse
import importlib.util
import json
from pathlib import Path
import time
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import Run, wait, require, read, write

spec=importlib.util.spec_from_file_location('scenic',Path(__file__).with_name('Test-ScenicWorlds.py'))
scenic=importlib.util.module_from_spec(spec);spec.loader.exec_module(scenic)
ready,travel,capture=scenic.ready,scenic.travel,scenic.capture

def command(client,action,**kw):
    # Authoritative fixture placement uses a read snapshot, which may lag a
    # just-acknowledged UI command. Retry ONLY an explicitly rejected revision.
    for _ in range(8):
        reply=scenic.command(client,action,**kw)
        if reply['accepted'] or reply['outcome']!='stale-revision':return reply
        time.sleep(.15)
    return reply

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'home';folder.mkdir();checks=[];fixture=None
    print('EVIDENCE '+str(folder),flush=True)
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    try:
        server=run.start('server');a=run.start('client','player-1');b=run.start('client','player-2');ready(a);ready(b)
        a.input('resize',x=1280,y=591);ready(a)
        def player(client):return next(p for p in server.state()['view']['players'] if p['id']==client.profile)
        def toy(id):return next(t for t in server.state()['view']['toys'] if t['id']==id)
        def settled(client):return wait(lambda:(s if not (s:=client.input('inspect'))['pending'] and not s['dragging'] else None),'home action settled')
        travel(a,'home');travel(b,'home')
        a.input('touchButton',text='Sit left');wait(lambda:player(a)['fixture']=='sofa-left','sofa entry')
        b.input('touchButton',text='Sit left');settled(b)
        require(player(b)['fixture']=='','Competing player took occupied seat')
        b.input('touchButton',text='Sit right');wait(lambda:player(b)['fixture']=='sofa-right','second sofa seat')
        time.sleep(.5);require(a.input('inspect')['homePose']=='Sit','Missing seated pose');capture(a,folder,'sofa-two-children-phone')
        require(command(a,1,value='orange-pup')['accepted'],'Character swap failed');time.sleep(.5)
        require(player(a)['fixture']=='sofa-left' and a.input('inspect')['character']=='bingo','Character swap lost seat')
        capture(a,folder,'bingo-seated-phone')
        if args.build>=126:
            def order(client):return client.input('inspect')['homeDrawOrder']
            def between(names,back,occupant,front):
                require(names.index(back)<names.index(occupant)<names.index(front),'Incorrect fixture occupant/cover order: '+occupant)
            names=order(a)
            for occupant in ('Player character','Friend-player-2'):between(names,'Home sofa',occupant,'Home sofa front')
            a.input('resize',x=1024,y=768);ready(a);capture(a,folder,'sofa-two-children-tablet')
            a.input('resize',x=1280,y=591);ready(a)
            for y,label in ((120,'front'),(360,'behind')):
                require(command(a,0,x=-3960,y=y)['accepted'],'Passerby placement failed');time.sleep(.4)
                names=order(a);between(names,'Home sofa','Friend-player-2','Home sofa front')
                require(names.index('Player character')>names.index('Home sofa front') if label=='front' else names.index('Player character')<names.index('Home sofa'),'Walking depth is incorrect')
                capture(a,folder,'sofa-passerby-'+label)
            a.input('touchButton',text='Sit left');wait(lambda:player(a)['fixture']=='sofa-left','return to seat')
            passed('layered sofa surrounds both sitters; front/behind passerby keeps sibling at support depth; phone/tablet composition')
        passed('two occupied sofa seats, same-seat rejection and seated character change through native UI')
        a.input('touchButton',text='Sit left');wait(lambda:player(a)['fixture']=='','stand up')
        require(command(a,0,x=-3480,y=150)['accepted'],'Position failed');time.sleep(.8)
        a.input('touchButton',text='Radio living power');wait(lambda:server.state()['view']['home']['livingRadio'],'radio switch')
        wait(lambda:a.input('inspect')['homePose']=='Dance','automatic dance')
        require(a.input('inspect')['homeMusicPlaying'],'Radio is silent');capture(a,folder,'radio-dancing-phone')
        a.input('touchButton',text='Menu');a.input('touchButton',text='Music setting');a.input('touchButton',text='Back to play')
        require(a.input('inspect')['musicMuted'] and not a.input('inspect')['homeMusicPlaying'],'Local music mute failed')
        require(server.state()['view']['home']['livingRadio'],'Local mute changed shared radio')
        a.input('touchButton',text='Tap to walk');a.input('touch-begin',role='stick',x=45,finger=20)
        wait(lambda:a.input('inspect')['homePose']=='Walk','walking interrupts dancing');a.input('touch-end',role='stick',finger=20)
        a.input('touchButton',text='Tap to walk');a.input('touchButton',text='Radio living power');settled(a)
        wait(lambda:a.input('inspect')['homePose']=='Idle','radio off settles pose')
        passed('radio plays original audio, idle character dances, movement overrides and device mute preserves shared power')
        travel(a,'garden');require(command(a,0,x=1650,y=220)['accepted'],'Position failed');time.sleep(.8)
        a.input('touchButton',text='Bounce left');wait(lambda:player(a)['fixture']=='trampoline-left','trampoline entry')
        wait(lambda:a.input('inspect')['homePose']=='Bounce','bounce pose')
        require(player(b)['fixture']=='sofa-right','Trampoline interrupted sibling seat')
        ages=[]
        for i in range(3):ages.append(capture(a,folder,'trampoline-'+str(i))['homePoseAge']);time.sleep(.2)
        require(ages[-1]>ages[0]+.2,'Shared bounce animation stalled')
        if args.build>=126:between(order(a),'Home trampoline','Player character','Home trampoline front')
        a.input('touchButton',text='Tap to walk');a.input('touch-begin',role='stick',x=45,finger=21)
        wait(lambda:player(a)['fixture']=='','walk exits trampoline')
        wait(lambda:a.input('inspect')['homePose']=='Walk','walking clears the client bounce pose')
        a.input('touch-end',role='stick',finger=21)
        wait(lambda:a.input('inspect')['homePose']=='Idle','bounce stays cleared after stopping')
        wait(lambda:next(p for p in b.state()['view']['players'] if p['id']==a.profile)['fixture']=='','sibling receives released slot')
        passed('backyard bounce poses run while sibling remains seated; joystick safely exits')
        # Fill the real existing bucket, then carry it to storage via the same drag transaction.
        require(command(a,2,item='bucket-1')['accepted'],'Grab failed')
        require(command(a,3,item='bucket-1',target='tap-1',x=150,y=340)['accepted'],'Fill failed')
        require(command(a,0,x=3550,y=180)['accepted'],'Shed placement failed');time.sleep(.8)
        # Move the bucket beside the player so every subsequent interaction is visible/touchable.
        require(command(a,2,item='bucket-1')['accepted'],'Grab failed')
        require(command(a,3,item='bucket-1',x=3190,y=280)['accepted'],'Bucket placement failed')
        a.input('touchButton',text='Shed doors');wait(lambda:server.state()['view']['home']['shedOpen'],'open shed')
        a.input('touch-begin',role='bucket-1',finger=22);a.input('touch-move',x=3465,y=430,finger=22);a.input('touch-end',x=3465,y=430,finger=22)
        wait(lambda:toy('bucket-1')['container']=='shed-0','store full bucket');settled(a)
        require(toy('bucket-1')['water']==3,'Stored bucket lost water')
        a.input('touch-begin',role='ball-1',finger=23);a.input('touch-move',x=3635,y=160,finger=23);a.input('touch-end',x=3635,y=160,finger=23)
        wait(lambda:toy('ball-1')['container']=='shed-3','store ball');settled(a);capture(a,folder,'shed-stored-open-phone')
        if args.build>=126:
            for item in ('bucket-1','ball-1'):between(order(a),'Home shed',item,'Home shed front')
        a.input('touchButton',text='Shed doors');wait(lambda:not server.state()['view']['home']['shedOpen'],'close shed');capture(a,folder,'shed-closed-phone')
        if args.build>=126:require(all(item not in order(a) for item in ('bucket-1','ball-1')),'Closed shed exposes stored items')
        travel(a,'creek');travel(a,'garden');command(a,0,x=3550,y=180);time.sleep(.8)
        a.input('touchButton',text='Shed doors');wait(lambda:server.state()['view']['home']['shedOpen'],'reopen shed')
        a.input('touch-begin',role='bucket-1',finger=24);a.input('touch-move',x=3290,y=200,finger=24);a.input('touch-end',x=3290,y=200,finger=24)
        wait(lambda:toy('bucket-1')['container']=='' and toy('bucket-1')['holder']=='','retrieve bucket')
        require(toy('bucket-1')['water']==3 and toy('ball-1')['container']=='shed-3','Travel/retrieval changed contents or other item')
        a.input('resize',x=1024,y=768);ready(a);capture(a,folder,'shed-open-tablet')
        passed('real touch shed open/store/close/travel/retrieve retains one full bucket and one ball; phone/tablet render')
        # Separate offline world: durable prop contents and music, no family-save access.
        fixture=RecoveryFixture(args.build);solo=fixture.launch(1);s=ready(solo);require(not s['shared'],'Expected offline')
        travel(solo,'home')
        # Reveal the right-hand radio with a real empty-wall camera pan.
        solo.input('touch-begin',role='screen',x=700,y=500,finger=40)
        solo.input('touch-move',role='screen',x=550,y=500,finger=40)
        solo.input('touch-move',role='screen',x=400,y=500,finger=40)
        solo.input('touch-end',role='screen',x=400,y=500,finger=40)
        solo.input('touchButton',text='Radio living power')
        wait(lambda:solo.input('inspect')['home']['livingRadio'],'offline radio on')
        solo.input('touchButton',text='Sit left');wait(lambda:solo.input('inspect')['homePose']=='Sit','offline sit')
        solo.input('touchButton',text='Menu');save_path=Path(solo.input('inspect')['savePath']);solo.close()
        saved=json.loads(save_path.read_bytes().split(b'\n',2)[2]);require(saved['schema']==(5 if args.build>=129 else 4) and saved['home']['livingRadio'],'Home save missing')
        reopened=fixture.launch(1);s=ready(reopened);require(not s['shared'] and s['home']['livingRadio'],'Offline home state lost')
        require(s['homePose']!='Sit','Recovered temporary seat lease');capture(reopened,folder,'offline-home-reopened')
        passed('versioned offline save/reopen retains radio and property, safely releases interrupted seat')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,evidence=str(folder),liveFamilyServerAccessed=False,physicalDevicesAccessed=False))
        print('RESULT '+str(folder/'result.json'),flush=True)
    finally:
        try:
            if fixture:fixture.cleanup()
        finally:run.close()

if __name__=='__main__':main()
