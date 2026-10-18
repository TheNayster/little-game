using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LittleWeeps.Client
{
    public static class SpideyArt
    {
        // Both recovered worlds consume the same checked rig/anchor pack schema.
        [Serializable] public sealed class ContentEntry {public string key,file;}
        [Serializable] public sealed class Content {public ContentEntry[] pictures,audio;}
        private static BirthdayArt.Pack pack;
        private static Content content;
        private static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        private static readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
        private static readonly Dictionary<Texture2D,byte[]> alpha=new Dictionary<Texture2D,byte[]>();
        public static string Root {
            get {
                var supplied=Environment.GetEnvironmentVariable("LITTLE_WEEPS_SPIDEY_ART");if(!string.IsNullOrEmpty(supplied))return Path.GetFullPath(supplied);
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../LocalData/Research/DuploMarvel/world-tests/SpideyHQ/Content-v4"));
#else
                return RecoveredWorldContent.Root("SpideyHQ");
#endif
            }
        }
        public static bool Available=>File.Exists(Path.Combine(Root,"hq-content.json"));
        public static BirthdayArt.Pack Data {
            get {
                if(pack!=null)return pack;if(!Available)return null;
                var candidate=JsonUtility.FromJson<BirthdayArt.Pack>(File.ReadAllText(Path.Combine(Root,"actor-frames.json")));
                if(candidate?.actions==null || candidate.props==null || candidate.format!=3 || candidate.fps!=12 || candidate.actions.Any(a=>a.frames<1 || a.size!=256 || a.columns!=8 || a.unitsPerPixel<=0 || a.hold?.Length!=a.frames || a.face?.Length!=a.frames))throw new InvalidDataException("Incomplete HQ action pack.");
                content=JsonUtility.FromJson<Content>(File.ReadAllText(Path.Combine(Root,"hq-content.json")));if(content?.pictures==null || content.audio==null)throw new InvalidDataException("Incomplete HQ pictures/audio.");pack=candidate;return pack;
            }
        }
        public static BirthdayArt.Frames Action(string role,string action)=>Data?.actions.FirstOrDefault(a=>a.actor==role && a.action==action);
        public static BirthdayArt.Prop Prop(string kind,string view)=>Data?.props.FirstOrDefault(p=>p.name==(kind=="pizza"?"PizzaProp":kind=="camera"?"CameraProp":kind=="board"?"HoverBoard":"balloon") && p.view==view);
        private static string FilePath(string relative)
        {
            if(Path.IsPathRooted(relative) || relative.Split('/','\\').Any(p=>p==".."))throw new InvalidDataException("Invalid HQ content path.");return Path.Combine(Root,relative);
        }
        public static Texture2D Texture(string relative)
        {
            if(textures.TryGetValue(relative,out var texture))return texture;
            texture=new Texture2D(2,2,TextureFormat.RGBA32,false){name="HQ "+relative,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            if(!texture.LoadImage(File.ReadAllBytes(FilePath(relative)),false))throw new InvalidDataException("Invalid HQ picture.");if(!relative.StartsWith("world-",StringComparison.Ordinal))alpha[texture]=texture.GetPixels32().Select(p=>p.a).ToArray();texture.Apply(false,true);textures.Add(relative,texture);return texture;
        }
        public static Texture2D Picture(string key){if(Data==null)return null;var row=content.pictures.SingleOrDefault(p=>p.key==key);return row==null?null:Texture(row.file);}
        public static Sprite MenuPicture(){var row=Action("Spiderman","idle");return row==null?null:Sprite.Create(Texture(row.texture),new Rect(0,Texture(row.texture).height-256,256,256),Vector2.one*.5f);}
        public static System.Collections.IEnumerator LoadAudio(string key,Action<AudioClip> done)
        {
            if(sounds.TryGetValue(key,out var clip)){done(clip);yield break;}if(Data==null)yield break;
            var entry=content.audio.SingleOrDefault(a=>a.key==key);if(entry==null)yield break;
            using(var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(FilePath(entry.file)).AbsoluteUri,AudioType.WAV)){
                yield return request.SendWebRequest();if(request.result!=UnityWebRequest.Result.Success){Debug.LogWarning("HQ audio: "+request.error);yield break;}
                clip=DownloadHandlerAudioClip.GetContent(request);sounds[key]=clip;done(clip);
            }
        }
        public static bool PixelVisible(Texture texture,Vector2 uv){if(!(texture is Texture2D image) || !alpha.TryGetValue(image,out var pixels))return true;int x=Mathf.Clamp((int)(uv.x*image.width),0,image.width-1),y=Mathf.Clamp((int)(uv.y*image.height),0,image.height-1);return pixels[y*image.width+x]>25;}
        public static void Release(){foreach(var image in textures.Values)UnityEngine.Object.Destroy(image);foreach(var clip in sounds.Values)UnityEngine.Object.Destroy(clip);textures.Clear();sounds.Clear();alpha.Clear();pack=null;content=null;}
    }
}
