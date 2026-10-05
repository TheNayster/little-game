using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class Program
{
    static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true};
    static void Need(bool value,string reason){if(!value)throw new Exception(reason);}
    static SoloResult Send(GameWorld world,string actor,SoloAction action,string value="",string target="")
    {
        var player=world.ReadPlayer(actor);
        return world.Apply(new SoloCommand{actor=actor,requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,zone=player.zone,visit=player.visit,action=action,value=value,target=target});
    }
    static void Boundary(float edge,int runner)
    {
        var actors=new[]{"one","two","three","four"};
        var world=GameWorld.WithDinosaurWorld(GameWorld.Create(actors));
        foreach(var actor in actors)Need(Send(world,actor,SoloAction.Travel,"daycare").Accepted,"Daycare arrival");
        Need(Send(world,"one",SoloAction.DaycarePlay,"tag:start").Accepted,"Tag start");
        world.InviteDaycarePlay(actors);
        foreach(var actor in actors.Skip(1))Need(Send(world,actor,SoloAction.DaycarePlay,"tag:join","1").Accepted,"Join shared Tag");
        var saved=world.Snapshot();var club=saved.tagClub;
        club.phase=ClubPhase.Playing;club.clock=6;club.grace=1000;
        // At clock6 both runner0 and runner1 offer themselves to a human
        // chaser. Their old +/-130 goals crossed the corresponding wall.
        saved.players[0].x=edge;saved.players[0].y=60;
        club.npcs[runner].x=edge;club.npcs[runner].y=100;
        var cast=club.npcs.Select(n=>n.avatar).ToArray();var route=club.order.ToArray();
        world=GameWorld.Restore(saved);
        for(var tick=0;tick<600;tick++){
            world.AdvanceIdle(.05,out _,actors);
            var checkpoint=world.Snapshot();GameWorld.Validate(checkpoint);
            Need(checkpoint.tagClub.npcs.All(n=>DaycarePlay.Point(n.x,n.y)),"NPC escaped arena at "+edge);
            if(tick%100==0){
                var text=JsonSerializer.Serialize(checkpoint,Json);
                world=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(text,Json));
                Need(world.ReadDaycarePlay(true).npcs.Select(n=>n.avatar).SequenceEqual(cast),"Reopen changed cast");
                Need(world.ReadDaycarePlay(true).order.SequenceEqual(route) && world.ReadDaycarePlay(true).round==1,"Reopen changed round/route");
                Need(world.ReadDaycarePlay(true).members.All(m=>m.attending),"Reopen lost participant");
            }
        }
        Need(Send(world,"four",SoloAction.DaycarePlay,"tag:leave").Accepted,"Independent exit");
        Need(world.ReadDaycarePlay(true).members.Count(m=>m.attending)==3 && world.ReadDaycarePlay(true).round==1,"Exit interrupted siblings");
        var invalid=world.Snapshot();invalid.tagClub.npcs[runner].x=edge==80?79:2321;
        var rejected=false;try{GameWorld.Validate(invalid);}catch(InvalidOperationException){rejected=true;}
        Need(rejected,"Invalid checkpoint must remain rejected");
        Console.WriteLine("PASS edge "+edge+": four players, 600 Tag ticks, checkpoint validation, JSON reopening, cast/round/route retention and independent exit");
    }
    static void Main(string[] args){if(args.Contains("--seed")){LittleWeeps.EditorTools.DaycarePlayTests.TagRoutes(new[]{int.Parse(args[Array.IndexOf(args,"--seed")+1])});return;}if(args.Contains("--routes")){LittleWeeps.EditorTools.DaycarePlayTests.TagRoutes();return;}Boundary(2320,0);Boundary(80,1);}
}
