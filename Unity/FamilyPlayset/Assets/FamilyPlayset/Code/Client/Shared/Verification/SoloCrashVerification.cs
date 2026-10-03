using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    // Opt-in Windows fixture. The wrapper terminates only its own GUID-scoped process.
    public sealed class SoloCrashVerification : MonoBehaviour
    {
        private GameScreen screen;
        private string error;
        private bool waitingForKill;
        private void OnEnable()=>Application.logMessageReceived+=OnLog;
        private void OnDisable()=>Application.logMessageReceived-=OnLog;
        private void OnLog(string message,string stack,LogType type)
        {if(type==LogType.Error || type==LogType.Exception)error=message;}
        private string PathFor(string name)=>Path.Combine(Path.GetDirectoryName(screen.SavePath),name);
        private async void Start()
        {
            screen=GetComponent<GameScreen>();
            var result=new Record{runId=screen.VerifyRun,build=Application.version,mode=screen.VerifyMode};
            try
            {
                Require(Guid.TryParseExact(screen.VerifyRun,"N",out _),"An isolated test namespace is required.");
                await Task.Delay(300);
                if(screen.VerifyMode=="crash-hold")
                {
                    Require(screen.LoadedStatus==CheckpointStatus.Missing,"Crash seed must be a new world.");
                    Apply(SoloAction.ChangeAvatar,value:"orange-pup");Apply(SoloAction.Move,x:345,y:210);
                    Apply(SoloAction.StartActivity,value:"garden");
                    DropAt("bucket-1","tap-1",150,340);DropAt("bucket-1","plant-1",810,330);
                    DropAt("bucket-1","tap-1",150,340);DropAt("sponge-1","puddle-1",680,140);
                    var bucket=screen.World.ReadToys().Single(t=>t.id=="bucket-1");
                    ExecuteEvents.Execute(screen.Surfaces["bucket-1"].gameObject,
                        new PointerEventData(EventSystem.current){pointerId=71,position=screen.ScreenPoint(bucket.x,bucket.y)},ExecuteEvents.pointerDownHandler);
                    Require(screen.World.ReadToys().Single(t=>t.id=="bucket-1").holder==screen.Actor,"Pointer did not hold the bucket.");
                    screen.SaveNow();screen.SaveNow();Require(error==null,error);
                    var baseline=JsonUtility.ToJson(screen.World.Snapshot(),true);
                    File.WriteAllText(PathFor("crash-baseline.json"),baseline);
                    waitingForKill=true;
                    File.WriteAllText(PathFor("crash-ready.json"),JsonUtility.ToJson(new Record{runId=screen.VerifyRun,build=Application.version,mode=screen.VerifyMode,passed=true},true));
                    // No graceful exit is permitted to count as a crash test.
                    await Task.Delay(60000);
                    throw new InvalidOperationException("Wrapper did not terminate the waiting test process.");
                }
                Require(screen.LoadedStatus==CheckpointStatus.Loaded,"Expected the committed checkpoint after OS termination.");
                var expected=JsonUtility.FromJson<SoloSnapshot>(File.ReadAllText(PathFor("crash-baseline.json")));
                Require(expected.toys.Count(t=>!string.IsNullOrEmpty(t.holder))==1,"Baseline must include a held toy.");
                foreach(var toy in expected.toys)toy.holder="";
                expected.revision++;
                Require(JsonUtility.ToJson(screen.World.Snapshot())==JsonUtility.ToJson(expected),"Restored world changed beyond releasing the transient hold.");
                result.exactStateRestored=true;result.staleHoldReleased=true;
                Apply(SoloAction.Grab,"bucket-1");Apply(SoloAction.CancelGrab,"bucket-1");
                screen.SaveNow();Require(error==null,error);
                result.toyUsableAgain=true;result.passed=true;
            }
            catch(Exception e){result.error=e.ToString();}
            result.utc=DateTime.UtcNow.ToString("O");
            File.WriteAllText(PathFor(screen.VerifyMode+".json"),JsonUtility.ToJson(result,true));
            Application.Quit(result.passed?0:1);
        }
        private void OnApplicationQuit()
        {if(waitingForKill)File.WriteAllText(PathFor("crash-graceful-exit.txt"),"OnApplicationQuit ran; this is not a hard-stop test.");}
        private void DropAt(string item,string target,float x,float y)
        {Apply(SoloAction.Grab,item);Apply(SoloAction.Drop,item,target,x:x,y:y);}
        private void Apply(SoloAction action,string item="",string target="",string value="",float x=0,float y=0)
        {Require(screen.Command(action,item,target,value,x,y).Accepted,"Rejected test action: "+action);}
        private static void Require(bool condition,string message)
        {if(!condition)throw new InvalidOperationException(message??"Runtime error");}
        [Serializable] private sealed class Record
        {
            public string runId,build,mode,utc,error;
            public bool passed,exactStateRestored,staleHoldReleased,toyUsableAgain;
        }
    }
}
