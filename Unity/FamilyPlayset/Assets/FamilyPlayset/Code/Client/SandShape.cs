using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Scene-native shapes keep each bucket's fill, water and tower visible.
    public sealed class SandShape : MaskableGraphic
    {
        public string kind="bucket";public int scoops,capacity=2,decoration;public bool wet,built;
        private VertexHelper mesh;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            mesh=vh;mesh.Clear();
            var wood=new Color(.57f,.38f,.21f);var edge=new Color(.37f,.25f,.15f);var sand=new Color(.96f,.83f,.53f);var damp=new Color(.83f,.66f,.37f);
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
            if(kind=="water"){Box(-28,-22,52,47,new Color(.36f,.67f,.87f));Poly(new Color(.36f,.67f,.87f),new Vector2(24,2),new Vector2(51,27),new Vector2(49,-5));Oval(-2,25,25,8,new Color(.7f,.86f,.95f));return;}
            if(kind=="flag"){Box(-3,-30,6,64,wood);Poly(new Color(.94f,.4f,.35f),new Vector2(3,34),new Vector2(43,23),new Vector2(3,8));return;}
            if(kind=="shell"){Shell(0,0);return;}
            if(kind=="tip"){Poly(new Color(.32f,.65f,.8f),new Vector2(-30,23),new Vector2(22,35),new Vector2(33,-13),new Vector2(-8,-24));Oval(28,-30,20,5,sand);return;}
            if(built){
                var h=capacity==3?105:75;
                Poly(damp,new Vector2(-48,0),new Vector2(-35,h),new Vector2(35,h),new Vector2(48,0));
                Box(-44,0,88,8,new Color(.72f,.55f,.29f));
                for(var i=0;i<3;i++)Box(-38+i*27,h-3,22,22,sand);
                Box(-10,5,20,30,new Color(.57f,.41f,.23f));Oval(0,35,10,10,new Color(.57f,.41f,.23f));
                for(var i=0;i<5;i++)Box(-28+i*12,48+(i%2)*12,5,3,sand);
                if(decoration==1){Box(-3,h+10,6,48,wood);Poly(new Color(.94f,.4f,.35f),new Vector2(3,h+60),new Vector2(40,h+49),new Vector2(3,h+34));}
                if(decoration==2)Shell(27,h+26);
            }else{
                var h=capacity==3?75:55;var bucket=new Color(.32f,.65f,.8f);
                Poly(edge,new Vector2(-46,h),new Vector2(46,h),new Vector2(31,0),new Vector2(-31,0));
                Poly(bucket,new Vector2(-40,h-4),new Vector2(40,h-4),new Vector2(26,4),new Vector2(-26,4));
                Oval(0,h,46,10,new Color(.72f,.89f,.95f));
                if(scoops>0){var f=scoops/(float)capacity;Poly(wet?damp:sand,new Vector2(-26,6),new Vector2(26,6),new Vector2(26+12*f,6+(h-10)*f),new Vector2(-26-12*f,6+(h-10)*f));}
                for(var i=0;i<capacity;i++)Box(-9,10+i*(h-15)/capacity,18,3,Color.white);
                if(wet){Oval(28,h+20,7,10,new Color(.35f,.65f,.94f));}
            }
        }
        private void Shell(float x,float y)
        {Oval(x,y,25,19,new Color(1,.67f,.52f));for(var i=-2;i<=2;i++)Box(x+i*7-1,y-8,3,20,new Color(.82f,.43f,.33f));}
        private void Box(float x,float y,float w,float h,Color c)=>Poly(c,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
        private void Oval(float x,float y,float rx,float ry,Color c){var p=new Vector2[20];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(c,p);}
        private void Poly(Color c,params Vector2[] points)
        {var start=mesh.currentVertCount;foreach(var p in points)mesh.AddVert(p,c,Vector2.zero);for(var i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);}
    }
}
