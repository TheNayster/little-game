using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static void SceneryTests()
    {
        Test("scenery upgrade preserves old identities items water receipts and positions",()=>{
            var old=SoloWorld.WithAreas(SoloWorld.Create("first","second"));
            Good(old,SoloAction.Grab,"bucket-1");Good(old,SoloAction.Drop,"bucket-1","tap-1",x:150,y:340);
            var before=old.Snapshot();var world=SoloWorld.WithScenery(old);var after=world.Snapshot();
            Check(after.schema==3 && after.worldId==before.worldId && after.revision==before.revision+1);
            Check(JsonSerializer.Serialize(before.players,Json)==JsonSerializer.Serialize(after.players,Json));
            Check(JsonSerializer.Serialize(before.toys,Json)==JsonSerializer.Serialize(after.toys,Json));
            Check(JsonSerializer.Serialize(before.receipts,Json)==JsonSerializer.Serialize(after.receipts,Json));
            Check(ReferenceEquals(world,SoloWorld.WithScenery(world)) && ReferenceEquals(world,SoloWorld.WithAreas(world)));
        });
        Test("all six entrances retain siblings and each scenic world has long walking bounds",()=>{
            var w=SoloWorld.WithScenery(SoloWorld.Create("first","second"));var sibling=JsonSerializer.Serialize(w.ReadPlayer("second"),Json);
            foreach(var place in new[]{"home","park","creek","beach","daycare","garden"})
            {
                Good(w,SoloAction.Travel,value:place);var p=w.ReadPlayer("first");Check(WorldLayout.Place(p)==place);
                for(var i=0;i<1000;i++)Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,.05f);
                Check(w.ReadPlayer("first").x>2000);SoloWorld.Validate(w.Snapshot());
                Check(JsonSerializer.Serialize(w.ReadPlayer("second"),Json)==sibling);
            }
        });
        Test("house and garden boundary is walking in one persistent property",()=>{
            var w=SoloWorld.WithScenery(SoloWorld.Create("first","second"));Good(w,SoloAction.Move,x:1,y:200);Good(w,SoloAction.Grab,"bucket-1");
            var visit=w.ReadPlayer("first").visit;var before=w.Snapshot();
            Walking.AdvanceLocal(w,"first",WalkMode.Direction,-1,0,.05f);
            Check(WorldLayout.Place(w.ReadPlayer("first"))=="home" && w.ReadPlayer("first").visit==visit);
            Check(w.Revision==before.revision && w.ReadToys().Single(t=>t.id=="bucket-1").holder=="first");
            Good(w,SoloAction.CancelGrab,"bucket-1");var restored=SoloWorld.Restore(w.Snapshot());
            Check(restored.ReadPlayer("first").x<0 && restored.Snapshot().toys.Length==10);
        });
        Test("scenic saves reject off-map coordinates and aliases as persistent areas",()=>{
            var w=SoloWorld.WithScenery(SoloWorld.Create("first"));
            foreach(var x in new[]{-4801f,4801,float.NaN,float.PositiveInfinity}){var s=w.Snapshot();s.players[0].x=x;Throws(()=>SoloWorld.Validate(s));}
            var alias=w.Snapshot();alias.players[0].zone="home";Throws(()=>SoloWorld.Validate(alias));
            var old=SoloWorld.WithAreas(SoloWorld.Create("first")).Snapshot();old.players[0].x=2000;Throws(()=>SoloWorld.Validate(old));
            Good(w,SoloAction.Travel,value:"park");var park=w.Snapshot();park.players[0].x=-1;Throws(()=>SoloWorld.Validate(park));
            Check(!w.Apply(new SoloCommand{requestId="bad-activity",actor="first",zone="park",visit=w.ReadPlayer("first").visit,expectedRevision=w.Revision,action=SoloAction.StartActivity,value="garden"}).Accepted);
        });
        Test("scenic recovery retains new areas and negative home positions",()=>{
            var r=RecoveryFixture();var w=SoloWorld.WithScenery(SoloWorld.Restore(r.snapshot));var p=w.ReadPlayer(r.snapshot.players[0].id);
            Check(w.Apply(new SoloCommand{requestId="home-trip",actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=SoloAction.Travel,value="home"}).Accepted);
            r.content=4;r.snapshot=w.Snapshot();r.Validate(r.family,r.authority,r.world);
            Check(r.snapshot.players[0].x<0);r.content=3;Throws(()=>r.Validate(r.family,r.authority,r.world));
        });
    }
}
