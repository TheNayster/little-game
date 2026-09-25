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

namespace LittleWeeps.NetworkProbe
{
    // Only installed for explicit isolated verification configs. Inputs travel
    // through Input System + UGUI and the same asynchronous presentation path.
    [DefaultExecutionOrder(1000)]
    public sealed class NetworkGardenVerification : MonoBehaviour
    {
        private NetworkProbe probe;
        private SoloScreen screen;
        private Mouse mouse;
        private Touchscreen touch;
        private InputDevice[] hostPointers;
        private int serial;
        private bool working,down;
        private bool evidencePending;
        private string failure;
        private Vector2 mousePoint;
        private string traceActor;
        private readonly List<MotionSample> motionTrace=new List<MotionSample>();
        [Serializable] private sealed class MotionSample {public double time;public Vector2 visual,authority;}
        [Serializable] private sealed class MotionEvidence {public string actor,build;public MotionSample[] samples;}
        [Serializable] private sealed class Step {public int serial;public string action,role,text;public float x,y;public int finger=11;}
        [Serializable] private sealed class PlayerView {public string id;public Vector2 position;public bool visible;}
        [Serializable] private sealed class ToyView {public string id,label;public Vector2 position;public float alpha;}
        [Serializable] private sealed class Evidence
        {
            public int serial,visiblePlayers,canvases,narrators,audioSources;public bool passed,ready,pending,connected,menuOpen,shared;
            public string error,build,actor,feedback,dragging,zone,savePath;public PlayerView[] players;public ToyView[] toys;
            public int screenWidth,screenHeight;public Rect safeArea,boardBounds;public float boardLayoutWidth;public bool controlsInSafeArea;
        }
        private void OnEnable()=>Application.logMessageReceived+=Log;
        private void OnDisable()=>Application.logMessageReceived-=Log;
        private void Log(string text,string stack,LogType kind){if(kind==LogType.Error || kind==LogType.Exception)failure=text;}
        private void Start()
        {
            probe=GetComponent<NetworkProbe>();screen=GetComponent<SoloScreen>();
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
            if(string.IsNullOrEmpty(step.role))return screen.ScreenPoint(step.x,step.y);
            var rect=screen.Surfaces[step.role].GetComponent<RectTransform>();
            return RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(step.x,step.y,0)));
        }
        private async void Run(Step step)
        {
            working=true;
            try
            {
                if(!screen.Ready && step.action!="inspect")throw new InvalidOperationException("Garden is not ready.");
                if(step.action=="resize")
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
                else if(step.action=="traceStart"){traceActor=step.role;motionTrace.Clear();}
                else if(step.action=="traceStop")
                {
                    File.WriteAllText(Path.Combine(probe.Output,"motion-trace.json"),JsonUtility.ToJson(new MotionEvidence{actor=traceActor,build=Application.version,samples=motionTrace.ToArray()},true));traceActor=null;
                }
                else if(step.action=="network-pause")probe.VerifyFamilyForeground(false);
                else if(step.action=="network-resume")probe.VerifyFamilyForeground(true);
                else if(step.action=="capture")StartCoroutine(Capture());
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
                File.WriteAllBytes(Path.Combine(probe.Output,"garden.png"),image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;RenderTexture.active=previous;
                camera.targetTexture=null;target.Release();Destroy(target);Destroy(go);if(image!=null)Destroy(image);Canvas.ForceUpdateCanvases();
            }
        }
        private void LateUpdate()
        {
            if(traceActor==null || !screen.Ready || motionTrace.Count>=1800)return;
            var p=probe.Latest.view.players.First(v=>v.id==traceActor);
            var rect=screen.Board.Find(traceActor==screen.Actor?"Pup":"Friend-"+traceActor) as RectTransform;
            if(rect!=null && rect.gameObject.activeSelf)motionTrace.Add(new MotionSample{time=Time.realtimeSinceStartupAsDouble,visual=BoardPosition(rect),authority=new Vector2(p.x,p.y)});
        }
        private Vector2 BoardPosition(RectTransform rect)
        {var p=screen.Board.InverseTransformPoint(rect.position);return new Vector2((p.x/screen.Board.rect.width+.5f)*1000,(p.y/screen.Board.rect.height+.5f)*500);}
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
                shared=screen.Shared,savePath=screen.SavePath??"",canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length,
                narrators=FindObjectsByType<SoloNarration>(FindObjectsSortMode.None).Length,audioSources=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length};
            if(screen.Ready)
            {
                evidence.screenWidth=Screen.width;evidence.screenHeight=Screen.height;evidence.safeArea=Screen.safeArea;
                evidence.boardBounds=Bounds(screen.Board);evidence.boardLayoutWidth=screen.Board.rect.width;
                evidence.controlsInSafeArea=FindObjectsByType<Button>(FindObjectsSortMode.None).All(b=>
                {var r=Bounds((RectTransform)b.transform);return r.xMin>=Screen.safeArea.xMin-2 && r.yMin>=Screen.safeArea.yMin-2 && r.xMax<=Screen.safeArea.xMax+2 && r.yMax<=Screen.safeArea.yMax+2;});
                evidence.players=(screen.Shared?probe.Latest.view.players:screen.World.Snapshot().players).Select(p=>
                {
                    var rect=screen.Board.Find(p.id==screen.Actor?"Pup":"Friend-"+p.id) as RectTransform;
                    return new PlayerView{id=p.id,visible=rect!=null && rect.gameObject.activeSelf,position=rect==null?Vector2.zero:BoardPosition(rect)};
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
            catch(IOException e) when((e.HResult&0xffff)==32 || (e.HResult&0xffff)==33)
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
