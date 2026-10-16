// Research asset-only tool. BLU-02; no original executable game logic is run.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class GoForADriveBake {
 [Serializable] public class LayoutRow{public int index;public string key,sourceGameObjectId;public Vector3 sourcePosition;public Vector2 point;public Rect bounds;}
[Serializable] public class Layout{public int width=1920,height=1080;public LayoutRow[] rows;}
 static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_DRIVE_SCENE_BAKE_OUTPUT")??throw new Exception("Set LITTLE_WEEPS_DRIVE_SCENE_BAKE_OUTPUT to a new research output directory."));
 public static void Run(){try{
 if(Directory.Exists(Output))throw new Exception("Preserve existing bake; select a fresh output directory.");Directory.CreateDirectory(Output);
 foreach(var index in new[]{1,2,3,4}){
 string scenePath="Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/Scenes/SUB_SCN_0"+index+"_GoForADrive_Scene"+index+".unity";
 var main=EditorSceneManager.OpenScene("Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/Scenes/SCN_00_GoForADrive.unity");var scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);var roots=main.GetRootGameObjects().Concat(scene.GetRootGameObjects()).ToArray();var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
 if(index==2||index==3){string name=index==2?"pfb_gfd_Road_1":"pfb_gfd_BG_1";string p=AssetDatabase.FindAssets(name+" t:Prefab").Select(AssetDatabase.GUIDToAssetPath).First(a=>Path.GetFileNameWithoutExtension(a)==name);for(int tile=-2;tile<=2;tile++){var bg=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(p));bg.transform.position=new Vector3((index==2?100:200)+tile*11.8f,0,0);bg.SetActive(true);foreach(var r in bg.GetComponentsInChildren<Renderer>(true))Recover(r);}} foreach(var r in transforms.SelectMany(t=>t.GetComponents<Renderer>()))Recover(r);
 foreach(var canvas in transforms.SelectMany(t=>t.GetComponents<Canvas>()))canvas.enabled=false;
 var cameras=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).SelectMany(t=>t.GetComponents<Camera>()).ToArray();var camera=cameras.FirstOrDefault(c=>c.name=="Main Camera")??cameras.First();
 camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.56f,.76f,.92f,1);camera.aspect=16f/9;camera.cullingMask=-1;if(index==4){camera.fieldOfView=32;camera.transform.position+=camera.transform.up*3;}
 File.WriteAllText(Path.Combine(Output,"scene"+index+"-inventory.txt"),string.Join("\n",transforms.Select(t=>t.name+" ID="+GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId+" pos="+t.position))+"\nCAMERA "+camera.name+" "+camera.transform.position+" "+camera.transform.eulerAngles+" ortho="+camera.orthographic+" fov="+camera.fieldOfView);
 if(index==4){var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();ulong[] statueIds={434,458,760,523,570,420,709,592,506,816,573,701};var rows=statueIds.Select((id,n)=>{var t=all.Single(a=>GlobalObjectId.GetGlobalObjectIdSlow(a.gameObject).targetObjectId==id);var p=camera.WorldToViewportPoint(t.position);var renderers=t.GetComponentsInChildren<Renderer>(true).Where(r=>!(r is ParticleSystemRenderer)).ToArray();var b=renderers.First().bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);Vector2 lo=new Vector2(float.MaxValue,float.MaxValue),hi=new Vector2(float.MinValue,float.MinValue);foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1}){var v=camera.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z)));lo=Vector2.Min(lo,new Vector2(v.x,1-v.y));hi=Vector2.Max(hi,new Vector2(v.x,1-v.y));}return new LayoutRow{index=n,key="statue-brick-"+n,sourceGameObjectId=id.ToString(),sourcePosition=t.position,point=new Vector2(p.x,1-p.y),bounds=new Rect(lo,hi-lo)};}).ToArray();File.WriteAllText(Path.Combine(Output,"statue-layout.json"),JsonUtility.ToJson(new Layout{rows=rows},true));}
 Capture(camera,Path.Combine(Output,"scene"+index+"-full.png"),1920,1080);
 var ids=index==1?new ulong[]{446,513,549}:index==2?new ulong[]{250,497,748,277,278}:index==3?new ulong[]{213,204}:new ulong[]{256,643,311,775};foreach(var t in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)))if(ids.Contains(GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).targetObjectId))t.gameObject.SetActive(false);
 Capture(camera,Path.Combine(Output,"scene"+index+"-background.png"),1920,1080);
 }
 Debug.Log("DRIVE_BAKE_COMPLETE "+Output);EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 public static void Recover(Renderer renderer){
 if(renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer){renderer.enabled=false;return;}
 if(renderer is SpriteRenderer){renderer.sharedMaterial=new Material(Shader.Find("Research/RecoveredSprite"));return;}
 var brick=renderer.GetComponent<LegoDuplo.Materials.LegoBrickMaterial>();
 if(!brick){renderer.sharedMaterials=renderer.sharedMaterials.Select(ReadMaterial).ToArray();return;}
 var material=new Material(Shader.Find("Research/RecoveredBrick"));material.SetColor("_Color",brick.color);
 if(brick.decal){material.mainTexture=brick.decal;material.mainTextureScale=brick.decalTiling;material.mainTextureOffset=brick.decalOffset;material.SetFloat("_UseDecal",1);}
 renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();}
 public static void Capture(Camera camera,string path,int w,int h){var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(w,h,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
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

 [Serializable] public class LayerRow{public int index;public string key,file,sourceGameObjectId;public Rect bounds;public Vector2 point,pivot;public int width,height;}
 [Serializable] public class Layers{public int width=1920,height=1080;public LayerRow[] rows;}
 public static void RunStatueLayers(){try{
 string output=Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_DRIVE_STATUE_LAYER_OUTPUT")??throw new Exception("Set fresh statue layer output."));if(Directory.Exists(output))throw new Exception("Preserve existing layers.");Directory.CreateDirectory(output);
 EditorSceneManager.OpenScene("Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/Scenes/SCN_00_GoForADrive.unity");var scene=EditorSceneManager.OpenScene("Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/Scenes/SUB_SCN_04_GoForADrive_Scene4.unity",OpenSceneMode.Additive);
 var transforms=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){Recover(r);r.enabled=false;}foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))canvas.enabled=false;
 var camera=transforms.SelectMany(t=>t.GetComponents<Camera>()).First(c=>c.name=="Main Camera");camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.aspect=16f/9;camera.fieldOfView=32;camera.transform.position+=camera.transform.up*3;camera.cullingMask=-1;
 ulong[] ids={434,458,760,523,570,420,709,592,506,816,573,701};var rows=new System.Collections.Generic.List<LayerRow>();
 foreach(var id in ids){var t=transforms.Single(a=>GlobalObjectId.GetGlobalObjectIdSlow(a.gameObject).targetObjectId==id);var renderers=t.GetComponentsInChildren<Renderer>(true).Where(r=>!(r is ParticleSystemRenderer)).ToArray();foreach(var r in renderers)r.enabled=true;
 var rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1920,1080,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();var pixels=image.GetPixels32();int left=1920,right=0,bottom=1080,top=0;for(int y=0;y<1080;y++)for(int x=0;x<1920;x++)if(pixels[y*1920+x].a>0){left=Math.Min(left,x);right=Math.Max(right,x+1);bottom=Math.Min(bottom,y);top=Math.Max(top,y+1);}if(right<=left||top<=bottom)throw new Exception("Empty statue layer "+id);
 int w=right-left,h=top-bottom;var cropped=new Texture2D(w,h,TextureFormat.RGBA32,false);cropped.SetPixels(image.GetPixels(left,bottom,w,h));cropped.Apply();int n=rows.Count;string file="statue-layer-"+n+".png";File.WriteAllBytes(Path.Combine(output,file),cropped.EncodeToPNG());var point=camera.WorldToViewportPoint(t.position);rows.Add(new LayerRow{index=n,key="statue-brick-"+n,file=file,sourceGameObjectId=id.ToString(),width=w,height=h,bounds=new Rect(left/1920f,(1080-top)/1080f,w/1920f,h/1080f),point=new Vector2(point.x,1-point.y),pivot=new Vector2(point.x*1920-left,point.y*1080-bottom)});
 foreach(var r in renderers)r.enabled=false;camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(cropped);
 }
 File.WriteAllText(Path.Combine(output,"statue-layout.json"),JsonUtility.ToJson(new Layers{rows=rows.ToArray()},true));Debug.Log("DRIVE_STATUE_LAYERS_COMPLETE "+output);EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
