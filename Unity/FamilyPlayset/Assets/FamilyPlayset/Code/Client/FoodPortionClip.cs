using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Clip every layer against the same food origin. Radially filling each
    // small sauce/topping sprite would cut each patch around its own centre.
    public sealed class FoodPortionClip : BaseMeshEffect
    {
        private int portions=15;
        private Vector2 origin;
        public static void Apply(Image image,int mask,Vector2 foodOrigin)
        {
            var effect=image.GetComponent<FoodPortionClip>();
            if(effect==null){if(mask==15)return;effect=image.gameObject.AddComponent<FoodPortionClip>();}
            var r=image.rectTransform;var scale=r.localScale;
            var point=foodOrigin-r.anchoredPosition;point=new Vector2(point.x/scale.x,point.y/scale.y);
            if(effect.portions==mask && effect.origin==point)return;
            effect.portions=mask;effect.origin=point;image.SetVerticesDirty();
        }
        public override void ModifyMesh(VertexHelper mesh)
        {
            if(!IsActive() || portions==15)return;
            var triangles=new List<UIVertex>();mesh.GetUIVertexStream(triangles);mesh.Clear();
            for(var bit=0;bit<4;bit++)if((portions&(1<<bit))!=0){
                var right=bit<2;var up=bit==0 || bit==3;
                for(var i=0;i<triangles.Count;i+=3){
                    var polygon=new List<UIVertex>{triangles[i],triangles[i+1],triangles[i+2]};
                    polygon=Clip(polygon,true,origin.x,right);polygon=Clip(polygon,false,origin.y,up);
                    var start=mesh.currentVertCount;foreach(var vertex in polygon)mesh.AddVert(vertex);
                    for(var j=1;j+1<polygon.Count;j++)mesh.AddTriangle(start,start+j,start+j+1);
                }
            }
        }
        private static List<UIVertex> Clip(List<UIVertex> source,bool horizontal,float edge,bool positive)
        {
            var result=new List<UIVertex>();if(source.Count==0)return result;
            float Distance(UIVertex v)=>(horizontal?v.position.x:v.position.y)-edge;
            var previous=source[source.Count-1];var a=Distance(previous);var insideA=positive?a>=0:a<=0;
            foreach(var vertex in source){
                var b=Distance(vertex);var insideB=positive?b>=0:b<=0;
                if(insideA!=insideB){
                    var t=a/(a-b);var cross=previous;cross.position=Vector3.LerpUnclamped(previous.position,vertex.position,t);
                    cross.uv0=Vector4.LerpUnclamped(previous.uv0,vertex.uv0,t);cross.color=Color.LerpUnclamped(previous.color,vertex.color,t);result.Add(cross);
                }
                if(insideB)result.Add(vertex);previous=vertex;a=b;insideA=insideB;
            }
            return result;
        }
    }
}
