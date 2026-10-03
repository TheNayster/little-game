using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform bubbleRoot,bubbleExtras;
        private BubbleSurface bubbleSurface,bubbleLiquid,bubblePrimaryIcon,bubbleShapeIcon;
        private Image bubblePrimaryArt;
        private Text bubblePrimary,bubbleSize,bubbleShape,bubbleUndo,bubbleHint,bubbleSound,bubbleCalm,bubbleAir;
        private BubbleState bubbleSample;
        private float bubbleSampleAt,bubblePaintAt;
        private bool BubbleOpen=>DiscoveryOpen && discoveryStation==6 && bubbleRoot!=null && bubbleRoot.gameObject.activeSelf;
        private bool BubbleReady=>BubbleOpen && Ready && !applicationPaused && !discoveryPending && !ActionPending;
        public Vector2 BubbleScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,bubbleSurface.rectTransform.TransformPoint(new Vector3(x-500,300-y)));
        private BubbleSurface BubbleDrawing(Transform parent,string name,Vector2 position,Vector2 size,string layer,string icon="")
        {var g=Rect(parent,name,position,size).gameObject.AddComponent<BubbleSurface>();g.Layer=layer;g.Icon=icon;g.raycastTarget=false;return g;}
        private Text BubbleButton(string name,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,string icon="",Transform parent=null)
        {
            var text=IceButton(name,label,position,size,action,parent:parent??bubbleRoot);text.transform.parent.name="Bubbles "+name;
            if(icon!=""){BubbleDrawing(text.transform.parent,"Tool picture",new Vector2(-size.x*.3f,0),new Vector2(84,72),"icon",icon);text.rectTransform.anchoredPosition=new Vector2(size.x*.13f,0);text.rectTransform.sizeDelta=new Vector2(size.x*.63f,size.y-12);}
            return text;
        }
        private void BuildBubbleLab()
        {
            if(SceneSchema<BubbleLab.Schema)return;
            bubbleRoot=Rect(discoveryPanel,"Bubble workshop",new Vector2(65,-20),new Vector2(1080,620));
            var back=HomePicture(bubbleRoot,"Bubble bowl rear",new Vector2(-10,-65),new Vector2(600,450),WorkshopArt.Bowl(0));back.raycastTarget=false;
            bubbleLiquid=BubbleDrawing(bubbleRoot,"Soapy mixture",new Vector2(0,15),new Vector2(1000,600),"liquid");
            var front=HomePicture(bubbleRoot,"Bubble bowl front",new Vector2(-10,-65),new Vector2(600,450),WorkshopArt.Bowl(1));front.raycastTarget=false;
            var water=HomePicture(bubbleRoot,"Water jug",new Vector2(-310,140),new Vector2(150,178),MixingIngredientSprite(2));water.preserveAspect=true;water.raycastTarget=false;
            var soap=HomePicture(bubbleRoot,"Soap bottle",new Vector2(-315,-30),new Vector2(140,165),MixingIngredientSprite(3));soap.preserveAspect=true;soap.raycastTarget=false;
            bubbleSurface=BubbleDrawing(bubbleRoot,"Bubble touch surface",new Vector2(0,15),new Vector2(1000,600),"scene");bubbleSurface.raycastTarget=true;bubbleSurface.CanInteract=()=>BubbleReady;bubbleSurface.Tap=TapBubbleLab;
            bubbleHint=Label(bubbleRoot,"Pour water into your bowl.",26,new Vector2(0,280),new Vector2(780,44));bubbleHint.fontStyle=FontStyle.Bold;
            bubblePrimary=BubbleButton("Next","Water",new Vector2(-282,-238),new Vector2(264,96),()=>BubbleNext(),"water");
            bubblePrimaryIcon=bubblePrimary.transform.parent.GetComponentsInChildren<BubbleSurface>().Single();
            bubblePrimaryArt=HomePicture(bubblePrimary.transform.parent,"Ingredient icon",new Vector2(-79,0),new Vector2(79,78),MixingIngredientSprite(2));bubblePrimaryArt.preserveAspect=true;bubblePrimaryArt.raycastTarget=false;
            bubbleSize=BubbleButton("Size","Big bubble",new Vector2(0,-238),new Vector2(264,96),()=>BubbleSend("size"),"size");
            bubbleShape=BubbleButton("Shape","Square wand",new Vector2(282,-238),new Vector2(264,96),()=>BubbleSend("shape"),"shape");bubbleShapeIcon=bubbleShape.transform.parent.GetComponentsInChildren<BubbleSurface>().Single();
            BubbleButton("Again","Again",new Vector2(-374,230),new Vector2(160,62),()=>BubbleSend("again"));
            bubbleUndo=BubbleButton("Undo","Undo",new Vector2(-204,230),new Vector2(160,62),()=>BubbleSend("undo"));
            BubbleButton("More","More to try",new Vector2(353,230),new Vector2(238,62),()=>{bubbleSurface.Cancel();bubbleExtras.gameObject.SetActive(!bubbleExtras.gameObject.activeSelf);});
            bubbleExtras=Panel(bubbleRoot,"Bubble extras",new Vector2(0,75),new Vector2(780,210),new Color(.89f,.96f,.91f,.98f),true).rectTransform;
            bubbleAir=BubbleButton("Air","Strong blow",new Vector2(-244,35),new Vector2(244,80),()=>BubbleSend("air"),"air",bubbleExtras);
            bubbleSound=BubbleButton("Sound","Sound on",new Vector2(0,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-sound"),MixingSound?0:1);PlayerPrefs.Save();if(!MixingSound)mixingAudio.Stop();PresentDiscovery();},parent:bubbleExtras);
            bubbleCalm=BubbleButton("Calm","Calm effects",new Vector2(244,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-calm"),MixingCalm?0:1);PlayerPrefs.Save();PresentDiscovery();},parent:bubbleExtras);
            BubbleButton("Done","Done",new Vector2(0,-58),new Vector2(220,64),()=>bubbleExtras.gameObject.SetActive(false),parent:bubbleExtras);
            bubbleExtras.gameObject.SetActive(false);bubbleRoot.gameObject.SetActive(false);
        }
        private void BubbleNext()
        {
            if(!BubbleReady)return;var s=OwnDiscovery.bubbles[0].current;var op=BubbleLab.Next(s);BubbleSend(op=="pop"?"pop:"+s.floating[0].id:op);
        }
        private void TapBubbleLab(Vector2 point)
        {
            if(!BubbleReady || bubbleSample==null)return;
            var clock=bubbleSample.clock+Math.Min(15,Time.unscaledTime-bubbleSampleAt);
            foreach(var b in bubbleSample.floating.Reverse()){var delta=point-new Vector2((float)BubbleLab.X(b,clock),(float)BubbleLab.Y(b,clock));if(BubbleLab.Alive(b,clock) && delta.sqrMagnitude<(b.radius+14)*(b.radius+14)){BubbleSend("pop:"+b.id);return;}}
            var s=OwnDiscovery.bubbles[0].current;
            if(point.x<285 && point.y>85 && point.y<265){if(!s.water)BubbleSend("water");return;}
            if(point.x<280 && point.y>=265 && point.y<435){if(s.water && !s.soap)BubbleSend("soap");return;}
            if(point.x>320 && point.x<675 && point.y>255 && point.y<480){if(s.water && s.soap)BubbleSend(!s.mixed?"stir":s.film==0 && s.solution>0?"dip":"");return;}
            if(point.x>685 && point.x<900 && point.y>120 && point.y<445){if(s.mixed)BubbleSend(s.film>0?"blow":s.solution>0?"dip":"refill");}
        }
        private void BubbleSend(string op)
        {
            if(!BubbleReady || op=="")return;var t=OwnDiscovery.bubbles[0];var token=BubbleLab.Token(t);var popped=op.StartsWith("pop:")?t.current.floating.FirstOrDefault(b=>b.id.ToString()==op.Substring(4)):null;
            var intent=++discoveryIntent;discoveryPending=true;
            void Done(SoloResult result){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;
                discoveryFeedback=result.Accepted || result.Outcome=="bubble-gone"?"":result.Outcome=="bubble-tray-full"?"Tap a bubble to pop it!":"Try the picture button.";discoveryFeedbackUntil=Time.unscaledTime+2;
                if(result.Accepted && BubbleOpen){bubbleSurface.Effect=bubbleLiquid.Effect=popped!=null?"pop":op;bubbleSurface.EffectAt=bubbleLiquid.EffectAt=Time.unscaledTime;
                    if(popped!=null){var clock=bubbleSample.clock+Math.Min(15,Time.unscaledTime-bubbleSampleAt);bubbleSurface.PopAt=new Vector2((float)BubbleLab.X(popped,clock),(float)BubbleLab.Y(popped,clock));bubbleSurface.PopRadius=popped.radius;}
                    if(MixingSound){mixingAudio.clip=op=="water" || op=="soap"?mixingPour:mixingTap;mixingAudio.Play();}}
                if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,token,"bubble:"+op,0,0,Done);else Done(Command(SoloAction.Discovery,Actor,token,"bubble:"+op));
        }
        private void PresentBubbleLab(DiscoveryWorkspace own,Vector2 size)
        {
            if(bubbleRoot==null)return;bubbleRoot.gameObject.SetActive(discoveryStation==6);if(!BubbleOpen){CloseBubbleLab();return;}
            bubbleRoot.localScale=Vector3.one*Mathf.Min((size.x-174)/1080,(size.y-140)/620);
            var t=own.bubbles[0];var s=t.current;
            if(!ReferenceEquals(s,bubbleSample)){bubbleSample=s;bubbleSampleAt=Time.unscaledTime;}
            bubbleSurface.State=bubbleLiquid.State=s;bubbleSurface.Calm=bubbleLiquid.Calm=MixingCalm;
            var op=BubbleLab.Next(s);var label=op=="water"?"Water":op=="soap"?"Soap":op=="stir"?"Stir":op=="dip"?"Dip":op=="blow"?"Blow!":op=="pop"?"Pop!":"New mix";
            bubblePrimary.text=label;bubblePrimaryIcon.Icon=op;bubblePrimaryIcon.SetVerticesDirty();bubblePrimaryArt.gameObject.SetActive(op=="water" || op=="soap");bubblePrimaryIcon.gameObject.SetActive(op!="water" && op!="soap");if(op=="water" || op=="soap")bubblePrimaryArt.sprite=MixingIngredientSprite(op=="water"?2:3);
            bubblePrimary.transform.parent.GetComponentInChildren<IceRescueSurface>().color=new Color(.53f,.83f,.77f);
            bubbleSize.text=s.big?"Little ones":"Big bubble";bubbleShape.text=s.square?"Round wand":"Square wand";bubbleShapeIcon.State=new BubbleState{square=!s.square};bubbleShapeIcon.SetVerticesDirty();
            bubbleSize.transform.parent.gameObject.SetActive(s.mixed);bubbleShape.transform.parent.gameObject.SetActive(s.mixed);
            foreach(var b in new[]{bubblePrimary,bubbleSize,bubbleShape,bubbleAir})b.transform.parent.GetComponent<Button>().interactable=BubbleReady;
            bubbleUndo.transform.parent.GetComponent<Button>().interactable=BubbleReady && t.previous.Length>0;
            bubbleHint.text=op=="water"?"Pour water into your bowl.":op=="soap"?"Add a little soap.":op=="stir"?"Stir your soap and water.":op=="dip"?"Dip the wand into your mixture.":op=="blow"?"Blow bubbles! Tap them to pop.":op=="pop"?"Tap a bubble — pop!":"Make a fresh bowl of bubble mixture.";
            if(discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil)bubbleHint.text=discoveryFeedback;
            bubbleSound.text=MixingSound?"Sound on":"Sound off";bubbleCalm.text=MixingCalm?"Calm effects":"Gentle effects";bubbleAir.text=s.strong?"Soft blow":"Strong blow";
            bubbleSurface.SetVerticesDirty();bubbleLiquid.SetVerticesDirty();
        }
        private void TickBubbleLab()
        {
            if(!BubbleOpen || bubbleSample==null)return;
            if(Time.unscaledTime<bubblePaintAt)return;bubblePaintAt=Time.unscaledTime+.033f;
            bubbleSurface.Clock=bubbleSample.clock+Math.Min(15,Time.unscaledTime-bubbleSampleAt);bubbleSurface.SetVerticesDirty();
            if(Time.unscaledTime-bubbleLiquid.EffectAt<1)bubbleLiquid.SetVerticesDirty();
        }
        private void CloseBubbleLab(){bubbleSurface?.Cancel();if(bubbleExtras!=null)bubbleExtras.gameObject.SetActive(false);bubbleSample=null;}
        private void ResetBubbleLab(){CloseBubbleLab();bubbleRoot=null;bubbleSurface=null;bubbleLiquid=null;}
    }
}
