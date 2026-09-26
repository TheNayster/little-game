using System;
using System.IO;
using System.Linq;
using LittleWeeps.Core;
using LittleWeeps.Adapters;

static partial class Program
{
    static SoloWorld Bedrooms()=>SoloWorld.WithBedrooms(SoloWorld.Create("first","second","third","fourth"));
    static void BedroomTests()
    {
        Test("bedroom migration appends stable rooms without replacing garden progress or receipts",()=>{
            var old=SoloWorld.WithAreas(SoloWorld.Create("first","second","third","fourth"));
            Fill(old);Good(old,SoloAction.ChangeAvatar,value:"orange-pup");
            var before=old.Snapshot();var next=SoloWorld.WithBedrooms(old);var s=next.Snapshot();
            Check(s.schema==3 && s.toys.Length==26 && s.worldId==before.worldId);
            Check(s.bedrooms[0].owner=="first" && s.bedrooms[1].owner=="second");
            Check(s.players.Select(p=>p.avatar).SequenceEqual(before.players.Select(p=>p.avatar)));
            Check(System.Text.Json.JsonSerializer.Serialize(s.toys.Take(10),Json)==System.Text.Json.JsonSerializer.Serialize(before.toys,Json));
            Check(s.receipts.Select(r=>r.fingerprint).SequenceEqual(before.receipts.Select(r=>r.fingerprint)));
            Check(ReferenceEquals(next,SoloWorld.WithBedrooms(next)));
        });
        Test("room ownership follows profiles across avatar changes and visitors cannot decorate",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-a");
            Good(w,SoloAction.ChangeAvatar,value:"orange-pup");Good(w,SoloAction.Decorate,target:"wall",value:"3");
            Good(w,SoloAction.Travel,value:"bedroom-a",actor:"second");
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Decorate,target:"wall",value:"0",actor:"second")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Grab,item:"bed-bedroom-a",actor:"second")).Accepted);
            Check(Encode(w.Snapshot())==before && w.ReadBedroom("bedroom-a").owner=="first");
            Good(w,SoloAction.Grab,"plush-bedroom-a",actor:"second");
            Good(w,SoloAction.Drop,"plush-bedroom-a",x:550,y:300,actor:"second");
        });
        Test("decorate together can be revoked during a visitor drag without accepting a late drop",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-a");Good(w,SoloAction.Travel,value:"bedroom-a",actor:"second");
            Good(w,SoloAction.DecorateTogether,value:"on");Good(w,SoloAction.Grab,"lamp-bedroom-a",actor:"second");
            Good(w,SoloAction.DecorateTogether,value:"off");
            Check(Toy(w,"lamp-bedroom-a").holder=="");
            Check(!w.Apply(Command(w,SoloAction.Drop,"lamp-bedroom-a",x:600,y:300,actor:"second")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.DecorateTogether,value:"on",actor:"second")).Accepted);
        });
        Test("invalid room placement leaves the previous layout and doorway intact",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-a");Good(w,SoloAction.Grab,"bed-bedroom-a");
            var before=Encode(w.Snapshot());
            Check(!w.Apply(Command(w,SoloAction.Drop,"bed-bedroom-a",x:500,y:80)).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Drop,"bed-bedroom-a",x:500,y:330)).Accepted);
            Check(Encode(w.Snapshot())==before);
            Good(w,SoloAction.Drop,"bed-bedroom-a",x:820,y:320);
            Check(Toy(w,"bed-bedroom-a").x==820);
        });
        Test("room style transactions commit once and cannot overwrite the other bedroom",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-a");
            var c=Command(w,SoloAction.Decorate,target:"bedding",value:"2");Check(w.Apply(c).Accepted);
            var before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && Encode(w.Snapshot())==before);
            Check(w.ReadBedroom("bedroom-b").bedding==1);
            c.value="0";Check(!w.Apply(c).Accepted);
            var copy=w.ReadBedroom("bedroom-a");copy.owner="second";Check(w.ReadBedroom("bedroom-a").owner=="first");
        });
        Test("personal toys and layouts survive idle returns and room departure",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-a");Good(w,SoloAction.Grab,"plush-bedroom-a");
            Good(w,SoloAction.Drop,"plush-bedroom-a",x:610,y:240);
            var before=Toy(w,"plush-bedroom-a");Good(w,SoloAction.Travel,value:"creek");Advance(w,600);
            var after=Toy(w,"plush-bedroom-a");Check(after.x==before.x && after.y==before.y && !after.resetPending);
            Check(!w.Snapshot().idleTimers.Any(t=>t.item==after.id));
        });
        Test("bedroom save reopens and recovers an interrupted primary without replacing identities",()=>{
            var w=Bedrooms();Good(w,SoloAction.Travel,value:"bedroom-b",actor:"second");
            Good(w,SoloAction.Decorate,target:"wall",value:"2",actor:"second");
            var store=new CheckpointStore(Path.Combine(root,"bedroom-save","world.save"),text=>{try{SoloWorld.Validate(Decode(text));return true;}catch{return false;}});
            var payload=Encode(w.Snapshot());store.Save(payload);store.Save(payload);
            File.WriteAllText(Path.Combine(root,"bedroom-save","world.save"),"interrupted");
            var loaded=SoloWorld.Restore(Decode(store.Load().Payload));
            Check(loaded.ReadBedroom("bedroom-b").wall==2 && loaded.Snapshot().worldId==w.Snapshot().worldId);
            Check(loaded.ReadPlayer("second").zone=="bedroom-b");
        });
        Test("malformed room catalog, owners, missing items and unsafe placements are rejected",()=>{
            var good=Bedrooms().Snapshot();
            var s=Decode(Encode(good));s.bedrooms[0].owner="third";Throws(()=>SoloWorld.Validate(s));
            s=Decode(Encode(good));s.bedrooms[0].wall=99;Throws(()=>SoloWorld.Validate(s));
            s=Decode(Encode(good));s.toys.First(t=>t.kind==ToyKind.Bed).y=20;Throws(()=>SoloWorld.Validate(s));
            s=Decode(Encode(good));s.toys.Last().id=s.toys[10].id;Throws(()=>SoloWorld.Validate(s));
            s=Decode(Encode(good));s.schema=2;Throws(()=>SoloWorld.Validate(s));
        });
    }
}
