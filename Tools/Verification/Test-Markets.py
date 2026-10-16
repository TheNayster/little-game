# /// script
# dependencies = ["cryptography"]
# ///
"""BLU-01 isolated native Markets acceptance with shared controls and saved reopening."""
import argparse,hashlib,importlib.util,json,sys,time,uuid
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import ROOT,Run,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
control_spec=importlib.util.spec_from_file_location('markets_controls',Path(__file__).with_name('Test-MarketsControls.py'));controls=importlib.util.module_from_spec(control_spec);control_spec.loader.exec_module(controls)

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);parser.add_argument('--smoke',action='store_true');parser.add_argument('--resume-smoothie');parser.add_argument('--resume-ponies');args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True,graphics_api='d3d11',resume=args.resume_smoothie or args.resume_ponies);out=run.path/('markets-pony-continue' if args.resume_ponies else 'markets-continue' if args.resume_smoothie else 'markets');out=out if not out.exists() else run.path/(out.name+'-retry-'+uuid.uuid4().hex[:8]);out.mkdir();checks=(read(run.path/'markets/result.json')['checks'] if args.resume_smoothie or args.resume_ponies else []);passed=False
    print('EVIDENCE '+str(out),flush=True)
    import re
    source=(ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/Core/Shared/State/GameWorld.cs').read_text(encoding='utf-8-sig')
    actions={};ordinal=-1
    for declaration in re.search(r'enum SoloAction\s*{([^}]+)}',source).group(1).split(','):
        name,assigned,value=declaration.strip().partition('=')
        ordinal=int(value.strip(),0) if assigned else ordinal+1
        actions[name.strip()]=ordinal
    require(actions['Markets']==39,'Markets wire ordinal must preserve retired Drive slot')
    def command(c,action,**kw):
        result=home.command(c,actions[action],**kw);require(result['accepted'],result['outcome']);home.ready(c);return result
    def button(c,name):
        controls.MarketsTouches(c).ensure(name)
        wait(lambda:any(x['name']==name and x['enabled'] for x in c.input('inspect')['controls']),'enabled '+name,20)
        home.ready(c);before=set(c.out.glob('reply-*.json'))
        c.input('touchButton',text=name)
        if name!='Markets close discovery' and not name.startswith('Markets choose brick'):
            reply=wait(lambda:next((v for p in c.out.glob('reply-*.json') if p not in before and (v:=read(p)) and v['outcome']!='stale-revision'),None),'acknowledged '+name,25)
            require(reply['accepted'],name+': '+reply['outcome'])
        home.ready(c);time.sleep(.15)
    def note(name):checks.append(name);print('PASS '+name,flush=True)
    def at(c,station,x):
        command(c,'Move',x=x,y=170);button(c,'Markets visit '+station)
        wait(lambda:any(m['actor']==c.profile and m['station']==station for m in c.input('inspect')['markets']['members']),station+' participation',25);home.ready(c)
    def view():return wait(lambda:server.state(),'fresh authoritative state',5)['view']['markets']
    def idle(station):wait(lambda:next(t for t in view()['stations'] if t['id']==station)['workingActor']=='','finished '+station,15)
    try:
        server=run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots[:1 if args.smoke else 4]]
        for c in clients:home.ready(c)
        a=clients[0]
        if not args.resume_smoothie and not args.resume_ponies:
            for c,avatar in zip(clients,['blue-pup','orange-pup','chilli','bandit']):command(c,'ChangeAvatar',value=avatar)
            home.scenic.NAMES['markets']='World Markets';home.travel(clients[0],'markets')
            for c in clients[1:]:command(c,'Travel',value='markets')
            a=clients[0];wait(lambda:a.input('inspect').get('marketsView')=='smoothie','Markets initial view')
            home.capture(a,out,'first-view');write(out/'initial-inspect.json',a.input('inspect'))
            if args.smoke:passed=True;note('packaged Markets opens through its world card');return
            note('four native clients enter one authoritative Markets world')
            for tool,field in [('Sign','sign'),('Umbrella','umbrella'),('Long dog','longDog')]:
                previous=view()[field];button(a,'Markets tool '+tool);wait(lambda:view()[field]!=previous,'shared '+tool)
                home.capture(a,out,'discovery-'+field);button(a,'Markets close discovery')
            note('sign, parasol and Long Dog controls change authoritative state and show recovered-art responses')
            for c in clients:at(c,'smoothie',550)
            for c,fruit in zip(clients,['apple','banana','carrot','grapes']):button(c,'Markets '+fruit);idle('smoothie')
            require(len(view()['stations'][0]['parts'])==4,'shared smoothie ingredients')
            button(a,'Markets Blend');wait(lambda:any(i['kind']=='smoothie' for i in view()['items']),'one blended smoothie')
            home.capture(a,out,'shared-smoothie');note('four children contribute fruit to one completed smoothie using pictured controls')
        if not args.resume_ponies:
            item=next(i for i in view()['items'] if i['kind']=='smoothie');command(a,'Markets',item=item['id'],value='claim:'+str(item['revision']))
            actor=next(p for p in server.state()['view']['players'] if p['id']==a.profile);command(clients[1],'Move',x=actor['x']+60,y=300);button(a,'Markets tool Give');wait(lambda:any(i['id']==item['id'] and i['holder']==clients[1].profile for i in view()['items']),'shared handoff')
            item=next(i for i in view()['items'] if i['id']==item['id']);command(clients[1],'Markets',item=item['id'],value='consume:'+str(item['revision']));note('single-owner product transfers once and is consumed')
            for c in clients:at(c,'flowers',1100)
            for c,color in zip(clients,['coral','green','lavender','pink']):button(c,'Markets '+color);idle('flowers')
            button(a,'Markets Bouquet');idle('flowers');require(any(i['kind']=='flowers' for i in view()['items']),'shared flowers')
            at(a,'cakes',1600);button(a,'Markets pink cake');idle('cakes');require(any(i['kind']=='cake' for i in view()['items']),'cake finish')
            note('shared flower arrangement and source cupcake service finish and replay')
            for c in clients:at(c,'bubbles',2100)
            for c in clients:button(c,'Markets Round');idle('bubbles')
            wait(lambda:len(view()['bubbles'])>=4,'shared bubbles');bubble=view()['bubbles'][0]['id'];button(a,'Markets pop '+str(bubble));wait(lambda:view()['popped']>=1,'shared pop');home.capture(a,out,'bubbles')
            note('four participants create and pop bubbles in the same field')
            for c in clients:at(c,'music',2700)
            button(a,'Markets Play')
            for c in clients:button(c,'Markets Drums')
            button(a,'Markets Guitar');wait(lambda:view()['guitar']>0 and view()['playing'],'interactive shared guitar');home.capture(a,out,'music')
            command(clients[3],'Travel',value='garden');require(view()['playing'],'departure stopped siblings music');command(clients[3],'Travel',value='markets');at(clients[3],'music',2700)
            note('common layered music/dance accepts late participation and survives independent world departure')
        if args.resume_ponies:
            pony=next(p for p in view()['ponies'] if p['id']=='buttermilk');require(pony['needs']==15 and pony['rounds']==1,'Saved already-checked Buttermilk loop missing')
            note('saved completed Buttermilk loop retained across narrow test continuation')
        for pony,x in ([('biscuit',3700)] if args.resume_ponies else [('buttermilk',3300),('biscuit',3700)]):
            if pony=='buttermilk':
                at(a,pony,x);button(a,'Markets Food');button(a,'Markets tool Leave')
                require(next(p for p in view()['ponies'] if p['id']==pony)['needs']==0,'interrupted care credited before contact')
                note('leaving during pony contact cancels only that uncompleted care contribution')
            for c in clients:at(c,pony,x)
            for c,job in zip(clients,['Food','Water','Brush','Clean']):
                button(c,'Markets '+job)
                if job=='Food':home.capture(c,out,pony+'-feeding-contact')
                idle(pony)
            p=next(p for p in view()['ponies'] if p['id']==pony);require(p['needs']==15 and p['rounds']==1,'shared '+pony+' care')
            button(a,'Markets Body');idle(pony);button(clients[1],'Markets Mane');idle(pony)
        home.capture(a,out,'pony-makeover');note('all four complete both pony care loops and shared source appearance changes')
        for c in clients:at(c,'icecream',4200)
        for i,c in enumerate(clients):button(c,'Markets build slot '+str(i))
        wait(lambda:len(view()['bricks'])==4,'one shared construction');ids=[b['id'] for b in view()['bricks']]
        home.capture(a,out,'shared-icecream');note('four players place source bricks into one durable creation')
        for order in ['Chocolate','Sundae','Mango']:
            station=next(t for t in view()['stations'] if t['id']=='icecream');head=next(c for c in clients if c.profile==station['queue'][0]);button(head,'Markets '+order)
            count={'Chocolate':5,'Sundae':8,'Mango':6}[order]
            for _ in range(count):
                station=next(t for t in view()['stations'] if t['id']=='icecream');head=next(c for c in clients if c.profile==station['queue'][0]);button(head,'Markets Add scoop');idle('icecream')
                if _==0 and order=='Chocolate':home.capture(head,out,'recipe-and-palette')
            require(any(i['kind']=='icecream' and i['choice']==order.lower() for i in view()['items']),'completed '+order+' source blueprint')
        note('all three original ice cream reference stacks complete through shared turns and finish delivery')
        last=clients[-1];last.close();wait(lambda:len(server.state()['connected'])==3,'independent disconnect')
        require([b['id'] for b in view()['bricks']]==ids,'disconnect lost creation');clients[-1]=run.start('client',last.profile);home.ready(clients[-1]);wait(lambda:len(server.state()['connected'])==4,'reconnect');require([b['id'] for b in clients[-1].input('inspect')['markets']['bricks']]==ids,'catch-up lost identity')
        note('disconnect and actual fresh client catch-up preserve siblings and exact creations')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,out,'phone-shaped');a.input('resize',x=1024,y=768);home.ready(a);home.capture(a,out,'tablet-shaped')
        saved=GameSaved=dict(bricks=view()['bricks'],ponies=view()['ponies'],items=view()['items'])
        write(out/'expected-reopen.json',saved);write(ROOT/'LocalData/markets-last-checked-run.json',dict(build=args.build,runId=run.run_id,evidence=str(out),expected=str(out/'expected-reopen.json')))
        passed=True
    finally:
        run.close();errors=[line for instance in run.instances for line in (instance.out/'player.log').read_text(encoding='utf-8',errors='replace').splitlines() if 'Exception:' in line or 'error CS' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,runtimeErrors=errors,physicalDevices=False,installedServerTouched=False));require(not errors,'Native exception; see result')
if __name__=='__main__':main()
