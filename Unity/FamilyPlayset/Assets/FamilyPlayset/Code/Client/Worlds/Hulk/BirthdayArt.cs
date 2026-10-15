using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LittleWeeps.Client
{
    // Recovered content stays in a local content pack, outside tracked source.
    // PC research candidates can select the pack without changing saves or prefs.
    public static class BirthdayArt
    {
        [Serializable] public sealed class Frames {
            public string actor,action,clip,texture; public int frames,columns,size;
            public float duration,unitsPerPixel; public Vector2 pivot;public Vector2[] hold,leftHand,rightHand,face;
        }
        [Serializable] public sealed class Prop {
            public string name,texture; public float unitsPerPixel;public Vector2 pivot,holdPivot;public long sourceBaseId,sourceHoldId;
        }
        [Serializable] public sealed class Pack {public int fps;public Frames[] actions;public Prop[] props;}
        private static Pack pack;
        private static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        private static readonly Dictionary<Texture2D,byte[]> alpha=new Dictionary<Texture2D,byte[]>();
        public const float PixelsPerUnit=1200/19.5f;
        public static string Root {
            get {
                var supplied=Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_ART");
                if(!string.IsNullOrWhiteSpace(supplied))return Path.GetFullPath(supplied);
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../LocalData/Research/DuploMarvel/world-tests/HulkCompleteProbe/Baked2D"));
#else
                return Path.Combine(Application.streamingAssetsPath,"HulkBirthday");
#endif
            }
        }
        public static bool Available=>File.Exists(Path.Combine(Root,"actor-frames.json"));
        public static Pack Data {
            get {
                if(pack!=null)return pack;
                if(!Available)return null;
                var candidate=JsonUtility.FromJson<Pack>(File.ReadAllText(Path.Combine(Root,"actor-frames.json")));
                if(candidate?.actions==null || candidate.props==null || candidate.fps!=12 || candidate.actions.Any(a=>a==null || a.frames<1 || a.size!=256 || a.columns!=8 || a.unitsPerPixel<=0 || a.hold==null || a.hold.Length!=a.frames || a.face==null || a.face.Length!=a.frames))
                    throw new InvalidDataException("Incomplete birthday animation pack.");
                pack=candidate;return pack;
            }
        }
        public static Frames Action(string actor,string action)=>Data?.actions.FirstOrDefault(a=>a.actor==actor && a.action==action);
        public static Texture2D Texture(string relative)
        {
            if(textures.TryGetValue(relative,out var cached))return cached;
            if(Path.IsPathRooted(relative) || relative.Split('/','\\').Any(p=>p==".."))throw new InvalidDataException("Invalid birthday texture path.");
            var file=Path.Combine(Root,relative);if(!File.Exists(file))throw new FileNotFoundException("Missing birthday content.",relative);
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false){name="Birthday "+relative,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            if(!texture.LoadImage(File.ReadAllBytes(file),false))throw new InvalidDataException("Invalid birthday texture: "+relative);
            if(!relative.StartsWith("world-",StringComparison.Ordinal))alpha.Add(texture,texture.GetPixels32().Select(p=>p.a).ToArray());
            texture.Apply(false,true);
            textures.Add(relative,texture);return texture;
        }
        public static Rect Uv(Frames action,int frame)
        {
            var rows=(action.frames+action.columns-1)/action.columns;
            return new Rect(frame%action.columns/(float)action.columns,(rows-1-frame/action.columns)/(float)rows,1f/action.columns,1f/rows);
        }
        public static bool PixelVisible(Texture texture,Vector2 uv)
        {
            if(!(texture is Texture2D image) || !alpha.TryGetValue(image,out var pixels))return true;
            int x=Mathf.Clamp((int)(uv.x*image.width),0,image.width-1),y=Mathf.Clamp((int)(uv.y*image.height),0,image.height-1);
            return pixels[y*image.width+x]>25;
        }
        public static Sprite MenuPicture()
        {
            var idle=Action("Hulk_NewNew","idle");if(idle==null)return null;
            return Sprite.Create(Texture(idle.texture),new Rect(0,0,idle.size,idle.size),new Vector2(.5f,.5f));
        }
        public static void Release()
        {
            foreach(var texture in textures.Values)if(texture)UnityEngine.Object.Destroy(texture);
            textures.Clear();alpha.Clear();pack=null;
        }
    }
}
