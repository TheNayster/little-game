// Configured conversion shares the established recovery shaders, rig sampling
// and exact-anchor helpers; it never executes the recovered game's scripts.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class RecoveredBirthdayBake
{
    [Serializable] public sealed class PlaysetAction {public string key,clip,kidClip;}
    [Serializable] public sealed class PlaysetWorldPoint {public string name;public Vector3 position;}
    [Serializable] public sealed class PlaysetSettings {public string scene,basePack;public string[] actorNames,hideObjects,controllerActors,controllerModes;public bool appendActors,controllerHolds;public float minX=-39,cameraY=10;public int tiles=4,height=1200;public PlaysetAction[] actions;public PointInput[] points;public PlaysetWorldPoint[] worldPoints;}
    [Serializable] public sealed class PlaysetPicture {public string key,source,file,sourceSha256;}
    [Serializable] public sealed class PlaysetPictures {public PlaysetPicture[] pictures;}
    public static void RunConfiguredPictures()
    {
        try {
            if(Directory.Exists(Output))throw new Exception("Select a fresh picture output.");Directory.CreateDirectory(Output);
            var input=JsonUtility.FromJson<PlaysetPictures>(File.ReadAllText("Assets/Research/world-pictures.json"));EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            foreach(var row in input.pictures){
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(row.source);
                if(!sprite){var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(row.source);if(!texture)throw new Exception("Missing recovered picture "+row.source);sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);}
                var renderer=new GameObject(row.key).AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));var bounds=renderer.bounds;
                var camera=new GameObject("Recovered picture capture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.03f;camera.transform.position=bounds.center+Vector3.back*10;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                var rt=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();row.file=row.key+".png";row.sourceSha256=Hash(row.source);File.WriteAllBytes(Path.Combine(Output,row.file),image.EncodeToPNG());
                camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(renderer.gameObject);
            }
            File.WriteAllText(Path.Combine(Output,"pictures.json"),JsonUtility.ToJson(input,true));EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    public static void RunConfigured()
    {
        try {
            var settings=JsonUtility.FromJson<PlaysetSettings>(File.ReadAllText("Assets/Research/world-bake.json"));
            if(string.IsNullOrEmpty(settings.scene) || settings.actions==null)throw new Exception("Missing world bake configuration.");
            if(Directory.Exists(Output))throw new Exception("Select a fresh bake output.");Directory.CreateDirectory(Output);
            var scene=EditorSceneManager.OpenScene(settings.scene);var roots=scene.GetRootGameObjects();
            var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
            var byObject=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString());
            var inputs=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/actor-bindings.json")).actors;
            foreach(var t in transforms){foreach(var r in t.GetComponents<Renderer>())Recover(r);foreach(var c in t.GetComponents<Canvas>())c.enabled=false;}
            var main=transforms.Select(t=>t.GetComponent<Camera>()).Single(c=>c && c.name=="Main Camera");
            var originalPosition=main.transform.position;var originalRotation=main.transform.rotation;
            foreach(var b in inputs)byObject[b.game_object_id].gameObject.SetActive(false);
            foreach(var id in settings.hideObjects??Array.Empty<string>())byObject[id].gameObject.SetActive(false);
            main.orthographic=true;main.orthographicSize=settings.height/2f/(1200/19.5f);main.aspect=1200f/settings.height;
            main.clearFlags=CameraClearFlags.SolidColor;main.backgroundColor=new Color(.73f,.88f,.98f);
            var target=new RenderTexture(1200,settings.height,24,RenderTextureFormat.ARGB32);main.targetTexture=target;
            var points=new List<ProjectedPoint>();
            for(int tile=0;tile<settings.tiles;tile++){
                main.transform.SetPositionAndRotation(new Vector3(settings.minX+(tile*1200+600)/(1200/19.5f),settings.cameraY,originalPosition.z)+originalRotation*Vector3.up*((settings.height-800)/2f/(1200/19.5f)),originalRotation);
                if(tile==0)foreach(var point in settings.points??Array.Empty<PointInput>()){
                    if(!byId.TryGetValue((ulong)point.transformId,out var t))throw new Exception("Missing source point "+point.name);
                    var v=main.WorldToViewportPoint(t.position);points.Add(new ProjectedPoint{name=point.name,sourceTransformId=point.transformId,sourceWorld=t.position,gamePoint=new Vector2(v.x*1200,(v.y*settings.height-150)/.45f)});
                }
                if(tile==0)foreach(var point in settings.worldPoints??Array.Empty<PlaysetWorldPoint>()){
                    var v=main.WorldToViewportPoint(point.position);points.Add(new ProjectedPoint{name=point.name,sourceWorld=point.position,gamePoint=new Vector2(v.x*1200,(v.y*settings.height-150)/.45f)});
                }
                main.Render();RenderTexture.active=target;var image=new Texture2D(1200,settings.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,settings.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Output,"world-"+tile+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
            }
            main.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);
            File.WriteAllText(Path.Combine(Output,"interaction-points.json"),JsonUtility.ToJson(new ProjectedPoints{sourceSceneSha256=Hash(settings.scene),points=points.ToArray()},true));
            if(!string.IsNullOrEmpty(settings.basePack)){
                var previous=Path.GetFullPath(settings.basePack);
                foreach(var file in Directory.GetFiles(previous,"*",SearchOption.AllDirectories)){
                    var relative=file.Substring(previous.Length+1);if(relative.StartsWith("world-",StringComparison.Ordinal) || relative=="interaction-points.json")continue;
                    var destination=Path.Combine(Output,relative);Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(file,destination);
                }
                File.WriteAllText(Path.Combine(Output,"world-bake-receipt.json"),JsonUtility.ToJson(new HeldReceipt{baseManifestSha256=Hash(Path.Combine(previous,"actor-frames.json")),bakerSha256=Hash("Assets/Editor/RecoveredPlaysetBake.cs")},true));
                if(!settings.appendActors){EditorApplication.Exit(0);return;}
            }
            foreach(var root in roots)root.SetActive(false);
            var actions=new List<ActionFrames>();var missing=new List<string>();
            var previousPack=string.IsNullOrEmpty(settings.basePack)?null:JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(settings.basePack,"actor-frames.json")));
            if(previousPack!=null)actions.AddRange(previousPack.actions);
            foreach(var input in inputs){
                if(settings.actorNames!=null && settings.actorNames.Length>0 && !settings.actorNames.Contains(input.name))continue;
                var source=byObject[input.game_object_id];var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.SetActive(true);clone.transform.position=Vector3.zero;clone.transform.rotation=Quaternion.identity;
                foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                foreach(var renderer in clone.GetComponentsInChildren<Renderer>(true))Recover(renderer);
                var animator=clone.GetComponentInChildren<Animator>(true);if(!animator || !animator.runtimeAnimatorController)throw new Exception("Missing actor controller "+input.name);animator.enabled=false;animator.fireEvents=false;
                Transform Anchor(long id){if(id==0)return clone.transform;var original=byId[(ulong)id];return clone.transform.Find(AnimationUtility.CalculateTransformPath(original,source))??throw new Exception("Anchor outside actor "+input.name);}
                var left=Anchor(input.leftId);var right=Anchor(input.rightId);var hold=Anchor(input.holdId);var face=Anchor(input.faceId);
                var bounds=new Bounds(clone.transform.position,Vector3.zero);foreach(var renderer in clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)))bounds.Encapsulate(renderer.bounds);
                var camera=new GameObject("Configured actor capture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(1,bounds.size.y*.95f);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
                var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;
                AnimationMode.StartAnimationMode();
                foreach(var view in new[]{"front","side","back"}){
                    clone.transform.rotation=Quaternion.Euler(0,Yaw(view),0);
                    foreach(var action in settings.actions){
                        if(view!="front" && !new[]{"idle","walk","run","hold","sit","sleep","happy"}.Contains(action.key))continue;
                        var clip=ResolveOverride(animator.runtimeAnimatorController,action.clip);
                        if(!clip && !string.IsNullOrEmpty(action.kidClip))clip=ResolveOverride(animator.runtimeAnimatorController,action.kidClip);
                        if(!clip){missing.Add(input.name+"/"+action.key);continue;}
                        int count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));
                        var row=new ActionFrames{actor=input.name,action=action.key+Suffix(view),clip=AssetDatabase.GetAssetPath(clip),frames=count,duration=clip.length,texture=input.name+"/"+action.key+Suffix(view)+".png",unitsPerPixel=camera.orthographicSize*2/256,pivot=Pixel(camera,clone.transform.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
                        var sheet=new Texture2D(2048,(count+7)/8*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                        for(int frame=0;frame<count;frame++){
                            AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,Mathf.Max(0,Mathf.Min(clip.length-.00001f,frame/12f)));AnimationMode.EndSampling();camera.Render();RenderTexture.active=rt;
                            var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();sheet.SetPixels32(frame%8*256,sheet.height-256-frame/8*256,256,256,image.GetPixels32());
                            row.leftHand[frame]=Pixel(camera,left.position);row.rightHand[frame]=Pixel(camera,right.position);row.hold[frame]=Pixel(camera,hold.position);row.face[frame]=Pixel(camera,face.position);UnityEngine.Object.DestroyImmediate(image);
                        }
                        sheet.Apply();var path=Path.Combine(Output,row.texture);Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);actions.RemoveAll(a=>a.actor==row.actor && a.action==row.action);actions.Add(row);
                    }
                }
                AnimationMode.StopAnimationMode();
                if(settings.controllerHolds && (settings.controllerActors==null || settings.controllerActors.Contains(input.name)))foreach(var view in new[]{"front","side","back"}){
                    clone.transform.rotation=Quaternion.Euler(0,Yaw(view),0);
                    foreach(var mode in settings.controllerModes??new[]{"hold","carry","pickup","happy-hold"}){var row=BakeControllerHold(animator,camera,rt,clone.transform,left,right,hold,face,input.name,mode,view);actions.RemoveAll(a=>a.actor==row.actor && a.action==row.action);actions.Add(row);}
                }
                camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);Debug.Log("CONFIGURED_BAKE "+input.name);
            }
            var props=new List<PropFrame>();
            if(previousPack!=null)props.AddRange(previousPack.props);
            if(File.Exists("Assets/Research/prop-bindings.json"))foreach(var input in JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/prop-bindings.json")).actors){
                Transform source;Dictionary<ulong,Transform> ids;
                if(string.IsNullOrEmpty(input.prefab)){source=byObject[input.game_object_id];ids=byId;}
                else{var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(input.prefab);if(!prefab)throw new Exception("Missing prop prefab "+input.prefab);var ts=prefab.GetComponentsInChildren<Transform>(true);ids=ts.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);source=ts.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString()==input.game_object_id);}
                foreach(var view in new[]{"front","side","back","held-front","held-side","held-back"})if(!props.Any(p=>p.name==input.name && p.view==view))props.Add(BakeProp(source,input,ids,view));
            }
            var pack=new Manifest{format=3,sourceScene=settings.scene,sourceSceneSha256=Hash(settings.scene),bakerSha256=Hash("Assets/Editor/RecoveredPlaysetBake.cs"),bindingsSha256=Hash("Assets/Research/actor-bindings.json"),unityVersion=Application.unityVersion,worldHeight=settings.height,tileCount=settings.tiles,actions=actions.ToArray(),props=props.ToArray(),missingActions=missing.ToArray(),limits=new[]{"Recovered clip/anchor sampling; behavior is a handwritten family adaptation.","Imported appearance and action timing need separate runtime review."}};
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(pack,true));EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
}
