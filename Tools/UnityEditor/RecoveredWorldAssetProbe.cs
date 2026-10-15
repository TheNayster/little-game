// Copied only into an isolated research project. Never attach to production scenes.
// Uses imported native Unity assets and handwritten rendering code; no recovered
// third-party executable assemblies are required or executed.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RecoveredWorldAssetProbe
{
    [Serializable] public sealed class ClipRow { public string name, asset; public float seconds; public int curves, events; }
    [Serializable] public sealed class ActorRow {
        public string name, controller, baseController; public string[] parameters, states;
        public ClipRow[] clips; public int skinnedRenderers, missingMeshes, missingBones;
    }
    [Serializable] public sealed class Report { public ActorRow[] actors; public int cakeSprites; public string[] errors; }
    static readonly List<string> errors = new List<string>();
    static string Output => Path.GetFullPath(Path.Combine(Application.dataPath,"../Evidence"));

    public static void Run()
    {
        try {
            Directory.CreateDirectory(Output);
            var scene=EditorSceneManager.OpenScene("Assets/DuploMarvel/PlayPacks/HulksBirthdayParty/Scenes/HBP_Main.unity");
            var transforms=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var rows=new List<ActorRow>();
            foreach(string name in new[]{"Hulk_NewNew","Spiderman_NewNEW","Spin_NewNEW","Gwen_NewNEW","GreenGoblin_NewNew","BlackCat_NewNew","Electro_NewNew_BAP","DocOck_NewNEW"}) {
                var actor=transforms.SingleOrDefault(t=>t.name==name);
                if(!actor){errors.Add("Missing actor: "+name);continue;}
                var animator=actor.GetComponentInChildren<Animator>(true);
                if(!animator || !animator.runtimeAnimatorController){errors.Add("Missing controller: "+name);continue;}
                RuntimeAnimatorController runtime=animator.runtimeAnimatorController;
                while(runtime is AnimatorOverrideController overrides)runtime=overrides.runtimeAnimatorController;
                var controller=runtime as AnimatorController;
                var skins=actor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var clips=animator.runtimeAnimatorController.animationClips.Distinct().ToArray();
                var row=new ActorRow {name=name,controller=AssetDatabase.GetAssetPath(animator.runtimeAnimatorController),baseController=AssetDatabase.GetAssetPath(runtime),
                    parameters=controller?controller.parameters.Select(p=>p.name+":"+p.type).ToArray():Array.Empty<string>(),
                    states=controller?controller.layers.SelectMany(l=>States(l.stateMachine,l.name)).ToArray():Array.Empty<string>(),
                    clips=clips.Select(c=>new ClipRow{name=c.name,asset=AssetDatabase.GetAssetPath(c),seconds=c.length,
                        curves=AnimationUtility.GetCurveBindings(c).Length,events=AnimationUtility.GetAnimationEvents(c).Length}).ToArray(),
                    skinnedRenderers=skins.Length,missingMeshes=skins.Count(s=>!s.sharedMesh),missingBones=skins.Sum(s=>s.bones.Count(b=>!b))};
                rows.Add(row);
                if(row.missingMeshes>0 || row.missingBones>0 || row.clips.Length==0)errors.Add("Incomplete rig: "+name);
            }
            // These are the exact original source references recovered from the catalog.
            var cakes=AssetDatabase.FindAssets("t:Sprite",new[]{"Assets/DuploMarvel/AddressableAssets/Remote/Textures/v20.0/BuildACake"});
            var report=new Report{actors=rows.ToArray(),cakeSprites=cakes.Length,errors=errors.ToArray()};
            File.WriteAllText(Path.Combine(Output,"imported-asset-audit.json"),JsonUtility.ToJson(report,true));
            Debug.Log("RECOVERED_ASSET_AUDIT actors="+rows.Count+" clips="+rows.Sum(r=>r.clips.Length)+" errors="+errors.Count);
            // Render a genuinely sampled original clip; files and a controller alone
            // cannot establish that extracted curves bind to the recovered rig.
            RenderActor(transforms,"Hulk_NewNew");
            RenderActor(transforms,"Spiderman_NewNEW");
            var rules=typeof(RecoveredWorldAssetProbe).Assembly.GetType("HulkBirthdayRuleChecks");
            if(rules!=null){rules.GetMethod("Run").Invoke(null,null);File.WriteAllText(Path.Combine(Output,"birthday-rules.txt"),"PASS: focused authority fixtures executed in Unity Editor. Four actual clients not tested.\n");}
            EditorApplication.Exit(errors.Count==0?0:1);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }

    static IEnumerable<string> States(AnimatorStateMachine machine,string path)
    {
        foreach(var s in machine.states)yield return path+"/"+s.state.name+" => "+(s.state.motion?s.state.motion.name:"<no motion>");
        foreach(var child in machine.stateMachines)foreach(var state in States(child.stateMachine,path+"/"+child.stateMachine.name))yield return state;
    }

    static void RenderActor(Transform[] transforms,string name)
    {
        var original=transforms.Single(t=>t.name==name);
        var clone=UnityEngine.Object.Instantiate(original.gameObject);
        clone.name=name;clone.transform.position=Vector3.zero;clone.SetActive(true);
        foreach(var t in clone.GetComponentsInChildren<Transform>(true)){
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            foreach(var renderer in t.GetComponents<Renderer>()) {
                if(renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer){renderer.enabled=false;continue;}
                if(renderer.name.IndexOf("shadow",StringComparison.OrdinalIgnoreCase)>=0){renderer.enabled=false;continue;}
                if(renderer is SpriteRenderer){renderer.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));continue;}
                var brick=t.GetComponent<Lego.LegoBrickMaterial>();
                if(!brick){renderer.sharedMaterials=renderer.sharedMaterials.Select(ReadMaterial).ToArray();continue;}
                var material=new Material(Shader.Find("Research/RecoveredBrick"));
                material.SetColor("_Color",brick?brick.color:Color.white);
                if(brick && brick.decal){material.mainTexture=brick.decal;material.mainTextureScale=brick.decalTiling;material.mainTextureOffset=brick.decalOffset;material.SetFloat("_UseDecal",1);}
                renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();
            }
        }
        foreach(var root in original.gameObject.scene.GetRootGameObjects())if(root!=clone)root.SetActive(false);
        // Activity scripts choose source clip/transition combinations at runtime.
        // This test samples curves directly and does not assert full action behavior.
        var animator=clone.GetComponentInChildren<Animator>(true);
        animator.enabled=false;
        var clips=animator.runtimeAnimatorController.animationClips.Distinct().Where(c=>c.length>.05f).ToArray();
        var selected=clips.Where(c=>new[]{"walk","lift","happy","idle","dance","jump"}.Any(k=>c.name.ToLowerInvariant().Contains(k))).Take(12).ToArray();
        var camera=new GameObject("Isolated actor camera").AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
        camera.orthographic=true;
        var renders=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled && !(r is ParticleSystemRenderer)).ToArray();
        Bounds bounds=new Bounds(clone.transform.position,Vector3.zero);foreach(var r in renders)bounds.Encapsulate(r.bounds);
        camera.orthographicSize=Mathf.Max(1,bounds.size.y*.72f);
        camera.transform.position=bounds.center+new Vector3(0,bounds.size.y*.25f,12);
        camera.transform.LookAt(bounds.center);
        string dir=Path.Combine(Output,name);Directory.CreateDirectory(dir);
        AnimationMode.StartAnimationMode();
        foreach(var clip in selected){
            for(int frame=0;frame<4;frame++) {
                AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,clip.length*(frame/4f));AnimationMode.EndSampling();
                Capture(camera,Path.Combine(dir,Safe(clip.name)+"_"+frame+".png"));
            }
        }
        AnimationMode.StopAnimationMode();
        UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(clone);
        foreach(var root in original.gameObject.scene.GetRootGameObjects())root.SetActive(true);
    }
    static string Safe(string text)=>new string(text.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c).ToArray());
    public static Material ReadMaterial(Material original)
    {
        var material=new Material(Shader.Find("Research/RecoveredBrick"));
        if(!original)return material;
        var serialized=new SerializedObject(original);
        var colors=serialized.FindProperty("m_SavedProperties.m_Colors");
        foreach(string key in new[]{"_BASE_COLOR","_BaseColor","_Color"})
            for(int i=0;i<(colors?.arraySize??0);i++){
                var entry=colors.GetArrayElementAtIndex(i);
                if(entry.FindPropertyRelative("first").stringValue==key)material.SetColor("_Color",entry.FindPropertyRelative("second").colorValue);
            }
        var textures=serialized.FindProperty("m_SavedProperties.m_TexEnvs");
        foreach(string key in new[]{"_BASE_COLOR_MAP","_BaseMap","_MainTex","_DecalMap"})
            for(int i=0;i<(textures?.arraySize??0);i++){
                var entry=textures.GetArrayElementAtIndex(i);if(entry.FindPropertyRelative("first").stringValue!=key)continue;
                var value=entry.FindPropertyRelative("second");var texture=value.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;
                if(!texture)continue;
                material.mainTexture=texture;material.mainTextureScale=value.FindPropertyRelative("m_Scale").vector2Value;material.mainTextureOffset=value.FindPropertyRelative("m_Offset").vector2Value;
                material.SetFloat("_UseDecal",1);
            }
        return material;
    }
    static void Capture(Camera camera,string path) {
        var rt=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32);
        var image=new Texture2D(512,512,TextureFormat.RGBA32,false);
        camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
    }
}
