using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

partial class Program
{
 static SoloResult Flexible(int actor,string op,string id,string kind="",float x=0,float y=0)=>Cmd(actor,SoloAction.Sandpit,op,DaycareSandpit.Target(id,w.ReadSandpit().round),x,y,kind);
 static SoloResult At(int actor,string id,string kind,float x,float y)
 {Need(SandDecorSurface.Resolve(M(id),kind,x,y,out var p),"local surface resolve "+kind);Need(Math.Abs(p.X-x)<=22 && Math.Abs(p.Y-y)<=22,"nearby support only");return Flexible(actor,"decorate",id,kind,p.X,p.Y);}
 static SandMould M(string id)=>w.ReadSandpit().moulds.Single(m=>m.id==id);
 static string ShapePiece(int a,string shape,int col,int row,int orientation=0)
 {var at=DaycareSandpit.PiecePoint(col,row,shape,orientation);var c=Command(a,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,DaycareSandpit.Choice(shape,orientation));Need(Send(a,c).Accepted,"direct construction "+shape);var id="p-"+c.requestId;Need(Tool(a,"water",id).Accepted,"water first");for(var i=0;i<3;i++)Need(Tool(a,"scoop",id).Accepted,"fill");Need(Tool(a,"tip",id).Accepted,"tip");return id;}
 static void FlexibleChecks()
 {
  Setup();var wall=ShapePiece(1,"wall",0,0);var rotated=ShapePiece(2,"gate",4,0,90);var tower=ShapePiece(3,"square",7,0);
  Need(Flexible(1,"decorate",wall,"window",-90,35).Accepted && Flexible(2,"decorate",wall,"window",-35,35).Accepted,"distinct windows on same wall shared contribution");
  var m=M(wall);Need(m.attachments.Length==2 && m.attachments[0].x!=m.attachments[1].x,"tap locations saved rather than slots");
  Need(SandDecorSurface.Resolve(m,"flag",-90,82,out var flag0),"first flag base");Need(SandDecorSurface.Resolve(m,"flag",40,66,out var flag1),"different flag base");
  Need(Flexible(2,"decorate",wall,"flag",flag0.X,flag0.Y).Accepted && Flexible(3,"decorate",wall,"flag",flag1.X,flag1.Y).Accepted,"several flags");
  var before=Json(w.ReadSandpit());Need(!Flexible(2,"decorate",wall,"window",-90,35).Accepted && Json(w.ReadSandpit())==before,"overlap never overwrites");
  Need(!Flexible(2,"decorate",rotated,"window",0,0).Accepted && !Flexible(2,"decorate",wall,"window",400,300).Accepted,"gate hole and unsupported surface reject");
  Need(At(2,rotated,"window",22,90).Accepted && At(3,rotated,"shell",25,150).Accepted,"rotated gate solid surfaces");
  Need(Flexible(3,"decorate",tower,"shell",-35,45).Accepted && Flexible(2,"decorate",tower,"pebble",25,105).Accepted,"mixed surface props");
  Need(Flexible(1,"ground-decorate","ground","shell",4092,430).Accepted && Flexible(2,"ground-decorate","ground","pebble",4190,430).Accepted && Flexible(3,"ground-decorate","ground","shell",4290,430).Accepted,"several ground props");
  before=Json(w.ReadSandpit());Need(!Flexible(3,"ground-decorate","ground","window",4380,430).Accepted && !Flexible(1,"ground-decorate","ground","shell",4092,430).Accepted && !Flexible(1,"ground-decorate","ground","shell",4000,430).Accepted && Json(w.ReadSandpit())==before,"ground type/overlap/edge preserve");
  var ground=w.ReadSandpit().ground[0];Need(!Flexible(2,"ground-remove","ground",ground.id).Accepted,"ground creator permission");
  var prop=M(wall).attachments[0];Need(!Flexible(2,"decor-remove",wall,M(wall).version+":"+prop.id).Accepted,"attached piece owner permission");
  var version=M(wall).version;Need(Flexible(1,"decor-replace",wall,version+":"+prop.id+":pebble",-90,35).Accepted,"explicit owner replacement preserves identity");
  Need(M(wall).attachments.Single(a=>a.id==prop.id).kind=="pebble","replacement kind saved");
  Need(!Flexible(1,"decor-remove",wall,version+":"+prop.id).Accepted,"stale editing preserves newer piece");
  Need(Flexible(1,"decor-remove",wall,M(wall).version+":"+prop.id).Accepted,"explicit decoration removal");
  var attachments=Json(M(wall).attachments);var grounds=Json(w.ReadSandpit().ground);var move=DaycareSandpit.PiecePoint(0,2,"wall",0);
  Need(Flexible(1,"move",wall,M(wall).version.ToString(),move.X,move.Y).Accepted && Json(M(wall).attachments)==attachments && Json(w.ReadSandpit().ground)==grounds,"piece-relative follow, independent ground");
  var captured=Command(2,SoloAction.Sandpit,"decorate",DaycareSandpit.Target(wall,w.ReadSandpit().round),35,30,"window");
  RetryConflict(2,captured,()=>Need(Flexible(3,"decorate",wall,"window",35,30).Accepted,"first contender"),false,"sand-decor-taken");
  captured=Command(2,SoloAction.Sandpit,"decorate",DaycareSandpit.Target(wall,w.ReadSandpit().round),100,15,"window");
  RetryConflict(2,captured,()=>Need(Flexible(3,"ground-decorate","ground","pebble",4490,430).Accepted,"separate contribution"),true);
  var once=Command(2,SoloAction.Sandpit,"ground-decorate",DaycareSandpit.Target("ground",w.ReadSandpit().round),4650,430,"shell");Need(Send(2,once).Accepted,"new stable ground identity");
  before=Json(w.ReadSandpit());Need(Send(2,once).Duplicate && Json(w.ReadSandpit())==before,"command UUID duplicate-safe");
  Need(Flexible(1,"ground-decorate","ground","pebble",4405,515).Accepted,"back sand edge exceeds walking-floor bound without moving players");
  var edge=w.ReadSandpit().ground.Last();Need(Flexible(1,"ground-remove","ground",edge.id).Accepted,"ground above walking floor removable by ID");before=Json(w.ReadSandpit());
  var save=w.Snapshot();var unrelated=Json(save.kingdom);var reopened=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options)));
  Need(Json(reopened.ReadSandpit())==Json(save.sandpit) && Json(reopened.ReadKingdom())==unrelated,"reopen preserves positions/IDs/other world");
  var copy=w.ReadSandpit();copy.ground[0].x++;copy.moulds[0].attachments[0].x++;Need(Json(w.ReadSandpit())==before,"deep copy protects attached and ground");
  var bad=w.Snapshot();bad.sandpit.ground[0].creator="outsider";Throws(()=>GameWorld.Validate(bad),"invalid ground creator");
  bad=w.Snapshot();bad.sandpit.moulds[0].attachments[0].x=float.NaN;Throws(()=>GameWorld.Validate(bad),"nonfinite position");
  bad=w.Snapshot();bad.sandpit.ground[1].id=bad.sandpit.ground[0].id;Throws(()=>GameWorld.Validate(bad),"duplicate decoration IDs");
  // Receipt eviction must not permit a reused UUID to create a second identity
  // at another point, even though normal network retry is caught earlier.
  save=w.Snapshot();save.receipts=save.receipts.Where(r=>r.requestId!=once.requestId).ToArray();w=GameWorld.Restore(save);family=new FamilySession(w);for(var a=1;a<=4;a++)Need(family.Attach((ulong)a,"p"+a,out _),"eviction fixture attach");
  var reused=Command(2,SoloAction.Sandpit,"ground-decorate",DaycareSandpit.Target("ground",w.ReadSandpit().round),4760,430,"shell");reused.requestId=once.requestId;Need(!Send(2,reused).Accepted && w.ReadSandpit().ground.Length==5,"saved ID remains unique after receipt eviction");
  // Limits guard the command itself; no rendering-only cap or silent truncation.
  var full=M(wall);full.attachments=Enumerable.Range(0,SandDecorSurface.PieceLimit).Select(i=>new SandAttachment{id="cap"+i,kind="window",x=1000+i*100,y=1000}).ToArray();Need(SandDecorSurface.AttachedReason(full,new SandAttachment{kind="window",x=-90,y=35})=="sand-decor-full","per-piece cap");
  copy=w.ReadSandpit();copy.ground=Enumerable.Range(0,SandDecorSurface.GroundLimit).Select(i=>new SandAttachment{id="cap"+i,kind="shell",x=1000+i*100,y=1000}).ToArray();Need(SandDecorSurface.GroundReason(copy,new SandAttachment{kind="shell",x=4650,y=430})=="sand-decor-full","ground cap");
  var edgePiece=new SandMould{shape="round",built=true,attachments=Array.Empty<SandAttachment>()};
  Need(SandDecorSurface.Resolve(edgePiece,"window",-58,100,out var edgeAt) && SandDecorSurface.Supported(edgePiece,new SandAttachment{kind="window",x=edgeAt.X,y=edgeAt.Y}) && Math.Abs(edgeAt.X+58)<=22,"measured edge support clamps nearby");
  Need(!SandDecorSurface.Supported(edgePiece,new SandAttachment{kind="window",x=-60,y=135}),"floating rounded corner rejected");
  Need(!SandDecorSurface.Supported(new SandMould{shape="wall",orientation=90,built=true},new SandAttachment{kind="window",x=-27,y=174}),"rotated wall empty silhouette rejected");
  // Serialization bound, deliberately not a claim that every surface can fit
  // sixteen props: long ASCII field lengths plus the existing world.
  var budget=w.Snapshot();var wide=new SandAttachment{id=new string('i',128),creator=new string('c',128),kind="window",x=9999.999f,y=9999.999f,version=int.MaxValue};
  budget.sandpit.moulds=Enumerable.Range(0,16).Select(i=>new SandMould{id="budget"+i,attachments=Enumerable.Range(0,16).Select(j=>wide.Copy()).ToArray()}).ToArray();
  budget.sandpit.ground=Enumerable.Range(0,48).Select(i=>wide.Copy()).ToArray();
  var budgetBytes=System.Text.Encoding.UTF8.GetByteCount(Json(budget));Need(budgetBytes<262144,"304-prop long-ASCII serialization envelope");Console.WriteLine("SAND_DECOR_BUDGET "+budgetBytes+" bytes (synthetic long-ASCII envelope, not gameplay state)");
  var wire=Json(save);Need(System.Text.Encoding.UTF8.GetByteCount(wire)<262144,"bounded current payload");
  Need(Flexible(1,"reset-request","reset").Accepted,"all-player consent");var vote=w.ReadSandpit().reset;Need(vote.voters.Length==4,"elsewhere voter included");
  grounds=Json(w.ReadSandpit().ground);Need(Flexible(4,"reset-decline","reset",vote.token).Accepted && Json(w.ReadSandpit().ground)==grounds,"decline without wiping new ground");
  Need(w.ReadSandpit().ground.Length==5 && w.ReadSandpit().moulds.Length==3,"decline retained creation");
  Need(Flexible(1,"reset-request","reset").Accepted,"new consent");vote=w.ReadSandpit().reset;for(var i=1;i<=4;i++)Need(Flexible(i,"reset-yes","reset",vote.token).Accepted,"explicit agreement");
  Need(w.ReadSandpit().ground.Length==0 && w.ReadSandpit().moulds.Length==0 && Json(w.ReadKingdom())==unrelated,"reset includes ground and attachments only");
  var doorWall=ShapePiece(1,"wall",0,0);Need(At(2,doorWall,"door",-85,40).Accepted && At(3,doorWall,"door",35,25).Accepted,"multiple distinct doors on visible face");
 }
}
