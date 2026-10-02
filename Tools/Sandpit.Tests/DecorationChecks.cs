using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Client;

static class DecorationChecks
{
    static void Need(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        var world=SoloWorld.WithDinosaurWorld(SoloWorld.Create("a","b","c","d"));var family=new FamilySession(world);var fx=new SandDecorationFeedback();
        SoloCommand Make(int a,SoloAction action,string value="",int i=0,int round=-1,float x=0,float y=0){var p=world.ReadPlayer(new[]{"a","b","c","d"}[a]);return new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=world.Revision,requestId=Guid.NewGuid().ToString("N"),action=action,value=value,target=DaycareSandpit.Target(i,round<0?world.ReadSandpit().round:round),x=x,y=y};}
        SoloResult Send(int a,SoloAction action,string value="",int i=0,int round=-1,float x=0,float y=0)=>family.Submit((ulong)(a+1),Make(a,action,value,i,round,x,y));
        void Work(int a,int i){var p=DaycareSandpit.Place(i);Need(Send(a,SoloAction.Move,x:p.X,y:p.Y).Accepted,"work");}
        SoloResult Tool(int a,string op,int i){Work(a,i);var result=Send(a,SoloAction.Sandpit,op,i);fx.Observe(world.ReadSandpit(),true,.01f);return result;}
        string Moulds()=>JsonSerializer.Serialize(world.ReadSandpit().moulds,new JsonSerializerOptions{IncludeFields=true});
        for(var a=0;a<4;a++){Need(family.Attach((ulong)(a+1),new[]{"a","b","c","d"}[a],out _),"attach");Need(Send(a,SoloAction.Travel,a==3?"park":"daycare").Accepted,"travel");}
        Need(Send(0,SoloAction.Sandpit,"start").Accepted,"start");family.AdvanceIdle(.1,out _);fx.Observe(world.ReadSandpit(),true,0);
        foreach(var op in new[]{"flag","shell"}){var before=Moulds();Need(Tool(0,op,0).Outcome=="try-sand-tools" && Moulds()==before,"unbuilt rejects "+op);fx.Reject(op,1);Need(fx.Events.All(e=>e==0) && fx.Wiggle(op=="flag"?0:1)>0,"invalid only wiggles source");}
        void Build(int i){for(var n=0;n<DaycareSandpit.Capacity(i);n++)Need(Tool(0,"scoop",i).Accepted,"solo fill");Need(Tool(0,"water",i).Accepted && Tool(0,"tip",i).Accepted,"solo build");}
        Build(0);Need(Tool(0,"flag",0).Accepted && world.ReadSandpit().moulds[0].decoration==1 && fx.Events[0]==1 && fx.Value(0)==1,"flag transition");
        Need(Tool(1,"shell",0).Accepted && world.ReadSandpit().moulds[0].decoration==2 && fx.Events[0]==2 && fx.Value(0)==2,"shell replaces flag and old effect");
        Need(Tool(0,"flag",0).Accepted && fx.Value(0)==1 && fx.Events[0]==3,"flag replaces shell");var count=fx.Events[0];Need(Tool(1,"flag",0).Accepted && fx.Events[0]==count && fx.Remaining(0)<SandDecorationFeedback.Duration,"redundant flag accepted without new effect");
        Build(1);Work(0,1);Work(1,1);var flag=Make(0,SoloAction.Sandpit,"flag",1);var shell=Make(1,SoloAction.Sandpit,"shell",1);
        Need(family.Submit(1,flag).Accepted,"first concurrent flag");fx.Observe(world.ReadSandpit(),true,0);Need(family.Submit(2,shell).Outcome=="stale-revision","concurrent shell conflicts");shell.expectedRevision=world.Revision;Need(family.Submit(2,shell).Accepted,"rebased shell wins");fx.Observe(world.ReadSandpit(),true,0);count=fx.Events[1];Need(fx.Value(1)==2 && world.ReadSandpit().moulds[1].decoration==2,"one final shell");
        Need(family.Submit(2,shell).Duplicate,"duplicate receipt");fx.Observe(world.ReadSandpit(),true,0);Need(fx.Events[1]==count,"duplicate no placement");
        Build(2);Build(3);Need(world.ReadSandpit().moulds.All(m=>m.built),"one child builds all towers");
        Need(Send(3,SoloAction.Travel,"daycare").Accepted,"late arrival");family.AdvanceIdle(.1,out _);var late=new SandDecorationFeedback();late.Observe(world.ReadSandpit(),true,0);Need(late.Events.All(e=>e==0) && Enumerable.Range(0,4).All(i=>late.Remaining(i)==0),"late join has final props without replay");
        for(var i=0;i<4;i++)Need(Tool(i,i%2==0?"shell":"flag",i).Accepted,"four mixed decorations");Need(world.ReadSandpit().moulds.Select(m=>m.decoration).SequenceEqual(new[]{2,1,2,1}),"four independent authoritative slots");
        var options=new JsonSerializerOptions{IncludeFields=true};var restored=SoloWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(world.Snapshot(),options),options));Need(restored.ReadSandpit().moulds.Select(m=>m.decoration).SequenceEqual(new[]{2,1,2,1}),"JSON retains both decorations");
        var beforeLeave=Moulds();Need(Send(2,SoloAction.Sandpit,"leave").Accepted && Moulds()==beforeLeave,"independent leave preserves castle");fx.Observe(world.ReadSandpit(),false,0);Need(Enumerable.Range(0,4).All(i=>fx.Remaining(i)==0) && fx.Wiggle(0)==0 && fx.Wiggle(1)==0,"leave clears local props/wiggles");
        Need(family.Detach(3) && family.Attach(3,"c",out _),"reconnect");family.AdvanceIdle(.1,out _);Need(Moulds()==beforeLeave,"reconnect retains decorations");fx.Observe(world.ReadSandpit(),true,0);var oldEvents=fx.Events;
        for(var n=0;n<400 && world.ReadSandpit().phase<3;n++)family.AdvanceIdle(.1,out _);Work(1,0);var pending=Make(1,SoloAction.Sandpit,"shell",0);Need(Send(0,SoloAction.Sandpit,"replay").Accepted,"reset");fx.Observe(world.ReadSandpit(),true,0);
        pending.expectedRevision=world.Revision;Need(family.Submit(2,pending).Outcome=="old-sandpit-lesson","rebased old decoration rejects");Need(world.ReadSandpit().moulds.All(m=>!m.built && m.decoration==0) && fx.Events.SequenceEqual(oldEvents) && Enumerable.Range(0,4).All(i=>fx.Remaining(i)==0),"reset clears old props without false feedback");
        fx.Reject("flag",1);Need(fx.Wiggle(0)==0,"stale rejection cannot animate new lesson");SoloWorld.Validate(world.Snapshot());
        Console.WriteLine("PASS: Flag/Shell built eligibility, replacement, redundant/duplicate feedback, same-tower revision conflict/rebase, four mixed slots, solo completion, JSON/late/reconnect/independent leave, reset/stale command and local cleanup.");
    }
}
