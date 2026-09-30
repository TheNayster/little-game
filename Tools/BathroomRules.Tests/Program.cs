using System;
using System.Linq;
using LittleWeeps.Core;
class Program
{
    static void Check(bool b,string text){if(!b)throw new Exception(text);}
    static SoloResult Send(SoloWorld w,string id,SoloAction action,string target="",float x=0,float y=0)
    {var p=w.ReadPlayer(id);return w.Apply(new SoloCommand{actor=id,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,target=target,x=x,y=y});}
    static void Main()
    {
        var old=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));
        var newer=typeof(SoloWorld).GetMethod("WithCreekFishing");if(newer!=null)old=(SoloWorld)newer.Invoke(null,new object[]{old});
        var before=old.Snapshot();var w=SoloWorld.WithBathroom(old);var upgraded=w.Snapshot();
        Check(upgraded.worldId==before.worldId && upgraded.toys.Select(t=>t.id).SequenceEqual(before.toys.Select(t=>t.id)) && upgraded.bedrooms.Select(r=>r.owner).SequenceEqual(before.bedrooms.Select(r=>r.owner)),"Room upgrade lost belongings or ownership");
        Check(!WorldLayout.Position(BathroomLayout.Area,38,500,100) && WorldLayout.Position(BathroomLayout.Area,39,500,100) && !WorldLayout.Destination(BathroomLayout.Area),"Room admitted before schema or became a world destination");
        var setup=w.Snapshot();foreach(var p in setup.players){p.zone=HomeRooms.Landing;p.x=BathroomLayout.HallX;p.y=BedroomLayout.DoorY;}w=SoloWorld.Restore(setup);
        for(var i=0;i<4;i++)
        {
            var id=w.ReadPlayers()[i].id;Check(Send(w,id,SoloAction.EnterDoor,BathroomLayout.Area).Accepted,"Bathroom door failed");
            Check(Send(w,id,SoloAction.UseFixture,BathroomLayout.BathSlots[i]).Accepted,"Shared bath failed");
        }
        Check(!Send(w,"two",SoloAction.UseFixture,"bath-0").Accepted && w.ReadPlayer("two").fixture=="bath-1","Competing lease changed occupant");
        Check(Send(w,"one",SoloAction.SetFixture,"bath-splash").Accepted && w.ReadPlayer("two").fixture=="bath-1","Splash disturbed sibling");
        Check(Send(w,"one",SoloAction.UseFixture,"sink-0").Accepted && w.ReadPlayer("one").fixture=="sink-0","Sink use failed");
        Check(Send(w,"one",SoloAction.LeaveFixture).Accepted && w.ReadPlayer("two").fixture=="bath-1","Dry/get out reset group");
        Check(Send(w,"one",SoloAction.Move,x:BathroomLayout.ExitX,y:BedroomLayout.DoorY).Accepted && Send(w,"one",SoloAction.EnterDoor,HomeRooms.Landing).Accepted,"Hallway exit failed");
        Check(w.ReadPlayer("three").zone==BathroomLayout.Area && w.ReadPlayer("three").fixture=="bath-2","Room exit moved sibling");
        var family=new FamilySession(w);for(ulong i=1;i<=4;i++)Check(family.Attach(i,w.ReadPlayers()[(int)i-1].id,out _),"Attach");family.Detach(4);
        Check(w.ReadPlayer("four").fixture=="" && w.ReadPlayer("three").fixture=="bath-2","Disconnect cleared group");
        var saved=w.Snapshot();var reopened=SoloWorld.Restore(saved);Check(reopened.ReadPlayer("three").zone==BathroomLayout.Area && reopened.ReadPlayer("three").fixture=="" && saved.players[2].fixture=="bath-2","Restore lease/source mutation");SoloWorld.Validate(reopened.Snapshot());
        Console.WriteLine("PASS Home retention, room admission, four bath places, lease competition, splash/sink/dry, independent hallway/disconnect and reopen");
    }
}
