using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Client;

static class WaterChecks
{
    static void Need(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        var world=SoloWorld.WithDinosaurWorld(SoloWorld.Create("a","b","c","d"));
        var family=new FamilySession(world);var feedback=new SandWaterFeedback();
        SoloCommand Make(int actor,SoloAction action,string value="",int index=0,int round=-1,float x=0,float y=0){
            var p=world.ReadPlayer(new[]{"a","b","c","d"}[actor]);
            return new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=world.Revision,requestId=Guid.NewGuid().ToString("N"),action=action,value=value,target=DaycareSandpit.Target(index,round<0?world.ReadSandpit().round:round),x=x,y=y};
        }
        SoloResult Send(int actor,SoloAction action,string value="",int index=0,int round=-1,float x=0,float y=0)=>family.Submit((ulong)(actor+1),Make(actor,action,value,index,round,x,y));
        void Work(int actor,int index){var at=DaycareSandpit.Place(index);Need(Send(actor,SoloAction.Move,x:at.X,y:at.Y).Accepted,"move to mould");}
        SoloResult Tool(int actor,string op,int index,int round=-1){Work(actor,index);var result=Send(actor,SoloAction.Sandpit,op,index,round);feedback.Observe(world.ReadSandpit(),true,.05f);SoloWorld.Validate(world.Snapshot());return result;}
        string Moulds()=>JsonSerializer.Serialize(world.ReadSandpit().moulds,new JsonSerializerOptions{IncludeFields=true});
        for(var i=0;i<4;i++){Need(family.Attach((ulong)(i+1),new[]{"a","b","c","d"}[i],out _),"attach");Need(Send(i,SoloAction.Travel,i==3?"park":"daycare").Accepted,"travel");}
        Need(Send(0,SoloAction.Sandpit,"start").Accepted,"start");family.AdvanceIdle(.1,out _);feedback.Observe(world.ReadSandpit(),true,0);
        Need(Tool(0,"scoop",0).Accepted && Tool(1,"water",0).Accepted && Tool(0,"scoop",0).Accepted,"scoop/water/scoop interleave");
        Need(world.ReadSandpit().moulds[0].wet && world.ReadSandpit().moulds[0].scoops==2 && feedback.Events[0]==1,"water and full scoops coexist");
        Need(Tool(1,"water",1).Accepted && world.ReadSandpit().moulds[1].scoops==0 && world.ReadSandpit().moulds[1].wet && feedback.Events[1]==1,"water before sand");
        var before=Moulds();Need(Tool(0,"water",1).Accepted && Moulds()==before && feedback.Events[1]==1,"already-wet water is accepted/idempotent without a new transition effect");
        Need(feedback.Remaining(1)<SandWaterFeedback.Duration,"redundant water did not restart pour");
        Work(0,2);Work(1,2);var first=Make(0,SoloAction.Sandpit,"water",2);var second=Make(1,SoloAction.Sandpit,"water",2);
        Need(family.Submit(1,first).Accepted,"same mould first water");feedback.Observe(world.ReadSandpit(),true,.05f);
        Need(family.Submit(2,second).Outcome=="stale-revision","same mould revision conflict");
        second.expectedRevision=world.Revision;Need(family.Submit(2,second).Accepted,"same mould rebased idempotent water");feedback.Observe(world.ReadSandpit(),true,.05f);
        Need(world.ReadSandpit().moulds[2].wet && feedback.Events[2]==1,"one shared transition for two requests");
        Need(Send(3,SoloAction.Travel,"daycare").Accepted,"late join");family.AdvanceIdle(.1,out _);
        Need(world.ReadSandpit().members.All(m=>m.attending),"late fourth joins");
        var late=new SandWaterFeedback();late.Observe(world.ReadSandpit(),true,0);Need(late.Events.All(e=>e==0),"late join does not pour old wet state");
        Need(Tool(3,"water",3).Accepted && world.ReadSandpit().moulds.All(m=>m.wet),"four different moulds remain wet");
        before=Moulds();Need(Send(2,SoloAction.Sandpit,"leave").Accepted && Moulds()==before,"independent exit preserves construction");
        Need(family.Detach(3) && family.Attach(3,"c",out _),"profile reconnect");family.AdvanceIdle(.1,out _);
        Need(world.ReadSandpit().moulds.All(m=>m.wet),"reconnect retains wet state");
        // Explicit Leave lasts for this visit; returning starts the next visit.
        Need(Send(2,SoloAction.Travel,"park").Accepted && Send(2,SoloAction.Travel,"daycare").Accepted,"return after explicit Leave");family.AdvanceIdle(.1,out _);
        feedback.Observe(world.ReadSandpit(),false,0);Need(Enumerable.Range(0,4).All(i=>feedback.Remaining(i)==0),"leave/pause clears active pour");
        var events=feedback.Events;feedback.Observe(world.ReadSandpit(),true,0);Need(feedback.Events.SequenceEqual(events),"re-entry is a baseline");
        var originalRound=world.ReadSandpit().round;
        for(var i=0;i<4;i++){
            while(world.ReadSandpit().moulds[i].scoops<DaycareSandpit.Capacity(i))Need(Tool(i,"scoop",i).Accepted,"fill");
            Need(Tool(i,"water",i).Accepted,"full unbuilt mould still accepts water");Need(Tool(i,"tip",i).Accepted,"existing wet tip");
            before=Moulds();Need(Tool(i,"water",i).Outcome=="tower-built" && Moulds()==before && feedback.Events.SequenceEqual(events),"built rejects water without false pour");
        }
        for(var n=0;n<400 && world.ReadSandpit().phase<3;n++)family.AdvanceIdle(.1,out _);
        Need(Send(1,SoloAction.Sandpit,"replay").Accepted,"new lesson");feedback.Observe(world.ReadSandpit(),true,0);before=Moulds();
        Need(Tool(0,"water",0,originalRound).Outcome=="old-sandpit-lesson" && Moulds()==before && feedback.Events.SequenceEqual(events),"stale water cannot wet a new lesson or start an effect");
        Need(Tool(0,"water",0).Accepted && feedback.Remaining(0)==SandWaterFeedback.Duration,"current lesson water starts a new effect");
        feedback.Observe(world.ReadSandpit(),true,2);Need(feedback.Remaining(0)==0 && world.ReadSandpit().moulds[0].wet,"pour expires; authority remains wet");
        for(var n=0;n<DaycareSandpit.Capacity(1);n++)Need(Tool(1,"scoop",1).Accepted,"fill a dry big mould");
        Need(!world.ReadSandpit().moulds[1].wet && Tool(1,"water",1).Accepted && world.ReadSandpit().moulds[1].wet && world.ReadSandpit().moulds[1].scoops==3,"full dry mould still accepts Water");
        Console.WriteLine("PASS: authoritative Water before sand, full/already-wet/built rules, same-mould revision conflict/idempotence, Scoop-Water interleave, four moulds, late join/reconnect/exit, accepted-transition-only pour, expiry/re-entry/reset/stale protection.");
    }
}
