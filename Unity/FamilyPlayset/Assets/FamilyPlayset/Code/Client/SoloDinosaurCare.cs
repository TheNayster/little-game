using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly Dictionary<string,RectTransform> dinosaurBuckets=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> dinosaurBucketHits=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> dinosaurHands=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> dinosaurPortions=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> dinosaurHearts=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> dinosaurTrays=new Dictionary<string,RectTransform>();
        private bool dinosaurCareApproach;
        private string dinosaurCareOp,dinosaurCareSpecies;
        private long dinosaurCareVisit;
        private Vector2 dinosaurCareEntry;
        private DinosaurCare OwnDinosaurCare=>Dinosaurs?.care?.FirstOrDefault(f=>f.actor==Actor);
        private void DinosaurHeart(Transform parent,Vector2 at,float size)
        {
            var pink=new Color(.96f,.48f,.59f);
            var baseShape=Panel(parent,"Heart base",at,new Vector2(size*.65f,size*.65f),pink,false);baseShape.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            Panel(parent,"Heart left",at+new Vector2(-size*.23f,size*.2f),Vector2.one*size*.65f,pink,false,true);
            Panel(parent,"Heart right",at+new Vector2(size*.23f,size*.2f),Vector2.one*size*.65f,pink,false,true);
        }
        private void BuildDinosaurCare()
        {
            if(Dinosaurs?.care==null)return;
            foreach(var species in DinosaurRides.Species){
                var id=species;var bucket=Rect(Board,"Dinosaur food bucket "+id,Vector2.zero,Vector2.zero);dinosaurBuckets.Add(id,bucket);
                Panel(bucket,"Bucket",new Vector2(0,52),new Vector2(105,85),new Color(.62f,.78f,.83f),false);
                Panel(bucket,"Rim",new Vector2(0,98),new Vector2(115,18),new Color(.36f,.57f,.64f),false,true);
                ZooFoodPicture(bucket,id=="tyrannosaurus"?ZooFoodKind.Meat:ZooFoodKind.Leaves,new Vector2(0,112),1.1f);
                // Input remains above the character while bucket pixels stay behind it.
                var bucketHit=Rect(Board,"Dinosaur food touch "+id,Vector2.zero,Vector2.zero);dinosaurBucketHits.Add(id,bucketHit);
                HomeHit(bucketHit,"Feed "+id,new Vector2(0,76),new Vector2(180,210),()=>DinosaurCareWalk("take",id),cancelPointers:false);
                var hand=Rect(Board,"Dinosaur pet picture "+id,Vector2.zero,Vector2.one*150);dinosaurHands.Add(id,hand);
                var face=Panel(hand,"Pet "+id,Vector2.zero,Vector2.one*132,Cream,true,true);NavButton(face,()=>DinosaurCareWalk("pet",id));
                var ink=new Color(.73f,.53f,.34f);
                Panel(face.transform,"Palm",new Vector2(-10,-7),new Vector2(46,40),ink,false,true);
                for(var i=0;i<4;i++)Panel(face.transform,"Finger "+i,new Vector2(-29+i*13,19),new Vector2(10,31-i*3),ink,false,true);
                var thumb=Panel(face.transform,"Thumb",new Vector2(21,-2),new Vector2(12,31),ink,false,true);thumb.rectTransform.localRotation=Quaternion.Euler(0,0,-40);
                DinosaurHeart(face.transform,new Vector2(32,32),24);
                var hearts=Rect(Board,"Dinosaur affection "+id,Vector2.zero,Vector2.zero);dinosaurHearts.Add(id,hearts);
                for(var i=0;i<3;i++)DinosaurHeart(hearts,new Vector2((i-1)*40,i%2*28),32);
            }
            foreach(var f in Dinosaurs.care){
                var portion=Rect(Board,"Dinosaur portion "+f.actor,Vector2.zero,Vector2.zero);dinosaurPortions.Add(f.actor,portion);
                foreach(var kind in new[]{ZooFoodKind.Meat,ZooFoodKind.Leaves}){var shape=Rect(portion,"Food "+(int)kind,Vector2.zero,Vector2.zero);ZooFoodPicture(shape,kind,Vector2.zero,.85f);}
                var tray=Rect(Board,"Dinosaur feeder "+f.actor,Vector2.zero,Vector2.zero);dinosaurTrays.Add(f.actor,tray);
                Plain(tray,"Feeder stem",Vector2.zero,new Vector2(9,100),new Color(.69f,.57f,.39f));
                Plain(tray,"Feeding tray",Vector2.zero,new Vector2(78,12),new Color(.49f,.68f,.56f));
                Panel(tray,"Your standing spot",new Vector2(-60,0),new Vector2(68,16),new Color(.97f,.82f,.42f),false,true);
            }
        }
        private void DinosaurCareWalk(string op,string species)
        {
            if(ActionPending || dinosaurCareApproach)return;
            var f=OwnDinosaurCare;
            if(f!=null && f.phase!=DinosaurCarePhase.None){
                if(f.phase!=DinosaurCarePhase.Held || f.kind!="feed")return;
                op="offer";species=f.species;
            }
            if(DinosaurRides.Usable(ReadPlayer(Actor).fixture)){
                SendDinosaurCare("off","",r=>{if(r.Accepted)DinosaurCareWalk(op,species);});return;
            }
            CancelPointers();manualCamera=false;dinosaurCareApproach=true;dinosaurCareOp=op;dinosaurCareSpecies=species;dinosaurCareVisit=ReadPlayer(Actor).visit;
            dinosaurCareEntry=DinosaurCareDestination();
        }
        private Vector2 DinosaurCareDestination()
        {
            if(dinosaurCareOp=="take")return new Vector2(DinosaurCareRules.BucketX(dinosaurCareSpecies),100);
            if(dinosaurCareOp=="offer"){var f=OwnDinosaurCare;return f==null?Vector2.zero:new Vector2(f.x,f.y);}
            var a=Dinosaurs.animals.Single(v=>v.species==dinosaurCareSpecies);
            return new Vector2(Mathf.Clamp(a.x+DinosaurCareRules.PetX(a.species),510,4400),100);
        }
        private void SendDinosaurCare(string op,string species,Action<SoloResult> done)
        {
            void Finish(SoloResult r){if(!r.Accepted)message.text=r.Outcome=="dinosaur-being-ridden"?"Get off the dinosaur first.":r.Outcome=="put-down-toy"?"Put down your toy first.":"Choose a dinosaur with a free turn.";done(r);Render();}
            if(Shared)SubmitShared(SoloAction.Dinosaur,"",species,op,0,0,Finish);else Finish(Command(SoloAction.Dinosaur,target:species,value:op));
        }
        private void CheckDinosaurCareInput()
        {
            if(!dinosaurCareApproach)return;var p=ReadPlayer(Actor);
            if(p.zone!=DinosaurRides.Area || p.visit!=dinosaurCareVisit || stickDirection.sqrMagnitude>.01f){dinosaurCareApproach=false;return;}
            dinosaurCareEntry=DinosaurCareDestination();
            if(MenuOpen || TravelPending || ActionPending || Vector2.Distance(new Vector2(p.x,p.y),dinosaurCareEntry)>9)return;
            var op=dinosaurCareOp;var id=dinosaurCareSpecies;dinosaurCareApproach=false;destination=null;shared?.Walk(WalkMode.Stop);
            SendDinosaurCare(op,id,r=>{if(r.Accepted && op=="take")DinosaurCareWalk("offer",id);});
        }
        private void TickDinosaurCare()
        {
            if(Dinosaurs?.care==null)return;var shown=CurrentArea==DinosaurRides.Area;
            foreach(var id in DinosaurRides.Species){
                var bucket=dinosaurBuckets[id];bucket.gameObject.SetActive(shown);bucket.anchoredPosition=ToBoard(DinosaurCareRules.BucketX(id),85);bucket.localScale=Vector3.one*sceneScale;
                var bucketHit=dinosaurBucketHits[id];bucketHit.gameObject.SetActive(shown);bucketHit.anchoredPosition=bucket.anchoredPosition;bucketHit.localScale=Vector3.one*sceneScale;
                var animal=(RectTransform)dinosaurPictures[id].transform.parent;var hand=dinosaurHands[id];hand.gameObject.SetActive(shown);hand.anchoredPosition=animal.anchoredPosition+new Vector2(-185,-30)*sceneScale;hand.localScale=Vector3.one*sceneScale;
                var f=Dinosaurs.care.FirstOrDefault(v=>v.species==id && (v.phase==DinosaurCarePhase.Pet || v.kind=="pet" && v.phase==DinosaurCarePhase.Done));
                var heart=dinosaurHearts[id];heart.gameObject.SetActive(shown && f!=null);heart.anchoredPosition=animal.anchoredPosition+new Vector2(65,200+(float)Math.Sin(Dinosaurs.clock*3)*12)*sceneScale;heart.localScale=Vector3.one*sceneScale;
            }
            foreach(var f in Dinosaurs.care){
                var portion=dinosaurPortions[f.actor];var feeding=shown && f.kind=="feed" && f.phase!=DinosaurCarePhase.None && !f.consumed;portion.gameObject.SetActive(feeding);
                var tray=dinosaurTrays[f.actor];var offered=feeding && f.phase!=DinosaurCarePhase.Held;tray.gameObject.SetActive(offered);
                if(!feeding)continue;
                var kind=f.species=="tyrannosaurus"?ZooFoodKind.Meat:ZooFoodKind.Leaves;
                foreach(Transform child in portion)child.gameObject.SetActive(child.name=="Food "+(int)kind);
                var p=ReadPlayer(f.actor);var at=Shared?shared.VisualPosition(f.actor):new Vector2(p.x,p.y);var height=DinosaurCareRules.MouthHeight(f.species)+50;
                portion.anchoredPosition=offered?ToBoard(f.x+60,f.y)+new Vector2(0,height)*sceneScale:ToBoard(at.x,at.y)+new Vector2(45,95)*sceneScale;portion.localScale=Vector3.one*sceneScale;
                tray.anchoredPosition=ToBoard(f.x+60,f.y);tray.localScale=Vector3.one*sceneScale;
                var stem=(RectTransform)tray.Find("Feeder stem");stem.sizeDelta=new Vector2(9,height);stem.anchoredPosition=new Vector2(0,height/2);
                ((RectTransform)tray.Find("Feeding tray")).anchoredPosition=new Vector2(0,height-14);
            }
        }
        private void PresentDinosaurPetters()
        {
            if(CurrentArea!=DinosaurRides.Area || Dinosaurs?.care==null)return;
            foreach(var f in Dinosaurs.care.Where(f=>f.phase==DinosaurCarePhase.Pet)){
                var visual=f.actor==Actor?characterVisual:friends.TryGetValue(f.actor,out var other)?other.view:null;
                visual?.PresentFrame(new CharacterFrame(CharacterPose.Wave,0,true,(float)(Dinosaurs.clock-f.started)),Time.unscaledDeltaTime);
            }
        }
        private void AddDinosaurCareDepth(Action<RectTransform,float,int,string> add)
        {
            // Buckets share the child ground key but sort behind the child; a
            // food station must never cover the face during an offering.
            foreach(var r in dinosaurBuckets.Values)add(r,ToBoard(0,100).y,0,r.name);
            foreach(var r in dinosaurBucketHits.Values)add(r,ToBoard(0,100).y,7,r.name);
            foreach(var r in dinosaurHands.Values)add(r,r.anchoredPosition.y,4,r.name);
            foreach(var r in dinosaurHearts.Values)add(r,r.anchoredPosition.y-220*sceneScale,6,r.name);
            foreach(var f in Dinosaurs?.care??Array.Empty<DinosaurCare>()){
                var ground=ToBoard(f.x,f.y).y;if(dinosaurPortions.TryGetValue(f.actor,out var portion))add(portion,ground,5,"care-"+f.actor);if(dinosaurTrays.TryGetValue(f.actor,out var tray))add(tray,ground,4,"care-"+f.actor);
            }
        }
        private void ResetDinosaurCare()
        {
            foreach(var group in new[]{dinosaurBuckets,dinosaurBucketHits,dinosaurHands,dinosaurPortions,dinosaurHearts,dinosaurTrays}){foreach(var r in group.Values)if(r!=null)Destroy(r.gameObject);group.Clear();}
            dinosaurCareApproach=false;
        }
    }
}
