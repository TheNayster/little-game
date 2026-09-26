# /// script
# dependencies = ["cryptography"]
# ///
"""Four native clients, real furniture controls and drag/drop in a disposable world."""
import argparse
from copy import deepcopy
import importlib.util
from pathlib import Path
import time
from shared_garden_runtime import Run,wait,require,write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
HALL_X=[710,1132,1601,2066]

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'furniture';folder.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots]
        for c in clients:home.ready(c)
        a,b,c,d=clients
        def world():return server.state()['view']
        def player(v):return next(p for p in world()['players'] if p['id']==v.profile)
        def roomstate(index=0):return world()['bedrooms'][index]
        def toy(id):return next(t for t in world()['toys'] if t['id']==id)
        def move(v,x,y=420):
            wait(lambda:next(p for p in v.state()['view']['players'] if p['id']==v.profile)['zone']==player(v)['zone'] and player(v)['stairs']==0,'arrival snapshot')
            result=home.command(v,0,x=x,y=y);require(result['accepted'],'Fixture positioning rejected: '+str(result));time.sleep(.5);home.ready(v)
        def room(v,index):
            move(v,HALL_X[index]);v.input('touchButton',text='Enter bedroom '+str(index+1))
            wait(lambda:player(v)['zone']=='home-bedroom-'+str(index+1) and v.input('inspect')['zone']=='home-bedroom-'+str(index+1),'bedroom door arrival');home.ready(v)
        def exitroom(v):
            move(v,325);v.input('touchButton',text='Return to hallway')
            wait(lambda:player(v)['zone']=='home-upstairs' and v.input('inspect')['zone']=='home-upstairs','bedroom exit');home.ready(v)
        def edit(v,target,value):
            return home.command(v,14,target=target,value=str(value)+':'+str(next(r for r in world()['bedrooms'] if r['id']==player(v)['zone'])['roomRevision']))
        def button(v,label):v.input('touchButton',text=label);time.sleep(.3);home.ready(v)
        def drag(v,item,x,y,finger):
            v.input('touch-begin',role=item,finger=finger)
            wait(lambda:toy(item)['holder']==v.profile,'real personal toy pickup')
            v.input('touch-move',x=x,y=y,finger=finger);v.input('touch-end',x=x,y=y,finger=finger)
            wait(lambda:toy(item)['holder']=='','real personal toy drop');home.ready(v)
        owners=[r['owner'] for r in world()['bedrooms']]
        require(len(world()['toys'])==27,'Missing bounded personal stock')
        for i,v in enumerate(clients):
            move(v,-2980);button(v,'Stair entry');wait(lambda:player(v)['zone']=='home-upstairs' and player(v)['stairs']==0 and v.input('inspect')['zone']=='home-upstairs','upstairs')
            room(v,i);move(v,1200,180);button(v,'Rest on bed')
            wait(lambda:player(v)['fixture']=='bedroom-bed','bed entry')
            wait(lambda:v.input('inspect')['homePose']=='Rest','Bed rest pose')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'bedroom-bed-phone')
        a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,folder,'bedroom-bed-tablet')
        record('four independent real bed buttons and rest poses; bounded stock and phone/tablet compositions')
        for v in (b,c,d):exitroom(v);room(v,0)
        for i,v in enumerate(clients):button(v,'Sit on cushion '+str(i+1));wait(lambda:player(v)['fixture']=='bedroom-cushion-'+str(i),'cushion entry')
        require(not home.command(a,8,target='bedroom-cushion-1')['accepted'],'Occupied cushion stolen')
        a.input('touch-begin',role='screen',x=700,y=650,finger=41)
        a.input('touch-move',role='screen',x=535,y=650,finger=41);a.input('touch-end',role='screen',x=535,y=650,finger=41)
        time.sleep(.4);home.capture(a,folder,'four-cushions-tablet')
        # Resume follow camera through the actual movement control after the
        # deliberate inspection pan, before fixture-positioning at the exit.
        button(a,'Tap to walk');a.input('touch-begin',role='stick',finger=42);a.input('touch-end',role='stick',finger=42);button(a,'Tap to walk')
        guests=[(player(v)['fixture'],player(v)['x'],player(v)['y']) for v in (b,c,d)]
        exitroom(a);require([(player(v)['fixture'],player(v)['x'],player(v)['y']) for v in (b,c,d)]==guests,'Owner exit interrupted guests');room(a,0)
        record('four simultaneous cushion occupants; competing seat refuses and owner departure preserves three guests')
        move(a,1800,180);button(a,'Open toy chest');require(roomstate()['chestOpen'],'Chest did not open')
        item='home-bedroom-1-toy-0'
        drag(a,item,1545,300+150/.45,51)
        require(toy(item)['container']=='home-bedroom-1/chest-0','Visual chest drop did not bind real container')
        home.capture(a,folder,'chest-open-stored-tablet')
        button(a,'Open toy chest');require(not roomstate()['chestOpen'],'Chest did not close')
        require(not home.command(a,2,item=item)['accepted'],'Closed chest item was reachable')
        require(item not in a.input('inspect')['homeDrawOrder'],'Closed chest renders contents')
        button(a,'Open toy chest');drag(a,item,1900,360+77/.45,52)
        require(toy(item)['container']=='home-bedroom-1/shelf-0','Visual shelf drop did not bind real support')
        button(a,'Bedroom lamp');require(roomstate()['lampOn'],'Lamp did not turn on')
        home.capture(a,folder,'shelf-and-lamp-tablet')
        record('real plush drag enters chest; close hides and protects it; reopen/retrieve onto shelf keeps exact identity; lamp switches')
        require(not edit(b,'theme',1)['accepted'],'Visitor changed owner decoration')
        button(a,'Decorate');button(a,'Colours');require(roomstate()['theme']==1,'Colour control failed')
        button(a,'Arrange');require(roomstate()['layout']==1,'Arrangement control failed')
        require(player(b)['fixture']=='bedroom-cushion-1' and player(b)['x']==1350,'Layout abandoned seated visitor')
        require(toy(item)['x']==790 and toy(item)['container'].endswith('shelf-0'),'Layout abandoned stored item')
        home.capture(a,folder,'decorated-layout-tablet')
        button(a,'Undo');require(roomstate()['layout']==0 and roomstate()['theme']==1,'Undo replaced unrelated decoration')
        button(a,'Together: off');require(roomstate()['decorateTogether'],'Together control failed')
        button(a,'Decorate');move(b,1200,180);button(b,'Decorate');button(b,'Colours')
        require(roomstate()['theme']==2,'Together permission did not enable visitor')
        require(not edit(a,'undo',0)['accepted'],'Owner undid another decorator operation')
        button(b,'Undo');require(roomstate()['theme']==1,'Visitor inverse failed');button(b,'Decorate')
        record('owner-only decoration, optional together permission, reversible arrangements and actor-scoped undo keep visitors and items')
        move(a,1800,180);drag(a,item,1400,180,53)
        button(a,'Decorate');button(a,'Put toys away');require(toy(item)['container'].endswith('chest-0'),'Tidy failed')
        button(a,'Decorate');a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'furnished-room-phone')
        require([r['owner'] for r in world()['bedrooms']]==owners and len(world()['toys'])==27,'Furnishing changed owners or duplicated stock')
        record('owner puts bounded loose toys away; owners and stock identities survive all visits and edits')
        passed=True
    except Exception:
        for i,v in enumerate(clients):
            if v.process.poll() is None:
                try:home.capture(v,folder,'failure-client-'+str(i))
                except Exception:pass
        raise
    finally:
        run.close();write(folder/'result.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTouched=False))
        print('RESULT '+str(folder/'result.json'),flush=True)

if __name__=='__main__':main()
