using System;
using System.Collections;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform loadingScreen;
        private Image destinationPicture;
        private Text destinationTitle, loadingCaption;
        private GameObject loadingBack;
        private readonly List<Image> loadingDots = new List<Image>();
        private readonly List<string> travelStages = new List<string>();
        private Coroutine travelRoutine;
        private string loadingDestination, travelFailure;
        private int travelGeneration;
        public bool WorldLoading => loadingScreen!=null && loadingScreen.gameObject.activeSelf;
        public string LoadingDestination => loadingDestination??"";
        public string[] TravelStages => travelStages.ToArray();
        public string LoadingFailure => travelFailure??"";

        private void ResetTravelScreen()
        {
            travelGeneration++;
            if(travelRoutine!=null)StopCoroutine(travelRoutine);
            travelRoutine=null;loadingScreen=null;loadingDestination=null;
            loadingDots.Clear();travelStages.Clear();travelFailure=null;
        }
        private void BuildTravelScreen()
        {
            loadingScreen=Plain(safe,"World loading",Vector2.zero,Vector2.zero,new Color(.38f,.78f,.95f),true).rectTransform;Stretch(loadingScreen);
            destinationTitle=Label(loadingScreen,"",38,new Vector2(0,245),new Vector2(1040,80));destinationTitle.fontStyle=FontStyle.Bold;
            var border=Panel(loadingScreen,"Destination picture",new Vector2(0,30),Vector2.one*326,Color.white,false,true);
            var mask=Panel(border.transform,"Round destination",Vector2.zero,Vector2.one*308,Color.white,false,true);mask.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            destinationPicture=Panel(mask.transform,"Scenery",Vector2.zero,Vector2.one*308,Color.white);destinationPicture.type=Image.Type.Simple;
            var edge=Panel(border.transform,"Picture edge",Vector2.zero,Vector2.one*326,Color.white,false,true);edge.sprite=pictureRim;
            loadingCaption=Label(loadingScreen,"Getting your place ready…",25,new Vector2(0,-190),new Vector2(1000,60));
            for(var i=0;i<3;i++)loadingDots.Add(Panel(loadingScreen,"Loading dot",new Vector2((i-1)*31,-245),Vector2.one*16,Color.white,false,true));
            var back=Button(loadingScreen,"Back to my game",new Vector2(0,-300),new Vector2(280,65),()=>
            {
                if(travelRoutine!=null)return;
                loadingScreen.gameObject.SetActive(false);familyCircle.gameObject.SetActive(true);stick.gameObject.SetActive(JoystickMode);
            },Cream);
            loadingBack=back.transform.parent.gameObject;loadingBack.SetActive(false);
            loadingScreen.gameObject.SetActive(false);
        }
        private void BeginWorldTravel(string target)
        {
            if(WorldLoading || !HasWorld || !SoloWorld.KnownArea(target))return;
            CancelPointers();Narration.Stop();CloseNavigation();menu.SetActive(false);
            loadingDestination=target;travelFailure=null;travelStages.Clear();travelStages.Add("loading-screen");
            destinationPicture.sprite=Resources.Load<Sprite>("WorldMenu/"+target);
            destinationTitle.text="Off to "+(target=="creek"?"the Creek":"the Garden")+"!";
            loadingCaption.text="Getting your place ready…";loadingBack.SetActive(false);
            foreach(var dot in loadingDots)dot.gameObject.SetActive(true);
            loadingScreen.gameObject.SetActive(true);loadingScreen.SetAsLastSibling();familyCircle.gameObject.SetActive(false);stick.gameObject.SetActive(false);
            lastLocalAction=Time.realtimeSinceStartup;
            travelRoutine=StartCoroutine(PrepareDestination(target,++travelGeneration));
        }
        private IEnumerator PrepareDestination(string target,int generation)
        {
            // Paint the loading screen before saving or rebuilding presentation.
            // There is no cosmetic wait or pretend percentage: readiness, command
            // acknowledgement and save success determine when play resumes.
            yield return null;
            var deadline=Time.realtimeSinceStartup+15;
            while(dragging!=null || shared!=null && shared.Busy)
            {
                if(!CanKeepLoading(deadline,generation))yield break;
                yield return null;
            }
            if(!CanKeepLoading(deadline,generation))yield break;
            travelStages.Add("gestures-settled");
            if(shared==null)
            {
                if(!TrySaveNow()){FailTravel("Your play couldn't be saved yet. Please ask a grown-up.");yield break;}
                travelStages.Add("departure-saved");
            }
            Travel(target);travelStages.Add("travel-requested");
            while(TravelPending || shared!=null && shared.Busy || CurrentArea!=target)
            {
                if(!CanKeepLoading(deadline,generation))yield break;
                yield return null;
            }
            if(!CanKeepLoading(deadline,generation))yield break;
            travelStages.Add(shared==null?"local-arrival":"authority-confirmed");
            if(shared==null)
            {
                if(!TrySaveNow()){FailTravel("Your new place couldn't be saved yet. Please ask a grown-up.");yield break;}
                travelStages.Add("arrival-saved");
            }
            // Both prototype areas already have installed views. Future scenic
            // loaders must finish here before this final presentation barrier.
            Render();Canvas.ForceUpdateCanvases();yield return null;
            if(!CanKeepLoading(deadline,generation))yield break;
            travelStages.Add("destination-presented");
            loadingScreen.gameObject.SetActive(false);familyCircle.gameObject.SetActive(true);stick.gameObject.SetActive(JoystickMode);
            travelStages.Add("ready");travelRoutine=null;
        }
        private bool CanKeepLoading(float deadline,int generation)
        {
            if(generation!=travelGeneration)return false;
            if(!string.IsNullOrEmpty(travelFailure)){FailTravel("That trip didn't finish. Please try again from your game.");return false;}
            if(shared!=null && !shared.Connected){FailTravel("The connection stopped. Your game is still here.");return false;}
            if(Time.realtimeSinceStartup>deadline){FailTravel("This place is taking longer to get ready. Please try again from your game.");return false;}
            return true;
        }
        private void FailTravel(string explanation)
        {
            // Never resubmit an uncertain command with a new request ID. The
            // normal session recovery owns disconnect/reconciliation decisions.
            travelFailure=explanation;loadingCaption.text=explanation;
            foreach(var dot in loadingDots)dot.gameObject.SetActive(false);
            loadingBack.SetActive(true);travelStages.Add("failed");travelRoutine=null;
        }
        private void AnimateTravelScreen()
        {
            if(!WorldLoading || travelRoutine==null)return;
            for(var i=0;i<loadingDots.Count;i++)loadingDots[i].color=new Color(1,1,1,.4f+.6f*(.5f+.5f*Mathf.Sin(Time.unscaledTime*5-i)));
        }
    }
}
