"""Focused native release test: real tool gestures, all eight patients, four family members."""
import argparse,importlib.util,time,json
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'));home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)
def main():
 parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--preview',action='store_true');args=parser.parse_args();run=Run(args.build,extended_test_lifetime=True);runs=[run];out=run.path/'vet';out.mkdir();clients=[];checks=[];passed=False;finger=60
 print('EVIDENCE '+str(out),flush=True)
 def state():return server.state()['view']['vet']
 def patient(i):return state()['patients'][i]
 def member(v):return next(m for m in state()['members'] if m['actor']==v.profile)
 def record(name):checks.append(name);print('PASS '+name,flush=True)
 def cmd(v,action,**kw):r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
 def button(v,name):wait(lambda:any(c['name']==name and c['enabled'] for c in v.input('inspect')['controls']),'enabled '+name,20);v.input('touchButton',text=name)
 def settled(v):wait(lambda:v.input('inspect')['vetQueued']==0 and not v.input('inspect')['pending'],'care replies settle')
 def friends(v):
  if not any(c['name'].startswith('Vet bed ') for c in v.input('inspect')['controls']):button(v,'Friends')
 def select(v,i):friends(v);button(v,'Vet bed '+str(patient(i)['bed']));wait(lambda:v.input('inspect')['vetSelected']==i,'patient selected');wait(lambda:state()['clock']-patient(i)['arrived']>=2.2,'animal reaches cushion')
 def spot(i,n):
  cx=.58 if i==6 else .55 if i>=4 else .47;cy=.35 if i==6 else .25 if i>=4 else .42
  return cx+(-.12 if n==0 else .12 if n==1 else 0),cy+(.06 if n==0 else .03 if n==1 else -.12)
 def tap(v,i,tool,n,check):
  nonlocal finger;finger+=1;button(v,'Vet tool '+tool);x,y=spot(i,n);v.input('touch-begin',role='vet',x=x,y=y,finger=finger);v.input('touch-end',role='vet',x=x,y=y,finger=finger);wait(check,tool+' touch commits',12);settled(v)
 def scrub(v,i,tool,n):
  nonlocal finger;finger+=1;field='washed' if tool=='wash' else 'brushed';button(v,'Vet tool '+tool);x,y=spot(i,n);start=patient(i)[field][n]
  v.input('touch-begin',role='vet',x=x-.05,y=y,finger=finger);wait(lambda:patient(i)[field][n]>start,'first visible stroke',12)
  for dx in [.015,.075]:
   before=patient(i)[field][n]
   if before>=3:break
   v.input('touch-move',role='vet',x=x+dx,y=y,finger=finger);wait(lambda:patient(i)[field][n]>before,'moving tool changes body spot',12)
  v.input('touch-end',role='vet',x=x+.075,y=y,finger=finger);settled(v)
 def bandage(v,i):
  nonlocal finger;finger+=1;x,y=spot(i,2);v.input('touch-begin',role='ui:Vet tool bandage',x=0,y=0,finger=finger);v.input('touch-move',role='vet',x=x,y=y,finger=finger);wait(lambda:patient(i)['bandaged'],'bandage dragged from tray onto patch',12);v.input('touch-end',role='vet',x=x,y=y,finger=finger);settled(v)
 def capture(v,name):home.capture(v,out,name)
 def prepare():
  for i,v in enumerate(clients):home.ready(v);v.input('resize',x=1024 if i==2 else 1280,y=768 if i==2 else 591);home.ready(v)
 try:
  server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients;prepare()
  for v in clients:cmd(v,1,value='blue-pup');cmd(v,7,value='park' if v==d else 'daycare')
  button(a,'Games');capture(a,'daycare-clinic-menu-phone');button(a,'Animal care clinic');wait(lambda:sum(m['attending'] for m in state()['members'])==3,'one start joins same-world group');cast=state()['friends']
  for v in clients[:3]:home.ready(v);require(v.input('inspect')['zone']=='daycare-vet','Clinic did not travel group')
  select(a,0);capture(a,'living-puppy-phone');select(c,4);capture(c,'small-existing-trex-tablet');button(a,'Calypso: show me');time.sleep(.5);capture(a,'calypso-sponge-demonstration-phone');require(patient(0)['washed']==[0,0,0],'Demonstration solved care')
  record('actual Daycare menu starts one shared clinic; living household pets and small existing dinosaurs, pictures and Calypso demonstration render on phone/tablet')
  if args.preview:passed=True;return
  select(b,4);button(b,'Vet tool bandage');x,y=spot(4,2);b.input('touch-begin',role='vet',x=x,y=y,finger=99);b.input('touch-end',role='vet',x=x,y=y,finger=99);wait(lambda:'Clean' in b.input('inspect')['vetFeedback'],'clean-before-bandage hint');require(not patient(4)['bandaged'],'Dirty dinosaur bandaged')
  tap(a,0,'wash',0,lambda:patient(0)['washed'][0]==1);select(b,0);tap(b,0,'wash',0,lambda:patient(0)['washed'][0]==2);capture(a,'two-players-share-washing-phone')
  button(a,'Vet tool brush');require(a.input('inspect')['vetTool']==1,'Pictured need cannot choose its care tool');a.input('application-pause');require(a.input('inspect')['vetQueued']==0 and not a.input('inspect')['vetSoundPlaying'],'Pause did not cancel care/audio');a.input('application-resume');home.ready(a)
  cmd(d,7,value='daycare');wait(lambda:member(d)['attending'],'late fourth joins');home.ready(d);require(patient(0)['washed'][0]==2 and state()['friends']==cast,'Late entry reset clinic');record('two players really wash the same spot; pictured care order protects bandage; late fourth retains progress and saved varied NPCs despite four identical player avatars')
  button(c,'Return to Daycare');wait(lambda:not member(c)['attending'],'independent exit');time.sleep(.4);require(sum(m['attending'] for m in state()['members'])==3 and patient(0)['washed'][0]==2,'Exit reset clinic')
  b.close();wait(lambda:not member(b)['attending'],'disconnect releases one helper');b=run.start('client',b.profile);clients[1]=b;home.ready(b);require(member(b)['attending'] and patient(0)['washed'][0]==2,'Reconnect lost care');record('independent Return and native disconnect/reconnect retain partial animal care')
  before=state();run.close();require(all(v.process.returncode==0 for v in run.instances),'Initial shutdown abnormal');run=Run(args.build,resume=run.run_id,extended_test_lifetime=True);runs.append(run);server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients;prepare();require(patient(0)['washed'][0]==2 and state()['friends']==cast and not member(c)['attending'],'Cold saved-server reopen lost care/exit');cmd(c,7,value='park');cmd(c,7,value='daycare');wait(lambda:member(c)['attending'],'new world arrival rejoins');home.ready(c);record('actual saved server reopens partial care, patient queue, varied cast and independent return; a new arrival rejoins the same clinic')
  for i in range(8):
   v=clients[i%4]
   if patient(i)['bed']==-1:
    friends(v);capture(v,'friends-choice-'+str(i));button(v,'Vet welcome '+str(i));wait(lambda:patient(i)['bed']>=0,'chosen patient enters empty care bed');home.ready(v);require(v.input('inspect')['vetSelected']==i,'Welcomed friend does not become the selected patient');capture(v,'patient-'+str(i)+'-arriving')
   select(v,i);capture(v,'patient-'+str(i)+'-before-care')
   for tool,field,bit in [('wash','washed',1),('brush','brushed',2)]:
    if not patient(i)['needs']&bit:continue
    for n in range(3):
     while patient(i)[field][n]<3:scrub(v,i,tool,n)
   if patient(i)['needs']&4:bandage(v,i)
   for n in range(3):
    if patient(i)['cuddles']>=3:break
    count=patient(i)['cuddles'];tap(v,i,'cuddle',2,lambda:patient(i)['cuddles']>count)
   if i in [0,4,6]:
    x,y=spot(i,2);v.input('touch-begin',role='vet',x=x,y=y,finger=199);v.input('touch-end',role='vet',x=x,y=y,finger=199);wait(lambda:v.input('inspect')['vetSoundPlaying'],'comfortable animal reacts when stroked',8)
   capture(v,'patient-'+str(i)+'-comfortable');button(v,'Send friend home');wait(lambda:patient(i)['bed']==-2,'comfortable friend goes home')
  record('real rub/brush gestures, tray-to-body bandage drags and cuddles care for all four pets and all four existing dinosaurs; players choose waiting patients and share all eight completions')
  capture(a,'family-clinic-complete-phone');capture(c,'family-clinic-complete-tablet');button(a,'Another clinic day');wait(lambda:state()['round']==2,'one deliberate new clinic day');require(not set(cast)&set(state()['friends']) and patient(0)['washed']==[0,0,0] and sum(m['attending'] for m in state()['members'])==4,'Replay did not reset one common clinic')
  select(a,4);a.input('treasureMotionStart');time.sleep(3);a.input('treasureMotionStop');capture(a,'small-existing-trex-new-day');record('completed common clinic stays visible; deliberate replay starts one day, rerolls classmates, and resets only care');passed=True
 finally:
  if not passed:
   for i,v in enumerate(clients):
    if v.process.poll() is not None:continue
    try:capture(v,'failure-'+str(i+1))
    except Exception:pass
  run.close();write(out/'result.json',dict(build=args.build,runId=run.run_id,gameplayPassed=passed,checks=checks,exitCodes=[v.process.returncode for r in runs for v in r.instances],scope='visual preview only' if args.preview else 'isolated release authority/four native clients; actual phone/tablet pointer gestures, all pets/dinosaurs, shared care, arrival/departure, reconnect and saved-server reopen; no device/live-server rollout'))
 print('PASS ALL '+str(len(checks))+' groups',flush=True)
if __name__=='__main__':main()
