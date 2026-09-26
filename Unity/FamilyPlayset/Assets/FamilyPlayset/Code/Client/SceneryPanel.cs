using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // A short overlap softens the meeting of separately authored panoramas.
    // The previous panel stays opaque underneath; only this leading edge fades.
    // Geometry uses the normal UI material, masking and texture batching.
    public sealed class SceneryPanel:RawImage
    {
        public bool blendLeadingEdge;
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();var r=GetPixelAdjustedRect();var fade=80f/2480;
            var columns=blendLeadingEdge?new[]{0f,fade,1f}:new[]{0f,1f};
            for(var i=0;i<columns.Length;i++)
            {
                var x=columns[i];var c=color;if(blendLeadingEdge && i==0)c.a=0;
                helper.AddVert(new Vector3(r.xMin+x*r.width,r.yMin),c,new Vector2(x,0));
                helper.AddVert(new Vector3(r.xMin+x*r.width,r.yMax),c,new Vector2(x,1));
                if(i==0)continue;var v=(i-1)*2;
                helper.AddTriangle(v,v+1,v+3);helper.AddTriangle(v+3,v+2,v);
            }
        }
    }
}
