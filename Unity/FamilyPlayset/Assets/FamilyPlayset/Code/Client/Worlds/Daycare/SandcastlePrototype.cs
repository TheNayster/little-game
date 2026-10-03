using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Isolated scene: production rules/session in memory, no checkpoint store or network bootstrap.
    public sealed class SandcastlePrototype : MonoBehaviour
    {
        public GameWorld World {get;private set;}
        public SandpitState State=>World.ReadSandpit();
        public RectTransform Surface {get;private set;}
        public Canvas Canvas {get;private set;}
        public bool Fixture {get;private set;}
        public string Selected {get;private set;}="";
        public string LastOutcome {get;private set;}="";
        public Vector2 PreviewLogical {get;private set;}
        public bool Placing {get;private set;}
        public bool HasPreview {get;private set;}
        private FamilySession session;
        private RectTransform root,pieces,toolTray,decorTray,confirmTray;
        private Text hint,title;
        private string mould="round";
        private int orientation;
        private SandShape preview,effect;
        private SandcastlePrototypePiece previewPicture;
        private float effectStarted=-10;
        private string effectOp="",effectId="";
        private readonly Dictionary<string,SandShape> views=new Dictionary<string,SandShape>();
        private readonly List<GameCharacterVisual> cast=new List<GameCharacterVisual>();
        private RawImage dinosaur;
        private SandShape toyShadow,selection;
        private Sprite panelSprite;
        private readonly Dictionary<string,SandcastlePrototypePiece> pictures=new Dictionary<string,SandcastlePrototypePiece>();
        private readonly Dictionary<string,List<SandShape>> props=new Dictionary<string,List<SandShape>>();
        private Button confirm,water,scoop,tip;
        private Font font;
        private void Awake()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);for(var x=0;x<64;x++)for(var y=0;y<64;y++){var dx=Mathf.Max(16-x,0,x-47);var dy=Mathf.Max(16-y,0,y-47);tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Mathf.Sqrt(dx*dx+dy*dy))));}tex.Apply();panelSprite=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(17,17,17,17));
            var cv=new GameObject("Prototype canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            Canvas=cv.GetComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=cv.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1400,1000);scaler.matchWidthOrHeight=.5f;
            root=Rect(cv.transform,"Sandcastle visual prototype",Vector2.zero,new Vector2(1400,1000));
            if(EventSystem.current==null)new GameObject("Prototype touch input",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var bg=Rect(cv.transform,"Existing daycare scenery",Vector2.zero,new Vector2(1600,1050)).gameObject.AddComponent<RawImage>();
            bg.transform.SetAsFirstSibling();bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;bg.rectTransform.sizeDelta=Vector2.zero;
            bg.texture=WorldResources.Load<Texture2D>("Worlds/Daycare/Scenery/daycare-garden");bg.uvRect=new Rect(.1f,0,.7f,1);bg.raycastTarget=false;
            // Characters are drawn before the alpha pit sprite: wood occludes lower bodies, never faces.
            var names=new[]{"blue-pup","orange-pup","muffin","socks"};
            for(var i=0;i<4;i++){
                var r=Rect(root,"Rim friend "+names[i],new Vector2(-450+i*300,280),new Vector2(220,300));
                r.localScale=Vector3.one*(i==3?1.8f:1.6f);var v=r.gameObject.AddComponent<GameCharacterVisual>();v.Select(names[i]);cast.Add(v);
                foreach(var g in r.GetComponentsInChildren<Graphic>())g.raycastTarget=false;
            }
            var pit=Rect(root,"Independent wood and sand asset",new Vector2(0,-50),new Vector2(1400,933)).gameObject.AddComponent<RawImage>();
            pit.texture=WorldResources.Load<Texture2D>("Worlds/Daycare/SandcastleClub/Prototype/empty-pit");pit.raycastTarget=false;
            Surface=Rect(root,"Projected sand surface",Vector2.zero,new Vector2(1300,570));
            var hit=Surface.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
            Surface.gameObject.AddComponent<SandcastlePrototypeSurface>().Owner=this;
            pieces=Rect(root,"Depth sorted pieces",Vector2.zero,Vector2.zero);
            preview=Shape(pieces,"Placement preview");preview.enabled=false;previewPicture=Rect(preview.transform,"Projected selected shape",Vector2.zero,new Vector2(160,300)).gameObject.AddComponent<SandcastlePrototypePiece>();previewPicture.raycastTarget=false;preview.gameObject.SetActive(false);
            effect=Shape(root,"Accepted tool motion");effect.gameObject.SetActive(false);
            toyShadow=Shape(pieces,"Dinosaur contact shadow");toyShadow.kind="toy-shadow";
            selection=Shape(root,"Selected piece pointer");selection.kind="selection";
            var toy=Rect(pieces,"Existing small dinosaur",Vector2.zero,new Vector2(120,120));dinosaur=toy.gameObject.AddComponent<RawImage>();
            dinosaur.texture=WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/tyrannosaurus");dinosaur.uvRect=new Rect(0,0,.25f,.5f);dinosaur.raycastTarget=false;
            Panel(root,"Prototype label backing",new Vector2(0,456),new Vector2(690,44),new Color(1,.98f,.89f,.95f));
            title=Text(root,"Prototype - labelled example castle",new Vector2(0,456),new Vector2(700,44),24);
            Button(root,"Empty pit",new Vector2(-585,452),new Vector2(130,80),()=>Reset(false));
            Button(root,"Example castle",new Vector2(575,452),new Vector2(180,80),()=>Reset(true));
            Button(root,"Leave",new Vector2(-610,355),new Vector2(100,80),()=>{Send(SoloAction.Sandpit,"leave");hint.text="Left the isolated preview - choose Empty pit or Example castle";});
            Button(root,"Help",new Vector2(610,355),new Vector2(100,80),()=>hint.text="Choose a shape - tap sand - - - Scoop, Water, Tip - decorate");
            var tray=Panel(root,"Illustrated mould tray",new Vector2(-90,-407),new Vector2(840,136),new Color(1,.96f,.84f));
            var shapes=new[]{"round","square","wall","gate"};var labels=new[]{"Round","Square","Wall","Gate"};
            for(var i=0;i<4;i++){
                var s=shapes[i];var b=Button(tray,labels[i],new Vector2(-310+i*207,0),new Vector2(185,113),()=>Choose(s));
                b.GetComponent<Image>().color=new[]{new Color(.7f,.89f,1),new Color(.78f,.94f,.7f),new Color(1,.88f,.6f),new Color(.87f,.75f,1)}[i];
                var pic=Rect(b.transform,"Mould picture",new Vector2(0,-20),new Vector2(160,240)).gameObject.AddComponent<SandcastlePrototypePiece>();pic.raycastTarget=false;pic.shape=s;pic.rectTransform.localScale=Vector3.one*(DaycareSandpit.LongShape(s)?.48f:.52f);
                b.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(0,-41);
            }
            toolTray=Panel(root,"Contextual tools",new Vector2(500,-310),new Vector2(365,112),new Color(.94f,.98f,1));
            scoop=Tool(toolTray,"Scoop","scoop",-116,()=>ToolAction("scoop"));water=Tool(toolTray,"Water","water",0,()=>ToolAction("water"));tip=Tool(toolTray,"Tip","tip",116,()=>ToolAction("tip"));
            decorTray=Panel(root,"Decorations",new Vector2(465,-403),new Vector2(235,136),new Color(1,.96f,.84f));
            Tool(decorTray,"Flag","flag",-58,()=>Decorate("flag"));Tool(decorTray,"Shell","shell",58,()=>Decorate("shell"));
            confirmTray=Panel(root,"Confirm preview",new Vector2(500,-310),new Vector2(365,104),new Color(1,.97f,.82f));
            confirm=Tool(confirmTray,"Confirm","confirm",-113,Confirm);Tool(confirmTray,"Rotate","rotate",0,()=>{orientation=orientation==0?90:0;if(HasPreview)SetPreview(PreviewLogical);});
            Tool(confirmTray,"Cancel","cancel",113,()=>{Placing=HasPreview=false;Refresh();});
            Panel(root,"Hint backing",new Vector2(-330,-322),new Vector2(720,46),new Color(1,.98f,.88f,.95f));
            hint=Text(root,"Choose a mould and tap the sand",new Vector2(-330,-322),new Vector2(720,50),25);
            Reset(true);gameObject.AddComponent<SandcastlePrototypeCapture>();
        }
        public SoloResult Send(SoloAction action,string op="",string target="",float x=0,float y=0,string item="",int actor=1)
        {
            var p=World.ReadPlayer("p"+actor);var r=session.Submit((ulong)actor,new SoloCommand{actor=p.id,zone=p.zone,visit=p.visit,expectedRevision=World.Revision,requestId=Guid.NewGuid().ToString("N"),action=action,value=op,target=target,x=x,y=y,item=item});
            LastOutcome=r.Outcome;return r;
        }
        private SoloResult PieceAction(string op,SandMould m,string item="")
        {
            var at=DaycareSandpit.Work(m);Send(SoloAction.Move,x:at.X,y:at.Y);
            return Send(SoloAction.Sandpit,op,DaycareSandpit.Target(m.id,State.round),item:item);
        }
        public void Reset(bool fixture)
        {
            World=GameWorld.WithDinosaurWorld(GameWorld.Create("p1","p2","p3","p4"));session=new FamilySession(World);
            for(var i=1;i<=4;i++){session.Attach((ulong)i,"p"+i,out _);Send(SoloAction.Travel,"daycare",actor:i);}Send(SoloAction.Sandpit,"start");
            Fixture=fixture;Selected="";Placing=HasPreview=false;effectStarted=-10;
            if(fixture){
                var specs=new[]{("square",1,0,0),("wall",2,0,0),("gate",4,0,0),("round",6,0,0),("round",1,3,0),("wall",2,3,0),("wall",4,3,0),("square",6,3,0),("wall",1,1,90),("wall",6,1,90),("round",3,2,0),("square",4,2,0)};
                foreach(var (shape,col,row,rot) in specs){var p=DaycareSandpit.PiecePoint(col,row,shape,rot);var r=Send(SoloAction.Sandpit,"place",DaycareSandpit.Target("place",State.round),p.X,p.Y,DaycareSandpit.Choice(shape,rot));if(!r.Accepted)throw new InvalidOperationException("Fixture place "+r.Outcome);
                    var m=State.moulds.Last();PieceAction("water",m);for(var k=0;k<3;k++)PieceAction("scoop",m);PieceAction("tip",m);
                    PieceAction("decorate",m,"0:flag");PieceAction("decorate",m,"2:shell");if(!DaycareSandpit.LongShape(shape))PieceAction("decorate",m,"5:window");
                }
                Send(SoloAction.Sandpit,"toy-place",DaycareSandpit.Target("toy",State.round),4150,180,"0");
            }
            title.text=fixture?"PROTOTYPE - authored example castle":"PROTOTYPE - empty pit - build with taps";
            hint.text="Choose a mould and tap the sand";Refresh();
        }
        public void Choose(string shape){mould=shape;orientation=0;Placing=true;HasPreview=false;hint.text="Tap the sand to choose a spot";Refresh();}
        public void Tap(Vector2 point)
        {
            if(Placing){SetPreview(SandcastleProjection.Inverse(point));return;}
            // Painted faces are selectable in the same front-to-back order as rendering.
            foreach(var m in State.moulds.OrderBy(m=>m.y)){
                var p=SandcastleProjection.Project(m.x,m.y);var s=SandcastleProjection.DepthScale(m.y);
                var longPiece=DaycareSandpit.LongShape(m.shape);var width=(longPiece && m.orientation==0?250:128)*s;var height=(longPiece && m.orientation==90?240:190)*s;
                if(point.x>p.x-width/2 && point.x<p.x+width/2 && point.y>p.y-(m.orientation==90?95*s:12) && point.y<p.y+height){Selected=m.id;hint.text=m.built?"Add a flag and shell - or keep building":"Scoop, Water, then Tip";Refresh();return;}
            }
        }
        private void SetPreview(Vector2 world)
        {
            var p=DaycareSandpit.Snap(world.x,world.y,mould,orientation);PreviewLogical=new Vector2(p.X,p.Y);HasPreview=true;
            var reason=DaycareSandpit.Placement(State,p.X,p.Y,mould,orientation);hint.text=reason==null?"- Place here":reason=="outside-sandpit"?"Choose inside the sand":"Choose a free spot";Refresh();
        }
        private void Confirm()
        {
            if(!HasPreview)return;
            var r=Send(SoloAction.Sandpit,"place",DaycareSandpit.Target("place",State.round),PreviewLogical.x,PreviewLogical.y,DaycareSandpit.Choice(mould,orientation));
            if(r.Accepted){Selected=State.moulds.Last().id;Placing=HasPreview=false;hint.text="Scoop, Water, then Tip";}Refresh();
        }
        private void ToolAction(string op)
        {
            var m=State.moulds.FirstOrDefault(v=>v.id==Selected);if(m==null)return;var r=PieceAction(op,m);
            hint.text=r.Accepted?(op=="tip"?"Your piece is ready - decorate or choose another shape":op=="water"?"Wet sand - fill the bucket, then Tip":"Scoop "+State.moulds.First(v=>v.id==m.id).scoops+" / "+m.capacity):r.Outcome=="fill-bucket-first"?"Keep your sand - add the remaining scoops":r.Outcome=="add-water-first"?"Keep your full bucket - add water, then Tip":r.Outcome;
            if(r.Accepted){effectStarted=Time.unscaledTime;effectOp=op;effectId=m.id;}Refresh();
        }
        private void Decorate(string kind)
        {
            var m=State.moulds.FirstOrDefault(v=>v.id==Selected);if(m==null || !m.built)return;var slot=kind=="flag"?0:2;
            var r=PieceAction("decorate",m,slot+":"+kind);hint.text=r.Accepted?"Flag and shell can stay together - keep playing":r.Outcome;Refresh();
        }
        private Vector3 PieceScale(SandMould m)
        {
            var s=SandcastleProjection.DepthScale(m.y);return new Vector3(s*(DaycareSandpit.LongShape(m.shape)?m.orientation==0?1.12f:1.1f:1),s*(m.orientation==90?.77f:1.4f),1);
        }
        private void Refresh()
        {
            foreach(var id in views.Keys.Where(id=>!State.moulds.Any(m=>m.id==id)).ToArray()){Destroy(views[id].gameObject);views.Remove(id);pictures.Remove(id);props.Remove(id);}
            foreach(var m in State.moulds.OrderByDescending(m=>m.y)){
                if(!views.TryGetValue(m.id,out var v)){v=Shape(pieces,"Piece "+m.id);views.Add(m.id,v);var created=Rect(v.transform,"Separate castle artwork",Vector2.zero,new Vector2(160,300)).gameObject.AddComponent<SandcastlePrototypePiece>();created.raycastTarget=false;pictures.Add(m.id,created);props.Add(m.id,new List<SandShape>());}
                v.rectTransform.anchoredPosition=SandcastleProjection.Project(m.x,m.y);v.rectTransform.localScale=PieceScale(m);v.transform.SetAsLastSibling();
                v.mould=m.shape;v.orientation=m.orientation;v.capacity=m.capacity;v.scoops=m.scoops;v.wet=m.wet;v.built=m.built;v.attachments=null;v.color=Color.white;v.enabled=!m.built;v.SetVerticesDirty();var pic=pictures[m.id];pic.enabled=m.built;pic.shape=m.shape;pic.orientation=m.orientation;pic.SetVerticesDirty();
                var ornaments=props[m.id];while(ornaments.Count<m.attachments.Length)ornaments.Add(Shape(v.transform,"Separate decoration"));for(var k=0;k<ornaments.Count;k++){var a=ornaments[k];a.gameObject.SetActive(k<m.attachments.Length);if(k>=m.attachments.Length)continue;var attachment=m.attachments[k];var at=SandpitPlay.Socket(m,attachment.slot);a.kind="attachment";a.decorationKind=attachment.kind;var heightOffset=attachment.slot<2?(DaycareSandpit.LongShape(m.shape)?m.shape=="gate"?65:10:33):attachment.slot>=4?20:0;a.rectTransform.anchoredPosition=new Vector2(at.X,at.Y+heightOffset);a.SetVerticesDirty();}
            }
            dinosaur.gameObject.SetActive(State.toy.placed);toyShadow.gameObject.SetActive(State.toy.placed);
            if(State.toy.placed){toyShadow.rectTransform.anchoredPosition=SandcastleProjection.Project(State.toy.x,State.toy.y);toyShadow.transform.SetAsLastSibling();dinosaur.transform.SetAsLastSibling();dinosaur.rectTransform.anchoredPosition=SandcastleProjection.Project(State.toy.x,State.toy.y)+new Vector2(0,120*(DinosaurLandmarks.Get("tyrannosaurus",4).z-.5f));foreach(var m in State.moulds.OrderByDescending(v=>v.y))if(m.y<State.toy.y && views.TryGetValue(m.id,out var foreground))foreground.transform.SetAsLastSibling();}
            preview.gameObject.SetActive(Placing && HasPreview);
            if(HasPreview){preview.rectTransform.anchoredPosition=SandcastleProjection.Project(PreviewLogical.x,PreviewLogical.y);var m=new SandMould{shape=mould,orientation=orientation,y=PreviewLogical.y};preview.rectTransform.localScale=PieceScale(m);preview.mould=mould;preview.orientation=orientation;preview.kind="mould-icon";preview.color=DaycareSandpit.Placement(State,PreviewLogical.x,PreviewLogical.y,mould,orientation)==null?new Color(.6f,.93f,1,.68f):new Color(1,.4f,.35f,.65f);previewPicture.shape=mould;previewPicture.orientation=orientation;previewPicture.color=preview.color;previewPicture.SetVerticesDirty();preview.transform.SetAsLastSibling();}
            confirmTray.gameObject.SetActive(Placing);confirm.interactable=HasPreview && DaycareSandpit.Placement(State,PreviewLogical.x,PreviewLogical.y,mould,orientation)==null;
            var selected=State.moulds.FirstOrDefault(v=>v.id==Selected);selection.gameObject.SetActive(selected!=null && !Placing);if(selected!=null)selection.rectTransform.anchoredPosition=SandcastleProjection.Project(selected.x,selected.y)+new Vector2(0,225*SandcastleProjection.DepthScale(selected.y));toolTray.gameObject.SetActive(!Placing && selected!=null && !selected.built);
            scoop.interactable=selected!=null && selected.scoops<selected.capacity;water.interactable=selected!=null && !selected.wet;tip.interactable=selected!=null;
            decorTray.gameObject.SetActive(!Placing && selected?.built==true);
        }
        private void Update()
        {
            var scale=Mathf.Min(Screen.width/1420f,Screen.height/1010f)/Canvas.scaleFactor;root.localScale=Vector3.one*scale;
            foreach(var v in cast)v.PresentFrame(new CharacterFrame(CharacterPose.Carry,0,false),Time.unscaledDeltaTime);
            foreach(var v in views.Values){v.presentationTime=Time.unscaledTime;v.SetVerticesDirty();}
            var m=State.moulds.FirstOrDefault(v=>v.id==effectId);var age=Time.unscaledTime-effectStarted;var duration=effectOp=="tip"?1.9f:effectOp=="water"?1.25f:.75f;
            effect.gameObject.SetActive(m!=null && age<duration);
            if(m!=null && age<duration){effect.rectTransform.anchoredPosition=SandcastleProjection.Project(m.x,m.y);effect.rectTransform.localScale=PieceScale(m);effect.kind=effectOp=="tip"?"mould-tip":effectOp=="water"?"water-pour":"sand-transfer";effect.progress=age/duration;effect.capacity=m.capacity;effect.mould=m.shape;effect.orientation=m.orientation;effect.tipOutcome="reveal";effect.source=new Vector2(-100,25);effect.target=new Vector2(0,60);effect.SetVerticesDirty();
                if(effectOp=="tip" && views.TryGetValue(m.id,out var v)){var pic=pictures[m.id];pic.reveal=Mathf.Clamp01((age/duration-.3f)/.7f);pic.SetVerticesDirty();}
            }else foreach(var v in pictures.Values)if(v.reveal<1){v.reveal=1;v.SetVerticesDirty();}
        }
        private RectTransform Rect(Transform parent,string name,Vector2 at,Vector2 size){var g=new GameObject(name,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=at;r.sizeDelta=size;return r;}
        private RectTransform Panel(Transform p,string n,Vector2 at,Vector2 size,Color c){var r=Rect(p,n,at,size);var im=r.gameObject.AddComponent<Image>();im.sprite=panelSprite;im.type=Image.Type.Sliced;im.color=c;im.raycastTarget=false;var o=r.gameObject.AddComponent<Outline>();o.effectColor=new Color(.42f,.27f,.12f,.5f);o.effectDistance=new Vector2(0,-4);return r;}
        private Text Text(Transform p,string value,Vector2 at,Vector2 size,int fs){var r=Rect(p,value,at,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=fs;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.12f,.23f,.24f);t.raycastTarget=false;return t;}
        private Button Button(Transform p,string name,Vector2 at,Vector2 size,Action a){var r=Panel(p,name,at,size,new Color(1,.97f,.87f));r.GetComponent<Image>().raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(()=>a());Text(r,name,Vector2.zero,size,22);return b;}
        private SandShape Shape(Transform p,string n){var r=Rect(p,n,Vector2.zero,new Vector2(160,240));var v=r.gameObject.AddComponent<SandShape>();v.raycastTarget=false;return v;}
        private Button Tool(Transform p,string n,string kind,float x,Action a){var b=Button(p,n,new Vector2(x,0),new Vector2(108,96),a);b.GetComponent<Image>().color=kind=="scoop"?new Color(1,.9f,.58f):kind=="water"?new Color(.64f,.9f,1):kind=="tip"?new Color(.86f,.73f,1):new Color(.94f,.98f,.85f);var s=Shape(b.transform,"Picture "+kind);s.kind=kind;s.rectTransform.anchoredPosition=new Vector2(0,12);b.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(0,-33);return b;}
        private void OnDestroy(){if(Canvas!=null)Destroy(Canvas.gameObject);SandArt.Release();if(panelSprite!=null){Destroy(panelSprite.texture);Destroy(panelSprite);}}
    }
    public sealed class SandcastlePrototypeSurface : MonoBehaviour,IPointerClickHandler
    {
        public SandcastlePrototype Owner;
        public void OnPointerClick(PointerEventData e){if(RectTransformUtility.ScreenPointToLocalPointInRectangle(Owner.Surface,e.position,e.pressEventCamera,out var p))Owner.Tap(p);}
    }
}
