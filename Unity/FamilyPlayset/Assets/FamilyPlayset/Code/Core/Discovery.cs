using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class Discovery
    {
        public const int Schema=16, ColoringSchema=18, LegacyPages=6, HistoryLimit=20;
        public const float MinX=-7200, ScienceX=-6590, ArtX=-5460;
        public static readonly string[] Pages={"Dinosaur","Truck","Unicorn","Snake","Garden","House","Bluey","Bingo's glasses","Jump, Bingo!","Bingo's scooter","Hooray, Bingo!","Rocket Bingo","Bluey's bike","Chilli plays hockey","The Heeler family","Bluey's surprise","Bluey's potion","Football friends"};
        public static readonly int[] Regions={4,9,6,6,12,7,33,34,30,61,28,58,69,45,83,75,6,114};
        public static readonly string[] LightNames={"Dark","Red","Green","Yellow","Blue","Magenta","Cyan","White"};
        public static bool InBay(SoloPlayer p)=>p.zone=="garden" && p.x>=MinX && p.x<=-4800;
        public static bool Sinks(DiscoveryWorkspace w)=>2+2*w.cargo>(w.wide?12:6);
        public static float Waterline(DiscoveryWorkspace w)=>Math.Min(1,(2+2*w.cargo)/(w.wide?12f:6f));
        public static string PageToken(int page,ColoringPage state)=>page+"@"+state.revision;
        public static DiscoveryWorkspace Create(string owner)=>new DiscoveryWorkspace{owner=owner,pages=Regions.Take(LegacyPages).Select(n=>new ColoringPage{colors=new int[n]}).ToArray()};
        public static void Magnet(DiscoveryWorkspace w,float x,float y)
        {
            w.magnetX=Math.Max(55,Math.Min(745,x));w.magnetY=Math.Max(70,Math.Min(340,y));
            var dx=w.ironX-w.magnetX;var dy=w.ironY-w.magnetY;
            if(dx*dx+dy*dy<155*155){w.ironX=w.magnetX;w.ironY=w.magnetY+40;}
        }
    }
    [Serializable] public sealed class ColoringPage
    {
        public long revision;
        public int[] colors,undo=Array.Empty<int>(),redo=Array.Empty<int>();
        public ColoringPage Copy()=>new ColoringPage{revision=revision,colors=(int[])colors.Clone(),undo=(int[])undo.Clone(),redo=(int[])redo.Clone()};
    }
    [Serializable] public sealed class DiscoveryWorkspace
    {
        public string owner;
        public int cargo,lights;
        public bool wide,outOfWater;
        public float magnetX=400,magnetY=110,ironX=140,ironY=315;
        public ColoringPage[] pages;
        public BubbleTray[] bubbles=Array.Empty<BubbleTray>();
        public IceRescueTray[] ice=Array.Empty<IceRescueTray>();
        public MixingTray[] mixtures=Array.Empty<MixingTray>();
        public DiscoveryWorkspace Copy(){var c=(DiscoveryWorkspace)MemberwiseClone();c.bubbles=(bubbles??Array.Empty<BubbleTray>()).Select(t=>t.Copy()).ToArray();c.ice=(ice??Array.Empty<IceRescueTray>()).Select(t=>t.Copy()).ToArray();c.pages=pages.Select(p=>p.Copy()).ToArray();c.mixtures=(mixtures??Array.Empty<MixingTray>()).Select(t=>t.Copy()).ToArray();return c;}
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithColoringCollection(SoloWorld world)
        {
            world=WithMixing(world);if(world.Schema>=Discovery.ColoringSchema)return world;
            var s=world.Snapshot();s.schema=Discovery.ColoringSchema;s.revision++;
            // Append only: old page IDs, colors, histories and every mixture remain exact.
            foreach(var w in s.discovery)w.pages=w.pages.Concat(Discovery.Regions.Skip(Discovery.LegacyPages).Select(n=>new ColoringPage{colors=new int[n]})).ToArray();
            Validate(s);return new SoloWorld(s);
        }
        public static SoloWorld WithDiscovery(SoloWorld world)
        {
            world=WithCakeFlow(world);if(world.Schema>=Discovery.Schema)return world;
            var s=world.Snapshot();s.schema=Discovery.Schema;
            s.discovery=s.players.Select(p=>Discovery.Create(p.id)).ToArray();s.revision++;
            Validate(s);return new SoloWorld(s);
        }
        public DiscoveryWorkspace[] ReadDiscovery()=>(state.discovery??Array.Empty<DiscoveryWorkspace>()).Select(w=>w.Copy()).ToArray();
        private static void ValidateDiscovery(SoloSnapshot s)
        {
            var work=s.discovery??Array.Empty<DiscoveryWorkspace>();
            if(s.schema<Discovery.Schema){if(work.Length>0)throw new InvalidOperationException("Discovery requires schema 16.");return;}
            if(work.Length!=s.players.Length || work.Any(w=>w==null) || !work.Select(w=>w.owner).OrderBy(x=>x,StringComparer.Ordinal).SequenceEqual(s.players.Select(p=>p.id).OrderBy(x=>x,StringComparer.Ordinal)))
                throw new InvalidOperationException("Discovery workspace ownership mismatch.");
            bool Finite(float v,float min,float max)=>!float.IsNaN(v) && !float.IsInfinity(v) && v>=min && v<=max;
            foreach(var w in work)
            {
                if(s.schema>=BubbleLab.Schema){if(w.bubbles==null || w.bubbles.Length!=1)throw new InvalidOperationException("Missing bubble tray.");BubbleLab.Validate(w.bubbles[0]);}else if(w.bubbles!=null && w.bubbles.Length>0)throw new InvalidOperationException("Bubble lab requires schema 20.");
                if(s.schema>=IceRescue.Schema){if(w.ice==null || w.ice.Length!=1)throw new InvalidOperationException("Missing rescue tray.");IceRescue.Validate(w.ice[0]);}else if(w.ice!=null && w.ice.Length>0)throw new InvalidOperationException("Ice rescue requires schema 19.");
                if(s.schema>=Mixing.Schema){if(w.mixtures==null || w.mixtures.Length!=Mixing.Modes)throw new InvalidOperationException("Missing mixing trays.");for(var mode=0;mode<Mixing.Modes;mode++)Mixing.Validate(w.mixtures[mode],mode);}
                else if(w.mixtures!=null && w.mixtures.Length>0)throw new InvalidOperationException("Mixing requires schema 17.");
                if(w.cargo<0 || w.cargo>7 || w.lights<0 || w.lights>7 || !Finite(w.magnetX,55,745) || !Finite(w.magnetY,70,340) || !Finite(w.ironX,55,745) || !Finite(w.ironY,70,380) || w.pages==null || w.pages.Length!=(s.schema>=Discovery.ColoringSchema?Discovery.Pages.Length:Discovery.LegacyPages))
                    throw new InvalidOperationException("Invalid science workspace.");
                for(var i=0;i<w.pages.Length;i++)
                {
                    var page=w.pages[i];var count=Discovery.Regions[i];
                    bool History(int[] h)=>h!=null && h.Length<=Discovery.HistoryLimit && h.All(v=>v>=0 && v/16<count && v%16<=8);
                    if(page==null || page.revision<0 || page.revision==long.MaxValue || page.colors==null || page.colors.Length!=count || page.colors.Any(v=>v<0 || v>8) || !History(page.undo) || !History(page.redo))
                        throw new InvalidOperationException("Invalid coloring page.");
                }
            }
        }
        private string DiscoveryOperation(SoloCommand c,SoloPlayer p)
        {
            if(state.schema<Discovery.Schema || !Discovery.InBay(p))return "use-discovery-area";
            if(c.item!=p.id)return "owner-only";
            var w=state.discovery.Single(v=>v.owner==p.id);
            var op=c.value;
            if(op.StartsWith("bubble:"))return BubbleOperation(c,w);
            if(op.StartsWith("ice:"))return IceRescueOperation(c,w);
            if(op.StartsWith("mix:"))return MixingOperation(c,w);
            if(op.StartsWith("fill:") || op=="undo" || op=="redo")
            {
                var token=c.target.Split('@');
                if(token.Length!=2 || !int.TryParse(token[0],out var pageIndex) || pageIndex<0 || pageIndex>=w.pages.Length || !long.TryParse(token[1],out var revision))return "invalid-page";
                var page=w.pages[pageIndex];
                // The generic command queue may rebase the world revision after a
                // sibling's action. This page token still rejects stale own edits.
                if(page.revision!=revision || revision>=long.MaxValue-1)return "page-changed";
                int[] Push(int[] h,int value)=>h.Concat(new[]{value}).Skip(Math.Max(0,h.Length+1-Discovery.HistoryLimit)).ToArray();
                if(op.StartsWith("fill:"))
                {
                    var args=op.Split(':');
                    if(args.Length!=3 || !int.TryParse(args[1],out var region) || region<0 || region>=page.colors.Length || !int.TryParse(args[2],out var color) || color<0 || color>8)return "invalid-color";
                    if(page.colors[region]==color)return "already-colored";
                    page.undo=Push(page.undo,region*16+page.colors[region]);page.redo=Array.Empty<int>();page.colors[region]=color;
                }
                else
                {
                    var history=op=="undo"?page.undo:page.redo;if(history.Length==0)return "nothing-to-"+op;
                    var entry=history[history.Length-1];var region=entry/16;var opposite=region*16+page.colors[region];
                    if(op=="undo"){page.undo=history.Take(history.Length-1).ToArray();page.redo=Push(page.redo,opposite);}
                    else {page.redo=history.Take(history.Length-1).ToArray();page.undo=Push(page.undo,opposite);}
                    page.colors[region]=entry%16;
                }
                page.revision++;return null;
            }
            switch(op)
            {
                case "cargo-add":w.cargo=Math.Min(7,w.cargo+1);break;
                case "cargo-remove":w.cargo=Math.Max(0,w.cargo-1);break;
                case "narrow":w.wide=false;break;
                case "wide":w.wide=true;break;
                case "lift":w.outOfWater=!w.outOfWater;break;
                case "reset-float":w.cargo=0;w.wide=false;w.outOfWater=false;break;
                case "red":w.lights^=1;break;
                case "green":w.lights^=2;break;
                case "blue":w.lights^=4;break;
                case "reset-lights":w.lights=0;break;
                case "magnet":
                    if(c.x<55 || c.x>745 || c.y<70 || c.y>340)return "invalid-magnet-position";
                    Discovery.Magnet(w,c.x,c.y);break;
                case "iron":Discovery.Magnet(w,w.ironX,w.ironY-65);break;
                case "wood":Discovery.Magnet(w,310,250);break;
                case "plastic":Discovery.Magnet(w,480,250);break;
                case "aluminum":Discovery.Magnet(w,650,250);break;
                case "reset-magnets":w.magnetX=400;w.magnetY=110;w.ironX=140;w.ironY=315;break;
                default:return "unknown-discovery-action";
            }
            return null;
        }
    }
}
