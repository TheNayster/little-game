using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform marketsPlacesDrawer,marketsFamilyDrawer,marketsMakeoverPanel,marketsSongsPanel,marketsPalette;
        private RectTransform marketsPrevious,marketsNext,marketsMap,marketsFamilyToggle;
        private RawImage marketsDragPicture;
        private readonly List<RectTransform> marketsLayoutSigns=new List<RectTransform>();
        private readonly Dictionary<string,RectTransform> marketsSceneObjects=new Dictionary<string,RectTransform>();
        private Action marketsQueuedAction;
        private string marketsQueuedStation="";
        private float marketsMakeoverTurn;
        private bool MarketsModalOpen=>CurrentArea==MarketsRules.Zone&&
            (marketsPlacesDrawer&&marketsPlacesDrawer.gameObject.activeInHierarchy||marketsFamilyDrawer&&marketsFamilyDrawer.gameObject.activeInHierarchy||marketsMakeoverPanel&&marketsMakeoverPanel.gameObject.activeInHierarchy||marketsSongsPanel&&marketsSongsPanel.gameObject.activeInHierarchy);

        // Layout evidence: source-findings/actual-layout.json and the task's
        // gameplay observations in pF6xmKOZvLo (1:54 stalls,4:47 care,7:50
        // building,8:44 guitar). This is a readable two-dimensional family
        // adaptation, not an assertion of pixel-identical original controls.
        // Source layout: edge arrows, world-space objects, a foreground guitar
        // and a scrolling brick rail. The pictured place/family drawers retain
        // accessible shortcuts for this shared four-player adaptation.
        private void BuildMarketsLayout()
        {
            marketsTabs=Rect(safe,"Markets edge navigation",Vector2.zero,new Vector2(1120,720));
            marketsPrevious=MarketsSmallButton(marketsTabs,"Markets previous scene","‹",new Vector2(-505,-276),new Vector2(80,70),()=>MarketsStepScene(-1));
            marketsNext=MarketsSmallButton(marketsTabs,"Markets next scene","›",new Vector2(505,-276),new Vector2(80,70),()=>MarketsStepScene(1));
            marketsMap=MarketsSmallButton(marketsTabs,"Markets open places","Places",new Vector2(0,-287),new Vector2(100,42),()=>MarketsToggleDrawer(marketsPlacesDrawer));
            marketsFamilyToggle=MarketsSmallButton(marketsTabs,"Markets open family tools","Family",new Vector2(385,-287),new Vector2(100,42),()=>MarketsToggleDrawer(marketsFamilyDrawer));
            marketsPlacesDrawer=Panel(safe,"Markets pictured place chooser",new Vector2(0,-20),new Vector2(790,350),new Color(1,.99f,.94f,.98f),true,false).rectTransform;
            string[] titles={"Smoothies","Flowers","Cakes","Bubbles","Music","Buttermilk","Biscuit","Ice cream"};
            string[] icons={"cup","pfb_mkd_Flower_Pink","pfb_mkd_CupcakePink","pfb_mkd_BubbleWand","Discoveries/guitar","pony-buttermilk","pony-biscuit","ice-chocolate"};
            for(int i=0;i<MarketsRules.StationIds.Length;i++){
                string id=MarketsRules.StationIds[i];var hit=MarketsWorldObject(marketsPlacesDrawer,"Markets visit "+id,icons[i],new Vector2((i%4-1.5f)*186,78-i/4*153),new Vector2(164,130),()=>{marketsPlacesDrawer.gameObject.SetActive(false);MarketsGo(id);},titles[i]);
                hit.GetComponent<Image>().color=new Color(.95f,.97f,.88f,.9f);
            }
            MarketsSmallButton(marketsPlacesDrawer,"Markets close places","×",new Vector2(368,151),new Vector2(35,35),()=>marketsPlacesDrawer.gameObject.SetActive(false));marketsPlacesDrawer.gameObject.SetActive(false);
            marketsTools=Rect(safe,"Markets compact family actions",Vector2.zero,new Vector2(1120,720));
            marketsFamilyDrawer=Panel(marketsTools,"Markets family drawer",new Vector2(333,-144),new Vector2(230,238),new Color(1,.99f,.94f,.98f),true,false).rectTransform;
            string[] family={"Put down","Give","Leave","More items"};
            for(int i=0;i<family.Length;i++){string action=family[i];MarketsSmallButton(marketsFamilyDrawer,"Markets tool "+action,action,new Vector2(0,80-i*51),new Vector2(207,43),()=>{MarketsTool(action);marketsFamilyDrawer.gameObject.SetActive(false);});}
            marketsFamilyDrawer.gameObject.SetActive(false);
            marketsControls=Rect(Board,"Markets on-world objects",Vector2.zero,Board.rect.size);
            var hint=Panel(safe,"Markets action hint",new Vector2(0,240),new Vector2(570,29),new Color(1,.99f,.91f,.86f),false,false);marketsHint=Label(hint.transform,"",16,Vector2.zero,new Vector2(560,28));
            var progress=Panel(safe,"Markets shared progress",new Vector2(0,272),new Vector2(570,29),new Color(1,.99f,.91f,.86f),false,false);marketsProgress=Label(progress.transform,"",16,Vector2.zero,new Vector2(560,28));
            // World sign pictures replace permanent top tabs. They stay outside
            // the work surface, and their targets use the original stall names.
            for(int i=0;i<3;i++){
                string id=new[]{"bubbles","smoothie","flowers"}[i],icon=new[]{"pfb_mkd_BubbleWand","cup","pfb_mkd_Flower_Pink"}[i];
                var sign=MarketsWorldObject(Board,"Markets stall sign "+id,"Discoveries/sign",new Vector2(-488,115-i*90),Vector2.one*76,()=>MarketsGo(id));
                MarketsPicture(sign,"Pictured stall",icon,new Vector2(0,7),Vector2.one*36);marketsLayoutSigns.Add(sign);
            }
            // Incidental source objects are actual world hotspots, not entries
            // in the former seven-label text toolbar.
            MarketsWorldObject(marketsTools,"Markets tool Sign","Discoveries/sign",new Vector2(-440,-287),Vector2.one*48,()=>MarketsTool("Sign"));
            MarketsWorldObject(marketsTools,"Markets tool Umbrella","flowers",new Vector2(-367,-287),Vector2.one*48,()=>MarketsTool("Umbrella"),"",new Rect(422/1600f,1-454/1000f,370/1600f,233/1000f));
            MarketsWorldObject(marketsTools,"Markets tool Long dog","Discoveries/tex_emb_long_dog",new Vector2(468,220),new Vector2(84,48),()=>MarketsTool("Long dog"));
            marketsBuildBoard=Rect(Board,"Markets shared building board",new Vector2(-45,-60),new Vector2(470,320));
            var rail=Panel(marketsBuildBoard,"Markets foreground building rail",new Vector2(0,-107),new Vector2(365,18),new Color(.76f,.55f,.36f,.85f),false,false);
            for(int i=0;i<16;i++){
                int slot=i;var panel=Panel(marketsBuildBoard,"Markets build slot "+i,new Vector2((i%4-1.5f)*76,-79+(i/4)*62),new Vector2(72,58),new Color(1,1,1,.60f),true,false);
                var image=MarketsPicture(panel.transform,"Placed brick","",Vector2.zero,new Vector2(68,57));marketsBuildSlots[i]=image;NavButton(panel,()=>MarketsBuildSlot(slot));
            }
            marketsPalette=Rect(Board,"Markets horizontal brick palette",new Vector2(-20,190),new Vector2(780,79));
            var viewport=Panel(marketsPalette,"Markets palette viewport",Vector2.zero,new Vector2(780,79),new Color(.98f,.96f,.88f,.84f),true,false).rectTransform;viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect(viewport,"Markets palette content",Vector2.zero,new Vector2(MarketsRules.Bricks.Length*67,73));content.anchorMin=content.anchorMax=new Vector2(0,.5f);content.pivot=new Vector2(0,.5f);content.anchoredPosition=Vector2.zero;
            var scroll=marketsPalette.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            for(int i=0;i<MarketsRules.Bricks.Length;i++){
                string brick=MarketsRules.Bricks[i];var hit=MarketsWorldObject(content,"Markets choose brick "+i,"pfb_mkd_byo_"+brick,new Vector2(34+i*67,0),new Vector2(63,66),()=>{marketsSelectedBrick=brick;marketsSelectedPlaced="";});
                // Rect helpers default to centered anchors; this scrolling rail is left anchored.
                hit.anchorMin=hit.anchorMax=new Vector2(0,.5f);hit.anchoredPosition=new Vector2(34+i*67,0);
                MarketsAttachDrag(hit,"pfb_mkd_byo_"+brick,e=>{
                    foreach(var pair in marketsBuildSlots){var target=(RectTransform)pair.Value.transform.parent;if(RectTransformUtility.RectangleContainsScreenPoint(target,e.position,e.pressEventCamera)){marketsSelectedBrick=brick;marketsSelectedPlaced="";MarketsActAt("icecream",()=>MarketsBuildSlot(pair.Key));break;}}
                });
            }
            MarketsSmallButton(marketsPalette,"Markets palette previous","‹",new Vector2(-416,0),new Vector2(43,69),()=>scroll.horizontalNormalizedPosition=Mathf.Max(0,scroll.horizontalNormalizedPosition-.4f));
            MarketsSmallButton(marketsPalette,"Markets palette next","›",new Vector2(416,0),new Vector2(43,69),()=>scroll.horizontalNormalizedPosition=Mathf.Min(1,scroll.horizontalNormalizedPosition+.4f));
            MarketsSmallButton(marketsBuildBoard,"Markets remove brick","↶",new Vector2(-208,-88),Vector2.one*47,()=>{var b=Markets.bricks.FirstOrDefault(a=>a.id==marketsSelectedPlaced);if(b!=null)MarketsSend("build-remove",b.id,value:b.revision.ToString());});
            MarketsSmallButton(marketsBuildBoard,"Markets clear my bricks","♧",new Vector2(-208,-25),Vector2.one*47,()=>MarketsSend("build-clear",value:Markets.stations.Single(a=>a.id=="icecream").revision.ToString()));
            marketsMakeoverPanel=Panel(safe,"Markets pony makeover tent",new Vector2(0,15),new Vector2(610,340),new Color(.98f,.94f,1,.98f),true,false).rectTransform;
            marketsSongsPanel=Panel(safe,"Markets radio song picker",new Vector2(-315,103),new Vector2(325,148),new Color(1,.97f,.83f,.98f),true,false).rectTransform;
            marketsMakeoverPanel.gameObject.SetActive(false);marketsSongsPanel.gameObject.SetActive(false);
            marketsDragPicture=MarketsPicture(safe,"Markets dragged source object","",Vector2.zero,Vector2.one*80);marketsDragPicture.gameObject.SetActive(false);
        }
        private void MarketsToggleDrawer(RectTransform selected)
        {
            // Capture before closing the others so clicking an open drawer still
            // closes it, while switching drawers leaves exactly one input surface.
            bool open=selected&&!selected.gameObject.activeSelf;
            foreach(var drawer in new[]{marketsPlacesDrawer,marketsFamilyDrawer,marketsMakeoverPanel,marketsSongsPanel})
                if(drawer)drawer.gameObject.SetActive(drawer==selected&&open);
        }
        private void ResetMarketsLayout()
        {
            marketsPlacesDrawer=null;marketsFamilyDrawer=null;marketsMakeoverPanel=null;marketsSongsPanel=null;marketsPalette=null;marketsPrevious=null;marketsNext=null;marketsMap=null;marketsFamilyToggle=null;marketsDragPicture=null;marketsQueuedAction=null;marketsQueuedStation="";marketsLayoutSigns.Clear();marketsSceneObjects.Clear();marketsMakeoverTurn=0;
        }
        private RectTransform MarketsSmallButton(Transform parent,string name,string label,Vector2 position,Vector2 size,Action action)
        {var t=Button(parent,label,position,size,()=>action(),new Color(1,1,1,.7f));t.transform.parent.name=name;return (RectTransform)t.transform.parent;}
        private RectTransform MarketsWorldObject(Transform parent,string name,string icon,Vector2 position,Vector2 size,Action action,string caption="",Rect? uv=null)
        {
            var hit=Panel(parent,name,position,size,new Color(1,1,1,.015f),true,false);var image=MarketsPicture(hit.transform,"Source object "+name,icon,new Vector2(0,string.IsNullOrEmpty(caption)?0:9),size-new Vector2(4,string.IsNullOrEmpty(caption)?4:22));
            if(uv.HasValue)image.uvRect=uv.Value;
            if(!string.IsNullOrEmpty(caption))Label(hit.transform,caption,14,new Vector2(0,-size.y*.5f+10),new Vector2(size.x+10,24));NavButton(hit,action);return hit.rectTransform;
        }
        private void MarketsStepScene(int direction)
        {
            string[] scenes={"smoothie","buttermilk","icecream","music"};string current=marketsLastView=="flowers"||marketsLastView=="bubbles"?"smoothie":marketsLastView=="ponies"?"buttermilk":marketsLastView;
            int index=Array.IndexOf(scenes,current);MarketsGo(scenes[(Math.Max(0,index)+direction+scenes.Length)%scenes.Length]);
        }
        private void MarketsActAt(string station,Action action)
        {
            if(ActionPending||CurrentArea!=MarketsRules.Zone)return;
            var member=Markets.members.Single(m=>m.actor==Actor);var p=ReadPlayer(Actor);
            if(member.station==station&&Math.Abs(p.x-MarketsRules.X(station))<=300){action();return;}
            MarketsGo(station);marketsQueuedAction=action;marketsQueuedStation=station;
        }
        private void MarketsAttachDrag(RectTransform source,string icon,Action<UnityEngine.EventSystems.PointerEventData> commit)
        {
            var handler=source.gameObject.AddComponent<MarketsObjectDrag>();
            handler.begin=e=>{if(ActionPending)return;CancelMarketsIntent();destination=null;if(Shared)shared.Walk(WalkMode.Stop);marketsDragPicture.texture=MarketsTexture(icon);marketsDragPicture.gameObject.SetActive(true);marketsDragPicture.transform.SetAsLastSibling();};
            handler.drag=e=>{if(!marketsDragPicture||!marketsDragPicture.gameObject.activeSelf)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,e.position,e.pressEventCamera,out var point);marketsDragPicture.rectTransform.anchoredPosition=point;};
            handler.end=e=>{bool active=marketsDragPicture&&marketsDragPicture.gameObject.activeSelf;if(marketsDragPicture)marketsDragPicture.gameObject.SetActive(false);if(active)commit(e);};
            handler.cancel=()=>{if(marketsDragPicture)marketsDragPicture.gameObject.SetActive(false);};
        }
        private RectTransform MarketsDirect(string name,string icon,Vector2 position,Vector2 size,string station,Action action,Func<RectTransform> target=null,string caption="")
        {
            var hit=MarketsWorldObject(marketsControls,"Markets "+name,icon,position,size,()=>MarketsActAt(station,action),caption);
            if(target!=null)MarketsAttachDrag(hit,icon,e=>{var t=target();if(t&&RectTransformUtility.RectangleContainsScreenPoint(t,e.position,e.pressEventCamera))MarketsActAt(station,action);});
            return hit;
        }
        private string MarketsPonyAt(UnityEngine.EventSystems.PointerEventData pointer)
        {
            string picked=null;float nearest=float.MaxValue;
            foreach(var pair in marketsPonies){
                var image=pair.Value;if(!image||!image.gameObject.activeInHierarchy)continue;
                var transform=image.rectTransform;
                if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform,pointer.position,pointer.pressEventCamera,out var point))continue;
                var rect=transform.rect;
                float u=(point.x-rect.xMin)/rect.width,v=(point.y-rect.yMin)/rect.height;
                // Actual alpha bounds of the512px source pony, not its broad
                // transparent canvas: x115..402, top-origin y77..434. If the
                // two transformed visible bounds overlap, the nearest center
                // wins deterministically instead of silently using selection.
                if(u<115/512f||u>402/512f||v<78/512f||v>435/512f)continue;
                float distance=(point-rect.center).sqrMagnitude;
                if(distance<nearest){nearest=distance;picked=pair.Key;}
            }
            return picked;
        }
        private void MarketsBuildWorldControls(string station)
        {
            marketsSceneObjects.Clear();if(station=="")return;var h=Markets;
            if(station=="smoothie"){
                var mixer=MarketsDirect("Blend","pfb_mkd_Mixer",new Vector2(245,-34),new Vector2(130,155),station,()=>MarketsSend("mix"));marketsSceneObjects["mixer"]=mixer;
                for(int i=0;i<MarketsRules.Fruit.Length;i++){string fruit=MarketsRules.Fruit[i];MarketsDirect(fruit,"pfb_mkd_FoodItem_"+Upper(fruit),new Vector2(-310+i*117,-105),Vector2.one*95,station,()=>MarketsSend("ingredient",fruit),()=>mixer);}
            }else if(station=="flowers"){
                var bunch=MarketsDirect("Bouquet","pfb_mkd_Flower_Pink",new Vector2(230,-25),new Vector2(110,150),station,()=>MarketsSend("bouquet"));
                for(int i=0;i<MarketsRules.Flowers.Length;i++){string flower=MarketsRules.Flowers[i];MarketsDirect(flower,"pfb_mkd_Flower_"+Upper(flower),new Vector2(-285+i%3*105,-55-i/3*89),Vector2.one*85,station,()=>MarketsSend("flower",flower),()=>bunch);}
                MarketsDirect("pink cake","pfb_mkd_CupcakePink",new Vector2(385,-97),Vector2.one*90,"cakes",()=>MarketsSend("cake","pink"));
            }else if(station=="cakes"){
                for(int i=0;i<MarketsRules.Cakes.Length;i++){string cake=MarketsRules.Cakes[i];MarketsDirect(cake+" cake","pfb_mkd_Cupcake_"+Upper(cake),new Vector2(-170+i*165,-70),Vector2.one*115,station,()=>MarketsSend("cake",cake));}
            }else if(station=="bubbles"){
                var bucket=MarketsWorldObject(marketsControls,"Markets bubble bucket","pfb_mkd_Bucket",new Vector2(235,-115),Vector2.one*100,()=>MarketsActAt(station,()=>MarketsSend("bubble","0")));
                for(int i=0;i<6;i++){string shape=i.ToString();string name=i==0?"Round":"BLUEY"[i-1].ToString();MarketsDirect(name,"pfb_mkd_BubbleWand",new Vector2(-325+i*108,-120),new Vector2(83,110),station,()=>MarketsSend("bubble",shape),()=>bucket,i==0?"":name);}
            }else if(station=="buttermilk"||station=="biscuit"){
                string pony=station;
                string[] jobs={"food","water","brush","clean","wash"},icons={"apple","water","brush","pitchfork","water"};
                Vector2[] points={new Vector2(-340,-130),new Vector2(350,-125),new Vector2(318,45),new Vector2(-330,18),new Vector2(424,-25)};
                void Tool(string name,string icon,Vector2 point,Vector2 size,string job,string food=""){
                    var hit=MarketsDirect(name,icon,point,size,pony,()=>MarketsSend("care",job,pony,food));
                    MarketsAttachDrag(hit,icon,e=>{string recipient=MarketsPonyAt(e);if(recipient!=null)MarketsActAt(recipient,()=>MarketsSend("care",job,recipient,food));});
                }
                for(int i=0;i<jobs.Length;i++){string job=jobs[i];Tool(Upper(job),icons[i],points[i],Vector2.one*(job=="clean"?112:95),job,job=="food"?"apple":"");}
                Tool("Carrot","pfb_mkd_Carrots_Scene1",new Vector2(-228,-158),Vector2.one*82,"food","carrot");
                MarketsSmallButton(marketsControls,"Markets open makeover","Style",new Vector2(400,121),new Vector2(90,48),()=>{MarketsPopulateMakeover(pony);MarketsToggleDrawer(marketsMakeoverPanel);});
                MarketsSmallButton(marketsControls,"Markets Care again","♥ ↺",new Vector2(-400,130),new Vector2(80,48),()=>MarketsActAt(pony,()=>MarketsSend("pony-reset",target:pony)));
            }else if(station=="music")MarketsBuildMusicControls();
            else if(station=="icecream"){
                for(int i=0;i<3;i++){string order=MarketsRules.IceCreamNames[i];MarketsDirect(Upper(order),"ice-"+order,new Vector2(390,109-i*102),new Vector2(110,93),station,()=>MarketsSend("ice-select",order,value:Markets.stations.Single(t=>t.id=="icecream").revision.ToString()));}
                var ice=h.stations.Single(t=>t.id=="icecream");string next=ice.parts.Length<MarketsRules.Recipes[ice.choice].Length?MarketsRules.Recipes[ice.choice][ice.parts.Length]:MarketsRules.Recipes[ice.choice][0];
                MarketsDirect("Add scoop","pfb_mkd_byo_"+next,new Vector2(233,-153),Vector2.one*89,station,()=>{var t=Markets.stations.Single(x=>x.id=="icecream");if(t.parts.Length<MarketsRules.Recipes[t.choice].Length)MarketsSend("ice-add",MarketsRules.Recipes[t.choice][t.parts.Length],value:t.revision.ToString());});
                MarketsSmallButton(marketsControls,"Markets Start again","↺",new Vector2(-302,-174),Vector2.one*45,()=>MarketsActAt(station,()=>MarketsSend("ice-reset",value:Markets.stations.Single(t=>t.id=="icecream").revision.ToString())));
            }
        }
        private void MarketsPopulateMakeover(string pony)
        {
            foreach(Transform child in marketsMakeoverPanel){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            Label(marketsMakeoverPanel,Upper(pony)+" — makeover",22,new Vector2(0,141),new Vector2(480,35));
            MarketsSmallButton(marketsMakeoverPanel,"Markets close makeover","‹",new Vector2(-263,140),Vector2.one*45,()=>marketsMakeoverPanel.gameObject.SetActive(false));
            var p=Markets.ponies.Single(x=>x.id==pony);MarketsPicture(marketsMakeoverPanel,"Markets makeover preview","pony-body-"+p.body+"-mane-"+p.mane+"-saddle-"+p.saddle,new Vector2(0,20),Vector2.one*225);
            string[] parts={"body","mane","saddle","tail"};
            for(int i=0;i<parts.Length;i++){string part=parts[i];var hit=MarketsWorldObject(marketsMakeoverPanel,"Markets "+Upper(part),"pony-body-"+p.body+"-mane-"+p.mane+"-saddle-"+p.saddle,new Vector2((i-1.5f)*134,-117),new Vector2(117,76),()=>{var now=Markets.ponies.Single(x=>x.id==pony);int index=part=="body"?now.body:part=="saddle"?now.saddle:now.mane;MarketsActAt(pony,()=>MarketsSend("makeover",part,pony,((index+1)%(part=="body"?5:6)).ToString()));},Upper(part));}
            MarketsSmallButton(marketsMakeoverPanel,"Markets rotate makeover left","↶",new Vector2(-175,10),Vector2.one*47,()=>marketsMakeoverTurn-=12);
            MarketsSmallButton(marketsMakeoverPanel,"Markets rotate makeover right","↷",new Vector2(175,10),Vector2.one*47,()=>marketsMakeoverTurn+=12);
        }
        private void MarketsBuildMusicControls()
        {
            var guitar=MarketsDirect("Guitar","Discoveries/guitar",new Vector2(0,-151),new Vector2(610,215),"music",()=>MarketsSend("guitar"));
            for(int i=0;i<3;i++){
                int note=i;var stringHit=Panel(guitar,"Markets Guitar string "+i,new Vector2(-103,9-i*16),new Vector2(150,27),new Color(1,1,1,.05f),true,false);
                NavButton(stringHit,()=>MarketsActAt("music",()=>MarketsSend("guitar")));
                var drag=stringHit.gameObject.AddComponent<MarketsObjectDrag>();drag.end=e=>MarketsActAt("music",()=>MarketsSend("guitar"));
            }
            string[] names={"Drums","Keys","Beat 3"};
            for(int i=0;i<3;i++){
                string beat=i.ToString();var hit=Panel(marketsControls,"Markets "+names[i],new Vector2(-205+i*205,-18),Vector2.one*77,new Color(.9f,.63f+i*.08f,.25f,.91f),true,false);hit.sprite=circle;
                Label(hit.transform,"♪",35,Vector2.zero,Vector2.one*70);NavButton(hit,()=>MarketsActAt("music",()=>MarketsSend("beat",beat)));
            }
            var h=Markets;var radio=MarketsDirect(h.playing?"Stop":"Play","music",new Vector2(-405,55),new Vector2(108,92),"music",()=>MarketsSend(h.playing?"music-stop":"music-start"));
            radio.GetComponentsInChildren<RawImage>().First().uvRect=new Rect(145/1600f,1-478/1000f,151/1600f,176/1000f);
            MarketsSmallButton(marketsControls,"Markets open songs","↻",new Vector2(-335,55),Vector2.one*46,()=>{
                MarketsToggleDrawer(marketsSongsPanel);
                foreach(Transform child in marketsSongsPanel){child.gameObject.SetActive(false);Destroy(child.gameObject);}
                for(int i=0;i<3;i++){string song=i.ToString();MarketsWorldObject(marketsSongsPanel,"Markets Song "+(i+1),"music",new Vector2((i-1)*100,0),new Vector2(94,94),()=>{MarketsActAt("music",()=>MarketsSend("song",song));marketsSongsPanel.gameObject.SetActive(false);},"♪ "+(i+1),new Rect(145/1600f,1-478/1000f,151/1600f,176/1000f));}
            });
        }
        private void TickMarketsLayout(MarketsState h,string current,bool visible)
        {
            if(!marketsTabs)return;
            marketsPalette.gameObject.SetActive(visible&&current=="icecream");foreach(var s in marketsLayoutSigns)s.gameObject.SetActive(visible&&(marketsLastView=="smoothie"||marketsLastView=="flowers"||marketsLastView=="bubbles"));
            if(!visible){marketsPlacesDrawer.gameObject.SetActive(false);marketsFamilyDrawer.gameObject.SetActive(false);marketsMakeoverPanel.gameObject.SetActive(false);marketsSongsPanel.gameObject.SetActive(false);if(marketsDragPicture)marketsDragPicture.gameObject.SetActive(false);return;}
            if(marketsQueuedAction!=null&&!ActionPending&&h.members.Single(m=>m.actor==Actor).station==marketsQueuedStation){var action=marketsQueuedAction;marketsQueuedAction=null;marketsQueuedStation="";action();}
            foreach(var pony in marketsPonies){var button=pony.Value.GetComponent<Button>();if(!button){string id=pony.Key;pony.Value.raycastTarget=true;NavButton(pony.Value,()=>MarketsGo(id));} }
            if(marketsLastView!="ponies")marketsMakeoverPanel.gameObject.SetActive(false);if(marketsLastView!="music")marketsSongsPanel.gameObject.SetActive(false);
            if(marketsMakeoverPanel.gameObject.activeSelf){var picture=marketsMakeoverPanel.GetComponentsInChildren<RawImage>().FirstOrDefault(i=>i.name=="Markets makeover preview");if(picture){var p=h.ponies.Single(x=>x.id==current);picture.texture=MarketsTexture("pony-body-"+p.body+"-mane-"+p.mane+"-saddle-"+p.saddle);picture.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Clamp(marketsMakeoverTurn,-20,20));}}
            marketsPalette.SetAsLastSibling();marketsControls.SetAsLastSibling();marketsTabs.SetAsLastSibling();marketsTools.SetAsLastSibling();
            marketsPlacesDrawer.SetAsLastSibling();marketsMakeoverPanel.SetAsLastSibling();marketsSongsPanel.SetAsLastSibling();
            if(marketsDragPicture&&marketsDragPicture.gameObject.activeSelf)marketsDragPicture.transform.SetAsLastSibling();
            // Prop pictures are foreground UI, with their origin bound to the
            // actual actor hand in MarketsPresentActionProps. Pony meshes and
            // badges must not cover the apple, bucket or brush at contact.
            foreach(var image in marketsActionProps.Values)if(image.gameObject.activeSelf)image.transform.SetAsLastSibling();
        }
    }
}
