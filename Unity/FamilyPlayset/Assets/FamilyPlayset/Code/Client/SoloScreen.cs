using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // One garden presentation; a supplied session routes actions to a remote
    // authority. The ordinary solo scene retains its own existing save adapter.
    public sealed class SoloScreen : MonoBehaviour
    {
        public SoloWorld World { get; private set; }
        public string Actor { get; private set; }
        public string SavePath { get; private set; }
        public string VerifyMode { get; private set; }
        public string VerifyRun { get; private set; }
        public CheckpointStatus LoadedStatus { get; private set; }
        public RectTransform Board { get; private set; }
        public bool JoystickMode { get; private set; }
        public SoloNarration Narration {get;private set;}
        public bool MenuOpen => menu != null && menu.activeSelf;
        public readonly Dictionary<string, SoloPointerSurface> Surfaces = new Dictionary<string, SoloPointerSurface>();
        private readonly Dictionary<string, RectTransform> toys = new Dictionary<string, RectTransform>();
        private readonly Dictionary<string, Image> fills = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> targetRings = new Dictionary<string, Image>();
        private readonly Dictionary<string, GameObject> targetArrows = new Dictionary<string, GameObject>();
        private CheckpointStore store;
        private string offlineBranch,offlineActor;
        public void ConfigureOfflineBranch(string world,string profile)
        {
            if(safe!=null || !FamilyPairing.Id(world) || !FamilyPairing.Id(profile))throw new ArgumentException("Invalid offline branch.");
            offlineBranch="paired-"+world+"-"+profile;offlineActor=profile;
        }
        private RectTransform safe, avatar, stickKnob, stick;
        private Image head, body;
        private Text message, activity, movementLabel, saveLabel, voiceLabel, listenLabel;
        private GameObject voiceSlash;
        private Font font;
        private Sprite rounded, circle, hintRing;
        private GameObject menu;
        private Vector2? destination;
        private Vector2 stickDirection;
        private string dragging;
        private bool dirty;
        private IGardenSession shared;
        private Text connecting;
        private Text areaLabel;
        private Image floorPath;
        private readonly List<GameObject> fence=new List<GameObject>();
        private readonly Dictionary<string,Button> travelButtons=new Dictionary<string,Button>();
        private string requestedArea;
        private bool travelSubmitted;
        public bool TravelPending=>requestedArea!=null || travelSubmitted;
        public string CurrentArea=>HasWorld?SoloWorld.AreaOf(ReadPlayer(Actor).zone):"garden";
        private long renderedSequence=-1;
        private bool grabConfirmed,gestureEnded,gestureCancelled,dropSubmitted,wasConnected;
        private Vector2 dragPoint;
        private readonly Dictionary<string,(RectTransform root,Image head,Image body)> friends=new Dictionary<string,(RectTransform,Image,Image)>();
        private readonly Dictionary<string,Text> holders=new Dictionary<string,Text>();
        public bool Shared=>shared!=null;
        public bool Ready=>Board!=null && (World!=null || shared?.View!=null);
        public bool ActionPending=>shared!=null && (shared.Busy || TravelPending);
        public string Feedback=>message?.text??"";
        public string Dragging=>dragging;
        public int VisiblePlayers=>1+friends.Values.Count(v=>v.root.gameObject.activeSelf);
        public void Configure(IGardenSession session){if(safe!=null)throw new InvalidOperationException("Configure before Start.");shared=session;}
        public SoloPlayer ReadPlayer(string id)=>shared==null?World.ReadPlayer(id):shared.View.players.First(p=>p.id==id).Copy();
        private SoloToy[] AllToys()=>shared==null?World.ReadToys():shared.View.toys.Select(t=>t.Copy()).ToArray();
        public SoloToy[] ReadToys()=>AllToys().Where(t=>SoloWorld.AreaOf(t.zone)==CurrentArea).ToArray();
        private bool HasWorld=>World!=null || shared?.View!=null;
        private float nextSave, nextMovement, lastMovement;
        private float lastSharedMovement;
        private Rect lastSafeArea;
        private static readonly Color Ink = new Color(.15f,.25f,.29f), Cream = new Color(.98f,.96f,.88f);
        private void Start()
        {
            Application.targetFrameRate = 30; Application.runInBackground = true;
            if(!ReadVerificationArgs())
            {Debug.LogError("Invalid solo verification arguments; normal saved play was not opened.");Application.Quit(2);return;}
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded = Shape(false); circle = Shape(true);hintRing=Ring();
            var canvas = new GameObject("Solo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scale = canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(1280,800); scale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            safe = Rect(canvas.transform,"Safe Area",Vector2.zero,Vector2.zero); UpdateSafeArea();
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Solo Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            if(shared!=null)
            {
                Actor=shared.Actor;connecting=Label(safe,"Joining your shared garden…",32,Vector2.zero,new Vector2(1000,160));
                return;
            }
            SavePath = Path.Combine(Application.persistentDataPath,"SoloPrototype",VerifyRun ?? offlineBranch ?? "family-local","world.save");
            store = new CheckpointStore(SavePath, ValidPayload);
            try
            {
                var loaded = store.Load(); LoadedStatus = loaded.Status;
                if (loaded.Status == CheckpointStatus.Corrupt || loaded.Status == CheckpointStatus.Unsupported)
                    throw new InvalidDataException("Existing save needs recovery or a compatible version.");
                World = loaded.Status == CheckpointStatus.Missing ? SoloWorld.Create(offlineActor ?? Guid.NewGuid().ToString("N")) : SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(loaded.Payload));
                Actor = World.Snapshot().players[0].id;
            }
            catch (Exception e)
            {
                Label(safe,"We couldn't open your saved play.\nA grown-up needs to check this save before continuing.",32,Vector2.zero,new Vector2(1100,200));
                Debug.LogError("Solo save load stopped: " + e.Message);
                if (VerifyRun != null) SoloVerification.FinishBlocked(this,e.Message);
                return;
            }
            JoystickMode = PlayerPrefs.GetInt(PreferenceKey("joystick"),0) == 1;
            Narration=gameObject.AddComponent<SoloNarration>();Narration.Initialize(VerifyRun!=null);
            Narration.SetVoiceEnabled(PlayerPrefs.GetInt(PreferenceKey("voice"),1)!=0);
            BuildScreen(); Render();
            if (LoadedStatus == CheckpointStatus.Missing) SaveNow();
            if (LoadedStatus == CheckpointStatus.Recovered) message.text = "Your last safe save is back. Let's play!";
            if (VerifyRun != null)
            {
                if(VerifyMode=="crash-hold" || VerifyMode=="crash-resume")gameObject.AddComponent<SoloCrashVerification>();
                else gameObject.AddComponent<SoloVerification>();
            }
        }
        private void InitializeShared()
        {
#if UNITY_STANDALONE_WIN
            QualitySettings.vSyncCount=0;
            Application.targetFrameRate=60;
#endif
            connecting.gameObject.SetActive(false);
            if(FindAnyObjectByType<AudioListener>()==null)gameObject.AddComponent<AudioListener>();
            if(FindAnyObjectByType<Camera>()==null)
            {
                var camera=new GameObject("Shared Garden Camera",typeof(Camera)).GetComponent<Camera>();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Cream;camera.transform.position=new Vector3(0,0,-10);
            }
            JoystickMode=PlayerPrefs.GetInt(PreferenceKey("joystick"),0)==1;
            Narration=gameObject.AddComponent<SoloNarration>();Narration.Initialize(shared.MutedTest);
            Narration.SetVoiceEnabled(PlayerPrefs.GetInt(PreferenceKey("voice"),1)!=0);
            BuildScreen();Render();wasConnected=shared.Connected;
        }
        public static bool ValidPayload(string payload)
        {
            try
            {
                var snapshot = JsonUtility.FromJson<SoloSnapshot>(payload);
                if (snapshot != null && snapshot.schema > 1) throw new NotSupportedException("Newer solo save schema.");
                SoloWorld.Validate(snapshot); return true;
            }
            catch (ArgumentException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        private bool ReadVerificationArgs()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            var requested=false;
            for (var i=0;i<args.Length;i++)
            {
                if(args[i]=="-soloVerify"){requested=true;if(i+1<args.Length)VerifyMode=args[i+1];}
                if(args[i]=="-soloRun"){requested=true;if(i+1<args.Length && Guid.TryParse(args[i+1],out var id))VerifyRun=id.ToString("N");}
            }
            if (VerifyMode != "seed" && VerifyMode != "resume" && VerifyMode != "recover" && VerifyMode != "input" && VerifyMode != "crash-hold" && VerifyMode != "crash-resume" && VerifyMode != "blocked" && VerifyMode != "settings-seed" && VerifyMode != "settings-resume") { VerifyMode=null; VerifyRun=null; }
            if(requested && (VerifyMode==null || VerifyRun==null))return false;
#endif
            return true;
        }
        private void BuildScreen()
        {
            Label(safe,"Little Weeps",36,new Vector2(-407,351),new Vector2(330,58));
            areaLabel=Label(safe,"",17,new Vector2(-404,313),new Vector2(350,34));
            Button(safe,"Blue pup",new Vector2(-95,343),new Vector2(150,64),()=>ChooseAvatar("blue-pup"),new Color(.7f,.87f,.97f));
            Button(safe,"Orange pup",new Vector2(75,343),new Vector2(160,64),()=>ChooseAvatar("orange-pup"),new Color(1,.81f,.61f));
            movementLabel=Button(safe,JoystickMode?"Joystick":"Tap to walk",new Vector2(285,343),new Vector2(210,64),ToggleMovement,Cream);
            Button(safe,"Menu",new Vector2(500,343),new Vector2(160,64),()=>SetMenu(true),Cream);
            var garden=Button(safe,"Grow a flower",new Vector2(-380,265),new Vector2(245,48),()=>StartActivity("garden"),new Color(.86f,.93f,.74f));
            garden.rectTransform.anchoredPosition=new Vector2(22,0);garden.rectTransform.sizeDelta=new Vector2(191,48);
            var blossom=Panel(garden.transform.parent,"Flower picture",new Vector2(-93,0),new Vector2(33,33),new Color(.96f,.52f,.61f),false,true);
            Panel(blossom.transform,"Center",Vector2.zero,new Vector2(12,12),new Color(1,.84f,.35f),false,true);
            var cleanup=Button(safe,"Splash cleanup",new Vector2(-115,265),new Vector2(245,48),()=>StartActivity("cleanup"),new Color(.76f,.9f,1));
            cleanup.rectTransform.anchoredPosition=new Vector2(22,0);cleanup.rectTransform.sizeDelta=new Vector2(191,48);
            Panel(cleanup.transform.parent,"Sponge picture",new Vector2(-94,0),new Vector2(34,23),new Color(1,.83f,.28f));
            Button(safe,"Free play",new Vector2(120,265),new Vector2(190,48),()=>StartActivity(""),Cream);
            if(shared!=null)
                foreach(var zone in new[]{"garden","creek"})
                {
                    var place=zone;
                    var button=Button(safe,zone=="garden"?"Garden":"Creek",new Vector2(zone=="garden"?320:505,265),new Vector2(165,56),()=>Travel(place),Cream);
                    travelButtons.Add(zone,button.transform.parent.GetComponent<Button>());
                }
            else Label(safe,"Pick any game. Leave any time.",17,new Vector2(413,265),new Vector2(330,42));
            Board=Panel(safe,"Garden",new Vector2(0,-25),new Vector2(1120,500),new Color(.76f,.89f,.72f),true).rectTransform;
            Surface(Board,"ground");
            Panel(Board,"Sky",new Vector2(0,140),new Vector2(1116,216),new Color(.8f,.92f,.96f));
            for(var i=0;i<7;i++) fence.Add(Panel(Board,"Fence",new Vector2(-465+i*155,80),new Vector2(142,65),new Color(.99f,.95f,.83f)).gameObject);
            floorPath=Panel(Board,"Path",new Vector2(0,-156),new Vector2(1050,70),new Color(.9f,.81f,.64f));
            foreach(var toy in AllToys()) DrawToy(toy);
            DrawAvatar();
            stick=Panel(safe,"Walk joystick",new Vector2(-477,-193),new Vector2(146,146),new Color(1,1,1,.8f),true,true).rectTransform;
            Surface(stick,"stick");
            stickKnob=Panel(stick,"Thumb",Vector2.zero,new Vector2(65,65),new Color(.34f,.55f,.62f),false,true).rectTransform;
            stick.gameObject.SetActive(JoystickMode);
            activity=Label(safe,"",24,new Vector2(0,-312),new Vector2(1130,44));
            message=Label(safe,"Drag the bucket to the tap, then to the plant.",21,new Vector2(-85,-354),new Vector2(950,40));
            var listen=Button(safe,"Listen",new Vector2(490,-352),new Vector2(150,52),Listen,Cream);
            listenLabel=listen;
            listen.rectTransform.anchoredPosition=new Vector2(15,0);listen.rectTransform.sizeDelta=new Vector2(108,52);
            for(var i=0;i<3;i++)Panel(listen.transform.parent,"Sound",new Vector2(-52+i*9,0),new Vector2(5,12+i*9),Ink);
            saveLabel=Label(safe,"Local solo prototype · placeholder art",15,new Vector2(0,-380),new Vector2(1130,28));
            menu=Panel(safe,"Pause panel",Vector2.zero,new Vector2(1190,760),new Color(.97f,.97f,.91f,.98f),true).gameObject;
            Label(menu.transform,"Take your time",42,new Vector2(0,230),new Vector2(760,100));
            Label(menu.transform,"Your toys stay where you put them.\nYou can always leave an activity.",25,new Vector2(0,140),new Vector2(800,80));
            voiceLabel=Button(menu.transform,"Voice on",new Vector2(0,35),new Vector2(350,75),ToggleVoice,new Color(.77f,.88f,.96f));
            voiceLabel.transform.parent.name="Voice setting";
            voiceLabel.rectTransform.anchoredPosition=new Vector2(20,0);voiceLabel.rectTransform.sizeDelta=new Vector2(250,75);
            for(var i=0;i<3;i++)Panel(voiceLabel.transform.parent,"Sound",new Vector2(-130+i*12,0),new Vector2(7,15+i*12),Ink);
            var slash=Panel(voiceLabel.transform.parent,"Voice off mark",new Vector2(-118,0),new Vector2(55,7),new Color(.73f,.26f,.23f));
            slash.rectTransform.localRotation=Quaternion.Euler(0,0,45);voiceSlash=slash.gameObject;
            Label(menu.transform,"Spoken hints on this device",18,new Vector2(0,-24),new Vector2(650,36));
            Button(menu.transform,"Back to play",new Vector2(0,-90),new Vector2(350,85),()=>SetMenu(false),new Color(.81f,.92f,.72f));
            Button(menu.transform,"Leave activity",new Vector2(0,-200),new Vector2(350,75),()=>{StartActivity("");SetMenu(false);},Cream);
            UpdateVoiceControls();
            menu.SetActive(false);
        }
        public SoloResult Command(SoloAction action,string item="",string target="",string value="",float x=0,float y=0)
        {
            if(shared!=null)
            {
                SubmitShared(action,item,target,value,x,y,result=>
                {if(!result.Accepted && result.Outcome!="superseded")message.text=Friendly(result.Outcome);Render();});
                return new SoloResult(false,"pending",shared.View.revision);
            }
            var result=World.Apply(new SoloCommand {requestId=Guid.NewGuid().ToString("N"),actor=Actor,expectedRevision=World.Revision,action=action,item=item,target=target,value=value,x=x,y=y});
            if(result.Accepted) {dirty=true;Render();}
            return result;
        }
        private bool SubmitShared(SoloAction action,string item,string target,string value,float x,float y,Action<SoloResult> done)
        {
            return shared.Submit(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=Actor,action=action,item=item,target=target,value=value,x=x,y=y},done);
        }
        private string Friendly(string outcome)=>outcome=="already-held"?"Your friend is using that toy. Try another!":outcome=="disconnected"?(shared?.Status??"The connection stopped."):"That move didn't finish. Please try again.";
        public void ChooseAvatar(string id) { if(HasWorld && !TravelPending) Command(SoloAction.ChangeAvatar,value:id); }
        public void Travel(string zone)
        {
            if(shared==null || !shared.Connected || !SoloWorld.KnownArea(zone))return;
            requestedArea=zone;CancelPointers();Narration.Stop();
            message.text="Going to the "+zone+"…";
            Render();
        }
        private void FinishTravel()
        {
            if(requestedArea==null || travelSubmitted || shared.Busy || dragging!=null)return;
            if(requestedArea==CurrentArea){requestedArea=null;message.text="Keep exploring the "+CurrentArea+".";Render();return;}
            var target=requestedArea;travelSubmitted=true;
            SubmitShared(SoloAction.Travel,"","",target,0,0,result=>
            {
                travelSubmitted=false;
                if(!result.Accepted || requestedArea==target)requestedArea=null;
                message.text=result.Accepted?"Welcome to the "+target+". Your friends can join you here!":Friendly(result.Outcome);
                Render();
            });
        }
        public void StartActivity(string id)
        {
            if(!HasWorld || TravelPending)return;
            if(shared!=null)
            {
                SubmitShared(id==""?SoloAction.LeaveActivity:SoloAction.StartActivity,"","",id,0,0,result=>
                {if(result.Accepted){Narration.Speak(id==""?"freeplay":id);message.text="Pick any toy. You can leave this activity any time.";}else message.text=Friendly(result.Outcome);Render();});
                return;
            }
            Command(id==""?SoloAction.LeaveActivity:SoloAction.StartActivity,value:id);
            Narration.Speak(id==""?"freeplay":id);
            message.text=id==""?"All your toys still work. Explore!":id=="garden"?"Fill your bucket at the tap. Give the flower a drink!":"Drag the sponge over the puddle to soak it up.";
        }
        public void ToggleMovement()
        {
            CancelPointers(); JoystickMode=!JoystickMode; movementLabel.text=JoystickMode?"Joystick":"Tap to walk";stick.gameObject.SetActive(JoystickMode);
            Narration.Speak(JoystickMode?"joystick":"tapwalk");
            PlayerPrefs.SetInt(PreferenceKey("joystick"),JoystickMode?1:0);PlayerPrefs.Save();
        }
        // Preferences belong to this device, not the shared world/checkpoint. Test
        // players use a fresh GUID key prefix and never alter the family's keys.
        private string PreferenceKey(string name)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            return "solo.prototype."+name;
#else
            return (shared!=null?shared.PreferenceScope:VerifyRun!=null?"solo.verify."+VerifyRun+".":offlineBranch!=null?offlineBranch+".":"solo.prototype.")+name;
#endif
        }
        public void ToggleVoice()
        {
            Narration.SetVoiceEnabled(!Narration.VoiceEnabled);
            PlayerPrefs.SetInt(PreferenceKey("voice"),Narration.VoiceEnabled?1:0);PlayerPrefs.Save();
            UpdateVoiceControls();
        }
        private void UpdateVoiceControls()
        {
            voiceLabel.text=Narration.VoiceEnabled?"Voice on":"Voice off";voiceSlash.SetActive(!Narration.VoiceEnabled);
            listenLabel.text=Narration.VoiceEnabled?"Listen":"Voice off";
            listenLabel.transform.parent.GetComponent<Button>().interactable=Narration.VoiceEnabled;
        }
        public void SetMenu(bool open) { if(open){CancelPointers();Narration.Stop();SaveNow();} menu.SetActive(open); }
        public void Listen(){var activityId=ReadPlayer(Actor).activity;Narration.Speak(activityId==""?"freeplay":activityId);}
        private Vector2 BoardPoint(Vector2 screen)
        { RectTransformUtility.ScreenPointToLocalPointInRectangle(Board,screen,null,out var local);return new Vector2((local.x/Board.rect.width+.5f)*SoloWorld.Width,(local.y/Board.rect.height+.5f)*SoloWorld.Height); }
        public Vector2 ScreenPoint(float x,float y) => RectTransformUtility.WorldToScreenPoint(null,Board.TransformPoint(new Vector3((x/SoloWorld.Width-.5f)*Board.rect.width,(y/SoloWorld.Height-.5f)*Board.rect.height,0)));
        public bool BeginPointer(string role,Vector2 screen)
        {
            if(!HasWorld || MenuOpen || TravelPending || (shared!=null && !shared.Connected))return false;
            if(role=="ground") {if(JoystickMode)return false;var point=BoardPoint(screen);destination=new Vector2(Mathf.Clamp(point.x,40,960),Mathf.Clamp(point.y,35,455));return true;}
            if(role=="stick") {if(!JoystickMode)return false;MovePointer(role,screen);return true;}
            if(shared!=null)return BeginSharedDrag(role,screen);
            if(dragging!=null || !Command(SoloAction.Grab,role).Accepted)return false;
            dragging=role;MovePointer(role,screen);return true;
        }
        public void MovePointer(string role,Vector2 screen)
        {
            if(role=="stick") {RectTransformUtility.ScreenPointToLocalPointInRectangle(stick,screen,null,out var point);stickDirection=Vector2.ClampMagnitude(point/55,1);stickKnob.anchoredPosition=stickDirection*40;}
            else if(role==dragging && !gestureEnded) {var point=BoardPoint(screen);dragPoint=point;toys[role].anchoredPosition=ToBoard(point.x,point.y);toys[role].SetAsLastSibling();UpdateTargetHints(point);if(grabConfirmed && SoloWorld.Position(point.x,point.y))shared?.Preview(role,point);}
        }
        public void EndPointer(string role,Vector2 screen)
        {
            if(role=="stick"){stickDirection=Vector2.zero;stickKnob.anchoredPosition=Vector2.zero;return;}
            if(role!=dragging)return;
            if(shared!=null){dragPoint=BoardPoint(screen);gestureEnded=true;gestureCancelled=!SoloWorld.Position(dragPoint.x,dragPoint.y);FinishSharedDrag();return;}
            var point=BoardPoint(screen);
            if(!SoloWorld.Position(point.x,point.y)){CancelPointer(role);return;}
            var target=ReadToys().Where(t=>t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle)
                .OrderBy(t=>Vector2.Distance(new Vector2(t.x,t.y),point)).First();
            var id=Vector2.Distance(new Vector2(target.x,target.y),point)<=SoloWorld.InteractionRadius?target.id:"";
            var result=Command(SoloAction.Drop,role,id,x:point.x,y:point.y);
            if(!result.Accepted)Command(SoloAction.CancelGrab,role);
            dragging=null;HideTargetHints();Render();
            message.text=result.Outcome=="bucket-filled"?"Splash! Your bucket is full.":result.Outcome=="plant-watered"?"A drink for the flower!":result.Outcome=="puddle-cleaned"?"Squeeze, soak, tidy!":"What shall we play next?";
        }
        public void CancelPointer(string role)
        {
            if(role=="stick"){stickDirection=Vector2.zero;if(stickKnob!=null)stickKnob.anchoredPosition=Vector2.zero;}
            if(role==dragging)
            {
                if(shared!=null){gestureEnded=true;gestureCancelled=true;HideTargetHints();FinishSharedDrag();}
                else{Command(SoloAction.CancelGrab,role);dragging=null;HideTargetHints();Render();}
            }
        }
        private bool BeginSharedDrag(string role,Vector2 screen)
        {
            if(dragging!=null)return false;
            var toy=ReadToys().FirstOrDefault(t=>t.id==role);
            if(toy==null || (toy.kind!=ToyKind.Bucket && toy.kind!=ToyKind.Sponge))return false;
            if(!string.IsNullOrEmpty(toy.holder)){message.text=Friendly("already-held");return false;}
            dragging=role;grabConfirmed=false;gestureEnded=false;gestureCancelled=false;dropSubmitted=false;
            MovePointer(role,screen);message.text="Picking it up…";Render();
            return SubmitShared(SoloAction.Grab,role,"","",0,0,result=>
            {
                if(dragging!=role)return;
                if(!result.Accepted){message.text=Friendly(result.Outcome);ClearSharedDrag();return;}
                grabConfirmed=true;message.text="You have it! Drag it somewhere fun.";Render();
                if(gestureEnded)FinishSharedDrag();
            });
        }
        private void FinishSharedDrag()
        {
            if(!grabConfirmed || dropSubmitted || dragging==null)return;
            var item=dragging;var target="";dropSubmitted=true;HideTargetHints();
            if(!gestureCancelled)
            {
                var nearest=ReadToys().Where(t=>t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle).OrderBy(t=>Vector2.Distance(new Vector2(t.x,t.y),dragPoint)).First();
                if(Vector2.Distance(new Vector2(nearest.x,nearest.y),dragPoint)<=SoloWorld.InteractionRadius)target=nearest.id;
            }
            var cancelled=gestureCancelled;
            message.text=cancelled?"Putting it back…":"Finishing your move…";
            SubmitShared(cancelled?SoloAction.CancelGrab:SoloAction.Drop,item,target,"",cancelled?0:dragPoint.x,cancelled?0:dragPoint.y,result=>
            {
                if(!result.Accepted && shared.Connected && ReadToys().Any(t=>t.id==item && t.holder==Actor))
                {
                    SubmitShared(SoloAction.CancelGrab,item,"","",0,0,release=>{message.text=Friendly(result.Outcome);ClearSharedDrag();});return;
                }
                message.text=!result.Accepted?Friendly(result.Outcome):cancelled?"Your toy is back. Keep exploring!":result.Outcome=="bucket-filled"?"Splash! Your bucket is full.":result.Outcome=="plant-watered"?"A drink for the flower!":result.Outcome=="puddle-cleaned"?"Squeeze, soak, tidy!":"What shall we play next?";
                ClearSharedDrag();
            });
        }
        private void ClearSharedDrag(){dragging=null;grabConfirmed=false;gestureEnded=false;dropSubmitted=false;HideTargetHints();Render();}
        private void HideTargetHints(){foreach(var ring in targetRings.Values)ring.gameObject.SetActive(false);foreach(var arrow in targetArrows.Values)arrow.SetActive(false);}
        private void UpdateTargetHints(Vector2 point)
        {
            foreach(var target in ReadToys())
            {
                if(!targetRings.TryGetValue(target.id,out var ring))continue;
                var item=ReadToys().FirstOrDefault(t=>t.id==dragging);
                var useful=SoloWorld.HasUsefulInteraction(item,target);
                var near=useful && Vector2.Distance(point,new Vector2(target.x,target.y))<=SoloWorld.InteractionRadius;
                ring.gameObject.SetActive(useful);ring.color=near?new Color(.23f,.6f,.39f):new Color(1,.86f,.35f);
                targetArrows[target.id].SetActive(near);
            }
        }
        public void CancelPointers()
        {
            foreach(var surface in Surfaces.Values)surface.Cancel();
            if(shared!=null && dragging!=null && !dropSubmitted)CancelPointer(dragging);
            destination=null;stickDirection=Vector2.zero;
            shared?.Walk(WalkMode.Stop);
        }
        private void Update()
        {
            if(safe!=null && lastSafeArea!=Screen.safeArea)UpdateSafeArea();
            if(shared!=null)
            {
                if(Board==null){if(shared.View!=null && safe!=null)InitializeShared();else if(connecting!=null && !shared.Connected)connecting.text="Joining your shared garden…\n"+shared.Status;return;}
                if(wasConnected && !shared.Connected){requestedArea=null;travelSubmitted=false;CancelPointers();ClearSharedDrag();message.text=Friendly("disconnected");Narration.Stop();}
                if(!wasConnected && shared.Connected){CancelPointers();renderedSequence=-1;message.text="You're back. Let's play!";}
                wasConnected=shared.Connected;
                if(renderedSequence!=shared.ViewSequence){Render();renderedSequence=shared.ViewSequence;}
                saveLabel.text=string.Join("   ·   ",shared.View.players.Where(p=>shared.Players.Contains(p.id)).Select(p=>p.id.Replace("player-","Player ")+": "+p.zone))+"   ·   "+shared.Status;
                if(!shared.Connected)return;
                FinishTravel();
                if(MenuOpen || TravelPending)shared.Walk(WalkMode.Stop);
                else if(JoystickMode)shared.Walk(stickDirection.sqrMagnitude>.0001f?WalkMode.Direction:WalkMode.Stop,stickDirection.x,stickDirection.y);
                else if(destination.HasValue)
                {
                    var player=ReadPlayer(Actor);
                    if(Vector2.Distance(new Vector2(player.x,player.y),destination.Value)<1){destination=null;shared.Walk(WalkMode.Stop);}
                    else shared.Walk(WalkMode.Destination,destination.Value.x,destination.Value.y);
                }
                else shared.Walk(WalkMode.Stop);
                return;
            }
            if(!HasWorld || MenuOpen || TravelPending)return;
            // Minimized Windows players may update much faster than presentation.
            // Bound command creation independently of render frequency.
            var now=Time.realtimeSinceStartup;
            if(now<nextMovement)return;
            var delta=Mathf.Clamp(now-lastMovement,0,.1f);lastMovement=now;nextMovement=now+1f/30;
            if(shared!=null)delta=Mathf.Clamp(now-lastSharedMovement,0,.1f);
            var p=ReadPlayer(Actor);var current=new Vector2(p.x,p.y);var next=current;
            if(JoystickMode)next+=stickDirection*(210*delta);
            else if(destination.HasValue){next=Vector2.MoveTowards(current,destination.Value,210*delta);if(Vector2.Distance(shared==null?next:current,destination.Value)<1)destination=null;}
            next.x=Mathf.Clamp(next.x,40,960);next.y=Mathf.Clamp(next.y,35,455);
            if(Vector2.Distance(current,next)>.01f && (shared==null || !shared.Busy)){lastSharedMovement=now;Command(SoloAction.Move,x:next.x,y:next.y);}
            if(dirty && Time.realtimeSinceStartup>=nextSave){SaveNow();nextSave=Time.realtimeSinceStartup+1;}
        }
        public void SaveNow()
        {
            if(World==null || store==null)return;
            try{store.Save(JsonUtility.ToJson(World.Snapshot()));dirty=false;if(saveLabel!=null)saveLabel.text="Saved on this device · solo prototype · "+Application.version;}
            catch(Exception e){if(saveLabel!=null)saveLabel.text="Couldn't save yet. Please ask a grown-up.";Debug.LogError("Solo checkpoint: "+e.Message);}
        }
        private void OnApplicationPause(bool paused){if(paused){CancelPointers();SaveNow();}}
        private void OnApplicationFocus(bool focused){if(!focused && HasWorld){CancelPointers();SaveNow();}}
        private void OnApplicationQuit(){if(HasWorld){CancelPointers();SaveNow();}}
        private Vector2 ToBoard(float x,float y)=>new Vector2((x/SoloWorld.Width-.5f)*Board.rect.width,(y/SoloWorld.Height-.5f)*Board.rect.height);
        private void LateUpdate()
        {
            if(shared==null || !Ready)return;
            var own=shared.VisualPosition(Actor);avatar.anchoredPosition=ToBoard(own.x,own.y);
            foreach(var friend in friends)
            {if(!friend.Value.root.gameObject.activeSelf)continue;var p=shared.VisualPosition(friend.Key);friend.Value.root.anchoredPosition=ToBoard(p.x,p.y);}
            foreach(var t in ReadToys())if(t.id!=dragging && !string.IsNullOrEmpty(t.holder) && shared.TryPreview(t.id,out var p))toys[t.id].anchoredPosition=ToBoard(p.x,p.y);
            foreach(var rect in toys.Where(pair=>pair.Key!=dragging && pair.Value.gameObject.activeSelf).Select(pair=>pair.Value).Concat(new[]{avatar}).Concat(friends.Values.Where(v=>v.root.gameObject.activeSelf).Select(v=>v.root)).OrderByDescending(r=>r.anchoredPosition.y))rect.SetAsLastSibling();
            if(dragging!=null)toys[dragging].SetAsLastSibling();
        }
        private void Render()
        {
            if(avatar==null)return;
            var zone=CurrentArea;var creek=zone=="creek";
            areaLabel.text=(shared==null?"Garden play lab":Actor.Replace("player-","Player ")+" · "+(creek?"Creek":"Garden"))+" / "+Application.version;
            Board.GetComponent<Image>().color=creek?new Color(.7f,.85f,.71f):new Color(.76f,.89f,.72f);
            floorPath.color=creek?new Color(.37f,.72f,.87f):new Color(.9f,.81f,.64f);
            foreach(var part in fence)part.SetActive(!creek);
            foreach(var pair in travelButtons)pair.Value.interactable=shared.Connected && (pair.Key!=zone || TravelPending);
            foreach(var toy in AllToys())toys[toy.id].gameObject.SetActive(SoloWorld.AreaOf(toy.zone)==zone);
            var toyStates=ReadToys();var p=ReadPlayer(Actor);avatar.anchoredPosition=ToBoard(p.x,p.y);
            head.color=p.avatar=="blue-pup"?new Color(.35f,.65f,.85f):new Color(.94f,.58f,.31f);body.color=head.color;
            foreach(var t in toyStates){if(t.id!=dragging)toys[t.id].anchoredPosition=ToBoard(t.x,t.y);
                if(shared!=null)
                {
                    if(t.id!=dragging && !string.IsNullOrEmpty(t.holder) && shared.TryPreview(t.id,out var preview))toys[t.id].anchoredPosition=ToBoard(preview.x,preview.y);
                    var group=toys[t.id].GetComponent<CanvasGroup>();group.alpha=t.id==dragging && !grabConfirmed ? .55f : 1;
                    holders[t.id].text=t.id==dragging && !grabConfirmed?"Picking up…":string.IsNullOrEmpty(t.holder)?"":t.holder==Actor?"Yours":t.holder.Replace("player-","Player ")+" has it";
                }
                if(t.kind==ToyKind.Bucket)fills[t.id].rectTransform.sizeDelta=new Vector2(58,5+13*t.water);
                if(t.kind==ToyKind.Plant)fills[t.id].gameObject.SetActive(t.water==3);
                if(t.kind==ToyKind.Puddle)fills[t.id].rectTransform.localScale=Vector3.one*(t.water/3f);
            }
            if(shared!=null)RenderFriends();
            // Larger y is farther back on the illustrated floor plane.
            foreach(var rect in toys.Where(pair=>pair.Key!=dragging).Select(pair=>pair.Value).Concat(new[]{avatar}).Concat(friends.Values.Select(v=>v.root)).OrderByDescending(r=>r.anchoredPosition.y))rect.SetAsLastSibling();
            if(dragging!=null)toys[dragging].SetAsLastSibling();
            activity.text=p.activity==""?"Free play · walk, drag, discover":p.activity=="garden"?(toyStates.First(t=>t.kind==ToyKind.Plant).water==3?"Your flower is happy! Keep exploring.":"Give the flower a drink"):(toyStates.First(t=>t.kind==ToyKind.Puddle).water==0?"All tidy! Keep exploring.":"Soak up the puddle");
        }
        private void DrawToy(SoloToy t)
        {
            var root=Rect(Board,t.id,ToBoard(t.x,t.y),new Vector2(125,115));toys.Add(t.id,root);
            if(shared!=null){root.gameObject.AddComponent<CanvasGroup>();holders[t.id]=Label(root,"",16,new Vector2(0,-91),new Vector2(190,28));}
            var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=t.kind==ToyKind.Bucket || t.kind==ToyKind.Sponge;
            // The graphic must stay in the raycast list after transparent-mesh culling.
            hit.canvasRenderer.cullTransparentMesh=false;
            if(t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle)
            {
                var ring=Panel(root,"Helpful target ring",Vector2.zero,new Vector2(202,180),Color.white);
                ring.sprite=hintRing;ring.type=Image.Type.Simple;targetRings.Add(t.id,ring);ring.gameObject.SetActive(false);
                var arrow=Rect(root,"Drop here",new Vector2(0,108),new Vector2(50,50));
                Panel(arrow,"Stem",new Vector2(0,5),new Vector2(9,34),Ink);
                var left=Panel(arrow,"Arrow left",new Vector2(-8,-8),new Vector2(24,9),Ink);left.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
                var right=Panel(arrow,"Arrow right",new Vector2(8,-8),new Vector2(24,9),Ink);right.rectTransform.localRotation=Quaternion.Euler(0,0,45);
                targetArrows.Add(t.id,arrow.gameObject);arrow.gameObject.SetActive(false);
            }
            if(hit.raycastTarget)Surface(root,t.id);
            if(t.kind==ToyKind.Bucket){Panel(root,"Handle",new Vector2(0,27),new Vector2(65,50),Ink,false,true);Panel(root,"Handle hole",new Vector2(0,29),new Vector2(52,40),new Color(.76f,.89f,.72f),false,true);Panel(root,"Bucket",Vector2.zero,new Vector2(80,70),new Color(.98f,.67f,.28f));fills[t.id]=Panel(root,"Water",new Vector2(0,3),new Vector2(58,10),new Color(.32f,.68f,.91f));}
            if(t.kind==ToyKind.Sponge){Panel(root,"Sponge",Vector2.zero,new Vector2(92,53),new Color(1,.87f,.39f));for(var i=0;i<4;i++)Panel(root,"Hole",new Vector2(-27+i*18,(i%2)*15-8),new Vector2(8,8),new Color(.78f,.58f,.25f),false,true);}
            if(t.kind==ToyKind.Tap){Panel(root,"Tap pipe",new Vector2(-14,5),new Vector2(28,100),new Color(.47f,.61f,.68f));Panel(root,"Spout",new Vector2(14,40),new Vector2(74,26),new Color(.59f,.71f,.76f));Panel(root,"Handle",new Vector2(-14,66),new Vector2(67,18),new Color(.29f,.5f,.61f));Panel(root,"Drop",new Vector2(40,6),new Vector2(20,28),new Color(.29f,.65f,.88f),false,true);}
            if(t.kind==ToyKind.Plant){Panel(root,"Stem",new Vector2(0,23),new Vector2(10,79),new Color(.27f,.51f,.29f));Panel(root,"Leaf",new Vector2(-19,32),new Vector2(40,20),new Color(.38f,.66f,.33f),false,true);Panel(root,"Pot",new Vector2(0,-22),new Vector2(76,54),new Color(.8f,.43f,.3f));fills[t.id]=Panel(root,"Bloom",new Vector2(0,64),new Vector2(68,68),new Color(.96f,.52f,.61f),false,true);Panel(fills[t.id].transform,"Pollen",Vector2.zero,new Vector2(26,26),new Color(1,.84f,.35f),false,true);}
            if(t.kind==ToyKind.Puddle)fills[t.id]=Panel(root,"Puddle",Vector2.zero,new Vector2(126,49),new Color(.41f,.73f,.86f),false,true);
            Label(root,t.kind.ToString(),18,new Vector2(0,-70),new Vector2(135,32));
        }
        private void DrawAvatar()
        {var visual=CreateAvatar("Pup");avatar=visual.root;head=visual.head;body=visual.body;}
        private (RectTransform root,Image head,Image body) CreateAvatar(string name)
        {
            var avatar=Rect(Board,name,Vector2.zero,new Vector2(85,136));
            Panel(avatar,"Shadow",new Vector2(0,-45),new Vector2(92,22),new Color(.27f,.41f,.26f,.3f),false,true);
            var body=Panel(avatar,"Body",new Vector2(0,-8),new Vector2(61,75),Color.white);
            var head=Panel(avatar,"Head",new Vector2(0,50),new Vector2(87,72),Color.white);
            Panel(head.transform,"Ear",new Vector2(-30,43),new Vector2(24,43),new Color(.23f,.38f,.48f));
            Panel(head.transform,"Ear",new Vector2(30,43),new Vector2(24,43),new Color(.23f,.38f,.48f));
            Panel(head.transform,"Muzzle",new Vector2(0,-10),new Vector2(67,32),Cream);
            foreach(var x in new[]{-20,20}){Panel(head.transform,"Eye",new Vector2(x,10),new Vector2(22,27),Color.white,false,true);Panel(head.transform,"Pupil",new Vector2(x,8),new Vector2(9,13),Ink,false,true);}
            Panel(head.transform,"Nose",new Vector2(0,-9),new Vector2(20,13),Ink,false,true);
            return(avatar,head,body);
        }
        private void RenderFriends()
        {
            foreach(var player in shared.View.players.Where(p=>p.id!=Actor))
            {
                if(!friends.TryGetValue(player.id,out var visual))
                {
                    visual=CreateAvatar("Friend-"+player.id);friends.Add(player.id,visual);
                    Label(visual.root,player.id.Replace("player-","Player "),16,new Vector2(0,-66),new Vector2(110,28));
                }
                visual.root.gameObject.SetActive(shared.Players.Contains(player.id) && SoloWorld.AreaOf(player.zone)==CurrentArea);
                visual.root.anchoredPosition=ToBoard(player.x,player.y);
                visual.head.color=player.avatar=="blue-pup"?new Color(.35f,.65f,.85f):new Color(.94f,.58f,.31f);visual.body.color=visual.head.color;
            }
        }
        private void Surface(RectTransform rect,string role){var s=rect.gameObject.AddComponent<SoloPointerSurface>();s.Screen=this;s.Role=role;Surfaces.Add(role,s);}
        private RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=position;r.sizeDelta=size;return r;}
        private Image Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color,bool raycast=false,bool oval=false)
        {var img=Rect(parent,name,position,size).gameObject.AddComponent<Image>();img.sprite=oval?circle:rounded;img.type=oval?Image.Type.Simple:Image.Type.Sliced;img.color=color;img.raycastTarget=raycast;return img;}
        private Text Label(Transform parent,string text,int size,Vector2 position,Vector2 dimensions)
        {var label=Rect(parent,text,position,dimensions).gameObject.AddComponent<Text>();label.font=font;label.fontSize=size;label.text=text;label.color=Ink;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;return label;}
        private Text Button(Transform parent,string text,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,Color color)
        {var image=Panel(parent,text,position,size,color,true);var button=image.gameObject.AddComponent<Button>();button.onClick.AddListener(action);return Label(image.transform,text,22,Vector2.zero,size);}
        private Sprite Shape(bool oval)
        {
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            for(var y=0;y<64;y++)for(var x=0;x<64;x++){var dx=Mathf.Abs(x-31.5f);var dy=Mathf.Abs(y-31.5f);var d=oval?Mathf.Sqrt(dx*dx+dy*dy)-31:Mathf.Sqrt(Mathf.Pow(Mathf.Max(0,dx-18),2)+Mathf.Pow(Mathf.Max(0,dy-18),2))-13;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(1-d)));}
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,oval?Vector4.zero:new Vector4(16,16,16,16));
        }
        private Sprite Ring()
        {
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            for(var y=0;y<96;y++)for(var x=0;x<96;x++)
            {var distance=Vector2.Distance(new Vector2(x,y),new Vector2(47.5f,47.5f));texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(47-distance)*Mathf.Clamp01(distance-41)));}
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,96,96),new Vector2(.5f,.5f));
        }
        private void UpdateSafeArea(){lastSafeArea=Screen.safeArea;safe.anchorMin=new Vector2(lastSafeArea.xMin/Screen.width,lastSafeArea.yMin/Screen.height);safe.anchorMax=new Vector2(lastSafeArea.xMax/Screen.width,lastSafeArea.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
    }
}
