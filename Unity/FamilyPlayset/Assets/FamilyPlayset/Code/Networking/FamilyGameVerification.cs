using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LittleWeeps.Client;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LittleWeeps.NetworkProbe
{
    // Only installed for explicit isolated verification configs. Inputs travel
    // through Input System + UGUI and the same asynchronous presentation path.
    [DefaultExecutionOrder(1000)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false,"LittleWeeps.NetworkProbe",null,"NetworkGardenVerification")]
    public sealed class FamilyGameVerification : MonoBehaviour
    {
        private FamilyNetworkBootstrap probe;
        private GameScreen screen;
        private Mouse mouse;
        private Touchscreen touch;
        private InputDevice[] hostPointers;
        private int serial;
        private bool working,down;
        private bool evidencePending;
        private string failure;
        private string[] lastTouchTargets;
        private Vector2 mousePoint;
        private string traceActor;
        private double treasureCaptureAt;private int treasureCapture;private bool traceTreasure;private readonly List<TreasureMotionSample> treasureTrace=new List<TreasureMotionSample>();
        [Serializable] private sealed class TreasureMotionSample {public double time;public float dt;public int frame;public Vector2[] points;public string[] poses;public float[] speeds;public int[] drawings;public float[] phases;public Vector2 teacherPoint;public int teacherDrawing;public bool teacherWalking;}
        [Serializable] private sealed class TreasureMotionEvidence {public string build;public TreasureMotionSample[] samples;}
        private readonly List<MotionSample> motionTrace=new List<MotionSample>();
        private string boatTraceActor;private readonly List<BoatSample> boatTrace=new List<BoatSample>();
        [Serializable] private sealed class BoatSample {public double time,clock,authority;public Vector2 position;public int phase;}
        [Serializable] private sealed class BoatEvidence {public string actor,build;public BoatSample[] samples;}
        private bool tracePond;private readonly List<PondSample> pondTrace=new List<PondSample>();
        [Serializable] private sealed class PondSample {public double time,clock,authority;public Vector2[] positions;public double[] routes,durations;public bool[] visible;}
        [Serializable] private sealed class PondEvidence {public string build;public PondSample[] samples;}
        [Serializable] private sealed class MotionSample
        {public double time;public Vector2 visual,authority,ground;public float phase,weight;public int drawing;public bool faceLeft;public string pose;}
        [Serializable] private sealed class MotionEvidence {public string actor,build;public MotionSample[] samples;}
        [Serializable] private sealed class Step {public int serial;public string action,role,text;public float x,y;public int finger=11;}
        [Serializable] private sealed class PlayerView {public string id,outfit;public Vector2 position;public bool visible,roarPlaying;}
        [Serializable] private sealed class ToyView {public string id,label;public Vector2 position;public float alpha;}
        [Serializable] private sealed class ControlView {public string name;public Rect bounds;public bool enabled;}
        [Serializable] private sealed class Evidence
        {
            public LittleWeeps.Core.SeagullState seagulls;public int visibleSeagulls,visibleGullTracks;
            public LittleWeeps.Core.BeachShoreState shore;public int visibleSandPrints,visibleWaveRiders;public bool visibleSeaVisitor;public int seaVisitorPose=-1;
            public int serial,visiblePlayers,canvases,narrators,audioSources;public bool passed,ready,pending,connected,menuOpen,shared;
            public bool tagNpcVisible;public bool tagSpeaking;public string tagCue;
            public string error,build,actor,feedback,dragging,zone,savePath,adventure,pendingRequest;public int pendingArchives;public PlayerView[] players;public ToyView[] toys;
            public int screenWidth,screenHeight;public Rect safeArea,boardBounds;public float boardLayoutWidth;public bool controlsInSafeArea;
            public bool worldsOpen,charactersOpen,joystickVisible,fullCharactersInTray,activeCharacterVisible;public string character;public int characterLayers;public ControlView[] controls;
            public bool outfitsOpen,roarPlaying;public string outfit,outfitColor;
            public bool worldLoading;public string loadingDestination,loadingFailure;public string[] travelStages;
            public float homePoseAge;public string homePose;public bool homeMusicPlaying,musicMuted;public string worldMusicTrack;public bool worldMusicPlaying;public float worldMusicVolume,worldMusicSignal;public int worldMusicClipCount,worldMusicSample;public LittleWeeps.Core.HomeState home;
            public bool secretDoorVisible,secretDoorInteractive,quietStill;public int quietBrightness,quietMusicLevel,quietEffectsLevel;public float quietPhase;
            public LittleWeeps.Core.DinosaurWorldState dinosaurWorld;public int dinosaurTextures;public bool dinosaurSoundPlaying;public Vector2 dinosaurSeat;
            public LittleWeeps.Core.ZooState zoo;public int visibleZooAnimals,zooTextures,zooAudioClips;public bool zooSoundPlaying;
            public int surpriseSounds,surpriseObjects;public Vector2[] elephantBrushPositions;public Color[] zooRouteColors;public int elephantCareFinishEvents;public string elephantCue;public int elephantSlot,elephantFinishEvents,elephantWaterEvents,elephantEffectObjects;
            public long allocatedMemory,managedMemory;
            public bool zooMapOpen,zooNavigationBusy;public string zooCurrentExhibit,zooPreviousDestination,zooNextDestination,zooMapShownTrail,zooMapMarker;
            public LittleWeeps.Core.CreekBoatState creekBoats;public bool boatWorkshopOpen,ownCreekBoatInView;public int visibleCreekBoats;
            public LittleWeeps.Core.PondState creekFishing;public bool creekFishingCloseup,creekFishingWaterPlaying;
            public LittleWeeps.Core.PondState pond;public bool pondCloseup,pondWaterPlaying;
            public string[] revealActors;public int[] revealVariants;public int homeRevealEvents,daycareRevealEvents;public GameScreen.RevealBody[] revealBodies;
            public LittleWeeps.Core.DaycarePlayState hideClub,tagClub;public string[] clubNpcArt;public Vector2[] clubNpcPoints;public int[] clubNpcFrames;public bool clubTeacherWalking;public int clubTeacherDrawing;public Vector2 clubTeacherPoint;
            public LittleWeeps.Core.VetState vet;public int vetSelected,vetTool,vetDrawing,vetQueued;public bool vetWalking,vetSoundPlaying,vetBackdropReady;public string[] vetNpcArt;public string vetFeedback;
            public LittleWeeps.Core.TreasureState treasure;public string[] treasureNpcArt;public Vector2[] treasureNpcPoints;public string treasureApproach;public bool treasureMystery;public int treasureDemoNote;public bool treasureTonePlaying;public string treasureFeedback;
            public bool sandLocalPending,sandDirectCueVisible,sandPlacementActive;public string sandDecorationChoice;public Vector2[] sandAttachmentScreenPoints;public string[] sandBuilderActors,sandBuilderReactions;public int[] sandBuilderEvents;public Vector2[] sandPieceScreenPoints;public LittleWeeps.Core.SandpitState sandpit; public string[] sandpitNpcArt;public bool sandIllustrated,sandPhoneLayout; public float[] sandVoiceVolumes;public string[] sandVoiceClips;public int sandPresentationEvents,sandAudioEvents,sandActiveVoices,sandActiveEffects; public int sandpitSelection,sandpitHighlight,sandpitApproach,sandpitApproachRound;public int[] sandpitScoopEvents,sandpitWaterEvents;public float[] sandpitWaterEffects;public bool[] sandpitWetVisuals,sandpitBuiltVisuals;public int[] sandpitTipEvents;public float[] sandpitTipEffects,sandpitTowerReveal;public string[] sandpitTipOutcomes;public int[] sandpitDecorationEvents,sandpitDecorationVisuals,sandpitDecorationEffectValues;public float[] sandpitDecorationEffects,sandpitDecorationWiggles;public bool[] sandpitDecorationHidden;
            public LittleWeeps.Core.DaycareState daycare;public int daycareRoutine,calypsoPose,calypsoDrawing;public bool calypsoVisible,calypsoMoving,calypsoWalkPlaying;public Vector2 calypsoWorldPoint;
            public LittleWeeps.Core.KingdomState kingdom;public int visibleKingdomNpcs;public string kingdomApproach;public string[] lastTouchTargets,kingdomNpcArt,picnicNpcArt,kingdomNpcJobs,kingdomNpcPoses;public Vector2[] kingdomNpcPoints;
            public LittleWeeps.Core.HideState hideAndSeek;public LittleWeeps.Core.KeepyState keepy;public Vector2 balloonPoint;
            public bool sceneryReady;public string place;public float cameraX;public int pendingScenery;public string[] residentScenery;public string[] homeDrawOrder;
            public bool bookAuto,bookWords,bookOptions,bookEffect,bookEffectPending,bookNaming;public string bookTitle;public int bookTextures,bookAudio;
            public bool discoveryOpen;public bool bookOpen,bookReady,bookPlaying,bookSpeaking;public int bookPage,bookSample;public string[] visibleText;
        }
        private void OnEnable()=>Application.logMessageReceived+=Log;
        private void OnDisable()=>Application.logMessageReceived-=Log;
        private void Log(string text,string stack,LogType kind){if(kind==LogType.Error || kind==LogType.Exception)failure=text;}
        private void Start()
        {
            probe=GetComponent<FamilyNetworkBootstrap>();screen=GetComponent<GameScreen>();
            hostPointers=InputSystem.devices.Where(d=>(d is Mouse || d is Pen) && d.enabled).ToArray();
            foreach(var device in hostPointers)InputSystem.DisableDevice(device);
            InputSystem.RegisterLayout("{\"name\":\"SharedTestMouse\",\"extend\":\"Mouse\",\"runInBackground\":\"enabled\"}");
            InputSystem.RegisterLayout("{\"name\":\"SharedTestTouch\",\"extend\":\"Touchscreen\",\"runInBackground\":\"enabled\"}");
            mouse=(Mouse)InputSystem.AddDevice("SharedTestMouse");touch=(Touchscreen)InputSystem.AddDevice("SharedTestTouch");
        }
        private void Update()
        {
            if(working || probe==null)return;
            if(evidencePending){Write();return;}
            var path=Path.Combine(probe.Output,"garden-control.json");if(!File.Exists(path))return;
            try
            {
                using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
                using var reader=new StreamReader(file);
                var step=JsonUtility.FromJson<Step>(reader.ReadToEnd());
                if(step==null || step.serial<=serial)return;serial=step.serial;Run(step);
            }
            catch(IOException){/* An atomically replaced test control may be briefly busy. */}
            catch(Exception e){failure=e.ToString();Write();}
        }
        private Vector2 Point(Step step)
        {
            if(step.role=="vet")return screen.VetScreenPoint(step.x,step.y);
            if(step.role=="discovery")return screen.DiscoveryScreenPoint(step.x,step.y);
            if(step.role=="colors")return screen.LiquidScreenPoint(step.x,step.y);
            if(step.role=="ramps")return screen.RampScreenPoint(step.x,step.y);
            if(step.role=="bubbles")return screen.BubbleScreenPoint(step.x,step.y);
            if(step.role=="ice")return screen.IceScreenPoint(step.x,step.y);
            if(step.role=="mixing")return screen.MixingScreenPoint(step.x,step.y);
            if(step.role=="screen")return new Vector2(step.x,step.y);
            if(string.IsNullOrEmpty(step.role))return screen.ScreenPoint(step.x,step.y);
            if(step.role.StartsWith("ui:"))
            {
                var button=FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==step.role.Substring(3) && b.gameObject.activeInHierarchy);
                return RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(new Vector3(step.x,step.y,0)));
            }
            var rect=screen.Surfaces[step.role].GetComponent<RectTransform>();
            return RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(step.x,step.y,0)));
        }
        private async void Run(Step step)
        {
            working=true;
            try
            {
                if(!screen.Ready && step.action!="inspect")throw new InvalidOperationException("Garden is not ready.");
                if(step.action=="performance")await screen.ExportPlayPerformance();
                else if(step.action=="treasureMotionStart"){treasureTrace.Clear();treasureCaptureAt=0;treasureCapture=0;Directory.CreateDirectory(Path.Combine(probe.Output,"npc-preview"));traceTreasure=true;}
                else if(step.action=="treasureMotionStop"){traceTreasure=false;File.WriteAllText(Path.Combine(probe.Output,"treasure-motion.json"),JsonUtility.ToJson(new TreasureMotionEvidence{build=Application.version,samples=treasureTrace.ToArray()},true));}
                else if(step.action=="frameRate")
                {
                    if(step.x!=30 && step.x!=60)throw new InvalidOperationException("Unsupported test frame rate.");
                    QualitySettings.vSyncCount=0;Application.targetFrameRate=(int)step.x;
                }
                else if(step.action=="resize")
                {
                    if(step.x<640 || step.x>2200 || step.y<400 || step.y>1400)throw new InvalidOperationException("Invalid test resolution.");
                    Screen.SetResolution((int)step.x,(int)step.y,FullScreenMode.Windowed);await Task.Delay(600);Canvas.ForceUpdateCanvases();
                }
                else if(step.action=="press" || step.action=="move" || step.action=="release")
                {
                    mousePoint=Point(step);if(step.action=="press")down=true;if(step.action=="release")down=false;
                    InputSystem.QueueStateEvent(mouse,new MouseState{position=mousePoint}.WithButton(MouseButton.Left,down));
                }
                else if(step.action.StartsWith("touch-"))
                {
                    var phase=step.action=="touch-begin"?UnityEngine.InputSystem.TouchPhase.Began:step.action=="touch-move"?UnityEngine.InputSystem.TouchPhase.Moved:step.action=="touch-end"?UnityEngine.InputSystem.TouchPhase.Ended:UnityEngine.InputSystem.TouchPhase.Canceled;
                    InputSystem.QueueStateEvent(touch,new TouchState{touchId=step.finger,phase=phase,position=Point(step),pressure=phase==UnityEngine.InputSystem.TouchPhase.Ended || phase==UnityEngine.InputSystem.TouchPhase.Canceled?0:1});
                }
                else if(step.action=="button" || step.action=="touchButton")
                {
                    var button=FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.gameObject.name==step.text && b.gameObject.activeInHierarchy);
                    var point=RectTransformUtility.WorldToScreenPoint(null,button.GetComponent<RectTransform>().position);
                    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);lastTouchTargets=hits.Take(5).Select(h=>h.gameObject.name).ToArray();
                    if(step.action=="touchButton")
                    {
                        InputSystem.QueueStateEvent(touch,new TouchState{touchId=99,phase=UnityEngine.InputSystem.TouchPhase.Began,position=point,pressure=1});await Task.Delay(80);
                        InputSystem.QueueStateEvent(touch,new TouchState{touchId=99,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=point});
                    }
                    else
                    {
                        InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left,true));await Task.Delay(80);
                        InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left,false));
                    }
                }
                else if(step.action=="boatTraceStart"){boatTraceActor=step.role;boatTrace.Clear();}
                else if(step.action=="pondTraceStart"){tracePond=true;pondTrace.Clear();}
                else if(step.action=="pondTraceStop"){File.WriteAllText(Path.Combine(probe.Output,"pond-trace.json"),JsonUtility.ToJson(new PondEvidence{build=Application.version,samples=pondTrace.ToArray()},true));tracePond=false;}
                else if(step.action=="boatTraceStop"){File.WriteAllText(Path.Combine(probe.Output,"boat-trace.json"),JsonUtility.ToJson(new BoatEvidence{actor=boatTraceActor,build=Application.version,samples=boatTrace.ToArray()},true));boatTraceActor=null;}
                else if(step.action=="traceStart"){traceActor=step.role;motionTrace.Clear();}
                else if(step.action=="walkChecks")WalkAnimationVerification.Run(screen.Board,probe.Output);
                else if(step.action=="walkFilm")StartCoroutine(WalkFilm());
                else if(step.action=="revealGallery")StartCoroutine(RevealGallery());
                else if(step.action=="revealOutfits")StartCoroutine(RevealOutfits());
                else if(step.action=="sandFilm")StartCoroutine(SandFilm(Mathf.Clamp(step.x,5,90)));
                else if(step.action=="fixtureTravel")screen.Travel(step.text);
                else if(step.action=="traceStop")
                {
                    File.WriteAllText(Path.Combine(probe.Output,"motion-trace.json"),JsonUtility.ToJson(new MotionEvidence{actor=traceActor,build=Application.version,samples=motionTrace.ToArray()},true));traceActor=null;
                }
                // Exercise Unity lifecycle callbacks in isolated verification
                // players. Network-only suspension does not suspend local books.
                else if(step.action=="application-pause")screen.SendMessage("OnApplicationPause",true);
                else if(step.action=="application-resume")screen.SendMessage("OnApplicationPause",false);
                else if(step.action=="network-pause")probe.VerifyFamilyForeground(false);
                else if(step.action=="network-resume")probe.VerifyFamilyForeground(true);
                else if(step.action=="drop-acks")GetComponent<NetworkWorldSession>().VerifyDropAcknowledgments(true);
                else if(step.action=="restore-acks")GetComponent<NetworkWorldSession>().VerifyDropAcknowledgments(false);
                else if(step.action=="capture")StartCoroutine(Capture());
                else if(step.action=="escape")
                {
                    InputSystem.RegisterLayout("{\"name\":\"SharedTestKeyboard\",\"extend\":\"Keyboard\",\"runInBackground\":\"enabled\"}");
                    var keyboard=(Keyboard)InputSystem.AddDevice("SharedTestKeyboard");
                    try{InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));await Task.Delay(100);InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(100);}
                    finally{InputSystem.RemoveDevice(keyboard);}
                }
                else if(step.action!="inspect")throw new ArgumentException("Unknown garden input action.");
                await Task.Delay(step.action=="inspect"?20:65);Write();
            }
            catch(Exception e){failure=e.ToString();Write();}
            finally{working=false;}
        }
        private IEnumerator Capture()
        {
            // Hidden Windows players do not have a capturable swap-chain image.
            // Render the actual live canvas to an offscreen target in this player.
            yield return null;
            CaptureFrame(Path.Combine(probe.Output,"garden.png"));
        }
        private IEnumerator RevealOutfits()
        {
            var canvas=screen.Board.GetComponentInParent<Canvas>();var bounds=((RectTransform)canvas.transform).rect;
            var scales=new[]{.65f,.95f,1f};
            for(var page=0;page<scales.Length;page++){
                var stage=new GameObject("Equipped outfit verification",typeof(RectTransform),typeof(Image));var panel=stage.GetComponent<RectTransform>();panel.SetParent(canvas.transform,false);panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;panel.offsetMin=panel.offsetMax=Vector2.zero;stage.GetComponent<Image>().color=new Color(.76f,.86f,.9f);
                for(var i=0;i<12;i++){
                    var body=new GameObject("Equipped dinosaur pose",typeof(RectTransform));var rect=body.GetComponent<RectTransform>();rect.SetParent(panel,false);rect.anchoredPosition=new Vector2((i%4-1.5f)*bounds.width/4,(1-i/4)*bounds.height/3-bounds.height/12+45);rect.localScale=Vector3.one*scales[page]*screen.Board.rect.height/Core.WorldLayout.SceneHeight;
                    var visual=body.AddComponent<GameCharacterVisual>();visual.Select(i<4?"blue-pup":i<8?"orange-pup":"blue-pup");if(i<8)visual.Wear("dinosaur",i<4?"green":"blue");
                    visual.PresentFrame(new CharacterFrame(i%4<2?CharacterPose.Surprise:CharacterPose.Wave,0,i%2==1),0);
                    var label=new GameObject("Pose label",typeof(RectTransform),typeof(Text));var text=label.GetComponent<Text>();text.rectTransform.SetParent(panel,false);text.rectTransform.anchoredPosition=rect.anchoredPosition+new Vector2(0,-65);text.rectTransform.sizeDelta=new Vector2(260,40);text.text=(i<4?"Bluey dinosaur":i<8?"Bingo blue dinosaur":"Bluey normal")+" / "+(i%4<2?"surprise":"happy")+" / "+(i%2==1?"left":"right");text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=16;text.color=Color.black;text.alignment=TextAnchor.MiddleCenter;
                }
                yield return null;Canvas.ForceUpdateCanvases();CaptureFrame(Path.Combine(probe.Output,"reveal-outfits-"+page+".png"));stage.SetActive(false);Destroy(stage);
            }
        }
        private IEnumerator RevealGallery()
        {
            var canvas=screen.Board.GetComponentInParent<Canvas>();var bounds=((RectTransform)canvas.transform).rect;
            for(var page=0;page<4;page++){
                var stage=new GameObject("Reveal expression verification",typeof(RectTransform),typeof(Image));var panel=stage.GetComponent<RectTransform>();panel.SetParent(canvas.transform,false);panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;panel.offsetMin=panel.offsetMax=Vector2.zero;stage.GetComponent<Image>().color=new Color(.76f,.86f,.9f);
                for(var i=0;i<12 && page*12+i<Core.PlayableCharacters.All.Count;i++){
                    var entry=Core.PlayableCharacters.All[page*12+i];var body=new GameObject(entry.Name,typeof(RectTransform));var rect=body.GetComponent<RectTransform>();rect.SetParent(panel,false);rect.anchoredPosition=new Vector2((i%4-1.5f)*bounds.width/4,(1-i/4)*bounds.height/3-bounds.height/8);rect.localScale=Vector3.one*Mathf.Min(bounds.width/850,bounds.height/650)*.85f;
                    var visual=body.AddComponent<GameCharacterVisual>();visual.Select(entry.AvatarId);visual.PresentFrame(new CharacterFrame(CharacterPose.Surprise,0,false),0);
                    var name=new GameObject("Name",typeof(RectTransform),typeof(Text));var text=name.GetComponent<Text>();text.rectTransform.SetParent(rect,false);text.rectTransform.anchoredPosition=new Vector2(0,-50);text.rectTransform.sizeDelta=new Vector2(200,40);text.text=entry.Name;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=20;text.color=Color.black;text.alignment=TextAnchor.MiddleCenter;
                }
                yield return null;Canvas.ForceUpdateCanvases();CaptureFrame(Path.Combine(probe.Output,"reveal-gallery-"+page+".png"));stage.SetActive(false);Destroy(stage);
            }
        }
        private bool sandFilming;
        private IEnumerator SandFilm(float seconds)
        {
            if(sandFilming)yield break;sandFilming=true;
            var folder=Path.Combine(probe.Output,"sand-film");Directory.CreateDirectory(folder);
            var times=new List<string>();var start=Time.realtimeSinceStartupAsDouble;var frame=0;
            try {
                while(Time.realtimeSinceStartupAsDouble-start<seconds && frame<720){
                    yield return null;
                    CaptureFrame(Path.Combine(folder,(frame++).ToString("D4")+".png"));
                    times.Add((Time.realtimeSinceStartupAsDouble-start).ToString("F6",System.Globalization.CultureInfo.InvariantCulture));
                    yield return new WaitForSecondsRealtime(.125f);
                }
            } finally {File.WriteAllLines(Path.Combine(folder,"times.txt"),times);sandFilming=false;}
        }
        private IEnumerator WalkFilm()
        {
            var folder=Path.Combine(probe.Output,"walk-film");Directory.CreateDirectory(folder);
            var stage=new GameObject("Walk review stage",typeof(RectTransform),typeof(Image));
            var rect=stage.GetComponent<RectTransform>();rect.SetParent(screen.Board.parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            stage.GetComponent<Image>().color=new Color(.83f,.94f,.89f);
            var cast=new List<GameCharacterVisual>();
            for(var i=0;i<2;i++)
            {
                var go=new GameObject("Review character",typeof(RectTransform));go.transform.SetParent(rect,false);
                go.transform.localPosition=new Vector3(i==0?-240:240,-80,0);go.transform.localScale=Vector3.one*2.3f;
                var visual=go.AddComponent<GameCharacterVisual>();visual.Select(i==0?"blue-pup":"orange-pup");cast.Add(visual);
            }
            try
            {
                const float dt=1f/60;
                // A fixed-time complete sequence, not snapshots labelled as a
                // real-time movie. Review both silhouettes at the same cadence.
                for(var f=0;f<180;f++)
                {
                    var moving=f>=15 && f<155;var left=f>=90;
                    foreach(var visual in cast)
                    {
                        visual.PresentFrame(new CharacterFrame(moving?CharacterPose.Walk:CharacterPose.Idle,moving?Core.Walking.Speed:0,left,
                            travel:moving?new Vector2((left?-1:1)*Core.Walking.Speed*dt,0):Vector2.zero),dt);
                    }
                    Canvas.ForceUpdateCanvases();yield return null;
                    CaptureFrame(Path.Combine(folder,f.ToString("D3")+".png"));
                }
                File.WriteAllText(Path.Combine(folder,"complete.json"),"{\"fps\":60,\"frames\":180,\"fixedTimePreview\":true}");
            }
            finally{stage.SetActive(false);Destroy(stage);}
        }
        private void CaptureFrame(string path)
        {
            var canvas=screen.Board.GetComponentInParent<Canvas>();
            var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;var oldDistance=canvas.planeDistance;
            var go=new GameObject("Verification capture",typeof(Camera));var camera=go.GetComponent<Camera>();
            camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=FindObjectsByType<Camera>(FindObjectsSortMode.None).First(c=>c!=camera).backgroundColor;
            camera.orthographic=true;camera.orthographicSize=Screen.height/2f;camera.transform.position=new Vector3(0,0,-10);
            var target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
            var previous=RenderTexture.active;Texture2D image=null;
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;RenderTexture.active=previous;
                camera.targetTexture=null;target.Release();Destroy(target);Destroy(go);if(image!=null)Destroy(image);Canvas.ForceUpdateCanvases();
            }
        }
        private void LateUpdate()
        {
            if(tracePond && screen.Ready && screen.PondGame!=null && pondTrace.Count<3600){
                var pond=screen.Board.Find("Backyard fish pond");
                if(pond!=null && pond.gameObject.activeInHierarchy){
                    var fish=screen.PondGame.fish;var drawings=fish.Select(f=>pond.Find("Pond fish "+f.id) as RectTransform).ToArray();
                    pondTrace.Add(new PondSample{time=screen.PondVisualTime,clock=screen.PondVisualClock,authority=screen.PondGame.clock,
                        positions=drawings.Select(d=>d.anchoredPosition).ToArray(),routes=fish.Select(f=>f.started).ToArray(),durations=fish.Select(f=>f.duration).ToArray(),visible=drawings.Select(d=>d.gameObject.activeInHierarchy).ToArray()});
                }
            }
            if(traceTreasure && screen.Ready && treasureTrace.Count<900)
            {
                // Use the frame-start clock: wall time inside LateUpdate also
                // measures scheduling delays unrelated to this frame's travel.
                treasureTrace.Add(new TreasureMotionSample{time=Time.unscaledTimeAsDouble,dt=Time.unscaledDeltaTime,frame=Time.frameCount,points=screen.TreasureNpcPoints,poses=screen.TreasureNpcPoses,speeds=screen.TreasureNpcSpeeds,drawings=screen.TreasureNpcFrames,phases=screen.TreasureNpcPhases,teacherPoint=screen.NpcTeacherPoint,teacherDrawing=screen.NpcTeacherDrawing,teacherWalking=screen.NpcTeacherWalking});
                if(Time.unscaledTimeAsDouble>=treasureCaptureAt)
                {
                    treasureCaptureAt=Time.unscaledTimeAsDouble+.1;
                    ScreenCapture.CaptureScreenshot(Path.Combine(probe.Output,"npc-preview",(treasureCapture++).ToString("D4")+".png"));
                }
            }
            if(boatTraceActor!=null && screen.Ready && boatTrace.Count<1800){
                var boat=screen.CreekBoatGame?.boats.FirstOrDefault(b=>b.actor==boatTraceActor);var drawing=screen.Board.Find("Creek boat "+boatTraceActor) as RectTransform;
                if(boat!=null && drawing!=null && drawing.gameObject.activeInHierarchy)boatTrace.Add(new BoatSample{time=screen.CreekBoatVisualTime,clock=screen.CreekBoatVisualClock,authority=screen.CreekBoatGame.clock,position=BoardPosition(drawing),phase=(int)boat.phase});
            }
            if(traceActor==null || !screen.Ready || motionTrace.Count>=1800)return;
            var p=screen.ReadPlayer(traceActor);
            var rect=screen.Board.Find(traceActor==screen.Actor?"Player character":"Friend-"+traceActor) as RectTransform;
            if(rect!=null && rect.gameObject.activeSelf)
            {
                var character=rect.GetComponentInChildren<CharacterSheetView>();
                motionTrace.Add(new MotionSample{time=Time.realtimeSinceStartupAsDouble,visual=BoardPosition(rect),authority=new Vector2(p.x,p.y),
                    ground=screen.WorldPoint(RectTransformUtility.WorldToScreenPoint(null,character.Ground)),drawing=character.FrameIndex,
                    phase=character.WalkPhase,weight=character.WalkWeight,faceLeft=character.Frame.FaceLeft,pose=character.Frame.Pose.ToString()});
            }
        }
        private Vector2 BoardPosition(RectTransform rect)
        {return screen.WorldPoint(RectTransformUtility.WorldToScreenPoint(null,rect.position));}
        private Rect Bounds(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        private void Write()
        {
            var evidence=new Evidence{serial=serial,passed=string.IsNullOrEmpty(failure),error=failure??"",build=Application.version,actor=probe.Settings.profile,ready=screen.Ready,connected=probe.ConnectedToServer,
                pending=screen.ActionPending,feedback=screen.Feedback,dragging=screen.Dragging??"",zone=screen.CurrentArea,visiblePlayers=screen.Ready?screen.VisiblePlayers:0,menuOpen=screen.MenuOpen,
                shared=screen.Shared,adventure=screen.AdventureId,savePath=screen.SavePath??"",pendingRequest=GetComponent<NetworkWorldSession>()?.PendingRequestId??"",pendingArchives=probe.PendingInterruptedArchives,canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length,
                narrators=FindObjectsByType<PlayerNarration>(FindObjectsSortMode.None).Length,audioSources=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length};
            if(screen.Ready)
            {
                evidence.screenWidth=Screen.width;evidence.screenHeight=Screen.height;evidence.safeArea=Screen.safeArea;
                evidence.worldsOpen=screen.WorldsOpen;evidence.charactersOpen=screen.CharactersOpen;evidence.character=screen.DisplayedCharacterId;evidence.characterLayers=screen.DisplayedCharacterLayers;
                evidence.joystickVisible=screen.Surfaces["stick"].gameObject.activeInHierarchy;
                evidence.worldLoading=screen.WorldLoading;evidence.loadingDestination=screen.LoadingDestination;evidence.loadingFailure=screen.LoadingFailure;evidence.travelStages=screen.TravelStages;
                evidence.homePoseAge=screen.HomePoseAge;evidence.homePose=screen.HomePose;evidence.homeMusicPlaying=screen.HomeMusicPlaying;evidence.musicMuted=screen.MusicMuted;evidence.home=screen.Home;
                evidence.worldMusicTrack=screen.WorldMusicTrack;evidence.worldMusicPlaying=screen.WorldMusicPlaying;evidence.worldMusicVolume=screen.WorldMusicVolume;evidence.worldMusicSignal=screen.WorldMusicSignal;evidence.worldMusicClipCount=screen.WorldMusicClipCount;evidence.worldMusicSample=screen.WorldMusicSample;
                evidence.secretDoorVisible=screen.SecretDoorVisible;evidence.secretDoorInteractive=screen.SecretDoorInteractive;evidence.quietStill=screen.QuietStill;evidence.quietBrightness=screen.QuietBrightness;evidence.quietMusicLevel=screen.QuietMusicLevel;evidence.quietEffectsLevel=screen.QuietEffectsLevel;evidence.quietPhase=screen.QuietPhase;
                evidence.sceneryReady=screen.SceneryReady;evidence.place=screen.CurrentPlace;evidence.cameraX=screen.CameraX;evidence.pendingScenery=screen.PendingScenery;evidence.residentScenery=screen.ResidentScenery;
                evidence.keepy=screen.Keepy;evidence.balloonPoint=screen.KeepyBalloonPoint;
                evidence.dinosaurWorld=screen.DinosaurGame;evidence.dinosaurTextures=screen.DinosaurTextureCount;evidence.dinosaurSoundPlaying=screen.DinosaurSoundPlaying;evidence.dinosaurSeat=screen.DinosaurSeat;
                evidence.zoo=screen.ZooGame;evidence.visibleZooAnimals=screen.VisibleZooAnimals;evidence.zooTextures=screen.ZooTextureCount;evidence.zooAudioClips=screen.ZooAudioClipCount;evidence.zooSoundPlaying=screen.ZooSoundPlaying;
                evidence.zooMapOpen=screen.ZooMapOpen;evidence.zooNavigationBusy=screen.ZooNavigationBusy;evidence.zooCurrentExhibit=screen.ZooCurrentExhibit;evidence.zooPreviousDestination=screen.ZooPreviousDestination;evidence.zooNextDestination=screen.ZooNextDestination;evidence.zooMapShownTrail=screen.ZooMapShownTrail;
                evidence.zooMapMarker=screen.ZooMapMarker;
                evidence.elephantBrushPositions=screen.ElephantBrushPositions;evidence.zooRouteColors=screen.ZooRouteColors;evidence.elephantCareFinishEvents=screen.ElephantCareFinishEvents;evidence.elephantCue=screen.ElephantCue;evidence.elephantSlot=screen.ElephantSlot;evidence.elephantFinishEvents=screen.ElephantFinishEvents;
                evidence.surpriseSounds=screen.SurpriseSounds;evidence.surpriseObjects=screen.SurpriseObjects;evidence.elephantWaterEvents=screen.ElephantWaterEvents;evidence.elephantEffectObjects=screen.ElephantEffectObjects;
                evidence.allocatedMemory=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();evidence.managedMemory=GC.GetTotalMemory(false);
                evidence.creekBoats=screen.CreekBoatGame;evidence.boatWorkshopOpen=screen.BoatWorkshopOpen;evidence.ownCreekBoatInView=screen.OwnCreekBoatInView;evidence.visibleCreekBoats=screen.VisibleCreekBoats;
                evidence.creekFishing=screen.CreekFishingGame;evidence.creekFishingCloseup=screen.CreekFishingCloseup;evidence.creekFishingWaterPlaying=screen.CreekFishingWaterPlaying;
                evidence.pond=screen.PondGame;evidence.pondCloseup=screen.PondCloseup;evidence.pondWaterPlaying=screen.PondWaterPlaying;
                evidence.revealActors=screen.RevealActors;evidence.revealVariants=screen.RevealVariants;evidence.homeRevealEvents=screen.HomeRevealEvents;evidence.daycareRevealEvents=screen.DaycareRevealEvents;evidence.revealBodies=screen.RevealBodies;
                evidence.hideClub=screen.HideClub;evidence.tagClub=screen.TagClub;evidence.clubNpcArt=screen.ClubNpcArt;evidence.clubNpcPoints=screen.ClubNpcPoints;evidence.clubNpcFrames=screen.ClubNpcFrames;evidence.clubTeacherWalking=screen.ClubTeacherWalking;evidence.clubTeacherDrawing=screen.ClubTeacherDrawing;evidence.clubTeacherPoint=screen.ClubTeacherPoint;
                evidence.vetBackdropReady=screen.VetBackdropReady;evidence.vet=screen.VetGame;evidence.vetSelected=screen.VetSelected;evidence.vetTool=screen.VetTool;evidence.vetDrawing=screen.VetDrawing;evidence.vetWalking=screen.VetWalking;evidence.vetQueued=screen.VetQueued;evidence.vetSoundPlaying=screen.VetSoundPlaying;evidence.vetNpcArt=screen.VetNpcArt;evidence.vetFeedback=screen.VetFeedback;
                evidence.treasure=screen.TreasureGame;evidence.treasureNpcArt=screen.TreasureNpcArt;evidence.treasureNpcPoints=screen.TreasureNpcPoints;evidence.treasureApproach=screen.TreasureApproach;evidence.treasureMystery=screen.TreasureMystery;evidence.treasureDemoNote=screen.TreasureDemoNote;evidence.treasureTonePlaying=screen.TreasureTonePlaying;evidence.treasureFeedback=screen.TreasureFeedback;
                evidence.sandLocalPending=screen.SandLocalPending;evidence.sandDirectCueVisible=screen.SandDirectCueVisible;evidence.sandPlacementActive=screen.SandPlacementActive;evidence.sandDecorationChoice=screen.SandDecorationChoice;evidence.sandAttachmentScreenPoints=screen.SandAttachmentScreenPoints;evidence.sandBuilderActors=screen.SandBuilderActors;evidence.sandBuilderReactions=screen.SandBuilderReactions;evidence.sandBuilderEvents=screen.SandBuilderEvents;evidence.sandPieceScreenPoints=screen.SandPieceScreenPoints;evidence.sandIllustrated=screen.SandIllustrated;evidence.sandPhoneLayout=screen.SandPhoneLayout;evidence.sandPresentationEvents=screen.SandPresentationEvents;evidence.sandVoiceVolumes=screen.SandVoiceVolumes;evidence.sandVoiceClips=screen.SandVoiceClips;evidence.sandAudioEvents=screen.SandAudioEvents;evidence.sandActiveVoices=screen.SandActiveVoices;evidence.sandActiveEffects=screen.SandActiveEffects;evidence.sandpit=screen.SandpitGame;evidence.sandpitNpcArt=screen.SandpitNpcArt;evidence.sandpitSelection=screen.SandpitSelection;evidence.sandpitHighlight=screen.SandpitHighlight;evidence.sandpitApproach=screen.SandpitApproach;evidence.sandpitApproachRound=screen.SandpitApproachRound;evidence.sandpitScoopEvents=screen.SandpitScoopEvents;evidence.sandpitWaterEvents=screen.SandpitWaterEvents;evidence.sandpitWaterEffects=screen.SandpitWaterEffects;evidence.sandpitWetVisuals=screen.SandpitWetVisuals;evidence.sandpitBuiltVisuals=screen.SandpitBuiltVisuals;evidence.sandpitTipEvents=screen.SandpitTipEvents;evidence.sandpitTipEffects=screen.SandpitTipEffects;evidence.sandpitTipOutcomes=screen.SandpitTipOutcomes;evidence.sandpitTowerReveal=screen.SandpitTowerReveal;evidence.sandpitDecorationEvents=screen.SandpitDecorationEvents;evidence.sandpitDecorationEffectValues=screen.SandpitDecorationEffectValues;evidence.sandpitDecorationVisuals=screen.SandpitDecorationVisuals;evidence.sandpitDecorationEffects=screen.SandpitDecorationEffects;evidence.sandpitDecorationWiggles=screen.SandpitDecorationWiggles;evidence.sandpitDecorationHidden=screen.SandpitDecorationHidden;evidence.daycare=screen.DaycareGame;evidence.daycareRoutine=screen.DaycareRoutine;evidence.calypsoVisible=screen.CalypsoVisible;evidence.calypsoMoving=screen.CalypsoMoving;evidence.calypsoPose=screen.CalypsoPose;evidence.calypsoDrawing=screen.CalypsoDrawing;evidence.calypsoWalkPlaying=screen.CalypsoWalkPlaying;evidence.calypsoWorldPoint=screen.CalypsoWorldPoint;
                evidence.kingdomNpcArt=screen.KingdomNpcArt;evidence.picnicNpcArt=screen.PicnicNpcArt;evidence.kingdomNpcJobs=screen.KingdomNpcJobs;evidence.kingdomNpcPoses=screen.KingdomNpcPoses;evidence.kingdomNpcPoints=screen.KingdomNpcPoints;
                evidence.kingdom=screen.KingdomGame;evidence.visibleKingdomNpcs=screen.VisibleKingdomNpcs;
                evidence.kingdomApproach=screen.KingdomApproach;evidence.lastTouchTargets=lastTouchTargets;
                evidence.hideAndSeek=screen.HideGame;
                evidence.seagulls=screen.Seagulls;evidence.visibleSeagulls=screen.VisibleSeagulls;evidence.visibleGullTracks=screen.VisibleGullTracks;
                evidence.shore=screen.Shore;evidence.visibleWaveRiders=screen.VisibleWaveRiders;evidence.visibleSandPrints=screen.VisibleSandPrints;evidence.visibleSeaVisitor=screen.VisibleSeaVisitor;evidence.seaVisitorPose=screen.SeaVisitorPose;
                evidence.homeDrawOrder=screen.Board.Cast<Transform>().Where(t=>t.gameObject.activeSelf).Select(t=>t.name).ToArray();
                evidence.discoveryOpen=screen.DiscoveryOpen;evidence.bookOpen=screen.BookOpen;evidence.bookReady=screen.BookPageReady;evidence.bookPlaying=screen.BookPlaying;evidence.bookSpeaking=screen.BookSpeaking;evidence.bookPage=screen.BookPageNumber;evidence.bookSample=screen.BookSample;
                evidence.bookAuto=screen.BookAutoTurn;evidence.bookWords=screen.BookWordsVisible;evidence.bookOptions=screen.BookOptionsOpen;evidence.bookEffect=screen.BookEffectPlaying;evidence.bookEffectPending=screen.BookEffectPending;evidence.bookNaming=screen.BookNaming;evidence.bookTitle=screen.BookTitleId;evidence.bookTextures=screen.BookResidentTextures;evidence.bookAudio=screen.BookResidentAudio;
                evidence.tagNpcVisible=screen.TagNpcVisible;
                evidence.tagSpeaking=screen.Narration.GetComponent<AudioSource>()?.isPlaying==true;evidence.tagCue=screen.Narration.GetComponent<AudioSource>()?.clip?.name??"";
                evidence.visibleText=FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy).Select(t=>t.text).ToArray();
                evidence.fullCharactersInTray=screen.CharactersOpen && FindObjectsByType<GameCharacterVisual>(FindObjectsSortMode.None)
                    .Where(v=>v.GetComponentsInParent<RectMask2D>().Any(m=>m.name=="Cast viewport")).All(v=>
                    {
                        var clip=Bounds((RectTransform)v.GetComponentInParent<RectMask2D>().transform);
                        return v.GetComponentsInChildren<Graphic>().Where(i=>i.enabled).All(i=>
                        {var r=Bounds(i.rectTransform);return r.xMin>=clip.xMin-2 && r.xMax<=clip.xMax+2 && r.yMin>=clip.yMin-2 && r.yMax<=clip.yMax+2;});
                    });
                evidence.boardBounds=Bounds(screen.Board);evidence.boardLayoutWidth=screen.Board.rect.width;
                var active=screen.Board.Find("Player character").GetComponentInChildren<GameCharacterVisual>();
                evidence.outfitsOpen=screen.OutfitsOpen;evidence.outfit=active.Outfit;evidence.outfitColor=screen.ReadPlayer(screen.Actor).outfitColor;evidence.roarPlaying=active.RoarPlaying;
                evidence.activeCharacterVisible=active.GetComponentsInChildren<Graphic>().Where(i=>i.enabled).All(i=>
                {var r=Bounds(i.rectTransform);var clip=evidence.boardBounds;return r.xMin>=clip.xMin-2 && r.xMax<=clip.xMax+2 && r.yMin>=clip.yMin-2 && r.yMax<=clip.yMax+2;});
                // Scrollable content can extend past the viewport. Check the
                // actual clipped targets, not invisible offscreen layout boxes.
                evidence.controls=FindObjectsByType<Button>(FindObjectsSortMode.None).Select(b=>
                {
                    var r=Bounds((RectTransform)b.transform);
                    foreach(var mask in b.GetComponentsInParent<RectMask2D>())
                    {var clip=Bounds((RectTransform)mask.transform);r=Rect.MinMaxRect(Mathf.Max(r.xMin,clip.xMin),Mathf.Max(r.yMin,clip.yMin),Mathf.Min(r.xMax,clip.xMax),Mathf.Min(r.yMax,clip.yMax));}
                    return new ControlView{name=b.name,bounds=r,enabled=b.interactable};
                }).Where(c=>c.bounds.width>0 && c.bounds.height>0).ToArray();
                evidence.controlsInSafeArea=evidence.controls.All(c=>
                {var r=c.bounds;return r.xMin>=Screen.safeArea.xMin-2 && r.yMin>=Screen.safeArea.yMin-2 && r.xMax<=Screen.safeArea.xMax+2 && r.yMax<=Screen.safeArea.yMax+2;});
                evidence.players=(screen.Shared?probe.Latest.view.players:screen.World.Snapshot().players).Select(p=>
                {
                    var rect=screen.Board.Find(p.id==screen.Actor?"Player character":"Friend-"+p.id) as RectTransform;
                    var visual=rect==null?null:rect.GetComponentInChildren<GameCharacterVisual>();
                    return new PlayerView{id=p.id,outfit=visual?.Outfit??"",roarPlaying=visual!=null && visual.RoarPlaying,visible=rect!=null && rect.gameObject.activeSelf,position=rect==null?Vector2.zero:BoardPosition(rect)};
                }).ToArray();
                evidence.toys=screen.ReadToys().Select(t=>
                {
                    var rect=(RectTransform)screen.Board.Find(t.id);
                    return new ToyView{id=t.id,position=BoardPosition(rect),alpha=rect.GetComponent<CanvasGroup>()?.alpha??1,label=string.Join(" ",rect.GetComponentsInChildren<Text>().Select(v=>v.text))};
                }).ToArray();
            }
            var path=Path.Combine(probe.Output,"garden-evidence.json");var temp=path+".pending";
            try
            {
                File.WriteAllText(temp,JsonUtility.ToJson(evidence,true));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
                evidencePending=false;
            }
            catch(IOException e) when(DiagnosticFileWriter.IsReplacementConflict(e.HResult))
            {
                // The Windows test reader can briefly deny atomic replacement.
                // Retry only this observation next frame, never the input action.
                evidencePending=true;
            }
        }
        private void OnDestroy()
        {
            if(mouse!=null)InputSystem.RemoveDevice(mouse);if(touch!=null)InputSystem.RemoveDevice(touch);
            if(hostPointers!=null)foreach(var device in hostPointers)InputSystem.EnableDevice(device);
        }
    }
}
