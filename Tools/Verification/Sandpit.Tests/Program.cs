using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;
using LittleWeeps.Client;

partial class Program
{
 static GameWorld w;static FamilySession family;
 static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};
 static int checks;
 static void Need(bool v,string why){checks++;if(!v)throw new Exception(why);}
 static string Json(object value)=>JsonSerializer.Serialize(value,options);
 static SoloCommand Command(int a,SoloAction action,string op="",string target="",float x=0,float y=0,string item="")
 {var p=w.ReadPlayer("p"+a);return new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=action,value=op,target=target,x=x,y=y,item=item};}
 static SoloResult Send(int a,SoloCommand command){var r=family.Submit((ulong)a,command);GameWorld.Validate(w.Snapshot());return r;}
 static SoloResult Cmd(int a,SoloAction action,string op="",string target="",float x=0,float y=0,string item="")=>Send(a,Command(a,action,op,target,x,y,item));
 static void Move(int a,SandMould m){var at=DaycareSandpit.Work(m);Need(Cmd(a,SoloAction.Move,x:at.X,y:at.Y).Accepted,"reachable work");}
 static SoloCommand ToolCommand(int a,string op,string id,int epoch=-1)=>Command(a,SoloAction.Sandpit,op,DaycareSandpit.Target(id,epoch<0?w.ReadSandpit().round:epoch));
 static SoloResult Tool(int a,string op,string id){Move(a,w.ReadSandpit().moulds.Single(m=>m.id==id));return Send(a,ToolCommand(a,op,id));}
 static string Place(int a,int col,int row)
 {var at=DaycareSandpit.Cell(col,row);var c=Command(a,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"round");Need(Send(a,c).Accepted,"place");return "p-"+c.requestId;}
 static void RetryConflict(int a,SoloCommand captured,Action siblingAction,bool shouldAccept,string outcome=null)
 {
  var queue=new WorldCommandQueue();SoloResult reply=default;Need(queue.Enqueue(captured,r=>reply=r),"enqueue");
  var sent=queue.Take(w.Revision);siblingAction();var conflict=Send(a,sent);Need(!conflict.Accepted && conflict.Outcome=="stale-revision","real revision conflict");
  queue.Complete(sent.requestId,conflict);var retry=queue.Take(w.Revision);
  Need(retry.target==captured.target && retry.x==captured.x && retry.y==captured.y && retry.requestId==captured.requestId,"retry target/location/identity preserved");
  var result=Send(a,retry);Need(result.Accepted==shouldAccept && (outcome==null || result.Outcome==outcome),"retry result "+result.Outcome);queue.Complete(retry.requestId,result);Need(!queue.Busy && reply.Accepted==shouldAccept,"queue completes");
 }
 static void Setup()
 {
  w=GameWorld.WithDinosaurWorld(GameWorld.Create("p1","p2","p3","p4"));family=new FamilySession(w);
  for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"travel");}
  Need(Cmd(1,SoloAction.Sandpit,"start").Accepted,"start");
 }
 static void Main(string[] args)
 {
  if(args.Any(a=>a!="--flexible"))throw new ArgumentException("Use --flexible or no arguments for current rules.");
  FlexibleChecks();Console.WriteLine("SAND_DECOR_PASS "+checks+" checks");
 }
 // Retained socket-era fixtures document the superseded contract. Current
 // migrations execute through SandpitJsonTests and FlexibleChecks instead.
 static void SocketEraFixtures()
 {
  Setup();Need(w.ReadSandpit().moulds.Length==0 && w.ReadSandpit().members.Count(m=>m.attending)==3,"empty shared creative start");
  var a=Place(1,0,0);var b=Place(2,3,1);var cast=w.ReadSandpit().friends.ToArray();
  Need(Tool(1,"water",a).Accepted && Tool(2,"water",a).Accepted,"early redundant water");
  var before=Json(w.ReadSandpit().moulds);
  var under=Tool(1,"tip",a);Need(!under.Accepted && under.Outcome=="fill-bucket-first" && Json(w.ReadSandpit().moulds)==before,"underfilled no progress loss");
  Move(1,w.ReadSandpit().moulds[0]);Move(2,w.ReadSandpit().moulds[0]);
  RetryConflict(2,ToolCommand(2,"scoop",a),()=>Need(Send(1,ToolCommand(1,"scoop",a)).Accepted,"sibling scoop"),true);
  Need(Tool(3,"scoop",a).Accepted && w.ReadSandpit().moulds[0].scoops==3,"three shared scoops");
  Need(!Tool(2,"scoop",a).Accepted && w.ReadSandpit().moulds[0].scoops==3,"bounded fill");
  Move(1,w.ReadSandpit().moulds[0]);Move(2,w.ReadSandpit().moulds[0]);
  RetryConflict(2,ToolCommand(2,"tip",a),()=>Need(Send(1,ToolCommand(1,"tip",a)).Accepted,"first tip"),false,"tower-built");
  Need(w.ReadSandpit().moulds.Count(m=>m.built)==1,"one competing tip result");
  var c=Place(1,5,2);Need(w.ReadSandpit().moulds[0].built && w.ReadSandpit().moulds.Length==3 && w.ReadSandpit().phase==1,"another piece during demo without restart");
  for(var n=0;n<3;n++)Need(Tool(2,"scoop",b).Accepted,"dry scoops");
  before=Json(w.ReadSandpit().moulds);var dry=Tool(2,"tip",b);
  Need(!dry.Accepted && dry.Outcome=="add-water-first" && Json(w.ReadSandpit().moulds)==before,"dry retains full fill");
  Need(Tool(3,"scoop",c).Accepted && Tool(2,"water",b).Accepted && Tool(2,"tip",b).Accepted,"different-piece work and Water Tip no refill");
  var at=DaycareSandpit.Cell(7,3);var place=Command(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"round");
  Need(Send(1,place).Accepted,"receipt placement");before=Json(w.Snapshot());var dup=Send(1,place);Need(dup.Accepted && dup.Duplicate && Json(w.Snapshot())==before,"placement duplicate no mutation");
  at=DaycareSandpit.Cell(2,3);var racing=Command(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"round");
  RetryConflict(1,racing,()=>Place(2,2,3),false,"sand-spot-taken");
  Need(!w.ReadSandpit().moulds.Any(m=>m.id=="p-"+racing.requestId),"retry never silently relocates");
  before=Json(w.ReadSandpit().moulds);
  Need(!Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),5000,200,"round").Accepted && Json(w.ReadSandpit().moulds)==before,"outside atomic");
  Need(!Send(1,ToolCommand(1,"water","missing")).Accepted,"unknown identity");
  Need(!Send(1,ToolCommand(1,"water",c,w.ReadSandpit().round+1)).Accepted,"wrong epoch");
  Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late travel");family.AdvanceIdle(.1,out _);
  Need(w.ReadSandpit().members.All(m=>m.attending) && w.ReadSandpit().friends.SequenceEqual(cast),"late joins existing creation");
  before=Json(w.ReadSandpit().moulds);Need(Cmd(2,SoloAction.Sandpit,"leave").Accepted,"independent leave");family.AdvanceIdle(.1,out _);
  Need(w.ReadSandpit().members.Count(m=>m.attending)==3 && Json(w.ReadSandpit().moulds)==before,"leave preserves siblings");
  Need(family.Detach(3) && w.ReadSandpit().members.Count(m=>m.attending)==2,"independent disconnect");
  Need(family.Attach(3,"p3",out _) && w.ReadSandpit().members.Single(m=>m.actor=="p3").attending,"reconnect");
  var epoch=w.ReadSandpit().round;Need(Cmd(1,SoloAction.Sandpit,"replay").Accepted && w.ReadSandpit().round==epoch && Json(w.ReadSandpit().moulds)==before,"old replay resumes without clear");
  // Fill all remaining free cells up to the configured cap, including unfinished pieces.
  for(var row=0;row<4 && w.ReadSandpit().moulds.Length<16;row++)for(var col=0;col<8 && w.ReadSandpit().moulds.Length<16;col++){at=DaycareSandpit.Cell(col,row);if(DaycareSandpit.Placement(w.ReadSandpit(),at.X,at.Y)==null)Place(1,col,row);}
  Need(w.ReadSandpit().moulds.Length==16,"cap reached");before=Json(w.ReadSandpit().moulds);
  var full=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",epoch),DaycareSandpit.Cell(7,0).X,130,"round");
  Need(!full.Accepted && full.Outcome=="sandpit-full" && Json(w.ReadSandpit().moulds)==before,"cap preserves all work");
  var reopened=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(w.Snapshot()),options)));
  Need(Json(reopened.ReadSandpit().moulds)==before && reopened.ReadSandpit().friends.SequenceEqual(cast),"current JSON reopen");
  Need(System.Text.Encoding.UTF8.GetByteCount(Json(family.View()))<131072,"bounded snapshot fits wire budget");
  var copied=reopened.ReadSandpit();copied.moulds[0].scoops=0;Need(reopened.ReadSandpit().moulds[0].scoops==3,"read copy isolation");
  var newEpoch=reopened.Snapshot();newEpoch.sandpit.round++;var replacement=GameWorld.Restore(newEpoch);var oldIntent=ToolCommand(1,"water",c,epoch);oldIntent.expectedRevision=replacement.Revision;Need(!replacement.Apply(oldIntent).Accepted,"replacement epoch rejects old intent");
  // Synthetic schema49 fixture: no real saves are opened or changed.
  var fixture=w.Snapshot();fixture.schema=49;var legacy=fixture.sandpit;legacy.format=legacy.pieceLimit=legacy.scoopCapacity=0;legacy.round=7;legacy.phase=3;
  legacy.moulds=new[]{
   new SandMould{scoops=2,wet=true,built=true,decoration=1},
   new SandMould{scoops=3,wet=true,built=true,decoration=2},
   new SandMould{scoops=2,wet=true,built=true},
   new SandMould{scoops=3,wet=true,built=true}};
  GameWorld.Validate(fixture);var source=Json(fixture);var migrated=GameWorld.WithDinosaurWorld(GameWorld.Restore(fixture));Need(Json(fixture)==source,"migration input untouched");
  var saved=migrated.ReadSandpit();Need(saved.round==7 && saved.phase==3 && Json(saved.members)==Json(legacy.members) && saved.friends.SequenceEqual(legacy.friends),"attendance cast epoch preserved");
  for(var i=0;i<4;i++){var m=saved.moulds[i];Need(m.id=="legacy-"+i && string.IsNullOrEmpty(m.creator) && m.capacity==DaycareSandpit.Capacity(i) && m.x==DaycareSandpit.Place(i).X && m.y==440 && m.scoops==legacy.moulds[i].scoops && m.wet==legacy.moulds[i].wet && m.built && m.decoration==0 && (legacy.moulds[i].decoration==0?m.attachments.Length==0:m.attachments.Length==1 && m.attachments[0].kind==(legacy.moulds[i].decoration==1?"flag":"shell")),"legacy field retention "+i);}
  Need(Json(migrated.Snapshot().players)==Json(fixture.players) && Json(migrated.Snapshot().toys)==Json(fixture.toys) && Json(migrated.Snapshot().kingdom)==Json(fixture.kingdom) && migrated.Snapshot().worldId==fixture.worldId,"unrelated world retained");
  var twice=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(migrated.Snapshot()),options)));Need(Json(twice.ReadSandpit())==Json(saved) && twice.Revision==migrated.Revision,"deterministic idempotent reopening");
  // Partial legacy pieces, including original alternating scoop capacity.
  fixture.schema=49;fixture.sandpit.phase=2;fixture.sandpit.moulds[0]=new SandMould{scoops=1};fixture.sandpit.moulds[1]=new SandMould{scoops=2,wet=true};
  migrated=GameWorld.WithDinosaurWorld(GameWorld.Restore(fixture));Need(migrated.ReadSandpit().moulds[0].scoops==1 && !migrated.ReadSandpit().moulds[0].wet && !migrated.ReadSandpit().moulds[0].built && migrated.ReadSandpit().moulds[1].scoops==2 && migrated.ReadSandpit().moulds[1].wet && migrated.ReadSandpit().moulds[1].capacity==3,"partial legacy retention");
  var older=fixture;older.schema=45;older.sandpit=null;var firstUpgrade=GameWorld.WithDinosaurWorld(GameWorld.Restore(older));var secondUpgrade=GameWorld.WithDinosaurWorld(GameWorld.Restore(older));Need(Json(firstUpgrade.ReadSandpit())==Json(secondUpgrade.ReadSandpit()),"deterministic missing-sandpit migration");
  var invalid=twice.Snapshot();invalid.sandpit.moulds[1].id=invalid.sandpit.moulds[0].id;Throws(()=>GameWorld.Validate(invalid),"duplicate ids");
  invalid=twice.Snapshot();invalid.sandpit.moulds[1].x=invalid.sandpit.moulds[0].x;Throws(()=>GameWorld.Validate(invalid),"overlapping saved footprint");
  invalid=twice.Snapshot();invalid.sandpit.moulds[0].capacity=0;Throws(()=>GameWorld.Validate(invalid),"invalid capacity");
  FeedbackChecks(twice.ReadSandpit());Need(DaycareSandpit.UsefulScoops(new SandMould{capacity=3,scoops=1},100)==2 && DaycareSandpit.UsefulScoops(new SandMould{capacity=3,scoops=3},100)==0,"retained useful bound");
  ShapeChecks();PlayChecks();
  Console.WriteLine("PASS "+checks+" focused checks: Stage 2 recovery/concurrency/legacy checks plus Stage 3 shapes, rotated footprints, atomic conflicts/cap, Stage 2 migration and oriented save retention. Stage 4 attachments/toy/versioned edits/global reset checked. Native transport/rendering not exercised.");
 }
 static string ShapePlace(int actor,string shape,int orientation,int col,int row)
 {var at=DaycareSandpit.PiecePoint(col,row,shape,orientation);var c=Command(actor,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,DaycareSandpit.Choice(shape,orientation));Need(Send(actor,c).Accepted,"shape place "+shape+orientation);return "p-"+c.requestId;}
 static void ShapeChecks()
 {
  Setup();Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"four shape builders");family.AdvanceIdle(.1,out _);
  var round=ShapePlace(1,"round",0,0,0);var wall=ShapePlace(2,"wall",0,1,0);var gate=ShapePlace(3,"gate",0,3,0);var square=ShapePlace(4,"square",0,5,0);
  var spine=ShapePlace(1,"wall",90,7,1);var sideGate=ShapePlace(2,"gate",90,0,2);
  foreach(var id in new[]{round,wall,gate,square,spine,sideGate}){
   Need(Tool(1,"water",id).Accepted,"shape early water");for(var i=0;i<3;i++)Need(Tool(i%3+1,"scoop",id).Accepted,"shape cooperative fill");Need(Tool(4,"tip",id).Accepted,"shape tip");
  }
  var before=Json(w.ReadSandpit().moulds);
  var at=DaycareSandpit.PiecePoint(7,0,"wall",0);
  var r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"wall");Need(!r.Accepted && r.Outcome=="outside-sandpit" && Json(w.ReadSandpit().moulds)==before,"two cell boundary atomic");
  at=DaycareSandpit.PiecePoint(3,3,"gate",90);r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"gate:90");Need(!r.Accepted && DaycareSandpit.Placement(w.ReadSandpit(),at.X,at.Y,"gate",90)=="outside-sandpit","rotated boundary");
  at=DaycareSandpit.Cell(2,0);r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"square");Need(!r.Accepted && r.Outcome=="sand-spot-taken" && Json(w.ReadSandpit().moulds)==before,"second cell occupied");
  at=DaycareSandpit.Cell(7,2);r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"round");Need(!r.Accepted && r.Outcome=="sand-spot-taken","rotated second cell occupied");
  at=DaycareSandpit.Cell(6,3);r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"square:90");Need(!r.Accepted && r.Outcome=="try-sand-tools","invalid orientation rejected");
  at=DaycareSandpit.PiecePoint(3,1,"wall",90);var competing=Command(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"wall:90");
  RetryConflict(1,competing,()=>ShapePlace(2,"gate",90,3,1),false,"sand-spot-taken");
  var save=w.Snapshot();var restored=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options)));
  Need(Json(restored.ReadSandpit())==Json(save.sandpit),"all oriented shapes reopen");
  var bad=JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options);bad.sandpit.moulds.Single(m=>m.id==spine).orientation=0;Throws(()=>GameWorld.Validate(bad),"orientation/footprint mismatch");
  bad=JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options);bad.sandpit.moulds.Single(m=>m.id==spine).width=float.NaN;Throws(()=>GameWorld.Validate(bad),"nonfinite footprint");
  // A real format-one Stage 2 checkpoint retains exact fields, including a
  // legacy footprint already migrated in Stage 2. No coordinate resnapping.
  var old=JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options);old.schema=50;old.sandpit.format=1;old.sandpit.moulds=old.sandpit.moulds.Where(m=>m.shape=="round").ToArray();
  old.sandpit.moulds[0].x=4210;old.sandpit.moulds[0].y=440;old.sandpit.moulds[0].width=140;old.sandpit.moulds[0].depth=100;old.sandpit.moulds[0].capacity=old.sandpit.moulds[0].scoops=2;old.sandpit.moulds[0].decoration=1;
  var original=Json(old.sandpit.moulds);var upgraded=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(old),options)));
  old.sandpit.moulds[0].attachments=new[]{new SandAttachment{slot=0,kind="flag"}};old.sandpit.moulds[0].decoration=0;original=Json(old.sandpit.moulds);
  Need(upgraded.Schema==WorldLayout.Schema && upgraded.ReadSandpit().format==DaycareSandpit.Format && Json(upgraded.ReadSandpit().moulds)==original,"Stage 2 migration preserves exact piece fields");
  Need(Json(upgraded.Snapshot().players)==Json(old.players) && Json(upgraded.Snapshot().toys)==Json(old.toys) && Json(upgraded.Snapshot().kingdom)==Json(old.kingdom) && Json(upgraded.ReadSandpit().members)==Json(old.sandpit.members),"Stage 2 migration preserves participation/unrelated data");
  for(var row=0;row<4 && w.ReadSandpit().moulds.Length<16;row++)for(var col=0;col<8 && w.ReadSandpit().moulds.Length<16;col++){at=DaycareSandpit.Cell(col,row);if(DaycareSandpit.Placement(w.ReadSandpit(),at.X,at.Y)==null)ShapePlace(1,"square",0,col,row);}
  Need(w.ReadSandpit().moulds.Length==16,"long pieces count once toward cap");before=Json(w.ReadSandpit().moulds);at=DaycareSandpit.PiecePoint(1,2,"gate",0);r=Cmd(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),at.X,at.Y,"gate");Need(!r.Accepted && r.Outcome=="sandpit-full" && Json(w.ReadSandpit().moulds)==before,"shape cap atomic");
 }
 static SoloResult Play(int a,string op,string id,string item="",float x=0,float y=0)=>Cmd(a,SoloAction.Sandpit,op,DaycareSandpit.Target(id,w.ReadSandpit().round),x,y,item);
 static void PlayChecks()
 {
  Setup();
  var ids=new[]{ShapePlace(1,"round",0,0,0),ShapePlace(2,"square",0,2,0),ShapePlace(3,"wall",0,4,0),ShapePlace(1,"gate",0,0,2),ShapePlace(2,"wall",90,4,2),ShapePlace(3,"gate",90,7,2)};
  foreach(var id in ids){for(var i=0;i<3;i++)Need(Tool(1,"scoop",id).Accepted,"play build scoop");Need(Tool(1,"water",id).Accepted && Tool(1,"tip",id).Accepted,"play built");
   foreach(var (slot,kind) in new[]{(0,"flag"),(1,"flag"),(2,"shell"),(3,"pebble"),(4,"door"),(5,"window")})Need(Play(2,"decorate",id,slot+":"+kind).Accepted,"applicable socket "+kind);
   Need(w.ReadSandpit().moulds.Single(m=>m.id==id).attachments.Length==6,"all shapes retain combined props");
  }
  var m=w.ReadSandpit().moulds[0];var before=Json(w.ReadSandpit().moulds);
  Need(!Play(1,"decorate",m.id,"0:door").Accepted && !Play(1,"decorate",m.id,"6:window").Accepted && Json(w.ReadSandpit().moulds)==before,"invalid attachments atomic");
  Need(!Play(3,"remove",m.id,m.version.ToString()).Accepted && !Play(3,"move",m.id,m.version.ToString(),4405,350).Accepted,"other creator edit forbidden");
  Need(Play(1,"decor-remove",m.id,m.version+":1").Accepted,"explicit prop removal");m=w.ReadSandpit().moulds[0];
  var captured=Command(1,SoloAction.Sandpit,"move",DaycareSandpit.Target(m.id,w.ReadSandpit().round),4405,350,m.version.ToString());
  RetryConflict(1,captured,()=>Need(Play(2,"decorate",m.id,"1:flag").Accepted,"intervening decor"),false,"sand-piece-changed");
  m=w.ReadSandpit().moulds[0];before=Json(w.ReadSandpit().moulds);
  Need(!Play(1,"move",m.id,m.version.ToString(),5000,350).Accepted && Json(w.ReadSandpit().moulds)==before,"outside move atomic");
  var at=DaycareSandpit.Cell(3,1);var props=Json(m.attachments);var fill=m.scoops;
  Need(Play(1,"move",m.id,m.version.ToString(),at.X,at.Y).Accepted && Json(w.ReadSandpit().moulds[0].attachments)==props && w.ReadSandpit().moulds[0].scoops==fill,"move carries complete work");
  m=w.ReadSandpit().moulds[0];Need(Play(1,"decor-replace",m.id,m.version+":3:shell").Accepted,"explicit owner replacement");
  m=w.ReadSandpit().moulds[0];before=Json(w.ReadSandpit().moulds);
  Need(!Play(2,"decor-replace",m.id,m.version+":3:pebble").Accepted && !Play(1,"decor-remove",m.id,(m.version-1)+":3").Accepted && Json(w.ReadSandpit().moulds)==before,"replacement permission and captured version");
  Need(Play(1,"decor-remove",m.id,m.version+":0").Accepted,"free race slot");
  var race=Command(2,SoloAction.Sandpit,"decorate",DaycareSandpit.Target(m.id,w.ReadSandpit().round),item:"0:flag");
  RetryConflict(2,race,()=>Need(Play(3,"decorate",m.id,"0:flag").Accepted,"first empty socket wins"),false,"sand-slot-taken");
  m=w.ReadSandpit().moulds[0];Need(Play(1,"decor-remove",m.id,m.version+":1").Accepted,"free distinct slot");
  var once=Command(3,SoloAction.Sandpit,"decorate",DaycareSandpit.Target(m.id,w.ReadSandpit().round),item:"1:flag");Need(Send(3,once).Accepted,"decoration accepted");before=Json(w.ReadSandpit());Need(Send(3,once).Duplicate && Json(w.ReadSandpit())==before,"duplicate does not duplicate props");
  var pos=DaycareSandpit.Cell(6,1);Need(Play(1,"toy-place","toy","0",pos.X,pos.Y).Accepted,"saved one toy");
  var toyRace=Command(2,SoloAction.Sandpit,"toy-place",DaycareSandpit.Target("toy",w.ReadSandpit().round),4092,130,"1");
  RetryConflict(2,toyRace,()=>Need(Play(3,"toy-react","toy").Accepted,"shared short reaction"),false,"sand-toy-changed");
  Need(w.ReadSandpit().toy.reaction==1 && !Play(2,"toy-place","toy",w.ReadSandpit().toy.version.ToString(),5000,200).Accepted,"toy bounds");
  var save=w.Snapshot();var unrelated=Json(save.kingdom);var retained=Json(save.sandpit);Need(Json(GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(save),options)).ReadSandpit())==retained,"decorated castle and toy reopen");
  m=w.ReadSandpit().moulds[0];var rest=Json(w.ReadSandpit().moulds.Skip(1).ToArray());Need(Play(1,"remove",m.id,m.version.ToString()).Accepted && Json(w.ReadSandpit().moulds)==rest && w.ReadSandpit().toy.placed,"only targeted piece removed");
  Need(Play(1,"reset-request","reset").Accepted,"request global agreement");var vote=w.ReadSandpit().reset;Need(vote.voters.Length==4 && vote.approved.Length==0,"outside Daycare voter included initiator not implicit yes");
  Need(Play(4,"reset-decline","reset",vote.token).Accepted && w.ReadSandpit().reset==null,"outside player decline");
  Need(Play(1,"reset-request","reset").Accepted,"request again");vote=w.ReadSandpit().reset;Need(Play(1,"reset-yes","reset",vote.token).Accepted && w.ReadSandpit().moulds.Length==5,"one yes not all");
  Need(Play(2,"toy-react","toy").Accepted && w.ReadSandpit().reset==null,"toy change cancels agreement");
  Need(Play(1,"reset-request","reset").Accepted,"membership test request");family.Detach(4);Need(w.ReadSandpit().reset==null,"disconnect cancels never yes");Need(family.Attach(4,"p4",out _),"reconnect");
  Need(Play(1,"reset-request","reset").Accepted,"intervening castle request");m=w.ReadSandpit().moulds.First(v=>v.creator=="p2");Need(Play(2,"decor-remove",m.id,m.version+":0").Accepted && w.ReadSandpit().reset==null,"castle change cancels");
  Need(Play(1,"reset-request","reset").Accepted,"new join cancellation request");family.Detach(4);Need(Play(1,"reset-request","reset").Accepted,"three player vote");family.Attach(4,"p4",out _);Need(w.ReadSandpit().reset==null,"new connection cancels");
  var epoch=w.ReadSandpit().round;var stale=Command(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",epoch),4150,130,"round");
  Need(Play(1,"reset-request","reset").Accepted,"complete reset request");vote=w.ReadSandpit().reset;
  for(var tick=0;tick<240;tick++)family.AdvanceIdle(.25,out _);Need(w.ReadSandpit().reset!=null && w.ReadSandpit().moulds.Length==5,"elapsed time never consent");
  unrelated=Json(w.ReadKingdom());for(var a=1;a<=4;a++)Need(Play(a,"reset-yes","reset",vote.token).Accepted,"each explicit agreement including elsewhere");
  Need(w.ReadSandpit().moulds.Length==0 && !w.ReadSandpit().toy.placed && w.ReadSandpit().round==epoch+1 && Json(w.ReadKingdom())==unrelated,"reset clears only sand creation and invalidates epoch");
  stale.expectedRevision=w.Revision;Need(!Send(1,stale).Accepted,"old epoch cannot reappear");
  family.Detach(2);family.Detach(3);family.Detach(4);Need(Play(1,"reset-request","reset").Accepted && w.ReadSandpit().reset.approved.Length==0,"single player explicit consent");vote=w.ReadSandpit().reset;Need(Play(1,"reset-yes","reset",vote.token).Accepted,"single confirm");
  var solo=GameWorld.WithDinosaurWorld(GameWorld.Create("solo"));var p=solo.ReadPlayer("solo");
  SoloResult Local(string op,string target="",string item="") {p=solo.ReadPlayer("solo");return solo.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor="solo",zone=p.zone,visit=p.visit,expectedRevision=solo.Revision,action=op=="daycare"?SoloAction.Travel:SoloAction.Sandpit,value=op,item=item,target=target});}
  Need(Local("daycare").Accepted && Local("start").Accepted && Local("reset-request",DaycareSandpit.Target("reset",1)).Accepted,"offline local request");Need(solo.ReadSandpit().reset.voters.SequenceEqual(new[]{"solo"}) && Local("reset-yes",DaycareSandpit.Target("reset",1),solo.ReadSandpit().reset.token).Accepted,"offline local confirm");
  Setup();var special=Command(1,SoloAction.Sandpit,"place",DaycareSandpit.Target("place",w.ReadSandpit().round),4150,130,"round");special.requestId="saved~identity";Need(Send(1,special).Accepted,"previously valid stable ID characters");var stable="p-"+special.requestId;
  for(var i=0;i<3;i++)Need(Tool(1,"scoop",stable).Accepted,"stable ID filling");Need(Tool(1,"water",stable).Accepted && Tool(1,"tip",stable).Accepted && Play(2,"decorate",stable,"0:flag").Accepted,"decoration payload never splits existing ID");
  m=w.ReadSandpit().moulds[0];Need(Play(1,"decorate",stable,"2:shell").Accepted,"combined saved decorations");
  var copy=w.ReadSandpit();copy.moulds[0].attachments[0].kind="window";Need(w.ReadSandpit().moulds[0].attachments[0].kind=="flag","attachment copy isolation");
  var bad=w.Snapshot();bad.sandpit.moulds[0].attachments=new[]{new SandAttachment{slot=0,kind="flag"},new SandAttachment{slot=0,kind="flag"}};Throws(()=>GameWorld.Validate(bad),"duplicate sockets");
  bad=w.Snapshot();bad.sandpit.moulds[0].attachments[0].kind="door";Throws(()=>GameWorld.Validate(bad),"invalid saved socket kind");
  bad=w.Snapshot();bad.sandpit.toy=new SandToy{placed=true,x=4150,y=130};Throws(()=>GameWorld.Validate(bad),"toy overlaps castle checkpoint");
  before=Json(w.ReadSandpit().moulds);at=DaycareSandpit.Cell(2,0);var other=Place(2,2,0);m=w.ReadSandpit().moulds[0];var complete=Json(w.ReadSandpit().moulds);Need(!Play(1,"move",stable,m.version.ToString(),at.X,at.Y).Accepted && Json(w.ReadSandpit().moulds)==complete,"occupied move preserves all work");
  Need(Play(1,"reset-request","reset").Accepted,"pending consent reopening fixture");var reopenedVote=GameWorld.Restore(JsonSerializer.Deserialize<SoloSnapshot>(Json(w.Snapshot()),options));Need(reopenedVote.ReadSandpit().reset==null && Json(reopenedVote.ReadSandpit().moulds)==complete,"save reopening cancels consent retains castle");
 }
 static void Throws(Action action,string reason){try{action();throw new Exception("accepted "+reason);}catch(InvalidOperationException){checks++;}}
 static void FeedbackChecks(SandpitState state)
 {
  var scoop=new SandScoopFeedback();var water=new SandWaterFeedback();var tip=new SandTipFeedback();
  scoop.Observe(state,true,.1f);water.Observe(state,true,.1f);tip.Observe(state,true,.1f);
  Need(scoop.Events.All(n=>n==0) && water.Events.All(n=>n==0) && tip.Events.All(n=>n==0),"late join no old feedback");
  state=state.Copy();state.moulds=state.moulds.Concat(new[]{new SandMould{id="new-piece",capacity=3}}).ToArray();
  scoop.Observe(state,true,.1f);water.Observe(state,true,.1f);tip.Observe(state,true,.1f);
  state.moulds[4].scoops++;state.moulds[4].wet=true;
  scoop.Observe(state,true,.1f);water.Observe(state,true,.1f);Need(scoop.Events[4]==1 && water.Events[4]==1,"feedback beyond fixed four");
  state.moulds[4].scoops=3;tip.Observe(state,true,.1f);tip.Cue(4,state.round,"dry");Need(tip.Outcome(4)=="dry" && tip.Events[4]==0,"dry cue no reveal");state.moulds[4].built=true;tip.Observe(state,true,.1f);Need(tip.Outcome(4)=="reveal" && tip.Events[4]==1,"authoritative reveal");
  tip.Observe(state,false,.1f);tip.Observe(state,true,.1f);Need(tip.Remaining(4)==0,"reconnect no reveal replay");
  // Owner removal can shift a wetter/fuller neighbour into this array slot.
  // A different stable ID is a baseline, never a scoop or pour on that object.
  var shifted=new SandpitState{round=1,moulds=new[]{new SandMould{id="a",capacity=3},new SandMould{id="b",capacity=3,scoops=3,wet=true}}};
  scoop=new SandScoopFeedback();water=new SandWaterFeedback();scoop.Observe(shifted,true,.1f);water.Observe(shifted,true,.1f);
  shifted.moulds=shifted.moulds.Skip(1).ToArray();scoop.Observe(shifted,true,.1f);water.Observe(shifted,true,.1f);
  Need(scoop.Events.All(n=>n==0) && scoop.Remaining(0)==0,"removed neighbour cannot replay scoop");
  Need(water.Events.All(n=>n==0) && water.Remaining(0)==0,"removed neighbour cannot replay water");
 }
}
