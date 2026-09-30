using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static class CharacterRosterRules
{
    static readonly JsonSerializerOptions Json=new JsonSerializerOptions {IncludeFields=true};
    static string Serialize(object value)=>JsonSerializer.Serialize(value,Json);
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static SoloResult Submit(FamilySession family,ulong connection,SoloWorld world,string actor,SoloAction action,string value="",string item="",float x=0,float y=100)
    {
        var p=world.ReadPlayer(actor);
        return family.Submit(connection,new SoloCommand {requestId=Guid.NewGuid().ToString("N"),
            actor=actor,expectedRevision=world.Revision,zone=p.zone,visit=p.visit,action=action,value=value,item=item,x=x,y=y});
    }
    static void Main()
    {
        var world=SoloWorld.Create("one","two","three","four");
        Check(PlayableCharacters.All.Count==37,"Research roster incomplete");
        var family=new FamilySession(world);
        var ids=new[]{"one","two","three","four"};
        for(var i=0;i<4;i++)Check(family.Attach((ulong)i+1,ids[i],out _),"Attach failed");
        Check(Submit(family,1,world,"one",SoloAction.Grab,item:"bucket-1").Accepted,"Pickup failed");
        foreach(var entry in PlayableCharacters.All)
        {
            var before=world.Snapshot();
            Check(Submit(family,1,world,"one",SoloAction.ChangeAvatar,entry.AvatarId).Accepted,"Select failed: "+entry.Name);
            var after=world.Snapshot();
            // Compare every stable player field, all possessions and room state.
            before.players[0].avatar=entry.AvatarId;
            Check(Serialize(before.players)==Serialize(after.players),"Selection changed player state");
            Check(Serialize(before.toys)==Serialize(after.toys),"Selection changed possessions");
            Check(Serialize(before.bedrooms)==Serialize(after.bedrooms),"Selection changed bedrooms");
            Check(Serialize(before.home)==Serialize(after.home),"Selection changed home progress");
            var decoded=JsonSerializer.Deserialize<SoloSnapshot>(Serialize(after),Json);
            var restored=SoloWorld.Restore(decoded).Snapshot();
            Check(Serialize(restored.players)==Serialize(after.players),"Saved character/player changed on reopen");
            // Restore deliberately releases pointer leases. Object identities,
            // placements and contents must still survive that existing rule.
            foreach(var toy in after.toys) {toy.holder="";toy.resetPending=false;}
            Check(Serialize(restored.toys)==Serialize(after.toys),"Saved belongings changed on reopen");
            Check(Serialize(restored.bedrooms)==Serialize(after.bedrooms),"Saved rooms changed on reopen");
            var selected=world.ReadPlayer("one");
            Check(Submit(family,1,world,"one",SoloAction.Move,x:selected.x+20,y:selected.y).Accepted,"Cannot walk as "+entry.Name);
            Check(Submit(family,1,world,"one",SoloAction.Move,x:selected.x,y:selected.y).Accepted,"Cannot return as "+entry.Name);
        }
        var unchanged=Serialize(world.Snapshot());
        Check(!Submit(family,1,world,"one",SoloAction.ChangeAvatar,"invented").Accepted,"Unknown character accepted");
        Check(Serialize(world.Snapshot())==unchanged,"Rejected avatar mutated state");
        for(var i=0;i<4;i++)Check(Submit(family,(ulong)i+1,world,ids[i],SoloAction.ChangeAvatar,"muffin").Accepted,"Duplicate favorite rejected");
        Check(world.ReadPlayers().All(p=>p.avatar=="muffin"),"Duplicate favorite not retained");
        var siblings=Serialize(world.ReadPlayers().Skip(1));
        Check(family.Detach(1),"Departure failed");
        Check(Serialize(world.ReadPlayers().Skip(1))==siblings,"Departure changed sibling players");
        Check(family.ConnectedPlayers.Length==3,"Sibling sessions lost");
        Check(world.ReadToys().Single(t=>t.id=="bucket-1").holder=="","Departed lease not released");
        Check(PlayableCharacters.Find("blue-pup").ArtId=="bluey" && PlayableCharacters.Find("orange-pup").ArtId=="bingo","Legacy identities changed");
        Console.WriteLine("PASS: all 37 characters select and walk; selection retains players, belongings, rooms and progress; JSON save/reopen; rejected IDs; four duplicate favorites and independent departure.");
    }
}
