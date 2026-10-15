using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed class BirthdayCastleTouch:Button,IDragHandler,IEndDragHandler
    {
        public Action<Vector2> dragged;public Action released;
        public void OnDrag(PointerEventData e){if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var p))dragged?.Invoke(p);}
        public void OnEndDrag(PointerEventData e)=>released?.Invoke();
    }
    public sealed partial class GameScreen
    {
        private RawImage birthdayCastle,birthdayCastleFront;
        private Image birthdayCastleTarget;
        private RectTransform birthdayCastleActions;
        private bool birthdayCastleApproach;
        private long birthdayCastleVisit=-1;
        private double birthdayCastleDragAt;
        private bool birthdayCastleRelease;
        private bool birthdayCastleLeave;
        private void BuildBirthdayCastleUi()
        {
            if(BirthdayArt.Data.castle==null)throw new InvalidOperationException("Birthday castle content is missing.");
            birthdayCastle=BirthdayPicture(Board,"Birthday castle back",Vector2.zero,Vector2.one);birthdayCastle.texture=BirthdayArt.Texture(BirthdayArt.Data.castle.texture);
            birthdayCastleFront=BirthdayPicture(Board,"Birthday castle arch",Vector2.zero,Vector2.one);birthdayCastleFront.texture=BirthdayArt.Texture(BirthdayArt.Data.castle.front);
            birthdayCastleTarget=Plain(Board,"Birthday bouncy castle",Vector2.zero,Vector2.one,Color.clear,true);
            var touch=birthdayCastleTarget.gameObject.AddComponent<BirthdayCastleTouch>();touch.onClick.AddListener(()=>{
                if(Birthday.members.Single(m=>m.actor==Actor).station=="castle")BirthdaySend("castle-tap");
                else {CancelBirthdayIntent();birthdayCastleApproach=true;birthdayCastleVisit=ReadPlayer(Actor).visit;}
            });
            touch.dragged=point=>{
                if(Birthday.members.Single(m=>m.actor==Actor).station!="castle" || Time.unscaledTimeAsDouble<birthdayCastleDragAt || ActionPending)return;
                birthdayCastleDragAt=Time.unscaledTimeAsDouble+.1;
                var data=BirthdayArt.Data.castle;var screen=birthdayCastleTarget.rectTransform.anchoredPosition+point;
                var floor=ToBoard(data.floorX,data.floorY);
                BirthdaySend("castle-drag",x:Mathf.Clamp((screen.x-floor.x)/(BirthdayArt.PixelsPerUnit*sceneScale),-2.5f,2.5f),y:Mathf.Clamp((screen.y-floor.y)/(BirthdayArt.PixelsPerUnit*sceneScale),0,5.5f));
            };
            touch.released=()=>birthdayCastleRelease=true;
            birthdayCastleActions=Rect(safe,"Birthday castle actions",new Vector2(0,-300),new Vector2(620,60));
            BirthdayButton(birthdayCastleActions,"Castle stunt",new Vector2(-145,0),new Vector2(240,60),()=>BirthdaySend("castle-tap"));
            BirthdayButton(birthdayCastleActions,"Leave castle",new Vector2(145,0),new Vector2(240,60),()=>birthdayCastleLeave=true);
        }
        private Vector2 BirthdayCastlePoint(BirthdayMember member)
        {
            var castle=BirthdayArt.Data.castle;
            // Cosmetic prediction uses the same bounded forces as authority,
            // never emitting collisions, scores, stunts or state changes.
            var m=member.Copy();double clock=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,Birthday.clock,Time.unscaledTimeAsDouble,Shared);
            double remaining=Math.Clamp(clock-Birthday.clock,0,.3);
            while(remaining>0){float dt=(float)Math.Min(remaining,1d/60);remaining-=dt;
                if(m.bounceDrag){m.bounceVx=Mathf.Clamp((m.bounceTargetX-m.bounceX)*10,-10,10);m.bounceVy=Mathf.Clamp((m.bounceTargetH-m.bounceH)*10,-10,10);}else m.bounceVy-=9.8f*dt;
                m.bounceX+=m.bounceVx*dt;m.bounceH+=m.bounceVy*dt;
                if(Mathf.Abs(m.bounceX)>2.5f){m.bounceX=Mathf.Clamp(m.bounceX,-2.5f,2.5f);m.bounceVx=-m.bounceVx*.8f;}
                if(m.bounceH>5.5f){m.bounceH=5.5f;m.bounceVy=-Mathf.Abs(m.bounceVy)*.8f;}
                if(m.bounceH<0){m.bounceH=0;m.bounceVy=Mathf.Max(6,-m.bounceVy*.8f);}
            }
            return new Vector2(castle.floorX+m.bounceX*BirthdayArt.PixelsPerUnit,castle.floorY+m.bounceH*BirthdayArt.PixelsPerUnit/.45f);
        }
        private string BirthdayCastlePose(BirthdayMember m)
        {
            if(Birthday.clock-m.stuntAt<.8)return "stunt";
            return m.bounceVy>1?"bounce-up":m.bounceVy< -1?"bounce-down":"bounce-idle";
        }
        private void TickBirthdayCastleUi(bool visible)
        {
            if(birthdayCastle==null)return;
            birthdayCastle.gameObject.SetActive(visible);birthdayCastleFront.gameObject.SetActive(visible);birthdayCastleTarget.gameObject.SetActive(visible);
            var own=Birthday.members.Single(m=>m.actor==Actor);birthdayCastleActions.gameObject.SetActive(visible && own.station=="castle");if(!visible){birthdayCastleApproach=false;birthdayCastleLeave=birthdayCastleRelease=false;return;}
            // Leaving wins over a drag-release acknowledgement. A click during
            // that brief pending request must not be silently discarded.
            if(birthdayCastleLeave && !ActionPending){birthdayCastleLeave=birthdayCastleRelease=false;if(own.station=="castle")BirthdaySend("station",target:"");}
            else if(birthdayCastleRelease && !ActionPending){birthdayCastleRelease=false;if(own.station=="castle")BirthdaySend("castle-release");}
            var data=BirthdayArt.Data.castle;var at=new Vector2((data.pixels.center.x-cameraX)*sceneScale,(data.pixels.center.y-400)*sceneScale);
            foreach(var image in new[]{birthdayCastle,birthdayCastleFront}){image.rectTransform.anchoredPosition=at;image.rectTransform.sizeDelta=data.pixels.size*sceneScale;}
            double age=Birthday.members.Select(m=>Birthday.clock-m.impactAt).Where(a=>a>=0).DefaultIfEmpty(10).Min();
            int deformation=Mathf.Clamp(Mathf.RoundToInt((float)(Math.Max(0,1-age/.35)*4)),0,4);birthdayCastle.uvRect=new Rect(deformation/5f,0,.2f,1);
            birthdayCastleTarget.rectTransform.anchoredPosition=at;birthdayCastleTarget.rectTransform.sizeDelta=data.pixels.size*sceneScale;
            birthdayCastleActions.anchoredPosition=new Vector2(0,-safe.rect.height/2+100);
            if(birthdayCastleApproach){
                var p=ReadPlayer(Actor);if(p.visit!=birthdayCastleVisit)birthdayCastleApproach=false;
                else if(Vector2.Distance(new Vector2(p.x,p.y),new Vector2(1169,100))<60 && !ActionPending){birthdayCastleApproach=false;destination=null;BirthdaySend("station",target:"castle");}
                else destination=new Vector2(1169,100);
            }
        }
        private void AddBirthdayCastleDepth(Action<RectTransform,float,int,string> add)
        {
            if(birthdayCastle==null)return;
            add(birthdayCastle.rectTransform,10000,0,"birthday-castle-back");add(birthdayCastleFront.rectTransform,ToBoard(0,100).y,3,"birthday-castle-front");
            add(birthdayCastleTarget.rectTransform,10000,4,"birthday-castle-touch");
        }
        private void ResetBirthdayCastleUi(){birthdayCastle=null;birthdayCastleFront=null;birthdayCastleTarget=null;birthdayCastleActions=null;birthdayCastleApproach=false;birthdayCastleLeave=birthdayCastleRelease=false;birthdayCastleVisit=-1;}
    }
}
