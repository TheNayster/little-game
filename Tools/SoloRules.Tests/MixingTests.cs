using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld MixWorld()=>SoloWorld.WithMixing(DiscoveryWorld());
    static MixingTray MixTray(SoloWorld w,int mode=0,string actor="first")=>Workspace(w,actor).mixtures[mode];
    static SoloResult Mix(SoloWorld w,string op,int mode=0,int amount=1,string actor="first")=>w.Apply(Command(w,SoloAction.Discovery,actor,Mixing.Token(mode,MixTray(w,mode,actor)),"mix:"+mode+":"+op,amount,0,actor));
    static void MixingTests()
    {
        Test("mixing migration preserves previous coloring science rooms and receipt data",()=>{
            var old=DiscoveryWorld();Check(Discover(old,"fill:0:5").Accepted);Check(Discover(old,"cargo-add","second").Accepted);var before=old.Snapshot();var world=SoloWorld.WithMixing(old);var after=world.Snapshot();
            Check(after.schema==17 && after.discovery.All(w=>w.mixtures.Length==4));after.schema=16;after.revision--;foreach(var w in after.discovery)w.mixtures=Array.Empty<MixingTray>();Check(Encode(before)==Encode(after));Check(ReferenceEquals(world,SoloWorld.WithMixing(world)));
        });
        Test("vinegar contact consumes finite portions and water stirring never create gas",()=>{
            var w=MixWorld();Check(Mix(w,"add:1",amount:4).Accepted);Check(Mix(w,"add:2",amount:4).Accepted);Check(Mix(w,"stir").Accepted);Check(MixTray(w).reacted==0);Check(Mix(w,"add:0",amount:2).Accepted);Check(MixTray(w).reacted==2 && MixTray(w).reaction>0);
            for(int i=0;i<20;i++)w.AdvanceIdle(1,out _);Check(MixTray(w).reaction==0);Check(Mix(w,"stir").Accepted);Check(MixTray(w).reacted==2 && MixTray(w).reaction==0);Check(Mix(w,"add:1").Accepted);Check(MixTray(w).reacted==2);Check(Mix(w,"add:0").Accepted);Check(MixTray(w).reacted==3);
        });
        Test("soap prolongs foam without multiplying reaction gas and wipe preserves mixture",()=>{
            var w=MixWorld();Check(Mix(w,"add:3").Accepted);Check(Mix(w,"add:1",amount:4).Accepted);Check(Mix(w,"add:0",amount:4).Accepted);var t=MixTray(w);Check(t.spill && t.reacted==4 && t.foam>t.reaction);Check(Mix(w,"wipe").Accepted);Check(!MixTray(w).spill && MixTray(w).reacted==4);Check(Mix(w,"add:3").Accepted);Check(MixTray(w).reacted==4);
        });
        Test("all four modes have distinct persisted results and relevant ingredients",()=>{
            var w=MixWorld();Check(Mix(w,"add:5",1).Accepted);Check(Mix(w,"add:0",1).Accepted);Check(Mixing.Result(1,MixTray(w,1)).Contains("pink"));Check(Mix(w,"add:1",1,3).Accepted);Check(Mixing.Result(1,MixTray(w,1)).Contains("blue-green"));
            Check(Mix(w,"add:2",2,3).Accepted && Mix(w,"add:6",2,2).Accepted && Mix(w,"add:7",2).Accepted && Mix(w,"stir",2).Accepted);Check(MixTray(w,2).stir>0 && MixTray(w,2).reacted==0);
            Check(Mix(w,"add:4",3,4).Accepted && Mix(w,"add:2",3,2).Accepted);Check(Mixing.Oobleck(MixTray(w,3)));Check(Mix(w,"poke",3).Accepted && MixTray(w,3).poke>0);Check(Mix(w,"add:2",3,4).Accepted);Check(!Mixing.Oobleck(MixTray(w,3)));Check(!Mix(w,"add:6",3).Accepted);Check(!Mix(w,"add:7",1).Accepted);
            Check(Encode(SoloWorld.Restore(Decode(Encode(w.Snapshot()))).Snapshot())==Encode(w.Snapshot()));
        });
        Test("four simultaneous mixtures advance on authority and one reset or travel is local",()=>{
            var w=MixWorld();var ids=w.Snapshot().players.Select(p=>p.id).ToArray();foreach(var id in ids){Check(Mix(w,"add:1",amount:4,actor:id).Accepted);Check(Mix(w,"add:0",amount:4,actor:id).Accepted);}
            Check(w.AdvanceIdle(.6,out var visible) && visible);var remaining=MixTray(w,0,"second").reaction;Check(Mix(w,"rinse").Accepted);Check(MixTray(w).reacted==0 && ids.Skip(1).All(id=>MixTray(w,0,id).reacted==4));Good(w,SoloAction.Travel,value:"park",actor:"second");Check(!Mix(w,"rinse",actor:"second").Accepted);w.AdvanceIdle(.6,out _);Check(MixTray(w,0,"second").reaction<remaining);
            var session=new FamilySession(w);Check(session.Attach(1,"third",out _));Check(session.Attach(2,"fourth",out _));Check(session.Detach(1));Check(session.AdvanceIdle(.6,out _));Check(MixTray(w,0,"third").reaction<remaining);
        });
        Test("stale duplicate foreign malformed and capacity edits cannot duplicate mixtures",()=>{
            var w=MixWorld();var c=Command(w,SoloAction.Discovery,"first",Mixing.Token(0,MixTray(w)),"mix:0:add:1",1);Check(w.Apply(c).Accepted && w.Apply(c).Duplicate);Check(MixTray(w).amounts[1]==1);
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Discovery,"first","mix:0@0","mix:0:add:0",1)).Accepted);Check(!w.Apply(Command(w,SoloAction.Discovery,"second",Mixing.Token(0,MixTray(w,0,"second")),"mix:0:add:0",1)).Accepted);
            foreach(var op in new[]{"add:99","add:-1","add:x","add:6","unknown"})Check(!Mix(w,op).Accepted);Check(!Mix(w,"add:1",amount:5).Accepted);Check(Encode(w.Snapshot())==before);
            for(var i=0;i<7;i++)Check(Mix(w,"add:1",amount:4).Accepted);Check(Mix(w,"add:1",amount:3).Accepted);Check(!Mix(w,"add:1").Accepted);
        });
        Test("mixing corruption and aliasing are rejected and active progress restores without replay",()=>{
            var w=MixWorld();Check(Mix(w,"add:1",amount:2).Accepted && Mix(w,"add:0",amount:2).Accepted);w.AdvanceIdle(1,out _);var s=w.Snapshot();var restored=SoloWorld.Restore(Decode(Encode(s)));Check(MixTray(restored).reaction==MixTray(w).reaction);for(var i=0;i<15;i++)restored.AdvanceIdle(1,out _);Check(MixTray(restored).reaction==0 && MixTray(restored).reacted==2);
            void Reject(Action<SoloSnapshot> edit){var v=Decode(Encode(s));edit(v);Throws(()=>SoloWorld.Validate(v));}
            Reject(v=>v.discovery[0].mixtures[0].amounts[0]=-1);Reject(v=>v.discovery[0].mixtures[0].reacted=31);Reject(v=>v.discovery[0].mixtures[0].reaction=double.NaN);Reject(v=>v.discovery[0].mixtures=null);Reject(v=>v.discovery[0].mixtures[2].amounts[1]=1);Reject(v=>v.discovery[0].mixtures[0].amounts=new[]{1});
            var copy=SoloWorld.CopySnapshot(s);copy.discovery[0].mixtures[0].amounts[0]=9;Check(s.discovery[0].mixtures[0].amounts[0]==2);
        });
        Test("full mixing plus four coloring histories stay within message budget",()=>{
            var w=MixWorld();foreach(var id in w.Snapshot().players.Select(p=>p.id)){for(var page=0;page<6;page++)for(var n=0;n<25;n++)Check(Discover(w,"fill:0:"+(n%8+1),id,page).Accepted);for(var mode=0;mode<4;mode++)foreach(var ingredient in Mixing.Supplies[mode])Check(Mix(w,"add:"+ingredient,mode,4,id).Accepted);}
            var bytes=System.Text.Encoding.UTF8.GetByteCount(Encode(new FamilySession(w).View()));Check(bytes<100000);System.IO.File.WriteAllText(System.IO.Path.Combine(root,"mixing-payload.txt"),bytes+" bytes");
        });
    }
}
