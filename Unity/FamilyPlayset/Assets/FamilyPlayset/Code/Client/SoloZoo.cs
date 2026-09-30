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
        private readonly Dictionary<string,RawImage> zooGatePictures=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Texture2D> zooGateTextures=new Dictionary<string,Texture2D>();
        private readonly Dictionary<string,(int sequence,double age,float sampled)> zooSamples=new Dictionary<string,(int,double,float)>();
        private readonly Dictionary<string,RectTransform> zooLeaves=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,Text> zooSigns=new Dictionary<string,Text>();
        private readonly Dictionary<string,RectTransform> zooBuckets=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform[]> zooRails=new Dictionary<string,RectTransform[]>();
        private RectTransform zooEntrance,zooExit,zooPrevious,zooNext;
        private Text zooPreviousLabel,zooNextLabel;
        private bool zooApproach;
        private Vector2 zooEntry;
        private string zooOperation,zooSpecies,zooApproachArea;
        private long zooApproachVisit;
        private ZooState Zoo=>HasWorld?(Shared?shared.View.zoo:World.ReadZoo()):null;
        public ZooState ZooGame=>Zoo;
        public int VisibleZooAnimals=>zooAnimals.Values.Count(i=>i.gameObject.activeInHierarchy);
        public int ZooTextureCount=>zooTextures.Count;
        private RectTransform ZooObject(string name)
        {var r=Rect(Board,name,Vector2.zero,Vector2.zero);zooObjects.Add(r);return r;}
        private void ZooFoodPicture(Transform parent,ZooFoodKind kind,Vector2 at,float scale=1)
        {
            if(kind==ZooFoodKind.Leaves || kind==ZooFoodKind.Seaweed){
                Plain(parent,"Food stem",at,new Vector2(7,48)*scale,new Color(.31f,.49f,.23f));
                for(var i=0;i<3;i++){
                    var leaf=Panel(parent,"Food leaf "+i,at+new Vector2((i%2==0?14:-14)*scale,(i-1)*14*scale),new Vector2(kind==ZooFoodKind.Seaweed?19:38,18)*scale,new Color(.39f,.66f,.28f),false,true);
                    leaf.rectTransform.localRotation=Quaternion.Euler(0,0,i%2==0?25:-25);
                }
            }else if(kind==ZooFoodKind.Hay){
                for(var i=0;i<6;i++){var h=Plain(parent,"Hay "+i,at+new Vector2((i-3)*7,0)*scale,new Vector2(5,48)*scale,new Color(.87f,.72f,.3f));h.rectTransform.localRotation=Quaternion.Euler(0,0,(i-3)*8);}
            }else if(kind==ZooFoodKind.Pellets){
                for(var i=0;i<5;i++)Panel(parent,"Pellet "+i,at+new Vector2((i%3-1)*17,(i/3)*17)*scale,new Vector2(13,13)*scale,new Color(.86f,.61f,.3f),false,true);
            }else if(kind==ZooFoodKind.Meat){
                Panel(parent,"Prepared meat",at,new Vector2(54,32)*scale,new Color(.83f,.46f,.45f),false,true);
                Panel(parent,"Meat center",at,new Vector2(15,13)*scale,Cream,false,true);
            }else if(kind==ZooFoodKind.Fish){
                Panel(parent,"Fish portion",at,new Vector2(49,22)*scale,new Color(.45f,.69f,.83f),false,true);
                Panel(parent,"Fish tail",at+new Vector2(-25,0)*scale,new Vector2(17,25)*scale,new Color(.34f,.56f,.72f),false,true);
                Panel(parent,"Fish eye",at+new Vector2(15,3)*scale,new Vector2(5,5)*scale,Ink,false,true);
            }else{
                Panel(parent,"Small insect",at,new Vector2(24,14)*scale,new Color(.55f,.39f,.22f),false,true);
                for(var i=0;i<4;i++)Plain(parent,"Insect leg "+i,at+new Vector2((i/2==0?-8:8),(i%2==0?-9:9))*scale,new Vector2(3,14)*scale,new Color(.55f,.39f,.22f));
            }
        }
        private void BuildZoo()
        {
            if(Zoo==null)return;
            zooEntrance=ZooObject("Zoo trail gateways");
            for(var i=0;i<ZooCatalog.Trails.Length;i++){
                var area=ZooCatalog.Trails[i];var label=area==ZooLayout.Savanna?"Visit the savanna":"Visit "+ZooCatalog.Name(area);
                var at=new Vector2(i%2==0?-300:300,i<2?150:420);
                Panel(zooEntrance,"Trail shadow",at+new Vector2(0,-8),Vector2.one*250,new Color(.24f,.43f,.35f,.22f),false,true);
                var rim=Panel(zooEntrance,label,at,Vector2.one*246,Cream,true,true);
                // The same picture circles as the world menu: the whole circle
                // is touchable, and the label is retained as an accessible name.
                NavButton(rim,()=>ZooWalk("gate",area,new Vector2(ZooCatalog.EntranceX(area),100)));
                var colors=new[]{new Color(.91f,.81f,.53f),new Color(.72f,.86f,.61f),new Color(.74f,.87f,.63f),new Color(.58f,.83f,.94f)};
                var face=Panel(rim.transform,"Animal picture circle",Vector2.zero,Vector2.one*228,colors[i],false,true);
                face.gameObject.AddComponent<Mask>().showMaskGraphic=true;
                var species=new[]{"elephant","brachiosaurus","crocodile","clownfish"}[i];
                var icon=Rect(face.transform,"Picture "+species,Vector2.zero,Vector2.one*210).gameObject.AddComponent<RawImage>();
                icon.raycastTarget=false;icon.uvRect=new Rect(.0023f,0,.2454f,.5f);zooGatePictures.Add(species,icon);
                if(species=="brachiosaurus")icon.rectTransform.sizeDelta=Vector2.one*180;
                if(species=="clownfish")icon.rectTransform.sizeDelta=Vector2.one*230;
            }

            zooExit=ZooObject("Zoo entrance gateway");
            Button(zooExit,"Zoo entrance",new Vector2(0,100),new Vector2(245,76),()=>ZooWalk("gate",ZooLayout.Entrance,new Vector2(200,100)),Cream);
            zooPrevious=ZooObject("Previous Zoo trail");
            var previous=Button(zooPrevious,"Previous trail",new Vector2(0,220),new Vector2(340,76),()=>ZooWalk("gate",ZooCatalog.Previous(CurrentArea),new Vector2(200,100)),Cream);
            zooPreviousLabel=previous.GetComponentInChildren<Text>();
            zooNext=ZooObject("Next Zoo trail");
            var next=Button(zooNext,"Next trail",new Vector2(0,100),new Vector2(340,76),()=>ZooWalk("gate",ZooCatalog.Next(CurrentArea),new Vector2(9200,100)),Cream);
            zooNextLabel=next.GetComponentInChildren<Text>();
            foreach(var info in ZooCatalog.All){
                var species=info.id;var root=ZooObject("Zoo animal "+species);
                var image=Rect(root,"Animated "+species,Vector2.zero,Vector2.one*info.size).gameObject.AddComponent<RawImage>();
                image.raycastTarget=false;image.rectTransform.pivot=new Vector2(.5f,0);zooAnimals.Add(species,image);
                HomeHit(root,"Hear "+species,new Vector2(0,info.size*.4f),new Vector2(info.size*.65f,info.size*.7f),()=>ZooCall(species));
                var bucket=ZooObject("Zoo food bucket "+species);zooBuckets.Add(species,bucket);
                Panel(bucket,"Food bucket",new Vector2(0,62),new Vector2(100,91),new Color(.62f,.77f,.81f),false);
                Panel(bucket,"Bucket rim",new Vector2(0,110),new Vector2(111,20),new Color(.37f,.57f,.63f),false,true);
                ZooFoodPicture(bucket,info.food,new Vector2(0,118),1.2f);
                HomeHit(bucket,"Take "+info.FoodName+" for "+species,new Vector2(0,95),new Vector2(150,190),()=>ZooWalk("take",species,new Vector2(ZooLayout.BucketX(species),100)));
                zooSigns.Add(species,Label(bucket,info.name,24,new Vector2(0,200),new Vector2(460,65)));
                var rails=new RectTransform[4];zooRails.Add(species,rails);
                for(var i=0;i<4;i++){
                    var rail=ZooObject("Zoo offering spot "+species+" "+i);rails[i]=rail;
                    Plain(rail,"Rail post",new Vector2(0,76),new Vector2(14,135),new Color(.62f,.45f,.3f));
                    Plain(rail,"Rail",new Vector2(0,112),new Vector2(102,15),new Color(.73f,.57f,.39f));
                    var height=info.FeedHeight;
                    Plain(rail,info.habitat==ZooHabitat.Tank?"Aquarium delivery chute":"Feeder support",new Vector2(65,height/2),new Vector2(info.habitat==ZooHabitat.Tank?16:9,height),new Color(.68f,.63f,.5f));
                    Plain(rail,"Feeding tray",new Vector2(65,height-18),new Vector2(72,14),new Color(.53f,.67f,.55f));
                    Panel(rail,"Stand here",Vector2.zero,new Vector2(72,18),new Color(.99f,.84f,.4f,.7f),false,true);
                }
            }
            var snapshot=Shared?shared.View:World.Snapshot();
            foreach(var player in snapshot.players){
                var r=ZooObject("Zoo held portion "+player.id);zooLeaves.Add(player.id,r);
                foreach(ZooFoodKind kind in Enum.GetValues(typeof(ZooFoodKind))){var shape=Rect(r,"Food "+(int)kind,Vector2.zero,Vector2.zero);ZooFoodPicture(shape,kind,Vector2.zero,.8f);}
            }
            TickZoo();
        }
        private void ZooWalk(string op,string target,Vector2 entry)
        {
            if(ActionPending)return;
            var food=Zoo?.food.Single(f=>f.actor==Actor);
            if(op=="take"){
                // Repeated food taps must not replace the active walk or return
                // an offered portion. Resume a held portion only after a walk
                // was interrupted; its original species and slot stay intact.
                if(zooApproach && (zooOperation=="take" || zooOperation=="offer"))return;
                if(food!=null && food.species!=""){
                    if(food.offered)return;
                    op="offer";target=food.species;entry=new Vector2(ZooLayout.SlotX(target,food.slot),100);
                }
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
            if(zooEntrance==null)return;var z=Zoo;var entrance=CurrentArea==ZooLayout.Entrance;var habitat=ZooCatalog.Trail(CurrentArea);
            void Place(RectTransform r,bool shown,float x){r.gameObject.SetActive(shown);r.anchoredPosition=ToBoard(x,100);r.localScale=Vector3.one*sceneScale;}
            Place(zooEntrance,entrance,1200);
            foreach(var picture in zooGatePictures){
                if(entrance && !zooGateTextures.ContainsKey(picture.Key)){
                    var texture=Resources.Load<Texture2D>("ZooArt/"+picture.Key);zooGateTextures.Add(picture.Key,texture);picture.Value.texture=texture;
                }else if(!entrance && zooGateTextures.TryGetValue(picture.Key,out var texture)){
                    picture.Value.texture=null;if(texture!=null)Resources.UnloadAsset(texture);zooGateTextures.Remove(picture.Key);
                }
            }
            Place(zooExit,habitat,200);Place(zooPrevious,habitat,200);Place(zooNext,habitat,9200);
            if(habitat){zooPreviousLabel.text="To "+ZooCatalog.Name(ZooCatalog.Previous(CurrentArea));zooNextLabel.text="To "+ZooCatalog.Name(ZooCatalog.Next(CurrentArea));}
            var half=Board.rect.width/(2*sceneScale);
            var visible=ZooCatalog.All.Where(i=>i.area==CurrentArea && Math.Abs(i.Center-cameraX)<half+1200).OrderBy(i=>Math.Abs(i.Center-cameraX)).Take(3).Select(i=>i.id).ToArray();
            foreach(var id in zooTextures.Keys.Where(id=>!visible.Contains(id)).ToArray()){
                zooAnimals[id].texture=null;Resources.UnloadAsset(zooTextures[id]);zooTextures.Remove(id);zooSamples.Remove(id);
            }
            foreach(var a in z.animals){
                var info=ZooCatalog.Get(a.species);var shown=visible.Contains(a.species);var image=zooAnimals[a.species];image.transform.parent.gameObject.SetActive(shown);
                if(!shown)continue;
                if(!zooTextures.TryGetValue(a.species,out var texture)){texture=Resources.Load<Texture2D>("ZooArt/"+a.species);zooTextures.Add(a.species,texture);image.texture=texture;}
                if(texture==null)continue;
                var now=Time.realtimeSinceStartup;
                if(!zooSamples.TryGetValue(a.species,out var sample) || sample.sequence!=a.sequence || sample.age!=a.age){sample=(a.sequence,a.age,now);zooSamples[a.species]=sample;}
                var extra=Shared?Math.Max(0,now-sample.sampled):0;var point=ZooLayout.Point(a,extra);var root=(RectTransform)image.transform.parent;
                root.anchoredPosition=ToBoard(point.X,point.Y);root.localScale=Vector3.one*sceneScale;
                var moving=a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach;
                var frame=moving && a.age+extra<a.duration?(int)((a.age+extra)*5)%4:a.phase==ZooPhase.Notice?5:a.phase==ZooPhase.Eat?((a.age+extra)<1.4?6:7):a.phase==ZooPhase.Browse?(a.species=="gecko"?4:5):a.phase==ZooPhase.Drink?4:4;
                // The legacy reaching trunk extends across its atlas cell. The
                // intact curled-trunk pose holds food at the same fitted socket.
                if(a.species=="elephant" && a.phase==ZooPhase.Eat)frame=7;
                if(moving && info.habitat==ZooHabitat.LandWater)frame=(int)((a.age+extra)*4)%2+(point.Y>=370?2:0);
                var inset=(a.species=="elephant"?9f:4f)/texture.width;
                image.uvRect=new Rect(frame%4*.25f+inset,frame<4?.5f:0,.25f-2*inset,.5f);
                image.rectTransform.sizeDelta=new Vector2(info.size*(1-8*inset),info.size);
                var left=moving && a.toX<a.fromX;
                var breathing=moving?1:1+(float)Math.Sin((a.age+extra)*2+a.random%17)*.006f;
                image.rectTransform.localScale=new Vector3(left?-1:1,breathing,1);
                image.rectTransform.anchoredPosition=new Vector2(0,info.footOffset+(info.habitat==ZooHabitat.Tank?(float)Math.Sin((a.age+extra)*2)*3:0));
            }
            foreach(var info in ZooCatalog.All){
                var shown=visible.Contains(info.id);Place(zooBuckets[info.id],shown,ZooLayout.BucketX(info.id));
                var animal=z.animals.Single(a=>a.species==info.id);
                zooSigns[info.id].text=info.name+(animal.owner==""?"\nTap the "+info.FoodName:animal.owner==Actor?"\nComing for your food":"\nTaking turns");
                for(var i=0;i<4;i++)Place(zooRails[info.id][i],shown,ZooLayout.SlotX(info.id,i));
            }
            foreach(var f in z.food){
                var r=zooLeaves[f.actor];var a=z.animals.FirstOrDefault(v=>v.owner==f.actor);var shown=f.species!="" && visible.Contains(f.species) && a?.consumed!=true;r.gameObject.SetActive(shown);
                if(!shown)continue;
                var info=ZooCatalog.Get(f.species);foreach(Transform child in r)child.gameObject.SetActive(child.name=="Food "+(int)info.food);
                var p=ReadPlayer(f.actor);r.anchoredPosition=ToBoard(p.x,p.y)+new Vector2(65,f.offered?info.FeedHeight:70)*sceneScale;r.localScale=Vector3.one*sceneScale;
            }
            TickZooAudio(z,visible);SortDepth();
        }
        private void AddZooDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var r in zooObjects){var ground=r.anchoredPosition.y;var part=0;
                if(r.name.StartsWith("Zoo held portion")){ground-=260*sceneScale;part=4;}
                add(r,ground,part,r.name);
            }
        }
        private void ResetZoo()
        {
            ResetZooAudio();foreach(var t in zooGateTextures.Values)if(t!=null)Resources.UnloadAsset(t);zooGateTextures.Clear();zooGatePictures.Clear();
            foreach(var r in zooObjects)if(r!=null)Destroy(r.gameObject);zooObjects.Clear();
            foreach(var t in zooTextures.Values)if(t!=null)Resources.UnloadAsset(t);zooTextures.Clear();zooAnimals.Clear();zooSamples.Clear();zooLeaves.Clear();zooSigns.Clear();zooBuckets.Clear();zooRails.Clear();
            zooEntrance=null;zooExit=null;zooNext=null;zooPrevious=null;zooApproach=false;
        }
    }
}
