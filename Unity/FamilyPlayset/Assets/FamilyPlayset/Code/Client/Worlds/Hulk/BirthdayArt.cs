using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            public string name,texture,view; public float unitsPerPixel;public Vector2 pivot,holdPivot;public long sourceBaseId,sourceHoldId;
        }
        [Serializable] public sealed class CakeSprite {public string name,texture,category;public Vector2 pivot;}
        [Serializable] public sealed class AudioEntry {public string key,file;public double duration;}
        [Serializable] public sealed class FloorCell {public long sourceRendererId;public Rect pixels,uv;}
        [Serializable] public sealed class Floor {public string texture;public float worldStart,interval;public Color offColor;public Color[] colors;public FloorCell[] cells;public Vector2 centre;}
        [Serializable] public sealed class Castle {public string texture,front;public Rect pixels;public int frames,columns;public float floorX,floorY;}
        [Serializable] public sealed class Pack {public int format,fps;public Frames[] actions;public Prop[] props;public CakeSprite[] cakeSprites;public AudioEntry[] audio;public Floor danceFloor;public Castle castle;}
        private static Pack pack;
        private static Dictionary<string,Frames> actions;
        private static Dictionary<string,Prop> props;
        private static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        private static readonly Dictionary<Texture2D,byte[]> alpha=new Dictionary<Texture2D,byte[]>();
        private static readonly Dictionary<string,AudioClip> audio=new Dictionary<string,AudioClip>();
        public const float PixelsPerUnit=1200/19.5f;
        public static string Root {
            get {
                var supplied=Environment.GetEnvironmentVariable("LITTLE_WEEPS_BIRTHDAY_ART");
                if(!string.IsNullOrWhiteSpace(supplied))return Path.GetFullPath(supplied);
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../LocalData/Research/DuploMarvel/world-tests/HulkCompleteProbe/Baked2D-v17"));
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
                if(candidate?.actions==null || candidate.props==null || candidate.format!=3 || candidate.fps!=12 || candidate.audio==null || candidate.audio.Length!=7 || candidate.danceFloor?.cells==null || candidate.danceFloor.cells.Length!=36 || candidate.actions.Any(a=>a==null || a.frames<1 || a.size!=256 || a.columns!=8 || a.unitsPerPixel<=0 || a.hold==null || a.hold.Length!=a.frames || a.face==null || a.face.Length!=a.frames))
                    throw new InvalidDataException("Incomplete birthday animation pack.");
                actions=candidate.actions.ToDictionary(a=>a.actor+"/"+a.action);
                props=candidate.props.ToDictionary(p=>p.name+"/"+p.view);
                pack=candidate;return pack;
            }
        }
        public static Frames Action(string actor,string action)=>Data!=null && actions.TryGetValue(actor+"/"+action,out var frame)?frame:null;
        public static Prop PropView(string name,string view)=>Data!=null && props.TryGetValue(name+"/"+view,out var prop)?prop:null;
        public static AudioClip Audio(string key)
        {
            if(audio.TryGetValue(key,out var cached))return cached;
            var entry=Data.audio?.SingleOrDefault(a=>a.key==key)??throw new InvalidDataException("Missing birthday recording: "+key);
            string relative=entry.file;
            if(Path.IsPathRooted(relative) || relative.Split('/','\\').Any(p=>p==".."))throw new InvalidDataException("Invalid birthday recording path.");
            using(var stream=File.OpenRead(Path.Combine(Root,relative)))using(var reader=new BinaryReader(stream)){
                string Four()=>Encoding.ASCII.GetString(reader.ReadBytes(4));
                if(Four()!="RIFF")throw new InvalidDataException("Expected PCM WAV.");reader.ReadUInt32();if(Four()!="WAVE")throw new InvalidDataException("Expected WAVE.");
                int format=0,channels=0,rate=0,bits=0;byte[] bytes=null;
                while(stream.Position+8<=stream.Length){
                    string chunk=Four();uint size=reader.ReadUInt32();long end=stream.Position+size;
                    if(end>stream.Length || size>32*1024*1024)throw new InvalidDataException("Invalid WAV chunk.");
                    if(chunk=="fmt "){
                        if(size<16)throw new InvalidDataException("Invalid WAV format.");
                        format=reader.ReadUInt16();channels=reader.ReadUInt16();rate=reader.ReadInt32();reader.ReadUInt32();reader.ReadUInt16();bits=reader.ReadUInt16();
                    }else if(chunk=="data")bytes=reader.ReadBytes((int)size);
                    stream.Position=end+(size&1);
                    if(bytes!=null && format!=0)break;
                }
                if(format!=1 || channels<1 || channels>2 || rate<8000 || rate>96000 || bits!=16 || bytes==null || bytes.Length==0 || bytes.Length%(channels*2)!=0)throw new InvalidDataException("Unsupported birthday PCM.");
                var samples=new float[bytes.Length/2];for(int i=0;i<samples.Length;i++)samples[i]=(short)(bytes[i*2]|bytes[i*2+1]<<8)/32768f;
                var clip=AudioClip.Create("Birthday "+key,samples.Length/channels,channels,rate,false);clip.SetData(samples,0);audio.Add(key,clip);return clip;
            }
        }
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
            foreach(var clip in audio.Values)if(clip)UnityEngine.Object.Destroy(clip);audio.Clear();
            textures.Clear();alpha.Clear();actions=null;props=null;pack=null;
        }
    }
}
