using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LittleWeeps.Client
{
    // Explicit opt-in local evidence adapter. Never opened by the production scene.
    public sealed class SandcastlePrototypeCapture : MonoBehaviour
    {
        [Serializable] public sealed class Step {public string id,action,text;public float x,y;}
        [Serializable] public sealed class Control {public string name;public float x,y,width,height;public bool enabled;}
        [Serializable] public sealed class Point {public float worldX,worldY,x,y;}
        [Serializable] public sealed class Evidence {public string id,error,selected,outcome,reaction;public bool fixture,placing,hasPreview;public float previewX,previewY;public int width,height;public LittleWeeps.Core.SandpitState sandpit;public Control[] controls;public Point[] cells;}
        private SandcastlePrototype owner;
        private string folder,last;
        private bool busy,filming;
        private Touchscreen touch;
        private void Start()
        {
            owner=GetComponent<SandcastlePrototype>();var args=Environment.GetCommandLineArgs();var i=Array.IndexOf(args,"-sandcastleEvidence");
            if(i<0 || i+1>=args.Length)return;
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",".."));
            folder=Path.GetFullPath(args[i+1]);
            // A dedicated caller-owned evidence directory, never a save or install destination.
            if(!folder.Contains(Path.DirectorySeparatorChar+"LocalData"+Path.DirectorySeparatorChar+"SandcastlePrototype"+Path.DirectorySeparatorChar)){folder=null;throw new IOException("Isolated prototype evidence path required");}
            Directory.CreateDirectory(folder);touch=InputSystem.AddDevice<Touchscreen>();
        }
        private void Update()
        {
            if(folder==null || busy || !File.Exists(Path.Combine(folder,"request.json")))return;
            Step step;try {step=JsonUtility.FromJson<Step>(File.ReadAllText(Path.Combine(folder,"request.json")));}catch(IOException){return;}if(step==null || step.id==last)return;
            last=step.id;StartCoroutine(Execute(step));
        }
        private IEnumerator Execute(Step s)
        {
            busy=true;
            if(s.action=="resize"){Screen.SetResolution((int)s.x,(int)s.y,false);yield return new WaitForSecondsRealtime(.8f);}
            if(s.action=="tap" || s.action=="button"){
                var p=new Vector2(s.x,s.y);
                if(s.action=="button"){
                    var b=owner.Canvas.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==s.text && b.interactable);
                    if(b==null){Write(s.id,"Missing enabled button "+s.text);busy=false;yield break;}
                    p=ScreenPoint((RectTransform)b.transform);
                }
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=91,phase=UnityEngine.InputSystem.TouchPhase.Began,position=p,pressure=1});
                yield return new WaitForSecondsRealtime(.09f);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=91,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=p});yield return new WaitForSecondsRealtime(.15f);
            }
            if(s.action=="capture"){yield return null;Capture(Path.Combine(folder,s.text+".png"));}
            if(s.action=="film")StartCoroutine(Film(Mathf.Clamp(s.x,5,90)));
            Write(s.id,"");busy=false;
        }
        private Vector2 ScreenPoint(RectTransform r)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
        private void Write(string id,string error)
        {
            Canvas.ForceUpdateCanvases();var controls=owner.Canvas.GetComponentsInChildren<Button>().Select(b=>{
                var r=(RectTransform)b.transform;var corners=new Vector3[4];r.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var z=RectTransformUtility.WorldToScreenPoint(null,corners[2]);return new Control{name=b.name,x=(a.x+z.x)/2,y=(a.y+z.y)/2,width=z.x-a.x,height=z.y-a.y,enabled=b.interactable};}).ToArray();
            var cells=Enumerable.Range(0,32).Select(i=>{var p=LittleWeeps.Core.DaycareSandpit.Cell(i%8,i/8);var display=SandcastleProjection.Project(p.X,p.Y);var s=RectTransformUtility.WorldToScreenPoint(null,owner.Surface.TransformPoint(display));return new Point{worldX=p.X,worldY=p.Y,x=s.x,y=s.y};}).ToArray();
            File.WriteAllText(Path.Combine(folder,"response.json"),JsonUtility.ToJson(new Evidence{id=id,error=error,selected=owner.Selected,outcome=owner.LastOutcome,reaction=owner.Reaction,fixture=owner.Fixture,placing=owner.Placing,hasPreview=owner.HasPreview,previewX=owner.PreviewLogical.x,previewY=owner.PreviewLogical.y,width=Screen.width,height=Screen.height,sandpit=owner.State,controls=controls,cells=cells},true));
        }
        private IEnumerator Film(float seconds)
        {
            if(filming)yield break;filming=true;var dir=Path.Combine(folder,"film");Directory.CreateDirectory(dir);var times=new System.Collections.Generic.List<string>();var start=Time.realtimeSinceStartup;var f=0;
            while(Time.realtimeSinceStartup-start<seconds && f<600){yield return null;Capture(Path.Combine(dir,(f++).ToString("D4")+".png"));times.Add((Time.realtimeSinceStartup-start).ToString("R",System.Globalization.CultureInfo.InvariantCulture));yield return new WaitForSecondsRealtime(.12f);}
            File.WriteAllLines(Path.Combine(dir,"times.txt"),times);filming=false;
        }
        private void Capture(string path)
        {
            var cv=owner.Canvas;var mode=cv.renderMode;var oldCamera=cv.worldCamera;var distance=cv.planeDistance;
            var go=new GameObject("Prototype evidence camera",typeof(Camera));var cam=go.GetComponent<Camera>();cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.19f,.37f,.24f);cam.orthographic=true;cam.orthographicSize=Screen.height/2f;cam.transform.position=new Vector3(0,0,-10);
            var target=new RenderTexture(Screen.width,Screen.height,24);target.Create();var previous=RenderTexture.active;Texture2D image=null;
            try {cam.targetTexture=target;cv.renderMode=RenderMode.ScreenSpaceCamera;cv.worldCamera=cam;cv.planeDistance=10;Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally {cv.renderMode=mode;cv.worldCamera=oldCamera;cv.planeDistance=distance;RenderTexture.active=previous;cam.targetTexture=null;target.Release();Destroy(target);Destroy(go);if(image!=null)Destroy(image);Canvas.ForceUpdateCanvases();}
        }
        private void OnDestroy(){if(touch!=null)InputSystem.RemoveDevice(touch);}
    }
}
