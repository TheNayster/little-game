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
        private string birthdayLastOutcome="";
        public string BirthdayLastOutcome=>birthdayLastOutcome;
        public string[] BirthdayRenderedPoses=>birthdayFrames.Select(p=>p.Key+"="+p.Value.action.action+":"+p.Value.frame+(birthdayMirrors.TryGetValue(p.Key,out var mirror) && mirror?":mirrored":"")).ToArray();
        private string birthdayPending="",birthdayPendingActor="";private long birthdayVisit=-1;
        private bool birthdayApproach,birthdayWasVisible;
        private bool BirthdayWalkingApproach=>CurrentArea==HulkBirthday.Zone && (birthdayApproach || birthdayPendingActor!="" || birthdayCakeApproach || birthdayUnwrapApproach || birthdayVendorApproach || birthdayBandApproach!="" || birthdaySocketApproach || birthdayCastleApproach || birthdayBubbleApproach);
        private void CancelBirthdayIntent(){birthdayApproach=false;birthdayPending="";birthdayPendingActor="";birthdayCakeApproach=false;birthdayUnwrapApproach=false;birthdayVendorApproach=false;birthdayBandApproach="";birthdaySocketApproach=false;birthdayCastleApproach=false;birthdayBubbleApproach=false;}
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
            BuildBirthdayBandUi();
            BuildBirthdayCastleUi();
            BuildBirthdayPlayUi();
        }
        private void BirthdaySend(string value,string item="",string target="",float x=0,float y=0)
        {
            if(ActionPending || CurrentArea!=HulkBirthday.Zone)return;
            if(HulkBirthday.StationaryOperation(value)){destination=null;if(Shared)shared.Walk(WalkMode.Stop);}
            void Done(SoloResult result){birthdayLastOutcome=result.Outcome;if(!result.Accepted)message.text=Friendly(result.Outcome);Render();}
            if(Shared)SubmitShared(SoloAction.Birthday,item,target,value,x,y,Done);
            else Done(Command(SoloAction.Birthday,item:item,target:target,value:value,x:x,y:y));
        }
        private void BirthdayGive(string actor)
        {
            var held=Birthday?.props.FirstOrDefault(p=>p.holder==Actor);if(actor==Actor)return;
            var npc=Birthday.cast.FirstOrDefault(n=>n.id==actor && n.active && n.controller=="");
            if(npc!=null){
                if(held==null && !Birthday.props.Any(p=>p.holder==npc.id))return;
                CancelBirthdayIntent();
                birthdayApproach=false;birthdayPending="";birthdayPendingActor=actor;birthdayVisit=ReadPlayer(Actor).visit;return;
            }
            if(held==null)return;
            BirthdaySend("give",held.id,actor);
        }
        private void BirthdayTapProp(string id)
        {
            if(ActionPending)return;var prop=Birthday.props.Single(p=>p.id==id);var player=ReadPlayer(Actor);
            if(prop.location=="counter")return;
            CancelBirthdayIntent();
            if(prop.holder==Actor){BirthdaySend("drop",id,x:Mathf.Clamp(player.x+45,0,4800),y:player.y);return;}
            var held=Birthday.props.FirstOrDefault(p=>p.holder==Actor);
            if(prop.holder!="" || held!=null && !(held.kind=="brick" && prop.kind=="brick"))return;
            birthdayPendingActor="";birthdayPending=id;birthdayVisit=player.visit;birthdayApproach=true;destination=new Vector2(prop.x,prop.y);
        }
        private void TickBirthday()
        {
            if(birthdayTiles.Count==0)return;
            bool visible=CurrentArea==HulkBirthday.Zone;
            if(visible && miniGamesButton!=null)miniGamesButton.gameObject.SetActive(false);
            TickBirthdayCastleUi(visible);
            TickBirthdayPlayUi(visible);
            foreach(var tile in birthdayTiles)tile.gameObject.SetActive(visible);
            foreach(var image in birthdayPeople.Values.Concat(birthdayProps.Values))image.gameObject.SetActive(visible);
            foreach(var face in birthdayFaces.Values)face.gameObject.SetActive(visible);
            if(birthdayWasVisible!=visible){birthdayWasVisible=visible;if(!visible){birthdayPending="";birthdayApproach=false;birthdayVisit=-1;characterVisual.gameObject.SetActive(true);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(true);if(miniGamesButton!=null && !VetOwn)miniGamesButton.gameObject.SetActive(true);}}
            if(!visible){TickBirthdayPartyUi(false,Birthday);HideBirthdayBand();return;}
            characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);
            var game=Birthday;if(game==null)return;
            if(game.reactionAt>=0 && game.reactionAt!=birthdayReactionSeen){
                birthdayReactionSeen=game.reactionAt;
                var hulk=game.cast[0];string receiver=hulk.controller==""?hulk.id:hulk.controller;
                if(game.reaction=="cake-happy" || game.reaction=="band-happy")foreach(var id in game.reaction=="band-happy"?game.bandActors:game.members.Select(m=>m.actor).Concat(game.cast.Where(n=>n.active).Select(n=>n.id)))birthdayReactions[id]=("happy",Time.unscaledTimeAsDouble);
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
                    if(Vector2.Distance(new Vector2(own.x,own.y),destination.Value)<14 && !ActionPending){var id=birthdayPending;birthdayApproach=false;birthdayPending="";destination=null;BirthdaySend(target.kind=="brick" && game.props.Any(p=>p.holder==Actor && p.kind=="brick")?"stack":"claim",id);}
                }
            }
            birthdayVisiblePlayers=0;
            var bodies=game.members.Select(m=>new{m.actor,m.role,m.station,npc=(BirthdayNpc)null})
                .Concat(game.cast.Select(n=>new{actor=n.id,n.role,station=n.station,npc=n}));
            foreach(var member in bodies){
                var player=member.npc==null?ReadPlayer(member.actor):null;
                var inWorld=player!=null && player.zone==HulkBirthday.Zone && (Shared?shared.Players.Contains(player.id):player.id==Actor);
                if(inWorld)birthdayVisiblePlayers++;
                var npc=member.npc;
                bool shown=npc==null?inWorld:npc.active && !game.members.Any(m=>m.role==npc.role && ReadPlayer(m.actor).zone==HulkBirthday.Zone && (Shared?shared.Players.Contains(m.actor):m.actor==Actor));
                birthdayPeople[member.actor].gameObject.SetActive(shown);birthdayFaces[member.actor].gameObject.SetActive(shown);
                if(!shown){birthdayFrames.Remove(member.actor);birthdayHoldPoints.Remove(member.actor);continue;}
                var point=npc!=null?new Vector2(npc.x,npc.y):Shared?shared.VisualPosition(member.actor):new Vector2(player.x,player.y);
                if(npc!=null && (npc.phase=="enter" || npc.phase=="leave" || npc.phase=="band-walk")){
                    double clock=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,game.clock,Time.unscaledTimeAsDouble,Shared);
                    point=Vector2.MoveTowards(point,new Vector2(npc.targetX,npc.targetY),(float)Math.Clamp(clock-game.clock,0,.3)*Walking.Speed);
                }
                var castleMember=npc==null && member.station=="castle"?game.members.Single(m=>m.actor==member.actor):null;
                if(castleMember!=null)point=BirthdayCastlePoint(castleMember);
                var continuity=npc!=null?npc.role:player.zone+"/"+player.visit;
                bool same=birthdayContinuities.TryGetValue(member.actor,out var last) && last==continuity;
                if(!same){birthdayFacing[member.actor]=0;birthdayReactions.Remove(member.actor);birthdayContinuities[member.actor]=continuity;}
                var previous=birthdayPositions.TryGetValue(member.actor,out var prior)?prior:point;
                var delta=point-previous;
                var moving=same && (npc!=null?npc.phase=="enter" || npc.phase=="leave" || npc.phase=="band-walk":delta.magnitude>.1f);
                if(npc!=null && moving)delta=new Vector2(npc.targetX,npc.targetY)-point;
                if(moving){birthdayFacing[member.actor]=Mathf.Abs(delta.x)>=Mathf.Abs(delta.y)*.45f?(delta.x<0?-1:1):delta.y>0?2:0;birthdayReactions.Remove(member.actor);}
                birthdayPositions[member.actor]=point;
                var holding=game.props.Any(p=>p.holder==member.actor);
                string pose=member.station==""?(moving?(holding?"carry":"walk"):holding?"hold":"idle"):member.station=="queue" || member.station=="vendor-exit"?(moving?(holding?"carry":"walk"):holding?"hold":"idle"):member.station=="castle"?"jump":member.station=="dance"?"dance":member.station;
                if(castleMember!=null)pose=BirthdayCastlePose(castleMember);
                if(npc!=null && !moving && member.station=="" && (npc.phase=="wave" || !game.introDone))pose=holding?"happy-hold":"wave";
                if(npc!=null && game.giftAt>=0 && game.giftActor==npc.id)pose="unwrap";
                if(npc!=null && npc.phase=="band-walk")pose="walk";
                int direction=member.station=="" || member.station=="queue" || member.station=="vendor-exit" || npc?.phase=="band-walk"?birthdayFacing[member.actor]:0;
                string view=direction==2?"back":direction!=0?"side":"front",suffix=view=="front"?"":"-"+view;
                var action=BirthdayArt.Action(member.role,pose+suffix)??BirthdayArt.Action(member.role,pose)??BirthdayArt.Action(member.role,"idle");
                bool reactionActive=false;double age=0;
                if(!moving && (inWorld || npc!=null) && (member.station=="" || member.station=="vendor-exit") && pose!="unwrap" && birthdayReactions.TryGetValue(member.actor,out var reaction)){
                    var candidate=BirthdayArt.Action(member.role,reaction.pose+suffix);
                    age=Time.unscaledTimeAsDouble-reaction.time;
                    if(candidate!=null && age>=0 && age<candidate.duration){action=candidate;reactionActive=true;}
                }
                if(action==null)continue;
                // A rounded-up final atlas frame must not lengthen the source
                // clip. Reactions finish once; locomotion keeps its source loop.
                double phase=reactionActive?age:Time.unscaledTimeAsDouble%Math.Max(.001,action.duration);
                if(castleMember!=null && pose=="stunt")phase=Math.Max(0,game.clock-castleMember.stuntAt);
                int frame=action.frames==1?0:Mathf.Min(action.frames-1,(int)(phase*BirthdayArt.Data.fps));
                bool mirror=direction<0;
                Vector2 Offset(Vector2 anchor){var offset=anchor-action.pivot;if(mirror)offset.x=-offset.x;return offset*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;}
                var image=birthdayPeople[member.actor];image.texture=BirthdayArt.Texture(action.texture);image.uvRect=BirthdayArt.Uv(action,frame);
                if(mirror){var uv=image.uvRect;image.uvRect=new Rect(uv.x+uv.width,uv.y,-uv.width,uv.height);}
                image.rectTransform.pivot=new Vector2(mirror?1-action.pivot.x/256:action.pivot.x/256,action.pivot.y/256);image.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=ToBoard(point.x,point.y);
                var face=birthdayFaces[member.actor];face.raycastTarget=(inWorld || npc!=null) && member.actor!=Actor && (game.props.Any(p=>p.holder==Actor) || npc!=null && holding);
                face.GetComponent<Button>().interactable=face.raycastTarget;
                face.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition+Offset(action.face[frame]);
                face.rectTransform.sizeDelta=new Vector2(42,48)*sceneScale;
                birthdayHoldPoints[member.actor]=image.rectTransform.anchoredPosition+Offset(action.hold[frame]);
                birthdayFrames[member.actor]=(action,frame);
                birthdayViews[member.actor]=view;birthdayMirrors[member.actor]=mirror;
            }
            var presents=BirthdayArt.Data.props.Where(p=>p.view=="front" && p.name.StartsWith("PresentActor",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            var bricks=BirthdayArt.Data.props.Where(p=>p.view=="front" && p.name.StartsWith("Bap_Brick",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            TickBirthdayVendorUi(game);
            TickBirthdayBandUi(game);
            foreach(var prop in game.props){
                if(!birthdayProps.TryGetValue(prop.id,out var image))continue;
                var art=prop.kind=="gift"?(prop.opened?BirthdayArt.PropView(HulkBirthday.Contents[prop.content],"front"):presents[prop.style%presents.Length]):
                    prop.kind=="food"?BirthdayArt.PropView(prop.style==1?"CakeActorVariant_VR":"CakeActorVariant_CR","front"):prop.kind=="drink"?BirthdayArt.PropView("FrostyDrink","front"):
                    prop.kind=="battery"?BirthdayArt.PropView("Battery","front"):bricks[prop.style%bricks.Length];
                image.gameObject.SetActive(prop.location!="stock");
                string carrier=prop.holder!=""?prop.holder:prop.location=="counter"?"birthday-squirrel":"";
                bool held=carrier!="" && birthdayHoldPoints.ContainsKey(carrier);
                bool mirror=held && birthdayMirrors[carrier];
                if(held)art=BirthdayArt.PropView(art.name,"held-"+birthdayViews[carrier])??BirthdayArt.PropView(art.name,birthdayViews[carrier])??art;
                var pivot=held?art.holdPivot:art.pivot;
                // The source gift peg sits near the base. In this flattened
                // projection that covers the face instead of passing in front
                // of the body. Lower bulky gifts to the hand area for readable
                // 2D play; the original peg remains intact in the local pack.
                if(held && (prop.kind=="gift" || prop.kind=="food"))pivot.y=Mathf.Max(pivot.y,128);
                image.texture=BirthdayArt.Texture(art.texture);image.uvRect=new Rect(mirror?1:0,0,mirror?-1:1,1);image.rectTransform.pivot=new Vector2(mirror?1-pivot.x/256:pivot.x/256,pivot.y/256);
                image.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                var attach=held?birthdayHoldPoints[carrier]:ToBoard(prop.x,prop.y);
                var eater=held?game.members.FirstOrDefault(m=>m.actor==carrier):null;
                if(eater!=null && (eater.station=="eat" || eater.station=="drink"))attach=BirthdayHandPoint(carrier,false);
                image.rectTransform.anchoredPosition=attach;
                if(prop.kind=="brick"){
                    image.rectTransform.localRotation=Quaternion.Euler(0,0,prop.turns*90);image.color=BirthdayBrickColours[prop.tint];
                    if(!held)image.rectTransform.anchoredPosition+=new Vector2(0,BirthdayPlay.BrickLevel(game,prop)*24*sceneScale);
                }
                birthdayRenderedOwners[prop.id]=prop.holder;
            }
            SortDepth();
            TickBirthdayPartyUi(visible,game);
        }
        private void AddBirthdayDepth(Action<RectTransform,float,int,string> add)
        {
            if(CurrentArea!=HulkBirthday.Zone || Birthday==null)return;
            if(birthdayWorldCake!=null)add(birthdayWorldCake,ToBoard(0,330).y,0,"birthday-cake");
            AddBirthdayBandDepth(add);
            AddBirthdayCastleDepth(add);
            AddBirthdayPlayDepth(add);
            if(birthdayPeople.TryGetValue("birthday-squirrel",out var seller))add(seller.rectTransform,ToBoard(0,216).y,1,"birthday-squirrel");
            if(birthdayFaces.TryGetValue("birthday-squirrel",out var sellerFace))add(sellerFace.rectTransform,ToBoard(0,216).y,4,"birthday-squirrel-face");
            foreach(var member in Birthday.members)if(birthdayPeople.TryGetValue(member.actor,out var image))add(image.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,1,"birthday-"+member.actor);
            foreach(var member in Birthday.members)if(birthdayFaces.TryGetValue(member.actor,out var face))add(face.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,4,"birthday-face-"+member.actor);
            foreach(var npc in Birthday.cast){
                if(birthdayPeople.TryGetValue(npc.id,out var image))add(image.rectTransform,ToBoard(0,npc.y).y,1,"birthday-"+npc.id);
                if(birthdayFaces.TryGetValue(npc.id,out var face))add(face.rectTransform,ToBoard(0,npc.y).y,4,"birthday-face-"+npc.id);
            }
            foreach(var prop in Birthday.props)if(birthdayProps.TryGetValue(prop.id,out var image)){
                var ground=prop.location=="counter"?216:prop.holder!="" && birthdayPositions.TryGetValue(prop.holder,out var p)?p.y:prop.y;
                add(image.rectTransform,ToBoard(0,ground).y,3+(prop.kind=="brick"?BirthdayPlay.BrickLevel(Birthday,prop):0),"birthday-prop-"+prop.id);
            }
        }
        private void ResetBirthday()
        {
            birthdayTiles.Clear();birthdayPeople.Clear();birthdayProps.Clear();birthdayFaces.Clear();birthdayPositions.Clear();birthdayHoldPoints.Clear();birthdayFrames.Clear();birthdayRenderedOwners.Clear();birthdayVisiblePlayers=0;birthdayApproach=false;birthdayPending="";birthdayVisit=-1;birthdayWasVisible=false;
            birthdayReactions.Clear();birthdayFacing.Clear();birthdayContinuities.Clear();birthdayViews.Clear();birthdayMirrors.Clear();birthdayPendingActor="";
            ResetBirthdayPartyUi();birthdayReactionSeen=-1;
            ResetBirthdayBandUi();
            ResetBirthdayCastleUi();
            ResetBirthdayPlayUi();
        }
    }
}
