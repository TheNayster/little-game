using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private bool sandDecorating; private string sandPlayMode="",sandDecorKind="flag",sandEditId;
        private int sandPlayEpoch=-1,sandSlot=-1,sandEditVersion,sandEditEpoch,sandToyVersion,sandToyReaction=-1;
        private Button sandPlayConfirm,sandPlayCancel,sandEdit,sandFamilyReset;
        private readonly List<Button> sandDecorCards=new List<Button>(),sandSockets=new List<Button>(),sandToySpots=new List<Button>();
        private RectTransform sandEditSheet,sandConfirmSheet,sandResetView,sandToyRoot;
        private SandShape sandAttachmentPreview;private RawImage sandToyPicture;private Texture2D sandToyTexture;
        private Text sandEditHint,sandConfirmHint,sandResetHint;private Button sandResetYes;private Action sandConfirmed;
        private float sandToyBounce;
        private readonly List<(GameCharacterVisual art,Image card)> sandVoterPictures=new List<(GameCharacterVisual,Image)>();
        private readonly List<SandShape> sandVoterChecks=new List<SandShape>();
        private SandMould SandEditPiece=>SandpitGame?.moulds.FirstOrDefault(m=>m.id==sandEditId);
        private bool SandEditing=>sandPlayMode=="move";
        private void SandPlaySend(string op,string id,string item="",float x=0,float y=0,int epoch=-1)
        {
            if(!Ready || sandpitSending || TravelPending)return;
            var round=epoch<0?SandpitGame.round:epoch;var target=DaycareSandpit.Target(id,round);
            CancelSandpitIntent();sandpitSending=true;
            void Done(SoloResult r){sandpitSending=false;if(!r.Accepted){SandCue(r.Outcome);sandPlayMode="";sandSlot=-1;}else{sandAck=0;sandPlayMode="";sandSlot=-1;}Render();}
            if(Shared){if(!SubmitShared(SoloAction.Sandpit,item,target,op,x,y,Done))sandpitSending=false;}
            else Done(Command(SoloAction.Sandpit,item:item,target:target,value:op,x:x,y:y));
        }
        private void SandTray(bool decorate)
        {CancelSandpitIntent();sandPlacing=false;sandPlayMode="";sandSlot=-1;sandDecorating=decorate;sandPlayEpoch=SandpitGame.round;sandEditSheet.gameObject.SetActive(false);TickSandpit();}
        private void SandChooseDecoration(string kind)
        {
            SandTray(true);sandDecorKind=kind;sandPlayMode="decorate";
            SuggestSandSlot();TickSandpit();
        }
        private void SuggestSandSlot()
        {
            var m=SandSelectedPiece;sandSlot=m?.built==true?Enumerable.Range(0,SandpitPlay.Slots).Where(s=>SandpitPlay.Attachment(s,sandDecorKind) && !m.attachments.Any(a=>a.slot==s)).DefaultIfEmpty(-1).First():-1;
        }
        private void SandSelectSocket(int slot){if(SandSelectedPiece?.built!=true)return;sandSlot=slot;TickSandpit();}
        private void SandConfirmDecoration()
        {
            var m=SandSelectedPiece;if(m?.built!=true || sandSlot<0 || !SandpitPlay.Attachment(sandSlot,sandDecorKind))return;
            var slot=sandSlot;var kind=sandDecorKind;var id=m.id;var epoch=SandpitGame.round;
            if(m.attachments.Any(a=>a.slot==slot)){
                if(m.creator!=Actor || string.IsNullOrEmpty(m.creator)){SandCue("not-your-sand-piece");return;}
                var version=m.version;SandAsk("Replace this decoration?",()=>SandPlaySend("decor-replace",id,version+":"+slot+":"+kind,epoch:epoch));
            }else SandPlaySend("decorate",id,slot+":"+kind,epoch:epoch);
        }
        private void SandAsk(string hint,Action yes)
        {sandConfirmed=yes;sandConfirmHint.text=hint;sandConfirmSheet.gameObject.SetActive(true);sandConfirmSheet.SetAsLastSibling();}
        private void SandOpenEdit()
        {
            var m=SandSelectedPiece;if(m==null || m.creator!=Actor || string.IsNullOrEmpty(m.creator))return;
            CancelSandpitIntent();sandPlacing=false;sandPlayMode="";sandEditId=m.id;sandEditVersion=m.version;sandEditEpoch=sandPlayEpoch=SandpitGame.round;sandSlot=-1;
            sandEditSheet.gameObject.SetActive(true);TickSandpit();
        }
        private void SandMovePiece()
        {
            var m=SandEditPiece;if(m==null)return;
            sandEditSheet.gameObject.SetActive(false);sandPlayMode="move";sandMould=m.shape;sandOrientation=m.orientation;sandPreviewEpoch=sandEditEpoch;sandPreviewCell=-1;sandPlacing=true;TickSandpit();
        }
        private void SandRemovePiece()
        {
            var id=sandEditId;var version=sandEditVersion;var epoch=sandEditEpoch;
            SandAsk("Remove just this piece?",()=>{sandEditSheet.gameObject.SetActive(false);SandPlaySend("remove",id,version.ToString(),epoch:epoch);});
        }
        private void SandRemoveAttachment()
        {
            var m=SandEditPiece;if(m==null || sandSlot<0 || !m.attachments.Any(a=>a.slot==sandSlot))return;
            var id=m.id;var item=sandEditVersion+":"+sandSlot;var epoch=sandEditEpoch;
            SandAsk("Remove just this decoration?",()=>{sandEditSheet.gameObject.SetActive(false);SandPlaySend("decor-remove",id,item,epoch:epoch);});
        }
        private WalkPoint SandToyPoint(int cell)=>cell<32?DaycareSandpit.Cell(cell%8,cell/8):new WalkPoint(4092,130+(cell-32)*110);
        private void SandChooseToy()
        {SandTray(true);sandPlayMode="toy";sandToyVersion=SandpitGame.toy.version;TickSandpit();}
        private void SandPlaceToy(int cell)
        {if(sandPlayMode!="toy")return;var at=SandToyPoint(cell);SandPlaySend("toy-place","toy",sandToyVersion.ToString(),at.X,at.Y);}
        private void BuildSandPlay()
        {
            var tab=Button(sandBuildView,"Decorate",new Vector2(-245,-234),new Vector2(220,64),()=>SandTray(true),new Color(.64f,.85f,.89f));tab.fontSize=22;
            var names=new[]{"Flag","Shell","Pebble","Door","Window"};var kinds=new[]{"flag","shell","pebble","door","window"};
            for(var i=0;i<5;i++){var kind=kinds[i];sandDecorCards.Add(SandPictureButton(sandBuildView,names[i],kind,new Vector2(-435+i*150,-318),()=>SandChooseDecoration(kind)));}
            var toy=SandPictureButton(sandBuildView,"Dinosaur","selection",new Vector2(315,-318),SandChooseToy);sandDecorCards.Add(toy);
            sandToyTexture=WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/tyrannosaurus");
            var icon=Rect(toy.transform,"Little dinosaur picture",new Vector2(0,6),new Vector2(92,70)).gameObject.AddComponent<RawImage>();icon.texture=sandToyTexture;icon.uvRect=new Rect(0,0,.25f,.5f);icon.raycastTarget=false;
            sandEdit=SandPictureButton(sandBuildView,"Edit","edit",new Vector2(325,-234),SandOpenEdit);((RectTransform)sandEdit.transform).sizeDelta=new Vector2(136,64);sandEdit.GetComponentInChildren<Text>().gameObject.SetActive(false);
            sandFamilyReset=SandPictureButton(sandBuildView,"Family reset","reset",new Vector2(510,-234),()=>SandPlaySend("reset-request","reset"));((RectTransform)sandFamilyReset.transform).sizeDelta=new Vector2(136,64);sandFamilyReset.GetComponentInChildren<Text>().gameObject.SetActive(false);
            sandPlayConfirm=SandPictureButton(sandBuildView,"Attach","confirm",new Vector2(510,-318),SandConfirmDecoration);sandPlayConfirm.name="Confirm sand decoration";
            sandPlayCancel=SandPictureButton(sandBuildView,"Cancel","cancel",new Vector2(510,-234),()=>{sandPlayMode="";sandSlot=-1;TickSandpit();});((RectTransform)sandPlayCancel.transform).sizeDelta=new Vector2(136,64);sandPlayCancel.GetComponentInChildren<Text>().gameObject.SetActive(false);sandPlayCancel.name="Cancel sand play";
            for(var s=0;s<SandpitPlay.Slots;s++){
                var slot=s;var t=Button(sandFloor,"+",Vector2.zero,new Vector2(78,70),()=>SandSelectSocket(slot),new Color(1,1,1,.6f));t.fontSize=32;t.transform.parent.name="Sand attachment "+s;sandSockets.Add(t.transform.parent.GetComponent<Button>());
            }
            sandAttachmentPreview=Rect(sandFloor,"Sand decoration preview",Vector2.zero,new Vector2(90,90)).gameObject.AddComponent<SandShape>();sandAttachmentPreview.kind="attachment";sandAttachmentPreview.raycastTarget=false;
            for(var i=0;i<36;i++){var cell=i;var at=SandToyPoint(i);var t=Button(sandFloor,"+",SandPoint(at.X,at.Y),new Vector2(i<32?120:64,96),()=>SandPlaceToy(cell),new Color(.73f,.91f,1,.6f));t.transform.parent.name="Sand toy spot "+i;sandToySpots.Add(t.transform.parent.GetComponent<Button>());}
            sandToyRoot=Rect(sandFloor,"Shared sand dinosaur",Vector2.zero,new Vector2(110,100));
            var shadow=Rect(sandToyRoot,"Toy ground contact",Vector2.zero,new Vector2(70,20)).gameObject.AddComponent<SandShape>();shadow.kind="toy-shadow";shadow.raycastTarget=false;
            sandToyPicture=Rect(sandToyRoot,"Toy dinosaur",new Vector2(0,32),new Vector2(105,105)).gameObject.AddComponent<RawImage>();sandToyPicture.texture=sandToyTexture;sandToyPicture.uvRect=new Rect(0,0,.25f,.5f);sandToyPicture.raycastTarget=false;
            HomeHit(sandToyRoot,"React sand dinosaur",new Vector2(0,35),new Vector2(110,100),()=>SandPlaySend("toy-react","toy"),false);
            sandEditSheet=Panel(sandBuildView,"Sand piece editing",Vector2.zero,new Vector2(580,400),Cream,true).rectTransform;
            sandEditHint=Label(sandEditSheet,"",24,new Vector2(0,158),new Vector2(540,54));
            for(var i=0;i<6;i++){var slot=i;var b=SandPictureButton(sandEditSheet,"Slot "+(i+1),"selection",new Vector2(-170+(i%3)*170,70-(i/3)*100),()=>{sandSlot=slot;TickSandpit();});b.name="Edit sand attachment "+i;}
            SandPictureButton(sandEditSheet,"Move","back",new Vector2(-170,-144),SandMovePiece);
            SandPictureButton(sandEditSheet,"Remove","remove",new Vector2(0,-144),()=>{if(sandSlot>=0)SandRemoveAttachment();else SandRemovePiece();});
            SandPictureButton(sandEditSheet,"Back","cancel",new Vector2(170,-144),()=>sandEditSheet.gameObject.SetActive(false));sandEditSheet.gameObject.SetActive(false);
            sandConfirmSheet=Panel(sandBuildView,"Confirm sand edit",Vector2.zero,new Vector2(600,280),Cream,true).rectTransform;
            sandConfirmHint=Label(sandConfirmSheet,"",28,new Vector2(0,84),new Vector2(570,70));
            SandPictureButton(sandConfirmSheet,"Yes, this only","confirm",new Vector2(-130,-54),()=>{var yes=sandConfirmed;sandConfirmed=null;sandConfirmSheet.gameObject.SetActive(false);yes?.Invoke();});
            SandPictureButton(sandConfirmSheet,"Keep it","cancel",new Vector2(130,-54),()=>{sandConfirmed=null;sandConfirmSheet.gameObject.SetActive(false);});sandConfirmSheet.gameObject.SetActive(false);
        }
        private void TickSandPlay(SandpitState g,bool joined)
        {
            if(sandPlayConfirm==null)return;
            if(!joined){sandPlayMode="";sandDecorating=false;sandEditSheet.gameObject.SetActive(false);sandConfirmSheet.gameObject.SetActive(false);}
            if(sandPlayEpoch!=g.round){sandPlayMode="";sandSlot=-1;sandEditSheet.gameObject.SetActive(false);sandConfirmSheet.gameObject.SetActive(false);sandConfirmed=null;}
            var m=SandSelectedPiece;var decorate=joined && sandPlayMode=="decorate" && m?.built==true;
            var toy=joined && sandPlayMode=="toy";
            foreach(var b in sandMouldButtons)b.gameObject.SetActive(!sandDecorating);
            foreach(var b in sandDecorCards){b.gameObject.SetActive(sandDecorating && !sandPlacing);b.interactable=!sandpitSending;}
            if(sandDecorating || sandPlayMode!="")foreach(var b in sandTools)b.gameObject.SetActive(false);
            sandEdit.gameObject.SetActive(joined && !sandPlacing && sandPlayMode=="");sandEdit.interactable=m!=null && !string.IsNullOrEmpty(m.creator) && m.creator==Actor;
            sandFamilyReset.gameObject.SetActive(joined && !sandPlacing && sandPlayMode=="");
            sandPlayConfirm.gameObject.SetActive(decorate);sandPlayConfirm.interactable=sandSlot>=0 && !sandpitSending && (!m.attachments.Any(a=>a.slot==sandSlot) || m.creator==Actor);
            sandPlayCancel.gameObject.SetActive(joined && !sandPlacing && sandPlayMode!="");
            for(var s=0;s<sandSockets.Count;s++){
                var b=sandSockets[s];var valid=decorate && SandpitPlay.Attachment(s,sandDecorKind);b.gameObject.SetActive(valid);
                if(valid){var p=SandpitPlay.Socket(m,s);((RectTransform)b.transform).anchoredPosition=SandPoint(m.x,m.y)+new Vector2(p.X,p.Y);b.transform.SetAsLastSibling();b.GetComponent<Image>().color=s==sandSlot?new Color(1,.74f,.24f,.7f):new Color(1,1,1,.55f);}
            }
            sandAttachmentPreview.gameObject.SetActive(decorate && sandSlot>=0);
            if(decorate && sandSlot>=0){var at=SandpitPlay.Socket(m,sandSlot);sandAttachmentPreview.rectTransform.anchoredPosition=SandPoint(m.x,m.y)+new Vector2(at.X,at.Y);sandAttachmentPreview.decorationKind=sandDecorKind;sandAttachmentPreview.color=new Color(1,1,1,.65f);sandAttachmentPreview.SetVerticesDirty();sandAttachmentPreview.transform.SetAsLastSibling();}
            for(var c=0;c<sandToySpots.Count;c++){var b=sandToySpots[c];var p=SandToyPoint(c);b.gameObject.SetActive(toy);b.interactable=toy && !sandpitSending && SandpitPlay.ToyPosition(g,p.X,p.Y);if(toy)b.transform.SetAsLastSibling();}
            if(joined && sandToyTexture==null){sandToyTexture=WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/tyrannosaurus");sandToyPicture.texture=sandToyTexture;sandDecorCards[5].GetComponentInChildren<RawImage>().texture=sandToyTexture;}
            sandToyRoot.gameObject.SetActive(joined && g.toy.placed);
            if(sandToyReaction<0 || !joined)sandToyReaction=g.toy.reaction;
            if(g.toy.reaction!=sandToyReaction){sandToyReaction=g.toy.reaction;sandToyBounce=Time.unscaledTime+.8f;}
            var bounce=Mathf.Max(0,sandToyBounce-Time.unscaledTime);sandToyRoot.anchoredPosition=SandPoint(g.toy.x,g.toy.y);sandToyPicture.uvRect=new Rect(bounce>0?.5f:0,0,.25f,.5f);sandToyPicture.rectTransform.anchoredPosition=new Vector2(0,105*(DinosaurLandmarks.Get("tyrannosaurus",bounce>0?6:4).z-.5f)+Mathf.Sin(bounce/.8f*Mathf.PI)*16);
            if(g.toy.placed){
                // The visitor belongs at its saved ground depth, not above every
                // castle face. Slot/placement previews remain in front of it.
                var front=Enumerable.Range(0,g.moulds.Length).Where(i=>g.moulds[i].y<g.toy.y).OrderByDescending(i=>g.moulds[i].y).DefaultIfEmpty(-1).First();
                if(front<0)sandToyRoot.SetAsLastSibling();else sandToyRoot.SetSiblingIndex(sandShapes[front].transform.GetSiblingIndex());
                if(decorate){foreach(var b in sandSockets)if(b.gameObject.activeSelf)b.transform.SetAsLastSibling();sandAttachmentPreview.transform.SetAsLastSibling();}
                sandToyRoot.GetComponentInChildren<Button>().interactable=!toy && !sandPlacing;
            }
            foreach(var hit in sandPieceHits)if(toy)hit.GetComponent<Graphic>().raycastTarget=false;
            if(joined && sandPlayMode=="decorate")sandHint.text=m?.built==true?"Choose an attachment · check to add":"Choose a finished piece";
            if(toy)sandHint.text="Tap a free toy spot";
            if(SandEditing)sandHint.text="Move this piece · check to keep";
            if(sandEditSheet.gameObject.activeSelf){
                var edited=SandEditPiece;sandEditHint.text=edited==null?"Piece removed":edited.version!=sandEditVersion?"Piece changed · reopen Edit":sandSlot>=0?"Remove this decoration?":"Move or remove just your piece";
                for(var i=0;i<6;i++){var b=sandEditSheet.Find("Edit sand attachment "+i).GetComponent<Button>();var a=edited?.attachments.FirstOrDefault(v=>v.slot==i);b.interactable=a!=null && edited.version==sandEditVersion;var shape=b.GetComponentInChildren<SandShape>();shape.kind=a?.kind ?? "selection";shape.SetVerticesDirty();b.GetComponent<Image>().color=sandSlot==i?new Color(1,.77f,.35f):Color.white;}
                sandEditSheet.SetAsLastSibling();
            }
            if(sandConfirmSheet.gameObject.activeSelf)sandConfirmSheet.SetAsLastSibling();
        }
        private void TickSandReset()
        {
            if(!HasWorld || safe==null)return;var g=SandpitGame;var r=g?.reset;
            if(sandResetView==null){
                sandResetView=Panel(safe,"Family sandcastle reset vote",Vector2.zero,new Vector2(640,340),Cream,true).rectTransform;
                sandResetHint=Label(sandResetView,"",26,new Vector2(0,126),new Vector2(440,65));
                var castle=Rect(sandResetView,"Creation to clear",new Vector2(-273,92),new Vector2(80,70)).gameObject.AddComponent<SandShape>();castle.kind="mould-icon";castle.mould="square";castle.rectTransform.localScale=Vector3.one*.55f;castle.raycastTarget=false;
                var clear=Rect(sandResetView,"Clear creation picture",new Vector2(273,128),new Vector2(70,70)).gameObject.AddComponent<SandShape>();clear.kind="reset";clear.rectTransform.localScale=Vector3.one*.75f;clear.raycastTarget=false;
                for(var i=0;i<4;i++){var card=Panel(sandResetView,"Reset voter "+i,new Vector2(-210+i*140,17),new Vector2(124,118),Color.white,false);var root=Rect(card.transform,"Family picture",new Vector2(0,-47),new Vector2(100,120));root.localScale=Vector3.one*.45f;sandVoterPictures.Add((root.gameObject.AddComponent<GameCharacterVisual>(),card));var check=Rect(card.transform,"Explicit agreement picture",new Vector2(39,36),new Vector2(40,40)).gameObject.AddComponent<SandShape>();check.kind="confirm";check.rectTransform.localScale=Vector3.one*.5f;check.raycastTarget=false;sandVoterChecks.Add(check);}
                sandResetYes=SandPictureButton(sandResetView,"Clear sandpit","confirm",new Vector2(-130,-109),()=>{var vote=SandpitGame.reset;if(vote!=null)SandPlaySend("reset-yes","reset",vote.token);});
                SandPictureButton(sandResetView,"Keep castle","cancel",new Vector2(130,-109),()=>{var vote=SandpitGame.reset;if(vote!=null)SandPlaySend("reset-decline","reset",vote.token);});
            }
            var show=r!=null && r.voters.Contains(Actor) && Ready && (!Shared || shared.Connected);sandResetView.gameObject.SetActive(show);
            if(!show)return;CancelSandpitIntent();sandResetView.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/680,safe.rect.height/390);sandResetView.SetAsLastSibling();
            sandResetHint.text="Clear only our sandcastle? · "+r.approved.Length+" / "+r.voters.Length;
            sandResetYes.interactable=!r.approved.Contains(Actor) && !sandpitSending;
            for(var i=0;i<4;i++){var entry=sandVoterPictures[i];entry.card.gameObject.SetActive(i<r.voters.Length);if(i<r.voters.Length){entry.art.Select(ReadPlayer(r.voters[i]).avatar);entry.art.PresentFrame(new CharacterFrame(CharacterPose.Sit,0,false,(float)calypsoClock),Mathf.Min(Time.unscaledDeltaTime,.1f));var yes=r.approved.Contains(r.voters[i]);entry.card.color=yes?new Color(.55f,.9f,.71f):Color.white;sandVoterChecks[i].gameObject.SetActive(yes);}}
        }
        private void ResetSandPlay()
        {
            if(sandResetView!=null)Destroy(sandResetView.gameObject);sandResetView=null;sandVoterPictures.Clear();sandVoterChecks.Clear();sandDecorCards.Clear();sandSockets.Clear();sandToySpots.Clear();sandPlayConfirm=sandPlayCancel=sandEdit=sandFamilyReset=null;sandDecorating=false;sandPlayMode="";sandSlot=-1;sandToyReaction=-1;sandConfirmed=null;sandToyTexture=null;
        }
    }
}
