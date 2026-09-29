using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld HideWorld(){var w=SoloWorld.WithHideAndSeek(DiscoveryWorld());foreach(var p in w.Snapshot().players)Good(w,SoloAction.Move,x:HideAndSeek.StartX,y:50,actor:p.id);return w;}
    static SoloResult Hide(SoloWorld w,string op,string actor="first",int slot=-1)=>w.Apply(Command(w,SoloAction.HideAndSeek,target:slot.ToString(),value:op,actor:actor));
    static void HideAt(SoloWorld w,string actor,int slot){Good(w,SoloAction.Move,x:HideAndSeek.SlotX[slot],y:50,actor:actor);Check(Hide(w,"hide",actor,slot).Accepted);}
    static void HideAndSeekTests()
    {
        Test("automatic hide approach uses the extended Home bounds in solo and shared play",()=>{
            var w=HideWorld();Check(Walking.AdvanceLocal(w,"first",WalkMode.Destination,-4900,50,.1f));
            var session=new FamilySession(w);Check(session.Attach(1,"first",out _));var movement=new MovementAuthority(w,session);var p=w.ReadPlayer("first");
            Check(movement.Accept(1,new WalkInput{actor="first",zone=p.zone,visit=p.visit,sequence=1,mode=WalkMode.Destination,x=-4900,y=50},0));
            Check(movement.Tick(.05,.05f));Check(!movement.Accept(1,new WalkInput{actor="first",zone=p.zone,visit=p.visit,sequence=2,mode=WalkMode.Destination,x=-9000,y=50},.1));
            Throws(()=>Walking.AdvanceLocal(w,"first",WalkMode.Destination,-9000,50,.1f));
        });
        Test("hiding migration adds inactive roles without changing rooms objects or player positions",()=>{
            var old=RampWorld();var before=old.Snapshot();var w=SoloWorld.WithHideAndSeek(old);var next=w.Snapshot();Check(next.schema==30 && next.hideAndSeek.hiders.Length==4);next.schema=before.schema;next.revision--;next.hideAndSeek=null;Check(Encode(next)==Encode(before));
        });
        Test("four independent hiders fit ten slots and Bandit uses no player slot",()=>{
            var w=HideWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();for(var i=0;i<4;i++){Check(Hide(w,"join",actors[i]).Accepted);HideAt(w,actors[i],i);}
            Check(w.Snapshot().players.Length==4 && w.ReadHideAndSeek().hiders.All(p=>p.mode==HiderMode.Hidden));Advance(w,9);Check(w.ReadHideAndSeek().phase==HidePhase.Counting);Advance(w,100);Check(w.ReadHideAndSeek().hiders.All(p=>p.mode==HiderMode.Found));SoloWorld.Validate(w.Snapshot());
        });
        Test("same-slot race has one winner duplicate requests never rejoin restart or move possessions",()=>{
            var w=HideWorld();Check(Hide(w,"join").Accepted && Hide(w,"join","second").Accepted);HideAt(w,"first",2);Good(w,SoloAction.Move,x:HideAndSeek.SlotX[2],y:50,actor:"second");Check(!Hide(w,"hide","second",2).Accepted);
            var c=Command(w,SoloAction.HideAndSeek,value:"out");Check(w.Apply(c).Accepted);Advance(w,1);var before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && before==Encode(w.Snapshot()));Check(Hide(w,"join").Accepted && w.ReadHideAndSeek().count<HideAndSeek.CountSeconds);
        });
        Test("late joining gets personal preparation without rewinding the existing parent search",()=>{
            var w=HideWorld();Check(Hide(w,"join").Accepted);HideAt(w,"first",9);Advance(w,(int)HideAndSeek.CountSeconds-1);Check(Hide(w,"join","second").Accepted);HideAt(w,"second",0);Advance(w,5);var s=w.ReadHideAndSeek();Check(s.hiders.Single(p=>p.actor=="second").preparation>4 && s.hiders.Single(p=>p.actor=="second").mode==HiderMode.Hidden && s.phase!=HidePhase.Counting);Advance(w,100);Check(w.ReadHideAndSeek().hiders.Where(p=>p.actor=="first" || p.actor=="second").All(p=>p.mode==HiderMode.Found));
        });
        Test("identical search observations produce identical targets with occupants in different covers",()=>{
            var a=HideWorld();var b=HideWorld();Check(Hide(a,"join").Accepted && Hide(b,"join").Accepted);HideAt(a,"first",4);HideAt(b,"first",5);
            for(var i=0;i<300;i++){a.AdvanceIdle(.1,out _);b.AdvanceIdle(.1,out _);var x=a.ReadHideAndSeek();var y=b.ReadHideAndSeek();if(x.hiders[0].mode==HiderMode.Found || y.hiders[0].mode==HiderMode.Found)break;Check(x.target==y.target && x.phase==y.phase && x.x==y.x);}
        });
        Test("come out and walking release cover without dropping or duplicating the held item",()=>{
            var w=HideWorld();Good(w,SoloAction.Grab,"bucket-1");Check(Hide(w,"join").Accepted);HideAt(w,"first",1);Good(w,SoloAction.ChangeAvatar,value:"orange-pup");Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));Check(Hide(w,"out").Accepted);Check(w.ReadToys().Single(t=>t.id=="bucket-1").holder=="first" && w.ReadPlayer("first").y==50);HideAt(w,"first",3);Check(Walking.AdvanceLocal(w,"first",WalkMode.Direction,1,0,.1f));Check(!HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));SoloWorld.Validate(w.Snapshot());
        });
        Test("starter disconnect and room travel release only that player's participation",()=>{
            var w=HideWorld();var session=new FamilySession(w);Check(session.Attach(1,"first",out _) && session.Attach(2,"second",out _));Check(Hide(w,"join").Accepted && Hide(w,"join","second").Accepted);HideAt(w,"first",0);HideAt(w,"second",5);Check(session.Detach(1));Check(w.ReadHideAndSeek().phase==HidePhase.Counting && HideAndSeek.Hidden(w.ReadHideAndSeek(),"second"));Advance(w,100);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Found);Check(Hide(w,"join","second").Accepted);Good(w,SoloAction.Travel,value:"park",actor:"second");Check(HideAndSeek.Player(w.ReadHideAndSeek(),"second").mode==HiderMode.Away);
        });
        Test("headless count advances independently of rendering and cold restore suspends stale roles",()=>{
            var w=HideWorld();Check(Hide(w,"join").Accepted);HideAt(w,"first",3);Advance(w,(int)HideAndSeek.CountSeconds-3);var snapshot=Decode(Encode(w.Snapshot()));Check(Math.Abs(snapshot.hideAndSeek.count-3)<.01);var restored=SoloWorld.Restore(snapshot);Check(restored.ReadHideAndSeek().phase==HidePhase.Idle && restored.ReadHideAndSeek().hiders.All(h=>h.mode==HiderMode.Away) && restored.ReadPlayer("first").y==50);Check(Encode(w.ReadToys())==Encode(restored.ReadToys()));SoloWorld.Validate(restored.Snapshot());
        });
        Test("unused preparation expires after five minutes and preserves all creations",()=>{
            var w=HideWorld();var before=Encode(w.ReadToys());Check(Hide(w,"join").Accepted);Advance(w,299);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode==HiderMode.Preparing);Advance(w,2);Check(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode==HiderMode.Away && before==Encode(w.ReadToys()));
        });
        Test("all ten stationary hiders are found within the first-level search budget with either parent",()=>{
            double longest=0;
            foreach(var start in new[]{-7100f,HideAndSeek.StartX,4300f})for(var reverse=0;reverse<2;reverse++)for(var slot=0;slot<HideAndSeek.SlotX.Length;slot++){
                var w=HideWorld();Good(w,SoloAction.Move,x:start,y:50);if(reverse==1){Check(Hide(w,"join").Accepted);Check(Hide(w,"leave").Accepted);}Check(Hide(w,"join").Accepted);HideAt(w,"first",slot);Advance(w,10);double seconds=0;
                while(HideAndSeek.Player(w.ReadHideAndSeek(),"first").mode!=HiderMode.Found && seconds<150){w.AdvanceIdle(.05,out _);seconds+=.05;}Check(seconds<100);longest=Math.Max(longest,seconds);
            }
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"hide-pacing.txt"),"Longest single-hider search across 60 slot/parent/start-position cases: "+longest.ToString("F2")+" seconds.");
        });
        Test("schema 28 active twenty-second saves migrate without losing parent turn or possessions",()=>{
            var w=HideWorld();Good(w,SoloAction.Grab,"bucket-1");Check(Hide(w,"join").Accepted);HideAt(w,"first",3);
            var old=w.Snapshot();old.schema=28;old.hideAndSeek.count=20;old.hideAndSeek.hiders[0].preparation=20;
            SoloWorld.Validate(old);var upgraded=SoloWorld.WithHideAndSeek(SoloWorld.Restore(Decode(Encode(old))));var s=upgraded.Snapshot();
            Check(s.schema==30 && s.hideAndSeek.round==old.hideAndSeek.round && s.hideAndSeek.hiders.All(h=>h.mode==HiderMode.Away));
            Check(s.toys.Select(t=>t.id).SequenceEqual(old.toys.Select(t=>t.id)) && Encode(s.bedrooms)==Encode(old.bedrooms) && s.homeCreations==old.homeCreations);
            Check(HideAndSeek.Parent(s.hideAndSeek,true)=="Chilli");SoloWorld.Validate(s);
        });
        Test("parents alternate rounds while late joining and re-hiding never switch an active seeker",()=>{
            var w=HideWorld();Check(Hide(w,"join").Accepted);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");
            HideAt(w,"first",9);Advance(w,12);var round=w.ReadHideAndSeek().round;Check(Hide(w,"join","second").Accepted);
            Check(w.ReadHideAndSeek().round==round && HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");
            Check(Hide(w,"leave").Accepted);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");Check(Hide(w,"leave","second").Accepted);
            Check(HideAndSeek.Parent(w.ReadHideAndSeek(),true)=="Chilli");Check(Hide(w,"join").Accepted);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Chilli");
            Check(Hide(w,"leave").Accepted && Hide(w,"join").Accepted);Check(HideAndSeek.Parent(w.ReadHideAndSeek())=="Bandit");
        });
        Test("search makes stationary left-right looks including stops during a long walk",()=>{
            var w=HideWorld();Check(Hide(w,"join").Accepted);HideAt(w,"first",9);Advance(w,10);
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
            var w=HideWorld();Check(Hide(w,"join").Accepted);HideAt(w,"first",6);Check(HideAndSeek.Hidden(w.ReadHideAndSeek(),"first"));Check(Hide(w,"out").Accepted);HideAt(w,"first",7);SoloWorld.Validate(w.Snapshot());
            var s=w.ReadHideAndSeek();var first=HideAndSeek.NextSlot(s);s.hiders[0].slot=9;Check(first==HideAndSeek.NextSlot(s));s.visited|=1<<first;Check(HideAndSeek.NextSlot(s)!=first);
            Check(!HideAndSeek.Zone(new SoloPlayer{zone="upstairs",x=100,y=50}) && !HideAndSeek.Zone(new SoloPlayer{zone="garden",stairs=1}));
        });
        Test("invalid hide timers and duplicate leases are rejected and snapshot reads are detached",()=>{
            var w=HideWorld();var s=w.Snapshot();s.hideAndSeek.count=double.NaN;Throws(()=>SoloWorld.Validate(s));s=w.Snapshot();s.hideAndSeek.hiders[0].slot=0;Throws(()=>SoloWorld.Validate(s));Check(w.ReadHideAndSeek().hiders[0].slot==-1);
        });
    }
}
