using System;
using System.Linq;
using UnityEngine;
using LittleWeeps.Core;
namespace LittleWeeps.EditorTools
{
    public static class VetRuleTests
    {
        private static SoloWorld w;private static FamilySession family;
        private static void Need(bool v,string why){if(!v)throw new Exception(why);}
        private static SoloResult Cmd(int actor,SoloAction action,string value="",string target="",float x=0,float y=0,string item="")
        {var p=w.ReadPlayer("p"+actor);return family.Submit((ulong)actor,new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=w.Revision,action=action,value=value,target=target,x=x,y=y,item=item});}
        private static void Care(int actor,int i,string tool,int spot=0)
        {family.AdvanceIdle(.2,out _);var at=DaycareVet.Spot(i,spot);var r=Cmd(actor,SoloAction.Vet,tool,i+"@"+w.ReadVet().round,at.X,at.Y,Guid.NewGuid().ToString("N"));Need(r.Accepted,tool+": "+r.Outcome);SoloWorld.Validate(w.Snapshot());}
        public static void Run()
        {
            w=SoloWorld.WithDinosaurWorld(SoloWorld.Create("p1","p2","p3","p4"));var old=w.Snapshot();old.schema=47;old.vet=null;var id=old.worldId;var home=JsonUtility.ToJson(old.home);var cast=old.treasure.friends.ToArray();w=SoloWorld.WithDinosaurWorld(SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));Need(w.Schema==WorldLayout.Schema && w.Snapshot().worldId==id && JsonUtility.ToJson(w.Snapshot().home)==home && w.ReadTreasure().friends.SequenceEqual(cast),"47 migration preserves existing world");family=new FamilySession(w);
            for(var i=1;i<=4;i++){Need(family.Attach((ulong)i,"p"+i,out _),"attach");Need(Cmd(i,SoloAction.ChangeAvatar,"blue-pup").Accepted,"same avatars");Need(Cmd(i,SoloAction.Travel,i==4?"park":"daycare").Accepted,"arrival");}
            Need(Cmd(1,SoloAction.Vet,"start").Accepted,"start");var g=w.ReadVet();var friends=g.friends.ToArray();Need(g.members.Count(m=>m.attending)==3 && g.patients.Count(p=>p.bed>=0)==4 && g.patients.Count(p=>p.bed== -1)==4,"one common clinic start");Need(g.patients[4].bed==1 && g.patients[5].bed==3 && DaycareNpcCasts.Valid(friends,2),"pets and existing dinosaurs; varied classmates");
            // All four beds are occupied. Two helpers can still choose and
            // treat waiting pets/dinosaurs without evicting a sibling's patient.
            var originalBeds=g.patients.Select(p=>p.bed).ToArray();
            foreach(var waiting in new[]{2,3,6,7}){
                var tool=DaycareVet.Needs(w.ReadVet().patients[waiting],0)?"wash":"brush";
                Care(1,waiting,tool,0);Care(2,waiting,tool,0);
                var patient=w.ReadVet().patients[waiting];
                Need((tool=="wash"?patient.washed[0]:patient.brushed[0])==2,"shared care for a waiting friend");
                Need(w.ReadVet().patients.Select(p=>p.bed).SequenceEqual(originalBeds),"waiting care evicted another patient");
            }
            var readyWaiting=w.Snapshot();var readyPet=readyWaiting.vet.patients[2];readyPet.brushed=new[]{3,3,3};readyPet.bandaged=true;readyPet.cuddles=3;
            var waitingWorld=SoloWorld.Restore(readyWaiting);var helper=waitingWorld.ReadPlayer("p1");
            var homeWaiting=waitingWorld.Apply(new SoloCommand{actor=helper.id,zone=helper.zone,visit=helper.visit,requestId=Guid.NewGuid().ToString("N"),expectedRevision=waitingWorld.Revision,action=SoloAction.Vet,value="home",target="2@1"});
            Need(homeWaiting.Accepted && waitingWorld.ReadVet().patients[2].bed== -2,"comfortable waiting friend can go home");
            SoloWorld.Validate(waitingWorld.Snapshot());
            var waitingCopy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot()));
            var resumed=SoloWorld.Restore(waitingCopy);
            Need(resumed.ReadVet().patients[2].brushed[0]==2 && resumed.ReadVet().patients[7].washed[0]==2,"waiting care survives restoration");
            var at=DaycareVet.Patch(4);Need(!Cmd(2,SoloAction.Vet,"bandage","4@1",at.X,at.Y,"gesture").Accepted,"bandage requires clean skin");Care(1,0,"wash",0);Care(2,0,"wash",0);Need(w.ReadVet().patients[0].washed[0]==2,"two helpers share same spot");
            Need(Cmd(4,SoloAction.Travel,"daycare").Accepted,"late arrival");family.AdvanceIdle(.1,out _);Need(w.ReadVet().members.All(m=>m.attending) && w.ReadVet().patients[0].washed[0]==2 && w.ReadVet().friends.SequenceEqual(friends),"late fourth retains care");
            Need(Cmd(3,SoloAction.Vet,"leave").Accepted,"independent return");family.AdvanceIdle(.1,out _);Need(w.ReadVet().members.Count(m=>m.attending)==3,"exit does not rejoin that visit");family.Detach(2);family.Attach(2,"p2",out _);Need(w.ReadVet().members.Single(m=>m.actor=="p2").attending && w.ReadVet().patients[0].washed[0]==2,"reconnect preserves care");
            var saved=JsonUtility.ToJson(w.Snapshot());w=SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(saved));family=new FamilySession(w);for(var i=1;i<=4;i++)family.Attach((ulong)i,"p"+i,out _);Need(w.ReadVet().patients[0].washed[0]==2 && w.ReadVet().friends.SequenceEqual(friends) && !w.ReadVet().members[2].attending,"partial JSON clinic survives");
            for(var i=0;i<8;i++){
                if(w.ReadVet().patients[i].bed== -1)Need(Cmd(1,SoloAction.Vet,"welcome",i+"@1").Accepted,"choose next patient");
                for(var t=0;t<4;t++)while(!DaycareVet.Done(w.ReadVet().patients[i],t)){var p=w.ReadVet().patients[i];var spot=t==0?Array.FindIndex(p.washed,n=>n<3):t==1?Array.FindIndex(p.brushed,n=>n<3):2;Care(i%2+1,i,new[]{"wash","brush","bandage","cuddle"}[t],spot);}
                Need(Cmd(1,SoloAction.Vet,"home",i+"@1").Accepted,"send comfortable friend home");
            }
            Need(w.ReadVet().patients.All(p=>p.bed== -2),"all four pets and all four dinosaurs cared for");var copy=w.ReadVet();copy.patients[0].washed[0]=0;Need(w.ReadVet().patients[0].washed[0]==3,"deep copy");Need(Cmd(1,SoloAction.Vet,"replay").Accepted,"new common day");Need(w.ReadVet().round==2 && !w.ReadVet().friends.Intersect(friends).Any() && w.ReadVet().patients[0].washed[0]==0,"deliberate new day only resets clinic");Need(!Cmd(1,SoloAction.Vet,"wash","0@1",.5f,.4f,"old").Accepted,"old round cannot touch new patient");SoloWorld.Validate(w.Snapshot());
            foreach(var resource in new[]{"Vet/pets","Vet/pet-walk","Vet/tools","Scenery/daycare-vet"})Need(Resources.Load<Texture2D>(resource)!=null,"art "+resource);
            foreach(var resource in new[]{"welcome","wash","brush","bandage","cuddle","ready","done","puppy","kitten","guinea-pig"})Need(Resources.Load<AudioClip>("Vet/"+resource)!=null,"audio "+resource);
            Debug.Log("VET_RULES_PASS: full-bed waiting-pet/dinosaur shared care, no eviction, waiting completion and JSON restoration; additive47 retention, shared same-patient strokes, four helpers, late arrival, independent departure, reconnect, partial JSON, eight living patients including all four existing dinosaurs, next-patient choice, new round and stale gesture protection.");
        }
    }
}
