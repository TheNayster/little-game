using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
 // Articulate the retained atlas drawing: head acknowledgement, tail/fins or
 // flippers. No elephant/trunk deformation and no change to feeding poses.
 public sealed class ZooArtView : RawImage
 {
  private Vector2 head,tail;private float headAngle,tailAngle,headReach,tailSway;
  public void Pose(string id,ZooPhase phase,float age,float envelope)
  {
   var greeting=phase==ZooPhase.Greet || phase==ZooPhase.CareFinish;var play=phase==ZooPhase.Curious || phase==ZooPhase.Splash;
   var idle=phase==ZooPhase.Rest || phase==ZooPhase.Browse || phase==ZooPhase.Drink;headAngle=tailAngle=headReach=tailSway=0;
   // Anchors are in the atlas cell; eyes and jaw move together as one head.
   switch(id){
    case "zebra":head=new Vector2(.75f,.29f);tail=new Vector2(.17f,.37f);headAngle=greeting?Mathf.Sin(age*5)*5*envelope:play?-5*envelope:idle?Mathf.Sin(age*1.3f)*2:0;tailAngle=Mathf.Sin(age*4)*(play?10:4);break;
    case "triceratops":head=new Vector2(.69f,.37f);tail=new Vector2(.2f,.32f);headAngle=greeting?6*envelope:play?Mathf.Sin(age*3)*5*envelope:idle?Mathf.Sin(age)*2:0;tailAngle=Mathf.Sin(age*1.4f)*3;break;
    case "stegosaurus":head=new Vector2(.79f,.4f);tail=new Vector2(.2f,.42f);headAngle=greeting?4*envelope:play?6*envelope:idle?Mathf.Sin(age*.9f)*2:0;tailAngle=Mathf.Sin(age*1.5f)*(play?7:3);break;
    case "tyrannosaurus":head=new Vector2(.65f,.6f);tail=new Vector2(.21f,.34f);headAngle=greeting?Mathf.Sin(age*4)*5*envelope:play?8*envelope:idle?Mathf.Sin(age*.9f)*1.5f:0;tailAngle=Mathf.Sin(age*1.8f)*4;break;
    case "clownfish":head=new Vector2(.7f,.52f);tail=new Vector2(.24f,.49f);headAngle=greeting?Mathf.Sin(age*3)*3*envelope:0;tailAngle=Mathf.Sin(age*7)*(play?13:7);tailSway=play?Mathf.Sin(age*5)*.02f*envelope:0;break;
    case "blue-tang":head=new Vector2(.69f,.51f);tail=new Vector2(.24f,.5f);headAngle=greeting?3*envelope:0;tailAngle=Mathf.Sin(age*5)*(play?11:6);tailSway=play?Mathf.Sin(age*4)*.025f*envelope:0;break;
    case "zebra-shark":head=new Vector2(.77f,.48f);tail=new Vector2(.2f,.45f);headAngle=greeting?2*envelope:0;tailAngle=Mathf.Sin(age*2)*(play?9:4);tailSway=play?Mathf.Sin(age*2)*.025f*envelope:0;break;
    case "penguin":head=new Vector2(.58f,.68f);tail=new Vector2(.3f,.39f);headAngle=greeting?Mathf.Sin(age*5)*5*envelope:idle?Mathf.Sin(age*1.5f)*2:0;tailAngle=play?Mathf.Sin(age*7)*13*envelope:3*Mathf.Sin(age*2);break;
    case "tortoise":head=new Vector2(.7f,.42f);tail=new Vector2(.26f,.27f);headAngle=greeting?3*envelope:idle?Mathf.Sin(age*.65f):0;headReach=play?.016f*envelope:idle?.005f*Mathf.Sin(age*.7f):0;break;
    case "gecko":head=new Vector2(.68f,.32f);tail=new Vector2(.24f,.25f);headAngle=greeting?Mathf.Sin(age*4)*4*envelope:play?6*envelope:idle?Mathf.Sin(age*1.8f)*2:0;tailAngle=Mathf.Sin(age*2.5f)*(play?6:2);break;
    case "iguana":head=new Vector2(.67f,.42f);tail=new Vector2(.23f,.26f);headAngle=greeting?Mathf.Sin(age*4)*5*envelope:play?4*envelope:idle?Mathf.Sin(age*1.1f)*2:0;tailAngle=Mathf.Sin(age*1.3f)*(play?6:2);break;
    case "crocodile":head=new Vector2(.72f,.37f);tail=new Vector2(.21f,.25f);headAngle=greeting?2*envelope:play?3*envelope:idle?Mathf.Sin(age*.6f):0;tailAngle=Mathf.Sin(age)*(play?5:1.5f);break;
   }
   SetVerticesDirty();
  }
  public void ClearPose(){if(headAngle==0 && tailAngle==0 && headReach==0 && tailSway==0)return;headAngle=tailAngle=headReach=tailSway=0;SetVerticesDirty();}
  private Vector2 Point(float x,float y)
  {
   var r=GetPixelAdjustedRect();var original=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);var result=original;
   var headWeight=Mathf.SmoothStep(0,1,Mathf.Clamp01((x-head.x)/.16f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((y-head.y+.1f)/.13f));
   var tailWeight=Mathf.SmoothStep(0,1,Mathf.Clamp01((tail.x-x)/.17f));
   result+=Turn(original,new Vector2(r.xMin+head.x*r.width,r.yMin+head.y*r.height),headAngle*headWeight)-original;
   result+=Turn(original,new Vector2(r.xMin+tail.x*r.width,r.yMin+tail.y*r.height),tailAngle*tailWeight)-original;
   result+=new Vector2(headReach*r.width*headWeight,tailSway*r.height*tailWeight);return result;
  }
  private static Vector2 Turn(Vector2 p,Vector2 hinge,float degrees){var d=p-hinge;var a=degrees*Mathf.Deg2Rad;return hinge+new Vector2(d.x*Mathf.Cos(a)-d.y*Mathf.Sin(a),d.x*Mathf.Sin(a)+d.y*Mathf.Cos(a));}
  protected override void OnPopulateMesh(VertexHelper mesh)
  {
   if(Mathf.Abs(headAngle)+Mathf.Abs(tailAngle)+Mathf.Abs(headReach)+Mathf.Abs(tailSway)<.001f){base.OnPopulateMesh(mesh);return;}
   mesh.Clear();const int n=24;var v=UIVertex.simpleVert;v.color=color;
   for(var y=0;y<=n;y++)for(var x=0;x<=n;x++){var u=x/(float)n;var t=y/(float)n;v.position=Point(u,t);v.uv0=new Vector2(uvRect.x+u*uvRect.width,uvRect.y+t*uvRect.height);mesh.AddVert(v);}
   for(var y=0;y<n;y++)for(var x=0;x<n;x++){var i=y*(n+1)+x;mesh.AddTriangle(i,i+n+1,i+1);mesh.AddTriangle(i+1,i+n+1,i+n+2);}
  }
 }
}
