using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    // Bend the existing resting atlas pixels at the shoulder. The feet/body
    // remain on the authority's path; no separate animal motion or new rules.
    public sealed class BrachiosaurusArtView : RawImage
    {
        private float bend;
        public void Pose(float degrees){if(Mathf.Abs(bend-degrees)<.05f)return;bend=degrees;SetVerticesDirty();}
        private Vector2 ArtPoint(float x,float y)
        {
            var r=GetPixelAdjustedRect();var p=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
            var weight=Mathf.SmoothStep(0,1,Mathf.Clamp01((y-.43f)/.16f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((x-.62f)/.11f));
            var hinge=new Vector2(r.xMin+.70f*r.width,r.yMin+.43f*r.height);var d=p-hinge;var angle=-bend*weight*Mathf.Deg2Rad;
            return hinge+new Vector2(d.x*Mathf.Cos(angle)-d.y*Mathf.Sin(angle),d.x*Mathf.Sin(angle)+d.y*Mathf.Cos(angle));
        }
        // Calibrated mouth in the retained resting cell. Both the food and
        // visible browse branch end here, including breathing and neck bend.
        public Vector2 MouthTip=>Vector2.Scale(ArtPoint(.89f,.887f),rectTransform.localScale)+rectTransform.anchoredPosition;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            if(Mathf.Abs(bend)<.05f){base.OnPopulateMesh(mesh);return;}mesh.Clear();const int n=40;var v=UIVertex.simpleVert;v.color=color;
            for(var y=0;y<=n;y++)for(var x=0;x<=n;x++){var u=x/(float)n;var t=y/(float)n;v.position=ArtPoint(u,t);v.uv0=new Vector2(uvRect.x+u*uvRect.width,uvRect.y+t*uvRect.height);mesh.AddVert(v);}
            for(var y=0;y<n;y++)for(var x=0;x<n;x++){var i=y*(n+1)+x;mesh.AddTriangle(i,i+n+1,i+1);mesh.AddTriangle(i+1,i+n+1,i+n+2);}
        }
    }
}
