using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Coordinates are in the retained P9 piece artwork's local units, not world
    // floor units. Authority and touch use one measured surface map. No slots.
    public static class SandDecorSurface
    {
        public const int Schema=53, PieceLimit=16, GroundLimit=48;
        public static bool Kind(string kind)=>kind=="flag" || kind=="shell" || kind=="pebble" || kind=="door" || kind=="window";
        public static bool GroundKind(string kind)=>kind=="shell" || kind=="pebble";
        public static WalkPoint LegacyAnchor(SandMould m,int slot)
        {
            var side=slot%2==0?-1:1;
            if(!DaycareSandpit.LongShape(m.shape))return new WalkPoint(side*(slot<2?22:slot<4?59:25),slot<2?176:slot<4?-12:76);
            if(m.orientation==90)return slot<2?new WalkPoint(side*34,side<0?104:215):slot<4?new WalkPoint(side*42,side<0?-78:87):new WalkPoint(side*28,side<0?5:120);
            return new WalkPoint(side*(slot<2?m.shape=="gate"?116:93:slot<4?140:116),slot<2?m.shape=="gate"?131:75:slot<4?-13:43);
        }
        private static float Clamp(float x,float lo,float hi)=>Math.Max(lo,Math.Min(hi,x));
        private static WalkPoint Rect(float x,float y,float l,float r,float b,float t)=>new WalkPoint(Clamp(x,l,r),Clamp(y,b,t));
        private static float Distance(WalkPoint p,float x,float y)=>(float)Math.Sqrt((p.X-x)*(p.X-x)+(p.Y-y)*(p.Y-y));
        public static bool Resolve(SandMould m,string kind,float x,float y,out WalkPoint at)
        {
            if(!Candidate(m,kind,x,y,out at))return false;
            if(SandDecorPaint.Fits(m,kind,at.X,at.Y))return true;
            // Near an illustrated edge, find support only within the intended
            // local region. Valid interior taps retain their exact coordinates.
            var best=float.MaxValue;var found=false;var resolved=at;
            for(var dx=-22;dx<=22;dx+=2)for(var dy=-22;dy<=22;dy+=2){
                if(!Candidate(m,kind,x+dx,y+dy,out var p) || !SandDecorPaint.Fits(m,kind,p.X,p.Y))continue;
                var d=Distance(p,x,y);if(d<=22 && d<best){resolved=p;best=d;found=true;}
            }
            at=resolved;return found;
        }
        private static bool Candidate(SandMould m,string kind,float x,float y,out WalkPoint at)
        {
            at=default;if(m==null || !m.built || !Kind(kind) || !KeepyRules.Finite(x) || !KeepyRules.Finite(y))return false;
            if(kind=="flag"){
                if(!DaycareSandpit.LongShape(m.shape)){var cx=Clamp(x,-43,32);at=new WalkPoint(cx,166+(float)Math.Sqrt(Math.Max(0,1-cx*cx/3600))*8);}
                else if(m.orientation==90){
                    // Follow the receding top edge, rather than jumping between its ends.
                    var dx=58f;var dy=106f;var t=Clamp(((x+30)*dx+(y-107)*dy)/(dx*dx+dy*dy),0,1);at=new WalkPoint(-30+t*dx,107+t*dy);
                }else{var cx=Clamp(x,-116,92);at=new WalkPoint(cx,m.shape=="gate"?130:82-.12f*(cx+90));}
                return Distance(at,x,y)<=64;
            }
            WalkPoint[] regions;
            if(!DaycareSandpit.LongShape(m.shape))regions=new[]{Rect(x,y,-44,44,18,122)};
            else if(m.orientation==0 && m.shape=="wall"){
                var cx=Clamp(x,-115,112);var slope=-.12f*(cx+90);regions=new[]{new WalkPoint(cx,Clamp(y,18+slope,62+slope))};
            }else if(m.orientation==0)regions=new[]{Rect(x,y,-119,-84,15,105),Rect(x,y,84,119,15,105),Rect(x,y,-84,84,113,122)};
            else if(m.shape=="wall")regions=new[]{Rect(x,y,-32,1,-48,154),Rect(x,y,1,28,75,184)};
            // The lower rotated gate pillars are narrower than the retained prop
            // artwork. New props use its wider upper face; old anchors migrate intact.
            else regions=new[]{Rect(x,y,-27,28,83,174)};
            at=regions.OrderBy(p=>Distance(p,x,y)).First();
            // Clamp only close to the intended face. Gate holes are never bridged.
            if(m.shape=="gate" && (m.orientation==0?Math.Abs(x)<72 && y<100:x<17 && y<65))return false;
            return Distance(at,x,y)<=22;
        }
        public static bool Supported(SandMould m,SandAttachment a)=>Resolve(m,a.kind,a.x,a.y,out var p) && Distance(p,a.x,a.y)<.1f;
        private static WalkPoint Center(SandAttachment a)=>new WalkPoint(a.x+(a.kind=="flag"?19:0),a.y+(a.kind=="flag"?27.5f:a.kind=="shell"?6:a.kind=="door"?2.5f:a.kind=="pebble"?4:1));
        private static float Width(string kind)=>kind=="flag" || kind=="shell"?48:kind=="door"?26:32;
        private static float Height(string kind)=>kind=="flag"?55:kind=="door"?45:kind=="shell"?38:kind=="pebble"?24:30;
        public static bool Overlap(SandAttachment a,SandAttachment b)
        {var p=Center(a);var q=Center(b);return Math.Abs(p.X-q.X)<(Width(a.kind)+Width(b.kind))/2+2 && Math.Abs(p.Y-q.Y)<(Height(a.kind)+Height(b.kind))/2+2;}
        public static string AttachedReason(SandMould m,SandAttachment a,string excluding=null)
        {
            if(!Supported(m,a))return "choose-sand-region";
            if(m.attachments.Length>=PieceLimit && excluding==null)return "sand-decor-full";
            return m.attachments.Any(b=>b.id!=excluding && Overlap(a,b))?"sand-decor-taken":null;
        }
        public static string GroundReason(SandpitState g,SandAttachment a,string excluding=null)
        {
            if(!GroundKind(a.kind) || !DaycareSandpit.Inside(a.x,a.y,28,28))return "outside-sandpit";
            if(g.ground.Length>=GroundLimit && excluding==null)return "sand-decor-full";
            if(g.moulds.Any(m=>Math.Abs(m.x-a.x)<m.width/2+12 && Math.Abs(m.y-a.y)<m.depth/2+12) || g.toy?.placed==true && Math.Abs(g.toy.x-a.x)<34 && Math.Abs(g.toy.y-a.y)<34)return "sand-spot-taken";
            return g.ground.Any(b=>b.id!=excluding && GroundOverlap(a,b))?"sand-decor-taken":null;
        }
        public static bool GroundOverlap(SandAttachment a,SandAttachment b)=>Math.Abs(a.x-b.x)<27 && Math.Abs(a.y-b.y)<30;
    }
}
