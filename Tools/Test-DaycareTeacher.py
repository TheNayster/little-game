"""Focused four-client Daycare teacher/counting checks on disposable release authority."""
import argparse,importlib.util,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--visual-only',action='store_true');args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True);out=run.path/'daycare';out.mkdir();checks=[];passed=False
 print('EVIDENCE '+str(out),flush=True)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def state():return server.state()['view']['daycare']
 def cmd(v,action,**kw):
  result=home.command(v,action,**kw);require(result['accepted'],result['outcome']);home.ready(v)
 def button(v,name):
  wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,15);v.input('touchButton',text=name);time.sleep(.2);home.ready(v)
 def menu(v):button(v,'Games');button(v,'Picnic counting');wait(lambda:next(m for m in state()['members'] if m['actor']==v.profile)['attending'],'joined picnic');home.ready(v)
 def plate(v,mask):
  wait(lambda:v.input('inspect')['daycare']['phase']==2,'client table phase',15);button(v,'Next picnic plate');wait(lambda:state()['plates']&mask,'shared plate',14)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
  for v in clients:home.ready(v);cmd(v,7,value='daycare')
  a.input('resize',x=1280,y=591);d.input('resize',x=1024,y=768);home.ready(a);home.ready(d)
  require(all(v.input('inspect')['calypsoVisible'] for v in clients),'Calypso missing')
  button(a,'Ask Calypso');require('Hear a story' in a.input('inspect')['visibleText'],'Teacher story choice missing');home.capture(a,out,'calypso-choices-phone');button(a,'Help me');require(state()['helpUntil']>state()['clock'],'Shared bounded help missing')
  button(a,'Games');require('The Adventure' in a.input('inspect')['visibleText'] and 'Picnic counting' in a.input('inspect')['visibleText'],'Daycare game choices missing');home.capture(a,out,'daycare-games-phone');button(a,'Picnic counting');round_id=state()['round'];menu(b);wait(lambda:state()['phase']==2,'common welcome');plate(a,1);menu(c);menu(d)
  require(state()['round']==round_id and state()['plates']==1,'Late join reset picnic');require(all(v.input('inspect')['daycare']['round']==round_id for v in clients),'Four clients have different picnic');home.capture(d,out,'four-player-picnic-tablet');
  if args.visual_only:
   wait(lambda:(s:=a.input('inspect'))['daycareRoutine']==1 and s['daycare']['clock']%18>=3,'settled teacher reading',100);home.capture(a,out,'calypso-reading-phone');record('final four-player table layout and settled Calypso reading pose');passed=True;return
  record('Calypso choices, direct world menu and four players sharing the current picnic')
  plate(b,2);button(a,'Leave picnic');require(state()['plates']==3 and next(m for m in state()['members'] if m['actor']==c.profile)['attending'],'Independent exit reset group');b.close();wait(lambda:not next(m for m in state()['members'] if m['actor']==b.profile)['attending'],'disconnect membership');plate(c,4);menu(a);require(state()['plates']==7,'Rejoin reset plates');plate(d,8);wait(lambda:state()['phase']==3,'picnic complete');home.capture(d,out,'four-plates-ready-tablet');record('shared committed counting, late rejoin, independent leave/disconnect and common completion')
  wait(lambda:(s:=a.input('inspect'))['daycareRoutine']==1 and s['daycare']['clock']%18>=3,'teacher reading routine',100);home.capture(a,out,'calypso-reading-phone');record('authoritative teacher progresses into her reading routine')
  for v in (a,c,d):cmd(v,7,value='park')
  clock=state()['clock'];time.sleep(1);require(state()['clock']==clock and state()['plates']==15,'Empty daycare checkpoint drifted')
  for v in (a,c,d):v.close()
  server.close();server=run.start('server');a=run.start('client',run.slots[0]['profile']);home.ready(a);require(state()['plates']==15 and state()['round']==round_id,'Saved picnic lost');cmd(a,7,value='daycare');menu(a);button(a,'Set it again');wait(lambda:state()['round']==round_id+1 and state()['plates']==0,'explicit new common round');record('empty suspension, native saved checkpoint reopen and explicit shared replay');passed=True
 finally:
  write(out/'result.json',dict(build=args.build,passed=passed,checks=checks,visualOnly=args.visual_only,scope='disposable release authority and four native clients; no physical-device or live-server update'));run.close()
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
