using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform habitatBasket,habitatPanel,habitatBar,habitatPreview,habitatRemove;
        private readonly RectTransform[,] habitatDrawings=new RectTransform[2,3];
        private readonly Image[] habitatEditButtons=new Image[2];
        private readonly Text[] habitatUseBadges=new Text[2];
        private readonly Image[] habitatSlots=new Image[3];
        private readonly Text[] habitatSymbols=new Text[3];
        private int habitatKind,habitatSlot=-1;
        private string habitatEditing="";
        private long habitatRevision;
        private bool habitatSending,habitatPreviousManual;
        private float habitatPreviousCamera;
        private Text habitatCue;
        public bool ElephantHabitatOpen=>habitatPanel!=null && habitatPanel.gameObject.activeSelf;
        public int ElephantHabitatPreview=>ElephantHabitatOpen?habitatSlot:-1;
        private bool HabitatReady=>Ready && !applicationPaused && !TravelPending && !ActionPending && (!Shared || shared.Connected);
        private void HabitatPicture(Transform parent,ElephantPropKind kind,float scale)
        {
            var r=Rect(parent,"Prop picture "+kind,Vector2.zero,Vector2.zero);r.localScale=Vector3.one*scale;
            var wood=new Color(.66f,.47f,.3f);var green=new Color(.43f,.65f,.38f);
            if(kind==ElephantPropKind.Canopy){
                for(var i=-1;i<=1;i+=2)Plain(r,"Canopy outer post",new Vector2(i*302,195),new Vector2(14,390),wood);
                Panel(r,"Canopy shade roof",new Vector2(0,410),new Vector2(650,75),new Color(.74f,.83f,.53f),false,true);
                Plain(r,"Canopy valance",new Vector2(0,382),new Vector2(630,17),new Color(.52f,.68f,.39f));
            }else if(kind==ElephantPropKind.Scratch){
                Panel(r,"Stable post foot",new Vector2(0,8),new Vector2(88,20),wood,false,true);
                Panel(r,"Gentle scratching post",new Vector2(0,103),new Vector2(56,190),wood,false,true);
                for(var i=0;i<7;i++)Plain(r,"Soft rope wrap",new Vector2(0,50+i*18),new Vector2(60,9),new Color(.86f,.75f,.52f));
                Panel(r,"Rounded post cap",new Vector2(0,195),new Vector2(64,24),new Color(.78f,.6f,.39f),false,true);
            }else{
                Panel(r,"Enrichment stand base",new Vector2(0,8),new Vector2(80,20),wood,false,true);
                Plain(r,"Leaf holder",new Vector2(0,45),new Vector2(19,74),wood);
                for(var i=-1;i<=1;i++){
                    var leaf=Panel(r,"Fixed enrichment leaf",new Vector2(i*23,75+i%2*8),new Vector2(38,57),green,false,true);
                    leaf.rectTransform.localRotation=Quaternion.Euler(0,0,i*25);
                }
            }
        }
        private void BuildElephantHabitat()
        {
            habitatBasket=ZooObject("Elephant decoration basket");
            Panel(habitatBasket,"Decoration basket weave",new Vector2(0,42),new Vector2(115,73),new Color(.78f,.59f,.35f),false,true);
            var open=Panel(habitatBasket,"Decorate elephant habitat",new Vector2(0,120),Vector2.one*138,Cream,true,true);
            HabitatPicture(open.transform,ElephantPropKind.Leaves,.6f);ZooArrow(open.transform,new Vector2(42,-32),1);
            NavButton(open,()=>{if(HabitatReady && !MenuOpen)OpenHabitat(0,"");});
            for(var n=0;n<2;n++)for(var k=0;k<3;k++){
                var index=n;var kind=k;var r=ZooObject("Habitat prop "+n+" "+k);habitatDrawings[n,k]=r;HabitatPicture(r,(ElephantPropKind)k,1);
                HomeHit(r,"Edit habitat prop "+n+" "+k,new Vector2(0,k==0?300:100),new Vector2(k==0?170:130,k==0?170:220),()=>{
                    if(!HabitatReady || MenuOpen)return;var p=Zoo.habitat.props.ElementAtOrDefault(index);if(p!=null && (int)p.kind==kind)OpenHabitat(kind,p.id);
                });
            }
            for(var n=0;n<2;n++){
                var r=ZooObject("Habitat in-use picture "+n);habitatUseBadges[n]=Label(r,"",34,new Vector2(0,230),new Vector2(60,50));
                var clock=Panel(r,"In use clock",new Vector2(0,230),Vector2.one*54,Cream,false,true);
                Plain(clock.transform,"Clock upright hand",new Vector2(0,9),new Vector2(5,20),Ink);
                var hand=Plain(clock.transform,"Clock short hand",new Vector2(7,3),new Vector2(17,5),Ink);hand.rectTransform.localRotation=Quaternion.Euler(0,0,-30);
            }
            habitatPanel=Plain(safe,"Habitat local preview shield",Vector2.zero,Vector2.zero,new Color(.1f,.2f,.15f,.08f),true).rectTransform;Stretch(habitatPanel);
            habitatBar=Panel(habitatPanel,"Decoration picture tray",Vector2.zero,new Vector2(680,140),Cream,false,true).rectTransform;
            for(var k=0;k<3;k++){
                var kind=k;var b=Panel(habitatBar,"Choose habitat "+k,new Vector2(-225+k*135,0),Vector2.one*112,new Color(.86f,.91f,.76f),true,true);
                var picture=Rect(b.transform,"Choice art",new Vector2(0,-37),Vector2.zero);HabitatPicture(picture,(ElephantPropKind)k,k==0?.14f:.4f);
                NavButton(b,()=>{if(HabitatReady && !habitatSending){habitatKind=kind;habitatEditing="";habitatRevision=Zoo.habitat.nextId;habitatSlot=-1;RefreshHabitatPreview();}});
            }
            var cancel=Panel(habitatBar,"Cancel habitat preview",new Vector2(295,0),Vector2.one*90,new Color(.86f,.79f,.66f),true,true);Label(cancel.transform,"\u00d7",52,Vector2.zero,Vector2.one*80);NavButton(cancel,CloseHabitat);
            var confirm=Panel(habitatBar,"Confirm habitat placement",new Vector2(167,0),Vector2.one*98,new Color(.65f,.83f,.67f),true,true);var checkLeft=Plain(confirm.transform,"Place check short stroke",new Vector2(-13,-7),new Vector2(26,10),Ink);checkLeft.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
            var checkRight=Plain(confirm.transform,"Place check long stroke",new Vector2(9,3),new Vector2(46,10),Ink);checkRight.rectTransform.localRotation=Quaternion.Euler(0,0,45);NavButton(confirm,()=>CommitHabitat(false));
            habitatRemove=Panel(habitatPanel,"Return habitat prop to basket",Vector2.zero,new Vector2(135,88),Cream,true,true).rectTransform;
            Label(habitatRemove,"\u21a9",44,Vector2.zero,new Vector2(110,75));NavButton(habitatRemove.GetComponent<Image>(),()=>CommitHabitat(true));
            habitatCue=Label(habitatPanel,"Choose a picture, then a footprint",21,Vector2.zero,new Vector2(620,45));
            for(var i=0;i<3;i++){
                var slot=i;var b=Panel(habitatPanel,"Habitat slot "+i,Vector2.zero,Vector2.one*108,new Color(.65f,.83f,.67f,.85f),true,true);habitatSlots[i]=b;
                habitatSymbols[i]=Label(b.transform,"\u25c7",45,Vector2.zero,Vector2.one*90);
                NavButton(b,()=>{if(!HabitatReady || habitatSending)return;
                    if(!ElephantHabitat.Fits(Zoo.habitat,(ElephantPropKind)habitatKind,slot,habitatEditing)){habitatCue.text="This spot needs a little space. Try a footprint.";return;}
                    habitatSlot=slot;RefreshHabitatPreview();});
            }
            for(var n=0;n<2;n++){
                var index=n;var b=Panel(habitatPanel,"Move habitat prop "+n,new Vector2(-85+n*170,150),Vector2.one*105,Cream,true,true);habitatEditButtons[n]=b;
                for(var k=0;k<3;k++) {var pic=Rect(b.transform,"Existing kind "+k,new Vector2(-13,-30),Vector2.zero);HabitatPicture(pic,(ElephantPropKind)k,k==0?.13f:.35f);}
                ZooArrow(b.transform,new Vector2(34,-30),1);
                NavButton(b,()=>{var p=Zoo.habitat.props.ElementAtOrDefault(index);if(p!=null && HabitatReady && !habitatSending)OpenHabitat((int)p.kind,p.id);});
            }
            habitatPanel.gameObject.SetActive(false);
        }
        private void OpenHabitat(int kind,string id)
        {
            var prop=Zoo.habitat.props.FirstOrDefault(p=>p.id==id);
            if(prop!=null && prop.creator!=Actor){message.text="This is your friend's decoration. Their picture can move it.";return;}
            if(!ElephantHabitatOpen){habitatPreviousCamera=cameraX;habitatPreviousManual=manualCamera;}
            CancelPointers();destination=null;zooApproach=false;shared?.Walk(WalkMode.Stop);manualCamera=true;cameraX=1260;
            habitatKind=kind;habitatEditing=id;habitatRevision=prop?.revision??Zoo.habitat.nextId;habitatSlot=-1;
            habitatCue.text="Choose a picture, then a footprint";habitatPanel.gameObject.SetActive(true);habitatPanel.SetAsLastSibling();RefreshHabitatPreview();
        }
        private void RefreshHabitatPreview()
        {
            if(habitatPreview!=null)Destroy(habitatPreview.gameObject);habitatPreview=null;if(habitatSlot<0)return;
            habitatPreview=Rect(habitatPanel,"Uncommitted habitat preview",Vector2.zero,Vector2.zero);HabitatPicture(habitatPreview,(ElephantPropKind)habitatKind,1);
            // No input surface, physics, reservation or server state.
            foreach(var image in habitatPreview.GetComponentsInChildren<Image>()){var c=image.color;c.a=.45f;image.color=c;image.raycastTarget=false;}
            foreach(var slot in habitatSlots)slot.transform.SetAsLastSibling();habitatBar.SetAsLastSibling();
        }
        private void CloseHabitat(){if(habitatSending)return;if(ElephantHabitatOpen){habitatPanel.gameObject.SetActive(false);cameraX=habitatPreviousCamera;manualCamera=habitatPreviousManual;}habitatSlot=-1;if(habitatPreview!=null)Destroy(habitatPreview.gameObject);habitatPreview=null;}
        private void CommitHabitat(bool remove)
        {
            if(!ElephantHabitatOpen || !HabitatReady || habitatSending || remove && habitatEditing=="" || !remove && habitatSlot<0)return;
            var op=remove?"habitat-remove":habitatEditing==""?"habitat-place":"habitat-move";
            var item=(habitatEditing==""?habitatKind.ToString():habitatEditing)+"/"+habitatRevision;habitatSending=true;
            void Done(SoloResult r){habitatSending=false;if(r.Accepted)CloseHabitat();else{
                    habitatCue.text=r.Outcome=="habitat-using"?"\u25f7 Elephant is using it. Try again after the gentle pause.":r.Outcome=="habitat-friend"?"Your friend's picture can move this decoration.":"The habitat changed. Choose a footprint again.";
                    habitatSlot=-1;var p=Zoo.habitat.props.FirstOrDefault(v=>v.id==habitatEditing);habitatRevision=p?.revision??Zoo.habitat.nextId;RefreshHabitatPreview();}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Zoo,item,(remove?-1:habitatSlot).ToString(),op,0,0,Done))habitatSending=false;}
            else Done(Command(SoloAction.Zoo,item:item,target:(remove?-1:habitatSlot).ToString(),value:op));
        }
        private void TickElephantHabitat(bool shown)
        {
            habitatBasket.gameObject.SetActive(shown);habitatBasket.anchoredPosition=ToBoard(ElephantHabitat.BasketX,ElephantHabitat.BasketY);habitatBasket.localScale=Vector3.one*sceneScale;var h=Zoo.habitat;
            for(var n=0;n<2;n++)for(var k=0;k<3;k++){
                var r=habitatDrawings[n,k];var p=h.props.ElementAtOrDefault(n);r.gameObject.SetActive(shown && p!=null && (int)p.kind==k);
                if(p!=null){r.anchoredPosition=ToBoard(ElephantHabitat.Slots[p.slot],ElephantHabitat.GroundY(p.kind));r.localScale=Vector3.one*sceneScale;}
            }
            for(var n=0;n<2;n++){
                var p=h.props.ElementAtOrDefault(n);var r=(RectTransform)habitatUseBadges[n].transform.parent;
                r.gameObject.SetActive(shown && p!=null && h.usingId==p.id && ElephantHabitat.AnimalPhase(Zoo.animals[0].phase));
                if(p!=null){r.anchoredPosition=ToBoard(ElephantHabitat.Slots[p.slot],ElephantHabitat.GroundY(p.kind));r.localScale=Vector3.one*sceneScale;}
            }
            if(!ElephantHabitatOpen)return;
            for(var n=0;n<2;n++){
                var p=h.props.ElementAtOrDefault(n);var b=habitatEditButtons[n];b.gameObject.SetActive(p!=null);b.rectTransform.anchoredPosition=new Vector2(-85+n*170,safe.rect.height/2-182);
                if(p!=null)for(var k=0;k<3;k++)b.transform.Find("Existing kind "+k).gameObject.SetActive(k==(int)p.kind);
            }
            if(CurrentArea!=ZooLayout.Savanna || Shared && !shared.Connected || applicationPaused){habitatSending=false;CloseHabitat();return;}
            habitatBar.anchoredPosition=new Vector2(0,-safe.rect.height/2+85);habitatBar.localScale=Vector3.one*Mathf.Min(1,(safe.rect.width-16)/680);
            habitatCue.rectTransform.anchoredPosition=new Vector2(0,safe.rect.height/2-110);habitatCue.rectTransform.localScale=Vector3.one*Mathf.Min(1,(safe.rect.width-16)/620);
            habitatRemove.gameObject.SetActive(habitatEditing!="");habitatRemove.anchoredPosition=new Vector2(safe.rect.width/2-85,safe.rect.height/2-165);
            for(var i=0;i<3;i++){
                var valid=ElephantHabitat.Fits(h,(ElephantPropKind)habitatKind,i,habitatEditing);var r=habitatSlots[i].rectTransform;
                r.position=Board.TransformPoint(ToBoard(ElephantHabitat.Slots[i],ElephantHabitat.GroundY((ElephantPropKind)habitatKind))+new Vector2(0,45)*sceneScale);r.localScale=Vector3.one*Mathf.Max(.75f,sceneScale);
                habitatSlots[i].color=valid?new Color(.65f,.83f,.67f,.85f):new Color(.81f,.75f,.67f,.9f);habitatSymbols[i].text=valid?(i==habitatSlot?"\u2713":"\u25c7"):"\u00d7";
            }
            if(habitatPreview!=null){habitatPreview.position=Board.TransformPoint(ToBoard(ElephantHabitat.Slots[habitatSlot],ElephantHabitat.GroundY((ElephantPropKind)habitatKind)));habitatPreview.localScale=Vector3.one*sceneScale;}
            if(habitatEditing!="" && h.usingId==habitatEditing && ElephantHabitat.AnimalPhase(Zoo.animals[0].phase))habitatCue.text="\u25f7 Elephant is using it. Wait for the gentle pause.";
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseHabitat();
        }
        private void ResetElephantHabitat(){if(habitatPanel!=null)Destroy(habitatPanel.gameObject);habitatPanel=null;habitatPreview=null;habitatSending=false;habitatSlot=-1;}
    }
}
