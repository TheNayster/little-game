using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld TidyWorld()=>SoloWorld.WithHomeTidying(LiquidWorld());
    static void HomeTidyingTests()
    {
        Test("home tidying migration preserves every old record and starts with full grace",()=>{
            var old=LiquidWorld();Check(Liquid(old,"red").Accepted && Discover(old,"fill:0:5").Accepted);var before=old.Snapshot();var w=SoloWorld.WithHomeTidying(old);var s=w.Snapshot();Check(s.schema==22 && s.homeIdleTimers.Length==0);s.schema=21;s.revision--;Check(Encode(s)==Encode(before));Check(ReferenceEquals(w,SoloWorld.WithHomeTidying(w)));Advance(w,299);Check(LiquidColorLab.Volume(LiquidTray(w).current)==1 && w.ReadTidyCues().Length==0);
        });
        Test("five minutes gives a cue then resets only the unused science tray",()=>{
            var w=TidyWorld();Check(Liquid(w,"red").Accepted);Advance(w,299);Check(LiquidColorLab.Volume(LiquidTray(w).current)==1);w.AdvanceIdle(1,out var visible);Check(visible && w.ReadTidyCues().Contains(HomeTidying.Lab("first","liquid")));Advance(w,4);Check(LiquidColorLab.Volume(LiquidTray(w).current)==1);w.AdvanceIdle(1,out visible);Check(visible && LiquidColorLab.Volume(LiquidTray(w).current)==0);Check(Liquid(w,"undo").Accepted && LiquidColorLab.Volume(LiquidTray(w).current)==1);SoloWorld.Validate(w.Snapshot());
        });
        Test("four independent players refresh only their own meaningful activity",()=>{
            var w=TidyWorld();var ids=w.Snapshot().players.Select(p=>p.id).ToArray();foreach(var id in ids)Check(Liquid(w,"red",id).Accepted);Advance(w,300);Check(Liquid(w,"blue","second").Accepted);Check(Discover(w,"cargo-add","third").Accepted);Good(w,SoloAction.ChangeAvatar,value:"orange-pup",actor:"fourth");Advance(w,5);Check(LiquidColorLab.Volume(LiquidTray(w,"first").current)==0 && LiquidColorLab.Volume(LiquidTray(w,"second").current)==2 && LiquidColorLab.Volume(LiquidTray(w,"third").current)==0 && LiquidColorLab.Volume(LiquidTray(w,"fourth").current)==0);Check(Workspace(w,"third").cargo==1);
        });
        Test("rejected and replayed commands cannot hold abandoned activities forever",()=>{
            var w=TidyWorld();var c=Command(w,SoloAction.Discovery,"first","colors@0","liquid:red");Check(w.Apply(c).Accepted);Advance(w,300);Check(w.Apply(c).Duplicate);Check(!Liquid(w,"green").Accepted);Advance(w,5);Check(LiquidColorLab.Volume(LiquidTray(w).current)==0);
        });
        Test("durable clocks pause with empty family and survive recovery",()=>{
            var w=TidyWorld();Check(Liquid(w,"red").Accepted);var session=new FamilySession(w);for(var i=0;i<400;i++)Check(!session.AdvanceIdle(1,out _));Check(LiquidColorLab.Volume(LiquidTray(w).current)==1);Check(session.Attach(1,"first",out _));for(var i=0;i<299;i++)session.AdvanceIdle(1,out _);Check(session.View().homeIdleTimers.Length==0);w=SoloWorld.Restore(Decode(Encode(session.Checkpoint())));Advance(w,5);Check(LiquidColorLab.Volume(LiquidTray(w).current)==1);Advance(w,1);Check(LiquidColorLab.Volume(LiquidTray(w).current)==0);
        });
        Test("science cleanup leaves paintings undo bedrooms personal toys and food exact",()=>{
            var w=TidyWorld();Check(Discover(w,"fill:0:5").Accepted && Liquid(w,"red").Accepted);var before=w.Snapshot();Advance(w,306);var after=w.Snapshot();Check(Encode(before.bedrooms)==Encode(after.bedrooms) && Encode(before.secrets)==Encode(after.secrets));Check(Encode(before.discovery.Select(d=>d.pages).ToArray())==Encode(after.discovery.Select(d=>d.pages).ToArray()));Check(Encode(before.toys)==Encode(after.toys));
        });
        Test("loose kitchen items return with exact food identities amounts and batches",()=>{
            var w=TidyWorld();var s=w.Snapshot();var t=s.toys.Single(t=>t.id=="ingredient-cheese");t.container="";t.x=-3700;t.y=75;t.kitchen.amount=7;t.kitchen.batch=3;w=SoloWorld.Restore(s);Advance(w,305);var returned=Toy(w,t.id);Check(returned.container==Kitchen.Stock().Single(x=>x.id==t.id).container && returned.kitchen.amount==7 && returned.kitchen.batch==3);SoloWorld.Validate(w.Snapshot());
            s=w.Snapshot();t=s.toys.Single(t=>t.id=="cookware-0");t.container="";t.x=-3700;t.y=75;t.kitchen.batch=1;t.kitchen.cook="first";t.kitchen.dish=new FoodDish{id="cookware-0/1",recipe="PIZ-01",portions=15,ingredients=new[]{new FoodAddition{unit="ingredient-dough/0/0",ingredient="dough"}}};var food=System.Text.Json.JsonSerializer.Serialize(t.kitchen,Json);w=SoloWorld.Restore(s);Advance(w,305);Check(Toy(w,t.id).container==Kitchen.Support("counter",0) && System.Text.Json.JsonSerializer.Serialize(Toy(w,t.id).kitchen,Json)==food);SoloWorld.Validate(w.Snapshot());
        });
        Test("a held shared book cancels the cue and gets a fresh grace on release",()=>{
            var w=TidyWorld();var s=w.Snapshot();var t=s.toys.Single(t=>t.id==HomeBooks.Copy(0));t.container="";t.x=-6500;t.y=200;w=SoloWorld.Restore(s);Advance(w,300);Check(w.ReadTidyCues().Contains(HomeTidying.Item(t.id)));Good(w,SoloAction.Grab,t.id);Advance(w,310);Check(Toy(w,t.id).holder=="first");Good(w,SoloAction.CancelGrab,t.id);Advance(w,304);Check(Toy(w,t.id).container=="");Advance(w,1);Check(Toy(w,t.id).container==HomeBooks.Support(0));SoloWorld.Validate(w.Snapshot());
        });
        Test("occupied return slots defer rather than move a sibling or duplicate items",()=>{
            var w=TidyWorld();var s=w.Snapshot();var a=s.toys.Single(t=>t.id=="cookware-0");var b=s.toys.Single(t=>t.id=="cookware-1");a.container="";a.x=-3700;a.y=75;b.container=Kitchen.Support("counter",0);b.x=Kitchen.X("counter",0);b.y=Kitchen.Y("counter",0);w=SoloWorld.Restore(s);Advance(w,305);Check(Toy(w,a.id).container=="");Advance(w,305);Check(Toy(w,a.id).container==Kitchen.Support("counter",0));Check(w.ReadToys().Select(t=>t.id).Distinct().Count()==s.toys.Length);SoloWorld.Validate(w.Snapshot());
        });
        Test("old science trays reset independently and late generation tokens are rejected",()=>{
            var w=TidyWorld();Check(Discover(w,"cargo-add").Accepted && Discover(w,"red").Accepted && Discover(w,"wood").Accepted);Check(Ice(w,"chip").Accepted && Bubbles(w,"water").Accepted);Check(Mix(w,"add:1",amount:1).Accepted);var token=IceRescue.Token(Workspace(w).ice[0]);Advance(w,305);Check(Workspace(w).cargo==0 && Workspace(w).lights==0 && Workspace(w).magnetX==400 && Mixing.Total(Workspace(w).mixtures[0])==0 && Workspace(w).ice[0].current.cells.All(v=>v==1) && !Workspace(w).bubbles[0].current.water);Check(!w.Apply(Command(w,SoloAction.Discovery,"first",token,"ice:chip",IceRescue.X(0),IceRescue.Y(0))).Accepted);SoloWorld.Validate(w.Snapshot());
        });
        Test("reopening one experiment cancels its cue without renewing siblings",()=>{
            var w=TidyWorld();Check(Liquid(w,"red").Accepted && Bubbles(w,"water").Accepted);Advance(w,300);Check(Discover(w,"visit:liquid").Accepted);Advance(w,5);Check(LiquidColorLab.Volume(LiquidTray(w).current)==1 && !Workspace(w).bubbles[0].current.water);Check(!Discover(w,"visit:unknown").Accepted);
        });
        Test("kitchen doors and balloon reset while live cooking and flight stay protected",()=>{
            var w=TidyWorld();var s=w.Snapshot();s.kitchen.fridgeOpen=s.kitchen.ovenOpen=s.kitchen.waterOn=true;s.kitchen.cupboards[2]=true;s.keepy.x=s.keepy.centerX=3100;w=SoloWorld.Restore(s);Advance(w,305);Check(!w.ReadKitchen().fridgeOpen && !w.ReadKitchen().ovenOpen && !w.ReadKitchen().waterOn && !w.ReadKitchen().cupboards.Any(v=>v) && w.ReadKeepy().x==KeepyRules.SpawnX);
            Good(w,SoloAction.Kitchen,target:"fridge",value:"door");Advance(w,300);Good(w,SoloAction.Kitchen,target:"water",value:"door");Advance(w,5);Check(w.ReadKitchen().fridgeOpen && w.ReadKitchen().waterOn);
        });
        Test("old garden mini games adopt five minute policy only after upgrade",()=>{
            var w=TidyWorld();Good(w,SoloAction.Grab,"bucket-1");Good(w,SoloAction.Drop,"bucket-1","tap-1",x:150,y:340);Advance(w,185);Check(Toy(w,"bucket-1").water==3);Advance(w,115);Check(Toy(w,"bucket-1").resetPending);Advance(w,5);Check(Toy(w,"bucket-1").water==0);SoloWorld.Validate(w.Snapshot());
        });
        Test("invalid clocks are rejected and timer copies do not alias recovery state",()=>{
            var w=TidyWorld();Check(Liquid(w,"red").Accepted);Advance(w,40);var s=w.Snapshot();var copy=SoloWorld.CopySnapshot(s);copy.homeIdleTimers[0].seconds=90;Check(w.Snapshot().homeIdleTimers[0].seconds==40);foreach(var value in new[]{double.NaN,double.PositiveInfinity,-1,306}){var bad=w.Snapshot();bad.homeIdleTimers[0].seconds=value;Throws(()=>SoloWorld.Validate(bad));}s.homeIdleTimers[0].key="missing";Throws(()=>SoloWorld.Validate(s));
        });
    }
}
