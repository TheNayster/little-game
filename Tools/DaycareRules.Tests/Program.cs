using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program {
 static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
 static SoloResult Cmd(SoloWorld w,string actor,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
 static void Do(SoloWorld w,string a,string op,string target=""){var r=Cmd(w,a,SoloAction.Daycare,op,target);Need(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
 static void Plate(SoloWorld w,string a,int i){var p=DaycareTeacher.Plate(i);Need(Cmd(w,a,SoloAction.Move,x:p.X,y:p.Y).Accepted,"walk");Do(w,a,"plate",i.ToString());}
 static void Main(){
 var old=SoloWorld.WithKingdom(SoloWorld.Create("a","b","c","d"));var before=old.Snapshot();var w=SoloWorld.WithNpcCasts(old);Need(w.Snapshot().worldId==before.worldId && w.ReadKingdom().round==before.kingdom.round && w.ReadToys().Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)),"additive upgrade");
 foreach(var a in new[]{"a","b","c","d"})Need(Cmd(w,a,SoloAction.Travel,"daycare").Accepted,"daycare");
 Do(w,"a","start");var round=w.ReadDaycare().round;Need(!w.ReadDaycare().members.Single(m=>m.actor=="b").attending,"forced participation");w.AdvanceIdle(1,out _);Do(w,"b","start");w.AdvanceIdle(1,out _);Do(w,"c","start");w.AdvanceIdle(1,out _);Need(w.ReadDaycare().phase==2,"common welcome");Plate(w,"a",0);Do(w,"d","start");Need(w.ReadDaycare().round==round && w.ReadDaycare().plates==1,"late join reset");
 Need(!Cmd(w,"a",SoloAction.Daycare,"plate","0").Accepted && DaycareTeacher.Count(w.ReadDaycare().plates)==1,"duplicate count");
 Plate(w,"b",1);Do(w,"a","leave");var family=new FamilySession(w);for(ulong i=1;i<=4;i++)Need(family.Attach(i,new[]{"a","b","c","d"}[i-1],out _),"admit");family.Detach(2);Need(w.ReadDaycare().plates==3 && w.ReadDaycare().members.Single(m=>m.actor=="c").attending,"independent disconnect");
 Plate(w,"c",2);Plate(w,"d",3);Need(w.ReadDaycare().phase==3 && DaycareTeacher.Count(w.ReadDaycare().plates)==4,"shared completion");
 Do(w,"c","help");Need(DaycareTeacher.Routine(w.ReadDaycare())==3,"bounded helper");w.AdvanceIdle(1,out _);var data=JsonSerializer.Serialize(w.Snapshot(),new JsonSerializerOptions{IncludeFields=true});var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(data,new JsonSerializerOptions{IncludeFields=true}));Need(restored.ReadDaycare().plates==15,"reopen");
 foreach(var a in new[]{"a","b","c","d"})Need(Cmd(w,a,SoloAction.Travel,"park").Accepted,"travel");var clock=w.ReadDaycare().clock;w.AdvanceIdle(1,out _);Need(w.ReadDaycare().clock==clock,"empty daycare drift");Need(Cmd(w,"a",SoloAction.Travel,"daycare").Accepted,"return");Do(w,"a","start");Need(w.ReadDaycare().round==round,"checkpoint join");w.AdvanceIdle(1,out _);Do(w,"a","replay");Need(w.ReadDaycare().round==round+1 && w.ReadDaycare().plates==0,"shared replay");Do(w,"a","leave");var started=w.ReadDaycare().started;w.AdvanceIdle(1,out _);Need(w.ReadDaycare().started==started+1 && w.ReadDaycare().phase==1,"unattended welcome moved");
 var d=w.Snapshot();d.daycare.clock=15.7447122;d.daycare.started=10;d.daycare.helpUntil=21.7447122;SoloWorld.Validate(d);
 var cast=w.ReadDaycare().guests;var npc=w.ReadKingdom().npcCast;Need(DaycareNpcCasts.Valid(cast,4) && DaycareNpcCasts.Valid(npc,9),"distinct prepared casts");
 var copy=w.Snapshot();copy.daycare.guests[0]="bandit";Need(w.ReadDaycare().guests[0]!="bandit","cast clone shared array");
 var opts=new JsonSerializerOptions{IncludeFields=true};var castReopen=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),opts),opts));Need(cast.SequenceEqual(castReopen.ReadDaycare().guests) && npc.SequenceEqual(castReopen.ReadKingdom().npcCast),"cast JSON stability");
 var fullPool=DaycareNpcCasts.Pick(33);Need(fullPool.Contains("blue-pup") && fullPool.Contains("orange-pup"),"player avatars removed from NPC pool");
 Need(Cmd(w,"a",SoloAction.ChangeAvatar,cast[0]).Accepted && Cmd(w,"b",SoloAction.ChangeAvatar,cast[0]).Accepted && cast.SequenceEqual(w.ReadDaycare().guests),"players cannot match NPCs or avatar choice rerolled cast");
 for(var i=0;i<40;i++){var draw=DaycareNpcCasts.Pick(9,npc);Need(DaycareNpcCasts.Valid(draw,9) && !draw.Intersect(npc).Any(),"variety and prior NPC exclusion");npc=draw;}
 Need(Cmd(w,"a",SoloAction.Travel,"daycare").Outcome=="already-there","fixture in daycare");Do(w,"a","start");Need(cast.SequenceEqual(w.ReadDaycare().guests),"rejoin rerolled cast");for(var i=0;i<3;i++)w.AdvanceIdle(1,out _);for(var i=0;i<4;i++)Plate(w,"a",i);for(var i=0;i<2;i++)w.AdvanceIdle(1,out _);Do(w,"a","replay");Need(!cast.Intersect(w.ReadDaycare().guests).Any(),"new picnic repeats prior guests");
 Console.WriteLine("PASS additive upgrade, four shared contributors, duplicate protection, late joining, independent exits/disconnect, teacher help, JSON reopen, empty pause and replay.");
 }
}
