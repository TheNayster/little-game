using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LittleWeeps.Client
{
    [Serializable] public sealed class ZooSticker { public int kind,cell; }
    [Serializable] public sealed class ZooPhoto
    {
        public string id,exhibit;
        public int width,height;
        public List<ZooSticker> stickers=new List<ZooSticker>();
    }
    [Serializable] public sealed class ZooAlbumManifest
    {
        public int version=1;
        public long generation;
        public List<ZooPhoto> photos=new List<ZooPhoto>();
        public List<ZooPhoto> recovery=new List<ZooPhoto>();
        public ZooPhoto Removed { get=>recovery.Count==0?null:recovery[0]; set { recovery.Clear();if(value!=null)recovery.Add(value); } }
        public int removedIndex;
    }
    // Local only: never enters SoloSnapshot, a gameplay command or networking.
    // Two alternating complete manifests keep the last committed generation
    // available after an interrupted write. Image files are immutable originals.
    public sealed class ZooPhotoAlbum
    {
        public const int PhotoLimit=24,StickerLimit=6,MaxDimension=960,ThumbnailDimension=192;
        public readonly string DirectoryPath;
        public ZooAlbumManifest Manifest { get; private set; }
        public bool ReadOnly { get; private set; }
        public string Warning { get; private set; }="";
        public ZooPhotoAlbum(string persistentPath,string scope)
        {
            using(var sha=SHA256.Create()){
                var key=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(scope))).Replace("-","").ToLowerInvariant();
                DirectoryPath=Path.Combine(persistentPath,"ZooAlbums",key);
            }
            Manifest=new ZooAlbumManifest();
            try{
                Directory.CreateDirectory(DirectoryPath);
                var found=false;
                for(var slot=0;slot<2;slot++){
                    var path=Path.Combine(DirectoryPath,"album-"+slot+".json");
                    if(!File.Exists(path))continue;
                    found=true;
                    try{
                        if(new FileInfo(path).Length>128*1024)throw new InvalidDataException();
                        var m=JsonUtility.FromJson<ZooAlbumManifest>(File.ReadAllText(path));
                        if(m==null || m.version!=1){ReadOnly=true;Warning="This album needs a grown-up's help.";continue;}
                        // A malformed entry is isolated instead of discarding
                        // otherwise healthy pictures in this generation.
                        if(m.photos!=null && m.photos.Count<=PhotoLimit){
                            m.photos=m.photos.Where(p=>{try{ValidatePhoto(p);return true;}catch(Exception){Warning="One picture needs help. Other pictures are kept.";return false;}}).GroupBy(p=>p.id).Select(g=>g.First()).ToList();
                            if(m.recovery!=null && m.recovery.Count==1)try{ValidatePhoto(m.recovery[0]);}catch(Exception){m.recovery.Clear();}
                        }
                        Validate(m);
                        if(m.generation>Manifest.generation)Manifest=m;
                    }catch(Exception){Warning="Some album data could not be opened. Healthy pictures are kept.";}
                }
                if(found && Manifest.generation==0){ReadOnly=true;Warning="Your album is kept safe. Ask a grown-up for help.";}
                // A single exact capture journal bounds crash leftovers. The
                // current committed generation decides whether that image won.
                var journal=Path.Combine(DirectoryPath,"capture.pending");
                if(!ReadOnly && File.Exists(journal)){
                    var id=File.ReadAllText(journal);
                    if(Id(id) && !Manifest.photos.Any(p=>p.id==id) && Manifest.Removed?.id!=id){
                        foreach(var thumb in new[]{false,true}){var image=ImagePath(id,thumb);File.Delete(image);File.Delete(image+".pending");}
                    }
                    File.Delete(journal);
                }
            }catch(Exception){ReadOnly=true;Warning="Pictures cannot be saved yet. Ask a grown-up for help.";}
        }
        private static bool Id(string id)=>id!=null && Guid.TryParseExact(id,"N",out _);
        private static void ValidatePhoto(ZooPhoto p)
        {
            if(p==null || !Id(p.id) || p.width<1 || p.height<1 || p.width>MaxDimension || p.height>MaxDimension || p.stickers==null || p.stickers.Count>StickerLimit)throw new InvalidDataException();
            if(p.stickers.Any(s=>s==null || s.kind<0 || s.kind>3 || s.cell<0 || s.cell>8))throw new InvalidDataException();
        }
        private static void Validate(ZooAlbumManifest m)
        {
            if(m.generation<1 || m.photos==null || m.photos.Count>PhotoLimit || m.recovery==null || m.recovery.Count>1)throw new InvalidDataException();
            foreach(var p in m.photos)ValidatePhoto(p);
            if(m.Removed!=null)ValidatePhoto(m.Removed);
            if(m.photos.Select(p=>p.id).Distinct().Count()!=m.photos.Count || m.Removed!=null && m.photos.Any(p=>p.id==m.Removed.id))throw new InvalidDataException();
        }
        public string ImagePath(string id,bool thumbnail=false)
        {
            if(!Id(id))throw new InvalidDataException("Invalid photo identity.");
            var path=Path.GetFullPath(Path.Combine(DirectoryPath,id+(thumbnail?"-thumb.jpg":".jpg")));
            var root=Path.GetFullPath(DirectoryPath)+Path.DirectorySeparatorChar;
            if(!path.StartsWith(root,StringComparison.Ordinal))throw new InvalidDataException("Photo outside album.");
            return path;
        }
        public bool Change(Action<ZooAlbumManifest> edit,out string error)
        {
            error="";
            if(ReadOnly){error=Warning;return false;}
            try{
                var next=JsonUtility.FromJson<ZooAlbumManifest>(JsonUtility.ToJson(Manifest));
                edit(next);next.generation=checked(Manifest.generation+1);Validate(next);
                var json=Encoding.UTF8.GetBytes(JsonUtility.ToJson(next));
                var target=Path.Combine(DirectoryPath,"album-"+(next.generation%2)+".json");
                var temp=target+".pending";
                using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(json,0,json.Length);stream.Flush(true);}
                if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);
                Manifest=next;return true;
            }catch(Exception e){Debug.LogWarning("Zoo album save: "+e.GetType().Name);error="Could not save yet. Your pictures are still here.";return false;}
        }
        public bool Add(ZooPhoto photo,byte[] image,byte[] thumb,out string error)
        {
            error="";if(Manifest.photos.Count>=PhotoLimit){error="Album full — choose a picture to remove first.";return false;}
            if(ReadOnly){error=Warning;return false;}
            try{
                var journal=Path.Combine(DirectoryPath,"capture.pending");
                using(var stream=new FileStream(journal,FileMode.Create,FileAccess.Write,FileShare.None)){var bytes=Encoding.ASCII.GetBytes(photo.id);stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
                WriteNew(ImagePath(photo.id),image);WriteNew(ImagePath(photo.id,true),thumb);
                if(Change(m=>m.photos.Add(photo),out error)){try{File.Delete(journal);}catch(Exception){}return true;}
            }catch(Exception){error="Could not save this picture. Try again later.";}
            // Delete only the two exact new files owned by this failed capture.
            try{foreach(var thumbFile in new[]{false,true}){var path=ImagePath(photo.id,thumbFile);File.Delete(path);File.Delete(path+".pending");}File.Delete(Path.Combine(DirectoryPath,"capture.pending"));}catch(Exception){}
            return false;
        }
        private static void WriteNew(string path,byte[] bytes)
        {
            var temp=path+".pending";
            using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            File.Move(temp,path);
        }
        public bool Remove(string id,out string error)
        {
            var old=Manifest.Removed;
            var ok=Change(m=>{var i=m.photos.FindIndex(p=>p.id==id);if(i<0)throw new InvalidDataException();m.Removed=m.photos[i];m.removedIndex=i;m.photos.RemoveAt(i);},out error);
            // Old recovery is retired only after BOTH committed slots have
            // stopped referencing it. No enumeration or broad cleanup occurs.
            if(ok && old!=null && old.id!=id){
                if(Change(m=>{},out _))try{File.Delete(ImagePath(old.id));File.Delete(ImagePath(old.id,true));}catch(Exception){}
            }
            return ok;
        }
        public bool Undo(out string error)=>Change(m=>{if(m.Removed==null || m.photos.Count>=PhotoLimit)throw new InvalidOperationException();m.photos.Insert(Mathf.Clamp(m.removedIndex,0,m.photos.Count),m.Removed);m.Removed=null;},out error);
        public Texture2D Load(string id,bool thumbnail)
        {
            Texture2D texture=null;
            try{
                var path=ImagePath(id,thumbnail);var info=new FileInfo(path);
                if(!info.Exists || info.Length>2*1024*1024)return null;
                // Validate JPEG dimensions before native decoding; corrupt files
                // cannot request an unbounded texture allocation.
                var bytes=File.ReadAllBytes(path);
                if(!JpegDimensions(bytes,thumbnail?ThumbnailDimension:MaxDimension))return null;
                texture=new Texture2D(2,2,TextureFormat.RGB24,false);
                if(!ImageConversion.LoadImage(texture,bytes,true)){UnityEngine.Object.Destroy(texture);return null;}
                return texture;
            }catch(Exception){if(texture!=null)UnityEngine.Object.Destroy(texture);return null;}
        }
        private static bool JpegDimensions(byte[] b,int limit)
        {
            if(b.Length<4 || b[0]!=255 || b[1]!=216)return false;
            for(var p=2;p+3<b.Length;){
                if(b[p++]!=255)return false;
                while(p<b.Length && b[p]==255)p++;
                if(p>=b.Length)return false;var marker=b[p++];
                if(marker==217 || marker==218)return false;
                if(marker==216 || marker==1 || marker>=208 && marker<=215)continue;
                if(p+1>=b.Length)return false;var n=(b[p]<<8)|b[p+1];if(n<2 || p+n>b.Length)return false;
                if(marker>=192 && marker<=195){if(n<8)return false;var h=(b[p+3]<<8)|b[p+4];var w=(b[p+5]<<8)|b[p+6];return h>0 && w>0 && h<=limit && w<=limit;}
                p+=n;
            }
            return false;
        }
    }
}
