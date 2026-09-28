using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private int coloringCollectionPage;
        private Text coloringCollectionPrevious,coloringCollectionNext,coloringCollectionTitle,coloringCollectionClose;
        private ReaderPictureControl coloringFooter;
        private readonly System.Collections.Generic.List<ReaderPictureControl> coloringSelections=new System.Collections.Generic.List<ReaderPictureControl>();

        // Keep the existing command buttons and stable input names. Only their
        // presentation changes; page revisions, ownership and history stay authoritative.
        private void ColorControl(Text text,string icon)
        {
            var root=(RectTransform)text.transform.parent;
            root.GetComponent<Image>().color=Color.clear;
            var card=ReaderDrawing(root,"Coloring card",Vector2.zero,root.sizeDelta,"card",new Color(1,.985f,.94f,.93f));
            card.transform.SetAsFirstSibling();Stretch(card.rectTransform);root.GetComponent<Button>().targetGraphic=card;
            ReaderDrawing(root,"Coloring icon",new Vector2(-root.sizeDelta.x/2+32,0),new Vector2(42,42),icon,Color.white);
            text.color=Ink;text.fontStyle=FontStyle.Bold;text.resizeTextForBestFit=true;text.resizeTextMinSize=18;text.resizeTextMaxSize=23;
        }
        private void LayoutColorControl(Text text,string label,Vector2 position,Vector2 size,bool enabled=true)
        {
            DiscoveryButton(text,label,position,size,enabled);
            var root=(RectTransform)text.transform.parent;
            var icon=(RectTransform)root.Find("Coloring icon");icon.anchoredPosition=new Vector2(-size.x/2+30,0);
            text.rectTransform.anchoredPosition=new Vector2(24,0);text.rectTransform.sizeDelta=new Vector2(size.x-64,size.y-8);
            text.color=enabled?Ink:new Color(.42f,.49f,.48f);
            icon.localScale=Vector3.one*(enabled?1:.85f);
        }
        private void OpenColoringCollection()
        {
            if(discoveryPending)return;
            discoverySurface.CancelGesture();coloringCollectionPage=discoveryPage/6;
            discoveryGallery.gameObject.SetActive(true);PresentDiscovery();
        }
        private void BuildColoringControls()
        {
            ColorControl(discoveryBack,"home");ColorControl(discoveryChoose,"pictures");ColorControl(discoveryPrev,"back");ColorControl(discoveryNext,"next");
            ColorControl(discoveryUndo,"undo");ColorControl(discoveryRedo,"redo");
            coloringSelections.Clear();
            foreach(var swatch in discoveryColors)coloringSelections.Add(ReaderDrawing(swatch.transform.parent,"Selected color",new Vector2(20,24),new Vector2(22,22),"check",Color.white));
            coloringFooter=ReaderDrawing(discoveryPanel,"Coloring saved status",Vector2.zero,new Vector2(600,70),"card",new Color(1,.985f,.94f,.92f));
            coloringFooter.transform.SetSiblingIndex(discoveryHint.transform.GetSiblingIndex());
            if(SceneSchema>=HomeCreations.Schema){
                coloringKeep=Button(discoveryPanel,"Keep picture",Vector2.zero,new Vector2(190,76),KeepColoringPicture,Cream);ColorControl(coloringKeep,"check");
                coloringFolder=Button(discoveryPanel,"My pictures",Vector2.zero,new Vector2(190,76),()=>OpenCollection(false),Cream);ColorControl(coloringFolder,"pictures");
            }
            coloringCollectionTitle=Label(discoveryGallery,"Choose a picture",30,Vector2.zero,new Vector2(700,60));
            coloringCollectionPrevious=Button(discoveryGallery,"Earlier pictures",Vector2.zero,new Vector2(190,68),()=>{coloringCollectionPage=Math.Max(0,coloringCollectionPage-1);PresentDiscovery();},Cream);
            coloringCollectionNext=Button(discoveryGallery,"More pictures",Vector2.zero,new Vector2(190,68),()=>{coloringCollectionPage=Math.Min((OwnDiscovery.pages.Length-1)/6,coloringCollectionPage+1);PresentDiscovery();},Cream);
            ColorControl(coloringCollectionPrevious,"back");ColorControl(coloringCollectionNext,"next");
            coloringCollectionClose=discoveryGallery.Find("Close picture collection").GetComponentInChildren<Text>();ColorControl(coloringCollectionClose,"check");
        }
        private void PresentColoringControls(DiscoveryWorkspace own,Vector2 size)
        {
            var width=size.x;var height=size.y;var page=own.pages[discoveryPage];
            coloringFooter.rectTransform.anchoredPosition=new Vector2(0,-height/2+35);coloringFooter.rectTransform.sizeDelta=new Vector2(Mathf.Min(720,width-420),68);
            for(var i=0;i<coloringSelections.Count;i++)coloringSelections[i].gameObject.SetActive(i==discoveryColor);
            LayoutColorControl(discoveryChoose,"Pictures",new Vector2(-width/2+112,height/2-135),new Vector2(190,76),!discoveryPending);
            LayoutColorControl(discoveryUndo,"Undo",new Vector2(-width/2+112,24),new Vector2(190,76),!discoveryPending && page.undo.Length>0);
            LayoutColorControl(discoveryRedo,"Redo",new Vector2(-width/2+112,-68),new Vector2(190,76),!discoveryPending && page.redo.Length>0);
            LayoutColorControl(discoveryPrev,"Back",new Vector2(-width/2+112,-height/2+88),new Vector2(190,76),!discoveryPending && discoveryPage>0);
            LayoutColorControl(discoveryNext,"Next",new Vector2(width/2-112,-height/2+88),new Vector2(190,76),!discoveryPending && discoveryPage<own.pages.Length-1);
            discoveryPageLabel.rectTransform.sizeDelta=new Vector2(Mathf.Max(240,width-470),44);
            discoveryPageLabel.resizeTextForBestFit=true;discoveryPageLabel.resizeTextMinSize=16;discoveryPageLabel.resizeTextMaxSize=23;
            discoveryTitle.text="Color & play";
            // The hint also exposes failed/rejected saves, rather than hiding an
            // error behind a reassuring saved label in the coloring workspace.
            discoveryHint.gameObject.SetActive(true);discoveryHint.rectTransform.anchoredPosition=new Vector2(0,-height/2+49);
            discoveryHint.rectTransform.sizeDelta=new Vector2(width-440,46);discoveryHint.resizeTextForBestFit=true;discoveryHint.resizeTextMinSize=16;discoveryHint.resizeTextMaxSize=20;
            if(discoveryGallery.gameObject.activeSelf){
                discoveryGallery.SetAsLastSibling();
                coloringCollectionTitle.text="Choose a picture   "+(coloringCollectionPage+1)+" / "+((own.pages.Length+5)/6);
                coloringCollectionTitle.rectTransform.anchoredPosition=new Vector2(-90,height/2-46);
                LayoutColorControl(coloringCollectionClose,"Done",new Vector2(width/2-109,height/2-46),new Vector2(190,70));
                var w=Mathf.Min(310,(width-100)/3);var h=(height-205)/2;
                for(var i=0;i<discoveryGalleryButtons.Count;i++){
                    var label=discoveryGalleryButtons[i];var visible=i/6==coloringCollectionPage;label.transform.parent.gameObject.SetActive(visible);if(!visible)continue;
                    var index=i%6;DiscoveryButton(label,Discovery.Pages[i],new Vector2((index%3-1)*w,height/2-100-h/2-index/3*h),new Vector2(w-18,h-18),!discoveryPending);
                    label.fontSize=22;label.resizeTextForBestFit=true;label.resizeTextMinSize=16;label.resizeTextMaxSize=22;
                    label.rectTransform.sizeDelta=new Vector2(w-36,40);label.rectTransform.anchoredPosition=new Vector2(0,-h/2+32);
                    var preview=label.transform.parent.Find(i>=Discovery.LegacyPages?"Page preview":"Saved page preview") as RectTransform;
                    preview.anchoredPosition=new Vector2(0,18);preview.sizeDelta=new Vector2(w-48,h-78);
                    if(i<Discovery.LegacyPages){preview.sizeDelta=new Vector2(Mathf.Min(w-48,(h-78)*800/460),Mathf.Min(h-78,(w-48)*460/800));preview.GetComponent<DiscoverySurface>().Present(own,3,i);}
                    label.transform.parent.GetComponent<Image>().color=i==discoveryPage?new Color(.72f,.91f,.85f):Color.white;
                }
                LayoutColorControl(coloringCollectionPrevious,"Back",new Vector2(-118,-height/2+50),new Vector2(208,72),coloringCollectionPage>0);
                LayoutColorControl(coloringCollectionNext,"More pictures",new Vector2(118,-height/2+50),new Vector2(208,72),(coloringCollectionPage+1)*6<own.pages.Length);
            }
        }
    }
}
