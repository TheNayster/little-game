using System;
using LittleWeeps.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform worlds, characterTray, familyCircle, worldButton, worldViewport, worldContent, trayPanel, trayViewport;
        private ScrollRect worldScroll, characterScroll;
        private Sprite pictureRim;
        private readonly List<RectTransform> worldBubbles = new List<RectTransform>();
        private readonly Dictionary<string, Image> avatarMarkers = new Dictionary<string, Image>();
        private readonly List<GameCharacterVisual> pickerCharacters = new List<GameCharacterVisual>();
        private readonly Dictionary<string, Text> worldBadges = new Dictionary<string, Text>();
        private static readonly string[] WorldIds = { "home", "park", "creek", "beach", "daycare" };
        private static readonly string[] WorldNames = { "Heeler Home", "Playground & Park", "The Creek", "The Beach", "Daycare" };
        private float navigationCamera;
        private bool navigationManualCamera;
        public bool WorldsOpen => CharactersOpen;
        public bool CharactersOpen => characterTray != null && characterTray.gameObject.activeSelf;

        private void ResetNavigation()
        {
            ResetTravelScreen();
            worlds=null;characterTray=null;familyCircle=null;worldButton=null;
            worldBubbles.Clear();avatarMarkers.Clear();pickerCharacters.Clear();worldBadges.Clear();
        }
        private static void Stretch(RectTransform r)
        { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero; }
        private NavigationTap NavButton(Image image, Action action)
        {
            var button=image.gameObject.AddComponent<NavigationTap>();
            button.onClick.AddListener(()=>action());
            button.navigation=new Navigation {mode=Navigation.Mode.None};
            return button;
        }
        private Image Plain(Transform parent,string name,Vector2 pos,Vector2 size,Color color,bool raycast=false)
        { var image=Panel(parent,name,pos,size,color,raycast);image.sprite=null;return image; }
        private Image RoundControl(Transform parent,string name,Vector2 pos,float size,Action action)
        {
            var rim=Panel(parent,name,pos,Vector2.one*size,Color.white,true,true);
            Panel(rim.transform,"Blue face",Vector2.zero,Vector2.one*(size-10),new Color(.33f,.76f,.94f),false,true);
            NavButton(rim,action);return rim;
        }
        private void Arrow(Transform parent)
        {
            Plain(parent,"Arrow stem",new Vector2(0,7),new Vector2(13,40),Color.white);
            var left=Plain(parent,"Arrow left",new Vector2(-11,-9),new Vector2(35,13),Color.white);
            left.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
            var right=Plain(parent,"Arrow right",new Vector2(11,-9),new Vector2(35,13),Color.white);
            right.rectTransform.localRotation=Quaternion.Euler(0,0,45);
        }
        private GameCharacterVisual PickerCharacter(Transform parent,string id,Vector2 pos,float scale)
        {
            var root=Rect(parent,id+" artwork",pos,Vector2.zero);root.localScale=Vector3.one*scale;
            var art=root.gameObject.AddComponent<GameCharacterVisual>();art.Select(id);
            pickerCharacters.Add(art);return art;
        }
        private void BuildNavigation()
        {
            if(pictureRim==null)pictureRim=Shape(true,true);
            worldButton=RoundControl(safe,"Worlds",new Vector2(-510,343),84,()=>ShowWorlds(true)).rectTransform;
            var house=Rect(worldButton,"House icon",Vector2.zero,Vector2.zero);
            Plain(house,"House",new Vector2(0,-9),new Vector2(34,32),Color.white);
            var roof=Plain(house,"Roof",new Vector2(0,9),new Vector2(33,33),Color.white);
            roof.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            Plain(house,"Door",new Vector2(0,-15),new Vector2(11,23),new Color(.33f,.76f,.94f));
            familyCircle=RoundControl(safe,"Characters",new Vector2(505,-224),120,()=>ShowCharacters(true)).rectTransform;
            var face=Panel(familyCircle,"Family portrait",Vector2.zero,Vector2.one*106,new Color(.77f,.91f,1),false,true);
            face.gameObject.AddComponent<Mask>().showMaskGraphic=true;
            PickerCharacter(face.transform,"blue-pup",new Vector2(-19,-24),.61f);
            PickerCharacter(face.transform,"orange-pup",new Vector2(25,-24),.66f);

            // One touch-owning overlay: horizontal cast below, vertical places
            // on the right. Neither list pauses the shared simulation.
            characterTray=Plain(safe,"Character tray",Vector2.zero,Vector2.zero,new Color(.12f,.27f,.33f,.12f),true).rectTransform;Stretch(characterTray);
            worlds=Plain(characterTray,"Places rail",Vector2.zero,Vector2.zero,new Color(.78f,.93f,.99f),true).rectTransform;
            worlds.anchorMin=new Vector2(1,0);worlds.anchorMax=Vector2.one;worlds.pivot=new Vector2(1,.5f);
            worlds.offsetMin=new Vector2(-310,300);worlds.offsetMax=Vector2.zero;
            var rim=Plain(worlds,"Rail edge",Vector2.zero,new Vector2(6,0),Color.white).rectTransform;
            rim.anchorMin=Vector2.zero;rim.anchorMax=new Vector2(0,1);rim.sizeDelta=new Vector2(6,0);
            var heading=Label(worlds,"Places",27,Vector2.zero,new Vector2(270,45));heading.fontStyle=FontStyle.Bold;
            heading.rectTransform.anchorMin=heading.rectTransform.anchorMax=new Vector2(.5f,1);heading.rectTransform.anchoredPosition=new Vector2(0,-27);
            worldViewport=Plain(worlds,"World viewport",Vector2.zero,Vector2.zero,Color.clear,true).rectTransform;
            Stretch(worldViewport);worldViewport.offsetMin=new Vector2(12,14);worldViewport.offsetMax=new Vector2(-12,-58);
            worldViewport.gameObject.AddComponent<RectMask2D>();
            worldContent=Rect(worldViewport,"World pictures",Vector2.zero,new Vector2(280,WorldIds.Length*212));
            worldContent.anchorMin=worldContent.anchorMax=new Vector2(.5f,1);worldContent.pivot=new Vector2(.5f,1);
            worldScroll=worldViewport.gameObject.AddComponent<ScrollRect>();worldScroll.viewport=worldViewport;worldScroll.content=worldContent;
            worldScroll.horizontal=false;worldScroll.vertical=true;worldScroll.movementType=ScrollRect.MovementType.Clamped;
            for(var i=0;i<WorldIds.Length;i++)BuildWorldBubble(i);

            trayPanel=Plain(characterTray,"Cast shelf",Vector2.zero,Vector2.zero,new Color(.71f,.89f,.99f),true).rectTransform;
            trayPanel.anchorMin=Vector2.zero;trayPanel.anchorMax=new Vector2(1,0);trayPanel.pivot=new Vector2(.5f,0);trayPanel.sizeDelta=new Vector2(0,300);
            var edge=Plain(trayPanel,"White top edge",Vector2.zero,Vector2.zero,Color.white).rectTransform;
            edge.anchorMin=new Vector2(0,1);edge.anchorMax=Vector2.one;edge.sizeDelta=new Vector2(0,7);
            var close=Panel(trayPanel,"Close characters",Vector2.zero,new Vector2(112,110),Color.white,true);
            close.rectTransform.anchorMin=close.rectTransform.anchorMax=new Vector2(0,1);close.rectTransform.anchoredPosition=new Vector2(87,40);
            Panel(close.transform,"Blue arrow tab",Vector2.zero,new Vector2(100,98),new Color(.33f,.76f,.94f));Arrow(close.transform);NavButton(close,()=>ShowCharacters(false));
            trayViewport=Plain(trayPanel,"Cast viewport",Vector2.zero,Vector2.zero,Color.clear,true).rectTransform;
            Stretch(trayViewport);trayViewport.offsetMin=new Vector2(168,0);trayViewport.offsetMax=new Vector2(-24,0);trayViewport.gameObject.AddComponent<RectMask2D>();
            var cast=Rect(trayViewport,"Full body cast",Vector2.zero,new Vector2(650,300));cast.anchorMin=cast.anchorMax=new Vector2(0,0);cast.pivot=Vector2.zero;
            characterScroll=trayViewport.gameObject.AddComponent<ScrollRect>();characterScroll.viewport=trayViewport;characterScroll.content=cast;characterScroll.vertical=false;characterScroll.movementType=ScrollRect.MovementType.Clamped;
            for(var i=0;i<2;i++)
            {
                var id=i==0?"blue-pup":"orange-pup";var name=i==0?"Bluey":"Bingo";
                var card=Plain(cast,name,new Vector2(150+i*270,150),new Vector2(210,292),Color.clear,true);
                card.rectTransform.anchorMin=card.rectTransform.anchorMax=Vector2.zero;
                var marker=Panel(card.transform,"Selected character",new Vector2(0,-80),new Vector2(146,30),new Color(1,.86f,.39f),false,true);avatarMarkers.Add(id,marker);
                PickerCharacter(card.transform,id,new Vector2(0,-24),1.45f);
                Label(card.transform,name,25,new Vector2(0,-124),new Vector2(190,38)).fontStyle=FontStyle.Bold;
                NavButton(card,()=>{if(!ActionPending && ReadPlayer(Actor).avatar!=id)ChooseAvatar(id);});
            }
            characterTray.gameObject.SetActive(false);
            BuildTravelScreen();
        }
        private void BuildWorldBubble(int index)
        {
            var id=WorldIds[index];var ready=true;
            var group=Rect(worldContent,"World "+id,Vector2.zero,new Vector2(280,212));worldBubbles.Add(group);
            group.anchorMin=group.anchorMax=new Vector2(.5f,1);
            group.anchoredPosition=new Vector2(0,-90-index*212);
            Panel(group,"Soft shadow",new Vector2(0,-6),Vector2.one*174,new Color(.23f,.51f,.63f,.23f),false,true);
            var border=Panel(group,"World "+(id=="garden"?"Garden":id=="creek"?"Creek":WorldNames[index]),Vector2.zero,Vector2.one*170,Color.white,true,true);
            var button=NavButton(border,()=>{if(!ready || WorldLoading)return;BeginWorldTravel(id);});button.interactable=ready;
            var picture=Panel(border.transform,"Round scene",Vector2.zero,Vector2.one*158,Color.white,false,true);
            picture.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var illustration=Panel(picture.transform,"Illustrated "+id,Vector2.zero,Vector2.one*158,Color.white);
            illustration.sprite=Resources.Load<Sprite>("WorldMenu/"+id);illustration.type=Image.Type.Simple;
            if(illustration.sprite==null)throw new InvalidOperationException("Missing world-menu illustration: "+id);
            if(!ready)illustration.color=new Color(1,1,1,.67f);
            var rim=Panel(border.transform,"White picture edge",Vector2.zero,Vector2.one*170,Color.white,false,true);
            rim.sprite=pictureRim;
            var label=Label(group,WorldNames[index],21,new Vector2(0,-99),new Vector2(280,36));label.fontStyle=FontStyle.Bold;
            if(id=="home")Label(group,"House + backyard",15,new Vector2(0,-122),new Vector2(280,25));
            var badge=Panel(group,"Destination status",new Vector2(0,-68),new Vector2(146,29),ready?new Color(.98f,.91f,.6f):new Color(.95f,.98f,1));
            var text=Label(badge.transform,ready?"Let's play":"Coming later",16,Vector2.zero,new Vector2(146,29));worldBadges.Add(id,text);
        }
        public void ShowWorlds(bool open) => ShowCharacters(open);
        public void ShowCharacters(bool open)
        {
            if(WorldLoading)return;
            lastLocalAction=Time.realtimeSinceStartup;
            if(open && !CharactersOpen)
            {CloseMiniGames();navigationCamera=cameraX;navigationManualCamera=manualCamera;CancelPointers();Narration.Stop();menu.SetActive(false);characterTray.SetAsLastSibling();}
            if(!open && CharactersOpen){cameraX=navigationCamera;manualCamera=navigationManualCamera;}
            characterTray.gameObject.SetActive(open);stick.gameObject.SetActive(JoystickMode && !MenuOpen);familyCircle.gameObject.SetActive(!MenuOpen);
            LayoutWorldViewport();
            RenderNavigation();
        }
        private void CloseNavigation()
        {
            CloseMiniGames();
            if(CharactersOpen){cameraX=navigationCamera;manualCamera=navigationManualCamera;characterTray.gameObject.SetActive(false);}
            LayoutWorldViewport();
            if(familyCircle!=null)familyCircle.gameObject.SetActive(true);
        }
        private void RenderNavigation()
        {
            if(!HasWorld)return;
            var selected=ReadPlayer(Actor).avatar;
            foreach(var pair in avatarMarkers)pair.Value.gameObject.SetActive(pair.Key==selected);
            foreach(var pair in worldBadges)pair.Value.text=(pair.Key=="home" && HomeRooms.Property(CurrentArea) || WorldLayout.Canonical(pair.Key)==CurrentArea)?"You're here":"Let's play";
        }
        private void AnimateNavigation()
        {
            foreach(var character in pickerCharacters)if(character!=null && character.gameObject.activeInHierarchy)
                character.Present(Vector2.zero,"picker/"+character.CharacterId,false,applicationPaused?0:Time.unscaledDeltaTime);
        }
        private void LayoutNavigation()
        {
            if(worlds==null)return;
            worldButton.anchoredPosition=new Vector2(-safe.rect.width/2+85,safe.rect.height/2-72);
            familyCircle.anchoredPosition=new Vector2(safe.rect.width/2-88,-safe.rect.height/2+103);
            worldScroll.StopMovement();worldScroll.verticalNormalizedPosition=1;
            characterScroll.StopMovement();characterScroll.horizontalNormalizedPosition=0;
        }
    }
}
