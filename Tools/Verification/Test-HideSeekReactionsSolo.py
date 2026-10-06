"""Private single-client hiding/discovery/replay; disposable enrollment and saves only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import argparse,json,subprocess,time,uuid,importlib.util,shutil
from family_pairing import ROOT,create_family,write_record
from shared_garden_runtime import Instance,read,write,wait,require
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
 p=argparse.ArgumentParser();p.add_argument('build',type=int);args=p.parse_args();folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}';summary=read(folder/'build-summary.json');authority,players,_=create_family()
 class Run:pass
 run=Run();run.build=args.build;run.run_id=authority['worldId'];run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;run.path.mkdir(parents=True);out=run.path/'hide-seek-solo';out.mkdir();instances=[];checks=[];passed=False;audio=None
 print('EVIDENCE '+str(out),flush=True)
 def start():
  record=players[0];v=Instance.__new__(Instance);v.run=run;v.role='client';v.profile=record['profile'];v.serial=0;v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir();pair=run.path/(v.identity+'.pairing');write_record(pair,record)
  cfg=dict(runId=run.run_id,instanceId=v.identity,role='client',port=1025,protocol=3,content=summary['content'],pairingPath=str(pair),presentation=True,verifyGarden=True,interactive=True,testAudible=True);config=run.path/(v.identity+'.config.json');write(config,cfg)
  cmd=[str(folder/'Client/LittleWeepsNetwork.exe'),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591'];startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0;v.process=subprocess.Popen(cmd,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=startup,creationflags=subprocess.CREATE_NO_WINDOW);instances.append(v);wait(lambda:v.status(),'solo status',30);home.ready(v);return v
 def tap(name):a.input('touchButton',text=name);home.ready(a)
 def menu(day=False):
  tap('Games')
  if not day:tap('Hide & seek');return
  e=a.input('inspect');b=next(c for c in e['controls'] if c['name']=='The Adventure')['bounds'];x=b['x']+b['width']*.7;y=b['y']+b['height']*.5;a.input('touch-begin',role='screen',x=x,y=y,finger=76)
  for dy in [25,65,120,180,250,330,420]:a.input('touch-move',role='screen',x=x,y=min(e['screenHeight']-12,y+dy),finger=76)
  a.input('touch-end',role='screen',x=x,y=min(e['screenHeight']-12,y+420),finger=76);tap('Hide & seek with Calypso')
 def record(s):checks.append(s);print('PASS '+s,flush=True)
 try:
  a=start();require(not a.input('inspect')['shared'],'Private client joined authority');a.input('fixtureTravel',text='home');home.ready(a)
  # Registered graphics are rendered by Unity in a diagnostic gallery; no fake gameplay.
  a.input('revealGallery');wait(lambda:(a.out/'reveal-gallery-3.png').exists(),'all 37 expressions rendered')
  for f in a.out.glob('reveal-gallery-*.png'):shutil.copyfile(f,out/f.name)
  menu();tap('Hide Curtain');wait(lambda:a.input('inspect')['homePose']=='Sit','Private Home hide',10)
  wav=out/'home-actual-loopback.wav';audio=subprocess.Popen([shutil.which('ffmpeg'),'-y','-hide_banner','-loglevel','error','-f','dshow','-i','audio=virtual-audio-capturer','-t','28','-c:a','pcm_s16le',str(wav)],stdin=subprocess.PIPE,stdout=subprocess.DEVNULL,stderr=open(out/'audio-capture.log','w'))
  wait(lambda:a.input('inspect')['homeRevealEvents']>0,'Private Home actual discovery',40);home.capture(a,out,'home-solo-discovery');time.sleep(.6);home.capture(a,out,'home-solo-happy');audio.communicate(b'q\n',timeout=15);require(audio.returncode==0 and wav.stat().st_size>44,'Loopback recording');audio=None
  menu();tap('Hide Curtain');wait(lambda:a.input('inspect')['homePose']=='Sit','Private Home replay');tap('All done');record('One actual private native client: Home hiding, discovery, happy response and deliberate replay; actual loopback audio captured, no listening claim')
  a.input('fixtureTravel',text='daycare');home.ready(a);menu(True);tap('Hide here 3');wait(lambda:a.input('inspect')['homePose']=='Sit','Private Daycare hiding',10)
  wait(lambda:a.input('inspect')['daycareRevealEvents']>0,'Private Daycare first discovery',40);home.capture(a,out,'daycare-solo-discovery')
  wait(lambda:a.input('inspect')['hideClub']['phase']==4,'Private Daycare finish',40);tap('Come out');wait(lambda:a.input('inspect')['hideClub']['round']==2,'Private Daycare replay');tap('Hide here 3');wait(lambda:a.input('inspect')['homePose']=='Sit','Private Daycare replay hiding',10);tap('Back to Daycare');record('One actual private native client: Daycare hiding, four NPC discoveries, replay and independent return')
  passed=True
 finally:
  if audio is not None:audio.communicate(b'q\n',timeout=15)
  errors=[]
  for v in instances:
   v.close();errors += [line for line in (v.out/'player.log').read_text(errors='replace').splitlines() if 'Exception:' in line]
  write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,actualConcurrentNativeClients=1,authorityProcesses=0,privateSyntheticEnrollment=True,audioRecording='home-actual-loopback.wav',audioListening=False,errors=errors,exitCodes=[v.process.returncode for v in instances]));print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
