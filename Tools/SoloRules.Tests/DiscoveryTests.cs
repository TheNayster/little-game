using System;
using System.Linq;
using System.Text;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld DiscoveryWorld(){var w=SoloWorld.WithDiscovery(SoloWorld.Create("first","second","third","fourth"));foreach(var p in w.Snapshot().players)Good(w,SoloAction.Move,x:Discovery.ScienceX,y:200,actor:p.id);return w;}
    static DiscoveryWorkspace Workspace(SoloWorld w,string actor="first")=>w.ReadDiscovery().Single(x=>x.owner==actor);
    static SoloResult Discover(SoloWorld w,string op,string actor="first",int page=0,float x=0,float y=0)=>w.Apply(Command(w,SoloAction.Discovery,actor,Discovery.PageToken(page,Workspace(w,actor).pages[page]),op,x,y,actor));
    static void DiscoveryTests()
    {
        Test("discovery migration preserves all existing world records and is idempotent",()=>{
            var old=SoloWorld.WithCakeFlow(SoloWorld.Create("first","second","third","fourth"));
            Good(old,SoloAction.Move,x:-4250,y:310);Good(old,SoloAction.ChangeAvatar,value:"orange-pup");
            var before=old.Snapshot();var migrated=SoloWorld.WithDiscovery(old);var after=migrated.Snapshot();
            Check(after.schema==16 && after.discovery.Length==4 && after.revision==before.revision+1);
            after.schema=before.schema;after.revision=before.revision;after.discovery=Array.Empty<DiscoveryWorkspace>();Check(Encode(after)==Encode(before));
            Check(ReferenceEquals(migrated,SoloWorld.WithDiscovery(migrated)));
            Check(WorldLayout.MinX("garden",15)==-4800 && WorldLayout.MinX("garden",16)==-7200);
            Check(!WorldLayout.Position("garden",15,-6600,200) && WorldLayout.Position("garden",16,-6600,200));
        });
        Test("four science trays remain independent and survive avatar change travel and restore",()=>{
            var w=DiscoveryWorld();var ids=w.Snapshot().players.Select(p=>p.id).ToArray();
            foreach(var id in ids){for(var i=0;i<3;i++)Check(Discover(w,"cargo-add",id).Accepted);Check(Discovery.Sinks(Workspace(w,id)));Check(Discover(w,"wide",id).Accepted);Check(!Discovery.Sinks(Workspace(w,id)));}
            Check(Discover(w,"reset-float").Accepted);Check(ids.Skip(1).All(id=>Workspace(w,id).cargo==3));
            Check(Discover(w,"red").Accepted && Discover(w,"green").Accepted && Workspace(w).lights==3);Check(Discover(w,"blue").Accepted && Workspace(w).lights==7);
            Good(w,SoloAction.ChangeAvatar,value:"orange-pup");Good(w,SoloAction.Travel,value:"park");
            Check(!Discover(w,"reset-lights").Accepted);Check(Workspace(w).lights==7);Check(Workspace(SoloWorld.Restore(Decode(Encode(w.Snapshot())))).lights==7);
        });
        Test("magnetic material state moves iron only and remains bounded",()=>{
            var w=DiscoveryWorld();Check(Discover(w,"iron").Accepted);var a=Workspace(w);Check(a.ironY==290);
            Check(Discover(w,"magnet",x:210,y:210).Accepted);a=Workspace(w);Check(a.ironX==210 && a.ironY==250);
            Check(Discover(w,"aluminum").Accepted);a=Workspace(w);Check(a.magnetX==650 && a.ironX==210);
            var before=Encode(w.Snapshot());Check(!Discover(w,"magnet",x:800,y:460).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("coloring undo is page and owner scoped with durable bounded history",()=>{
            var w=DiscoveryWorld();foreach(var id in w.Snapshot().players.Select(p=>p.id))Check(Discover(w,"fill:0:5",id).Accepted);
            Check(Discover(w,"fill:0:6",page:1).Accepted);Check(Discover(w,"undo").Accepted);
            Check(Workspace(w).pages[0].colors[0]==0 && Workspace(w).pages[1].colors[0]==6 && Workspace(w,"second").pages[0].colors[0]==5);
            Check(Discover(w,"redo").Accepted && Workspace(w).pages[0].colors[0]==5);
            var restored=SoloWorld.Restore(Decode(Encode(w.Snapshot())));Check(Encode(restored.Snapshot())==Encode(w.Snapshot()));Check(Discover(restored,"undo",page:1).Accepted);
            for(var n=0;n<50;n++)Check(Discover(w,"fill:0:"+(n%8+1)).Accepted);Check(Workspace(w).pages[0].undo.Length==20);
        });
        Test("foreign-owner stale-page malformed and duplicate edits never corrupt pictures",()=>{
            var w=DiscoveryWorld();var c=Command(w,SoloAction.Discovery,"first","0@0","fill:0:5");Check(w.Apply(c).Accepted);Check(w.Apply(c).Duplicate);
            var before=Encode(w.Snapshot());var stale=Command(w,SoloAction.Discovery,"first","0@0","fill:1:3");Check(!w.Apply(stale).Accepted);
            foreach(var op in new[]{"fill:99:2","fill:0:9","fill:0:-1","fill:x:2","unknown"})Check(!Discover(w,op).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Discovery,"second","0@0","fill:0:5")).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("discovery validation rejects corrupt ownership geometry and page history",()=>{
            var baseline=DiscoveryWorld().Snapshot();void Reject(Action<SoloSnapshot> edit){var s=Decode(Encode(baseline));edit(s);Throws(()=>SoloWorld.Validate(s));}
            Reject(s=>s.discovery[1].owner="first");Reject(s=>s.discovery[0].cargo=8);Reject(s=>s.discovery[0].magnetX=float.NaN);
            Reject(s=>s.discovery[0].pages[0].colors[0]=9);Reject(s=>s.discovery[0].pages[0].undo=new[]{999});Reject(s=>s.discovery[0].pages=null);
            var copy=SoloWorld.CopySnapshot(baseline);copy.discovery[0].pages[0].colors[0]=3;Check(baseline.discovery[0].pages[0].colors[0]==0);
        });
        Test("four maximal coloring histories fit the existing world message budget",()=>{
            var w=DiscoveryWorld();foreach(var id in w.Snapshot().players.Select(p=>p.id))for(var page=0;page<6;page++)for(var n=0;n<40;n++)Check(Discover(w,"fill:0:"+(n%8+1),id,page).Accepted);
            var view=new FamilySession(w).View();var bytes=Encoding.UTF8.GetByteCount(Encode(view));Check(bytes<100000);
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"discovery-payload.txt"),bytes+" bytes, indented snapshot with four bounded histories");
            var restored=SoloWorld.Restore(Decode(Encode(w.Snapshot())));Check(restored.ReadDiscovery().All(x=>x.pages.All(p=>p.undo.Length==20)));
        });
        Test("disconnect releases only transient participation and never erases discovery creations",()=>{
            var w=DiscoveryWorld();var session=new FamilySession(w);Check(session.Attach(1,"first",out _));Check(session.Attach(2,"second",out _));
            Check(Discover(w,"fill:0:4").Accepted && Discover(w,"cargo-add","second").Accepted);var before=Encode(w.Snapshot());Check(session.Detach(1));Check(Encode(w.Snapshot())==before && session.ConnectedPlayers.SequenceEqual(new[]{"second"}));
        });
    }
}
