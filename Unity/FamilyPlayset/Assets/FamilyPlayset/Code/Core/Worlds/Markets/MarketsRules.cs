using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class MarketsMember
    {
        public string actor, station="", pose="idle";
        public double at, until;
        public MarketsMember Copy()=>(MarketsMember)MemberwiseClone();
    }
    [Serializable] public sealed class MarketsItem
    {
        public string id,kind,choice,holder="",station;
        public int revision;
        public MarketsItem Copy()=>(MarketsItem)MemberwiseClone();
    }
    [Serializable] public sealed class MarketsStation
    {
        public string id;
        public string[] queue=Array.Empty<string>(),parts=Array.Empty<string>();
        public int completed,revision,choice;
        public double busyUntil;
        public string workingActor="";
        public string pendingKind="",pendingChoice="";
        public MarketsStation Copy()=>new MarketsStation{id=id,queue=queue.ToArray(),parts=parts.ToArray(),completed=completed,revision=revision,choice=choice,busyUntil=busyUntil,workingActor=workingActor,pendingKind=pendingKind,pendingChoice=pendingChoice};
    }
    [Serializable] public sealed class MarketsPony
    {
        public string id;
        public int needs,washed,body,mane,saddle,tail,rounds;
        public MarketsPony Copy()=>(MarketsPony)MemberwiseClone();
    }
    [Serializable] public sealed class MarketsBubble
    {
        public int id,shape;
        public double born;
        public MarketsBubble Copy()=>(MarketsBubble)MemberwiseClone();
    }
    [Serializable] public sealed class MarketsBrick
    {
        public string id,creator,variant;
        public int slot,turn,revision;
        public MarketsBrick Copy()=>(MarketsBrick)MemberwiseClone();
    }
    [Serializable] public sealed class MarketsState
    {
        public int version=1,serial,bubbleSerial,popped,song,layers,musicHits,guitar,sign,umbrella,longDog,photos;
        public double clock,musicAt;
        public bool playing;
        public MarketsMember[] members;
        public MarketsStation[] stations;
        public MarketsPony[] ponies;
        public MarketsItem[] items=Array.Empty<MarketsItem>();
        public MarketsBubble[] bubbles=Array.Empty<MarketsBubble>();
        public int buildSerial;
        public MarketsBrick[] bricks=Array.Empty<MarketsBrick>();
        public MarketsState Copy()=>new MarketsState{version=version,serial=serial,bubbleSerial=bubbleSerial,popped=popped,song=song,layers=layers,musicHits=musicHits,guitar=guitar,sign=sign,umbrella=umbrella,longDog=longDog,photos=photos,buildSerial=buildSerial,bricks=bricks.Select(x=>x.Copy()).ToArray(),clock=clock,musicAt=musicAt,playing=playing,members=members.Select(x=>x.Copy()).ToArray(),stations=stations.Select(x=>x.Copy()).ToArray(),ponies=ponies.Select(x=>x.Copy()).ToArray(),items=items.Select(x=>x.Copy()).ToArray(),bubbles=bubbles.Select(x=>x.Copy()).ToArray()};
    }

    // Scene-bound objects and blueprints inform the catalog. Queueing, shared
    // preparation, finite stock and cooperative rhythm are family adaptations;
    // empty recovered IL2CPP declarations do not establish original timing.
    public static class MarketsRules
    {
        public const string Zone="markets";
        public static readonly string[] StationIds={"smoothie","flowers","cakes","bubbles","music","buttermilk","biscuit","icecream"};
        public static readonly string[] Fruit={"apple","banana","carrot","grapes"};
        public static readonly string[] Flowers={"coral","green","lavender","pink","red","yellow"};
        public static readonly string[] Cakes={"pink","purple","yellow"};
        public static readonly string[] IceCreamNames={"chocolate","sundae","mango"};
        public static readonly string[][] Recipes={
            new[]{"Base_LightPurple","Scoop_ReddishBrown","Scoop_ReddishBrown","Scoop_ReddishBrown","Swirl_ReddishBrown"},
            new[]{"Base_MedAzure","pfb_mkd_byo_HalfInverseBow_Reverse","HalfInverseBow_White","coop_LightPurple","Scoop_FlameYellowishOrange","2x4_Plate_ReddishBrown","Scoop_ReddishBrown","Berries_BrightRed"},
            new[]{"IceCream_Cup","Swirl_FlameYellowishOrange","Scoop_FlameYellowishOrange","Swirl_FlameYellowishOrange","Scoop_FlameYellowishOrange","Swirl_FlameYellowishOrange"}
        };
        public static readonly string[] Bricks={"2x4_Plate_ReddishBrown","Base_LightPurple","Base_MedAzure","Berries_BrightRed","pfb_mkd_byo_HalfInverseBow_Reverse","HalfInverseBow_White","IceCream_Cup","Scoop_FlameYellowishOrange","coop_LightPurple","Scoop_ReddishBrown","Swirl_FlameYellowishOrange","Swirl_LightPurple","Swirl_ReddishBrown","Swirl_White"};
        public static readonly string[] Care={"food","water","brush","clean","wash"};
        public static readonly string[] Poses={"idle","reach","feed-apple","feed-carrot","water","mix","eat","drink","dance","guitar","brush","wash","clean","celebrate","wave"};
        public static float X(string station)=>station=="smoothie"?550:station=="flowers"?1100:station=="cakes"?1600:station=="bubbles"?2100:station=="music"?2700:station=="buttermilk"?3300:station=="biscuit"?3700:4200;
        private static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        private static bool Actor(MarketsState s,string a)=>a!=null&&s.members.Any(m=>m.actor==a);
        private static bool Present(SoloPlayer[] ps,string a)=>ps!=null&&ps.Any(p=>p!=null&&p.id==a&&p.zone==Zone);
        public static MarketsState Create(string[] actors)
        {
            if(actors==null||actors.Length<1||actors.Length>4||actors.Any(string.IsNullOrEmpty)||actors.Distinct().Count()!=actors.Length)throw new InvalidOperationException("Invalid Markets actors.");
            return new MarketsState{members=actors.Select(a=>new MarketsMember{actor=a}).ToArray(),stations=StationIds.Select(id=>new MarketsStation{id=id}).ToArray(),ponies=new[]{new MarketsPony{id="buttermilk"},new MarketsPony{id="biscuit",body=1}}};
        }
        public static MarketsState Normalize(MarketsState s,string[] actors)
        {
            // Only a wholly absent legacy inline record is defaultable. Never
            // repair partially populated or malformed durable state silently.
            if(s==null || s.version==0 && s.clock==0 && (s.members==null||s.members.Length==0))return Create(actors);
            return s;
        }
        public static void Validate(MarketsState s,SoloPlayer[] players)
        {
            void Need(bool b){if(!b)throw new InvalidOperationException("Invalid Markets world.");}
            Need(s!=null&&s.version==1&&Finite(s.clock)&&s.clock>=0&&Finite(s.musicAt)&&s.musicAt>=0&&s.musicAt<=s.clock);
            Need(players!=null&&players.All(p=>p!=null&&!string.IsNullOrEmpty(p.id))&&s.members!=null&&s.members.Length>=1&&s.members.Length<=4&&s.members.All(m=>m!=null&&m.actor!=null&&m.station!=null&&m.pose!=null)&&s.members.Select(m=>m.actor).OrderBy(x=>x).SequenceEqual(players.Select(p=>p.id).OrderBy(x=>x)));
            Need(s.members.Select(m=>m.actor).Distinct().Count()==s.members.Length&&s.members.All(m=>(m.station==""||StationIds.Contains(m.station))&&Poses.Contains(m.pose)&&Finite(m.at)&&m.at>=0&&m.at<=s.clock&&Finite(m.until)&&m.until>=0));
            Need(s.stations!=null&&s.stations.Length==StationIds.Length&&s.stations.Select(x=>x?.id).SequenceEqual(StationIds));
            foreach(var t in s.stations){Need(t.queue!=null&&t.queue.Length<=4&&t.queue.Distinct().Count()==t.queue.Length&&t.queue.All(a=>Actor(s,a)&&Present(players,a))&&t.parts!=null&&t.parts.Length<=8&&t.parts.All(x=>x!=null)&&t.completed>=0&&t.revision>=0&&t.choice>=0&&t.choice<=5&&Finite(t.busyUntil)&&t.busyUntil>=0&&t.workingActor!=null&&(t.workingActor==""||t.queue.Length>0&&t.queue[0]==t.workingActor));
                Need(t.queue.All(a=>s.members.Single(m=>m.actor==a).station==t.id));
                Need(t.workingActor==""?t.busyUntil==0:t.busyUntil>s.clock);
                Need(t.id=="smoothie"?t.parts.Length<=4&&t.parts.All(Fruit.Contains):t.id=="flowers"?t.parts.Length<=6&&t.parts.All(Flowers.Contains):t.id=="icecream"?t.choice<3&&t.parts.Length<=Recipes[t.choice].Length&&t.parts.Select((v,i)=>v==Recipes[t.choice][i]).All(x=>x):t.parts.Length==0);
                Need(t.pendingKind!=null&&t.pendingChoice!=null&&(t.workingActor!=""||t.pendingKind=="")&&(t.pendingKind!=""||t.pendingChoice==""));
                if(t.pendingKind!="")Need(t.workingActor!=""&&t.busyUntil>0&&(t.pendingKind=="smoothie"?t.id=="smoothie"&&t.parts.Length>0&&t.pendingChoice==string.Join("+",t.parts):t.pendingKind=="flowers"?t.id=="flowers"&&t.parts.Length>0&&t.pendingChoice==string.Join("+",t.parts):t.pendingKind=="cake"?t.id=="cakes"&&Cakes.Contains(t.pendingChoice):t.pendingKind=="care"?(t.id=="buttermilk"||t.id=="biscuit")&&Care.Contains(t.pendingChoice):t.pendingKind=="icecream"&&t.id=="icecream"&&t.parts.Length==Recipes[t.choice].Length&&t.pendingChoice==IceCreamNames[t.choice]));
            }
            Need(s.members.All(m=>m.station==""||s.stations.Single(t=>t.id==m.station).queue.Contains(m.actor)));
            Need(s.ponies!=null&&s.ponies.Length==2&&s.ponies[0]?.id=="buttermilk"&&s.ponies[1]?.id=="biscuit"&&s.ponies.All(p=>p.needs>=0&&p.needs<=15&&p.washed>=0&&p.body>=0&&p.body<5&&p.mane>=0&&p.mane<6&&p.saddle>=0&&p.saddle<6&&p.tail==p.mane&&p.rounds>=0));
            Need(s.serial>=0&&s.serial<int.MaxValue&&s.bubbleSerial>=0&&s.bubbleSerial<int.MaxValue&&new[]{s.popped,s.musicHits,s.guitar,s.longDog,s.photos}.All(x=>x>=0)&&s.song>=0&&s.song<3&&s.layers>=0&&s.layers<=5&&s.sign>=0&&s.sign<3&&s.umbrella>=0&&s.umbrella<3);
            Need(s.items!=null&&s.items.Length<=64&&s.items.All(i=>i!=null&&i.id!=null&&i.kind!=null&&i.choice!=null&&i.station!=null&&i.holder!=null)&&s.items.Select(i=>i.id).Distinct().Count()==s.items.Length);
            foreach(var i in s.items){Need(i.id.StartsWith("market-item-",StringComparison.Ordinal)&&int.TryParse(i.id.Substring(12),out var serial)&&serial>0&&serial<=s.serial&&i.id=="market-item-"+serial&&i.revision>=0&&StationIds.Contains(i.station)&&(i.holder==""||Actor(s,i.holder)&&Present(players,i.holder)));
                Need(i.kind=="smoothie"?i.choice.Split('+').Length<=4&&i.choice.Split('+').All(Fruit.Contains):i.kind=="flowers"?i.choice.Split('+').Length<=6&&i.choice.Split('+').All(Flowers.Contains):i.kind=="cake"?Cakes.Contains(i.choice):i.kind=="icecream"&&IceCreamNames.Contains(i.choice));
            }
            Need(s.items.Where(i=>i.holder!="").GroupBy(i=>i.holder).All(g=>g.Count()==1));
            Need(s.items.Length+s.stations.Count(t=>t.pendingKind!=""&&t.pendingKind!="care")<=64);
            Need(s.buildSerial>=0&&s.buildSerial<int.MaxValue&&s.bricks!=null&&s.bricks.Length<=16);
            Need(s.bricks.All(b=>b!=null&&b.id!=null&&Actor(s,b.creator)&&Bricks.Contains(b.variant)&&b.slot>=0&&b.slot<16&&b.turn>=0&&b.turn<4&&b.revision>=0&&b.revision<int.MaxValue));
            Need(s.bricks.Select(b=>b.id).Distinct().Count()==s.bricks.Length&&s.bricks.Select(b=>b.slot).Distinct().Count()==s.bricks.Length);
            foreach(var b in s.bricks)Need(b.id.StartsWith("market-brick-",StringComparison.Ordinal)&&int.TryParse(b.id.Substring(13),out var serial)&&serial>0&&serial<=s.buildSerial&&b.id=="market-brick-"+serial);
            Need(s.bubbles!=null&&s.bubbles.Length<=24&&s.bubbles.All(b=>b!=null&&b.id>0&&b.id<=s.bubbleSerial&&b.shape>=0&&b.shape<6&&Finite(b.born)&&b.born>=0&&b.born<=s.clock)&&s.bubbles.Select(b=>b.id).Distinct().Count()==s.bubbles.Length);
        }
        private static void Pose(MarketsState s,string a,string pose,double duration)
        {var m=s.members.Single(x=>x.actor==a);m.pose=pose;m.at=s.clock;m.until=s.clock+duration;}
        public static void Release(MarketsState s,string actor,SoloPlayer[] players=null,bool dropItems=true)
        {
            if(s==null||!Actor(s,actor))return;
            var m=s.members.Single(x=>x.actor==actor);m.station="";m.pose="idle";m.at=s.clock;m.until=s.clock;
            foreach(var t in s.stations){t.queue=t.queue.Where(a=>a!=actor).ToArray();if(t.workingActor==actor){
                // The final recipe brick has not contacted the creation yet.
                // Cancellation returns to its prior step, retaining siblings'
                // completed placements and the shared smoothie/flower inputs.
                if(t.pendingKind=="icecream")t.parts=t.parts.Take(t.parts.Length-1).ToArray();
                t.pendingKind=t.pendingChoice="";t.workingActor="";t.busyUntil=0;
            } }
            if(dropItems)foreach(var i in s.items.Where(i=>i.holder==actor)){i.holder="";i.revision++;}
            if(!s.stations.Single(t=>t.id=="music").queue.Any()){s.playing=false;s.layers=0;}
        }
        public static void Restore(MarketsState s)
        {
            if(s==null)return;
            foreach(var a in s.members.Select(m=>m.actor).ToArray())Release(s,a);
            s.bubbles=Array.Empty<MarketsBubble>();s.playing=false;s.layers=0;
        }
        private static bool Revision(string value,int revision)=>int.TryParse(value,out var r)&&r==revision;
        private static MarketsStation Station(MarketsState s,string id)=>s.stations.FirstOrDefault(t=>t.id==id);
        private static string Turn(MarketsState s,string actor,MarketsStation t)
        {
            if(t==null)return "unknown-station";
            var member=s.members.Single(m=>m.actor==actor);
            if(member.station!=t.id){Release(s,actor,dropItems:false);member.station=t.id;t.queue=t.queue.Concat(new[]{actor}).ToArray();}
            return t.busyUntil>s.clock||t.queue[0]!=actor?"wait-your-turn":null;
        }
        private static void CompleteTurn(MarketsState s,string actor,MarketsStation t,string pose,double duration)
        {t.workingActor=actor;t.busyUntil=s.clock+duration;Pose(s,actor,pose,duration);}
        private static bool Product(MarketsState s,string kind,string choice,string station)
        {
            if(s.items.Length>=64)return false;
            s.items=s.items.Concat(new[]{new MarketsItem{id="market-item-"+(++s.serial),kind=kind,choice=choice,station=station}}).ToArray();return true;
        }
        private static bool PrepareProduct(MarketsState s,MarketsStation t,string kind,string choice)
        {
            // Reserve stock space until the authoritative action finishes.
            int reserved=s.stations.Count(x=>x.pendingKind!=""&&x.pendingKind!="care");
            if(s.items.Length+reserved>=64||(long)s.serial+reserved>=int.MaxValue-1)return false;
            t.pendingKind=kind;t.pendingChoice=choice;return true;
        }
        // Rejected commands must not partially mutate authority state. Apply to
        // a copy, then commit the accepted record's owned fields atomically.
        public static string Apply(MarketsState s,string actor,string operation,string item,string target,string value,SoloPlayer[] players)
        {
            if(s==null||!Actor(s,actor)||!Present(players,actor))return "come-to-markets";
            var copy=s.Copy();string result=ApplyChecked(copy,actor,operation??"",item??"",target??"",value??"",players);
            if(result!=null)return result;
            s.buildSerial=copy.buildSerial;s.bricks=copy.bricks;s.members=copy.members;s.stations=copy.stations;s.ponies=copy.ponies;s.items=copy.items;s.bubbles=copy.bubbles;
            s.serial=copy.serial;s.bubbleSerial=copy.bubbleSerial;s.popped=copy.popped;s.song=copy.song;s.layers=copy.layers;s.musicHits=copy.musicHits;s.guitar=copy.guitar;s.playing=copy.playing;s.musicAt=copy.musicAt;s.sign=copy.sign;s.umbrella=copy.umbrella;s.longDog=copy.longDog;s.photos=copy.photos;return null;
        }
        private static string ApplyChecked(MarketsState s,string actor,string op,string item,string target,string value,SoloPlayer[] players)
        {
            var m=s.members.Single(x=>x.actor==actor);
            if(op=="leave"){Release(s,actor);return null;}
            if(op=="join"){
                var t=Station(s,target);if(t==null)return "unknown-station";if(m.station==target)return null;
                Release(s,actor,dropItems:false);m.station=target;t.queue=t.queue.Concat(new[]{actor}).ToArray();if(target=="music"&&s.playing)Pose(s,actor,"dance",99999);return null;
            }
            if(op=="claim"||op=="drop"||op=="give"||op=="consume"){
                if(s.stations.Any(t=>t.workingActor==actor&&t.busyUntil>s.clock))return "finish-your-action";
                var p=s.items.SingleOrDefault(i=>i.id==item);if(p==null||!Revision(value,p.revision))return "item-changed";
                if(op=="claim"){if(p.holder!=""||s.items.Any(i=>i.holder==actor))return "hands-busy";p.holder=actor;}
                else{if(p.holder!=actor)return "not-your-item";
                    if(op=="give"){if(target==actor||!Present(players,target)||!Actor(s,target)||s.items.Any(i=>i.holder==target))return "friends-hands-busy";p.holder=target;}
                    else if(op=="consume"){if(p.kind=="flowers")return "flowers-are-for-giving";s.items=s.items.Where(i=>i!=p).ToArray();Pose(s,actor,p.kind=="smoothie"?"drink":"eat",3);return null;}
                    else p.holder="";
                }p.revision++;Pose(s,actor,"reach",.6);return null;
            }
            if(op=="ingredient"||op=="mix"||op=="flower"||op=="bouquet"||op=="cake"){
                string station=op=="ingredient"||op=="mix"?"smoothie":op=="flower"||op=="bouquet"?"flowers":"cakes";
                var t=Station(s,station);var error=Turn(s,actor,t);if(error!=null)return error;
                if(op=="ingredient"){if(!Fruit.Contains(item)||t.parts.Length>=4)return "choose-fruit";t.parts=t.parts.Concat(new[]{item}).ToArray();CompleteTurn(s,actor,t,"reach",.6);}
                if(op=="mix"){if(t.parts.Length==0)return "add-fruit-first";if(!PrepareProduct(s,t,"smoothie",string.Join("+",t.parts)))return "table-full";CompleteTurn(s,actor,t,"mix",3);}
                if(op=="flower"){if(!Flowers.Contains(item)||t.parts.Length>=6)return "choose-flower";t.parts=t.parts.Concat(new[]{item}).ToArray();CompleteTurn(s,actor,t,"reach",.6);}
                if(op=="bouquet"){if(t.parts.Length==0)return "add-flowers-first";if(!PrepareProduct(s,t,"flowers",string.Join("+",t.parts)))return "table-full";CompleteTurn(s,actor,t,"celebrate",1);}
                if(op=="cake"){if(!Cakes.Contains(item))return "choose-cake";if(!PrepareProduct(s,t,"cake",item))return "table-full";CompleteTurn(s,actor,t,"reach",1);}
                t.revision++;return null;
            }
            if(op=="bubble"){
                var t=Station(s,"bubbles");var error=Turn(s,actor,t);if(error!=null)return error;
                if(s.bubbles.Length>=24)return "bubble-sky-full";
                int shape;if(!int.TryParse(item,out shape)||shape<0||shape>5)return "choose-bubble";
                s.bubbles=s.bubbles.Concat(new[]{new MarketsBubble{id=++s.bubbleSerial,shape=shape,born=s.clock}}).ToArray();t.completed++;CompleteTurn(s,actor,t,"reach",.5);return null;
            }
            if(op=="pop"){
                if(!int.TryParse(item,out var id)||!s.bubbles.Any(b=>b.id==id))return "bubble-already-popped";
                s.bubbles=s.bubbles.Where(b=>b.id!=id).ToArray();s.popped++;Pose(s,actor,s.popped%8==0?"celebrate":"reach",.6);return null;
            }
            if(op=="music-start"||op=="music-stop"||op=="beat"||op=="guitar"||op=="song"){
                var t=Station(s,"music");if(m.station!="music"){Release(s,actor,dropItems:false);m.station="music";t.queue=t.queue.Concat(new[]{actor}).ToArray();}
                if(op=="music-start"){if(!s.playing){s.playing=true;s.layers=1;s.musicAt=s.clock;}foreach(var a in t.queue)Pose(s,a,"dance",99999);return null;}
                if(op=="music-stop"){Release(s,actor);return null;}
                if(op=="song"){if(!int.TryParse(item,out var song)||song<0||song>2)return "choose-song";s.song=song;s.musicAt=s.clock;return null;}
                if(!s.playing)return "start-music-first";
                if(op=="guitar"){s.guitar++;Pose(s,actor,"guitar",.6);}else{if(!int.TryParse(item,out var button)||button<0||button>2)return "choose-beat";s.musicHits++;s.layers=Math.Min(5,1+s.musicHits/3);foreach(var a in t.queue)Pose(s,a,"dance",99999);}return null;
            }
            if(op=="care"||op=="makeover"||op=="pony-reset"){
                var pony=s.ponies.SingleOrDefault(p=>p.id==target);if(pony==null)return "choose-pony";
                var t=Station(s,target);var error=Turn(s,actor,t);if(error!=null)return error;
                if(op=="pony-reset"){
                    // Deliberate shared replay replaces unknown source need
                    // recurrence; finished rounds and appearance stay durable.
                    if(pony.needs!=15)return "finish-care-first";
                    pony.needs=0;CompleteTurn(s,actor,t,"wave",1);
                }else if(op=="care"){
                    if(!Care.Contains(item))return "choose-care";
                    if(item=="food"&&!new[]{"apple","carrot"}.Contains(value))return "choose-pony-food";
                    // Care is credited only after its contact action finishes.
                    // Departure/load cancels this existing station reservation.
                    t.pendingKind="care";t.pendingChoice=item;
                    CompleteTurn(s,actor,t,item=="brush"?"brush":item=="wash"?"wash":item=="clean"?"clean":item=="water"?"water":"feed-"+value,item=="food"||item=="water"?4.03:3);
                }else{
                    if(!int.TryParse(value,out var choice)||choice<0||choice>=(item=="body"?5:6)||!new[]{"body","mane","saddle","tail"}.Contains(item))return "choose-appearance";
                    if(item=="body")pony.body=choice;else if(item=="saddle")pony.saddle=choice;else pony.mane=pony.tail=choice;
                    CompleteTurn(s,actor,t,"celebrate",1);
                }if(op!="care")t.completed++;t.revision++;return null;
            }
            if(op=="build-add"||op=="build-move"||op=="build-turn"||op=="build-remove"||op=="build-clear"){
                if(s.stations.Any(t=>t.workingActor==actor&&t.busyUntil>s.clock))return "finish-your-action";
                var t=Station(s,"icecream");
                if(t.revision>=int.MaxValue-1)return "build-full";
                // Free building has one shared board rather than personal rounds
                // or the recipe queue. Only a brick's creator can revise it.
                if(op=="build-add"){
                    if(!Revision(value,t.revision))return "build-changed";
                    if(!Bricks.Contains(item))return "choose-brick";
                    if(!int.TryParse(target,out var slot)||slot<0||slot>=16)return "choose-build-slot";
                    if(s.bricks.Any(b=>b.slot==slot))return "build-slot-busy";
                    if(s.bricks.Length>=16||s.buildSerial>=int.MaxValue-1)return "build-full";
                    s.bricks=s.bricks.Concat(new[]{new MarketsBrick{id="market-brick-"+(++s.buildSerial),creator=actor,variant=item,slot=slot}}).ToArray();
                }else if(op=="build-clear"){
                    if(!Revision(value,t.revision))return "build-changed";
                    // Clearing is deliberately ownership-scoped: a stale or
                    // impatient sibling cannot erase another player's work.
                    if(!s.bricks.Any(b=>b.creator==actor))return "no-your-bricks";
                    s.bricks=s.bricks.Where(b=>b.creator!=actor).ToArray();
                }else{
                    var brick=s.bricks.SingleOrDefault(b=>b.id==item);
                    if(brick==null||!Revision(value,brick.revision))return "build-changed";
                    if(brick.creator!=actor)return "not-your-brick";
                    if(brick.revision>=int.MaxValue-1)return "build-full";
                    if(op=="build-move"){
                        if(!int.TryParse(target,out var slot)||slot<0||slot>=16)return "choose-build-slot";
                        if(s.bricks.Any(b=>b!=brick&&b.slot==slot))return "build-slot-busy";
                        brick.slot=slot;
                    }else if(op=="build-turn"){
                        if(!int.TryParse(target,out var turn)||turn<0||turn>=4)return "choose-build-turn";
                        brick.turn=turn;
                    }else s.bricks=s.bricks.Where(b=>b!=brick).ToArray();
                    brick.revision++;
                }
                t.revision++;Pose(s,actor,"reach",.7);return null;
            }
            if(op=="ice-select"||op=="ice-add"||op=="ice-reset"){
                var t=Station(s,"icecream");var error=Turn(s,actor,t);if(error!=null)return error;
                if(!Revision(value,t.revision))return "build-changed";
                if(op=="ice-select"){int choice=Array.IndexOf(IceCreamNames,item);if(choice<0)return "choose-order";if(t.parts.Length>0)return "finish-or-clear-build";t.choice=choice;}
                else if(op=="ice-reset")t.parts=Array.Empty<string>();
                else{var recipe=Recipes[t.choice];if(t.parts.Length>=recipe.Length||item!=recipe[t.parts.Length])return "follow-order-picture";if(t.parts.Length+1==recipe.Length&&!PrepareProduct(s,t,"icecream",IceCreamNames[t.choice]))return "table-full";t.parts=t.parts.Concat(new[]{item}).ToArray();CompleteTurn(s,actor,t,"reach",.7);}
                t.revision++;return null;
            }
            if(op=="sign"){s.sign=(s.sign+1)%3;Pose(s,actor,"wave",1);return null;}
            if(op=="umbrella"){s.umbrella=(s.umbrella+1)%3;Pose(s,actor,"reach",1);return null;}
            if(op=="longdog"){s.longDog++;Pose(s,actor,"celebrate",1);return null;}
            if(op=="photo"){s.photos++;Pose(s,actor,"celebrate",1);return null;}
            return "unknown-markets-action";
        }
        public static bool Advance(MarketsState s,double dt,SoloPlayer[] players)
        {
            if(s==null||!Finite(dt)||dt<=0||dt>5)return false;
            bool occupied=s.members.Any(m=>Present(players,m.actor));if(!occupied)return false;
            s.clock+=dt;
            foreach(var m in s.members){if(!Present(players,m.actor))Release(s,m.actor);else if(m.pose!="idle"&&m.pose!="dance"&&s.clock>=m.until)m.pose=m.station=="music"&&s.playing?"dance":"idle";}
            foreach(var t in s.stations){if(t.workingActor!=""&&s.clock>=t.busyUntil){if(t.pendingKind!=""){if(t.pendingKind=="care"){
                    var pony=s.ponies.Single(p=>p.id==t.id);int previous=pony.needs;
                    if(t.pendingChoice=="food")pony.needs|=1;if(t.pendingChoice=="water")pony.needs|=2;if(t.pendingChoice=="brush")pony.needs|=4;if(t.pendingChoice=="clean")pony.needs|=8;if(t.pendingChoice=="wash")pony.washed++;
                    if(previous!=15&&pony.needs==15)pony.rounds++;
                }else Product(s,t.pendingKind,t.pendingChoice,t.id);t.completed++;if(t.id!="cakes")t.parts=Array.Empty<string>();t.pendingKind=t.pendingChoice="";}var done=t.workingActor;t.workingActor="";t.busyUntil=0;t.queue=t.queue.Where(a=>a!=done).Concat(t.queue.Contains(done)?new[]{done}:Array.Empty<string>()).ToArray();}}
            s.bubbles=s.bubbles.Where(b=>s.clock-b.born<12).ToArray();return true;
        }
    }
}
