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
        private RectTransform daycareTeacher,daycareTable,daycareHud,daycareControls,daycareTeacherCard;
        private RawImage calypsoPicture;
        private Text daycareHint,teacherRoutineLabel;
        private Button daycareReplay,daycareNext;
        private readonly List<GameCharacterVisual> daycareGuests=new List<GameCharacterVisual>();
        private readonly List<RectTransform> daycarePlates=new List<RectTransform>();
        private readonly List<AudioClip> daycareAudio=new List<AudioClip>();
        private Texture2D calypsoTexture;
        private bool daycareSending;
        private int daycareApproach=-1,daycareSeenRound=-1,daycareSeenPhase=-1,daycareSeenCount=-1;
        private long daycareGreetingVisit=-1;
        private RectTransform daycarePlateTray;private readonly Dictionary<string,Image> daycareHeldPlates=new Dictionary<string,Image>();
        private Vector2 calypsoWorldPoint;private double calypsoSample,calypsoClock;private float calypsoSampleAt;private int daycareFrame=-1;
        public Vector2 CalypsoWorldPoint=>calypsoWorldPoint;public bool CalypsoMoving{get;private set;}public int CalypsoPose{get;private set;}
        public DaycareState DaycareGame=>HasWorld?(Shared?shared.View.daycare:World.ReadDaycare()):null;
        public string[] PicnicNpcArt=>daycareGuests.Select(n=>n.CharacterId).ToArray();
        public int DaycareRoutine=>DaycareGame==null?-1:DaycareTeacher.Routine(DaycareGame);
        private bool TeacherCardOpen=>daycareTeacherCard!=null && daycareTeacherCard.gameObject.activeSelf;
        public bool CalypsoVisible=>daycareTeacher!=null && daycareTeacher.gameObject.activeInHierarchy;
        private bool PicnicOwn=>DaycareGame?.members.FirstOrDefault(m=>m.actor==Actor)?.attending==true;
        private bool PicnicPlateHeld=>DaycareGame?.members.FirstOrDefault(m=>m.actor==Actor)?.carryingPlate==true;
        private void SendDaycare(string op,int plate=-1)
        {
            if(!Ready || TravelPending || daycareSending)return;
            daycareSending=true;destination=null;daycareApproach=-1;CancelPointers();shared?.Walk(WalkMode.Stop);
            void Done(SoloResult r){daycareSending=false;if(!r.Accepted && r.Outcome!="plate-already-placed"){homeFeedback.text="Try the next picnic picture.";homeFeedbackUntil=Time.unscaledTime+2;}if(r.Accepted && op=="help")Narration.Speak("daycare-help");Render();}
            if(Shared){if(!SubmitShared(SoloAction.Daycare,"",plate<0?"":plate.ToString(),op,0,0,Done))daycareSending=false;}
            else Done(Command(SoloAction.Daycare,target:plate<0?"":plate.ToString(),value:op));
        }
        private void ChoosePicnic(){CloseMiniGames();CloseNavigation();CloseTeacherCard();Narration.Stop();SendDaycare("start");}
        private void RequestDaycarePlate(int index)
        {
            if(!Ready || MenuOpen || daycareSending || !PicnicOwn || DaycareGame.phase!=2)return;
            CancelPointers();var at=index==4?DaycareTeacher.Tray:DaycareTeacher.Plate(index);daycareApproach=index;manualCamera=false;destination=new Vector2(at.X,at.Y);
        }
        private void NextDaycarePlate(){if(!PicnicPlateHeld){RequestDaycarePlate(4);return;}for(var i=0;i<4;i++)if((DaycareGame.plates&(1<<i))==0){RequestDaycarePlate(i);return;}}
        private void CheckDaycareInput()
        {
            if(daycareApproach<0)return;
            if(CurrentArea!="daycare" || MenuOpen || applicationPaused || stickDirection.sqrMagnitude>.1f){daycareApproach=-1;destination=null;return;}
            var p=ReadPlayer(Actor);var at=daycareApproach==4?DaycareTeacher.Tray:DaycareTeacher.Plate(daycareApproach);
            if(Math.Abs(p.x-at.X)<=65 && Math.Abs(p.y-at.Y)<=65)SendDaycare(daycareApproach==4?"take-plate":"plate",daycareApproach);
        }
        private void CloseTeacherCard(){if(daycareTeacherCard!=null)daycareTeacherCard.gameObject.SetActive(false);}
        private void ShowTeacherCard()
        {
            if(!Ready || MenuOpen || CurrentArea!="daycare")return;
            CancelPointers();daycareApproach=-1;destination=null;shared?.Walk(WalkMode.Stop);daycareTeacherCard.gameObject.SetActive(true);daycareTeacherCard.SetAsLastSibling();
        }
        private void ListenDaycare()
        {Narration.Speak(DaycareGame.phase==3?"daycare-done":"daycare-invite");}
        private void BuildDaycare()
        {
            if(SceneSchema<DaycareTeacher.Schema)return;
            calypsoTexture=Resources.Load<Texture2D>("Daycare/calypso-poses");if(calypsoTexture==null)throw new InvalidOperationException("Missing Calypso artwork.");
            daycareTeacher=Rect(Board,"Calypso teacher",Vector2.zero,new Vector2(220,300));
            calypsoPicture=Rect(daycareTeacher,"Calypso drawing",new Vector2(0,130),new Vector2(220,294)).gameObject.AddComponent<RawImage>();calypsoPicture.texture=calypsoTexture;calypsoPicture.raycastTarget=false;
            HomeHit(daycareTeacher,"Ask Calypso",new Vector2(0,130),new Vector2(220,295),ShowTeacherCard);
            teacherRoutineLabel=Label(daycareTeacher,"",19,new Vector2(0,300),new Vector2(270,60));
            daycareTable=Rect(Board,"Daycare picnic table",Vector2.zero,new Vector2(740,240));daycareTable.gameObject.SetActive(false);
            for(var i=0;i<4;i++){
                var index=i;var child=Rect(daycareTable,"Picnic guest place "+(i+1),new Vector2((i-1.5f)*150,70),new Vector2(160,210));var guest=child.gameObject.AddComponent<GameCharacterVisual>();guest.Select(DaycareGame.guests[i]);child.localScale=Vector3.one*.72f;daycareGuests.Add(guest);
            }
            Plain(daycareTable,"Table left leg",new Vector2(-270,-35),new Vector2(22,110),new Color(.62f,.39f,.22f));Plain(daycareTable,"Table right leg",new Vector2(270,-35),new Vector2(22,110),new Color(.62f,.39f,.22f));
            Panel(daycareTable,"Shared picnic tabletop",new Vector2(0,40),new Vector2(710,110),new Color(.88f,.65f,.4f),false);
            for(var i=0;i<4;i++){
                var index=i;var root=Rect(daycareTable,"Picnic place "+(i+1),new Vector2((i-1.5f)*150,40),new Vector2(135,100));daycarePlates.Add(root);
                Panel(root,"Place a plate",Vector2.zero,new Vector2(100,64),new Color(1,1,1,.35f),false,true);
                HomeHit(root,"Place picnic plate "+(i+1),Vector2.zero,new Vector2(135,100),()=>RequestDaycarePlate(index),false);
            }
            HomeHit(daycareTable,"Join picnic table",new Vector2(0,-50),new Vector2(710,90),ChoosePicnic);
            daycarePlateTray=Rect(Board,"Picnic plate stack",Vector2.zero,new Vector2(175,155));
            Panel(daycarePlateTray,"Plate tray",new Vector2(0,22),new Vector2(160,105),new Color(.63f,.42f,.26f),false);
            for(var i=0;i<4;i++){Panel(daycarePlateTray,"Stacked plate rim "+i,new Vector2(0,30+i*12),new Vector2(120,58),new Color(.88f,.72f,.24f),false,true);Panel(daycarePlateTray,"Stacked white plate "+i,new Vector2(0,34+i*12),new Vector2(108,49),Color.white,false,true);}
            Label(daycarePlateTray,"Plates",24,new Vector2(0,130),new Vector2(170,40));HomeHit(daycarePlateTray,"Take picnic plate",new Vector2(0,55),new Vector2(170,140),()=>RequestDaycarePlate(4),false);
            foreach(var member in DaycareGame.members){var held=Panel(Board,"Carried picnic plate "+member.actor,Vector2.zero,new Vector2(83,58),Color.white,false,true);daycareHeldPlates.Add(member.actor,held);Panel(held.transform,"Plate rim",Vector2.zero,new Vector2(68,44),new Color(.98f,.92f,.71f),false,true);}
            daycareHud=Panel(safe,"Picnic counting hint",Vector2.zero,new Vector2(780,94),new Color(1,.98f,.88f,.97f),false).rectTransform;daycareHud.anchorMin=daycareHud.anchorMax=new Vector2(.5f,1);daycareHud.anchoredPosition=new Vector2(0,-155);
            daycareHint=Label(daycareHud,"",30,new Vector2(-50,0),new Vector2(620,85));
            var next=Button(daycareHud,">",new Vector2(320,0),new Vector2(100,76),NextDaycarePlate,new Color(.82f,.91f,.62f));next.fontSize=44;daycareNext=next.transform.parent.GetComponent<Button>();daycareNext.name="Next picnic plate";
            var replay=Button(daycareHud,"Set it again",new Vector2(0,-95),new Vector2(230,70),()=>SendDaycare("replay"),new Color(.82f,.91f,.62f));daycareReplay=replay.transform.parent.GetComponent<Button>();
            daycareControls=Rect(safe,"Picnic controls",Vector2.zero,new Vector2(800,80));daycareControls.anchorMin=daycareControls.anchorMax=new Vector2(.5f,0);daycareControls.anchoredPosition=new Vector2(0,65);
            Button(daycareControls,"Show me",new Vector2(-245,0),new Vector2(200,70),()=>{Narration.Speak("daycare-help");NextDaycarePlate();},Color.white).fontSize=26;
            Button(daycareControls,"Listen again",new Vector2(-25,0),new Vector2(215,70),ListenDaycare,Color.white).fontSize=26;
            Button(daycareControls,"Leave picnic",new Vector2(230,0),new Vector2(240,70),()=>{Narration.Stop();SendDaycare("leave");},Color.white).fontSize=26;
            daycareTeacherCard=Panel(safe,"Calypso choices",Vector2.zero,new Vector2(700,450),new Color(1,.98f,.88f),true).rectTransform;
            Label(daycareTeacherCard,"Hello from Calypso",36,new Vector2(0,160),new Vector2(650,65));
            var options=new[]{"Hear a story","Help me","Picnic counting","Back to play"};
            for(var i=0;i<4;i++){var option=i;Button(daycareTeacherCard,options[i],new Vector2(i%2==0?-165:165,i<2?50:-80),new Vector2(300,95),()=>{CloseTeacherCard();if(option==0)Narration.Speak("daycare-story");else if(option==1)SendDaycare("help");else if(option==2)ChoosePicnic();},i==2?new Color(.82f,.91f,.62f):Color.white).fontSize=27;}
            CloseTeacherCard();
            foreach(var id in new[]{"hello","story","help","invite","one","two","three","four","done"}){var clip=Resources.Load<AudioClip>("Daycare/"+id);if(clip!=null){daycareAudio.Add(clip);Narration.AddClip("daycare-"+id,clip);}}
            TickDaycare();
        }
        private void TickDaycare()
        {
            if(daycareTeacher==null)return;var d=DaycareGame;var visible=CurrentArea=="daycare" && d!=null;
            if(daycareFrame==Time.frameCount)return;daycareFrame=Time.frameCount;
            daycareTeacher.gameObject.SetActive(visible);daycareTable.gameObject.SetActive(visible);daycareHud.gameObject.SetActive(visible && PicnicOwn);daycareControls.gameObject.SetActive(visible && PicnicOwn);
            daycarePlateTray.gameObject.SetActive(visible && d.phase>0);foreach(var pair in daycareHeldPlates)pair.Value.gameObject.SetActive(visible && d.members.First(m=>m.actor==pair.Key).carryingPlate);
            if(!visible){CloseTeacherCard();daycareApproach=-1;daycareSeenPhase=-1;return;}
            var scale=Mathf.Min(1,safe.rect.width/1100);daycareHud.localScale=daycareControls.localScale=Vector3.one*scale;daycareTeacherCard.localScale=Vector3.one*Mathf.Min(safe.rect.width/800,safe.rect.height/520);
            if(d.clock!=calypsoSample){calypsoSample=d.clock;calypsoSampleAt=Time.unscaledTime;}
            calypsoClock=Math.Max(calypsoClock,d.clock+(Shared && shared.Connected?Math.Min(.35,Time.unscaledTime-calypsoSampleAt):0));
            var point=DaycareTeacher.Point(d,calypsoClock);var target=new Vector2(point.X,point.Y);var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(calypsoWorldPoint==Vector2.zero)calypsoWorldPoint=target;var prior=calypsoWorldPoint;calypsoWorldPoint=Vector2.MoveTowards(prior,target,180*dt);
            daycareTeacher.anchoredPosition=ToBoard(calypsoWorldPoint.x,calypsoWorldPoint.y);daycareTeacher.localScale=Vector3.one*sceneScale;
            // Keep a grounded standing drawing throughout travel. Switching to
            // sitting/reading before arrival made the moving teacher pop poses.
            CalypsoMoving=Vector2.Distance(prior,calypsoWorldPoint)>dt;var routine=DaycareTeacher.Routine(d,calypsoClock);var pose=CalypsoMoving?0:routine==1?2:routine==4?3:routine==0 || routine==3?1:0;CalypsoPose=pose;calypsoPicture.uvRect=new Rect(pose*.25f,0,.25f,1);
            teacherRoutineLabel.text=new[]{"Calypso · hello","Calypso · reading","Calypso · watching play","Calypso · helping","Calypso · resting"}[routine];
            daycareTable.anchoredPosition=ToBoard(1445,180);daycareTable.localScale=Vector3.one*sceneScale;
            var tray=DaycareTeacher.Tray;daycarePlateTray.anchoredPosition=ToBoard(tray.X,tray.Y);daycarePlateTray.localScale=Vector3.one*sceneScale;
            foreach(var pair in daycareHeldPlates){if(!pair.Value.gameObject.activeSelf)continue;var player=ReadPlayer(pair.Key);var at=Shared?shared.VisualPosition(pair.Key):new Vector2(player.x,player.y);pair.Value.rectTransform.anchoredPosition=ToBoard(at.x,at.y)+new Vector2(45,90)*sceneScale;pair.Value.rectTransform.localScale=Vector3.one*sceneScale;}
            for(var i=0;i<4;i++){
                daycareGuests[i].Select(d.guests[i]);daycareGuests[i].PresentFrame(new CharacterFrame(CharacterPose.Sit,0,i%2==0,(float)d.clock),dt);
                var graphic=daycarePlates[i].GetComponentInChildren<Image>();graphic.color=(d.plates&(1<<i))!=0?Color.white:new Color(.63f,.83f,.66f,.75f);daycarePlates[i].GetComponentInChildren<Button>().interactable=d.phase==2 && PicnicOwn && (d.plates&(1<<i))==0;
            }
            daycareNext.gameObject.SetActive(d.phase==2);daycareNext.interactable=!daycareSending;daycareReplay.gameObject.SetActive(d.phase==3);daycareReplay.interactable=d.clock-d.started>=2 && !daycareSending;
            var count=DaycareTeacher.Count(d.plates);daycareHint.text=d.phase==1?"Four friends need plates for their picnic!":d.phase==3?"1, 2, 3, 4! Every friend has a plate.":PicnicPlateHeld?"You've got a plate! Tap a green place. · "+count+" / 4":"Tap the plate stack to take a plate. · "+count+" / 4";
            var own=ReadPlayer(Actor);if(daycareGreetingVisit!=own.visit){daycareGreetingVisit=own.visit;if(!Narration.Speaking && !BookOpen)Narration.Speak("daycare-hello");}
            if(!PicnicOwn){daycareSeenPhase=-1;return;}
            if(daycareSeenRound!=d.round || daycareSeenPhase<0){daycareSeenRound=d.round;daycareSeenPhase=d.phase;daycareSeenCount=count;Narration.Speak(d.phase==3?"daycare-done":"daycare-invite");}
            else if(count>daycareSeenCount){daycareSeenCount=count;Narration.Speak("daycare-"+new[]{"","one","two","three","four"}[count]);}
            daycareSeenPhase=d.phase;
        }
        private void AddDaycareDepth(Action<RectTransform,float,int,string> add)
        {if(CalypsoVisible){add(daycareTeacher,daycareTeacher.anchoredPosition.y,3,"calypso");add(daycareTable,daycareTable.anchoredPosition.y,3,"picnic-table");if(daycarePlateTray.gameObject.activeSelf)add(daycarePlateTray,daycarePlateTray.anchoredPosition.y,3,"plate-tray");foreach(var pair in daycareHeldPlates)if(pair.Value.gameObject.activeSelf)add(pair.Value.rectTransform,pair.Value.rectTransform.anchoredPosition.y-90*sceneScale,4,pair.Key);}}
        private void ResetDaycare()
        {daycareTeacher=daycareTable=daycareHud=daycareControls=daycareTeacherCard=null;daycarePlateTray=null;daycareHeldPlates.Clear();daycareGuests.Clear();daycarePlates.Clear();calypsoWorldPoint=Vector2.zero;calypsoClock=calypsoSample=0;daycareFrame=-1;daycareApproach=-1;daycareSending=false;daycareSeenRound=daycareSeenPhase=daycareSeenCount=-1;daycareGreetingVisit=-1;foreach(var clip in daycareAudio)if(clip!=null)Resources.UnloadAsset(clip);daycareAudio.Clear();if(calypsoTexture!=null)Resources.UnloadAsset(calypsoTexture);calypsoTexture=null;}
    }
}
