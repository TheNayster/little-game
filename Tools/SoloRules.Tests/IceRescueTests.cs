using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld IceWorld()=>SoloWorld.WithIceRescue(DiscoveryWorld());
    static IceRescueTray IceTray(SoloWorld w,string actor="first")=>Workspace(w,actor).ice[0];
    static SoloResult Ice(SoloWorld w,string op,float x=316,float y=185,string actor="first")=>w.Apply(Command(w,SoloAction.Discovery,actor,IceRescue.Token(IceTray(w,actor)),"ice:"+op,x,y,actor));
    static void IceRescueTests()
    {
        Test("ice migration adds four trays while every previous record stays exact",()=>{
            var old=SoloWorld.WithColoringCollection(DiscoveryWorld());Check(Discover(old,"fill:0:5").Accepted);Check(Mix(old,"add:1").Accepted);var before=old.Snapshot();var w=SoloWorld.WithIceRescue(old);var after=w.Snapshot();Check(after.schema==19 && after.discovery.All(v=>v.ice[0].current.cells.Length==24));after.schema=18;after.revision--;foreach(var v in after.discovery)v.ice=Array.Empty<IceRescueTray>();Check(Encode(before)==Encode(after));Check(ReferenceEquals(w,SoloWorld.WithIceRescue(w)));
        });
        Test("hammer chips spatially without heat and repeated strikes free one toy",()=>{
            var w=IceWorld();Check(Ice(w,"chip").Accepted);var s=IceTray(w).current;Check(s.cells[0]<.4f && s.cells[23]==1 && s.energy.All(v=>v==0));var count=0;
            while(!IceTray(w).current.freed && count++<80){var i=IceRescue.Next(IceTray(w).current);Check(Ice(w,"chip",IceRescue.X(i),IceRescue.Y(i)).Accepted);}
            Check(count<80 && IceTray(w).current.freed);Check(!Ice(w,"chip").Accepted);Check(Ice(w,"move",700,350).Accepted);Check(IceTray(w).current.x==700);Check(!Ice(w,"move",900,405).Accepted);
        });
        Test("warm water melts more than cool water and continues while a sibling leaves",()=>{
            var w=IceWorld();Check(Ice(w,"water-warm").Accepted && Ice(w,"water-cool",actor:"second").Accepted);var before=IceTray(w).current.cells[0];Check(w.AdvanceIdle(.5,out var visible) && visible);Check(IceTray(w).current.cells[0]<before);Good(w,SoloAction.Travel,value:"park",actor:"first");for(var i=0;i<8;i++)w.AdvanceIdle(.5,out _);Check(IceTray(w).current.cells[0]<IceTray(w,"second").current.cells[0]);Check(!Ice(w,"chip").Accepted);
        });
        Test("four players reset undo travel and disconnect without changing sibling rescues",()=>{
            var w=IceWorld();foreach(var id in w.Snapshot().players.Select(p=>p.id))Check(Ice(w,"chip",actor:id).Accepted);var others=Encode(w.ReadDiscovery().Skip(1).ToArray());var prior=System.Text.Json.JsonSerializer.Serialize(IceTray(w).current,Json);Check(Ice(w,"again").Accepted && Ice(w,"undo").Accepted);Check(System.Text.Json.JsonSerializer.Serialize(IceTray(w).current,Json)==prior);Check(!Ice(w,"undo").Accepted);Check(Encode(w.ReadDiscovery().Skip(1).ToArray())==others);var session=new FamilySession(w);Check(session.Attach(1,"third",out _) && session.Detach(1));Check(Encode(w.ReadDiscovery().Skip(1).ToArray())==others);
        });
        Test("ice duplicate stale foreign and malformed commands cannot mutate progress",()=>{
            var w=IceWorld();var command=Command(w,SoloAction.Discovery,"first",IceRescue.Token(IceTray(w)),"ice:chip",316,185);Check(w.Apply(command).Accepted && w.Apply(command).Duplicate);var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Discovery,"first","ice@0","ice:chip",316,185)).Accepted);Check(!w.Apply(Command(w,SoloAction.Discovery,"second",IceRescue.Token(IceTray(w,"second")),"ice:chip",316,185)).Accepted);
            foreach(var op in new[]{"move","dinosaur","chip:extra","garbage"})Check(!Ice(w,op).Accepted);Check(!Ice(w,"chip",float.NaN,185).Accepted);Check(!Ice(w,"chip",float.PositiveInfinity,185).Accepted);Check(!Ice(w,"chip",0,0).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("ice save restore deep copy and corrupt state handling preserve bounded payloads",()=>{
            var w=IceWorld();Check(Ice(w,"chip").Accepted);Check(Ice(w,"water-warm").Accepted);w.AdvanceIdle(.4,out _);var s=w.Snapshot();Check(Encode(SoloWorld.Restore(Decode(Encode(s))).Snapshot())==Encode(s));
            var copy=SoloWorld.CopySnapshot(s);copy.discovery[0].ice[0].current.cells[0]=1;copy.discovery[0].ice[0].previous[0].cells[0]=1;Check(s.discovery[0].ice[0].current.cells[0]<1 && s.discovery[0].ice[0].previous[0].cells[0]<1);
            void Reject(Action<SoloSnapshot> edit){var v=Decode(Encode(s));edit(v);Throws(()=>SoloWorld.Validate(v));}
            Reject(v=>v.discovery[0].ice=null);Reject(v=>v.discovery[0].ice[0].current.cells[0]=float.NaN);Reject(v=>v.discovery[0].ice[0].current.energy[0]=9);Reject(v=>v.discovery[0].ice[0].current.freed=true);Reject(v=>v.discovery[0].ice[0].current.toy=3);Reject(v=>v.discovery[0].ice[0].previous[0].cells=new float[1]);
            foreach(var id in w.Snapshot().players.Select(p=>p.id))for(var page=0;page<Discovery.Pages.Length;page++)for(var i=0;i<21;i++)Check(Discover(w,"fill:0:"+(i%8+1),id,page).Accepted);
            var bytes=System.Text.Encoding.UTF8.GetByteCount(System.Text.Json.JsonSerializer.Serialize(new FamilySession(w).View(),new System.Text.Json.JsonSerializerOptions{IncludeFields=true}));Check(bytes<100000);System.IO.File.WriteAllText(System.IO.Path.Combine(root,"ice-rescue-payload.txt"),bytes+" bytes");
        });
    }
}
