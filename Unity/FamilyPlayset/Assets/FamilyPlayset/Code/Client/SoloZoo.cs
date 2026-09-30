using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly List<RectTransform> zooObjects=new List<RectTransform>();
        private readonly Dictionary<string,RawImage> zooAnimals=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Texture2D> zooTextures=new Dictionary<string,Texture2D>();
        private readonly Dictionary<string,(int sequence,double age,float sampled)> zooSamples=new Dictionary<string,(int,double,float)>();
        private readonly Dictionary<string,RectTransform> zooLeaves=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,Text> zooSigns=new Dictionary<string,Text>();
        private RectTransform zooEntrance,zooExit;
        private bool zooApproach;
        private Vector2 zooEntry;
        private string zooOperation,zooSpecies,zooApproachArea;
        private long zooApproachVisit;
        private ZooState Zoo=>HasWorld?(Shared?shared.View.zoo:World.ReadZoo()):null;
        public ZooState ZooGame=>Zoo;
        public int VisibleZooAnimals=>zooAnimals.Values.Count(i=>i.gameObject.activeInHierarchy);
        private RectTransform ZooObject(string name)
        {var r=Rect(Board,name,Vector2.zero,Vector2.zero);zooObjects.Add(r);return r;}
        private void ZooLeaf(Transform parent,Vector2 point,float scale=1)
        {
            var stem=Plain(parent,"Leaf stem",point,new Vector2(9,50)*scale,new Color(.31f,.49f,.23f));stem.rectTransform.localRotation=Quaternion.Euler(0,0,-20);
            for(var i=0;i<3;i++){
                var leaf=Panel(parent,"Browse leaf "+i,point+new Vector2((i%2==0?14:-14)*scale,(i-1)*14*scale),new Vector2(38,18)*scale,new Color(.39f,.66f,.28f),false,true);
                leaf.rectTransform.localRotation=Quaternion.Euler(0,0,i%2==0?25:-25);leaf.raycastTarget=false;
            }
        }
        private void BuildZoo()
        {
            if(Zoo==null)return;
            zooEntrance=ZooObject("Zoo savanna gateway");
            Button(zooEntrance,"Visit the savanna",new Vector2(0,140),new Vector2(340,85),()=>ZooWalk("gate",ZooLayout.Savanna,new Vector2(1200,100)),Cream);
            Label(zooEntrance,"Elephants & giraffes",28,new Vector2(0,235),new Vector2(420,55));
            zooExit=ZooObject("Zoo entrance gateway");
            Button(zooExit,"Zoo entrance",new Vector2(0,100),new Vector2(245,76),()=>ZooWalk("gate",ZooLayout.Entrance,new Vector2(200,100)),Cream);
            foreach(var species in ZooLayout.Species){
                var root=ZooObject("Zoo animal "+species);var image=Rect(root,"Animated "+species,Vector2.zero,Vector2.one*400).gameObject.AddComponent<RawImage>();
                image.raycastTarget=false;image.rectTransform.pivot=new Vector2(.5f,0);zooAnimals.Add(species,image);
                var bucket=ZooObject("Zoo food bucket "+species);
                // Interactive food and rail are separate from the painted
                // background, so shared offers never duplicate scenery props.
                var tub=Panel(bucket,"Food bucket",new Vector2(0,62),new Vector2(100,91),new Color(.62f,.77f,.81f),false);
                Panel(bucket,"Bucket rim",new Vector2(0,110),new Vector2(111,20),new Color(.37f,.57f,.63f),false,true);
                ZooLeaf(bucket,new Vector2(0,118),1.2f);
                HomeHit(bucket,"Take leaves for "+species,new Vector2(0,95),new Vector2(150,190),()=>ZooWalk("take",species,new Vector2(ZooLayout.BucketX(species),100)));
                var name=species=="elephant"?"Elephant":"Giraffe";
                zooSigns.Add(species,Label(bucket,name,25,new Vector2(0,220),new Vector2(420,65)));
                for(var i=0;i<4;i++){
                    var rail=ZooObject("Zoo offering spot "+species+" "+i);
                    Plain(rail,"Rail post",new Vector2(0,76),new Vector2(14,135),new Color(.62f,.45f,.3f));
                    Plain(rail,"Rail",new Vector2(0,112),new Vector2(102,15),new Color(.73f,.57f,.39f));
                    var height=species=="giraffe"?400f:220f;
                    Plain(rail,"Browse feeder pole",new Vector2(65,height/2),new Vector2(9,height),new Color(.68f,.53f,.35f));
                    Plain(rail,"Browse tray",new Vector2(65,height-18),new Vector2(72,14),new Color(.53f,.67f,.55f));
                    Panel(rail,"Stand here",new Vector2(0,0),new Vector2(72,18),new Color(.99f,.84f,.4f,.7f),false,true);
                }
            }
            var snapshot=Shared?shared.View:World.Snapshot();
            foreach(var p in snapshot.players){var r=ZooObject("Zoo held browse "+p.id);ZooLeaf(r,Vector2.zero,.7f);zooLeaves.Add(p.id,r);}
            TickZoo();
        }
        private void ZooWalk(string op,string target,Vector2 entry)
        {
            if(ActionPending)return;
            var food=Zoo?.food.Single(f=>f.actor==Actor);
            if(op=="take" && food?.species!="" && food!=null){
                if(food.species==target && !food.offered){op="offer";entry=new Vector2(ZooLayout.SlotX(target,food.slot),100);}
                else {SendZoo("return","",_=>{});return;}
            }
            CancelPointers();manualCamera=false;zooApproach=true;zooOperation=op;zooSpecies=target;zooEntry=entry;
            var p=ReadPlayer(Actor);zooApproachArea=p.zone;zooApproachVisit=p.visit;
        }
        private void SendZoo(string op,string target,Action<SoloResult> done)
        {
            void Complete(SoloResult result){if(!result.Accepted){message.text=result.Outcome=="hands-full"?"Put down your toy first.":"Try the food bucket again.";}done(result);Render();}
            if(Shared)SubmitShared(SoloAction.Zoo,"",target,op,0,0,Complete);
            else Complete(Command(SoloAction.Zoo,target:target,value:op));
        }
        private void CheckZooInput()
        {
            if(!zooApproach)return;var p=ReadPlayer(Actor);
            if(p.zone!=zooApproachArea || p.visit!=zooApproachVisit || stickDirection.sqrMagnitude>.01f){zooApproach=false;return;}
            if(MenuOpen || TravelPending || ActionPending || Vector2.Distance(new Vector2(p.x,p.y),zooEntry)>8)return;
            var op=zooOperation;var target=zooSpecies;zooApproach=false;destination=null;shared?.Walk(WalkMode.Stop);
            SendZoo(op,target,result=>{
                if(!result.Accepted)return;
                if(op=="take"){
                    var f=Zoo.food.Single(v=>v.actor==Actor);
                    ZooWalk("offer",target,new Vector2(ZooLayout.SlotX(target,f.slot),100));
                }else if(op=="gate"){manualCamera=false;cameraArea=null;}
            });
        }
        private void TickZoo()
        {
            if(zooEntrance==null)return;var z=Zoo;var entrance=CurrentArea==ZooLayout.Entrance;var habitat=CurrentArea==ZooLayout.Savanna;
            zooEntrance.gameObject.SetActive(entrance);zooEntrance.anchoredPosition=ToBoard(1200,100);zooEntrance.localScale=Vector3.one*sceneScale;
            zooExit.gameObject.SetActive(habitat);zooExit.anchoredPosition=ToBoard(200,100);zooExit.localScale=Vector3.one*sceneScale;
            if(!habitat && zooTextures.Count>0){foreach(var image in zooAnimals.Values)image.texture=null;foreach(var t in zooTextures.Values)Resources.UnloadAsset(t);zooTextures.Clear();zooSamples.Clear();}
            foreach(var a in z.animals){
                var image=zooAnimals[a.species];image.transform.parent.gameObject.SetActive(habitat);
                if(!habitat)continue;
                if(!zooTextures.TryGetValue(a.species,out var texture)){texture=Resources.Load<Texture2D>("ZooArt/"+a.species);zooTextures.Add(a.species,texture);image.texture=texture;}
                var now=Time.realtimeSinceStartup;
                if(!zooSamples.TryGetValue(a.species,out var sample) || sample.sequence!=a.sequence || sample.age!=a.age){sample=(a.sequence,a.age,now);zooSamples[a.species]=sample;}
                var extra=Shared?Math.Max(0,now-sample.sampled):0;var point=ZooLayout.Point(a,extra);var root=(RectTransform)image.transform.parent;
                root.anchoredPosition=ToBoard(point.X,point.Y);root.localScale=Vector3.one*sceneScale;
                var moving=a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach;
                var frame=moving && a.age+extra<a.duration?(int)((a.age+extra)*5)%4:a.phase==ZooPhase.Notice?5:a.phase==ZooPhase.Eat?((a.age+extra)<1.4?6:7):a.phase==ZooPhase.Browse?7:4;
                var inset=a.species=="elephant"?9f/texture.width:0;
                image.uvRect=new Rect(frame%4*.25f+inset,frame<4?.5f:0,.25f-2*inset,.5f);
                image.rectTransform.sizeDelta=a.species=="elephant"?new Vector2(540*(1-8*inset),540):new Vector2(480,480);
                // Facing the offer guarantees a readable reach pose. Walking
                // direction is a presentation reflection of the server segment.
                var left=moving && a.toX<a.fromX;
                image.rectTransform.localScale=new Vector3(left?-1:1,1,1);
                image.rectTransform.anchoredPosition=new Vector2(0,a.species=="elephant"?-55:-10);
            }
            foreach(var species in ZooLayout.Species){
                var center=ZooLayout.Center(species);var bucket=zooObjects.Single(r=>r.name=="Zoo food bucket "+species);
                bucket.gameObject.SetActive(habitat);bucket.anchoredPosition=ToBoard(ZooLayout.BucketX(species),100);bucket.localScale=Vector3.one*sceneScale;
                var animal=z.animals.Single(a=>a.species==species);
                zooSigns[species].text=(species=="elephant"?"Elephant":"Giraffe")+(animal.owner==""?"\nTap the leaves":animal.owner==Actor?"\nComing for your leaves":"\nTaking turns");
                for(var i=0;i<4;i++){var r=zooObjects.Single(v=>v.name=="Zoo offering spot "+species+" "+i);r.gameObject.SetActive(habitat);r.anchoredPosition=ToBoard(ZooLayout.SlotX(species,i),100);r.localScale=Vector3.one*sceneScale;}
            }
            foreach(var f in z.food){
                var r=zooLeaves[f.actor];var a=z.animals.FirstOrDefault(v=>v.owner==f.actor);r.gameObject.SetActive(habitat && f.species!="" && a?.consumed!=true);
                if(!r.gameObject.activeSelf)continue;
                var p=ReadPlayer(f.actor);r.anchoredPosition=ToBoard(p.x,p.y)+new Vector2(65,f.species=="giraffe" && f.offered?400:220)*sceneScale;r.localScale=Vector3.one*sceneScale;
            }
            SortDepth();
        }
        private void AddZooDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var r in zooObjects){var ground=r.anchoredPosition.y;var part=0;
                if(r.name.StartsWith("Zoo held browse")){ground-=260*sceneScale;part=4;}
                add(r,ground,part,r.name);
            }
        }
        private void ResetZoo()
        {
            foreach(var r in zooObjects)if(r!=null)Destroy(r.gameObject);zooObjects.Clear();
            foreach(var t in zooTextures.Values)Resources.UnloadAsset(t);zooTextures.Clear();zooAnimals.Clear();zooSamples.Clear();zooLeaves.Clear();zooSigns.Clear();
            zooEntrance=null;zooExit=null;zooApproach=false;
        }
    }
}
