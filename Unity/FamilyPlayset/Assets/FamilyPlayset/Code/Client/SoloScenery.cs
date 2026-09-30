using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private sealed class SceneTile
        {
            public readonly string id,area;
            public readonly float start;
            public SceneTile(string id,string area,float start){this.id=id;this.area=area;this.start=start;}
        }
        private static readonly SceneTile[] SceneTiles={
            new SceneTile("home-secret",SecretRooms.Id(0),0),new SceneTile("home-secret",SecretRooms.Id(1),0),
            new SceneTile("home-secret",SecretRooms.Id(2),0),new SceneTile("home-secret",SecretRooms.Id(3),0),
            new SceneTile("home-bedroom",BedroomLayout.Id(0),0),new SceneTile("home-bedroom",BedroomLayout.Id(1),0),
            new SceneTile("home-bedroom",BedroomLayout.Id(2),0),new SceneTile("home-bedroom",BedroomLayout.Id(3),0),
            new SceneTile("zoo-entrance",ZooLayout.Entrance,0),
            new SceneTile("zoo-elephant",ZooLayout.Savanna,0),
            new SceneTile("zoo-giraffe",ZooLayout.Savanna,2400),
            new SceneTile("zoo-zebra",ZooLayout.Savanna,4800),
            new SceneTile("zoo-lion",ZooLayout.Savanna,7200),
            new SceneTile("zoo-brachiosaurus",ZooCatalog.Dinosaurs,0),
            new SceneTile("zoo-triceratops",ZooCatalog.Dinosaurs,2400),
            new SceneTile("zoo-stegosaurus",ZooCatalog.Dinosaurs,4800),
            new SceneTile("zoo-tyrannosaurus",ZooCatalog.Dinosaurs,7200),
            new SceneTile("zoo-clownfish",ZooCatalog.Aquarium,0),
            new SceneTile("zoo-blue-tang",ZooCatalog.Aquarium,2400),
            new SceneTile("zoo-zebra-shark",ZooCatalog.Aquarium,4800),
            new SceneTile("zoo-penguin",ZooCatalog.Aquarium,7200),
            new SceneTile("zoo-tortoise",ZooCatalog.Reptiles,0),
            new SceneTile("zoo-gecko",ZooCatalog.Reptiles,2400),
            new SceneTile("zoo-iguana",ZooCatalog.Reptiles,4800),
            new SceneTile("zoo-crocodile",ZooCatalog.Reptiles,7200),
            new SceneTile("home-upstairs",HomeRooms.Landing,0),
            new SceneTile("home-discovery","garden",-7200),new SceneTile("home-living","garden",-4800),new SceneTile("home-kitchen","garden",-2400),
            new SceneTile("garden-tree","garden",0),new SceneTile("garden-shed","garden",2400),
            new SceneTile("park-playground","park",0),new SceneTile("park-picnic","park",2400),
            new SceneTile("creek-bank","creek",0),new SceneTile("creek-crossing","creek",2400),
            new SceneTile("beach-dunes","beach",0),new SceneTile("beach-rockpools","beach",2400),
            new SceneTile("daycare-playroom","daycare",0),new SceneTile("daycare-garden","daycare",2400)
        };
        private readonly Dictionary<string,RawImage> scenicImages=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,ResourceRequest> scenicRequests=new Dictionary<string,ResourceRequest>();
        private RectTransform sceneryRoot;
        private GameObject sceneryCurtain;
        private float cameraX,sceneScale=1,groundCamera;
        private string cameraArea;
        private long cameraVisit=-1;
        private Vector2 groundDown;
        private bool groundPan,manualCamera,hideCameraFollowing;
        public string CurrentPlace=>HasWorld?WorldLayout.Place(ReadPlayer(Actor)):"garden";
        private int SceneSchema=>shared==null?World.Schema:shared.View.schema;
        public float CameraX=>cameraX;
        public Vector2 WorldPoint(Vector2 screen)=>BoardPoint(screen);
        public string[] ResidentScenery=>scenicImages.Keys.OrderBy(id=>id).ToArray();
        public int PendingScenery=>scenicRequests.Count;
        public bool SceneryReady=>sceneryRoot!=null && VisibleTiles().All(t=>scenicImages.ContainsKey(t.id));
        private bool ScenePosition(float x,float y)=>WorldLayout.Position(CurrentArea,SceneSchema,x,y);
        private Vector2 FromBoard(Vector2 point)=>new Vector2(point.x/sceneScale+cameraX,(point.y/sceneScale+250)/.45f);
        private void LayoutWorldViewport()
        {
            if(Board==null)return;
            // Frame the real scene inside the remaining space. A character at
            // the bottom keeps their world position but is visible above the tray.
            Board.sizeDelta=safe.rect.size-(CharactersOpen?new Vector2(310,300):Vector2.zero);
            Board.anchoredPosition=CharactersOpen?new Vector2(-155,150):Vector2.zero;
            sceneScale=Board.rect.height/WorldLayout.SceneHeight;
        }
        private void BuildScenery()
        {
            Board.anchoredPosition=Vector2.zero;Board.sizeDelta=safe.rect.size;
            Board.GetComponent<Image>().sprite=null;Board.gameObject.AddComponent<RectMask2D>();Board.SetAsFirstSibling();
            Board.Find("Sky").gameObject.SetActive(false);floorPath.gameObject.SetActive(false);
            foreach(var part in fence)part.SetActive(false);
            sceneryRoot=Rect(Board,"Illustrated world",Vector2.zero,Vector2.zero);Stretch(sceneryRoot);sceneryRoot.SetAsFirstSibling();
            sceneryCurtain=Plain(safe,"Preparing scenery",Vector2.zero,Vector2.zero,new Color(.66f,.88f,.97f),true).gameObject;
            Stretch((RectTransform)sceneryCurtain.transform);
            Label(sceneryCurtain.transform,"Getting your place ready…",30,Vector2.zero,new Vector2(900,70));
            // The old diagnostic lab chrome stays available to verification via
            // model state; it no longer occupies the child's illustrated world.
            foreach(var name in new[]{"Grow a flower","Splash cleanup","Free play"})
            {
                var item=safe.Find(name);if(item==null)continue;
                item.SetParent(menu.transform,false);((RectTransform)item).anchoredPosition=new Vector2(name=="Grow a flower"?-280:name=="Splash cleanup"?0:260,320);
            }
            areaLabel.gameObject.SetActive(false);activity.gameObject.SetActive(false);message.gameObject.SetActive(false);saveLabel.gameObject.SetActive(false);
            listenLabel.transform.parent.gameObject.SetActive(false);
            cameraArea=null;cameraVisit=-1;hideCameraFollowing=false;
        }
        private IEnumerable<SceneTile> VisibleTiles()
        {
            var half=Board.rect.width/(2*sceneScale);
            return SceneTiles.Where(t=>t.area==CurrentArea && (t.id!="home-discovery" || SceneSchema>=Discovery.Schema) && t.start<cameraX+half && t.start+WorldLayout.TileWidth>cameraX-half);
        }
        private void ClampCamera()
        {
            var half=Board.rect.width/(2*sceneScale);
            cameraX=Mathf.Clamp(cameraX,WorldLayout.MinX(CurrentArea,SceneSchema)+half,WorldLayout.MaxX(CurrentArea)-half);
        }
        private void TickScenery()
        {
            if(sceneryRoot==null)return;
            LayoutWorldViewport();
            var player=ReadPlayer(Actor);
            var position=shared!=null && shared.Connected?shared.VisualPosition(Actor):new Vector2(player.x,player.y);
            if(cameraArea!=player.zone || cameraVisit!=player.visit)
            {cameraArea=player.zone;cameraVisit=player.visit;cameraX=player.zone==ZooLayout.Entrance?1200:position.x;manualCamera=player.zone==ZooLayout.Entrance;}
            var followParent=FollowingHideParent;
            if(followParent!=hideCameraFollowing)
            {
                // Switch viewpoints locally; never move the hidden player or fly
                // across the whole property when entering/leaving a hiding spot.
                hideCameraFollowing=followParent;manualCamera=false;groundPan=false;
                cameraX=followParent?HideGame.x:position.x;
            }
            if(followParent)
            {manualCamera=false;cameraX=Mathf.Lerp(cameraX,HideGame.x,1-Mathf.Exp(-9*Time.unscaledDeltaTime));}
            else if(CharactersOpen)cameraX=position.x;
            else if(!manualCamera)
            {
                var dead=Board.rect.width/sceneScale*.12f;
                var target=position.x-Mathf.Clamp(position.x-cameraX,-dead,dead);
                cameraX=Mathf.Lerp(cameraX,target,1-Mathf.Exp(-9*Time.unscaledDeltaTime));
            }
            ClampCamera();
            var wanted=VisibleTiles().ToList();
            // At most two visible 3:1 panoramas plus one adjacent prefetch.
            // Distant areas are released, while persistent world records remain.
            var neighbor=SceneTiles.Where(t=>t.area==CurrentArea && (t.id!="home-discovery" || SceneSchema>=Discovery.Schema) && !wanted.Contains(t)).OrderBy(t=>Mathf.Abs(t.start+1200-cameraX)).FirstOrDefault();
            var stairTile=SceneTiles.FirstOrDefault(t=>t.id==(DoorPreload??StairPreload));
            if(stairTile!=null && !wanted.Contains(stairTile) && wanted.Count<3)wanted.Add(stairTile);
            else if(neighbor!=null && wanted.Count<3)wanted.Add(neighbor);
            foreach(var pair in scenicRequests.ToArray())if(pair.Value.isDone)
            {
                scenicRequests.Remove(pair.Key);var texture=pair.Value.asset as Texture2D;
                if(texture==null){Debug.LogError("Missing scenery: "+pair.Key);continue;}
                if(!wanted.Any(t=>t.id==pair.Key)){Resources.UnloadAsset(texture);continue;}
                var r=Rect(sceneryRoot,pair.Key,Vector2.zero,Vector2.zero);
                var image=r.gameObject.AddComponent<SceneryPanel>();image.texture=texture;image.raycastTarget=false;
                var tile=SceneTiles.First(t=>t.id==pair.Key);image.blendLeadingEdge=tile.start>WorldLayout.MinX(tile.area);
                image.SetVerticesDirty();scenicImages.Add(pair.Key,image);
            }
            foreach(var pair in scenicImages.ToArray())if(!wanted.Any(t=>t.id==pair.Key))
            {var texture=pair.Value.texture;pair.Value.texture=null;Destroy(pair.Value.gameObject);scenicImages.Remove(pair.Key);Resources.UnloadAsset(texture);}
            foreach(var tile in wanted)if(!scenicImages.ContainsKey(tile.id) && !scenicRequests.ContainsKey(tile.id) && scenicImages.Count+scenicRequests.Count<3)
                scenicRequests[tile.id]=Resources.LoadAsync<Texture2D>(SceneSchema>=ParkPlay.Schema && (tile.id=="park-playground" || tile.id=="park-picnic")?"ParkArt/"+(tile.id=="park-playground"?"playground-clean":"picnic-clean"):"Scenery/"+(tile.id=="home-kitchen" && SceneSchema>=Kitchen.Schema?"home-kitchen-working":tile.id));
            foreach(var tile in wanted.OrderBy(t=>t.start))if(scenicImages.TryGetValue(tile.id,out var image))
            {
                image.gameObject.SetActive(tile.area==CurrentArea);
                image.rectTransform.SetAsLastSibling();
                image.rectTransform.anchoredPosition=new Vector2((tile.start+1200-cameraX)*sceneScale,0);
                image.rectTransform.sizeDelta=new Vector2((WorldLayout.TileWidth+80)*sceneScale,WorldLayout.SceneHeight*sceneScale);
            }
            sceneryCurtain.SetActive(!SceneryReady && !WorldLoading);
            avatar.anchoredPosition=ToBoard(position.x,position.y);avatar.localScale=Vector3.one*sceneScale;
            foreach(var toy in ReadToys())
            {
                var point=toy.id==dragging?dragPoint:new Vector2(toy.x,toy.y);
                if(toy.id!=dragging && shared!=null && !string.IsNullOrEmpty(toy.holder) && shared.TryPreview(toy.id,out var preview))point=preview;
                toys[toy.id].anchoredPosition=ToBoard(point.x,point.y);toys[toy.id].localScale=Vector3.one*sceneScale;
            }
            foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)
            {var point=shared.VisualPosition(friend.Key);friend.Value.root.anchoredPosition=ToBoard(point.x,point.y);friend.Value.root.localScale=Vector3.one*sceneScale;}
            SortDepth();
        }
        private void ResetScenery(bool disposing=false)
        {
            foreach(var pair in scenicImages)
            {
                // Unity can destroy the canvas children before this owner's
                // OnDestroy during player shutdown. Do not dirty a dead Graphic.
                if(pair.Value==null)continue;
                var texture=pair.Value.texture;pair.Value.texture=null;
                if(texture!=null)Resources.UnloadAsset(texture);
            }
            // Keep pending requests across screen resets and reconcile them in
            // TickScenery. Unloading a late request while a new screen loads the
            // same asset would invalidate the new screen's texture reference.
            if(disposing)foreach(var request in scenicRequests.Values)
                if(request.isDone){if(request.asset!=null)Resources.UnloadAsset(request.asset);}
                else request.completed+=op=>{if(request.asset!=null)Resources.UnloadAsset(request.asset);};
            scenicImages.Clear();if(disposing)scenicRequests.Clear();sceneryRoot=null;sceneryCurtain=null;
        }
        private bool BeginGround(Vector2 screen)
        {groundDown=screen;groundCamera=cameraX;groundPan=false;return true;}
        private void MoveGround(Vector2 screen)
        {
            if(Vector2.Distance(screen,groundDown)>14)groundPan=true;
            if(!groundPan || FollowingHideParent)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Board,screen,null,out var now);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Board,groundDown,null,out var start);
            cameraX=groundCamera-(now.x-start.x)/sceneScale;ClampCamera();manualCamera=true;zooApproach=false;destination=null;
        }
        private void EndGround(Vector2 screen)
        {
            if(!groundPan && !JoystickMode)
            {zooApproach=false;var point=BoardPoint(screen);destination=new Vector2(Mathf.Clamp(point.x,WorldLayout.MinX(CurrentArea)+40,WorldLayout.MaxX(CurrentArea)-40),Mathf.Clamp(point.y,35,ZooLayout.Area(CurrentArea)?120:455));manualCamera=false;}
            groundPan=false;
        }
    }
}
