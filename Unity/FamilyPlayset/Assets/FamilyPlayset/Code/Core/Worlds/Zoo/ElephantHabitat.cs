using System;
using System.Linq;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    public enum ElephantPropKind { Canopy, Scratch, Leaves }
    [Serializable] public sealed class ElephantProp
    {
        public string id="",creator="";
        public ElephantPropKind kind;
        public int slot;
        public long revision;
        public ElephantProp Copy()=>(ElephantProp)MemberwiseClone();
    }
    [Serializable] public sealed class ElephantHabitatState
    {
        public int format=1, recoveredEntries;
        public long nextId;
        public ElephantProp[] props=Array.Empty<ElephantProp>();
        public string usingId="",pendingId="";
        public double cooldown=25;
        public ElephantHabitatState Copy(){var h=(ElephantHabitatState)MemberwiseClone();h.props=props.Select(p=>p.Copy()).ToArray();return h;}
    }
    public static class ElephantHabitat
    {
        public const float Y=420, BasketX=330, BasketY=300;
        public static readonly float[] Slots={1020,1260,1650};
        public static bool AnimalPhase(ZooPhase p)=>p==ZooPhase.PropWalk || p==ZooPhase.PropUse || p==ZooPhase.PropExit;
        public static bool Legal(ElephantPropKind kind,int slot)=>Enum.IsDefined(typeof(ElephantPropKind),kind) && slot>=0 && slot<3 && (kind==ElephantPropKind.Canopy?slot==1:kind==ElephantPropKind.Scratch?slot<=1:slot>=1);
        // Canopy's entire roof/posts span the three-slot patch. Small objects
        // need the two outer slots to coexist; neighboring body/use envelopes
        // overlap. Visitor paths and food/care/water sockets are never moved.
        public static bool Fits(ElephantHabitatState h,ElephantPropKind kind,int slot,string except="")=>Legal(kind,slot) &&
            !h.props.Any(p=>p.id!=except && (p.kind==ElephantPropKind.Canopy || kind==ElephantPropKind.Canopy || Math.Abs(p.slot-slot)<2));
        public static float GroundY(ElephantPropKind kind)=>kind==ElephantPropKind.Canopy?300:Y;
        public static WalkPoint Anchor(ElephantProp p)=>new WalkPoint(Slots[p.slot]+(p.kind==ElephantPropKind.Scratch?120:p.kind==ElephantPropKind.Leaves?-190:0),GroundY(p.kind));
        public static double Duration(ElephantPropKind k)=>k==ElephantPropKind.Canopy?2.8:2;
    }
    public sealed partial class GameWorld
    {
        private string HabitatOperation(SoloCommand c,SoloPlayer actor)
        {
            if(!AtElephant(actor))return "come-to-exhibit";
            var h=state.zoo.habitat;
            var bits=(c.item??"").Split('/');
            if(bits.Length!=2 || !long.TryParse(bits[1],out var rev))return "habitat-changed";
            if(!int.TryParse(c.target,out var slot))return "habitat-slot";
            var prop=h.props.FirstOrDefault(p=>p.id==bits[0]);
            if(c.value=="habitat-place"){
                if(!int.TryParse(bits[0],out var type) || type<0 || type>2 || rev!=h.nextId)return "habitat-changed";
                var kind=(ElephantPropKind)type;
                if(!ElephantHabitat.Fits(h,kind,slot))return "habitat-occupied";
                if(h.nextId>=long.MaxValue-1)return "habitat-limit";
                prop=new ElephantProp{id="habitat-"+(++h.nextId),creator=actor.id,kind=kind,slot=slot};
                h.props=h.props.Concat(new[]{prop}).ToArray();
            }else{
                if(prop==null || prop.revision!=rev)return "habitat-changed";
                if(prop.creator!=actor.id)return "habitat-friend";
                if(prop.id==h.usingId && ElephantHabitat.AnimalPhase(state.zoo.animals[0].phase))return "habitat-using";
                if(c.value=="habitat-remove"){
                    h.props=h.props.Where(p=>p.id!=prop.id).ToArray();if(h.pendingId==prop.id)h.pendingId="";return null;
                }
                if(c.value!="habitat-move")return "unknown-habitat-action";
                if(!ElephantHabitat.Fits(h,prop.kind,slot,prop.id))return "habitat-occupied";
                if(prop.revision>=long.MaxValue-1)return "habitat-limit";
                prop.slot=slot;prop.revision++;
            }
            // One coalesced notice; rapid rearrangement never accumulates work.
            h.pendingId=prop.id;return null;
        }
        private static void NormalizeHabitat(ZooState z)
        {
            if(z.habitat==null){z.habitat=new ElephantHabitatState();return;}
            var h=z.habitat;var valid=new List<ElephantProp>();var rejected=0;
            if(h.format!=1){rejected=h.props?.Length??1;h.props=Array.Empty<ElephantProp>();h.format=1;}
            foreach(var p in h.props??Array.Empty<ElephantProp>()){
                long id=0;
                if(p==null || p.id==null || !p.id.StartsWith("habitat-",StringComparison.Ordinal) || !long.TryParse(p.id.Substring(8),out id) || id<=0 || id>=long.MaxValue-1 ||
                    string.IsNullOrEmpty(p.creator) || p.creator.Length>128 || p.revision<0 || p.revision>=long.MaxValue-1 ||
                    valid.Any(v=>v.id==p.id) || !ElephantHabitat.Fits(new ElephantHabitatState{props=valid.ToArray()},p.kind,p.slot)){rejected++;continue;}
                valid.Add(p);h.nextId=Math.Max(h.nextId,id);
            }
            h.props=valid.ToArray();if(h.nextId<0 || h.nextId>=long.MaxValue-1)h.nextId=valid.Select(p=>long.Parse(p.id.Substring(8))).DefaultIfEmpty(0).Max();
            if(rejected>0){h.recoveredEntries=(int)Math.Min(int.MaxValue,(long)Math.Max(0,h.recoveredEntries)+rejected);System.Diagnostics.Trace.TraceWarning("Elephant habitat: ignored "+rejected+" invalid placement entries; healthy Zoo state retained.");}
            if(h.usingId==null || !valid.Any(p=>p.id==h.usingId))h.usingId="";
            if(h.pendingId==null || !valid.Any(p=>p.id==h.pendingId))h.pendingId="";
            if(!ZooClock(h.cooldown,60))h.cooldown=25;
        }
        private bool TryHabitatRoutine(ZooAnimal a)
        {
            var h=state.zoo.habitat;if(h.props.Length==0 || h.pendingId=="" && h.cooldown>0)return false;
            var p=h.props.FirstOrDefault(v=>v.id==h.pendingId)??h.props[(int)(ZooLayout.Roll(a)*h.props.Length)%h.props.Length];
            h.pendingId="";h.usingId=p.id;h.cooldown=35+ZooLayout.Roll(a)*20;
            var anchor=ElephantHabitat.Anchor(p);ZooLayout.Segment(a,ZooPhase.PropWalk,anchor.X,anchor.Y,WaterWalkTime(ZooLayout.Point(a),anchor.X,anchor.Y));AmbientChanged=true;return true;
        }
    }
}
