# /// script
# dependencies = ["cryptography"]
# ///
"""Focused Hulk foundation check with four real isolated release clients.

Exercises the current entrance/prop slice. This is not whole-world acceptance.
The asset pack and family are local; the installed family server is untouched.
"""
import argparse
import hashlib
import importlib.util
import os
from pathlib import Path
import re
import sys
import time

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT
from shared_garden_runtime import Run,read,write,wait,require

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int)
    parser.add_argument('--art',type=Path,default=ROOT/'LocalData/Research/DuploMarvel/world-tests/HulkCompleteProbe/Baked2D')
    args=parser.parse_args()
    art=args.art.resolve();require((art/'actor-frames.json').is_file(),'Current birthday pack is required')
    previous=os.environ.get('LITTLE_WEEPS_BIRTHDAY_ART')
    os.environ['LITTLE_WEEPS_BIRTHDAY_ART']=str(art)
    run=Run(args.build,extended_test_lifetime=True)
    out=run.path/'hulk-birthday';out.mkdir()
    checks=[];clients=[];passed=False
    source=read(run.folder/'source-manifest.json')
    relevant=[f for f in source['files'] if f['path'].startswith('Unity/')]
    require(relevant and all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in relevant),'Build does not match current Unity source')
    core=(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text(encoding='utf-8-sig')
    actions=[a.strip() for a in re.search(r'enum SoloAction\s*{([^}]+)}',core).group(1).split(',')]
    birthday=actions.index('Birthday')
    pack=read(art/'actor-frames.json')
    required={'actor-frames.json',*(a['texture'] for a in pack['actions']),*(p['texture'] for p in pack['props']),*(f'world-{i}.png' for i in range(4))}
    asset_receipt=[{'path':name,'sha256':hashlib.sha256((art/name).read_bytes()).hexdigest()} for name in sorted(required)]
    write(out/'asset-receipt.json',asset_receipt)
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def command(client,action,**kw):
        result=home.command(client,action,**kw);require(result['accepted'],result['outcome']);home.ready(client);return result
    def view():return server.state()['view']
    def prop():return next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-0')
    def rendered(client,holder):
        return wait(lambda:'birthday-gift-0='+holder in client.input('inspect').get('birthdayPropOwners',[]),'rendered gift owner '+holder,20)
    def tap_prop(client,name):
        # The image pivot is its recovered ground/hold anchor, not the button
        # centre. Touch the visible cutout instead of a transparent edge pixel.
        control=next(c for c in client.input('inspect')['controls'] if c['name']==name)
        rect=control['bounds'];x=rect['x']+rect['width']*.5;y=rect['y']+rect['height']*.5
        client.input('touch-begin',role='screen',x=x,y=y,finger=91);time.sleep(.09)
        client.input('touch-end',role='screen',x=x,y=y,finger=91)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots];a,b,c,d=clients
        for client in clients:home.ready(client)
        a.input('resize',x=1280,y=800);home.ready(a)
        # The world card is a real UGUI control; remaining travel/placement are fixtures.
        home.scenic.NAMES['hulk-birthday']="World Hulk's Birthday"
        home.travel(a,'hulk-birthday')
        wait(lambda:home.ready(a)['zone']=='hulk-birthday','birthday world-card arrival',30)
        for client in clients[1:]:command(client,actions.index('Travel'),value='hulk-birthday')
        for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
        wait(lambda:len(server.state()['connected'])==4,'four connected clients')
        for client in clients:
            evidence=client.input('inspect');controls=evidence['controls']
            require(all(any(v['name']=='Birthday friend '+other.profile for v in controls) for other in clients),'Missing shared birthday character controls')
            require(evidence['visibleBirthdayPlayers']==4,'Client does not show all four connected birthday players')
        record('four real clients enter one shared birthday world with the recovered cast')
        tap_prop(a,'Birthday prop birthday-gift-0')
        wait(lambda:prop()['holder']==a.profile,'actual gift pickup',20)
        for client in clients:rendered(client,a.profile)
        rejected=home.command(b,birthday,value='claim',item='birthday-gift-0')
        require(not rejected['accepted'] and rejected['outcome']=='prop-already-in-use','Contended gift claim did not reject once')
        home.capture(a,out,'hulk-holding-gift')
        record('UGUI pickup attaches one authoritative gift; competing client cannot duplicate it')
        a.input('touchButton',text='Birthday friend '+b.profile)
        wait(lambda:prop()['holder']==b.profile,'actual gift handoff',20)
        rendered(b,b.profile)
        home.capture(b,out,'spidey-receives-gift')
        record('UGUI character handoff transfers ownership across clients')
        # A clear drop pad avoids asking a real touch to select an obscured
        # object beneath the untouched starting gifts.
        if b.input('inspect')['joystickVisible']:b.input('touchButton',text='Tap to walk')
        b.input('touch-begin',x=1150,y=250,finger=92);time.sleep(.09);b.input('touch-end',x=1150,y=250,finger=92)
        wait(lambda:abs(next(p for p in view()['players'] if p['id']==b.profile)['x']-1150)<2,'real carried-prop walking destination',20)
        time.sleep(.5);home.capture(a,out,'spidey-carries-shared-gift')
        require(prop()['holder']==b.profile,'Walking dropped carried gift')
        b.process.terminate();b.process.wait(timeout=15)
        wait(lambda:len(server.state()['connected'])==3 and prop()['holder']=='','disconnect releases only own gift',25)
        for client in (a,c,d):wait(lambda:client.input('inspect')['visibleBirthdayPlayers']==3,'disconnected player leaves active presentation')
        require(all(next(p for p in view()['players'] if p['id']==client.profile)['zone']=='hulk-birthday' for client in (a,c,d)),'Departure moved a sibling')
        command(a,actions.index('Move'),x=prop()['x'],y=prop()['y'])
        time.sleep(.8)
        tap_prop(a,'Birthday prop birthday-gift-0')
        wait(lambda:prop()['holder']==a.profile,'released gift reusable')
        rendered(a,a.profile)
        record('real disconnect releases the carried prop and leaves the other three players together')
        home.capture(a,out,'three-siblings-continue-party')
        passed=True
    finally:
        if clients:
            try:
                write(out/'last-authority-state.json',server.state())
                write(out/'last-client-state.json',clients[0].input('inspect'))
            except (RuntimeError,OSError):pass
        write(out/'result.json',{'passed':passed,'checks':checks,'build':args.build,'source_commit':source['sourceCommit'],
            'scope':'Entrance, cast and shared prop foundation only; later birthday activities are not accepted.',
            'art_root':str(art),'clients_started':len(clients)})
        run.close()
        if previous is None:os.environ.pop('LITTLE_WEEPS_BIRTHDAY_ART',None)
        else:os.environ['LITTLE_WEEPS_BIRTHDAY_ART']=previous


if __name__=='__main__':
    main()
