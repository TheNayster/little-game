using System;
using System.Linq;
using LittleWeeps.Core;
public static class PeppaFunfairRuleChecks
{
    static readonly string[] Actors={"one","two","three","four"};
    static void Need(bool ok,string text){if(!ok)throw new Exception("Funfair: "+text);}
#if !UNITY_EDITOR
    public static void Main()=>Run();
#endif
    public static void Run()
    {
        var h=PeppaFunfair.Create(Actors);var players=Actors.Select(a=>new SoloPlayer{id=a,zone=PeppaFunfair.Zone,y=150}).ToArray();
        string Send(int who,string op,string target="",string item="",float x=0,float y=0)=>PeppaFunfair.Apply(h,players,new SoloCommand{actor=Actors[who],value=op,target=target,item=item,x=x,y=y},Actors);
        void Act(int who,string op,string target="",string item="",float x=0,float y=0){Need(Send(who,op,target,item,x,y)==null,op);PeppaFunfair.Validate(h,Actors);}
        void Near(string key){foreach(var p in players)p.x=PeppaFunfair.X(key);}
        void Tick(double seconds,string[] active=null){for(double t=0;t<seconds;t+=.1){PeppaFunfair.Advance(h,players,Math.Min(.1,seconds-t),active??Actors,out _);PeppaFunfair.Validate(h,Actors);}}
        Near("wheel");foreach(int i in Enumerable.Range(0,4))Act(i,"board","wheel");Need(h.members.Select(m=>m.seat).Distinct().Count()==4,"four shared seat assignments");Act(0,"ride-start","wheel");Tick(3.2);Need(h.wheel.stage=="running","common countdown");PeppaFunfair.Release(h,"one",players);Tick(9);Need(h.wheel.rounds==1 && h.wheel.riders.Length==0,"siblings finish after departure");
        Near("teacups");Act(1,"board","teacups");Act(2,"board","teacups");Act(1,"ride-start","teacups");Tick(3.2);Act(2,"ride-spin","teacups");Tick(9);Need(h.teacups.rounds==1,"shared teacup replay cycle");
        Near("slide");for(int i=0;i<4;i++)Act(i,"slide-join");Tick(.2);Act(0,"leave");Tick(52);Need(h.slides==3 && h.members.Skip(1).All(m=>m.photos==1),"queue survives head departure and takes photos");
        Near("hook");for(int i=0;i<4;i++)Act(i,"hook-join");Tick(1.8);int round=h.hookRound;Need(Send(1,"hook-pull",round.ToString(),"0",.1f,.2f)!=null,"only shared queue head can catch");Act(0,"hook-pull",round.ToString(),"0",PeppaFunfair.FlowerX(h.clock,0),PeppaFunfair.FlowerY(h.clock,0));Need(Send(0,"hook-pull",round.ToString(),"0",.1f,.2f)!=null,"catch cannot be duplicated");Tick(4);Need(h.catches==1 && h.members[0].prizes.Sum()==1,"one prize per target");Act(1,"leave");Act(2,"leave");Act(3,"leave");
        Near("paint");for(int i=0;i<4;i++)Act(i,"paint-join");Tick(.2);Act(1,"paint-style",item:"2");int revision=h.paintRevision;Act(2,"paint-stroke",revision.ToString(),"3",2,3);Need(Send(3,"paint-stroke",revision.ToString(),"4",2,4)!=null,"stale stroke rejected");Act(3,"paint-stroke",h.paintRevision.ToString(),"4",3,4);Act(1,"paint-done");Need(h.members[0].paintStyle==2 && h.members[0].paint.Count(v=>v>0)==2 && h.paintings==1,"siblings paint one recipient together");Act(1,"leave");Tick(.2);Need(h.paintActor=="three","recipient departure advances shared queue");Act(2,"leave");Act(3,"leave");Tick(4);
        Near("smoothie");Act(0,"smoothie-fruit",item:"0");Act(1,"smoothie-fruit",item:"1");Act(2,"smoothie-fruit",item:"2");Act(3,"smoothie-blend");Tick(1.7);Act(0,"drink-take");Need(Send(1,"drink-take")!=null,"one shared prepared drink");Act(0,"drink");Tick(2.2);Need(h.props.All(p=>p.holder==""),"finished drink releases original prop");
        Act(0,"smoothie-machine");Need(Send(1,"smoothie-machine")!=null,"shared machine cannot double-start");Tick(1.7);Act(1,"smoothie-machine");Need(h.smoothieMask==0 && h.props.Any(p=>p.kind=="drink"&&p.holder=="two"),"direct machine cup belongs to the tapping sibling");Need(Send(0,"drink-take")!=null,"prepared cup cannot be taken twice");Act(1,"drink");Tick(2.2);
        Near("slide");Act(0,"balloon-take");Act(0,"give","two");Need(h.props.Single(p=>p.location=="held").holder=="two","prop handoff");Act(1,"float");Tick(10);Need(h.members[1].station=="","balloon float lands");PeppaFunfair.Release(h,"two",players);Need(h.props.Any(p=>p.location=="ground"),"departure drops held prop");var copy=h.Copy();copy.members[0].paint[0]=8;Need(h.members[0].paint[0]==0,"nested snapshots detached");PeppaFunfair.Restore(copy);PeppaFunfair.Validate(copy,Actors);Need(copy.members.All(m=>m.station=="") && copy.catches==h.catches && copy.paintings==h.paintings,"restore clears occupancy and preserves rewards");
        Need(copy.photoActor==h.photoActor,"completed photo subject survives restore");
        Console.WriteLine("PASS Funfair shared rides, queues, painting, rewards, props, departure and restore.");
        Integration();
    }
    static void Integration()
    {
        var w=GameWorld.WithDinosaurWorld(GameWorld.Create(Actors));var legacy=w.Snapshot();legacy.schema=55;legacy.funfair=null;string identity=legacy.worldId;var upgraded=GameWorld.WithDinosaurWorld(GameWorld.Restore(legacy));Need(upgraded.ReadFunfair()!=null&&upgraded.WorldId==identity&&upgraded.ReadSpidey().props.Length==legacy.spidey.props.Length,"additive migration preserves existing world");w=upgraded;
        void Act(string actor,SoloAction action,string value,string target=""){var p=w.ReadPlayer(actor);var result=w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,actor=actor,zone=p.zone,visit=p.visit,action=action,value=value,target=target},Actors);Need(result.Accepted,value+" "+result.Outcome);GameWorld.Validate(w.Snapshot());}
        foreach(var actor in Actors)Act(actor,SoloAction.Travel,PeppaFunfair.Zone);
        foreach(var actor in Actors){var p=w.ReadPlayer(actor);Need(w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,actor=actor,zone=p.zone,visit=p.visit,action=SoloAction.Move,x=700,y=150},Actors).Accepted,"approach");Act(actor,SoloAction.Funfair,"board","wheel");}
        Act("one",SoloAction.Funfair,"ride-start","wheel");Act("one",SoloAction.Travel,"garden");for(int i=0;i<120;i++)w.AdvanceIdle(.1,out _,Actors);Need(w.ReadFunfair().wheel.rounds==1&&w.ReadPlayer("two").zone==PeppaFunfair.Zone,"authoritative dispatch, time and independent travel");
        var save=w.Snapshot();
