using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum WalkMode { Stop, Direction, Destination }
    [Serializable] public sealed class WalkInput
    {
        public string actor,zone;
        public long visit,sequence;
        public WalkMode mode;
        public float x,y;
        public WalkInput Copy()=>(WalkInput)MemberwiseClone();
    }
    public readonly struct WalkPoint
    {
        public readonly float X,Y;
        public WalkPoint(float x,float y){X=x;Y=y;}
    }
    public static class Walking
    {
        // Accepted family default for every current and future character.
        // Avatar art/scale must not introduce separate gameplay speed values.
        public const float Speed=420;
        // Local play owns its world, so it can apply continuous motion each
        // displayed frame without manufacturing inventory receipts. Shared
        // clients must continue to submit input to MovementAuthority instead.
        public static bool AdvanceLocal(SoloWorld world,string actor,WalkMode mode,float x,float y,float dt)
        {
            if(world==null || !Enum.IsDefined(typeof(WalkMode),mode) || float.IsNaN(dt) || float.IsInfinity(dt) || dt<0 || dt>.1f ||
                float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y))
                throw new ArgumentException("Invalid local walking step.");
            var p=world.ReadPlayer(actor);
            if(!ValidInput(p,world.Schema,mode,x,y))throw new ArgumentException("Invalid local walking destination.");
            var next=ParkWheels.Step(p,p.x,p.y,BedroomFurniture.Route(p,new WalkInput{mode=mode,x=x,y=y},world.Schema),dt,world.Schema);
            return world.SetWalkingPosition(p.id,p.zone,p.visit,next.X,next.Y);
        }
        // Destination bounds follow the current saved layout, including the
        // connected downstairs extension. Direction inputs retain their old limit.
        internal static bool ValidInput(SoloPlayer p,int schema,WalkMode mode,float x,float y)=>mode==WalkMode.Destination?
            WorldLayout.Position(p.zone,schema,x,y):Math.Abs(x)<=4800 && Math.Abs(y)<=1000;
        public static WalkPoint Step(float x,float y,WalkInput input,float dt,float minX=40,float maxX=960)
        {
            if(input==null || input.mode==WalkMode.Stop || dt<=0)return new WalkPoint(x,y);
            var dx=input.mode==WalkMode.Direction?input.x:input.x-x;
            var dy=input.mode==WalkMode.Direction?input.y:input.y-y;
            var length=(float)Math.Sqrt(dx*dx+dy*dy);
            var distance=input.mode==WalkMode.Direction?Math.Min(1,length)*Speed*dt:Math.Min(length,Speed*dt);
            if(length>0){x+=dx/length*distance;y+=dy/length*distance;}
            return new WalkPoint(Math.Max(minX,Math.Min(maxX,x)),Math.Max(35,Math.Min(455,y)));
        }
    }
    // A separate continuous-motion lane. Transactions still use world revisions
    // and durable receipts; walking cannot starve them with global revision churn.
    public sealed class MovementAuthority
    {
        private sealed class Control {public ulong connection;public WalkInput input;public double received;}
        private readonly Dictionary<string,Control> controls=new Dictionary<string,Control>();
        private readonly SoloWorld world;
        private readonly FamilySession session;
        public const double InputTimeout=.4;
        public MovementAuthority(SoloWorld world,FamilySession session){this.world=world;this.session=session;}
        public bool Accept(ulong connection,WalkInput input,double now)
        {
            if(input==null || !session.TryPlayer(connection,out var actor) || input.actor!=actor || input.sequence<=0 ||
                !Enum.IsDefined(typeof(WalkMode),input.mode) || float.IsNaN(input.x) || float.IsNaN(input.y) ||
                float.IsInfinity(input.x) || float.IsInfinity(input.y))return false;
            var p=world.ReadPlayer(actor);
            if(input.zone!=p.zone || input.visit!=p.visit || !Walking.ValidInput(p,world.Schema,input.mode,input.x,input.y))return false;
            if(controls.TryGetValue(actor,out var prior) && prior.connection==connection && input.sequence<=prior.input.sequence)return false;
            controls[actor]=new Control{connection=connection,input=input.Copy(),received=now};return true;
        }
        public bool Tick(double now,float dt)
        {
            if(dt<=0 || dt>.1f)throw new ArgumentOutOfRangeException(nameof(dt));
            var moved=false;
            foreach(var pair in controls.ToArray())
            {
                var c=pair.Value;var p=world.ReadPlayer(pair.Key);
                if(!session.TryPlayer(c.connection,out var actor) || actor!=pair.Key || p.zone!=c.input.zone || p.visit!=c.input.visit)
                {controls.Remove(pair.Key);continue;}
                if(now-c.received>InputTimeout)continue;
                var next=ParkWheels.Step(p,p.x,p.y,BedroomFurniture.Route(p,c.input,world.Schema),dt,world.Schema);
                moved|=world.SetWalkingPosition(p.id,p.zone,p.visit,next.X,next.Y);
            }
            return moved;
        }
        public long Acknowledged(string actor)=>controls.TryGetValue(actor,out var c)?c.input.sequence:0;
        public void Forget(string actor)=>controls.Remove(actor);
    }
    // Timestamped visual samples only. No extrapolation through long outages,
    // no smoothing across rooms/visits or a changed item reservation.
    public sealed class MotionBuffer
    {
        private readonly List<(double time,float x,float y)> points=new List<(double,float,float)>();
        private string generation;
        public int Count=>points.Count;
        public bool Add(string key,double time,float x,float y)
        {
            if(key!=generation){points.Clear();generation=key;}
            if(points.Count>0 && time<=points[points.Count-1].time)return false;
            points.Add((time,x,y));if(points.Count>32)points.RemoveAt(0);return true;
        }
        public WalkPoint Sample(double time)
        {
            if(points.Count==0)return new WalkPoint(0,0);
            while(points.Count>2 && points[1].time<=time)points.RemoveAt(0);
            var first=points[0];if(time<=first.time)return new WalkPoint(first.x,first.y);
            for(var i=1;i<points.Count;i++)
            {
                var b=points[i];if(b.time<time)continue;var a=points[i-1];var t=(float)((time-a.time)/(b.time-a.time));
                return new WalkPoint(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t);
            }
            var last=points[points.Count-1];return new WalkPoint(last.x,last.y);
        }
    }
}
