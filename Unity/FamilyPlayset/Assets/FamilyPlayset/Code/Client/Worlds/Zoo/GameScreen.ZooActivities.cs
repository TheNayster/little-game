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
        // One bounded presentation per exhibit. All geometry is editable native
        // UI, allocated once; snapshots own progress and event timing.
        private sealed class ZooActivityView
        {
            public RectTransform play,playMotion,tool,careRoot;
            public Image toolButton;
            public readonly RectTransform[] discoveries=new RectTransform[2],reveals=new RectTransform[2],helpers=new RectTransform[4];
            public readonly Image[] dust=new Image[3];
            public readonly VetTouchSurface[] patches=new VetTouchSurface[3];
            public readonly Vector2[] patchPoints=new Vector2[3];
            public long gesture;public float nextStroke;
            public int waterSeen=-1;public double waterAge;public float waterSampleAt;
            public readonly int[] discoverySeen={-1,-1};
            public readonly double[] discoveryAge={10,10},brushAge={10,10,10,10};
            public readonly float[] discoverySampleAt=new float[2],brushSampleAt=new float[4];
        }
        private readonly Dictionary<string,ZooActivityView> zooActivityViews=new Dictionary<string,ZooActivityView>();
        private void ActivityLeaf(Transform parent,Vector2 at,float scale=1)
        {var leaf=Panel(parent,"Leaf",at,new Vector2(48,22)*scale,new Color(.41f,.65f,.31f),false,true);leaf.rectTransform.localRotation=Quaternion.Euler(0,0,25);Plain(leaf.transform,"Vein",Vector2.zero,new Vector2(34,3)*scale,new Color(.74f,.83f,.47f));}
        private void ActivityTool(Transform parent,string tool,float scale)
        {
            if(tool=="brush"){DrawElephantBrush(parent,Vector2.zero,scale);return;}
            if(tool.Contains("cloth")){
                Panel(parent,"Soft cleaning cloth",Vector2.zero,new Vector2(59,43)*scale,new Color(.49f,.76f,.84f),false,true);
                Plain(parent,"Cloth fold",new Vector2(5,-7)*scale,new Vector2(39,4)*scale,Cream);
            }else{
                Panel(parent,"Gentle rinse bottle",new Vector2(0,-3)*scale,new Vector2(38,49)*scale,new Color(.42f,.76f,.85f),false,true);
                Plain(parent,"Rinse nozzle",new Vector2(15,28)*scale,new Vector2(35,13)*scale,new Color(.3f,.57f,.63f));
                for(var i=0;i<3;i++)Panel(parent,"Rinse drop "+i,new Vector2(31+i*9,11-i*10)*scale,new Vector2(6,9)*scale,new Color(.6f,.85f,.94f),false,true);
            }
        }
        private void BuildZooActivities()
        {
            foreach(var info in ZooCatalog.All.Where(v=>v.id!="elephant")){
                var id=info.id;var view=new ZooActivityView();zooActivityViews.Add(id,view);
                view.play=ZooObject(info.name+" "+info.PlayName);
                view.playMotion=Rect(view.play,"Habitat response",new Vector2(0,60),Vector2.zero);
                DrawHabitatActivity(view.playMotion,info);
                HomeHit(view.play,info.PlayName,new Vector2(0,72),new Vector2(170,165),()=>{if(CareInputReady && !ElephantSnackOpen && !ZooPhotoOpen)SendZoo("water",id,_=>{});},false);
                view.tool=ZooObject(info.name+" care tools");
                view.toolButton=Panel(view.tool,"Choose "+info.CareTool,new Vector2(0,70),new Vector2(110,105),Cream,true,true);
                ActivityTool(view.toolButton.transform,info.CareTool,1);
                NavButton(view.toolButton,()=>{if(CareInputReady && !ElephantSnackOpen && !ZooPhotoOpen)SendZoo(Zoo.Activity(id).careMembers.Contains(Actor)?"put-brush":"care",id,_=>{});});
                var animalRoot=(RectTransform)zooAnimals[id].transform.parent;
                view.careRoot=info.HabitatCare?ZooObject(info.name+" habitat cleaning pane"):Rect(animalRoot,"Gentle care targets",Vector2.zero,Vector2.zero);
                if(info.HabitatCare){
                    Panel(view.careRoot,info.habitat==ZooHabitat.Tank?"Tank glass":"Habitat stone",new Vector2(0,78),new Vector2(214,135),info.habitat==ZooHabitat.Tank?new Color(.57f,.82f,.88f,.3f):new Color(.69f,.64f,.51f),false,true);
                }
                var size=savannaFeeding.TryGetValue(id,out var feeding)?feeding.size:id=="brachiosaurus"?BrachiosaurusSize:info.size;
                for(var i=0;i<3;i++){
                    var patch=i;var point=info.HabitatCare?new Vector2((i-1)*66,80):new Vector2((i-1)*size*.17f,info.footOffset+size*(i==1?.36f:.27f));view.patchPoints[i]=point;
                    var area=Panel(view.careRoot,"Care picture "+i,point,info.HabitatCare?new Vector2(68,108):new Vector2(size*.23f,size*.18f),new Color(0,0,0,0),true,true);
                    view.dust[i]=Panel(area.transform,"Gentle cleaning patch",Vector2.zero,info.HabitatCare?new Vector2(53,65):new Vector2(size*.18f,size*.12f),new Color(.78f,.66f,.45f,.6f),false,true);
                    var surface=area.gameObject.AddComponent<VetTouchSurface>();view.patches[i]=surface;
                    surface.Stroke=(pointOnSurface,gesture,first)=>{if(first)CareZooPatch(id,patch);};
                }
                for(var i=0;i<4;i++){view.helpers[i]=Rect(view.careRoot,"Helper tool "+i,Vector2.zero,Vector2.zero);ActivityTool(view.helpers[i],info.CareTool,.6f);}
                for(var i=0;i<2;i++){
                    var slot=i;var name=info.Discovery(i);var prop=view.discoveries[i]=ZooObject(info.name+" hidden "+name);
                    var aquatic=info.habitat==ZooHabitat.Tank;
                    Panel(prop,aquatic?"Reef hiding rock":"Discovery hiding stone",new Vector2(0,28),new Vector2(96,56),aquatic?new Color(.52f,.7f,.72f):new Color(.64f,.64f,.45f),false,true);
                    if(aquatic){for(var k=0;k<3;k++)ActivityLeaf(prop,new Vector2((k-1)*24,35+k*13),.65f);}else ActivityLeaf(prop,new Vector2(-32,40),.85f);
                    view.reveals[i]=Rect(prop,name,new Vector2(0,48),Vector2.zero);DrawZooDiscovery(view.reveals[i],name);
                    HomeHit(prop,"Discover "+name,new Vector2(0,56),new Vector2(138,130),()=>{
                        if(!CareInputReady || ElephantSnackOpen || ZooPhotoOpen || Zoo.Activity(id).surpriseAge[slot]<ZooLayout.SurpriseReset)return;
                        SendZoo("surprise",id+"/"+slot,_=>{});
                    },false);
                }
            }
        }
        private void DrawHabitatActivity(RectTransform root,ZooSpecies info)
        {
            var kind=info.PlayKind;
            if(info.id=="zebra"){
                var log=Panel(root,"Scratch log",Vector2.zero,new Vector2(129,49),new Color(.65f,.45f,.27f),false,true);
                for(var i=0;i<4;i++)Plain(log.transform,"Bark line",new Vector2((i-1.5f)*25,0),new Vector2(4,31),new Color(.47f,.32f,.22f));
            }else if(kind=="browse"){
                Plain(root,"Rooted branch",new Vector2(0,24),new Vector2(16,119),new Color(.56f,.41f,.26f));
                for(var i=0;i<5;i++)ActivityLeaf(root,new Vector2((i%2==0?-1:1)*30,12+i*17),info.id=="brachiosaurus"?1.1f:.8f);
            }else if(kind=="toy"){
                var toy=Panel(root,info.id=="lion"?"Enrichment cylinder":"Scent roll",Vector2.zero,new Vector2(106,61),new Color(.81f,.64f,.4f),false,true);
                for(var i=0;i<3;i++)Plain(toy.transform,"Toy stripe",new Vector2((i-1)*24,0),new Vector2(7,42),new Color(.52f,.69f,.57f));
            }else if(kind=="current"){
                Panel(root,"Reef current shell",Vector2.zero,new Vector2(115,54),new Color(.65f,.75f,.86f),false,true);
                for(var i=0;i<6;i++)Panel(root,"Bubble "+i,new Vector2((i%3-1)*28,25+i*10),Vector2.one*(10+i%3*4),new Color(.79f,.94f,.98f,.75f),false,true);
            }else if(info.id=="gecko"){
                Panel(root,"Warm rock",Vector2.zero,new Vector2(123,52),new Color(.75f,.64f,.42f),false,true);
                var shade=Panel(root,"Shade leaf",new Vector2(0,74),new Vector2(123,39),new Color(.54f,.69f,.36f),false,true);Plain(shade.transform,"Leaf vein",Vector2.zero,new Vector2(91,4),Cream);
            }else{
                Panel(root,kind=="water"?"Splashing water":"Misted basking stone",Vector2.zero,new Vector2(132,43),kind=="water"?new Color(.48f,.77f,.87f):new Color(.73f,.7f,.53f),false,true);
                if(info.id=="iguana" || info.id=="tortoise")ActivityLeaf(root,new Vector2(-40,18),1);
                for(var i=0;i<5;i++)Panel(root,"Gentle drop "+i,new Vector2((i-2)*24,36+i%2*19),new Vector2(9,14),new Color(.64f,.86f,.94f),false,true);
            }
            // The fixed symbol is always visible and is part of the habitat prop.
            var tap=Panel(root,"Play picture",new Vector2(-66,87),Vector2.one*42,new Color(.99f,.84f,.49f),false,true);ZooArrow(tap.transform,Vector2.zero,1);tap.transform.localScale=Vector3.one*.7f;
        }
        private void DrawZooDiscovery(RectTransform root,string name)
        {
            var lower=name.ToLowerInvariant();
            if(lower.Contains("shell") || lower.Contains("snail")){
                Panel(root,"Shell outline",Vector2.zero,new Vector2(60,46),new Color(.45f,.47f,.4f),false,true);
                Panel(root,"Shell",Vector2.zero,new Vector2(53,40),new Color(.94f,.8f,.57f),false,true);
                for(var i=0;i<3;i++)Plain(root,"Shell marking",new Vector2((i-1)*12,0),new Vector2(3,27),new Color(.72f,.57f,.41f));
                if(lower.Contains("snail")){Panel(root,"Snail body",new Vector2(8,-23),new Vector2(72,14),new Color(.67f,.75f,.43f),false,true);Panel(root,"Snail head",new Vector2(36,-12),Vector2.one*21,new Color(.67f,.75f,.43f),false,true);}
            }else if(lower.Contains("star")){
                for(var i=0;i<5;i++){var ray=Panel(root,"Sea-star arm",Vector2.zero,new Vector2(14,62),new Color(.94f,.67f,.48f),false,true);ray.rectTransform.localRotation=Quaternion.Euler(0,0,i*72);}
                Panel(root,"Sea-star middle",Vector2.zero,Vector2.one*29,new Color(.98f,.75f,.52f),false,true);
            }else if(lower.Contains("crystal") || lower.Contains("amber")){
                for(var i=0;i<3;i++){var crystal=Plain(root,"Crystal",new Vector2((i-1)*22,i==1?8:0),new Vector2(29,42),lower.Contains("amber")?new Color(.98f,.73f,.31f):new Color(.73f,.76f,.94f));crystal.rectTransform.localRotation=Quaternion.Euler(0,0,(i-1)*18+45);}
            }else if(lower.Contains("fern") || lower.Contains("leaf") || lower.Contains("seed") || lower.Contains("cone")){
                Plain(root,"Discovery stem",Vector2.zero,new Vector2(5,70),new Color(.41f,.57f,.27f));
                for(var i=0;i<5;i++)ActivityLeaf(root,new Vector2((i%2==0?-1:1)*17,-20+i*12),.65f);
                if(lower.Contains("pod") || lower.Contains("cone"))Panel(root,"Seed pod",new Vector2(0,34),new Vector2(27,42),new Color(.73f,.52f,.31f),false,true);
            }else if(lower.Contains("nest") || lower.Contains("feather")){
                Panel(root,"Nest",new Vector2(0,-6),new Vector2(70,28),new Color(.67f,.48f,.3f),false,true);
                for(var i=0;i<3;i++){var feather=Panel(root,"Feather",new Vector2((i-1)*20,14),new Vector2(13,48),new Color(.85f,.81f,.65f),false,true);feather.rectTransform.localRotation=Quaternion.Euler(0,0,(i-1)*27);}
            }else if(lower.Contains("footprint")){
                Panel(root,"Footprint stone",Vector2.zero,new Vector2(66,49),new Color(.75f,.66f,.5f),false,true);
                for(var i=0;i<3;i++)Panel(root,"Footprint toe",new Vector2((i-1)*14,10),new Vector2(9,26),new Color(.49f,.46f,.36f),false,true);
            }else{
                var water=lower.Contains("shrimp") || lower.Contains("crab");var frog=lower.Contains("frog");
                Panel(root,"Little visitor body",Vector2.zero,new Vector2(water?53:39,27),frog?new Color(.51f,.72f,.35f):water?new Color(.92f,.62f,.48f):new Color(.68f,.63f,.36f),false,true);
                for(var i=0;i<6;i++){var leg=Plain(root,"Visitor leg",new Vector2((i%3-1)*17,i<3?-18:18),new Vector2(4,21),new Color(.47f,.49f,.3f));leg.rectTransform.localRotation=Quaternion.Euler(0,0,(i%3-1)*30);}
                if(lower.Contains("dragonfly"))for(var i=0;i<2;i++)Panel(root,"Dragonfly wing",new Vector2(i==0?-24:24,5),new Vector2(38,17),new Color(.72f,.89f,.89f),false,true);
                for(var i=-1;i<=1;i+=2)Panel(root,"Visitor eye",new Vector2(i*10,12),Vector2.one*6,Ink,false,true);
            }
        }
        private void CareZooPatch(string id,int patch)
        {
            var view=zooActivityViews[id];var h=Zoo?.Activity(id);var a=Zoo?.animals.First(v=>v.species==id);
            if(!CareInputReady || ElephantSnackOpen || ZooPhotoOpen || h==null || !h.careMembers.Contains(Actor) || h.careComplete || a.phase!=ZooPhase.Care || Time.realtimeSinceStartup<view.nextStroke)return;
            view.nextStroke=Time.realtimeSinceStartup+.35f;
            var token=h.careSession+"/"+h.careMemberEpoch[Array.IndexOf(h.careMembers,Actor)]+"/"+patch+"/"+(++view.gesture);
            if(Shared)SubmitShared(SoloAction.Zoo,token,id,"brush",0,0,_=>{});else Command(SoloAction.Zoo,item:token,target:id,value:"brush");
        }
        private void TickZooActivities(ZooState z)
        {
            var current=ZooCatalog.Trail(CurrentArea)?ZooAtPlayer().id:"";
            foreach(var pair in zooActivityViews){
                var id=pair.Key;var view=pair.Value;var info=ZooCatalog.Get(id);var h=z.Activity(id);var a=z.animals.First(v=>v.species==id);
                var shown=current==id && !MenuOpen && !ZooMapOpen && !ZooPhotoOpen && !ElephantSnackOpen;
                var own=h.careMembers.Contains(Actor);var care=shown && a.phase==ZooPhase.Care && !h.careComplete;
                var holding=z.food.Any(f=>f.actor==Actor && f.species!="");
                view.play.gameObject.SetActive(shown && !own);view.tool.gameObject.SetActive(shown && (!holding || own));
                view.play.anchoredPosition=ToBoard(info.Center-560,info.ActivityY);view.play.localScale=Vector3.one*sceneScale;
                view.tool.anchoredPosition=ToBoard(info.ToolX,180);view.tool.localScale=Vector3.one*sceneScale;view.toolButton.color=own?new Color(1,.85f,.46f):Cream;
                var now=Time.realtimeSinceStartup;
                if(view.waterSeen!=h.waterSequence || view.waterAge!=h.waterAge){view.waterSeen=h.waterSequence;view.waterAge=h.waterAge;view.waterSampleAt=now;}
                var waterAge=h.waterAge+(Shared?Math.Max(0,now-view.waterSampleAt):0);
                var animalAge=a.age+(Shared && zooSamples.TryGetValue(id,out var sample)?Math.Max(0,now-sample.sampled):0);
                var pulse=waterAge<1.2?Mathf.Sin((float)waterAge*Mathf.PI/1.2f):a.phase==ZooPhase.Splash && animalAge<1.8?Mathf.Sin((float)animalAge*Mathf.PI/1.8f):0;
                view.playMotion.localRotation=Quaternion.Euler(0,0,pulse*(info.PlayKind=="toy"?22:info.PlayKind=="browse"?9:3));
                view.playMotion.anchoredPosition=new Vector2(info.PlayKind=="toy"?pulse*40:0,60+(info.PlayKind=="current"?pulse*19:0));
                if(info.HabitatCare){view.careRoot.gameObject.SetActive(care);view.careRoot.anchoredPosition=ToBoard(info.Center-360,info.ActivityY);view.careRoot.localScale=Vector3.one*sceneScale;}
                for(var i=0;i<3;i++){
                    view.patches[i].gameObject.SetActive(care && h.careProgress[i]<4);view.patches[i].GetComponent<Image>().raycastTarget=own && CareInputReady;
                    view.dust[i].color=new Color(.78f,.66f,.45f,.6f*(1-h.careProgress[i]/4f));
                }
                for(var i=0;i<4;i++){
                    var helper=view.helpers[i];helper.gameObject.SetActive(care && i<h.careMembers.Length);
                    if(view.brushAge[i]!=h.careBrushAge[i]){view.brushAge[i]=h.careBrushAge[i];view.brushSampleAt[i]=now;}
                    if(i>=h.careMembers.Length)continue;var age=h.careBrushAge[i]+(Shared?Math.Max(0,now-view.brushSampleAt[i]):0);var sweep=age<.65?Mathf.Sin((float)age/.65f*Mathf.PI*4)*16:0;
                    helper.anchoredPosition=view.patchPoints[h.carePatch[i]]+new Vector2((i-1.5f)*22+sweep,-30);
                }
                for(var i=0;i<2;i++){
                    var prop=view.discoveries[i];prop.gameObject.SetActive(shown && !own);
                    prop.anchoredPosition=ToBoard(info.Center+(i==0?-720:720),info.habitat==ZooHabitat.Tank?405:440);prop.localScale=Vector3.one*sceneScale;
                    if(view.discoverySeen[i]!=h.surpriseSequence[i] || view.discoveryAge[i]!=h.surpriseAge[i]){view.discoverySeen[i]=h.surpriseSequence[i];view.discoveryAge[i]=h.surpriseAge[i];view.discoverySampleAt[i]=now;}
                    var age=h.surpriseAge[i]+(Shared?Math.Max(0,now-view.discoverySampleAt[i]):0);
                    var active=h.surpriseSequence[i]>0 && age<6;var reveal=view.reveals[i];reveal.gameObject.SetActive(active);
                    reveal.anchoredPosition=new Vector2(h.surpriseVariation[i]==0?-9:9,48+(active?Mathf.Sin(Mathf.Clamp01((float)age/.6f)*Mathf.PI/2)*48:0));
                    reveal.localRotation=Quaternion.Euler(0,0,active?Mathf.Sin((float)age*3)*(i==0?6:10):0);
                }
            }
        }
        private void PoseZooPersonality(RawImage image,ZooAnimal a,double age)
        {
            if(a.species=="elephant")return;
            var info=ZooCatalog.Get(a.species);var wave=Mathf.Sin(Mathf.Clamp01((float)(age/a.duration))*Mathf.PI);
            var greeting=a.phase==ZooPhase.Greet || a.phase==ZooPhase.CareFinish;
            var playing=a.phase==ZooPhase.Curious || a.phase==ZooPhase.Splash;
            // Move intact authored drawings, never copy the elephant's trunk mesh.
            if(!greeting && !playing)return;
            var amplitude=info.habitat==ZooHabitat.Tank?5:info.id=="tortoise"?1:info.id=="penguin"?6:info.id=="tyrannosaurus" || info.id=="lion"?3:2;
            image.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin((float)age*(info.speed/20))*amplitude*wave);
            if(info.habitat==ZooHabitat.Tank)image.rectTransform.anchoredPosition+=new Vector2(Mathf.Sin((float)age*2)*16*wave,8*wave);
            else if(info.id=="penguin")image.rectTransform.anchoredPosition+=new Vector2(Mathf.Sin((float)age*4)*6*wave,3*wave);
            else if(info.PlayKind=="browse" && playing)image.rectTransform.anchoredPosition+=new Vector2(-5*wave,-3*wave);
        }
    }
}
