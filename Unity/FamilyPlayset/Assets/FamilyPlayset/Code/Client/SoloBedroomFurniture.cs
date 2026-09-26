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
        private readonly Dictionary<string,Sprite> bedroomSprites=new Dictionary<string,Sprite>();
        private readonly Dictionary<string,RectTransform> bedroomFurniture=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,HomeArtPart> bedroomFronts=new Dictionary<string,HomeArtPart>();
        private readonly List<Image> bedroomTints=new List<Image>();
        private readonly List<RectTransform> bedroomStorageHints=new List<RectTransform>();
        private Image chestImage,lampGlow;
        private HomeArtPart bedQuilt;
        private RectTransform bedroomDecorButton,bedroomDecorPanel;
        private Text bedroomTogether,bedroomUndo;
        private string bedroomDragTarget="";
        private static readonly Color[] BedroomColors={Color.white,new Color(1,.87f,.85f),new Color(.84f,1,.9f),new Color(.93f,.85f,1)};
        private BedroomState FurnishedRoom=>SceneSchema>=BedroomFurniture.Schema?Bedrooms.FirstOrDefault(r=>r.id==CurrentArea):null;

        private Sprite BedroomSprite(string id)
        {
            if(bedroomSprites.TryGetValue(id,out var prior))return prior;
            var texture=Resources.Load<Texture2D>("BedroomArt/"+id);
            if(texture==null)throw new InvalidOperationException("Missing bedroom art: "+id);
            homeTextures.Add(texture);var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
            homeSprites.Add(sprite);bedroomSprites.Add(id,sprite);return sprite;
        }
        private RectTransform FurnitureRoot(string id)
        {var r=Rect(Board,"Bedroom "+id,Vector2.zero,Vector2.zero);bedroomFurniture.Add(id,r);return r;}
        private HomeArtPart FurnitureMask(string id,Sprite sprite,Vector2 center,Vector2 size,params Vector2[][] polygons)
        {
            var root=Rect(Board,"Bedroom "+id,Vector2.zero,Vector2.zero);
            var part=Rect(root,id,center,size).gameObject.AddComponent<HomeArtPart>();
            part.Configure(sprite.texture,polygons.Select(points=>new HomeArtPart.Polygon{points=points}).ToArray());
            bedroomFronts.Add(id,part);return part;
        }
        private static Vector2[] Patch(float x0,float y0,float x1,float y1)=>new[]{new Vector2(x0,y0),new Vector2(x1,y0),new Vector2(x1,y1),new Vector2(x0,y1)};
        private void BuildBedroomFurniture()
        {
            if(SceneSchema<BedroomFurniture.Schema)return;
            var bed=FurnitureRoot("bed");var bedSprite=BedroomSprite("bed");
            HomePicture(bed,"Bed",new Vector2(0,110),new Vector2(600,400),bedSprite);
            bedQuilt=FurnitureMask("quilt",bedSprite,new Vector2(0,110),new Vector2(600,400),
                new[]{new Vector2(.31f,.375f),new Vector2(.87f,.375f),new Vector2(.948f,.45f),new Vector2(.948f,.705f),new Vector2(.23f,.705f),new Vector2(.23f,.51f)});
            FurnitureMask("bed-front",bedSprite,new Vector2(0,110),new Vector2(600,400),Patch(0,.705f,1,1),Patch(.9f,.39f,1,.82f));
            HomeHit(bed,"Rest on bed",new Vector2(0,130),new Vector2(510,210),()=>UseHome(BedroomFurniture.Bed));
            var shelf=FurnitureRoot("shelf");var shelfSprite=BedroomSprite("shelf");
            HomePicture(shelf,"Shelf",new Vector2(0,95),new Vector2(460,307),shelfSprite);
            FurnitureMask("shelf-front",shelfSprite,new Vector2(0,95),new Vector2(460,307),Patch(0,.705f,1,1),Patch(.485f,.32f,.515f,.76f));
            var chest=FurnitureRoot("chest");var chestSprite=BedroomSprite("chest-open");
            chestImage=HomePicture(chest,"Toy chest",new Vector2(0,100),new Vector2(390,260),BedroomSprite("chest-closed"));
            FurnitureMask("chest-front",chestSprite,new Vector2(0,100),new Vector2(390,260),Patch(0,.525f,1,1));
            HomeHit(chest,"Open toy chest",new Vector2(0,20),new Vector2(310,115),()=>HomeAction(SoloAction.SetFixture,"bedroom-chest",FurnishedRoom.chestOpen?"off":"on"));
            var rug=FurnitureRoot("rug");HomePicture(rug,"Play rug",Vector2.zero,new Vector2(1080,330),BedroomSprite("rug"));
            for(var i=0;i<4;i++)
            {
                var index=i;var root=FurnitureRoot("cushion-"+i);var picture=HomePicture(root,"Reading cushion",new Vector2(0,10),new Vector2(180,120),BedroomSprite("cushion"));bedroomTints.Add(picture);
                HomeHit(root,"Sit on cushion "+(i+1),new Vector2(0,35),new Vector2(175,125),()=>UseHome(BedroomFurniture.Cushion(index)));
            }
            var lamp=FurnitureRoot("lamp");lampGlow=Panel(lamp,"Warm lamp light",new Vector2(0,265),new Vector2(220,220),new Color(1,.86f,.43f,.12f),false,true);
            HomePicture(lamp,"Lamp",new Vector2(0,260),new Vector2(80,120),BedroomSprite("lamp"));
            HomeHit(lamp,"Bedroom lamp",new Vector2(0,260),new Vector2(120,140),()=>HomeAction(SoloAction.SetFixture,"bedroom-lamp",FurnishedRoom.lampOn?"off":"on"));
            var pictureFrame=FurnitureRoot("picture");
            Panel(pictureFrame,"Timber frame",Vector2.zero,new Vector2(210,170),new Color(.74f,.47f,.24f));
            bedroomTints.Add(Panel(pictureFrame,"Picture paper",Vector2.zero,new Vector2(186,146),Cream));
            HomePicture(pictureFrame,"Dinosaur print",Vector2.zero,new Vector2(170,120),BedroomSprite("plush"));
            for(var i=0;i<12;i++)
            {var hint=Panel(Board,"Bedroom storage place "+i,Vector2.zero,new Vector2(75,65),new Color(1,.9f,.45f,.65f));hint.sprite=hintRing;hint.type=Image.Type.Simple;bedroomStorageHints.Add(hint.rectTransform);}
            bedroomDecorButton=(RectTransform)Button(safe,"Decorate",new Vector2(-380,315),new Vector2(180,60),()=>{CancelPointers();bedroomDecorPanel.gameObject.SetActive(!bedroomDecorPanel.gameObject.activeSelf);},Cream).transform.parent;
            bedroomDecorPanel=Panel(safe,"Room decoration",new Vector2(20,-235),new Vector2(850,95),new Color(1,.98f,.89f,.97f)).rectTransform;
            Button(bedroomDecorPanel,"Colours",new Vector2(-325,0),new Vector2(145,62),()=>BedroomEdit("theme",(FurnishedRoom.theme+1)%4),new Color(.83f,.93f,1));
            Button(bedroomDecorPanel,"Arrange",new Vector2(-163,0),new Vector2(145,62),()=>BedroomEdit("layout",1-FurnishedRoom.layout),new Color(.86f,.94f,.81f));
            bedroomTogether=Button(bedroomDecorPanel,"Together: off",new Vector2(0,0),new Vector2(158,62),()=>BedroomEdit("together",FurnishedRoom.decorateTogether?0:1),new Color(.96f,.86f,.94f));
            bedroomUndo=Button(bedroomDecorPanel,"Undo",new Vector2(164,0),new Vector2(145,62),()=>BedroomEdit("undo",0),Cream);
            Button(bedroomDecorPanel,"Put toys away",new Vector2(326,0),new Vector2(162,62),()=>BedroomEdit("tidy",0),new Color(.89f,.94f,.81f));
            bedroomDecorPanel.gameObject.SetActive(false);
        }
        private void BedroomEdit(string target,int value)
        {var room=FurnishedRoom;if(room==null)return;HomeAction(SoloAction.DecorateRoom,target,value+":"+room.roomRevision);}
        private Vector2 FurnitureAnchor(string id,BedroomState room)
        {
            if(id=="bed" || id=="quilt" || id=="bed-front")return new Vector2(BedroomFurniture.BedX(room.layout),350);
            if(id=="shelf" || id=="shelf-front" || id=="lamp")return new Vector2(BedroomFurniture.ShelfX(room.layout),380);
            if(id=="chest" || id=="chest-front")return new Vector2(BedroomFurniture.ChestX(room.layout),350);
            if(id=="rug")return new Vector2(room.layout==0?1120:1460,105);
            if(id=="picture")return new Vector2(BedroomFurniture.BedX(room.layout)-80,990);
            var index=int.Parse(id.Substring("cushion-".Length));return new Vector2(BedroomFurniture.SeatX(BedroomFurniture.Cushion(index),room.layout),95);
        }
        private Vector2 StoragePicture(BedroomState room,int slot)
        {
            var offset=slot<4?150:slot<8?116:slot<10?77:203.5f;
            return ToBoard(BedroomFurniture.StorageX(room.layout,slot),BedroomFurniture.StorageY(slot))+new Vector2(0,offset)*sceneScale;
        }
        private bool VisibleToy(SoloToy t)
        {
            if(t.zone!=CurrentArea)return false;if(string.IsNullOrEmpty(t.container))return true;
            if(HomeLayout.StorageSlot(t.container)>=0)return Home!=null && Home.shedOpen;
            var room=FurnishedRoom;var slot=BedroomFurniture.Slot(t.zone,t.container);return room!=null && slot>=0 && (slot>=8 || room.chestOpen);
        }
        private Vector2 FurnitureToyPoint(SoloToy t)
        {var room=FurnishedRoom;var slot=BedroomFurniture.Slot(t.zone,t.container);return room!=null && slot>=0?StoragePicture(room,slot):ToBoard(t.x,t.y);}
        private Vector2 BedroomDropPoint(Vector2 raw)
        {
            bedroomDragTarget="";var room=FurnishedRoom;if(room==null)return raw;
            var board=ToBoard(raw.x,raw.y);var closest=-1;var distance=float.MaxValue;
            for(var i=0;i<12;i++)
            {var d=Vector2.Distance(board,StoragePicture(room,i))/sceneScale;if(d<distance){distance=d;closest=i;}}
            if(distance>62)return raw;
            bedroomDragTarget=BedroomFurniture.Storage(room.id,closest);
            return new Vector2(BedroomFurniture.StorageX(room.layout,closest),BedroomFurniture.StorageY(closest));
        }
        private void PresentBedroomFurniture()
        {
            if(bedroomDecorButton==null)return;var room=FurnishedRoom;var visible=room!=null && !WorldLoading;var items=ReadToys();
            foreach(var pair in bedroomFurniture)
            {pair.Value.gameObject.SetActive(visible);if(!visible)continue;var anchor=FurnitureAnchor(pair.Key,room);pair.Value.anchoredPosition=ToBoard(anchor.x,anchor.y);pair.Value.localScale=Vector3.one*sceneScale;}
            foreach(var pair in bedroomFronts)
            {var root=(RectTransform)pair.Value.transform.parent;root.gameObject.SetActive(visible && (pair.Key!="chest-front" || room.chestOpen));if(!visible)continue;var anchor=FurnitureAnchor(pair.Key,room);root.anchoredPosition=ToBoard(anchor.x,anchor.y);root.localScale=Vector3.one*sceneScale;}
            var editable=visible && !MenuOpen && !TravelPending && (room.owner==Actor || room.decorateTogether);
            bedroomDecorButton.gameObject.SetActive(editable);if(!editable)bedroomDecorPanel.gameObject.SetActive(false);
            for(var i=0;i<12;i++)
            {var hint=bedroomStorageHints[i];hint.gameObject.SetActive(visible && dragging!=null && (i>=8 || room.chestOpen) && !items.Any(t=>t.container==BedroomFurniture.Storage(room.id,i)));if(visible){hint.anchoredPosition=StoragePicture(room,i);hint.localScale=Vector3.one*sceneScale;hint.SetAsLastSibling();}}
            if(!visible)return;
            chestImage.sprite=BedroomSprite(room.chestOpen?"chest-open":"chest-closed");lampGlow.gameObject.SetActive(room.lampOn);
            foreach(var tint in bedroomTints)tint.color=BedroomColors[room.theme];bedQuilt.color=BedroomColors[room.theme];
            bedroomTogether.text=room.decorateTogether?"Together: on":"Together: off";
            bedroomTogether.transform.parent.GetComponent<Button>().interactable=room.owner==Actor;
            bedroomUndo.transform.parent.GetComponent<Button>().interactable=room.undoKind!="" && room.undoActor==Actor;
            var tidy=bedroomDecorPanel.Find("Put toys away").GetComponent<Button>();tidy.interactable=room.owner==Actor;
            foreach(var toy in items)if(toy.id!=dragging && toy.zone==CurrentArea && toy.container!="")toys[toy.id].anchoredPosition=FurnitureToyPoint(toy);
            if(bedroomDecorPanel.gameObject.activeSelf)bedroomDecorPanel.SetAsLastSibling();
            SortDepth();
        }
        private void AddBedroomDepth(Action<RectTransform,float,int,string> add)
        {
            var room=FurnishedRoom;if(room==null)return;
            foreach(var pair in bedroomFurniture)
            {
                var root=pair.Value;var ground=pair.Key=="rug" || pair.Key=="picture"?99999:root.anchoredPosition.y;
                add(root,ground,0,pair.Key);
            }
            foreach(var pair in bedroomFronts){var root=(RectTransform)pair.Value.transform.parent;add(root,root.anchoredPosition.y,2,pair.Key);}
        }
        private void ResetBedroomFurniture()
        {bedroomSprites.Clear();bedroomFurniture.Clear();bedroomFronts.Clear();bedroomTints.Clear();bedroomStorageHints.Clear();bedroomDecorButton=bedroomDecorPanel=null;bedroomDragTarget="";}
    }
}
