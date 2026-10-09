using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Articulate the original transparent resting drawing, without replacing
    // approved pixels or moving/scaling the body and its gameplay anchors.
    public sealed class ElephantArtView:RawImage
    {
        private float trunk,ear;
        private Vector2 reach;
        public void Reach(Vector2 offset){if((reach-offset).sqrMagnitude<.0001f)return;reach=offset;SetVerticesDirty();}
        public void Pose(float angle,float earShift)
        {if(Mathf.Abs(trunk-angle)<.02f && Mathf.Abs(ear-earShift)<.02f)return;trunk=angle;ear=earShift;SetVerticesDirty();}
        private Vector2 ArtPoint(float x,float y)
        {
            var r=GetPixelAdjustedRect();var p=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
            var weight=Mathf.Clamp01((x-.76f)/.09f)*Mathf.Clamp01((.55f-y)/.1f);
            var hinge=new Vector2(r.xMin+.88f*r.width,r.yMin+.55f*r.height);var d=p-hinge;var angle=trunk*weight*Mathf.Deg2Rad;
            p=hinge+new Vector2(d.x*Mathf.Cos(angle)-d.y*Mathf.Sin(angle),d.x*Mathf.Sin(angle)+d.y*Mathf.Cos(angle));
            var earWeight=Mathf.Clamp01(1-Mathf.Abs(x-.55f)/.13f)*Mathf.Clamp01(1-Mathf.Abs(y-.65f)/.22f);
            p.x+=ear*earWeight;return p+reach*weight;
        }
        public Vector2 TrunkTip=>Vector2.Scale(ArtPoint(.87f,.23f),rectTransform.localScale)+rectTransform.anchoredPosition;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            if(Mathf.Abs(trunk)<.02f && Mathf.Abs(ear)<.02f && reach.sqrMagnitude<.0001f){base.OnPopulateMesh(mesh);return;}
            mesh.Clear();const int n=24;var vertex=UIVertex.simpleVert;vertex.color=color;
            for(var y=0;y<=n;y++)for(var x=0;x<=n;x++){
                var u=x/(float)n;var v=y/(float)n;vertex.position=ArtPoint(u,v);
                vertex.uv0=new Vector2(uvRect.x+u*uvRect.width,uvRect.y+v*uvRect.height);mesh.AddVert(vertex);
            }
            for(var y=0;y<n;y++)for(var x=0;x<n;x++){var i=y*(n+1)+x;mesh.AddTriangle(i,i+n+1,i+1);mesh.AddTriangle(i+1,i+n+1,i+n+2);}
        }
    }
}
