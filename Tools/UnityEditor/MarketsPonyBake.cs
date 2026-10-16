// BLU-01 asset-only bake. Run in the retained Markets/AssetProbe, never the game.
// Source scenes are opened and read; only fresh PNGs/receipts and this probe's
// handwritten replacement shader are written. No recovered controller executes.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MarketsPonyBake
{
    const string ScenePath="Assets/LDBMain/AddressableAssets/Remote/Playpacks/05_MarketDay/Scenes/SUB_SCN_02_MarketDay_Scene2.unity";
    const string ShaderPath="Assets/Research/MarketsPonyRecovered.shader";
    const string HeadDownPath="Assets/LDBMain/AddressableAssets/Remote/Shared/Animals/Duplo/Pony/Animations/ani_rem_Pony_HeadRotation_Down.anim";
    sealed class Choice { public Texture2D decal; public Color color; public bool accessory; public int index; public string guid; }
    sealed class Part { public int kind,decalIndex; public long material,accessory; public Choice[] choices; }
    [Serializable] public sealed class Variant { public string file,sha256,pose; public int body,mane,tail,saddle,visiblePixels,changedPixels; public Vector2 headAnchor; }
    [Serializable] public sealed class ChoiceReceipt { public int part,index;public string decal,decalSha256;public Color color;public bool accessory; }
    [Serializable] public sealed class Receipt {
        public string sourceScene,sourceSceneSha256,unity,shaderSha256,note,headDownClip,headDownClipSha256;
        public int size,fullVariants,tailVariants,headDownVariants,changedHeadBones;
        public float headDownSampleTime;
        public string[] headDownBindings;
        public Vector3 cameraPosition,cameraEuler,min,max;
        public float orthographicSize;
        public Vector2 pivot,footAnchor,headAnchor;
        public ChoiceReceipt[] choices;public Variant[] variants;
    }
    static readonly CultureInfo Invariant=CultureInfo.InvariantCulture;
    static string Hash(string path){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
    static string Field(string text,string name){var m=Regex.Match(text,@"(?m)^\s*"+Regex.Escape(name)+@":\s*([^\r\n]*)");if(!m.Success)throw new Exception("Missing source field "+name);return m.Groups[1].Value;}
    static long Ref(string text,string name)=>long.Parse(Regex.Match(Field(text,name),@"fileID: (\d+)").Groups[1].Value,Invariant);
    static Color ColorField(string text,string name){var values=Regex.Matches(Field(text,name),@"[rgba]: ([\d.Ee+\-]+)").Cast<Match>().Select(m=>float.Parse(m.Groups[1].Value,Invariant)).ToArray();if(values.Length!=4)throw new Exception("Malformed source color");return new Color(values[0],values[1],values[2],values[3]);}
    static Vector2 VectorField(string text,string name){var values=Regex.Matches(Field(text,name),@"[xy]: ([\d.Ee+\-]+)").Cast<Match>().Select(m=>float.Parse(m.Groups[1].Value,Invariant)).ToArray();if(values.Length!=2)throw new Exception("Malformed source UV");return new Vector2(values[0],values[1]);}
    static Dictionary<long,string> Blocks(string text)=>Regex.Split(text,@"(?m)^--- !u!").Skip(1).ToDictionary(s=>long.Parse(Regex.Match(s,@"^\d+ &(\d+)").Groups[1].Value,Invariant));
    static Part ReadPart(string text){
        string body=text.Substring(text.IndexOf("  bodyPart:",StringComparison.Ordinal));
        var choices=Regex.Split(text.Substring(0,text.IndexOf("  bodyPart:",StringComparison.Ordinal)),@"(?m)^  - Decal:").Skip(1).Select(chunk=>{
            var match=Regex.Match(chunk,@"guid: (\w+)");string guid=match.Success?match.Groups[1].Value:"";
            var texture=guid==""?null:AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));if(guid!=""&&!texture)throw new Exception("Missing pony decal "+guid);
            return new Choice{decal=texture,guid=guid,color=ColorField(chunk,"MainColor"),accessory=Field(chunk,"IsOptionalAccessoryEnabled")=="1",index=int.Parse(Field(chunk,"index"),Invariant)};
        }).ToArray();
        return new Part{kind=int.Parse(Field(body,"BodyPartType"),Invariant),decalIndex=int.Parse(Field(body,"decalIndex"),Invariant),material=Ref(body,"LegoBrickMaterial"),accessory=Ref(body,"OptionalAccessory"),choices=choices};
    }
    static void SetDecal(Material material,string slot,Texture2D texture,Color color,Vector2 tiling,Vector2 offset){
        material.SetTexture(slot,texture?texture:Texture2D.blackTexture);material.SetColor(slot+"Color",color);material.SetFloat(slot+"Use",texture?1:0);material.SetTextureScale(slot,tiling);material.SetTextureOffset(slot,offset);
    }
    static Material Recover(Renderer renderer,Shader brick){
        var source=renderer.GetComponent<LegoDuplo.Materials.LegoBrickMaterial>();var m=new Material(brick);
        if(source){m.SetColor("_Color",source.color);if(source.decal){m.mainTexture=source.decal;m.mainTextureScale=source.decalTiling;m.mainTextureOffset=source.decalOffset;m.SetFloat("_UseDecal",1);}}
        else{var old=renderer.sharedMaterial;if(old&&old.HasProperty("_Color"))m.SetColor("_Color",old.GetColor("_Color"));}
        renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>m).ToArray();return m;
    }
    public static void Run(){
        try{
            if(!Application.dataPath.Replace('\\','/').EndsWith("/Markets/AssetProbe/Assets",StringComparison.OrdinalIgnoreCase))throw new Exception("MarketsPonyBake requires the isolated Markets AssetProbe.");
            string output=Environment.GetEnvironmentVariable("LITTLE_WEEPS_MARKETS_PONY_BAKE");
            if(string.IsNullOrWhiteSpace(output)||Directory.Exists(output))throw new Exception("Choose a fresh LITTLE_WEEPS_MARKETS_PONY_BAKE folder.");
            Directory.CreateDirectory(output);Directory.CreateDirectory(Path.GetDirectoryName(ShaderPath));File.WriteAllText(ShaderPath,ShaderSource);AssetDatabase.ImportAsset(ShaderPath,ImportAssetOptions.ForceSynchronousImport);
            var shader=Shader.Find("Research/MarketsPony");var brick=Shader.Find("Research/RecoveredBrick");if(!shader||!brick)throw new Exception("Handwritten recovery shaders unavailable.");
            string source=File.ReadAllText(ScenePath);var blocks=Blocks(source);
            var parts=new[]{2996L,3032L,3039L,3085L}.Select(id=>ReadPart(blocks[id])).OrderBy(p=>p.kind).ToArray();
            if(parts.Length!=4||parts[0].kind!=0||parts[1].kind!=1||parts[2].kind!=2||parts[3].kind!=3||parts[0].choices.Length!=6||parts[1].choices.Length!=5||parts[2].choices.Length!=6||parts[3].choices.Length!=6)throw new Exception("Source makeover catalog differs; inspect before baking.");
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var transforms=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var objects=transforms.Select(t=>t.gameObject).ToDictionary(g=>(long)GlobalObjectId.GetGlobalObjectIdSlow(g).targetObjectId);
            var model=objects[545];model.transform.SetParent(null,true);model.SetActive(true);model.transform.position=Vector3.zero;model.transform.rotation=Quaternion.identity;model.transform.localScale=Vector3.one;
            foreach(var root in scene.GetRootGameObjects())if(root!=model)root.SetActive(false);
            foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.SetActive(true);
            foreach(var a in model.GetComponentsInChildren<Animator>(true))a.enabled=false;
            // A source idle clip is sampled once for framing, without entering
            // Play Mode or invoking any original script/Animator controller.
            var idle=AssetDatabase.FindAssets("Pony_Idle t:AnimationClip").Select(AssetDatabase.GUIDToAssetPath).Where(p=>Path.GetFileNameWithoutExtension(p)=="Pony_Idle").Select(AssetDatabase.LoadAssetAtPath<AnimationClip>).FirstOrDefault();
            if(idle)idle.SampleAnimation(model,0);
            var headDown=AssetDatabase.LoadAssetAtPath<AnimationClip>(HeadDownPath);if(!headDown)throw new Exception("Recovered source head-down clip unavailable.");
            var headBindings=AnimationUtility.GetCurveBindings(headDown).Select(b=>b.path).Distinct().ToArray();
            if(headBindings.Length==0||headBindings.Any(path=>!model.transform.Find(path)))throw new Exception("Source head-down binding does not resolve on makeover Animator root545; no pose fabricated.");
            var poseTransforms=model.GetComponentsInChildren<Transform>(true);
            var positions=poseTransforms.Select(t=>t.localPosition).ToArray();var rotations=poseTransforms.Select(t=>t.localRotation).ToArray();var scales=poseTransforms.Select(t=>t.localScale).ToArray();
            void RestorePose(){for(int i=0;i<poseTransforms.Length;i++){poseTransforms[i].localPosition=positions[i];poseTransforms[i].localRotation=rotations[i];poseTransforms[i].localScale=scales[i];}}
            // This recovered clip is a single key at t=0 (length0). Sampling
            // that real key is intentional; no invented interpolation is used.
            headDown.SampleAnimation(model,0);
            int changedHeadBones=poseTransforms.Where((t,i)=>Quaternion.Angle(t.localRotation,rotations[i])>.1f||Vector3.Distance(t.localPosition,positions[i])>.0001f).Count();
            if(changedHeadBones==0)throw new Exception("Source head-down sample changed no makeover bones; no pose fabricated.");
            RestorePose();
            var all=model.GetComponentsInChildren<Renderer>(true);foreach(var renderer in all){if(renderer is ParticleSystemRenderer||renderer is TrailRenderer||renderer is LineRenderer){renderer.enabled=false;continue;}renderer.enabled=true;Recover(renderer,brick);}
            Renderer Reader(long id)=>objects[Ref(blocks[id],"m_GameObject")].GetComponent<Renderer>();
            var bodyRenderer=Reader(3343);var tailRenderer=Reader(3320);if(!bodyRenderer||!tailRenderer)throw new Exception("Source body/tail mesh missing.");
            var bodyMaterial=new Material(shader);var tailMaterial=new Material(shader);bodyRenderer.sharedMaterial=bodyMaterial;tailRenderer.sharedMaterial=tailMaterial;
            foreach(var id in parts.Where(p=>p.accessory!=0).Select(p=>p.accessory))objects[id].SetActive(true);
            var visible=all.Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();var bounds=visible[0].bounds;foreach(var r in visible)bounds.Encapsulate(r.bounds);
            int size=512;var camera=new GameObject("Markets fixed makeover camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.magnitude*.68f)*1.13f;
            camera.transform.position=bounds.center+Quaternion.Euler(8,142,0)*Vector3.back*20;camera.transform.LookAt(bounds.center);camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=~0;camera.allowHDR=false;camera.allowMSAA=false;
            var rows=new List<Variant>();var choices=new List<ChoiceReceipt>();
            var head=model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.IndexOf("Head",StringComparison.OrdinalIgnoreCase)>=0);
            foreach(var p in parts)foreach(var c in p.choices)choices.Add(new ChoiceReceipt{part=p.kind,index=c.index,decal=c.decal?AssetDatabase.GetAssetPath(c.decal):"",decalSha256=c.decal?Hash(AssetDatabase.GetAssetPath(c.decal)):"",color=c.color,accessory=c.accessory});
            void Apply(int body,int mane,int saddle){
                var selected=new Dictionary<int,int>{{0,mane},{1,body},{2,saddle},{3,mane}};
                foreach(var p in parts){var c=p.choices[selected[p.kind]];var m=p.kind==3?tailMaterial:bodyMaterial;var serialized=blocks[p.material];
                    if(p.kind==1)m.SetColor("_Color",c.color);
                    string field=p.decalIndex==0?"decal":p.decalIndex==1?"secondaryDecal":"tertiaryDecal";
                    string slot=p.decalIndex==0?"_Primary":p.decalIndex==1?"_Secondary":"_Tertiary";
                    // MainColor is the source tint of that specific decal; body
                    // colour remains independent from mane and saddle patterns.
                    SetDecal(m,slot,c.decal,c.color,VectorField(serialized,field+"Tiling"),VectorField(serialized,field+"Offset"));
                    if(p.kind==3)m.SetColor("_Color",c.color);
                    if(p.accessory!=0)objects[p.accessory].SetActive(c.accessory);
                }
            }
            for(int body=0;body<5;body++)for(int mane=0;mane<6;mane++)for(int saddle=0;saddle<6;saddle++){
                RestorePose();Apply(body,mane,saddle);string name=$"pony-body-{body}-mane-{mane}-saddle-{saddle}.png";int pixels=Capture(camera,Path.Combine(output,name),size);
                rows.Add(new Variant{file=name,sha256=Hash(Path.Combine(output,name)),pose="rest",body=body,mane=mane,tail=mane,saddle=saddle,visiblePixels=pixels,headAnchor=camera.WorldToViewportPoint(head?head.position:bounds.center)});
                headDown.SampleAnimation(model,0);string eat=$"pony-eat-body-{body}-mane-{mane}-saddle-{saddle}.png";int eatPixels=Capture(camera,Path.Combine(output,eat),size);
                int differences=ChangedPixels(Path.Combine(output,name),Path.Combine(output,eat));
                if(differences<100)throw new Exception("Head-down source sample has no meaningful visible change: "+eat+"; no pose fabricated.");
                rows.Add(new Variant{file=eat,sha256=Hash(Path.Combine(output,eat)),pose="head-down",body=body,mane=mane,tail=mane,saddle=saddle,visiblePixels=eatPixels,changedPixels=differences,headAnchor=camera.WorldToViewportPoint(head?head.position:bounds.center)});
            }
            // Tail-only images preserve the same frame/pivot for source-editable
            // layering; complete combinations above are the faithful runtime path.
            RestorePose();foreach(var r in all)r.enabled=r==tailRenderer;
            foreach(var p in parts.Where(p=>p.accessory!=0))objects[p.accessory].SetActive(false);
            for(int mane=0;mane<6;mane++){
                Apply(0,mane,0);foreach(var p in parts.Where(p=>p.accessory!=0))objects[p.accessory].SetActive(false);
                string name=$"tail-{mane}.png";int pixels=Capture(camera,Path.Combine(output,name),size);rows.Add(new Variant{file=name,sha256=Hash(Path.Combine(output,name)),body=-1,mane=mane,tail=mane,saddle=-1,visiblePixels=pixels});
            }
            var receipt=new Receipt{sourceScene=ScenePath,sourceSceneSha256=Hash(ScenePath),unity=Application.unityVersion,shaderSha256=Hash(ShaderPath),size=size,fullVariants=180,tailVariants=6,headDownVariants=180,headDownClip=HeadDownPath,headDownClipSha256=Hash(HeadDownPath),headDownSampleTime=0,headDownBindings=headBindings,changedHeadBones=changedHeadBones,cameraPosition=camera.transform.position,cameraEuler=camera.transform.eulerAngles,orthographicSize=camera.orthographicSize,min=bounds.min,max=bounds.max,pivot=new Vector2(.5f,.5f),footAnchor=camera.WorldToViewportPoint(model.transform.position),headAnchor=camera.WorldToViewportPoint(head?head.position:bounds.center),choices=choices.ToArray(),variants=rows.ToArray(),note="Asset-only source palette/decal reconstruction using handwritten three-decal shader. Mane and tail linked. Actual recovered single-key head-down clip sampled on source Animator root545; every posed image differs visibly from its palette-matching rest image. Original runtime bodies remain unavailable; source pose/UV/material rendering requires image review. No original gameplay executed and no source scene saved."};
            File.WriteAllText(Path.Combine(output,"pony-variants.json"),JsonUtility.ToJson(receipt,true));
            Debug.Log("MARKETS_PONY_BAKE 180 rest + 180 source head-down + 6 aligned tails: "+output);EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static int ChangedPixels(string before,string after){
        var a=new Texture2D(2,2,TextureFormat.RGBA32,false);var b=new Texture2D(2,2,TextureFormat.RGBA32,false);
        try{if(!a.LoadImage(File.ReadAllBytes(before))||!b.LoadImage(File.ReadAllBytes(after))||a.width!=b.width||a.height!=b.height)throw new Exception("Cannot compare head-down captures.");var x=a.GetPixels32();var y=b.GetPixels32();int count=0;for(int i=0;i<x.Length;i++)if(Math.Abs(x[i].r-y[i].r)>4||Math.Abs(x[i].g-y[i].g)>4||Math.Abs(x[i].b-y[i].b)>4||Math.Abs(x[i].a-y[i].a)>4)count++;return count;}
        finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
    }
    static int Capture(Camera camera,string path,int size){
        var previous=RenderTexture.active;var rt=new RenderTexture(size,size,24,RenderTextureFormat.ARGB32);var image=new Texture2D(size,size,TextureFormat.RGBA32,false);
        try{camera.targetTexture=rt;camera.aspect=1;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,size,size),0,0);image.Apply();int count=image.GetPixels32().Count(p=>p.a>20);if(count<100)throw new Exception("Empty pony capture: "+path);File.WriteAllBytes(path,image.EncodeToPNG());return count;}
        finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
    }
    const string ShaderSource=@"// BLU-01 handwritten source-data recovery; not original shader code.
