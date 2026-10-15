// Research-only conversion from recovered rigs to the existing 2D game pipeline.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class RecoveredBirthdayBake
{
    [Serializable] public sealed class ActorInput {public string name,game_object_id,prefab;public long leftId,rightId,holdId,baseId,faceId;}
    [Serializable] public sealed class ActorInputs {public ActorInput[] actors;}
    [Serializable] public sealed class ActionFrames {public string actor,action,clip,texture;public int frames,columns=8,size=256;public float duration,unitsPerPixel;public Vector2[] rightHand,leftHand,hold,face;public Vector2 pivot;}
    [Serializable] public sealed class PropFrame {public string name,texture,view;public float unitsPerPixel;public Vector2 pivot,holdPivot;public long sourceBaseId,sourceHoldId;}
    [Serializable] public sealed class Manifest {public string sourceScene,sourceSceneSha256,bakerSha256,bindingsSha256,unityVersion;public int format=2,fps=12;public ActionFrames[] actions;public PropFrame[] props;public string[] missingActions,limits;public CakeSprite[] cakeSprites;public AudioRow[] audio;}
    [Serializable] public sealed class AudioRow {public string key,file,source,source_sha256,sha256;public double duration;}
    [Serializable] public sealed class Landmark {public string name;public Vector3 position;}
    [Serializable] public sealed class Geometry {public Vector3 cameraPosition,cameraRotation;public float fieldOfView,tileWorldWidth=19.5f,pixelsPerUnit=1200/19.5f,worldMinX=-39;public Landmark[] landmarks;}
    [Serializable] public sealed class CakeSprite {public string name,source,texture,sourceSha256,category;public Vector2 pivot;public float sourcePixelsPerUnit;}
    [Serializable] public sealed class CakeSprites {public string bakerSha256;public CakeSprite[] sprites;}
    [Serializable] public sealed class PointInput {public string name;public long transformId;}
    [Serializable] public sealed class PointInputs {public PointInput[] points;}
    [Serializable] public sealed class ProjectedPoint {public string name;public long sourceTransformId;public Vector3 sourceWorld;public Vector2 gamePoint;}
    [Serializable] public sealed class ProjectedPoints {public string sourceSceneSha256,bakerSha256;public ProjectedPoint[] points;}
    public static void RunGeometry()
    {
        try {
            if(Directory.Exists(Output))throw new Exception("Geometry inspection requires a fresh directory.");Directory.CreateDirectory(Output);
            const string scenePath="Assets/DuploMarvel/PlayPacks/HulksBirthdayParty/Scenes/HBP_Main.unity";
            var scene=EditorSceneManager.OpenScene(scenePath);
            var transforms=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
            var camera=transforms.Select(t=>t.GetComponent<Camera>()).Single(c=>c && c.name=="Main Camera");
            camera.orthographic=true;camera.orthographicSize=6.5f;camera.aspect=1.5f;camera.transform.position=new Vector3(-29.25f,10,camera.transform.position.z);
            var inputs=JsonUtility.FromJson<PointInputs>(File.ReadAllText("Assets/Research/interaction-points.json"));
            var rows=inputs.points.Select(p=>{
                var world=byId[(ulong)p.transformId].position;var view=camera.WorldToViewportPoint(world);
                return new ProjectedPoint{name=p.name,sourceTransformId=p.transformId,sourceWorld=world,gamePoint=new Vector2((world.x+39)*1200/19.5f,(view.y*800-150)/.45f)};
            }).ToArray();
            File.WriteAllText(Path.Combine(Output,"interaction-points.json"),JsonUtility.ToJson(new ProjectedPoints{sourceSceneSha256=Hash(scenePath),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs"),points=rows},true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_BAKE_OUTPUT")??Path.Combine(Application.dataPath,"../Baked2D"));
    static string Suffix(string view)=>view=="front"?"":"-"+view;
    static float Yaw(string view)=>view=="side"?-60:view=="back"?180:0;
    public static void RunSquirrel()
    {
        try {
            string previous=Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_BASE_PACK")??throw new Exception("Base pack required."));
            if(Directory.Exists(Output))throw new Exception("Squirrel bake requires a fresh directory.");Directory.CreateDirectory(Output);
            foreach(var file in Directory.GetFiles(previous,"*",SearchOption.AllDirectories)){
                string target=Path.Combine(Output,file.Substring(previous.Length+1));Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target);
            }
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(previous,"actor-frames.json")));
            var scene=EditorSceneManager.OpenScene(manifest.sourceScene);
            var roots=scene.GetRootGameObjects();var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var input=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/actor-bindings.json")).actors.Single(a=>a.name=="Squirrel");
            var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
            var source=transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString()==input.game_object_id);
            var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.transform.position=Vector3.zero;clone.SetActive(true);
            foreach(var root in roots)root.SetActive(false);
            foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
            if(clone.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(r=>!r.sharedMesh || r.bones.Any(b=>!b)))throw new Exception("Squirrel rig is incomplete.");
            Transform Anchor(long id)=>clone.transform.Find(AnimationUtility.CalculateTransformPath(byId[(ulong)id],source))??throw new Exception("Squirrel anchor missing.");
            var hold=Anchor(input.holdId);var face=Anchor(input.faceId);
            var animator=clone.GetComponentInChildren<Animator>(true);animator.enabled=false;
            var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var r in clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)))bounds.Encapsulate(r.bounds);
            var camera=new GameObject("Squirrel camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(.5f,bounds.size.y*.85f);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
            var targetTexture=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=targetTexture;
            var rows=manifest.actions.Where(a=>a.actor!="Squirrel").ToList();Directory.CreateDirectory(Path.Combine(Output,"Squirrel"));
            AnimationMode.StartAnimationMode();
            foreach(var pair in new[]{("idle","Idle"),("serve","ServeProp"),("happy","PositiveReaction"),("negative","NegativeReaction")}){
                string name="ani_sa_Squirrel_"+pair.Item2;var clip=ResolveOverride(animator.runtimeAnimatorController,name);
                if(!clip)clip=AssetDatabase.FindAssets(name+" t:AnimationClip",new[]{"Assets/DuploMarvel"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<AnimationClip>).Where(c=>c && c.name==name).SingleOrDefault();
                if(!clip)throw new Exception("Missing squirrel clip: "+name);
                foreach(var binding in AnimationUtility.GetCurveBindings(clip))if(binding.path!="" && !animator.transform.Find(binding.path))throw new Exception("Missing squirrel clip binding: "+binding.path);
                int count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));
                var row=new ActionFrames{actor="Squirrel",action=pair.Item1,clip=AssetDatabase.GetAssetPath(clip),texture="Squirrel/"+pair.Item1+".png",frames=count,duration=clip.length,unitsPerPixel=camera.orthographicSize*2/256,pivot=Pixel(camera,clone.transform.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
                var sheet=new Texture2D(2048,(count+7)/8*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                for(int frame=0;frame<count;frame++){
                    AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,Mathf.Min(frame/12f,clip.length));AnimationMode.EndSampling();camera.Render();RenderTexture.active=targetTexture;
                    var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();sheet.SetPixels32(frame%8*256,sheet.height-256-frame/8*256,256,256,image.GetPixels32());UnityEngine.Object.DestroyImmediate(image);
                    row.leftHand[frame]=row.rightHand[frame]=row.hold[frame]=Pixel(camera,hold.position);row.face[frame]=Pixel(camera,face.position);
                }
                sheet.Apply();File.WriteAllBytes(Path.Combine(Output,row.texture),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);rows.Add(row);
            }
            AnimationMode.StopAnimationMode();manifest.actions=rows.ToArray();
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(manifest,true));
            File.WriteAllText(Path.Combine(Output,"squirrel-bake.json"),JsonUtility.ToJson(new HeldReceipt{baseManifestSha256=Hash(Path.Combine(previous,"actor-frames.json")),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs")},true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    public static void RunOutcomes()
    {
        try {
            if(Directory.Exists(Output))throw new Exception("Outcome export requires a fresh directory.");Directory.CreateDirectory(Output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var inputs=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/outcome-bindings.json")).actors;
            var rows=new List<PropFrame>();
            foreach(var input in inputs){
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(input.prefab);if(!prefab)throw new Exception("Missing outcome prefab: "+input.prefab);
                var transforms=prefab.GetComponentsInChildren<Transform>(true);
                var source=transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString()==input.game_object_id);
                var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
                foreach(var view in new[]{"front","side","back","held-front","held-side","held-back"})rows.Add(BakeProp(source,input,byId,view));
            }
            File.WriteAllText(Path.Combine(Output,"outcome-props.json"),JsonUtility.ToJson(new Manifest{props=rows.ToArray(),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs"),bindingsSha256=Hash("Assets/Research/outcome-bindings.json")},true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    public static void RunHeldProps()
    {
        try {
            string previous=Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_BASE_PACK")??throw new Exception("Base pack required."));
            if(Directory.Exists(Output))throw new Exception("Held-prop bake requires a fresh directory.");Directory.CreateDirectory(Output);
            foreach(var file in Directory.GetFiles(previous,"*",SearchOption.AllDirectories)){
                string target=Path.Combine(Output,file.Substring(previous.Length+1));Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target);
            }
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(previous,"actor-frames.json")));
            var scene=EditorSceneManager.OpenScene(manifest.sourceScene);
            var roots=scene.GetRootGameObjects();var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
            var byObject=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString());
            var inputs=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/actor-bindings.json")).actors;
            foreach(var root in roots)root.SetActive(false);
            var props=manifest.props.ToList();
            foreach(var binding in inputs.Where(b=>manifest.props.Any(p=>p.name==b.name)))
                foreach(var view in new[]{"front","side","back"})props.Add(BakeProp(byObject[binding.game_object_id],binding,byId,"held-"+view));
            manifest.props=props.ToArray();
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(manifest,true));
            File.WriteAllText(Path.Combine(Output,"held-prop-bake.json"),JsonUtility.ToJson(new HeldReceipt{baseManifestSha256=Hash(Path.Combine(previous,"actor-frames.json")),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs")},true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    [Serializable] public sealed class HeldReceipt {public string baseManifestSha256,bakerSha256;}
    public static void RunCake()
    {
        try {
            if(Directory.Exists(Output))throw new Exception("Cake export requires a fresh directory.");Directory.CreateDirectory(Output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            const string folder="Assets/DuploMarvel/AddressableAssets/Remote/Textures/v20.0/BuildACake";
            var rows=new List<CakeSprite>();
            foreach(var path in Directory.GetFiles(folder,"*.asset",SearchOption.AllDirectories).OrderBy(p=>p,StringComparer.Ordinal)){
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!sprite)continue;
                var renderer=new GameObject(sprite.name).AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));
                var camera=new GameObject("Cake sprite camera").AddComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                var bounds=renderer.bounds;camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.03f;
                camera.transform.position=bounds.center+Vector3.back*10;camera.transform.rotation=Quaternion.identity;
                var target=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                var image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();
                string relative=rows.Count.ToString("D2")+".png";File.WriteAllBytes(Path.Combine(Output,relative),image.EncodeToPNG());
                var pivot=camera.WorldToViewportPoint(renderer.transform.position);
                rows.Add(new CakeSprite{name=sprite.name,source=path.Replace('\\','/'),sourceSha256=Hash(path),texture=relative,pivot=new Vector2(pivot.x,pivot.y),sourcePixelsPerUnit=sprite.pixelsPerUnit});
                camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(renderer.gameObject);
            }
            File.WriteAllText(Path.Combine(Output,"cake-sprites.json"),JsonUtility.ToJson(new CakeSprites{sprites=rows.ToArray(),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs")},true));
            Debug.Log("BIRTHDAY_CAKE_SPRITES "+rows.Count);EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    // Separate append pass keeps the accepted cast/background bake reproducible.
    // Only the exact thirteen prefabs selected by the source spawner are used.
    public static void RunGuests()
    {
        try {
            var previous=Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_BASE_PACK")??throw new Exception("Base pack required."));
            if(Directory.Exists(Output))throw new Exception("Guest bake requires a fresh output directory.");
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(previous,"actor-frames.json")));
            Directory.CreateDirectory(Output);
            foreach(var file in Directory.GetFiles(previous,"*",SearchOption.AllDirectories)){
                var target=Path.Combine(Output,file.Substring(previous.Length+1));Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target);
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var inputs=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/guest-bindings.json")).actors;
            var actions=manifest.actions.Where(a=>!inputs.Any(i=>i.name==a.actor)).ToList();
            foreach(var input in inputs){
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(input.prefab);if(!prefab)throw new Exception("Guest prefab missing: "+input.prefab);
                var originals=prefab.GetComponentsInChildren<Transform>(true);
                var source=originals.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString()==input.game_object_id);
                var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.transform.position=Vector3.zero;clone.transform.rotation=Quaternion.identity;clone.SetActive(true);
                foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
                Transform Anchor(long id){
                    var original=originals.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==(ulong)id);
                    return clone.transform.Find(AnimationUtility.CalculateTransformPath(original,source))??throw new Exception("Guest anchor outside actor.");
                }
                var left=Anchor(input.leftId);var right=Anchor(input.rightId);var hold=Anchor(input.holdId);var face=Anchor(input.faceId);
                var animator=clone.GetComponentInChildren<Animator>(true);if(!animator || !animator.runtimeAnimatorController)throw new Exception("Guest controller missing.");animator.enabled=false;
                var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer));
                var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var r in visible)bounds.Encapsulate(r.bounds);
                var camera=new GameObject("Guest bake camera").AddComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                camera.orthographicSize=Mathf.Max(1,bounds.size.y*.85f);camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
                var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;
                Directory.CreateDirectory(Path.Combine(Output,input.name));
                AnimationMode.StartAnimationMode();
                foreach(var view in new[]{"front","side","back"}){
                    clone.transform.rotation=Quaternion.Euler(0,Yaw(view),0);
                    foreach(var pair in new[]{("idle","anim_human_Idle"),("walk","anim_human_Walk"),("wave","anim_human_Wave"),("happy","anim_human_Happy")}){
                        var clip=ResolveOverride(animator.runtimeAnimatorController,pair.Item2);if(!clip)throw new Exception("Missing guest action "+input.name+"/"+pair.Item1);
                        int count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));
                        var row=new ActionFrames{actor=input.name,action=pair.Item1+Suffix(view),clip=AssetDatabase.GetAssetPath(clip),texture=input.name+"/"+pair.Item1+Suffix(view)+".png",
                            frames=count,duration=clip.length,unitsPerPixel=camera.orthographicSize*2/256,pivot=Pixel(camera,clone.transform.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
                        var sheet=new Texture2D(2048,(count+7)/8*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                        for(int frame=0;frame<count;frame++){
                            AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,Mathf.Min(frame/12f,clip.length));AnimationMode.EndSampling();
                            camera.Render();RenderTexture.active=target;
                            var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
                            sheet.SetPixels32(frame%8*256,sheet.height-256-frame/8*256,256,256,image.GetPixels32());UnityEngine.Object.DestroyImmediate(image);
                            row.leftHand[frame]=Pixel(camera,left.position);row.rightHand[frame]=Pixel(camera,right.position);row.hold[frame]=Pixel(camera,hold.position);row.face[frame]=Pixel(camera,face.position);
                        }
                        sheet.Apply();File.WriteAllBytes(Path.Combine(Output,row.texture),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);actions.Add(row);
                    }
                }
                AnimationMode.StopAnimationMode();
                foreach(var view in new[]{"front","side","back"}){
                    clone.transform.rotation=Quaternion.Euler(0,Yaw(view),0);
                    foreach(var mode in new[]{"hold","carry","pickup","happy-hold"})actions.Add(BakeControllerHold(animator,camera,target,clone.transform,left,right,hold,face,input.name,mode,view));
                }
                Debug.Log("BIRTHDAY_GUEST "+input.name);
                camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);
            }
            manifest.actions=actions.ToArray();manifest.bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs");
            File.Copy("Assets/Research/guest-bindings.json",Path.Combine(Output,"guest-bindings.json"),true);
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(manifest,true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    static readonly string[,] Actions={
        {"idle","anim_human_Idle"},{"walk","anim_human_Walk"},{"run","anim_human_Run"},
        {"happy","anim_human_Happy"},{"negative","anim_human_Negative"},{"wave","anim_human_Wave"},
        {"eat","anim_human_Eat"},{"drink","anim_human_Drink"},{"unwrap","anim_human_Unwrap"},
        {"guitar","anim_human_Guitar"},{"keyboard","anim_human_Keyboard"},{"drums","anim_human_Drums"},
        {"dance","anim_human_Adult_Dancing"},{"jump","anim_human_Jump"},{"land","anim_human_Land"},
        {"smash","ani_hero_smashTwoHands"},{"throw","anim_human_throw1"},
        {"web-right","anim_spiderman_WebShootRightHand"},{"web-left","anim_spiderman_WebShootLeftHand"}
    };
    public static void Run()
    {
        try {
            if(File.Exists(Path.Combine(Output,"actor-frames.json")))throw new Exception("Preserve the completed pack; select a fresh LITTLE_WEEPS_BIRTHDAY_BAKE_OUTPUT.");
            Directory.CreateDirectory(Output);
            const string source="Assets/DuploMarvel/PlayPacks/HulksBirthdayParty/Scenes/HBP_Main.unity";
            var scene=EditorSceneManager.OpenScene(source);
            var originalRoots=scene.GetRootGameObjects();
            var transforms=originalRoots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var bindings=JsonUtility.FromJson<ActorInputs>(File.ReadAllText("Assets/Research/actor-bindings.json")).actors;
            var byId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId);
            var byObjectId=transforms.ToDictionary(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId.ToString());
            BakeWorld(transforms,bindings.Select(b=>byObjectId[b.game_object_id]).ToArray());
            var manifest=new List<ActionFrames>();
            var missing=new List<string>();
            foreach(string actorName in new[]{"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW","GreenGoblin_NewNew","BlackCat_NewNew","Electro_NewNew_BAP","DocOck_NewNEW"}) {
                var binding=bindings.Single(b=>b.name==actorName);
                var actor=byObjectId[binding.game_object_id];
                var clone=UnityEngine.Object.Instantiate(actor.gameObject);clone.name=actorName;
                clone.transform.position=Vector3.zero;clone.SetActive(true);
                foreach(var root in originalRoots)root.SetActive(false);
                foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                foreach(var renderer in clone.GetComponentsInChildren<Renderer>(true))Recover(renderer);
                var animator=clone.GetComponentInChildren<Animator>(true);animator.enabled=false;
                var clips=animator.runtimeAnimatorController.animationClips.Distinct().ToArray();
                var camera=new GameObject("2D bake camera").AddComponent<Camera>();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;
                var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)).ToArray();
                var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var r in visible)bounds.Encapsulate(r.bounds);
                camera.orthographicSize=Mathf.Max(1,bounds.size.y*.85f);
                camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
                var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;
                Transform Anchor(long id){if(id==0)throw new Exception("Missing source anchor "+actorName);var sourceAnchor=byId[(ulong)id];var result=clone.transform.Find(AnimationUtility.CalculateTransformPath(sourceAnchor,actor));if(!result)throw new Exception("Anchor outside actor "+actorName);return result;}
                var left=Anchor(binding.leftId);var right=Anchor(binding.rightId);var hold=Anchor(binding.holdId);var face=Anchor(binding.faceId);
                var originalRotation=clone.transform.rotation;
                AnimationMode.StartAnimationMode();
                foreach(var view in new[]{"front","side","back"}){
                clone.transform.rotation=originalRotation*Quaternion.Euler(0,Yaw(view),0);
                for(int action=0;action<Actions.GetLength(0);action++){
                    if(view!="front" && !new[]{"idle","walk","run","happy","negative"}.Contains(Actions[action,0]))continue;
                    if(Actions[action,0]=="smash" && actorName!="Hulk_NewNew")continue;
                    if(Actions[action,0].StartsWith("web-") && !new[]{"Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"}.Contains(actorName))continue;
                    var clip=clips.FirstOrDefault(c=>c.name==Actions[action,1])??ResolveOverride(animator.runtimeAnimatorController,Actions[action,1]);
                    if(!clip){missing.Add(actorName+"/"+Actions[action,0]);continue;}
                    float originalFraming=camera.orthographicSize;
                    // The source smash raises a hand past the ordinary idle
                    // bounds. Keep physical scale through unitsPerPixel while
                    // giving that action sufficient transparent capture margin.
                    if(Actions[action,0]=="smash")camera.orthographicSize*=1.2f;
                    var count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));
                    var sheet=new Texture2D(8*256,Mathf.CeilToInt(count/8f)*256,TextureFormat.RGBA32,false);
                    sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                    var row=new ActionFrames{actor=actorName,action=Actions[action,0]+Suffix(view),clip=AssetDatabase.GetAssetPath(clip),frames=count,duration=clip.length,
                        texture=actorName+"/"+Actions[action,0]+Suffix(view)+".png",unitsPerPixel=camera.orthographicSize*2/256,
                        pivot=Pixel(camera,clone.transform.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
                    for(int frame=0;frame<count;frame++){
                        AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,clip.length>0?Math.Min(clip.length-.00001f,frame/12f):0);AnimationMode.EndSampling();
                        camera.Render();RenderTexture.active=rt;
                        var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
                        sheet.SetPixels32(frame%8*256,(count+7)/8*256-256-frame/8*256,256,256,image.GetPixels32());
                        row.leftHand[frame]=left?Pixel(camera,left.position):row.pivot;row.rightHand[frame]=right?Pixel(camera,right.position):row.pivot;
                        row.hold[frame]=Pixel(camera,hold.position);
                        row.face[frame]=Pixel(camera,face.position);
                        UnityEngine.Object.DestroyImmediate(image);
                    }
                    sheet.Apply();var path=Path.Combine(Output,row.texture);Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,sheet.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(sheet);manifest.Add(row);
                    camera.orthographicSize=originalFraming;
                }
                }
                AnimationMode.StopAnimationMode();camera.targetTexture=null;RenderTexture.active=null;
                if(new[]{"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"}.Contains(actorName)) {
                    foreach(var view in new[]{"front","side","back"}){
                        clone.transform.rotation=originalRotation*Quaternion.Euler(0,Yaw(view),0);
                        foreach(var mode in new[]{"hold","carry","pickup","happy-hold"})manifest.Add(BakeControllerHold(animator,camera,rt,clone.transform,left,right,hold,face,actorName,mode,view));
                    }
                }
                UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);
                Debug.Log("BIRTHDAY_BAKE actor="+actorName+" actions="+Actions.GetLength(0));
            }
            var props=new List<PropFrame>();
            foreach(var binding in bindings.Where(b=>!manifest.Any(m=>m.actor==b.name)))foreach(var view in new[]{"front","side","back"})props.Add(BakeProp(byObjectId[binding.game_object_id],binding,byId,view));
            var result=new Manifest{sourceScene=source,sourceSceneSha256=Hash(source),bakerSha256=Hash("Assets/Editor/RecoveredBirthdayBake.cs"),bindingsSha256=Hash("Assets/Research/actor-bindings.json"),unityVersion=Application.unityVersion,
                actions=manifest.ToArray(),props=props.ToArray(),missingActions=missing.ToArray(),limits=new[]{
                "Clip sampling is not a recovered action state machine.","Controller holding/pickup/happy layers are included for four companions; complete activity behavior and cancellation remain separate work.",
                "Attachment tracks use exact original scene IDs. Actor and static prop baking does not implement activities."}};
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(result,true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    static Vector2 Pixel(Camera camera,Vector3 point){var p=camera.WorldToViewportPoint(point);return new Vector2(p.x*256,p.y*256);}
    static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
    static ActionFrames BakeControllerHold(Animator animator,Camera camera,RenderTexture target,Transform root,Transform left,Transform right,Transform hold,Transform face,string actor,string mode,string view)
    {
        bool walking=mode=="carry",pickup=mode=="pickup",happy=mode=="happy-hold";
        var graph=PlayableGraph.Create("Recovered holding controller");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        animator.enabled=true;animator.applyRootMotion=false;animator.fireEvents=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var controller=AnimatorControllerPlayable.Create(graph,animator.runtimeAnimatorController);
        var output=AnimationPlayableOutput.Create(graph,"Recovered controller",animator);output.SetSourcePlayable(controller);
        graph.Play();graph.Evaluate(0);
        controller.SetBool("isHolding",true);controller.SetFloat("armBlend",pickup?0:1);
        // Original Default_holdPreset: arms=3, vertical=50%, horizontal=0%,
        // spine=100%. HoldPresetSo.get_HoldPresets divides each by 100 in
        // independently mapped native code at 0x3947710.
        controller.SetFloat("holdLeftArmVertical",.5f);controller.SetFloat("holdLeftArmHorizontal",0);
        controller.SetFloat("holdRightArmVertical",.5f);controller.SetFloat("holdRightArmHorizontal",0);controller.SetFloat("holdSpine",1);
        controller.SetBool("isWalking",walking);controller.SetFloat("walkSpeed",walking?1:0);graph.Evaluate(pickup?0:.5f);
        if(happy){controller.SetTrigger("Happy");graph.Evaluate(0);}
        int count=happy?30:pickup?4:walking?10:1;
        var row=new ActionFrames{actor=actor,action=mode+Suffix(view),clip="controller:"+AssetDatabase.GetAssetPath(animator.runtimeAnimatorController),texture=actor+"/"+mode+Suffix(view)+".png",
            frames=count,duration=count==1?0:count/12f,unitsPerPixel=camera.orthographicSize*2/256,pivot=Pixel(camera,root.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
        var sheet=new Texture2D(2048,(count+7)/8*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
        camera.targetTexture=target;
        for(int frame=0;frame<count;frame++){
            if(pickup)controller.SetFloat("armBlend",frame/(float)(count-1));
            if(frame>0)graph.Evaluate(1/12f);
            camera.Render();RenderTexture.active=target;
            var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
            sheet.SetPixels32(frame%8*256,sheet.height-256-frame/8*256,256,256,image.GetPixels32());
            row.leftHand[frame]=Pixel(camera,left.position);row.rightHand[frame]=Pixel(camera,right.position);row.hold[frame]=Pixel(camera,hold.position);
            row.face[frame]=Pixel(camera,face.position);
            UnityEngine.Object.DestroyImmediate(image);
        }
        sheet.Apply();File.WriteAllBytes(Path.Combine(Output,row.texture),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
        camera.targetTexture=null;RenderTexture.active=null;graph.Destroy();animator.enabled=false;return row;
    }
    static AnimationClip ResolveOverride(RuntimeAnimatorController runtime,string name)
    {
        if(runtime is AnimatorOverrideController overrides){
            var entries=new List<KeyValuePair<AnimationClip,AnimationClip>>();overrides.GetOverrides(entries);
            var entry=entries.FirstOrDefault(e=>e.Key.name==name);
            if(entry.Key)return entry.Value?entry.Value:entry.Key;
            var inherited=ResolveOverride(overrides.runtimeAnimatorController,name);
            if(inherited){entry=entries.FirstOrDefault(e=>e.Key==inherited);return entry.Value?entry.Value:inherited;}
        }
        return runtime.animationClips.FirstOrDefault(c=>c.name==name);
    }
    static void BakeWorld(Transform[] transforms,Transform[] actors)
    {
        foreach(var t in transforms){
            foreach(var renderer in t.GetComponents<Renderer>())Recover(renderer);
            foreach(var canvas in t.GetComponents<Canvas>())canvas.enabled=false;
        }
        var camera=transforms.Select(t=>t.GetComponent<Camera>()).First(c=>c && c.name=="Main Camera");
        var originalPosition=camera.transform.position;var originalRotation=camera.transform.rotation;
        var names=new[]{"BuildACakeActivity","HBPBandActivity","BouncyCastle","IntroActivity","Hulk_NewNew","Spiderman_NewNEW","Gwen_NewNEW","Spin_NewNEW","Battery","PresentActor","PresentActor_1","PresentActor_2","PresentActor_3","PresentActor_4"};
        var geometry=new Geometry{cameraPosition=originalPosition,cameraRotation=originalRotation.eulerAngles,fieldOfView=camera.fieldOfView,
            landmarks=transforms.Where(t=>names.Contains(t.name)).Select(t=>new Landmark{name=t.name,position=t.position}).ToArray()};
        File.WriteAllText(Path.Combine(Output,"world-geometry.json"),JsonUtility.ToJson(geometry,true));
        foreach(var actor in actors)actor.gameObject.SetActive(false);
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.73f,.88f,.98f);camera.allowHDR=false;
        var rt=new RenderTexture(1200,800,24);camera.targetTexture=rt;
        // A fixed orthographic projection yields four genuinely adjoining 2D
        // tiles. Reframing perspective shots would introduce seams/parallax jumps.
        camera.orthographic=true;camera.orthographicSize=6.5f;
        for(int i=0;i<4;i++){
            camera.transform.SetPositionAndRotation(new Vector3(-29.25f+i*19.5f,10,originalPosition.z),originalRotation);
            camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1200,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,800),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(Output,"world-"+i+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
        }
        camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);
        camera.transform.SetPositionAndRotation(originalPosition,originalRotation);
    }
    static PropFrame BakeProp(Transform source,ActorInput binding,Dictionary<ulong,Transform> byId,string view)
    {
        var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.transform.position=Vector3.zero;clone.SetActive(true);
        Transform Anchor(long id)=>id==0?clone.transform:clone.transform.Find(AnimationUtility.CalculateTransformPath(byId[(ulong)id],source));
        bool held=view.StartsWith("held-",StringComparison.Ordinal);string facing=held?view.Substring(5):view;
        if(held){
            // Ground props do not share the character's orientation. Align the
            // source prop peg to the character's upright holding peg, including
            // the prop anchor's own rotation, before projecting either pivot.
            var relative=Quaternion.Inverse(clone.transform.rotation)*Anchor(binding.holdId).rotation;
            clone.transform.rotation=Quaternion.Euler(0,180+Yaw(facing),0)*Quaternion.Inverse(relative);
        }else clone.transform.rotation*=Quaternion.Euler(0,Yaw(facing),0);
        foreach(var renderer in clone.GetComponentsInChildren<Renderer>(true))Recover(renderer);
        var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)).ToArray();
        var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var renderer in visible)bounds.Encapsulate(renderer.bounds);
        var camera=new GameObject("Recovered prop camera").AddComponent<Camera>();camera.orthographic=true;
        camera.orthographicSize=Mathf.Max(.1f,Mathf.Max(bounds.size.y,bounds.size.x)*.7f);
        camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
        var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
        var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
        var row=new PropFrame{name=binding.name,view=view,texture="Props/"+binding.name+(held?"-held":"")+Suffix(facing)+".png",unitsPerPixel=camera.orthographicSize*2/256,
            pivot=Pixel(camera,Anchor(binding.baseId).position),holdPivot=Pixel(camera,Anchor(binding.holdId).position),sourceBaseId=binding.baseId,sourceHoldId=binding.holdId};
        var file=Path.Combine(Output,row.texture);Directory.CreateDirectory(Path.GetDirectoryName(file));File.WriteAllBytes(file,image.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);
        return row;
    }
    public static void Recover(Renderer renderer)
    {
        if(renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer || renderer.name.IndexOf("shadow",StringComparison.OrdinalIgnoreCase)>=0){renderer.enabled=false;return;}
        if(renderer is SpriteRenderer){renderer.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));return;}
        var brick=renderer.GetComponent<Lego.LegoBrickMaterial>();
        if(!brick){renderer.sharedMaterials=renderer.sharedMaterials.Select(RecoveredWorldAssetProbe.ReadMaterial).ToArray();return;}
        var material=new Material(Shader.Find("Research/RecoveredBrick"));material.SetColor("_Color",brick.color);
        if(brick.decal){material.mainTexture=brick.decal;material.mainTextureScale=brick.decalTiling;material.mainTextureOffset=brick.decalOffset;material.SetFloat("_UseDecal",1);}
        renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();
    }
}
