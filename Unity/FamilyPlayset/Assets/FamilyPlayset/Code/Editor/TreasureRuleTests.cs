using System;
using System.Linq;
using UnityEngine;
using LittleWeeps.Core;
namespace LittleWeeps.EditorTools { public static class TreasureRuleTests {
 static SoloWorld w;static FamilySession family;
 static void Need(bool v,string why){if(!v)throw new Exception(why);}
 static SoloResult Cmd(int a,SoloAction action,string value="",string target="",float x=0,float y=0){var p=w.ReadPlayer("p"+a);return family.Submit((ulong)a,new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y});}
 static void Do(int a,string op,string target){var g=w.ReadTreasure();var at=op=="wheel"?TreasureHunt.Dig(g.site):TreasureHunt.Point(g,target);Need(Cmd(a,SoloAction.Move,x:at.X,y:at.Y).Accepted,"move");var r=Cmd(a,SoloAction.Treasure,op,target);Need(r.Accepted,op+" "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
 public static void Run(){
  w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=w.Snapshot();old.schema=46;old.treasure=null;var id=old.worldId;var toys=old.toys.Select(t=>t.id).ToArray();var sand=old.sandpit.friends.ToArray();w=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(old));Need(w.Schema==47 && w.Snapshot().worldId==id && w.Snapshot().toys.Select(t=>t.id).SequenceEqual(toys) && w.ReadSandpit().friends.SequenceEqual(sand),"additive46 migration");family=new FamilySession(w);
  for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.ChangeAvatar,"blue-pup").Accepted,"same player avatar");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"travel");}
  Need(Cmd(1,SoloAction.Treasure,"start").Accepted,"start");var g=w.ReadTreasure();var cast=g.friends.ToArray();Need(g.members.Count(m=>m.attending)==3 && w.Snapshot().players.Take(3).All(p=>p.zone==TreasureHunt.Zone),"one start travels group");Need(DaycareNpcCasts.Valid(cast,3),"random distinct children ignore player avatars");Need(Cmd(1,SoloAction.Treasure,"begin").Accepted,"opening");
  Do(1,"lift","cover-"+(g.shell+1)%3);Need(w.ReadTreasure().lastFind==1 && w.ReadTreasure().phase==2,"friendly wrong find");Do(2,"lift","cover-"+g.shell);Do(1,"take","cover-"+g.shell);
  Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late travel");family.AdvanceIdle(.1,out _);Need(w.ReadTreasure().members.All(m=>m.attending) && w.ReadTreasure().friends.SequenceEqual(cast) && w.ReadTreasure().phase==3,"late fourth same progress");
  Do(3,"lift","cover-"+(3+g.pot));Do(4,"take","cover-"+(3+g.pot));Do(1,"note",((g.tune[0]+1)%3).ToString());Need(w.ReadTreasure().tuneStep==0,"gentle tune restart");Do(1,"note",g.tune[0].ToString());Do(2,"note",g.tune[1].ToString());Need(Cmd(3,SoloAction.Treasure,"leave").Accepted,"independent leave");family.AdvanceIdle(.1,out _);Need(w.ReadTreasure().members.Count(m=>m.attending)==3 && w.ReadTreasure().tuneStep==2,"leave keeps partial shared puzzle");
  Need(family.Detach(2),"disconnect");Need(family.Attach(2,"p2",out _),"reconnect");Need(w.ReadTreasure().members.Single(m=>m.actor=="p2").attending && w.ReadTreasure().tuneStep==2,"reconnect keeps song");
  w=SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot())));Need(w.ReadTreasure().friends.SequenceEqual(cast) && w.ReadTreasure().tuneStep==2,"JSON partial puzzle cast retention");family=new FamilySession(w);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);Do(4,"note",g.tune[2].ToString());Do(1,"dig","dig-"+(g.site+1)%3);Need(w.ReadTreasure().phase==5,"wrong dig keeps map");Do(2,"dig","dig-"+g.site);
  Need(!Cmd(2,SoloAction.Treasure,"open","chest").Accepted,"wrong lock rejected");for(var i=0;i<3;i++)for(var n=0;n<g.tune[i];n++)Do(i%2+1,"wheel",i.ToString());Do(1,"open","chest");Need(w.ReadTreasure().phase==7,"shared chest opens");
  Need(Cmd(3,SoloAction.Travel,"park").Accepted && Cmd(3,SoloAction.Travel,"daycare").Accepted,"return");family.AdvanceIdle(.1,out _);Need(w.ReadTreasure().members.All(m=>m.attending) && w.ReadTreasure().phase==7,"return joins completion");Need(Cmd(1,SoloAction.Treasure,"replay").Accepted,"new hunt");Need(w.ReadTreasure().round==2 && !w.ReadTreasure().friends.Intersect(cast).Any() && w.ReadTreasure().lifted.All(b=>!b),"deliberate reset rerolls only new hunt");SoloWorld.Validate(w.Snapshot());
  Debug.Log("PASS additive46 migration; all-Bluey players/distinct saved NPCs; shared world start, wrong searches, cross-player map/tune/chest puzzles, late join, independent exit, reconnect, partial JSON retention, return and new hunt.");
 }
}

}