#if UNITY_EDITOR
        save=UnityEngine.JsonUtility.FromJson<SoloSnapshot>(UnityEngine.JsonUtility.ToJson(save));
#endif
        var restored=GameWorld.Restore(save);GameWorld.Validate(restored.Snapshot());Need(restored.ReadFunfair().wheel.rounds==1&&restored.ReadFunfair().wheel.riders.Length==0,"snapshot serialization retains results and releases occupancy");
        // Exercise both real branch shapes: Funfair60 lacked Markets, while
        // the Markets fork lacked Funfair. Preserve identity and earned history.
        SoloSnapshot Roundtrip(SoloSnapshot s){
#if UNITY_EDITOR
            return UnityEngine.JsonUtility.FromJson<SoloSnapshot>(UnityEngine.JsonUtility.ToJson(s));
#else
            return s;
#endif
        }
        var funfairFork=restored.Snapshot();funfairFork.schema=60;funfairFork.markets=null;var combined=GameWorld.WithDinosaurWorld(GameWorld.Restore(Roundtrip(funfairFork)));Need(combined.WorldId==identity && combined.ReadFunfair().wheel.rounds==1 && combined.ReadMarkets()!=null,"Funfair fork gains Markets without resetting history");
        var marketsFork=combined.Snapshot();marketsFork.schema=60;marketsFork.funfair=null;foreach(var p in marketsFork.players)if(p.zone==PeppaFunfair.Zone)p.zone=MarketsRules.Zone;marketsFork.markets.photos=3;marketsFork.markets.stations[0].completed=2;marketsFork.markets.ponies[0].rounds=1;var joined=GameWorld.WithDinosaurWorld(GameWorld.Restore(Roundtrip(marketsFork)));Need(joined.WorldId==identity && joined.ReadFunfair()!=null && joined.ReadMarkets().photos==3 && joined.ReadMarkets().stations[0].completed==2 && joined.ReadMarkets().ponies[0].rounds==1,"Markets fork gains Funfair without resetting earned history");
        bool Invalid(SoloSnapshot s){try{GameWorld.Validate(s);return false;}catch(ArgumentException){return true;}catch(InvalidOperationException){return true;}}
        var missing=joined.Snapshot();missing.markets=null;Need(Invalid(missing),"current-schema missing world is rejected");missing.schema=60;Need(Invalid(missing),"legacy player cannot occupy an absent Markets world");
        Console.WriteLine("PASS both isolated world forks migrate additively; current and occupied missing-module states remain invalid.");
        Console.WriteLine("PASS Funfair GameWorld integration, additive migration and restore.");
    }
}
