using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly Dictionary<string,RawImage> spideyBricks=new Dictionary<string,RawImage>();
        private string spideyPanelStamp="";
        private int spideyPicture,spideyDifficulty=2;
        private int spideyDraggingPiece=-1;
        private RawImage spideyPieceGhost;
        private bool spideyDropPending;
        private Vector2 spideyDropCell;
        private void SpideyOpen(string kind)
        {
            if(MenuOpen)return;SpideyGo(kind=="puzzle"?"puzzle-join":"leave",SpideyHQ.X(kind),160);spideyStation=kind;
        }
        private void SpideyShowPanel(string kind)
        {spideyPanelKind=kind;spideyPanelOpen=true;spideyPanelStamp="";spideyPanel.gameObject.SetActive(true);destination=null;if(Shared)shared.Walk(WalkMode.Stop);TickSpideyPanel(Spidey);}
        private void SpideyClose(){FinishSpideyPieceDrag();spideyPanelOpen=false;if(spideyPanel)spideyPanel.gameObject.SetActive(false);}
        private void SpideyPanelButton(string name,string text,Vector2 point,Vector2 size,Action action)
        {var label=Button(spideyPanel,text,point,size,()=>action(),new Color(.81f,.91f,.97f));label.transform.parent.name=name;spideyPanelItems.Add(label.transform.parent.gameObject);}
        private void SpideyPanelPicture(string name,Texture texture,Vector2 point,Vector2 size,Action action,Rect? uv=null)
        {
            var image=SpideyImage(name,spideyPanel);image.texture=texture;image.uvRect=uv??new Rect(0,0,1,1);image.rectTransform.anchoredPosition=point;image.rectTransform.sizeDelta=size;image.raycastTarget=true;if(action!=null)NavButton(image,action);spideyPanelItems.Add(image.gameObject);
        }
        private void TickSpideyPanel(SpideyState h)
        {
            if(!spideyPanelOpen || CurrentArea!=SpideyHQ.Zone || spideyDraggingPiece>=0)return;
            string stamp=spideyPanelKind+"/"+h.puzzleRevision+"/"+spideyPicture+"/"+spideyDifficulty+"/"+string.Join(",",h.bricks.Select(b=>b.id+":"+b.owner+":"+b.version));if(stamp==spideyPanelStamp)return;spideyPanelStamp=stamp;
            foreach(var item in spideyPanelItems){if(item)item.SetActive(false);Destroy(item);}spideyPanelItems.Clear();
            SpideyPanelButton("HQ activity close","Back",new Vector2(465,280),new Vector2(120,55),()=>{SpideyClose();SpideySend("leave");});
            if(spideyPanelKind=="build"){
                var title=Label(spideyPanel,"Build your headquarters together",26,new Vector2(-80,275),new Vector2(740,50));spideyPanelItems.Add(title.gameObject);
                var own=h.bricks.FirstOrDefault(b=>b.owner==Actor);
                if(own==null){
                    for(int i=0;i<26;i++){int variant=i;var point=new Vector2(-455+i%9*113,145-i/9*112);SpideyPanelPicture("HQ brick choice "+i,SpideyArt.Picture("brick-"+i),point,new Vector2(95,95),()=>SpideySend("brick-new",variant.ToString()));}
                    var hint=Label(spideyPanel,"Choose a brick. You can move your own piece while the others build.",19,new Vector2(0,-250),new Vector2(960,55));spideyPanelItems.Add(hint.gameObject);
                }else{
                    SpideyPanelPicture("HQ selected brick",SpideyArt.Picture("brick-"+own.variant),new Vector2(-420,100),new Vector2(140,140),null);
                    SpideyPanelButton("HQ brick rotate","Turn",new Vector2(-420,-35),new Vector2(150,55),()=>SpideySend("brick-turn",own.id,own.version.ToString()));
                    SpideyPanelButton("HQ brick remove","Put away",new Vector2(-420,-110),new Vector2(150,55),()=>SpideySend("brick-remove",own.id,own.version.ToString()));
                    for(int row=0;row<7;row++)for(int col=0;col<10;col++){
                        int x=col,y=row;var point=new Vector2(-240+col*70,-200+row*58);var button=Panel(spideyPanel,"HQ building spot "+col+" "+row,point,new Vector2(62,50),new Color(.72f,.84f,.85f),true,true);spideyPanelItems.Add(button.gameObject);
                        NavButton(button,()=>SpideySend("brick-place",own.id,own.version.ToString(),SpideyHQ.X("build")-200+x*40,40+y*40));
                        var brick=h.bricks.FirstOrDefault(b=>b.id!=own.id && Math.Abs(b.x-(SpideyHQ.X("build")-200+x*40))<24 && Math.Abs(b.y-(40+y*40))<24);
                        if(brick!=null){var icon=SpideyImage("HQ mat brick "+brick.id,button.transform);icon.texture=SpideyArt.Picture("brick-"+brick.variant);icon.rectTransform.sizeDelta=new Vector2(60,50);icon.rectTransform.localEulerAngles=new Vector3(0,0,brick.turn*90);}
                    }
                }
            }else{
                var title=Label(spideyPanel,h.puzzlePlaced.All(v=>v==1)?"You finished it together!":"Our shared puzzle",26,new Vector2(-160,275),new Vector2(650,50));spideyPanelItems.Add(title.gameObject);
                for(int i=0;i<5;i++){int picture=i;SpideyPanelPicture("HQ puzzle picture "+i,SpideyArt.Picture("puzzle-"+i),new Vector2(220+i*62-30,200),new Vector2(58,58),()=>{spideyPicture=picture;spideyPanelStamp="";});}
                for(int i=2;i<=5;i++){int size=i;SpideyPanelButton("HQ puzzle difficulty "+i,(i*i).ToString(),new Vector2(195+(i-2)*80,120),new Vector2(70,50),()=>{spideyDifficulty=size;spideyPanelStamp="";});}
                SpideyPanelButton("HQ puzzle start",h.puzzlePlaced.All(v=>v==1)?"Play again":"Choose picture",new Vector2(310,45),new Vector2(250,55),()=>SpideySend("puzzle-select",spideyPicture.ToString(),spideyDifficulty.ToString()));
                int n=h.puzzleSize;float cell=400f/n;var texture=SpideyArt.Picture("puzzle-"+h.puzzleImage);int selected=Array.IndexOf(h.puzzleHolders,Actor);
                for(int i=0;i<n*n;i++){
                    int piece=i,col=i%n,row=i/n;var point=new Vector2(-330+(col+.5f)*cell,200-(row+.5f)*cell);
                    var panel=Panel(spideyPanel,"HQ puzzle slot "+i,point,Vector2.one*(cell-3),new Color(.74f,.82f,.88f),true);spideyPanelItems.Add(panel.gameObject);
                    if(h.puzzlePlaced[i]==1){var image=SpideyImage("HQ placed puzzle "+i,panel.transform);image.texture=texture;image.rectTransform.sizeDelta=Vector2.one*(cell-3);image.uvRect=new Rect(col/(float)n,1-(row+1f)/n,1f/n,1f/n);}
                    else NavButton(panel,()=>{if(selected>=0)SpideySend("puzzle-place",selected.ToString(),Spidey.puzzleRevision.ToString(),col,row);});
                    if(h.puzzlePlaced[i]!=0)continue;
                    var tray=new Vector2(170+(i%5)*62,-95-i/5*44);var uv=new Rect(col/(float)n,1-(row+1f)/n,1f/n,1f/n);
                    SpideyPanelPicture("HQ puzzle piece "+i,texture,tray,new Vector2(56,40),()=>{if(Spidey.puzzleHolders[piece]==Actor)SpideySend("puzzle-release",piece.ToString());else SpideySend("puzzle-claim",piece.ToString());},uv);
                    var surface=spideyPanelItems.Last().AddComponent<SpideyPuzzleDrag>();
                    surface.begin=data=>BeginSpideyPieceDrag(piece,texture,uv,data);surface.drag=MoveSpideyPieceDrag;surface.end=EndSpideyPieceDrag;
                    if(h.puzzleHolders[i]!=""){
                        var badge=Label(spideyPanel,h.puzzleHolders[i]==Actor?"â†‘":"â€¢",21,tray,new Vector2(40,36));badge.raycastTarget=false;spideyPanelItems.Add(badge.gameObject);
                    }
                }
            }
        }
        private void BeginSpideyPieceDrag(int piece,Texture texture,Rect uv,PointerEventData data)
        {
            if(ActionPending || spideyDraggingPiece>=0 || Spidey.puzzleHolders[piece]!="" && Spidey.puzzleHolders[piece]!=Actor)return;
            spideyDraggingPiece=piece;spideyDropPending=false;spideyPieceGhost=SpideyImage("HQ dragging puzzle piece",spideyPanel);spideyPieceGhost.texture=texture;spideyPieceGhost.uvRect=uv;spideyPieceGhost.rectTransform.sizeDelta=Vector2.one*(400f/Spidey.puzzleSize);MoveSpideyPieceDrag(data);
            if(Spidey.puzzleHolders[piece]!=Actor)SpideySend("puzzle-claim",piece.ToString(),callback:result=>{if(!result.Accepted)FinishSpideyPieceDrag();else if(spideyDropPending)SubmitSpideyPieceDrop();});
        }
        private void MoveSpideyPieceDrag(PointerEventData data)
        {if(spideyDraggingPiece>=0 && spideyPieceGhost && RectTransformUtility.ScreenPointToLocalPointInRectangle(spideyPanel,data.position,data.pressEventCamera,out var local))spideyPieceGhost.rectTransform.anchoredPosition=local;}
        private void EndSpideyPieceDrag(PointerEventData data)
        {
            if(spideyDraggingPiece<0)return;MoveSpideyPieceDrag(data);var point=spideyPieceGhost.rectTransform.anchoredPosition;float cell=400f/Spidey.puzzleSize;spideyDropCell=new Vector2(Mathf.Floor((point.x+330)/cell),Mathf.Floor((200-point.y)/cell));spideyDropPending=true;if(!ActionPending)SubmitSpideyPieceDrop();
        }
        private void SubmitSpideyPieceDrop()
        {
            if(spideyDraggingPiece<0)return;int piece=spideyDraggingPiece;bool inside=spideyDropCell.x>=0 && spideyDropCell.y>=0 && spideyDropCell.x<Spidey.puzzleSize && spideyDropCell.y<Spidey.puzzleSize;
            SpideySend(inside?"puzzle-place":"puzzle-release",piece.ToString(),Spidey.puzzleRevision.ToString(),spideyDropCell.x,spideyDropCell.y,_=>FinishSpideyPieceDrag());
        }
        private void FinishSpideyPieceDrag(){if(spideyPieceGhost)Destroy(spideyPieceGhost.gameObject);spideyPieceGhost=null;spideyDraggingPiece=-1;spideyDropPending=false;spideyPanelStamp="";}
        private void TickSpideyCreations(SpideyState h)
        {
            foreach(var pair in spideyBricks)pair.Value.gameObject.SetActive(CurrentArea==SpideyHQ.Zone && h.bricks.Any(b=>b.id==pair.Key));
            foreach(var brick in h.bricks){
                if(!spideyBricks.TryGetValue(brick.id,out var image)){image=SpideyImage("HQ world brick "+brick.id);spideyBricks[brick.id]=image;image.raycastTarget=true;string id=brick.id;NavButton(image,()=>{SpideyGo("brick-claim",SpideyHQ.X("build"),160,id);spideyStation="build";});}
                image.texture=SpideyArt.Picture("brick-"+brick.variant);image.rectTransform.sizeDelta=Vector2.one*75*sceneScale;image.rectTransform.anchoredPosition=ToBoard(brick.x,brick.y);image.rectTransform.localEulerAngles=new Vector3(0,0,brick.turn*90);image.color=brick.owner==""?Color.white:new Color(1,1,1,.65f);
            }
        }
    }
}
