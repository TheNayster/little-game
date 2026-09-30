using System;
using System.Text.Json;
using LittleWeeps.Core;
class Program
{
    static SoloWorld w=null!;
    static void Check(bool v,string s){if(!v)throw new Exception(s);}
    static SoloResult Cmd(string id,SoloAction a,string value="",float x=1600,float y=70)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=id,action=a,value=value,x=x,y=y,expectedRevision=w.Revision,zone=w.ReadPlayer(id).zone,visit=w.ReadPlayer(id).visit});
    static void Join(string id){Check(Cmd(id,SoloAction.Park,"tag-join").Accepted,"join");}
    static void Tick(double seconds){while(seconds>.0001){var d=Math.Min(.05,seconds);w.AdvanceIdle(d,out _);seconds-=d;}}
    static void Main()
    {
        var old=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));var prior=old.Snapshot();w=SoloWorld.WithTag(old);
        Check(w.Schema==prior.schema && w.ReadTag()!.phase==TagPhase.Idle && w.ReadToys().Length==prior.toys.Length,"additive initialization");
        foreach(var id in new[]{"one","two","three","four"}){Check(Cmd(id,SoloAction.Travel,"park").Accepted,"park");Check(Cmd(id,SoloAction.Move,x:id=="one"?1600:id=="two"?1800:2200).Accepted,"position");}
        Join("one");var stationary=w.Revision;Tick(.1);Check(w.Revision==stationary,"countdown caused durable revision churn");var deadline=w.ReadTag()!.starts;Tick(.5);Join("two");Tick(.5);Join("three");Join("four");
        Check(w.ReadTag()!.starts==deadline && w.ReadTag()!.members.Length==4,"shared countdown");Tick(2.1);Check(w.ReadTag()!.phase==TagPhase.Playing,"common start");Tick(2.6);
        Check(Cmd("two",SoloAction.Move,x:1650).Accepted,"contact");Tick(.05);Check(w.ReadTag()!.it=="two" && w.ReadTag()!.turns==1,"automatic handoff");Tick(1);Check(w.ReadTag()!.it=="two","no instant tag back");
        Check(Cmd("two",SoloAction.ChangeAvatar,"orange-pup").Accepted && ParkTag.Member(w.ReadTag(),"two"),"character switch");
        var copy=w.Snapshot();copy.park.tag.members[0].safeUntil=123;Check(w.ReadTag()!.members[0].safeUntil!=123,"deep copy");
        var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),new JsonSerializerOptions{IncludeFields=true}),new JsonSerializerOptions{IncludeFields=true})!);Check(restored.ReadTag()!.phase==TagPhase.Idle && restored.Revision>w.Revision,"JSON/recovery clears temporary round and schedules its save");
        var session=new FamilySession(w);for(ulong i=1;i<=4;i++)Check(session.Attach(i,new[]{"one","two","three","four"}[i-1],out _),"attach");
        Check(session.Detach(2) && w.ReadTag()!.members.Length==3 && w.ReadTag()!.it!="two","tagger disconnect");
        Check(Cmd("four",SoloAction.Travel,"creek").Accepted && w.ReadTag()!.members.Length==2,"independent area departure");
        Check(Cmd("three",SoloAction.Park,"tag-leave").Accepted && w.ReadTag()!.members.Length==1,"independent leave");
        Check(w.ReleaseTag("one") && w.ReadTag()!.phase==TagPhase.Idle,"last participant exit");
        Join("one");Tick(6);Check(w.ReadTag()!.members.Length==1,"NPC solo");Check(Cmd("one",SoloAction.Move,x:w.ReadTag()!.npcX,y:w.ReadTag()!.npcY).Accepted,"NPC contact");Tick(.05);Check(w.ReadTag()!.it==ParkTag.Npc,"NPC can become it");
        Check(ParkTag.SweptDistance(new WalkPoint(0,0),new WalkPoint(400,0),new WalkPoint(400,0),new WalkPoint(0,0))==0,"swept crossing");
        Join("three");Check(w.ReadTag()!.members.Length==2 && w.ReadTag()!.it!=ParkTag.Npc,"late human replaces NPC");
        Check(Cmd("three",SoloAction.UseFixture,value:"").Accepted==false,"invalid fixture rejected");
        var bad=w.Snapshot();bad.park.tag.npcX=float.NaN;try{SoloWorld.Validate(bad);throw new Exception("invalid state accepted");}catch(InvalidOperationException){}
        Console.WriteLine("PASS shared countdown, four participants, automatic contact/grace, character switch, deep copy, old-save/JSON, independent exits, NPC fallback, swept crossing and validation");
    }
}
