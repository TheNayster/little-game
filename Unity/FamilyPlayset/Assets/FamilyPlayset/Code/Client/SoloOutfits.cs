using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform outfitWindow,outfitFrame,roarButton;
        private GameCharacterVisual outfitPreview;
        private Text outfitName;
        private readonly Dictionary<string,Image> outfitChoices=new Dictionary<string,Image>();
        private readonly Dictionary<string,Image> outfitColors=new Dictionary<string,Image>();
        private bool outfitSending;
        private float nextRoar;
        public bool OutfitsOpen=>outfitWindow!=null && outfitWindow.gameObject.activeSelf;
        private void BuildOutfits()
        {
            var roar=Button(safe,"Roar!",Vector2.zero,new Vector2(175,82),Roar,new Color(1,.85f,.43f));
            roarButton=(RectTransform)roar.transform.parent;
            roarButton.anchorMin=roarButton.anchorMax=new Vector2(1,0);roarButton.anchoredPosition=new Vector2(-90,218);
            roarButton.gameObject.SetActive(false);
            outfitWindow=Plain(safe,"Outfit window",Vector2.zero,Vector2.zero,new Color(.08f,.2f,.29f,.58f),true).rectTransform;
            Stretch(outfitWindow);NavButton(outfitWindow.GetComponent<Image>(),CloseOutfits);
            outfitFrame=Panel(outfitWindow,"Choose an outfit",Vector2.zero,new Vector2(880,590),new Color(1,.97f,.87f),true).rectTransform;
            NavButton(outfitFrame.GetComponent<Image>(),()=>{});
            Label(outfitFrame,"Outfits",42,new Vector2(0,237),new Vector2(650,60)).fontStyle=FontStyle.Bold;
            outfitName=Label(outfitFrame,"",25,new Vector2(258,170),new Vector2(250,45));
            var previewRoot=Rect(outfitFrame,"Outfit preview",new Vector2(255,-90),Vector2.zero);
            outfitPreview=previewRoot.gameObject.AddComponent<GameCharacterVisual>();
            var viewport=Plain(outfitFrame,"Outfit choices",new Vector2(-135,104),new Vector2(510,155),Color.clear,true).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect(viewport,"Available outfits",Vector2.zero,new Vector2(CharacterOutfits.All.Count*235,155));
            content.anchorMin=content.anchorMax=new Vector2(0,.5f);content.pivot=new Vector2(0,.5f);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;
            scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            for(var i=0;i<CharacterOutfits.All.Count;i++)
            {
                var entry=CharacterOutfits.All[i];
                var label=Button(content,entry.Name,new Vector2(117+i*235,0),new Vector2(215,130),()=>ChooseOutfit(entry.Id),new Color(.77f,.9f,.96f));
                var root=(RectTransform)label.transform.parent;root.anchorMin=root.anchorMax=new Vector2(0,.5f);
                outfitChoices.Add(entry.Id,root.GetComponent<Image>());
            }
            Label(outfitFrame,"Color",27,new Vector2(-130,-7),new Vector2(500,40)).fontStyle=FontStyle.Bold;
            for(var i=0;i<CharacterOutfits.Colors.Count;i++)
            {
                var id=CharacterOutfits.Colors[i];var name=char.ToUpperInvariant(id[0])+id.Substring(1);
                var color=Button(outfitFrame,name,new Vector2(-325+i*122,-83),new Vector2(105,92),()=>ChooseOutfitColor(id),CharacterOutfitPalette.Cloth(id));
                color.fontSize=23;outfitColors.Add(id,color.transform.parent.GetComponent<Image>());
            }
            Label(outfitFrame,"Choose an outfit, then a color",21,new Vector2(-130,-171),new Vector2(500,40));
            Button(outfitFrame,"Done",new Vector2(0,-239),new Vector2(235,72),CloseOutfits,new Color(.68f,.87f,.98f));
            outfitWindow.gameObject.SetActive(false);
        }
        public void ShowOutfits()
        {
            if(!Ready || ActionPending || WorldLoading || SceneSchema<CharacterOutfits.Schema || !CharacterOutfits.Available(ReadPlayer(Actor).avatar))return;
            ShowCharacters(true);CancelPointers();Narration.Stop();outfitWindow.SetAsLastSibling();outfitWindow.gameObject.SetActive(true);
            PresentOutfits();
        }
        private void CloseOutfits(){if(outfitWindow!=null)outfitWindow.gameObject.SetActive(false);}
        private void ChooseOutfit(string id)=>SetOutfit(id,ReadPlayer(Actor).outfitColor);
        private void ChooseOutfitColor(string color)=>SetOutfit(ReadPlayer(Actor).outfit,color);
        private void SetOutfit(string id,string color)
        {
            if(outfitSending || ActionPending || CharacterOutfits.Find(id)==null || !CharacterOutfits.Color(color))return;
            var p=ReadPlayer(Actor);if(p.outfit==id && p.outfitColor==color)return;
            outfitSending=true;
            if(Shared)
            {if(!SubmitShared(SoloAction.ChangeOutfit,"",color,id,0,0,_=>{outfitSending=false;Render();PresentOutfits();}))outfitSending=false;}
            else {Command(SoloAction.ChangeOutfit,value:id,target:color);outfitSending=false;PresentOutfits();}
        }
        public void Roar()
        {
            if(!Ready || MenuOpen || ActionPending || Time.unscaledTime<nextRoar || !CharacterOutfits.CanRoar(ReadPlayer(Actor)))return;
            nextRoar=Time.unscaledTime+(float)CharacterOutfits.RoarSeconds;
            Command(SoloAction.Roar);
        }
        private void PresentOutfits()
        {
            if(roarButton==null || !HasWorld)return;var p=ReadPlayer(Actor);
            roarButton.gameObject.SetActive(CharacterOutfits.CanRoar(p) && !MenuOpen && !applicationPaused);
            roarButton.GetComponent<Button>().interactable=!ActionPending && !StairBusy && Time.unscaledTime>=nextRoar;
            if(!OutfitsOpen)return;
            outfitFrame.localScale=Vector3.one*Mathf.Min(safe.rect.width/930,safe.rect.height/640);
            var entry=PlayableCharacters.Find(p.avatar);outfitName.text=entry.Name;
            outfitPreview.Select(p.avatar);outfitPreview.transform.localScale=Vector3.one*(1.55f/Mathf.Max(1,entry.Scale));
            outfitPreview.Wear(p.outfit,p.outfitColor);outfitPreview.Present(Vector2.zero,"outfit-preview/"+p.avatar,false,Time.unscaledDeltaTime);
            foreach(var choice in outfitChoices)
            {choice.Value.color=choice.Key==p.outfit?new Color(1,.85f,.43f):new Color(.77f,.9f,.96f);choice.Value.GetComponent<Button>().interactable=!outfitSending && !ActionPending;}
            foreach(var choice in outfitColors)
            {
                choice.Value.GetComponent<Button>().interactable=p.outfit!="" && !outfitSending && !ActionPending;
                choice.Value.transform.GetChild(0).GetComponent<Text>().text=(choice.Key==p.outfitColor?"✓ ":"")+char.ToUpperInvariant(choice.Key[0])+choice.Key.Substring(1);
            }
        }
        private void ResetOutfits()
        {outfitWindow=null;outfitFrame=null;roarButton=null;outfitPreview=null;outfitName=null;outfitSending=false;outfitChoices.Clear();outfitColors.Clear();}
    }
}
