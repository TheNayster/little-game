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
        private int birthdayVisiblePlayers;
        public int VisibleBirthdayPlayers=>CurrentArea==HulkBirthday.Zone?birthdayVisiblePlayers:0;
        public BirthdayState BirthdayView=>HasWorld?Birthday?.Copy():null;
        public string[] BirthdayRenderedOwners=>birthdayRenderedOwners.Select(p=>p.Key+"="+p.Value).ToArray();
        private string birthdayPending="";private long birthdayVisit=-1;
        private bool birthdayApproach,birthdayWasVisible;
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
            foreach(var prop in Birthday.props){
                if(prop.kind=="food" || prop.kind=="drink")continue;
                var image=BirthdayImage("Birthday prop "+prop.id);birthdayProps.Add(prop.id,image);
                image.raycastTarget=true;NavButton(image,()=>BirthdayTapProp(prop.id));
            }
        }
        private void BirthdaySend(string value,string item="",string target="",float x=0,float y=0)
        {
            if(ActionPending || CurrentArea!=HulkBirthday.Zone)return;
            if(Shared)SubmitShared(SoloAction.Birthday,item,target,value,x,y,result=>{if(!result.Accepted)message.text=Friendly(result.Outcome);Render();});
            else {var result=Command(SoloAction.Birthday,item:item,target:target,value:value,x:x,y:y);if(!result.Accepted)message.text=Friendly(result.Outcome);Render();}
        }
        private void BirthdayGive(string actor)
        {
            var held=Birthday?.props.FirstOrDefault(p=>p.holder==Actor);if(held==null || actor==Actor)return;
            BirthdaySend("give",held.id,actor);
        }
        private void BirthdayTapProp(string id)
        {
            if(ActionPending)return;var prop=Birthday.props.Single(p=>p.id==id);var player=ReadPlayer(Actor);
            if(prop.holder==Actor){BirthdaySend("drop",id,x:Mathf.Clamp(player.x+45,0,4800),y:player.y);return;}
            if(prop.holder!="" || Birthday.props.Any(p=>p.holder==Actor))return;
            birthdayPending=id;birthdayVisit=player.visit;birthdayApproach=true;destination=new Vector2(prop.x,prop.y);
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
            if(!visible)return;
            characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);
            var game=Birthday;if(game==null)return;
            for(int i=3;i>=0;i--){var tile=birthdayTiles[i];tile.rectTransform.anchoredPosition=new Vector2((i*1200+600-cameraX)*sceneScale,0);tile.rectTransform.sizeDelta=new Vector2(1200,800)*sceneScale;tile.rectTransform.SetAsFirstSibling();}
            var own=ReadPlayer(Actor);
            if(birthdayApproach){
                var target=game.props.FirstOrDefault(p=>p.id==birthdayPending);
                if(own.visit!=birthdayVisit || target==null || target.holder!=""){birthdayApproach=false;birthdayPending="";destination=null;}
                else {
                    destination=new Vector2(target.x,target.y);
                    if(Vector2.Distance(new Vector2(own.x,own.y),destination.Value)<14 && !ActionPending){var id=birthdayPending;birthdayApproach=false;birthdayPending="";destination=null;BirthdaySend("claim",id);}
                }
            }
            birthdayVisiblePlayers=0;
            foreach(var member in game.members){
                var player=ReadPlayer(member.actor);var inWorld=player.zone==HulkBirthday.Zone && (Shared?shared.Players.Contains(player.id):player.id==Actor);
                if(inWorld)birthdayVisiblePlayers++;
                var point=inWorld?(Shared?shared.VisualPosition(member.actor):new Vector2(player.x,player.y)):new Vector2(2200+Array.IndexOf(HulkBirthday.Roles,member.role)*135,200);
                var moving=inWorld && birthdayPositions.TryGetValue(member.actor,out var previous) && Vector2.Distance(point,previous)>.1f;
                birthdayPositions[member.actor]=point;
                var holding=game.props.Any(p=>p.holder==member.actor);
                string pose=member.station==""?(moving?(holding?"carry":"walk"):holding?"hold":"idle"):member.station=="castle"?"jump":member.station=="dance"?"dance":member.station;
                var action=BirthdayArt.Action(member.role,pose)??BirthdayArt.Action(member.role,"idle");
                if(action==null)continue;
                int frame=action.frames==1?0:(int)(game.clock*12)%action.frames;
                var image=birthdayPeople[member.actor];image.texture=BirthdayArt.Texture(action.texture);image.uvRect=BirthdayArt.Uv(action,frame);
                image.rectTransform.pivot=action.pivot/256;image.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=ToBoard(point.x,point.y);
                var face=birthdayFaces[member.actor];face.raycastTarget=inWorld && member.actor!=Actor;
                face.GetComponent<Button>().interactable=face.raycastTarget;
                face.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition+(action.face[frame]-action.pivot)*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                face.rectTransform.sizeDelta=new Vector2(42,48)*sceneScale;
                birthdayHoldPoints[member.actor]=image.rectTransform.anchoredPosition+(action.hold[frame]-action.pivot)*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                birthdayFrames[member.actor]=(action,frame);
            }
            var presents=BirthdayArt.Data.props.Where(p=>p.name.StartsWith("PresentActor",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            var bricks=BirthdayArt.Data.props.Where(p=>p.name.StartsWith("Bap_Brick",StringComparison.Ordinal)).OrderBy(p=>p.name).ToArray();
            foreach(var prop in game.props){
                if(!birthdayProps.TryGetValue(prop.id,out var image))continue;
                var art=prop.kind=="gift"?presents[prop.style%presents.Length]:prop.kind=="battery"?BirthdayArt.Data.props.Single(p=>p.name=="Battery"):bricks[prop.style%bricks.Length];
                bool held=prop.holder!="" && birthdayHoldPoints.ContainsKey(prop.holder);
                image.texture=BirthdayArt.Texture(art.texture);image.rectTransform.pivot=(held?art.holdPivot:art.pivot)/256;
                image.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=held?birthdayHoldPoints[prop.holder]:ToBoard(prop.x,prop.y);
                birthdayRenderedOwners[prop.id]=prop.holder;
            }
            SortDepth();
        }
        private void AddBirthdayDepth(Action<RectTransform,float,int,string> add)
        {
            if(CurrentArea!=HulkBirthday.Zone || Birthday==null)return;
            foreach(var member in Birthday.members)if(birthdayPeople.TryGetValue(member.actor,out var image))add(image.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,1,"birthday-"+member.actor);
            foreach(var member in Birthday.members)if(birthdayFaces.TryGetValue(member.actor,out var face))add(face.rectTransform,ToBoard(0,birthdayPositions.TryGetValue(member.actor,out var p)?p.y:200).y,4,"birthday-face-"+member.actor);
            foreach(var prop in Birthday.props)if(birthdayProps.TryGetValue(prop.id,out var image)){
                var ground=prop.holder!="" && birthdayPositions.TryGetValue(prop.holder,out var p)?p.y:prop.y;
                add(image.rectTransform,ToBoard(0,ground).y,3,"birthday-prop-"+prop.id);
            }
        }
        private void ResetBirthday()
        {
            birthdayTiles.Clear();birthdayPeople.Clear();birthdayProps.Clear();birthdayFaces.Clear();birthdayPositions.Clear();birthdayHoldPoints.Clear();birthdayFrames.Clear();birthdayRenderedOwners.Clear();birthdayVisiblePlayers=0;birthdayApproach=false;birthdayPending="";birthdayVisit=-1;birthdayWasVisible=false;
        }
    }
}
