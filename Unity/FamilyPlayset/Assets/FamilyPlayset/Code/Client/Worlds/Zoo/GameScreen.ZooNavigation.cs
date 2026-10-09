using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform zooNav,zooMap,zooMapFrame,zooCurrentGate;
        private RawImage zooCurrentPicture,zooPreviousPicture,zooNextPicture,zooTrailPicture;
        private Text zooCurrentName,zooTrailName,zooPreviousName,zooNextName;
        private NavigationTap zooBackButton,zooForwardButton,zooGateButton;
        private readonly Dictionary<string,Texture2D> zooPortraitCache=new Dictionary<string,Texture2D>();
        private readonly Dictionary<string,Image> zooMapRings=new Dictionary<string,Image>();
        private readonly Dictionary<string,RectTransform> zooMapPointers=new Dictionary<string,RectTransform>();
        private readonly List<RawImage> zooMapStops=new List<RawImage>();
        private readonly List<Text> zooMapStopNames=new List<Text>();
        private string zooMapTrail,zooMapArea,zooNavSeen="";
        private int zooMapPanel=-1;
        private float zooNavUntil;
        public bool ZooMapOpen=>zooMap!=null && zooMap.gameObject.activeSelf;
        public string ZooCurrentExhibit=>ZooCatalog.Trail(CurrentArea)?ZooInView().id:"entrance";
        public string ZooPreviousDestination=>ZooCatalog.Trail(CurrentArea)?ZooNeighbor(-1).id:"";
        public string ZooNextDestination=>ZooCatalog.Trail(CurrentArea)?ZooNeighbor(1).id:"";
        public string ZooMapShownTrail=>zooMapTrail??"";
        public string ZooMapMarker=>!ZooMapOpen?"":CurrentArea==ZooLayout.Entrance?"entrance":CurrentArea==zooMapTrail?ZooAtPlayer().id:CurrentArea;
        public bool ZooNavigationBusy=>zooApproach || ActionPending || Time.unscaledTime<zooNavUntil;
        private ZooSpecies ZooAtPlayer()=>ZooCatalog.All.First(s=>s.area==CurrentArea && s.panel==Mathf.Clamp(Mathf.FloorToInt(ReadPlayer(Actor).x/2400),0,3));
        // Panning changes what the child is looking at, not where their player
        // stands. Navigation and contextual controls must describe that view.
        private ZooSpecies ZooInView()=>ZooCatalog.All.First(s=>s.area==CurrentArea && s.panel==Mathf.Clamp(Mathf.FloorToInt(cameraX/2400),0,3));
        private bool ZooExhibitVisible(ZooSpecies info)=>info.area==CurrentArea && Mathf.Abs(info.Center-cameraX)<Board.rect.width/(2*sceneScale)+650;
        private ZooSpecies ZooNeighbor(int direction)
        {
            var current=ZooInView();var panel=current.panel+direction;
            var area=panel<0?ZooCatalog.Previous(current.area):panel>3?ZooCatalog.Next(current.area):current.area;
            return ZooCatalog.All.First(s=>s.area==area && s.panel==(panel<0?3:panel>3?0:panel));
        }
        private Texture2D ZooPortrait(string id)
        {
            if(!zooPortraitCache.TryGetValue(id,out var t)){t=WorldResources.Load<Texture2D>("Worlds/Zoo/Navigation/"+id);zooPortraitCache.Add(id,t);}
            return t;
        }
        private RawImage ZooPortraitView(Transform parent,string name,Vector2 at,float size)
        {
            var image=Rect(parent,name,at,Vector2.one*size).gameObject.AddComponent<RawImage>();image.raycastTarget=false;return image;
        }
        private void ZooArrow(Transform parent,Vector2 at,int direction)
        {
            Plain(parent,"Arrow stem",at,new Vector2(29,7),Ink);
            for(var i=0;i<2;i++){
                var arm=Plain(parent,"Arrow head",at+new Vector2(direction*11,i==0?6:-6),new Vector2(20,7),Ink);
                arm.rectTransform.localRotation=Quaternion.Euler(0,0,(i==0?-1:1)*direction*45);
            }
        }
        // Editable UI geometry follows the entrance's wooden arch; it is
        // deliberately distinct from the family's world chooser/home picture.
        private void ZooGatePicture(Transform parent,Vector2 at,float scale)
        {
            var root=Rect(parent,"Zoo gate picture",at,Vector2.zero);root.localScale=Vector3.one*scale;
            for(var i=-1;i<=1;i+=2){Plain(root,"Gate post",new Vector2(i*25,-4),new Vector2(13,55),new Color(.65f,.46f,.27f));Panel(root,"Post cap",new Vector2(i*25,23),new Vector2(19,10),new Color(.86f,.66f,.39f));}
            Panel(root,"Entrance arch",new Vector2(0,26),new Vector2(70,22),new Color(.86f,.66f,.39f));
            for(var i=-1;i<=1;i++)Panel(root,"Paw toe",new Vector2(i*9,31),Vector2.one*7,Ink,false,true);
            Panel(root,"Paw pad",new Vector2(0,21),new Vector2(17,11),Ink,false,true);
            Plain(root,"Path through gate",new Vector2(0,-25),new Vector2(38,7),new Color(.91f,.8f,.57f));
        }
        private void ZooMapPicture(Transform parent)
        {
            for(var i=0;i<3;i++){
                var p=Plain(parent,"Folded map",new Vector2((i-1)*18,8),new Vector2(19,49),i==1?new Color(.86f,.9f,.71f):new Color(.72f,.85f,.68f));
                p.rectTransform.localRotation=Quaternion.Euler(0,0,i==1?8:-8);
            }
            Plain(parent,"Map path",new Vector2(0,8),new Vector2(46,6),Cream);
            Panel(parent,"Map stop",new Vector2(-17,8),Vector2.one*11,Ink,false,true);
            Panel(parent,"Map stop",new Vector2(18,8),Vector2.one*11,Ink,false,true);
        }
        private void ZooYouHere(Transform parent,string id,Vector2 at,float size)
        {
            var ring=Panel(parent,"You are here ring",at,Vector2.one*size,new Color(.95f,.65f,.17f),false,true);ring.sprite=hintRing;zooMapRings.Add(id,ring);
            var pointer=Rect(parent,"You are here pointer",at+new Vector2(0,size/2+19),Vector2.zero);zooMapPointers.Add(id,pointer);
            ZooArrow(pointer,Vector2.zero,-1);pointer.localRotation=Quaternion.Euler(0,0,90);
        }
        private void BuildZooNavigation()
        {
            zooNav=Rect(safe,"Zoo picture navigation",Vector2.zero,new Vector2(880,124));
            var back=Panel(zooNav,"Zoo previous animal",new Vector2(-260,0),new Vector2(150,124),Cream,true);zooBackButton=NavButton(back,()=>NavigateZoo(-1));
            zooPreviousPicture=ZooPortraitView(back.transform,"Previous destination portrait",new Vector2(16,16),84);ZooArrow(back.transform,new Vector2(-49,11),-1);
            zooPreviousName=Label(back.transform,"",17,new Vector2(0,-40),new Vector2(144,28));
            var next=Panel(zooNav,"Zoo next animal",new Vector2(260,0),new Vector2(150,124),Cream,true);zooForwardButton=NavButton(next,()=>NavigateZoo(1));
            zooNextPicture=ZooPortraitView(next.transform,"Next destination portrait",new Vector2(-16,16),84);ZooArrow(next.transform,new Vector2(49,11),1);
            zooNextName=Label(next.transform,"",17,new Vector2(0,-40),new Vector2(144,28));
            var current=Panel(zooNav,"Current Zoo exhibit",Vector2.zero,new Vector2(326,112),Cream);
            ZooGatePicture(current.transform,new Vector2(-106,9),1.15f);zooCurrentGate=(RectTransform)current.transform.Find("Zoo gate picture");
            zooCurrentPicture=ZooPortraitView(current.transform,"Current animal portrait",new Vector2(-106,9),84);
            zooCurrentName=Label(current.transform,"",21,new Vector2(40,22),new Vector2(206,32));
            zooTrailPicture=ZooPortraitView(current.transform,"Trail symbol",new Vector2(-36,-20),35);
            zooTrailName=Label(current.transform,"",17,new Vector2(58,-20),new Vector2(160,36));
            var gate=Panel(zooNav,"Zoo entrance",new Vector2(-398,0),new Vector2(120,124),Cream,true);zooGateButton=NavButton(gate,ReturnZooEntrance);ZooGatePicture(gate.transform,new Vector2(0,10),1);
            Label(gate.transform,"Entrance",17,new Vector2(0,-40),new Vector2(100,28));
            var map=Panel(zooNav,"Zoo map",new Vector2(398,0),new Vector2(120,124),Cream,true);NavButton(map,OpenZooMap);ZooMapPicture(map.transform);
            Label(map.transform,"Map",17,new Vector2(0,-40),new Vector2(100,28));
            zooMap=Plain(safe,"Zoo map input shield",Vector2.zero,Vector2.zero,new Color(.16f,.25f,.22f,.74f),true).rectTransform;Stretch(zooMap);
            zooMapFrame=Panel(zooMap,"Picture trail map",Vector2.zero,new Vector2(1100,650),Cream,true).rectTransform;
            Label(zooMapFrame,"Zoo picture trails",30,new Vector2(0,282),new Vector2(700,48));
            var close=Panel(zooMapFrame,"Close Zoo map",new Vector2(481,267),Vector2.one*88,new Color(.78f,.88f,.79f),true);NavButton(close,CloseZooMap);
            for(var i=-1;i<=1;i+=2){var stroke=Plain(close.transform,"Close cross",Vector2.zero,new Vector2(39,7),Ink);stroke.rectTransform.localRotation=Quaternion.Euler(0,0,45*i);}
            ZooGatePicture(zooMapFrame,new Vector2(0,204),1.1f);Label(zooMapFrame,"Entrance",18,new Vector2(0,151),new Vector2(180,28));ZooYouHere(zooMapFrame,"entrance",new Vector2(0,204),93);
            Plain(zooMapFrame,"Entrance path",new Vector2(0,124),new Vector2(7,33),new Color(.7f,.67f,.51f));
            Plain(zooMapFrame,"Connected trails",new Vector2(0,106),new Vector2(780,7),new Color(.7f,.67f,.51f));
            for(var i=0;i<4;i++){
                var area=ZooCatalog.Trails[i];var x=-390+i*260;
                Plain(zooMapFrame,"Trail branch",new Vector2(x,85),new Vector2(7,44),new Color(.7f,.67f,.51f));
                var choice=Panel(zooMapFrame,"Show "+ZooCatalog.Name(area)+" trail",new Vector2(x,31),new Vector2(204,136),new Color(.89f,.91f,.79f),true);
                NavButton(choice,()=>{zooMapTrail=area;RefreshZooMap();});
                var pic=ZooPortraitView(choice.transform,"Trail picture",new Vector2(0,16),89);pic.texture=ZooPortrait(ZooTrailSymbol(area));
                Label(choice.transform,ZooCatalog.Name(area),18,new Vector2(0,-49),new Vector2(198,30));
                ZooYouHere(zooMapFrame,area,new Vector2(x,47),104);
                var stop=Panel(zooMapFrame,"Map animal stop "+i,new Vector2(x,-170),Vector2.one*126,Color.white,false,true);
                zooMapStops.Add(ZooPortraitView(stop.transform,"Animal stop portrait",Vector2.zero,112));
                zooMapStopNames.Add(Label(zooMapFrame,"",18,new Vector2(x,-252),new Vector2(245,38)));
                ZooYouHere(zooMapFrame,"stop"+i,new Vector2(x,-170),137);
            }
            // All four stops are connected; the upper trail chain and its
            // return leg depict the actual cyclic trail order, not a new route.
            var path=Plain(zooMapFrame,"Four connected animal stops",new Vector2(0,-170),new Vector2(780,7),new Color(.7f,.67f,.51f));path.transform.SetSiblingIndex(1);
            Plain(zooMapFrame,"Trail return path",new Vector2(0,-62),new Vector2(780,5),new Color(.7f,.67f,.51f));
            for(var i=-1;i<=1;i+=2)Plain(zooMapFrame,"Trail return end",new Vector2(i*390,-46),new Vector2(5,32),new Color(.7f,.67f,.51f));
            ZooArrow(zooMapFrame,new Vector2(0,-62),-1);
            for(var i=0;i<3;i++)ZooArrow(zooMapFrame,new Vector2(-260+i*260,30),1);
            Label(zooMapFrame,"Picture guide — close the map to walk",18,new Vector2(0,-299),new Vector2(850,29));
            zooMap.gameObject.SetActive(false);
        }
        private static string ZooTrailSymbol(string area)=>area==ZooLayout.Savanna?"elephant":area==ZooCatalog.Dinosaurs?"brachiosaurus":area==ZooCatalog.Reptiles?"crocodile":"clownfish";
        private void NavigateZoo(int direction)
        {
            if(ZooMapOpen || ZooNavigationBusy || TravelPending || !ZooCatalog.Trail(CurrentArea))return;
            var target=ZooNeighbor(direction);zooNavUntil=Time.unscaledTime+.35f;
            if(target.area!=CurrentArea)ZooWalk("gate",target.area,new Vector2(direction<0?200:9200,100));
            else ZooWalk("look",target.id,new Vector2(target.Center,100));
        }
        private void ReturnZooEntrance()
        {
            if(ZooMapOpen || ZooNavigationBusy || TravelPending || !ZooCatalog.Trail(CurrentArea))return;
            zooNavUntil=Time.unscaledTime+.35f;ZooWalk("gate",ZooLayout.Entrance,new Vector2(200,100));
        }
        private void OpenZooMap()
        {
            if(MenuOpen || TravelPending)return;
            // Clear held input without dropping a pending food walk or offer.
            // The modal stops only this client's movement; shared animals run on.
            var approach=zooApproach;var walking=destination;CancelPointers();zooApproach=approach;destination=walking;
            zooMapTrail=ZooCatalog.Trail(CurrentArea)?CurrentArea:ZooCatalog.Trails[0];zooMapArea=CurrentArea;
            zooMapPanel=ZooCatalog.Trail(CurrentArea)?ZooAtPlayer().panel:-1;
            zooMap.gameObject.SetActive(true);zooMap.SetAsLastSibling();RefreshZooMap();
        }
        private void CloseZooMap(){if(!ZooMapOpen)return;zooMap.gameObject.SetActive(false);stick.gameObject.SetActive(JoystickMode && !MenuOpen);}
        private void RefreshZooMap()
        {
            for(var i=0;i<4;i++){
                var info=ZooCatalog.All.First(s=>s.area==zooMapTrail && s.panel==i);zooMapStops[i].texture=ZooPortrait(info.id);zooMapStopNames[i].text=info.name;
            }
            foreach(var ring in zooMapRings){
                var here=ring.Key=="entrance"?CurrentArea==ZooLayout.Entrance:ring.Key.StartsWith("stop")?CurrentArea==zooMapTrail && int.Parse(ring.Key.Substring(4))==zooMapPanel:ring.Key==CurrentArea;
                ring.Value.gameObject.SetActive(here);zooMapPointers[ring.Key].gameObject.SetActive(here);
            }
            foreach(var area in ZooCatalog.Trails){var choice=zooMapFrame.Find("Show "+ZooCatalog.Name(area)+" trail");choice.GetComponent<Image>().color=area==zooMapTrail?new Color(.77f,.88f,.73f):new Color(.89f,.91f,.79f);choice.localScale=Vector3.one*(area==zooMapTrail?1.03f:1);}
        }
        private void TickZooNavigation(bool visible)
        {
            if(zooNav==null)return;
            if(!visible || WorldLoading){CloseZooMap();zooNav.gameObject.SetActive(false);return;}
            if(applicationPaused)CloseZooMap();
            zooNav.gameObject.SetActive(!CharactersOpen && !MenuOpen);
            // Reserve the world chooser on the left and movement/menu on the
            // right. The same strip stays usable at phone and tablet aspects.
            var scale=Mathf.Min(1,(safe.rect.width-600)/920);zooNav.localScale=Vector3.one*scale;zooNav.anchoredPosition=new Vector2(-140,safe.rect.height/2-76*scale);
            zooMapFrame.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(safe.rect.width/1140,safe.rect.height/690));
            if(ZooMapOpen){
                if(CurrentArea!=zooMapArea){zooMapArea=CurrentArea;zooMapTrail=ZooCatalog.Trail(CurrentArea)?CurrentArea:ZooCatalog.Trails[0];}
                zooMapPanel=ZooCatalog.Trail(CurrentArea)?ZooAtPlayer().panel:-1;RefreshZooMap();zooMap.SetAsLastSibling();
                if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseZooMap();
            }
            var habitat=ZooCatalog.Trail(CurrentArea);zooBackButton.gameObject.SetActive(habitat);zooForwardButton.gameObject.SetActive(habitat);zooGateButton.gameObject.SetActive(habitat);
            zooBackButton.interactable=zooForwardButton.interactable=zooGateButton.interactable=!ZooNavigationBusy && !TravelPending;
            // A brush acknowledgment is not a route change. Keep the arrows'
            // tint stable while retaining their normal transaction guards.
            void Tint(NavigationTap button){
                var colors=button.colors;colors.disabledColor=elephantCareSending && !zooApproach && !TravelPending?colors.normalColor:new Color(.521f,.521f,.521f,.502f);button.colors=colors;
            }
            Tint(zooBackButton);Tint(zooForwardButton);Tint(zooGateButton);
            var id=ZooCurrentExhibit;
            if(id==zooNavSeen)return;zooNavSeen=id;
            zooCurrentPicture.gameObject.SetActive(habitat);zooTrailPicture.gameObject.SetActive(habitat);
            zooCurrentGate.gameObject.SetActive(!habitat);
            if(!habitat){zooCurrentName.text="Zoo entrance";zooTrailName.text="Choose a picture trail";return;}
            var info=ZooInView();var previous=ZooNeighbor(-1);var next=ZooNeighbor(1);
            zooCurrentPicture.texture=ZooPortrait(info.id);zooTrailPicture.texture=ZooPortrait(ZooTrailSymbol(CurrentArea));zooCurrentName.text=info.name;zooTrailName.text=ZooCatalog.Name(CurrentArea);
            zooPreviousPicture.texture=ZooPortrait(previous.id);zooNextPicture.texture=ZooPortrait(next.id);zooPreviousName.text=previous.name;zooNextName.text=next.name;
        }
        private void ResetZooNavigation()
        {
            if(zooNav!=null)Destroy(zooNav.gameObject);if(zooMap!=null)Destroy(zooMap.gameObject);
            zooNav=zooMap=zooMapFrame=null;zooNavSeen="";zooNavUntil=0;
            zooMapRings.Clear();zooMapPointers.Clear();zooMapStops.Clear();zooMapStopNames.Clear();
            foreach(var texture in zooPortraitCache.Values)if(texture!=null)Resources.UnloadAsset(texture);zooPortraitCache.Clear();
        }
    }
}
