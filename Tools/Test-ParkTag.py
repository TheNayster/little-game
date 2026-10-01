# /// script
# dependencies = ["cryptography"]
# ///
"""One focused four-player tag check; isolated authority, never the live family."""
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build);folder=run.path/'park-tag';folder.mkdir();checks=[];clients=[];passed=False
    print('EVIDENCE '+str(folder),flush=True)
    def record(s):checks.append(s);print('PASS '+s,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',v['profile']) for v in run.slots];a,b,c,d=clients
        def tag():return server.state()['view']['park']['tag']
        def player(client):return next(v for v in server.state()['view']['players'] if v['id']==client.profile)
        def move(client,x,y=70):require(home.command(client,0,x=x,y=y)['accepted'],'fixture placement');home.ready(client);time.sleep(.3)
        for client in clients:require(home.command(client,7,value='creek' if client==d else 'park')['accepted'],'area arrival');home.ready(client)
        for client,x in zip(clients,[1800,2150,2500,700]):move(client,x)
        a.input('resize',x=1024,y=768);home.ready(a)
        a.input('touchButton',text='Games');home.capture(a,folder,'park-tag-menu-ipad');a.input('touchButton',text='Tag')
        wait(lambda:tag()['phase']==1,'common countdown');deadline=tag()['starts'];require(not a.input('inspect')['menuOpen'],'menu remained open')
        wait(lambda:len(tag()['members'])==3,'one start includes all park players')
        require(not any(v['actor']==d.profile for v in tag()['members']),'creek player included')
        for client in [a,b,c]:
            evidence=client.input('inspect');require(not evidence['tagNpcVisible'],'Bandit replaced human group');require(not evidence['tagCue'].startswith('tag-'),'Tag voice still plays')
        wait(lambda:tag()['phase']==2,'common playing phase');time.sleep(2.7)
        require(tag()['it']==a.profile,'initial human tagger')
        record('one picture-card start enrolls every connected park player; other area unaffected; no NPC or Tag speech')
        require(home.command(d,7,value='park')['accepted'],'fourth enters park');home.ready(d)
        wait(lambda:len(tag()['members'])==4,'fourth automatically joins');require(tag()['starts']==deadline,'arrival restarted game')
        move(d,2900)
        home.capture(a,folder,'four-player-tag-ipad')
        # Native tap-to-walk, not a tag award sent by the client.
        a.input('touch-begin',x=2040,y=60,finger=39);a.input('touch-end',x=2040,y=60,finger=39)
        wait(lambda:tag()['it']==b.profile,'tap movement automatic tag',seconds=8)
        turns=tag()['turns'];time.sleep(.6);require(tag()['it']==b.profile and tag()['turns']==turns,'instant tag-back')
        if not b.input('inspect')['joystickVisible']:b.input('touchButton',text='Tap to walk')
        before=player(b)['x'];b.input('touch-begin',role='stick',x=45,finger=40);time.sleep(.4);b.input('touch-end',role='stick',finger=40)
        require(player(b)['x']>before+30,'joystick stopped during tag')
        require(any(v['actor']==b.profile for v in tag()['members']),'movement removed participant')
        require(home.command(b,1,value='orange-pup')['accepted'],'character switch');require(any(v['actor']==b.profile for v in tag()['members']),'avatar switch removed participant')
        record('fourth auto-arrival preserves countdown; human contact swaps roles with grace; joystick and avatar switch work')
        a.input('resize',x=1280,y=591);home.ready(a);home.capture(a,folder,'tag-phone')
        b.close();wait(lambda:len(tag()['members'])==3,'tagger departure');require(tag()['it']!=b.profile,'departed player stayed it')
        require(home.command(d,7,value='creek')['accepted'],'independent creek departure');home.ready(d);wait(lambda:len(tag()['members'])==2,'area departure')
        # The same UGUI button changes its label to All done; its object name stays Join tag.
        c.input('touchButton',text='Join tag');wait(lambda:len(tag()['members'])==1,'personal all done');time.sleep(.5);require(len(tag()['members'])==1 and tag()['phase']==2,'All done immediately rejoined or reset siblings')
        home.capture(a,folder,'bandit-playmate-phone')
        audio=a.input('inspect');require(audio['tagNpcVisible'],'single participant fallback absent');require(not audio['tagCue'].startswith('tag-') and 'Listen' not in audio['visibleText'],'Tag narration remained')
        a.input('touchButton',text='Join tag');wait(lambda:tag()['phase']==0,'last exit idle')
        record('phone/tablet controls; tagger disconnect, area departure and All done preserve siblings; Bandit only for one player; no Tag narration')
        passed=True
    finally:
        if not passed:
            for client in clients:
                if client.process.poll() is None:
                    try:home.capture(client,folder,'failure-'+client.profile)
                    except Exception as exc:print('Capture unavailable: '+str(exc),flush=True)
        write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)
if __name__=='__main__':main()
