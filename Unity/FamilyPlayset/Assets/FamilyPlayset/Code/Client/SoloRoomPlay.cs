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
        private readonly Dictionary<string,Sprite> roomPlaySprites=new Dictionary<string,Sprite>();
        private readonly List<RectTransform> playSupports=new List<RectTransform>();
        private readonly List<HomeArtPart> nestFronts=new List<HomeArtPart>();
        private readonly List<Image> playHints=new List<Image>();
        private readonly Dictionary<string,Image> teaFills=new Dictionary<string,Image>();
        private RectTransform roomPictureCard;
        private Image roomPrint,roomRug;
        private RectTransform factPrint;
        private string playArea;
        private SoloToy[] roomPlayItems=Array.Empty<SoloToy>();
        private const string AuroraWords="Those glowing ribbons are called auroras. Far above Earth, tiny particles from space meet gases in the air. The gases give off colorful light! Our secret room has a pretend aurora, so we can enjoy the colors together.";
        private Sprite RoomPlaySprite(string id)
        {
            if(roomPlaySprites.TryGetValue(id,out var sprite))return sprite;
            var texture=Resources.Load<Texture2D>("RoomPlay/"+id);if(texture==null)throw new InvalidOperationException("Missing room play art: "+id);
            homeTextures.Add(texture);sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));homeSprites.Add(sprite);roomPlaySprites.Add(id,sprite);return sprite;
        }
        public bool IsTapObject(string id)=>IsBook(id) || HasWorld && SceneSchema>=RoomPlay.Schema && AllToys().Any(t=>t.id==id && (t.kind==ToyKind.Plush || RoomPlay.Tea(t.kind)));
        public void TapObject(string id)
        {
            if(IsBook(id)){OpenBook(id);return;}if(!Ready || MenuOpen || TravelPending || dragging!=null)return;
            var toy=ReadToys().FirstOrDefault(t=>t.id==id);if(toy==null)return;
            // Keep an active movement pointer intact, as with the accepted
            // balloon input. Seating takes effect only while the player rests.
            void Done(SoloResult result)
            {
                RoomPlayFeedback(result.Accepted?(toy.kind==ToyKind.Plush?"A cuddle! Drag your friend to carry or tuck in.":toy.kind==ToyKind.TeaPot?"Pretend tea is ready. Drag the pot to a cup!":"A little sip! Your cup is ready for more."):
                    result.Outcome=="fixture-busy"?"The cushions are busy. Try a bed or the fort.":"That toy is being used. Try another one!");Render();
            }
            if(Shared)SubmitShared(SoloAction.RoomObject,id,"","",0,0,Done);else Done(Command(SoloAction.RoomObject,id));
        }
        private void RoomPlayFeedback(string text){if(homeFeedback==null)return;homeFeedback.text=text;homeFeedbackUntil=Time.unscaledTime+3;}
        private void BuildRoomPlay()
        {
            if(SceneSchema<RoomPlay.Schema)return;
            Narration?.AddClip("aurora",Resources.Load<AudioClip>("RoomPlay/aurora"));
            factPrint=Panel(bedroomFurniture["picture"],"Aurora print",Vector2.zero,new Vector2(186,146),new Color(.12f,.2f,.35f)).rectTransform;
            var sky=Rect(factPrint,"Aurora ribbons",Vector2.zero,new Vector2(168,130)).gameObject.AddComponent<SecretSky>();sky.raycastTarget=false;sky.Present(1,0);
            for(var i=0;i<9;i++)
            {
                var root=Rect(Board,i<4?"Blanket nest "+(i+1):"Tea place "+(i-3),Vector2.zero,Vector2.zero);playSupports.Add(root);
                if(i<4)
                {
                    var sprite=RoomPlaySprite("nest");HomePicture(root,"Tiny blanket bed",new Vector2(0,30),new Vector2(200,133),sprite);
                    var frontRoot=Rect(Board,"Nest quilt front "+i,Vector2.zero,Vector2.zero);
                    var front=Rect(frontRoot,"Quilt",new Vector2(0,30),new Vector2(200,133)).gameObject.AddComponent<HomeArtPart>();
                    front.Configure(sprite.texture,new[]{new HomeArtPart.Polygon{points=new[]{new Vector2(0,.48f),new Vector2(.25f,.48f),new Vector2(.5f,.525f),new Vector2(.75f,.48f),new Vector2(1,.48f),new Vector2(1,1),new Vector2(0,1)}}});nestFronts.Add(front);
                    var index=i;HomeHit(root,"Tuck a toy "+(i+1),new Vector2(0,25),new Vector2(180,100),()=>RoomPlayFeedback("Drag a plush friend onto an empty blanket nest."),false);
                }
                else
                {
                    var plate=Panel(root,"Picnic plate",new Vector2(0,6),new Vector2(i==8?130:112,44),new Color(.68f,.84f,.82f),false,true);
                    Panel(plate.transform,"Cream plate",Vector2.zero,new Vector2(i==8?113:94,32),Cream,false,true);
                }
                var hint=Panel(root,"Drop here",new Vector2(0,i<4?62:25),new Vector2(90,70),new Color(1,.85f,.25f,.8f));hint.sprite=hintRing;hint.type=Image.Type.Simple;playHints.Add(hint);
            }
            var card=Panel(safe,"Room picture",Vector2.zero,new Vector2(860,440),new Color(.97f,.98f,1,.98f));roomPictureCard=card.rectTransform;
            Label(card.transform,"Our room",34,new Vector2(0,162),new Vector2(660,58)).name="Picture title";
            Label(card.transform,"",26,new Vector2(0,30),new Vector2(740,210)).name="Picture words";
            Button(card.transform,"Read to me",new Vector2(-170,-157),new Vector2(230,66),()=>{Narration?.Speak("aurora");},new Color(.83f,.91f,1)).transform.parent.name="Picture read";
            Button(card.transform,"Close",new Vector2(170,-157),new Vector2(230,66),CloseRoomPicture,Cream);roomPictureCard.gameObject.SetActive(false);
        }
        private void OpenRoomPicture()
        {
            if(roomPictureCard==null || FurnishedRoom==null)return;
            var secret=SecretRooms.Index(CurrentArea)>=0;
            roomPictureCard.Find("Picture title").GetComponent<Text>().text=secret?"Aurora lights":"Play with your toy friends";
            roomPictureCard.Find("Picture words").GetComponent<Text>().text=secret?AuroraWords:"Tap a plush for a cuddle. Drag it to a blanket nest to tuck it in.\n\nPile up to three plush toys or blocks. Tap the teapot to fill it, drag it to a cup, then tap the cup for a pretend sip.";
            roomPictureCard.Find("Picture read").gameObject.SetActive(secret);roomPictureCard.gameObject.SetActive(true);roomPictureCard.SetAsLastSibling();
        }
        private void CloseRoomPicture(){if(roomPictureCard!=null && roomPictureCard.gameObject.activeSelf){roomPictureCard.gameObject.SetActive(false);Narration?.Stop();}}
        private bool Cuddling(SoloToy t,out SoloPlayer p)
        {p=t.holder==""?null:ReadPlayer(t.holder);return t.kind==ToyKind.Plush && p!=null && BedroomFurniture.Seat(p.fixture);}
        private int StackHeight(SoloToy t,SoloToy[] items)
        {var height=0;while(RoomPlay.Parent(t)!="" && height<3){t=items.FirstOrDefault(v=>v.id==RoomPlay.Parent(t));if(t==null)break;height++;}return height;}
        private bool RoomToyPoint(SoloToy t,out Vector2 point)
        {
            point=Vector2.zero;if(SceneSchema<RoomPlay.Schema)return false;
            if(Cuddling(t,out var p)){point=ToBoard(p.x,p.y)+new Vector2(22,p.fixture==BedroomFurniture.Bed?30:20)*sceneScale;return true;}
            var slot=RoomPlay.Slot(t.zone,t.container);
            if(slot>=0){point=ToBoard(t.x,t.y)+new Vector2(0,slot<4?65:38)*sceneScale;return true;}
            if(RoomPlay.Parent(t)!=""){point=ToBoard(t.x,t.y)+new Vector2(0,StackHeight(t,roomPlayItems)*60)*sceneScale;return true;}
            return false;
        }
        private bool RoomDropPoint(Vector2 raw,out Vector2 point)
        {
            point=raw;if(SceneSchema<RoomPlay.Schema || FurnishedRoom==null || dragging==null)return false;
            var items=ReadToys();roomPlayItems=items;var held=items.FirstOrDefault(t=>t.id==dragging);if(held==null)return false;
            var board=ToBoard(raw.x,raw.y);var distance=70f;string target="";var result=raw;
            void Candidate(string id,Vector2 visual,float x,float y)
            {var d=Vector2.Distance(board,visual)/sceneScale;if(d>=distance)return;distance=d;target=id;result=new Vector2(x,y);}
            // An occupied cup/pile is an object target, never an extra copy.
            foreach(var t in items.Where(t=>t.id!=held.id && t.zone==CurrentArea && VisibleToy(t)))
            {
                if(held.kind==ToyKind.TeaPot && t.kind==ToyKind.TeaCup)Candidate(t.id,FurnitureToyPoint(t),t.x,t.y);
                if(RoomPlay.Stackable(held.kind) && RoomPlay.Stackable(t.kind) && t.holder=="" && (t.container=="" || RoomPlay.Parent(t)!="") && !items.Any(v=>v.container==RoomPlay.Stack(t.id)))
                    Candidate(RoomPlay.Stack(t.id),FurnitureToyPoint(t)+new Vector2(0,42)*sceneScale,t.x,t.y);
            }
            for(var i=0;i<9;i++)if(held.kind==(i<4?ToyKind.Plush:i==8?ToyKind.TeaPot:ToyKind.TeaCup))
                Candidate(RoomPlay.Support(CurrentArea,i),ToBoard(RoomPlay.X(i),RoomPlay.Y(i))+new Vector2(0,i<4?65:38)*sceneScale,RoomPlay.X(i),RoomPlay.Y(i));
            if(target=="")return false;bedroomDragTarget=target;point=result;return true;
        }
        private void PresentRoomPlay()
        {
            if(playSupports.Count==0)return;var room=FurnishedRoom;var visible=room!=null && !WorldLoading;
            if(playArea!=CurrentArea){CloseRoomPicture();playArea=CurrentArea;}if(BookOpen || MenuOpen || applicationPaused)CloseRoomPicture();
            var items=ReadToys();roomPlayItems=items;var held=items.FirstOrDefault(t=>t.id==dragging);
            for(var i=0;i<playSupports.Count;i++)
            {
                var root=playSupports[i];root.gameObject.SetActive(visible);root.anchoredPosition=ToBoard(RoomPlay.X(i),RoomPlay.Y(i));root.localScale=Vector3.one*sceneScale;
                playHints[i].gameObject.SetActive(visible && held!=null && held.kind==(i<4?ToyKind.Plush:i==8?ToyKind.TeaPot:ToyKind.TeaCup) && !items.Any(t=>t.container==RoomPlay.Support(CurrentArea,i)));
                if(i<4){var front=nestFronts[i];front.transform.parent.gameObject.SetActive(visible);((RectTransform)front.transform.parent).anchoredPosition=root.anchoredPosition;front.transform.parent.localScale=root.localScale;front.color=room==null?Color.white:BedroomColors[room.bedding];}
            }
            foreach(var t in items){if(t.id!=dragging && RoomToyPoint(t,out var point))toys[t.id].anchoredPosition=point;if(teaFills.TryGetValue(t.id,out var liquid))liquid.gameObject.SetActive(t.water>0);}
            if(visible){factPrint.gameObject.SetActive(SecretRooms.Index(CurrentArea)>=0 && room.picture==0);roomPrint.sprite=room.picture==0?BedroomSprite("plush"):SecretSprite("plush-"+(room.picture==1?1:room.picture==2?2:5));roomRug.color=BedroomColors[room.rug];lampGlow.color=new[]{new Color(1,.86f,.43f,.12f),new Color(1,.67f,.77f,.12f),new Color(.55f,1,.76f,.12f),new Color(.72f,.65f,1,.12f)}[room.lamp];}
            SortDepth();
        }
        private void AddRoomPlayDepth(Action<RectTransform,float,int,string> add)
        {for(var i=0;i<playSupports.Count;i++){var root=playSupports[i];add(root,root.anchoredPosition.y,0,"play-"+i);if(i<4)add((RectTransform)nestFronts[i].transform.parent,root.anchoredPosition.y,2,"nest-front-"+i);}}
        private bool RoomToyDepth(SoloToy t,RectTransform rect,Action<RectTransform,float,int,string> add,SoloToy[] items)
        {
            if(SceneSchema<RoomPlay.Schema)return false;
            if(Cuddling(t,out var p)){add(rect,ToBoard(p.x,p.y).y,1,"zz-cuddle-"+t.id);return true;}
            var slot=RoomPlay.Slot(t.zone,t.container);if(slot>=0){add(rect,ToBoard(t.x,t.y).y,1,t.id);return true;}
            if(RoomPlay.Parent(t)!="" || items.Any(v=>v.container==RoomPlay.Stack(t.id))){add(rect,ToBoard(t.x,t.y).y,3+StackHeight(t,items),t.id);return true;}
            return false;
        }
        private void DrawTea(SoloToy t,RectTransform root)
        {
            if(!RoomPlay.Tea(t.kind))return;
            HomePicture(root,"Pretend tea",Vector2.zero,t.kind==ToyKind.TeaPot?new Vector2(132,88):new Vector2(106,71),RoomPlaySprite(t.kind==ToyKind.TeaPot?"teapot":"teacup"));
            teaFills[t.id]=Panel(root,"Pretend tea ready",new Vector2(t.kind==ToyKind.TeaPot?0:-4,t.kind==ToyKind.TeaPot?8:13),t.kind==ToyKind.TeaPot?new Vector2(12,12):new Vector2(42,9),new Color(.74f,.46f,.24f),false,true);
        }
        private void ResetRoomPlay()
        {roomPlaySprites.Clear();playSupports.Clear();nestFronts.Clear();playHints.Clear();teaFills.Clear();roomPictureCard=factPrint=null;roomPrint=roomRug=null;playArea=null;roomPlayItems=Array.Empty<SoloToy>();}
    }
}
