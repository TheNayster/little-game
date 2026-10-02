using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Client;

static class TipChecks
{
    static void Need(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        var world=SoloWorld.WithDinosaurWorld(SoloWorld.Create("a","b","c","d"));var family=new FamilySession(world);var fx=new SandTipFeedback();
        SoloCommand Make(int a,SoloAction action,string value="",int i=0,float x=0,float y=0,int round=-1){var p=world.ReadPlayer(new[]{"a","b","c","d"}[a]);return new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=world.Revision,requestId=Guid.NewGuid().ToString("N"),action=action,value=value,target=DaycareSandpit.Target(i,round<0?world.ReadSandpit().round:round),x=x,y=y};}
        SoloResult Send(int a,SoloAction action,string value="",int i=0,float x=0,float y=0,int round=-1)=>family.Submit((ulong)(a+1),Make(a,action,value,i,x,y,round));
        void Work(int a,int i){var p=DaycareSandpit.Place(i);Need(Send(a,SoloAction.Move,x:p.X,y:p.Y).Accepted,"work");}
        SoloResult Tool(int a,string op,int i,bool observe=true){Work(a,i);var r=Send(a,SoloAction.Sandpit,op,i);if(observe)fx.Observe(world.ReadSandpit(),true,.01f);return r;}
        for(var a=0;a<4;a++){Need(family.Attach((ulong)(a+1),new[]{"a","b","c","d"}[a],out _),"attach");Need(Send(a,SoloAction.Travel,"daycare").Accepted,"travel");}
        Need(Send(0,SoloAction.Sandpit,"start").Accepted,"start");family.AdvanceIdle(.1,out _);fx.Observe(world.ReadSandpit(),true,0);
        Need(Tool(0,"scoop",0).Accepted,"one scoop");Need(Tool(0,"tip",0).Outcome=="fill-bucket-first" && world.ReadSandpit().moulds[0].scoops==1,"underfilled retains progress");fx.Underfilled(0,1);Need(fx.Outcome(0)=="underfilled" && fx.Events[0]==0,"only wiggle");fx.Observe(world.ReadSandpit(),true,1);
        // Client last saw one scoop; authority accepts the final scoop before Tip.
        Need(Tool(1,"scoop",0,false).Accepted && Tool(0,"tip",0).Accepted,"scoop before tip");Need(fx.Outcome(0)=="collapse" && fx.Events[0]==1 && world.ReadSandpit().moulds[0].scoops==0 && !world.ReadSandpit().moulds[0].built,"confirmed dry reset even with coalesced final scoop");
        Need(Tool(1,"tip",0).Outcome=="fill-bucket-first" && fx.Events[0]==1,"repeat dry Tip cannot collapse twice");
        Need(Tool(0,"scoop",0).Accepted && Tool(0,"scoop",0).Accepted,"refill");Work(0,0);Work(1,0);var tip=Make(0,SoloAction.Sandpit,"tip",0);
        Need(Tool(1,"water",0,false).Accepted,"water wins before tip");Need(family.Submit(1,tip).Outcome=="stale-revision","conflict");tip.expectedRevision=world.Revision;Need(family.Submit(1,tip).Accepted,"same lesson retry");fx.Observe(world.ReadSandpit(),true,0);
        Need(fx.Outcome(0)=="reveal" && fx.Events[0]==2 && world.ReadSandpit().moulds[0].built,"wet outcome overrides stale dry belief");
        Need(family.Submit(1,tip).Duplicate,"duplicate receipt");fx.Observe(world.ReadSandpit(),true,0);Need(Tool(1,"tip",0).Outcome=="tower-built" && fx.Events[0]==2,"no duplicate reveal");
        for(var i=1;i<4;i++){for(var n=0;n<DaycareSandpit.Capacity(i);n++)Need(Tool(i,"scoop",i).Accepted,"fill");Need(Tool(i,"water",i).Accepted,"wet");Need(Tool(i,"tip",i).Accepted,"build different mould");}
        Need(fx.Events.SequenceEqual(new[]{2,1,1,1}),"independent transitions");Need(Enumerable.Range(1,3).All(i=>fx.Outcome(i)=="reveal"),"independent reveals");
        var saved=JsonSerializer.Serialize(world.Snapshot(),new JsonSerializerOptions{IncludeFields=true});var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(saved,new JsonSerializerOptions{IncludeFields=true}));Need(restored.ReadSandpit().moulds.All(m=>m.built && m.wet),"checkpoint retains towers");
        var late=new SandTipFeedback();late.Observe(restored.ReadSandpit(),true,0);Need(late.Events.All(e=>e==0) && Enumerable.Range(0,4).All(i=>late.Remaining(i)==0),"late/reconnect baseline has no half reveal");
        Need(Send(2,SoloAction.Sandpit,"leave").Accepted && world.ReadSandpit().moulds.All(m=>m.built),"independent leave preserves siblings");fx.Observe(world.ReadSandpit(),false,0);Need(Enumerable.Range(0,4).All(i=>fx.Remaining(i)==0),"leave clears effects");
        fx.Observe(world.ReadSandpit(),true,0);for(var n=0;n<400 && world.ReadSandpit().phase<3;n++)family.AdvanceIdle(.1,out _);Need(Send(0,SoloAction.Sandpit,"replay").Accepted,"reset");fx.Observe(world.ReadSandpit(),true,0);Need(Enumerable.Range(0,4).All(i=>fx.Remaining(i)==0),"reset clears effects without collapse");
        Work(1,1);var stale=Send(1,SoloAction.Sandpit,"tip",1,round:1);Need(stale.Outcome=="old-sandpit-lesson" && world.ReadSandpit().moulds.All(m=>m.scoops==0 && !m.built),"old tip cannot mutate reset");
        // Two clients submit against one revision. The losing request is rebased,
        // reaches the existing rejection rule and never starts a second effect.
        for(var n=0;n<3;n++)Need(Tool(0,"scoop",1).Accepted,"fill dry big");Work(0,1);Work(1,1);var first=Make(0,SoloAction.Sandpit,"tip",1);var second=Make(1,SoloAction.Sandpit,"tip",1);
        Need(family.Submit(1,first).Accepted,"first dry tip");fx.Observe(world.ReadSandpit(),true,0);var events=fx.Events[1];Need(family.Submit(2,second).Outcome=="stale-revision","second conflicts");second.expectedRevision=world.Revision;Need(family.Submit(2,second).Outcome=="fill-bucket-first","second rejects after retry");fx.Observe(world.ReadSandpit(),true,.1f);Need(fx.Events[1]==events && world.ReadSandpit().moulds[1].scoops==0,"one dry reset/effect");fx.Observe(world.ReadSandpit(),true,3);Need(fx.Remaining(1)==0,"bounded expiry");
        Console.WriteLine("PASS: Tip underfill/wiggle, dry reset once, coalesced scoop-before-tip, water-before-tip revision race, duplicate/redundant Tip, four towers, restore/late baseline, independent leave, reset/stale Tip and effect expiry.");
    }
}
