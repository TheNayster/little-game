"""Four independent drags and six local readers in an isolated release family."""
import argparse, concurrent.futures, importlib.util, math, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, read, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    p=argparse.ArgumentParser();p.add_argument('build',type=int);p.add_argument('--seconds',type=int,default=180);args=p.parse_args()
    require(30<=args.seconds<=600,'Bounded stress duration required')
    run=Run(args.build);out=run.path/'shared-play-regression';out.mkdir();passed=False;checks=[];metrics={}
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        def cmd(c,action,**kw):
            r=home.command(c,action,**kw);require(r['accepted'],r['outcome']);return r
        for c in clients:home.ready(c)
        # Audio playback is observed with test volumes muted. This verifies
        # clip loading/cursor/lifecycle, not the physical iPad speaker route.
        a=clients[0];cmd(a,0,x=-4500,y=200);time.sleep(.8)
        titles=['hello-dinosaurs','little-bridge','rocket-moon','fairy-garden','princess-star','mermaid-shell']
        for title in titles:
            a.input('touchButton',text='Read shared books');a.input('touchButton',text='Choose '+title)
            wait(lambda:a.input('inspect')['bookReady'],'book media '+title,20)
            a.input('touchButton',text='Read to me')
            wait(lambda:a.input('inspect')['bookSpeaking'],'narration starts '+title,5)
            time.sleep(1)
            require(a.input('inspect')['bookSpeaking'],'Narration ended before one second')
            a.input('touchButton',text='Read to me')
            paused=a.input('inspect');require(not paused['bookSpeaking'] and not paused['bookPlaying'] and paused['bookSample']>0,'Pause lost audio cursor')
            a.input('touchButton',text='Read to me');wait(lambda:a.input('inspect')['bookSpeaking'],'narration resumes')
            a.input('touchButton',text='Close');time.sleep(.2)
            require(not a.input('inspect')['bookOpen'] and not a.input('inspect')['bookSpeaking'],'Closed reader resumed')
        checks.append('six cold title loads, explicit playback, pause/resume samples and close cancellation')
        print('PASS '+checks[-1],flush=True)
        items=['bucket-1','sponge-1','ball-1','cookware-3']
        for i,c in enumerate(clients):
            cmd(c,0,x=1400,y=180);cmd(c,2,item=items[i]);cmd(c,3,item=items[i],x=1250+i*100,y=240)
        time.sleep(.8)
        for i,c in enumerate(clients):
            c.input('touch-begin',role=items[i],finger=41+i)
            wait(lambda:not c.input('inspect')['pending'],'grab settles')
        start=time.monotonic();initial=read(server.out/'activity-stats.json') or {};passes=0
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
            while time.monotonic()-start<args.seconds:
                elapsed=time.monotonic()-start
                futures=[pool.submit(c.input,'touch-move',role='',x=1250+i*100+math.sin(elapsed*2+i)*40,y=240+math.cos(elapsed*2+i)*45,finger=41+i) for i,c in enumerate(clients)]
                for future in futures:require(future.result()['connected'],'Client disconnected during simultaneous drag')
                passes+=1
                if passes%100==0:print('DRAG seconds '+str(round(elapsed)),flush=True)
        for i,c in enumerate(clients):c.input('touch-end',role='',x=1250+i*100,y=240,finger=41+i)
        for c in clients:home.ready(c)
        metrics=read(server.out/'activity-stats.json') or {}
        require(metrics.get('packets',0)>initial.get('packets',0)+100,'Compact activity stream was not exercised')
        require(metrics['maxBytes']<=1200,'Activity datagram exceeds budget')
        for c in [server,*clients]:
            evidence=read(c.out/'connection-evidence.json')
            require(not any(e['phase']=='disconnected' for e in evidence['events']),'Unexpected disconnect in sustained play')
            log=(c.out/'player.log').read_text(errors='replace')
            require('send queue full' not in log and 'Exception' not in log,'Transport queue or runtime failure')
        checks.append(str(args.seconds)+' seconds of four simultaneous object drags with zero disconnects or send-queue errors')
        print('PASS '+checks[-1],flush=True);passed=True
    finally:
        run.close();write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,activity=metrics,
            liveFamilyTouched=False,physicalAudioVerified=False));print('RESULT '+str(out/'result.json'),flush=True)


if __name__=='__main__':main()
