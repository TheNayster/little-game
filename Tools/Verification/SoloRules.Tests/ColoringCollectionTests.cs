using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static void ColoringCollectionTests()
    {
        Test("expanded coloring preserves all six original pages histories and sixteen mixtures",()=>{
            var old=MixWorld();Check(Discover(old,"fill:0:5").Accepted);Check(Discover(old,"fill:1:3").Accepted);Check(Discover(old,"undo").Accepted);Check(Mix(old,"add:5",1).Accepted);
            var before=old.Snapshot();var current=GameWorld.WithColoringCollection(old);var after=current.Snapshot();
            Check(after.schema==18 && after.discovery.All(w=>w.pages.Length==18));
            after.schema=before.schema;after.revision--;foreach(var w in after.discovery)w.pages=w.pages.Take(6).ToArray();Check(Encode(after)==Encode(before));
            Check(ReferenceEquals(current,GameWorld.WithColoringCollection(current)));
        });
        Test("all new Bluey pages support independent four-player fill undo redo and restart",()=>{
            var w=GameWorld.WithColoringCollection(MixWorld());var ids=w.Snapshot().players.Select(x=>x.id).ToArray();
            for(var p=6;p<18;p++)foreach(var id in ids){Check(Discover(w,"fill:"+(Discovery.Regions[p]-1)+":5",id,p).Accepted);Check(Discover(w,"undo",id,p).Accepted);Check(Discover(w,"redo",id,p).Accepted);}
            var before=Encode(w.Snapshot());Check(!Discover(w,"fill:114:4",page:17).Accepted);Check(Encode(w.Snapshot())==before);
            var restored=GameWorld.Restore(Decode(before));Check(Encode(restored.Snapshot())==before);
            Check(Discover(restored,"undo",page:17).Accepted);Check(Workspace(restored,"second").pages[17].colors[113]==5);
        });
        Test("four complete coloring collections remain within the shared view budget",()=>{
            var w=GameWorld.WithColoringCollection(MixWorld());var s=w.Snapshot();
            foreach(var ws in s.discovery)foreach(var page in ws.pages){for(var i=0;i<page.colors.Length;i++)page.colors[i]=i%9;page.undo=Enumerable.Range(0,20).Select(i=>(i%page.colors.Length)*16+i%9).ToArray();page.redo=(int[])page.undo.Clone();page.revision=100;}
            GameWorld.Validate(s);var view=new FamilySession(GameWorld.Restore(s)).View();var options=new JsonSerializerOptions{IncludeFields=true};var bytes=System.Text.Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(view,options));Check(bytes<100000);
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"expanded-coloring-payload.txt"),bytes+" compact bytes, four 18-page collections with max undo and redo");
        });
    }
}
