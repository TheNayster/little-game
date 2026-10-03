"""Native release menus, through Unity Input System touches and isolated authority."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import shutil
import time
from shared_garden_runtime import Run, wait, require, write


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int)
    args=parser.parse_args()
    run=Run(args.build);checks=[]
    evidence=run.path/'navigation';evidence.mkdir()
    def passed(name,**extra):
        checks.append(dict(check=name,passed=True,**extra));print('PASS '+name,flush=True)
    try:
        server=run.start('server');first=run.start('client','player-1');second=run.start('client','player-2')
        def player(id):return next(p for p in server.state()['view']['players'] if p['id']==id)
        def toy(id):return next(t for t in server.state()['view']['toys'] if t['id']==id)
        def settled(client):return wait(lambda: not client.input('inspect')['pending'] and client.input('inspect')['dragging']=='','settled menu action')
        def tap(name,client=first):return client.input('touchButton',text=name)
        def capture(name):
            target=first.out/'garden.png'
            old=target.stat().st_mtime_ns if target.exists() else 0
            state=first.input('capture')
            wait(lambda:target.exists() and target.stat().st_mtime_ns!=old,'fresh capture')
            shutil.copyfile(target,evidence/(name+'.png'));write(evidence/(name+'.json'),state)
            require(state['controlsInSafeArea'],'Controls outside safe area: '+name)
            if state['charactersOpen']:require(state['fullCharactersInTray'],'Full-body artwork clipped by tray: '+name)
            return state
        first.input('resize',x=1024,y=768)
        state=capture('tablet-play')
        family=next(c['bounds'] for c in state['controls'] if c['name']=='Characters')
        require(family['width']>=80,'Family target too small')
        before=dict(player(first.profile))
        require(tap('Characters')['charactersOpen'],'Family circle did not open tray')
        capture('tablet-characters')
        tap('Bingo');settled(first)
        wait(lambda:player(first.profile)['avatar']=='orange-pup','Bingo acknowledged')
        after=player(first.profile)
        require({k:v for k,v in before.items() if k!='avatar'}=={k:v for k,v in after.items() if k!='avatar'},'Character switch changed stable player state')
        require(first.input('inspect')['character']=='bingo' and first.input('inspect')['characterLayers']==11,'Layered Bingo not displayed')
        require(second.state()['view']['players'][0]['avatar']=='orange-pup','Sibling did not receive avatar')
        passed('family circle opens full-body tray; touch changes only avatar and replicates')

        first.input('touch-begin',role='ui:Bluey',finger=31)
        first.input('touch-move',role='ui:Bluey',x=-120,finger=31)
        first.input('touch-end',role='ui:Bluey',x=-120,finger=31)
        first.input('touch-begin',role='ui:Bluey',finger=32)
        first.input('touch-cancel',role='ui:Bluey',finger=32)
        time.sleep(.4)
        require(player(first.profile)['avatar']=='orange-pup','Swipe or canceled touch selected character')
        passed('tray swipe and canceled touch never select another character')
        tap('Close characters');settled(first)
        require(not first.input('inspect')['menuOpen'],'Arrow did not close tray')
        require(player(first.profile)==after,'Closing tray moved player through UI')

        tap('Tap to walk')
        require(first.input('inspect')['joystickVisible'],'Joystick did not appear')
        first.input('touch-begin',role='stick',x=45,finger=40)
        wait(lambda: player(first.profile)['x']>before['x']+5,'joystick moved player')
        tap('Characters');time.sleep(.6)
        stopped=dict(player(first.profile));time.sleep(.4)
        require(player(first.profile)==stopped and not first.input('inspect')['joystickVisible'],'Tray failed to stop/hide walking')
        first.input('touch-end',role='stick',finger=40)
        tap('Close characters')
        require(first.input('inspect')['joystickVisible'],'Closing did not restore joystick')
        require(player(first.profile)==stopped,'Released old joystick gesture resumed walking')
        tap('Tap to walk')
        passed('tray stops and hides joystick; close restores control without replaying old gesture')

        # A menu settles an in-flight direct drag; its contents and committed
        # position survive. ChangeAvatar itself retains held leases in core tests.
        before_bucket=dict(toy('bucket-1'))
        first.input('touch-begin',role='bucket-1',finger=41)
        tap('Characters');settled(first)
        first.input('touch-cancel',role='bucket-1',finger=41)
        require(toy('bucket-1')==before_bucket,'Menu cancellation changed prop or stranded its lease')
        tap('Close characters');passed('menu during pickup settles lease without changing the prop')

        tap('Worlds');require(first.input('inspect')['worldsOpen'],'Home circle did not open world browser')
        state=capture('tablet-worlds')
        require(len([c for c in state['controls'] if c['name'].startswith('World ') and c['name']!='World settings'])==6,'Six world bubbles not visible on tablet')
        require(not next(c for c in state['controls'] if c['name']=='World Heeler Home')['enabled'],'Unbuilt house presented as playable')
        tap('World Heeler Home');require(first.input('inspect')['worldsOpen'],'Unbuilt world entered')
        # Sibling can move and use an object while the first player browses.
        second.input('press',x=910,y=50);second.input('release',x=910,y=50)
        wait(lambda:abs(player(second.profile)['x']-910)<2,'sibling walking while menu open')
        second.input('press',role='bucket-1');second.input('release',x=150,y=340);settled(second)
        require(toy('bucket-1')['water']==3,'Sibling could not fill bucket while menu open')
        tap('World Creek');settled(first)
        require(player(first.profile)['zone']=='creek' and player(second.profile)['zone']=='garden','Travel regrouped sibling or failed')
        require(toy('bucket-1')['water']==3,'Travel reset shared objects')
        tap('Worlds');tap('World Garden');settled(first)
        require(player(first.profile)['zone']=='garden' and toy('bucket-1')['water']==3,'Return failed or duplicated area state')
        passed('six picture bubbles; unfinished entries inactive; independent Creek round trip preserves sibling play')

        first.input('resize',x=1280,y=591)
        capture('phone-play');tap('Characters');capture('phone-characters');tap('Close characters')
        tap('Worlds');phone_worlds=capture('phone-worlds')
        zone=player(first.profile)['zone']
        first.input('touch-begin',role='ui:World Garden',finger=52)
        first.input('touch-move',role='ui:World Garden',x=-60,finger=52)
        first.input('touch-move',role='ui:World Garden',x=-650,finger=52)
        first.input('touch-end',role='ui:World Garden',x=-650,finger=52)
        require(first.input('inspect')['worldsOpen'] and player(first.profile)['zone']==zone,'World swipe caused travel')
        scrolled=capture('phone-worlds-scrolled')
        old_x=next(c['bounds']['x'] for c in phone_worlds['controls'] if c['name']=='World Garden')
        new_x=next(c['bounds']['x'] for c in scrolled['controls'] if c['name']=='World Garden')
        require(new_x<old_x-50 and any(c['name']=='World Daycare' for c in scrolled['controls']),'Swipe did not browse to the last world')
        tap('Back from worlds');require(not first.input('inspect')['menuOpen'],'World back failed')
        tap('Worlds');tap('World settings');require(first.input('inspect')['menuOpen'] and not first.input('inspect')['worldsOpen'],'World gear failed')
        tap('Back to play');require(not first.input('inspect')['menuOpen'],'Settings return failed')
        passed('phone layout, world swipe, back arrow and settings keep correct touch ownership')
        result=dict(passed=True,build=args.build,utc=datetime.now(timezone.utc).isoformat(),runId=run.run_id,checks=checks,
                    physicalDevicesAccessed=False,liveFamilyServerAccessed=False,evidence=str(evidence))
        write(evidence/'result.json',result);print('Evidence: '+str(evidence),flush=True)
    except Exception as error:
        write(evidence/'result.json',dict(passed=False,build=args.build,checks=checks,error=str(error)))
        print('FAIL '+str(error)+'; evidence: '+str(evidence),flush=True);raise
    finally:run.close()


if __name__=='__main__':main()
