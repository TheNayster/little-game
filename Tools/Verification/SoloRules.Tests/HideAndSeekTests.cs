using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static GameWorld HideWorld(){var w=GameWorld.WithHideAndSeek(DiscoveryWorld());foreach(var p in w.Snapshot().players)Good(w,SoloAction.Move,x:HideAndSeek.StartX,y:50,actor:p.id);return w;}
    static SoloResult Hide(GameWorld w,string op,string actor="first",int slot=-1)=>w.Apply(Command(w,SoloAction.HideAndSeek,target:(op=="join" || op=="start"?w.ReadHideAndSeek().round:slot).ToString(),value:op,actor:actor));
    static void HideAt(GameWorld w,string actor,int slot){Good(w,SoloAction.Move,x:HideAndSeek.SlotX[slot],y:50,actor:actor);Check(Hide(w,"hide",actor,slot).Accepted);}
    static void StartHide(GameWorld w,params string[] actors)
    {
        if(actors.Length==0)actors=new[]{"first"};
        Check(Hide(w,"invite",actors[0]).Accepted);
        foreach(var actor in actors)Check(Hide(w,"join",actor).Accepted);
    }
    static void HideAndSeekTests()
    {
        Test("automatic hide approach uses the extended Home bounds in solo and shared play",()=>{
            var w=HideWorld();Check(Walking.AdvanceLocal(w,"first",WalkMode.Destination,-4900,50,.1f));
            var session=new FamilySession(w);Check(session.Attach(1,"first",out _));var movement=new MovementAuthority(w,session);var p=w.ReadPlayer("first");
            Check(movement.Accept(1,new WalkInput{actor="first",zone=p.zone,visit=p.visit,sequence=1,mode=WalkMode.Destination,x=-4900,y=50},0));
            Check(movement.Tick(.05,.05f));Check(!movement.Accept(1,new WalkInput{actor="first",zone=p.zone,visit=p.visit,sequence=2,mode=WalkMode.Destination,x=-9000,y=50},.1));
            Throws(()=>Walking.AdvanceLocal(w,"first",WalkMode.Destination,-9000,50,.1f));
        });
        Test("hiding migration adds shared invitation roles without changing rooms objects or player positions",()=>{
            var old=RampWorld();var before=old.Snapshot();var w=GameWorld.WithHideAndSeek(old);var next=w.Snapshot();Check(next.schema==32 && next.hideAndSeek.hiders.Length==4);next.schema=before.schema;next.revision--;next.hideAndSeek=null;Check(Encode(next)==Encode(before));
        });
        Test("four hiders share a round across ten slots and Bandit uses no player slot",()=>{
            var w=HideWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();StartHide(w,actors);for(var i=0;i<4;i++)HideAt(w,actors[i],i);
            Check(w.Snapshot().players.Length==4 && w.ReadHideAndSeek().hiders.All(p=>p.mode==HiderMode.Hidden));Advance(w,9);Check(w.ReadHideAndSeek().phase==HidePhase.Counting);Advance(w,100);Check(w.ReadHideAndSeek().hiders.All(p=>p.mode==HiderMode.Found));GameWorld.Validate(w.Snapshot());
        });
        Test("same-slot race has one winner duplicate requests never rejoin restart or move possessions",()=>{
            var w=HideWorld();StartHide(w,"first","second");HideAt(w,"first",2);Good(w,SoloAction.Move,x:HideAndSeek.SlotX[2],y:50,actor:"second");Check(!Hide(w,"hide","second",2).Accepted);
            var c=Command(w,SoloAction.HideAndSeek,value:"out");Check(w.Apply(c).Accepted);Advance(w,1);var before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && before==Encode(w.Snapshot()));Check(Hide(w,"join").Accepted && w.ReadHideAndSeek().count<HideAndSeek.CountSeconds);
        });
        Test("start immediately opens one fifteen-second window and hiding joins without accepting",()=>{
            var w=HideWorld();Good(w,SoloAction.Travel,value:"park",actor:"second");var before=w.ReadPlayer("second");
            Check(Hide(w,"invite").Accepted);Check(w.ReadHideAndSeek().phase==HidePhase.Counting && w.ReadHideAndSeek().count==15);
            Check(w.ReadHideAndSeek().hiders.All(h=>h.mode==HiderMode.Away && h.preparation==0));
            Advance(w,4);HideAt(w,"third",4);Advance(w,3);HideAt(w,"fourth",9);
            Check(Math.Abs(w.ReadHideAndSeek().count-8)<.01 && w.ReadPlayer("second").zone==before.zone && w.ReadPlayer("second").visit==before.visit);
            Advance(w,9);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode==HiderMode.Away && HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Away);
            Advance(w,100);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"third").mode==HiderMode.Found && HideAndSeek.Player(w.ReadHideAndSeek(),"fourth").mode==HiderMode.Found);
        });
        Test("optional Go hide travels only its user and uses the remaining shared time",()=>{
            var w=HideWorld();Good(w,SoloAction.Travel,value:"park",actor:"second");Check(Hide(w,"invite").Accepted);Advance(w,6);
            Check(Hide(w,"join","second").Accepted && w.ReadPlayer("second").zone=="garden");Check(Math.Abs(w.ReadHideAndSeek().count-9)<.01);
            HideAt(w,"first",9);Advance(w,10);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Away);
            Check(!Hide(w,"join","third").Accepted && !Hide(w,"hide","third",3).Accepted);GameWorld.Validate(w.Snapshot());
        });
        Test("starter and nonparticipants leaving never cancel the open hiding window",()=>{
            var w=HideWorld();Check(Hide(w,"invite").Accepted);Check(!w.ReleaseHideAndSeek("first"));Advance(w,5);
            Check(w.ReadHideAndSeek().phase==HidePhase.Counting && Math.Abs(w.ReadHideAndSeek().count-10)<.01);
            Check(Hide(w,"leave").Accepted);HideAt(w,"second",9);Advance(w,11);Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"second"));
            Check(Hide(w,"out","second").Accepted && w.ReadHideAndSeek().phase==HidePhase.Idle);
        });
        Test("repeat starts and stale Go hide never restart or join another countdown",()=>{
            var w=HideWorld();Check(Hide(w,"invite").Accepted);var round=w.ReadHideAndSeek().round;Advance(w,5);var before=Encode(w.Snapshot());
            Check(!Hide(w,"invite","second").Accepted && !Hide(w,"start","third").Accepted && before==Encode(w.Snapshot()));
            Advance(w,11);Check(w.ReadHideAndSeek().phase==HidePhase.Idle && Hide(w,"invite","second").Accepted);
            Check(!w.Apply(Command(w,SoloAction.HideAndSeek,value:"join",target:round.ToString(),actor:"first")).Accepted);GameWorld.Validate(w.Snapshot());
        });
        Test("coming out or walking after zero leaves only that hider and forbids rehide",()=>{
            var w=HideWorld();StartHide(w,"first","second","third");HideAt(w,"first",0);HideAt(w,"second",9);HideAt(w,"third",8);Advance(w,16);
            var round=w.ReadHideAndSeek().round;var parent=HideAndSeek.Parent(w.ReadHideAndSeek());
            Check(Hide(w,"out").Accepted && HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode==HiderMode.Away);
            Good(w,SoloAction.Move,x:HideAndSeek.SlotX[0],y:50);Check(!Hide(w,"hide",slot:0).Accepted);
            Check(Walking.AdvanceLocal(w,"second",WalkMode.Direction,1,0,.1f));Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Away);
            Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"third") && w.ReadHideAndSeek().round==round && HideAndSeek.Parent(w.ReadHideAndSeek())==parent && w.ReadHideAndSeek().count==0);
            Advance(w,100);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"third").mode==HiderMode.Found);GameWorld.Validate(w.Snapshot());
        });
        Test("late or found players cannot start personal countdowns during another player's search",()=>{
            var w=HideWorld();StartHide(w,"first","second");HideAt(w,"first",1);HideAt(w,"second",9);Advance(w,16);
            Check(!Hide(w,"join","third").Accepted && !Hide(w,"invite","third").Accepted);
            while(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode!=HiderMode.Found)w.AdvanceIdle(.1,out _);
            var before=Encode(w.Snapshot());Check(!Hide(w,"invite").Accepted && !Hide(w,"join").Accepted && before==Encode(w.Snapshot()));
            Advance(w,100);Check(Hide(w,"invite").Accepted && HideAndSeek.Parent(w.ReadHideAndSeek())=="Chilli");
        });
        Test("identical search observations produce identical targets with occupants in different covers",()=>{
            var a=HideWorld();var b=HideWorld();StartHide(a);StartHide(b);HideAt(a,"first",4);HideAt(b,"first",5);
            for(var i=0;i<300;i++){a.AdvanceIdle(.1,out _);b.AdvanceIdle(.1,out _);var x=a.ReadHideAndSeek();var y=b.ReadHideAndSeek();if(x.hiders[0].mode==HiderMode.Found || y.hiders[0].mode==HiderMode.Found)break;Check(x.target==y.target && x.phase==y.phase && x.x==y.x);}
        });
        Test("come out and walking release cover without dropping or duplicating the held item",()=>{
            var w=HideWorld();Good(w,SoloAction.Grab,"bucket-1");StartHide(w);HideAt(w,"first",1);Good(w,SoloAction.ChangeAvatar,value:"orange-pup");Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));Check(Hide(w,"out").Accepted);Check(w.ReadToys().Single(t=>t.id=="bucket-1").holder=="first" && w.ReadPlayer("first").y==50);HideAt(w,"first",3);Check(Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,.1f));Check(!HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));GameWorld.Validate(w.Snapshot());
        });
        Test("starter disconnect and room travel release only that player's participation",()=>{
            var w=HideWorld();var session=new FamilySession(w);Check(session.Attach(1,"first",out _) && session.Attach(2,"second",out _));StartHide(w,"first","second");HideAt(w,"first",0);HideAt(w,"second",5);Check(session.Detach(1));Check(w.ReadHideAndSeek().phase==HidePhase.Counting && HideAndSeek.Hidden(w.ReadHideAndSeek(),"second"));Advance(w,100);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Found);StartHide(w,"second");Good(w,SoloAction.Travel,value:"park",actor:"second");Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Away);
        });
        Test("headless count advances independently of rendering and cold restore suspends stale roles",()=>{
            var w=HideWorld();StartHide(w);HideAt(w,"first",3);Advance(w,(int)HideAndSeek.CountSeconds-3);var snapshot=Decode(Encode(w.Snapshot()));Check(Math.Abs(snapshot.hideAndSeek.count-3)<.01);var restored=GameWorld.Restore(snapshot);Check(restored.ReadHideAndSeek().phase==HidePhase.Idle && restored.ReadHideAndSeek().hiders.All(h=>h.mode==HiderMode.Away) && restored.ReadPlayer("first").y==50);Check(Encode(w.ReadToys())==Encode(restored.ReadToys()));GameWorld.Validate(restored.Snapshot());
        });
        Test("no hidden players at zero ends the round and preserves all creations",()=>{
            var w=HideWorld();var before=Encode(w.ReadToys());StartHide(w);Advance(w,14);Check(w.ReadHideAndSeek().phase==HidePhase.Counting);Advance(w,2);Check(w.ReadHideAndSeek().phase==HidePhase.Idle && w.ReadHideAndSeek().hiders.All(h=>h.mode==HiderMode.Away) && before==Encode(w.ReadToys()));
        });
        Test("all ten stationary hiders are found within the first-level search budget with either parent",()=>{
            double longest=0;
            foreach(var start in new[]{-7100f,HideAndSeek.StartX,4300f})for(var reverse=0;reverse<2;reverse++)for(var slot=0;slot<HideAndSeek.SlotX.Length;slot++){
                var w=HideWorld();Good(w,SoloAction.Move,x:start,y:50);if(reverse==1){StartHide(w);Check(Hide(w,"leave").Accepted);Advance(w,16);}StartHide(w);HideAt(w,"first",slot);Advance(w,10);double seconds=0;
                while(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode!=HiderMode.Found && seconds<150){w.AdvanceIdle(.05,out _);seconds+=.05;}Check(seconds<100);longest=Math.Max(longest,seconds);
            }
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"hide-pacing.txt"),"Longest single-hider search across 60 slot/parent/start-position cases: "+longest.ToString("F2")+" seconds.");
        });
        Test("schema 28 active twenty-second saves migrate without losing parent turn or possessions",()=>{
            var w=HideWorld();Good(w,SoloAction.Grab,"bucket-1");StartHide(w);HideAt(w,"first",3);
            var old=w.Snapshot();old.schema=28;old.hideAndSeek.count=20;old.hideAndSeek.hiders[0].preparation=20;
            GameWorld.Validate(old);var upgraded=GameWorld.WithHideAndSeek(GameWorld.Restore(Decode(Encode(old))));var s=upgraded.Snapshot();
            Check(s.schema==32 && s.hideAndSeek.round==old.hideAndSeek.round && s.hideAndSeek.hiders.All(h=>h.mode==HiderMode.Away));
            Check(s.toys.Select(t=>t.id).SequenceEqual(old.toys.Select(t=>t.id)) && Encode(s.bedrooms)==Encode(old.bedrooms) && s.homeCreations==old.homeCreations);
            Check(HideAndSeek.Parent(s.hideAndSeek,true)=="Chilli");GameWorld.Validate(s);
        });
        Test("parents alternate shared rounds without switching when a participant leaves",()=>{
            var w=HideWorld();StartHide(w,"first","second");Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");HideAt(w,"second",9);
            Check(Hide(w,"leave").Accepted && HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");Check(Hide(w,"leave","second").Accepted);Advance(w,16);
            StartHide(w);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Chilli");Check(Hide(w,"leave").Accepted);Advance(w,16);StartHide(w);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");
        });
        Test("search makes stationary left-right looks including stops during a long walk",()=>{
            var w=HideWorld();StartHide(w);HideAt(w,"first",9);Advance(w,10);
            var left=false;var right=false;var travelPause=false;
            for(var i=0;i<1600;i++){
                var before=w.ReadHideAndSeek();w.AdvanceIdle(.05,out _);var after=w.ReadHideAndSeek();
                if(before.phase==HidePhase.Looking && after.phase==HidePhase.Looking){Check(before.x==after.x);left|=HideAndSeek.Facing(after)<0;right|=HideAndSeek.Facing(after)>0;travelPause|=after.target>=0;}
                if(after.hiders[0].mode==HiderMode.Found)break;
            }
            Check(left && right && travelPause);
        });
        Test("ten authored covers span first level and remain independent of hidden occupancy",()=>{
            Check(HideAndSeek.SlotX.Length==10 && HideAndSeek.SlotX.Distinct().Count()==10 && HideAndSeek.SlotX.Min()<-6900 && HideAndSeek.SlotX.Max()>4200);
            var w=HideWorld();StartHide(w);HideAt(w,"first",6);Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));Check(Hide(w,"out").Accepted);HideAt(w,"first",7);GameWorld.Validate(w.Snapshot());
            var s=w.ReadHideAndSeek();var first=HideAndSeek.NextSlot(s);s.hiders[0].slot=9;Check(first==HideAndSeek.NextSlot(s));s.visited|=1<<first;Check(HideAndSeek.NextSlot(s)!=first);
            Check(!HideAndSeek.Zone(new SoloPlayer{zone="upstairs",x=100,y=50}) && !HideAndSeek.Zone(new SoloPlayer{zone="garden",stairs=1}));
        });
        Test("deployed schema 30 personal timer migrates safely without losing held objects or rooms",()=>{
            var w=HideWorld();Good(w,SoloAction.Grab,"bucket-1");StartHide(w);HideAt(w,"first",9);var old=w.Snapshot();old.schema=30;old.hideAndSeek.hiders[0].preparation=12;
            GameWorld.Validate(old);var upgraded=GameWorld.WithHideAndSeek(GameWorld.Restore(Decode(Encode(old))));var s=upgraded.Snapshot();
            Check(s.schema==32 && s.hideAndSeek.phase==HidePhase.Idle && s.hideAndSeek.hiders.All(h=>h.preparation==0 && h.mode==HiderMode.Away));
            Check(s.toys.Count(t=>t.id=="bucket-1")==1 && s.toys.Single(t=>t.id=="bucket-1").holder=="" && Encode(s.bedrooms)==Encode(old.bedrooms) && s.homeCreations==old.homeCreations);
        });
        Test("invalid hide timers and duplicate leases are rejected and snapshot reads are detached",()=>{
            var w=HideWorld();var s=w.Snapshot();s.hideAndSeek.count=double.NaN;Throws(()=>GameWorld.Validate(s));s=w.Snapshot();s.hideAndSeek.hiders[0].slot=0;Throws(()=>GameWorld.Validate(s));Check(w.ReadHideAndSeek().hiders[0].slot==-1);
        });
    }
}
