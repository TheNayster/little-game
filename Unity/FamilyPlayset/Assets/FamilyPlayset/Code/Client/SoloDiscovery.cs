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
        private RectTransform discoveryPanel;
        private DiscoverySurface discoverySurface;
        private Text discoveryTitle,discoveryHint,discoverySave,discoveryBack,discoveryPrev,discoveryNext,discoveryPageLabel,discoveryUndo,discoveryRedo;
        private DiscoveryWorkspace[] discoveryCached;
        private string discoveryFeedback="";
        private float discoveryFeedbackUntil;
        private int discoveryStation,discoveryPage,discoveryColor=5,discoveryIntent;
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
            discoveryStation=station;discoveryPanel.gameObject.SetActive(true);discoveryPanel.SetAsLastSibling();discoveryPaintRevision=-1;
            stick.gameObject.SetActive(false);PresentDiscovery();
        }
        private void CloseDiscovery()
        {discoverySurface?.CancelGesture();if(discoveryPanel!=null)discoveryPanel.gameObject.SetActive(false);CancelPointers();if(stick!=null)stick.gameObject.SetActive(JoystickMode && !MenuOpen);}
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
            discoveryTitle=Label(discoveryPanel,"",30,Vector2.zero,new Vector2(550,55));
            discoveryBack=Button(discoveryPanel,"Back to Home",Vector2.zero,new Vector2(200,65),CloseDiscovery,Cream);
            for(var i=0;i<3;i++){var index=i;discoveryStationButtons.Add(Button(discoveryPanel,new[]{"Floating boats","Magnet materials","Colored light"}[i],Vector2.zero,new Vector2(240,65),()=>{if(discoveryPending)return;discoverySurface.CancelGesture();discoveryStation=index;discoveryPaintRevision=-1;PresentDiscovery();},Cream));}
            var paper=Panel(discoveryPanel,"Discovery paper",Vector2.zero,Vector2.zero,Color.white,true).rectTransform;
            discoverySurface=Rect(paper,"Discovery drawing surface",Vector2.zero,Vector2.zero).gameObject.AddComponent<DiscoverySurface>();Stretch(discoverySurface.rectTransform);
            discoverySurface.CanInteract=()=>DiscoveryOpen && !discoveryPending && !ActionPending && Ready && !applicationPaused;
            discoverySurface.FillRegion=region=>DiscoveryCommand("fill:"+region+":"+discoveryColor);
            discoverySurface.MoveMagnet=point=>DiscoveryCommand("magnet",point.x,point.y);
            discoveryHint=Label(discoveryPanel,"",22,Vector2.zero,new Vector2(1000,50));
            discoverySave=Label(discoveryPanel,"",16,Vector2.zero,new Vector2(900,26));
            for(var i=0;i<6;i++){var index=i;discoveryActions.Add(Button(discoveryPanel,"",Vector2.zero,new Vector2(150,68),()=>DiscoveryAction(index),Cream));}
            discoveryPrev=Button(discoveryPanel,"< Page",Vector2.zero,new Vector2(165,65),()=>ChangeDiscoveryPage(-1),Cream);
            discoveryNext=Button(discoveryPanel,"Page >",Vector2.zero,new Vector2(165,65),()=>ChangeDiscoveryPage(1),Cream);
            discoveryPageLabel=Label(discoveryPanel,"",26,Vector2.zero,new Vector2(480,60));
            for(var i=0;i<DiscoverySurface.Palette.Length;i++){
                var index=i;var label=Button(discoveryPanel,"",Vector2.zero,new Vector2(58,64),()=>{discoveryColor=index;PresentDiscovery();},DiscoverySurface.Palette[i]);
                label.transform.parent.name="Discovery crayon "+new[]{"White","Red","Orange","Yellow","Green","Blue","Purple","Brown","Black"}[i];discoveryColors.Add(label);
            }
            discoveryUndo=Button(discoveryPanel,"Undo",Vector2.zero,new Vector2(145,64),()=>DiscoveryCommand("undo"),Cream);
            discoveryRedo=Button(discoveryPanel,"Redo",Vector2.zero,new Vector2(145,64),()=>DiscoveryCommand("redo"),Cream);
            discoveryPanel.gameObject.SetActive(false);PresentDiscovery();
        }
        private void ChangeDiscoveryPage(int delta){if(discoveryPending)return;discoverySurface.CancelGesture();discoveryPage=(discoveryPage+delta+6)%6;discoveryPaintRevision=-1;PresentDiscovery();}
        private void DiscoveryAction(int index)
        {
            var operations=discoveryStation==0?new[]{"cargo-remove","cargo-add","narrow","wide","lift","reset-float"}:discoveryStation==1?new[]{"iron","wood","plastic","aluminum","reset-magnets"}:new[]{"red","green","blue","reset-lights"};
            if(index<operations.Length)DiscoveryCommand(operations[index]);
        }
        private static void DiscoveryButton(Text label,string text,Vector2 position,Vector2 size,bool enabled=true)
        {label.text=text;var r=(RectTransform)label.transform.parent;r.anchoredPosition=position;r.sizeDelta=size;label.rectTransform.sizeDelta=size;label.transform.parent.GetComponent<Button>().interactable=enabled;}
        private void PresentDiscovery()
        {
            if(discoveryPanel==null || !HasWorld)return;
            foreach(var t in discoveryTables){t.root.gameObject.SetActive(CurrentArea=="garden");t.root.anchoredPosition=ToBoard(t.x,340);t.root.localScale=Vector3.one*sceneScale;}
            if(DiscoveryOpen && (CurrentArea!="garden" || !Discovery.InBay(ReadPlayer(Actor)) || menu.activeSelf || BookOpen || BookLibraryOpen || CharactersOpen || TravelPending || applicationPaused)){CloseDiscovery();return;}
            var revision=Shared?shared.View.revision:World.Revision;
            if(discoveryCached==null || revision!=discoveryPaintRevision)discoveryCached=DiscoveryWorkspaces;
            var work=discoveryCached;
            if(revision!=discoveryPaintRevision){foreach(var m in discoveryMats){m.surface.gameObject.SetActive(m.owner<work.Length);if(m.owner<work.Length)m.surface.Present(work[m.owner],m.station,0);}discoveryPaintRevision=revision;}
            if(!DiscoveryOpen)return;var own=work.First(w=>w.owner==Actor);var art=discoveryStation==3;var size=discoveryPanel.rect.size;var width=size.x;var height=size.y;var available=Mathf.Min(height-350,(width-70)*460/800);
            discoveryTitle.text=art?"Your coloring book":"Your science tray";discoveryTitle.rectTransform.anchoredPosition=new Vector2(-width/2+310,height/2-55);
            DiscoveryButton(discoveryBack,"Back to Home",new Vector2(width/2-135,height/2-55),new Vector2(220,65));
            for(var i=0;i<3;i++){var label=discoveryStationButtons[i];label.transform.parent.gameObject.SetActive(!art);DiscoveryButton(label,label.text,new Vector2((i-1)*265,height/2-137),new Vector2(250,65),!discoveryPending);label.transform.parent.GetComponent<Image>().color=i==discoveryStation?new Color(.66f,.87f,.82f):Cream;}
            var paper=(RectTransform)discoverySurface.transform.parent;paper.sizeDelta=new Vector2(available*800/460,available);paper.anchoredPosition=new Vector2(0,-10);
            paper.GetComponent<Image>().color=art?Color.white:new Color(.9f,.96f,.94f);discoverySurface.Present(own,discoveryStation,discoveryPage);
            discoveryHint.rectTransform.anchoredPosition=new Vector2(0,-height/2+140);discoveryHint.rectTransform.sizeDelta=new Vector2(width-80,60);
            discoveryHint.text=discoveryPending?"Saving…":discoveryFeedback!="" && Time.unscaledTime<discoveryFeedbackUntil?discoveryFeedback:art?"Choose a crayon, then tap a shape.":discoveryStation==0?(own.outOfWater?"Your boat is on the tray. Put it back whenever you like.":Discovery.Sinks(own)?"This boat sank. Try less cargo or a wider boat.":"Your boat floats. Add cargo and watch it sit lower."):discoveryStation==1?"Drag the magnet or tap a material. Iron follows; the others stay.":"Red + green makes yellow light. Your screen: "+Discovery.LightNames[own.lights]+".";
            discoverySave.rectTransform.anchoredPosition=new Vector2(0,-height/2+15);
            discoverySave.text=Shared?(shared.Connected?"Playing with family":"Connection interrupted"):
                saveLabel!=null && saveLabel.text.StartsWith("Couldn't save")?saveLabel.text:dirty || localSave.Pending?"Saving on this device…":"Saved on this device";
            var texts=discoveryStation==0?new[]{"− Cargo","+ Cargo","Narrow boat","Wide boat",own.outOfWater?"In water":"Lift out","Reset tray"}:discoveryStation==1?new[]{"Iron","Wood","Plastic","Aluminum","Reset tray"}:new[]{"Red lamp","Green lamp","Blue lamp","Reset tray"};
            for(var i=0;i<discoveryActions.Count;i++){var label=discoveryActions[i];label.transform.parent.gameObject.SetActive(!art && i<texts.Length);if(i<texts.Length){var bwidth=Mathf.Min(160,(width-70)/texts.Length-9);DiscoveryButton(label,texts[i],new Vector2((i-(texts.Length-1)/2f)*(bwidth+9),-height/2+63),new Vector2(bwidth,68),!discoveryPending);label.transform.parent.name="Discovery "+texts[i];}}
            discoveryPrev.transform.parent.gameObject.SetActive(art);discoveryNext.transform.parent.gameObject.SetActive(art);discoveryPageLabel.gameObject.SetActive(art);
            DiscoveryButton(discoveryPrev,"< Page",new Vector2(-width/2+132,height/2-137),new Vector2(200,65),!discoveryPending);DiscoveryButton(discoveryNext,"Page >",new Vector2(width/2-132,height/2-137),new Vector2(200,65),!discoveryPending);
            discoveryPageLabel.text=Discovery.Pages[discoveryPage]+"  "+(discoveryPage+1)+" / 6";discoveryPageLabel.rectTransform.anchoredPosition=new Vector2(0,height/2-137);
            for(var i=0;i<discoveryColors.Count;i++){var label=discoveryColors[i];label.transform.parent.gameObject.SetActive(art);DiscoveryButton(label,i==discoveryColor?"✓":"",new Vector2((i-4)*65,-height/2+63),new Vector2(58,64));label.color=i==8?Color.white:Ink;}
            discoveryUndo.transform.parent.gameObject.SetActive(art);discoveryRedo.transform.parent.gameObject.SetActive(art);
            DiscoveryButton(discoveryUndo,"Undo",new Vector2(-width/2+110,-height/2+63),new Vector2(145,64),!discoveryPending && own.pages[discoveryPage].undo.Length>0);
            DiscoveryButton(discoveryRedo,"Redo",new Vector2(width/2-110,-height/2+63),new Vector2(145,64),!discoveryPending && own.pages[discoveryPage].redo.Length>0);
        }
        private void AddDiscoveryDepth(Action<RectTransform,float,int,string> add){foreach(var t in discoveryTables)add(t.root,ToBoard(t.x,340).y,0,t.root.name);}
        private void ResetDiscovery(){discoveryIntent++;discoveryPending=false;discoverySurface?.CancelGesture();discoveryTables.Clear();discoveryMats.Clear();discoveryStationButtons.Clear();discoveryActions.Clear();discoveryColors.Clear();discoveryPanel=null;discoverySurface=null;discoveryCached=null;discoveryPaintRevision=-1;}
    }
}
