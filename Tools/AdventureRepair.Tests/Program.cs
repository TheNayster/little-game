using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
class Program {
 static SoloWorld w;static FamilySession family;
 static void Need(bool yes,string why){if(!yes)throw new Exception(why);}
 static SoloResult Cmd(int actor,SoloAction action,string value="",string target="",float x=0,float y=0){var id="p"+actor;var p=w.ReadPlayer(id);return family.Submit((ulong)actor,new SoloCommand{actor=id,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
 static void Do(int actor,string op,string target=""){if(target!=""){var at=KingdomAdventure.Prop(target);Need(Cmd(actor,SoloAction.Move,x:at.X,y:at.Y).Accepted,"move");}var r=Cmd(actor,SoloAction.Kingdom,op,target);Need(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
 static void Main(){
 w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=w.Snapshot();old.schema=42;old.kingdom.boardAt=null;old.kingdom.wakeAt=null;foreach(var m in old.kingdom.members)m.carrying=null;w=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(old));Need(w.Schema==43 && w.Snapshot().worldId==old.worldId,"retained42 upgrade");
 family=new FamilySession(w);for(int i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"admit");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"fixture travel");}
 Do(1,"start");Need(w.ReadKingdom().members.Count(m=>m.attending)==3 && w.ReadPlayer("p2").zone==KingdomAdventure.Zone && w.ReadPlayer("p4").zone=="park","same-world auto entry");Do(1,"begin");
 Do(1,"fruit","fruit-0");Need(w.ReadKingdom().supplies==0 && w.ReadKingdom().members[0].carrying=="fruit-0","visible hold before delivery");Need(!Cmd(2,SoloAction.Kingdom,"fruit","fruit-0").Accepted,"duplicate hold");
 var json=new JsonSerializerOptions{IncludeFields=true};var reopened=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),json),json));Need(reopened.ReadKingdom().members[0].carrying=="fruit-0","saved hold");
 Do(1,"deliver","basket");Do(2,"fruit","fruit-1");Do(2,"leave");Need(w.ReadKingdom().supplies==1 && w.ReadKingdom().members[1].carrying=="","exit released fruit");family.AdvanceIdle(1,out _);Need(w.ReadPlayer("p2").zone=="daycare","explicit exit pulled back");Do(2,"start");Do(2,"fruit","fruit-1");Do(2,"deliver","basket");Do(3,"fruit","fruit-2");Do(3,"deliver","basket");Need(w.ReadKingdom().phase==KingdomPhase.Bridge,"food delivered together");Need(KingdomAdventure.NpcPoint(w.ReadKingdom(),0,w.ReadKingdom().started).X==950,"guide returned to entrance at next stage");
 Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late same-world entry");family.AdvanceIdle(.1,out _);Need(w.ReadPlayer("p4").zone==KingdomAdventure.Zone && w.ReadKingdom().supplies==7,"late auto join kept checkpoint");
 Need(Cmd(4,SoloAction.Travel,"park").Accepted,"leave world");family.AdvanceIdle(.1,out _);Need(w.ReadPlayer("p4").zone=="park","world departure pulled back");Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"return world");family.AdvanceIdle(.1,out _);Need(w.ReadKingdom().members[3].attending && w.ReadPlayer("p4").zone==KingdomAdventure.Zone && w.ReadKingdom().supplies==7,"world return did not rejoin retained adventure");
 Do(1,"board","board-0");var g=w.ReadKingdom();var begin=KingdomAdventure.NpcPoint(g,2,g.boardAt[0]);var end=KingdomAdventure.NpcPoint(g,2,g.boardAt[0]+2);Need(end.X>begin.X+200,"builder did not carry plank to bridge");Do(2,"board","board-1");Do(3,"board","board-2");
 Do(4,"toss","ball");Do(1,"wand","wand");Do(2,"wake","friend-0");g=w.ReadKingdom();Need(KingdomAdventure.NpcPoint(g,6,g.wakeAt[0]+3).X<KingdomAdventure.NpcPoint(g,6,g.wakeAt[0]).X,"rescued friend stayed frozen");Do(3,"wake","friend-1");Do(4,"wake","friend-2");for(int i=0;i<6;i++)family.AdvanceIdle(1,out _);var cast=w.ReadKingdom().npcCast;Do(1,"replay");Need(!cast.Intersect(w.ReadKingdom().npcCast).Any(),"replay cast");family.Detach(3);Need(!w.ReadKingdom().members[2].attending && w.ReadKingdom().members[0].attending,"disconnect reset siblings");
 // Picnic uses one shared stock tray, carried plates and four counted places.
 Need(family.Attach(3,"p3",out _),"reconnect");for(int i=1;i<=4;i++)Do(i,"leave");
 Need(Cmd(1,SoloAction.Daycare,"start").Accepted,"picnic start");Need(w.ReadDaycare().members.All(m=>m.attending),"picnic automatic group");for(int i=0;i<3;i++)family.AdvanceIdle(1,out _);
 for(int i=1;i<=4;i++){var tray=DaycareTeacher.Tray;Need(Cmd(i,SoloAction.Move,x:tray.X,y:tray.Y).Accepted,"tray move");Need(Cmd(i,SoloAction.Daycare,"take-plate").Accepted,"take plate");Need(w.ReadDaycare().members[i-1].carryingPlate,"plate held");var place=DaycareTeacher.Plate(i-1);Need(Cmd(i,SoloAction.Move,x:place.X,y:place.Y).Accepted,"place move");Need(Cmd(i,SoloAction.Daycare,"plate",(i-1).ToString()).Accepted,"place plate");}
 Need(w.ReadDaycare().phase==3 && w.ReadDaycare().plates==15 && w.ReadDaycare().members.All(m=>!m.carryingPlate),"four counted plates");SoloWorld.Validate(w.Snapshot());
 Console.WriteLine("PASS42 upgrade, same-world automatic and late joins, explicit departure, saved fruit holds/delivery, exclusive pickup, returning fruit, timed builder/rescue jobs, shared finish/replay and independent disconnect; four-player shared plate pickup/carry/place/count.");
 }
}
