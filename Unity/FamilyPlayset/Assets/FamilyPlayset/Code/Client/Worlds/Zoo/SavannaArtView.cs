using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    // Articulates the original giraffe neck or lion head. Body, feet and authority's
    // walking route stay intact. Mouth and leaves use the same deformation.
    public sealed class SavannaArtView : RawImage
    {
        private float bend;
        public bool lion;
        public Vector2 mouth=new Vector2(.772f,.735f);
        public void Pose(float degrees){if(Mathf.Abs(bend-degrees)<.03f)return;bend=degrees;SetVerticesDirty();}
        private Vector2 ArtPoint(float x,float y)
        {
            var r=GetPixelAdjustedRect();var p=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
            var hinge=new Vector2(r.xMin+(lion ? .72f : .54f)*r.width,r.yMin+(lion ? .20f : .42f)*r.height);
            var shoulder=Mathf.SmoothStep(0,1,Mathf.Clamp01((x-.46f)/.12f));
            // The entire upper head, including the far ear, moves rigidly.
            // A shoulder-only horizontal mask would stretch that ear.
            var weight=Mathf.SmoothStep(0,1,Mathf.Clamp01((y-.40f)/.19f))*Mathf.Lerp(shoulder,1,Mathf.Clamp01((y-.5f)/.12f));
            if(lion)weight=Mathf.SmoothStep(0,1,Mathf.Clamp01((x-.64f)/.16f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((y-.18f)/.13f));
            var d=p-hinge;var angle=-bend*weight*Mathf.Deg2Rad;
            return hinge+new Vector2(d.x*Mathf.Cos(angle)-d.y*Mathf.Sin(angle),d.x*Mathf.Sin(angle)+d.y*Mathf.Cos(angle));
        }
        public Vector2 MouthTip=>Vector2.Scale(ArtPoint(mouth.x,mouth.y),rectTransform.localScale)+rectTransform.anchoredPosition;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            if(bend<.03f){base.OnPopulateMesh(mesh);return;}mesh.Clear();const int n=40;var v=UIVertex.simpleVert;v.color=color;
            for(var y=0;y<=n;y++)for(var x=0;x<=n;x++){var u=x/(float)n;var t=y/(float)n;v.position=ArtPoint(u,t);v.uv0=new Vector2(uvRect.x+u*uvRect.width,uvRect.y+t*uvRect.height);mesh.AddVert(v);}
            for(var y=0;y<n;y++)for(var x=0;x<n;x++){var i=y*(n+1)+x;mesh.AddTriangle(i,i+n+1,i+1);mesh.AddTriangle(i+1,i+n+1,i+n+2);}
        }
    }
}
