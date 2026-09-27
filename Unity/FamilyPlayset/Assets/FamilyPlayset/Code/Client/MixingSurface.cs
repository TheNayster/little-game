using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Bounded local illustration of the authoritative mixture. No fluid or
    // particle simulation is sent to the server or stored in the save.
    public sealed class MixingSurface:MaskableGraphic
    {
        public MixingTray Tray;public int Mode,PourIngredient=-1;public bool Calm,Pouring;
        public float Clock;
        private static Color C(float r,float g,float b,float a=1)=>new Color(r,g,b,a);
        public void Paint(MixingTray t,int mode,bool calm){Tray=t;Mode=mode;Calm=calm;SetVerticesDirty();}
        private Vector3 P(float x,float y){var r=rectTransform.rect;return new Vector3(r.xMin+x/600*r.width,r.yMin+y/450*r.height);}
        private void Quad(VertexHelper h,float x,float y,float w,float z,Color c){var i=h.currentVertCount;h.AddVert(P(x,y),c,Vector2.zero);h.AddVert(P(x+w,y),c,Vector2.zero);h.AddVert(P(x+w,y+z),c,Vector2.zero);h.AddVert(P(x,y+z),c,Vector2.zero);h.AddTriangle(i,i+1,i+2);h.AddTriangle(i,i+2,i+3);}
        private void Oval(VertexHelper h,float x,float y,float rx,float ry,Color c,int count=32){var i=h.currentVertCount;h.AddVert(P(x,y),c,Vector2.zero);for(var k=0;k<=count;k++){var a=k*Mathf.PI*2/count;h.AddVert(P(x+rx*Mathf.Cos(a),y+ry*Mathf.Sin(a)),c,Vector2.zero);if(k>0)h.AddTriangle(i,i+k,i+k+1);}}
        private void Shape(VertexHelper h,Vector2[] points,Color c){var i=h.currentVertCount;foreach(var p in points)h.AddVert(P(p.x,p.y),c,Vector2.zero);for(var k=1;k<points.Length-1;k++)h.AddTriangle(i,i+k,i+k+1);}
        public static Color Liquid(MixingTray t,int mode)
        {
            if(mode==1 && t.amounts[5]>0){var balance=t.amounts[0]-t.amounts[1];return balance>0?C(.94f,.37f,.57f):balance<0?C(.29f,.66f,.72f):C(.62f,.4f,.79f);}
            var color=C(.55f,.82f,.91f);if(mode==3)color=C(.98f,.93f,.77f);
            var count=t.amounts[7]+t.amounts[8]+t.amounts[9];if(count==0)return color;
            var dye=(C(.98f,.31f,.43f)*t.amounts[7]+C(.28f,.54f,.93f)*t.amounts[8]+C(1,.83f,.25f)*t.amounts[9])/count;dye.a=1;return Color.Lerp(color,dye,.72f);
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var t=Tray;if(t==null)return;var clock=Calm?0:Clock;
            // Mint catch tray and soft shadow anchor the working vessel.
            Oval(h,300,50,270,43,C(.3f,.49f,.43f,.15f));Oval(h,300,76,276,53,C(.48f,.73f,.65f));Oval(h,300,85,260,45,C(.75f,.9f,.81f));
            var liquid=Liquid(t,Mode);var total=Mixing.Total(t);var level=Mathf.Min(250,108+total*4.4f);var volcano=Mode==0 && t.volcano;
            if(!volcano){
                Shape(h,new[]{new Vector2(95,276),new Vector2(505,276),new Vector2(468,110),new Vector2(420,77),new Vector2(180,77),new Vector2(132,110)},C(.74f,.9f,.94f,.26f));
                if(total>0){
                    var half=120+(level-85)*.4f;
                    Shape(h,new[]{new Vector2(300-half,level),new Vector2(300+half,level),new Vector2(432,110),new Vector2(414,95),new Vector2(186,95),new Vector2(168,110)},liquid);
                    Oval(h,300,level,half,27,Color.Lerp(liquid,Color.white,.22f));
                    if(Mode==2 && t.amounts[6]>0){
                        if(t.stir<=0){var oilTop=level+Mathf.Min(38,t.amounts[6]*5);var o=C(.98f,.77f,.31f,.85f);Quad(h,300-half,level,half*2,oilTop-level,o);Oval(h,300,oilTop,half,27,C(1,.84f,.43f,.9f));}
                        else for(var i=0;i<24;i++)Oval(h,195+(i*43%210),112+(i*29%Mathf.Max(10,level-110)),4+i%5,5+i%4,C(1,.8f,.35f,.9f));
                    }
                    if((Mode==0 && t.amounts[1]>t.reacted) || (Mode==3 && Mixing.Water(t)==0))for(var i=0;i<16;i++)Oval(h,205+i*13,109+Mathf.Sin(i)*5,11,7,C(1,.98f,.89f));
                    if(t.stir>0 && !Calm){for(var i=0;i<10;i++){var a=clock*1.8f+i*.5f;Oval(h,300+Mathf.Cos(a)*100,level+Mathf.Sin(a)*18,12,3,Color.Lerp(liquid,Color.white,.5f));}}
                    if(Mode==3 && t.poke>0){Oval(h,300,level,38,12,Mixing.Oobleck(t)?C(1,.98f,.87f):C(.4f,.64f,.74f,.55f));if(Mixing.Oobleck(t))for(var i=0;i<7;i++)Quad(h,252+i*16,level+18,3,14,C(.98f,.77f,.3f));}
                }
                // Rim and shine stay in front of the liquid, showing a real interior.
                Oval(h,300,282,212,39,C(.29f,.56f,.63f));Oval(h,300,282,206,33,C(.89f,.97f,.98f,.6f));Oval(h,300,282,196,26,C(.65f,.85f,.91f,.25f));
                Quad(h,139,143,7,89,C(1,1,1,.5f));Quad(h,153,202,5,28,C(1,1,1,.65f));
            }
            if(t.reaction>0 || t.foam>0){
                var origin=volcano?285:level;var foam=t.amounts[3]>0;var n=Calm?12:foam?36:18;
                for(var i=0;i<n;i++){var progress=Mathf.Repeat(clock*(.25f+i%5*.035f)+i*.618f,1);var x=300+Mathf.Sin(i*8.7f)*(volcano?65:120);var y=origin+(Calm?i%4*9:progress*(foam?100:110));var radius=foam?11+i%4*5:4+i%5;
                    Oval(h,x,y,radius,radius,C(.98f,.99f,.91f,foam?.88f:.55f));Oval(h,x-2,y+2,radius*.45f,radius*.45f,C(1,1,1,.65f));}
            }
            if(t.spill){for(var i=0;i<13;i++)Oval(h,140+i*27,85+Mathf.Sin(i)*12,24,10,Color.Lerp(liquid,Color.white,.7f));}
            if(Pouring){var powder=PourIngredient==1 || PourIngredient==4;var c=powder?C(1,.98f,.9f):PourIngredient==5?C(.65f,.38f,.8f):PourIngredient==6?C(1,.79f,.35f):C(.56f,.81f,.98f,.8f);
                var end=volcano?302:Mathf.Max(135,level);if(powder){for(var i=0;i<18;i++)Oval(h,300+Mathf.Sin(i*7)*13,end+Mathf.Repeat(i*.13f-clock*.7f,1)*(390-end),3,3,c);}
                else {Quad(h,294,end,12,390-end,c);Oval(h,300,end,24,6,c);}
            }
        }
    }
}
