using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program {
 static SoloWorld w;static FamilySession family;
 static void Need(bool v,string why){if(!v)throw new Exception(why);}
 static SoloResult Cmd(int a,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer("p"+a);return family.Submit((ulong)a,new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
 static void Do(int a,string op,int i){var at=DaycareSandpit.Place(i);Need(Cmd(a,SoloAction.Move,x:at.X,y:at.Y).Accepted,"move");var r=Cmd(a,SoloAction.Sandpit,op,i.ToString());Need(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
 static void Main(){
  w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=w.Snapshot();old.schema=45;old.sandpit=null;var id=old.worldId;var prior=old.kingdom.npcCast.ToArray();w=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(old));Need(w.Schema==46 && w.Snapshot().worldId==id && w.ReadKingdom().npcCast.SequenceEqual(prior),"additive45 migration");family=new FamilySession(w);
  for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"travel");}
  Need(Cmd(1,SoloAction.Sandpit,"start").Accepted,"start");var g=w.ReadSandpit();var cast=g.friends.ToArray();Need(g.members.Count(m=>m.attending)==3,"common start");
  // There is no personal bucket lock: two children can fill the same mould.
  Do(1,"scoop",0);Do(2,"scoop",0);Do(1,"tip",0);Need(w.ReadSandpit().moulds[0].scoops==0 && !w.ReadSandpit().moulds[0].built,"dry sand crumbles");
  Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late travel");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().members.All(m=>m.attending) && w.ReadSandpit().friends.SequenceEqual(cast),"late join same lesson");
  for(var i=0;i<4;i++){for(var n=0;n<DaycareSandpit.Capacity(i);n++)Do(i+1,"scoop",i);Do(i+1,"water",i);Do(i+1,"tip",i);Do(i+1,i%2==0?"flag":"shell",i);}
  for(var i=0;i<400 && w.ReadSandpit().phase<3;i++)family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().phase==3,"one common completed castle");
  Need(Cmd(2,SoloAction.Sandpit,"leave").Accepted,"leave");family.AdvanceIdle(.1,out _);Need(w.ReadSandpit().members.Count(m=>m.attending)==3 && w.ReadSandpit().moulds.All(m=>m.built),"independent explicit leave");Need(family.Detach(3),"detach");Need(w.ReadSandpit().members.Count(m=>m.attending)==2,"independent disconnect");Need(family.Attach(3,"p3",out _),"reconnect");Need(w.ReadSandpit().members.Single(m=>m.actor=="p3").attending,"reconnect resumes");
  var options=new JsonSerializerOptions{IncludeFields=true};var saved=JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),options),options);w=SoloWorld.Restore(saved);Need(w.ReadSandpit().friends.SequenceEqual(cast) && w.ReadSandpit().moulds.All(m=>m.built && m.decoration>0),"saved castle and cast");family=new FamilySession(w);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);
  Need(Cmd(1,SoloAction.Sandpit,"replay").Accepted,"new lesson");g=w.ReadSandpit();Need(g.round==2 && g.moulds.All(m=>!m.built && !m.wet && m.scoops==0) && !g.friends.Intersect(cast).Any(),"deliberate reset with different NPCs");SoloWorld.Validate(w.Snapshot());
  Console.WriteLine("PASS additive45 migration; common four-player building, shared mould contributions, dry/wet experiment, late arrival, independent leave/disconnect/reconnect, JSON castle/cast retention and deliberate new lesson.");
 }
}
