// Research-only conversion from recovered rigs to the existing 2D game pipeline.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class RecoveredBirthdayBake
{
    [Serializable] public sealed class ActorInput {public string name,game_object_id;public long leftId,rightId,holdId,baseId,faceId;}
    [Serializable] public sealed class ActorInputs {public ActorInput[] actors;}
    [Serializable] public sealed class ActionFrames {public string actor,action,clip,texture;public int frames,columns=8,size=256;public float duration,unitsPerPixel;public Vector2[] rightHand,leftHand,hold,face;public Vector2 pivot;}
    [Serializable] public sealed class PropFrame {public string name,texture;public float unitsPerPixel;public Vector2 pivot,holdPivot;public long sourceBaseId,sourceHoldId;}
    [Serializable] public sealed class Manifest {public string sourceScene;public int fps=12;public ActionFrames[] actions;public PropFrame[] props;public string[] missingActions,limits;}
    [Serializable] public sealed class Landmark {public string name;public Vector3 position;}
    [Serializable] public sealed class Geometry {public Vector3 cameraPosition,cameraRotation;public float fieldOfView,tileWorldWidth=19.5f,pixelsPerUnit=1200/19.5f,worldMinX=-39;public Landmark[] landmarks;}
    static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Baked2D"));
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
                AnimationMode.StartAnimationMode();
                for(int action=0;action<Actions.GetLength(0);action++){
                    if(Actions[action,0]=="smash" && actorName!="Hulk_NewNew")continue;
                    if(Actions[action,0].StartsWith("web-") && !new[]{"Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"}.Contains(actorName))continue;
                    var clip=clips.FirstOrDefault(c=>c.name==Actions[action,1])??ResolveOverride(animator.runtimeAnimatorController,Actions[action,1]);
                    if(!clip){missing.Add(actorName+"/"+Actions[action,0]);continue;}
                    var count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));
                    var sheet=new Texture2D(8*256,Mathf.CeilToInt(count/8f)*256,TextureFormat.RGBA32,false);
                    sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
                    var row=new ActionFrames{actor=actorName,action=Actions[action,0],clip=AssetDatabase.GetAssetPath(clip),frames=count,duration=clip.length,
                        texture=actorName+"/"+Actions[action,0]+".png",unitsPerPixel=camera.orthographicSize*2/256,
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
                }
                AnimationMode.StopAnimationMode();camera.targetTexture=null;RenderTexture.active=null;
                if(new[]{"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW"}.Contains(actorName)) {
                    manifest.Add(BakeControllerHold(animator,camera,rt,clone.transform,left,right,hold,face,actorName,false));
                    manifest.Add(BakeControllerHold(animator,camera,rt,clone.transform,left,right,hold,face,actorName,true));
                }
                UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);
                Debug.Log("BIRTHDAY_BAKE actor="+actorName+" actions="+Actions.GetLength(0));
            }
            var props=new List<PropFrame>();
            foreach(var binding in bindings.Where(b=>!manifest.Any(m=>m.actor==b.name)))props.Add(BakeProp(byObjectId[binding.game_object_id],binding,byId));
            var result=new Manifest{sourceScene=source,actions=manifest.ToArray(),props=props.ToArray(),missingActions=missing.ToArray(),limits=new[]{
                "Clip sampling is not a recovered action state machine.","Holding layers, face changes and cancellation remain separate reconstruction work.",
                "Attachment tracks use exact original scene IDs. Actor and static prop baking does not implement activities."}};
            File.WriteAllText(Path.Combine(Output,"actor-frames.json"),JsonUtility.ToJson(result,true));
            EditorApplication.Exit(0);
        }catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    static Vector2 Pixel(Camera camera,Vector3 point){var p=camera.WorldToViewportPoint(point);return new Vector2(p.x*256,p.y*256);}
    static ActionFrames BakeControllerHold(Animator animator,Camera camera,RenderTexture target,Transform root,Transform left,Transform right,Transform hold,Transform face,string actor,bool walking)
    {
        var graph=PlayableGraph.Create("Recovered holding controller");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        animator.enabled=true;animator.applyRootMotion=false;animator.fireEvents=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var controller=AnimatorControllerPlayable.Create(graph,animator.runtimeAnimatorController);
        var output=AnimationPlayableOutput.Create(graph,"Recovered controller",animator);output.SetSourcePlayable(controller);
        graph.Play();graph.Evaluate(0);
        controller.SetBool("isHolding",true);controller.SetFloat("armBlend",1);
        // Original Default_holdPreset: arms=3, vertical=50%, horizontal=0%,
        // spine=100%. HoldPresetSo.get_HoldPresets divides each by 100 in
        // independently mapped native code at 0x3947710.
        controller.SetFloat("holdLeftArmVertical",.5f);controller.SetFloat("holdLeftArmHorizontal",0);
        controller.SetFloat("holdRightArmVertical",.5f);controller.SetFloat("holdRightArmHorizontal",0);controller.SetFloat("holdSpine",1);
        controller.SetBool("isWalking",walking);controller.SetFloat("walkSpeed",walking?1:0);graph.Evaluate(.5f);
        int count=walking?10:1;
        var row=new ActionFrames{actor=actor,action=walking?"carry":"hold",clip="controller:"+AssetDatabase.GetAssetPath(animator.runtimeAnimatorController),texture=actor+"/"+(walking?"carry":"hold")+".png",
            frames=count,duration=walking?10/12f:0,unitsPerPixel=camera.orthographicSize*2/256,pivot=Pixel(camera,root.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
        var sheet=new Texture2D(2048,(count+7)/8*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
        camera.targetTexture=target;
        for(int frame=0;frame<count;frame++){
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
    static PropFrame BakeProp(Transform source,ActorInput binding,Dictionary<ulong,Transform> byId)
    {
        var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.transform.position=Vector3.zero;clone.SetActive(true);
        foreach(var renderer in clone.GetComponentsInChildren<Renderer>(true))Recover(renderer);
        var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)).ToArray();
        var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var renderer in visible)bounds.Encapsulate(renderer.bounds);
        var camera=new GameObject("Recovered prop camera").AddComponent<Camera>();camera.orthographic=true;
        camera.orthographicSize=Mathf.Max(.1f,Mathf.Max(bounds.size.y,bounds.size.x)*.7f);
        camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);camera.transform.LookAt(bounds.center);
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
        var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
        var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
        Transform Anchor(long id)=>id==0?clone.transform:clone.transform.Find(AnimationUtility.CalculateTransformPath(byId[(ulong)id],source));
        var row=new PropFrame{name=source.name,texture="Props/"+source.name+".png",unitsPerPixel=camera.orthographicSize*2/256,
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
