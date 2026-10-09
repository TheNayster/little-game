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
  // Allocate drawings once; authority sequences start effects, not camera changes.
  private sealed class ZooActivityView
  {
   public RectTransform play,playMotion,playControl,effectsRoot,tool,careRoot,putAway,waiting,finish;
   public Image toolButton,playButton;
   public readonly RectTransform[] discoveries=new RectTransform[2],reveals=new RectTransform[2],helpers=new RectTransform[4],effects=new RectTransform[12],cleanEffects=new RectTransform[9];
   public readonly RectTransform[][] revealParts=new RectTransform[2][];
   public readonly List<RectTransform> foliage=new List<RectTransform>();
   public readonly List<RectTransform>[] hiding={new List<RectTransform>(),new List<RectTransform>()};
   public readonly Image[] dust=new Image[3];public readonly VetTouchSurface[] patches=new VetTouchSurface[3];public readonly Vector2[] patchPoints=new Vector2[3];
   public long gesture;public float nextStroke,nextPlay,localStrokeAt=-10;public int localPatch;public bool sending;
   public int waterSeen=-1;public double waterAge;public float waterSampleAt;
   public readonly int[] discoverySeen={-1,-1};public readonly double[] discoveryAge={10,10},brushAge={10,10,10,10};public readonly float[] discoverySampleAt=new float[2],brushSampleAt=new float[4];
  }
  private readonly Dictionary<string,ZooActivityView> zooActivityViews=new Dictionary<string,ZooActivityView>();
  private void ActivityLeaf(Transform parent,Vector2 at,float scale=1)
  {
   var leaf=Panel(parent,"Leaf outline",at,new Vector2(51,25)*scale,new Color(.25f,.43f,.23f),false,true);leaf.rectTransform.localRotation=Quaternion.Euler(0,0,25);
   var face=Panel(leaf.transform,"Leaf",Vector2.zero,new Vector2(45,19)*scale,new Color(.46f,.69f,.31f),false,true);Plain(face.transform,"Vein",Vector2.zero,new Vector2(34,3)*scale,new Color(.77f,.86f,.5f));
  }
  private void ActivityTool(Transform parent,string tool,float scale)
  {
   if(tool=="brush"){DrawElephantBrush(parent,Vector2.zero,scale);return;}
   if(tool.Contains("cloth")){
    Panel(parent,"Cloth outline",Vector2.zero,new Vector2(64,48)*scale,Ink,false,true);var cloth=Panel(parent,"Folded cleaning cloth",Vector2.zero,new Vector2(58,42)*scale,new Color(.49f,.76f,.84f),false,true);
    Plain(cloth.transform,"Cloth fold",new Vector2(5,-7)*scale,new Vector2(39,4)*scale,Cream);for(var i=0;i<3;i++)Plain(cloth.transform,"Stitched edge",new Vector2(-18+i*14,15)*scale,new Vector2(7,2)*scale,Cream);
   }else{
    Panel(parent,"Rinse bottle outline",new Vector2(0,-3)*scale,new Vector2(43,54)*scale,Ink,false,true);Panel(parent,"Gentle rinse bottle",new Vector2(0,-3)*scale,new Vector2(37,48)*scale,new Color(.42f,.76f,.85f),false,true);
    Plain(parent,"Rinse nozzle",new Vector2(15,28)*scale,new Vector2(35,13)*scale,new Color(.3f,.57f,.63f));Panel(parent,"Bottle drop picture",new Vector2(0,-3)*scale,new Vector2(12,18)*scale,Cream,false,true);
   }
  }
  private void ZooActivityAction(string id,string op,int discovery=-1)
  {
   if(!CareInputReady || ElephantSnackOpen || ZooPhotoOpen)return;
   var info=ZooCatalog.Get(id);var p=ReadPlayer(Actor);var target=discovery<0?id:id+"/"+discovery;
   // A distant prop retains its species. Use normal walking and authority guards.
   if(p.zone==info.area && p.x>=info.Center-1000 && p.x<info.Center+1000)SendZoo(op,target,_=>{});
   else ZooWalk(op,target,new Vector2(info.Center,100));
  }
  private void BuildZooActivities()
  {
   foreach(var info in ZooCatalog.All.Where(v=>v.id!="elephant")){
    var id=info.id;var v=new ZooActivityView();zooActivityViews.Add(id,v);
    v.play=ZooObject(info.name+" "+info.PlayName);v.playMotion=Rect(v.play,"Habitat response",Vector2.zero,Vector2.zero);v.effectsRoot=ZooObject(info.name+" habitat effects");DrawHabitatActivity(v,info);
    v.playControl=ZooObject(info.name+" play control");
    v.playButton=Panel(v.playControl,"Start "+info.PlayName,new Vector2(-72,62),Vector2.one*76,new Color(.99f,.84f,.49f),true,true);DrawActivitySymbol(v.playButton.transform,info);
    NavButton(v.playButton,()=>{if(Time.realtimeSinceStartup<v.nextPlay)return;v.nextPlay=Time.realtimeSinceStartup+1.2f;ZooActivityAction(id,"water");});
    HomeHit(v.play,info.PlayName,new Vector2(66,105),new Vector2(180,190),()=>ZooActivityAction(id,"water"),false);
    v.tool=ZooObject(info.name+" care tools");Panel(v.tool,"Care basket shadow",new Vector2(0,-4),new Vector2(116,18),new Color(.32f,.36f,.24f,.2f),false,true);
    v.toolButton=Panel(v.tool,"Choose "+info.name+" "+info.CareTool,new Vector2(0,64),new Vector2(114,120),Cream,true,true);
    Panel(v.toolButton.transform,"Woven tool basket",new Vector2(0,-34),new Vector2(94,39),new Color(.72f,.53f,.32f),false,true);for(var k=0;k<5;k++)Plain(v.toolButton.transform,"Basket weave",new Vector2((k-2)*17,-34),new Vector2(3,28),new Color(.88f,.71f,.47f));
    ActivityTool(v.toolButton.transform,info.CareTool,1);NavButton(v.toolButton,()=>ZooActivityAction(id,"care"));
    var put=Panel(v.tool,"Put "+info.name+" care tool away",new Vector2(0,188),new Vector2(104,82),Cream,true,true);v.putAway=put.rectTransform;
    Panel(put.transform,"Empty basket",new Vector2(0,-17),new Vector2(65,25),new Color(.72f,.53f,.32f),false,true);var arrow=Rect(put.transform,"Put-away arrow",new Vector2(0,15),Vector2.zero);ZooArrow(arrow,Vector2.zero,1);arrow.localRotation=Quaternion.Euler(0,0,-90);NavButton(put,()=>ZooActivityAction(id,"put-brush"));
    v.waiting=Rect(v.tool,"Care waiting picture",new Vector2(-72,80),Vector2.zero);Panel(v.waiting,"Waiting clock",Vector2.zero,Vector2.one*49,new Color(.8f,.88f,.74f),false,true);Plain(v.waiting,"Clock hand",new Vector2(0,6),new Vector2(4,20),Ink);Plain(v.waiting,"Clock hand across",new Vector2(7,0),new Vector2(18,4),Ink);
    var animalRoot=(RectTransform)zooAnimals[id].transform.parent;v.careRoot=info.HabitatCare?ZooObject(info.name+" habitat care"):Rect(animalRoot,"Gentle care targets",Vector2.zero,Vector2.zero);
    if(info.HabitatCare)DrawCareHabitat(v.careRoot,info);
    var size=savannaFeeding.TryGetValue(id,out var feeding)?feeding.size:id=="brachiosaurus"?BrachiosaurusSize:info.size;
    for(var i=0;i<3;i++){
     var patch=i;var point=CarePatchPoint(info,size,i);v.patchPoints[i]=point;
     var area=Panel(v.careRoot,"Care picture "+i,point,info.HabitatCare?new Vector2(78,100):new Vector2(size*.23f,size*.19f),new Color(0,0,0,0),true,true);
     v.dust[i]=Panel(area.transform,"Soft cleaning patch",Vector2.zero,info.HabitatCare?new Vector2(57,54):new Vector2(size*.18f,size*.12f),new Color(.78f,.66f,.45f,.6f),false,true);
     for(var k=0;k<4;k++)Panel(v.dust[i].transform,"Dust speck",new Vector2((k%2==0?-1:1)*15,(k<2?-1:1)*9),new Vector2(9,6),new Color(.93f,.81f,.58f,.7f),false,true);
     var surface=area.gameObject.AddComponent<VetTouchSurface>();v.patches[i]=surface;surface.Stroke=(pointOnSurface,gesture,first)=>{if(first)CareZooPatch(id,patch);};
    }
    for(var i=0;i<4;i++){v.helpers[i]=Rect(v.careRoot,"Helper care tool "+i,Vector2.zero,Vector2.zero);ActivityTool(v.helpers[i],info.CareTool,.72f);}
    for(var i=0;i<9;i++)v.cleanEffects[i]=CareSpark(v.careRoot,i,info.CareTool=="rinse");v.finish=Rect(v.careRoot,"Care finish shine",Vector2.zero,Vector2.zero);DrawCareFinish(v.finish);
    for(var i=0;i<2;i++){
     var slot=i;var name=info.Discovery(i);var prop=v.discoveries[i]=ZooObject(info.name+" discovery "+name);DrawDiscoveryHiding(v,info,i);
     v.reveals[i]=Rect(prop,name,new Vector2(0,48),Vector2.zero);DrawZooDiscovery(v.reveals[i],info.id,i);v.revealParts[i]=v.reveals[i].GetComponentsInChildren<RectTransform>(true);foreach(var cover in v.hiding[i])cover.SetAsLastSibling();
     HomeHit(prop,"Discover "+name,new Vector2(0,56),new Vector2(156,152),()=>{if(Zoo.Activity(id).surpriseAge[slot]>=ZooLayout.SurpriseReset)ZooActivityAction(id,"surprise",slot);},false);
    }
   }
  }
  private void CareZooPatch(string id,int patch)
  {
   var v=zooActivityViews[id];var h=Zoo?.Activity(id);var a=Zoo?.animals.First(animal=>animal.species==id);
   if(!CareInputReady || ElephantSnackOpen || ZooPhotoOpen || h==null || !h.careMembers.Contains(Actor) || h.careComplete || a.phase!=ZooPhase.Care || Time.realtimeSinceStartup<v.nextStroke || v.sending)return;
   v.nextStroke=Time.realtimeSinceStartup+.35f;v.localPatch=patch;v.localStrokeAt=Time.realtimeSinceStartup;v.sending=Shared;
   var token=h.careSession+"/"+h.careMemberEpoch[Array.IndexOf(h.careMembers,Actor)]+"/"+patch+"/"+(++v.gesture);
   void Done(SoloResult result){v.sending=false;if(!result.Accepted)v.localStrokeAt=-10;}
   if(Shared){if(!SubmitShared(SoloAction.Zoo,token,id,"brush",0,0,Done))v.sending=false;}else Done(Command(SoloAction.Zoo,item:token,target:id,value:"brush"));
  }
  private void TickZooActivities(ZooState z)
  {
   var now=Time.realtimeSinceStartup;
   foreach(var pair in zooActivityViews){
    var id=pair.Key;var v=pair.Value;var info=ZooCatalog.Get(id);var h=z.Activity(id);var a=z.animals.First(animal=>animal.species==id);
    var shown=ZooExhibitVisible(info) && !MenuOpen && !ZooMapOpen && !ZooPhotoOpen && !ElephantSnackOpen;var own=h.careMembers.Contains(Actor);var care=shown && a.phase==ZooPhase.Care && !h.careComplete && !applicationPaused;
    v.play.gameObject.SetActive(shown && !own);v.playControl.gameObject.SetActive(shown && !own);v.effectsRoot.gameObject.SetActive(shown && !own);v.tool.gameObject.SetActive(shown);v.putAway.gameObject.SetActive(own);v.waiting.gameObject.SetActive(own && !care && !h.careComplete);v.waiting.Find("Clock hand").localRotation=Quaternion.Euler(0,0,-now*120);
    v.play.anchoredPosition=ToBoard(info.PlayPropX,info.ActivityY);v.play.localScale=Vector3.one*sceneScale;v.effectsRoot.anchoredPosition=v.play.anchoredPosition;v.effectsRoot.localScale=Vector3.one*sceneScale;v.playControl.anchoredPosition=ToBoard(info.PlayPropX,100);v.playControl.localScale=Vector3.one*sceneScale;v.tool.anchoredPosition=ToBoard(info.ToolX,100);v.tool.localScale=Vector3.one*sceneScale;v.toolButton.color=own?new Color(1,.85f,.46f):Cream;
    if(v.waterSeen!=h.waterSequence || v.waterAge!=h.waterAge){if(v.waterSeen>=0 && v.waterSeen!=h.waterSequence && shown && h.waterAge<1.2 && !applicationPaused)ZooHabitatSound(info);v.waterSeen=h.waterSequence;v.waterAge=h.waterAge;v.waterSampleAt=now;}
    var waterAge=h.waterAge+(Shared?Math.Max(0,now-v.waterSampleAt):0);var animalAge=a.age+(Shared && zooSamples.TryGetValue(id,out var sample)?Math.Max(0,now-sample.sampled):0);
    var playAge=waterAge<1.4?waterAge:a.phase==ZooPhase.Splash?animalAge:10;AnimateHabitat(v,info,(float)playAge,shown && !applicationPaused && playAge<1.8,a,animalAge);v.playButton.color=h.waterCooldown>0?new Color(.77f,.91f,.91f):new Color(.99f,.84f,.49f);
    if(info.HabitatCare){v.careRoot.anchoredPosition=ToBoard(info.Center-350,info.ActivityY);v.careRoot.localScale=Vector3.one*sceneScale;}v.careRoot.gameObject.SetActive(shown && (care || a.phase==ZooPhase.CareFinish));
    for(var i=0;i<3;i++){
     v.patches[i].gameObject.SetActive(care && h.careProgress[i]<4);v.patches[i].GetComponent<Image>().raycastTarget=own && CareInputReady;var remaining=1-h.careProgress[i]/4f;v.dust[i].color=new Color(.78f,.66f,.45f,.62f*remaining);
     foreach(Transform dot in v.dust[i].transform)dot.GetComponent<Image>().color=new Color(.93f,.81f,.58f,.7f*remaining);
    }
    var newest=10f;var recentPatch=0;
    for(var i=0;i<4;i++){
     var helper=v.helpers[i];helper.gameObject.SetActive(care && i<h.careMembers.Length);if(v.brushAge[i]!=h.careBrushAge[i]){v.brushAge[i]=h.careBrushAge[i];v.brushSampleAt[i]=now;}if(i>=h.careMembers.Length)continue;
     var age=(float)(h.careBrushAge[i]+(Shared?Math.Max(0,now-v.brushSampleAt[i]):0));var patch=Mathf.Clamp(h.carePatch[i],0,2);if(h.careMembers[i]==Actor && now-v.localStrokeAt<.65f){patch=v.localPatch;age=now-v.localStrokeAt;}if(age<newest){newest=age;recentPatch=patch;}
     var sweep=age<.65f?Mathf.Sin(age/.65f*Mathf.PI*4)*23:0;helper.anchoredPosition=v.patchPoints[patch]+new Vector2((i-1.5f)*23+sweep,-29);helper.localRotation=Quaternion.Euler(0,0,(i-1.5f)*7+sweep*.4f);
    }
    for(var i=0;i<v.cleanEffects.Length;i++){var effect=v.cleanEffects[i];effect.gameObject.SetActive(care && newest<.65f);var t=Mathf.Clamp01(newest/.65f);var angle=i*Mathf.PI*2/9;effect.anchoredPosition=v.patchPoints[recentPatch]+new Vector2(Mathf.Cos(angle)*(12+t*35),Mathf.Sin(angle)*(10+t*23)+t*20);effect.localScale=Vector3.one*(1-t);effect.localRotation=Quaternion.Euler(0,0,t*70);}
    var finish=shown && a.phase==ZooPhase.CareFinish && h.careComplete;v.finish.gameObject.SetActive(finish);v.finish.anchoredPosition=info.HabitatCare?new Vector2(0,105):v.patchPoints[1]+new Vector2(0,50);v.finish.localScale=Vector3.one*(finish?Mathf.Sin(Mathf.Clamp01((float)animalAge/2)*Mathf.PI):0);
    for(var i=0;i<2;i++){
     var prop=v.discoveries[i];prop.gameObject.SetActive(shown && !own);prop.anchoredPosition=ToBoard(info.Center+(i==0?-480:480),460);prop.localScale=Vector3.one*sceneScale;
     if(v.discoverySeen[i]!=h.surpriseSequence[i] || v.discoveryAge[i]!=h.surpriseAge[i]){if(v.discoverySeen[i]>=0 && v.discoverySeen[i]!=h.surpriseSequence[i] && shown && h.surpriseAge[i]<.8 && !applicationPaused)PlaySurpriseSound(info.habitat==ZooHabitat.Tank?1:i);v.discoverySeen[i]=h.surpriseSequence[i];v.discoveryAge[i]=h.surpriseAge[i];v.discoverySampleAt[i]=now;}
     var age=h.surpriseAge[i]+(Shared?Math.Max(0,now-v.discoverySampleAt[i]):0);AnimateZooDiscovery(v,info,i,(float)age,h.surpriseSequence[i]>0 && age<6);
    }
   }
  }
  private void ZooHabitatSound(ZooSpecies info){if(info.PlayKind=="mist" || info.PlayKind=="current" || info.PlayKind=="water")ZooWaterSound();else PlaySurpriseSound(1);}
  private void PoseZooPersonality(RawImage image,ZooAnimal a,double age)
  {
   if(a.species=="elephant" || a.owner!="")return;var greeting=a.phase==ZooPhase.Greet || a.phase==ZooPhase.CareFinish;var playing=a.phase==ZooPhase.Curious || a.phase==ZooPhase.Splash;var wave=Mathf.Sin(Mathf.Clamp01((float)(age/a.duration))*Mathf.PI);var clock=(float)age;var idle=a.phase==ZooPhase.Rest || a.phase==ZooPhase.Browse || a.phase==ZooPhase.Drink;
   if(image is SavannaArtView articulated){if(a.species=="giraffe")articulated.Pose(playing?-12*wave:greeting?7*Mathf.Sin(clock*4)*wave:idle?3+2*Mathf.Sin(clock*1.3f):0);else{articulated.Pose(playing?9*wave:greeting?5*Mathf.Sin(clock*4)*wave:idle?2*Mathf.Max(0,Mathf.Sin(clock*1.1f)):0);articulated.PawPose(a.phase==ZooPhase.Splash?-13*wave:0);}}
   else if(image is BrachiosaurusArtView neck)neck.Pose(playing?-8*wave:greeting?5*wave:idle?2*Mathf.Max(0,Mathf.Sin(clock)):0);else if(image is ZooArtView body)body.Pose(a.species,a.phase,clock,wave);
   if(a.species=="zebra" && a.phase==ZooPhase.Splash)image.rectTransform.anchoredPosition+=new Vector2(Mathf.Sin(clock*7)*5*wave,0);
   if(a.species=="penguin" && (greeting || playing)){image.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*5)*5*wave);image.rectTransform.anchoredPosition+=new Vector2(Mathf.Sin(clock*3)*7*wave,3*wave);}
  }
 }
}
