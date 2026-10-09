using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private sealed class FeedingPresentation
        {
            public readonly string species;public readonly float size,offset;public readonly Vector2 mouth;
            public readonly Image[] pads=new Image[4],badges=new Image[4];
            public readonly RawImage[] portraits=new RawImage[4];
            public readonly RectTransform[] pointers=new RectTransform[4],done=new RectTransform[4],waiting=new RectTransform[4];
            public readonly string[] avatars=new string[4];
            public int seen=-1,reaction=-1,events;public string cue="idle";public float gap;
            public FeedingPresentation(string id,float scale,Vector2 socket,float side=0){species=id;size=scale;mouth=socket;offset=side;}
        }
        private readonly Dictionary<string,FeedingPresentation> savannaFeeding=CreateFeedingPresentations();
        private static Dictionary<string,FeedingPresentation> CreateFeedingPresentations()
        {
            var result=new Dictionary<string,FeedingPresentation>{
            {"giraffe",new FeedingPresentation("giraffe",350,new Vector2(.772f,.735f))},
            {"zebra",new FeedingPresentation("zebra",380,new Vector2(.952f,.17f),65)},
            // Resting atlas cell: lip at (415,237) in the original 443x444
            // cell, expressed bottom-up. The paw height is not a mouth socket.
            {"lion",new FeedingPresentation("lion",420,new Vector2(.936f,.466f),35)}
            };
            foreach(var info in ZooCatalog.All)
                if(info.id!="elephant" && info.id!="brachiosaurus" && !result.ContainsKey(info.id))
                    result.Add(info.id,new FeedingPresentation(info.id,info.size,Vector2.zero));
            return result;
        }
        public string SavannaCue=>savannaFeeding.TryGetValue(ZooCurrentExhibit,out var p)?p.cue:"";
        public float SavannaMouthGap=>savannaFeeding.TryGetValue(ZooCurrentExhibit,out var p)?p.gap:0;
        public int[] SavannaFinishEvents=>new[]{"giraffe","zebra","lion"}.Select(id=>savannaFeeding.TryGetValue(id,out var p)?p.events:0).ToArray();
        private Vector2 ZooHandFoodPoint(string actor)
        {
            var p=ReadPlayer(actor);
            var point=Shared && shared.Connected?shared.VisualPosition(actor):new Vector2(p.x,p.y);
            // Player artwork has a 180-unit reference height. Keep the portion
            // above it at one stable hand socket, including shared interpolation.
            return ToBoard(point.x,point.y)+new Vector2(65,205)*sceneScale;
        }
        private Vector2 ZooAnimalMouth(RawImage image,ZooAnimal a)
        {
            var root=(RectTransform)image.transform.parent;
            Vector2 tip;
            if(image is BrachiosaurusArtView brachio)tip=brachio.MouthTip;
            else if(image is SavannaArtView savanna)tip=savanna.MouthTip;
            else if(image is ElephantArtView elephant && a.owner!="")tip=elephant.TrunkTip;
            else if(savannaFeeding.TryGetValue(a.species,out var presentation) && presentation.mouth!=Vector2.zero)return SavannaMouth(presentation);
            else{
                // Fitted reach/eat atlas sockets from the original sixteen-species
                // feeding implementation. The animal moves toward the held food.
                var info=ZooCatalog.Get(a.species);
                tip=Vector2.Scale(new Vector2(info.mouthX,info.mouthY),image.rectTransform.localScale)+image.rectTransform.anchoredPosition;
            }
            return Board.InverseTransformPoint(root.TransformPoint(tip));
        }
        private void PoseZooHandReach(RawImage image,ZooAnimal a,double age)
        {
            if(a.owner=="" || a.phase!=ZooPhase.Approach && a.phase!=ZooPhase.Eat)return;
            var f=Zoo.food.FirstOrDefault(v=>v.actor==a.owner && v.species==a.species && v.offered);
            if(f==null)return;
            // The animal reaches before the 1.4-second eating moment. Never
            // animate food away from its player or through another child.
            var reach=a.phase==ZooPhase.Eat?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(((float)(age/a.duration)-.65f)/.35f));
            var root=(RectTransform)image.transform.parent;
            var delta=(ZooHandFoodPoint(a.owner)-ZooAnimalMouth(image,a))*reach;
            var local=(Vector2)image.rectTransform.InverseTransformVector(Board.TransformVector(delta));
            if(image is ElephantArtView elephant)elephant.Reach(local);
            else if(image is BrachiosaurusArtView brachio)brachio.Reach(local);
            else if(image is SavannaArtView head)head.Reach(local);
            else root.anchoredPosition+=delta; // Fish swim to the hand.
        }
        private void ResetZooHandReach(RawImage image,ZooAnimal a)
        {
            if(image is ElephantArtView elephant)elephant.Reach(Vector2.zero);
            else if(image is BrachiosaurusArtView brachio)brachio.Reach(Vector2.zero);
            else if(image is SavannaArtView head){
                head.Reach(Vector2.zero);
                if(head.localHead){
                    var info=ZooCatalog.Get(a.species);var r=image.GetPixelAdjustedRect();
                    head.mouth=savannaFeeding[a.species].mouth!=Vector2.zero?savannaFeeding[a.species].mouth:new Vector2((info.mouthX-r.xMin)/r.width,info.mouthY/r.height);
                }
            }
        }
        private void BuildSavannaSpot(RectTransform rail,FeedingPresentation p,int i)
        {
            p.pads[i]=Panel(rail,"Your standing picture",Vector2.zero,new Vector2(80,18),new Color(.76f,.69f,.53f,.24f),false,true);
            // Keep pictures above both feeding and standing finish poses.
            // Board coordinates scale with the safe-area composition.
            p.badges[i]=Panel(rail,"Feeder portrait",new Vector2(0,380),Vector2.one*58,Cream,false,true);
            p.portraits[i]=Rect(p.badges[i].transform,"Child picture",Vector2.zero,Vector2.one*48).gameObject.AddComponent<RawImage>();p.portraits[i].raycastTarget=false;
            p.pointers[i]=Rect(rail,"Your feeding pointer",new Vector2(0,337),Vector2.zero);ZooArrow(p.pointers[i],Vector2.zero,1);p.pointers[i].localRotation=Quaternion.Euler(0,0,-90);
            p.done[i]=Rect(p.badges[i].transform,"Food consumed picture",new Vector2(17,-19),Vector2.zero);FossilCheck(p.done[i],true);p.done[i].localScale=Vector3.one*.45f;
            p.waiting[i]=Rect(p.badges[i].transform,"Waiting turn picture",new Vector2(18,-18),Vector2.zero);Panel(p.waiting[i],"Turn clock",Vector2.zero,new Vector2(24,24),Cream,false,true);Plain(p.waiting[i],"Clock hand",new Vector2(0,3),new Vector2(3,9),Ink);Plain(p.waiting[i],"Clock hand",new Vector2(3,0),new Vector2(9,3),Ink);
        }
        private float GiraffeBend(ZooAnimal a,double age)
        {
            if(a.phase==ZooPhase.Eat)return 43+(a.consumed?(savannaFeeding["giraffe"].reaction==a.fed?Mathf.Sin(Mathf.Clamp01((float)(age-1.4)/1.2f)*Mathf.PI*2)*2:0):Mathf.Sin((float)age*6)*.8f);
            return a.phase==ZooPhase.Approach?43*Mathf.SmoothStep(0,1,Mathf.Clamp01((float)(age/a.duration)-.65f)/.35f):0;
        }
        private int SavannaFrame(ZooAnimal a,double age,int frame)
        {
            if(!savannaFeeding.TryGetValue(a.species,out var p))return frame;
            if(a.species=="giraffe")return GiraffeBend(a,age)>0?4:frame;
            if(a.phase!=ZooPhase.Eat || p.species!="zebra" && p.species!="lion")return frame;
            var reaction=a.consumed && p.reaction==a.fed && age<2.6;
            // Zebra keeps its actual lowered-head hay pose until consumed.
            // Lion keeps its resting finish nod; the food remains on the child.
            return a.species=="zebra"?(reaction?7:a.consumed?4:6):4;
        }
        private void PoseSavanna(RawImage image,ZooAnimal a,double age)
        {
            if(!savannaFeeding.TryGetValue(a.species,out var p))return;
            image.rectTransform.anchoredPosition+=new Vector2(p.offset,0);
            if(image is SavannaArtView art){
                if(art.localHead){art.Pose(0);return;}
                art.mouth=p.mouth;art.lion=a.species=="lion";
                var head=0f;
                if(a.phase==ZooPhase.Eat){
                    if(!a.consumed)head=7+Mathf.Sin((float)age*6)*1.2f;
                    else if(p.reaction==a.fed){var finish=Mathf.Clamp01((float)(age-1.4)/1.2f);head=7*(1-finish)+Mathf.Sin(finish*Mathf.PI*2)*2;}
                }
                art.Pose(a.species=="giraffe"?GiraffeBend(a,age):head);
            }
        }
        private Vector2 SavannaMouth(FeedingPresentation p)
        {
            var image=zooAnimals[p.species];var root=(RectTransform)image.transform.parent;
            Vector2 tip;
            if(image is SavannaArtView articulated)tip=articulated.MouthTip;
            else{var r=image.GetPixelAdjustedRect();tip=Vector2.Scale(new Vector2(r.xMin+p.mouth.x*r.width,r.yMin+p.mouth.y*r.height),image.rectTransform.localScale)+image.rectTransform.anchoredPosition;}
            return Board.InverseTransformPoint(root.TransformPoint(tip));
        }
        private string FeedingFeedback(string species,ZooAnimal a,ZooFood own)
        {
            if(applicationPaused || CurrentArea!=ZooCatalog.Get(species).area || Shared && !shared.Connected)return "";
            if(own?.species==species)return !own.offered?"carrying":a.owner!=Actor?"waiting":a.consumed?"finished":a.phase==ZooPhase.Eat?"eating":"approaching";
            return zooApproach && zooSpecies==species?"collecting":"idle";
        }
        private void TickSavannaSpots(ZooState z,string[] visible)
        {
            foreach(var p in savannaFeeding.Values){
                var a=z.animals.First(v=>v.species==p.species);var shown=visible.Contains(p.species);p.cue=FeedingFeedback(p.species,a,z.food.FirstOrDefault(v=>v.actor==Actor));p.gap=0;
                // Seed after reconnect/pause and while away. No old handoff or
                // celebration is replayed by an initial/late snapshot.
                if(p.seen>=0 && a.fed>p.seen && shown && !applicationPaused && (!Shared || shared.Connected) && a.phase==ZooPhase.Eat && a.consumed && a.age<2.6){p.reaction=a.fed;p.events++;}
                p.seen=a.fed;
                var info=ZooCatalog.Get(p.species);
                var cue=p.cue=="collecting"?"Getting your "+info.FoodName:p.cue=="carrying"?"Bring food to your picture":p.cue=="waiting"?"Your food is waiting":p.cue=="approaching"?"Coming to your food":p.cue=="eating"?"Eating your "+info.FoodName:p.cue=="finished"?"Yum! Thank you":"Tap the "+info.FoodName;
                zooSigns[p.species].text=info.name+"\n"+cue;
                for(var i=0;i<4;i++){
                    var f=z.food.FirstOrDefault(v=>v.species==p.species && v.slot==i);var local=f?.actor==Actor;var done=f!=null && a.owner==f.actor && a.consumed;
                    p.pads[i].color=local?new Color(.98f,.8f,.33f,.85f):f!=null?new Color(.63f,.75f,.55f,.5f):new Color(.76f,.69f,.53f,.24f);
                    p.badges[i].gameObject.SetActive(f!=null);p.pointers[i].gameObject.SetActive(local && !done);p.done[i].gameObject.SetActive(done);p.waiting[i].gameObject.SetActive(f!=null && f.offered && a.owner!=f.actor);
                    if(f!=null && a.owner==f.actor && a.phase==ZooPhase.Eat && !a.consumed)p.gap=Vector2.Distance(ZooHandFoodPoint(f.actor),ZooAnimalMouth(zooAnimals[p.species],a))/sceneScale;
                    if(f==null)continue;var avatar=ReadPlayer(f.actor).avatar;
                    if(p.avatars[i]!=avatar){var art=WorldResources.Load<CharacterMenuArt>("Shared/Characters/Menu/"+PlayableCharacters.Find(avatar).ArtId);p.portraits[i].texture=art?.texture;if(art!=null)p.portraits[i].rectTransform.sizeDelta=art.size*(48/Mathf.Max(art.size.x,art.size.y));p.avatars[i]=avatar;}
                    p.badges[i].color=done?new Color(.7f,.87f,.58f):local?new Color(1,.87f,.48f):Cream;
                }
            }
        }
        private void ResetSavannaObservation(){foreach(var p in savannaFeeding.Values){p.seen=-1;p.reaction=-1;}}
        private void ResetSavannaFeeding(){ResetSavannaObservation();foreach(var p in savannaFeeding.Values){p.events=0;p.cue="idle";p.gap=0;Array.Clear(p.avatars,0,4);}}
    }
}
