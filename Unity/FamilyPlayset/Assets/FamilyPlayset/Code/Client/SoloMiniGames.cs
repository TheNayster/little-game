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
        private RectTransform miniGamesKingdom;
        private Text miniGamesKingdomHint;
        public bool MiniGamesOpen=>miniGamesMenu!=null && miniGamesMenu.gameObject.activeSelf;

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
            Label(miniGamesFrame,"Let's play!",46,new Vector2(0,190),new Vector2(660,72)).fontStyle=FontStyle.Bold;
            var viewport=Plain(miniGamesFrame,"Game list",new Vector2(0,15),new Vector2(740,300),Color.clear,true).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();
            miniGamesContent=Rect(viewport,"Available mini games",Vector2.zero,new Vector2(740,300));
            miniGamesContent.anchorMin=miniGamesContent.anchorMax=new Vector2(.5f,1);miniGamesContent.pivot=new Vector2(.5f,1);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=miniGamesContent;
            scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            // Register additional area games here when they exist. The balloon and
            // prototype quests keep their own direct interactions outside this menu.
            var choice=Button(miniGamesContent,"Hide & seek",new Vector2(0,-145),new Vector2(700,250),ChooseHideMiniGame,new Color(.76f,.9f,.99f));
            var root=(RectTransform)choice.transform.parent;root.anchorMin=root.anchorMax=new Vector2(.5f,1);
            choice.rectTransform.anchoredPosition=new Vector2(120,45);choice.rectTransform.sizeDelta=new Vector2(405,70);choice.fontSize=40;
            miniGamesHideChoice=root.GetComponent<Button>();
            miniGamesHidePicture=HomePicture(root,"Hide and seek picture",new Vector2(-220,0),new Vector2(210,210),banditFrames[5]);
            miniGamesHideHint=Label(root,"Hide while a parent counts!",26,new Vector2(120,-45),new Vector2(405,100));
            var adventure=Button(miniGamesContent,"The Adventure",new Vector2(0,-145),new Vector2(700,250),ChooseKingdom,new Color(.86f,.82f,.98f));
            miniGamesKingdom=(RectTransform)adventure.transform.parent;miniGamesKingdom.anchorMin=miniGamesKingdom.anchorMax=new Vector2(.5f,1);
            adventure.rectTransform.anchoredPosition=new Vector2(105,45);adventure.rectTransform.sizeDelta=new Vector2(435,70);adventure.fontSize=38;
            var portrait=Rect(miniGamesKingdom,"Chloe adventure picture",new Vector2(-215,-45),new Vector2(180,210));var chloe=portrait.gameObject.AddComponent<GameCharacterVisual>();chloe.Select("chloe");chloe.PresentFrame(new CharacterFrame(CharacterPose.Wave,0,false),0);portrait.localScale=Vector3.one*.8f;
            miniGamesKingdomHint=Label(miniGamesKingdom,"Save the kingdom together!",26,new Vector2(105,-45),new Vector2(435,100));
            Button(miniGamesFrame,"Back to play",new Vector2(0,-200),new Vector2(300,75),CloseMiniGames,Color.white);
            miniGamesMenu.gameObject.SetActive(false);
        }

        private void ShowMiniGames()
        {
            if(!Ready || MenuOpen || TravelPending )return;
            CloseNavigation();CancelPointers();Narration.Stop();hideApproach=-1;
            miniGamesMenu.gameObject.SetActive(true);miniGamesMenu.SetAsLastSibling();
            stick.gameObject.SetActive(false);
        }

        private void CloseMiniGames()
        {
            if(miniGamesMenu!=null)miniGamesMenu.gameObject.SetActive(false);
            if(stick!=null)stick.gameObject.SetActive(JoystickMode && !MenuOpen);
        }

        private void ChooseHideMiniGame()
        {
            if(!Ready || hideSending )return;
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
            var daycare=CurrentArea=="daycare" || CurrentArea==KingdomAdventure.Zone;
            miniGamesButton.gameObject.SetActive((inZone || daycare) && !MenuOpen && !applicationPaused);
            if(!MiniGamesOpen)return;
            if((!inZone && !daycare) || WorldLoading || applicationPaused){CloseMiniGames();return;}
            miniGamesFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/900,safe.rect.height/600);
            miniGamesHidePicture.sprite=parent;
            miniGamesHideChoice.gameObject.SetActive(inZone);miniGamesKingdom.gameObject.SetActive(daycare && SceneSchema>=KingdomAdventure.Schema);
            miniGamesKingdomHint.text=KingdomGame!=null && KingdomGame.phase>KingdomPhase.Ready?"Join our adventure where it is now!":"Save the kingdom together!";
            var counting=HideGame.phase==HidePhase.Counting;var ready=HideAndSeek.NextRound(HideGame);
            miniGamesHideChoice.interactable=!hideSending && (counting || ready);
            miniGamesHideHint.text=counting?"Go hide before zero!":ready?"Hide while a parent counts!":"A round is playing.\nChoose it when everyone is found.";
        }

        private void ResetMiniGames()
        {miniGamesButton=null;miniGamesMenu=null;miniGamesFrame=null;miniGamesContent=null;miniGamesHidePicture=null;miniGamesHideHint=null;miniGamesHideChoice=null;miniGamesKingdom=null;miniGamesKingdomHint=null;}
    }
}
