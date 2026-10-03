using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Bend the existing vector-exported limb without changing its texture or
    // renderer. The lower paw stays rigid; only the limb above it deforms.
    [DisallowMultipleComponent]
    public sealed class CharacterLimbBend : BaseMeshEffect
    {
        private Vector2 endOffset;
        private float knee, length;
        private Vector3 sourceOffset;
        public Vector2 EndOffset=>endOffset;

        public void Configure(float jointToPaw)
        {length=jointToPaw;sourceOffset=transform.localPosition;}

        public void Pose(Vector2 end,float bend)
        {
            if((end-endOffset).sqrMagnitude<.00000001f && Mathf.Abs(bend-knee)<.00001f)return;
            endOffset=end;knee=bend;graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vertices)
        {
            if(!IsActive() || length<=0 || vertices.currentVertCount!=4)return;
            if(endOffset.sqrMagnitude<.00000001f && Mathf.Abs(knee)<.00001f)return;
            var a=UIVertex.simpleVert;var b=a;var c=a;var d=a;
            vertices.PopulateUIVertex(ref a,0);vertices.PopulateUIVertex(ref b,1);
            vertices.PopulateUIVertex(ref c,2);vertices.PopulateUIVertex(ref d,3);
            vertices.Clear();
            const int rows=20,columns=2;
            for(var y=0;y<=rows;y++)for(var x=0;x<=columns;x++)
            {
                var u=x/(float)columns;var v=y/(float)rows;
                var point=Vector3.Lerp(Vector3.Lerp(a.position,d.position,u),Vector3.Lerp(b.position,c.position,u),v);
                var t=Mathf.Clamp01(-(point.y+sourceOffset.y)/length);
                // A smooth knee arc gives the leg an actual curved silhouette.
                // Above the hip there is no deformation; below the ankle every
                // vertex receives the same offset, keeping toes level and intact.
                var bend=Mathf.Sin(Mathf.PI*t);
                point+=new Vector3(endOffset.x*t+knee*bend,endOffset.y*t,0);
                var vertex=a;vertex.position=point;
                vertex.uv0=Vector4.Lerp(Vector4.Lerp(a.uv0,d.uv0,u),Vector4.Lerp(b.uv0,c.uv0,u),v);
                vertices.AddVert(vertex);
            }
            for(var y=0;y<rows;y++)for(var x=0;x<columns;x++)
            {var i=y*(columns+1)+x;vertices.AddTriangle(i,i+columns+1,i+columns+2);vertices.AddTriangle(i,i+columns+2,i+1);}
        }
    }
}
