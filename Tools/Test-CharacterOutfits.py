# /// script
# dependencies = ["cryptography"]
# ///
"""One focused native wardrobe/roar pass on an isolated four-player release."""
import argparse
import importlib.util
from pathlib import Path
import time
from shared_garden_runtime import Run,wait,require,write

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build);folder=run.path/'outfits';folder.mkdir();checks=[]
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a=clients[0]
        for c in clients:home.ready(c)
        def player(c):return next(p for p in server.state()['view']['players'] if p['id']==c.profile)
        def settled(c):return wait(lambda:(s if not (s:=c.input('inspect'))['pending'] else None),'outfit settled')
        for width,height,label in [(1280,591,'phone'),(1024,768,'ipad')]:
            a.input('resize',x=width,y=height);home.ready(a)
            a.input('touchButton',text='Characters');before=dict(player(a));toys=server.state()['view']['toys']
            a.input('touchButton',text='Bluey');s=settled(a)
            require(s['outfitsOpen'] and dict(player(a))==before and server.state()['view']['toys']==toys,'Repeated selection must only open wardrobe')
            a.input('touchButton',text='Dinosaur');wait(lambda:player(a)['outfit']=='dinosaur','dinosaur equipped')
            wait(lambda:any(c['name']=='Pink' and c['enabled'] for c in a.input('inspect')['controls']),'color choices enabled')
            for color in ['Pink','Blue','Green','Red']:
                a.input('touchButton',text=color);wait(lambda:player(a)['outfitColor']==color.lower(),'color '+color)
                s=settled(a);require(s['outfit']=='dinosaur' and s['outfitColor']==color.lower(),'wrong preview state')
                home.capture(a,folder,label+'-'+color.lower())
            s=settled(a);controls=[x for x in s['controls'] if x['name'] in ['Dinosaur','Normal clothes','Pink','Blue','Green','Red','Done']]
            require(len(controls)==7 and all(x['bounds']['width']>45 for x in controls),'Outfit controls clipped')
            a.input('touchButton',text='Done');require(not settled(a)['outfitsOpen'],'Done failed')
            a.input('touchButton',text='Close characters')
            checks.append(label+': repeat tap opens outfit/color window; four colors, live preview and close')
        for avatar in ['blue-pup','orange-pup']:
            require(home.command(a,1,value=avatar)['accepted'],'avatar '+avatar)
            require(home.command(a,23,value='dinosaur',target='green')['accepted'],'outfit '+avatar)
            settled(a);home.capture(a,folder,'character-'+avatar)
        checks.append('Bluey and Bingo render their prepared onesie sheets')
        for i,c in enumerate(clients):
            require(home.command(c,23,value='dinosaur',target=['pink','blue','green','red'][i])['accepted'],'independent colors')
        wait(lambda:all(c.input('inspect')['outfit']=='dinosaur' for c in clients),'four visible outfits')
        a.input('touchButton',text='Roar!');wait(lambda:player(a)['roar']==1,'native roar')
        wait(lambda:all(any(p['id']==a.profile and p['roarPlaying'] for p in c.input('inspect')['players']) for c in clients),'roar audible on four clients')
        require(not home.command(a,24)['accepted'],'cooldown missing')
        home.capture(a,folder,'four-dinosaurs-roar')
        checks.append('native roar plays approved installed T-Rex audio to same-area siblings, with a repeat cooldown')
        clients[0].close();wait(lambda:len(server.state()['connected'])==3,'independent departure')
        require(home.command(clients[1],24)['accepted'],'sibling cannot roar after departure')
        require(home.command(clients[1],23,value='',target='blue')['accepted'],'normal clothes unavailable')
        state=settled(clients[1]);require(state['outfit']=='' and not any(x['name']=='Roar!' for x in state['controls']),'normal outfit kept roar button')
        checks.append('one player exits; siblings keep dressing/roaring; normal outfit removes roar')
        write(folder/'result.json',dict(passed=True,build=args.build,checks=checks,liveFamilyTouched=False))
        print('PASS '+str(folder/'result.json'),flush=True)
    finally:run.close()

if __name__=='__main__':main()
