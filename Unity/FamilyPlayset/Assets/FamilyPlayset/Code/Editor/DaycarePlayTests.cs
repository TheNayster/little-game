using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class DaycarePlayTests
    {
        private static SoloWorld world;private static FamilySession family;
        private static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
        private static SoloResult Cmd(int i,SoloAction action,string value="",string target="",float x=0,float y=0)
        {var p=world.ReadPlayer("p"+i);return family.Submit((ulong)i,new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=world.Revision,action=action,value=value,target=target,x=x,y=y});}
        private static void Club(int i,string op,string target=""){var r=Cmd(i,SoloAction.DaycarePlay,op,target);Need(r.Accepted,op+": "+r.Outcome);SoloWorld.Validate(world.Snapshot());}
        private static void Advance(double seconds){while(seconds>0){var dt=Math.Min(.1,seconds);family.AdvanceIdle(dt,out _);seconds-=dt;}SoloWorld.Validate(world.Snapshot());}
        public static void Run()
        {
            world=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=world.Snapshot();old.schema=48;old.hideClub=null;old.tagClub=null;
            var id=old.worldId;var clinic=JsonUtility.ToJson(old.vet);world=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));
            Need(world.Schema==49 && world.WorldId==id && JsonUtility.ToJson(world.Snapshot().vet)==clinic,"additive48 migration retains clinic");family=new FamilySession(world);
            for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.Travel,i==4?"creek":"daycare").Accepted,"arrive");}
            Club(1,"hide:start");var g=world.ReadDaycarePlay(false);var cast=g.npcs.Select(n=>n.avatar).ToArray();var spots=g.npcs.Select(n=>n.slot).ToArray();
            Need(g.members.Count(m=>m.attending)==1 && g.members.Count(m=>m.invited)==2 && world.ReadPlayer("p2").zone=="daycare" && !g.members[3].invited,"small invitations do not force peers or other worlds");
            Need(g.npcs.Length==4 && DaycareNpcCasts.Valid(cast,4) && spots.Distinct().Count()==4,"four varied NPCs choose distinct places");
            Need(Cmd(2,SoloAction.ChangeAvatar,cast[0]).Accepted,"human may match NPC");Club(2,"hide:join","1");Club(3,"hide:decline","1");Advance(.5);
            Need(world.ReadPlayer("p3").zone=="daycare" && !world.ReadDaycarePlay(false).members[2].invited,"decline stays dismissed");Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late Daycare entry");Advance(.1);Club(4,"hide:join","1");
            var place=DaycarePlay.Cover(0);Need(Cmd(1,SoloAction.Move,x:place.X,y:90).Accepted,"walk to cover");Club(1,"hide:hide","0@1");Advance(3);
            Need(world.ReadDaycarePlay(false).npcs.Any(n=>n.x!=560 && n.hidden) && world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).SequenceEqual(cast),"NPCs walk and hide, joins retain cast");
            Advance(12);Need(world.ReadDaycarePlay(false).phase!=ClubPhase.Counting,"one common 15 second count");Need(!Cmd(2,SoloAction.DaycarePlay,"hide:hide","1@1").Accepted,"no mid-search rehide");
            Club(4,"hide:leave");Need(world.ReadDaycarePlay(false).members.Count(m=>m.attending)==2,"independent departure");Advance(30);g=world.ReadDaycarePlay(false);
            Need(g.phase==ClubPhase.Finished && g.npcs.All(n=>n.found) && g.members[0].found,"Calypso inspects and finds human and four NPCs");
            Club(1,"hide:replay");Need(!world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).Intersect(cast).Any(),"new round alone rerolls the four friends");
            var saved=JsonUtility.ToJson(world.Snapshot());var newCast=world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).ToArray();world=SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(saved));family=new FamilySession(world);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);
            Need(world.ReadDaycarePlay(false).round==2 && world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).SequenceEqual(newCast),"saved reopening retains NPCs and round");
            Club(1,"hide:leave");Club(2,"hide:leave");Club(1,"tag:start");Need(world.ReadDaycarePlay(false).members.All(m=>!m.invited),"new game replaces older outstanding invitations");var t=world.ReadDaycarePlay(true);Need(t.npcs.Length==4 && t.members.Count(m=>m.attending)==1,"Tag has four real NPC runners even solo");
            Club(2,"tag:join","1");Advance(6);t=world.ReadDaycarePlay(true);var npc=t.npcs[0];Need(Cmd(1,SoloAction.Move,x:npc.x,y:npc.y).Accepted,"chase runner");Advance(.1);
            Need(world.ReadDaycarePlay(true).turns>0,"authority proximity transfers star to runner");Club(2,"tag:leave");family.Detach(1);family.Attach(1,"p1",out _);
            Need(world.ReadDaycarePlay(true).members[0].attending && !world.ReadDaycarePlay(true).members[1].attending,"sole player reconnect resumes; sibling stays out");
            SoloWorld.Validate(world.Snapshot());Debug.Log("DAYCARE_PLAY_PASS: additive48 retention, optional same-world invitations/decline/late fourth, independent avatar and saved four-NPC cast, random distinct covers, 15s shared count, Calypso physical search, no late hiding, independent exit, replay, JSON reopening, Tag runners/contact and sole reconnect.");
        }
    }
}
