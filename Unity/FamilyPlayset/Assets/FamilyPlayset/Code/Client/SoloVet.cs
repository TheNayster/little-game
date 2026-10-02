using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform vetFrame,vetSurface,vetPatientRoot,vetDemoCursor,vetScratch,vetBackdrop,vetFriendsPanel,vetTray,vetFinish;private RawImage vetRoomImage;private bool vetWasVisible;private VetAnimalView vetAnimal;private RawImage vetCursor,vetBandage;
        private Texture2D vetToolsTexture;private Text vetHint,vetName,vetProgress,vetFeedback,vetVisit;private Button vetHome,vetReplay;
        private readonly List<RawImage> vetDirt=new List<RawImage>(),vetTufts=new List<RawImage>(),vetFoam=new List<RawImage>(),vetHearts=new List<RawImage>(),vetNeedIcons=new List<RawImage>();
        private readonly List<Text> vetChecks=new List<Text>(),vetBedNames=new List<Text>(),vetWaitingNames=new List<Text>();
        private readonly List<Button> vetBeds=new List<Button>(),vetWaiting=new List<Button>(),vetToolButtons=new List<Button>();
        private readonly List<VetAnimalView> vetBedAnimals=new List<VetAnimalView>(),vetWaitingAnimals=new List<VetAnimalView>();
        private readonly List<(RectTransform root,GameCharacterVisual view)> vetHelpers=new List<(RectTransform,GameCharacterVisual)>(),vetClassmates=new List<(RectTransform,GameCharacterVisual)>();
        private TeacherWalkView vetTeacher;private RectTransform vetTeacherRoot;private readonly NpcLocomotion vetTeacherMotion=new NpcLocomotion();private readonly NpcPresentationClock vetClock=new NpcPresentationClock();
        private double vetDisplayClock;private AudioSource vetVoice,vetFoley;private readonly Dictionary<string,AudioClip> vetClips=new Dictionary<string,AudioClip>();
        private readonly Queue<VetStroke> vetStrokes=new Queue<VetStroke>();private sealed class VetStroke {public int patient,round,tool;public string gesture;public Vector2 point;}
        private int vetSelected=0,vetTool=0,vetSeenRound=-1,vetSeenReaction=-1,vetSpoken=-1,vetTickFrame=-1,vetCelebratedRound=-1;private bool vetSending;private float vetSendAt,vetDemoUntil,vetSoundAt,vetFeedbackUntil;private string vetQueuedGesture="";private Vector2 vetQueuedPoint;
        public VetState VetGame=>HasWorld?(Shared?shared.View.vet:World.ReadVet()):null;
        public bool VetBackdropReady=>vetRoomImage!=null && vetRoomImage.texture!=null;
        public bool VetFriendsOpen=>vetFriendsPanel!=null && vetFriendsPanel.gameObject.activeSelf;public int VetSelected=>vetSelected;public int VetTool=>vetTool;public int VetDrawing=>vetAnimal==null?-1:vetAnimal.Drawing;public bool VetWalking=>vetAnimal!=null && vetAnimal.Walking;
        public bool VetSoundPlaying=>vetVoice!=null && vetVoice.isPlaying;public string[] VetNpcArt=>vetClassmates.Select(n=>n.view.CharacterId).ToArray();public int VetQueued=>vetStrokes.Count;public string VetFeedback=>vetFeedback==null?"":vetFeedback.text;
        private bool VetOwn=>VetGame?.members.Any(m=>m.actor==Actor && m.attending)==true;
        public Vector2 VetScreenPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,vetSurface.TransformPoint(new Vector3((x-.5f)*350,(y-.5f)*350,0)));
        private void ChooseVet(){CloseMiniGames();CloseNavigation();CloseTeacherCard();Narration.Stop();SendVet("start");}
        private RawImage VetIcon(Transform parent,string name,Vector2 pos,Vector2 size,int tile)
        {var image=Rect(parent,name,pos,size).gameObject.AddComponent<RawImage>();image.texture=vetToolsTexture??Resources.Load<Texture2D>("Vet/tools");image.uvRect=new Rect(tile%4*.25f,tile<4?.5f:0,.25f,.5f);image.raycastTarget=false;return image;}
        private void SendVet(string op,int patient=-1,Vector2? point=null,string gesture="",int round=-1)
        {
            if(!Ready || TravelPending || vetSending)return;vetSending=true;destination=null;CancelPointers();shared?.Walk(WalkMode.Stop);
            var target=patient<0?"":patient+"@"+(round<0?VetGame.round:round);var at=point??Vector2.zero;
            void Done(SoloResult result){vetSending=false;if(result.Accepted && op=="welcome"){vetFriendsPanel.gameObject.SetActive(false);vetSelected=patient;vetSeenReaction=-1;vetStrokes.Clear();SelectVetTool(DaycareVet.NextTool(VetGame.patients[patient]));VetPlayAnimal(patient);}if(!result.Accepted && result.Outcome!="gentle-strokes"){vetFeedback.text=result.Outcome=="wash-before-bandage"?"Clean the muddy spots first.":result.Outcome=="care-beds-full"?"Send a comfortable friend home to make room.":result.Outcome=="touch-the-pictured-spot"?"Move the tool onto a pictured spot.":result.Outcome=="try-the-picture-card"?"Look at the care pictures above your friend.":"Try the care picture that is still waiting.";vetFeedbackUntil=Time.unscaledTime+3;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Vet,gesture,target,op,at.x,at.y,Done))vetSending=false;}else Done(Command(SoloAction.Vet,item:gesture,target:target,value:op,x:at.x,y:at.y));
        }
        private void VetStrokeAt(Vector2 point,string gesture,bool first)
        {
            if(!Ready || MenuOpen || VetFriendsOpen || !VetOwn || VetGame.patients[vetSelected].bed== -2 || VetGame.patients[vetSelected].bed>=0 && VetGame.clock-VetGame.patients[vetSelected].arrived<2)return;
            if(DaycareVet.Ready(VetGame.patients[vetSelected])){if(first)VetPlayAnimal(vetSelected,true);return;}vetCursor.gameObject.SetActive(true);vetCursor.rectTransform.anchoredPosition=(point-Vector2.one*.5f)*350;
            if(!first && gesture==vetQueuedGesture && Vector2.Distance(point,vetQueuedPoint)<.045f)return;
            vetQueuedGesture=gesture;vetQueuedPoint=point;if(vetStrokes.Count<32)vetStrokes.Enqueue(new VetStroke{point=point,gesture=gesture,tool=vetTool,patient=vetSelected,round=VetGame.round});
        }
        private void SelectVetTool(int tool,bool speak=true){vetTool=tool;vetCursor.uvRect=new Rect(tool*.25f,.5f,.25f,.5f);vetSpoken=tool;if(speak)Narration.Speak("vet-"+new[]{"wash","brush","bandage","cuddle"}[tool]);}
        private void SelectVetPatient(int index)
        {
            if(!Ready || VetGame.patients[index].bed== -2)return;
            CancelVetGestures();vetFriendsPanel.gameObject.SetActive(false);vetSelected=index;vetSeenReaction=-1;
            SelectVetTool(DaycareVet.NextTool(VetGame.patients[index]));VetPlayAnimal(index);
        }
        private void SelectVetBed(int bed){var index=Array.FindIndex(VetGame.patients,p=>p.bed==bed);if(index>=0)SelectVetPatient(index);}
        private void SelectVetWaiting(int patient)
        {
            // Bed assignment organizes the shared room; it must not block a
            // child's choice of animal or force another helper's patient out.
            if(VetGame.patients.Count(p=>p.bed>=0)<4)SendVet("welcome",patient);
            else SelectVetPatient(patient);
        }
        private void VetDemonstrate(){var p=VetGame.patients[vetSelected];var tool=Enumerable.Range(0,4).FirstOrDefault(t=>!DaycareVet.Done(p,t));SelectVetTool(tool);vetDemoUntil=Time.unscaledTime+5;vetDemoCursor.GetComponent<RawImage>().uvRect=new Rect(tool*.25f,.5f,.25f,.5f);}
        private Text VetButton(Transform parent,string text,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,Color fill)
        {
            var label=Button(parent,text,pos,size,action,fill);label.fontSize=26;label.fontStyle=FontStyle.Bold;label.color=Ink;
            var button=label.transform.parent.GetComponent<Button>();var colors=button.colors;colors.disabledColor=Color.white;button.colors=colors;return label;
        }
        private void ShowVetFriends(){CancelVetGestures();vetFriendsPanel.gameObject.SetActive(true);vetFriendsPanel.SetAsLastSibling();}
        private void BuildVet()
        {
            if(SceneSchema<DaycareVet.Schema)return;vetToolsTexture=Resources.Load<Texture2D>("Vet/tools");
            // A full-screen activity owns touches and covers the walking HUD.
            // Patient selection is a separate sheet, not another permanent dashboard.
            vetBackdrop=Plain(safe,"Clinic full-screen backdrop",Vector2.zero,Vector2.zero,Color.white,true).rectTransform;Stretch(vetBackdrop);
            vetRoomImage=Rect(vetBackdrop,"Sunny clinic room",Vector2.zero,Vector2.zero).gameObject.AddComponent<RawImage>();vetRoomImage.texture=Resources.Load<Texture2D>("Scenery/daycare-vet");vetRoomImage.raycastTarget=false;
            vetFrame=Rect(vetBackdrop,"Animal care clinic",Vector2.zero,new Vector2(1100,660));
            var top=Panel(vetFrame,"Patient heading",new Vector2(0,286),new Vector2(1080,80),Cream,false).rectTransform;
            VetButton(top,"Daycare",new Vector2(-437,0),new Vector2(185,64),()=>{CancelVetGestures();Narration.Stop();SendVet("leave");},new Color(.79f,.93f,.83f)).transform.parent.name="Return to Daycare";
            VetButton(top,"Friends",new Vector2(437,0),new Vector2(185,64),ShowVetFriends,new Color(.81f,.93f,1));
            vetName=Label(top,"",31,new Vector2(0,15),new Vector2(655,38));vetName.fontStyle=FontStyle.Bold;
            vetVisit=Label(top,"",21,new Vector2(0,-20),new Vector2(660,28));
            var progress=Panel(vetFrame,"Family care progress",new Vector2(0,228),new Vector2(760,34),Cream,false).rectTransform;
            vetProgress=Label(progress,"",21,new Vector2(-80,0),new Vector2(550,32));
            var hint=Panel(vetFrame,"Care hint card",new Vector2(0,180),new Vector2(850,50),Cream,false).rectTransform;vetHint=Label(hint,"",23,Vector2.zero,new Vector2(810,48));
            vetSurface=Plain(vetFrame,"Vet patient touch surface",new Vector2(0,30),Vector2.one*350,Color.clear,true).rectTransform;
            vetPatientRoot=Rect(vetSurface,"Living patient",new Vector2(0,-135),Vector2.one*350);vetAnimal=vetPatientRoot.gameObject.AddComponent<VetAnimalView>();
            for(var i=0;i<3;i++){vetDirt.Add(VetIcon(vetSurface,"Washable mud "+i,Vector2.zero,Vector2.one*67,4));vetTufts.Add(VetIcon(vetSurface,"Brushable tuft "+i,Vector2.zero,Vector2.one*55,6));vetFoam.Add(VetIcon(vetSurface,"Soap bubbles "+i,Vector2.zero,Vector2.one*74,5));}
            vetScratch=Rect(vetSurface,"Little scratch to cover",Vector2.zero,new Vector2(38,26));
            for(var i=0;i<3;i++){var mark=Plain(vetScratch,"Gentle scratch mark "+i,new Vector2(-9+i*9,0),new Vector2(3,17),new Color(.85f,.42f,.29f,.85f),false).rectTransform;mark.localRotation=Quaternion.Euler(0,0,-30);}
            vetBandage=VetIcon(vetSurface,"Patient bandage",Vector2.zero,new Vector2(76,46),2);
            for(var i=0;i<3;i++)vetHearts.Add(VetIcon(vetSurface,"Cuddle heart "+i,Vector2.zero,Vector2.one*55,3));
            vetCursor=VetIcon(vetSurface,"Your care tool",Vector2.zero,Vector2.one*91,0);vetCursor.gameObject.SetActive(false);vetDemoCursor=VetIcon(vetSurface,"Calypso demonstrates care",Vector2.zero,Vector2.one*95,0).rectTransform;vetDemoCursor.gameObject.SetActive(false);
            var touch=vetSurface.gameObject.AddComponent<VetTouchSurface>();touch.Stroke=VetStrokeAt;touch.End=()=>vetCursor.gameObject.SetActive(false);
            vetTray=Panel(vetFrame,"Care tools",new Vector2(0,-274),new Vector2(950,110),Cream,false).rectTransform;
            for(var i=0;i<4;i++){
                var tool=i;var label=VetButton(vetTray,new[]{"Wash","Brush","Bandage","Cuddle"}[i],Vector2.zero,new Vector2(205,104),()=>SelectVetTool(tool),Color.white);var root=(RectTransform)label.transform.parent;root.name="Vet tool "+new[]{"wash","brush","bandage","cuddle"}[i];label.rectTransform.anchoredPosition=new Vector2(0,-33);label.rectTransform.sizeDelta=new Vector2(190,35);
                vetNeedIcons.Add(VetIcon(root,"Care tool picture "+i,new Vector2(0,17),new Vector2(86,60),i));vetChecks.Add(Label(root,"",27,new Vector2(78,35),new Vector2(44,42)));vetToolButtons.Add(root.GetComponent<Button>());
                var drag=root.gameObject.AddComponent<VetTouchSurface>();drag.Target=vetSurface;drag.Begin=()=>SelectVetTool(tool,false);drag.Stroke=VetStrokeAt;drag.End=()=>vetCursor.gameObject.SetActive(false);
            }
            var home=VetButton(vetFrame,"All better!",new Vector2(0,-200),new Vector2(280,78),()=>SendVet("home",vetSelected),new Color(.75f,.93f,.79f));home.transform.parent.name="Send friend home";vetHome=home.transform.parent.GetComponent<Button>();
            vetTeacherRoot=Rect(vetFrame,"Clinic Calypso",new Vector2(-390,-100),new Vector2(100,130));var teacherImage=Rect(vetTeacherRoot,"Calypso clinic drawing",Vector2.zero,new Vector2(100,130)).gameObject.AddComponent<RawImage>();teacherImage.raycastTarget=false;vetTeacher=vetTeacherRoot.gameObject.AddComponent<TeacherWalkView>();vetTeacher.Configure(teacherImage,calypsoTexture);vetTeacherRoot.localScale=Vector3.one*.75f;
            VetButton(vetFrame,"Show me",new Vector2(-390,-183),new Vector2(190,60),VetDemonstrate,new Color(.81f,.93f,1)).transform.parent.name="Calypso: show me";
            for(var i=0;i<2;i++){var root=Rect(vetFrame,"Clinic classmate "+i,new Vector2(340+i*95,-130),new Vector2(100,140));root.localScale=Vector3.one*.5f;vetClassmates.Add((root,root.gameObject.AddComponent<GameCharacterVisual>()));}
            for(var i=0;i<4;i++){var root=Rect(progress,"Clinic family helper "+i,new Vector2(230+i*36,-12),new Vector2(100,140));root.localScale=Vector3.one*.15f;vetHelpers.Add((root,root.gameObject.AddComponent<GameCharacterVisual>()));}
            var feedback=Panel(vetFrame,"Care feedback",new Vector2(0,-174),new Vector2(620,50),Cream,false).rectTransform;vetFeedback=Label(feedback,"",23,Vector2.zero,new Vector2(590,48));
            vetFinish=Panel(vetFrame,"Clinic celebration",new Vector2(0,-25),new Vector2(650,265),Cream,false).rectTransform;
            Label(vetFinish,"Our pets and little dinos feel cared for!",27,new Vector2(0,92),new Vector2(615,55)).fontStyle=FontStyle.Bold;
            for(var i=0;i<8;i++){var root=Rect(vetFinish,"Happy friend "+i,new Vector2(-265+i*76,-5),Vector2.one*70);root.gameObject.AddComponent<VetAnimalView>().Present(i,1,false,true,68);}
            var replay=VetButton(vetFinish,"Another clinic day",new Vector2(0,-87),new Vector2(330,70),()=>SendVet("replay"),new Color(.75f,.93f,.79f));vetReplay=replay.transform.parent.GetComponent<Button>();
            vetFriendsPanel=Panel(vetFrame,"Choose clinic friends",Vector2.zero,new Vector2(1080,640),Cream,true).rectTransform;
            Label(vetFriendsPanel,"Choose a friend",33,new Vector2(-70,267),new Vector2(600,55)).fontStyle=FontStyle.Bold;
            VetButton(vetFriendsPanel,"Back",new Vector2(433,267),new Vector2(160,65),()=>vetFriendsPanel.gameObject.SetActive(false),new Color(.81f,.93f,1));
            Label(vetFriendsPanel,"Care beds",26,new Vector2(-263,212),new Vector2(480,42));Label(vetFriendsPanel,"More friends",26,new Vector2(263,212),new Vector2(480,42));
            for(var b=0;b<4;b++){
                var bed=b;var label=VetButton(vetFriendsPanel,"",new Vector2(-387+b%2*245,91-b/2*201),new Vector2(225,185),()=>SelectVetBed(bed),Color.white);var root=(RectTransform)label.transform.parent;root.name="Vet bed "+b;label.fontSize=23;label.rectTransform.anchoredPosition=new Vector2(0,-57);label.rectTransform.sizeDelta=new Vector2(210,65);vetBedNames.Add(label);vetBeds.Add(root.GetComponent<Button>());
                var animalRoot=Rect(root,"Care bed animal "+b,new Vector2(0,-21),Vector2.one*110);vetBedAnimals.Add(animalRoot.gameObject.AddComponent<VetAnimalView>());
            }
            for(var i=0;i<8;i++){
                var patient=i;var label=VetButton(vetFriendsPanel,"",Vector2.zero,new Vector2(225,185),()=>SelectVetWaiting(patient),Color.white);var root=(RectTransform)label.transform.parent;root.name="Vet welcome "+i;label.fontSize=23;label.rectTransform.anchoredPosition=new Vector2(0,-57);label.rectTransform.sizeDelta=new Vector2(210,65);vetWaitingNames.Add(label);vetWaiting.Add(root.GetComponent<Button>());
                var animalRoot=Rect(root,"Waiting pet "+i,new Vector2(0,-21),Vector2.one*110);vetWaitingAnimals.Add(animalRoot.gameObject.AddComponent<VetAnimalView>());
            }
            Label(vetFriendsPanel,"Choose any animal. You can help every friend, even when the beds are full.",22,new Vector2(0,-276),new Vector2(1030,70));vetFriendsPanel.gameObject.SetActive(false);
            vetVoice=vetFrame.gameObject.AddComponent<AudioSource>();vetVoice.playOnAwake=false;vetVoice.spatialBlend=0;vetVoice.volume=.27f;vetFoley=vetFrame.gameObject.AddComponent<AudioSource>();vetFoley.playOnAwake=false;vetFoley.spatialBlend=0;vetFoley.volume=.2f;
            foreach(var id in new[]{"welcome","wash","brush","bandage","cuddle","ready","done"})Narration.AddClip("vet-"+id,Resources.Load<AudioClip>("Vet/"+id));
            TickVet();
        }
        private void VetPlayAnimal(int patient,bool requested=false)
        {
            if(applicationPaused || vetVoice==null || !requested && Time.unscaledTime<vetSoundAt)return;vetSoundAt=Time.unscaledTime+2.5f;
            var id=patient>=4?"DinosaurWorldAudio/"+DaycareVet.Species(patient):patient==2?"ZooAudio/tortoise":"Vet/"+new[]{"puppy","kitten","rabbit","guinea-pig"}[patient];
            if(!vetClips.TryGetValue(id,out var clip)){clip=Resources.Load<AudioClip>(id);vetClips[id]=clip;}if(clip!=null){vetVoice.Stop();vetVoice.clip=clip;vetVoice.volume=.25f*ForegroundDucking;vetVoice.Play();}
        }
        private void TickVet()
        {
            if(vetFrame==null || vetTickFrame==Time.frameCount)return;vetTickFrame=Time.frameCount;var g=VetGame;var visible=CurrentArea==DaycareVet.Zone && VetOwn && !MenuOpen && !applicationPaused;
            if(vetWasVisible!=visible){vetWasVisible=visible;if(miniGamesButton!=null)miniGamesButton.gameObject.SetActive(!visible);if(worldButton!=null)worldButton.gameObject.SetActive(!visible);if(familyCircle!=null)familyCircle.gameObject.SetActive(!visible);if(movementLabel!=null)movementLabel.transform.parent.parent.gameObject.SetActive(!visible);if(stick!=null)stick.gameObject.SetActive(!visible && JoystickMode && !MenuOpen);}
            vetBackdrop.gameObject.SetActive(visible);if(!visible){vetStrokes.Clear();vetCursor.gameObject.SetActive(false);vetVoice.Stop();vetFoley.Stop();return;}
            vetFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/1120,safe.rect.height/680);vetBackdrop.SetAsLastSibling();
            var roomScale=Mathf.Max(safe.rect.width/1774,safe.rect.height/887);vetRoomImage.rectTransform.sizeDelta=new Vector2(1774,887)*roomScale;
            var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);vetDisplayClock=vetClock.Step(g.clock,"vet/"+g.round,Shared && shared.Connected,Time.unscaledTimeAsDouble,dt);
            if(vetSeenRound!=g.round){vetSeenRound=g.round;vetSelected=Array.FindIndex(g.patients,p=>p.bed==Array.FindIndex(g.members,m=>m.actor==Actor)%4);if(vetSelected<0)vetSelected=0;vetStrokes.Clear();vetSeenReaction=-1;SelectVetTool(DaycareVet.NextTool(g.patients[vetSelected]),false);vetDemoUntil=Time.unscaledTime+4;Narration.Speak("vet-welcome");}
            var p=g.patients[vetSelected];if(p.bed== -2){var next=Array.FindIndex(g.patients,v=>v.bed>=0);if(next<0)next=Array.FindIndex(g.patients,v=>v.bed== -1);if(next>=0){vetSelected=next;p=g.patients[next];vetSeenReaction=-1;SelectVetTool(DaycareVet.NextTool(p),false);}}
            var allDone=g.patients.All(a=>a.bed== -2);vetSurface.gameObject.SetActive(p.bed!= -2);vetName.text=allDone?"Our family clinic":p.bed== -2?"Welcome a friend":""+DaycareVet.Name(vetSelected)+" · "+DaycareVet.Kind(vetSelected);vetHint.text=allDone?"Our pets and little dinosaurs are ready to play!":p.bed== -2?"Open Friends and choose another animal.":DaycareVet.Hint(vetSelected,p,vetTool);vetVisit.text=p.bed!= -2?DaycareVet.Visit(vetSelected):"";
            if(allDone && vetCelebratedRound!=g.round){vetCelebratedRound=g.round;Narration.Speak("vet-done");}
            vetProgress.text=g.patients.Count(a=>a.bed== -2)+" / 8 friends cared for · "+g.members.Count(m=>m.attending)+" helpers";vetHome.gameObject.SetActive(p.bed!= -2 && DaycareVet.Ready(p));vetTray.gameObject.SetActive(p.bed!= -2 && !DaycareVet.Ready(p));vetFinish.gameObject.SetActive(allDone);vetHome.interactable=p.bed!= -2 && DaycareVet.Ready(p) && !vetSending;vetReplay.gameObject.SetActive(allDone);vetReplay.interactable=!vetSending;
            for(var b=0;b<4;b++){var index=Array.FindIndex(g.patients,a=>a.bed==b);var root=vetBeds[b].transform;root.GetComponent<Image>().color=index==vetSelected?new Color(.77f,.95f,.83f):new Color(1,.97f,.89f);vetBedAnimals[b].gameObject.SetActive(index>=0);vetBedNames[b].text=index<0?"Empty bed\nWelcome a friend":DaycareVet.Name(index)+(DaycareVet.Ready(g.patients[index])?"\nReady!":"\nNeeds care");vetBeds[b].interactable=index>=0;if(index>=0)vetBedAnimals[b].Present(index,vetDisplayClock+b,false,DaycareVet.Ready(g.patients[index]),110);}
            var waiting=0;for(var i=0;i<8;i++){var card=vetWaiting[i];var show=g.patients[i].bed== -1;card.gameObject.SetActive(show);if(!show)continue;var at=new Vector2(143+waiting%2*245,91-waiting/2*201);((RectTransform)card.transform).anchoredPosition=at;vetWaitingNames[i].text=DaycareVet.Name(i)+"\n"+(i<4?DaycareVet.Pets[i]:"Little dino");var t=vetDisplayClock+i;var wander=t%12<2;var animal=vetWaitingAnimals[i];animal.Present(i,t,wander,false,110,!wander || Math.Cos((t%12)*Math.PI)>=0);((RectTransform)animal.transform).anchoredPosition=new Vector2(wander?(float)Math.Sin((t%12)*Math.PI)*8:0,-21);waiting++;}
            var required=Enumerable.Range(0,4).Where(t=>DaycareVet.Needs(p,t)).ToArray();
            for(var tool=0;tool<4;tool++){var button=vetToolButtons[tool];var rank=Array.IndexOf(required,tool);button.gameObject.SetActive(rank>=0);button.GetComponent<RectTransform>().anchoredPosition=new Vector2((rank-(required.Length-1)*.5f)*225,0);vetChecks[tool].text=DaycareVet.Done(p,tool)?"✓":"";vetChecks[tool].color=new Color(.13f,.42f,.22f);button.GetComponent<Image>().color=tool==vetTool?new Color(.69f,.92f,.8f):Color.white;}
            if(p.bed!= -2){var age=vetDisplayClock-p.arrived;var walking=p.bed>=0 && age<2;vetPatientRoot.anchoredPosition=new Vector2(walking?Mathf.SmoothStep(-110,0,Mathf.Clamp01((float)age/2)):0,-135);vetAnimal.Present(vetSelected,vetDisplayClock,walking,vetDisplayClock-p.reaction<1.2 && p.reactions>0 || DaycareVet.Ready(p),vetSelected<4?280:vetSelected==6?300:350);
                for(var i=0;i<3;i++){var spot=DaycareVet.Spot(vetSelected,i);var at=(new Vector2(spot.X,spot.Y)-Vector2.one*.5f)*350+new Vector2(vetPatientRoot.anchoredPosition.x,0);vetDirt[i].rectTransform.anchoredPosition=at;vetDirt[i].gameObject.SetActive(DaycareVet.Needs(p,0) && p.washed[i]<3 && (vetTool==0 || vetTool==2));vetDirt[i].color=new Color(1,1,1,1-p.washed[i]/3f);vetTufts[i].rectTransform.anchoredPosition=at+new Vector2(7,12);vetTufts[i].gameObject.SetActive(DaycareVet.Needs(p,1) && p.brushed[i]<3 && vetTool==1);vetTufts[i].color=new Color(1,1,1,1-p.brushed[i]/3f);if(vetSelected>=4)vetTufts[i].uvRect=new Rect(0,0,.25f,.5f);else vetTufts[i].uvRect=new Rect(.5f,0,.25f,.5f);vetFoam[i].rectTransform.anchoredPosition=at;vetFoam[i].gameObject.SetActive(p.washed[i]>0 && vetDisplayClock-p.reaction<.6 && g.members.Any(m=>m.patient==vetSelected && m.tool=="wash" && g.clock-m.last<.7));}
                var patch=DaycareVet.Patch(vetSelected);vetBandage.rectTransform.anchoredPosition=(new Vector2(patch.X,patch.Y)-Vector2.one*.5f)*350+new Vector2(vetPatientRoot.anchoredPosition.x,0);vetScratch.anchoredPosition=vetBandage.rectTransform.anchoredPosition;vetScratch.gameObject.SetActive(DaycareVet.Needs(p,2) && !p.bandaged && (vetTool==2 || DaycareVet.Done(p,0)));vetBandage.gameObject.SetActive(DaycareVet.Needs(p,2) && p.bandaged);vetBandage.color=Color.white;
                for(var i=0;i<3;i++){vetHearts[i].gameObject.SetActive(i<p.cuddles);vetHearts[i].rectTransform.anchoredPosition=new Vector2(215+(float)Math.Sin(vetDisplayClock*3+i)*4,70-i*65);}
                if(vetSeenReaction!=p.reactions){if(vetSeenReaction>=0 && p.reactions>vetSeenReaction){if(DaycareVet.Ready(p))Narration.Speak("vet-ready");VetPlayAnimal(vetSelected);if(vetTool==0 || vetTool==1){var clip=Resources.Load<AudioClip>(vetTool==0?"ZooAudio/water":"ZooAudio/feeding");if(clip!=null)vetFoley.PlayOneShot(clip,.45f);}}vetSeenReaction=p.reactions;}
            }
            if(p.bed>=0 && g.clock-p.arrived<2)vetHint.text=DaycareVet.Name(vetSelected)+" is coming to the cushion…";
            vetFeedback.transform.parent.gameObject.SetActive(Time.unscaledTime<vetFeedbackUntil);
            var demonstrating=!VetFriendsOpen && Time.unscaledTime<vetDemoUntil && p.bed!= -2 && !DaycareVet.Ready(p);vetDemoCursor.gameObject.SetActive(demonstrating);if(demonstrating){var spot=DaycareVet.Spot(vetSelected,Mathf.FloorToInt(Time.unscaledTime*1.1f)%3);vetDemoCursor.anchoredPosition=(new Vector2(spot.X,spot.Y)-Vector2.one*.5f)*350+new Vector2(Mathf.Sin(Time.unscaledTime*7)*18,0);vetDemoCursor.GetComponent<RawImage>().uvRect=new Rect(vetTool*.25f,.5f,.25f,.5f);}
            var teacherFrame=vetTeacherMotion.Step(new Vector2(-390,-100),"vet-teacher/"+g.round,false,95,dt);vetTeacherRoot.anchoredPosition=vetTeacherMotion.Point;vetTeacher.Present(teacherFrame,demonstrating?1:0,dt,.75f);
            for(var i=0;i<2;i++){var n=vetClassmates[i];n.view.Select(g.friends[i]);n.view.PresentFrame(new CharacterFrame(allDone?CharacterPose.Dance:vetDisplayClock%9<1?CharacterPose.Wave:CharacterPose.Idle,0,i==0,(float)vetDisplayClock),dt);}
            for(var i=0;i<4;i++){var n=vetHelpers[i];var m=g.members[i];n.root.gameObject.SetActive(m.attending);if(m.attending){n.view.Select(ReadPlayer(m.actor).avatar);n.view.PresentFrame(new CharacterFrame(CharacterPose.Wave,0,false,(float)vetDisplayClock),dt);}}
            if(!vetSending && !ActionPending && Time.unscaledTime>=vetSendAt && vetStrokes.Count>0){var stroke=vetStrokes.Dequeue();vetSendAt=Time.unscaledTime+.17f;if(stroke.round==g.round && g.patients[stroke.patient].bed!= -2)SendVet(new[]{"wash","brush","bandage","cuddle"}[stroke.tool],stroke.patient,stroke.point,stroke.gesture,stroke.round);}
        }
        private void CancelVetGestures(){vetStrokes.Clear();vetQueuedGesture="";vetDemoUntil=0;if(vetFrame!=null)foreach(var touch in vetFrame.GetComponentsInChildren<VetTouchSurface>(true))touch.Cancel();if(vetVoice!=null)vetVoice.Stop();if(vetFoley!=null)vetFoley.Stop();}
        private void ResetVet(){vetStrokes.Clear();vetBeds.Clear();vetWaiting.Clear();vetBedAnimals.Clear();vetWaitingAnimals.Clear();vetToolButtons.Clear();vetDirt.Clear();vetTufts.Clear();vetFoam.Clear();vetHearts.Clear();vetNeedIcons.Clear();vetChecks.Clear();vetBedNames.Clear();vetWaitingNames.Clear();vetHelpers.Clear();vetClassmates.Clear();vetFrame=null;vetBackdrop=null;vetFriendsPanel=null;vetWasVisible=false;vetAnimal=null;vetVoice=vetFoley=null;vetSending=false;vetSeenRound=vetSeenReaction=vetTickFrame=vetCelebratedRound=-1;vetClock.Reset();vetTeacherMotion.Reset();vetClips.Clear();}
    }
}
