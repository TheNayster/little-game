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
        private RectTransform iceRoot,iceExtras;
        private IceRescueSurface iceSurface;
        private Image iceDinosaur;
        private Text iceChip,iceWater,iceChoose,iceHint,iceAgain,iceUndo,iceWarm,iceSound,iceCalm;
        private bool iceUseWater,iceWarmWater=true,iceArtLoading;
        private int iceArtGeneration;
        private readonly List<Sprite> iceSprites=new List<Sprite>();
        private readonly List<BookLease> iceLeases=new List<BookLease>();
        private bool IceOpen=>DiscoveryOpen && discoveryStation==5 && iceRoot!=null && iceRoot.gameObject.activeSelf;
        private bool IceReady=>IceOpen && Ready && !applicationPaused && !discoveryPending && !ActionPending;
        public Vector2 IceScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,iceSurface.rectTransform.TransformPoint(new Vector3(x-500,300-y)));
        private IceRescueSurface IceDrawing(Transform parent,string name,Vector2 position,Vector2 size,string icon)
        {var g=Rect(parent,name,position,size).gameObject.AddComponent<IceRescueSurface>();g.Icon=icon;g.raycastTarget=false;return g;}
        private Text IceButton(string name,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,string icon="",Transform parent=null)
        {
            var text=Button(parent??iceRoot,label,position,size,action,Color.clear);var root=text.transform.parent;root.name="Ice "+name;
            var card=IceDrawing(root,"Rounded picture card",Vector2.zero,size,"card");card.color=Cream;card.transform.SetAsFirstSibling();root.GetComponent<Button>().targetGraphic=card;
            if(icon!=""){IceDrawing(root,"Tool picture",new Vector2(-size.x*.3f,0),new Vector2(82,68),icon);text.rectTransform.anchoredPosition=new Vector2(size.x*.13f,0);text.rectTransform.sizeDelta=new Vector2(size.x*.65f,size.y-12);}
            text.fontSize=23;text.fontStyle=FontStyle.Bold;return text;
        }
        private void BuildIceRescue()
        {
            if(SceneSchema<IceRescue.Schema)return;
            iceRoot=Rect(discoveryPanel,"Dinosaur ice rescue",new Vector2(65,-20),new Vector2(1080,620));
            IceDrawing(iceRoot,"Rescue tray",new Vector2(0,15),new Vector2(1000,600),"tray");
            iceDinosaur=HomePicture(iceRoot,"Rescue dinosaur",new Vector2(0,-15),new Vector2(330,248),null);iceDinosaur.preserveAspect=true;iceDinosaur.raycastTarget=false;iceDinosaur.gameObject.SetActive(false);
            iceSurface=IceDrawing(iceRoot,"Tap and chip ice",new Vector2(0,15),new Vector2(1000,600),"");iceSurface.raycastTarget=true;
            iceSurface.CanInteract=()=>IceReady;iceSurface.Touch=(p,drag)=>{if(p.x<260 || p.x>740 || p.y<125 || p.y>465)return;IceSend(iceUseWater?(iceWarmWater?"water-warm":"water-cool"):"chip",p.x,p.y);};
            iceSurface.MoveToy=p=>IceSend("move",p.x,p.y);
            iceHint=Label(iceRoot,"Tap the ice with your little hammer.",26,new Vector2(0,280),new Vector2(780,45));iceHint.fontStyle=FontStyle.Bold;
            iceChip=IceButton("Chip","Chip!",new Vector2(-282,-238),new Vector2(264,96),()=>{iceUseWater=false;IceNext();},"hammer");
            iceWater=IceButton("Water","Water",new Vector2(0,-238),new Vector2(264,96),()=>{iceUseWater=true;IceNext();},"water");
            iceChoose=IceButton("Dinosaur","Dinosaur",new Vector2(282,-238),new Vector2(264,96),()=>IceSend("dinosaur"),"dinosaur");
            IceButton("More","More to try",new Vector2(353,230),new Vector2(238,62),()=>{iceSurface.Cancel();iceExtras.gameObject.SetActive(!iceExtras.gameObject.activeSelf);});
            iceAgain=IceButton("Again","Again",new Vector2(-374,230),new Vector2(160,62),()=>IceSend("again"));
            iceUndo=IceButton("Undo","Undo",new Vector2(-204,230),new Vector2(160,62),()=>IceSend("undo"));
            iceExtras=Panel(iceRoot,"Extra rescue tools",new Vector2(0,75),new Vector2(780,210),new Color(.89f,.96f,.91f,.98f),true).rectTransform;
            iceWarm=IceButton("Temperature","Warm water",new Vector2(-244,35),new Vector2(244,80),()=>{iceWarmWater=!iceWarmWater;PresentDiscovery();},"water",iceExtras);
            iceSound=IceButton("Sound","Sound on",new Vector2(0,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-sound"),MixingSound?0:1);PlayerPrefs.Save();if(!MixingSound)mixingAudio.Stop();PresentDiscovery();},parent:iceExtras);
            iceCalm=IceButton("Calm","Gentle",new Vector2(244,35),new Vector2(236,80),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-calm"),MixingCalm?0:1);PlayerPrefs.Save();PresentDiscovery();},parent:iceExtras);
            IceButton("Done","Done",new Vector2(0,-58),new Vector2(220,64),()=>iceExtras.gameObject.SetActive(false),parent:iceExtras);
            iceExtras.gameObject.SetActive(false);iceRoot.gameObject.SetActive(false);
        }
        private void IceNext(){if(!IceReady)return;var index=IceRescue.Next(OwnDiscovery.ice[0].current);if(index>=0)IceSend(iceUseWater?(iceWarmWater?"water-warm":"water-cool"):"chip",IceRescue.X(index),IceRescue.Y(index));}
        private void IceSend(string op,float x=0,float y=0)
        {
            if(!IceReady)return;var token=IceRescue.Token(OwnDiscovery.ice[0]);var intent=++discoveryIntent;discoveryPending=true;
            void Done(SoloResult result){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;
                if(result.Accepted && IceOpen){if(op=="chip" || op.StartsWith("water-")){iceSurface.Strike=new Vector2(x,y);iceSurface.StrikeTime=Time.unscaledTime;iceSurface.Water=op!="chip";if(MixingSound){mixingAudio.clip=op=="chip"?mixingTap:mixingPour;mixingAudio.Play();}}}
                discoveryFeedback=result.Accepted?"":result.Outcome=="ice-changed"?"Try that tap once more.":"Try a piece of ice.";discoveryFeedbackUntil=Time.unscaledTime+2;if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,token,"ice:"+op,x,y,Done);else Done(Command(SoloAction.Discovery,Actor,token,"ice:"+op,x,y));
        }
        private IEnumerator LoadIceArt(int generation,BookLease first,BookLease second)
        {
            foreach(var lease in new[]{first,second}){yield return lease.request;CompleteBookLoad(lease);}
            if(generation!=iceArtGeneration || !IceOpen)yield break;
            var content=JsonUtility.FromJson<BookContent>(Resources.Load<TextAsset>("Books/hello-dinosaurs/content").text);
            foreach(var index in new[]{0,1,6}){var texture=(index<6?first:second).asset as Texture2D;if(texture==null)yield break;var r=content.artRects[index];iceSprites.Add(Sprite.Create(texture,new Rect(r.x*texture.width,(1-r.y-r.height)*texture.height,r.width*texture.width,r.height*texture.height),new Vector2(.5f,.5f)));}
            PresentDiscovery();
        }
        private void PresentIceRescue(DiscoveryWorkspace own,Vector2 size)
        {
            if(iceRoot==null)return;iceRoot.gameObject.SetActive(discoveryStation==5);if(!IceOpen){CloseIceRescue();return;}
            if(!iceArtLoading){iceArtLoading=true;iceLeases.Add(AcquireBook("Books/hello-dinosaurs/dinosaurs"));iceLeases.Add(AcquireBook("Books/hello-dinosaurs/dinosaurs-extra"));StartCoroutine(LoadIceArt(++iceArtGeneration,iceLeases[0],iceLeases[1]));}
            iceRoot.localScale=Vector3.one*Mathf.Min((size.x-174)/1080,(size.y-140)/620);
            var t=own.ice[0];var s=t.current;iceSurface.State=s;iceSurface.Calm=MixingCalm;iceSurface.SetVerticesDirty();
            iceDinosaur.gameObject.SetActive(iceSprites.Count==3);if(iceSprites.Count==3)iceDinosaur.sprite=iceSprites[s.toy];iceDinosaur.rectTransform.anchoredPosition=new Vector2(Mathf.Clamp(s.x,IceRescue.ToyMinX,IceRescue.ToyMaxX)-500,315-Mathf.Clamp(s.y,IceRescue.ToyMinY,IceRescue.ToyMaxY));
            iceHint.text=s.freed?"You rescued "+new[]{"T. rex","Triceratops","Brontosaurus"}[s.toy]+"! Move your dinosaur around.":iceUseWater?"Pour water and watch the ice melt.":"Tap the ice with your little hammer.";
            if(discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil)iceHint.text=discoveryFeedback;
            foreach(var label in new[]{iceChip,iceWater})label.transform.parent.GetComponent<Button>().interactable=IceReady && !s.freed;
            iceChoose.transform.parent.GetComponent<Button>().interactable=IceReady && s.cells.All(v=>v==1) && s.energy.All(v=>v==0);
            iceUndo.transform.parent.GetComponent<Button>().interactable=IceReady && t.previous.Length>0;iceAgain.transform.parent.GetComponent<Button>().interactable=IceReady;
            iceWarm.text=iceWarmWater?"Warm water":"Cool water";iceSound.text=MixingSound?"Sound on":"Sound off";iceCalm.text=MixingCalm?"Calm effects":"Gentle effects";
            iceChip.transform.parent.GetComponentInChildren<IceRescueSurface>().color=iceUseWater?Cream:new Color(.53f,.83f,.77f);
            iceWater.transform.parent.GetComponentInChildren<IceRescueSurface>().color=iceUseWater?new Color(.53f,.83f,.77f):Cream;
        }
        private void CloseIceRescue()
        {
            iceSurface?.Cancel();if(iceExtras!=null)iceExtras.gameObject.SetActive(false);
            if(!iceArtLoading)return;iceArtGeneration++;iceArtLoading=false;if(iceDinosaur!=null){iceDinosaur.sprite=null;iceDinosaur.gameObject.SetActive(false);}
            foreach(var sprite in iceSprites)Destroy(sprite);iceSprites.Clear();foreach(var item in iceLeases){var lease=item;ReleaseBook(ref lease);}iceLeases.Clear();
            if(mixingAudio!=null)mixingAudio.Stop();
        }
        private void ResetIceRescue(){CloseIceRescue();iceRoot=null;iceSurface=null;iceDinosaur=null;}
    }
}
