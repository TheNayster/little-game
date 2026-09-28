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
        private RectTransform mixingRoot;
        private MixingSurface mixingSurface;
        private Image mixingVolcano,mixingGhost,mixingBowlBack,mixingBowlFront;
        private readonly List<Image> mixingSupplyCards=new List<Image>();
        private readonly List<Text> mixingModes=new List<Text>();
        private readonly List<Image> mixingSupplyArt=new List<Image>();
        private readonly List<Text> mixingSupplyLabels=new List<Text>();
        private Text mixingInstruction,mixingAmounts,mixingSoundButton,mixingMotionButton,mixingVesselButton,mixingPokeButton;
        private int mixingMode,mixingSelected=1,mixingHeld=-1;
        private bool mixingDragged,mixingOverBowl,mixingSent,mixingSurfaceDrag;
        private Vector2 mixingLastPoint;
        private float mixingNextPour,mixingNextPaint,mixingPourUntil;
        private int mixingPourIngredient;
        private AudioSource mixingAudio;
        private AudioClip mixingFizz,mixingPour,mixingTap;
        private static Sprite[] mixingSprites,mixingColorSprites;
        private bool MixingOpen=>DiscoveryOpen && discoveryStation==4 && mixingRoot!=null && mixingRoot.gameObject.activeSelf;
        private bool MixingSound=>PlayerPrefs.GetInt(PreferenceKey("science-mixing-sound"),1)!=0;
        private bool MixingCalm=>PlayerPrefs.GetInt(PreferenceKey("science-mixing-calm"),0)!=0;
        private bool MixingReady=>MixingOpen && Ready && !applicationPaused && !discoveryPending && !ActionPending;
        public Vector2 MixingScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,mixingRoot.TransformPoint(new Vector3(x,y)));
        private Sprite MixingSprite(int index)
        {
            if(mixingSprites==null){var texture=Resources.Load<Texture2D>("Discovery/mixing-props");
                // Authored bounds avoid neighboring props in the illustration;
                // each food-color button shows its own recognizable bottle.
                var bounds=new[]{new Rect(.058f,.008f,.135f,.343f),new Rect(.279f,.057f,.2f,.278f),new Rect(.526f,.02f,.218f,.323f),new Rect(.802f,.015f,.134f,.337f),new Rect(.039f,.395f,.186f,.278f),new Rect(.291f,.355f,.18f,.323f),new Rect(.559f,.351f,.124f,.338f),new Rect(.771f,.423f,.069f,.219f),new Rect(.016f,.704f,.223f,.292f),new Rect(.258f,.75f,.222f,.225f),new Rect(.496f,.693f,.235f,.299f),new Rect(.744f,.724f,.238f,.247f),new Rect(.895f,.421f,.068f,.222f),new Rect(.831f,.44f,.067f,.222f)};
                mixingSprites=new Sprite[bounds.Length];for(var i=0;i<bounds.Length;i++){var b=bounds[i];mixingSprites[i]=Sprite.Create(texture,new Rect(b.x*texture.width,(1-b.y-b.height)*texture.height,b.width*texture.width,b.height*texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}}
            return mixingSprites[index];
        }
        private Sprite MixingIngredientSprite(int ingredient)
        {
            if(ingredient<7)return MixingSprite(ingredient);
            if(mixingColorSprites==null){var texture=Resources.Load<Texture2D>("Discovery/mixing-colors");var width=texture.width/3f;mixingColorSprites=new Sprite[3];for(var i=0;i<3;i++)mixingColorSprites[i]=Sprite.Create(texture,new Rect(i*width,0,width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}
            return mixingColorSprites[ingredient-7];
        }
        private void BuildMixing()
        {
            if(SceneSchema<Mixing.Schema)return;
            mixingRoot=Rect(discoveryPanel,"Mix and discover",new Vector2(0,-30),new Vector2(1080,570));
            for(var i=0;i<4;i++){var mode=i;mixingModes.Add(Button(mixingRoot,Mixing.Names[i],new Vector2((i-1.5f)*258,259),new Vector2(248,58),()=>{if(discoveryPending)return;CancelMixingGesture();mixingMode=mode;mixingSelected=Mixing.Supplies[mode][0];PresentDiscovery();},Cream));}
            // A direct touch surface and authored prop sprites keep the working
            // vessel central; text labels supplement the recognizable pictures.
            mixingBowlBack=HomePicture(mixingRoot,"Glass bowl rear",new Vector2(0,-20),new Vector2(580,435),WorkshopArt.Bowl(0));mixingBowlBack.raycastTarget=false;
            mixingSurface=Rect(mixingRoot,"Mixture bowl",new Vector2(0,-20),new Vector2(580,435)).gameObject.AddComponent<MixingSurface>();
            var touch=mixingSurface.gameObject.AddComponent<MixingGesture>();
            touch.Begin=point=>{if(!MixingReady || mixingHeld>=0)return;mixingSurfaceDrag=true;mixingLastPoint=point;mixingSent=false;};
            touch.Move=point=>{if(!mixingSurfaceDrag || !MixingReady)return;if(Vector2.Distance(point,mixingLastPoint)>28){MixingSend("stir");mixingLastPoint=point;mixingSent=true;}};
            touch.End=moved=>{if(!mixingSurfaceDrag)return;mixingSurfaceDrag=false;if(!moved && MixingReady)MixingSend(mixingMode==3?"poke":"add:"+mixingSelected);};
            mixingBowlFront=HomePicture(mixingRoot,"Glass bowl front",new Vector2(0,-20),new Vector2(580,435),WorkshopArt.Bowl(1));mixingBowlFront.raycastTarget=false;
            mixingVolcano=HomePicture(mixingRoot,"Toy volcano",new Vector2(0,-56),new Vector2(355,320),MixingSprite(10));mixingVolcano.preserveAspect=true;mixingVolcano.raycastTarget=false;
            for(var i=0;i<6;i++){
                var slot=i;var point=new Vector2(i<3?-430:430,145-i%3*148);
                var card=Panel(mixingRoot,"Mixing supply "+i,point,new Vector2(184,139),Color.clear,true);mixingSupplyCards.Add(card);
                var art=HomePicture(card.transform,"Ingredient picture",new Vector2(0,12),new Vector2(128,117),MixingSprite(i));art.preserveAspect=true;art.raycastTarget=false;mixingSupplyArt.Add(art);
                var label=Label(card.transform,"",20,new Vector2(0,-57),new Vector2(174,33));label.raycastTarget=false;mixingSupplyLabels.Add(label);
                var gesture=card.gameObject.AddComponent<MixingGesture>();
                var ownsGesture=false;
                gesture.Begin=pointOnScreen=>{ownsGesture=false;if(!MixingReady || mixingHeld>=0 || mixingSurfaceDrag)return;ownsGesture=true;mixingHeld=Mixing.Supplies[mixingMode][slot];mixingSelected=mixingHeld;mixingDragged=false;mixingSent=false;mixingOverBowl=false;mixingNextPour=Time.unscaledTime;MoveMixingTool(pointOnScreen);};
                gesture.Move=pointOnScreen=>{if(!ownsGesture || mixingHeld<0)return;mixingDragged=true;MoveMixingTool(pointOnScreen);};
                gesture.End=moved=>{if(!ownsGesture)return;ownsGesture=false;if(mixingHeld<0)return;var ingredient=mixingHeld;var add=!moved || mixingOverBowl && !mixingSent;CancelMixingGesture();if(add && MixingReady)MixingSend("add:"+ingredient);};
            }
            mixingInstruction=Label(mixingRoot,"Tap an ingredient, or drag it over the bowl to pour.",22,new Vector2(0,199),new Vector2(730,34));
            mixingAmounts=Label(mixingRoot,"",18,new Vector2(0,-230),new Vector2(630,24));
            mixingVesselButton=Button(mixingRoot,"Volcano",new Vector2(-402,-264),new Vector2(170,56),()=>MixingSend("vessel"),Cream);
            Button(mixingRoot,"Stir",new Vector2(-208,-264),new Vector2(164,56),()=>MixingSend("stir"),Cream).transform.parent.name="Mixing Stir";
            mixingPokeButton=Button(mixingRoot,"Tap mixture",new Vector2(-22,-264),new Vector2(170,56),()=>MixingSend("poke"),Cream);
            Button(mixingRoot,"Wipe tray",new Vector2(160,-264),new Vector2(166,56),()=>MixingSend("wipe"),Cream).transform.parent.name="Mixing Wipe tray";
            Button(mixingRoot,"Rinse bowl",new Vector2(370,-264),new Vector2(196,56),()=>MixingSend("rinse"),Cream).transform.parent.name="Mixing Rinse bowl";
            mixingSoundButton=Button(mixingRoot,"Sound on",new Vector2(-93,162),new Vector2(175,42),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-sound"),MixingSound?0:1);PlayerPrefs.Save();if(!MixingSound)mixingAudio.Stop();PresentDiscovery();},Cream);
            mixingMotionButton=Button(mixingRoot,"Gentle motion",new Vector2(93,162),new Vector2(185,42),()=>{PlayerPrefs.SetInt(PreferenceKey("science-mixing-calm"),MixingCalm?0:1);PlayerPrefs.Save();PresentDiscovery();},Cream);
            mixingGhost=HomePicture(mixingRoot,"Held mixing tool",Vector2.zero,new Vector2(110,110),MixingSprite(0));mixingGhost.preserveAspect=true;mixingGhost.raycastTarget=false;mixingGhost.gameObject.SetActive(false);
            mixingAudio=gameObject.AddComponent<AudioSource>();mixingAudio.playOnAwake=false;mixingAudio.spatialBlend=0;mixingAudio.volume=.25f;
            mixingFizz=MixingSoundClip("Gentle science fizz",1.5f,0);mixingPour=MixingSoundClip("Small pour",.38f,1);mixingTap=MixingSoundClip("Soft spoon",.16f,2);
            mixingRoot.gameObject.SetActive(false);
        }
        private static AudioClip MixingSoundClip(string name,float duration,int kind)
        {
            const int hz=24000;var samples=new float[(int)(duration*hz)];var random=new System.Random(57+kind);float low=0;
            for(var i=0;i<samples.Length;i++){var time=i/(float)hz;var progress=i/(float)samples.Length;low=.78f*low+.22f*((float)random.NextDouble()*2-1);var envelope=Mathf.Min(1,progress*24)*Mathf.Pow(1-progress,kind==0?.7f:2);
                samples[i]=envelope*(kind==2?Mathf.Sin(time*1250)*Mathf.Exp(-time*32)*.5f:low*(kind==0?.85f:1)+Mathf.Sin(time*(kind==0?3900:1500))*.035f*(.5f+.5f*Mathf.Sin(time*37)));}
            var clip=AudioClip.Create(name,samples.Length,1,hz,false);clip.SetData(samples,0);return clip;
        }
        private void CancelMixingGesture(){mixingHeld=-1;mixingDragged=false;mixingOverBowl=false;mixingSurfaceDrag=false;if(mixingGhost!=null)mixingGhost.gameObject.SetActive(false);}
        private void MoveMixingTool(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mixingRoot,screen,null,out var point);mixingOverBowl=Mathf.Abs(point.x)<225 && point.y>-160 && point.y<145;
            if(mixingGhost!=null && mixingHeld>=0){mixingGhost.gameObject.SetActive(mixingDragged);mixingGhost.sprite=MixingIngredientSprite(mixingHeld);mixingGhost.rectTransform.anchoredPosition=point+new Vector2(0,45);mixingGhost.rectTransform.localRotation=Quaternion.Euler(0,0,mixingOverBowl?35:0);}
        }
        private void MixingSend(string op)
        {
            if(!MixingReady)return;var mode=mixingMode;var tray=OwnDiscovery.mixtures[mode];var token=Mixing.Token(mode,tray);var intent=++discoveryIntent;discoveryPending=true;
            void Done(SoloResult result){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;
                discoveryFeedback=result.Accepted?"":result.Outcome=="mixing-full"?"This bowl is full. Rinse it to start another experiment.":result.Outcome=="mixing-empty"?"Add an ingredient first.":"The mixture changed. Try once more.";discoveryFeedbackUntil=Time.unscaledTime+3;
                if(result.Accepted && MixingOpen && mixingMode==mode){if(op.StartsWith("add:")){mixingPourIngredient=int.Parse(op.Substring(4));mixingPourUntil=Time.unscaledTime+.45f;}if(MixingSound && mixingAudio!=null){var current=OwnDiscovery.mixtures[mode];mixingAudio.clip=op.StartsWith("add:") && current.reaction>0?mixingFizz:op.StartsWith("add:")?mixingPour:mixingTap;mixingAudio.Play();}}
                if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,token,"mix:"+mode+":"+op,1,0,Done);else Done(Command(SoloAction.Discovery,Actor,token,"mix:"+mode+":"+op,1,0));
        }
        private void PresentMixing(DiscoveryWorkspace own,Vector2 size)
        {
            if(mixingRoot==null)return;mixingRoot.gameObject.SetActive(discoveryStation==4);if(!MixingOpen)return;
            mixingRoot.localScale=Vector3.one*Mathf.Min((size.x-182)/1080,(size.y-167)/625);mixingRoot.anchoredPosition=new Vector2(70,-26);
            for(var i=0;i<4;i++)mixingModes[i].transform.parent.GetComponent<Image>().color=i==mixingMode?new Color(.65f,.87f,.8f):Cream;
            var supplies=Mixing.Supplies[mixingMode];var tray=own.mixtures[mixingMode];
            for(var i=0;i<6;i++){var card=mixingSupplyCards[i];card.gameObject.SetActive(i<supplies.Length);if(i>=supplies.Length)continue;var ingredient=supplies[i];card.name="Mixing ingredient "+ingredient;card.color=ingredient==mixingSelected?new Color(1,.92f,.73f,.24f):Color.clear;
                mixingSupplyArt[i].sprite=MixingIngredientSprite(ingredient);mixingSupplyLabels[i].text=mixingMode==1 && ingredient==1?"Soda solution":Mixing.Labels[ingredient];}
            mixingBowlBack.gameObject.SetActive(!(mixingMode==0 && tray.volcano));mixingBowlFront.gameObject.SetActive(!(mixingMode==0 && tray.volcano));mixingVolcano.gameObject.SetActive(mixingMode==0 && tray.volcano);mixingSurface.Paint(tray,mixingMode,MixingCalm);
            mixingVesselButton.transform.parent.gameObject.SetActive(mixingMode==0);mixingVesselButton.text=tray.volcano?"Clear bowl":"Toy volcano";mixingVesselButton.transform.parent.name="Mixing Vessel";
            mixingPokeButton.transform.parent.gameObject.SetActive(mixingMode==3);mixingPokeButton.transform.parent.name="Mixing Tap mixture";
            mixingSoundButton.text=MixingSound?"Sound on":"Sound off";mixingMotionButton.text=MixingCalm?"Calm motion":"Gentle motion";
            mixingInstruction.text=mixingMode==3 && Mixing.Oobleck(tray)?"Drag slowly through the bowl, then try a quick tap.":"Tap an ingredient, or drag it over the bowl to pour.";
            mixingAmounts.text=string.Join("   ",supplies.Where(i=>tray.amounts[i]>0).Select(i=>(mixingMode==1 && i==1?"Soda":Mixing.Labels[i])+" "+tray.amounts[i]));
            discoveryHint.text=discoveryPending?"Adding to your experiment…":discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil?discoveryFeedback:Mixing.Result(mixingMode,tray);
        }
        private void TickMixing()
        {
            // Solo's ordinary idle loop stops beneath overlays. Keep chemistry
            // (and kitchen maintenance) advancing once while this view is open.
            if(!Shared && DiscoveryOpen && !applicationPaused){if(World.AdvanceIdle(Mathf.Clamp(Time.unscaledDeltaTime,0,.1f),out var visible,new[]{Actor}))dirty=true;if(visible){discoveryPaintRevision=-1;PresentDiscovery();}if(dirty && Time.realtimeSinceStartup>=nextSave){SaveDuringPlay();nextSave=Time.realtimeSinceStartup+1;}}
            if(IceOpen && Time.unscaledTime-iceSurface.StrikeTime<.5f)iceSurface.SetVerticesDirty();
            if(!MixingOpen){CancelMixingGesture();if(!IceOpen && mixingAudio!=null && mixingAudio.isPlaying)mixingAudio.Stop();return;}
            if(mixingHeld>=0 && mixingDragged && mixingOverBowl && MixingReady && Time.unscaledTime>=mixingNextPour){mixingNextPour=Time.unscaledTime+.45f;mixingSent=true;MixingSend("add:"+mixingHeld);}
            if(Time.unscaledTime<mixingNextPaint)return;mixingNextPaint=Time.unscaledTime+.04f;
            mixingSurface.Clock=Time.unscaledTime;mixingSurface.Pouring=Time.unscaledTime<mixingPourUntil;mixingSurface.PourIngredient=mixingPourIngredient;mixingSurface.SetVerticesDirty();
        }
        private void ResetMixing(){CancelMixingGesture();mixingRoot=null;mixingSupplyCards.Clear();mixingSupplyArt.Clear();mixingSupplyLabels.Clear();mixingModes.Clear();if(mixingAudio!=null)Destroy(mixingAudio);foreach(var clip in new[]{mixingFizz,mixingPour,mixingTap})if(clip!=null)Destroy(clip);}
    }
}
