# /// script
# dependencies = ["cryptography"]
# ///
"""Real native pointer gestures through contextual Markets pictures and drawers."""
import argparse,hashlib,importlib.util,re,sys,time
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require

def load(name,file):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).with_name(file));module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
home=load('home','Test-HomeWorld.py');controls=load('markets_controls','Test-MarketsControls.py')


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('build',type=int);a=p.parse_args()
    run=Run(a.build,extended_test_lifetime=True,graphics_api='d3d11');out=run.path/'markets-gestures';out.mkdir();checks=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    manifest=read(run.folder/'source-manifest.json')
    require(all(hashlib.sha256((ROOT/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in manifest['files'] if f['path'].startswith('Unity/')),'Source differs from native gesture candidate')
    source=(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text(encoding='utf-8-sig')
    actions={};ordinal=-1
    for declaration in re.search(r'enum SoloAction\s*{([^}]+)}',source).group(1).split(','):
        name,assigned,value=declaration.strip().partition('=')
        ordinal=int(value.strip(),0) if assigned else ordinal+1
        actions[name.strip()]=ordinal
    require(actions['Markets']==39,'Markets wire ordinal must preserve retired Drive slot')
    def command(c,action,**kw):
        v=home.command(c,actions[action],**kw);require(v['accepted'],v['outcome']);home.ready(c)
    def view():return wait(lambda:server.state(),'fresh authoritative state',5)['view']['markets']
    def idle(station):wait(lambda:not next(t for t in view()['stations'] if t['id']==station)['workingActor'],'finished '+station,15)
    def join(c,station,x):
        # Setup places a real player nearby; pictured chooser still initiates
        # its normal approach/join. Gesture actions never use command fixtures.
        command(c,'Move',x=x,y=300);ui[c.profile].tap('Markets visit '+station)
        wait(lambda:any(m['actor']==c.profile and m['station']==station for m in c.input('inspect')['markets']['members']),'joined '+station,25)
    def note(text):checks.append(text);print('PASS '+text,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for c in clients:home.ready(c)
        ui={c.profile:controls.MarketsTouches(c) for c in clients};actor=clients[0]
        home.scenic.NAMES['markets']='World Markets';home.travel(actor,'markets')
        for c in clients[1:]:command(c,'Travel',value='markets')
        ui[actor.profile].tap('Markets open places',acknowledge=False)
        first=ui[actor.profile].control('Markets visit smoothie')['bounds'];second=ui[actor.profile].control('Markets visit flowers')['bounds']
        gap=((first['x']+first['width']+second['x'])/2,first['y']+first['height']/2)
        require(first['x']+first['width']<gap[0]<second['x'],'No real chooser gap available')
        previous=next(p for p in server.state()['view']['players'] if p['id']==actor.profile);previous=(previous['x'],previous['y'])
        receipts=ui[actor.profile].receipt_files()
        actor.input('touch-begin',role='screen',x=gap[0],y=gap[1],finger=68);time.sleep(.1)
        actor.input('touch-end',role='screen',x=gap[0],y=gap[1],finger=68);time.sleep(.8)
        after=next(p for p in server.state()['view']['players'] if p['id']==actor.profile)
        require((after['x'],after['y'])==previous and ui[actor.profile].receipt_files()==receipts,'Chooser gap leaked a walking or authority action')
        require(ui[actor.profile].control('Markets visit smoothie') is not None,'Chooser gap unexpectedly closed modal')
        home.capture(actor,out,'pictured-chooser-modal-gap');ui[actor.profile].tap('Markets close places',acknowledge=False)
        note('Actual pointer tap in pictured chooser gap leaves authoritative position and commands unchanged')
        ui[actor.profile].tap('Markets open family tools',acknowledge=False)
        upper=ui[actor.profile].control('Markets tool Put down')['bounds'];lower=ui[actor.profile].control('Markets tool Give')['bounds']
        gap=(upper['x']+upper['width']/2,(upper['y']+lower['y']+lower['height'])/2)
        require(lower['y']+lower['height']<gap[1]<upper['y'],'No real family drawer gap available')
        receipts=ui[actor.profile].receipt_files()
        actor.input('touch-begin',role='screen',x=gap[0],y=gap[1],finger=69);time.sleep(.1)
        actor.input('touch-end',role='screen',x=gap[0],y=gap[1],finger=69);time.sleep(.8)
        after=next(p for p in server.state()['view']['players'] if p['id']==actor.profile)
        require((after['x'],after['y'])==previous and ui[actor.profile].receipt_files()==receipts,'Family drawer gap leaked a walking or authority action')
        require(ui[actor.profile].control('Markets tool Put down') is not None,'Family gap unexpectedly closed modal')
        ui[actor.profile].tap('Markets open family tools',acknowledge=False)
        note('Actual family drawer gap touch leaves authoritative position and commands unchanged')
        ui[actor.profile].tap('Markets next scene')
        wait(lambda:any(m['actor']==actor.profile and m['station']=='buttermilk' for m in actor.input('inspect')['markets']['members']),'next source scene ponies',25)
        ui[actor.profile].tap('Markets previous scene')
        wait(lambda:any(m['actor']==actor.profile and m['station']=='smoothie' for m in actor.input('inspect')['markets']['members']),'previous scene smoothie',25)
        note('Pictured scene-edge next/previous navigation reaches adjacent stations through real pointer input')
        for c in clients:join(c,'smoothie',550)
        for c,fruit in zip(clients,['apple','banana','carrot','grapes']):
            ui[c.profile].drag('Markets '+fruit,'Markets Blend');idle('smoothie')
        require(view()['stations'][0]['parts']==['apple','banana','carrot','grapes'],'Fruit drag lost shared input order')
        ui[actor.profile].tap('Markets Blend');wait(lambda:any(i['kind']=='smoothie' for i in view()['items']),'finished drag-prepared shared smoothie')
        home.capture(actor,out,'fruit-drag-result');note('Four real fruit-to-blender drags produce one common smoothie')
        for c in clients:join(c,'buttermilk',3300)
        for c,job in zip(clients,['Food','Water','Brush','Clean']):
            ui[c.profile].drag('Markets '+job,'Markets pony buttermilk')
            if job=='Food':home.capture(c,out,'food-drag-contact')
            idle('buttermilk')
        require(next(p for p in view()['ponies'] if p['id']=='buttermilk')['needs']==15,'Pony gesture care incomplete')
        note('Four native tool-to-pony drags finish one shared care loop after contact')
        # Deliberately bypass the test helper's cleanup here: production must
        # make its drawers exclusive without an explicit Style close gesture.
        actor.input('touchButton',text='Markets open makeover')
        wait(lambda:ui[actor.profile].control('Markets close makeover'),'Style drawer open')
        actor.input('touchButton',text='Markets open places')
        wait(lambda:ui[actor.profile].control('Markets visit biscuit'),'Places replaces Style')
        require(ui[actor.profile].control('Markets close makeover') is None,'Opening Places left Style modal active')
        ui[actor.profile].tap('Markets visit biscuit')
        wait(lambda:any(m['actor']==actor.profile and m['station']=='biscuit' for m in view()['members']),'Style to Places Biscuit navigation',25)
        note('Style to Places to Biscuit works through native touches with no manual Style close')
        join(actor,'buttermilk',3300)
        before=next(p for p in view()['ponies'] if p['id']=='buttermilk').copy()
        ui[actor.profile].drag('Markets Water','Markets pony biscuit')
        # A cross-pony drop first approaches/joins its recipient. Its first
        # accepted receipt can be that join: durable care is the success check.
        wait(lambda:next(p for p in view()['ponies'] if p['id']=='biscuit')['needs']==2,'cross-pony water recipient completes after queued approach',35)
        require(next(p for p in view()['ponies'] if p['id']=='buttermilk')==before,'Biscuit drop incorrectly credited selected Buttermilk')
        require(any(m['actor']==actor.profile and m['station']=='biscuit' for m in view()['members']),'Cross-pony gesture did not join actual recipient')
        idle('biscuit');note('Water dropped onto Biscuit from Buttermilk selection credits only Biscuit after approach and contact')
        for c in clients:join(c,'music',2700)
        ui[actor.profile].tap('Markets Play');before=view()['guitar'];ui[actor.profile].swipe('Markets Guitar string 0')
        wait(lambda:view()['guitar']>before,'guitar string stroke');require(view()['guitar']==before+1,'One guitar stroke triggered multiple operations')
        for c in clients:ui[c.profile].tap('Markets Drums')
        require(view()['playing'] and view()['layers']>=2,'Shared source rhythm not active')
        home.capture(actor,out,'guitar-string-drag');note('One real guitar-string stroke produces one pluck while family rhythm layers continue')
        for c in clients:join(c,'icecream',4200)
        # Exercise the pictured scroll controls, then confirm the last source
        # brick has an actual clipped native hitbox inside the palette arrows.
        for _ in range(3):ui[actor.profile].tap('Markets palette next',acknowledge=False);time.sleep(.15)
        last=wait(lambda:ui[actor.profile].control('Markets choose brick 13'),'last recovered brick visible after palette scroll')
        left=ui[actor.profile].control('Markets palette previous')['bounds'];right=ui[actor.profile].control('Markets palette next')['bounds'];r=last['bounds']
        require(r['width']>20 and r['x']>=left['x']+left['width'] and r['x']+r['width']<=right['x'],'Last brick hitbox is clipped or outside pictured palette')
        home.capture(actor,out,'palette-last-source')
        for _ in range(3):ui[actor.profile].tap('Markets palette previous',acknowledge=False);time.sleep(.15)
        require(ui[actor.profile].control('Markets choose brick 0') is not None,'Palette did not return to first recovered brick')
        note('Actual pictured palette scrolling exposes the fourteenth source brick inside its viewport and returns to the first')
        for i,c in enumerate(clients):
            ui[c.profile].drag('Markets choose brick '+str(i),'Markets build slot '+str(i))
            wait(lambda:len(view()['bricks'])==i+1,'shared brick drop '+str(i))
        require(len({b['creator'] for b in view()['bricks']})==4 and {b['slot'] for b in view()['bricks']}==set(range(4)),'Board drags did not retain four creators/slots')
        ids=[b['id'] for b in view()['bricks']];home.capture(actor,out,'four-creator-brick-drag')
        ui[clients[-1].profile].tap('Markets tool Leave')
        require([b['id'] for b in view()['bricks']]==ids and len(view()['stations'][-1]['queue'])==3,'Drawer departure damaged siblings creation')
        note('Four pictured brick-to-board drags create one shared construction; drawer departure preserves siblings')
        passed=True
    finally:
        run.close();errors=[line for v in run.instances for line in (v.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=a.build,checks=checks,runtimeErrors=errors,actualPointerGestures=True,physicalDevices=False,installedFamilyTouched=False,ownerAcceptance=False))
        print('RESULT '+str(out/'result.json'),flush=True);require(not errors,'Native gesture runtime exception')


if __name__=='__main__':main()
