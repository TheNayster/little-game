using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class Program
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true};
    static string Serialize(object value)=>JsonSerializer.Serialize(value,Json);
    static void Main()
    {
        var prior=SoloWorld.WithPark(SoloWorld.Create("a","b","c","d"));
        // Exercise the actual available predecessor in the combined checkout,
        // while keeping this focused fixture usable before Zoo is integrated.
        var zooUpgrade=typeof(SoloWorld).GetMethod("WithZoo");
        if(zooUpgrade!=null)prior=(SoloWorld)zooUpgrade.Invoke(null,new object[]{prior});
        var old=prior.Snapshot();
        foreach(var p in old.players){p.outfit=null;p.outfitColor=null;}
        var before=Serialize(old.toys);var id=old.worldId;
        var world=SoloWorld.WithOutfits(SoloWorld.Restore(old));var family=new FamilySession(world);
        Check(world.Schema==35 && world.WorldId==id && Serialize(world.ReadToys())==before,"Additive upgrade changed possessions/world");
        foreach(var p in world.ReadPlayers())Check(p.outfit=="" && p.outfitColor=="green","Legacy defaults wrong");
        var ids=new[]{"a","b","c","d"};for(var i=0;i<4;i++)Check(family.Attach((ulong)i,ids[i],out _),"Attach failed");
        SoloResult Send(int who,SoloAction action,string value="",string color="")
        {var p=world.ReadPlayer(ids[who]);return family.Submit((ulong)who,new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),expectedRevision=world.Revision,zone=p.zone,visit=p.visit,action=action,value=value,target=color});}
        Check(Send(0,SoloAction.Grab).Accepted==false,"Unexpected pickup");
        foreach(var entry in PlayableCharacters.All.Where(e=>CharacterOutfits.Available(e.AvatarId)))
        {
            Check(Send(0,SoloAction.ChangeAvatar,entry.AvatarId).Accepted,"Avatar unavailable");
            foreach(var color in CharacterOutfits.Colors)
            {
                var player=world.ReadPlayer("a");var toys=Serialize(world.ReadToys());
                Check(Send(0,SoloAction.ChangeOutfit,"dinosaur",color).Accepted,"Outfit unavailable for "+entry.Name);
                var dressed=world.ReadPlayer("a");player.outfit="dinosaur";player.outfitColor=color;
                Check(Serialize(player)==Serialize(dressed) && toys==Serialize(world.ReadToys()),"Outfit altered unrelated state");
            }
        }
        for(var i=0;i<4;i++)Check(Send(i,SoloAction.ChangeOutfit,"dinosaur",CharacterOutfits.Colors[i]).Accepted,"Independent color");
        Check(Send(0,SoloAction.Roar).Accepted && world.ReadPlayer("a").roar==1,"Roar failed");
        Check(!Send(0,SoloAction.Roar).Accepted,"Roar cooldown missing");
        Check(Send(1,SoloAction.Roar).Accepted,"Sibling roar blocked");
        for(var i=0;i<4;i++)world.AdvanceIdle(1,out _,ids);
        Check(Send(0,SoloAction.Roar).Accepted,"Roar cooldown never ends");
        Check(!Send(0,SoloAction.ChangeOutfit,"dinosaur","purple").Accepted,"Invalid color accepted");
        Check(!Send(0,SoloAction.ChangeOutfit,"dragon","pink").Accepted,"Invalid outfit accepted");
        var data=JsonSerializer.Deserialize<SoloSnapshot>(Serialize(world.Snapshot()),Json);
        var restored=SoloWorld.Restore(data);
        Check(restored.ReadPlayer("a").outfit=="dinosaur" && restored.ReadPlayer("d").outfitColor=="red","Reopen lost outfits");
        Check(Send(0,SoloAction.ChangeOutfit,"","blue").Accepted && !Send(0,SoloAction.Roar).Accepted,"Normal outfit can roar");
        Check(family.Detach(0) && Send(2,SoloAction.ChangeOutfit,"dinosaur","red").Accepted,"Departure blocked wardrobe");
        Check(Send(2,SoloAction.ChangeAvatar,"muffin").Accepted && world.ReadPlayer("c").outfit=="","Other cast did not keep normal appearance");
        Check(!Send(2,SoloAction.ChangeOutfit,"dinosaur","green").Accepted,"Unprepared character accepted outfit");
        var invalid=world.Snapshot();invalid.players[0].outfit="unknown";
        try{SoloWorld.Validate(invalid);throw new Exception("Invalid saved outfit accepted");}catch(InvalidOperationException){}
        Console.WriteLine("PASS: additive migration/reopen, Bluey/Bingo and four colors, other cast unchanged, state isolation, roar/cooldown, validation, four-player independence/departure");
    }
}