Shader ""Research/MarketsPony"" {
 Properties {
  _Color(""Source body color"",Color)=(1,1,1,1)
  _Primary(""Source primary decal"",2D)=""black""{}
  _Secondary(""Source saddle decal"",2D)=""black""{}
  _Tertiary(""Source mane decal"",2D)=""black""{}
  _PrimaryColor(""Primary tint"",Color)=(1,1,1,1)
  _SecondaryColor(""Secondary tint"",Color)=(1,1,1,1)
  _TertiaryColor(""Tertiary tint"",Color)=(1,1,1,1)
  _PrimaryUse(""Primary enabled"",Float)=0
  _SecondaryUse(""Secondary enabled"",Float)=0
  _TertiaryUse(""Tertiary enabled"",Float)=0
 }
 SubShader { Tags {""RenderType""=""Opaque""} Cull Back Pass {
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include ""UnityCG.cginc""
  sampler2D _Primary,_Secondary,_Tertiary;
  float4 _Primary_ST,_Secondary_ST,_Tertiary_ST;
  fixed4 _Color,_PrimaryColor,_SecondaryColor,_TertiaryColor;
  float _PrimaryUse,_SecondaryUse,_TertiaryUse;
  struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
  struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;};
  v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;return o;}
  fixed4 frag(v2f i):SV_Target{
   fixed4 p=tex2D(_Primary,i.uv*_Primary_ST.xy+_Primary_ST.zw)*_PrimaryColor;
   fixed4 s=tex2D(_Secondary,i.uv*_Secondary_ST.xy+_Secondary_ST.zw)*_SecondaryColor;
   fixed4 t=tex2D(_Tertiary,i.uv*_Tertiary_ST.xy+_Tertiary_ST.zw)*_TertiaryColor;
   fixed3 c=lerp(_Color.rgb,p.rgb,p.a*_PrimaryUse);c=lerp(c,s.rgb,s.a*_SecondaryUse);c=lerp(c,t.rgb,t.a*_TertiaryUse);
   float diffuse=saturate(dot(normalize(i.normal),normalize(float3(-.3,.8,-.6))));return fixed4(c*(.70+.30*diffuse),1);
  }
  ENDCG
 } }
}";
}
