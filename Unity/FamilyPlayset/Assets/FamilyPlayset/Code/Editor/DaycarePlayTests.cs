using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class DaycarePlayTests
    {
        private static GameWorld world;private static FamilySession family;
        private static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
        private static SoloResult Cmd(int i,SoloAction action,string value="",string target="",float x=0,float y=0)
        {var p=world.ReadPlayer("p"+i);return family.Submit((ulong)i,new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=world.Revision,action=action,value=value,target=target,x=x,y=y});}
        private static void Club(int i,string op,string target=""){var r=Cmd(i,SoloAction.DaycarePlay,op,target);Need(r.Accepted,op+": "+r.Outcome);GameWorld.Validate(world.Snapshot());}
        private static void Advance(double seconds){while(seconds>0){var dt=Math.Min(.1,seconds);family.AdvanceIdle(dt,out _);seconds-=dt;}GameWorld.Validate(world.Snapshot());}
        private static void TagRoutes()
        {
            foreach(var npcChaser in new[]{false,true})foreach(var edge in new[]{80f,2320f}){
                var w=GameWorld.WithDinosaurWorld(GameWorld.Create("a","b","c","d"));
                SoloResult Send(string actor,SoloAction action,string value,string target=""){
                    var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target});
                }
                foreach(var actor in new[]{"a","b","c","d"})Need(Send(actor,SoloAction.Travel,"daycare").Accepted,"route fixture arrival");
                Need(Send("a",SoloAction.DaycarePlay,"tag:start").Accepted,"route fixture start");w.InviteDaycarePlay(new[]{"a","b","c","d"});
                foreach(var actor in new[]{"b","c","d"})Need(Send(actor,SoloAction.DaycarePlay,"tag:join","1").Accepted,"route fixture join");
                var save=w.Snapshot();var g=save.tagClub;g.phase=ClubPhase.Playing;g.grace=1000;g.it=npcChaser?DaycarePlay.NpcId(0):"a";
                save.players[0].x=edge;save.players[0].y=60;
                for(var i=0;i<4;i++){g.npcs[i].x=i%2==0?80:2320;g.npcs[i].y=i<2?60:240;}
                w=GameWorld.Restore(save);
                var minX=g.npcs.Select(n=>n.x).ToArray();var maxX=minX.ToArray();var minY=g.npcs.Select(n=>n.y).ToArray();var maxY=minY.ToArray();
                var signs=new int[4];var turns=new int[4];var edges=new double[4];var maxEdges=new double[4];
                for(var tick=0;tick<600;tick++){
                    var before=w.ReadDaycarePlay(true);w.AdvanceIdle(.05,out _,new[]{"a","b","c","d"});var after=w.ReadDaycarePlay(true);
                    GameWorld.Validate(w.Snapshot());
                    for(var i=npcChaser?1:0;i<4;i++){
                        var n=after.npcs[i];var dx=n.x-before.npcs[i].x;var dy=n.y-before.npcs[i].y;
                        Need(dx*dx+dy*dy>1,"runner stopped or oscillated in place");
                        minX[i]=Math.Min(minX[i],n.x);maxX[i]=Math.Max(maxX[i],n.x);minY[i]=Math.Min(minY[i],n.y);maxY[i]=Math.Max(maxY[i],n.y);
                        if(Math.Abs(dx)>1){var sign=Math.Sign(dx);if(signs[i]!=0 && signs[i]!=sign)turns[i]++;signs[i]=sign;}
                        edges[i]=n.x<180 || n.x>2220 || n.y<70 || n.y>230?edges[i]+.05:0;maxEdges[i]=Math.Max(maxEdges[i],edges[i]);
                    }
                }
                for(var i=npcChaser?1:0;i<4;i++){
                    Need(maxEdges[i]<1.2,"runner remains at arena edge");
                    Need(maxX[i]-minX[i]>700 && maxY[i]-minY[i]>50 && turns[i]>=2,"runner lacks varied interior routes");
                }
                Need(Send("d",SoloAction.DaycarePlay,"tag:leave").Accepted && w.ReadDaycarePlay(true).members.Count(m=>m.attending)==3,"route change interrupts independent departure");
            }
            Debug.Log("DAYCARE_TAG_ROUTES_PASS: both walls, four corner starts, human/NPC chasers, 2400 bounded movement ticks, continuous movement, inward departure, varied lanes and repeated turns, independent exit.");
        }
        public static void Run()
        {
            world=GameWorld.WithDinosaurWorld(GameWorld.Create("p1","p2","p3","p4"));var old=world.Snapshot();old.schema=48;old.hideClub=null;old.tagClub=null;
            var id=old.worldId;var clinic=JsonUtility.ToJson(old.vet);world=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));
            Need(world.Schema==WorldLayout.Schema && world.WorldId==id && JsonUtility.ToJson(world.Snapshot().vet)==clinic,"additive48 migration retains clinic");family=new FamilySession(world);
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
            var saved=JsonUtility.ToJson(world.Snapshot());var newCast=world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).ToArray();world=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(saved));family=new FamilySession(world);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);
            Need(world.ReadDaycarePlay(false).round==2 && world.ReadDaycarePlay(false).npcs.Select(n=>n.avatar).SequenceEqual(newCast),"saved reopening retains NPCs and round");
            Club(1,"hide:leave");Club(2,"hide:leave");Club(1,"tag:start");Need(world.ReadDaycarePlay(false).members.All(m=>!m.invited),"new game replaces older outstanding invitations");var t=world.ReadDaycarePlay(true);Need(t.npcs.Length==4 && t.members.Count(m=>m.attending)==1,"Tag has four real NPC runners even solo");
            Club(2,"tag:join","1");Advance(6);t=world.ReadDaycarePlay(true);var npc=t.npcs[0];Need(Cmd(1,SoloAction.Move,x:npc.x,y:npc.y).Accepted,"chase runner");Advance(.1);
            Need(world.ReadDaycarePlay(true).turns>0,"authority proximity transfers star to runner");Club(2,"tag:leave");family.Detach(1);family.Attach(1,"p1",out _);
            Need(world.ReadDaycarePlay(true).members[0].attending && !world.ReadDaycarePlay(true).members[1].attending,"sole player reconnect resumes; sibling stays out");
            TagRoutes();
            GameWorld.Validate(world.Snapshot());Debug.Log("DAYCARE_PLAY_PASS: additive48 retention, optional same-world invitations/decline/late fourth, independent avatar and saved four-NPC cast, random distinct covers, 15s shared count, Calypso physical search, no late hiding, independent exit, replay, JSON reopening, Tag runners/contact and sole reconnect.");
        }
    }
}
