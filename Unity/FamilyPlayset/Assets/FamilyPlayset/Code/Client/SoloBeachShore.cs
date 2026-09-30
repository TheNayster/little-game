using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform shoreRoot,visitorRoot,splashRoot;private const float VisitorClipCenter=247;
        private BeachWaterGraphic shoreWater;private Image visitorPicture;
        private readonly RectTransform[] sandPrints=new RectTransform[4*BeachShore.PrintsPerPlayer];
        private readonly CanvasGroup[] sandAlpha=new CanvasGroup[4*BeachShore.PrintsPerPlayer];
        private readonly BeachRippleGraphic[] shoreRipples=new BeachRippleGraphic[4];
        private readonly Texture2D[] visitorTextures=new Texture2D[3];
        private readonly Sprite[,] visitorSprites=new Sprite[3,3];
        [Serializable] private sealed class VisitorFrame {public float x,y,width,height;}
        [Serializable] private sealed class VisitorSheet {public string name;public float width,height;public VisitorFrame[] frames;}
        [Serializable] private sealed class VisitorSheets {public VisitorSheet[] sheets;}
        private readonly Vector2[,] visitorSizes=new Vector2[3,3];
        private int visitorPose=-1;
        private double shoreDisplayClock,shoreSampleClock=-1;private string shoreWorld="";
        public BeachShoreState Shore=>HasWorld?(Shared?shared.View.shore:World.ReadShore()):null;
        public int VisibleSandPrints=>sandPrints.Count(v=>v!=null && v.gameObject.activeInHierarchy);
        public bool VisibleSeaVisitor=>visitorRoot!=null && visitorRoot.gameObject.activeInHierarchy;
        public int SeaVisitorPose=>VisibleSeaVisitor?visitorPose:-1;
        private void BuildShore()
        {
            if(SceneSchema<BeachShore.Schema)return;
            shoreRoot=Rect(Board,"Beach shore ambience",Vector2.zero,Vector2.zero);
            var waterRect=Rect(shoreRoot,"Moving shallow water and foam",Vector2.zero,new Vector2(4800,800));
            shoreWater=waterRect.gameObject.AddComponent<BeachWaterGraphic>();shoreWater.raycastTarget=false;
            for(var i=0;i<sandPrints.Length;i++){
                var root=Rect(Board,"Sand footprint "+i,Vector2.zero,Vector2.zero);sandPrints[i]=root;sandAlpha[i]=root.gameObject.AddComponent<CanvasGroup>();sandAlpha[i].blocksRaycasts=false;
                Panel(root,"Paw heel",new Vector2(0,-1),new Vector2(12,8),new Color(.64f,.43f,.23f,.6f),false,true);
                for(var toe=0;toe<3;toe++)Panel(root,"Paw toe "+toe,new Vector2((toe-1)*5,5),new Vector2(4,4),new Color(.64f,.43f,.23f,.56f),false,true);
            }
            for(var i=0;i<shoreRipples.Length;i++){
                var r=Rect(Board,"Shared beach ripple "+i,Vector2.zero,new Vector2(200,100));shoreRipples[i]=r.gameObject.AddComponent<BeachRippleGraphic>();shoreRipples[i].raycastTarget=false;
            }
            var names=new[]{"whale","dolphin","mermaid"};
            var frames=Resources.Load<TextAsset>("BeachArt/sea-visitor-frames-v2");if(frames==null)throw new InvalidOperationException("Missing sea visitor frame map.");
            var sheets=JsonUtility.FromJson<VisitorSheets>(frames.text);Resources.UnloadAsset(frames);
            if(sheets?.sheets==null || sheets.sheets.Length!=3)throw new InvalidOperationException("Invalid sea visitor frame map.");
            for(var i=0;i<names.Length;i++){
                var sheet=sheets.sheets[i];if(sheet.name!=names[i] || sheet.frames==null || sheet.frames.Length!=3 || sheet.width<=0 || sheet.height<=0)throw new InvalidOperationException("Invalid sea visitor poses.");
                var tex=Resources.Load<Texture2D>("BeachArt/"+names[i]+"-poses-v2");if(tex==null)throw new InvalidOperationException("Missing sea visitor "+names[i]);visitorTextures[i]=tex;
                var longest=sheet.frames.Max(f=>Mathf.Max(f.width,f.height));var pixelSize=(i==0?300:225)/longest;
                for(var pose=0;pose<3;pose++){
                    var b=sheet.frames[pose];if(b.width<=0 || b.height<=0 || b.x<0 || b.y<0 || b.x+b.width>sheet.width || b.y+b.height>sheet.height)throw new InvalidOperationException("Sea visitor frame outside atlas.");
                    visitorSprites[i,pose]=Sprite.Create(tex,new Rect(b.x*tex.width/sheet.width,(sheet.height-b.y-b.height)*tex.height/sheet.height,b.width*tex.width/sheet.width,b.height*tex.height/sheet.height),new Vector2(.5f,.5f));
                    visitorSizes[i,pose]=new Vector2(b.width,b.height)*pixelSize;
                }
            }
            var clip=Rect(shoreRoot,"Offshore water surface clip",new Vector2(0,VisitorClipCenter),new Vector2(10000,310));clip.gameObject.AddComponent<RectMask2D>();
            visitorRoot=Rect(clip,"Offshore sea visitor",Vector2.zero,Vector2.zero);visitorPicture=HomePicture(visitorRoot,"Sea visitor art",Vector2.zero,Vector2.one,visitorSprites[0,0]);visitorPicture.preserveAspect=true;
            splashRoot=Rect(shoreRoot,"Offshore landing splash",Vector2.zero,Vector2.zero);
            for(var i=0;i<9;i++){
                var drop=Panel(splashRoot,"Splash drop "+i,new Vector2((i-4)*13,Mathf.Sin(i/8f*Mathf.PI)*35),new Vector2(7,14),new Color(.87f,.98f,1,.9f),false,true);drop.rectTransform.localRotation=Quaternion.Euler(0,0,(4-i)*12);
            }
            // Broad pictured-water targets remain out of the walking strip.
            // Children tap the surf without covering their joystick/characters.
            for(var i=0;i<20;i++){
                var x=120+i*240;
                HomeHit(shoreRoot,"Splash water "+(i+1),Vector2.zero,new Vector2(240,100),()=>{
                    var p=ReadPlayer(Actor);var y=Mathf.Clamp(Mathf.Max(BeachShore.WetY,BeachShore.WaterY(Shore?.clock??0)),BeachShore.WetY,500);
                    if(Shared)SubmitShared(SoloAction.Beach,"","water","ripple",x,y,null);
                    else Command(SoloAction.Beach,target:"water",value:"ripple",x:x,y:y);
                },false);
            }
            TickShore();
        }
        private void TickShore()
        {
            var g=Shore;if(g==null || shoreRoot==null)return;var visible=CurrentArea=="beach";
            shoreRoot.gameObject.SetActive(visible);
            var world=Shared?shared.View.worldId:World.WorldId;
            if(shoreWorld!=world || g.clock<shoreSampleClock){shoreWorld=world;shoreDisplayClock=g.clock;}
            shoreSampleClock=g.clock;shoreDisplayClock=Math.Max(g.clock,Math.Min(g.clock+1.05,shoreDisplayClock+Math.Min(.1,Time.unscaledDeltaTime)));
            shoreWater.clock=shoreDisplayClock;shoreWater.cameraX=cameraX;shoreWater.scale=sceneScale;shoreWater.SetVerticesDirty();
            for(var i=0;i<sandPrints.Length;i++){
                var root=sandPrints[i];root.gameObject.SetActive(visible && i<g.prints.Length);if(i>=g.prints.Length)continue;
                var p=g.prints[i];root.anchoredPosition=ToBoard(p.x,p.y);root.localScale=new Vector3(sceneScale,sceneScale*.65f,1);root.localRotation=Quaternion.Euler(0,0,p.angle);
                // Stable left/right paw placement follows the shared heading.
                root.anchoredPosition+=new Vector2(p.left?-7:7,0)*sceneScale;sandAlpha[i].alpha=BeachShore.PrintAlpha(p,shoreDisplayClock);
            }
            for(var i=0;i<shoreRipples.Length;i++){
                var r=shoreRipples[i];r.gameObject.SetActive(visible && i<g.ripples.Length);if(i>=g.ripples.Length)continue;
                var p=g.ripples[i];r.rectTransform.anchoredPosition=ToBoard(p.x,p.y);r.rectTransform.localScale=Vector3.one*sceneScale;r.age=(float)(shoreDisplayClock-p.born);r.SetVerticesDirty();
            }
            var age=(float)(shoreDisplayClock-g.visitStarted);var visiting=visible && BeachShore.Visiting(g);visitorRoot.gameObject.SetActive(visiting && age<4);splashRoot.gameObject.SetActive(visiting);
            visitorPose=age<1?0:age<2.75f?1:2;
            if(visiting){
                var t=Mathf.Clamp01(age/4);var arc=Mathf.Sin(t*Mathf.PI);var big=g.visitor==SeaVisitor.Whale;var height=(big?240:230)*arc;
                var pos=ToBoard(g.visitX+(g.right?1:-1)*(t-.5f)*220,760);
                ((RectTransform)visitorRoot.parent).anchoredPosition=new Vector2(0,VisitorClipCenter*sceneScale);((RectTransform)visitorRoot.parent).sizeDelta=new Vector2(10000,310)*sceneScale;
                visitorRoot.anchoredPosition=pos+Vector2.up*((height-65-VisitorClipCenter)*sceneScale);visitorRoot.localScale=new Vector3(g.right?sceneScale:-sceneScale,sceneScale,1);
                // Each stage changes the drawing (limbs/tail/hair), not only
                // its rotation. One shared event clock picks all clients' pose.
                visitorRoot.localRotation=Quaternion.identity;visitorPicture.sprite=visitorSprites[(int)g.visitor,visitorPose];visitorPicture.rectTransform.sizeDelta=visitorSizes[(int)g.visitor,visitorPose];
                splashRoot.anchoredPosition=pos;var splash=age<.8f?Mathf.Sin(age/.8f*Mathf.PI):age>3.4f?Mathf.Sin(Mathf.Clamp01((age-3.4f)/1.4f)*Mathf.PI):0;
                splashRoot.localScale=new Vector3((big?1.6f:1)*sceneScale,Mathf.Max(.01f,splash)*sceneScale,1);splashRoot.gameObject.SetActive(splash>.02f);
            }
            for(var i=0;i<20;i++){var hit=(RectTransform)shoreRoot.Find("Splash water "+(i+1));hit.anchoredPosition=ToBoard(120+i*240,650);hit.localScale=Vector3.one*sceneScale;}
        }
        private void AddShoreDepth(Action<RectTransform,float,int,string> add)
        {
            // Ocean is behind every floor actor; prints are behind feet while
            // moving foam overlays only the shallow strip it actually reaches.
            add(shoreRoot,10000,0,"shore");
            for(var i=0;i<sandPrints.Length;i++)add(sandPrints[i],sandPrints[i]==null?0:sandPrints[i].anchoredPosition.y,0,"sand-"+i);
            foreach(var r in shoreRipples)if(r!=null)add(r.rectTransform,r.rectTransform.anchoredPosition.y,0,"ripple");
        }
        private void ResetShore()
        {
            foreach(var sprite in visitorSprites)if(sprite!=null)Destroy(sprite);
            foreach(var texture in visitorTextures)if(texture!=null)Resources.UnloadAsset(texture);
            Array.Clear(visitorSprites,0,visitorSprites.Length);Array.Clear(visitorTextures,0,3);Array.Clear(sandPrints,0,sandPrints.Length);Array.Clear(sandAlpha,0,sandAlpha.Length);Array.Clear(shoreRipples,0,4);visitorPose=-1;
            shoreRoot=null;shoreWater=null;visitorRoot=null;splashRoot=null;shoreWorld="";shoreSampleClock=-1;
        }
    }
}
