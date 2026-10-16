using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        readonly Queue<(int x,int y,int colour)> fairPaintPending=new Queue<(int,int,int)>();
        void FairClose(){fairPanelKind="";fairHookGesture=null;fairPaintPending.Clear();if(fairPanel)fairPanel.gameObject.SetActive(false);}
        void FairOpen(string kind)
        {
            if(!fairPanel)return;destination=null;foreach(var go in fairPanelItems)if(go)Destroy(go);fairPanelItems.Clear();fairFlowers.Clear();fairPaintCells.Clear();fairPanelKind=kind;fairPanel.gameObject.SetActive(true);
            fairPanel.GetComponent<Image>().color=kind=="paint"?new Color(.59f,.82f,.9f):new Color(1,.97f,.9f,.99f);
            void Own(Component c)=>fairPanelItems.Add(c.gameObject);
            void Text(string text,Vector2 p,Vector2 size,int font=24)=>Own(Label(fairPanel,text,font,p,size));
            void ButtonAt(string title,Vector2 p,Action action,Vector2? size=null){var t=Button(fairPanel,title,p,size??new Vector2(155,58),()=>action(),new Color(.85f,.92f,1));Own(t.transform.parent);t.transform.parent.name="Funfair activity "+title;}
            Text(kind=="cast"?"Choose your character":kind=="hook"?"Hook a Flower":kind=="paint"?"Paint together":kind=="slide"?"Spiral slide":kind=="smoothie"?"Make a smoothie":"Our Funfair prizes",new Vector2(0,280),new Vector2(700,48),30);ButtonAt("Close",new Vector2(440,280),FairClose,new Vector2(130,52));fairPanelStatus=Label(fairPanel,"",21,new Vector2(0,-255),new Vector2(1000,50));Own(fairPanelStatus);
            if(kind=="cast"){
                for(int i=0;i<PeppaFunfair.Roles.Length;i++){string role=PeppaFunfair.Roles[i];var image=FairImage("Funfair choose "+role,fairPanel);var a=FunfairArt.Action(role,"idle");image.texture=FunfairArt.Texture(a.texture);image.uvRect=BirthdayArt.Uv(a,0);image.rectTransform.anchoredPosition=new Vector2(-330+i%3*330,160-i/3*240);image.rectTransform.sizeDelta=Vector2.one*230;Own(image);ButtonAt(role,new Vector2(-330+i%3*330,65-i/3*240),()=>FairSend("role",target:role,done:r=>{if(r.Accepted)FairClose();}),new Vector2(220,55));}
            }else if(kind=="hook"){
                var scenery=FairImage("Funfair fishing scenery",fairPanel);scenery.texture=FunfairArt.Texture("world-1.png");scenery.rectTransform.sizeDelta=new Vector2(1090,550);Own(scenery);scenery.transform.SetAsFirstSibling();
                var pond=Panel(fairPanel,"Funfair shared flower pond",new Vector2(0,-145),new Vector2(950,160),Color.clear,true);Own(pond);
                var drag=pond.gameObject.AddComponent<FunfairPondInput>();drag.Aim=(x,y,pull)=>fairHookGesture=()=>{var h=Fair;if(h.hookActor!=Actor||h.hookStage!="aim"||ActionPending)return;int target=Enumerable.Range(0,15).OrderBy(i=>Mathf.Abs(PeppaFunfair.FlowerX(h.clock,i)-x)+Mathf.Abs(PeppaFunfair.FlowerY(h.clock,i)-y)).First();FairSend(pull?"hook-pull":"hook-aim",pull?target.ToString():"",pull?h.hookRound.ToString():"",x,y);};
                for(int i=0;i<15;i++){int target=i;var image=FairImage("Funfair flower "+i,pond.transform);image.texture=FunfairArt.Texture("Props/flower.png");image.rectTransform.sizeDelta=Vector2.one*78;image.raycastTarget=true;fairFlowers.Add(image);NavButton(image,()=>{var h=Fair;FairSend("hook-pull",target.ToString(),h.hookRound.ToString(),PeppaFunfair.FlowerX(h.clock,target),PeppaFunfair.FlowerY(h.clock,target));});}
                fairPanelPerson=FairImage("Funfair fishing character",fairPanel);fairPanelPerson.rectTransform.anchoredPosition=new Vector2(0,0);fairPanelPerson.rectTransform.sizeDelta=Vector2.one*280;Own(fairPanelPerson);
                fairPanelRod=FairImage("Funfair fishing rod closeup",fairPanel);fairPanelRod.texture=FunfairArt.Texture("Props/rod.png");fairPanelRod.uvRect=new Rect(1,0,-1,1);Own(fairPanelRod);
                fairFishingLine=Plain(fairPanel,"Funfair connected fishing line",Vector2.zero,Vector2.one,new Color(.9f,.98f,1));fairFishingLine.raycastTarget=false;Own(fairFishingLine);
                fairReward=FairImage("Funfair won prize",fairPanel);fairReward.rectTransform.anchoredPosition=new Vector2(240,80);fairReward.rectTransform.sizeDelta=Vector2.one*250;Own(fairReward);
            }else if(kind=="paint"){
                fairDraftRevision=-1;
                // Source scene4: a design icon over a two-column/six-colour palette,
                // a round stencil on the right, and a lower-right green tick.
                var canvas=Panel(fairPanel,"Funfair shared face canvas",new Vector2(120,20),new Vector2(420,420),new Color(.59f,.82f,.9f),true);Own(canvas);
                fairPaintHead=FairImage("Funfair shared painted stencil",canvas.transform);fairPaintHead.rectTransform.sizeDelta=Vector2.one*420;
                var stroke=canvas.gameObject.AddComponent<FunfairPaintInput>();stroke.Changed=(x,y)=>{if(fairPaintPending.Count<64)fairPaintPending.Enqueue((x,y,fairColour));};
                fairPaintDetail=FairImage("Funfair original design details",canvas.transform);fairPaintDetail.rectTransform.sizeDelta=Vector2.one*420;
                var outline=FairImage("Funfair original dotted outline",canvas.transform);outline.texture=FunfairArt.Picture("paint-outline");outline.rectTransform.sizeDelta=Vector2.one*420;
                var palette=Panel(fairPanel,"Funfair six colour palette",new Vector2(-330,-85),new Vector2(200,280),Color.white,true,true);Own(palette);
                for(int i=0;i<6;i++){int colour=i+1;var b=Panel(palette.transform,"Funfair colour "+colour,new Vector2(-48+i%2*96,90-i/2*90),Vector2.one*76,FairColours[i],true,true);NavButton(b,()=>fairColour=colour);}
                fairPaintSelection=Plain(palette.transform,"Funfair selected colour",Vector2.zero,Vector2.one,new Color(.18f,.54f,.7f));fairPaintSelection.raycastTarget=false;fairPaintSelection.transform.SetAsFirstSibling();
                fairPaintIcon=FairImage("Funfair activity Next design",fairPanel);fairPaintIcon.rectTransform.anchoredPosition=new Vector2(-330,165);fairPaintIcon.rectTransform.sizeDelta=Vector2.one*170;fairPaintIcon.raycastTarget=true;Own(fairPaintIcon);NavButton(fairPaintIcon,()=>FairSend("paint-style",item:(Fair.paintStyle%3+1).ToString()));
                var accept=FairImage("Funfair activity All done!",fairPanel);accept.texture=FunfairArt.Picture("paint-icon-accept");accept.uvRect=FunfairArt.PictureUv("paint-icon-accept");accept.rectTransform.anchoredPosition=new Vector2(390,-190);accept.rectTransform.sizeDelta=new Vector2(120,90);accept.raycastTarget=true;Own(accept);NavButton(accept,()=>FairSend("paint-done"));
            }else if(kind=="slide"){
                var poses=new[]{"Backwards","On belly","Spinning"};for(int i=0;i<3;i++){int pose=i;ButtonAt(poses[i],new Vector2(-300+i*300,20),()=>FairSend("slide-pose",item:pose.ToString()),new Vector2(240,110));}
            }else if(kind=="smoothie"){
                var fruit=new[]{"Banana","Strawberry","Orange"};for(int i=0;i<3;i++){int f=i;ButtonAt(fruit[i],new Vector2(-300+i*300,100),()=>FairSend("smoothie-fruit",item:f.ToString()),new Vector2(240,100));}ButtonAt("Blend!",new Vector2(-170,-50),()=>FairSend("smoothie-blend"),new Vector2(250,90));ButtonAt("Take a drink",new Vector2(170,-50),()=>FairSend("drink-take"),new Vector2(250,90));
            }else if(kind=="prizes"){
                for(int i=0;i<4;i++){var image=FairImage("Funfair prize "+i,fairPanel);image.texture=FunfairArt.Picture("prize-"+i);image.rectTransform.anchoredPosition=new Vector2(-360+i*240,40);image.rectTransform.sizeDelta=Vector2.one*180;Own(image);}
            }
        }
        void TickFairPanel(FunfairState h,double clock)
        {
            if(fairPanelKind=="")return;var m=h.members.Single(v=>v.actor==Actor);
            if(fairPanelKind=="hook"){
                if(fairHookGesture!=null&&!ActionPending){var gesture=fairHookGesture;fairHookGesture=null;gesture();}
                for(int i=0;i<fairFlowers.Count;i++){var image=fairFlowers[i];image.rectTransform.anchoredPosition=new Vector2((PeppaFunfair.FlowerX(clock,i)-.5f)*950,(PeppaFunfair.FlowerY(clock,i)-.5f)*160);image.gameObject.SetActive(h.hookStage!="pull"||h.hookTarget!=i);}
                var fisher=h.members.FirstOrDefault(v=>v.actor==h.hookActor);fairPanelPerson.gameObject.SetActive(fisher!=null);fairPanelRod.gameObject.SetActive(fisher!=null);fairFishingLine.gameObject.SetActive(fisher!=null);fairReward.gameObject.SetActive(h.hookStage=="reward");
                if(h.hookStage=="reward")fairReward.texture=FunfairArt.Picture("prize-"+(Math.Max(0,h.hookTarget)%4));
                if(fisher!=null){var a=FunfairArt.Action(fisher.role,fisher.pose);int f=(int)(Math.Max(0,clock-fisher.at)%Math.Max(.0833,a.duration)*12)%a.frames;fairPanelPerson.texture=FunfairArt.Texture(a.texture);fairPanelPerson.uvRect=BirthdayArt.Uv(a,f);var hand=fairPanelPerson.rectTransform.anchoredPosition+(a.rightHand[f]-Vector2.one*128)*(280f/256);var rod=FunfairArt.Prop("rod");float scale=rod.unitsPerPixel/a.unitsPerPixel*280;fairPanelRod.rectTransform.pivot=new Vector2(1-rod.holdPivot.x/256,rod.holdPivot.y/256);fairPanelRod.rectTransform.sizeDelta=Vector2.one*scale;fairPanelRod.rectTransform.anchoredPosition=hand;var tip=hand+new Vector2(.6f,.2f)*scale;var target=new Vector2((h.aimX-.5f)*950,-145+(h.aimY-.5f)*160);var delta=target-tip;fairFishingLine.rectTransform.anchoredPosition=(tip+target)*.5f;fairFishingLine.rectTransform.sizeDelta=new Vector2(delta.magnitude,2);fairFishingLine.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
                fairPanelStatus.text=h.hookActor==Actor?h.hookStage=="aim"?"Tap a flower, or drag the hook onto one!":h.hookStage=="reward"?"You won a prize!":"Here comes your catch...":"Waiting together · "+(Array.IndexOf(h.hookQueue,Actor)+1)+" in line";
            }else if(fairPanelKind=="paint"){
                if(fairPaintPending.Count>0&&!ActionPending&&m.station=="paint"&&h.paintActor!=""){var cell=fairPaintPending.Dequeue();FairSend("paint-stroke",cell.colour.ToString(),h.paintRevision.ToString(),cell.x,cell.y,r=>{if(!r.Accepted&&r.Outcome=="stale-paint"&&fairPanelKind=="paint"&&fairPaintPending.Count<64)fairPaintPending.Enqueue(cell);});}
                if(fairDraftRevision!=h.paintRevision){if(fairDraftTexture)Destroy(fairDraftTexture);fairDraftTexture=PaintTexture(h.paintDraft,h.paintStyle,true);fairPaintHead.texture=fairDraftTexture;fairDraftRevision=h.paintRevision;}
                string styleKey=new[]{"tiger","dinosaur","fairy"}[h.paintStyle-1];fairPaintDetail.texture=FunfairArt.Picture("paint-"+styleKey);fairPaintIcon.texture=FunfairArt.Picture("paint-icon-"+styleKey);fairPaintIcon.uvRect=FunfairArt.PictureUv("paint-icon-"+styleKey);
                fairPaintSelection.rectTransform.anchoredPosition=new Vector2(-48+(fairColour-1)%2*96,90-(fairColour-1)/2*90);fairPaintSelection.rectTransform.sizeDelta=Vector2.one*84;
                var recipient=h.members.FirstOrDefault(v=>v.actor==h.paintActor);fairPanelStatus.text=recipient==null?"Choose Face painting to join the queue":"Painting "+recipient.role+" together · tap the picture to change design";
            }else if(fairPanelKind=="slide")fairPanelStatus.text=m.station=="slide"?"Your place in our queue: "+(Array.IndexOf(h.slideQueue,Actor)+1):"Choose Spiral slide to join again";
            else if(fairPanelKind=="smoothie")fairPanelStatus.text=h.smoothieAt>=0?"Blending...":h.smoothieMask==8?"Ready to drink!":"Fruit added: "+new[]{0,1,2}.Count(i=>(h.smoothieMask&(1<<i))!=0)+" / 3";
            else if(fairPanelKind=="cast")fairPanelStatus.text="Each friend chooses a different character";
            else fairPanelStatus.text=string.Join("     ",m.prizes.Select((n,i)=>new[]{"Bear","Football","Candy floss","Balloon"}[i]+": "+n));
        }
    }
    public sealed class FunfairRideInput:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action Dragged;Vector2 last;float next;
        public void OnBeginDrag(PointerEventData e){last=e.pressPosition;next=0;}
        public void OnDrag(PointerEventData e){if((e.position-last).sqrMagnitude<24*24||Time.unscaledTime<next)return;last=e.position;next=Time.unscaledTime+.18f;Dragged?.Invoke();}
        public void OnEndDrag(PointerEventData e){}
    }
    public sealed class FunfairPondInput:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action<float,float,bool> Aim;float next;
        void Send(PointerEventData e,bool pull){if(!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var p))return;var r=((RectTransform)transform).rect;Aim?.Invoke(Mathf.Clamp01((p.x-r.x)/r.width),Mathf.Clamp01((p.y-r.y)/r.height),pull);}
        public void OnBeginDrag(PointerEventData e){next=0;Send(e,false);}
        public void OnDrag(PointerEventData e){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.18f;Send(e,false);}
        public void OnEndDrag(PointerEventData e){Send(e,true);}
    }
    public sealed class FunfairPaintInput:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public Action<int,int> Changed;Vector2Int previous=new Vector2Int(-1,-1);int pointer=int.MinValue;
        void Draw(PointerEventData e){if(!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var p))return;var r=((RectTransform)transform).rect;var cell=new Vector2Int(Mathf.FloorToInt((p.x-r.x)/r.width*8),Mathf.FloorToInt((p.y-r.y)/r.height*8));if(cell.x<0||cell.y<0||cell.x>7||cell.y>7||cell==previous)return;previous=cell;Changed?.Invoke(cell.x,cell.y);}
        public void OnPointerDown(PointerEventData e){if(pointer!=int.MinValue)return;pointer=e.pointerId;previous=new Vector2Int(-1,-1);Draw(e);}public void OnDrag(PointerEventData e){if(pointer==e.pointerId)Draw(e);}public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)pointer=int.MinValue;}
    }
}
