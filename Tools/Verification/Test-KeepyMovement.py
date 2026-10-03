# /// script
# dependencies = ["cryptography"]
# ///
"""Real multi-touch regression in disposable release clients, never family data."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import importlib.util
from pathlib import Path
import time
from recovery_fixture import RecoveryFixture
from shared_garden_runtime import Run, require, wait, write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);fixture=None;checks=[];folder=run.path/'keepy-movement';folder.mkdir()
    print('EVIDENCE '+str(folder),flush=True)
    def passed(name):checks.append(dict(check=name,passed=True));print('PASS '+name,flush=True)
    def x(client):return next(p['position']['x'] for p in client.input('inspect')['players'] if p['id']==client.profile)
    def moving_after(client,action,label,direction=1):
        action();start=x(client);time.sleep(.4);delta=(x(client)-start)*direction
        require(delta>45,label+' canceled walking: '+str(delta));passed(label)
    def exercise(client,label):
        home.ready(client);home.travel(client,'garden')
        if not client.input('inspect')['joystickVisible']:client.input('touchButton',text='Tap to walk')
        if args.build>=151:
            # Carry the disposable fixture prop to the new clear-lawn start.
            # Use ordinary input so this setup also works in private solo.
            destination=client.input('inspect')['keepy']['x']-200
            client.input('touch-begin',role='bucket-1',finger=28);client.input('touch-begin',role='stick',x=30,finger=29)
            wait(lambda:x(client)>=destination,'walk held setup prop to balloon',15)
            client.input('touch-end',role='stick',x=30,finger=29)
            client.input('touch-end',x=x(client)-60,y=100,finger=28)
            wait(lambda:not client.input('inspect')['dragging'],'setup prop released');home.ready(client);time.sleep(.6)
        client.input('touch-begin',role='stick',x=30,finger=21)
        moving_after(client,lambda:client.input('touchButton',text='Play Keepy Uppy'),label+': stationary joystick finger keeps walking after balloon tap')
        # A second touch picks up a real prop while the first continues moving.
        client.input('touch-begin',role='bucket-1',finger=22)
        wait(lambda:client.input('inspect')['dragging']=='bucket-1','held object')
        moving_after(client,lambda:None,label+': walking continues during object pickup/hold')
        moving_after(client,lambda:client.input('touchButton',text='Play Keepy Uppy'),label+': third balloon touch preserves both movement and held object')
        require(client.input('inspect')['dragging']=='bucket-1','Balloon released the held prop')
        client.input('touch-cancel',role='bucket-1',finger=22)
        wait(lambda:not client.input('inspect')['dragging'],'object cancel settles')
        moving_after(client,lambda:None,label+': releasing object does not cancel joystick')
        client.input('touch-end',role='stick',x=30,finger=21);time.sleep(.3);at=x(client);time.sleep(.3)
        require(abs(x(client)-at)<5,'Joystick release did not stop walking')
        client.input('touch-begin',role='stick',x=30,finger=23);client.input('touchButton',text='Menu');time.sleep(.3);at=x(client);time.sleep(.3)
        require(abs(x(client)-at)<5,'Menu failed to stop movement');passed(label+': joystick release and menu still stop movement')
        client.input('touch-end',role='stick',finger=23);client.input('touchButton',text='Back to play')
        client.input('touchButton',text='Tap to walk')
        time.sleep(.6)
        start=x(client);client.input('touch-begin',x=start-400,y=35,finger=24);client.input('touch-end',x=start-400,y=35,finger=24)
        moving_after(client,lambda:client.input('touchButton',text='Play Keepy Uppy'),label+': tap-to-walk destination survives balloon tap',direction=-1)
    try:
        server=run.start('server');clients=[run.start('client','player-'+str(i)) for i in range(1,5)]
        for c in clients:home.ready(c)
        peers={c.profile:dict(next(p for p in server.state()['view']['players'] if p['id']==c.profile)) for c in clients[1:]}
        exercise(clients[0],'four-player authority')
        for c in clients[1:]:
            now=next(p for p in server.state()['view']['players'] if p['id']==c.profile);before=peers[c.profile]
            require((now['x'],now['y'],now['zone'])==(before['x'],before['y'],before['zone']),'Sibling moved')
        passed('all three siblings remain independent during mixed input')
        fixture=RecoveryFixture(args.build);solo=fixture.launch(1);exercise(solo,'private solo')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,liveFamilyServerAccessed=False,physicalMultiTouchQualified=False))
        print('RESULT '+str(folder/'result.json'),flush=True)
    finally:
        if fixture:fixture.cleanup()
        run.close()

if __name__=='__main__':main()
