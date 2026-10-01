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
    public sealed partial class SoloScreen : MonoBehaviour
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
        public bool MenuOpen => WaveRideCardOpen || TeacherCardOpen || BoatWorkshopOpen || OutfitsOpen || PondCloseup || CreekFishingCloseup || MiniGamesOpen || HideCardOpen || CollectionOpen || DiscoveryOpen || BookOpen || BookLibraryOpen || menu != null && menu.activeSelf || CharactersOpen || WorldLoading || sceneryCurtain!=null && sceneryCurtain.activeSelf;
        public readonly Dictionary<string, SoloPointerSurface> Surfaces = new Dictionary<string, SoloPointerSurface>();
        private readonly Dictionary<string, RectTransform> toys = new Dictionary<string, RectTransform>();
        private readonly Dictionary<string, Image> fills = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> targetRings = new Dictionary<string, Image>();
        private readonly Dictionary<string, GameObject> targetArrows = new Dictionary<string, GameObject>();
        private readonly Dictionary<string,GameObject> resetCues=new Dictionary<string,GameObject>();
        private readonly Dictionary<RectTransform,Vector2> layoutPositions=new Dictionary<RectTransform,Vector2>();
        private CheckpointStore store;
        private SoloWorld localWorld;
        private Action familyModeAction;
        private bool familyTestMuted;
        private float lastLocalAction, nextTransitionSave;
        private GameObject ownedCanvas, ownedEvents;
        public void ConfigureFamilyMode(Action changeMode,bool muted=false) { familyModeAction=changeMode;familyTestMuted=muted; }
        public bool RecoveringDisconnected => Shared && !shared.Connected;
        public bool CanChangeSession => Ready && (!WorldLoading || RecoveringDisconnected) && dragging==null && !ActionPending && !TravelPending &&
            !destination.HasValue && stickDirection.sqrMagnitude<.0001f && !Surfaces.Values.Any(s=>s.Pressed) &&
            // Lost-session gestures were canceled. A finger still resting on
            // glass must not veto recovery; joining live play still waits for it.
            (RecoveringDisconnected || !UnityEngine.InputSystem.InputSystem.devices.Any(d=>
                d is UnityEngine.InputSystem.Mouse m && m.leftButton.isPressed ||
                d is UnityEngine.InputSystem.Touchscreen t && t.touches.Any(p=>p.press.isPressed)));
        public bool TryJoinFamily(IGardenSession session)
        {
            if(Shared || !CanChangeSession || MenuOpen || Time.realtimeSinceStartup-lastLocalAction<1 ||
                Time.realtimeSinceStartup<nextTransitionSave || !session.Connected || session.View==null)return false;
            nextTransitionSave=Time.realtimeSinceStartup+2;
            // Never replace the only live copy with a server snapshot. Commit the
            // separate device branch before showing the server's current world.
            // Offline edits are never uploaded or replayed on this transition.
            if(!TrySaveNow())return false;
            if(!TryDeselectAdventure())return false;
            var savedAdventure=continuation!=null;
            if(continuation==null)localWorld=World;
            continuation=null;pendingContinuation=null;SavePath=soloSavePath;ResetPresentation();World=null;shared=session;Actor=session.Actor;
            InitializeShared();message.text=savedAdventure?"Your adventure is in Menu â†’ Saved adventures. You're with family now!":"Your solo play is saved. You're playing with family now!";
            return true;
        }
        public bool TryReturnToLocal()
        {
            if(localWorld==null || !CanChangeSession)return false;
            if(!Shared && continuation==null)return TryDeselectAdventure();
            if(World!=null && !TrySaveNow() || !TryDeselectAdventure())return false;
            var keepMenu=MenuOpen;
            continuation=null;SavePath=soloSavePath;
            ResetPresentation();shared=null;World=localWorld;Actor=offlineActor ?? World.Snapshot().players[0].id;
            BuildScreen();Render();message.text="Your saved solo play. Menu lets you find your family again.";
            if(keepMenu)SetMenu(true);
            return true;
        }
        private void ResetPresentation()
        {
            CancelPointers();CancelStairApproach();ResetBedrooms();ResetBathroom();ResetBedroomFurniture();ResetRoomPlay();ResetCollections();ResetDiscovery();ResetKitchen();ResetBooks();ResetSecrets();stairControl=null;stairFront=null;stairVisuals.Clear();Narration?.Stop();ResetScenery();ResetHome();
            // Keep one canvas, event system and narration source across switches.
            // Disable old children now so deferred Destroy cannot receive input.
            foreach(Transform child in safe){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            Surfaces.Clear();toys.Clear();fills.Clear();targetRings.Clear();targetArrows.Clear();
            resetCues.Clear();layoutPositions.Clear();
            friends.Clear();holders.Clear();travelButtons.Clear();fence.Clear();
            ResetDaycare();ResetKingdom();ResetNavigation();
            Board=null;avatar=null;menu=null;connecting=null;dragging=null;requestedArea=null;travelSubmitted=false;
            grabConfirmed=false;gestureEnded=false;gestureCancelled=false;dropSubmitted=false;renderedSequence=-1;
        }
        private string offlineBranch,offlineActor;
        public void ConfigureOfflineBranch(string world,string profile)
        {
            if(safe!=null || !FamilyPairing.Id(world) || !FamilyPairing.Id(profile))throw new ArgumentException("Invalid offline branch.");
            offlineBranch="paired-"+world+"-"+profile;offlineActor=profile;
        }
        private RectTransform safe, avatar, stickKnob, stick;
        private GameCharacterVisual characterVisual;
        public string DisplayedCharacterId => characterVisual == null ? "" : characterVisual.CharacterId;
        public int DisplayedCharacterLayers => characterVisual == null ? 0 : characterVisual.LayerCount;
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
        private readonly Dictionary<string,(RectTransform root,GameCharacterVisual view)> friends=new Dictionary<string,(RectTransform,GameCharacterVisual)>();
        private readonly Dictionary<string,Text> holders=new Dictionary<string,Text>();
        public bool Shared=>shared!=null;
        public bool Ready=>Board!=null && (World!=null || shared?.View!=null);
        public bool ActionPending=>shared!=null && (shared.Busy || TravelPending);
        public string Feedback=>message?.text??"";
        public string Dragging=>dragging;
        public int VisiblePlayers=>1+friends.Values.Count(v=>v.root.gameObject.activeSelf);
        public void Configure(IGardenSession session){if(safe!=null)throw new InvalidOperationException("Configure before Start.");shared=session;}
        public SoloPlayer ReadPlayer(string id)=>shared==null?World.ReadPlayer(id):shared.View.players.First(p=>p.id==id).Copy();
        private SoloToy[] frameToys,frameAreaToys;
        private int frameToysAt=-1;private SoloSnapshot frameToyView;private string frameToyArea;
        private SoloToy[] AllToys()
        {
            var view=shared?.View;
            if(frameToysAt!=Time.frameCount || frameToyView!=view || frameToys==null){
                frameToysAt=Time.frameCount;frameToyView=view;
                frameToys=shared==null?World.ReadToys():view.toys.Select(t=>t.Copy()).ToArray();
                frameAreaToys=null;frameToyArea=null;
            }
            return frameToys;
        }
        public SoloToy[] ReadToys()
        {
            var items=AllToys();var area=CurrentArea;
            if(frameAreaToys==null || frameToyArea!=area){frameToyArea=area;frameAreaToys=items.Where(t=>SoloWorld.AreaOf(t.zone)==area).ToArray();}
            return frameAreaToys;
        }
        private bool HasWorld=>World!=null || shared?.View!=null;
        private float nextSave;
        private bool applicationPaused;
        private Rect lastSafeArea;
        private static readonly Color Ink = new Color(.15f,.25f,.29f), Cream = new Color(.98f,.96f,.88f);
        private void Start()
        {
            Application.targetFrameRate = 60; Application.runInBackground = true;
            if(!ReadVerificationArgs())
            {Debug.LogError("Invalid solo verification arguments; normal saved play was not opened.");Application.Quit(2);return;}
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded = Shape(false); circle = Shape(true);hintRing=Ring();
            var canvas = new GameObject("Solo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ownedCanvas=canvas;
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scale = canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(1280,800); scale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            safe = Rect(canvas.transform,"Safe Area",Vector2.zero,Vector2.zero); UpdateSafeArea();
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Solo Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
                ownedEvents=events;
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            if(shared!=null)
            {
                Actor=shared.Actor;connecting=Label(safe,"Joining your shared gardenâ€¦",32,Vector2.zero,new Vector2(1000,160));
                return;
            }
            SavePath = Path.Combine(Application.persistentDataPath,"SoloPrototype",VerifyRun ?? offlineBranch ?? "family-local","world.save");
            soloSavePath=SavePath;
            store = new CheckpointStore(SavePath, ValidPayload);
            try
            {
                var loaded = store.Load(); LoadedStatus = loaded.Status;
                if (loaded.Status == CheckpointStatus.Corrupt || loaded.Status == CheckpointStatus.Unsupported)
                    throw new InvalidDataException("Existing save needs recovery or a compatible version.");
                var snapshot=loaded.Status==CheckpointStatus.Missing?null:JsonUtility.FromJson<SoloSnapshot>(loaded.Payload);
                World = snapshot==null ? SoloWorld.Create(offlineActor ?? Guid.NewGuid().ToString("N")) : SoloWorld.Restore(snapshot);
                // The existing additive area upgrade preserves the old garden,
                // player and receipts while adding the missing Creek station.
                World = SoloWorld.WithDinosaurWorld(World);
                World = SoloWorld.WithWaveRides(World);
                // Restore releases interrupted holds, hiding roles and fixture/stair
                // leases. Persist every revision-changing restore, including an
                // otherwise idle player who has just come out of cover.
                // Otherwise merely opening another saved adventure must not
                // rewrite an untouched solo payload (including precise timers).
                dirty=snapshot==null || snapshot.revision!=World.Revision;
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
            Narration=gameObject.AddComponent<SoloNarration>();Narration.Initialize(VerifyRun!=null || familyTestMuted);
            Narration.SetVoiceEnabled(PlayerPrefs.GetInt(PreferenceKey("voice"),1)!=0);
            BuildScreen(); Render();
            if (LoadedStatus == CheckpointStatus.Missing) SaveNow();
            if (LoadedStatus == CheckpointStatus.Recovered) message.text = "Your last safe save is back. Let's play!";
            localWorld=World;OpenInitialAdventure();
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
            if(connecting!=null)connecting.gameObject.SetActive(false);
            if(FindAnyObjectByType<AudioListener>()==null)gameObject.AddComponent<AudioListener>();
            if(FindAnyObjectByType<Camera>()==null)
            {
                var camera=new GameObject("Shared Garden Camera",typeof(Camera)).GetComponent<Camera>();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Cream;camera.transform.position=new Vector3(0,0,-10);
            }
            JoystickMode=PlayerPrefs.GetInt(PreferenceKey("joystick"),0)==1;
            if(Narration==null){Narration=gameObject.AddComponent<SoloNarration>();Narration.Initialize(shared.MutedTest);}
            Narration.SetVoiceEnabled(PlayerPrefs.GetInt(PreferenceKey("voice"),1)!=0);
            BuildScreen();Render();wasConnected=shared.Connected;
        }
        public static bool ValidPayload(string payload)
        {
            try
            {
                var snapshot = JsonUtility.FromJson<SoloSnapshot>(payload);
                if (snapshot != null && snapshot.schema > WorldLayout.Schema) throw new NotSupportedException("Newer solo save schema.");
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
            areaLabel=Label(safe,"",17,new Vector2(-245,313),new Vector2(350,34));
            // One safe-area corner anchor keeps both controls on the same row,
            // including the first frame and tablet/phone orientation changes.
            var controls=Rect(safe,"Play controls",new Vector2(-20,-23),new Vector2(390,64));
            controls.anchorMin=controls.anchorMax=Vector2.one;controls.pivot=Vector2.one;
            movementLabel=Button(controls,JoystickMode?"Joystick":"Tap to walk",new Vector2(-90,0),new Vector2(210,64),ToggleMovement,Cream);
            Button(controls,"Menu",new Vector2(115,0),new Vector2(160,64),()=>SetMenu(true),Cream);
            // Legacy prototype activities: kept for now, may be removed later.
            // These are not entries in the new Games menu; Keepy Uppy stays balloon-triggered.
            var garden=Button(safe,"Grow a flower",new Vector2(-380,265),new Vector2(245,48),()=>StartActivity("garden"),new Color(.86f,.93f,.74f));
            garden.rectTransform.anchoredPosition=new Vector2(22,0);garden.rectTransform.sizeDelta=new Vector2(191,48);
            var blossom=Panel(garden.transform.parent,"Flower picture",new Vector2(-93,0),new Vector2(33,33),new Color(.96f,.52f,.61f),false,true);
            Panel(blossom.transform,"Center",Vector2.zero,new Vector2(12,12),new Color(1,.84f,.35f),false,true);
            var cleanup=Button(safe,"Splash cleanup",new Vector2(-115,265),new Vector2(245,48),()=>StartActivity("cleanup"),new Color(.76f,.9f,1));
            cleanup.rectTransform.anchoredPosition=new Vector2(22,0);cleanup.rectTransform.sizeDelta=new Vector2(191,48);
            Panel(cleanup.transform.parent,"Sponge picture",new Vector2(-94,0),new Vector2(34,23),new Color(1,.83f,.28f));
            Button(safe,"Free play",new Vector2(120,265),new Vector2(190,48),()=>StartActivity(""),Cream);
            Board=Panel(safe,"Garden",new Vector2(0,-25),new Vector2(1120,500),new Color(.76f,.89f,.72f),true).rectTransform;
            Surface(Board,"ground");
            Panel(Board,"Sky",new Vector2(0,140),new Vector2(1116,216),new Color(.8f,.92f,.96f));
            for(var i=0;i<7;i++) fence.Add(Panel(Board,"Fence",new Vector2(-465+i*155,80),new Vector2(142,65),new Color(.99f,.95f,.83f)).gameObject);
            floorPath=Panel(Board,"Path",new Vector2(0,-156),new Vector2(1050,70),new Color(.9f,.81f,.64f));
            foreach(var toy in AllToys()) DrawToy(toy);
            DrawAvatar();
            stick=Panel(safe,"Walk joystick",new Vector2(-480,-223),new Vector2(146,146),new Color(1,1,1,.9f),true,true).rectTransform;
            Panel(stick,"Joystick face",Vector2.zero,new Vector2(134,134),new Color(.55f,.82f,.94f,.85f),false,true);
            Surface(stick,"stick");
            stickKnob=Panel(stick,"Thumb",Vector2.zero,new Vector2(65,65),Color.white,false,true).rectTransform;
            Panel(stickKnob,"Thumb face",Vector2.zero,new Vector2(55,55),new Color(.28f,.65f,.83f),false,true);
            stick.gameObject.SetActive(JoystickMode);
            activity=Label(safe,"",24,new Vector2(0,-312),new Vector2(1130,44));
            message=Label(safe,"Drag the bucket to the tap, then to the plant.",21,new Vector2(-85,-354),new Vector2(950,40));
            var listen=Button(safe,"Listen",new Vector2(490,-352),new Vector2(150,52),Listen,Cream);
            listenLabel=listen;
            listen.rectTransform.anchoredPosition=new Vector2(15,0);listen.rectTransform.sizeDelta=new Vector2(108,52);
            for(var i=0;i<3;i++)Panel(listen.transform.parent,"Sound",new Vector2(-52+i*9,0),new Vector2(5,12+i*9),Ink);
            saveLabel=Label(safe,"Local solo prototype Â· placeholder art",15,new Vector2(0,-380),new Vector2(1130,28));
            menu=Panel(safe,"Pause panel",Vector2.zero,new Vector2(1190,760),new Color(.97f,.97f,.91f,.98f),true).gameObject;
            Label(menu.transform,"Take your time",42,new Vector2(0,230),new Vector2(760,100));
            Label(menu.transform,"Unused garden tools go back after a while.\nYou can always leave an activity.",25,new Vector2(0,140),new Vector2(800,80));
            voiceLabel=Button(menu.transform,"Voice on",new Vector2(0,35),new Vector2(350,75),ToggleVoice,new Color(.77f,.88f,.96f));
            voiceLabel.transform.parent.name="Voice setting";
            voiceLabel.rectTransform.anchoredPosition=new Vector2(20,0);voiceLabel.rectTransform.sizeDelta=new Vector2(250,75);
            for(var i=0;i<3;i++)Panel(voiceLabel.transform.parent,"Sound",new Vector2(-130+i*12,0),new Vector2(7,15+i*12),Ink);
            var slash=Panel(voiceLabel.transform.parent,"Voice off mark",new Vector2(-118,0),new Vector2(55,7),new Color(.73f,.26f,.23f));
            slash.rectTransform.localRotation=Quaternion.Euler(0,0,45);voiceSlash=slash.gameObject;
            Label(menu.transform,"Spoken hints on this device",18,new Vector2(0,-24),new Vector2(650,36));
            Button(menu.transform,"Back to play",new Vector2(0,-90),new Vector2(350,85),()=>SetMenu(false),new Color(.81f,.92f,.72f));
            Button(menu.transform,"Leave activity",new Vector2(0,-200),new Vector2(350,75),()=>{StartActivity("");SetMenu(false);},Cream);
            if(familyModeAction!=null)
            {
                Button(menu.transform,Shared?"Play by myself":"Find my family",new Vector2(-220,-290),new Vector2(390,65),()=>familyModeAction(),new Color(.77f,.88f,.96f));
                if(adventures!=null)Button(menu.transform,"Saved adventures",new Vector2(220,-290),new Vector2(390,65),()=>ShowAdventures(0),new Color(.81f,.92f,.72f));
                Label(menu.transform,"Solo, family and saved adventures stay separate.",18,new Vector2(0,-341),new Vector2(850,32));
            }
            UpdateVoiceControls();
            menu.SetActive(false);
            BuildNavigation();BuildScenery();BuildHome();BuildKeepy();BuildRooms();BuildBedrooms();BuildBedroomFurniture();BuildSecrets();BuildBooks();BuildRoomPlay();BuildKitchen();BuildDiscovery();BuildHideAndSeek();BuildPark();BuildTag();BuildPond();BuildBathroom();BuildCreekBoats();BuildCreekFishing();BuildZoo();BuildDinosaurWorld();BuildKingdom();BuildDaycare();BuildSeagulls();BuildShore();BuildWaveRide();
            // Session switches destroy the old (already disabled) children at
            // frame end; do not retain them for later orientation/layout changes.
            foreach(RectTransform child in safe)if(child.gameObject.activeSelf)layoutPositions[child]=child.anchoredPosition;
            ApplyResponsiveLayout();
        }
        public SoloResult Command(SoloAction action,string item="",string target="",string value="",float x=0,float y=0)
        {
            if(shared!=null)
            {
                SubmitShared(action,item,target,value,x,y,result=>
                {if(!result.Accepted && result.Outcome!="superseded")message.text=Friendly(result.Outcome);Render();});
                return new SoloResult(false,"pending",shared.View.revision);
            }
            var player=World.ReadPlayer(Actor);
            var result=World.Apply(new SoloCommand {requestId=Guid.NewGuid().ToString("N"),actor=Actor,zone=player.zone,visit=player.visit,expectedRevision=World.Revision,action=action,item=item,target=target,value=value,x=x,y=y});
            if(result.Accepted) {dirty=true;lastLocalAction=Time.realtimeSinceStartup;Render();}
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
            if(HasWorld && shared==null && WorldLayout.Destination(zone))
            {
                if(zone==CurrentPlace)return;
                CancelPointers();Narration.Stop();
                var result=Command(SoloAction.Travel,value:zone);
                if(WorldLoading && !result.Accepted)travelFailure=result.Outcome;
                message.text=result.Accepted?"Welcome to the "+zone+". Keep exploring!":Friendly(result.Outcome);
                return;
            }
            if(shared==null || !shared.Connected || !WorldLayout.Destination(zone))return;
            requestedArea=zone;CancelPointers();Narration.Stop();
            message.text="Going to the "+zone+"â€¦";
            Render();
        }
        private void FinishTravel()
        {
            if(requestedArea==null || travelSubmitted || shared.Busy || dragging!=null)return;
            if(requestedArea==CurrentPlace){requestedArea=null;message.text="Keep exploring the "+CurrentArea+".";Render();return;}
            var target=requestedArea;travelSubmitted=true;
            SubmitShared(SoloAction.Travel,"","",target,0,0,result=>
            {
                travelSubmitted=false;
                if(WorldLoading && !result.Accepted)travelFailure=result.Outcome;
                if(!result.Accepted || requestedArea==target)requestedArea=null;
                message.text=result.Accepted?"Welcome to the "+target+". Your friends can join you here!":Friendly(result.Outcome);
                Render();
            });
        }
        public void StartActivity(string id)
        {
            if(id==KeepyRules.Activity && CurrentArea!="garden")return;
            if(!HasWorld || TravelPending || id!="" && CurrentArea!="garden" && CurrentArea!="creek")return;
            if(shared!=null)
            {
                SubmitShared(id==""?SoloAction.LeaveActivity:SoloAction.StartActivity,"","",id,0,0,result=>
                {if(result.Accepted){if(id!=KeepyRules.Activity)Narration.Speak(id==""?"freeplay":id);message.text=id==KeepyRules.Activity?"Move under the balloon to tap it up!":"Pick any toy. You can leave this activity any time.";}else message.text=Friendly(result.Outcome);Render();});
                return;
            }
            var result=Command(id==""?SoloAction.LeaveActivity:SoloAction.StartActivity,value:id);
            if(!result.Accepted){message.text=Friendly(result.Outcome);return;}
            if(id==KeepyRules.Activity){message.text="Move under the balloon to tap it up!";return;}
            Narration.Speak(id==""?"freeplay":id);
            message.text=id==""?"All your toys still work. Explore!":id=="garden"?"Fill your bucket at the tap. Give the flower a drink!":"Drag the sponge over the puddle to soak it up.";
        }
        public void ToggleMovement()
        {
            CancelPointers(); JoystickMode=!JoystickMode; movementLabel.text=JoystickMode?"Joystick":"Tap to walk";stick.gameObject.SetActive(JoystickMode && !MenuOpen);
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
        public void SetMenu(bool open) { if(WorldLoading)return;lastLocalAction=Time.realtimeSinceStartup;if(open){CloseNavigation();CancelPointers();Narration.Stop();SaveNow();ExportPlayPerformance();} menu.SetActive(open);if(open)menu.transform.SetAsLastSibling();stick.gameObject.SetActive(JoystickMode && !MenuOpen); }
        public void Listen(){if(ParkTag.Member(TagGame,Actor))return;var activityId=ReadPlayer(Actor).activity;Narration.Speak(activityId==""?"freeplay":activityId);}
        private Vector2 BoardPoint(Vector2 screen)
        { RectTransformUtility.ScreenPointToLocalPointInRectangle(Board,screen,null,out var local);return FromBoard(local); }
        public Vector2 ScreenPoint(float x,float y) => RectTransformUtility.WorldToScreenPoint(null,Board.TransformPoint(ToBoard(x,y)));
        public bool BeginPointer(string role,Vector2 screen)
        {
            if(!HasWorld || MenuOpen || TravelPending || (shared!=null && !shared.Connected))return false;
            if(role=="ground")return BeginGround(screen);
            if(role=="stick") {if(!JoystickMode)return false;MovePointer(role,screen);return true;}
            if(shared!=null)return BeginSharedDrag(role,screen);
            if(dragging!=null || !Command(SoloAction.Grab,role).Accepted)return false;
            dragging=role;MovePointer(role,screen);return true;
        }
        public void MovePointer(string role,Vector2 screen)
        {
            if(role=="ground"){MoveGround(screen);return;}
            if(role=="stick") {manualCamera=false;RectTransformUtility.ScreenPointToLocalPointInRectangle(stick,screen,null,out var point);stickDirection=Vector2.ClampMagnitude(point/55,1);stickKnob.anchoredPosition=stickDirection*40;}
            else if(role==dragging && !gestureEnded) {var point=BoardPoint(screen);dragPoint=point;toys[role].anchoredPosition=ToBoard(point.x,point.y);toys[role].SetAsLastSibling();UpdateTargetHints(point);if(grabConfirmed && ScenePosition(point.x,point.y))shared?.Preview(role,point);}
        }
        public void EndPointer(string role,Vector2 screen)
        {
            if(role=="ground"){EndGround(screen);return;}
            if(role=="stick"){stickDirection=Vector2.zero;stickKnob.anchoredPosition=Vector2.zero;return;}
            if(role!=dragging)return;
            if(shared!=null){dragPoint=BedroomDropPoint(BoardPoint(screen));readAfterDrop=BookDropReads(role,dragPoint)?role:null;gestureEnded=true;gestureCancelled=!ScenePosition(dragPoint.x,dragPoint.y);FinishSharedDrag();return;}
            var point=BedroomDropPoint(BoardPoint(screen));var openBook=BookDropReads(role,point);
            if(!ScenePosition(point.x,point.y)){CancelPointer(role);return;}
            var target=ReadToys().Where(t=>t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle)
                .OrderBy(t=>Vector2.Distance(new Vector2(t.x,t.y),point)).FirstOrDefault();
            var id=target!=null && Vector2.Distance(new Vector2(target.x,target.y),point)<=SoloWorld.InteractionRadius?target.id:"";
            var homeTarget=bedroomDragTarget!=""?bedroomDragTarget:HomeDropTarget(point);if(homeTarget!="")id=homeTarget;
            var result=Command(SoloAction.Drop,role,id,x:point.x,y:point.y);
            if(!result.Accepted)Command(SoloAction.CancelGrab,role);
            dragging=null;HideTargetHints();Render();if(result.Accepted && openBook)OpenBook(role);
            message.text=result.Outcome=="bucket-filled"?"Splash! Your bucket is full.":result.Outcome=="plant-watered"?"A drink for the flower!":result.Outcome=="puddle-cleaned"?"Squeeze, soak, tidy!":"What shall we play next?";
        }
        public void CancelPointer(string role)
        {
            if(role=="ground")groundPan=false;
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
            if(toy==null || !SoloWorld.Carryable(toy.kind))return false;
            if(!string.IsNullOrEmpty(toy.holder)){message.text=Friendly("already-held");return false;}
            dragging=role;grabConfirmed=false;gestureEnded=false;gestureCancelled=false;dropSubmitted=false;
            MovePointer(role,screen);message.text="Picking it upâ€¦";Render();
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
                var nearest=ReadToys().Where(t=>t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle).OrderBy(t=>Vector2.Distance(new Vector2(t.x,t.y),dragPoint)).FirstOrDefault();
                if(nearest!=null && Vector2.Distance(new Vector2(nearest.x,nearest.y),dragPoint)<=SoloWorld.InteractionRadius)target=nearest.id;
                var homeTarget=bedroomDragTarget!=""?bedroomDragTarget:HomeDropTarget(dragPoint);if(homeTarget!="")target=homeTarget;
            }
            var cancelled=gestureCancelled;
            message.text=cancelled?"Putting it backâ€¦":"Finishing your moveâ€¦";
            SubmitShared(cancelled?SoloAction.CancelGrab:SoloAction.Drop,item,target,"",cancelled?0:dragPoint.x,cancelled?0:dragPoint.y,result=>
            {
                if(!result.Accepted && shared.Connected && ReadToys().Any(t=>t.id==item && t.holder==Actor))
                {
                    SubmitShared(SoloAction.CancelGrab,item,"","",0,0,release=>{message.text=Friendly(result.Outcome);ClearSharedDrag();});return;
                }
                message.text=!result.Accepted?Friendly(result.Outcome):cancelled?"Your toy is back. Keep exploring!":result.Outcome=="bucket-filled"?"Splash! Your bucket is full.":result.Outcome=="plant-watered"?"A drink for the flower!":result.Outcome=="puddle-cleaned"?"Squeeze, soak, tidy!":"What shall we play next?";
                var read=result.Accepted && !cancelled?readAfterDrop:null;ClearSharedDrag();if(read!=null)OpenBook(read);
            });
        }
        private void ClearSharedDrag(){readAfterDrop=null;dragging=null;grabConfirmed=false;gestureEnded=false;dropSubmitted=false;HideTargetHints();Render();}
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
            dinosaurCareApproach=false;zooApproach=false;kingdomApproach="";daycareApproach=-1;destination=null;stickDirection=Vector2.zero;
            shared?.Walk(WalkMode.Stop);
        }
        private void Update()
        {
            FinishBackgroundSave(false);
            TickWorldMusic();
            if(Ready)TickCake();
            if(Ready){TickMixing();TickBubbleLab();TickLiquidColors();TickMarbleRamps();}
            if(Ready){CheckKingdomInput();CheckDaycareInput();CheckStairInput();CheckDoorInput();CheckHideInput();CheckZooInput();CheckDinosaurCareInput();}
            if(safe!=null && lastSafeArea!=Screen.safeArea)UpdateSafeArea();
            if(shared!=null)
            {
                if(Board==null){if(shared.View!=null && safe!=null)InitializeShared();else if(connecting!=null && !shared.Connected)connecting.text="Joining your shared gardenâ€¦\n"+shared.Status;return;}
                if(wasConnected && !shared.Connected){requestedArea=null;travelSubmitted=false;CancelPointers();ClearSharedDrag();message.text=Friendly("disconnected");Narration.Stop();}
                if(!wasConnected && shared.Connected){CancelPointers();renderedSequence=-1;message.text="You're back. Let's play!";}
                wasConnected=shared.Connected;
                if(renderedSequence!=shared.ViewSequence){Render();renderedSequence=shared.ViewSequence;}
                saveLabel.text=string.Join("   Â·   ",shared.View.players.Where(p=>shared.Players.Contains(p.id)).Select(p=>p.id.Replace("player-","Player ")+": "+p.zone))+"   Â·   "+shared.Status;
                if(!shared.Connected)return;
                FinishTravel();
                if(MenuOpen || OwnWaveRider!=null || TravelPending || StairBusy || doorSubmitted)shared.Walk(WalkMode.Stop);
                else if(dinosaurCareApproach)shared.Walk(WalkMode.Destination,dinosaurCareEntry.x,dinosaurCareEntry.y);
                else if(zooApproach)shared.Walk(WalkMode.Destination,zooEntry.x,zooEntry.y);
                else if(hideApproach>=0)shared.Walk(WalkMode.Destination,HideEntry.x,HideEntry.y);
                else if(doorApproach)shared.Walk(WalkMode.Destination,DoorEntry.x,DoorEntry.y);
                else if(stairApproach)shared.Walk(WalkMode.Destination,HomeRooms.EntryX(CurrentArea),HomeRooms.EntryY(CurrentArea));
                else if((kingdomApproach!="" || daycareApproach>=0) && destination.HasValue)shared.Walk(WalkMode.Destination,destination.Value.x,destination.Value.y);
                else if(JoystickMode)shared.Walk(stickDirection.sqrMagnitude>.0001f?WalkMode.Direction:WalkMode.Stop,stickDirection.x,stickDirection.y);
                else if(destination.HasValue)
                {
                    var player=ReadPlayer(Actor);
                    if(ParkWheels.Usable(player.fixture)?Mathf.Abs(player.x-Mathf.Clamp(destination.Value.x,ParkWheels.MinX,ParkWheels.MaxX))<1:Vector2.Distance(new Vector2(player.x,player.y),destination.Value)<1){destination=null;shared.Walk(WalkMode.Stop);}
                    else shared.Walk(WalkMode.Destination,destination.Value.x,destination.Value.y);
                }
                else shared.Walk(WalkMode.Stop);
                return;
            }
            if(!HasWorld || MenuOpen || TravelPending || applicationPaused)return;
            // A wall-clock 30 Hz gate skipped render frames even at a 30 fps
            // target. Advance local motion every frame, without transaction
            // receipt churn; save cadence stays separate from visual motion.
            var now=Time.realtimeSinceStartup;
            var delta=Mathf.Clamp(Time.unscaledDeltaTime,0,.1f);
            if(World.AdvanceIdle(delta,out var maintenanceVisible,new[]{Actor}))dirty=true;
            if(maintenanceVisible)Render();
            var mode=OwnWaveRider!=null || StairBusy || doorSubmitted?WalkMode.Stop:dinosaurCareApproach || zooApproach || hideApproach>=0 || doorApproach || stairApproach || kingdomApproach!="" || daycareApproach>=0?WalkMode.Destination:JoystickMode?WalkMode.Direction:destination.HasValue?WalkMode.Destination:WalkMode.Stop;
            var input=dinosaurCareApproach?dinosaurCareEntry:zooApproach?zooEntry:hideApproach>=0?HideEntry:doorApproach?DoorEntry:stairApproach?new Vector2(HomeRooms.EntryX(CurrentArea),HomeRooms.EntryY(CurrentArea)):kingdomApproach!="" || daycareApproach>=0?destination??Vector2.zero:JoystickMode?stickDirection:destination??Vector2.zero;
            if(Walking.AdvanceLocal(World,Actor,mode,input.x,input.y,delta))
            {
                dirty=true;lastLocalAction=now;
                var p=ReadPlayer(Actor);avatar.anchoredPosition=ToBoard(p.x,p.y);SortDepth();
                if(!JoystickMode && destination.HasValue && (ParkWheels.Usable(p.fixture)?Mathf.Abs(p.x-Mathf.Clamp(destination.Value.x,ParkWheels.MinX,ParkWheels.MaxX))<1:Vector2.Distance(new Vector2(p.x,p.y),destination.Value)<1))destination=null;
            }
            if(dirty && Time.realtimeSinceStartup>=nextSave){SaveDuringPlay();nextSave=Time.realtimeSinceStartup+1;}
        }
        public void SaveNow()
        {TrySaveNow();}
        public bool TrySaveNow()
        {
            FinishBackgroundSave(true);
            if(World==null || store==null)return false;
            if(!dirty)return true;
            var saveStarted=System.Diagnostics.Stopwatch.GetTimestamp();
            try{if(continuation!=null)adventures.Save(continuation,World.Snapshot());else store.Save(JsonUtility.ToJson(World.Snapshot()));dirty=false;if(saveLabel!=null)saveLabel.text=continuation!=null?"Your adventure is saved on this device":"Saved on this device Â· solo prototype Â· "+Application.version;return true;}
            catch(Exception e){if(saveLabel!=null)saveLabel.text="Couldn't save yet. Please ask a grown-up.";Debug.LogWarning("Solo checkpoint: "+e.Message);return false;}
            finally{RecordSaveWork(saveStarted);}
        }
        private void OnDestroy()
        {
            if(HasWorld)SaveNow();else FinishBackgroundSave(true);
            ResetScenery(true);ResetSecrets();ResetHome();
            if(ownedCanvas!=null)Destroy(ownedCanvas);if(ownedEvents!=null)Destroy(ownedEvents);
            ResetBooks();if(Narration!=null)Destroy(Narration);
            foreach(var sprite in new[]{rounded,circle,hintRing,pictureRim})if(sprite!=null){Destroy(sprite.texture);Destroy(sprite);}
        }
        private void OnApplicationPause(bool paused){applicationPaused=paused;worldMusic?.Suspend(paused);if(paused){if(DiscoveryOpen)CloseDiscovery();if(CollectionOpen)CloseCollection(false);PauseBook(false);CancelPointers();SettleHomeUse();SaveNow();ExportPlayPerformance();}}
        private void OnApplicationFocus(bool focused){if(!focused && HasWorld){if(DiscoveryOpen)CloseDiscovery();if(CollectionOpen)CloseCollection(false);PauseBook(false);CancelPointers();SettleHomeUse();SaveNow();}}
        private void OnApplicationQuit(){if(HasWorld){PauseBook(false);CancelPointers();SaveNow();}}
        private Vector2 ToBoard(float x,float y)=>new Vector2((x-cameraX)*sceneScale,(y*.45f-250)*sceneScale);
        private void LateUpdate()
        {
            RecordPlayFrame();
            if(!Ready)return;
            AnimateNavigation();
            EnsureToyViews();AnimateTravelScreen();TickScenery();TickHome();TickKeepy();TickPark();TickZoo();TickDinosaurWorld();TickPond();TickCreekBoats();TickCreekFishing();TickKingdom();TickDaycare();TickSeagulls();TickShore();
            if(shared!=null && shared.Connected)
            {
                var own=shared.VisualPosition(Actor);avatar.anchoredPosition=ToBoard(own.x,own.y);
                foreach(var friend in friends)
                {if(!friend.Value.root.gameObject.activeSelf)continue;var p=shared.VisualPosition(friend.Key);friend.Value.root.anchoredPosition=ToBoard(p.x,p.y);}
                foreach(var t in ReadToys())if(t.id!=dragging && !string.IsNullOrEmpty(t.holder) && shared.TryPreview(t.id,out var p))toys[t.id].anchoredPosition=ToBoard(p.x,p.y);
                SortDepth();
            }
            // Drive animation from the same displayed motion, without changing
            // authoritative coordinates, inventory, gestures or saved avatar IDs.
            var items=ReadToys();
            void Present(string id,GameCharacterVisual visual)
            {
                var player=ReadPlayer(id);
                var point=shared!=null && shared.Connected?shared.VisualPosition(id):new Vector2(player.x,player.y);
                if(player.stairs>0)point=StairPoint(player);
                // Book sound controls apply only to the reader, not the outfit roar.
                visual.Wear(player.outfit,player.outfitColor);visual.ObserveRoar(player.roar,!applicationPaused);
                var storyHold=player.zone==KingdomAdventure.Zone && !string.IsNullOrEmpty(KingdomGame?.members.FirstOrDefault(m=>m.actor==id)?.carrying) || player.zone=="daycare" && DaycareGame?.members.FirstOrDefault(m=>m.actor==id)?.carryingPlate==true;
                visual.PresentHome(point,id+"/"+player.zone+"/"+player.visit,items.Any(t=>t.holder==id) || storyHold,applicationPaused?0:Time.unscaledDeltaTime,player,Home,Keepy);
            }
            PresentRooms();
            PresentBedrooms();PresentBathroom();PresentBedroomFurniture();PresentSecrets();PresentBooks();PresentRoomPlay();PresentKitchen();PresentDiscovery();PresentCollections();PresentCreationEntrances();
            Present(Actor,characterVisual);
            foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)Present(friend.Key,friend.Value.view);
            PresentHideAndSeek();PresentPark();PresentWheels();PresentTag();PresentDinosaurRiders();PresentDinosaurPetters();PresentWaveRide();
        }
        private readonly List<(RectTransform root,float ground,int part,string key)> depthOrder=new List<(RectTransform,float,int,string)>();
        private void SortDepth()
        {
            depthOrder.Clear();
            void Add(RectTransform root,float ground,int part,string key)
            {if(root!=null && root.gameObject.activeSelf)depthOrder.Add((root,ground,part,key));}
            // Occupants and stored objects use their support's ground plane,
            // not their animated height. A closer walker still covers the whole
            // assembly; no fixture user or stored prop is globally topmost.
            void Player(string id,RectTransform root)
            {
                var player=HasWorld?ReadPlayer(id):null;
                var rider=BeachWaveRide.Rider(Shore?.ride,id);
                if(rider!=null && waveSupports[rider.seat]!=null){Add(root,WaveGround(rider.seat),1,"wave-rider-"+rider.seat);return;}
                if(player!=null && player.stairs>0){Add(root,ToBoard(HomeRooms.EntryX(player.zone),HomeRooms.EntryY(player.zone)).y,1,id);return;}
                var hider=HideAndSeek.Player(HideGame,id);
                if(hider?.mode==HiderMode.Hidden){Add(root,ToBoard(player.x,HideAndSeek.GroundY[hider.slot]).y,1,id);return;}
                var fixture=player?.fixture??"";
                if(DinosaurRides.Usable(fixture)){Add(root,DinosaurPlayerGround(fixture),1,fixture);return;}
                if(BathroomLayout.Usable(fixture)){Add(root,ToBoard(0,BathroomLayout.Y).y,1,id);return;}
                if(ParkWheels.Usable(fixture)){Add(root,WheelsGround(fixture),1,id);return;}
                if(ParkPlay.Usable(fixture)){var station=ParkPlay.Station(fixture);var ground=ParkPlayerGround(fixture);Add(root,ground,1,id);return;}
                if(SecretRooms.FortIndex(fixture)>=0 && secretFort!=null){Add(root,secretFort.anchoredPosition.y,1,id);return;}
                if(BedroomFurniture.Seat(fixture) && FurnishedRoom!=null){var key=fixture==BedroomFurniture.Bed?"bed":"cushion-"+BedroomFurniture.CushionIndex(fixture);if(bedroomFurniture.TryGetValue(key,out var furniture)){Add(root,furniture.anchoredPosition.y,1,id);return;}}
                if(Kitchen.Seat(fixture)){Add(root,ToBoard(0,130).y,1,id);return;}
                var home=HomeLayout.Seat(fixture)?"Home sofa":HomeLayout.Bounce(fixture)?"Home trampoline":"";
                if(home!="" && homeObjects.TryGetValue(home,out var support))Add(root,support.root.anchoredPosition.y,1,id);
                else Add(root,root.anchoredPosition.y,3,id);
            }
            AddCreekFishingDepth(Add);AddCreekBoatDepth(Add);AddPondDepth(Add);AddBathroomDepth(Add);AddZooDepth(Add);AddParkDepth(Add);AddWheelsDepth(Add);AddTagDepth(Add);AddDinosaurDepth(Add);AddDinosaurCareDepth(Add);AddHideDepth(Add);AddBedroomDepth(Add);AddSecretDepth(Add);AddBookDepth(Add);AddRoomPlayDepth(Add);AddKitchenDepth(Add);AddDiscoveryDepth(Add);AddKingdomDepth(Add);AddDaycareDepth(Add);AddShoreDepth(Add);AddSeagullDepth(Add);AddWaveRideDepth(Add);
            foreach(var pair in homeObjects)
            {
                var root=pair.Value.root;Add(root,root.anchoredPosition.y,0,pair.Key);
                if(homeFronts.TryGetValue(pair.Key,out var front))Add(front,root.anchoredPosition.y,2,pair.Key);
            }
            var depthToys=ReadToys();roomPlayItems=depthToys;
            foreach(var toy in depthToys)
            {
                if(toy.id==dragging)continue;var rect=toys[toy.id];
                if(KitchenDepth(toy,rect,Add) || RoomToyDepth(toy,rect,Add,depthToys))continue;
                var roomSlot=BedroomFurniture.Slot(toy.zone,toy.container);
                if(HomeBooks.Slot(toy.container)>=0 && bookRack!=null)Add(rect,bookRack.anchoredPosition.y,1,toy.id);
                else if(roomSlot>=0 && FurnishedRoom!=null && bedroomFurniture.TryGetValue(roomSlot<8?"chest":"shelf",out var roomSupport))Add(rect,roomSupport.anchoredPosition.y,1,toy.id);
                else if(HomeLayout.StorageSlot(toy.container)>=0 && homeObjects.TryGetValue("Home shed",out var shed))Add(rect,shed.root.anchoredPosition.y,1,toy.id);
                else Add(rect,rect.anchoredPosition.y,3,toy.id);
            }
            if(stairFront!=null && HomeRooms.Property(CurrentArea))Add(stairFront.rectTransform,ToBoard(HomeRooms.EntryX(CurrentArea),HomeRooms.EntryY(CurrentArea)).y,2,"stair-cover");
            Player(Actor,avatar);
            foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)Player(friend.Key,friend.Value.root);
            if(keepyRoot!=null && Keepy!=null)Add(keepyRoot,ToBoard(Keepy.x,Keepy.y).y,4,"keepy-balloon");
            depthOrder.Sort((a,b)=>{var depth=b.ground.CompareTo(a.ground);if(depth!=0)return depth;if(a.key.StartsWith("dinosaur-") && b.key.StartsWith("dinosaur-")){var assembly=string.CompareOrdinal(a.key,b.key);if(assembly!=0)return assembly;}var part=a.part.CompareTo(b.part);return part!=0?part:string.CompareOrdinal(a.key,b.key);});
            foreach(var entry in depthOrder)entry.root.SetAsLastSibling();
            if(dragging!=null)toys[dragging].SetAsLastSibling();
        }
        private void Render()
        {
            frameToysAt=-1; // An acknowledged edit can render again within this frame.
            if(avatar==null)return;
            EnsureToyViews();
            var zone=CurrentArea;var creek=zone=="creek";
            areaLabel.text=(shared==null?(creek?"Creek":"Garden")+" play lab":Actor.Replace("player-","Player ")+" Â· "+(creek?"Creek":"Garden"))+" / "+Application.version;
            Board.GetComponent<Image>().color=creek?new Color(.7f,.85f,.71f):new Color(.76f,.89f,.72f);
            floorPath.color=creek?new Color(.37f,.72f,.87f):new Color(.9f,.81f,.64f);
            foreach(var part in fence)part.SetActive(false);
            foreach(var pair in travelButtons)pair.Value.interactable=(shared==null || shared.Connected) && (pair.Key!=zone || TravelPending);
            // Network-driven renders can occur before LateUpdate. Enforce closed
            // container visibility here too, so hidden props never re-enter the
            // UI raycast list for the remainder of that frame.
            foreach(var toy in AllToys())toys[toy.id].gameObject.SetActive(VisibleToy(toy));
            var tidyCues=Shared?HomeTidying.CueKeys(shared.View):World.ReadTidyCues();
            var toyStates=ReadToys();roomPlayItems=toyStates;var p=ReadPlayer(Actor);avatar.anchoredPosition=ToBoard(p.x,p.y);
            renderedArea=p.zone;renderedVisit=p.visit;
            characterVisual.Select(p.avatar);
            RenderNavigation();
            foreach(var t in toyStates){if(t.id!=dragging)toys[t.id].anchoredPosition=FurnitureToyPoint(t);
                if(shared!=null)
                {
                    if(t.id!=dragging && !string.IsNullOrEmpty(t.holder) && shared.TryPreview(t.id,out var preview))toys[t.id].anchoredPosition=ToBoard(preview.x,preview.y);
                    var group=toys[t.id].GetComponent<CanvasGroup>();group.alpha=t.id==dragging && !grabConfirmed ? .55f : 1;
                    holders[t.id].text=Cuddling(t,out _)?"":t.id==dragging && !grabConfirmed?"Picking upâ€¦":string.IsNullOrEmpty(t.holder)?"":t.holder==Actor?"Yours":t.holder.Replace("player-","Player ")+" has it";
                }
                if(t.kind==ToyKind.Bucket)fills[t.id].rectTransform.sizeDelta=new Vector2(58,5+13*t.water);
                if(t.kind==ToyKind.Plant)fills[t.id].gameObject.SetActive(t.water==3);
            if(t.kind==ToyKind.Puddle)fills[t.id].rectTransform.localScale=Vector3.one*(t.water/3f);
                if(resetCues.TryGetValue(t.id,out var cue))cue.SetActive(t.resetPending || tidyCues.Contains(HomeTidying.Item(t.id)));
            }
            if(shared!=null)RenderFriends();
            // Larger y is farther back on the illustrated floor plane.
            SortDepth();
            activity.text=p.activity==""?"Free play Â· walk, drag, discover":p.activity=="garden"?(toyStates.First(t=>t.kind==ToyKind.Plant).water==3?"Your flower is happy! Keep exploring.":"Give the flower a drink"):(toyStates.First(t=>t.kind==ToyKind.Puddle).water==0?"All tidy! Keep exploring.":"Soak up the puddle");
            // Reliable replies must keep sea riders attached to their support.
            PresentWaveRide();
        }
        private void DrawToy(SoloToy t)
        {
            var root=Rect(Board,t.id,ToBoard(t.x,t.y),new Vector2(125,115));toys.Add(t.id,root);
            if(shared!=null){root.gameObject.AddComponent<CanvasGroup>();holders[t.id]=Label(root,"",16,new Vector2(0,-91),new Vector2(190,28));}
            var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=SoloWorld.Carryable(t.kind);
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
            DrawTea(t,root);DrawKitchenItem(t,root);
            if(t.kind==ToyKind.Bucket){var handle=Panel(root,"Handle",new Vector2(0,27),new Vector2(65,50),Ink,false,true);handle.sprite=hintRing;Panel(root,"Bucket",Vector2.zero,new Vector2(80,70),new Color(.98f,.67f,.28f));fills[t.id]=Panel(root,"Water",new Vector2(0,3),new Vector2(58,10),new Color(.32f,.68f,.91f));}
            if(t.kind==ToyKind.Sponge){Panel(root,"Sponge",Vector2.zero,new Vector2(92,53),new Color(1,.87f,.39f));for(var i=0;i<4;i++)Panel(root,"Hole",new Vector2(-27+i*18,(i%2)*15-8),new Vector2(8,8),new Color(.78f,.58f,.25f),false,true);}
            if(t.kind==ToyKind.Tap && t.id!="tap-park"){Panel(root,"Tap pipe",new Vector2(-14,5),new Vector2(28,100),new Color(.47f,.61f,.68f));Panel(root,"Spout",new Vector2(14,40),new Vector2(74,26),new Color(.59f,.71f,.76f));Panel(root,"Handle",new Vector2(-14,66),new Vector2(67,18),new Color(.29f,.5f,.61f));Panel(root,"Drop",new Vector2(40,6),new Vector2(20,28),new Color(.29f,.65f,.88f),false,true);}
            if(t.kind==ToyKind.Plant){Panel(root,"Stem",new Vector2(0,23),new Vector2(10,79),new Color(.27f,.51f,.29f));Panel(root,"Leaf",new Vector2(-19,32),new Vector2(40,20),new Color(.38f,.66f,.33f),false,true);Panel(root,"Pot",new Vector2(0,-22),new Vector2(76,54),new Color(.8f,.43f,.3f));fills[t.id]=Panel(root,"Bloom",new Vector2(0,64),new Vector2(68,68),new Color(.96f,.52f,.61f),false,true);Panel(fills[t.id].transform,"Pollen",Vector2.zero,new Vector2(26,26),new Color(1,.84f,.35f),false,true);}
            if(t.kind==ToyKind.Ball){Panel(root,"Ball outline",Vector2.zero,new Vector2(86,86),Ink,false,true);Panel(root,"Ball",Vector2.zero,new Vector2(80,80),new Color(.97f,.66f,.29f),false,true);Panel(root,"Ball stripe",Vector2.zero,new Vector2(24,78),new Color(.35f,.76f,.84f),false,true);}
            if(t.kind==ToyKind.Book){var cover=HomePicture(root,"Picture book",Vector2.zero,new Vector2(130,170),BookCoverSprite(HomeBooks.Index(t.id)));cover.preserveAspect=true;}
            if(t.kind==ToyKind.Plush)HomePicture(root,"Plush toy",Vector2.zero,SecretRooms.PlushIndex(t.id)>=0?new Vector2(100,100):new Vector2(110,74),SecretRooms.PlushIndex(t.id)>=0?SecretSprite("plush-"+SecretRooms.PlushIndex(t.id)):BedroomSprite("plush"));
            if(t.kind==ToyKind.Block){Panel(root,"Soft block edge",Vector2.zero,new Vector2(77,72),Ink);Panel(root,"Soft block",Vector2.zero,new Vector2(69,64),new Color(.78f,.88f,.94f));Panel(root,"Block circle",Vector2.zero,new Vector2(35,35),new Color(.96f,.75f,.39f),false,true);}
            if(t.kind==ToyKind.Puddle)fills[t.id]=Panel(root,"Puddle",Vector2.zero,new Vector2(126,49),new Color(.41f,.73f,.86f),false,true);
            // Scene props use their pictures; instructions live in the optional activity menu.
            if(t.kind!=ToyKind.Tap && t.kind!=ToyKind.Ball && (!BedroomFurniture.Personal(t.kind) || t.kind==ToyKind.Book))
            {
                var cue=Panel(root,"Idle return cue",new Vector2(0,115),new Vector2(160,36),Cream).gameObject;
                // A picture plus plain text; never a ticking challenge/failure timer.
                var picture=Panel(cue.transform,"Return picture",new Vector2(-59,0),new Vector2(26,26),new Color(.87f,.72f,.33f),false,true);
                Panel(picture.transform,"Arrow stem",new Vector2(0,1),new Vector2(15,4),Ink);
                var arrow=Panel(picture.transform,"Arrow tip",new Vector2(-5,4),new Vector2(10,4),Ink);arrow.rectTransform.localRotation=Quaternion.Euler(0,0,45);
                Label(cue.transform,t.kind==ToyKind.Bucket || t.kind==ToyKind.Sponge || t.kind==ToyKind.Book || Kitchen.Kind(t.kind)?"Back soon":"Again soon",16,new Vector2(15,0),new Vector2(120,32));
                cue.SetActive(false);resetCues[t.id]=cue;
            }
        }
        private void DrawAvatar()
        {var visual=CreateAvatar("Player character");avatar=visual.root;characterVisual=visual.view;}
        private (RectTransform root,GameCharacterVisual view) CreateAvatar(string name)
        {
            var root=Rect(Board,name,Vector2.zero,new Vector2(85,136));
            return(root,root.gameObject.AddComponent<GameCharacterVisual>());
        }
        private void RenderFriends()
        {
            foreach(var player in shared.View.players.Where(p=>p.id!=Actor))
            {
                if(!friends.TryGetValue(player.id,out var visual))
                {
                    visual=CreateAvatar("Friend-"+player.id);friends.Add(player.id,visual);
                    // Personal profile identifiers are not labels in the play scene.
                }
                visual.root.gameObject.SetActive(shared.Players.Contains(player.id) && SoloWorld.AreaOf(player.zone)==CurrentArea);
                visual.root.anchoredPosition=ToBoard(player.x,player.y);
                visual.view.Select(player.avatar);
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
        private Sprite Shape(bool oval,bool rim=false)
        {
            var size=oval?256:64;var center=(size-1)*.5f;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            for(var y=0;y<size;y++)for(var x=0;x<size;x++){var dx=Mathf.Abs(x-center);var dy=Mathf.Abs(y-center);var d=oval?Mathf.Sqrt(dx*dx+dy*dy)-(size/2f-1):Mathf.Sqrt(Mathf.Pow(Mathf.Max(0,dx-18),2)+Mathf.Pow(Mathf.Max(0,dy-18),2))-13;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(1-d)*(rim?Mathf.Clamp01(d+7):1)));}
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,oval?Vector4.zero:new Vector4(16,16,16,16));
        }
        private Sprite Ring()
        {
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            for(var y=0;y<96;y++)for(var x=0;x<96;x++)
            {var distance=Vector2.Distance(new Vector2(x,y),new Vector2(47.5f,47.5f));texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(47-distance)*Mathf.Clamp01(distance-41)));}
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,96,96),new Vector2(.5f,.5f));
        }
        private void ApplyResponsiveLayout()
        {
            if(Board==null)return;
            Canvas.ForceUpdateCanvases();
            LayoutWorldViewport();
            stick.anchoredPosition=new Vector2(-safe.rect.width/2+102,-safe.rect.height/2+110);
            ((RectTransform)menu.transform).sizeDelta=safe.rect.size;
            LayoutNavigation();
            if(avatar!=null)Render();
        }
        private void UpdateSafeArea()
        {
            lastSafeArea=Screen.safeArea;safe.anchorMin=new Vector2(lastSafeArea.xMin/Screen.width,lastSafeArea.yMin/Screen.height);
            safe.anchorMax=new Vector2(lastSafeArea.xMax/Screen.width,lastSafeArea.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
            ApplyResponsiveLayout();
        }
    }
}
