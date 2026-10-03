using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform rampRoot,rampBoard,rampExtras;
        private RampSurface rampTouch;
        private Image[] rampTracks,rampStops,rampKnobs;
        private Image rampBall,rampSurfacePicture;
        private Text rampHint,rampRoll,rampSurfaceButton,rampKeep,rampUndo,rampRestore,rampSound,rampCalm;
        private RampState rampSample;
        private float rampSampleAt,rampPaintAt;
        private bool rampCaught;
        private static Sprite[] rampSprites;
        private bool RampsOpen=>DiscoveryOpen && discoveryStation==8 && rampRoot!=null && rampRoot.gameObject.activeSelf;
        private bool RampsReady=>RampsOpen && Ready && !applicationPaused && !discoveryPending && !ActionPending;
        public Vector2 RampScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,rampBoard.TransformPoint(new Vector3(x-500,300-y)));
        private static Sprite RampArt(int index)
        {
            if(rampSprites==null){var t=WorldResources.Load<Texture2D>("Worlds/Home/Discovery/marble-ramps");
                // The rear basket crop excludes its painted front wall, which
                // is a separate foreground layer that can cover the marble.
                var boxes=new[]{new Rect(48,69,467,240),new Rect(543,159,452,72),new Rect(1044,162,452,76),new Rect(49,473,461,76),new Rect(552,417,447,130),new Rect(1043,473,459,85),new Rect(122,658,292,288),new Rect(635,661,261,275),new Rect(1233,659,88,294)};
                rampSprites=boxes.Select(b=>Sprite.Create(t,new Rect(b.x/1536*t.width,(1024-b.y-b.height)/1024*t.height,b.width/1536*t.width,b.height/1024*t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)).ToArray();}
            return rampSprites[index];
        }
        private Text RampButton(string name,string label,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,Transform parent=null)
        {var t=IceButton(name,label,p,size,action,parent:parent??rampRoot);t.transform.parent.name="Ramps "+name;return t;}
        private Image RampPicture(Transform parent,string name,int art,Vector2 p,Vector2 size)
        {var image=HomePicture(parent,name,p,size,RampArt(art));image.raycastTarget=false;return image;}
        private void BuildMarbleRamps()
        {
            if(SceneSchema<MarbleRamps.Schema)return;
            rampRoot=Rect(discoveryPanel,"Marble ramps",new Vector2(65,-20),new Vector2(1080,640));
            rampBoard=Rect(rampRoot,"Adjust the ramps",new Vector2(0,-5),new Vector2(1000,600));rampBoard.localScale=Vector3.one*.8f;
            RampPicture(rampBoard,"Empty pegboard",0,new Vector2(0,-10),new Vector2(925,555));
            rampTracks=Enumerable.Range(0,3).Select(i=>RampPicture(rampBoard,"Working ramp "+i,1,Vector2.zero,new Vector2(550,36))).ToArray();
            rampStops=Enumerable.Range(0,2).Select(i=>RampPicture(rampBoard,"Catch stop "+i,8,Vector2.zero,new Vector2(18,125))).ToArray();
            RampPicture(rampBoard,"Basket rear",4,new Vector2(250,-226),new Vector2(230,67));
            rampKnobs=Enumerable.Range(0,6).Select(i=>RampPicture(rampBoard,"Ramp handle "+i,7,Vector2.zero,new Vector2(48,48))).ToArray();
            rampBall=RampPicture(rampBoard,"One shared marble",6,Vector2.zero,new Vector2(35,35));
            RampPicture(rampBoard,"Basket front",5,new Vector2(250,-260),new Vector2(230,42));
            rampTouch=Rect(rampBoard,"Ramp touch surface",Vector2.zero,new Vector2(1000,600)).gameObject.AddComponent<RampSurface>();rampTouch.raycastTarget=true;
            rampTouch.CanInteract=()=>RampsReady && !rampExtras.gameObject.activeSelf;rampTouch.CurrentToken=()=>MarbleRamps.Token(OwnDiscovery.ramps[0]);rampTouch.Commit=(i,p,token)=>RampSend("move:"+i,p.x,p.y,token);
            rampHint=Label(rampRoot,"Roll, then try moving a gold handle.",26,new Vector2(0,287),new Vector2(950,44));rampHint.fontStyle=FontStyle.Bold;
            rampRoll=RampButton("Roll","Roll!",new Vector2(0,-282),new Vector2(270,90),()=>RampSend("roll"));
            var ball=RampPicture(rampRoll.transform.parent,"Marble picture",6,new Vector2(-85,0),new Vector2(54,54));rampRoll.rectTransform.anchoredPosition=new Vector2(32,0);rampRoll.rectTransform.sizeDelta=new Vector2(185,72);
            rampRoll.transform.parent.GetComponentInChildren<IceRescueSurface>().color=new Color(.53f,.83f,.77f);
            rampSurfaceButton=RampButton("Surface","Wood",new Vector2(-320,-282),new Vector2(270,90),()=>RampSend("surface:"+((OwnDiscovery.ramps[0].current.course.surface+1)%3)));
            rampSurfacePicture=RampPicture(rampSurfaceButton.transform.parent,"Surface picture",1,new Vector2(-85,0),new Vector2(80,28));rampSurfaceButton.rectTransform.anchoredPosition=new Vector2(34,0);rampSurfaceButton.rectTransform.sizeDelta=new Vector2(182,72);
            rampKeep=RampButton("Keep","Keep course",new Vector2(320,-282),new Vector2(270,90),()=>RampSend("keep"));ColorControl(rampKeep,"check");
            rampUndo=RampButton("Undo","Undo edit",new Vector2(-345,230),new Vector2(230,62),()=>RampSend("undo"));
            RampButton("More","More to try",new Vector2(345,230),new Vector2(238,62),()=>{rampTouch.Cancel();rampExtras.gameObject.SetActive(!rampExtras.gameObject.activeSelf);});
            rampExtras=Panel(rampRoot,"More ramp tools",new Vector2(0,50),new Vector2(970,218),new Color(.89f,.96f,.91f,.98f),true).rectTransform;
            RampButton("Starter","Starter course",new Vector2(-351,40),new Vector2(224,80),()=>RampSend("starter"),rampExtras);
            rampRestore=RampButton("Restore","Use kept course",new Vector2(-117,40),new Vector2(224,80),()=>RampSend("restore"),rampExtras);
            void Nudge(int amount){var i=rampTouch.Selected;var c=OwnDiscovery.ramps[0].current.course;RampSend("move:"+i,c.points[i*2],Mathf.Clamp(c.points[i*2+1]+amount,0,600));}
            RampButton("Higher","Higher",new Vector2(117,40),new Vector2(224,80),()=>Nudge(-20),rampExtras);
            RampButton("Lower","Lower",new Vector2(351,40),new Vector2(224,80),()=>Nudge(20),rampExtras);
            rampSound=RampButton("Sound","Sound on",new Vector2(-244,-57),new Vector2(236,70),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-sound"),MixingSound?0:1);PlayerPrefs.Save();if(!MixingSound)mixingAudio.Stop();PresentDiscovery();},rampExtras);
            rampCalm=RampButton("Calm","Gentle effects",new Vector2(0,-57),new Vector2(236,70),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-calm"),MixingCalm?0:1);PlayerPrefs.Save();PresentDiscovery();},rampExtras);
            RampButton("Done","Done",new Vector2(244,-57),new Vector2(236,70),()=>rampExtras.gameObject.SetActive(false),rampExtras);
            rampExtras.gameObject.SetActive(false);rampRoot.gameObject.SetActive(false);
        }
        private void RampSend(string op,float x=0,float y=0,string gestureToken=null)
        {
            if(!RampsReady)return;var token=gestureToken??MarbleRamps.Token(OwnDiscovery.ramps[0]);var intent=++discoveryIntent;discoveryPending=true;
            void Done(SoloResult r){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;rampTouch.Cancel();discoveryFeedback=r.Accepted?"":"Your course changed. Try that handle again.";discoveryFeedbackUntil=Time.unscaledTime+2;if(r.Accepted && RampsOpen && MixingSound){mixingAudio.clip=mixingTap;mixingAudio.Play();}if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,token,"ramp:"+op,x,y,Done);else Done(Command(SoloAction.Discovery,Actor,token,"ramp:"+op,x,y));
        }
        private void PresentMarbleRamps(DiscoveryWorkspace own,Vector2 size)
        {
            if(rampRoot==null)return;rampRoot.gameObject.SetActive(discoveryStation==8);if(!RampsOpen){CloseMarbleRamps();return;}
            rampRoot.localScale=Vector3.one*Mathf.Min((size.x-174)/1080,(size.y-140)/640);
            var t=own.ramps[0];var s=t.current;if(!ReferenceEquals(s,rampSample)){rampSample=s;rampSampleAt=Time.unscaledTime;rampCaught=s.released && !MarbleRamps.Running(s);}
            if(rampTouch.GestureToken!=null && rampTouch.GestureToken!=MarbleRamps.Token(t))rampTouch.Cancel();
            rampTouch.Course=s.course;rampTouch.Calm=MixingCalm;rampSurfacePicture.sprite=RampArt(s.course.surface+1);
            rampSurfaceButton.text=new[]{"Wood","Felt","Rubber"}[s.course.surface];rampRoll.text=s.released?"Roll again":"Roll!";
            var kept=t.kept.Length>0 && MarbleRamps.Same(s.course,t.kept[0]);rampKeep.text=kept?"Kept!":t.kept.Length>0?"Update kept":"Keep course";
            LayoutColorControl(rampKeep,rampKeep.text,new Vector2(320,-282),new Vector2(270,90),RampsReady && !kept);rampRestore.transform.parent.GetComponent<Button>().interactable=RampsReady && t.kept.Length>0;
            rampUndo.transform.parent.GetComponent<Button>().interactable=RampsReady && t.previous.Length>0;rampRoll.transform.parent.GetComponent<Button>().interactable=rampSurfaceButton.transform.parent.GetComponent<Button>().interactable=RampsReady;
            rampSound.text=MixingSound?"Sound on":"Sound off";rampCalm.text=MixingCalm?"Calm effects":"Gentle effects";PaintMarbleRamps();
        }
        private void PaintMarbleRamps()
        {
            if(rampSample==null)return;var c=rampTouch.Preview??rampSample.course;var p=c.points;
            for(var i=0;i<3;i++){var x=p[i*4];var y=p[i*4+1];var dx=p[i*4+2]-x;var dy=p[i*4+3]-y;var r=rampTracks[i].rectTransform;rampTracks[i].sprite=RampArt(c.surface+1);r.anchoredPosition=new Vector2(x+dx/2f-500,300-y-dy/2f-12);r.sizeDelta=new Vector2(Mathf.Sqrt(dx*dx+dy*dy),36);r.localEulerAngles=new Vector3(0,0,Mathf.Atan2(-dy*Mathf.Sign(dx),Mathf.Abs(dx))*Mathf.Rad2Deg);}
            for(var i=0;i<2;i++)rampStops[i].rectTransform.anchoredPosition=new Vector2(p[(i+1)*4]+(i==0?8:-8)-500,345-p[(i+1)*4+1]);
            for(var i=0;i<6;i++)rampKnobs[i].rectTransform.anchoredPosition=new Vector2(p[i*2]-500,300-p[i*2+1]);
            var path=MarbleRamps.Path(c);var released=rampSample.released && rampTouch.Preview==null;
            var clock=released?Math.Min(path.Duration,rampSample.elapsed+Math.Max(0,Time.unscaledTime-rampSampleAt)):0;var ball=path.At(clock);
            rampBall.rectTransform.anchoredPosition=new Vector2((float)ball.x-500,300-(float)ball.y);rampBall.rectTransform.localEulerAngles=new Vector3(0,0,MixingCalm?0:-(float)ball.roll);
            rampTouch.Path=path;rampTouch.Clock=clock;rampTouch.Released=released;rampTouch.SetVerticesDirty();
            rampHint.text=!released?"Roll, then try moving a gold handle.":clock<path.Duration?"Watch your marble follow the ramps.":path.Caught?"In the basket! Try another ramp or surface.":"Your marble stopped. Move a handle and try again.";
            if(discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil)rampHint.text=discoveryFeedback;
            if(released && clock>=path.Duration && !rampCaught){rampCaught=true;if(MixingSound && path.Caught){mixingAudio.clip=mixingTap;mixingAudio.Play();}}
        }
        private void TickMarbleRamps(){if(!RampsOpen || Time.unscaledTime<rampPaintAt)return;rampPaintAt=Time.unscaledTime+.033f;PaintMarbleRamps();}
        private void CloseMarbleRamps(){rampTouch?.Cancel();if(rampExtras!=null)rampExtras.gameObject.SetActive(false);rampSample=null;}
        private void ResetMarbleRamps(){CloseMarbleRamps();rampRoot=null;rampBoard=null;rampTouch=null;}
    }
}
