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
        public Action<string> ScienceOperation;
        private RawImage lightScreen;private Material lightMaterial;
        public Func<bool> CanInteract;
        public int Station,Page;
        private DiscoveryWorkspace workspace,preview;
        private readonly List<Image> equipment=new List<Image>();
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
        {if(ReferenceEquals(workspace,value) && Station==station && Page==page)return;workspace=value;Station=station;Page=page;if(pointer==null)preview=null;Equipment();SetVerticesDirty();}
        public void CancelGesture(){pointer=null;preview=null;Equipment();SetVerticesDirty();}
        protected override void OnDisable(){CancelGesture();base.OnDisable();}
        private Vector2 Point(PointerEventData e){RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;return new Vector2((p.x-r.xMin)/r.width*800,(r.yMax-p.y)/r.height*460);}
        public void OnPointerDown(PointerEventData e)
        {
            if(pointer.HasValue || !(CanInteract?.Invoke()??true))return;
            down=Point(e);if(Station!=1 || workspace==null || Mathf.Abs(down.x-workspace.magnetX)>65 || Mathf.Abs(down.y-workspace.magnetY)>60)return;
            pointer=e.pointerId;preview=workspace.Copy();
        }
        public void OnDrag(PointerEventData e){if(pointer!=e.pointerId || preview==null)return;var p=Point(e);Discovery.Magnet(preview,p.x,p.y);Equipment();SetVerticesDirty();}
        public void OnPointerUp(PointerEventData e)
        {if(pointer!=e.pointerId || preview==null)return;var p=new Vector2(preview.magnetX,preview.magnetY);pointer=null;preview=null;if(CanInteract?.Invoke()??true)MoveMagnet?.Invoke(p);Equipment();SetVerticesDirty();}
        public void OnPointerClick(PointerEventData e)
        {
            if(!(CanInteract?.Invoke()??true))return;var p=Point(e);
            if(Station!=3){
                if(Station==0 && p.x>200 && p.x<600)ScienceOperation?.Invoke("cargo-add");
                else if(Station==1 && p.y>260){var index=Mathf.Clamp((int)((p.x-55)/175),0,3);ScienceOperation?.Invoke(new[]{"iron","wood","plastic","aluminum"}[index]);}
                else if(Station==2 && p.y>305 && p.x>145 && p.x<665)ScienceOperation?.Invoke(new[]{"red","green","blue"}[Mathf.Clamp((int)((p.x-145)/170),0,2)]);
                return;
            }
            var shapes=Pages.pages[Page].shapes;
            for(var i=shapes.Length-1;i>=0;i--)if(shapes[i].region>=0 && Inside(shapes[i].points,p)){FillRegion?.Invoke(shapes[i].region);return;}
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();if(workspace==null)return;var w=preview??workspace;
            if(Station==3){foreach(var shape in Pages.pages[Page].shapes){var c=shape.region<0?Ink:Palette[w.pages[Page].colors[shape.region]];if(shape.closed && shape.solid)Poly(h,shape.points,c,shape.triangles);if(shape.region>=0 || !shape.closed)Line(h,shape.points,Ink,5,shape.closed);}return;}
            if(Station==0){
                Ellipse(h,400,393,310,26,new Color(.18f,.35f,.4f,.19f));
                Poly(h,new[]{new Vector2(207,235),new Vector2(593,235),new Vector2(586,280),new Vector2(560,328),new Vector2(530,351),new Vector2(500,360),new Vector2(300,360),new Vector2(270,351),new Vector2(240,328),new Vector2(214,280)},new Color(.21f,.64f,.79f,.68f));
                Ellipse(h,400,235,189,30,new Color(.52f,.88f,.94f,.87f));
                for(var i=0;i<5;i++)Line(h,new[]{new Vector2(245+i*65,235),new Vector2(265+i*65,239),new Vector2(284+i*65,235)},new Color(.86f,.99f,1,.65f),3,false);
            }
        }

        private void Equipment()
        {
            if(workspace==null)return;
            while(equipment.Count<12){var go=new GameObject("Illustrated science prop",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(transform,false);var img=go.GetComponent<Image>();img.preserveAspect=true;img.raycastTarget=false;equipment.Add(img);}
            foreach(var img in equipment)img.gameObject.SetActive(false);
            if(lightScreen==null){var go=new GameObject("Additive light canvas",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));go.transform.SetParent(transform,false);lightScreen=go.GetComponent<RawImage>();lightScreen.raycastTarget=false;lightMaterial=new Material(Resources.Load<Shader>("Discovery/LightMix"));lightScreen.material=lightMaterial;var r=lightScreen.rectTransform;r.anchorMin=new Vector2(.095f,.31f);r.anchorMax=new Vector2(.905f,.98f);r.offsetMin=r.offsetMax=Vector2.zero;go.transform.SetAsFirstSibling();}
            lightScreen.gameObject.SetActive(Station==2);
            var w=preview??workspace;lightMaterial.SetFloat("_Lights",w.lights);var slot=0;
            void Show(Sprite sprite,float x,float y,float width,float height,Color tint){var img=equipment[slot++];img.gameObject.SetActive(true);img.sprite=sprite;img.color=tint;var r=img.rectTransform;r.anchorMin=new Vector2((x-width/2)/800,1-(y+height/2)/460);r.anchorMax=new Vector2((x+width/2)/800,1-(y-height/2)/460);r.offsetMin=r.offsetMax=Vector2.zero;}
            void Prop(int i,float x,float y,float width,float height)=>Show(WorkshopArt.Prop(i),x,y,width,height,Color.white);
            if(Station==0){
                var y=w.outOfWater?95:Discovery.Sinks(w)?326:175+Discovery.Waterline(w)*45;
                Prop(w.wide?1:0,400,y,w.wide?330:240,160);
                for(var i=0;i<w.cargo;i++)Prop(7,355+i%4*29,y-34-i/4*27,40,36);
                Show(WorkshopArt.Bowl(1),400,253,850,470,new Color(1,1,1,.65f));
            }else if(Station==1){
                Prop(3,w.ironX,w.ironY,82,82);Prop(4,310,315,90,90);Prop(5,480,315,82,82);Prop(6,650,315,90,90);Prop(2,w.magnetX,w.magnetY,130,130);
            }else if(Station==2){for(var i=0;i<3;i++)Prop(8+i,230+i*170,375,105,115);}
        }

        protected override void OnDestroy(){if(lightMaterial!=null)Destroy(lightMaterial);base.OnDestroy();}
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
