using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class BirthdayCakeTap:Button
    {
        public Action<Vector2> clicked;
        public override void OnPointerClick(PointerEventData e)
        {
            if(!IsActive() || !IsInteractable())return;
            base.OnPointerClick(e);
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var p))
                clicked?.Invoke(new Vector2((p.x-rect.rect.xMin)/rect.rect.width,(p.y-rect.rect.yMin)/rect.rect.height));
        }
    }
    public sealed partial class GameScreen
    {
        private RectTransform birthdayPartyActions,birthdayCakePanel,birthdayCakeFrame,birthdayWorldCake;
        private RawImage birthdayCakeBase,birthdayWorldCakeBase;
        private Text birthdayCakeStatus,birthdayEatText;
        private Button birthdayUnwrapButton,birthdayEatButton,birthdayWrapButton;
        private readonly List<RawImage> birthdayCakeStickers=new List<RawImage>(),birthdayWorldCakeStickers=new List<RawImage>();
        private BirthdayArt.CakeSprite[] birthdayBases,birthdayDecorations;
        private int birthdayDecoration;
        private bool birthdayCakeOpen,birthdayCakeApproach,birthdayUnwrapApproach;
        private long birthdayCakeVisit=-1;
        private long birthdayUnwrapVisit=-1;
        private bool birthdayVendorApproach;
        private long birthdayVendorVisit=-1;
        private Button birthdayLeaveQueue;
        private double birthdayVendorSeen=-2,birthdayVendorOrigin,birthdayVendorHappyUntil;
        private int birthdayVendorCount=-1;
        private RawImage BirthdayPicture(Transform parent,string name,Vector2 point,Vector2 size)
        {var image=Rect(parent,name,point,size).gameObject.AddComponent<RawImage>();image.raycastTarget=false;return image;}
        private Text BirthdayButton(Transform parent,string name,Vector2 point,Vector2 size,Action clicked)
        {var text=Button(parent,name,point,size,()=>clicked(),Cream);text.fontSize=23;return text;}
        private void BuildBirthdayPartyUi()
        {
            if(BirthdayArt.Data.cakeSprites==null)return;
            birthdayBases=BirthdayArt.Data.cakeSprites.Where(s=>s.category=="base").ToArray();
            birthdayDecorations=BirthdayArt.Data.cakeSprites.Where(s=>s.category=="sticker").ToArray();
            if(birthdayBases.Length!=3 || birthdayDecorations.Length!=26)throw new InvalidOperationException("Birthday cake content is incomplete.");
            birthdayWorldCake=Rect(Board,"Shared birthday cake",Vector2.zero,new Vector2(420,420));
            birthdayWorldCakeBase=BirthdayPicture(birthdayWorldCake,"Birthday cake table",Vector2.zero,new Vector2(420,420));
            birthdayWorldCakeBase.raycastTarget=true;NavButton(birthdayWorldCakeBase,OpenBirthdayCake);
            birthdayPartyActions=Rect(safe,"Birthday actions",Vector2.zero,new Vector2(650,60));
            birthdayUnwrapButton=BirthdayButton(birthdayPartyActions,"Open present with Hulk",new Vector2(-180,0),new Vector2(265,60),()=>{CancelBirthdayIntent();birthdayUnwrapApproach=true;birthdayUnwrapVisit=ReadPlayer(Actor).visit;}).transform.parent.GetComponent<Button>();
            birthdayEatText=BirthdayButton(birthdayPartyActions,"Eat snack",new Vector2(110,0),new Vector2(210,60),()=>{
                var held=Birthday.props.FirstOrDefault(p=>p.holder==Actor);BirthdaySend(held?.kind=="drink"?"drink":"eat");
            });birthdayEatButton=birthdayEatText.transform.parent.GetComponent<Button>();
            birthdayWrapButton=BirthdayButton(birthdayPartyActions,"Wrap again",new Vector2(330,0),new Vector2(200,60),()=>BirthdaySend("wrap")).transform.parent.GetComponent<Button>();
            birthdayCakePanel=Plain(safe,"Birthday cake editor",Vector2.zero,Vector2.zero,new Color(.93f,.97f,1),true).rectTransform;Stretch(birthdayCakePanel);
            birthdayCakeFrame=Rect(birthdayCakePanel,"Family cake decorating",Vector2.zero,new Vector2(1100,660));
            Label(birthdayCakeFrame,"Our birthday cake",35,new Vector2(0,280),new Vector2(700,60));
            BirthdayButton(birthdayCakeFrame,"Back to party",new Vector2(-430,280),new Vector2(210,64),()=>birthdayCakeOpen=false);
            birthdayCakeStatus=Label(birthdayCakeFrame,"",23,new Vector2(170,220),new Vector2(650,45));
            birthdayCakeBase=BirthdayPicture(birthdayCakeFrame,"Cake decorating surface",new Vector2(-300,30),new Vector2(420,420));birthdayCakeBase.raycastTarget=true;
            birthdayCakeBase.gameObject.AddComponent<BirthdayCakeTap>().clicked=point=>BirthdaySend("cake-sticker",birthdayDecoration.ToString(),x:point.x,y:point.y);
            for(int i=0;i<32;i++){
                birthdayCakeStickers.Add(BirthdayPicture(birthdayCakeBase.transform,"Cake decoration "+i,Vector2.zero,new Vector2(100,100)));
                birthdayWorldCakeStickers.Add(BirthdayPicture(birthdayWorldCake,"World cake decoration "+i,Vector2.zero,new Vector2(100,100)));
            }
            for(int i=0;i<3;i++){
                int index=i;var image=BirthdayPicture(birthdayCakeFrame,"Cake base "+i,new Vector2(-440+i*140,-175),new Vector2(130,100));
                image.texture=BirthdayArt.Texture(birthdayBases[i].texture);image.raycastTarget=true;NavButton(image,()=>BirthdaySend("cake-base",index.ToString()));
            }
            for(int i=0;i<birthdayDecorations.Length;i++){
                int index=i;var image=BirthdayPicture(birthdayCakeFrame,"Cake sticker "+i,new Vector2(35+i%7*72,140-i/7*80),new Vector2(66,72));
                image.texture=BirthdayArt.Texture(birthdayDecorations[i].texture);image.raycastTarget=true;NavButton(image,()=>birthdayDecoration=index);
            }
            BirthdayButton(birthdayCakeFrame,"Undo",new Vector2(90,-200),new Vector2(160,60),()=>BirthdaySend("cake-undo"));
            BirthdayButton(birthdayCakeFrame,"Start again",new Vector2(330,-200),new Vector2(220,60),()=>BirthdaySend("cake-clear"));
            BirthdayButton(birthdayCakeFrame,"Cake is ready!",new Vector2(-320,-275),new Vector2(245,64),()=>BirthdaySend("cake-finish"));
            BirthdayButton(birthdayCakeFrame,"Celebrate",new Vector2(-30,-275),new Vector2(230,64),()=>BirthdaySend("cake-celebrate"));
            BirthdayButton(birthdayCakeFrame,"Have some cake",new Vector2(245,-275),new Vector2(230,64),()=>{BirthdaySend("serve");birthdayCakeOpen=false;});
            BirthdayButton(birthdayCakeFrame,"Cold drink",new Vector2(440,280),new Vector2(190,64),()=>{BirthdaySend("pour");birthdayCakeOpen=false;});
            birthdayCakePanel.gameObject.SetActive(false);
            BuildBirthdayVendorUi();
        }
        private void BuildBirthdayVendorUi()
        {
            birthdayPeople.Add("birthday-squirrel",BirthdayImage("Birthday squirrel artwork"));
            var face=Plain(Board,"Birthday squirrel stall",Vector2.zero,new Vector2(45,45),Color.clear,true);
            birthdayFaces.Add("birthday-squirrel",face);
            NavButton(face,()=>{
                if(Birthday.vendorQueue.Contains(Actor)){BirthdaySend("leave-queue");return;}
                if(Birthday.props.Any(p=>p.holder==Actor))return;
                CancelBirthdayIntent();
                birthdayVendorApproach=true;birthdayVendorVisit=ReadPlayer(Actor).visit;
            });
            birthdayLeaveQueue=BirthdayButton(birthdayPartyActions,"Leave line",new Vector2(-400,0),new Vector2(180,60),()=>BirthdaySend("leave-queue")).transform.parent.GetComponent<Button>();
        }
        private void TickBirthdayVendorUi(BirthdayState game)
        {
            if(!birthdayPeople.TryGetValue("birthday-squirrel",out var seller))return;
            var own=ReadPlayer(Actor);
            if(birthdayVendorApproach){
                if(own.visit!=birthdayVendorVisit || game.vendorQueue.Contains(Actor) || game.props.Any(p=>p.holder==Actor))birthdayVendorApproach=false;
                else {
                    int place=Mathf.Min(3,game.vendorQueue.Length);var target=new Vector2(HulkBirthday.QueueX[place],HulkBirthday.QueueY[place]);
                    if(Vector2.Distance(new Vector2(own.x,own.y),target)<50 && !ActionPending){birthdayVendorApproach=false;destination=null;BirthdaySend("queue");}
                    else destination=target;
                }
            }
            birthdayLeaveQueue.gameObject.SetActive(game.vendorQueue.Contains(Actor));
            if(game.vendorAt!=birthdayVendorSeen){birthdayVendorSeen=game.vendorAt;birthdayVendorOrigin=Time.unscaledTimeAsDouble-Math.Max(0,game.clock-game.vendorAt);}
            if(game.vendorCount!=birthdayVendorCount){if(birthdayVendorCount>=0)birthdayVendorHappyUntil=Time.unscaledTimeAsDouble+1;birthdayVendorCount=game.vendorCount;}
            string pose=game.vendorAt>=0?"serve":Time.unscaledTimeAsDouble<birthdayVendorHappyUntil?"happy":"idle";
            var action=BirthdayArt.Action("Squirrel",pose)??throw new InvalidOperationException("Squirrel serving content is missing.");
            double age=pose=="serve"?Time.unscaledTimeAsDouble-birthdayVendorOrigin:Time.unscaledTimeAsDouble%Math.Max(.001,action.duration);
            int frame=Mathf.Clamp((int)(age*BirthdayArt.Data.fps),0,action.frames-1);
            seller.texture=BirthdayArt.Texture(action.texture);seller.uvRect=BirthdayArt.Uv(action,frame);
            seller.rectTransform.pivot=action.pivot/256;seller.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
            seller.rectTransform.anchoredPosition=ToBoard(4445,216);
            Vector2 Offset(Vector2 anchor)=>(anchor-action.pivot)*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
            birthdayFaces["birthday-squirrel"].rectTransform.anchoredPosition=seller.rectTransform.anchoredPosition+Offset(action.face[frame]);
            birthdayFaces["birthday-squirrel"].rectTransform.sizeDelta=new Vector2(45,45)*sceneScale;
            birthdayHoldPoints["birthday-squirrel"]=seller.rectTransform.anchoredPosition+Offset(action.hold[frame]);
            birthdayFrames["birthday-squirrel"]=(action,frame);birthdayPositions["birthday-squirrel"]=new Vector2(4445,216);birthdayViews["birthday-squirrel"]="front";birthdayMirrors["birthday-squirrel"]=false;
        }
        private void OpenBirthdayCake()
        {
            CancelBirthdayIntent();
            var player=ReadPlayer(Actor);
            if(Mathf.Abs(player.x-2400)>GameWorld.InteractionRadius || Mathf.Abs(player.y-200)>GameWorld.InteractionRadius){birthdayCakeVisit=player.visit;birthdayCakeApproach=true;destination=new Vector2(2400,200);return;}
            destination=null;CancelPointers();birthdayCakeOpen=true;birthdayCakeApproach=false;
        }
        private void TickBirthdayPartyUi(bool visible,BirthdayState game)
        {
            if(birthdayCakePanel==null)return;
            birthdayWorldCake.gameObject.SetActive(visible);birthdayPartyActions.gameObject.SetActive(visible);
            birthdayCakePanel.gameObject.SetActive(visible && birthdayCakeOpen);
            if(!visible){birthdayCakeOpen=false;birthdayCakeApproach=false;birthdayUnwrapApproach=false;birthdayVendorApproach=false;return;}
            var player=ReadPlayer(Actor);var held=game.props.FirstOrDefault(p=>p.holder==Actor);
            if(birthdayUnwrapApproach){
                if(player.visit!=birthdayUnwrapVisit || held==null || held.kind!="gift" || held.opened || game.giftAt>=0)birthdayUnwrapApproach=false;
                else {
                    var hulk=game.cast[0];var human=hulk.controller==""?null:ReadPlayer(hulk.controller);
                    var point=human==null?new Vector2(hulk.x,hulk.y):new Vector2(human.x,human.y);
                    if(Vector2.Distance(new Vector2(player.x,player.y),point)<60 && !ActionPending){birthdayUnwrapApproach=false;destination=null;BirthdaySend("unwrap");}
                    else destination=point;
                }
            }
            if(birthdayCakeApproach){
                if(player.visit!=birthdayCakeVisit)birthdayCakeApproach=false;
                else if(Vector2.Distance(new Vector2(player.x,player.y),new Vector2(2400,200))<60)OpenBirthdayCake();
                else destination=new Vector2(2400,200);
            }
            birthdayPartyActions.anchoredPosition=new Vector2(0,-safe.rect.height*.5f+55);
            birthdayUnwrapButton.gameObject.SetActive(held!=null && held.kind=="gift" && !held.opened && game.giftAt<0);
            birthdayEatButton.gameObject.SetActive(held!=null && (held.kind=="food" || held.kind=="drink" || held.kind=="gift" && held.opened && held.content==0));
            birthdayWrapButton.gameObject.SetActive(held!=null && held.kind=="gift" && held.opened);
            birthdayEatText.text=held?.kind=="drink"?"Drink":"Eat snack";
            birthdayWorldCake.anchoredPosition=ToBoard(2400,330);birthdayWorldCake.localScale=Vector3.one*(sceneScale*.5f);
            birthdayWorldCakeBase.texture=birthdayCakeBase.texture=BirthdayArt.Texture(birthdayBases[game.cakeBase].texture);
            for(int i=0;i<32;i++){
                bool used=i<game.stickers.Length;
                foreach(var image in new[]{birthdayCakeStickers[i],birthdayWorldCakeStickers[i]}){
                    image.gameObject.SetActive(used);if(!used)continue;
                    image.texture=BirthdayArt.Texture(birthdayDecorations[game.stickers[i]].texture);
                    image.rectTransform.anchoredPosition=new Vector2((game.stickerX[i]-.5f)*420,(game.stickerY[i]-.5f)*420);
                }
            }
            birthdayCakeStatus.text=game.cakeReady?"Ready to share together!":"Choose a decoration, then tap the cake.";
            birthdayCakeFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/1120,safe.rect.height/690);
            if(birthdayCakeOpen){birthdayCakePanel.SetAsLastSibling();destination=null;}
        }
        private void ResetBirthdayPartyUi()
        {
            birthdayPartyActions=null;birthdayCakePanel=null;birthdayCakeFrame=null;birthdayWorldCake=null;birthdayCakeBase=null;birthdayWorldCakeBase=null;
            birthdayCakeStickers.Clear();birthdayWorldCakeStickers.Clear();birthdayCakeOpen=false;birthdayCakeApproach=false;birthdayUnwrapApproach=false;birthdayCakeVisit=-1;birthdayUnwrapVisit=-1;birthdayDecoration=0;
            birthdayVendorApproach=false;birthdayVendorVisit=-1;birthdayVendorSeen=-2;birthdayVendorCount=-1;birthdayVendorHappyUntil=0;
        }
    }
}
