using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

class Program
{
    static void Check(bool value,string reason){if(!value)throw new Exception(reason);}
    static SoloResult Do(SoloWorld w,string actor,SoloAction action,string target="",string value="",string item="",float x=0,float y=0)
    {var p=w.ReadPlayer(actor);return w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,expectedRevision=w.Revision,zone=p.zone,visit=p.visit,action=action,target=target,value=value,item=item,x=x,y=y});}
    static void Wheels()
    {
        var w=SoloWorld.WithPark(SoloWorld.Create("a","b","c","d"));
        var ids=new[]{"a","b","c","d"};var session=new FamilySession(w);
        var movement=new MovementAuthority(w,session);
        for(var i=0;i<4;i++){Check(Do(w,ids[i],SoloAction.Travel,value:"park").Accepted,"wheel travel");session.Attach((ulong)i+1,ids[i],out _);}
        foreach(var baseIndex in new[]{0,4})
        {
            for(var i=0;i<4;i++)Check(Do(w,ids[i],SoloAction.UseFixture,ParkWheels.Id(baseIndex+i)).Accepted,"four wheel riders");
            Check(!Do(w,"b",SoloAction.UseFixture,ParkWheels.Id(baseIndex)).Accepted,"exclusive vehicle");
            for(var i=0;i<4;i++)
            {
                var p=w.ReadPlayer(ids[i]);var origin=p.x;
                Check(movement.Accept((ulong)i+1,new WalkInput{actor=p.id,zone=p.zone,visit=p.visit,sequence=baseIndex+1,mode=WalkMode.Direction,x=1,y=1},1),"mounted direction");
                movement.Tick(1,.05f);p=w.ReadPlayer(ids[i]);
                Check(p.x>origin && p.y==ParkWheels.Lane(p.fixture) && p.fixture==ParkWheels.Id(baseIndex+i),"right ride stays mounted and on lane");
                Check(Walking.AdvanceLocal(w,p.id,WalkMode.Direction,-1,-1,.1f),"offline left ride");
                Check(w.ReadPlayer(p.id).x<p.x,"left direction");
                Check(Do(w,p.id,SoloAction.Move,x:ParkWheels.MaxX,y:440).Accepted,"mounted boundary");
                Walking.AdvanceLocal(w,p.id,WalkMode.Direction,1,0,.1f);
                Check(w.ReadPlayer(p.id).x==ParkWheels.MaxX,"right safety limit");
                for(var t=0;t<12;t++)Walking.AdvanceLocal(w,p.id,WalkMode.Destination,ParkWheels.MaxX-200,400,.05f);
                p=w.ReadPlayer(p.id);Check(Math.Abs(p.x-(ParkWheels.MaxX-200))<.01 && p.y==ParkWheels.Lane(p.fixture),"tap projects to riding lane");
                Check(Do(w,p.id,SoloAction.ChangeAvatar,value:"orange-pup").Accepted && w.ReadPlayer(p.id).fixture==p.fixture,"avatar switch keeps ride");
            }
            SoloWorld.Validate(w.Snapshot());
            var restored=SoloWorld.Restore(w.Snapshot());Check(restored.ReadPlayers().All(p=>p.fixture==""),"restoring releases vehicles");
            foreach(var id in ids)Check(Do(w,id,SoloAction.LeaveFixture).Accepted,"get off");
        }
        for(var i=0;i<4;i++)Do(w,ids[i],SoloAction.UseFixture,ParkWheels.Id(i));
        session.Detach(1);Check(w.ReadPlayer("a").fixture=="" && w.ReadPlayer("b").fixture==ParkWheels.Id(1),"departing rider leaves siblings riding");
        Check(Do(w,"c",SoloAction.Travel,value:"creek").Accepted && w.ReadPlayer("c").fixture=="" && w.ReadPlayer("d").fixture==ParkWheels.Id(3),"independent travel releases only own vehicle");
        Check(Do(w,"b",SoloAction.Grab,item:"bucket-park").Accepted && w.ReadPlayer("b").fixture=="","grabbing a toy dismounts");
        SoloWorld.Validate(w.Snapshot());Console.WriteLine("PASS four bicycles/scooters, lane steering, bounds, exclusivity, avatar changes and independent departure");
    }
    static void Main(string[] args)
    {
        var before=SoloWorld.WithHideAndSeek(SoloWorld.Create("a","b","c","d"));
        Do(before,"a",SoloAction.Move,x:1742,y:130);var retained=before.Snapshot();
        var w=SoloWorld.WithPark(before);var s=w.Snapshot();
        Check(s.schema==33 && s.toys.Length==retained.toys.Length+2,"additive migration");
        Check(s.worldId==retained.worldId && s.players[0].x==1742 && s.toys.Take(retained.toys.Length).Select(t=>t.id).SequenceEqual(retained.toys.Select(t=>t.id)),"identity and placement retention");
        foreach(var p in s.players)Check(Do(w,p.id,SoloAction.Travel,value:"park").Accepted,"travel");
        var session=new FamilySession(w);for(ulong i=1;i<=4;i++)Check(session.Attach(i,s.players[i-1].id,out _),"attach");
        foreach(var station in ParkPlay.Stations)
        {
            for(var i=0;i<4;i++)Check(Do(w,s.players[i].id,SoloAction.UseFixture,ParkPlay.Slot(station,i)).Accepted,"four seats: "+station);
            Check(!Do(w,"b",SoloAction.UseFixture,ParkPlay.Slot(station,0)).Accepted,"exclusive seat");
            for(var i=0;i<10;i++)session.AdvanceIdle(.1,out _);SoloWorld.Validate(w.Snapshot());
            if(station=="slide"){
                Do(w,"a",SoloAction.LeaveFixture);
                for(var i=0;i<60;i++)session.AdvanceIdle(.1,out _);
                Check(w.Snapshot().players.All(p=>p.fixture==""),"automatic slide landing and early exit");}
            else foreach(var p in s.players)Check(Do(w,p.id,SoloAction.LeaveFixture).Accepted,"leave "+station);
        }
        for(var i=0;i<4;i++)Do(w,s.players[i].id,SoloAction.UseFixture,ParkPlay.Slot("roundabout",i));
        session.Detach(1);Check(w.ReadPlayer("a").fixture=="" && w.ReadPlayer("b").fixture!="","independent departure");
        Do(w,"b",SoloAction.Park,value:"stop");for(var i=0;i<40;i++)session.AdvanceIdle(.1,out _);Check(w.ReadPark().speed<.001,"smooth deceleration");
        Do(w,"b",SoloAction.Park,value:"turn");session.AdvanceIdle(.1,out _);Check(w.ReadPark().speed>0,"restart shared roundabout");
        Do(w,"c",SoloAction.LeaveFixture);Check(Do(w,"c",SoloAction.Grab,item:"bucket-park").Accepted,"bucket grab");
        Check(Do(w,"c",SoloAction.Drop,target:"tap-park",item:"bucket-park",x:ParkPlay.FountainX,y:ParkPlay.FountainY).Accepted,"fill at fountain");
        Check(w.ReadToys().Single(t=>t.id=="bucket-park").water==3 && w.ReadPark().waterUntil>w.ReadPark().clock,"water result");
        var saved=w.Snapshot();var restored=SoloWorld.Restore(saved);SoloWorld.Validate(restored.Snapshot());
        Check(restored.ReadToys().Single(t=>t.id=="bucket-park").water==3 && restored.Snapshot().players.All(p=>p.fixture=="" && p.rideStarted==0),"restore contents and release leases");
        if(args.Length>0){Directory.CreateDirectory(args[0]);File.WriteAllText(Path.Combine(args[0],"park-rules.json"),JsonSerializer.Serialize(new{passed=true,schema=33,checks=new[]{"additive migration","four-seat occupancy","exclusive leases","slide landing","independent departure","roundabout stop/restart","fountain bucket contents","save/restore"}}));}
        Wheels();
        Console.WriteLine("PASS park migration, four-player rides, departure, water and restore");
    }
}
