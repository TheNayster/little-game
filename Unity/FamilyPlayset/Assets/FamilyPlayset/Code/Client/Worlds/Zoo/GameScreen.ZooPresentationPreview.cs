#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
 public sealed partial class GameScreen
 {
  // Editor-only still-image fixture for the actual Zoo renderer. It has no
  // Start/Update, event system, store, network, simulated input or commands.
  // The fixture is never used by a player or written to a save.
  public static GameScreen CreateZooPresentationPreview(string species,string pose,int width,int height)
  {
   var go=new GameObject("Zoo presentation still fixture");var screen=go.AddComponent<GameScreen>();screen.enabled=false;
   screen.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");screen.rounded=screen.Shape(false);screen.circle=screen.Shape(true);screen.hintRing=screen.Ring();
   var cv=new GameObject("Zoo preview canvas",typeof(Canvas));cv.transform.SetParent(go.transform,false);screen.ownedCanvas=cv;
   var canvas=cv.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;
   var cameraRoot=new GameObject("Zoo presentation camera",typeof(Camera));cameraRoot.transform.SetParent(go.transform,false);var camera=cameraRoot.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=height/2f;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Cream;canvas.worldCamera=camera;canvas.planeDistance=10;
   var target=new RenderTexture(width,height,24);target.Create();camera.targetTexture=target;
   screen.safe=screen.Rect(cv.transform,"Safe Area",Vector2.zero,new Vector2(width,height));
   screen.Board=screen.Panel(screen.safe,"Zoo",Vector2.zero,new Vector2(width,height),Cream,false).rectTransform;
   screen.menu=new GameObject("Closed preview menu");screen.menu.transform.SetParent(go.transform,false);screen.menu.SetActive(false);
   screen.message=screen.Label(screen.safe,"Tap to walk",21,new Vector2(width/2-270,height/2-52),new Vector2(200,48));screen.message.gameObject.SetActive(false);
   screen.stick=screen.Rect(screen.safe,"Inactive preview joystick",Vector2.zero,Vector2.zero);screen.stick.gameObject.SetActive(false);
   screen.musicMuted=true;screen.familyTestMuted=true;
   screen.SetZooPresentationPreview(species,pose,width,height,true);
   return screen;
  }
  public void SetZooPresentationPreview(string species,string pose,int width,int height,bool build=false)
  {
   var info=ZooCatalog.Get(species);var state=GameWorld.WithDinosaurWorld(GameWorld.Create("zoo-render-preview")).Snapshot();
   var player=state.players[0];player.zone=info.area;player.x=info.Center;player.y=100;
   var animal=state.zoo.animals.First(a=>a.species==species);animal.fromX=animal.toX=info.Center;animal.fromY=animal.toY=320;animal.phase=ZooPhase.Rest;animal.age=1;animal.duration=5;
   var activity=state.zoo.Activity(species);
   if(pose=="play"){animal.phase=ZooPhase.Splash;animal.fromX=animal.toX=info.PlayX;animal.age=.65;animal.duration=1.8;activity.waterSequence=1;activity.waterAge=.5;}
   if(pose=="care"){animal.phase=ZooPhase.Care;animal.age=1;animal.duration=30;activity.careMembers=new[]{player.id};activity.careProgress=new[]{1,2,3};activity.carePatch[0]=1;activity.careBrushAge[0]=.22;}
   if(pose=="discoveries"){activity.surpriseSequence=new[]{1,1};activity.surpriseAge=new[]{1.3,1.3};animal.phase=ZooPhase.Greet;animal.duration=2.2;}
   if(pose=="snack"){var food=state.zoo.food.First(f=>f.actor==player.id);food.preparing=true;food.prepSpecies=species;food.prepEpoch=1;food.edit=2;food.pieces=new[]{(int)info.food,(int)info.food};}
   // Construct an ephemeral drawing snapshot without Restore's interruption
   // normalization; the source renderer sees these declared still-image poses.
   World=(GameWorld)Activator.CreateInstance(typeof(GameWorld),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{state},null);Actor=player.id;cameraX=info.Center;sceneScale=height/800f;
   var scenery=Board.Find("Illustrated world") as RectTransform;
   if(scenery==null)scenery=Rect(Board,"Illustrated world",Vector2.zero,new Vector2(width,height));
   else for(var index=scenery.childCount-1;index>=0;index--)UnityEngine.Object.DestroyImmediate(scenery.GetChild(index).gameObject);
   scenery.SetAsFirstSibling();
   foreach(var tile in SceneTiles.Where(t=>t.area==info.area)){
    var texture=WorldResources.Load<Texture2D>("Scenery/"+tile.id);if(texture==null)continue;
    var panel=Rect(scenery,tile.id,new Vector2((tile.start+1200-cameraX)*sceneScale,0),new Vector2(2480*sceneScale,height));var image=panel.gameObject.AddComponent<SceneryPanel>();image.texture=texture;image.raycastTarget=false;image.blendLeadingEdge=tile.start>0;
   }
   if(build){
    DrawAvatar();characterVisual.Select(player.avatar);BuildZoo();
    var controls=Rect(safe,"Play controls",new Vector2(width/2-200,height/2-52),Vector2.zero);Panel(controls,"Movement",new Vector2(-85,0),new Vector2(195,61),Cream,false,true);Label(controls,"Tap to walk",21,new Vector2(-85,0),new Vector2(180,50));Panel(controls,"Menu",new Vector2(99,0),new Vector2(145,61),Cream,false,true);Label(controls,"Menu",21,new Vector2(99,0),new Vector2(120,50));
   }
   avatar.anchoredPosition=ToBoard(player.x,player.y);avatar.localScale=Vector3.one*sceneScale;
   snackPanel.gameObject.SetActive(false);TickZoo();if(pose=="snack"){snackSpecies=species;snackPanel.gameObject.SetActive(true);TickElephantSnack(false);}
   foreach(var graphic in GetComponentsInChildren<Graphic>(true))if(graphic.GetComponent<CanvasRenderer>()==null)graphic.gameObject.AddComponent<CanvasRenderer>();
   foreach(var graphic in GetComponentsInChildren<MaskableGraphic>(true))graphic.maskable=false;
   Canvas.ForceUpdateCanvases();
  }
 }
}
#endif
