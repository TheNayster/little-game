using System;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // One bounded UI mesh. Coloring regions are authored closed polygons, so
    // filling never scans a texture or leaks through antialiased outlines.
    public sealed class DiscoverySurface:MaskableGraphic,IPointerDownHandler,IDragHandler,IPointerUpHandler,IPointerClickHandler
    {
        [Serializable] public sealed class ShapeData {public int region;public string label;public bool closed,solid;public Vector2[] points;[NonSerialized]public int[] triangles;}
        [Serializable] public sealed class PageData {public string name;public ShapeData[] shapes;}
        [Serializable] public sealed class Catalog {public int version;public PageData[] pages;}
        public static readonly Color[] Palette={Color.white,new Color(.91f,.34f,.34f),new Color(.98f,.64f,.3f),new Color(1,.87f,.4f),new Color(.45f,.71f,.5f),new Color(.37f,.66f,.87f),new Color(.68f,.52f,.81f),new Color(.67f,.47f,.34f),new Color(.15f,.24f,.26f)};
        public static readonly Color[] Lights={new Color(.07f,.13f,.17f),Color.red,Color.green,Color.yellow,Color.blue,Color.magenta,Color.cyan,Color.white};
        private static Catalog catalog;
        private static readonly Color Ink=new Color(.2f,.28f,.3f);
        public Action<int> FillRegion;
        public Action<Vector2> MoveMagnet;
        public Func<bool> CanInteract;
        public int Station,Page;
        private DiscoveryWorkspace workspace,preview;
        private int? pointer;
        private Vector2 down;
        public static Catalog Pages
        {
            get{
                if(catalog!=null)return catalog;
                catalog=JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("Discovery/pages").text);
                if(catalog.version!=1 || catalog.pages.Length!=6)throw new InvalidOperationException("Invalid coloring catalog.");
                foreach(var page in catalog.pages)foreach(var shape in page.shapes)if(shape.closed)shape.triangles=Triangulate(shape.points);
                return catalog;
            }
        }
        public void Present(DiscoveryWorkspace value,int station,int page)
        {if(ReferenceEquals(workspace,value) && Station==station && Page==page)return;workspace=value;Station=station;Page=page;if(pointer==null)preview=null;SetVerticesDirty();}
        public void CancelGesture(){pointer=null;preview=null;SetVerticesDirty();}
        protected override void OnDisable(){CancelGesture();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;return new Vector2((p.x-r.xMin)/r.width*800,(r.yMax-p.y)/r.height*460);}
        public void OnPointerDown(PointerEventData e)
        {
            if(pointer.HasValue || !(CanInteract?.Invoke()??true))return;
            down=Point(e);if(Station!=1 || workspace==null || Mathf.Abs(down.x-workspace.magnetX)>65 || Mathf.Abs(down.y-workspace.magnetY)>60)return;
            pointer=e.pointerId;preview=workspace.Copy();
        }
        public void OnDrag(PointerEventData e){if(pointer!=e.pointerId || preview==null)return;var p=Point(e);Discovery.Magnet(preview,p.x,p.y);SetVerticesDirty();}
        public void OnPointerUp(PointerEventData e)
        {if(pointer!=e.pointerId || preview==null)return;var p=new Vector2(preview.magnetX,preview.magnetY);pointer=null;preview=null;if(CanInteract?.Invoke()??true)MoveMagnet?.Invoke(p);SetVerticesDirty();}
        public void OnPointerClick(PointerEventData e)
        {
            if(Station!=3 || !(CanInteract?.Invoke()??true))return;var p=Point(e);
            var shapes=Pages.pages[Page].shapes;
            for(var i=shapes.Length-1;i>=0;i--)if(shapes[i].region>=0 && Inside(shapes[i].points,p)){FillRegion?.Invoke(shapes[i].region);return;}
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();if(workspace==null)return;var w=preview??workspace;
            if(Station==3){foreach(var shape in Pages.pages[Page].shapes){var c=shape.region<0?Ink:Palette[w.pages[Page].colors[shape.region]];if(shape.closed && shape.solid)Poly(h,shape.points,c,shape.triangles);if(shape.region>=0 || !shape.closed)Line(h,shape.points,Ink,5,shape.closed);}return;}
            if(Station==0){
                Box(h,70,85,660,315,new Color(.8f,.91f,.92f));Box(h,80,245,640,145,new Color(.51f,.79f,.9f));
                var y=w.outOfWater?70:Discovery.Sinks(w)?350:220+Discovery.Waterline(w)*50;var width=w.wide?285:185;
                Poly(h,new[]{new Vector2(400-width/2,y-15),new Vector2(400+width/2,y-15),new Vector2(380+width/2,y+35),new Vector2(420-width/2,y+35)},new Color(.81f,.58f,.26f));
                Box(h,393-width/2,y-25,width+14,12,new Color(1,.84f,.5f));
                for(var i=0;i<w.cargo;i++)Box(h,330+i%4*36,y-57-i/4*33,29,29,Palette[i%5+2]);
            }else if(Station==1){
                Ellipse(h,w.ironX,w.ironY,29,29,new Color(.5f,.58f,.61f));Ellipse(h,w.ironX,w.ironY,12,12,new Color(.91f,.96f,.94f));
                Box(h,282,287,56,56,new Color(.8f,.58f,.35f));Ellipse(h,480,315,29,29,Palette[1]);Ellipse(h,650,315,29,29,new Color(.7f,.8f,.86f));
                // A single closed outline avoids gaps between thick curve segments.
                var points=new List<Vector2>{new Vector2(w.magnetX-39,w.magnetY+30)};
                for(var i=0;i<=24;i++){var a=Mathf.PI+i*Mathf.PI/24;points.Add(new Vector2(w.magnetX+Mathf.Cos(a)*39,w.magnetY-12+Mathf.Sin(a)*39));}
                points.Add(new Vector2(w.magnetX+39,w.magnetY+30));points.Add(new Vector2(w.magnetX+15,w.magnetY+30));
                for(var i=24;i>=0;i--){var a=Mathf.PI+i*Mathf.PI/24;points.Add(new Vector2(w.magnetX+Mathf.Cos(a)*15,w.magnetY-12+Mathf.Sin(a)*15));}
                points.Add(new Vector2(w.magnetX-15,w.magnetY+30));Poly(h,points.ToArray(),Palette[1]);
                Box(h,w.magnetX-39,w.magnetY+15,24,20,Palette[5]);Box(h,w.magnetX+15,w.magnetY+15,24,20,Palette[5]);
            }else{
                Box(h,120,30,560,280,Ink);Box(h,138,48,524,244,Lights[w.lights]);
                for(var i=0;i<3;i++){var x=230+i*170;Poly(h,new[]{new Vector2(x,345),new Vector2(x-33,412),new Vector2(x+33,412)},(w.lights&(1<<i))!=0?Lights[1<<i]:new Color(.64f,.7f,.69f));Box(h,x-41,410,82,12,Ink);}
            }
        }
        private Vector3 ScreenPoint(Vector2 p){var r=rectTransform.rect;return new Vector3(r.xMin+p.x/800*r.width,r.yMax-p.y/460*r.height);}
        private void Poly(VertexHelper h,Vector2[] p,Color c,int[] indices=null)
        {var offset=h.currentVertCount;foreach(var v in p)h.AddVert(ScreenPoint(v),c,Vector2.zero);var t=indices??Triangulate(p);for(var i=0;i<t.Length;i+=3)h.AddTriangle(offset+t[i],offset+t[i+1],offset+t[i+2]);}
        private void Box(VertexHelper h,float x,float y,float width,float height,Color c)=>Poly(h,new[]{new Vector2(x,y),new Vector2(x+width,y),new Vector2(x+width,y+height),new Vector2(x,y+height)},c,new[]{0,1,2,0,2,3});
        private void Ellipse(VertexHelper h,float x,float y,float rx,float ry,Color c){var p=new Vector2[40];for(var i=0;i<40;i++){var a=i*Mathf.PI*2/40;p[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}Poly(h,p,c);}
        private void Line(VertexHelper h,Vector2[] p,Color c,float width,bool closed)
        {for(var i=0;i<(closed?p.Length:p.Length-1);i++){var a=p[i];var b=p[(i+1)%p.Length];var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;Poly(h,new[]{a+n,b+n,b-n,a-n},c,new[]{0,1,2,0,2,3});}}
        private static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
        private static bool Inside(Vector2[] polygon,Vector2 p){var hit=false;for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)if((polygon[i].y>p.y)!=(polygon[j].y>p.y) && p.x<(polygon[j].x-polygon[i].x)*(p.y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)hit=!hit;return hit;}
        private static int[] Triangulate(Vector2[] p)
        {
            var available=new List<int>();float area=0;for(var i=0;i<p.Length;i++){available.Add(i);var b=p[(i+1)%p.Length];area+=p[i].x*b.y-b.x*p[i].y;}
            if(area<0)available.Reverse();var result=new List<int>();var safety=p.Length*p.Length;
            while(available.Count>2 && safety-->0){var found=false;for(var i=0;i<available.Count;i++){
                var a=available[(i+available.Count-1)%available.Count];var b=available[i];var c=available[(i+1)%available.Count];
                var cross=Cross(p[a],p[b],p[c]);if(Mathf.Abs(cross)<.0001f){available.RemoveAt(i);found=true;break;}if(cross<0)continue;
                var blocked=false;foreach(var k in available)if(k!=a && k!=b && k!=c && Cross(p[a],p[b],p[k])>=-.0001f && Cross(p[b],p[c],p[k])>=-.0001f && Cross(p[c],p[a],p[k])>=-.0001f){blocked=true;break;}
                if(blocked)continue;result.Add(a);result.Add(b);result.Add(c);available.RemoveAt(i);found=true;break;
            }if(!found)throw new InvalidOperationException("Coloring polygon could not be triangulated.");}
            return result.ToArray();
        }
    }
}
