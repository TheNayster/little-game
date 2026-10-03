using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Authored convex patches reference the original texture coordinates. Rear
    // and front pieces therefore stay registered without duplicate texture RAM.
    public sealed class HomeArtPart : RawImage
    {
        [Serializable] public sealed class Polygon { public Vector2[] points; }
        [Serializable] public sealed class Part { public string id; public Polygon[] polygons; }
        [Serializable] public sealed class Layout { public Part[] parts; }
        private Polygon[] polygons;
        public void Configure(Texture2D source, Polygon[] shapes)
        { texture=source;polygons=shapes;raycastTarget=false;SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();if(polygons==null)return;var rect=GetPixelAdjustedRect();
            foreach(var polygon in polygons)
            {
                var start=helper.currentVertCount;
                foreach(var p in polygon.points)
                    helper.AddVert(new Vector3(rect.xMin+p.x*rect.width,rect.yMax-p.y*rect.height),color,new Vector2(p.x,1-p.y));
                for(var i=1;i<polygon.points.Length-1;i++)helper.AddTriangle(start,start+i,start+i+1);
            }
        }
    }
}
