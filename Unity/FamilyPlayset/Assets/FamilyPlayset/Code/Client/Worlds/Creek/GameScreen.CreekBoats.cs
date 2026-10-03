using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private sealed class BoatView {public RectTransform root;public Image hull,passenger,flower;public CreekBoatRipples wake;}
        private readonly Dictionary<string,BoatView> creekBoatViews=new Dictionary<string,BoatView>();
        private readonly List<RectTransform> creekDocks=new List<RectTransform>();
        private Texture2D creekBoatTexture;private Sprite[] creekBoatSprites;
        private RectTransform boatEntry,boatControls,boatWorkshop,boatFrame;
        private Image boatPreview,boatPreviewPassenger,boatPreviewFlower;
        private Text boatHint,boatActionText,boatPassengerText,boatFlowerText,boatDockText;
        private Button boatAction,boatDecorate;
        private readonly List<(Button button,int hull)> boatHullChoices=new List<(Button,int)>();
        private readonly List<(Image image,int colour)> boatColourChoices=new List<(Image,int)>();
        private bool boatSending;private double boatDisplayClock;
        private readonly BoatVisualClock boatClock=new BoatVisualClock();
        public double CreekBoatVisualClock=>boatDisplayClock;
        public double CreekBoatVisualTime=>boatClock.SampleTime;
        private static readonly Color[] BoatColours={new Color(1,1,1),new Color(.88f,.97f,1),new Color(1,.88f,.9f),new Color(1,.98f,.77f)};
        public CreekBoatState CreekBoatGame=>HasWorld?(Shared?shared.View.creekBoats:World.ReadCreekBoats()):null;
        public bool BoatWorkshopOpen=>boatWorkshop!=null && boatWorkshop.gameObject.activeSelf;
        public int VisibleCreekBoats=>creekBoatViews.Values.Count(v=>v.root.gameObject.activeInHierarchy);
        public bool OwnCreekBoatInView
        {
            get {if(!creekBoatViews.TryGetValue(Actor,out var v) || !v.root.gameObject.activeInHierarchy)return false;
                var point=RectTransformUtility.WorldToScreenPoint(null,v.hull.rectTransform.position);
                return point.x>=35 && point.x<=Screen.width-35 && point.y>=100 && point.y<=Screen.height-65;}
        }
        private CreekBoat OwnCreekBoat=>CreekBoatGame?.boats.FirstOrDefault(b=>b.actor==Actor);
        private bool BoatCameraFollowing=>CurrentArea=="creek" && OwnCreekBoat?.attending==true;
        private float BoatCameraX=>OwnCreekBoat.phase==CreekBoatPhase.Ready?960+OwnCreekBoat.slot*80:CreekBoats.Point(OwnCreekBoat,boatDisplayClock).X;
        private void SendBoat(string action,string target="")
        {
            if(boatSending || !Ready || TravelPending)return;
            destination=null;manualCamera=false;CancelPointers();boatSending=true;
            void Done(SoloResult result){boatSending=false;
                if(!result.Accepted){homeFeedback.text=result.Outcome=="boat-is-sailing"?"Your boat is sailing. Wait for the dock.":"Try your boat at the launch.";homeFeedbackUntil=Time.unscaledTime+2.5f;}
                else if(action=="start"){cameraX=BoatCameraX;cameraArea=null;cameraVisit=-1;}
                Render();}
            if(Shared){if(!SubmitShared(SoloAction.CreekBoat,"",target,action,0,0,Done))boatSending=false;}
            else Done(Command(SoloAction.CreekBoat,target:target,value:action));
        }
        private Image BoatPicture(Transform parent,string name,Vector2 point,Vector2 size,int sprite)
        {var p=HomePicture(parent,name,point,size,creekBoatSprites[sprite]);p.preserveAspect=true;return p;}
        private void BoatPortrait(Image hull,Image passenger,Image flower,CreekBoat b)
        {
            hull.sprite=creekBoatSprites[b.hull];hull.color=BoatColours[b.colour];
            passenger.gameObject.SetActive(b.passenger);flower.gameObject.SetActive(b.flower);
            // These attachment anchors stay inside the illustrated hulls. The
            // paper boat has a shorter deck, so its passenger sits lower.
            var scale=hull.rectTransform.sizeDelta.x/170;
            passenger.rectTransform.anchoredPosition=hull.rectTransform.anchoredPosition+new Vector2(23,b.hull==2?-7:-13)*scale;
            flower.rectTransform.anchoredPosition=hull.rectTransform.anchoredPosition+new Vector2(-42,b.hull==2?-13:-19)*scale;
        }
        private void OpenBoatWorkshop()
        {
            if(!Ready || MenuOpen || OwnCreekBoat?.attending!=true || OwnCreekBoat.phase!=CreekBoatPhase.Ready)return;
            destination=null;if(Shared)shared.Walk(WalkMode.Stop);CancelPointers();Narration.Stop();
            boatWorkshop.gameObject.SetActive(true);boatWorkshop.SetAsLastSibling();TickCreekBoats();
        }
        private void CloseBoatWorkshop(){if(boatWorkshop!=null)boatWorkshop.gameObject.SetActive(false);}
        private void LaunchBoat(){CloseBoatWorkshop();SendBoat("launch");}
        private void BuildCreekBoats()
        {
            if(SceneSchema<CreekBoats.Schema)return;
            creekBoatTexture=WorldResources.Load<Texture2D>("Worlds/Creek/Boats/boats");
            if(creekBoatTexture==null)throw new InvalidOperationException("Missing creek boat artwork.");
            // The generated atlas has organic padding rather than exact cells.
            // Explicit source bounds retain every curled tip and dock post.
            var bounds=new[]{new Rect(16,40,518,490),new Rect(539,36,487,498),new Rect(1034,250,491,277),new Rect(22,599,579,390),new Rect(653,682,337,258),new Rect(1110,698,402,238)};
            creekBoatSprites=bounds.Select(r=>Sprite.Create(creekBoatTexture,new Rect(r.x/1536*creekBoatTexture.width,(1024-r.y-r.height)/1024*creekBoatTexture.height,r.width/1536*creekBoatTexture.width,r.height/1024*creekBoatTexture.height),new Vector2(.5f,.5f))).ToArray();
            foreach(var dock in new[]{0,1}){
                var p=CreekBoats.Dock(dock);var root=Rect(Board,"Creek landing "+dock,Vector2.zero,Vector2.zero);creekDocks.Add(root);
                BoatPicture(root,dock==0?"Little landing":"Leaf landing",Vector2.zero,new Vector2(190,125),3);
            }
            var launch=Rect(Board,"Boat launch dock",Vector2.zero,Vector2.zero);creekDocks.Add(launch);BoatPicture(launch,"Launch boards",Vector2.zero,new Vector2(240,145),3);
            foreach(var b in CreekBoatGame.boats){
                var root=Rect(Board,"Creek boat "+b.actor,Vector2.zero,new Vector2(195,125));
                var wake=Rect(root,"Boat ripples",new Vector2(0,-5),new Vector2(115,27)).gameObject.AddComponent<CreekBoatRipples>();wake.raycastTarget=false;
                var hull=BoatPicture(root,"Boat hull",Vector2.zero,new Vector2(120,b.hull==2?68:108),b.hull);
                var passenger=BoatPicture(root,"Leaf passenger",new Vector2(23,-13),new Vector2(33,28),4);
                var flower=BoatPicture(root,"Boat flower",new Vector2(-42,-19),new Vector2(27,21),5);
                var hit=Plain(root,"Boat touch target",Vector2.zero,new Vector2(195,125),Color.clear,true);
                var owner=b.actor;NavButton(hit,()=>{if(owner!=Actor)return;if(OwnCreekBoat.attending){if(OwnCreekBoat.phase==CreekBoatPhase.Docked)SendBoat("retrieve");else OpenBoatWorkshop();}else SendBoat("start");});
                creekBoatViews.Add(b.actor,new BoatView{root=root,hull=hull,passenger=passenger,flower=flower,wake=wake});
            }
            var entry=Button(safe,"Boats",Vector2.zero,new Vector2(250,84),()=>SendBoat("start"),new Color(.79f,.94f,.87f));boatEntry=(RectTransform)entry.transform.parent;
            BoatPicture(boatEntry,"Boat launch picture",new Vector2(-78,0),new Vector2(70,62),1);entry.rectTransform.anchoredPosition=new Vector2(35,0);entry.rectTransform.sizeDelta=new Vector2(160,74);
            boatControls=Panel(safe,"Creek boat controls",Vector2.zero,new Vector2(850,130),Cream,true).rectTransform;
            boatHint=Label(boatControls,"",27,new Vector2(0,39),new Vector2(800,45));
            var decorate=Button(boatControls,"Decorate boat",new Vector2(-253,-24),new Vector2(285,73),OpenBoatWorkshop,new Color(.9f,.94f,.8f));decorate.fontSize=26;boatDecorate=decorate.transform.parent.GetComponent<Button>();
            boatActionText=Button(boatControls,"Launch boat",new Vector2(82,-24),new Vector2(300,73),()=>{if(OwnCreekBoat.phase==CreekBoatPhase.Docked)SendBoat("retrieve");else LaunchBoat();},new Color(.75f,.9f,1));boatActionText.fontSize=27;boatAction=boatActionText.transform.parent.GetComponent<Button>();
            Button(boatControls,"Leave boats",new Vector2(330,-24),new Vector2(160,73),()=>SendBoat("leave"),Color.white).fontSize=23;
            boatWorkshop=Panel(safe,"Boat workshop",Vector2.zero,Vector2.zero,new Color(.08f,.24f,.27f,.55f),true).rectTransform;Stretch(boatWorkshop);NavButton(boatWorkshop.GetComponent<Image>(),CloseBoatWorkshop);
            boatFrame=Panel(boatWorkshop,"Make your creek boat",Vector2.zero,new Vector2(910,580),Cream,true).rectTransform;NavButton(boatFrame.GetComponent<Image>(),()=>{});
            Label(boatFrame,"Make your boat",43,new Vector2(0,228),new Vector2(710,60)).fontStyle=FontStyle.Bold;
            var preview=Panel(boatFrame,"Boat preview water",new Vector2(-230,46),new Vector2(370,243),new Color(.74f,.91f,.94f),false,true).rectTransform;
            boatPreview=BoatPicture(preview,"Decorated boat preview",Vector2.zero,new Vector2(275,208),1);
            boatPreviewPassenger=BoatPicture(preview,"Preview leaf passenger",Vector2.zero,new Vector2(74,63),4);
            boatPreviewFlower=BoatPicture(preview,"Preview flower",Vector2.zero,new Vector2(60,47),5);
            foreach(var hull in new[]{0,1,2}){
                var choice=hull;var name=new[]{"Leaf boat","Bark boat","Paper boat"}[hull];
                var label=Button(boatFrame,name,new Vector2(-352+hull*121,-130),new Vector2(111,118),()=>SendBoat("hull",choice.ToString()),new Color(.87f,.94f,.88f));label.fontSize=19;label.rectTransform.anchoredPosition=new Vector2(0,-39);label.rectTransform.sizeDelta=new Vector2(111,32);
                BoatPicture(label.transform.parent,name+" picture",new Vector2(0,14),new Vector2(90,74),hull);boatHullChoices.Add((label.transform.parent.GetComponent<Button>(),hull));
            }
            boatPassengerText=Button(boatFrame,"Leaf passenger",new Vector2(214,128),new Vector2(355,77),()=>SendBoat("passenger"),new Color(.85f,.94f,.76f));boatPassengerText.fontSize=25;
            BoatPicture(boatPassengerText.transform.parent,"Passenger choice",new Vector2(-125,0),new Vector2(60,48),4);boatPassengerText.rectTransform.anchoredPosition=new Vector2(35,0);boatPassengerText.rectTransform.sizeDelta=new Vector2(275,72);
            boatFlowerText=Button(boatFrame,"Flower decoration",new Vector2(214,33),new Vector2(355,77),()=>SendBoat("flower"),new Color(1,.89f,.9f));boatFlowerText.fontSize=25;
            BoatPicture(boatFlowerText.transform.parent,"Flower choice",new Vector2(-125,0),new Vector2(60,48),5);boatFlowerText.rectTransform.anchoredPosition=new Vector2(35,0);boatFlowerText.rectTransform.sizeDelta=new Vector2(275,72);
            foreach(var colour in new[]{0,1,2,3}){var choice=colour;var swatch=Panel(boatFrame,"Boat colour "+colour,new Vector2(88+colour*84,-57),new Vector2(65,51),BoatColours[colour],true,true);NavButton(swatch,()=>SendBoat("colour",choice.ToString()));boatColourChoices.Add((swatch,colour));}
            boatDockText=Button(boatFrame,"Choose landing",new Vector2(214,-140),new Vector2(355,73),()=>SendBoat("dock",(1-OwnCreekBoat.dock).ToString()),new Color(.8f,.91f,.99f));boatDockText.fontSize=25;
            Button(boatFrame,"Back to creek",new Vector2(-220,-235),new Vector2(320,73),CloseBoatWorkshop,Color.white).fontSize=26;
            Button(boatFrame,"Launch decorated boat",new Vector2(180,-235),new Vector2(400,73),LaunchBoat,new Color(.73f,.89f,.98f)).fontSize=28;
            boatWorkshop.gameObject.SetActive(false);TickCreekBoats();
        }
        private void TickCreekBoats()
        {
            var game=CreekBoatGame;if(game==null || boatEntry==null)return;
            var id=Shared?shared.View.worldId:World.WorldId;
            boatDisplayClock=boatClock.Sample(id,game.clock,Time.realtimeSinceStartupAsDouble,Shared && !applicationPaused);
            var visible=CurrentArea=="creek";var own=OwnCreekBoat;var playing=visible && own?.attending==true;
            if(BoatWorkshopOpen && (!playing || own.phase!=CreekBoatPhase.Ready || WorldLoading || applicationPaused))CloseBoatWorkshop();
            foreach(var pair in creekBoatViews){var b=game.boats.First(v=>v.actor==pair.Key);var v=pair.Value;v.root.gameObject.SetActive(visible);if(!visible)continue;
                var point=CreekBoats.Point(b,boatDisplayClock);var moving=b.phase==CreekBoatPhase.Floating || b.phase==CreekBoatPhase.Returning;
                v.root.anchoredPosition=new Vector2((point.X-cameraX)*sceneScale,(point.Y+(moving?Mathf.Sin((float)boatDisplayClock*2+b.slot)*2:0))*sceneScale);v.root.localScale=Vector3.one*sceneScale;
                v.hull.rectTransform.sizeDelta=new Vector2(120,b.hull==2?68:108);v.hull.rectTransform.anchoredPosition=new Vector2(0,b.hull==2?25:46);BoatPortrait(v.hull,v.passenger,v.flower,b);
                v.hull.rectTransform.localEulerAngles=new Vector3(0,0,moving?Mathf.Sin((float)boatDisplayClock*1.4f+b.slot)*1.5f:0);
                v.wake.phase=(float)boatDisplayClock+b.slot;v.wake.moving=moving;v.wake.SetVerticesDirty();
            }
            for(var i=0;i<creekDocks.Count;i++){var d=creekDocks[i];d.gameObject.SetActive(visible);var p=i<2?CreekBoats.Dock(i):new WalkPoint(710,-119);d.anchoredPosition=new Vector2((p.X-cameraX)*sceneScale,(p.Y-30)*sceneScale);d.localScale=Vector3.one*sceneScale;}
            var uiScale=Mathf.Min(1,safe.rect.width/910);boatEntry.gameObject.SetActive(visible && !playing && !MenuOpen && Math.Abs(cameraX-CreekBoats.CentreX)<1450);boatEntry.localScale=Vector3.one*uiScale;boatEntry.anchoredPosition=new Vector2(0,-safe.rect.height/2+63);
            boatControls.gameObject.SetActive(playing && !MenuOpen);boatControls.localScale=Vector3.one*uiScale;boatControls.anchoredPosition=new Vector2(0,-safe.rect.height/2+76);
            if(playing){var ready=own.phase==CreekBoatPhase.Ready;var docked=own.phase==CreekBoatPhase.Docked;var returning=own.phase==CreekBoatPhase.Returning;
                boatHint.text=ready?"Decorate, add a passenger, then launch!":docked?"Your boat reached the "+(own.dock==0?"little":"leaf")+" landing!":returning?"Your boat is coming back to the launch.":"Watch all your boats float down the creek.";
                boatActionText.text=ready?"Launch boat":docked?"Bring boat back":returning?"Coming back…":"Sailing…";boatActionText.transform.parent.name=ready?"Launch boat":docked?"Bring boat back":"Boat sailing";
                boatAction.interactable=!boatSending && (ready || docked);boatDecorate.interactable=!boatSending && ready;
            }
            if(BoatWorkshopOpen){boatWorkshop.SetAsLastSibling();boatFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/970,safe.rect.height/635);BoatPortrait(boatPreview,boatPreviewPassenger,boatPreviewFlower,own);
                boatPassengerText.text=own.passenger?"Remove passenger":"Add passenger";boatFlowerText.text=own.flower?"Remove flower":"Add flower";
                boatDockText.text=own.dock==0?"Little landing  →":"Leaf landing  →";
                foreach(var h in boatHullChoices){h.button.image.color=h.hull==own.hull?new Color(.69f,.88f,.8f):new Color(.93f,.96f,.91f);h.button.interactable=!boatSending;}
                foreach(var c in boatColourChoices)c.image.rectTransform.localScale=Vector3.one*(c.colour==own.colour?1.12f:1);
            }
        }
        private void AddCreekBoatDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var d in creekDocks)add(d,-120,0,d.name);
            foreach(var pair in creekBoatViews)add(pair.Value.root,-90,1,"creek-boat-"+pair.Key);
        }
        private void ResetCreekBoats()
        {
            CloseBoatWorkshop();if(boatWorkshop!=null)Destroy(boatWorkshop.gameObject);if(boatEntry!=null)Destroy(boatEntry.gameObject);if(boatControls!=null)Destroy(boatControls.gameObject);
            foreach(var v in creekBoatViews.Values)Destroy(v.root.gameObject);foreach(var d in creekDocks)Destroy(d.gameObject);
            creekBoatViews.Clear();creekDocks.Clear();boatHullChoices.Clear();boatColourChoices.Clear();
            if(creekBoatSprites!=null)foreach(var s in creekBoatSprites)Destroy(s);creekBoatSprites=null;if(creekBoatTexture!=null)Resources.UnloadAsset(creekBoatTexture);creekBoatTexture=null;
            boatWorkshop=null;boatControls=null;boatEntry=null;boatSending=false;boatClock.Reset();
        }
    }
    public sealed class CreekBoatRipples : MaskableGraphic
    {
        public float phase;public bool moving;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();for(var ring=0;ring<3;ring++){
                var t=Mathf.Repeat(phase*.35f+ring/3f,1);var rx=31+t*22;var ry=4+t*4;var colour=new Color(.91f,1,1,(1-t)*(moving?.42f:.2f));
                for(var i=0;i<28;i++){var a=i*Mathf.PI*2/28;var b=(i+1)*Mathf.PI*2/28;var start=mesh.currentVertCount;
                    mesh.AddVert(new Vector3(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),colour,Vector2.zero);mesh.AddVert(new Vector3(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry),colour,Vector2.zero);
                    mesh.AddVert(new Vector3(Mathf.Cos(b)*(rx+2),Mathf.Sin(b)*(ry+1.1f)),colour,Vector2.zero);mesh.AddVert(new Vector3(Mathf.Cos(a)*(rx+2),Mathf.Sin(a)*(ry+1.1f)),colour,Vector2.zero);
                    mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
                }
            }
        }
    }
}
