using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program {
 static SoloWorld w;static FamilySession family;
 static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};
 static void Need(bool v,string why){if(!v)throw new Exception(why);}
 static string Checkpoint()=>JsonSerializer.Serialize(w.Snapshot(),options);
 static SoloCommand SandCommand(int a,string op,int index,int round){var p=w.ReadPlayer("p"+a);return new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=SoloAction.Sandpit,value=op,target=DaycareSandpit.Target(index,round)};}
 static SoloResult Cmd(int a,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer("p"+a);return family.Submit((ulong)a,new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
 static void Do(int a,string op,int i){var at=DaycareSandpit.Place(i);Need(Cmd(a,SoloAction.Move,x:at.X,y:at.Y).Accepted,"move");var r=family.Submit((ulong)a,SandCommand(a,op,i,w.ReadSandpit().round));Need(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
 static void Main(string[] args){
  w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=w.Snapshot();old.schema=45;old.sandpit=null;var id=old.worldId;var prior=old.kingdom.npcCast.ToArray();w=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(old));Need(w.Schema==WorldLayout.Schema && w.Snapshot().worldId==id && w.ReadKingdom().npcCast.SequenceEqual(prior),"additive45 migration");family=new FamilySession(w);
  for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"travel");}
  Need(Cmd(1,SoloAction.Sandpit,"start").Accepted,"start");var g=w.ReadSandpit();var cast=g.friends.ToArray();Need(g.members.Count(m=>m.attending)==3,"common start");
  // There is no personal bucket lock: two children can fill the same mould.
  Do(1,"scoop",0);Do(2,"scoop",0);Do(1,"tip",0);Need(w.ReadSandpit().moulds[0].scoops==0 && !w.ReadSandpit().moulds[0].built,"dry sand crumbles");
  Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late travel");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().members.All(m=>m.attending) && w.ReadSandpit().friends.SequenceEqual(cast),"late join same lesson");
  for(var i=0;i<4;i++){for(var n=0;n<DaycareSandpit.Capacity(i);n++)Do(i+1,"scoop",i);Do(i+1,"water",i);Do(i+1,"tip",i);Do(i+1,i%2==0?"flag":"shell",i);}
  for(var i=0;i<400 && w.ReadSandpit().phase<3;i++)family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().phase==3,"one common completed castle");
  Need(Cmd(2,SoloAction.Sandpit,"leave").Accepted,"leave");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().members.Count(m=>m.attending)==3 && w.ReadSandpit().moulds.All(m=>m.built),"independent explicit leave");Need(family.Detach(3),"detach");Need(w.ReadSandpit().members.Count(m=>m.attending)==2,"independent disconnect");Need(family.Attach(3,"p3",out _),"reconnect");Need(w.ReadSandpit().members.Single(m=>m.actor=="p3").attending,"reconnect resumes");
  var saved=JsonSerializer.Deserialize<SoloSnapshot>(Checkpoint(),options);w=SoloWorld.Restore(saved);Need(w.ReadSandpit().friends.SequenceEqual(cast) && w.ReadSandpit().moulds.All(m=>m.built && m.decoration>0),"saved castle and cast");family=new FamilySession(w);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);
  // Capture intent before a sibling's reset. Queue.Take may rebase only its revision.
  var stale=new[]{"scoop","water","tip","flag","shell"}.Select(op=>SandCommand(2,op,0,w.ReadSandpit().round)).ToArray();
  var queue=new GardenCommandQueue();SoloResult staleResult=default;Need(queue.Enqueue(stale[0],r=>staleResult=r),"queue old intent");var inFlight=queue.Take(w.Revision);var oldRevision=inFlight.expectedRevision;var oldTarget=inFlight.target;
  Need(Cmd(1,SoloAction.Sandpit,"replay").Accepted,"new lesson");g=w.ReadSandpit();Need(g.round==2 && g.moulds.All(m=>!m.built && !m.wet && m.scoops==0) && !g.friends.Intersect(cast).Any(),"deliberate reset with different NPCs");SoloWorld.Validate(w.Snapshot());
  var before=Checkpoint();var conflict=family.Submit(2,inFlight);Need(!conflict.Accepted && conflict.Outcome=="stale-revision","reset invalidates old revision");Need(queue.Complete(inFlight.requestId,conflict),"queue conflict");
  var retry=queue.Take(w.Revision);Need(retry.requestId==inFlight.requestId && retry.target==oldTarget && retry.expectedRevision!=oldRevision,"retry retains captured lesson and identity");
  var rejected=family.Submit(2,retry);Need(!rejected.Accepted && rejected.Outcome=="old-sandpit-lesson","old scoop rejected after rebase");queue.Complete(retry.requestId,rejected);Need(!queue.Busy && !staleResult.Accepted && staleResult.Outcome=="old-sandpit-lesson" && Checkpoint()==before,"stale retry changes no checkpoint state");
  foreach(var command in stale.Skip(1)){command.expectedRevision=w.Revision;var result=family.Submit(2,command);Need(!result.Accepted && result.Outcome=="old-sandpit-lesson" && Checkpoint()==before,"reject old "+command.value+" atomically");}
  // Untagged/malformed/future targets cannot bypass the lesson guard.
  foreach(var target in new[]{"0","0@","0@0","0@3","4@2","0@2@2"}){var command=SandCommand(2,"water",0,2);command.target=target;var result=family.Submit(2,command);Need(!result.Accepted && result.Outcome=="old-sandpit-lesson" && Checkpoint()==before,"reject target "+target);}
  var point=DaycareSandpit.Place(0);Need(Cmd(2,SoloAction.Move,x:point.X,y:point.Y).Accepted,"approach current mould");
  SoloResult validResult=default;var valid=SandCommand(2,"water",0,g.round);Need(queue.Enqueue(valid,r=>validResult=r),"queue current water");var sent=queue.Take(w.Revision);
  Need(Cmd(1,SoloAction.Move,x:DaycareSandpit.Place(1).X,y:DaycareSandpit.Place(1).Y).Accepted,"sibling creates revision conflict");conflict=family.Submit(2,sent);Need(!conflict.Accepted && conflict.Outcome=="stale-revision","current action conflicts");queue.Complete(sent.requestId,conflict);
  retry=queue.Take(w.Revision);Need(retry.target==DaycareSandpit.Target(0,g.round) && retry.requestId==valid.requestId,"current retry keeps lesson");var accepted=family.Submit(2,retry);Need(accepted.Accepted && !accepted.Duplicate && w.ReadSandpit().moulds[0].wet,"current-round retry succeeds");queue.Complete(retry.requestId,accepted);Need(validResult.Accepted && !queue.Busy,"valid callback finishes");
  before=Checkpoint();var duplicate=family.Submit(2,retry);Need(duplicate.Accepted && duplicate.Duplicate && Checkpoint()==before,"accepted duplicate has no mutation");
  Do(3,"scoop",1);var construction=JsonSerializer.Serialize(w.ReadSandpit().moulds,options);var currentCast=w.ReadSandpit().friends.ToArray();
  Need(Cmd(2,SoloAction.Sandpit,"leave").Accepted,"leave second lesson");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().round==2 && w.ReadSandpit().members.Count(m=>m.attending)==3 && JsonSerializer.Serialize(w.ReadSandpit().moulds,options)==construction && w.ReadSandpit().friends.SequenceEqual(currentCast),"departure preserves new lesson and sibling work");
  Need(Cmd(2,SoloAction.Travel,"park").Accepted && Cmd(2,SoloAction.Travel,"daycare").Accepted,"return to second lesson");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().members.All(m=>m.attending) && w.ReadSandpit().round==2 && w.ReadSandpit().friends.SequenceEqual(currentCast),"return retains current lesson");
  for(var i=0;i<4;i++){while(w.ReadSandpit().moulds[i].scoops<DaycareSandpit.Capacity(i))Do(i+1,"scoop",i);Do(i+1,"water",i);Do(i+1,"tip",i);Do(i+1,i%2==0?"shell":"flag",i);}
  for(var i=0;i<400 && w.ReadSandpit().phase<3;i++)family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().phase==3,"current lesson completes normally");Need(Cmd(1,SoloAction.Sandpit,"replay").Accepted,"third lesson");
  before=Checkpoint();duplicate=family.Submit(2,retry);Need(duplicate.Accepted && duplicate.Duplicate && Checkpoint()==before,"old accepted receipt remains duplicate after reset");SoloWorld.Validate(w.Snapshot());
  Console.WriteLine("PASS: four-profile shared building, late join, independent leave/disconnect/reconnect, dry/wet/decorations, schema45 migration and JSON retention.");
  Console.WriteLine("PASS: all five stale operations rejected without checkpoint mutation; malformed/missing/future round rejected; actual queue revision rebase preserves captured round; current retry and duplicate succeed; later-reset duplicate cannot mutate the new lesson.");
  ScoopFeedbackChecks.Run();
  WaterChecks.Run();
  TipChecks.Run();
  if(args.Length>0)ClientIntentChecks.Run(args[0]);
 }
}
