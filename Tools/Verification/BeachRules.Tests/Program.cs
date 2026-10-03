using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
static class Program
{
    static void Need(bool value,string why){if(!value)throw new Exception(why);}
    static SoloResult Send(GameWorld w,string actor,SoloAction action,string value,string target="")
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=target});}
    static void Main()
    {
        var actors=new[]{"one","two","three","four"};
        var w=GameWorld.WithSeagulls(GameWorld.Create(actors));
        foreach(var id in actors)Need(Send(w,id,SoloAction.Travel,"beach").Accepted,"Beach arrival");
        var s=w.Snapshot();
        foreach(var p in s.players){p.x=BeachSeagulls.Patches[0]+200;p.y=BeachSeagulls.GroundY;}
        w=GameWorld.Restore(s);
        Need(Send(w,"one",SoloAction.Beach,"hello","flock").Accepted,"Bird touch starts shared encounter");
        Need(w.ReadSeagulls().phase==GullPhase.Notice && w.ReadSeagulls().flights==1,"One common flock");
        var goal=w.ReadSeagulls().to;
        Need(Send(w,"one",SoloAction.Travel,"creek").Accepted,"Independent travel");
        for(var i=0;i<150;i++)w.AdvanceIdle(.05,out _,actors.Skip(1).ToArray());
        var landed=w.ReadSeagulls();
        Need(landed.phase==GullPhase.Resting && landed.from==goal && landed.flights==1,"Remaining siblings finish same flight");
        var clock=landed.clock;w.AdvanceIdle(1,out _,Array.Empty<string>());
        Need(w.ReadSeagulls().clock==clock,"Empty Beach pauses its flock");
        var options=new JsonSerializerOptions{IncludeFields=true};
        var restored=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(JsonSerializer.Serialize(w.Snapshot(),options),options));
        Need(restored.ReadSeagulls().flights==1 && restored.ReadPlayer("one").zone=="creek" && restored.ReadPlayers().Length==4,"Checkpoint preserves flock and individual areas");
        Console.WriteLine("PASS Beach: four-player shared encounter, independent travel, landing, empty-area pause and checkpoint retention.");
    }
}
