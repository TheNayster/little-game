// BLU-01. Asset-only recovery: original scenes/material data, handwritten capture.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class MarketsAssetBake
{
    public static void RunPonyApple()
    {
        try {
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_BAKE");
            if(string.IsNullOrEmpty(output)||Directory.Exists(output))throw new Exception("Choose a fresh Markets pony prop output.");Directory.CreateDirectory(output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            const string path="Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Prefabs/Pony_Scene2/pfb_mkd_PonyApple.prefab";
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!source)throw new Exception(path);
            var clone=UnityEngine.Object.Instantiate(source);clone.SetActive(true);clone.transform.position=Vector3.zero;
            foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
            var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(visible.Length==0)throw new Exception("Missing pony apple geometry");
            var bounds=visible[0].bounds;foreach(var r in visible)bounds.Encapsulate(r.bounds);
            var camera=new GameObject("Markets pony apple camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.1f;camera.transform.position=bounds.center+Vector3.back*100;camera.nearClipPlane=.01f;camera.farClipPlane=300;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            Capture(camera,Path.Combine(output,"pony-apple.png"),256,256,true);
            File.WriteAllText(Path.Combine(output,"pony-apple.json"),JsonUtility.ToJson(new Receipt{scene=path,sceneHash=Hash(path),renderers=visible.Length,min=bounds.min,max=bounds.max},true));
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);return;}
        EditorApplication.Exit(0);
    }
    public static void RunIceCreamView()
    {
        try {
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_BAKE");
            if(string.IsNullOrEmpty(output)||Directory.Exists(output))throw new Exception("Choose a fresh Markets ice cream output.");
            Directory.CreateDirectory(output);
            const string scene="Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Scenes/SUB_SCN_03_MarketDay_Scene3.unity";
            var loaded=EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            var shell=EditorSceneManager.OpenScene("Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Scenes/SCN_00_MarketDay.unity",OpenSceneMode.Additive);
            var backdrop=shell.GetRootGameObjects().Single(r=>r.name=="Background");
            // The pack shell supplies the shared backdrop; the runtime places
            // it at the selected subscene. Reproduce that composition as data.
            backdrop.transform.position=new Vector3(100,0,0);
            var transforms=loaded.GetRootGameObjects().Concat(shell.GetRootGameObjects()).SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            foreach(var t in transforms)if(t.name.IndexOf("minifig",StringComparison.OrdinalIgnoreCase)>=0)t.gameObject.SetActive(false);
            foreach(var canvas in transforms.Select(t=>t.GetComponent<Canvas>()).Where(c=>c))canvas.gameObject.SetActive(false);
            foreach(var renderer in transforms.Select(t=>t.GetComponent<Renderer>()).Where(r=>r))Recover(renderer);
            // The shell's runtime parallax/floor setup is not executable in
            // this data-only probe. Compose a neutral grass plane and two of
            // its original tree drawings behind the recovered Scene3 props.
            // This staging is an explicit 2D adaptation, not original runtime.
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Markets capture grass staging";floor.transform.position=new Vector3(100,-.04f,0);floor.transform.localScale=new Vector3(20,1,20);
            var grass=new Material(Shader.Find("Unlit/Color"));grass.color=new Color(.63f,.77f,.28f);floor.GetComponent<Renderer>().sharedMaterial=grass;
            var tree=transforms.First(t=>t.name=="tex_emb_bg_Tree_07"&&t.GetComponent<Renderer>());
            for(int i=0;i<2;i++){
                var clone=UnityEngine.Object.Instantiate(tree.gameObject);clone.name="Markets source tree staging "+i;clone.SetActive(true);clone.transform.SetParent(null,true);
                var renderer=clone.GetComponent<Renderer>();clone.transform.localScale*=4.7f/renderer.bounds.size.y;
                var bounds=renderer.bounds;clone.transform.position+=new Vector3(i==0?94.5f:106.8f,.1f,7)-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            }
            var camera=loaded.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Camera>(true)).Single(c=>c.name=="Main Camera");
            camera.gameObject.SetActive(true);camera.enabled=true;camera.transform.position=new Vector3(100,5.15f,-20);camera.transform.rotation=Quaternion.Euler(9,0,0);camera.fieldOfView=20;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.64f,.87f,.98f);camera.cullingMask=~0;
            Capture(camera,Path.Combine(output,"icecream.png"));
            File.WriteAllText(Path.Combine(output,"icecream.json"),JsonUtility.ToJson(new Receipt{scene=scene,sceneHash=Hash(scene),unity=Application.unityVersion},true));
        } catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);return;}
        EditorApplication.Exit(0);
    }
    public static void RunViews()
    {
        try {
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_BAKE");
            if(string.IsNullOrEmpty(output)||Directory.Exists(output))throw new Exception("Choose a fresh Markets view output.");Directory.CreateDirectory(output);
            string prefix="Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Scenes/";
            foreach(var key in new[]{"smoothie","flowers","bubbles","ponies","music"}){
                string scene=prefix+(key=="ponies"?"SUB_SCN_02_MarketDay_Scene2":key=="music"?"SUB_SCN_04_MarketDay_Scene4":"SUB_SCN_01_MarketDay_Scene1")+".unity";
                var loaded=EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);var transforms=loaded.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                var roots=new Dictionary<string,long>{{"smoothie",3405},{"flowers",2244},{"bubbles",3008}};
                if(roots.ContainsKey(key))foreach(var pair in roots){var root=transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==(ulong)pair.Value);root.gameObject.SetActive(key==pair.Key);}
                foreach(var t in transforms)if(t.name.Contains("Minifig")||t.name.Contains("minifig"))t.gameObject.SetActive(false);
                if(key=="ponies"){
                    transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==1223).gameObject.SetActive(false);
                    transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==1262).gameObject.SetActive(true);
                    // Ponies are sampled separately so care and makeovers remain visible state.
                    foreach(var t in transforms)if(t.name=="pfb_emb_Pony_Grey"||t.name=="pfb_emb_Pony_Brown"||t.name=="PonyMakeover"||t.name=="Hearts"||t.name=="Hearts_1")t.gameObject.SetActive(false);
                }
                foreach(var canvas in transforms.Select(t=>t.GetComponent<Canvas>()).Where(c=>c))canvas.gameObject.SetActive(false);
                foreach(var r in transforms.Select(t=>t.GetComponent<Renderer>()).Where(r=>r))Recover(r);
                var camera=transforms.Select(t=>t.GetComponent<Camera>()).First(c=>c&&c.name!="UI_Camera");camera.gameObject.SetActive(true);camera.enabled=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.64f,.87f,.98f);camera.cullingMask=~0;
                if(roots.ContainsKey(key)){camera.transform.position=new Vector3(0,4.57f,-17.13f);camera.transform.rotation=Quaternion.Euler(10.483085f,0,0);camera.fieldOfView=20;}
                Capture(camera,Path.Combine(output,key+".png"));
                File.WriteAllText(Path.Combine(output,key+".json"),JsonUtility.ToJson(new Receipt{scene=scene,sceneHash=Hash(scene),unity=Application.unityVersion},true));
            }
            {
                string scene=prefix+"SUB_SCN_02_MarketDay_Scene2.unity";var loaded=EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);var transforms=loaded.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                string yaml=File.ReadAllText(scene);
                foreach(var tool in new[]{new KeyValuePair<string,int>("pony-brush",3093),new KeyValuePair<string,int>("pony-pitchfork",3268)}){
                    string block=System.Text.RegularExpressions.Regex.Match(yaml,@"(?ms)^--- !u!114 &"+tool.Value+@"\r?\n.*?(?=^---|\z)").Value;
                    var match=System.Text.RegularExpressions.Regex.Match(block,@"m_GameObject: \{fileID: (\d+)\}");if(!match.Success)throw new Exception("Missing tool binding "+tool.Key);
                    ulong id=ulong.Parse(match.Groups[1].Value);var source=transforms.Single(t=>GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId==id);
                    foreach(var r in transforms.Select(t=>t.GetComponent<Renderer>()).Where(r=>r))r.enabled=false;
                    var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.SetActive(true);foreach(var r in clone.GetComponentsInChildren<Renderer>(true)){r.enabled=true;Recover(r);}
                    var renderers=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(renderers.Length==0)throw new Exception("No tool image "+tool.Key);var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                    var camera=new GameObject("Care tool capture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.15f;camera.transform.position=bounds.center+Vector3.back*30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;Capture(camera,Path.Combine(output,tool.Key+".png"),256,256,true);UnityEngine.Object.DestroyImmediate(clone);UnityEngine.Object.DestroyImmediate(camera.gameObject);
                }
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            foreach(var kind in new[]{"Brown","Grey"}){
                string path=AssetDatabase.FindAssets("pfb_emb_Pony_"+kind+" t:Prefab").Select(AssetDatabase.GUIDToAssetPath).Single(p=>Path.GetFileNameWithoutExtension(p)=="pfb_emb_Pony_"+kind);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var clone=UnityEngine.Object.Instantiate(prefab);clone.SetActive(true);clone.transform.position=Vector3.zero;var pivot=new GameObject("Pony capture facing");clone.transform.SetParent(pivot.transform,false);pivot.transform.rotation=Quaternion.Euler(0,38,0);
                foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
                var animator=clone.GetComponentInChildren<Animator>(true);if(!animator)animator=clone.AddComponent<Animator>();
                var renderers=clone.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                var camera=new GameObject("Pony sprite capture").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.15f;camera.transform.position=bounds.center+Vector3.back*20;camera.backgroundColor=Color.clear;camera.clearFlags=CameraClearFlags.SolidColor;
                foreach(var action in new[]{"Idle","Jump","TailSwish","HeadRotation_Down"}){
                    var clip=AssetDatabase.FindAssets("Pony_"+action+" t:AnimationClip").Select(AssetDatabase.GUIDToAssetPath).Where(p=>Path.GetFileNameWithoutExtension(p).EndsWith("Pony_"+action)).Select(AssetDatabase.LoadAssetAtPath<AnimationClip>).FirstOrDefault();
                    if(!clip)continue;int count=Mathf.Clamp(Mathf.CeilToInt(clip.length*12),1,48);var atlas=new Texture2D(2048,Mathf.CeilToInt(count/8f)*256,TextureFormat.RGBA32,false);atlas.SetPixels32(new Color32[atlas.width*atlas.height]);
                    AnimationMode.StartAnimationMode();for(int frame=0;frame<count;frame++){
                        AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,Mathf.Min(clip.length-.00001f,frame/12f));AnimationMode.EndSampling();
                        var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.aspect=1;camera.Render();RenderTexture.active=rt;var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();atlas.SetPixels32(frame%8*256,atlas.height-256-frame/8*256,256,256,image.GetPixels32());UnityEngine.Object.DestroyImmediate(image);camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);
                    }AnimationMode.StopAnimationMode();atlas.Apply();File.WriteAllBytes(Path.Combine(output,"pony-"+kind.ToLowerInvariant()+"-"+action+".png"),atlas.EncodeToPNG());UnityEngine.Object.DestroyImmediate(atlas);
                    File.WriteAllText(Path.Combine(output,"pony-"+kind.ToLowerInvariant()+"-"+action+".json"),"{\"frames\":"+count+",\"duration\":"+clip.length.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"clip\":\""+AssetDatabase.GetAssetPath(clip)+"\"}");
                }
                UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(pivot);
            }
            EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    public static void RunPrefabs()
    {
        try {
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_BAKE");
            if(string.IsNullOrEmpty(output)||Directory.Exists(output))throw new Exception("Choose a fresh Markets prefab output.");
            Directory.CreateDirectory(output);
            var files=AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("/05_MarketDay/")&&p.Contains("/Prefabs/")&&(p.Contains("/SmoothieMarket/")||p.Contains("/FoodAndFlowersMarket/")||p.Contains("/Scene3/")||p.Contains("/Scene2/")||p.Contains("/Pony/")||p.Contains("/BubbleMarket/"))).ToList();
            files.AddRange(AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.EndsWith("pfb_emb_Pony_Brown.prefab")||p.EndsWith("pfb_emb_Pony_Grey.prefab")));
            foreach(var path in files.Distinct()){
                string key=Path.GetFileNameWithoutExtension(path);
                if(key.IndexOf("audio",StringComparison.OrdinalIgnoreCase)>=0||key.Contains("Camera")||key.Contains("NPC")||key.Contains("Collider"))continue;
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!source)throw new Exception(path);
                var clone=UnityEngine.Object.Instantiate(source);clone.SetActive(true);clone.transform.position=Vector3.zero;
                foreach(var t in clone.GetComponentsInChildren<Transform>(true))if(t.name.Contains("Minifig")||t.name.Contains("minifig"))t.gameObject.SetActive(false);
                foreach(var r in clone.GetComponentsInChildren<Renderer>(true))Recover(r);
                var visible=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&r.bounds.size.magnitude<1000).ToArray();
                if(visible.Length==0){UnityEngine.Object.DestroyImmediate(clone);continue;}
                var bounds=visible[0].bounds;foreach(var r in visible)bounds.Encapsulate(r.bounds);
                var camera=new GameObject("Markets asset camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.1f;camera.transform.position=bounds.center+Vector3.back*100;camera.nearClipPlane=.01f;camera.farClipPlane=300;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                bool environment=key.Contains("SubSceneController")||key.Contains("Background");
                if(environment){camera.orthographic=false;camera.fieldOfView=20;camera.transform.position=new Vector3(0,3.35f,-16.25f);camera.transform.rotation=Quaternion.Euler(9.75f,0,0);camera.backgroundColor=new Color(.64f,.87f,.98f);}
                Capture(camera,Path.Combine(output,key+".png"),environment?1600:256,environment?1000:256,!environment);
                File.WriteAllText(Path.Combine(output,key+".json"),JsonUtility.ToJson(new Receipt{scene=path,sceneHash=Hash(path),renderers=visible.Length,min=bounds.min,max=bounds.max},true));
                Debug.Log("MARKETS_PREFAB "+key);
            }
            EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void Recover(Renderer r)
    {
        if(r is ParticleSystemRenderer||r is TrailRenderer||r is LineRenderer){r.enabled=false;return;}
        if(r is SpriteRenderer){r.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));return;}
        var b=r.GetComponent<LegoDuplo.Materials.LegoBrickMaterial>();
        r.sharedMaterials=r.sharedMaterials.Select(old=>{var m=new Material(Shader.Find("Research/RecoveredBrick"));if(b){m.SetColor("_Color",b.color);if(b.decal){m.mainTexture=b.decal;m.mainTextureScale=b.decalTiling;m.mainTextureOffset=b.decalOffset;m.SetFloat("_UseDecal",1);}}else if(old){if(old.HasProperty("_Color"))m.SetColor("_Color",old.GetColor("_Color"));if(old.mainTexture){m.mainTexture=old.mainTexture;m.SetFloat("_UseDecal",1);}}return m;}).ToArray();
    }
    [Serializable] public class Receipt {public string unity,scene,sceneHash;public int renderers;public string[] cameras;public Vector3 min,max;}
    public static void Run()
    {
        try {
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_BAKE");
            if(string.IsNullOrEmpty(output)||Directory.Exists(output))throw new Exception("Choose a fresh Markets output.");
            Directory.CreateDirectory(output);
            foreach(var path in AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("/05_MarketDay/Scenes/"))){
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var roots=scene.GetRootGameObjects();
                var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                var renderers=transforms.Select(t=>t.GetComponent<Renderer>()).Where(r=>r).ToArray();
                foreach(var t in transforms)if(t.name.Contains("Minifig")||t.name.Contains("minifig"))t.gameObject.SetActive(false);
                if(path.EndsWith("Scene2.unity"))foreach(var t in transforms)if(t.name=="tentEnvironment"||t.name.IndexOf("Makeover",StringComparison.OrdinalIgnoreCase)>=0)t.gameObject.SetActive(false);
                foreach(var r in renderers){
                    if(r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer){r.enabled=false;continue;}
                    if(r is SpriteRenderer){r.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));continue;}
                    var b=r.GetComponent<LegoDuplo.Materials.LegoBrickMaterial>();
                    r.sharedMaterials=r.sharedMaterials.Select(old=>{
                        var m=new Material(Shader.Find("Research/RecoveredBrick"));
                        if(b){m.SetColor("_Color",b.color);if(b.decal){m.mainTexture=b.decal;m.mainTextureScale=b.decalTiling;m.mainTextureOffset=b.decalOffset;m.SetFloat("_UseDecal",1);}}
                        else if(old){if(old.HasProperty("_Color"))m.SetColor("_Color",old.GetColor("_Color"));if(old.HasProperty("_BaseColor"))m.SetColor("_Color",old.GetColor("_BaseColor"));if(old.mainTexture){m.mainTexture=old.mainTexture;m.SetFloat("_UseDecal",1);}}
                        return m;
                    }).ToArray();
                }
                var cameras=transforms.Select(t=>t.GetComponent<Camera>()).Where(c=>c).ToArray();
                foreach(var canvas in transforms.Select(t=>t.GetComponent<Canvas>()).Where(c=>c))canvas.gameObject.SetActive(false);
                var visible=renderers.Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.bounds.size.magnitude<1000).ToArray();
                if(visible.Length==0)throw new Exception("No visible geometry: "+path);
                var bounds=visible[0].bounds;foreach(var r in visible)bounds.Encapsulate(r.bounds);
                var camera=cameras.FirstOrDefault(c=>c.CompareTag("MainCamera"))??cameras.FirstOrDefault();
                if(!camera)camera=new GameObject("Markets recovery camera").AddComponent<Camera>();
                camera.gameObject.SetActive(true);camera.enabled=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.64f,.87f,.98f);camera.cullingMask=~0;
                camera.nearClipPlane=.01f;camera.farClipPlane=2000;
                var receipt=new Receipt{unity=Application.unityVersion,scene=path,sceneHash=Hash(path),renderers=visible.Length,cameras=cameras.Select(c=>c.name+" pos="+c.transform.position+" rot="+c.transform.eulerAngles+" ortho="+c.orthographic+" size="+c.orthographicSize+" fov="+c.fieldOfView).ToArray(),min=bounds.min,max=bounds.max};
                string name=Path.GetFileNameWithoutExtension(path);
                File.WriteAllText(Path.Combine(output,name+".json"),JsonUtility.ToJson(receipt,true));
                Capture(camera,Path.Combine(output,name+".png"));
                Debug.Log("MARKETS_BAKE "+name+" "+visible.Length);
            }
            EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static string Hash(string path){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
    static void Capture(Camera camera,string path,int width=1600,int height=1000,bool alpha=false){var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.aspect=(float)width/height;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,alpha?TextureFormat.RGBA32:TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
}
