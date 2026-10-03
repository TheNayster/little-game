using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform liquidRoot,liquidExtras;
        private LiquidColorSurface liquidFill,liquidTouch;
        private Text liquidHint,liquidResult,liquidAgain,liquidUndo,liquidWater,liquidSound,liquidCalm;
        private readonly Text[] liquidButtons=new Text[3];
        private static Sprite[] liquidBottles;
        private LiquidColorState liquidSample;
        private float liquidPaintAt;
        private bool LiquidOpen=>DiscoveryOpen && discoveryStation==7 && liquidRoot!=null && liquidRoot.gameObject.activeSelf;
        private bool LiquidReady=>LiquidOpen && Ready && !applicationPaused && !discoveryPending && !ActionPending;
        public Vector2 LiquidScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,liquidTouch.rectTransform.TransformPoint(new Vector3(x-500,300-y)));
        private Sprite LiquidBottle(int color)
        {
            if(liquidBottles==null){var t=WorldResources.Load<Texture2D>("Worlds/Home/Discovery/mixing-colors");var bounds=new[]{new Rect(271,81,270,563),new Rect(1629,81,269,563),new Rect(950,80,269,564)};liquidBottles=new Sprite[3];for(var i=0;i<3;i++){var b=bounds[i];liquidBottles[i]=Sprite.Create(t,new Rect(b.x/2172*t.width,(724-b.y-b.height)/724*t.height,b.width/2172*t.width,b.height/724*t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}}
            return liquidBottles[color];
        }
        private LiquidColorSurface LiquidDrawing(Transform parent,string name,Vector2 pos,Vector2 size,bool front=false)
        {var g=Rect(parent,name,pos,size).gameObject.AddComponent<LiquidColorSurface>();g.Front=front;g.raycastTarget=false;return g;}
        private Text LiquidButton(string name,string label,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,Transform parent=null)
        {var t=IceButton(name,label,pos,size,action,parent:parent??liquidRoot);t.transform.parent.name="Colors "+name;return t;}
        private void BuildLiquidColors()
        {
            if(SceneSchema<LiquidColorLab.Schema)return;
            liquidRoot=Rect(discoveryPanel,"Liquid color lab",new Vector2(65,-20),new Vector2(1080,620));
            var back=HomePicture(liquidRoot,"Color bowl rear",new Vector2(-10,-65),new Vector2(600,450),WorkshopArt.Bowl(0));back.raycastTarget=false;
            liquidFill=LiquidDrawing(liquidRoot,"Blending colored liquid",new Vector2(0,15),new Vector2(1000,600));
            var front=HomePicture(liquidRoot,"Color bowl front",new Vector2(-10,-65),new Vector2(600,450),WorkshopArt.Bowl(1));front.raycastTarget=false;
            for(var i=0;i<3;i++){var pos=LiquidColorSurface.Bottle(i);var art=HomePicture(liquidRoot,LiquidColorLab.Names[i]+" bottle",new Vector2(pos.x-500,315-pos.y),new Vector2(115,i==1?166:206),LiquidBottle(i));art.preserveAspect=true;art.raycastTarget=false;}
            liquidTouch=LiquidDrawing(liquidRoot,"Pour colors by touch",new Vector2(0,15),new Vector2(1000,600),true);liquidTouch.raycastTarget=true;liquidTouch.CanInteract=()=>LiquidReady;liquidTouch.Pour=i=>LiquidSend(LiquidColorLab.Names[i].ToLowerInvariant());
            liquidHint=Label(liquidRoot,"Tap a bottle. Try two colors!",26,new Vector2(0,280),new Vector2(870,44));liquidHint.fontStyle=FontStyle.Bold;
            liquidResult=Label(liquidRoot,"",27,new Vector2(0,-198),new Vector2(760,34));liquidResult.fontStyle=FontStyle.Bold;
            for(var i=0;i<3;i++){var index=i;var t=LiquidButton(LiquidColorLab.Names[i],LiquidColorLab.Names[i],new Vector2((i-1)*282,-265),new Vector2(264,88),()=>LiquidSend(LiquidColorLab.Names[index].ToLowerInvariant()));liquidButtons[i]=t;var art=HomePicture(t.transform.parent,"Bottle picture",new Vector2(-76,0),new Vector2(46,72),LiquidBottle(i));art.preserveAspect=true;art.raycastTarget=false;t.rectTransform.anchoredPosition=new Vector2(31,0);t.rectTransform.sizeDelta=new Vector2(180,64);t.transform.parent.GetComponentInChildren<IceRescueSurface>().color=Color.Lerp(Cream,LiquidColorSurface.Hue(i),.13f);}
            liquidAgain=LiquidButton("Again","Again",new Vector2(-374,230),new Vector2(160,62),()=>LiquidSend("again"));
            liquidUndo=LiquidButton("Undo","Undo",new Vector2(-204,230),new Vector2(160,62),()=>LiquidSend("undo"));
            LiquidButton("More","More to try",new Vector2(353,230),new Vector2(238,62),()=>{liquidTouch.Cancel();liquidExtras.gameObject.SetActive(!liquidExtras.gameObject.activeSelf);});
            liquidExtras=Panel(liquidRoot,"Liquid color extras",new Vector2(0,75),new Vector2(780,210),new Color(.89f,.96f,.91f,.98f),true).rectTransform;
            liquidWater=LiquidButton("Water","Water",new Vector2(-244,35),new Vector2(244,80),()=>LiquidSend("water"),liquidExtras);var water=HomePicture(liquidWater.transform.parent,"Water picture",new Vector2(-74,0),new Vector2(69,67),MixingIngredientSprite(2));water.preserveAspect=true;water.raycastTarget=false;liquidWater.rectTransform.anchoredPosition=new Vector2(35,0);liquidWater.rectTransform.sizeDelta=new Vector2(155,64);
            liquidSound=LiquidButton("Sound","Sound on",new Vector2(0,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-sound"),MixingSound?0:1);PlayerPrefs.Save();if(!MixingSound)mixingAudio.Stop();PresentDiscovery();},liquidExtras);
            liquidCalm=LiquidButton("Calm","Calm effects",new Vector2(244,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-calm"),MixingCalm?0:1);PlayerPrefs.Save();PresentDiscovery();},liquidExtras);
            LiquidButton("Done","Done",new Vector2(0,-58),new Vector2(220,64),()=>liquidExtras.gameObject.SetActive(false),liquidExtras);
            liquidExtras.gameObject.SetActive(false);liquidRoot.gameObject.SetActive(false);
        }
        private void LiquidSend(string op)
        {
            if(!LiquidReady)return;var token=LiquidColorLab.Token(OwnDiscovery.liquid[0]);var intent=++discoveryIntent;discoveryPending=true;
            void Done(SoloResult result){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;discoveryFeedback=result.Accepted?"":result.Outcome=="color-beaker-full"?"Your bowl is full. Try Again or Undo.":"Try your color again.";discoveryFeedbackUntil=Time.unscaledTime+2;
                if(result.Accepted && LiquidOpen){liquidTouch.PourIndex=op=="red"?0:op=="yellow"?1:op=="blue"?2:op=="water"?3:-1;if(MixingSound){mixingAudio.clip=liquidTouch.PourIndex>=0?mixingPour:mixingTap;mixingAudio.Play();}}
                if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,token,"liquid:"+op,0,0,Done);else Done(Command(SoloAction.Discovery,Actor,token,"liquid:"+op));
        }
        private void PresentLiquidColors(DiscoveryWorkspace own,Vector2 size)
        {
            if(liquidRoot==null)return;liquidRoot.gameObject.SetActive(discoveryStation==7);if(!LiquidOpen){CloseLiquidColors();return;}
            liquidRoot.localScale=Vector3.one*Mathf.Min((size.x-174)/1080,(size.y-140)/620);
            var t=own.liquid[0];var s=t.current;var volume=LiquidColorLab.Volume(s);
            if(liquidSample==null || liquidSample.water!=s.water || !System.Linq.Enumerable.SequenceEqual(liquidSample.parts,s.parts)){
                var from=liquidSample??s;var shown=liquidSample==null?1:liquidFill.Blend;liquidFill.FromColor=Color.Lerp(liquidFill.FromColor,LiquidColorSurface.Mixture(from),shown);liquidFill.FromVolume=Mathf.Lerp(liquidFill.FromVolume,LiquidColorLab.Volume(from),shown);liquidFill.ChangedAt=liquidTouch.ChangedAt=liquidSample==null?-20:Time.unscaledTime;liquidSample=s;
            }
            liquidFill.State=liquidTouch.State=s;liquidFill.Calm=liquidTouch.Calm=MixingCalm;
            foreach(var b in liquidButtons)b.transform.parent.GetComponent<Button>().interactable=LiquidReady && volume<LiquidColorLab.Capacity;
            liquidWater.transform.parent.GetComponent<Button>().interactable=LiquidReady && volume<LiquidColorLab.Capacity;
            liquidAgain.transform.parent.GetComponent<Button>().interactable=LiquidReady && volume>0;liquidUndo.transform.parent.GetComponent<Button>().interactable=LiquidReady && t.previous.Length>0;
            liquidHint.text=volume==12?"Your bowl is full. Again makes room for new colors.":"Tap a bottle. Try two colors!";
            if(discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil)liquidHint.text=discoveryFeedback;
            liquidResult.text=LiquidColorLab.Name(s);liquidSound.text=MixingSound?"Sound on":"Sound off";liquidCalm.text=MixingCalm?"Calm effects":"Gentle effects";
            liquidFill.SetVerticesDirty();liquidTouch.SetVerticesDirty();
        }
        private void TickLiquidColors(){if(!LiquidOpen || Time.unscaledTime<liquidPaintAt || Time.unscaledTime-liquidFill.ChangedAt>1)return;liquidPaintAt=Time.unscaledTime+.033f;liquidFill.SetVerticesDirty();liquidTouch.SetVerticesDirty();}
        private void CloseLiquidColors(){liquidTouch?.Cancel();if(liquidExtras!=null)liquidExtras.gameObject.SetActive(false);liquidSample=null;}
        private void ResetLiquidColors(){CloseLiquidColors();liquidRoot=null;liquidFill=null;liquidTouch=null;}
    }
}
