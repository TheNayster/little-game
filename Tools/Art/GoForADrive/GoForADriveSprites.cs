// Research asset-only tool. BLU-02; no original executable game logic is run.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class GoForADriveSprites {
 [Serializable]public class Picture{public string key,file,source; public Vector2 pivot;public float unitsPerPixel;}
 [Serializable]public class Frames{public string actor,action,clip,texture;public int frames,columns=8,size=256;public float duration,unitsPerPixel;public Vector2 pivot;public Vector2[] leftHand,rightHand,hold,face;}
 [Serializable]public class Pack{public int format=1;public Picture[] pictures;public Frames[] actions;}
 static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("LITTLE_WEEPS_DRIVE_SPRITE_BAKE_OUTPUT")??throw new Exception("Set LITTLE_WEEPS_DRIVE_SPRITE_BAKE_OUTPUT to a new research output directory."));
 static readonly List<Picture> pictures=new List<Picture>();static readonly List<Frames> actions=new List<Frames>();
 public static void Run(){try{
 if(Directory.Exists(Output))throw new Exception("Preserve existing bake; select a fresh output directory.");Directory.CreateDirectory(Output);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 string root="Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/";
 var prefabPaths=AssetDatabase.FindAssets("t:Prefab",new[]{root}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
 var propNames=new Dictionary<string,string>{{"car","pfb_gfd_HeelerCar"},{"nomad","pfb_gfd_NPCVehicle_GreyNomad"},{"peanut","pfb_gfd_Peanutv2"},{"house","pfb_gfd_HeelersHouse"},{"cow","pfb_gfd_Cow"},{"pony-brown","pfb_gfd_Pony_Brown"},{"pony-grey","pfb_gfd_Pony_Grey"},{"chicken","pfb_gfd_Chicken"},{"star","pfb_gfd_Star"},{"toilet","pfb_gfd_Road_Toilet_OneShot"},{"rubble","pfb_gfd_Road_Roadworks_OneShot"}};
 foreach(var pair in propNames){var path=prefabPaths.SingleOrDefault(p=>Path.GetFileNameWithoutExtension(p)==pair.Value);if(path!=null)BakeProp(path,pair.Key);}
 foreach(var path in prefabPaths.Where(p=>p.Contains("/ImperfectPacking/")&&!Path.GetFileNameWithoutExtension(p).Contains("Car")&&!Path.GetFileNameWithoutExtension(p).Contains("House")&&p.Contains("/PackingBricks/")))BakeProp(path,"packing-"+Path.GetFileNameWithoutExtension(path));
 foreach(var path in prefabPaths.Where(p=>p.Contains("/BuildBigThing/")&&Path.GetFileNameWithoutExtension(p).StartsWith("pfb_gfd_bbt_")))BakeProp(path,"brick-"+Path.GetFileNameWithoutExtension(path));
 string charRoot="Assets/LDBMain/AddressableAssets/Embedded/Minifigs/";
 foreach(var name in new[]{"Bluey","Bandit","Bingo","Chilli"}){
 var path=AssetDatabase.FindAssets("t:Prefab",new[]{charRoot}).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p=>Path.GetFileNameWithoutExtension(p).StartsWith("pfb_emb_DuploMinifig_Variant - "+name+"_"));
 if(path==null)throw new Exception("Missing character "+name);BakeActor(path,name.ToLowerInvariant());}
 File.WriteAllText(Path.Combine(Output,"drive-content.json"),JsonUtility.ToJson(new Pack{pictures=pictures.ToArray(),actions=actions.ToArray()},true));Debug.Log("DRIVE_SPRITES_COMPLETE "+pictures.Count+" "+actions.Count);EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 static GameObject Create(string path){var clone=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));clone.name=Path.GetFileNameWithoutExtension(path);clone.transform.position=Vector3.zero;clone.SetActive(true);foreach(var t in clone.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);foreach(var r in clone.GetComponentsInChildren<Renderer>(true))GoForADriveBake.Recover(r);return clone;}
 static Camera CameraFor(GameObject go){var visible=go.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&!(r is ParticleSystemRenderer)).ToArray();if(visible.Length==0)throw new Exception("No renderers "+go.name);var b=visible[0].bounds;foreach(var r in visible)b.Encapsulate(r.bounds);var c=new GameObject("Bake Camera").AddComponent<Camera>();c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.clear;c.orthographic=true;c.orthographicSize=Mathf.Max(b.size.y,b.size.x)*.7f;c.transform.position=b.center+new Vector3(0,b.size.y*.3f,12);c.transform.LookAt(b.center);return c;}
 static void BakeProp(string path,string key){var go=Create(path);go.transform.rotation=Quaternion.Euler(0,25,0);var c=CameraFor(go);string file=key+".png";GoForADriveBake.Capture(c,Path.Combine(Output,file),512,512);pictures.Add(new Picture{key=key,file=file,source=path,pivot=Pixel(c,go.transform.position,512),unitsPerPixel=c.orthographicSize*2/512});UnityEngine.Object.DestroyImmediate(c.gameObject);UnityEngine.Object.DestroyImmediate(go);}
 static Vector2 Pixel(Camera c,Vector3 v,int size=256){var p=c.WorldToViewportPoint(v);return new Vector2(p.x*size,p.y*size);}
 static void BakeActor(string path,string name){
 var go=Create(path);var animator=go.GetComponentInChildren<Animator>(true);if(!animator)throw new Exception("No animator "+path);animator.enabled=false;
 var c=CameraFor(go);c.orthographicSize*=1.15f;var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);c.targetTexture=rt;
 string age=name=="bluey"||name=="bingo"?"Child":"Adult";
 var clips=AssetDatabase.FindAssets("t:AnimationClip").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<AnimationClip>).Where(a=>a).ToArray();
 File.WriteAllText(Path.Combine(Output,name+"-rig.txt"),string.Join("\n",go.GetComponentsInChildren<Transform>(true).Select(t=>AnimationUtility.CalculateTransformPath(t,go.transform))));
 var all=go.GetComponentsInChildren<Transform>(true);Transform Find(string term)=>all.FirstOrDefault(t=>t.name.IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0);
 var left=Find("HandParent_L");var right=Find("HandParent_R");var face=Find("Head");
 var skins=go.GetComponentsInChildren<SkinnedMeshRenderer>(true);var baked=skins.Select(s=>new Mesh()).ToArray();var filters=skins.Select(s=>{var temp=new GameObject("CPU skin");temp.transform.SetParent(s.transform,false);var f=temp.AddComponent<MeshFilter>();temp.AddComponent<MeshRenderer>().sharedMaterials=s.sharedMaterials;s.enabled=false;return f;}).ToArray(); AnimationMode.StartAnimationMode();
 foreach(var view in new[]{"front","side","back"})foreach(var action in new[]{"Idle","Walk","Run","Happy","Wave","Sitting","Dancing","carry","throw","balance","fuel","toilet","build","smash","driving"}){
 string clipName="ani_emb_"+action+"_"+age;
 switch(action){case "carry":clipName="ani_emb_Walk_"+age;break;case "throw":clipName="ani_emb_Wave_"+age;break;case "balance":case "smash":clipName="ani_emb_FreakOut_"+age;break;case "fuel":clipName=age=="Adult"?"ani_emb_HoseWashing_Adult":"ani_emb_HandAngle_Right_Mid_Child";break;case "toilet":clipName=age=="Child"?"ani_gfd_Sitting_Child_Toilet":"ani_emb_Sitting_Adult";break;case "build":clipName="ani_emb_"+age+"_BendFWD";break;case "driving":clipName=age=="Adult"?"ani_gfd_Adult_DrivingLoop":"ani_emb_Sitting_Child";break;}
 var clip=clips.FirstOrDefault(a=>a.name==clipName);if(!clip)continue;go.transform.rotation=Quaternion.Euler(0,view=="side"?90:view=="back"?180:0,0);
 int count=Math.Max(1,Math.Min(72,(int)Math.Ceiling(clip.length*12)));var sheet=new Texture2D(2048,Mathf.CeilToInt(count/8f)*256,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[sheet.width*sheet.height]);
 string act=action.ToLowerInvariant()+(view=="front"?"":view=="side"?"-side":"-back");var row=new Frames{actor=name,action=act,clip=AssetDatabase.GetAssetPath(clip),texture=name+"-"+act+".png",frames=count,duration=clip.length,unitsPerPixel=c.orthographicSize*2/256,pivot=Pixel(c,go.transform.position),leftHand=new Vector2[count],rightHand=new Vector2[count],hold=new Vector2[count],face=new Vector2[count]};
 for(int frame=0;frame<count;frame++){AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,clip.length>0?Math.Min(clip.length-.00001f,frame/12f):0);if(action=="carry"){foreach(var hand in new[]{"Left","Right"}){var layer=clips.First(a=>a.name=="ani_emb_HandAngle_"+hand+"_Mid_"+age);AnimationMode.SampleAnimationClip(animator.gameObject,layer,0);}}AnimationMode.EndSampling();for(int k=0;k<skins.Length;k++){skins[k].BakeMesh(baked[k]);filters[k].sharedMesh=baked[k];}c.Render();RenderTexture.active=rt;var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();sheet.SetPixels32(frame%8*256,sheet.height-256-frame/8*256,256,256,image.GetPixels32());row.leftHand[frame]=left?Pixel(c,left.position):row.pivot;row.rightHand[frame]=right?Pixel(c,right.position):row.pivot;row.hold[frame]=row.rightHand[frame];row.face[frame]=face?Pixel(c,face.position):row.pivot;UnityEngine.Object.DestroyImmediate(image);}
 sheet.Apply();File.WriteAllBytes(Path.Combine(Output,row.texture),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);actions.Add(row);}
 AnimationMode.StopAnimationMode();c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(c.gameObject);UnityEngine.Object.DestroyImmediate(go);
 }
}
