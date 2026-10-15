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
from concurrent.futures import ThreadPoolExecutor

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT
from shared_garden_runtime import Run,read,write,wait,require

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int)
    parser.add_argument('--actions',action='store_true',help='Also check directional carry and the handoff reaction frames.')
    parser.add_argument('--guests',action='store_true',help='Also check the shared intro, source guest arrivals and NPC gift exchange.')
    parser.add_argument('--party',action='store_true',help='Also check shared cake editing, portions, drinks and one Hulk unwrap.')
    parser.add_argument('--vendor',action='store_true',help='Also check the one shared squirrel queue with four real clients.')
    parser.add_argument('--band',action='store_true',help='Also check the shared band, power, floor lights and independent departure.')
    parser.add_argument('--castle',action='store_true',help='Also check four shared castle bodies, gravity, stunts, drag and independent exit.')
    parser.add_argument('--play',action='store_true',help='Also check shared brick pegging and the bubble machine.')
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
    required={'actor-frames.json',*(a['texture'] for a in pack['actions']),*(p['texture'] for p in pack['props']),*(s['texture'] for s in pack.get('cakeSprites',[])),*(f'world-{i}.png' for i in range(4))}
    required.update(a['file'] for a in pack.get('audio',[]))
    if pack.get('danceFloor'):required.add(pack['danceFloor']['texture'])
    if pack.get('castle'):required.update([pack['castle']['texture'],pack['castle']['front']])
    asset_receipt=[{'path':name,'sha256':hashlib.sha256((art/name).read_bytes()).hexdigest()} for name in sorted(required)]
    write(out/'asset-receipt.json',asset_receipt)
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def command(client,action,**kw):
        result=home.command(client,action,**kw);require(result['accepted'],result['outcome']);home.ready(client);return result
    def button(client,text):
        home.ready(client)
        wait(lambda:any(c['name']==text and c['enabled'] for c in client.input('inspect')['controls']),'visible enabled button '+text,15)
        result=client.input('touchButton',text=text);home.ready(client);return result
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
    def walk_carry(client,x,y,mirrored=False):
        client.input('touch-begin',x=x,y=y,finger=92);time.sleep(.09);client.input('touch-end',x=x,y=y,finger=92)
        samples=[];until=time.monotonic()+20
        while time.monotonic()<until:
            evidence=client.input('inspect');pose=next((p for p in evidence.get('birthdayPoses',[]) if p.startswith(client.profile+'=')),'')
            samples.append({'pose':pose,'time':time.monotonic()})
            position=next(p for p in view()['players'] if p['id']==client.profile)
            if abs(position['x']-x)<2 and abs(position['y']-y)<2:break
        write(out/('carry-left-trace.json' if mirrored else 'carry-right-trace.json'),samples)
        if abs(position['x']-x)>=2 or abs(position['y']-y)>=2:write(out/'failed-walk-input.json',evidence)
        require(abs(position['x']-x)<2 and abs(position['y']-y)<2,'Real carried-prop walk did not arrive')
        if args.actions:
            poses={s['pose'] for s in samples if '=carry-side:' in s['pose'] and (':mirrored' in s['pose'])==mirrored}
            require(len(poses)>=2,'Directional carry did not advance through multiple original frames')
            require(prop()['holder']==client.profile,'Directional walk changed possession')
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
        if args.play:
            for client in clients:command(client,actions.index('Move'),x=4050,y=140)
            time.sleep(1.5);tap_prop(a,'Birthday prop birthday-brick-0')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-brick-0')['holder']==a.profile,'building pickup')
            button(a,'Turn brick');button(a,'Brick colour');tap_prop(a,'Birthday prop birthday-brick-1')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-brick-0')['support']=='birthday-brick-1','shared brick pegs',15)
            brick=next(p for p in view()['birthday']['props'] if p['id']=='birthday-brick-0');require(brick['turns']==1 and brick['tint']==1,'Brick appearance changes were lost')
            denied=home.command(b,birthday,value='claim',item='birthday-brick-1');require(not denied['accepted'],'Sibling moved an occupied brick support')
            home.capture(a,out,'shared-building-bricks')
            command(a,actions.index('Move'),x=1850,y=150);time.sleep(1.5);tap_prop(a,'Birthday bubble machine')
            wait(lambda:view()['birthday']['bubblesOn'],'shared bubble machine on')
            wait(lambda:len([c for c in a.input('inspect')['controls'] if c['name'].startswith('Birthday bubble ') and c['name']!='Birthday bubble machine'])>=3,'visible shared bubbles',10)
            button(a,'Birthday bubble 2');wait(lambda:view()['birthday']['bubblesPopped']==1,'one actual bubble pop',10)
            popped=view()['birthday']['poppedBubbles'][-1]
            denied=home.command(b,birthday,value='bubble-pop',item=str(popped));require(not denied['accepted'],'Competing bubble pop duplicated the event')
            button(a,'Birthday bubble switch');wait(lambda:not view()['birthday']['bubblesOn'],'shared machine switches off')
            record('actual UI rotates/colours/pegs a shared brick, protects occupied support and pops shared bubbles exactly once')
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
            time.sleep(1.5)
        if args.castle:
            for client in clients:command(client,actions.index('Move'),x=1169,y=100)
            time.sleep(1.5)
            with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda client:button(client,'Birthday bouncy castle'),clients))
            wait(lambda:all(m['station']=='castle' for m in view()['birthday']['members']),'four actual touches join the same castle',20)
            wait(lambda:all(m['landings']>=2 for m in view()['birthday']['members']),'all four physics bodies rebound from floor',15)
            states=[]
            for i in range(8):states.append(view()['birthday']);time.sleep(.1)
            write(out/'castle-physics-trace.json',states)
            require(all(max(s['members'][i]['bounceH'] for s in states)-min(s['members'][i]['bounceH'] for s in states)>.2 for i in range(4)),'A castle body did not move vertically')
            home.capture(a,out,'four-player-bouncy-castle')
            button(a,'Castle stunt');wait(lambda:view()['birthday']['members'][0]['stunts']==1,'real stunt touch changes shared body',10)
            control=next(c for c in a.input('inspect')['controls'] if c['name']=='Birthday bouncy castle');rect=control['bounds']
            x=rect['x']+rect['width']*.5;y=rect['y']+rect['height']*.6
            a.input('touch-begin',role='screen',x=x,y=y,finger=94)
            a.input('touch-move',role='screen',x=x+70,y=y-50,finger=94);time.sleep(.2)
            a.input('touch-end',role='screen',x=x+70,y=y-50,finger=94)
            before=[m['landings'] for m in view()['birthday']['members']]
            button(a,'Leave castle');wait(lambda:view()['birthday']['members'][0]['station']=='','independent castle exit')
            wait(lambda:all(m['station']=='castle' and m['landings']>before[i] for i,m in enumerate(view()['birthday']['members']) if i>0),'siblings keep bouncing after independent exit',10)
            record('four real touches join one castle; shared gravity/floor rebounds and stunt/drag input work; one exit preserves three moving siblings')
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
            time.sleep(1.5)
        if args.band:
            for client,x,y in zip(clients,[2880,2804,3229,2985],[60,180,191,249]):command(client,actions.index('Move'),x=x,y=y)
            time.sleep(1.5)
            with ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(lambda client:button(client,'Play band together'),clients))
            state=wait(lambda:view()['birthday'] if view()['birthday']['bandRoundAt']>=0 else None,'one common band countdown',20)
            round_at=state['bandRoundAt'];require(len(state['bandActors'])==4,'Band did not include all four family members')
            wait(lambda:all(any(s['key']=='electric' and s['playing'] for s in client.input('inspect')['birthdaySounds']) for client in clients),'all clients play the powered arrangement',15)
            samples=[client.input('inspect') for client in clients]
            require(all(len(s['birthdayFloorColours'])==36 and any(c>=0 for c in s['birthdayFloorColours']) for s in samples),'Shared dance floor is not lit')
            write(out/'band-powered-client-evidence.json',samples);home.capture(c,out,'shared-band-powered')
            command(a,actions.index('Move'),x=3454,y=67);time.sleep(.8)
            tap_prop(a,'Birthday prop birthday-battery')
            wait(lambda:not view()['birthday']['powered'],'battery removal changes one shared circuit',10)
            wait(lambda:all(any(s['key']=='acoustic' and s['playing'] for s in client.input('inspect')['birthdaySounds']) for client in (b,c,d)),'siblings continue the same song acoustically',10)
            require(view()['birthday']['bandRoundAt']==round_at,'Removing battery restarted the band round')
            write(out/'band-acoustic-client-evidence.json',[client.input('inspect') for client in clients])
            button(a,'Return battery');wait(lambda:view()['birthday']['powered'],'battery returned to shared socket',10)
            require(view()['birthday']['bandRoundAt']==round_at,'Returning battery restarted the round')
            wait(lambda:view()['birthday']['bandRounds']==1,'one shared band completion',30)
            require(all(m['station']=='' for m in view()['birthday']['members']),'Band completion stranded a participant')
            record('four actual clients share band countdown/audio/floor; battery removal selects acoustic playback without restarting siblings; one common completion')
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
            time.sleep(1.5)
        if args.vendor:
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=[4374,4298,4234,4170][i],y=[60,54,39,35][i])
            for client in (b,d):
                if not client.input('inspect')['joystickVisible']:button(client,'Tap to walk')
            time.sleep(1.5)
            with ThreadPoolExecutor(max_workers=4) as pool:
                list(pool.map(lambda client:button(client,'Birthday squirrel stall'),clients))
            queue_samples=[]
            def full_queue():
                state=view()['birthday'];queue_samples.append({'clock':state['clock'],'queue':state['vendorQueue'],'served':state['vendorCount'],'serviceAt':state['vendorAt']})
                return state['vendorQueue'] if len(state['vendorQueue'])==4 else None
            try:queue=wait(full_queue,'four players in one squirrel queue',20)
            finally:write(out/'vendor-admission-trace.json',queue_samples)
            quitter=next(c for c in clients if c.profile==queue[-1]);before=view()['birthday']
            button(quitter,'Leave line')
            after=wait(lambda:view()['birthday'] if quitter.profile not in view()['birthday']['vendorQueue'] else None,'only one patron leaves')
            if before['vendorCount']==after['vendorCount'] and before['vendorAt']>=0:require(after['vendorAt']==before['vendorAt'],'Leaving the line restarted a sibling service')
            wait(lambda:sum(p['kind']=='food' and p['holder']!='' for p in view()['birthday']['props'])==3,'three patrons served once each',20)
            button(quitter,'Birthday squirrel stall')
            wait(lambda:sum(p['kind']=='food' and p['holder']!='' for p in view()['birthday']['props'])==4,'fourth patron returns to the same stall',20)
            require(view()['birthday']['vendorCount']==4 and not view()['birthday']['vendorQueue'],'Shared queue duplicated service or left reservations')
            require({p['holder'] for p in view()['birthday']['props'] if p['kind']=='food'}=={c.profile for c in clients},'Food holders are not the four family members')
            wait(lambda:all(m['station']=='' for m in view()['birthday']['members']),'served players walk aside',15)
            home.capture(a,out,'four-player-squirrel-stall')
            for client in clients:button(client,'Eat snack')
            wait(lambda:all(p['location']=='stock' and p['holder']=='' for p in view()['birthday']['props'] if p['kind']=='food'),'stall food consumed',15)
            for client in (b,d):
                if client.input('inspect')['joystickVisible']:button(client,'Tap to walk')
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
            time.sleep(1.5)
            record('four actual clients share one animated squirrel queue; independent departure preserves service and return serves exactly one portion per patron')
        if args.party:
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=[2315,2325,2475,2485][i],y=200)
            time.sleep(1.5)
            for client in clients:button(client,'Birthday cake table')
            button(a,'Cake base 1')
            for i,client in enumerate(clients):
                button(client,'Cake sticker '+str([0,10,16,23][i]))
                control=next(c for c in client.input('inspect')['controls'] if c['name']=='Cake decorating surface')
                bounds=control['bounds'];x=bounds['x']+bounds['width']*(.3+i*.13);y=bounds['y']+bounds['height']*.5
                client.input('touch-begin',role='screen',x=x,y=y,finger=93);client.input('touch-end',role='screen',x=x,y=y,finger=93)
                wait(lambda:len(view()['birthday']['stickers'])==i+1,'shared cake decoration '+str(i),15)
                home.ready(client)
            button(d,'Cake is ready!')
            wait(lambda:view()['birthday']['cakeReady'],'shared cake finished')
            button(a,'Celebrate');wait(lambda:view()['birthday']['celebrations']==1,'one shared celebration')
            home.capture(a,out,'four-player-shared-cake')
            for client in clients:button(client,'Have some cake')
            wait(lambda:sum(p['kind']=='food' and p['holder']!='' for p in view()['birthday']['props'])==4,'four portions of the same cake')
            for client in clients:button(client,'Eat snack')
            wait(lambda:all(p['holder']=='' and p['location']=='stock' for p in view()['birthday']['props'] if p['kind']=='food'),'four snacks consumed',15)
            require(len(view()['birthday']['stickers'])==4 and view()['birthday']['cakeReady'],'Snack consumption erased shared cake')
            button(a,'Birthday cake table');button(a,'Cold drink')
            wait(lambda:any(p['kind']=='drink' and p['holder']==a.profile for p in view()['birthday']['props']),'cold drink served')
            button(a,'Eat snack')
            wait(lambda:all(p['holder']=='' for p in view()['birthday']['props'] if p['kind']=='drink'),'drink consumed',15)
            # This gift claim is fixture setup; opening/consumption below uses
            # real UI. The entrance slice separately checks real gift pickup.
            for client in clients:command(client,actions.index('Move'),x=650,y=160)
            time.sleep(1.5)
            command(b,birthday,value='claim',item='birthday-gift-1')
            wait(lambda:'birthday-gift-1='+b.profile in b.input('inspect')['birthdayPropOwners'],'gift rendered before opening')
            button(b,'Open present with Hulk')
            wait(lambda:view()['birthday']['giftAt']>=0,'Hulk begins one shared unwrap')
            wait(lambda:any(p.startswith(a.profile+'=unwrap') for p in a.input('inspect')['birthdayPoses']),'Hulk unwrap animation')
            home.capture(a,out,'hulk-unwrapping-shared-gift')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-1')['opened'],'gift revealed',15)
            button(a,'Eat snack')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-1')['location']=='stock','Hulk eats revealed cake',15)
            command(b,actions.index('Move'),x=710,y=160)
            command(b,birthday,value='claim',item='birthday-gift-2')
            button(b,'Open present with Hulk')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-2')['opened'],'second present reveals boulder',15)
            require(next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-2')['content']==1,'Second source outcome is not boulder')
            button(a,'Wrap again');button(a,'Open present with Hulk')
            wait(lambda:next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-2')['opened'],'same present reopens as flower',15)
            require(next(p for p in view()['birthday']['props'] if p['id']=='birthday-gift-2')['content']==2 and view()['birthday']['reaction']=='negative','Flower did not produce the source negative reaction')
            home.capture(a,out,'hulk-reacts-to-flower')
            button(a,'Wrap again')
            button(a,'Open present with Hulk')
            wait(lambda:view()['birthday']['giftAt']>=0,'interruptible unwrap')
            command(a,actions.index('Move'),x=680,y=160)
            wait(lambda:view()['birthday']['giftAt']<0,'movement cancels only own unwrap')
            command(a,birthday,value='drop',item='birthday-gift-2',x=720,y=160)
            for i,client in enumerate(clients):command(client,actions.index('Move'),x=450 if i==0 else 650+i*50,y=160+i*20)
            time.sleep(1.5)
            record('four clients decorate/share one cake; portions/drink consume once; Hulk unwraps cake/boulder/flower, repeats the same gift and safely cancels an unwrap')
        if args.guests:
            intro=wait(lambda:view()['birthday'] if view()['birthday']['introAt']>=0 else None,'shared intro begins',15)['introAt']
            command(a,actions.index('Move'),x=2400,y=200)
            wait(lambda:view()['birthday']['introDone'] and sum(p['location']=='table' for p in view()['birthday']['props'])==2,'two source guests deliver their gifts',20)
            for client in clients:
                require(client.input('inspect')['birthday']['introAt']==intro,'Clients restarted the shared intro')
            home.capture(a,out,'two-source-guests-deliver-presents')
            command(c,actions.index('Travel'),value='garden')
            wait(lambda:any(p.startswith('birthday-companion-2=') for p in a.input('inspect')['birthdayPoses']),'departed role continues as NPC')
            command(c,actions.index('Travel'),value='hulk-birthday');command(c,actions.index('Move'),x=750,y=200)
            require(view()['birthday']['introAt']==intro,'Late return restarted introduction')
            command(a,actions.index('Move'),x=450,y=160)
            # Fixture jumps do not wait for the local camera interpolation. Read
            # tap bounds only after it settles, as a player walking here would.
            time.sleep(1.5)
            record('one shared introduction, two recovered civilian guests, delivered gifts and late-return role continuity')
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
        if args.actions:
            wait(lambda:any(p.startswith(b.profile+'=happy-hold') for p in b.input('inspect').get('birthdayPoses',[])),'receiver reaction with holding layer',10)
        home.capture(b,out,'spidey-receives-gift')
        record('UGUI character handoff transfers ownership across clients')
        # A clear drop pad avoids asking a real touch to select an obscured
        # object beneath the untouched starting gifts.
        if b.input('inspect')['joystickVisible']:b.input('touchButton',text='Tap to walk')
        walk_carry(b,1150,250)
        if args.actions:walk_carry(b,700,80,True)
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
        if args.guests:
            wait(lambda:any(p.startswith('birthday-companion-1=') for p in a.input('inspect')['birthdayPoses']),'recipient remains as one NPC companion')
            a.input('touchButton',text='Birthday friend birthday-companion-1')
            wait(lambda:prop()['holder']=='birthday-companion-1','gift to continuing NPC companion',20)
            rendered(a,'birthday-companion-1')
            a.input('touchButton',text='Birthday friend birthday-companion-1')
            wait(lambda:prop()['holder']==a.profile,'NPC returns gift through real UI',20)
            rendered(a,a.profile)
            wait(lambda:any(p.startswith(a.profile+'=hold') for p in a.input('inspect')['birthdayPoses']),'stable holding pose after pickup transition',10)
            home.capture(a,out,'gift-returned-by-continuing-companion')
            record('disconnected role has one continuing NPC body; NPC give/receive preserves the same gift')
        if args.actions:record('original right/left carry frames advance, mirrored anchors follow and the receiver reacts while retaining the gift')
        home.capture(a,out,'three-siblings-continue-party')
        passed=True
    finally:
        if clients:
            try:
                write(out/'last-authority-state.json',server.state())
                write(out/'last-client-state.json',clients[0].input('inspect'))
            except (RuntimeError,OSError,AssertionError):pass
            for client in clients:
                if client.process.poll() is None:
                    try:write(out/('last-'+client.profile+'.json'),client.input('inspect'))
                    except (RuntimeError,OSError,AssertionError):pass
        write(out/'result.json',{'passed':passed,'checks':checks,'build':args.build,'source_commit':source['sourceCommit'],
            'scope':'Checked birthday slices: shared props'+(' / directional actions' if args.actions else '')+(' / introduction and guests' if args.guests else '')+(' / shared cake and gift replay' if args.party else '')+(' / one shared vendor queue' if args.vendor else '')+(' / shared band, audio and power' if args.band else '')+(' / shared castle physics' if args.castle else '')+(' / brick pegs and bubbles' if args.play else '')+'. Villains and whole-world acceptance remain open.',
            'art_root':str(art),'clients_started':len(clients)})
        run.close()
        if previous is None:os.environ.pop('LITTLE_WEEPS_BIRTHDAY_ART',None)
        else:os.environ['LITTLE_WEEPS_BIRTHDAY_ART']=previous


if __name__=='__main__':
    main()
