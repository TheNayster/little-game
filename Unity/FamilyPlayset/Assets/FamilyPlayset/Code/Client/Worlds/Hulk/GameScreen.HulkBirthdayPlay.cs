using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // A lightweight 2D particle cutout follows the recovered machine's shared
    // on/off and pop semantics; particle paths are an explicit family adaptation.
    public sealed class BirthdayBubble:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;float radius=Math.Min(rect.width,rect.height)*.5f;
            for(int i=0;i<32;i++){
                float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;int start=vh.currentVertCount;
                foreach(var p in new[]{new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius-3),new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius-3)})vh.AddVert(p,color,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start+2,start+1,start+3);
            }
        }
    }
    public sealed partial class GameScreen
    {
        private static readonly Color[] BirthdayBrickColours={Color.white,new Color(.65f,1,1),new Color(1,.7f,.7f),new Color(1,1,.65f)};
        private RawImage birthdayBubbleMachine;
        private Text birthdayBubbleSwitch;
        private readonly List<BirthdayBubble> birthdayBubbles=new List<BirthdayBubble>();
        private readonly int[] birthdayBubbleIds=new int[12];
        private RectTransform birthdayBrickActions;
        private bool birthdayBubbleApproach;
        private long birthdayBubbleVisit=-1;
        private void BuildBirthdayPlayUi()
        {
            birthdayBubbleMachine=BirthdayImage("Birthday bubble machine");var art=BirthdayArt.PropView("BubbleMachinActor","front");birthdayBubbleMachine.texture=BirthdayArt.Texture(art.texture);birthdayBubbleMachine.raycastTarget=true;
            NavButton(birthdayBubbleMachine,()=>{CancelBirthdayIntent();birthdayBubbleApproach=true;birthdayBubbleVisit=ReadPlayer(Actor).visit;});
            birthdayBubbleSwitch=BirthdayButton(Board,"Birthday bubble switch",Vector2.zero,new Vector2(190,55),()=>{CancelBirthdayIntent();birthdayBubbleApproach=true;birthdayBubbleVisit=ReadPlayer(Actor).visit;});
            for(int i=0;i<12;i++){
                int index=i;var image=Rect(Board,"Birthday bubble "+i,Vector2.zero,new Vector2(42,42)).gameObject.AddComponent<BirthdayBubble>();image.color=new Color(.8f,.96f,1,.85f);image.raycastTarget=true;
                NavButton(image,()=>BirthdaySend("bubble-pop",birthdayBubbleIds[index].ToString()));birthdayBubbles.Add(image);
            }
            birthdayBrickActions=Rect(safe,"Birthday brick actions",Vector2.zero,new Vector2(500,60));
            BirthdayButton(birthdayBrickActions,"Turn brick",new Vector2(-135,0),new Vector2(240,60),()=>BirthdaySend("brick-turn"));
            BirthdayButton(birthdayBrickActions,"Brick colour",new Vector2(135,0),new Vector2(240,60),()=>BirthdaySend("brick-color"));
        }
        private void TickBirthdayPlayUi(bool visible)
        {
            if(birthdayBubbleMachine==null)return;
            var b=Birthday;birthdayBubbleMachine.gameObject.SetActive(visible);birthdayBrickActions.gameObject.SetActive(visible && b.props.Any(p=>p.holder==Actor && p.kind=="brick"));
            birthdayBubbleSwitch.transform.parent.gameObject.SetActive(visible);birthdayBubbleSwitch.text=b.bubblesOn?"Stop bubbles":"Bubbles!";
            birthdayBrickActions.anchoredPosition=new Vector2(0,-safe.rect.height/2+100);
            if(!visible){foreach(var bubble in birthdayBubbles)bubble.gameObject.SetActive(false);birthdayBubbleApproach=false;return;}
            var art=BirthdayArt.PropView("BubbleMachinActor","front");birthdayBubbleMachine.rectTransform.pivot=art.pivot/256;birthdayBubbleMachine.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;birthdayBubbleMachine.rectTransform.anchoredPosition=ToBoard(BirthdayPlay.BubbleX,BirthdayPlay.BubbleY);
            var control=(RectTransform)birthdayBubbleSwitch.transform.parent;control.anchoredPosition=ToBoard(BirthdayPlay.BubbleX+130,35);control.localScale=Vector3.one*sceneScale;
            var own=ReadPlayer(Actor);
            if(birthdayBubbleApproach){
                if(own.visit!=birthdayBubbleVisit)birthdayBubbleApproach=false;
                else if(Vector2.Distance(new Vector2(own.x,own.y),new Vector2(BirthdayPlay.BubbleX,BirthdayPlay.BubbleY))<60 && !ActionPending){birthdayBubbleApproach=false;destination=null;BirthdaySend("bubbles");}
                else destination=new Vector2(BirthdayPlay.BubbleX,BirthdayPlay.BubbleY);
            }
            double clock=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,b.clock,Time.unscaledTimeAsDouble,Shared);
            for(int i=0;i<12;i++){
                int last=BirthdayPlay.LastBubble(b),id=last-((last-i+12)%12);birthdayBubbleIds[i]=id;var bubble=birthdayBubbles[i];bool active=BirthdayPlay.BubbleVisible(b,id);bubble.gameObject.SetActive(active);if(!active)continue;
                float age=(float)(clock-b.bubblesAt-id*.35);bubble.rectTransform.anchoredPosition=ToBoard(BirthdayPlay.BubbleXAt(id,age),BirthdayPlay.BubbleYAt(id,age));bubble.rectTransform.sizeDelta=Vector2.one*(34+id%3*6)*sceneScale;
            }
        }
        private void AddBirthdayPlayDepth(Action<RectTransform,float,int,string> add)
        {
            if(birthdayBubbleMachine!=null)add(birthdayBubbleMachine.rectTransform,ToBoard(0,BirthdayPlay.BubbleY).y,3,"birthday-bubble-machine");
            foreach(var bubble in birthdayBubbles)add(bubble.rectTransform,-2000,4,bubble.name);
            if(birthdayBubbleSwitch!=null)add((RectTransform)birthdayBubbleSwitch.transform.parent,-3000,4,"birthday-bubble-switch");
        }
        private void ResetBirthdayPlayUi(){birthdayBubbleMachine=null;birthdayBubbleSwitch=null;birthdayBubbles.Clear();birthdayBrickActions=null;birthdayBubbleApproach=false;birthdayBubbleVisit=-1;}
    }
}
