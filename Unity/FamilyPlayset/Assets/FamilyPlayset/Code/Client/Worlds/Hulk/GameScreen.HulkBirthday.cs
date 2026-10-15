using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly List<RawImage> birthdayTiles=new List<RawImage>();
        private readonly Dictionary<string,RawImage> birthdayPeople=new Dictionary<string,RawImage>(),birthdayProps=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Image> birthdayFaces=new Dictionary<string,Image>();
        private readonly Dictionary<string,Vector2> birthdayPositions=new Dictionary<string,Vector2>(),birthdayHoldPoints=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,(BirthdayArt.Frames action,int frame)> birthdayFrames=new Dictionary<string,(BirthdayArt.Frames,int)>();
        private readonly Dictionary<string,string> birthdayRenderedOwners=new Dictionary<string,string>();
        private readonly Dictionary<string,(string pose,double time)> birthdayReactions=new Dictionary<string,(string,double)>();
        private readonly Dictionary<string,int> birthdayFacing=new Dictionary<string,int>();
        private readonly Dictionary<string,string> birthdayContinuities=new Dictionary<string,string>(),birthdayViews=new Dictionary<string,string>();
        private readonly Dictionary<string,bool> birthdayMirrors=new Dictionary<string,bool>();
        private int birthdayVisiblePlayers;
        public int VisibleBirthdayPlayers=>CurrentArea==HulkBirthday.Zone?birthdayVisiblePlayers:0;
        public BirthdayState BirthdayView=>HasWorld?Birthday?.Copy():null;
        public string[] BirthdayRenderedOwners=>birthdayRenderedOwners.Select(p=>p.Key+"="+p.Value).ToArray();
        public string[] BirthdayRenderedPoses=>birthdayFrames.Select(p=>p.Key+"="+p.Value.action.action+":"+p.Value.frame+(birthdayMirrors.TryGetValue(p.Key,out var mirror) && mirror?":mirrored":"")).ToArray();
        private string birthdayPending="",birthdayPendingActor="";private long birthdayVisit=-1;
        private bool birthdayApproach,birthdayWasVisible;
        private double birthdayReactionSeen=-1;
        private BirthdayState Birthday=>Shared?shared.View.birthday:World.ReadBirthday();
        private RawImage BirthdayImage(string name)
        {
            var rect=Rect(Board,name,Vector2.zero,Vector2.one);var image=rect.gameObject.AddComponent<BirthdayImage>();image.raycastTarget=false;return image;
        }
        private void BuildBirthday()
        {
            if(SceneSchema<HulkBirthday.Schema || !BirthdayArt.Available)return;
            for(int i=0;i<4;i++){
                var tile=BirthdayImage("Hulk birthday scenery "+i);tile.texture=BirthdayArt.Texture("world-"+i+".png");birthdayTiles.Add(tile);
            }
            foreach(var member in Birthday.members){
                var image=BirthdayImage("Birthday artwork "+member.actor);birthdayPeople.Add(member.actor,image);
                var face=Plain(Board,"Birthday friend "+member.actor,Vector2.zero,new Vector2(42,48),Color.clear,true);
                birthdayFaces.Add(member.actor,face);NavButton(face,()=>BirthdayGive(member.actor));
            }
            foreach(var npc in Birthday.cast){
                birthdayPeople.Add(npc.id,BirthdayImage("Birthday artwork "+npc.id));
                var face=Plain(Board,"Birthday friend "+npc.id,Vector2.zero,new Vector2(42,48),Color.clear,true);
                birthdayFaces.Add(npc.id,face);NavButton(face,()=>BirthdayGive(npc.id));
            }
            foreach(var prop in Birthday.props){
                var image=BirthdayImage("Birthday prop "+prop.id);birthdayProps.Add(prop.id,image);
                image.raycastTarget=true;NavButton(image,()=>BirthdayTapProp(prop.id));
            }
            BuildBirthdayPartyUi();
        }
        private void BirthdaySend(string value,string item="",string target="",float x=0,float y=0)
        {
            if(ActionPending || CurrentArea!=HulkBirthday.Zone)return;
            if(Shared)SubmitShared(SoloAction.Birthday,item,target,value,x,y,result=>{if(!result.Accepted)message.text=Friendly(result.Outcome);Render();});
            else {var result=Command(SoloAction.Birthday,item:item,target:target,value:value,x:x,y:y);if(!result.Accepted)message.text=Friendly(result.Outcome);Render();}
        }
        private void BirthdayGive(string actor)
        {
            var held=Birthday?.props.FirstOrDefault(p=>p.holder==Actor);if(actor==Actor)return;
            var npc=Birthday.cast.FirstOrDefault(n=>n.id==actor && n.active && n.controller=="");
            if(npc!=null){
                if(held==null && !Birthday.props.Any(p=>p.holder==npc.id))return;
                birthdayApproach=false;birthdayPending="";birthdayPendingActor=actor;birthdayVisit=ReadPlayer(Actor).visit;return;
            }
            if(held==null)return;
            BirthdaySend("give",held.id,actor);
        }
        private void BirthdayTapProp(string id)
        {
            if(ActionPending)return;var prop=Birthday.props.Single(p=>p.id==id);var player=ReadPlayer(Actor);
            if(prop.holder==Actor){BirthdaySend("drop",id,x:Mathf.Clamp(player.x+45,0,4800),y:player.y);return;}
            if(prop.holder!="" || Birthday.props.Any(p=>p.holder==Actor))return;
            birthdayPendingActor="";birthdayPending=id;birthdayVisit=player.visit;birthdayApproach=true;destination=new Vector2(prop.x,prop.y);
        }
        private void TickBirthday()
        {
            if(birthdayTiles.Count==0)return;
            bool visible=CurrentArea==HulkBirthday.Zone;
            if(visible && miniGamesButton!=null)miniGamesButton.gameObject.SetActive(false);
            foreach(var tile in birthdayTiles)tile.gameObject.SetActive(visible);
            foreach(var image in birthdayPeople.Values.Concat(birthdayProps.Values))image.gameObject.SetActive(visible);
            foreach(var face in birthdayFaces.Values)face.gameObject.SetActive(visible);
            if(birthdayWasVisible!=visible){birthdayWasVisible=visible;if(!visible){birthdayPending="";birthdayApproach=false;birthdayVisit=-1;characterVisual.gameObject.SetActive(true);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(true);if(miniGamesButton!=null && !VetOwn)miniGamesButton.gameObject.SetActive(true);}}
            if(!visible){TickBirthdayPartyUi(false,Birthday);return;}
            characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);
            var game=Birthday;if(game==null)return;
            if(game.reactionAt>=0 && game.reactionAt!=birthdayReactionSeen){
                birthdayReactionSeen=game.reactionAt;
                var hulk=game.cast[0];string receiver=hulk.controller==""?hulk.id:hulk.controller;
                if(game.reaction=="cake-happy")foreach(var id in game.members.Select(m=>m.actor).Concat(game.cast.Where(n=>n.active).Select(n=>n.id)))birthdayReactions[id]=("happy",Time.unscaledTimeAsDouble);
                else birthdayReactions[receiver]=(game.reaction=="happy" && game.props.Any(p=>p.holder==receiver)?"happy-hold":game.reaction,Time.unscaledTimeAsDouble);
            }
            foreach(var prop in game.props)if(birthdayRenderedOwners.TryGetValue(prop.id,out var before) && before!=prop.holder){
                bool nextHuman=game.members.Any(m=>m.actor==prop.holder),priorHuman=game.members.Any(m=>m.actor==before);
                // Cosmetics observe authority-owned possession. Animation frames
                // never issue a second transfer, consume or reward command.
                if(nextHuman)birthdayReactions[prop.holder]=(priorHuman?"happy-hold":"pickup",Time.unscaledTimeAsDouble);
                if(nextHuman && priorHuman)birthdayReactions[before]=("happy",Time.unscaledTimeAsDouble);
            }
            for(int i=3;i>=0;i--){var tile=birthdayTiles[i];tile.rectTransform.anchoredPosition=new Vector2((i*1200+600-cameraX)*sceneScale,0);tile.rectTransform.sizeDelta=new Vector2(1200,800)*sceneScale;tile.rectTransform.SetAsFirstSibling();}
            var own=ReadPlayer(Actor);
            if(birthdayPendingActor!=""){
                var npc=game.cast.FirstOrDefault(n=>n.id==birthdayPendingActor && n.active && n.controller=="");
                if(own.visit!=birthdayVisit || npc==null){birthdayPendingActor="";destination=null;}
                else {
                    destination=new Vector2(npc.x,npc.y);
                    if(Vector2.Distance(new Vector2(own.x,own.y),destination.Value)<60 && !ActionPending){
                        var held=game.props.FirstOrDefault(p=>p.holder==Actor);var offered=game.props.FirstOrDefault(p=>p.holder==npc.id);
                        birthdayPendingActor="";destination=null;
                        if(held!=null)BirthdaySend("give",held.id,npc.id);else if(offered!=null)BirthdaySend("take",offered.id,npc.id);
                    }
                }
            }
            if(birthdayApproach){
                var target=game.props.FirstOrDefault(p=>p.id==birthdayPending);
                if(own.visit!=birthdayVisit || target==null || target.holder!=""){birthdayApproach=false;birthdayPending="";destination=null;}
                else {
                    destination=new Vector2(target.x,target.y);
                    if(Vector2.Distance(new Vector2(own.x,own.y),destination.Value)<14 && !ActionPending){var id=birthdayPending;birthdayApproach=false;birthdayPending="";destination=null;BirthdaySend("claim",id);}
                }
            }
            birthdayVisiblePlayers=0;
            var bodies=game.members.Select(m=>new{m.actor,m.role,m.station,npc=(BirthdayNpc)null})
                .Concat(game.cast.Select(n=>new{actor=n.id,n.role,station="",npc=n}));
            foreach(var member in bodies){
                var player=member.npc==null?ReadPlayer(member.actor):null;
                var inWorld=player!=null && player.zone==HulkBirthday.Zone && (Shared?shared.Players.Contains(player.id):player.id==Actor);
                if(inWorld)birthdayVisiblePlayers++;
                var npc=member.npc;
                bool shown=npc==null?inWorld:npc.active && !game.members.Any(m=>m.role==npc.role && ReadPlayer(m.actor).zone==HulkBirthday.Zone && (Shared?shared.Players.Contains(m.actor):m.actor==Actor));
                birthdayPeople[member.actor].gameObject.SetActive(shown);birthdayFaces[member.actor].gameObject.SetActive(shown);
                if(!shown){birthdayFrames.Remove(member.actor);birthdayHoldPoints.Remove(member.actor);continue;}
                var point=npc!=null?new Vector2(npc.x,npc.y):Shared?shared.VisualPosition(member.actor):new Vector2(player.x,player.y);
                var continuity=npc!=null?npc.role:player.zone+"/"+player.visit;
                bool same=birthdayContinuities.TryGetValue(member.actor,out var last) && last==continuity;
                if(!same){birthdayFacing[member.actor]=0;birthdayReactions.Remove(member.actor);birthdayContinuities[member.actor]=continuity;}
                var previous=birthdayPositions.TryGetValue(member.actor,out var prior)?prior:point;
                var delta=point-previous;
                var moving=same && (npc!=null?npc.phase=="enter" || npc.phase=="leave":delta.magnitude>.1f);
                if(npc!=null && moving)delta=new Vector2(npc.targetX,npc.targetY)-point;
                if(moving){birthdayFacing[member.actor]=Mathf.Abs(delta.x)>=Mathf.Abs(delta.y)*.45f?(delta.x<0?-1:1):delta.y>0?2:0;birthdayReactions.Remove(member.actor);}
                birthdayPositions[member.actor]=point;
                var holding=game.props.Any(p=>p.holder==member.actor);
                string pose=member.station==""?(moving?(holding?"carry":"walk"):holding?"hold":"idle"):member.station=="castle"?"jump":member.station=="dance"?"dance":member.station;
                if(npc!=null && !moving && (npc.phase=="wave" || !game.introDone))pose=holding?"happy-hold":"wave";
                if(npc!=null && game.giftAt>=0 && game.giftActor==npc.id)pose="unwrap";
                int direction=member.station==""?birthdayFacing[member.actor]:0;
                string view=direction==2?"back":direction!=0?"side":"front",suffix=view=="front"?"":"-"+view;
                var action=BirthdayArt.Action(member.role,pose+suffix)??BirthdayArt.Action(member.role,pose)??BirthdayArt.Action(member.role,"idle");
                bool reactionActive=false;double age=0;
                if(!moving && (inWorld || npc!=null) && member.station=="" && pose!="unwrap" && birthdayReactions.TryGetValue(member.actor,out var reaction)){
                    var candidate=BirthdayArt.Action(member.role,reaction.pose+suffix);
                    age=Time.unscaledTimeAsDouble-reaction.time;
                    if(candidate!=null && age>=0 && age<candidate.duration){action=candidate;reactionActive=true;}
                }
                if(action==null)continue;
                // A rounded-up final atlas frame must not lengthen the source
                // clip. Reactions finish once; locomotion keeps its source loop.
                double phase=reactionActive?age:Time.unscaledTimeAsDouble%Math.Max(.001,action.duration);
                int frame=action.frames==1?0:Mathf.Min(action.frames-1,(int)(phase*BirthdayArt.Data.fps));
                bool mirror=direction<0;
                Vector2 Offset(Vector2 anchor){var offset=anchor-action.pivot;if(mirror)offset.x=-offset.x;return offset*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;}
                var image=birthdayPeople[member.actor];image.texture=BirthdayArt.Texture(action.texture);image.uvRect=BirthdayArt.Uv(action,frame);
                if(mirror){var uv=image.uvRect;image.uvRect=new Rect(uv.x+uv.width,uv.y,-uv.width,uv.height);}
                image.rectTransform.pivot=new Vector2(mirror?1-action.pivot.x/256:action.pivot.x/256,action.pivot.y/256);image.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=ToBoard(point.x,point.y);
                var face=birthdayFaces[member.actor];face.raycastTarget=(inWorld || npc!=null) && member.actor!=Actor;
                face.GetComponent<Button>().interactable=face.raycastTarget;
                face.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition+Offset(action.face[frame]);
                face.rectTransform.sizeDelta=new Vector2(42,48)*sceneScale;
                birthdayHoldPoints[member.actor]=image.rectTransform.anchoredPosition+Offset(action.hold[frame]);
                birthdayFrames[member.actor]=(action,frame);
                birthdayViews[member.actor]=view;birthdayMirrors[member.actor]=mirror;
            }
            var presents=BirthdayArt.Data.props.Where(p=>p.view=="front" && p.name.StartsWith("PresentActor",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            var bricks=BirthdayArt.Data.props.Where(p=>p.view=="front" && p.name.StartsWith("Bap_Brick",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            foreach(var prop in game.props){
                if(!birthdayProps.TryGetValue(prop.id,out var image))continue;
                var art=prop.kind=="gift"?(prop.opened?BirthdayArt.PropView(HulkBirthday.Contents[prop.content],"front"):presents[prop.style%presents.Length]):
                    prop.kind=="food"?BirthdayArt.PropView("CakeActorVariant_CR","front"):prop.kind=="drink"?BirthdayArt.PropView("FrostyDrink","front"):
                    prop.kind=="battery"?BirthdayArt.PropView("Battery","front"):bricks[prop.style%bricks.Length];
                image.gameObject.SetActive(prop.location!="stock");
                bool held=prop.holder!="" && birthdayHoldPoints.ContainsKey(prop.holder);
                bool mirror=held && birthdayMirrors[prop.holder];
                if(held)art=BirthdayArt.PropView(art.name,"held-"+birthdayViews[prop.holder])??BirthdayArt.PropView(art.name,birthdayViews[prop.holder])??art;
                var pivot=held?art.holdPivot:art.pivot;
                // The source gift peg sits near the base. In this flattened
                // projection that covers the face instead of passing in front
                // of the body. Lower bulky gifts to the hand area for readable
                // 2D play; the original peg remains intact in the local pack.
                if(held && prop.kind=="gift" && !prop.opened)pivot.y=Mathf.Max(pivot.y,128);
                image.texture=BirthdayArt.Texture(art.texture);image.uvRect=new Rect(mirror?1:0,0,mirror?-1:1,1);image.rectTransform.pivot=new Vector2(mirror?1-pivot.x/256:pivot.x/256,pivot.y/256);
                image.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=held?birthdayHoldPoints[prop.holder]:ToBoard(prop.x,prop.y);
                birthdayRenderedOwners[prop.id]=prop.holder;
            }
            SortDepth();
            TickBirthdayPartyUi(visible,game);
        }
        private void AddBirthdayDepth(Action<RectTransform,float,int,string> add)
        {
            if(CurrentArea!=HulkBirthday.Zone || Birthday==null)return;
            if(birthdayWorldCake!=null)add(birthdayWorldCake,ToBoard(0,330).y,0,"birthday-cake");
            foreach(var member in Birthday.members)if(birthdayPeople.TryGetValue(member.actor,out var image))add(image.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,1,"birthday-"+member.actor);
            foreach(var member in Birthday.members)if(birthdayFaces.TryGetValue(member.actor,out var face))add(face.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,4,"birthday-face-"+member.actor);
            foreach(var npc in Birthday.cast){
                if(birthdayPeople.TryGetValue(npc.id,out var image))add(image.rectTransform,ToBoard(0,npc.y).y,1,"birthday-"+npc.id);
                if(birthdayFaces.TryGetValue(npc.id,out var face))add(face.rectTransform,ToBoard(0,npc.y).y,4,"birthday-face-"+npc.id);
            }
            foreach(var prop in Birthday.props)if(birthdayProps.TryGetValue(prop.id,out var image)){
                var ground=prop.holder!="" && birthdayPositions.TryGetValue(prop.holder,out var p)?p.y:prop.y;
                add(image.rectTransform,ToBoard(0,ground).y,3,"birthday-prop-"+prop.id);
            }
        }
        private void ResetBirthday()
        {
            birthdayTiles.Clear();birthdayPeople.Clear();birthdayProps.Clear();birthdayFaces.Clear();birthdayPositions.Clear();birthdayHoldPoints.Clear();birthdayFrames.Clear();birthdayRenderedOwners.Clear();birthdayVisiblePlayers=0;birthdayApproach=false;birthdayPending="";birthdayVisit=-1;birthdayWasVisible=false;
            birthdayReactions.Clear();birthdayFacing.Clear();birthdayContinuities.Clear();birthdayViews.Clear();birthdayMirrors.Clear();birthdayPendingActor="";
            ResetBirthdayPartyUi();birthdayReactionSeen=-1;
        }
    }
}
