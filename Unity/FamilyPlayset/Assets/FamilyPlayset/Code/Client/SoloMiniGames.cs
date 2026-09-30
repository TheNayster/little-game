using System;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform miniGamesButton,miniGamesMenu,miniGamesFrame,miniGamesContent;
        private Image miniGamesHidePicture;
        private Text miniGamesHideHint;
        private Button miniGamesHideChoice;
        private Text miniGamesHeading,miniGamesEmpty;
        private string miniGamesOpenedWorld;
        private readonly List<(string world,RectTransform root)> miniGamesChoices=new List<(string,RectTransform)>();
        public bool MiniGamesOpen=>miniGamesMenu!=null && miniGamesMenu.gameObject.activeSelf;
        // The house, backyard and all upstairs rooms belong to the same world.
        private string MiniGamesWorld {
            get {var p=ReadPlayer(Actor);return p.zone=="garden" || HomeRooms.Internal(p.zone)?"home":WorldLayout.Place(p);}
        }
        private string MiniGamesWorldName=>MiniGamesWorld=="home"?"Home":MiniGamesWorld=="park"?"Park":MiniGamesWorld=="creek"?"Creek":MiniGamesWorld=="beach"?"Beach":MiniGamesWorld=="daycare"?"Daycare":"Zoo";
        private Text MiniGameChoice(string world,string name,Vector2 pos,Vector2 size,Action choose,Color fill)
        {
            var label=Button(miniGamesContent,name,pos,size,()=>{
                // Recheck at the tap: an old open menu cannot travel across worlds.
                if(!Ready || TravelPending || !MiniGamesOpen || miniGamesOpenedWorld!=world || MiniGamesWorld!=world)return;
                choose();
            },fill);
            miniGamesChoices.Add((world,(RectTransform)label.transform.parent));return label;
        }
        private void FilterMiniGames()
        {
            var world=MiniGamesWorld;var count=0;
            foreach(var choice in miniGamesChoices){var show=choice.world==world;choice.root.gameObject.SetActive(show);if(show)count++;}
            miniGamesHeading.text=MiniGamesWorldName+" games";
            miniGamesEmpty.gameObject.SetActive(count==0);
            miniGamesContent.anchoredPosition=Vector2.zero;
        }

        private void BuildMiniGames()
        {
            var title=Button(safe,"Games",Vector2.zero,new Vector2(265,78),ShowMiniGames,new Color(.76f,.9f,.99f));
            miniGamesButton=(RectTransform)title.transform.parent;
            miniGamesButton.anchorMin=miniGamesButton.anchorMax=new Vector2(.5f,1);
            miniGamesButton.anchoredPosition=new Vector2(0,-60);
            // Draw the list icon from existing UI shapes, so no extra art asset is needed.
            for(var i=0;i<3;i++){
                Panel(miniGamesButton,"Game dot",new Vector2(-92,19-i*19),new Vector2(10,10),Color.white,false,true);
                Plain(miniGamesButton,"Game line",new Vector2(-64,19-i*19),new Vector2(30,7),Color.white);
            }
            title.rectTransform.anchoredPosition=new Vector2(25,0);
            title.rectTransform.sizeDelta=new Vector2(190,78);
            miniGamesMenu=Panel(safe,"Mini games menu",Vector2.zero,Vector2.zero,new Color(.08f,.2f,.29f,.55f),true).rectTransform;
            Stretch(miniGamesMenu);
            NavButton(miniGamesMenu.GetComponent<Image>(),CloseMiniGames);
            miniGamesFrame=Panel(miniGamesMenu,"Choose a game",Vector2.zero,new Vector2(820,510),new Color(1,.97f,.86f),true).rectTransform;
            // The panel owns inside taps; the backdrop dismisses only outside taps.
            NavButton(miniGamesFrame.GetComponent<Image>(),()=>{});
            miniGamesHeading=Label(miniGamesFrame,"Home games",46,new Vector2(0,190),new Vector2(660,72));miniGamesHeading.fontStyle=FontStyle.Bold;
            var viewport=Plain(miniGamesFrame,"Game list",new Vector2(0,15),new Vector2(740,300),Color.clear,true).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();
            miniGamesContent=Rect(viewport,"Available mini games",Vector2.zero,new Vector2(740,300));
            miniGamesContent.anchorMin=miniGamesContent.anchorMax=new Vector2(.5f,1);miniGamesContent.pivot=new Vector2(.5f,1);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=miniGamesContent;
            scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            miniGamesEmpty=Label(viewport,"No mini games here yet.\nExplore and play!",32,Vector2.zero,new Vector2(680,145));
            // Register each new game with its world. The balloon and
            // prototype quests keep their own direct interactions outside this menu.
            var choice=MiniGameChoice("home","Hide & seek",new Vector2(0,-52),new Vector2(700,90),ChooseHideMiniGame,new Color(.76f,.9f,.99f));
            var root=(RectTransform)choice.transform.parent;root.anchorMin=root.anchorMax=new Vector2(.5f,1);
            choice.rectTransform.anchoredPosition=new Vector2(65,15);choice.rectTransform.sizeDelta=new Vector2(480,40);choice.fontSize=30;
            miniGamesHideChoice=root.GetComponent<Button>();
            miniGamesHidePicture=HomePicture(root,"Hide and seek picture",new Vector2(-268,0),new Vector2(75,75),banditFrames[5]);
            miniGamesHideHint=Label(root,"Hide while a parent counts!",20,new Vector2(65,-23),new Vector2(480,37));
            foreach(var feeding in new[]{false,true}){
                var feed=feeding;var name=feed?"Feed fish":"Fishing";
                var label=MiniGameChoice("home",name,new Vector2(feed?180:-180,-200),new Vector2(340,180),()=>ChoosePondMiniGame(feed),feed?new Color(.83f,.91f,.65f):new Color(.75f,.9f,.99f));
                var card=(RectTransform)label.transform.parent;card.anchorMin=card.anchorMax=new Vector2(.5f,1);
                label.rectTransform.anchoredPosition=new Vector2(0,-43);label.rectTransform.sizeDelta=new Vector2(310,55);label.fontSize=32;
                PondPicture(card,name+" menu picture",new Vector2(0,25),new Vector2(175,100),feed?"feeding-icon":"fishing-icon");
            }
            AddTagChoice();
            Button(miniGamesFrame,"Back to play",new Vector2(0,-200),new Vector2(300,75),CloseMiniGames,Color.white);
            miniGamesMenu.gameObject.SetActive(false);
        }

        private void ShowMiniGames()
        {
            if(!Ready || MenuOpen || TravelPending)return;
            CloseNavigation();CancelPointers();Narration.Stop();hideApproach=-1;
            miniGamesOpenedWorld=MiniGamesWorld;FilterMiniGames();
            miniGamesMenu.gameObject.SetActive(true);miniGamesMenu.SetAsLastSibling();
            stick.gameObject.SetActive(false);
        }

        private void CloseMiniGames()
        {
            if(miniGamesMenu!=null)miniGamesMenu.gameObject.SetActive(false);
            miniGamesOpenedWorld=null;
            if(stick!=null)stick.gameObject.SetActive(JoystickMode && !MenuOpen);
        }

        private void ChooseHideMiniGame()
        {
            if(!Ready || hideSending || !HideAndSeek.Zone(ReadPlayer(Actor)))return;
            var state=HideGame;if(state==null)return;
            if(state.phase!=HidePhase.Counting && !HideAndSeek.NextRound(state))return;
            // Dismiss before submitting: only the existing server command starts
            // the round. Opening/closing the menu never enrolls or removes a player.
            CloseMiniGames();hideCardRound=state.round;
            SendHide(state.phase==HidePhase.Counting?"join":"invite");
        }

        private void PresentMiniGames(Sprite parent)
        {
            if(miniGamesButton==null)return;
            var inZone=HideAndSeek.Zone(ReadPlayer(Actor));
            miniGamesButton.gameObject.SetActive(!MenuOpen && !applicationPaused);
            if(!MiniGamesOpen)return;
            if(WorldLoading || applicationPaused || miniGamesOpenedWorld!=MiniGamesWorld){CloseMiniGames();return;}
            miniGamesFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/600);
            miniGamesHidePicture.sprite=parent;
            var counting=HideGame.phase==HidePhase.Counting;var ready=HideAndSeek.NextRound(HideGame);
            miniGamesHideChoice.interactable=inZone && !hideSending && (counting || ready);
            miniGamesHideHint.text=!inZone?"Head downstairs to play.":counting?"Go hide before zero!":ready?"Hide while a parent counts!":"A round is playing.\nChoose it when everyone is found.";
        }

        private void ResetMiniGames()
        {miniGamesButton=null;miniGamesMenu=null;miniGamesFrame=null;miniGamesContent=null;miniGamesHidePicture=null;miniGamesHideHint=null;miniGamesHideChoice=null;miniGamesHeading=null;miniGamesEmpty=null;miniGamesOpenedWorld=null;miniGamesChoices.Clear();}
    }
}
