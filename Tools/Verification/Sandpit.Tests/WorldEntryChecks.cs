using System;
using System.Linq;
using LittleWeeps.Core;

partial class Program
{
 static bool Attending(SoloSnapshot s,SoloAction game,string actor)=>game switch {
  SoloAction.Sandpit=>s.sandpit.members.Single(m=>m.actor==actor).attending,
  SoloAction.Kingdom=>s.kingdom.members.Single(m=>m.actor==actor).attending,
  SoloAction.Daycare=>s.daycare.members.Single(m=>m.actor==actor).attending,
  SoloAction.Vet=>s.vet.members.Single(m=>m.actor==actor).attending,
  SoloAction.Treasure=>s.treasure.members.Single(m=>m.actor==actor).attending,
  _=>throw new ArgumentException()
 };
 static void WorldEntryChecks()
 {
  foreach(var game in new[]{SoloAction.Sandpit,SoloAction.Kingdom,SoloAction.Daycare,SoloAction.Vet,SoloAction.Treasure}) {
   w=GameWorld.WithDinosaurWorld(GameWorld.Create("p1","p2","p3","p4"));family=new FamilySession(w);
   for(int i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"entry attach");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"entry fixture");}
   Need(Cmd(1,game,"start").Accepted,"start shared "+game);
   Need(Enumerable.Range(1,3).All(i=>Attending(w.Snapshot(),game,"p"+i)),"existing Daycare group shares start "+game);
   if(game==SoloAction.Sandpit)ShapePiece(1,"round",0,0);
   var pieces=Json(w.ReadSandpit().moulds);
   Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"world card arrival "+game);family.AdvanceIdle(.1,out _);
   var p=w.ReadPlayer("p4");
   Need(p.zone=="daycare" && p.x==WorldLayout.ArrivalX("daycare") && p.y==100,"arrive in free play "+game);
   Need(!Attending(w.Snapshot(),game,"p4"),"world arrival does not enter "+game);
   Need(Json(w.ReadSandpit().moulds)==pieces,"arrival retains castle");
   Need(Cmd(4,game,"start").Accepted && Attending(w.Snapshot(),game,"p4"),"explicit selection joins existing "+game);
   Need(Cmd(4,SoloAction.Travel,"park").Accepted && Cmd(4,SoloAction.Travel,"daycare").Accepted,"return world card "+game);family.AdvanceIdle(.1,out _);
   Need(w.ReadPlayer("p4").zone=="daycare" && !Attending(w.Snapshot(),game,"p4"),"return stays in free play "+game);
   Need(Enumerable.Range(1,3).All(i=>Attending(w.Snapshot(),game,"p"+i)) && Json(w.ReadSandpit().moulds)==pieces,"siblings and creation retained "+game);
   family.Detach(4);Need(family.Attach(4,"p4",out _),"entry reconnect");family.AdvanceIdle(.1,out _);
   Need(w.ReadPlayer("p4").zone=="daycare" && !Attending(w.Snapshot(),game,"p4"),"reconnect respects free play "+game);
  }
 }
}
