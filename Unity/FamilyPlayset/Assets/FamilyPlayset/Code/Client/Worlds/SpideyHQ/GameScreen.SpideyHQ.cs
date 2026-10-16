using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly List<RawImage> spideyTiles=new List<RawImage>();
        private readonly Dictionary<string,RawImage> spideyPeople=new Dictionary<string,RawImage>(),spideyProps=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Vector2> spideyPositions=new Dictionary<string,Vector2>(),spideyHands=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,Vector2> spideyLeftHands=new Dictionary<string,Vector2>(),spideyRightHands=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,Image> spideyWebs=new Dictionary<string,Image>();
        private readonly List<Image> spideyGlobeDots=new List<Image>();
        private readonly Dictionary<string,string> spideyLastStations=new Dictionary<string,string>();
        private readonly Dictionary<string,Image> spideyNpcHits=new Dictionary<string,Image>();
        private Image spideySenseRing;
        private readonly Dictionary<string,(BirthdayArt.Frames strip,int frame)> spideyFrames=new Dictionary<string,(BirthdayArt.Frames,int)>();
        private readonly Dictionary<string,RectTransform> spideyBadges=new Dictionary<string,RectTransform>();
        private readonly List<(RectTransform rect,Vector2 point)> spideyHotspots=new List<(RectTransform,Vector2)>();
        private RectTransform spideyToolbar,spideyPanel;
        private Text spideyHint;
        private RawImage spideyPoster,spideyPhoto,spideyCamera,spideyBoard;
        private Texture2D spideyPhotoTexture;
        private AudioSource spideyMusic,spideyEffects;
        private bool spideyWasVisible,spideyPanelOpen;
        private string spideyPanelKind="",spideyPending="",spideyStation="",spideyItem="",spideyTarget="",spideyOutcome="";
        private double spideyPhotoSeen=-1,spideyGlobeSeen=-1,spideyDominoSeen=-1,spideyPosterSeen=-1;
        private int spideyVisiblePlayers;
        private readonly BoatVisualClock spideyClock=new BoatVisualClock();
        private readonly List<GameObject> spideyPanelItems=new List<GameObject>();
        public int VisibleSpideyPlayers=>CurrentArea==SpideyHQ.Zone?spideyVisiblePlayers:0;
        public SpideyState SpideyView=>HasWorld?Spidey?.Copy():null;
        public string SpideyLastOutcome=>spideyOutcome;
        public string[] SpideyRenderedPoses=>spideyFrames.Select(p=>p.Key+"="+p.Value.strip.action+":"+p.Value.frame).ToArray();
        private SpideyState Spidey=>Shared?shared.View.spidey:World.ReadSpidey();
        private bool SpideyWalkingApproach=>CurrentArea==SpideyHQ.Zone && spideyPending!="";
        private void CancelSpideyIntent(){spideyPending="";spideyStation="";}
        private RawImage SpideyImage(string name,Transform parent=null)
        {var rect=Rect(parent?parent:Board,name,Vector2.zero,Vector2.one);var image=rect.gameObject.AddComponent<SpideyImage>();image.raycastTarget=false;return image;}
        private void SpideySound(string key){if(spideyEffects)StartCoroutine(SpideyArt.LoadAudio(key,clip=>{if(spideyEffects && clip)spideyEffects.PlayOneShot(clip,.55f);}));}
        private void BuildSpidey()
        {
            if(SceneSchema<SpideyHQ.Schema || !SpideyArt.Available)return;
            for(int i=0;i<SpideyArt.Data.tileCount;i++){var image=SpideyImage("Spidey HQ scenery "+i);image.texture=SpideyArt.Texture("world-"+i+".png");spideyTiles.Add(image);}
            foreach(var member in Spidey.members){
                var id=member.actor;spideyPeople[id]=SpideyImage("HQ character "+id);
                var badge=Panel(Board,"HQ player "+id,Vector2.zero,new Vector2(43,37),new Color(.91f,.95f,1),true,true);spideyBadges[id]=badge.rectTransform;
                Label(badge.transform,(Array.FindIndex(Spidey.members,m=>m.actor==id)+1).ToString(),22,Vector2.zero,new Vector2(40,36));
                NavButton(badge,()=>{var held=Spidey.props.FirstOrDefault(p=>p.holder==Actor);if(held!=null && id!=Actor)SpideySend("give",held.id,id);});
            }
            foreach(var npc in Spidey.cast){
                string id="hq-npc-"+npc.role;spideyPeople[id]=SpideyImage("HQ resident "+npc.role);
                if(npc.role=="GreenGoblin")continue;var hit=Panel(Board,"HQ resident tap "+npc.role,Vector2.zero,new Vector2(43,43),new Color(1,1,1,.01f),true,true);spideyNpcHits[id]=hit;
                NavButton(hit,()=>{var n=Spidey.cast.Single(a=>"hq-npc-"+a.role==id);var held=Spidey.props.FirstOrDefault(p=>p.holder==Actor);var offered=Spidey.props.FirstOrDefault(p=>p.holder==id);SpideyGo(held!=null?"give":offered!=null?"npc-take":"npc-greet",n.x,n.y,held?.id??offered?.id??"",id);});
            }
            foreach(var prop in Spidey.props){var id=prop.id;var image=SpideyImage("HQ prop "+id);spideyProps[id]=image;image.raycastTarget=true;NavButton(image,()=>{var p=Spidey.props.Single(a=>a.id==id);if(p.holder=="" && p.location=="ground")SpideyGo("claim",p.x,p.y,id);});}
            void Station(string key,string label,string operation,string target=""){
                var panel=Panel(Board,"HQ "+key,Vector2.zero,new Vector2(114,66),new Color(.94f,.97f,1,.9f),true,true);Label(panel.transform,label,15,new Vector2(0,-21),new Vector2(112,24));SpideyStationIcon(key,panel.transform);
                var point=new Vector2(SpideyHQ.X(key),key=="globe"?410:130);spideyHotspots.Add((panel.rectTransform,point));NavButton(panel,()=>SpideyGo(operation,point.x,160,"",target));
            }
            Station("pizza","Pizza","queue","pizza");Station("balloon","Balloons","queue","balloon");Station("slide","Slide","queue","slide");Station("poster","Picture","poster");Station("globe","Globe","globe");Station("photo","Photo","photo");Station("domino","Tumble","domino");
            var puzzle=Panel(Board,"HQ puzzle station",Vector2.zero,new Vector2(122,70),new Color(.95f,.87f,.55f),true,true);Label(puzzle.transform,"Puzzles",20,Vector2.zero,new Vector2(120,65));NavButton(puzzle,()=>SpideyOpen("puzzle"));spideyHotspots.Add((puzzle.rectTransform,new Vector2(SpideyHQ.X("puzzle"),180)));
            var build=Panel(Board,"HQ building station",Vector2.zero,new Vector2(122,70),new Color(.68f,.87f,.96f),true,true);Label(build.transform,"Build HQ",20,Vector2.zero,new Vector2(120,65));NavButton(build,()=>SpideyOpen("build"));spideyHotspots.Add((build.rectTransform,new Vector2(SpideyHQ.X("build"),180)));
            for(int i=0;i<6;i++){int seat=i;var button=Panel(Board,"HQ seat "+i,Vector2.zero,new Vector2(50,45),new Color(.9f,.72f,.6f,.7f),true,true);Label(button.transform,"Sit",16,Vector2.zero,new Vector2(48,42));spideyHotspots.Add((button.rectTransform,new Vector2(SpideyHQLayout.SeatX[i],SpideyHQLayout.SeatY[i]+90)));NavButton(button,()=>SpideyGo("seat",SpideyHQLayout.SeatX[seat],SpideyHQLayout.SeatY[seat],seat.ToString()));}
            spideyPoster=SpideyImage("HQ changing picture");spideyPhoto=SpideyImage("HQ keepsake photo");
            spideyCamera=SpideyImage("HQ original camera");spideyBoard=SpideyImage("HQ Goblin hoverboard");
            foreach(var id in Spidey.members.Select(m=>m.actor).Concat(SpideyHQ.Heroes.Select(role=>"hq-npc-"+role))){var line=Plain(Board,"HQ connected web "+id,Vector2.zero,new Vector2(1,3),Color.white);line.raycastTarget=false;spideyWebs[id]=line;}
            for(int i=0;i<3;i++)spideyGlobeDots.Add(Panel(Board,"HQ globe dot "+i,Vector2.zero,Vector2.one*8,new Color(.94f,.18f,.22f),false,true));
            spideyToolbar=Rect(safe,"HQ play controls",new Vector2(0,-287),new Vector2(1030,66));
            string[] names={"Eat","Float","Drop","Web","Vanish","Sense","Sleep","Leave"};
            for(int i=0;i<names.Length;i++){string name=names[i];var label=Button(spideyToolbar,name,new Vector2(-430+i*123,0),new Vector2(115,58),()=>SpideyTool(name),new Color(.82f,.92f,.98f));label.transform.parent.name="HQ tool "+name;}
            for(int i=0;i<SpideyHQ.Roles.Length;i++){
                string role=SpideyHQ.Roles[i];var action=SpideyArt.Action(role,"idle");var image=SpideyImage("HQ choose "+role,spideyToolbar);image.texture=SpideyArt.Texture(action.texture);image.uvRect=BirthdayArt.Uv(action,0);image.rectTransform.anchoredPosition=new Vector2(-440+i*60,78);image.rectTransform.sizeDelta=Vector2.one*70;image.raycastTarget=true;NavButton(image,()=>SpideySend("role",target:role));
            }
            spideySenseRing=Panel(Board,"HQ Spidey sense",Vector2.zero,Vector2.one*180,new Color(.35f,.8f,1,.3f),false,true);
            spideyHint=Label(safe,"",20,new Vector2(0,-235),new Vector2(1000,42));
            spideyPanel=Panel(safe,"HQ shared activity",Vector2.zero,new Vector2(1110,655),new Color(.95f,.97f,1,.99f),true).rectTransform;spideyPanel.gameObject.SetActive(false);
            spideyMusic=gameObject.AddComponent<AudioSource>();spideyMusic.playOnAwake=false;spideyMusic.loop=true;spideyMusic.volume=.2f;spideyEffects=gameObject.AddComponent<AudioSource>();spideyEffects.playOnAwake=false;
        }
        private void SpideyStationIcon(string key,Transform parent)
        {
            var icon=SpideyImage("HQ pictured "+key,parent);icon.rectTransform.anchoredPosition=new Vector2(0,9);icon.rectTransform.sizeDelta=new Vector2(65,37);
            if(key=="pizza" || key=="balloon" || key=="photo"){
                var prop=SpideyArt.Prop(key=="photo"?"camera":key,"front");if(prop!=null){icon.texture=SpideyArt.Texture(prop.texture);icon.rectTransform.sizeDelta=Vector2.one*53;}
            }else if(key=="poster")icon.texture=SpideyArt.Picture("poster-0");
            else{
                // Picture the actual recovered set piece, using its location in
                // the orthographic tile; the globe crop stays inside tile 1.
                float x=SpideyHQ.X(key),y=key=="slide"?310:key=="globe"?330:160;if(key=="globe")x=2340;
                int tile=(int)(x/1200);icon.texture=SpideyArt.Texture("world-"+tile+".png");icon.uvRect=new Rect((x-tile*1200-60)/1200,(y*.45f+150-35)/SpideyArt.Data.worldHeight,120f/1200,70f/SpideyArt.Data.worldHeight);
            }
        }
        private void SpideyTool(string name)
        {
            var held=Spidey.props.FirstOrDefault(p=>p.holder==Actor);
            if(name=="Drop"){var p=ReadPlayer(Actor);if(held!=null)SpideySend("drop",held.id,"",p.x+35,p.y);return;}
            if(name=="Web"){SpideyGo("web",SpideyHQ.X("web"),160);return;}
            if(name=="Sleep"){var m=Spidey.members.Single(a=>a.actor==Actor);if(m.seat>=0)SpideySend("sleep",m.seat.ToString());return;}
            SpideySend(name=="Eat"?"eat":name=="Float"?"float":name=="Vanish"?"invisible":name=="Sense"?"sense":"leave");
        }
        private void SpideySend(string operation,string item="",string target="",float x=0,float y=0,Action<SoloResult> callback=null)
        {
            if(ActionPending || CurrentArea!=SpideyHQ.Zone)return;
            destination=null;if(Shared)shared.Walk(WalkMode.Stop);
            void Done(SoloResult result){spideyOutcome=result.Outcome;if(!result.Accepted)message.text=Friendly(result.Outcome);callback?.Invoke(result);Render();}
            if(Shared)SubmitShared(SoloAction.Spidey,item,target,operation,x,y,Done);else Done(Command(SoloAction.Spidey,item:item,target:target,value:operation,x:x,y:y));
        }
        private void SpideyGo(string operation,float x,float y,string item="",string target="")
        {if(MenuOpen || TravelPending)return;CancelSpideyIntent();spideyPending=operation;spideyItem=item;spideyTarget=target;destination=new Vector2(x,Mathf.Clamp(y,50,400));}
        private void TickSpidey()
        {
            if(spideyTiles.Count==0)return;bool visible=CurrentArea==SpideyHQ.Zone;
            foreach(var tile in spideyTiles)tile.gameObject.SetActive(visible);foreach(var image in spideyPeople.Values.Concat(spideyProps.Values))image.gameObject.SetActive(visible);foreach(var badge in spideyBadges.Values)badge.gameObject.SetActive(visible);foreach(var hotspot in spideyHotspots)hotspot.rect.gameObject.SetActive(visible);
            foreach(var web in spideyWebs.Values)web.gameObject.SetActive(false);foreach(var dot in spideyGlobeDots)dot.gameObject.SetActive(visible);spideyCamera.gameObject.SetActive(visible);spideyBoard.gameObject.SetActive(visible && Spidey.battle!="idle" && Spidey.battle!="celebrate");foreach(var brick in spideyBricks.Values)brick.gameObject.SetActive(visible);
            foreach(var hit in spideyNpcHits.Values)hit.gameObject.SetActive(false);spideySenseRing.gameObject.SetActive(false);
            spideyMusic.mute=spideyEffects.mute=musicMuted || applicationPaused;spideyMusic.volume=.2f*ForegroundDucking;
            spideyToolbar.gameObject.SetActive(visible && !spideyPanelOpen);spideyHint.gameObject.SetActive(visible);spideyPoster.gameObject.SetActive(visible);spideyPhoto.gameObject.SetActive(visible && spideyPhotoTexture);
            if(spideyWasVisible!=visible){
                spideyWasVisible=visible;
                if(visible)StartCoroutine(SpideyArt.LoadAudio("music",clip=>{if(spideyMusic && CurrentArea==SpideyHQ.Zone){spideyMusic.clip=clip;spideyMusic.Play();}}));
                else{CancelSpideyIntent();SpideyClose();characterVisual.gameObject.SetActive(true);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(true);spideyMusic.Stop();}
            }
            if(!visible)return;
            characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);if(miniGamesButton)miniGamesButton.gameObject.SetActive(false);
            var h=Spidey;if(h==null)return;
            double visualClock=spideyClock.Sample(Shared?shared.View.worldId:World.WorldId,h.clock,Time.realtimeSinceStartupAsDouble,Shared);
            if(Math.Abs(visualClock-h.clock)>1){spideyClock.Reset();visualClock=h.clock;}
            if(spideyPending!="" && destination.HasValue && !ActionPending){
                bool npcTarget=spideyTarget.StartsWith("hq-npc-",StringComparison.Ordinal);if(npcTarget){var n=h.cast.FirstOrDefault(v=>"hq-npc-"+v.role==spideyTarget);if(n!=null)destination=new Vector2(n.x,n.y);}
                var p=ReadPlayer(Actor);if(Vector2.Distance(new Vector2(p.x,p.y),destination.Value)<(npcTarget?25:65)){var op=spideyPending;var item=spideyItem;var target=spideyTarget;var panel=spideyStation;CancelSpideyIntent();SpideySend(op,item,target,callback:result=>{if(result.Accepted && panel!="")SpideyShowPanel(panel);});}
            }
            for(int i=spideyTiles.Count-1;i>=0;i--){var tile=spideyTiles[i];tile.rectTransform.anchoredPosition=new Vector2((i*1200+600-cameraX)*sceneScale,(SpideyArt.Data.worldHeight-800)*.5f*sceneScale);tile.rectTransform.sizeDelta=new Vector2(1200,SpideyArt.Data.worldHeight)*sceneScale;tile.rectTransform.SetAsFirstSibling();}
            foreach(var hotspot in spideyHotspots){hotspot.rect.anchoredPosition=ToBoard(hotspot.point.x,hotspot.point.y);hotspot.rect.localScale=Vector3.one*sceneScale;}
            spideyVisiblePlayers=0;
            foreach(var m in h.members){
                var p=ReadPlayer(m.actor);bool shown=p.zone==SpideyHQ.Zone && (Shared?shared.Players.Contains(p.id):p.id==Actor);spideyPeople[m.actor].gameObject.SetActive(shown);spideyBadges[m.actor].gameObject.SetActive(shown);if(!shown)continue;spideyVisiblePlayers++;
                var position=Shared?shared.VisualPosition(m.actor):new Vector2(p.x,p.y);var previous=spideyPositions.TryGetValue(m.actor,out var old)?old:position;bool walking=Vector2.Distance(position,previous)>.1f;
                var held=h.props.FirstOrDefault(a=>a.holder==m.actor);bool holding=held!=null;
                string pose=m.station==""?(walking?(holding?(held.kind=="balloon"?"balloon-carry":"carry"):"walk"):holding?(held.kind=="balloon"?"balloon-hold":"hold"):visualClock-m.at<2 && m.pose=="happy"?"happy":"idle"):m.pose;
                if(walking && m.station==""){string view=position.y-previous.y>Math.Abs(position.x-previous.x)*.5f?"back":"side";if(SpideyArt.Action(m.role,pose+"-"+view)!=null)pose+="-"+view;}
                float lift=m.station=="float"?80+Mathf.Sin((float)(visualClock-m.at)*2)*12:0;
                if((h.battle=="fight" || h.battle=="pull") && h.webActors.Contains(m.actor))pose=h.battle=="pull"?"web-left":"web-right";
                if(h.introAt>=0 && visualClock-h.introAt<3 && SpideyHQ.Heroes.Contains(m.role) && m.station==""){pose=m.role=="Spiderman_Morales"?"web-swing":"web-fall";lift=(float)(1-(visualClock-h.introAt)/3)*130;}
                if(m.station=="slide" && h.slideQueue.Length>0 && h.slideQueue[0]==m.actor){double t=(visualClock-m.at)/SpideyHQLayout.SlideDuration;position=new Vector2(SpideyHQLayout.SlidePoint(t,false),SpideyHQLayout.SlidePoint(t,true));lift=0;}
                float opacity=m.station=="invisible"?Mathf.Lerp(1,.15f,Mathf.Min(1,(float)Math.Min(visualClock-m.at,m.until-visualClock)/.75f)):1;
                DrawSpideyPerson(m.actor,m.role,pose,position,lift,opacity,walking && position.x<previous.x,visualClock-m.at);
                if(m.station=="sense"){float t=Mathf.Clamp01((float)(visualClock-m.at)/2.1f);spideySenseRing.gameObject.SetActive(true);spideySenseRing.rectTransform.anchoredPosition=ToBoard(position.x,position.y)+Vector2.up*75*sceneScale;spideySenseRing.rectTransform.sizeDelta=Vector2.one*Mathf.Lerp(130,260,t)*sceneScale;spideySenseRing.color=new Color(.25f,.7f,1,.35f*Mathf.Sin(t*Mathf.PI));}
                if(!spideyLastStations.TryGetValue(m.actor,out var lastStation) || lastStation!=m.station){if(m.station=="slide" && h.slideQueue.FirstOrDefault()==m.actor)SpideySound("slide");if(m.station=="web")SpideySound("web");if(m.station=="float")SpideySound("balloon");spideyLastStations[m.actor]=m.station;}
                var face=spideyFrames[m.actor];var facePoint=face.strip.face[face.frame]-face.strip.pivot;spideyBadges[m.actor].anchoredPosition=ToBoard(position.x,position.y)+new Vector2(0,lift*sceneScale)+facePoint*face.strip.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale+Vector2.up*25*sceneScale;spideyBadges[m.actor].localScale=Vector3.one*sceneScale;
            }
            foreach(var n in h.cast){
                string id="hq-npc-"+n.role;bool shown=!h.members.Any(m=>m.role==n.role && ReadPlayer(m.actor).zone==SpideyHQ.Zone && (Shared?shared.Players.Contains(m.actor):m.actor==Actor));if(n.role=="GreenGoblin")shown=h.battle!="idle";spideyPeople[id].gameObject.SetActive(shown);if(!shown)continue;
                var point=n.role=="GreenGoblin"?new Vector2(2400,230):new Vector2(n.x,n.y);
                if(n.pose=="walk")point=Vector2.MoveTowards(point,new Vector2(n.targetX,n.targetY),(float)Math.Clamp(visualClock-h.clock,0,.3)*Walking.Speed*.35f);string pose=n.pose;float lift=n.role=="GreenGoblin"?(h.battle=="celebrate"?0:160):n.pose=="hang"?80+Mathf.Sin((float)visualClock*2)*12:0;
                int hero=Array.IndexOf(SpideyHQ.Heroes,n.role);if(hero>=0 && h.battle=="fight" && h.heroLinks[hero]){point=new Vector2(2220+hero*150,160);pose="web-right";}
                DrawSpideyPerson(id,n.role,pose,point,lift,1,false,visualClock-n.at);
                if(spideyNpcHits.TryGetValue(id,out var hit)){hit.gameObject.SetActive(true);var frame=spideyFrames[id];hit.rectTransform.anchoredPosition=spideyPeople[id].rectTransform.anchoredPosition+(frame.strip.face[frame.frame]-frame.strip.pivot)*frame.strip.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;hit.rectTransform.localScale=Vector3.one*sceneScale;}
            }
            foreach(var prop in h.props){
                var image=spideyProps[prop.id];bool shown=prop.location!="stock";image.gameObject.SetActive(shown);if(!shown)continue;
                string carrier=prop.holder=="goblin"?"hq-npc-GreenGoblin":prop.holder;var art=SpideyArt.Prop(prop.kind,carrier!=""?"held-front":"front");if(art==null)continue;
                image.texture=SpideyArt.Texture(art.texture);image.uvRect=new Rect(0,0,1,1);image.rectTransform.pivot=carrier!=""?art.holdPivot/256:art.pivot/256;image.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                var anchors=prop.kind=="balloon"?spideyLeftHands:spideyHands;
                image.rectTransform.anchoredPosition=carrier!="" && anchors.TryGetValue(carrier,out var hand)?hand:ToBoard(prop.x,prop.y);
            }
            foreach(var pair in spideyWebs){bool linked=h.battle=="fight" || h.battle=="pull";linked&=pair.Key.StartsWith("hq-npc-",StringComparison.Ordinal)?h.heroLinks[Array.IndexOf(SpideyHQ.Heroes,pair.Key.Substring(7))] && spideyPeople[pair.Key].gameObject.activeSelf:h.webActors.Contains(pair.Key);if(!linked || !spideyRightHands.TryGetValue(pair.Key,out var from) || !spideyPositions.ContainsKey("hq-npc-GreenGoblin"))continue;var to=ToBoard(2400,230)+Vector2.up*180*sceneScale;var delta=to-from;pair.Value.gameObject.SetActive(true);pair.Value.rectTransform.anchoredPosition=(from+to)*.5f;pair.Value.rectTransform.sizeDelta=new Vector2(delta.magnitude,3*sceneScale);pair.Value.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
            var cameraArt=SpideyArt.Prop("camera",visualClock-h.photoAt<3 && h.photoAt>=0?"held-front":"front");if(cameraArt!=null){spideyCamera.texture=SpideyArt.Texture(cameraArt.texture);bool held=h.photoAt>=0 && visualClock-h.photoAt<3 && spideyRightHands.ContainsKey(h.photoActor);spideyCamera.rectTransform.pivot=(held?cameraArt.holdPivot:cameraArt.pivot)/256;spideyCamera.rectTransform.sizeDelta=Vector2.one*256*cameraArt.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;spideyCamera.rectTransform.anchoredPosition=held?spideyRightHands[h.photoActor]:ToBoard(SpideyHQ.X("photo"),160);}
            var boardArt=SpideyArt.Prop("board","front");if(boardArt!=null){spideyBoard.texture=SpideyArt.Texture(boardArt.texture);spideyBoard.rectTransform.pivot=boardArt.pivot/256;spideyBoard.rectTransform.sizeDelta=Vector2.one*256*boardArt.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;spideyBoard.rectTransform.anchoredPosition=ToBoard(2400,230)+Vector2.up*150*sceneScale;}
            for(int i=0;i<spideyGlobeDots.Count;i++){float spread=Mathf.Lerp(h.globe==0?1:0,h.globe==1?1:0,Mathf.Clamp01((float)(visualClock-h.globeAt)/.5f));var offset=new[]{new Vector2(-15,9),new Vector2(13,5),new Vector2(0,-14)}[i]*spread;var dot=spideyGlobeDots[i];dot.rectTransform.anchoredPosition=ToBoard(SpideyHQ.X("globe"),269)+new Vector2(offset.x,65+offset.y)*sceneScale;dot.rectTransform.sizeDelta=Vector2.one*8*sceneScale*(1+.4f*Mathf.Sin(Mathf.Clamp01((float)(visualClock-h.globeAt))*Mathf.PI));}
            double posterAge=visualClock-h.posterAt;spideyPoster.texture=SpideyArt.Picture("poster-"+(h.posterAt>=0 && posterAge<.3?(h.poster+2)%3:h.poster));spideyPoster.rectTransform.anchoredPosition=ToBoard(SpideyHQ.X("poster"),230)+Vector2.up*65*sceneScale;spideyPoster.rectTransform.sizeDelta=new Vector2(h.posterAt>=0 && posterAge<.6?Mathf.Max(2,100*Mathf.Abs(Mathf.Cos((float)posterAge/.6f*Mathf.PI))):100,100)*sceneScale;
            if(h.posterAt!=spideyPosterSeen){if(spideyPosterSeen>=0)SpideySound("poster");spideyPosterSeen=h.posterAt;}
            if(h.globeAt!=spideyGlobeSeen){if(spideyGlobeSeen>=0)SpideySound("globe");spideyGlobeSeen=h.globeAt;}
            if(h.dominoAt!=spideyDominoSeen){if(spideyDominoSeen>=0)SpideySound("fall");spideyDominoSeen=h.dominoAt;}
            if(h.photoAt!=spideyPhotoSeen){if(h.photoAt>=0)StartCoroutine(CaptureSpideyPhoto());spideyPhotoSeen=h.photoAt;}
            spideyPhoto.texture=spideyPhotoTexture;spideyPhoto.rectTransform.anchoredPosition=ToBoard(SpideyHQ.X("photo"),290)+Vector2.up*60*sceneScale;spideyPhoto.rectTransform.sizeDelta=new Vector2(155,95)*sceneScale;
            spideyHint.text=h.battle=="fight"?"Help together â€” tap Web!":h.battle=="steal"?"Green Goblin is taking something!":h.battle=="pull"?"Pull together!":h.battle=="celebrate"?"You got it back!":"";
            TickSpideyCreations(h);TickSpideyPanel(h);
        }
        private void DrawSpideyPerson(string id,string role,string pose,Vector2 point,float lift,float alpha,bool mirrored,double age)
        {
            var action=SpideyArt.Action(role,pose)??SpideyArt.Action(role,"idle");if(action==null)return;
            int frame=action.frames==1?0:(int)(Math.Max(0,age)%Math.Max(.001,action.duration)*12)%action.frames;
            if(pose.StartsWith("web-",StringComparison.Ordinal) && age>=action.duration)frame=action.frames-1;
            var image=spideyPeople[id];image.texture=SpideyArt.Texture(action.texture);image.uvRect=BirthdayArt.Uv(action,frame);if(mirrored){var uv=image.uvRect;image.uvRect=new Rect(uv.x+uv.width,uv.y,-uv.width,uv.height);}
            image.rectTransform.pivot=new Vector2(mirrored?1-action.pivot.x/256:action.pivot.x/256,action.pivot.y/256);image.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;image.rectTransform.anchoredPosition=ToBoard(point.x,point.y)+Vector2.up*lift*sceneScale;image.color=new Color(1,1,1,alpha);
            spideyPositions[id]=point;var offset=action.hold[frame]-action.pivot;if(mirrored)offset.x=-offset.x;spideyHands[id]=image.rectTransform.anchoredPosition+offset*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;spideyFrames[id]=(action,frame);
            Vector2 Hand(Vector2 anchor){var delta=anchor-action.pivot;if(mirrored)delta.x=-delta.x;return image.rectTransform.anchoredPosition+delta*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;}
            spideyLeftHands[id]=Hand(action.leftHand[frame]);spideyRightHands[id]=Hand(action.rightHand[frame]);
        }
        private IEnumerator CaptureSpideyPhoto()
        {
            yield return new WaitForSecondsRealtime(1.1f);yield return new WaitForEndOfFrame();if(CurrentArea!=SpideyHQ.Zone)yield break;
            var texture=CaptureZooPicture(g=>g is RawImage image && (spideyTiles.Contains(image) || spideyPeople.Values.Contains(image) || spideyProps.Values.Contains(image) || spideyBricks.Values.Contains(image) || image==spideyPoster || image==spideyBoard) || g is Image detail && (spideyWebs.Values.Contains(detail) || spideyGlobeDots.Contains(detail)),1200);if(spideyPhotoTexture)Destroy(spideyPhotoTexture);spideyPhotoTexture=texture;SpideySound("camera");
        }
        private void AddSpideyDepth(Action<RectTransform,float,int,string> add)
        {
            // A saved HQ visit renders once before BuildSpidey creates its UI.
            if(CurrentArea!=SpideyHQ.Zone || !spideyCamera)return;
            foreach(var hotspot in spideyHotspots)if(hotspot.rect.gameObject.activeSelf)add(hotspot.rect,-10000,9,"hq-hotspot-"+hotspot.rect.name);
            foreach(var pair in spideyPeople)if(pair.Value.gameObject.activeSelf && spideyPositions.TryGetValue(pair.Key,out var point))add(pair.Value.rectTransform,ToBoard(0,point.y).y,1,"hq-"+pair.Key);
            foreach(var pair in spideyBadges)if(pair.Value.gameObject.activeSelf && spideyPositions.TryGetValue(pair.Key,out var badgePoint))add(pair.Value,ToBoard(0,badgePoint.y).y,4,"hq-badge-"+pair.Key);
            foreach(var prop in Spidey.props)if(spideyProps.TryGetValue(prop.id,out var image) && image.gameObject.activeSelf)add(image.rectTransform,ToBoard(0,prop.holder!="" && spideyPositions.TryGetValue(prop.holder,out var point)?point.y:prop.y).y,3,"hq-prop-"+prop.id);
            foreach(var web in spideyWebs)if(web.Value.gameObject.activeSelf)add(web.Value.rectTransform,ToBoard(0,160).y,5,"hq-web-"+web.Key);
            if(spideyCamera.gameObject.activeSelf)add(spideyCamera.rectTransform,ToBoard(0,160).y,4,"hq-camera");if(spideyBoard.gameObject.activeSelf)add(spideyBoard.rectTransform,ToBoard(0,230).y,0,"hq-hoverboard");
            foreach(var brick in spideyBricks)if(brick.Value.gameObject.activeSelf)add(brick.Value.rectTransform,brick.Value.rectTransform.anchoredPosition.y,3,"hq-brick-"+brick.Key);
            foreach(var hit in spideyNpcHits)if(hit.Value.gameObject.activeSelf && spideyPositions.TryGetValue(hit.Key,out var hitPoint))add(hit.Value.rectTransform,ToBoard(0,hitPoint.y).y,4,"hq-npc-hit-"+hit.Key);
        }
        private void ResetSpidey()
        {
            spideyTiles.Clear();spideyPeople.Clear();spideyProps.Clear();spideyPositions.Clear();spideyHands.Clear();spideyFrames.Clear();spideyBadges.Clear();spideyHotspots.Clear();spideyPanelItems.Clear();spideyBricks.Clear();spideyDraggingPiece=-1;spideyDropPending=false;spideyClock.Reset();spideyToolbar=null;spideyPanel=null;spideyWasVisible=spideyPanelOpen=false;CancelSpideyIntent();spideyVisiblePlayers=0;
            spideyLeftHands.Clear();spideyRightHands.Clear();spideyWebs.Clear();spideyGlobeDots.Clear();spideyLastStations.Clear();spideyNpcHits.Clear();if(spideyMusic)Destroy(spideyMusic);if(spideyEffects)Destroy(spideyEffects);if(spideyPhotoTexture)Destroy(spideyPhotoTexture);spideyPhotoTexture=null;
        }
    }
}
