"""Native picture reader: real touch, four independent readers, old bookmarks and audio cancellation."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
TITLES=['hello-dinosaurs','little-bridge','rocket-moon','fairy-garden','princess-star','mermaid-shell']

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--previous',type=int,default=194);args=p.parse_args()
    old=Run(args.previous);run=None;out=old.path/'reader-controls';out.mkdir();checks=[];passed=False;legacy={}
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def info(c):return c.input('inspect')
    def button(c,name):c.input('touchButton',text=name);time.sleep(.12)
    def ready(c):return wait(lambda:info(c)['bookReady'],'reader media',25)
    def openbook(c,title):
        if info(c)['bookOpen']:button(c,'Our books')
        else:button(c,'Read shared books')
        button(c,'Choose '+title);ready(c)
    def quiet(c):
        s=info(c);require(not s['bookSpeaking'] and not s['bookPlaying'] and not s.get('bookEffect',False),'Reader unexpectedly plays audio')
    try:
        old.start('server');v=old.start('client',old.slots[0]['profile']);home.ready(v)
        require(home.command(v,0,x=-4500,y=200)['accepted'],'legacy reader position');time.sleep(.6)
        for title,page in [('hello-dinosaurs',2),('little-bridge',3)]:
            openbook(v,title)
            for _ in range(page):button(v,'>')
            ready(v);button(v,'Read to me');wait(lambda:info(v)['bookSpeaking'],'legacy narration');time.sleep(.6);button(v,'Read to me')
            s=info(v);legacy[title]={'page':s['bookPage'],'sample':s['bookSample']};require(s['bookSample']>0,'Legacy sample absent');button(v,'Close')
        old.close();run=Run(args.build,resume=old.run_id);server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        for v in clients:
            home.ready(v);require(home.command(v,0,x=-4500,y=200)['accepted'],'reader position')
        time.sleep(.6)
        for title,saved in legacy.items():
            openbook(a,title);s=info(a);quiet(a);require(s['bookPage']==saved['page'] and s['bookSample']==saved['sample'],'Existing title bookmark changed');require(s['bookAuto'],'Old explicit auto preference lost')
        record('194 to candidate retains two title page and narration bookmarks exactly and never starts playback on open')
        for i,v in enumerate(clients[1:]):
            openbook(v,'hello-dinosaurs');require(not info(v)['bookAuto'],'New reader defaults to automatic turns');quiet(v)
            for _ in range(i+1):button(v,'>')
            ready(v)
        require([info(v)['bookPage'] for v in [b,c,d]]==[1,2,3],'Four local cursors conflict')
        button(a,'Our books');button(a,'Choose hello-dinosaurs');ready(a);button(a,'Read to me');wait(lambda:info(a)['bookSpeaking'],'primary read')
        button(b,'Read to me');wait(lambda:info(b)['bookSpeaking'],'second read');time.sleep(.5);button(a,'Read to me')
        require('Keep reading' in info(a)['visibleText'] and info(a)['bookSample']>0,'Pause did not preserve sample');require(info(b)['bookPlaying'],'Sibling reader stopped')
        button(c,'Close');require(home.command(c,7,value='park')['accepted'],'Sibling travel');require(info(b)['bookPlaying'] and info(d)['bookPage']==3,'Travel interrupted reader')
        button(b,'Read to me');quiet(b);record('four readers keep independent pages playback and world travel')
        # Effect and name requests take turns with narration. Closing cancels
        # pending ResourceRequests as well as playing clips.
        button(a,'Read to me');wait(lambda:info(a)['bookSpeaking'],'resume')
        button(a,'Hear sound');wait(lambda:info(a)['bookEffect'],'effect starts');require(not info(a)['bookSpeaking'],'Narration overlaps effect')
        wait(lambda:info(a)['bookSpeaking'] and not info(a)['bookEffect'],'narration resumes after effect',15)
        button(a,'Read to me');quiet(a);button(a,'<');button(a,'<');ready(a)
        button(a,'Animal choice 0');button(a,'>');ready(a);time.sleep(.5);quiet(a)
        button(a,'Hear sound');button(a,'Close');time.sleep(.7);s=info(a);require(not s['bookOpen'] and not s['bookSpeaking'] and not s['bookEffect'] and s['bookTextures']==0 and s['bookAudio']==0,'Closed reader retained or restarted media')
        record('name page changes and effect close cancel stale audio; narration and effects do not compete')
        openbook(a,'hello-dinosaurs');button(a,'More reading options');button(a,'Auto pages on');require(not info(a)['bookAuto'],'Auto preference not disabled');button(a,'Reading options done')
        while info(a)['bookPage']>0:button(a,'<')
        ready(a);button(a,'Read to me');wait(lambda:info(a)['bookSpeaking'],'cover narration');wait(lambda:not info(a)['bookPlaying'],'manual narration completes',30);require(info(a)['bookPage']==0,'Manual mode advanced')
        button(a,'More reading options');button(a,'Auto pages on');button(a,'Reading options done');button(a,'Start again');wait(lambda:info(a)['bookPage']==1,'auto page turns after real narration',30);button(a,'Read to me');quiet(a)
        record('manual mode stays on page and optional automatic mode follows actual narration completion')
        # Exercise every installed page. Disabled arrows must not wrap.
        pages=0
        for title in TITLES:
            openbook(a,title)
            while info(a)['bookPage']>0:button(a,'<')
            button(a,'<');require(info(a)['bookPage']==0,'First page wrapped')
            count=14 if title=='hello-dinosaurs' else 8
            for page in range(count):
                ready(a);s=info(a);require(s['bookPage']==page,'Wrong page');require(s['bookTextures']==(2 if title=='hello-dinosaurs' else 1),'Unexpected texture budget');quiet(a);pages+=1
                if page<count-1:button(a,'>')
            button(a,'>');require(info(a)['bookPage']==count-1,'Last page wrapped')
            button(a,'Read to me');wait(lambda:info(a)['bookSpeaking'],'all-title audio '+title);button(a,'Read to me');quiet(a)
        record(f'all six installed titles and {pages} pages load; bounded textures, explicit speech and nonwrapping arrows')
        openbook(a,'little-bridge');button(a,'<');button(a,'<');ready(a)
        for width,height,label in [(1024,768,'tablet'),(1280,591,'phone')]:
            a.input('resize',x=width,y=height);time.sleep(.4);s=info(a)
            controls={v['name']:v for v in s['controls']}
            for name in ['Our books','More reading options','Close','<','>','Read to me','Start again','Hear sound']:
                r=controls[name]['bounds'];require(r['x']>=0 and r['y']>=0 and r['x']+r['width']<=s['screenWidth']+1 and r['y']+r['height']<=s['screenHeight']+1,'Control outside screen '+name);require(r['width']>=44 and r['height']>=44,'Tiny control '+name)
            home.capture(a,out,'story-'+label);button(a,'More reading options');home.capture(a,out,'options-'+label);button(a,'Reading options done')
        button(a,'Our books');button(a,'Choose hello-dinosaurs');ready(a)
        while info(a)['bookPage']>0:button(a,'<')
        home.capture(a,out,'dinosaurs-phone');a.input('resize',x=1024,y=768);home.capture(a,out,'dinosaurs-tablet')
        record('phone and tablet controls remain inside screen with at least 44px targets; native screenshots captured')
        button(a,'More reading options');button(a,'Show words');require(not info(a)['bookWords'],'Words toggle failed');button(a,'Sounds on');button(a,'Voice on');s=info(a);require('Voice off' in s['visibleText'] and 'Sounds off' in s['visibleText'],'Options not visible');home.capture(a,out,'preferences');button(a,'Reading options done')
        # Reopening in a new process must restore preferences without resuming.
        expected=(info(a)['bookPage'],info(a)['bookSample']);a.close();a=run.start('client',a.profile);home.ready(a);openbook(a,'hello-dinosaurs');quiet(a);s=info(a);require((s['bookPage'],s['bookSample'])==expected and not s['bookWords'],'Cold reopen lost state');button(a,'More reading options');require('Voice off' in info(a)['visibleText'] and 'Sounds off' in info(a)['visibleText'],'Cold reopen lost sound settings');button(a,'Voice on');button(a,'Sounds on');button(a,'Reading options done')
        button(a,'Read to me');wait(lambda:info(a)['bookSpeaking'],'speech before lifecycle');a.input('network-pause');a.input('network-resume');home.ready(a);require(info(a)['bookPlaying'],'Network-only interruption stopped local reading');a.input('application-pause');a.input('application-resume');home.ready(a);time.sleep(.6);quiet(a)
        record('words voice effects and bookmark survive process restart; network-only changes preserve reading and simulated application lifecycle return stays quiet')
        run.close()
        for instance in run.instances:
            log=(instance.out/'player.log').read_text(errors='replace');require('Exception:' not in log and 'send queue full' not in log,'Native exception or queue warning')
        record('all candidate clients and authority close without reader cleanup exceptions or queue warnings')
        passed=True
    finally:
        if run:run.close()
        old.close();write(out/'results.json',dict(passed=passed,build=args.build,previous=args.previous,checks=checks,legacyBookmarks=legacy,liveFamilyTouched=False,physicalDevicesTested=False,physicalAudioVerified=False));print('RESULT '+str(out/'results.json'),flush=True)
if __name__=='__main__':main()
