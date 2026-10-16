using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LittleWeeps.Client
{
    // The local preview and packaged player consume the same immutable recovered pack.
    public static class GoForADriveArt
    {
        [Serializable] public sealed class Entry { public string key,file;public Vector2 pivot,holdPivot;public float unitsPerPixel; }
        [Serializable] public sealed class Bounds {public float x,y,width,height;}
        [Serializable] public sealed class StatuePiece {public int index;public string key;public Vector2 point;public Bounds bounds;}
        [Serializable] public sealed class StatueLayout {public int width,height;public StatuePiece[] rows;}
        [Serializable] public sealed class Content
        {
            public int format;
            public Entry[] pictures,audio;
            public BirthdayArt.Frames[] actions;
            public StatueLayout statueLayout;
        }
        private static Content content;
        private static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        private static readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        private static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static string Root {
            get {
                var supplied=Environment.GetEnvironmentVariable("LITTLE_WEEPS_DRIVE_ART");
                if(!string.IsNullOrWhiteSpace(supplied))return Path.GetFullPath(supplied);
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../LocalData/Research/GoForADrive/Content-fixtures"));
#else
                return Path.Combine(Application.streamingAssetsPath,"GoForADrive");
#endif
            }
        }
        public static bool Available=>File.Exists(Path.Combine(Root,"drive-content.json"));
        public static Content Data {
            get {
                if(content!=null || !Available)return content;
                var candidate=JsonUtility.FromJson<Content>(File.ReadAllText(Path.Combine(Root,"drive-content.json")));
                if(candidate==null || candidate.format!=1 || candidate.pictures==null || candidate.audio==null || candidate.actions==null || candidate.statueLayout?.rows==null || candidate.statueLayout.rows.Length!=12 ||
                    candidate.pictures.Any(p=>p==null || string.IsNullOrEmpty(p.key) || string.IsNullOrEmpty(p.file)) ||
                    candidate.audio.Any(p=>p==null || string.IsNullOrEmpty(p.key) || string.IsNullOrEmpty(p.file)) ||
                    candidate.pictures.Select(p=>p.key).Distinct().Count()!=candidate.pictures.Length || candidate.audio.Select(p=>p.key).Distinct().Count()!=candidate.audio.Length ||
                    candidate.actions.Any(a=>a==null || a.frames<1 || a.columns<1 || a.size<1 || a.duration<0 || a.frames>1 && a.duration<=0 || a.unitsPerPixel<=0 || a.hold==null || a.hold.Length!=a.frames || a.face==null || a.face.Length!=a.frames))
                    throw new InvalidDataException("Incomplete Go for a Drive content pack.");
                content=candidate;return content;
            }
        }
        private static string FilePath(string relative)
        {
            if(string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Split('/','\\').Any(p=>p==".."))throw new InvalidDataException("Invalid drive content path.");
            return Path.Combine(Root,relative);
        }
        public static Texture2D Texture(string file)
        {
            if(textures.TryGetValue(file,out var texture))return texture;
            texture=new Texture2D(2,2,TextureFormat.RGBA32,false){name="Drive "+file,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            if(!texture.LoadImage(File.ReadAllBytes(FilePath(file)),false)){UnityEngine.Object.Destroy(texture);throw new InvalidDataException("Invalid drive picture: "+file);}
            texture.Apply(false,true);textures.Add(file,texture);return texture;
        }
        public static Texture2D Picture(string key)
        {var entry=Data?.pictures.SingleOrDefault(p=>p.key==key);return entry==null?null:Texture(entry.file);}
        public static Entry PictureInfo(string key)=>Data?.pictures.SingleOrDefault(p=>p.key==key);
        public static Sprite Icon(string key)
        {
            if(sprites.TryGetValue(key,out var sprite))return sprite;var texture=Picture(key);if(!texture)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);sprites.Add(key,sprite);return sprite;
        }
        public static BirthdayArt.Frames Action(string actor,string pose)=>Data?.actions.FirstOrDefault(a=>a.actor==actor && a.action==pose);
        public static IEnumerator LoadAudio(string key,Action<AudioClip> done)
        {
            if(clips.TryGetValue(key,out var clip)){done(clip);yield break;}
            var entry=Data?.audio.SingleOrDefault(a=>a.key==key);if(entry==null)yield break;
            using(var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(FilePath(entry.file)).AbsoluteUri,AudioType.WAV)){
                yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success){Debug.LogWarning("Drive audio: "+request.error);yield break;}
                clip=DownloadHandlerAudioClip.GetContent(request);clips.Add(key,clip);done(clip);
            }
        }
        public static void Release()
        {
            foreach(var sprite in sprites.Values)UnityEngine.Object.Destroy(sprite);
            foreach(var texture in textures.Values)UnityEngine.Object.Destroy(texture);
            foreach(var clip in clips.Values)UnityEngine.Object.Destroy(clip);
            sprites.Clear();textures.Clear();clips.Clear();content=null;
        }
    }
}
