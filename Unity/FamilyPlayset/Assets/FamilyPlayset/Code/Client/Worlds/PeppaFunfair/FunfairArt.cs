using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
namespace LittleWeeps.Client
{
    public static class FunfairArt
    {
        [Serializable] public sealed class Entry{public string key,file;}
        [Serializable] public sealed class Content{public Entry[] pictures,audio;public Vector2[] slide;}
        [Serializable] public sealed class Wheel{public Rect pixels;public string texture;public int frames,columns,size;public Vector2[] seats;}
        static BirthdayArt.Pack data;static Content content;static Wheel wheel;
        static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        static readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
        static readonly Dictionary<Texture2D,byte[]> alpha=new Dictionary<Texture2D,byte[]>();
        public static string Root{
            get{var supplied=Environment.GetEnvironmentVariable("LITTLE_WEEPS_FUNFAIR_ART");if(!string.IsNullOrEmpty(supplied))return Path.GetFullPath(supplied);
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../LocalData/Research/PeppaPig/world-tests/Funfair/Content-v4"));
#else
                return Path.Combine(Application.streamingAssetsPath,"PeppaFunfair");
#endif
            }
        }
        public static bool Available=>File.Exists(Path.Combine(Root,"funfair-content.json"));
        static string FilePath(string relative){if(Path.IsPathRooted(relative)||relative.Split('/','\\').Any(p=>p==".."))throw new InvalidDataException("Invalid Funfair content path.");return Path.Combine(Root,relative);}
        public static BirthdayArt.Pack Data{get{if(data!=null)return data;if(!Available)return null;var pack=JsonUtility.FromJson<BirthdayArt.Pack>(File.ReadAllText(FilePath("actor-frames.json")));if(pack.actions==null||pack.fps!=12||pack.actions.Any(a=>a.frames<1||a.size!=256||a.hold?.Length!=a.frames))throw new InvalidDataException("Incomplete Funfair actions.");content=JsonUtility.FromJson<Content>(File.ReadAllText(FilePath("funfair-content.json")));wheel=JsonUtility.FromJson<Wheel>(File.ReadAllText(FilePath("wheel.json")));if(wheel.seats?.Length!=216||content.slide?.Length!=141)throw new InvalidDataException("Incomplete Funfair geometry.");return data=pack;}}
        public static Wheel WheelData{get{_ = Data;return wheel;}}
        public static Vector2 Slide(double seconds){_ = Data;float f=Mathf.Clamp((float)seconds*10,0,140);int i=(int)f;return Vector2.Lerp(content.slide[i],content.slide[Math.Min(140,i+1)],f-i);}
        public static BirthdayArt.Frames Action(string role,string pose)=>Data?.actions.FirstOrDefault(a=>a.actor==role&&a.action==pose)??Data?.actions.FirstOrDefault(a=>a.actor==role&&a.action==pose+"-front")??Data?.actions.FirstOrDefault(a=>a.actor==role&&a.action==(pose.StartsWith("slide-")?"sit-front":pose=="fairy"?"happy-front":"idle-front"));
        public static BirthdayArt.Prop Prop(string key)=>Data?.props?.FirstOrDefault(p=>p.name==key);
        public static Texture2D Texture(string relative){if(textures.TryGetValue(relative,out var t))return t;t=new Texture2D(2,2,TextureFormat.RGBA32,false){name="Funfair "+relative,wrapMode=TextureWrapMode.Clamp};if(!t.LoadImage(File.ReadAllBytes(FilePath(relative)),false))throw new InvalidDataException(relative);if(!relative.StartsWith("world-"))alpha[t]=t.GetPixels32().Select(p=>p.a).ToArray();t.Apply(false,true);textures[relative]=t;return t;}
        public static Texture2D Picture(string key){_ = Data;var e=content.pictures.FirstOrDefault(v=>v.key==key);return e==null?null:Texture(e.file);}
        public static Sprite MenuPicture(){var a=Action("Peppa","idle");return a==null?null:Sprite.Create(Texture(a.texture),new Rect(0,Texture(a.texture).height-256,256,256),Vector2.one*.5f);}
        public static System.Collections.IEnumerator Audio(string key,Action<AudioClip> done){if(sounds.TryGetValue(key,out var clip)){done(clip);yield break;}_=Data;var e=content.audio.FirstOrDefault(a=>a.key==key);if(e==null)yield break;using(var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(FilePath(e.file)).AbsoluteUri,AudioType.WAV)){yield return request.SendWebRequest();if(request.result!=UnityWebRequest.Result.Success)yield break;clip=DownloadHandlerAudioClip.GetContent(request);sounds[key]=clip;done(clip);}}
        public static bool PixelVisible(Texture texture,Vector2 uv){if(!(texture is Texture2D t)||!alpha.TryGetValue(t,out var pixels))return true;return pixels[Mathf.Clamp((int)(uv.y*t.height),0,t.height-1)*t.width+Mathf.Clamp((int)(uv.x*t.width),0,t.width-1)]>25;}
        public static void Release(){foreach(var t in textures.Values)UnityEngine.Object.Destroy(t);foreach(var s in sounds.Values)UnityEngine.Object.Destroy(s);textures.Clear();sounds.Clear();alpha.Clear();data=null;content=null;wheel=null;}
    }
}
