using System;
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
        private static readonly string[] WorldIds = { "home", "garden", "park", "creek", "beach", "daycare" };
        private static readonly string[] WorldNames = { "Heeler Home", "Backyard Garden", "Playground & Park", "The Creek", "The Beach", "Daycare" };
        public bool WorldsOpen => worlds != null && worlds.gameObject.activeSelf;
        public bool CharactersOpen => characterTray != null && characterTray.gameObject.activeSelf;

        private void ResetNavigation()
        {
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
            familyCircle=RoundControl(safe,"Characters",new Vector2(-505,-224),120,()=>ShowCharacters(true)).rectTransform;
            var face=Panel(familyCircle,"Family portrait",Vector2.zero,Vector2.one*106,new Color(.77f,.91f,1),false,true);
            face.gameObject.AddComponent<Mask>().showMaskGraphic=true;
            PickerCharacter(face.transform,"blue-pup",new Vector2(-19,-24),.61f);
            PickerCharacter(face.transform,"orange-pup",new Vector2(25,-24),.66f);

            worlds=Plain(safe,"World browser",Vector2.zero,Vector2.zero,new Color(.38f,.78f,.95f),true).rectTransform;Stretch(worlds);
            // Decorative clouds stay behind the independently scrollable bubbles.
            for(var i=0;i<7;i++)
            {
                var cloud=Rect(worlds,"Cloud",new Vector2(-640+i*210,(i%3-1)*270),new Vector2(220,90));
                for(var n=0;n<4;n++)Panel(cloud,"Cloud puff",new Vector2(-65+n*45,(n%2)*20),new Vector2(105,80),new Color(.94f,.99f,1,.92f),false,true);
            }
            var back=RoundControl(worlds,"Back from worlds",Vector2.zero,78,()=>ShowWorlds(false));
            var arrowRoot=Rect(back.transform,"Back arrow",Vector2.zero,Vector2.zero);Arrow(arrowRoot);arrowRoot.localRotation=Quaternion.Euler(0,0,-90);
            var heading=Label(worlds,"Where shall we play?",36,Vector2.zero,new Vector2(740,60));heading.name="World heading";heading.fontStyle=FontStyle.Bold;
            var settings=RoundControl(worlds,"World settings",Vector2.zero,78,()=>SetMenu(true));
            var cog=Panel(settings.transform,"Cog",Vector2.zero,Vector2.one*37,Color.white,false,true);
            for(var i=0;i<8;i++)
            {
                var tooth=Plain(cog.transform,"Tooth",Vector2.zero,new Vector2(47,11),Color.white);
                tooth.rectTransform.localRotation=Quaternion.Euler(0,0,i*45);
            }
            Panel(cog.transform,"Cog middle",Vector2.zero,Vector2.one*19,new Color(.33f,.76f,.94f),false,true);
            worldViewport=Plain(worlds,"World viewport",Vector2.zero,Vector2.zero,Color.clear,true).rectTransform;
            worldViewport.gameObject.AddComponent<RectMask2D>();
            worldContent=Rect(worldViewport,"World pictures",Vector2.zero,Vector2.zero);worldContent.anchorMin=worldContent.anchorMax=new Vector2(0,.5f);worldContent.pivot=new Vector2(0,.5f);
            worldScroll=worldViewport.gameObject.AddComponent<ScrollRect>();worldScroll.viewport=worldViewport;worldScroll.content=worldContent;worldScroll.vertical=false;worldScroll.movementType=ScrollRect.MovementType.Clamped;
            for(var i=0;i<WorldIds.Length;i++)BuildWorldBubble(i);
            var foot=Label(worlds,"More places are growing. Garden and Creek are ready to play.",20,Vector2.zero,new Vector2(1120,40));foot.name="World footer";
            worlds.gameObject.SetActive(false);

            // The transparent shield owns all remaining scene touches. It stops
            // local walking without pausing the server or another player's view.
            characterTray=Plain(safe,"Character tray",Vector2.zero,Vector2.zero,Color.clear,true).rectTransform;Stretch(characterTray);
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
        }
        private void BuildWorldBubble(int index)
        {
            var id=WorldIds[index];var ready=id=="garden" || id=="creek";
            var group=Rect(worldContent,"World "+id,Vector2.zero,new Vector2(300,335));worldBubbles.Add(group);
            group.anchorMin=group.anchorMax=new Vector2(0,.5f);
            Panel(group,"Soft shadow",new Vector2(0,-6),Vector2.one*284,new Color(.23f,.51f,.63f,.23f),false,true);
            var border=Panel(group,"World "+(id=="garden"?"Garden":id=="creek"?"Creek":WorldNames[index]),Vector2.zero,Vector2.one*280,Color.white,true,true);
            var button=NavButton(border,()=>{if(!ready || ActionPending)return;ShowWorlds(false);Travel(id);});button.interactable=ready;
            var picture=Panel(border.transform,"Round scene",Vector2.zero,Vector2.one*264,Color.white,false,true);
            picture.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var illustration=Panel(picture.transform,"Illustrated "+id,Vector2.zero,Vector2.one*264,Color.white);
            illustration.sprite=Resources.Load<Sprite>("WorldMenu/"+id);illustration.type=Image.Type.Simple;
            if(illustration.sprite==null)throw new InvalidOperationException("Missing world-menu illustration: "+id);
            if(!ready)illustration.color=new Color(1,1,1,.67f);
            var rim=Panel(border.transform,"White picture edge",Vector2.zero,Vector2.one*280,Color.white,false,true);
            rim.sprite=pictureRim;
            var label=Label(group,WorldNames[index],24,new Vector2(0,-164),new Vector2(320,38));label.fontStyle=FontStyle.Bold;
            var badge=Panel(group,"Destination status",new Vector2(0,-117),new Vector2(166,35),ready?new Color(.98f,.91f,.6f):new Color(.95f,.98f,1));
            var text=Label(badge.transform,ready?"Let's play":"Coming later",18,Vector2.zero,new Vector2(166,35));worldBadges.Add(id,text);
        }
        public void ShowWorlds(bool open)
        {
            lastLocalAction=Time.realtimeSinceStartup;
            if(open){CancelPointers();Narration.Stop();characterTray.gameObject.SetActive(false);menu.SetActive(false);worlds.SetAsLastSibling();}
            worlds.gameObject.SetActive(open);stick.gameObject.SetActive(JoystickMode && !MenuOpen);familyCircle.gameObject.SetActive(!MenuOpen);
            RenderNavigation();
        }
        public void ShowCharacters(bool open)
        {
            lastLocalAction=Time.realtimeSinceStartup;
            if(open){CancelPointers();Narration.Stop();worlds.gameObject.SetActive(false);menu.SetActive(false);characterTray.SetAsLastSibling();worldButton.SetAsLastSibling();}
            characterTray.gameObject.SetActive(open);stick.gameObject.SetActive(JoystickMode && !MenuOpen);familyCircle.gameObject.SetActive(!MenuOpen);
            RenderNavigation();
        }
        private void CloseNavigation()
        {
            if(worlds!=null)worlds.gameObject.SetActive(false);
            if(characterTray!=null)characterTray.gameObject.SetActive(false);
            if(familyCircle!=null)familyCircle.gameObject.SetActive(true);
        }
        private void RenderNavigation()
        {
            if(!HasWorld)return;
            var selected=ReadPlayer(Actor).avatar;
            foreach(var pair in avatarMarkers)pair.Value.gameObject.SetActive(pair.Key==selected);
            foreach(var pair in worldBadges)if(pair.Key=="garden" || pair.Key=="creek")pair.Value.text=pair.Key==CurrentArea?"You're here":"Let's play";
        }
        private void AnimateNavigation()
        {
            foreach(var character in pickerCharacters)if(character!=null && character.gameObject.activeInHierarchy)
                character.Present(Vector2.zero,"picker/"+character.CharacterId,false,applicationPaused?0:Time.unscaledDeltaTime);
        }
        private void LayoutNavigation()
        {
            if(worlds==null)return;
            worldButton.anchoredPosition=new Vector2(-safe.rect.width/2+90,343);
            familyCircle.anchoredPosition=new Vector2(-Board.rect.width/2+64,-223);
            var wide=safe.rect.width/safe.rect.height>1.85f;
            ((RectTransform)worlds.Find("Back from worlds")).anchoredPosition=new Vector2(-safe.rect.width/2+70,safe.rect.height/2-64);
            ((RectTransform)worlds.Find("World settings")).anchoredPosition=new Vector2(safe.rect.width/2-70,safe.rect.height/2-64);
            ((RectTransform)worlds.Find("World heading")).anchoredPosition=new Vector2(0,safe.rect.height/2-62);
            ((RectTransform)worlds.Find("World footer")).anchoredPosition=new Vector2(0,-safe.rect.height/2+36);
            worldViewport.anchoredPosition=new Vector2(0,-22);worldViewport.sizeDelta=new Vector2(safe.rect.width-48,safe.rect.height-162);
            var width=wide?2160:Mathf.Max(1140,worldViewport.rect.width);
            worldContent.sizeDelta=new Vector2(width,worldViewport.rect.height);
            var bubbleScale=wide?1:Mathf.Min(1,(worldViewport.rect.height-20)/730);
            for(var i=0;i<worldBubbles.Count;i++)
            {
                worldBubbles[i].localScale=Vector3.one*bubbleScale;
                worldBubbles[i].anchoredPosition=wide?new Vector2(180+i*355,(i%2==0?22:-22))
                    :new Vector2(width/6+(i%3)*width/3,((i<3?180:-180)+(i%3==1?20:-10))*bubbleScale);
            }
            worldScroll.StopMovement();worldScroll.horizontalNormalizedPosition=0;
            characterScroll.StopMovement();characterScroll.horizontalNormalizedPosition=0;
        }
    }
}
