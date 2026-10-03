using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static GameWorld BubbleWorld()=>GameWorld.WithBubbleLab(DiscoveryWorld());
    static BubbleTray BubbleTrayOf(GameWorld w,string actor="first")=>Workspace(w,actor).bubbles[0];
    static SoloResult Bubbles(GameWorld w,string op,string actor="first")=>w.Apply(Command(w,SoloAction.Discovery,actor,BubbleLab.Token(BubbleTrayOf(w,actor)),"bubble:"+op,actor:actor));
    static void PrepareBubbles(GameWorld w,string actor="first"){foreach(var op in new[]{"water","soap","stir","dip"})Check(Bubbles(w,op,actor).Accepted);}
    static string BubbleJson(object value)=>JsonSerializer.Serialize(value,Json);
    static void BubbleLabTests()
    {
        Test("bubble migration adds four trays while all existing records remain exact",()=>{
            var old=IceWorld();Check(Ice(old,"chip").Accepted && Discover(old,"fill:0:5").Accepted && Mix(old,"add:1").Accepted);var before=old.Snapshot();var w=GameWorld.WithBubbleLab(old);var after=w.Snapshot();Check(after.schema==20 && after.discovery.All(v=>v.bubbles.Length==1));after.schema=19;after.revision--;foreach(var d in after.discovery)d.bubbles=Array.Empty<BubbleTray>();Check(Encode(before)==Encode(after));Check(ReferenceEquals(w,GameWorld.WithBubbleLab(w)));
        });
        Test("bubbles require water soap stirring and a dipped wand with conserved mixture",()=>{
            var w=BubbleWorld();foreach(var op in new[]{"soap","stir","dip","blow"})Check(!Bubbles(w,op).Accepted);Check(BubbleLab.Next(BubbleTrayOf(w).current)=="water");
            PrepareBubbles(w);Check(BubbleTrayOf(w).current.solution==15 && BubbleTrayOf(w).current.film==4);foreach(var op in new[]{"water","soap","stir","dip","refill"})Check(!Bubbles(w,op).Accepted);
            for(var i=0;i<4;i++)Check(Bubbles(w,"blow").Accepted);Check(BubbleTrayOf(w).current.film==0 && BubbleTrayOf(w).current.floating.Length==12);Check(!Bubbles(w,"blow").Accepted);Check(BubbleLab.Next(BubbleTrayOf(w).current)=="dip");Check(Bubbles(w,"dip").Accepted && BubbleTrayOf(w).current.solution==14);
        });
        Test("big and little round bubbles respond to air even with a square wand",()=>{
            var w=BubbleWorld();PrepareBubbles(w);Check(Bubbles(w,"shape").Accepted && Bubbles(w,"blow").Accepted);var t=BubbleTrayOf(w);Check(t.current.square && t.current.floating.Length==3 && t.current.floating[0].radius==31);Check(Bubbles(w,"size").Accepted && Bubbles(w,"air").Accepted && Bubbles(w,"blow").Accepted);t=BubbleTrayOf(w);Check(t.current.floating.Length==4 && t.current.floating[3].radius==86 && t.current.film==1);Check(BubbleLab.X(t.current.floating[3],1)<BubbleLab.X(t.current.floating[0],1));Check(Bubbles(w,"blow").Accepted && BubbleTrayOf(w).current.film==0);
        });
        Test("bubble capacity pop and refill never create unlimited objects or free film",()=>{
            var w=BubbleWorld();PrepareBubbles(w);for(var i=0;i<4;i++)Check(Bubbles(w,"blow").Accepted);Check(Bubbles(w,"dip").Accepted);for(var i=0;i<2;i++)Check(Bubbles(w,"blow").Accepted);Check(BubbleTrayOf(w).current.floating.Length==18 && BubbleLab.Next(BubbleTrayOf(w).current)=="pop");var unchanged=Encode(w.Snapshot());Check(!Bubbles(w,"blow").Accepted && Encode(w.Snapshot())==unchanged);Check(Bubbles(w,"pop:1").Accepted && !Bubbles(w,"pop:1").Accepted);Check(Bubbles(w,"pop:2").Accepted && Bubbles(w,"pop:3").Accepted && Bubbles(w,"blow").Accepted);
            var s=w.Snapshot();var state=s.discovery[0].bubbles[0].current;state.solution=0;state.film=0;w=GameWorld.Restore(s);var flying=BubbleJson(BubbleTrayOf(w).current.floating);Check(Bubbles(w,"refill").Accepted && BubbleJson(BubbleTrayOf(w).current.floating)==flying);Check(!BubbleTrayOf(w).current.mixed && !Bubbles(w,"blow").Accepted);PrepareBubbles(w);
        });
        Test("four saved bubble trays have independent reset undo travel and disconnect",()=>{
            var w=BubbleWorld();foreach(var id in w.Snapshot().players.Select(p=>p.id)){PrepareBubbles(w,id);Check(Bubbles(w,"blow",id).Accepted);}var siblings=Encode(w.ReadDiscovery().Skip(1).ToArray());var previous=BubbleJson(BubbleTrayOf(w).current);var serial=BubbleTrayOf(w).serial;Check(Bubbles(w,"again").Accepted && Bubbles(w,"undo").Accepted);Check(BubbleJson(BubbleTrayOf(w).current)==previous && !Bubbles(w,"undo").Accepted);Check(Bubbles(w,"again").Accepted);PrepareBubbles(w);Check(Bubbles(w,"blow").Accepted && BubbleTrayOf(w).current.floating.Min(b=>b.id)>serial);Good(w,SoloAction.Travel,value:"park");Check(!Bubbles(w,"water").Accepted);var session=new FamilySession(w);Check(session.Attach(1,"third",out _) && session.Detach(1));Check(Encode(w.ReadDiscovery().Skip(1).ToArray())==siblings);
        });
        Test("bubble commands reject stale duplicate foreign and malformed changes",()=>{
            var w=BubbleWorld();var cmd=Command(w,SoloAction.Discovery,"first","bubbles@0","bubble:water");Check(w.Apply(cmd).Accepted && w.Apply(cmd).Duplicate);var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Discovery,"first","bubbles@0","bubble:soap")).Accepted);Check(!w.Apply(Command(w,SoloAction.Discovery,"second","bubbles@0","bubble:water")).Accepted);foreach(var op in new[]{"water:1","pop:NaN","pop:1","bad"})Check(!Bubbles(w,op).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("bubble time is deterministic across steps and only expiry publishes a revision",()=>{
            var a=BubbleWorld();PrepareBubbles(a);Check(Bubbles(a,"blow").Accepted);var b=GameWorld.Restore(a.Snapshot());var revision=a.Revision;Check(a.AdvanceIdle(.5,out var visible) && !visible && a.Revision==revision);for(var i=0;i<5;i++)b.AdvanceIdle(.1,out _);Check(Math.Abs(BubbleTrayOf(a).current.clock-BubbleTrayOf(b).current.clock)<1e-8);var particle=BubbleTrayOf(a).current.floating[0];Check(BubbleLab.Y(particle,.5)<BubbleLab.Y(particle,0));for(var i=0;i<14;i++)a.AdvanceIdle(1,out _);Check(BubbleTrayOf(a).current.floating.Length==0 && a.Revision>revision);var s=b.Snapshot();var t=s.discovery[0].bubbles[0];t.current.clock=1e9;foreach(var p in t.current.floating)p.born=1e9-.5;b=GameWorld.Restore(s);b.AdvanceIdle(.25,out _);Check(BubbleTrayOf(b).current.clock<100);GameWorld.Validate(b.Snapshot());
        });
        Test("bubble save restore copy and corrupt-state validation include undo particles",()=>{
            var w=BubbleWorld();PrepareBubbles(w);Check(Bubbles(w,"blow").Accepted && Bubbles(w,"shape").Accepted);w.AdvanceIdle(.1,out _);var s=w.Snapshot();Check(Encode(GameWorld.Restore(Decode(Encode(s))).Snapshot())==Encode(s));var copy=GameWorld.CopySnapshot(s);copy.discovery[0].bubbles[0].current.floating[0].radius=99;copy.discovery[0].bubbles[0].previous[0].floating[0].radius=99;Check(s.discovery[0].bubbles[0].current.floating[0].radius==31 && s.discovery[0].bubbles[0].previous[0].floating[0].radius==31);
            void Reject(Action<BubbleTray> edit){var v=Decode(Encode(s));edit(v.discovery[0].bubbles[0]);Throws(()=>GameWorld.Validate(v));}
            Reject(t=>t.current.clock=double.NaN);Reject(t=>t.current.solution=17);Reject(t=>t.current.film=5);Reject(t=>t.current.water=false);Reject(t=>t.current.floating[0].born=9);Reject(t=>t.current.floating[0].id=99);Reject(t=>t.current.floating[1].id=1);Reject(t=>t.current.floating[0].radius=99);Reject(t=>t.previous=new[]{t.current.Copy(),t.current.Copy()});Reject(t=>t.previous[0].floating=null);
        });
    }
}
