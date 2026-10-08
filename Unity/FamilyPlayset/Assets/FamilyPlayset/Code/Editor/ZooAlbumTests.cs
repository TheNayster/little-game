using System;
using System.IO;
using System.Linq;
using LittleWeeps.Client;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class ZooAlbumTests
    {
        private static void Need(bool value,string message){if(!value)throw new InvalidOperationException("Zoo album: "+message);}
        public static void Run()
        {
            var scope="verification/"+Guid.NewGuid().ToString("N");
            var album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);
            var texture=new Texture2D(16,12,TextureFormat.RGB24,false);byte[] jpg;
            try{texture.SetPixels(Enumerable.Repeat(Color.green,192).ToArray());texture.Apply();jpg=texture.EncodeToJPG(82);}finally{UnityEngine.Object.DestroyImmediate(texture);}
            ZooPhoto Photo()=>new ZooPhoto{id=Guid.NewGuid().ToString("N"),exhibit="elephant",width=16,height=12};
            for(var i=0;i<24;i++)Need(album.Add(Photo(),jpg,jpg,out var error),"save "+i+": "+error);
            Need(!album.Add(Photo(),jpg,jpg,out _) && album.Manifest.photos.Count==24,"capacity");
            var first=album.Manifest.photos[0];var bytes=File.ReadAllBytes(album.ImagePath(first.id));
            Need(album.Change(m=>m.photos[0].stickers.Add(new ZooSticker{kind=2,cell=5}),out _),"sticker save");
            Need(File.ReadAllBytes(album.ImagePath(first.id)).SequenceEqual(bytes),"immutable original");
            album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);Need(!album.ReadOnly && album.Manifest.photos.Count==24 && album.Manifest.photos[0].stickers[0].cell==5,"JSON reload");
            Need(album.Remove(first.id,out _) && album.Manifest.photos.Count==23,"remove");
            album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);Need(album.Undo(out _) && album.Manifest.photos[0].id==first.id,"persistent undo");
            Need(!album.Change(m=>m.photos[0].stickers.Add(new ZooSticker{kind=10,cell=100}),out _),"invalid metadata rejected");
            Need(album.Manifest.photos[0].stickers.Count==1,"invalid edit rollback");
            var healthy=album.Manifest.photos[2].id;
            File.WriteAllBytes(album.ImagePath(first.id),new byte[]{1,2,3});Need(album.Load(first.id,false)==null,"corrupt JPEG");
            var image=album.Load(healthy,false);Need(image!=null,"healthy JPEG survives");UnityEngine.Object.DestroyImmediate(image);
            try{album.ImagePath("../outside");throw new Exception("path accepted");}catch(InvalidDataException){}
            var committed=Directory.GetFiles(album.DirectoryPath,"album-*.json").OrderByDescending(p=>JsonUtility.FromJson<ZooAlbumManifest>(File.ReadAllText(p)).generation).First();
            File.WriteAllText(committed+".pending","{interrupted");album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);Need(album.Manifest.photos.Count==24,"interrupted manifest");
            // Malformed per-photo metadata flags only that entry.
            var manifest=JsonUtility.FromJson<ZooAlbumManifest>(File.ReadAllText(committed));manifest.photos[1].stickers.Add(new ZooSticker{kind=99,cell=4});File.WriteAllText(committed,JsonUtility.ToJson(manifest));
            album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);Need(album.Manifest.photos.Count==23 && album.Manifest.photos.Any(p=>p.id==healthy),"isolate bad entry");
            // Simulate a crashed new-image save with its exact generated journal.
            var orphan=Photo();File.WriteAllText(Path.Combine(album.DirectoryPath,"capture.pending"),orphan.id);File.WriteAllBytes(album.ImagePath(orphan.id),jpg);File.WriteAllBytes(album.ImagePath(orphan.id,true)+".pending",jpg);
            album=new ZooPhotoAlbum(Application.temporaryCachePath,scope);Need(!File.Exists(album.ImagePath(orphan.id)) && !File.Exists(album.ImagePath(orphan.id,true)+".pending") && File.Exists(album.ImagePath(healthy)),"exact interrupted image recovery");
            var before=album.Manifest.generation;var pending=Path.Combine(album.DirectoryPath,"album-"+((before+1)%2)+".json.pending");File.Delete(pending);Directory.CreateDirectory(pending);
            Need(!album.Change(m=>m.photos.Reverse(),out _) && album.Manifest.generation==before,"I/O failure retains manifest");
            Debug.Log("ZOO_ALBUM_PASS: 24 bound, immutable originals, native JSON restart, stickers, persistent undo, invalid edits, paths, corrupt image, interrupted manifest/image, per-entry metadata and I/O failure retention.");
        }
    }
}
