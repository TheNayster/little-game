// Research-only PEP-01 sampling of original Unity assets. No publisher scripts run.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class RecoveredPeppaFunfairBake
{
    [Serializable] public sealed class Actor {public string role;public long go,head;}
    [Serializable] public sealed class Scene {public int index;public string scene;public Actor[] actors;}
    [Serializable] public sealed class Input {public string rootScene;public Scene[] scenes;}
    [Serializable] public sealed class Point {public long id;public string name;public Vector3 world;public Vector2 pixel;}
    [Serializable] public sealed class Geometry {public string scene;public Vector3 cameraPosition,cameraAngles;public float fov;public Point[] points;}
    [Serializable] public sealed class Clip {public string name,path;public float duration;public int curves;}
    [Serializable] public sealed class ActorAudit {public string role,scene,controller;public long go;public Clip[] clips;public string[] transforms;public int meshes,missingMeshes,missingBones;}
    [Serializable] public sealed class Audit {public ActorAudit[] actors;public string[] errors;}
    [Serializable] public sealed class Pack {public int format=1,fps=12,worldHeight=900,tileCount=4;public RecoveredBirthdayBake.ActionFrames[] actions;public RecoveredBirthdayBake.PropFrame[] props;public string[] limits;}
    static readonly List<RecoveredBirthdayBake.PropFrame> propFrames=new List<RecoveredBirthdayBake.PropFrame>();
    static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("PEPPA_BAKE_OUTPUT")??throw new Exception("PEPPA_BAKE_OUTPUT required"));
    static readonly List<string> errors=new List<string>();
    static void Recover(Renderer r)
    {
        var plastic=r.GetComponent<Duplo.Materials.LegoPlasticMaterial>();
        if(!plastic){
            // The face atlas ST is stored in colour properties, not texture ST.
            if(r.sharedMaterials.Any(m=>m && m.name.Contains("MessyMouth_Face"))){
                r.sharedMaterials=r.sharedMaterials.Select(source=>{
                    var saved=new SerializedObject(source);var colours=saved.FindProperty("m_SavedProperties.m_Colors");
                    Color Read(string key,Color fallback){for(int i=0;i<colours.arraySize;i++){var row=colours.GetArrayElementAtIndex(i);if(row.FindPropertyRelative("first").stringValue==key)return row.FindPropertyRelative("second").colorValue;}return fallback;}
                    var m=RecoveredWorldAssetProbe.ReadMaterial(source);var scale=Read("_MainTex_Scale",new Color(.25f,.25f,0,0));var offset=Read("_MainTex_Offset",new Color(0,.75f,0,0));
                    m.SetColor("_Color",Read("_Color",Color.white));m.mainTextureScale=new Vector2(scale.r,scale.g);m.mainTextureOffset=new Vector2(offset.r,offset.g);m.SetFloat("_UseDecal",1);return m;
                }).ToArray();return;
            }
            RecoveredBirthdayBake.Recover(r);return;
        }
        var m=new Material(Shader.Find("Research/RecoveredBrick"));m.SetColor("_Color",plastic.color);
        if(plastic.decal){m.mainTexture=plastic.decal;m.mainTextureScale=plastic.decalTiling;m.mainTextureOffset=plastic.decalOffset;m.SetFloat("_UseDecal",1);}
        r.sharedMaterials=r.sharedMaterials.Select(_=>m).ToArray();
    }
    static Texture2D Capture(Camera camera,int width,int height)
    {
        var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var t=new Texture2D(width,height,TextureFormat.RGBA32,false);t.ReadPixels(new Rect(0,0,width,height),0,0);t.Apply();camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);return t;
    }
    static Vector2 Pixel(Camera camera,Vector3 p){var v=camera.WorldToViewportPoint(p);return new Vector2(v.x*256,v.y*256);}
    static string ActionKey(string name)
    {
        if(name.EndsWith("_Idle") && !name.Contains("Sitting") && !name.Contains("Fishing") && !name.Contains("Waiting"))return "idle";
        if(name.Contains("WalkWithItem"))return "carry";if(name.EndsWith("Walk"))return "walk";
        if(name.EndsWith("Sitting_Idle"))return "sit";if(name.EndsWith("_ReceiveItem") && !name.Contains("Sitting"))return "hold";
        if(name.EndsWith("Celebration_Standing"))return "happy";if(name.EndsWith("_Cheer"))return "cheer";
        if(name.EndsWith("_Wave") && !name.Contains("Fishing"))return "wave";
        if(name.EndsWith("_Drinking"))return "drink";if(name.EndsWith("Drinking_Sitting"))return "sit-drink";
        if(name.EndsWith("_Laugh"))return "laugh";if(name.EndsWith("_Fall"))return "fall";if(name.EndsWith("_Land"))return "land";
        if(name.EndsWith("Fishing_CastLine"))return "cast";if(name.EndsWith("Fishing_Idle"))return "fish";
        if(name.EndsWith("Fishing_PullCatch"))return "pull";if(name.EndsWith("Fishing_Collect"))return "collect";
        if(name.EndsWith("Fishing_Celebration"))return "fish-happy";
        if(name.EndsWith("Sliding_Backwards"))return "slide-back";if(name.EndsWith("Sliding_OnBelly"))return "slide-belly";if(name.EndsWith("Sliding_Spinning"))return "slide-spin";
        if(name.EndsWith("Tiger_Roar"))return "tiger";if(name.EndsWith("Dinosaur_Roar"))return "dinosaur";if(name.EndsWith("Fairy_Spin"))return "fairy";
        if(name.EndsWith("_LoseBalance"))return "dizzy";return null;
    }
    public static void Run()
    {
        try{
            if(Directory.Exists(Output))throw new Exception("Choose a new immutable bake output");Directory.CreateDirectory(Output);
            bool auditOnly=Environment.GetEnvironmentVariable("PEPPA_BAKE_AUDIT")=="1";
            var config=JsonUtility.FromJson<Input>(File.ReadAllText("Assets/Research/peppa-bake.json"));var audits=new List<ActorAudit>();var actions=new List<RecoveredBirthdayBake.ActionFrames>();var captured=new HashSet<string>();
            foreach(var sceneInput in config.scenes){
                var scene=EditorSceneManager.OpenScene(sceneInput.scene);var roots=scene.GetRootGameObjects();var ts=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                if(!string.IsNullOrEmpty(config.rootScene)){var environment=EditorSceneManager.OpenScene(config.rootScene,OpenSceneMode.Additive);foreach(var t in environment.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true))){foreach(var r in t.GetComponents<Renderer>())Recover(r);foreach(var c in t.GetComponents<Canvas>())c.enabled=false;}}
                var byGo=ts.ToDictionary(t=>(long)GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId);var byId=ts.ToDictionary(t=>(long)GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
                foreach(var t in ts){foreach(var r in t.GetComponents<Renderer>())Recover(r);foreach(var c in t.GetComponents<Canvas>())c.enabled=false;}
                var main=ts.Select(t=>t.GetComponent<Camera>()).First(c=>c && c.name=="Main Camera");main.cullingMask=-1;main.allowHDR=false;main.clearFlags=CameraClearFlags.SolidColor;main.backgroundColor=new Color(.56f,.76f,.92f,1);main.aspect=4f/3;
                if(sceneInput.index==2){main.fieldOfView=70;main.transform.position+=main.transform.up*9;}
                var image=Capture(main,1200,900);File.WriteAllBytes(Path.Combine(Output,"source-world-"+sceneInput.index+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
                var geometry=new Geometry{scene=sceneInput.scene,cameraPosition=main.transform.position,cameraAngles=main.transform.eulerAngles,fov=main.fieldOfView,points=ts.Select(t=>{var v=main.WorldToViewportPoint(t.position);return new Point{id=(long)GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId,name=t.name,world=t.position,pixel=new Vector2(v.x*1200,v.y*900)};}).ToArray()};File.WriteAllText(Path.Combine(Output,"geometry-"+sceneInput.index+".json"),JsonUtility.ToJson(geometry,true));
                foreach(var input in sceneInput.actors)byGo[input.go].gameObject.SetActive(false);
                if(!auditOnly)BakeSceneParts(sceneInput.index,main,byGo,byId);
                image=Capture(main,1200,900);File.WriteAllBytes(Path.Combine(Output,"world-"+sceneInput.index+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
                foreach(var input in sceneInput.actors){
                    var original=byGo[input.go];var originalAnimator=original.GetComponentInChildren<Animator>(true);if(!originalAnimator || !originalAnimator.runtimeAnimatorController){errors.Add("Missing animator "+input.role);continue;}
                    var clips=originalAnimator.runtimeAnimatorController.animationClips.Distinct().ToArray();var skins=original.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s=>s.enabled && s.sharedMesh).ToArray();
                    audits.Add(new ActorAudit{role=input.role,scene=sceneInput.scene,go=input.go,controller=AssetDatabase.GetAssetPath(originalAnimator.runtimeAnimatorController),clips=clips.Select(c=>new Clip{name=c.name,path=AssetDatabase.GetAssetPath(c),duration=c.length,curves=AnimationUtility.GetCurveBindings(c).Length}).ToArray(),transforms=original.GetComponentsInChildren<Transform>(true).Select(t=>AnimationUtility.CalculateTransformPath(t,original)).ToArray(),meshes=skins.Length,missingMeshes=skins.Count(s=>!s.sharedMesh),missingBones=skins.Sum(s=>s.bones.Count(b=>!b))});
                    if(!captured.Add(input.role))continue;
                    var clone=UnityEngine.Object.Instantiate(original.gameObject);clone.SetActive(true);clone.transform.position=new Vector3(10000,0,0);
                    foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    foreach(var t in clone.GetComponentsInChildren<Transform>(true))if(t.name.Contains("DizzyHead"))t.gameObject.SetActive(false);
                    foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
                    var animator=clone.GetComponentInChildren<Animator>(true);animator.enabled=false;animator.fireEvents=false;
                    var renderers=clone.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled && r.gameObject.activeInHierarchy && !(r is ParticleSystemRenderer)).ToArray();var bounds=new Bounds(clone.transform.position,Vector3.zero);foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                    var cam=new GameObject("PEP-01 actor capture").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=Mathf.Max(1,bounds.size.y*.8f);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.transform.rotation=main.transform.rotation;cam.transform.position=bounds.center-cam.transform.forward*12;
                    var baseRotation=clone.transform.rotation;
                    var head=input.head!=0?clone.transform.Find(AnimationUtility.CalculateTransformPath(byId[input.head],original)):null;
                    var faceRenderer=head?head.GetComponent<Renderer>():null;
                    var right=clone.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.EndsWith("Arm_R_SKNBN_4"))??clone.transform;
                    var left=clone.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.EndsWith("Arm_L_SKNBN_4"))??clone.transform;
                    var selected=clips.Where(c=>ActionKey(c.name)!=null && (!auditOnly || ActionKey(c.name)=="idle")).GroupBy(c=>ActionKey(c.name)).Select(g=>g.First()).ToArray();
                    AnimationMode.StartAnimationMode();
                    foreach(var clip in selected)foreach(var view in new[]{"front","side","back"}){
                        if(auditOnly && (input.role!="Peppa" || view!="front"))continue;if(view!="front" && !new[]{"idle","walk","carry","hold","sit","happy"}.Contains(ActionKey(clip.name)))continue;
                        clone.transform.rotation=baseRotation*Quaternion.Euler(0,view=="front"?0:view=="side"?65:180,0);
                        int count=auditOnly || ActionKey(clip.name)=="hold"?1:Math.Max(1,Math.Min(60,(int)Math.Ceiling(clip.length*12)));var row=new RecoveredBirthdayBake.ActionFrames{actor=input.role,action=ActionKey(clip.name)+"-"+view,clip=AssetDatabase.GetAssetPath(clip),texture="Actors/"+input.role+"/"+ActionKey(clip.name)+"-"+view+".png",frames=count,duration=Mathf.Max(.083333f,clip.length),unitsPerPixel=cam.orthographicSize*2/256,rightHand=new Vector2[count],leftHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count],pivot=Pixel(cam,clone.transform.position)};
                        var sheet=new Texture2D(256*8,256*((count+7)/8),TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                        for(int f=0;f<count;f++){AnimationMode.SampleAnimationClip(animator.gameObject,clip,Mathf.Max(0,ActionKey(clip.name)=="hold"?clip.length-.0001f:Mathf.Min(clip.length-.0001f,f/12f)));row.rightHand[f]=Pixel(cam,right.position);row.leftHand[f]=Pixel(cam,left.position);row.hold[f]=row.rightHand[f];row.face[f]=Pixel(cam,faceRenderer?faceRenderer.bounds.center:head?head.position:bounds.center);var frame=Capture(cam,256,256);sheet.SetPixels(f%8*256,(count+7)/8*256-256-f/8*256,256,256,frame.GetPixels());UnityEngine.Object.DestroyImmediate(frame);}
                        sheet.Apply();var file=Path.Combine(Output,row.texture);Directory.CreateDirectory(Path.GetDirectoryName(file));File.WriteAllBytes(file,sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);actions.Add(row);
                    }
                    AnimationMode.StopAnimationMode();UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(clone);Debug.Log("PEPPA_ACTOR "+input.role+" clips="+selected.Length);
                }
            }
            File.WriteAllText(Path.Combine(Output,"import-audit.json"),JsonUtility.ToJson(new Audit{actors=audits.ToArray(),errors=errors.ToArray()},true));
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(new Pack{actions=actions.ToArray(),props=propFrames.ToArray(),limits=new[]{"Source curve sampling and geometry; reconstructed family rules are separate."}},true));
            if(errors.Count>0)throw new Exception(string.Join("; ",errors));EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
