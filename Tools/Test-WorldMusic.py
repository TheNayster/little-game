"""Native release acceptance: local music, four travelers, reading and lifecycle."""
import argparse, importlib.util, json, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'world-music';out.mkdir();checks=[];samples=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    def info(v):
        s=v.input('inspect');require(s['worldMusicClipCount']<=2,'More than two music clips');return s
    def button(v,name):v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
    def cmd(v,action,**kw):
        require(home.command(v,action,**kw)['accepted'],'Rejected music fixture command');home.ready(v)
        time.sleep(.8) # Allow the camera to settle before physical screen taps.
    def music(v,track):
        s=wait(lambda:(s if (s:=info(v))['worldMusicTrack']==track and s['worldMusicPlaying'] and s['worldMusicVolume']>.25 else None),'audible '+track,20)
        first=s['worldMusicSample'];time.sleep(.4);s=info(v);require(s['worldMusicSample']!=first,'Audio cursor stalled')
        signal=wait(lambda:(s if (s:=info(v))['worldMusicSignal']>.00001 else None),'nonzero decoded music '+track,10)
        samples.append({k:signal[k] for k in signal if k.startswith('worldMusic')})
        return s
    try:
        run.start('server');clients=[run.start('client',slot['profile']) for slot in run.slots];a,b,c,d=clients
        for v in clients:home.ready(v)
        for v,track in zip(clients,['home','park','creek','beach']):
            if track=='home':cmd(v,0,x=-4500,y=200)
            else:cmd(v,7,value=track)
            music(v,track)
        require([info(v)['worldMusicTrack'] for v in clients]==['home','park','creek','beach'],'Shared travelers changed local themes')
        cmd(b,7,value='daycare');music(b,'daycare');cmd(a,0,x=1500,y=200);music(a,'yard')
        record('six themes decode and advance in release players; four destinations remain independent')
        button(a,'Menu');button(a,'Music setting');require(info(a)['musicMuted'] and info(a)['worldMusicVolume']==0,'Music mute failed')
        require(info(b)['worldMusicPlaying'] and info(c)['worldMusicPlaying'],'Sibling music muted')
        button(a,'Back to play');cmd(a,7,value='park');time.sleep(3);require(info(a)['worldMusicVolume']==0,'Travel unmuted music')
        a.close();a=run.start('client',run.slots[0]['profile']);home.ready(a);require(info(a)['musicMuted'] and info(a)['worldMusicVolume']==0,'Restart forgot mute')
        button(a,'Menu');button(a,'Music setting');button(a,'Back to play');music(a,'park')
        record('mute is immediate and local and remains off across travel and client restart')
        for track in ['beach','creek','daycare','park','home']:
            cmd(a,7,value=track)
            info(a)
        cmd(a,0,x=-4500,y=200);music(a,'home')
        wait(lambda:info(a)['worldMusicClipCount']==1,'old music clips released')
        button(a,'Read shared books');button(a,'Choose hello-dinosaurs');wait(lambda:info(a)['bookReady'],'reader ready')
        require(not info(a)['bookSpeaking'],'Open autoplays narration');button(a,'Read to me')
        wait(lambda:info(a)['bookSpeaking'],'explicit narration')
        wait(lambda:info(a)['worldMusicVolume']<.12,'music ducked under narration',5)
        require(info(c)['worldMusicVolume']>.25,'Reading ducked sibling music')
        button(a,'Close');music(a,'home')
        record('rapid travel releases old clips; deliberate book narration ducks only its local music')
        cmd(a,0,x=-3480,y=150);button(a,'Radio living power')
        wait(lambda:info(a)['homeMusicPlaying'] and info(a)['worldMusicVolume']<.06,'radio replaces foreground score')
        button(a,'Radio living power');music(a,'home')
        a.input('application-pause');time.sleep(.2);s=info(a);require(not s['worldMusicPlaying'],'Paused music still playing');sample=s['worldMusicSample']
        time.sleep(.4);require(info(a)['worldMusicSample']==sample,'Paused audio cursor advanced')
        require(info(c)['worldMusicPlaying'],'Pause stopped sibling')
        a.input('application-resume');music(a,'home')
        record('radio keeps its original dance music; background pause stops audio and resume preserves playback')
        cmd(a,0,x=-2980,y=420);button(a,'Stair entry');wait(lambda:info(a)['zone']=='home-upstairs','upstairs')
        cmd(a,0,x=710,y=420);button(a,'Enter bedroom 1');wait(lambda:info(a)['zone']=='home-bedroom-1','bedroom')
        music(a,'home');cmd(a,0,x=2310,y=220);time.sleep(.8);button(a,'Secret star door');wait(lambda:info(a)['zone']=='home-secret-1','secret')
        wait(lambda:info(a)['worldMusicVolume']==0,'ordinary music yields to quiet room')
        require(info(a)['quietMusicLevel']==1 and info(c)['worldMusicPlaying'],'Quiet preference or sibling changed')
        button(a,'Bedroom ←');wait(lambda:info(a)['zone']=='home-bedroom-1','bedroom return');music(a,'home')
        record('upstairs and bedrooms retain a softer Home score; secret room keeps its separate quiet ambience')
        for log in run.path.rglob('player.log'):require('Exception:' not in log.read_text(errors='replace'),'Native exception '+str(log))
        passed=True
    finally:
        run.close();write(out/'summary.json',dict(build=args.build,passed=passed,checks=checks,audio=samples))
    require(passed,'Music qualification incomplete')

if __name__=='__main__':main()
