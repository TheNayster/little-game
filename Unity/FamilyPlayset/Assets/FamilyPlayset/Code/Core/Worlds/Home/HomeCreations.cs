using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    public sealed class StoredFood
    {
        public int key; public string owner; public FoodDish dish;
        public StoredFood Copy()=>new StoredFood{key=key,owner=owner,dish=dish.Copy()};
    }
    public sealed class StoredPicture
    {
        public int key,page; public string owner; public bool displayed; public ColoringPage drawing;
        public StoredPicture Copy()=>new StoredPicture{key=key,page=page,owner=owner,displayed=displayed,drawing=drawing.Copy()};
    }
    public sealed class CreationCollection
    {
        public int serial;
        public StoredFood[] foods=Array.Empty<StoredFood>();
        public StoredPicture[] pictures=Array.Empty<StoredPicture>(),removed=Array.Empty<StoredPicture>();
        public CreationCollection Copy()=>new CreationCollection{serial=serial,foods=foods.Select(f=>f.Copy()).ToArray(),pictures=pictures.Select(p=>p.Copy()).ToArray(),removed=removed.Select(p=>p.Copy()).ToArray()};
    }
    public static class HomeCreations
    {
        public const int Schema=23,FoodPlaces=2,PicturePlaces=4,MaxEncoded=8192,MaxRaw=65536;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        private static readonly Dictionary<string,CreationCollection> cache=new Dictionary<string,CreationCollection>();
        public static string FoodToken(FoodDish d)=>d.id+"@"+d.stage+"@"+d.step+"@"+d.portions;
        // A bounded versioned binary payload avoids repeating JSON property names
        // and food details in every frequent world view. It is also in the full
        // save, recovery and private continuation: a thumbnail is never the data.
        public static string Encode(CreationCollection data)
        {
            using(var raw=new MemoryStream()){
                using(var w=new BinaryWriter(raw,Utf8,true)){
                    w.Write((byte)2);w.Write(data.serial);w.Write((byte)data.foods.Length);
                    foreach(var f in data.foods){w.Write(f.key);WriteString(w,f.owner);WriteFood(w,f.dish);}
                    WritePictures(w,data.pictures);WritePictures(w,data.removed);
                }
                if(raw.Length>MaxRaw)throw new InvalidDataException("Creation storage is full.");
                using(var packed=new MemoryStream()){
                    using(var deflate=new DeflateStream(packed,CompressionLevel.Optimal,true)){raw.Position=0;raw.CopyTo(deflate);}
                    var result=Convert.ToBase64String(packed.ToArray());if(result.Length>MaxEncoded)throw new InvalidDataException("Creation storage is full.");return result;
                }
            }
        }
        public static CreationCollection Decode(string encoded)
        {
            if(string.IsNullOrEmpty(encoded))return new CreationCollection();
            if(encoded.Length>MaxEncoded)throw new InvalidOperationException("Oversized creation storage.");
            lock(cache){if(cache.TryGetValue(encoded,out var found))return found.Copy();}
            try{
                using(var input=new MemoryStream(Convert.FromBase64String(encoded)))
                using(var deflate=new DeflateStream(input,CompressionMode.Decompress))
                using(var raw=new MemoryStream()){
                    var buffer=new byte[2048];int n;
                    while((n=deflate.Read(buffer,0,buffer.Length))>0){if(raw.Length+n>MaxRaw)throw new InvalidDataException("Oversized decoded creations.");raw.Write(buffer,0,n);}
                    raw.Position=0;
                    using(var r=new BinaryReader(raw,Utf8,true)){
                        var version=r.ReadByte();if(version!=1 && version!=2)throw new InvalidDataException("Unknown creation format.");
                        var data=new CreationCollection{serial=r.ReadInt32()};var count=Count(r,FoodPlaces*4);
                        data.foods=Enumerable.Range(0,count).Select(_=>new StoredFood{key=r.ReadInt32(),owner=ReadString(r),dish=ReadFood(r,version)}).ToArray();
                        data.pictures=ReadPictures(r,PicturePlaces*4);data.removed=ReadPictures(r,4);
                        if(raw.Position!=raw.Length)throw new InvalidDataException("Trailing creation data.");
                        lock(cache){if(cache.Count>=4)cache.Clear();cache[encoded]=data.Copy();}return data;
                    }
                }
            }catch(Exception e) when(e is IOException || e is FormatException || e is ArgumentException){throw new InvalidOperationException("Invalid saved creations.",e);}
        }
        private static int Count(BinaryReader r,int max){var n=r.ReadByte();if(n>max)throw new InvalidDataException("Creation count exceeds limit.");return n;}
        private static void WriteString(BinaryWriter w,string value)
        {if(value==null){w.Write((ushort)65535);return;}var bytes=Utf8.GetBytes(value);if(bytes.Length>512)throw new InvalidDataException("Creation string exceeds limit.");w.Write((ushort)bytes.Length);w.Write(bytes);}
        private static string ReadString(BinaryReader r)
        {var n=r.ReadUInt16();if(n==65535)return null;if(n>512)throw new InvalidDataException("Creation string exceeds limit.");var b=r.ReadBytes(n);if(b.Length!=n)throw new EndOfStreamException();return Utf8.GetString(b);}
        private static void WriteFood(BinaryWriter w,FoodDish d)
        {
            WriteString(w,d.id);WriteString(w,d.recipe);w.Write(d.step);w.Write(d.portions);w.Write(d.heated);w.Write(d.assisted);w.Write(d.guided);w.Write(d.experiment);
            w.Write(d.recipeVersion);w.Write(d.layers);w.Write(d.icingMask);w.Write(d.cutMask);WriteString(w,d.stage);w.Write(d.mixed);w.Write(d.poured);w.Write(d.heat);WriteString(w,d.preparation);
            w.Write((byte)d.ingredients.Length);foreach(var a in d.ingredients){WriteString(w,a.unit);WriteString(w,a.ingredient);WriteString(w,a.by);WriteString(w,a.phase);w.Write(a.x);w.Write(a.y);}
        }
        private static FoodDish ReadFood(BinaryReader r,int version)
        {
            var d=new FoodDish{id=ReadString(r),recipe=ReadString(r),step=r.ReadInt32(),portions=r.ReadInt32(),heated=r.ReadBoolean(),assisted=r.ReadBoolean(),guided=r.ReadBoolean(),experiment=r.ReadBoolean(),recipeVersion=r.ReadInt32(),layers=r.ReadInt32(),icingMask=r.ReadInt32(),cutMask=r.ReadInt32(),stage=ReadString(r),mixed=r.ReadSingle(),poured=r.ReadSingle(),heat=r.ReadDouble()};
            if(version>=2)d.preparation=ReadString(r);
            d.ingredients=Enumerable.Range(0,Count(r,24)).Select(_=>new FoodAddition{unit=ReadString(r),ingredient=ReadString(r),by=ReadString(r),phase=ReadString(r),x=r.ReadSingle(),y=r.ReadSingle()}).ToArray();return d;
        }
        private static void WritePictures(BinaryWriter w,StoredPicture[] pictures)
        {
            w.Write((byte)pictures.Length);foreach(var p in pictures){w.Write(p.key);WriteString(w,p.owner);w.Write((byte)p.page);w.Write(p.displayed);w.Write(p.drawing.revision);
                foreach(var values in new[]{p.drawing.colors,p.drawing.undo,p.drawing.redo}){w.Write((byte)values.Length);foreach(var n in values)w.Write((ushort)n);}}
        }
        private static StoredPicture[] ReadPictures(BinaryReader r,int max)
        {
            return Enumerable.Range(0,Count(r,max)).Select(_=>{
                var p=new StoredPicture{key=r.ReadInt32(),owner=ReadString(r),page=r.ReadByte(),displayed=r.ReadBoolean(),drawing=new ColoringPage{revision=r.ReadInt64()}};
                int[] ReadValues(int limit)=>Enumerable.Range(0,Count(r,limit)).Select(__=>(int)r.ReadUInt16()).ToArray();
                p.drawing.colors=ReadValues(114);p.drawing.undo=ReadValues(Discovery.HistoryLimit);p.drawing.redo=ReadValues(Discovery.HistoryLimit);return p;
            }).ToArray();
        }
    }
    public sealed partial class GameWorld
    {
        public string ReadCreationData()=>state.homeCreations;
        public CreationCollection ReadCreations()=>HomeCreations.Decode(state.homeCreations);
        public static GameWorld WithHomeCreations(GameWorld world)
        {
            world=WithHomeTidying(world);if(world.Schema>=HomeCreations.Schema)return world;
            var s=world.Snapshot();s.schema=HomeCreations.Schema;s.homeCreations="";s.revision++;Validate(s);return new GameWorld(s);
        }
        private static void ValidateCreations(SoloSnapshot s)
        {
            if(s.schema<HomeCreations.Schema){if(!string.IsNullOrEmpty(s.homeCreations))throw new InvalidOperationException("Creations require schema 23.");return;}
            var data=HomeCreations.Decode(s.homeCreations);var seen=new HashSet<int>();
            bool Key(int key,string owner)=>key>0 && key<=data.serial && seen.Add(key) && s.players.Any(p=>p.id==owner);
            if(data.serial<0 || data.serial>1000000000)throw new InvalidOperationException("Invalid creation serial.");
            foreach(var f in data.foods){if(!Key(f.key,f.owner) || f.dish.portions==0)throw new InvalidOperationException("Invalid stored food.");ValidateFood(f.dish,ToyKind.Cookware,s);}
            foreach(var p in data.pictures.Concat(data.removed)){
                if(!Key(p.key,p.owner) || p.page<0 || p.page>=Discovery.Pages.Length)throw new InvalidOperationException("Invalid stored picture.");
                ValidateColoringPage(p.drawing,Discovery.Regions[p.page]);
            }
            foreach(var p in s.players){if(data.foods.Count(f=>f.owner==p.id)>HomeCreations.FoodPlaces || data.pictures.Count(f=>f.owner==p.id)>HomeCreations.PicturePlaces || data.removed.Count(f=>f.owner==p.id)>1 || data.pictures.Count(f=>f.owner==p.id && f.displayed)>1)throw new InvalidOperationException("Full personal collection.");}
        }
        private bool SaveCreations(CreationCollection data)
        {try{state.homeCreations=HomeCreations.Encode(data);return true;}catch(InvalidDataException){return false;}}
        private string StoreFood(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            if(state.schema<HomeCreations.Schema || p.zone!="garden")return "use-kitchen-storage";
            var data=ReadCreations();
            if(c.value=="store-food"){
                var d=item?.kitchen?.dish;
                if(!PrepAvailable(item,p) || !Kitchen.Dish(item.kind) || item.holder!="" || d==null || d.portions==0)return "need-unheld-food";
                if(item.kind==ToyKind.Cookware && !string.IsNullOrEmpty(item.kitchen.cook) && item.kitchen.cook!=p.id)return "owner-only";
                if(HomeCreations.FoodToken(d)!=c.target)return "food-changed";
                if(MealFlow.Active(d)?Kitchen.Heating(item):d.heat>0 && !d.heated)return "food-still-cooking";
                if(data.foods.Count(f=>f.owner==p.id)>=HomeCreations.FoodPlaces || data.serial>=1000000000)return "food-storage-full";
                data.foods=data.foods.Concat(new[]{new StoredFood{key=++data.serial,owner=p.id,dish=d.Copy()}}).ToArray();
                if(!SaveCreations(data))return "creation-storage-full";
                item.kitchen.dish=null;item.kitchen.dirty=false;item.kitchen.cook="";Touch(item);return null;
            }
            if(c.value!="restore-food" || !int.TryParse(c.target,out var key))return "unknown-storage-action";
            var saved=data.foods.FirstOrDefault(f=>f.key==key && f.owner==p.id);if(saved==null)return "stored-food-missing";
            if(item==null)item=state.toys.FirstOrDefault(t=>t.kind==ToyKind.Cookware && PrepAvailable(t,p) && t.holder=="" && t.kitchen.dish==null && !t.kitchen.dirty && (string.IsNullOrEmpty(t.kitchen.cook) || t.kitchen.cook==p.id));
            if(!PrepAvailable(item,p) || item.kind!=ToyKind.Cookware || item.holder!="" || item.kitchen.dish!=null || item.kitchen.dirty || !string.IsNullOrEmpty(item.kitchen.cook) && item.kitchen.cook!=p.id)return "need-empty-clean-tray";
            data.foods=data.foods.Where(f=>f.key!=key).ToArray();if(!SaveCreations(data))return "creation-storage-full";
            item.kitchen.dish=saved.dish.Copy();item.kitchen.cook=p.id;Touch(item);return null;
        }
        private string PictureCollection(SoloCommand c,SoloPlayer p,DiscoveryWorkspace work)
        {
            if(state.schema<HomeCreations.Schema)return "picture-storage-unavailable";
            var data=ReadCreations();
            if(c.value=="save-picture"){
                var parts=c.target.Split('@');if(parts.Length!=2 || !int.TryParse(parts[0],out var page) || page<0 || page>=work.pages.Length || !long.TryParse(parts[1],out var revision) || revision!=work.pages[page].revision)return "page-changed";
                var drawing=work.pages[page];if(!drawing.colors.Any(color=>color!=0))return "picture-empty";
                if(data.pictures.Any(q=>q.owner==p.id && q.page==page && q.drawing.colors.SequenceEqual(drawing.colors)))return null;
                if(data.pictures.Count(q=>q.owner==p.id)>=HomeCreations.PicturePlaces || data.serial>=1000000000)return "picture-folder-full";
                data.pictures=data.pictures.Concat(new[]{new StoredPicture{key=++data.serial,owner=p.id,page=page,drawing=drawing.Copy()}}).ToArray();
            }else{
                if(!int.TryParse(c.target,out var key))return "picture-missing";
                var picture=data.pictures.FirstOrDefault(q=>q.key==key && q.owner==p.id);
                if(c.value=="undo-picture-remove"){
                    var removed=data.removed.FirstOrDefault(q=>q.key==key && q.owner==p.id);if(removed==null || data.pictures.Count(q=>q.owner==p.id)>=HomeCreations.PicturePlaces)return "picture-folder-full";
                    if(removed.displayed)foreach(var q in data.pictures.Where(q=>q.owner==p.id))q.displayed=false;
                    data.pictures=data.pictures.Concat(new[]{removed}).ToArray();data.removed=data.removed.Where(q=>q.key!=key).ToArray();
                }else{
                    if(picture==null)return "owner-only";
                    if(c.value=="display-picture"){var shown=picture.displayed;foreach(var q in data.pictures.Where(q=>q.owner==p.id))q.displayed=false;picture.displayed=!shown;}
                    else if(c.value=="remove-picture"){data.removed=data.removed.Where(q=>q.owner!=p.id).Concat(new[]{picture}).ToArray();data.pictures=data.pictures.Where(q=>q.key!=key).ToArray();}
                    else return "unknown-picture-action";
                }
            }
            return SaveCreations(data)?null:"creation-storage-full";
        }
    }
}
