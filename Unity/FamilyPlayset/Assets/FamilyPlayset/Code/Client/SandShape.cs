using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Scene-native shapes keep each bucket's fill, water and tower visible.
    public sealed class SandShape : MaskableGraphic
    {
        public string kind="bucket";public int scoops,capacity=2,decoration;public bool wet,built,pouring;
        public Vector2 source,target;public float progress;
        public string tipOutcome;public float towerReveal=1,wiggle;public bool hideBucket;
        private VertexHelper mesh;private float ceiling=float.PositiveInfinity, bucketAngle;private Vector2 bucketAt,bucketScale=Vector2.one;private bool transformingBucket;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            mesh=vh;mesh.Clear();ceiling=float.PositiveInfinity;transformingBucket=false;
            var wood=new Color(.57f,.38f,.21f);var edge=new Color(.37f,.25f,.15f);var sand=new Color(.96f,.83f,.53f);var damp=new Color(.83f,.66f,.37f);
            if(kind=="tip-cue"){
                var c=new Color(1,.65f,.05f);for(var i=0;i<9;i++){var a=(i/8f)*Mathf.PI;Oval(Mathf.Cos(a)*19,Mathf.Sin(a)*19,4,4,c);}
                Poly(c,new Vector2(-29,5),new Vector2(-13,4),new Vector2(-20,-12));return;
            }
            if(kind=="mould-tip"){DrawTip();return;}
            if(kind=="watering-can"){
                Oval(0,0,47,10,new Color(.37f,.36f,.22f,.28f));
                if(!pouring)WaterCan(new Vector2(0,24),0);return;
            }
            if(kind=="water-pour"){
                const float tilt=-.6f;
                var nozzle=Turn(new Vector2(51,27),tilt);
                var above=target+new Vector2(-nozzle.x,40-nozzle.y);
                var t=progress<.25f?Mathf.SmoothStep(0,1,progress/.25f):progress>.75f?1-Mathf.SmoothStep(0,1,(progress-.75f)/.25f):1;
                var at=Vector2.Lerp(source,above,t);
                WaterCan(at,tilt*t);
                if(progress>=.25f && progress<=.75f){
                    var from=above+nozzle;var blue=new Color(.25f,.68f,1);
                    Box(from.x-2,target.y,4,from.y-target.y,new Color(.4f,.76f,1,.5f));
                    for(var i=0;i<5;i++){var drop=Vector2.Lerp(from,target,Mathf.Repeat(progress*7+i*.2f,1));Oval(drop.x,drop.y,4,7,blue);}
                    Oval(target.x,target.y,13,4,blue);
                }
                return;
            }
            if(kind=="shovel"){
                Oval(0,-8,72,21,edge);Oval(0,-3,68,21,sand);
                for(var i=0;i<9;i++)Oval(-49+i*12,(i%3)*5-6,3,2,damp);
                Poly(wood,new Vector2(-7,8),new Vector2(3,10),new Vector2(-20,84),new Vector2(-30,82));
                Poly(new Color(.15f,.55f,.57f),new Vector2(-22,32),new Vector2(24,26),new Vector2(15,-7),new Vector2(-3,-17),new Vector2(-19,-5));
                Oval(-25,86,23,19,new Color(1,.52f,.22f));Oval(-25,87,12,9,new Color(1,.95f,.78f));return;
            }
            if(kind=="sand-transfer"){
                var p=Vector2.Lerp(source,target,progress)+new Vector2(0,Mathf.Sin(progress*Mathf.PI)*65);
                Oval(p.x,p.y,17,10,sand);
                for(var i=0;i<5;i++)Oval(p.x-24+i*9,p.y-13-(i%2)*7,3,3,damp);return;
            }
            if(kind=="wall"){
                Box(-250,0,500,37,damp);for(var i=0;i<17;i++)Box(-250+i*30,35,19,13,sand);
                Box(-22,0,44,29,new Color(.57f,.41f,.23f));Oval(0,29,22,10,new Color(.57f,.41f,.23f));return;
            }
            if(kind=="pit"){
                Poly(edge,new Vector2(-382,-35),new Vector2(-350,80),new Vector2(345,80),new Vector2(382,-35));
                Poly(sand,new Vector2(-355,-27),new Vector2(-335,62),new Vector2(330,62),new Vector2(356,-27));
                Box(-380,-51,760,25,wood);Box(-353,62,704,22,wood);
                for(var i=0;i<10;i++)Box(-325+i*72,-43,43,3,new Color(.7f,.49f,.29f));
                for(var i=0;i<18;i++)Oval(-310+(i*83)%620,-10+(i*19)%52,5,2,damp);
                foreach(var x in new[]{-380f,350f}){Box(x,-47,32,55,edge);Oval(x+16,8,19,7,wood);}
                return;
            }
            if(kind=="scoop"){Box(-5,-26,10,48,wood);Oval(0,29,24,14,new Color(.85f,.44f,.23f));return;}
            if(kind=="water"){WaterCan(Vector2.zero,0);return;}
            if(kind=="flag"){Box(-3,-30,6,64,wood);Poly(new Color(.94f,.4f,.35f),new Vector2(3,34),new Vector2(43,23),new Vector2(3,8));return;}
            if(kind=="shell"){Shell(0,0);return;}
            if(kind=="tip"){Poly(new Color(.32f,.65f,.8f),new Vector2(-30,23),new Vector2(22,35),new Vector2(33,-13),new Vector2(-8,-24));Oval(28,-30,20,5,sand);return;}
            if(built){
                var h=capacity==3?105:75;ceiling=towerReveal>=1?float.PositiveInfinity:towerReveal*(h+22);
                Poly(damp,new Vector2(-48,0),new Vector2(-35,h),new Vector2(35,h),new Vector2(48,0));
                Box(-44,0,88,8,new Color(.72f,.55f,.29f));
                for(var i=0;i<3;i++)Box(-38+i*27,h-3,22,22,sand);
                Box(-10,5,20,30,new Color(.57f,.41f,.23f));Oval(0,35,10,10,new Color(.57f,.41f,.23f));
                for(var i=0;i<5;i++)Box(-28+i*12,48+(i%2)*12,5,3,sand);
                if(decoration==1){Box(-3,h+10,6,48,wood);Poly(new Color(.94f,.4f,.35f),new Vector2(3,h+60),new Vector2(40,h+49),new Vector2(3,h+34));}
                if(decoration==2)Shell(27,h+26);
            }else if(!hideBucket){
                transformingBucket=true;bucketAt=new Vector2(wiggle,0);bucketAngle=0;bucketScale=Vector2.one;DrawBucket(scoops,wet);transformingBucket=false;
            }
        }
        private void DrawBucket(int fill,bool water)
        {
                var edge=new Color(.37f,.25f,.15f);var sand=new Color(.96f,.83f,.53f);var damp=new Color(.83f,.66f,.37f);
                var h=capacity==3?75:55;var bucket=new Color(.32f,.65f,.8f);
                Poly(edge,new Vector2(-46,h),new Vector2(46,h),new Vector2(31,0),new Vector2(-31,0));
                Poly(bucket,new Vector2(-40,h-4),new Vector2(40,h-4),new Vector2(26,4),new Vector2(-26,4));
                Oval(0,h,46,10,new Color(.72f,.89f,.95f));
                if(water && fill==0)Oval(0,h,34,6,new Color(.38f,.72f,.93f));
                if(fill>0){var f=fill/(float)capacity;Poly(water?damp:sand,new Vector2(-26,6),new Vector2(26,6),new Vector2(26+12*f,6+(h-10)*f),new Vector2(-26-12*f,6+(h-10)*f));}
                for(var i=0;i<capacity;i++)Box(-9,10+i*(h-15)/capacity,18,3,Color.white);
                if(water){Oval(28,h+20,7,10,new Color(.35f,.65f,.94f));}
        }
        private void DrawTip()
        {
            var t=Mathf.Clamp01(progress);var h=capacity==3?75f:55f;
            var turn=Mathf.SmoothStep(0,1,t/.32f);var returnHome=Mathf.SmoothStep(0,1,(t-.82f)/.18f);
            var reveal=tipOutcome=="reveal";var height=capacity==3?127f:97f;
            var lift=reveal?Mathf.SmoothStep(0,1,(t-.52f)/.3f)*height:Mathf.SmoothStep(0,1,(t-.45f)/.2f)*45;
            // The inverted open rim sits at ground level. Lift uncovers the permanent tower.
            var angle=Mathf.PI*turn*(1-returnHome);
            var stretch=reveal?Mathf.Lerp(1,height/h,turn)*(1-returnHome)+returnHome:1;
            var at=new Vector2(reveal?0:-70*Mathf.SmoothStep(0,1,(t-.45f)/.2f),Mathf.Sin(turn*Mathf.PI)*35+h*stretch*turn+lift);
            if(reveal){angle=Mathf.PI*turn;stretch=Mathf.Lerp(1,height/h,turn);at=new Vector2(-90*returnHome,Mathf.Sin(turn*Mathf.PI)*35+h*stretch*turn+lift+25*returnHome);}
            else at=Vector2.Lerp(at,Vector2.zero,returnHome);
            if(!reveal && t>=.28f){
                var crumble=Mathf.SmoothStep(0,1,(t-.52f)/.3f);var pileHeight=Mathf.Lerp(53,7,crumble);var spread=Mathf.Lerp(34,73,crumble);
                // Separate the loose pile from the similarly coloured pit floor.
                Oval(0,1,spread+5,7,new Color(.57f,.38f,.21f,.3f));
                Poly(new Color(.72f,.55f,.29f),new Vector2(-spread-2,0),new Vector2(-spread*.4f-2,pileHeight+2),new Vector2(3,pileHeight+6),new Vector2(spread*.55f+2,pileHeight*.6f+2),new Vector2(spread+2,0));
                Poly(new Color(1,.9f,.65f),new Vector2(-spread,0),new Vector2(-spread*.4f,pileHeight),new Vector2(3,pileHeight+4),new Vector2(spread*.55f,pileHeight*.6f),new Vector2(spread,0));
                for(var i=0;i<12;i++){var x=-55+i*10;var fall=Mathf.Repeat(t*3+i*.07f,1);Oval(x,Mathf.Lerp(65,3,fall)*(1-crumble),3,2,new Color(.83f,.66f,.37f));}
            }
            transformingBucket=true;bucketAt=at;bucketAngle=angle;bucketScale=new Vector2(1,stretch);
            DrawBucket(t<.32f?capacity:0,reveal);transformingBucket=false;
            if(reveal && t>=.68f && t<=.88f){
                var fade=1-(t-.68f)/.2f;for(var i=0;i<7;i++)Oval(-60+i*20,10+(i%3)*13,3*fade,3*fade,new Color(1,.88f,.4f));
            }
        }
        private static Vector2 Turn(Vector2 p,float angle)=>new Vector2(p.x*Mathf.Cos(angle)-p.y*Mathf.Sin(angle),p.x*Mathf.Sin(angle)+p.y*Mathf.Cos(angle));
        private void CanPoly(Vector2 at,float angle,Color color,params Vector2[] points)
        {for(var i=0;i<points.Length;i++)points[i]=at+Turn(points[i],angle);Poly(color,points);}
        private void CanOval(Vector2 at,float angle,float x,float y,float rx,float ry,Color color)
        {var p=new Vector2[20];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}CanPoly(at,angle,color,p);}
        private void WaterCan(Vector2 at,float angle)
        {
            var blue=new Color(.36f,.67f,.87f);var start=mesh.currentVertCount;
            // Open handle rather than painting a solid hole over the scenery.
            for(var i=0;i<20;i++){
                var a=i*Mathf.PI*2/20;
                mesh.AddVert(at+Turn(new Vector2(-30+Mathf.Cos(a)*20,4+Mathf.Sin(a)*25),angle),blue,Vector2.zero);
                mesh.AddVert(at+Turn(new Vector2(-30+Mathf.Cos(a)*12,4+Mathf.Sin(a)*17),angle),blue,Vector2.zero);
            }
            for(var i=0;i<20;i++){var next=(i+1)%20;mesh.AddTriangle(start+i*2,start+next*2,start+i*2+1);mesh.AddTriangle(start+i*2+1,start+next*2,start+next*2+1);}
            CanPoly(at,angle,blue,new Vector2(-28,-22),new Vector2(24,-22),new Vector2(24,25),new Vector2(-28,25));
            CanPoly(at,angle,blue,new Vector2(24,2),new Vector2(51,27),new Vector2(49,-5));
            CanOval(at,angle,-2,25,25,8,new Color(.7f,.86f,.95f));
        }
        private void Shell(float x,float y)
        {Oval(x,y,25,19,new Color(1,.67f,.52f));for(var i=-2;i<=2;i++)Box(x+i*7-1,y-8,3,20,new Color(.82f,.43f,.33f));}
        private void Box(float x,float y,float w,float h,Color c)=>Poly(c,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
        private void Oval(float x,float y,float rx,float ry,Color c){var p=new Vector2[20];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(c,p);}
        private void Poly(Color c,params Vector2[] points)
        {
            if(kind=="mould-tip" && tipOutcome=="reveal")c.a*=1-Mathf.SmoothStep(0,1,(progress-.82f)/.18f);
            if(transformingBucket){for(var i=0;i<points.Length;i++)points[i]=bucketAt+Turn(Vector2.Scale(points[i],bucketScale),bucketAngle);}
            if(!float.IsPositiveInfinity(ceiling)){
                var clipped=new System.Collections.Generic.List<Vector2>();var previous=points[points.Length-1];
                foreach(var point in points){
                    if((point.y<=ceiling)!=(previous.y<=ceiling))clipped.Add(Vector2.Lerp(previous,point,(ceiling-previous.y)/(point.y-previous.y)));
                    if(point.y<=ceiling)clipped.Add(point);previous=point;
                }
                points=clipped.ToArray();
            }
            var start=mesh.currentVertCount;foreach(var p in points)mesh.AddVert(p,c,Vector2.zero);for(var i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);
        }
    }
}
