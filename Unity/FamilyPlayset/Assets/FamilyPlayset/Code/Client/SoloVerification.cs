using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LittleWeeps.Client
{
    public sealed class SoloVerification : MonoBehaviour
    {
        private SoloScreen screen;
        private string failure;
        private void OnEnable()=>Application.logMessageReceived+=Log;
        private void OnDisable()=>Application.logMessageReceived-=Log;
        private void Log(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)failure=message;}
        private async void Start()
        {
            screen=GetComponent<SoloScreen>();var result=new Record{mode=screen.VerifyMode,runId=screen.VerifyRun,build=Application.version};
            try
            {
                await Task.Delay(300);Check(screen.World!=null,"World did not start.");
                Check(screen.Narration.Ready,"Bundled English narration did not load.");result.narrationLoaded=true;
                Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled && l.gameObject.activeInHierarchy)==1,"Scene needs exactly one active audio listener.");
                screen.Narration.Speak("garden");
                var speaker=screen.Narration.GetComponent<AudioSource>();
                await Wait(()=>speaker.isPlaying && speaker.timeSamples>0,"Muted narration playback");
                screen.Narration.Speak("cleanup");Check(speaker.clip.name=="cleanup","New hint did not replace old narration.");
                screen.Narration.Stop();Check(!speaker.isPlaying,"Narration stop failed.");result.narrationPlayback=true;
                if(screen.VerifyMode=="input")
                {
                    Check(screen.LoadedStatus==CheckpointStatus.Missing,"Input test needs fresh storage.");
                    Canvas.ForceUpdateCanvases();
                    // The production UI intentionally unifies mouse/pen pointers.
                    // Keep live host motion from overwriting this test's virtual mouse.
                    var hostPointers=InputSystem.devices.Where(d=>(d is Mouse || d is Pen) && d.enabled).ToArray();
                    foreach(var device in hostPointers)InputSystem.DisableDevice(device);
                    // These are process-local test devices. Hidden players have no OS focus;
                    // real Mouse/Touchscreen layouts are correctly disabled in that state.
                    InputSystem.RegisterLayout("{\"name\":\"SoloTestMouse\",\"extend\":\"Mouse\",\"runInBackground\":\"enabled\"}");
                    InputSystem.RegisterLayout("{\"name\":\"SoloTestTouch\",\"extend\":\"Touchscreen\",\"runInBackground\":\"enabled\"}");
                    var mouse=(Mouse)InputSystem.AddDevice("SoloTestMouse");
                    try
                    {
                        Check(mouse.canRunInBackground && mouse.enabled,"Hidden-test mouse layout is not enabled.");
                        await MouseDrag(mouse,"bucket-1","tap-1");Check(Toy("bucket-1").water==3,"Input-module drag did not fill bucket.");
                        await MouseDrag(mouse,"bucket-1","plant-1");Check(Toy("plant-1").water==3 && Toy("bucket-1").water==0,"Input-module pour failed.");
                        await MouseDrag(mouse,"sponge-1","puddle-1");Check(Toy("puddle-1").water==2,"Input-module cleanup failed.");
                        result.mouseInputModule=true;
                    }
                    finally {InputSystem.RemoveDevice(mouse);foreach(var device in hostPointers)InputSystem.EnableDevice(device);}
                    var touch=(Touchscreen)InputSystem.AddDevice("SoloTestTouch");
                    try
                    {
                        Check(touch.canRunInBackground && touch.enabled,"Hidden-test touch layout is not enabled.");
                        screen.ToggleMovement();
                        var stick=screen.Surfaces["stick"].GetComponent<RectTransform>();
                        var stickPoint=RectTransformUtility.WorldToScreenPoint(null,stick.TransformPoint(new Vector3(45,0,0)));
                        var from=ToyPoint("bucket-1");var to=ToyPoint("tap-1");var before=Player().x;
                        Touch(touch,11,UnityEngine.InputSystem.TouchPhase.Began,stickPoint);
                        Touch(touch,12,UnityEngine.InputSystem.TouchPhase.Began,from);
                        await Wait(()=>Toy("bucket-1").holder==screen.Actor && Player().x>before+20,"Two simultaneous touch pointers");
                        for(var i=1;i<=6;i++){Touch(touch,12,UnityEngine.InputSystem.TouchPhase.Moved,Vector2.Lerp(from,to,i/6f));await Task.Delay(60);}
                        Touch(touch,12,UnityEngine.InputSystem.TouchPhase.Ended,to);
                        Touch(touch,11,UnityEngine.InputSystem.TouchPhase.Ended,stickPoint);
                        await Wait(()=>Toy("bucket-1").holder=="","Touch release");Check(Toy("bucket-1").water==3,"Touch drag did not fill bucket.");
                        Touch(touch,13,UnityEngine.InputSystem.TouchPhase.Began,ToyPoint("sponge-1"));
                        await Wait(()=>Toy("sponge-1").holder==screen.Actor,"Touch grab before menu");
                        screen.SetMenu(true);Check(Toy("sponge-1").holder=="","Menu did not cancel real input-module touch.");
                        Touch(touch,13,UnityEngine.InputSystem.TouchPhase.Canceled,ToyPoint("sponge-1"));
                        screen.SetMenu(false);
                        await Task.Delay(100);
                        var originalSponge=Toy("sponge-1");var waterBefore=Toy("puddle-1").water;
                        Touch(touch,14,UnityEngine.InputSystem.TouchPhase.Began,ToyPoint("sponge-1"));
                        await Wait(()=>Toy("sponge-1").holder==screen.Actor,"Grab before OS touch cancellation");
                        Touch(touch,14,UnityEngine.InputSystem.TouchPhase.Moved,ToyPoint("puddle-1"));await Task.Delay(100);
                        Touch(touch,14,UnityEngine.InputSystem.TouchPhase.Canceled,ToyPoint("puddle-1"));
                        await Wait(()=>Toy("sponge-1").holder=="","OS touch cancellation");
                        Check(Toy("puddle-1").water==waterBefore && Toy("sponge-1").x==originalSponge.x && Toy("sponge-1").y==originalSponge.y,"Cancelled touch committed an interaction.");
                        result.touchInputModule=true;result.osTouchCancellation=true;
                    }
                    finally {InputSystem.RemoveDevice(touch);InputSystem.RemoveLayout("SoloTestMouse");InputSystem.RemoveLayout("SoloTestTouch");}
                }
                else if(screen.VerifyMode=="seed")
                {
                    Check(screen.LoadedStatus==CheckpointStatus.Missing,"Seed must use fresh storage.");
                    // Ground raycasts reach the movement surface, not an overlaid control.
                    var target=screen.ScreenPoint(480,80);Check(Hit(target)==screen.Surfaces["ground"].gameObject,"Ground raycast blocked.");
                    Down("ground",10,target);Up("ground",10,target);
                    await Wait(()=>Math.Abs(Player().x-480)<2 && Math.Abs(Player().y-80)<2,"Tap to walk");
                    result.tapWalk=true;
                    screen.ToggleMovement();
                    var stick=screen.Surfaces["stick"].GetComponent<RectTransform>();
                    var stickPoint=RectTransformUtility.WorldToScreenPoint(null,stick.TransformPoint(new Vector3(45,0,0)));
                    var start=Player().x;Down("stick",20,stickPoint);
                    Down("bucket-1",21,ToyPoint("bucket-1"));
                    Check(Toy("bucket-1").holder==screen.Actor,"Grab failed.");
                    Down("bucket-1",22,ToyPoint("bucket-1"));Up("bucket-1",22,screen.ScreenPoint(200,200));
                    Check(Toy("bucket-1").holder==screen.Actor,"A second pointer stole the gesture.");
                    screen.ChooseAvatar("orange-pup");
                    Check(Player().avatar=="orange-pup" && Toy("bucket-1").holder==screen.Actor,"Avatar switch lost the held prop.");
                    await Wait(()=>Player().x>start+30,"Joystick while dragging");
                    Up("stick",20,stickPoint);Drop("bucket-1",21,"tap-1");
                    Check(Toy("bucket-1").water==3,"Drag did not fill bucket.");result.joystickAndDrag=true;result.pointerExclusivity=true;
                    DragToy("bucket-1","plant-1");Check(Toy("plant-1").water==3 && Toy("bucket-1").water==0,"Pour failed.");
                    screen.StartActivity("garden");screen.StartActivity("cleanup");screen.StartActivity("");
                    Check(Player().activity=="" && Toy("plant-1").water==3,"Leaving a quest reset the world.");
                    for(var i=0;i<3;i++)DragToy("sponge-1","puddle-1");
                    Check(Toy("puddle-1").water==0,"Cleanup failed.");result.interactions=true;
                    Down("bucket-1",30,ToyPoint("bucket-1"));screen.SetMenu(true);
                    Check(Toy("bucket-1").holder=="","Menu retained a pointer lease.");
                    Up("bucket-1",30,ToyPoint("tap-1"));Check(Toy("bucket-1").water==0,"Cancelled gesture committed after menu opened.");
                    screen.SetMenu(false);Down("sponge-1",40,ToyPoint("sponge-1"));
                    screen.SendMessage("OnApplicationFocus",false);Check(Toy("sponge-1").holder=="","Focus loss did not cancel drag.");
                    result.cancellation=true;
                    screen.SaveNow();screen.SaveNow(); // Both committed and previous-good contain the acceptance baseline.
                }
                else
                {
                    var seed=JsonUtility.FromJson<Record>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(screen.SavePath),"seed.json")));
                    Check(seed.passed,"Seed did not pass.");
                    var s=screen.World.Snapshot();
                    Check(s.worldId==seed.worldId && screen.Actor==seed.playerId,"Profile/world identity changed.");
                    Check(Player().avatar=="orange-pup" && Toy("plant-1").water==3 && Toy("bucket-1").water==0 && Toy("puddle-1").water==0,"Saved play did not survive relaunch.");
                    Check(s.toys.All(t=>string.IsNullOrEmpty(t.holder)),"A stale pointer lease survived.");
                    Check(screen.LoadedStatus==(screen.VerifyMode=="recover"?CheckpointStatus.Recovered:CheckpointStatus.Loaded),"Unexpected recovery path.");
                    screen.SaveNow();result.relaunch=true;
                }
                Check(failure==null,failure??"Runtime error.");
                result.worldId=screen.World.Snapshot().worldId;result.playerId=screen.Actor;result.revision=screen.World.Revision;
                result.loadedStatus=screen.LoadedStatus.ToString();result.passed=true;
            }
            catch(Exception e){result.passed=false;result.error=e.ToString();}
            result.utc=DateTime.UtcNow.ToString("O");
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(screen.SavePath),screen.VerifyMode+".json"),JsonUtility.ToJson(result,true));
            Debug.Log("LITTLE_WEEPS_SOLO_VERIFY "+JsonUtility.ToJson(result));Application.Quit(result.passed?0:1);
        }
        private SoloPlayer Player()=>screen.World.Snapshot().players[0];
        private void Touch(Touchscreen touch,int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 position)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=position,pressure=phase==UnityEngine.InputSystem.TouchPhase.Ended?0:1});
        private async Task MouseDrag(Mouse mouse,string item,string target)
        {
            var from=ToyPoint(item);var to=ToyPoint(target);
            Check(Hit(from)==screen.Surfaces[item].gameObject,"Input-module origin raycast did not hit "+item);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=from});await Task.Delay(120);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=from}.WithButton(MouseButton.Left));
            await Task.Delay(120);
            var module=EventSystem.current.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("SOLO_TEST_POINTER enabled="+mouse.enabled+" background="+mouse.canRunInBackground+" position="+mouse.position.ReadValue()+" pressed="+mouse.leftButton.isPressed+" action="+module.leftClick.action.enabled+" focus="+EventSystem.current.isFocused);
            await Wait(()=>Toy(item).holder==screen.Actor,"Input-module press "+item);
            for(var i=1;i<=6;i++){InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(from,to,i/6f)}.WithButton(MouseButton.Left));await Task.Delay(60);}
            InputSystem.QueueStateEvent(mouse,new MouseState{position=to});
            await Wait(()=>Toy(item).holder=="","Input-module release "+item);
        }
        private SoloToy Toy(string id)=>screen.World.Snapshot().toys.First(t=>t.id==id);
        private Vector2 ToyPoint(string id){var t=Toy(id);return screen.ScreenPoint(t.x,t.y);}
        private GameObject Hit(Vector2 point){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);return hits.Count>0?hits[0].gameObject:null;}
        private void Down(string role,int pointer,Vector2 point)=>ExecuteEvents.Execute(screen.Surfaces[role].gameObject,new PointerEventData(EventSystem.current){pointerId=pointer,position=point},ExecuteEvents.pointerDownHandler);
        private void Up(string role,int pointer,Vector2 point)=>ExecuteEvents.Execute(screen.Surfaces[role].gameObject,new PointerEventData(EventSystem.current){pointerId=pointer,position=point},ExecuteEvents.pointerUpHandler);
        private void Drop(string item,int pointer,string target){var point=ToyPoint(target);ExecuteEvents.Execute(screen.Surfaces[item].gameObject,new PointerEventData(EventSystem.current){pointerId=pointer,position=point},ExecuteEvents.dragHandler);Up(item,pointer,point);}
        private void DragToy(string item,string target){var point=ToyPoint(item);Check(Hit(point)==screen.Surfaces[item].gameObject,"Toy raycast blocked: "+item);Down(item,50,point);Drop(item,50,target);}
        private async Task Wait(Func<bool> condition,string check,float seconds=10){var until=Time.realtimeSinceStartup+seconds;while(!condition()){Check(failure==null && Time.realtimeSinceStartup<until,check+" failed/timed out: "+failure);await Task.Delay(30);}}
        private static void Check(bool condition,string text){if(!condition)throw new InvalidOperationException(text);}
        public static void FinishBlocked(SoloScreen screen,string error)
        {Directory.CreateDirectory(Path.GetDirectoryName(screen.SavePath));File.WriteAllText(Path.Combine(Path.GetDirectoryName(screen.SavePath),screen.VerifyMode+".json"),JsonUtility.ToJson(new Record{error=error,runId=screen.VerifyRun,mode=screen.VerifyMode,build=Application.version}));Application.Quit(1);}
        [Serializable] private sealed class Record
        {
            public string mode,runId,build,worldId,playerId,loadedStatus,utc,error;
            public bool passed,tapWalk,joystickAndDrag,pointerExclusivity,interactions,cancellation,relaunch,mouseInputModule,touchInputModule,osTouchCancellation,narrationLoaded,narrationPlayback;
            public long revision;
        }
    }
}
