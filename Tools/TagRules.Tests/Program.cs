using System;
using System.Linq;
using LittleWeeps.Core;
class Program
{
    static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
    static void Main()
    {
        var w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("one","two","three","four"));var f=new FamilySession(w);
        var ids=new[]{"one","two","three","four"};for(ulong i=1;i<=4;i++)Need(f.Attach(i,ids[i-1],out _),"attach");
        SoloResult Act(int i,SoloAction a,string value="",float x=1600){var p=w.ReadPlayer(ids[i-1]);return f.Submit((ulong)i,new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=a,value=value,x=x,y=70});}
        void Tick(double n){while(n>.001){var dt=Math.Min(.05,n);f.AdvanceIdle(dt,out _);n-=dt;}}
        for(int i=1;i<=3;i++){Need(Act(i,SoloAction.Travel,"park").Accepted,"park");Need(Act(i,SoloAction.Move,x:1400+i*350).Accepted,"position");}
        Need(Act(4,SoloAction.Travel,"creek").Accepted,"independent area");
        Need(Act(1,SoloAction.Park,"tag-join").Accepted,"one start");var start=w.ReadTag().starts;
        Need(w.ReadTag().members.Length==3 && !ParkTag.Member(w.ReadTag(),"four"),"one start includes connected park group only");
        var rev=w.Revision;Tick(.1);Need(w.Revision==rev,"no countdown revision churn");
        Need(Act(4,SoloAction.Travel,"park").Accepted,"late arrival");Tick(.05);
        Need(w.ReadTag().members.Length==4 && w.ReadTag().starts==start,"arrival automatically joins same round");
        Need(Act(2,SoloAction.Park,"tag-leave").Accepted,"all done");Tick(.5);Need(!ParkTag.Member(w.ReadTag(),"two"),"All done was auto-rejoined");
        Need(Act(2,SoloAction.Travel,"creek").Accepted && Act(2,SoloAction.Travel,"park").Accepted,"return");Tick(.05);Need(w.ReadTag().members.Length==4,"return automatically joins");
        Tick(5.5);Need(Act(2,SoloAction.Move,x:w.ReadPlayer("one").x+50).Accepted,"human contact");Tick(.05);Need(w.ReadTag().it=="two","human role swap");Tick(.5);Need(w.ReadTag().it=="two","instant tag back");
        Need(f.Detach(2),"tagger departure");Need(w.ReadTag().members.Length==3 && w.ReadTag().it!="two","remaining shared game");
        Need(Act(3,SoloAction.Travel,"creek").Accepted,"independent travel");Need(Act(4,SoloAction.Park,"tag-leave").Accepted,"independent stop");Tick(.2);Need(w.ReadTag().members.Length==1,"sibling game changed");
        // A solo checkpoint may contain old family profiles, but these are not live playmates.
        var solo=SoloWorld.WithDinosaurWorld(SoloWorld.Create("one","ghost"));var p=solo.ReadPlayer("one");
        SoloResult S(SoloAction a,string value)=>solo.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor="one",zone=solo.ReadPlayer("one").zone,visit=solo.ReadPlayer("one").visit,expectedRevision=solo.Revision,action=a,value=value,x=1600,y=70});
        Need(S(SoloAction.Travel,"park").Accepted && S(SoloAction.Park,"tag-join").Accepted,"solo start");solo.AdvanceIdle(.1,out _);Need(solo.ReadTag().members.Length==1,"saved ghost became participant");
        Console.WriteLine("PASS one start includes park group; late arrivals; independent All done/travel/disconnect; human tagging/grace; no saved-profile ghosts or countdown churn");
    }
}
