using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // A bounded uGUI mesh, with no particles, bloom, allocations or world state.
    public sealed class SecretSky : MaskableGraphic
    {
        public bool door;
        private float phase,burst;
        public void Present(float time,float twinkle)
        {if(Mathf.Abs(phase-time)<.002f && Mathf.Abs(burst-twinkle)<.002f)return;phase=time;burst=twinkle;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;
            if(!door)for(var ribbon=0;ribbon<2;ribbon++)for(var i=0;i<40;i++)
            {
                var x0=i/40f;var x1=(i+1)/40f;
                float Y(float x)=>r.yMin+r.height*(.45f+ribbon*.22f+.1f*Mathf.Sin(x*8+phase*.13f+ribbon*2));
                for(var band=0;band<4;band++)
                {
                    var c=ribbon==0?new Color(.31f,1,.72f,.12f):new Color(.76f,.47f,1,.12f);c.a*=1-band/4f;
                    var start=v.currentVertCount;var low=band*r.height*.065f;var high=(band+1)*r.height*.065f;
                    v.AddVert(new Vector3(r.xMin+x0*r.width,Y(x0)+low),c,Vector2.zero);v.AddVert(new Vector3(r.xMin+x1*r.width,Y(x1)+low),c,Vector2.zero);
                    c.a*=.6f;v.AddVert(new Vector3(r.xMin+x1*r.width,Y(x1)+high),c,Vector2.zero);v.AddVert(new Vector3(r.xMin+x0*r.width,Y(x0)+high),c,Vector2.zero);
                    v.AddTriangle(start,start+1,start+2);v.AddTriangle(start,start+2,start+3);
                }
            }
            for(var i=0;i<12;i++)
            {
                var x=door?.5f+Mathf.Sin(i*2.4f+phase*.35f)*.43f:.08f+((i*37)%89)/100f;
                var y=door?.1f+Mathf.Repeat(i*.0833f+phase*.024f,.85f):.15f+((i*29)%75)/100f;
                var size=(door?3:4)*(1+.28f*Mathf.Sin(phase*.8f+i)+burst*.65f);
                var c=new Color(1,.93f,.67f,.48f+.16f*Mathf.Sin(phase*.65f+i));var start=v.currentVertCount;
                var center=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
                v.AddVert(center+new Vector2(0,size*1.7f),c,Vector2.zero);v.AddVert(center+new Vector2(size,0),c,Vector2.zero);
                v.AddVert(center-new Vector2(0,size*1.7f),c,Vector2.zero);v.AddVert(center-new Vector2(size,0),c,Vector2.zero);
                v.AddTriangle(start,start+1,start+2);v.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
