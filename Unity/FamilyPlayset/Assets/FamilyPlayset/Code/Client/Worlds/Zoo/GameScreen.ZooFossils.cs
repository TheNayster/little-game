using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform fossilTray,fossilBoard,fossilJoin,fossilLeave,fossilDrop,fossilReplay,fossilConfirm,fossilCancel;
        private readonly Image[] fossilSand=new Image[3],fossilTargets=new Image[3];
        private readonly FossilPicture[] fossilPieces=new FossilPicture[3],fossilGuide=new FossilPicture[3];
        private readonly RectTransform[] fossilHeld=new RectTransform[3],fossilDust=new RectTransform[3],fossilSparkles=new RectTransform[3];
        private readonly RawImage[] fossilPortraits=new RawImage[3];
        private readonly float[] fossilTapAt={-10,-10,-10};
        private long fossilSerial,fossilSeenRound=-1;
        private bool fossilSeenComplete;
        private float fossilCelebration=-10,fossilReplayUntil=-10,fossilHintUntil=-10;
        private int fossilCelebrations;
        public int FossilCelebrations=>fossilCelebrations;
        private ZooFossilState Fossils=>Zoo?.fossils;
        private int FossilMember=>Fossils==null?-1:Array.IndexOf(Fossils.members,Actor);
        private bool FossilBlocked=>MenuOpen || ZooPhotoOpen || ZooMapOpen || ElephantSnackOpen || ElephantHabitatOpen || TravelPending || applicationPaused || ActionPending || Shared && !shared.Connected;
        private FossilPicture FossilDrawing(Transform parent,int piece,bool assembled,Vector2 at,float width,Color tint)
        {var r=Rect(parent,"Dinosaur picture section "+piece,at,new Vector2(width,width/2));var g=r.gameObject.AddComponent<FossilPicture>();g.piece=piece;g.assembled=assembled;g.color=tint;g.raycastTarget=false;return g;}
        private RectTransform FossilButton(Transform parent,string name,Vector2 at,Action action)
        {var b=Panel(parent,name,at,new Vector2(88,70),Cream,true,true);NavButton(b,()=>{if(!FossilBlocked)action();});return b.rectTransform;}
        private void FossilCheck(Transform p,bool yes)
        {
            var a=Plain(p,"Picture stroke",new Vector2(-9,0),new Vector2(7,yes?24:42),Ink);a.rectTransform.localRotation=Quaternion.Euler(0,0,yes?40:45);
            var b=Plain(p,"Picture stroke",new Vector2(yes?7:0,yes?5:0),new Vector2(7,42),Ink);b.rectTransform.localRotation=Quaternion.Euler(0,0,-40);
        }
        private void FossilBrush(Transform p)
        {var a=Plain(p,"Brush handle",new Vector2(-5,7),new Vector2(16,40),new Color(.52f,.68f,.72f));a.rectTransform.localRotation=Quaternion.Euler(0,0,-32);Panel(p,"Soft brush",new Vector2(8,-13),new Vector2(40,22),new Color(.88f,.73f,.47f),false,true);}
        private void BuildZooFossils()
        {
            fossilTray=ZooObject("Fossil dig tray");Panel(fossilTray,"Rounded tray outline",Vector2.zero,new Vector2(420,120),new Color(.44f,.37f,.27f),false,true);Panel(fossilTray,"Shallow sand",Vector2.zero,new Vector2(400,104),new Color(.89f,.77f,.53f),false,true);
            fossilBoard=ZooObject("Fossil picture guide");
            Plain(fossilBoard,"Guide left leg",new Vector2(-92,-45),new Vector2(12,68),new Color(.55f,.43f,.29f));Plain(fossilBoard,"Guide right leg",new Vector2(92,-45),new Vector2(12,68),new Color(.55f,.43f,.29f));Panel(fossilBoard,"Wooden picture board",new Vector2(0,62),new Vector2(300,160),new Color(.55f,.43f,.29f),false,true);Panel(fossilBoard,"Picture guide",new Vector2(0,62),new Vector2(284,144),Cream,false,true);
            for(var i=0;i<3;i++){
                var piece=i;var x=(i-1)*132;
                var hit=Panel(fossilTray,"Uncover or pick up fossil "+i,new Vector2(x,0),new Vector2(128,104),new Color(1,1,1,0),true,true);NavButton(hit,()=>FossilTap(piece));
                fossilPieces[i]=FossilDrawing(hit.transform,i,false,Vector2.zero,108,new Color(.94f,.88f,.68f));
                fossilSand[i]=Panel(hit.transform,"Sand patch "+i,Vector2.zero,new Vector2(116,92),new Color(.82f,.65f,.38f),false,true);FossilBrush(fossilSand[i].transform);
                fossilDust[i]=Rect(hit.transform,"Small dust",Vector2.zero,Vector2.zero);for(var n=0;n<3;n++)Panel(fossilDust[i],"Dust",new Vector2(n*22-22,20+n*7),new Vector2(8,8),Cream,false,true);
                var tx=i==0?-95:i==1?-10:81;
                var slot=Panel(fossilBoard,"Match fossil picture "+i,new Vector2(tx,63),new Vector2(i==1?110:90,110),new Color(.76f,.78f,.67f,.25f),true,true);fossilTargets[i]=slot;NavButton(slot,()=>FossilPlace(piece));
                fossilGuide[i]=FossilDrawing(fossilBoard,i,true,Vector2.zero,240,new Color(.72f,.72f,.62f));
                fossilHeld[i]=ZooObject("Fossil carried piece "+i);FossilDrawing(fossilHeld[i],i,false,new Vector2(0,52),85,new Color(.94f,.88f,.68f));
                var badge=Panel(fossilHeld[i],"Holder picture",new Vector2(0,107),new Vector2(48,48),Cream,false,true);fossilPortraits[i]=Rect(badge.transform,"Child portrait",Vector2.zero,new Vector2(40,40)).gameObject.AddComponent<RawImage>();fossilPortraits[i].raycastTarget=false;
                fossilSparkles[i]=Rect(fossilBoard,"Small completion sparkle "+i,new Vector2((i-1)*100,136),Vector2.zero);Plain(fossilSparkles[i],"Sparkle",Vector2.zero,new Vector2(5,18),new Color(.94f,.72f,.28f));Plain(fossilSparkles[i],"Sparkle",Vector2.zero,new Vector2(18,5),new Color(.94f,.72f,.28f));
            }
            // Only pictured actions; accessible names remain for assistive/test tools.
            fossilJoin=FossilButton(fossilTray,"Join fossil discovery",new Vector2(-260,15),()=>FossilBegin());FossilBrush(fossilJoin);
            fossilLeave=FossilButton(fossilTray,"Leave fossil discovery",new Vector2(-260,15),()=>FossilSend("fossil-leave",-1));ZooArrow(fossilLeave,Vector2.zero,-1);
            fossilDrop=FossilButton(fossilTray,"Put fossil down",new Vector2(260,15),()=>FossilSend("fossil-drop",-1));ZooArrow(fossilDrop,Vector2.zero,-1);Panel(fossilDrop,"Return sand",new Vector2(12,-14),new Vector2(24,10),new Color(.82f,.65f,.38f),false,true);
            fossilReplay=FossilButton(fossilBoard,"Play fossil discovery again",new Vector2(207,64),()=>fossilReplayUntil=Time.unscaledTime+5);ZooArrow(fossilReplay,new Vector2(0,9),-1);FossilBrush(fossilReplay);
            fossilConfirm=FossilButton(fossilBoard,"Confirm fossil replay",new Vector2(207,106),()=>{fossilReplayUntil=-10;FossilSend("fossil-replay",-1);});FossilCheck(fossilConfirm,true);
            fossilCancel=FossilButton(fossilBoard,"Keep dinosaur picture",new Vector2(207,21),()=>fossilReplayUntil=-10);FossilCheck(fossilCancel,false);
        }
        private void FossilBegin()
        {
            if(FossilBlocked || !ZooFossils.Nearby(ReadPlayer(Actor)))return;
            FossilSend("fossil-join",-1,r=>{
                if(!r.Accepted)return;
                var member=FossilMember;if(member<0)return;var spot=ZooFossils.Spot(member);CancelPointers();zooApproach=false;zooEntry=new Vector2(spot.X,spot.Y);zooApproachArea=CurrentArea;zooApproachVisit=ReadPlayer(Actor).visit;
                // Reuse the existing continuous walk path; only this child moves.
                ZooWalk("look","brachiosaurus",zooEntry);
            });
        }
        private void FossilSend(string op,int piece,Action<SoloResult> done=null)
        {
            if(FossilBlocked)return;var f=Fossils;if(f==null)return;var n=FossilMember;
            var item=f.round+"/"+(n<0?0:f.epochs[n])+"/"+(++fossilSerial);
            void End(SoloResult r){if(!r.Accepted){fossilHintUntil=Time.unscaledTime+1.5f;message.text=ZooRejection(r.Outcome);}done?.Invoke(r);Render();}
            if(Shared)SubmitShared(SoloAction.Zoo,item,piece.ToString(),op,0,0,End);else End(Command(SoloAction.Zoo,item:item,target:piece.ToString(),value:op));
        }
        private void FossilTap(int piece)
        {
            if(FossilBlocked)return;if(FossilMember<0){FossilBegin();return;}
            fossilTapAt[piece]=Time.unscaledTime;var f=Fossils;
            FossilSend(f.revealed[piece]<6?"fossil-brush":"fossil-pickup",piece);
        }
        private void FossilPlace(int piece)
        {if(FossilMember>=0)FossilSend("fossil-place",piece);else FossilBegin();}
        private void TickZooFossils()
        {
            var f=Fossils;if(fossilTray==null || f==null)return;
            var shown=CurrentArea==ZooCatalog.Dinosaurs && Math.Abs(cameraX-ZooFossils.X)<Board.rect.width/sceneScale/2+420;
            void Place(RectTransform r,float x,float y){r.gameObject.SetActive(shown);r.anchoredPosition=ToBoard(x,y);r.localScale=Vector3.one*sceneScale;}
            Place(fossilTray,ZooFossils.X,ZooFossils.TrayY);Place(fossilBoard,ZooFossils.X,ZooFossils.BoardY);
            if(!shown || applicationPaused || Shared && !shared.Connected){fossilSeenRound=-1;fossilReplayUntil=-10;fossilCelebration=-10;}
            if(fossilSeenRound==f.round && !fossilSeenComplete && f.Complete && shown && !applicationPaused && (!Shared || shared.Connected)){
                fossilCelebration=Time.unscaledTime;fossilCelebrations++;ZooPlay("brachiosaurus",false);
            }
            if(shown && !applicationPaused && (!Shared || shared.Connected)){fossilSeenRound=f.round;fossilSeenComplete=f.Complete;}
            var member=FossilMember;var held=Array.IndexOf(f.holders,Actor);var motion=Time.unscaledTime-fossilCelebration;
            if(FossilBlocked)fossilReplayUntil=-10;
            fossilJoin.gameObject.SetActive(member<0);fossilLeave.gameObject.SetActive(member>=0);fossilDrop.gameObject.SetActive(held>=0);
            var confirm=Time.unscaledTime<fossilReplayUntil;var canReplay=f.Complete && member>=0 && f.celebrationAge>=2.2;
            fossilReplay.gameObject.SetActive(canReplay && !confirm);fossilConfirm.gameObject.SetActive(canReplay && confirm);fossilCancel.gameObject.SetActive(canReplay && confirm);
            for(var i=0;i<3;i++){
                fossilPieces[i].gameObject.SetActive(!f.placed[i] && f.holders[i]=="");
                var sand=fossilSand[i];sand.gameObject.SetActive(f.revealed[i]<6);sand.color=new Color(.82f,.65f,.38f,1-f.revealed[i]/6f);sand.transform.localScale=Vector3.one*(1-f.revealed[i]*.045f);
                sand.rectTransform.anchoredPosition=new Vector2(Mathf.Sin((Time.unscaledTime-fossilTapAt[i])*20)*Mathf.Max(0,1-(Time.unscaledTime-fossilTapAt[i])/.3f)*5,0);
                fossilDust[i].gameObject.SetActive(Time.unscaledTime-fossilTapAt[i]<.3f && !FossilBlocked && held<0);
                fossilGuide[i].color=f.placed[i]?(f.Complete?new Color(.48f,.73f,.51f):new Color(.94f,.88f,.68f)):new Color(.72f,.72f,.62f);fossilGuide[i].SetVerticesDirty();
                fossilGuide[i].rectTransform.anchoredPosition=new Vector2(0,motion>=0 && motion<2.2?Mathf.Sin(motion*3)*3:0);
                var hint=held==i && member>=0;fossilTargets[i].color=hint?new Color(1,.78f,.32f,Time.unscaledTime<fossilHintUntil?.6f:.3f+.12f*Mathf.Sin(Time.unscaledTime*3)):new Color(.76f,.78f,.67f,.25f);
                fossilSparkles[i].gameObject.SetActive(shown && motion>=0 && motion<2.2 && !applicationPaused);fossilSparkles[i].localScale=Vector3.one*(.7f+.2f*Mathf.Sin(motion*4+i));
                var carrier=f.holders[i];fossilHeld[i].gameObject.SetActive(shown && carrier!="");
                if(carrier!=""){
                    var p=ReadPlayer(carrier);fossilHeld[i].anchoredPosition=ToBoard(p.x,p.y)+new Vector2(65,0)*sceneScale;fossilHeld[i].localScale=Vector3.one*sceneScale;
                    fossilPortraits[i].texture=WorldResources.Load<CharacterMenuArt>("Shared/Characters/Menu/"+PlayableCharacters.Find(p.avatar).ArtId)?.texture;
                }
            }
        }
        private void ResetFossils(){fossilTray=null;fossilBoard=null;fossilSerial=0;fossilSeenRound=-1;fossilSeenComplete=false;fossilCelebration=-10;fossilReplayUntil=-10;fossilCelebrations=0;}
    }
}

