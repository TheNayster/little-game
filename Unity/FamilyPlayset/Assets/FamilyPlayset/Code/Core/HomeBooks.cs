using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // A shared physical copy and an independently readable title are different
    // identities. Reading never locks a page cursor in the shared world.
    public static class HomeBooks
    {
        public static bool BookmarkExpired(long lastUseUtc,long nowUtc)=>lastUseUtc<=0 || nowUtc>=lastUseUtc && nowUtc-lastUseUtc>=TimeSpan.FromSeconds(HomeTidying.IdleSeconds).Ticks;
        public const int FirstSchema=10, Schema=11;
        public const string Title="hello-dinosaurs", CopyId="living-book-hello";
        public static readonly string[] Titles={Title,"little-bridge","rocket-moon","fairy-garden","princess-star","mermaid-shell"};
        public static bool Available(string id,int schema)=>Index(id)>=0 && (schema>=Schema || schema==FirstSchema && id!="living-book-rocket-moon" && id!="living-book-princess-star");
        public static string Copy(int index)=>index==0?CopyId:"living-book-"+Titles[index];
        public static int Index(string id){for(var i=0;i<Titles.Length;i++)if(id==Copy(i))return i;return -1;}
        public const float RackX=-4500, RackY=240;
        public static string Support(int i)=>"living-books-"+i;
        public static int Slot(string id){for(var i=0;i<6;i++)if(id==Support(i))return i;return -1;}
        public static float X(int i)=>RackX+(i%3-1)*95;
        public static float Y(int i)=>i<3?300:420;
        public static int ExtraStock(int schema)=>schema>=Schema?Titles.Length:schema>=FirstSchema?4:0;
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithBooks(SoloWorld world)
        {
            world=WithSecretRooms(world);if(world.Schema>=HomeBooks.Schema)return world;
            var s=world.Snapshot();s.schema=HomeBooks.Schema;s.revision++;
            // Build 147 already owns four copies. Add missing identities only,
            // retaining every existing holder, storage location and bookmark.
            var stock=s.toys.ToList();
            for(var i=0;i<HomeBooks.Titles.Length;i++)
            {
                if(stock.Any(t=>t.id==HomeBooks.Copy(i)))continue;
                var slot=Enumerable.Range(0,6).First(j=>!stock.Any(t=>t.container==HomeBooks.Support(j)));
                stock.Add(new SoloToy{id=HomeBooks.Copy(i),kind=ToyKind.Book,zone="garden",container=HomeBooks.Support(slot),x=HomeBooks.X(slot),y=HomeBooks.Y(slot)});
            }
            s.toys=stock.ToArray();
            Validate(s);return new SoloWorld(s);
        }
        private static void ValidateBooks(SoloSnapshot s)
        {
            var books=s.toys.Where(t=>t.kind==ToyKind.Book).ToArray();
            if(s.schema<HomeBooks.FirstSchema){if(books.Length!=0)throw new InvalidOperationException("Books require schema 10.");return;}
            if(books.Length!=HomeBooks.ExtraStock(s.schema) || books.Select(t=>t.id).Distinct().Count()!=books.Length || books.Any(t=>!HomeBooks.Available(t.id,s.schema) || t.water!=0 || t.wet || t.resetPending || t.personalRoom!=""))
                throw new InvalidOperationException("Invalid fixed book stock.");
        }
        private string StoreBook(SoloCommand c,SoloPlayer player,SoloToy item,int slot)
        {
            if(state.schema<HomeBooks.FirstSchema || player.zone!="garden" || item.kind!=ToyKind.Book)return "invalid-target";
            if(state.toys.Any(t=>t.container==c.target))return "storage-full";
            if(Math.Abs(c.x-HomeBooks.X(slot))>75 || Math.Abs(c.y-HomeBooks.Y(slot))>75)return "target-too-far";
            item.container=c.target;item.holder="";item.x=HomeBooks.X(slot);item.y=HomeBooks.Y(slot);Touch(item);return null;
        }
    }

    // Local reader intent is explicit: a late load/name completion must never
    // resume after a newer pause, page turn, close or lifecycle interruption.
    public sealed class BookCursor
    {
        public int Pages {get;private set;}=14;
        public int ContentRevision {get;private set;}=2;
        public int Page {get;private set;}
        public int Sample {get;set;}
        public int Generation {get;private set;}
        public bool Open {get;private set;}
        public bool Playing {get;private set;}
        public bool AutoTurn {get;set;}
        public void Begin(int page,int sample,int revision,int pages=14,int contentRevision=2)
        {if(pages<1 || contentRevision<1)throw new ArgumentOutOfRangeException();Pages=pages;ContentRevision=contentRevision;Open=true;Playing=false;Page=revision==ContentRevision?Math.Max(0,Math.Min(Pages-1,page)):0;Sample=revision==ContentRevision?Math.Max(0,sample):0;Generation++;}
        public void Turn(int page){Page=Math.Max(0,Math.Min(Pages-1,page));Sample=0;Playing=false;Generation++;}
        public void Play(){if(Open){Playing=true;Generation++;}}
        public void Pause(){Playing=false;Generation++;}
        public void Close(){Pause();Open=false;}
        public bool Current(int generation)=>Open && generation==Generation;
        public bool CanResume(int generation)=>Current(generation) && Playing;
        public void ClampSamples(int samples){Sample=Math.Max(0,Math.Min(Math.Max(0,samples-1),Sample));}
    }
}
