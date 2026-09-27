using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly List<(RectTransform root,float x)> discoveryTables=new List<(RectTransform,float)>();
        private readonly List<(DiscoverySurface surface,int owner,int station)> discoveryMats=new List<(DiscoverySurface,int,int)>();
        private readonly List<Text> discoveryStationButtons=new List<Text>(),discoveryActions=new List<Text>(),discoveryColors=new List<Text>();
        private RectTransform discoveryPanel,discoveryGallery;
        private ColoringSheet coloringSheet;
        private Text discoveryChoose;
        private readonly List<Image> discoveryStationArt=new List<Image>(),discoveryCrayonArt=new List<Image>();
        private readonly List<Text> discoveryGalleryButtons=new List<Text>();
        private DiscoverySurface discoverySurface;
        private Text discoveryTitle,discoveryHint,discoverySave,discoveryBack,discoveryPrev,discoveryNext,discoveryPageLabel,discoveryUndo,discoveryRedo;
        private DiscoveryWorkspace[] discoveryCached;
        private string discoveryFeedback="";
        private float discoveryFeedbackUntil;
        private int discoveryStation,discoveryPage=6,discoveryColor=5,discoveryIntent;
        private bool discoveryPending;
        private long discoveryPaintRevision=-1;
        public bool DiscoveryOpen=>discoveryPanel!=null && discoveryPanel.gameObject.activeSelf;
        public Vector2 DiscoveryScreenPoint(float x,float y){var r=discoverySurface.rectTransform;return RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3((x/800-.5f)*r.rect.width,(.5f-y/460)*r.rect.height)));}
        private DiscoveryWorkspace[] DiscoveryWorkspaces=>Shared?shared.View.discovery:World.ReadDiscovery();
        private DiscoveryWorkspace OwnDiscovery=>DiscoveryWorkspaces.First(w=>w.owner==Actor);
        private void DiscoveryCommand(string op,float x=0,float y=0)
        {
            if(!DiscoveryOpen || discoveryPending || !Ready || ActionPending || applicationPaused)return;
            var intent=++discoveryIntent;var page=OwnDiscovery.pages[discoveryPage];discoveryPending=true;discoverySurface.CancelGesture();
            void Done(SoloResult result){if(intent!=discoveryIntent)return;discoveryPending=false;discoveryPaintRevision=-1;
                discoveryFeedback=result.Accepted?"":result.Outcome=="page-changed"?"Your page changed. Try that color again.":"That change did not finish. Try again.";discoveryFeedbackUntil=Time.unscaledTime+3;if(HasWorld)Render();}
            if(Shared)SubmitShared(SoloAction.Discovery,Actor,Discovery.PageToken(discoveryPage,page),op,x,y,Done);
            else Done(Command(SoloAction.Discovery,Actor,Discovery.PageToken(discoveryPage,page),op,x,y));
            PresentDiscovery();
        }
        private void OpenDiscovery(int station)
        {
            if(!Ready || MenuOpen || TravelPending || SceneSchema<Discovery.Schema)return;
            if(!Discovery.InBay(ReadPlayer(Actor))){RoomPlayFeedback("Walk a little closer to the activity tables.");return;}
            CancelPointers();CancelStairApproach();CancelDoorApproach();CloseNavigation();CloseKitchen();Narration.Stop();
            discoveryStation=station;discoveryPage=Math.Min(discoveryPage,OwnDiscovery.pages.Length-1);discoveryGallery.gameObject.SetActive(false);discoveryPanel.gameObject.SetActive(true);discoveryPanel.SetAsLastSibling();discoveryPaintRevision=-1;
            stick.gameObject.SetActive(false);PresentDiscovery();
        }
        private void CloseDiscovery()
        {CancelMixingGesture();discoverySurface?.CancelGesture();if(discoveryPanel!=null)discoveryPanel.gameObject.SetActive(false);CancelPointers();if(stick!=null)stick.gameObject.SetActive(JoystickMode && !MenuOpen);}
        private void BuildDiscovery()
        {
            if(SceneSchema<Discovery.Schema)return;
            var table=HomeSprite("discovery-table");var work=DiscoveryWorkspaces;
            for(var kind=0;kind<2;kind++){
                var station=kind==0?0:3;var x=kind==0?Discovery.ScienceX:Discovery.ArtX;
                var root=Rect(Board,kind==0?"Discovery science bench":"Discovery coloring table",Vector2.zero,Vector2.zero);discoveryTables.Add((root,x));
                HomePicture(root,"Workbench",new Vector2(0,152),new Vector2(1000,333),table);
                for(var i=0;i<4;i++){
                    var owner=i;var mat=Panel(root,"Workspace "+(i+1),new Vector2(-330+i*220,252),new Vector2(186,108),new Color(1,.99f,.94f));
                    var graphic=Rect(mat.transform,"Persistent workspace picture",Vector2.zero,new Vector2(170,98)).gameObject.AddComponent<DiscoverySurface>();graphic.raycastTarget=false;
                    discoveryMats.Add((graphic,owner,station));
                    Label(root,(i+1).ToString(),19,new Vector2(-330+i*220,194),new Vector2(60,26));
                }
                HomeHit(root,kind==0?"Science bench":"Coloring table",new Vector2(0,220),new Vector2(980,240),()=>OpenDiscovery(station));
                Label(root,kind==0?"Science":"Coloring",30,new Vector2(0,362),new Vector2(600,55));
            }
            discoveryPanel=Panel(safe,"Discovery activity",Vector2.zero,Vector2.zero,new Color(.98f,.97f,.91f),true).rectTransform;Stretch(discoveryPanel);
            var backdrop=HomePicture(discoveryPanel,"Illustrated workshop",Vector2.zero,Vector2.zero,WorkshopArt.Background);Stretch(backdrop.rectTransform);backdrop.raycastTarget=false;
            discoveryTitle=Label(discoveryPanel,"",30,Vector2.zero,new Vector2(550,55));
            discoveryBack=Button(discoveryPanel,"Back to Home",Vector2.zero,new Vector2(200,65),CloseDiscovery,Cream);
            for(var i=0;i<(SceneSchema>=Mixing.Schema?4:3);i++){var index=i==3?4:i;discoveryStationButtons.Add(Button(discoveryPanel,new[]{"Floating boats","Magnet materials","Colored light","Mix & discover"}[i],Vector2.zero,new Vector2(240,65),()=>{if(discoveryPending)return;CancelMixingGesture();discoverySurface.CancelGesture();discoveryStation=index;discoveryPaintRevision=-1;PresentDiscovery();},Cream));}
            for(var i=0;i<discoveryStationButtons.Count;i++){
                var img=HomePicture(discoveryStationButtons[i].transform.parent,"Activity picture",new Vector2(0,12),new Vector2(68,62),i==3?MixingSprite(10):WorkshopArt.Prop(i==0?0:i==1?2:9));img.preserveAspect=true;img.raycastTarget=false;discoveryStationArt.Add(img);
            }
            var paper=Panel(discoveryPanel,"Discovery paper",Vector2.zero,Vector2.zero,Color.white,true).rectTransform;
            discoverySurface=Rect(paper,"Discovery drawing surface",Vector2.zero,Vector2.zero).gameObject.AddComponent<DiscoverySurface>();Stretch(discoverySurface.rectTransform);
            discoverySurface.CanInteract=()=>DiscoveryOpen && !discoveryPending && !ActionPending && Ready && !applicationPaused;
            discoverySurface.FillRegion=region=>DiscoveryCommand("fill:"+region+":"+discoveryColor);
            discoverySurface.ScienceOperation=op=>DiscoveryCommand(op);
            discoverySurface.MoveMagnet=point=>DiscoveryCommand("magnet",point.x,point.y);
            coloringSheet=Rect(paper,"Official coloring sheet",Vector2.zero,Vector2.zero).gameObject.AddComponent<ColoringSheet>();Stretch(coloringSheet.rectTransform);
            coloringSheet.CanInteract=()=>DiscoveryOpen && !discoveryPending && !ActionPending && Ready && !applicationPaused;
            coloringSheet.Fill=region=>DiscoveryCommand("fill:"+region+":"+discoveryColor);coloringSheet.gameObject.SetActive(false);
            discoveryHint=Label(discoveryPanel,"",22,Vector2.zero,new Vector2(1000,50));
            discoverySave=Label(discoveryPanel,"",16,Vector2.zero,new Vector2(900,26));
            for(var i=0;i<6;i++){var index=i;discoveryActions.Add(Button(discoveryPanel,"",Vector2.zero,new Vector2(150,68),()=>DiscoveryAction(index),Cream));}
            discoveryPrev=Button(discoveryPanel,"< Page",Vector2.zero,new Vector2(165,65),()=>ChangeDiscoveryPage(-1),Cream);
            discoveryNext=Button(discoveryPanel,"Page >",Vector2.zero,new Vector2(165,65),()=>ChangeDiscoveryPage(1),Cream);
            discoveryPageLabel=Label(discoveryPanel,"",26,Vector2.zero,new Vector2(480,60));
            for(var i=0;i<DiscoverySurface.Palette.Length;i++){
                var index=i;var label=Button(discoveryPanel,"",Vector2.zero,new Vector2(58,64),()=>{discoveryColor=index;PresentDiscovery();},DiscoverySurface.Palette[i]);
                label.transform.parent.name="Discovery crayon "+new[]{"White","Red","Orange","Yellow","Green","Blue","Purple","Brown","Black"}[i];discoveryColors.Add(label);
                var crayon=HomePicture(label.transform.parent,"Crayon",new Vector2(0,8),new Vector2(52,86),WorkshopArt.Prop(11));crayon.color=DiscoverySurface.Palette[i];crayon.preserveAspect=true;crayon.raycastTarget=false;discoveryCrayonArt.Add(crayon);
            }
            discoveryUndo=Button(discoveryPanel,"Undo",Vector2.zero,new Vector2(145,64),()=>DiscoveryCommand("undo"),Cream);
            discoveryRedo=Button(discoveryPanel,"Redo",Vector2.zero,new Vector2(145,64),()=>DiscoveryCommand("redo"),Cream);
            BuildMixing();BuildColoringGallery();discoveryPanel.gameObject.SetActive(false);PresentDiscovery();
        }
        private void ChangeDiscoveryPage(int delta){if(discoveryPending)return;discoverySurface.CancelGesture();discoveryPage=(discoveryPage+delta+OwnDiscovery.pages.Length)%OwnDiscovery.pages.Length;discoveryPaintRevision=-1;PresentDiscovery();}
        private void DiscoveryAction(int index)
        {
            var operations=discoveryStation==0?new[]{"cargo-remove","cargo-add","narrow","wide","lift","reset-float"}:discoveryStation==1?new[]{"iron","wood","plastic","aluminum","reset-magnets"}:new[]{"red","green","blue","reset-lights"};
            if(index<operations.Length)DiscoveryCommand(operations[index]);
        }
        private static void DiscoveryButton(Text label,string text,Vector2 position,Vector2 size,bool enabled=true)
        {label.text=text;var r=(RectTransform)label.transform.parent;r.anchoredPosition=position;r.sizeDelta=size;label.rectTransform.sizeDelta=size;label.transform.parent.GetComponent<Button>().interactable=enabled;}
        private void BuildColoringGallery()
        {
            discoveryChoose=Button(discoveryPanel,"Choose picture",Vector2.zero,new Vector2(210,62),()=>{discoveryGallery.gameObject.SetActive(true);PresentDiscovery();},Cream);
            discoveryGallery=Panel(discoveryPanel,"Coloring picture collection",Vector2.zero,Vector2.zero,new Color(.9f,.97f,.95f),true).rectTransform;Stretch(discoveryGallery);
            var count=DiscoveryWorkspaces[0].pages.Length;
            for(var i=0;i<count;i++){
                var index=i;var label=Button(discoveryGallery,Discovery.Pages[i],Vector2.zero,new Vector2(142,170),()=>{discoveryPage=index;discoveryPaintRevision=-1;discoveryGallery.gameObject.SetActive(false);PresentDiscovery();},Color.white);label.transform.parent.name="Coloring page "+i;discoveryGalleryButtons.Add(label);
                if(i>=Discovery.LegacyPages){var item=ColoringSheet.Pages.pages[i-Discovery.LegacyPages];var texture=Resources.Load<Texture2D>("Discovery/Coloring/"+item.id+"-thumb");var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));var image=HomePicture(label.transform.parent,"Page preview",new Vector2(0,12),new Vector2(116,128),sprite);image.preserveAspect=true;image.raycastTarget=false;}
                else {var picture=Rect(label.transform.parent,"Saved page preview",new Vector2(0,12),new Vector2(124,90)).gameObject.AddComponent<DiscoverySurface>();picture.raycastTarget=false;picture.Present(OwnDiscovery,3,i);}
            }
            var close=Button(discoveryGallery,"Back to picture",Vector2.zero,new Vector2(220,60),()=>discoveryGallery.gameObject.SetActive(false),Cream);
            close.transform.parent.name="Close picture collection";
            discoveryGallery.gameObject.SetActive(false);
        }
        private void PresentDiscovery()
        {
            if(discoveryPanel==null || !HasWorld)return;
            foreach(var t in discoveryTables){t.root.gameObject.SetActive(CurrentArea=="garden");t.root.anchoredPosition=ToBoard(t.x,340);t.root.localScale=Vector3.one*sceneScale;}
            if(DiscoveryOpen && (CurrentArea!="garden" || !Discovery.InBay(ReadPlayer(Actor)) || menu.activeSelf || BookOpen || BookLibraryOpen || CharactersOpen || TravelPending || applicationPaused)){CloseDiscovery();return;}
            var revision=Shared?shared.View.revision:World.Revision;
            if(discoveryCached==null || revision!=discoveryPaintRevision)discoveryCached=DiscoveryWorkspaces;
            var work=discoveryCached;
            if(revision!=discoveryPaintRevision){foreach(var m in discoveryMats){m.surface.gameObject.SetActive(m.owner<work.Length);if(m.owner<work.Length)m.surface.Present(work[m.owner],m.station,0);}discoveryPaintRevision=revision;}
            if(!DiscoveryOpen)return;var own=work.First(w=>w.owner==Actor);var art=discoveryStation==3;var size=discoveryPanel.rect.size;var width=size.x;var height=size.y;
            discoveryTitle.text=art?"Color & play":"Explore & discover";discoveryTitle.fontStyle=FontStyle.Bold;discoveryTitle.rectTransform.anchoredPosition=new Vector2(-width/2+300,height/2-44);
            DiscoveryButton(discoveryBack,"Back to Home",new Vector2(width/2-132,height/2-44),new Vector2(220,62));
            for(var i=0;i<discoveryStationButtons.Count;i++){
                var label=discoveryStationButtons[i];label.transform.parent.gameObject.SetActive(!art);DiscoveryButton(label,new[]{"Boats","Magnets","Light","Mixing"}[i],new Vector2(-width/2+74,height/2-165-i*112),new Vector2(116,104),!discoveryPending);
                label.transform.parent.GetComponent<Image>().color=(i==3?4:i)==discoveryStation?new Color(.57f,.86f,.83f):new Color(1,1,1,.8f);
                label.fontSize=18;label.rectTransform.anchoredPosition=new Vector2(0,-33);label.rectTransform.sizeDelta=new Vector2(110,30);
            }
            var paper=(RectTransform)discoverySurface.transform.parent;
            paper.gameObject.SetActive(discoveryStation!=4);
            var official=art && discoveryPage>=Discovery.LegacyPages;
            coloringSheet.gameObject.SetActive(official);
            if(official)coloringSheet.Present(discoveryPage,own.pages[discoveryPage]);
            var aspect=official?coloringSheet.Aspect:800f/460;
            var available=Mathf.Min(height-(art?145:230),(width-(art?390:220))/aspect);
            paper.sizeDelta=new Vector2(available*aspect,available);paper.anchoredPosition=new Vector2(art?0:66,art?-20:-15);
            paper.GetComponent<Image>().color=art?Color.white:Color.clear;discoverySurface.gameObject.SetActive(!official);
            if(!official)discoverySurface.Present(own,discoveryStation,discoveryPage);
            discoveryHint.rectTransform.anchoredPosition=new Vector2(art?0:65,-height/2+83);discoveryHint.rectTransform.sizeDelta=new Vector2(width-210,52);discoveryHint.fontSize=20;
            discoveryHint.text=discoveryPending?"Saving…":discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil?discoveryFeedback:art?"Choose a crayon, then tap a shape.":discoveryStation==0?(own.outOfWater?"Put your boat back in the water to keep exploring.":Discovery.Sinks(own)?"Try less cargo or a wider boat.":"Add cargo and watch your boat sit lower."):discoveryStation==1?"Drag the magnet. What follows it?":"Your light: "+Discovery.LightNames[own.lights]+". Tap the lamps to mix colors.";
            if(art)discoveryHint.gameObject.SetActive(false);else discoveryHint.gameObject.SetActive(true);
            discoverySave.rectTransform.anchoredPosition=new Vector2(0,-height/2+14);
            discoverySave.text=Shared?(shared.Connected?"Playing with family":"Connection interrupted"):
                saveLabel!=null && saveLabel.text.StartsWith("Couldn't save")?saveLabel.text:dirty || localSave.Pending?"Saving on this device…":"Saved on this device";
            var texts=discoveryStation==0?new[]{"− Cargo","+ Cargo","Narrow boat","Wide boat",own.outOfWater?"In water":"Lift out","Reset tray"}:discoveryStation==1?new[]{"Iron","Wood","Plastic","Aluminum","Reset tray"}:new[]{"Red lamp","Green lamp","Blue lamp","Reset tray"};
            for(var i=0;i<discoveryActions.Count;i++){var label=discoveryActions[i];label.transform.parent.gameObject.SetActive(!art && discoveryStation!=4 && i<texts.Length);if(i<texts.Length){var bw=Mathf.Min(148,(width-190)/texts.Length-9);DiscoveryButton(label,texts[i],new Vector2(65+(i-(texts.Length-1)/2f)*(bw+9),-height/2+47),new Vector2(bw,55),!discoveryPending);label.fontSize=19;label.transform.parent.name="Discovery "+texts[i];}}
            discoveryPrev.transform.parent.gameObject.SetActive(art);discoveryNext.transform.parent.gameObject.SetActive(art);discoveryPageLabel.gameObject.SetActive(art);discoveryChoose.transform.parent.gameObject.SetActive(art);
            DiscoveryButton(discoveryPrev,"< Page",new Vector2(-width/2+114,-height/2+100),new Vector2(178,62),!discoveryPending);DiscoveryButton(discoveryNext,"Page >",new Vector2(width/2-114,-height/2+100),new Vector2(178,62),!discoveryPending);
            DiscoveryButton(discoveryChoose,"Choose picture",new Vector2(-width/2+124,height/2-122),new Vector2(212,62),!discoveryPending);
            discoveryPageLabel.text=Discovery.Pages[discoveryPage]+"   "+(discoveryPage+1)+" / "+own.pages.Length;discoveryPageLabel.fontSize=22;discoveryPageLabel.rectTransform.anchoredPosition=new Vector2(0,height/2-80);
            for(var i=0;i<discoveryColors.Count;i++){
                var label=discoveryColors[i];label.transform.parent.gameObject.SetActive(art);
                var col=i/5;var row=i%5;
                DiscoveryButton(label,i==discoveryColor?"●":"",new Vector2(width/2-139+col*73,height/2-160-row*89),new Vector2(66,83));
                label.fontSize=16;label.rectTransform.anchoredPosition=new Vector2(0,-33);label.color=Ink;label.transform.parent.GetComponent<Image>().color=i==discoveryColor?new Color(1,.87f,.49f,.92f):new Color(1,1,1,.65f);
                discoveryCrayonArt[i].rectTransform.sizeDelta=new Vector2(46,69);
            }
            discoveryUndo.transform.parent.gameObject.SetActive(art);discoveryRedo.transform.parent.gameObject.SetActive(art);
            DiscoveryButton(discoveryUndo,"Undo",new Vector2(-width/2+116,20),new Vector2(170,62),!discoveryPending && own.pages[discoveryPage].undo.Length>0);
            DiscoveryButton(discoveryRedo,"Redo",new Vector2(-width/2+116,-64),new Vector2(170,62),!discoveryPending && own.pages[discoveryPage].redo.Length>0);
            if(discoveryStation==4)discoveryHint.rectTransform.anchoredPosition=new Vector2(65,-height/2+40);
            PresentMixing(own,size);
            if(discoveryGallery.gameObject.activeSelf){
                discoveryGallery.SetAsLastSibling();var w=Mathf.Min(152,(width-80)/6);var h=Mathf.Min(190,(height-110)/3);
                for(var i=0;i<discoveryGalleryButtons.Count;i++){var label=discoveryGalleryButtons[i];DiscoveryButton(label,Discovery.Pages[i],new Vector2((i%6-2.5f)*w,height/2-88-h/2-i/6*h),new Vector2(w-12,h-12));label.fontSize=16;label.rectTransform.sizeDelta=new Vector2(w-18,35);label.rectTransform.anchoredPosition=new Vector2(0,-h/2+26);}
                var close=discoveryGallery.Find("Close picture collection") as RectTransform;close.anchoredPosition=new Vector2(width/2-140,height/2-43);
            }
        }
        private void AddDiscoveryDepth(Action<RectTransform,float,int,string> add){foreach(var t in discoveryTables)add(t.root,ToBoard(t.x,340).y,0,t.root.name);}
        private void ResetDiscovery(){discoveryIntent++;discoveryPending=false;ResetMixing();discoverySurface?.CancelGesture();discoveryTables.Clear();discoveryMats.Clear();discoveryStationButtons.Clear();discoveryActions.Clear();discoveryColors.Clear();discoveryGalleryButtons.Clear();discoveryStationArt.Clear();discoveryCrayonArt.Clear();discoveryPanel=null;discoveryGallery=null;discoverySurface=null;discoveryCached=null;discoveryPaintRevision=-1;}
    }
}
