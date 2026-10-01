using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client {
 public sealed class TreasureShape : MaskableGraphic {
  public string kind="shell";public int symbol,count=3;public bool special,raised,open;public float pulse=-1;private VertexHelper mesh;
  protected override void OnPopulateMesh(VertexHelper vh){mesh=vh;mesh.Clear();var gold=new Color(1,.77f,.2f);var wood=new Color(.58f,.34f,.16f);var ink=new Color(.36f,.28f,.24f);
   if(kind=="palm"){Box(-7,-42,14,67,wood);for(var i=-2;i<=2;i++)Poly(new Color(.38f,.64f,.29f),new Vector2(0,24),new Vector2(i*21-21,45-Mathf.Abs(i)*8),new Vector2(i*25,12));Oval(0,-42,45,10,new Color(.94f,.81f,.5f));return;}
   if(kind=="flowers"){Oval(0,-35,57,15,new Color(.5f,.72f,.34f));for(var i=-1;i<=1;i++){Box(i*26-2,-30,4,48,wood);Oval(i*26,27,19,18,new Color(.7f,.43f,.87f));Oval(i*26,27,7,7,gold);}return;}
   if(kind=="lookout"){Poly(new Color(.48f,.7f,.3f),new Vector2(-60,-40),new Vector2(0,26),new Vector2(60,-40));Box(-3,16,6,36,wood);Poly(new Color(.66f,.8f,.88f),new Vector2(3,50),new Vector2(42,34),new Vector2(3,22));return;}
   if(kind=="map"){Poly(new Color(1,.93f,.7f),new Vector2(-46,-32),new Vector2(-37,34),new Vector2(39,29),new Vector2(45,-34));Box(-32,-23,64,3,wood);Glyph(symbol,0,2,27);return;}
   if(kind=="pot"){Poly(new Color(.83f,.43f,.3f),new Vector2(-38,18),new Vector2(38,18),new Vector2(28,-25),new Vector2(-28,-25));Box(-44,16,88,12,new Color(.96f,.6f,.4f));for(var i=0;i<count;i++){var x=-23+i*23;Box(x-2,22,4,31,new Color(.36f,.59f,.28f));for(var j=0;j<5;j++){var a=j*Mathf.PI*2/5;Oval(x+Mathf.Cos(a)*10,55+Mathf.Sin(a)*10,8,8,special?new Color(.7f,.43f,.87f):new Color(1,.78f,.33f));}Oval(x,55,6,6,gold);}return;}
   if(kind=="chimes"){Box(-120,125,240,10,wood);for(var i=0;i<3;i++){Box(-83+i*83,55,3,72,wood);if(pulse==i)Oval(-83+i*83,45,39,39,new Color(1,.91f,.5f));Glyph(i,-83+i*83,45,28);Box(-93+i*83,-25,22,42,new Color(.66f,.8f,.88f));}return;}
   if(kind=="dig"){Oval(0,-10,70,21,new Color(.78f,.62f,.35f));Oval(0,0,60,20,new Color(.95f,.82f,.56f));for(var i=0;i<6;i++)Oval(-45+i*17,-3+i%2*8,3,2,wood);Box(-7,10,14,3,wood);Box(-1,6,3,12,wood);return;}
   if(kind=="chest"){Box(-95,-10,190,78,wood);Box(-85,0,170,53,new Color(.76f,.48f,.25f));Box(-88,-11,13,80,gold);Box(75,-11,13,80,gold);Box(-98,53,196,13,ink);Box(-12,18,24,26,gold);if(!open){Oval(0,71,95,34,wood);Box(-95,50,190,22,new Color(.73f,.43f,.2f));Box(-11,55,22,45,gold);}else{Box(-95,92,190,34,wood);Box(-85,100,170,18,gold);Glyph(2,-53,55,27);Glyph(1,0,58,24);Glyph(0,50,62,27);Box(-22,47,45,8,gold);}return;}
   if(kind=="crab"){Oval(0,0,27,15,new Color(.98f,.43f,.32f));for(var i=-1;i<=1;i+=2){Box(i*36,-7,15,5,new Color(.98f,.43f,.32f));Oval(i*36,14,12,10,new Color(.98f,.43f,.32f));Box(i*12-2,11,4,16,ink);Oval(i*12,29,5,5,Color.white);Oval(i*12,29,2,3,ink);}return;}
   if(kind=="boot"){Box(-19,-18,33,43,new Color(.37f,.68f,.75f));Oval(10,-16,27,12,new Color(.37f,.68f,.75f));Box(-23,-27,60,7,ink);return;}
   if(kind=="compass"){Oval(0,0,32,32,gold);Oval(0,0,25,25,new Color(1,.97f,.84f));Poly(new Color(.93f,.34f,.31f),new Vector2(0,22),new Vector2(-8,-9),new Vector2(8,-9));return;}
   if(kind=="boat"){Poly(wood,new Vector2(-36,0),new Vector2(36,0),new Vector2(21,-20),new Vector2(-21,-20));Box(-2,0,4,59,wood);Poly(new Color(.97f,.62f,.36f),new Vector2(2,59),new Vector2(2,5),new Vector2(36,5));return;}
   Glyph(kind=="symbol"?symbol:0,0,0,42);
  }
  private void Glyph(int i,float x,float y,float r){
   if(i==0){Poly(special?new Color(.94f,.32f,.31f):new Color(1,.68f,.48f),new Vector2(x-r*.5f,y-r*.4f),new Vector2(x-r,y),new Vector2(x-r*.85f,y+r*.45f),new Vector2(x-r*.45f,y+r*.75f),new Vector2(x,y+r*.85f),new Vector2(x+r*.45f,y+r*.75f),new Vector2(x+r*.85f,y+r*.45f),new Vector2(x+r,y),new Vector2(x+r*.5f,y-r*.4f));for(var a=-2;a<=2;a++)Box(x+a*r*.26f-2,y-r*.42f,4,r*.9f,special?new Color(1,.91f,.76f):new Color(.82f,.45f,.31f));}
   if(i==1){Poly(new Color(.44f,.7f,.35f),new Vector2(x-r,y-r*.5f),new Vector2(x-r*.6f,y+r*.5f),new Vector2(x+r,y+r*.8f),new Vector2(x+r*.6f,y-r*.5f));Box(x-r*.4f,y-2,r,4,new Color(.27f,.48f,.23f));}
   if(i==2){var p=new Vector2[10];for(var n=0;n<10;n++){var a=n*Mathf.PI/5+Mathf.PI/2;var v=n%2==0?r:r*.46f;p[n]=new Vector2(x+Mathf.Cos(a)*v,y+Mathf.Sin(a)*v);}Poly(new Color(1,.78f,.25f),p);}
  }
  private void Box(float x,float y,float w,float h,Color c)=>Poly(c,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
  private void Oval(float x,float y,float rx,float ry,Color c){var p=new Vector2[24];for(var i=0;i<p.Length;i++){var a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(c,p);}
  private void Poly(Color c,params Vector2[] p){var s=mesh.currentVertCount;foreach(var v in p)mesh.AddVert(v,c,Vector2.zero);for(var i=1;i<p.Length-1;i++)mesh.AddTriangle(s,s+i,s+i+1);}
 }
}
