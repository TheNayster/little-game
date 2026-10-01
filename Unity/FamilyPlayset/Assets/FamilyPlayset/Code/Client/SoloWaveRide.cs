using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform waveCard,waveFrame,waveHud,waveInvitation,waveMenuChoice;
        private Text waveReady,waveStart,waveStatus,waveSplash,waveMenuHint;
        private readonly Text[] waveChoices=new Text[3];
        private readonly Image[] waveBadges=new Image[4];
        private readonly RectTransform[] waveSupports=new RectTransform[4];
        private readonly Image[] wavePictures=new Image[4];
        private bool waveSending;private int waveCardRound;
        private WaveRideState WaveRide=>Shore?.ride;
        private WaveRider OwnWaveRider=>BeachWaveRide.Rider(WaveRide,Actor);
        private double WaveDisplayAge=>WaveRide?.phase==WaveRidePhase.Riding?Math.Min(BeachWaveRide.Duration,WaveRide.age-BeachWaveRide.Countdown+Math.Max(0,shoreDisplayClock-Shore.clock)):0;
        private bool WaveRideCardOpen=>waveCard!=null && waveCard.gameObject.activeSelf;
        public int VisibleWaveRiders=>waveSupports.Count(r=>r!=null && r.gameObject.activeInHierarchy);
        private void BuildWaveRide()
        {
            if(SceneSchema<BeachWaveRide.Schema)return;
            var choice=MiniGameChoice("beach","Ride the waves",new Vector2(0,-145),new Vector2(700,250),()=>{CloseMiniGames();ShowWaveCard();},new Color(.65f,.9f,.94f));
            waveMenuChoice=(RectTransform)choice.transform.parent;waveMenuChoice.anchorMin=waveMenuChoice.anchorMax=new Vector2(.5f,1);
            choice.rectTransform.anchoredPosition=new Vector2(120,45);choice.rectTransform.sizeDelta=new Vector2(405,70);choice.fontSize=40;
            HomePicture(waveMenuChoice,"Wave ride picture",new Vector2(-220,0),new Vector2(210,170),visitorSprites[0,1]);
            waveMenuHint=Label(waveMenuChoice,"Whales, dolphins & mermaids!\nRide together through the surf.",26,new Vector2(120,-45),new Vector2(405,100));
            waveCard=Panel(safe,"Wave ride invitation",Vector2.zero,Vector2.zero,new Color(.08f,.2f,.29f,.55f),true).rectTransform;Stretch(waveCard);
            waveFrame=Panel(waveCard,"Choose your sea friend",Vector2.zero,new Vector2(820,540),new Color(.93f,.99f,1),true).rectTransform;
            Label(waveFrame,"Ride the waves",44,new Vector2(0,216),new Vector2(740,68)).fontStyle=FontStyle.Bold;
            waveReady=Label(waveFrame,"Choose a friend to get ready",28,new Vector2(0,156),new Vector2(740,65));
            for(var kind=0;kind<3;kind++){
                var k=kind;var x=(kind-1)*255;
                var label=Button(waveFrame,new[]{"Ride whale","Ride dolphin","Ride mermaid"}[kind],new Vector2(x,18),new Vector2(240,215),()=>SendWaveRide("ride-join",k.ToString()),new Color(.76f,.91f,.97f));waveChoices[k]=label;
                label.rectTransform.anchoredPosition=new Vector2(0,-76);label.rectTransform.sizeDelta=new Vector2(232,54);label.fontSize=26;
                HomePicture(label.transform.parent,"Sea friend choice",new Vector2(0,28),new Vector2(220,130),visitorSprites[k,1]);
            }
            for(var i=0;i<4;i++)waveBadges[i]=HomePicture(waveFrame,"Ready sea friend "+i,new Vector2((i-1.5f)*70,-125),new Vector2(62,46),visitorSprites[0,1]);
            Button(waveFrame,"Back to beach",new Vector2(-240,-213),new Vector2(240,72),CloseWaveCard,Color.white);
            waveStart=Button(waveFrame,"Start wave ride",new Vector2(155,-213),new Vector2(440,72),()=>SendWaveRide("ride-start",waveCardRound.ToString()),new Color(.6f,.88f,.7f));
            waveCard.gameObject.SetActive(false);
            waveInvitation=Rect(safe,"Family wave invitation",Vector2.zero,new Vector2(520,72));waveInvitation.anchorMin=waveInvitation.anchorMax=new Vector2(.5f,1);waveInvitation.anchoredPosition=new Vector2(0,-145);
            Button(waveInvitation,"Join wave ride",Vector2.zero,new Vector2(490,72),ShowWaveCard,new Color(.65f,.9f,.94f));waveInvitation.gameObject.SetActive(false);
            waveHud=Rect(safe,"Wave ride controls",Vector2.zero,new Vector2(820,76));waveHud.anchorMin=waveHud.anchorMax=new Vector2(.5f,0);waveHud.anchoredPosition=new Vector2(0,70);
            waveStatus=Label(Panel(waveHud,"Wave ride progress",new Vector2(-55,0),new Vector2(380,76),new Color(.94f,.99f,1,.96f)).transform,"",26,Vector2.zero,new Vector2(370,72));
            Button(waveHud,"All done riding",new Vector2(-346,0),new Vector2(180,76),()=>SendWaveRide("ride-leave"),new Color(1,.88f,.72f));
            waveSplash=Button(waveHud,"Splash together",new Vector2(276,0),new Vector2(245,76),()=>SendWaveRide("ride-wave",WaveRide.round.ToString()),new Color(.64f,.89f,.97f));
            for(var i=0;i<4;i++){
                waveSupports[i]=Rect(Board,"Wave ride sea friend "+i,Vector2.zero,Vector2.zero);
                wavePictures[i]=HomePicture(waveSupports[i],"Riding visitor",Vector2.zero,Vector2.one,visitorSprites[0,1]);
                for(var j=0;j<5;j++)Panel(waveSupports[i],"Ride foam "+j,new Vector2(-80+j*40,-36+(j%2)*3),new Vector2(36,5),new Color(.9f,.99f,1,.88f),false,true);
            }
        }
        private void ShowWaveCard()
        {
            if(!Ready || TravelPending || CurrentArea!="beach" || waveCard==null)return;
            CloseNavigation();CancelPointers();waveCardRound=WaveRide.round;waveCard.gameObject.SetActive(true);waveCard.SetAsLastSibling();stick.gameObject.SetActive(false);
        }
        private void CloseWaveCard()
        {if(waveCard!=null)waveCard.gameObject.SetActive(false);CancelPointers();}
        private void SendWaveRide(string op,string target="")
        {
            if(!Ready || waveSending)return;CancelPointers();waveSending=true;
            void Done(SoloResult result){waveSending=false;if(result.Accepted){waveCardRound=WaveRide.round;if(op=="ride-start" || op=="ride-leave")CloseWaveCard();}else{homeFeedback.text=result.Outcome=="join-next-ride"?"Friends are riding. Join the next ride!":result.Outcome=="wait-for-wave"?"Tap Splash when a wave lifts everyone!":"Please try again.";homeFeedbackUntil=Time.unscaledTime+3;}}
            if(Shared){if(!SubmitShared(SoloAction.Beach,"",target,op,0,0,Done))waveSending=false;}
            else Done(Command(SoloAction.Beach,target:target,value:op));
        }
        private float WaveGround(int seat)=>ToBoard(WaveRide?.x??1200,760).y-seat*.01f;
        private void PresentWaveRide()
        {
            var g=WaveRide;if(g==null || waveHud==null)return;var beach=CurrentArea=="beach";var own=OwnWaveRider;
            var running=g.phase==WaveRidePhase.Countdown || g.phase==WaveRidePhase.Riding;
            waveMenuChoice.GetComponent<Button>().interactable=!running;
            waveMenuHint.text=running?"Friends are riding!\nJoin the next ride.":"Whales, dolphins & mermaids!\nRide together through the surf.";
            if(!beach || applicationPaused || WorldLoading || g.phase==WaveRidePhase.Countdown || g.phase==WaveRidePhase.Riding)CloseWaveCardIfOpen();
            if(WaveRideCardOpen){
                waveFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/600);
                waveReady.text=own==null?g.phase==WaveRidePhase.Boarding?"Friends are ready — choose your ride!":"Choose a friend to get ready":g.riders.Length+" of 4 ready — start when everyone is here!";
                waveStart.transform.parent.GetComponent<Button>().interactable=own!=null && g.phase==WaveRidePhase.Boarding && !waveSending;
                for(var k=0;k<3;k++){waveChoices[k].text=(own?.visitor==(SeaVisitor)k?"✓ ":"")+new[]{"Ride whale","Ride dolphin","Ride mermaid"}[k];waveChoices[k].transform.parent.GetComponent<Button>().interactable=!waveSending;}
                for(var i=0;i<4;i++){var r=g.riders.FirstOrDefault(v=>v.seat==i);waveBadges[i].color=r==null?new Color(1,1,1,.18f):Color.white;if(r!=null)waveBadges[i].sprite=visitorSprites[(int)r.visitor,1];}
            }
            waveInvitation.gameObject.SetActive(beach && own==null && g.phase==WaveRidePhase.Boarding && !MenuOpen && !applicationPaused);
            waveHud.gameObject.SetActive(beach && own!=null && !MenuOpen && !applicationPaused);
            waveHud.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/930);
            if(beach && !MenuOpen)stick.gameObject.SetActive(JoystickMode && own==null);
            var age=g.age-BeachWaveRide.Countdown;
            waveStatus.text=g.phase==WaveRidePhase.Boarding?g.riders.Length+" ready — Games to start":g.phase==WaveRidePhase.Countdown?"Ready… "+Math.Max(1,(int)Math.Ceiling(BeachWaveRide.Countdown-g.age)):"Wave "+(BeachWaveRide.Wave(age)+1)+" of 3  ·  "+g.splashes+" splashes";
            waveSplash.text="Splash!";waveSplash.transform.parent.GetComponent<Button>().interactable=own!=null && g.phase==WaveRidePhase.Riding && BeachWaveRide.SplashWindow(age) && (own.waves&(1<<BeachWaveRide.Wave(age)))==0 && !waveSending;
            // One authority age moves the entire convoy. Pose/contact positions
            // are presentation only; saved players stay in the legal beach strip.
            var displayAge=WaveDisplayAge;var fit=Mathf.Min(1,Mathf.Max(.25f,(Board.rect.width/sceneScale-100)/955));
            for(var i=0;i<4;i++){
                var r=g.riders.FirstOrDefault(v=>v.seat==i);var root=waveSupports[i];root.gameObject.SetActive(beach && r!=null);if(r==null)continue;
                var kind=(int)r.visitor;var size=visitorSizes[kind,1];var ratio=220/size.x;size*=ratio;
                var lift=g.phase==WaveRidePhase.Riding?BeachWaveRide.Lift(displayAge):0;
                root.anchoredPosition=ToBoard(g.x+(i-1.5f)*245*fit+(float)Math.Sin(displayAge*.6)*65,760)+new Vector2(0,(20+lift)*fit)*sceneScale;root.localScale=Vector3.one*(sceneScale*fit);
                wavePictures[i].sprite=visitorSprites[kind,1];wavePictures[i].rectTransform.sizeDelta=size;
                var tilt=g.phase==WaveRidePhase.Riding?6*Mathf.Sin((float)displayAge*Mathf.PI/4):0;wavePictures[i].rectTransform.localRotation=Quaternion.Euler(0,0,tilt);
                var contact=kind==2?new Vector2(-34,-10):kind==1?new Vector2(-22,10):new Vector2(-15,16);
                // Contact follows the body's tilt, rather than floating above it.
                var a=tilt*Mathf.Deg2Rad;contact=new Vector2(contact.x*Mathf.Cos(a)-contact.y*Mathf.Sin(a),contact.x*Mathf.Sin(a)+contact.y*Mathf.Cos(a));
                void Place(RectTransform child,GameCharacterVisual visual){child.anchoredPosition=root.anchoredPosition+contact*(sceneScale*fit);child.localScale=Vector3.one*(sceneScale*.6f*fit);visual.PresentBeachRide(new CharacterFrame(CharacterPose.Sit,0,false,2),Time.unscaledDeltaTime,tilt);}
                if(r.actor==Actor)Place(avatar,characterVisual);else if(friends.TryGetValue(r.actor,out var friend) && friend.root.gameObject.activeSelf)Place(friend.root,friend.view);
            }
            SortDepth();
        }
        private void CloseWaveCardIfOpen(){if(WaveRideCardOpen)CloseWaveCard();}
        private void AddWaveRideDepth(Action<RectTransform,float,int,string> add)
        {for(var i=0;i<4;i++)add(waveSupports[i],WaveGround(i),0,"wave-support-"+i);}
        private void ResetWaveRide()
        {waveCard=null;waveFrame=null;waveHud=null;waveInvitation=null;waveMenuChoice=null;waveSending=false;Array.Clear(waveSupports,0,4);Array.Clear(wavePictures,0,4);Array.Clear(waveBadges,0,4);Array.Clear(waveChoices,0,3);}
    }
}
