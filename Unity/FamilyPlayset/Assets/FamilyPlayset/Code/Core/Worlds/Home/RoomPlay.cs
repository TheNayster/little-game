using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class RoomPlay
    {
        public const int Schema=12;
        public static bool Tea(ToyKind kind)=>kind==ToyKind.TeaCup || kind==ToyKind.TeaPot;
        public static bool Stackable(ToyKind kind)=>kind==ToyKind.Plush || kind==ToyKind.Block;
        public static string TeaId(string room,int slot)=>room+"-tea-"+slot;
        public static string Support(string room,int slot)=>room+(slot<4?"/nest-"+slot:"/tea-"+(slot-4));
        public static int Slot(string room,string id)
        {if(!SecretRooms.Furnished(room))return -1;for(var i=0;i<9;i++)if(id==Support(room,i))return i;return -1;}
        public static float X(int slot)=>slot<4?700+slot*310:slot<8?820+(slot-4)*200:1620;
        public static float Y(int slot)=>slot<4?265:slot<8?30:95;
        public static string Stack(string parent)=>"stack/"+parent;
        public static string Parent(SoloToy toy)=>toy.container!=null && toy.container.StartsWith("stack/",StringComparison.Ordinal)?toy.container.Substring(6):"";
        public static int ExtraStock(SoloSnapshot s)=>s.schema<Schema?0:5*(4+(s.secrets??Array.Empty<SecretRoomState>()).Count(r=>r!=null && r.created));
        public static bool Decoration(string key)=>key=="bedding" || key=="rug" || key=="picture" || key=="lamp";
        public static int DecorValue(BedroomState r,string key)=>key=="theme"?r.theme:key=="layout"?r.layout:key=="bedding"?r.bedding:key=="rug"?r.rug:key=="lamp"?r.lamp:r.picture;
        public static void SetDecor(BedroomState r,string key,int value)
        {if(key=="theme")r.theme=value;else if(key=="bedding")r.bedding=value;else if(key=="rug")r.rug=value;else if(key=="lamp")r.lamp=value;else r.picture=value;}
        public static SoloToy[] Stock(string room)=>Enumerable.Range(0,5).Select(i=>new SoloToy{
            id=TeaId(room,i),kind=i==4?ToyKind.TeaPot:ToyKind.TeaCup,personalRoom=SecretRooms.Index(room)>=0?SecretRooms.Parent(room):room,
            zone=room,container=Support(room,i+4),x=X(i+4),y=Y(i+4),water=i==4?3:0}).ToArray();
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithRoomPlay(GameWorld world)
        {
            world=WithBooks(world);if(world.Schema>=RoomPlay.Schema)return world;
            var s=world.Snapshot();s.schema=RoomPlay.Schema;s.revision++;
            foreach(var r in SecretRooms.Furnishings(s)){r.bedding=r.rug=r.lamp=r.theme;}
            s.toys=s.toys.Concat(SecretRooms.Furnishings(s).SelectMany(r=>RoomPlay.Stock(r.id))).ToArray();
            Validate(s);return new GameWorld(s);
        }
        private static void ValidateRoomPlay(SoloSnapshot s)
        {
            var all=(s.bedrooms??Array.Empty<BedroomState>()).Concat((s.secrets??Array.Empty<SecretRoomState>()).Select(r=>r.furniture));
            foreach(var r in all)
                if(r.bedding<0 || r.bedding>3 || r.rug<0 || r.rug>3 || r.picture<0 || r.picture>3 || r.lamp<0 || r.lamp>3 ||
                   s.schema<RoomPlay.Schema && (r.bedding!=0 || r.rug!=0 || r.picture!=0 || r.lamp!=0))throw new InvalidOperationException("Invalid room decoration.");
            if(s.schema<RoomPlay.Schema)
            {if(s.toys.Any(t=>RoomPlay.Tea(t.kind) || RoomPlay.Parent(t)!="" || RoomPlay.Slot(t.zone,t.container)>=0))throw new InvalidOperationException("Room play requires schema 12.");return;}
            foreach(var room in SecretRooms.Furnishings(s))for(var i=0;i<5;i++)
            {
                var t=s.toys.SingleOrDefault(v=>v.id==RoomPlay.TeaId(room.id,i));
                if(t==null || t.kind!=(i==4?ToyKind.TeaPot:ToyKind.TeaCup) || t.personalRoom!=(SecretRooms.Index(room.id)>=0?SecretRooms.Parent(room.id):room.id) || t.wet || t.resetPending || i<4 && t.water>1)
                    throw new InvalidOperationException("Invalid pretend tea stock.");
            }
            foreach(var t in s.toys.Where(t=>RoomPlay.Parent(t)!=""))
            {
                var seen=new System.Collections.Generic.HashSet<string>{t.id};var child=t;
                while(RoomPlay.Parent(child)!="")
                {
                    var parent=s.toys.SingleOrDefault(v=>v.id==RoomPlay.Parent(child));
                    if(parent==null || !seen.Add(parent.id) || seen.Count>3 || !RoomPlay.Stackable(child.kind) || !RoomPlay.Stackable(parent.kind) ||
                       child.zone!=parent.zone || !SecretRooms.Furnished(child.zone) || child.holder!="" || parent.holder!="" || child.x!=parent.x || child.y!=parent.y ||
                       parent.container!="" && RoomPlay.Parent(parent)=="")throw new InvalidOperationException("Invalid toy stack.");
                    child=parent;
                }
            }
        }
        // Removing any lower toy settles its dependents before handing out the
        // lease. No hidden child follows a held parent or survives as a duplicate.
        private void SettleStack(SoloToy parent)
        {
            var chain=new System.Collections.Generic.List<SoloToy>();var current=parent;
            while(true){var child=state.toys.FirstOrDefault(t=>t.container==RoomPlay.Stack(current.id));if(child==null)break;chain.Add(child);current=child;}
            for(var i=0;i<chain.Count;i++){var child=chain[i];child.container="";child.x=Math.Max(650,Math.Min(1950,parent.x))+100*(i+1);child.y=Math.Max(35,Math.Min(240,parent.y-65));Touch(child);}
        }
        private void SettleCuddle(SoloPlayer player)
        {
            if(state.schema<RoomPlay.Schema || !BedroomFurniture.Seat(player.fixture))return;
            foreach(var t in state.toys.Where(t=>t.holder==player.id))
            {t.holder="";t.x=player.x;t.y=Math.Max(35,Math.Min(250,player.y-65));Touch(t);}
        }
        private string ApplyRoomObject(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            if(state.schema<RoomPlay.Schema || !SecretRooms.Furnished(p.zone) || item==null)return "wrong-area";
            if(item.holder!="" && item.holder!=p.id)return "already-held";
            if(!StorageOpen(item))return "storage-closed";
            if(RoomPlay.Tea(item.kind))
            {item.water=item.kind==ToyKind.TeaPot?3:0;Touch(item);return null;}
            if(item.kind!=ToyKind.Plush)return "not-cuddleable";
            if(state.toys.Any(t=>t.holder==p.id && t!=item))return "already-held";
            if(item.holder==p.id && BedroomFurniture.Seat(p.fixture)){ClearFixture(p);return null;}
            var room=SecretRooms.Furnishings(state).Single(r=>r.id==p.zone);
            var seat=BedroomFurniture.Seat(p.fixture)?p.fixture:Enumerable.Range(0,4).Select(BedroomFurniture.Cushion)
                .Where(id=>!state.players.Any(v=>v.id!=p.id && v.zone==p.zone && v.fixture==id))
                .OrderBy(id=>Math.Abs(p.x-BedroomFurniture.SeatX(id,room.layout))).FirstOrDefault();
            if(seat==null)return "fixture-busy";
            SettleStack(item);ClearFixture(p);item.container="";item.holder=p.id;p.fixture=seat;p.activity="";p.useSeconds=0;
            p.x=BedroomFurniture.SeatX(seat,room.layout);p.y=BedroomFurniture.SeatY(seat);
            item.x=p.x;item.y=Math.Max(35,Math.Min(250,p.y-65));Touch(item);return null;
        }
        private string StoreRoomPlay(SoloCommand c,SoloPlayer p,SoloToy item,int slot)
        {
            if(state.schema<RoomPlay.Schema || slot<4 && item.kind!=ToyKind.Plush || slot>=4 && item.kind!=(slot==8?ToyKind.TeaPot:ToyKind.TeaCup))return "wrong-support";
            if(state.toys.Any(t=>t.container==c.target))return "storage-full";
            if(Math.Abs(c.x-RoomPlay.X(slot))>80 || Math.Abs(c.y-RoomPlay.Y(slot))>80)return "target-too-far";
            item.container=c.target;item.holder="";item.x=RoomPlay.X(slot);item.y=RoomPlay.Y(slot);Touch(item);return null;
        }
        private string StackToy(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            if(state.schema<RoomPlay.Schema || !SecretRooms.Furnished(p.zone) || !RoomPlay.Stackable(item.kind))return "invalid-stack";
            var parent=state.toys.FirstOrDefault(t=>RoomPlay.Stack(t.id)==c.target);
            if(parent==null || parent==item || parent.zone!=p.zone || !RoomPlay.Stackable(parent.kind) || parent.holder!="" ||
               parent.container!="" && RoomPlay.Parent(parent)=="")return "invalid-stack";
            if(state.toys.Any(t=>t.container==c.target))return "storage-full";
            var root=parent;var depth=2;
            while(RoomPlay.Parent(root)!=""){root=state.toys.Single(t=>t.id==RoomPlay.Parent(root));if(root==item || ++depth>3)return "stack-too-high";}
            if(Math.Abs(c.x-parent.x)>90 || Math.Abs(c.y-parent.y)>90)return "target-too-far";
            item.container=c.target;item.holder="";item.x=parent.x;item.y=parent.y;Touch(item);return null;
        }
        private string PourTea(SoloToy pot,SoloToy cup,SoloPlayer p)
        {
            if(state.schema<RoomPlay.Schema || !SecretRooms.Furnished(p.zone) || cup.zone!=p.zone || cup.holder!="" || !StorageOpen(cup))return "cup-busy";
            if(pot.water==0)return "teapot-empty";if(cup.water>0)return "cup-full";
            pot.water--;cup.water=1;pot.holder="";pot.x=cup.x+85;pot.y=Math.Max(35,cup.y);Touch(pot);Touch(cup);return null;
        }
    }
}
