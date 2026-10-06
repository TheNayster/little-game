using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Small editable vector expression, registered to each original idle crop.
    // No replacement character sheet, moving face decal or world indicator.
    public sealed class RevealExpression : MaskableGraphic
    {
        [Serializable] public sealed class Face { public string id; public float x,y,rx,ry,r,g,b; }
        [Serializable] private sealed class Catalog { public Face[] faces; }
        private static Catalog catalog;
        private Face face;
        public void Configure(string id, float pixelsToUnits)
        {
            if(catalog==null)catalog=JsonUtility.FromJson<Catalog>(WorldResources.Load<TextAsset>("Shared/Characters/reveal-expressions").text);
            face=Array.Find(catalog.faces,f=>f.id==id);
            rectTransform.localScale=Vector3.one*pixelsToUnits;
            if(face!=null)rectTransform.anchoredPosition=new Vector2(face.x,-face.y)*pixelsToUnits;
            raycastTarget=false;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(face==null)return;
            Ellipse(mesh,face.rx,face.ry,new Color(face.r,face.g,face.b));
            // Wide eyes in the original idle art plus a small open "oh!" mouth.
            Ellipse(mesh,face.ry*.65f,face.ry*.9f,new Color(.12f,.10f,.14f));
            Ellipse(mesh,face.ry*.3f,face.ry*.25f,new Color(.86f,.43f,.43f),-face.ry*.45f);
        }
        private static void Ellipse(VertexHelper mesh,float rx,float ry,Color color,float y=0)
        {
            var start=mesh.currentVertCount;mesh.AddVert(new Vector3(0,y),color,Vector2.zero);
            for(var i=0;i<=40;i++){var angle=i*Mathf.PI*2/40;mesh.AddVert(new Vector3(Mathf.Cos(angle)*rx,y+Mathf.Sin(angle)*ry),color,Vector2.zero);if(i>0)mesh.AddTriangle(start,start+i,start+i+1);}
        }
    }
}
