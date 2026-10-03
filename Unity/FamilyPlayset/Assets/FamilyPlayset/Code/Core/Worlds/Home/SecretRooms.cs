using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class SecretRoomState
    {
        public string id,parent,owner;
        public bool created,active;
        public int slot,style;
        public long entranceRevision=1;
        public BedroomState furniture;
        public SecretRoomState Copy(){var copy=(SecretRoomState)MemberwiseClone();copy.furniture=furniture.Copy();return copy;}
    }
    public static class SecretRooms
    {
        public const int Schema=9;
        public const float DoorY=220,RevealRadius=260,HideRadius=340;
        public static string Id(int index)=>index>=0 && index<4?"home-secret-"+(index+1):"";
        public static int Index(string area){for(var i=0;i<4;i++)if(area==Id(i))return i;return -1;}
        public static bool Furnished(string area)=>BedroomLayout.Index(area)>=0 || Index(area)>=0;
        public static string Parent(string secret)=>BedroomLayout.Id(Index(secret));
        public static float DoorX(int slot)=>slot==0?2310:2250;
        public static string Fort(int i)=>"secret-fort-"+i;
        public static int FortIndex(string id){for(var i=0;i<4;i++)if(id==Fort(i))return i;return -1;}
        public static bool Route(string source,string target)=>Index(target)>=0 && Parent(target)==source || Index(source)>=0 && Parent(source)==target;
        public static IEnumerable<BedroomState> Furnishings(SoloSnapshot s)=>(s.bedrooms??Array.Empty<BedroomState>()).Concat((s.secrets??Array.Empty<SecretRoomState>()).Where(r=>r!=null && r.created).Select(r=>r.furniture));
        public static string ToyId(int room,int kind)=>Id(room)+"-plush-"+kind;
        public static int PlushIndex(string id){for(var i=0;i<4;i++)for(var k=0;k<6;k++)if(id==ToyId(i,k))return k;return -1;}
        public static bool Reveal(bool wasVisible,float x,float y,int slot)
        {var dx=x-DoorX(slot);var dy=y-DoorY;var radius=wasVisible?HideRadius:RevealRadius;return dx*dx+dy*dy<=radius*radius;}
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithSecretRooms(GameWorld world)
        {
            world=WithFurnishedRooms(world);if(world.Schema>=SecretRooms.Schema)return world;
            var s=world.Snapshot();s.schema=SecretRooms.Schema;s.revision++;
            s.secrets=Enumerable.Range(0,4).Select(i=>new SecretRoomState{id=SecretRooms.Id(i),parent=BedroomLayout.Id(i),owner=s.bedrooms.Single(r=>r.id==BedroomLayout.Id(i)).owner,
                furniture=new BedroomState{id=SecretRooms.Id(i),owner=s.bedrooms.Single(r=>r.id==BedroomLayout.Id(i)).owner,theme=i,roomRevision=1}}).ToArray();
            Validate(s);return new GameWorld(s);
        }
        public SecretRoomState[] ReadSecrets()=>state.secrets.Select(r=>r.Copy()).ToArray();
        private static void ValidateSecrets(SoloSnapshot s)
        {
            if(s.schema<SecretRooms.Schema){if(s.secrets!=null && s.secrets.Length!=0)throw new InvalidOperationException("Secrets require schema 9.");return;}
            if(s.secrets==null || s.secrets.Length!=4 || s.secrets.Any(r=>r==null || SecretRooms.Index(r.id)<0) || s.secrets.Select(r=>r.id).Distinct().Count()!=4)throw new InvalidOperationException("Invalid secret registry.");
            foreach(var r in s.secrets)
            {
                var index=SecretRooms.Index(r.id);
                if(r.parent!=BedroomLayout.Id(index) || r.owner!=s.bedrooms.Single(b=>b.id==r.parent).owner || r.furniture==null || r.furniture.id!=r.id || r.furniture.owner!=r.owner ||
                    r.slot<0 || r.slot>1 || r.style<0 || r.style>1 || r.entranceRevision<1 || r.entranceRevision>=long.MaxValue || r.created && r.owner=="" ||
                    !r.created && (r.active || r.slot!=0 || r.style!=0 || r.entranceRevision!=1 || r.furniture.roomRevision!=1 || r.furniture.theme!=index || r.furniture.layout!=0 || r.furniture.chestOpen || r.furniture.lampOn || r.furniture.decorateTogether || !string.IsNullOrEmpty(r.furniture.undoKind) || !string.IsNullOrEmpty(r.furniture.undoActor) || r.furniture.undoBefore!=0 || r.furniture.undoAfter!=0))
                    throw new InvalidOperationException("Invalid secret room or entrance.");
                for(var k=0;k<6;k++)
                {
                    var t=s.toys.SingleOrDefault(v=>v.id==SecretRooms.ToyId(index,k));
                    if(!r.created?t!=null:t==null || t.kind!=ToyKind.Plush || t.personalRoom!=r.parent || t.water!=0 || t.wet || t.resetPending)throw new InvalidOperationException("Invalid secret plush stock.");
                }
                if(!r.created && (s.players.Any(p=>p.zone==r.id) || s.toys.Any(t=>t.zone==r.id)))throw new InvalidOperationException("Uncreated secret is occupied.");
            }
        }
        private SecretRoomState CurrentSecret(SoloPlayer p)=>state.secrets.FirstOrDefault(r=>r.id==p.zone || r.parent==p.zone);
        private string ChangeSecret(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<SecretRooms.Schema)return "wrong-area";var r=CurrentSecret(p);
            if(r==null || r.owner!=p.id)return "owner-only";
            if(c.target=="create" && r.created)return null;
            var parts=c.value.Split(':');
            if(parts.Length!=2 || !int.TryParse(parts[0],out var value) || !long.TryParse(parts[1],out var revision) || revision!=r.entranceRevision || r.entranceRevision>=long.MaxValue-1)return "entrance-changed";
            if(c.target=="create")
            {
                if(p.zone!=r.parent || !SecretRooms.Reveal(false,p.x,p.y,r.slot))return "door-too-far";
                r.created=r.active=true;r.furniture.chestOpen=true;var index=SecretRooms.Index(r.id);
                state.toys=state.toys.Concat(Enumerable.Range(0,6).Select(k=>{var slot=k<4?8+k:k-4;return new SoloToy{id=SecretRooms.ToyId(index,k),kind=ToyKind.Plush,personalRoom=r.parent,zone=r.id,
                    container=BedroomFurniture.Storage(r.id,slot),x=BedroomFurniture.StorageX(0,slot),y=BedroomFurniture.StorageY(slot)};})).ToArray();
                if(state.schema>=RoomPlay.Schema){r.furniture.bedding=r.furniture.rug=r.furniture.lamp=r.furniture.theme;state.toys=state.toys.Concat(RoomPlay.Stock(r.id)).ToArray();}
            }
            else
            {
                if(!r.created || value<0 || value>1)return "invalid-entrance";
                if(c.target=="active")r.active=value==1;else if(c.target=="slot")r.slot=value;else if(c.target=="style")r.style=value;else return "invalid-entrance";
            }
            r.entranceRevision++;return null;
        }
        private string SecretTravel(SoloPlayer p,string target,string revision,bool escape=false)
        {
            if(state.schema<SecretRooms.Schema || !SecretRooms.Route(p.zone,target))return "invalid-door";
            var r=CurrentSecret(p);if(r==null || !r.created)return "missing-room";
            var entering=p.zone==r.parent;
            if(entering && (!r.active || !long.TryParse(revision,out var expected) || expected!=r.entranceRevision))return "entrance-changed";
            var x=entering?SecretRooms.DoorX(r.slot):BedroomLayout.ExitX;var y=entering?SecretRooms.DoorY:BedroomLayout.DoorY;
            if((entering || !escape) && (Math.Abs(p.x-x)>65 || Math.Abs(p.y-y)>65))return "door-too-far";
            if(p.visit>=long.MaxValue-1)return "visit-limit";
            var slot=Array.FindIndex(state.players,v=>v.id==p.id);
            ClearFixture(p);p.activity="";p.zone=target;p.visit++;p.y=220;
            p.x=entering?480+slot*130:r.active?Math.Min(2280,SecretRooms.DoorX(r.slot)-100-slot*65):480+slot*130;
            foreach(var toy in state.toys.Where(t=>t.holder==p.id)){toy.zone=target;toy.x=p.x;toy.y=p.y;Touch(toy);}
            return null;
        }
    }
}
