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
        // The shared screen registers the authoritative read/command callbacks
        // at integration. Owned UI can compile while other worlds are edited.
        Func<FunfairState> funfairRead;
        Action<string,string,string,float,float,Action<SoloResult>> funfairDispatch;
        readonly List<RawImage> fairTiles=new List<RawImage>();
        readonly List<RawImage> fairCups=new List<RawImage>();
        readonly Dictionary<string,RawImage> fairPeople=new Dictionary<string,RawImage>(),fairProps=new Dictionary<string,RawImage>(),fairFaces=new Dictionary<string,RawImage>();
        readonly Dictionary<string,Vector2> fairPositions=new Dictionary<string,Vector2>(),fairHands=new Dictionary<string,Vector2>();
        readonly Dictionary<string,RectTransform> fairBadges=new Dictionary<string,RectTransform>();
        readonly Dictionary<string,Texture2D> fairPaintTextures=new Dictionary<string,Texture2D>();
        readonly Dictionary<string,int> fairPaintVersions=new Dictionary<string,int>();
        readonly List<(RectTransform rect,Vector2 point)> fairHotspots=new List<(RectTransform,Vector2)>();
        readonly List<GameObject> fairPanelItems=new List<GameObject>();
        readonly List<RawImage> fairFlowers=new List<RawImage>();
        readonly List<Image> fairPaintCells=new List<Image>();
        readonly BoatVisualClock fairClock=new BoatVisualClock();
        RectTransform fairToolbar,fairPanel;RawImage fairWheel,fairPhoto,fairPanelPerson,fairPanelRod,fairReward,fairRod,fairPaintHead;Image fairFishingLine;Text fairHint,fairPanelStatus;
        AudioSource fairMusic,fairEffects;bool fairVisible;string fairPanelKind="",fairPending="",fairItem="",fairTarget="",fairOutcome="";int fairColour=1,fairVisiblePlayers;double fairSeenPhoto=-1;Texture2D fairPhotoTexture;
        FunfairState Fair=>funfairRead?.Invoke();
        public FunfairState FunfairView=>Fair?.Copy();
        public int VisibleFunfairPlayers=>CurrentArea==PeppaFunfair.Zone?fairVisiblePlayers:0;
        public string FunfairLastOutcome=>fairOutcome;
        string fairMusicKey="";int fairSeenPaint=-1,fairSeenCatch=-1;
        public string FunfairMusicTrack=>fairMusicKey;
        public bool FunfairMusicPlaying=>fairMusic&&fairMusic.isPlaying;
        bool FunfairWalkingApproach=>CurrentArea==PeppaFunfair.Zone&&fairPending!="";
        void CancelFunfairIntent(){fairPending="";}
        RawImage FairImage(string name,Transform parent=null){var rect=Rect(parent?parent:Board,name,Vector2.zero,Vector2.one);var image=rect.gameObject.AddComponent<FunfairImage>();image.raycastTarget=false;return image;}
        Vector2 FairPixel(int section,Vector2 pixel)=>new Vector2((section*1200+pixel.x-cameraX)*sceneScale,(pixel.y*8/9-400)*sceneScale);
        void FairSound(string key){if(fairEffects)StartCoroutine(FunfairArt.Audio(key,c=>{if(fairEffects&&c)fairEffects.PlayOneShot(c,.55f);}));}
        void BuildFunfair(Func<FunfairState> read,Action<string,string,string,float,float,Action<SoloResult>> dispatch)
        {
            funfairRead=read;funfairDispatch=dispatch;if(!FunfairArt.Available||Fair==null)return;
            for(int i=0;i<4;i++){var tile=FairImage("Funfair scenery "+i);tile.texture=FunfairArt.Texture("world-"+i+".png");fairTiles.Add(tile);}
            fairWheel=FairImage("Funfair moving wheel");fairWheel.texture=FunfairArt.Texture("wheel.png");
            fairRod=FairImage("Funfair held fishing rod");fairRod.texture=FunfairArt.Texture("Props/rod.png");fairRod.uvRect=new Rect(1,0,-1,1);
            for(int i=0;i<4;i++){var cup=FairImage("Funfair teacup "+i);cup.texture=FunfairArt.Texture("Props/teacup.png");fairCups.Add(cup);}
            foreach(var m in Fair.members){string id=m.actor;fairPeople[id]=FairImage("Funfair character "+id);fairFaces[id]=FairImage("Funfair painted face "+id);var badge=Panel(Board,"Funfair player "+id,Vector2.zero,new Vector2(34,30),new Color(.96f,.91f,.5f),true,true);fairBadges[id]=badge.rectTransform;Label(badge.transform,(Array.FindIndex(Fair.members,a=>a.actor==id)+1).ToString(),19,Vector2.zero,new Vector2(33,29));NavButton(badge,()=>{if(id!=Actor)FairSend("give",target:id);});}
            foreach(var n in Fair.npcs)fairPeople[NpcId(n)]=FairImage("Funfair resident "+NpcId(n));
            foreach(var p in Fair.props){string id=p.id;var image=FairImage("Funfair prop "+id);image.texture=FunfairArt.Texture("Props/"+p.kind+".png");image.raycastTarget=true;fairProps[id]=image;NavButton(image,()=>{var prop=Fair.props.First(v=>v.id==id);if(prop.location=="ground")FairGo("take",prop.x,prop.y,id);});}
            void Station(string key,string label,string op,string target=""){
                var panel=Panel(Board,"Funfair station "+key,Vector2.zero,new Vector2(126,58),new Color(1,.96f,.82f,.92f),true,true);Label(panel.transform,label,18,Vector2.zero,new Vector2(123,56));fairHotspots.Add((panel.rectTransform,new Vector2(PeppaFunfair.X(key),100)));NavButton(panel,()=>FairGo(op,PeppaFunfair.X(key),150,"",target));
            }
            Station("wheel","Ferris wheel","board","wheel");Station("teacups","Teacups","board","teacups");Station("slide","Spiral slide","slide-join");Station("hook","Hook a Flower","hook-join");Station("paint","Face painting","paint-join");Station("smoothie","Smoothies","panel","smoothie");
            fairToolbar=Rect(safe,"Funfair play controls",new Vector2(0,-286),new Vector2(1080,64));
            var tools=new[]{"Start","Spin","Play","Drink","Balloon","Float","Drop","Cast","Wash","Leave"};for(int i=0;i<tools.Length;i++){string name=tools[i];var b=Button(fairToolbar,name,new Vector2(-495+i*110,0),new Vector2(104,56),()=>FairTool(name),new Color(.93f,.85f,.98f));b.transform.parent.name="Funfair tool "+name;}
            fairHint=Label(safe,"",20,new Vector2(120,250),new Vector2(800,46));fairPhoto=FairImage("Funfair slide keepsake",safe);fairPhoto.rectTransform.anchoredPosition=new Vector2(440,210);fairPhoto.rectTransform.sizeDelta=new Vector2(180,110);
            fairPanel=Panel(safe,"Funfair shared activity",Vector2.zero,new Vector2(1090,650),new Color(1,.97f,.9f,.99f),true).rectTransform;fairPanel.gameObject.SetActive(false);
            fairMusic=gameObject.AddComponent<AudioSource>();fairMusic.playOnAwake=false;fairMusic.loop=true;fairMusic.volume=.18f;fairEffects=gameObject.AddComponent<AudioSource>();fairEffects.playOnAwake=false;
        }
        static string NpcId(FunfairNpc n)=>"fair-npc-"+n.section+"-"+n.role;
        void FairTool(string name){var m=Fair.members.Single(v=>v.actor==Actor);switch(name){case "Start":FairSend("ride-start",target:m.station);break;case "Spin":FairSend("ride-spin",target:m.station);break;case "Play":if(m.station=="hook"||m.station=="paint")FairOpen(m.station);else if(m.station=="slide")FairOpen("slide");else FairOpen("prizes");break;case "Drink":FairSend(Fair.props.Any(p=>p.holder==Actor&&p.kind=="drink")?"drink":"drink-take");break;case "Balloon":FairGo("balloon-take",PeppaFunfair.X("slide"),150);break;case "Float":FairSend("float");break;case "Drop":FairSend("drop");break;case "Cast":FairOpen("cast");break;case "Wash":FairSend("paint-clear");break;case "Leave":FairClose();FairSend("leave");break;}}
        void FairSend(string op,string item="",string target="",float x=0,float y=0,Action<SoloResult> done=null){if(ActionPending||CurrentArea!=PeppaFunfair.Zone)return;destination=null;if(Shared)shared.Walk(WalkMode.Stop);funfairDispatch(op,item,target,x,y,result=>{fairOutcome=result.Outcome;if(!result.Accepted)message.text=Friendly(result.Outcome);done?.Invoke(result);Render();});}
        void FairGo(string op,float x,float y,string item="",string target=""){if(MenuOpen||TravelPending)return;fairPending=op;fairItem=item;fairTarget=target;destination=new Vector2(x,y);}
        void TickFunfair()
        {
            if(fairTiles.Count==0)return;bool visible=CurrentArea==PeppaFunfair.Zone;foreach(var image in fairTiles.Concat(fairPeople.Values).Concat(fairProps.Values).Concat(fairFaces.Values))image.gameObject.SetActive(visible);foreach(var badge in fairBadges.Values)badge.gameObject.SetActive(visible);foreach(var hit in fairHotspots)hit.rect.gameObject.SetActive(visible);fairWheel.gameObject.SetActive(visible);fairToolbar.gameObject.SetActive(visible&&fairPanelKind=="");fairHint.gameObject.SetActive(visible);fairPhoto.gameObject.SetActive(visible&&fairPhotoTexture);
            fairMusic.mute=fairEffects.mute=musicMuted||applicationPaused;fairMusic.volume=.18f*ForegroundDucking;
            if(fairVisible!=visible){fairVisible=visible;if(!visible){fairMusicKey="";CancelFunfairIntent();FairClose();fairMusic.Stop();characterVisual.gameObject.SetActive(true);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(true);}}
            foreach(var cup in fairCups)cup.gameObject.SetActive(visible);fairRod.gameObject.SetActive(false);
            if(!visible)return;characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);if(miniGamesButton)miniGamesButton.gameObject.SetActive(false);var h=Fair;if(h==null)return;double clock=fairClock.Sample(Shared?shared.View.worldId:World.WorldId,h.clock,Time.realtimeSinceStartupAsDouble,Shared);
            int section=Mathf.Clamp((int)(ReadPlayer(Actor).x/1200),0,3);string music=section==1?"hook":section==2?"slide":section==3?"paint":h.wheel.stage=="running"?"wheel":"music";
            if(fairMusicKey!=music){fairMusicKey=music;StartCoroutine(FunfairArt.Audio(music,c=>{if(fairMusic&&CurrentArea==PeppaFunfair.Zone&&fairMusicKey==music){fairMusic.clip=c;fairMusic.Play();}}));}
            if(fairSeenCatch>=0&&h.catches>fairSeenCatch)FairSound("reward");if(fairSeenPaint>=0&&h.paintings>fairSeenPaint)FairSound("reveal");fairSeenCatch=h.catches;fairSeenPaint=h.paintings;
            if(fairPending!=""&&destination.HasValue&&!ActionPending){var p=ReadPlayer(Actor);if(Vector2.Distance(new Vector2(p.x,p.y),destination.Value)<60){string op=fairPending;fairPending="";if(op=="panel"){destination=null;FairOpen(fairTarget);}else FairSend(op,fairItem,fairTarget,done:r=>{if(r.Accepted&&(op=="hook-join"||op=="paint-join"||op=="slide-join"))FairOpen(op.Split('-')[0]);});}}
            for(int i=3;i>=0;i--){var tile=fairTiles[i];tile.rectTransform.anchoredPosition=new Vector2((i*1200+600-cameraX)*sceneScale,0);tile.rectTransform.sizeDelta=new Vector2(1200,800)*sceneScale;tile.rectTransform.SetAsFirstSibling();}
            foreach(var hit in fairHotspots){hit.rect.anchoredPosition=ToBoard(hit.point.x,hit.point.y);hit.rect.localScale=Vector3.one*sceneScale;}
            var wheel=FunfairArt.WheelData;int wheelFrame=((int)(h.wheel.angle/5)%72+72)%72;fairWheel.uvRect=new Rect(wheelFrame%8/8f,(8-wheelFrame/8)/9f,1f/8,1f/9);fairWheel.rectTransform.anchoredPosition=FairPixel(0,wheel.pixels.center);fairWheel.rectTransform.sizeDelta=new Vector2(wheel.pixels.width,wheel.pixels.height*8/9)*sceneScale;
            for(int i=0;i<4;i++){float angle=(h.teacups.angle+i*90)*Mathf.Deg2Rad;var cup=fairCups[i];cup.rectTransform.anchoredPosition=ToBoard(310+Mathf.Cos(angle)*100,190+Mathf.Sin(angle)*35)+Vector2.up*20*sceneScale;cup.rectTransform.sizeDelta=Vector2.one*125*sceneScale;cup.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Sin(h.teacups.angle*Mathf.Deg2Rad)*8);}
            fairVisiblePlayers=0;
            foreach(var m in h.members){var p=ReadPlayer(m.actor);bool shown=p.zone==PeppaFunfair.Zone&&(Shared?shared.Players.Contains(m.actor):m.actor==Actor);fairPeople[m.actor].gameObject.SetActive(shown);fairBadges[m.actor].gameObject.SetActive(shown);fairFaces[m.actor].gameObject.SetActive(false);if(!shown)continue;fairVisiblePlayers++;var point=Shared?shared.VisualPosition(m.actor):new Vector2(p.x,p.y);var previous=fairPositions.TryGetValue(m.actor,out var old)?old:point;bool walking=Vector2.Distance(point,previous)>.1f;string pose=m.station==""?(walking?"walk":clock-m.at<2&&m.pose=="happy"?"happy":"idle"):m.pose;float lift=m.station=="float"?80+Mathf.Sin((float)(clock-m.at)*2)*12:0;Vector2? exact=null;
                if(m.station=="wheel"){int cabin=m.seat%3;var pixel=wheel.seats[wheelFrame*3+cabin];if(m.seat==3)pixel.x+=36;exact=FairPixel(0,pixel);pose="sit";}
                if(m.station=="teacups"){float a=(h.teacups.angle+m.seat*90)*Mathf.Deg2Rad;point=new Vector2(310+Mathf.Cos(a)*100,190+Mathf.Sin(a)*35);pose="sit";}
                if(m.station=="slide"){int q=Array.IndexOf(h.slideQueue,m.actor);if(q==0){double age=clock-m.at;if(age<3){var start=FunfairArt.Slide(0);exact=FairPixel(2,Vector2.Lerp(new Vector2(start.x,80),start,(float)age/3));pose="walk";}else{exact=FairPixel(2,FunfairArt.Slide(age-3));pose=new[]{"slide-back","slide-belly","slide-spin"}[m.slidePose];}}else point=new Vector2(2900-q*55,150);}
                if(m.station=="hook")point=new Vector2(1760+Math.Max(0,Array.IndexOf(h.hookQueue,m.actor))*60,180);if(m.station=="paint")point=new Vector2(4160+Math.Max(0,Array.IndexOf(h.paintQueue,m.actor))*65,160);
                if(walking&&m.station=="")pose+=point.y-previous.y>Math.Abs(point.x-previous.x)*.5f?"-back":"-side";
                var frame=FairPerson(m.actor,m.role,pose,point,exact,lift,walking&&point.x<previous.x,clock-m.at);var anchor=FairFacePoint(frame,m.actor);fairBadges[m.actor].anchoredPosition=anchor+Vector2.up*36*sceneScale;fairBadges[m.actor].localScale=Vector3.one*sceneScale;
                if(m.paintStyle>0){if(!fairPaintVersions.TryGetValue(m.actor,out int rev)||rev!=m.paintRevision){if(fairPaintTextures.TryGetValue(m.actor,out var oldTexture))Destroy(oldTexture);fairPaintTextures[m.actor]=PaintTexture(m.paint,m.paintStyle);fairPaintVersions[m.actor]=m.paintRevision;}var face=fairFaces[m.actor];face.gameObject.SetActive(true);face.texture=fairPaintTextures[m.actor];face.rectTransform.anchoredPosition=anchor+Vector2.up*10*sceneScale;face.rectTransform.sizeDelta=new Vector2(38,40)*sceneScale;}
            }
            foreach(var n in h.npcs){string id=NpcId(n);bool shown=!h.members.Any(m=>m.role==n.role&&ReadPlayer(m.actor).zone==PeppaFunfair.Zone&&(int)(ReadPlayer(m.actor).x/1200)==n.section&&(Shared?shared.Players.Contains(m.actor):m.actor==Actor));fairPeople[id].gameObject.SetActive(shown);if(shown)FairPerson(id,n.role,n.pose=="walk"?"walk-side":n.pose,new Vector2(n.x,n.y),null,0,n.targetX<n.x,clock-n.at);}
            foreach(var p in h.props){var image=fairProps[p.id];image.gameObject.SetActive(p.location!="stock");image.rectTransform.sizeDelta=Vector2.one*(p.kind=="balloon"?90:58)*sceneScale;image.rectTransform.anchoredPosition=p.holder!=""&&fairHands.TryGetValue(p.holder,out var hand)?hand+Vector2.up*(p.kind=="balloon"?40:0)*sceneScale:ToBoard(p.x,p.y);}
            foreach(var p in h.props){var art=FunfairArt.Prop(p.kind);if(art==null)continue;var image=fairProps[p.id];image.rectTransform.pivot=art.holdPivot/256;image.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*90*sceneScale;}
            fairRod.gameObject.SetActive(h.hookActor!=""&&fairHands.ContainsKey(h.hookActor));if(fairRod.gameObject.activeSelf){var art=FunfairArt.Prop("rod");fairRod.rectTransform.pivot=new Vector2(1-art.holdPivot.x/256,art.holdPivot.y/256);fairRod.rectTransform.sizeDelta=Vector2.one*256*art.unitsPerPixel*90*sceneScale;fairRod.rectTransform.anchoredPosition=fairHands[h.hookActor];}
            if(h.photoAt!=fairSeenPhoto){if(h.photoAt>=0){var m=h.members.FirstOrDefault(v=>v.actor==h.photoActor);if(m!=null){fairPhotoTexture=FunfairArt.Picture("photo-"+(string.IsNullOrEmpty(h.photoRole)?m.role:h.photoRole)+"-"+(h.photoPose+1));fairPhoto.texture=fairPhotoTexture;FairSound("camera");}}fairSeenPhoto=h.photoAt;}
            var own=h.members.Single(v=>v.actor==Actor);fairHint.text=own.station=="wheel"||own.station=="teacups"?(own.station=="wheel"?h.wheel.stage:h.teacups.stage)=="boarding"?"Everyone aboard? Tap Start!":"Spin together!":own.station=="slide"?"One shared slide queue":own.station=="paint"?"Paint your friend's face together":own.station=="hook"?"Take turns hooking a flower":"Choose a ride or activity";TickFairPanel(h,clock);
        }
        (BirthdayArt.Frames strip,int frame) FairPerson(string id,string role,string pose,Vector2 point,Vector2? exact,float lift,bool mirror,double age){var a=FunfairArt.Action(role,pose);int f=(int)(Math.Max(0,age)%Math.Max(.0833,a.duration)*12)%a.frames;var image=fairPeople[id];image.texture=FunfairArt.Texture(a.texture);image.uvRect=BirthdayArt.Uv(a,f);if(mirror){var uv=image.uvRect;image.uvRect=new Rect(uv.x+uv.width,uv.y,-uv.width,uv.height);}float scale=a.unitsPerPixel*90*sceneScale;image.rectTransform.pivot=new Vector2(mirror?1-a.pivot.x/256:a.pivot.x/256,a.pivot.y/256);image.rectTransform.sizeDelta=Vector2.one*256*scale;image.rectTransform.anchoredPosition=(exact??ToBoard(point.x,point.y))+Vector2.up*lift*sceneScale;fairPositions[id]=point;var hand=a.rightHand[f]-a.pivot;if(mirror)hand.x=-hand.x;fairHands[id]=image.rectTransform.anchoredPosition+hand*scale;return(a,f);}
        Vector2 FairFacePoint((BirthdayArt.Frames strip,int frame) row,string id)=>fairPeople[id].rectTransform.anchoredPosition+(row.strip.face[row.frame]-row.strip.pivot)*row.strip.unitsPerPixel*90*sceneScale;
        static readonly Color[] FairColours={new Color(1,.52f,.12f),new Color(.15f,.65f,.2f),new Color(.6f,.25f,.85f),new Color(.2f,.55f,.95f),new Color(.98f,.23f,.4f),new Color(1,.85f,.1f),Color.white,new Color(.2f,.16f,.25f)};
        static Texture2D PaintTexture(int[] cells,int style){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color32[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=(x-31.5f)/31.5f,dy=(y-31.5f)/31.5f;int c=cells[y/8*8+x/8];if(c>0&&dx*dx+dy*dy<1){var color=FairColours[c-1];color.a=.5f;pixels[y*64+x]=color;}}t.SetPixels32(pixels);t.Apply();return t;}
        void AddFunfairDepth(Action<RectTransform,float,int,string> add){if(CurrentArea!=PeppaFunfair.Zone||!fairWheel)return;add(fairWheel.rectTransform,10000,0,"fair-wheel");for(int i=0;i<fairCups.Count;i++)add(fairCups[i].rectTransform,fairCups[i].rectTransform.anchoredPosition.y-20*sceneScale,2,"fair-cup-"+i);if(fairRod.gameObject.activeSelf)add(fairRod.rectTransform,ToBoard(0,180).y,4,"fair-rod");foreach(var p in fairPeople)if(p.Value.gameObject.activeSelf)add(p.Value.rectTransform,fairPositions.TryGetValue(p.Key,out var pos)?ToBoard(0,pos.y).y:0,1,"fair-"+p.Key);foreach(var p in fairFaces)if(p.Value.gameObject.activeSelf)add(p.Value.rectTransform,p.Value.rectTransform.anchoredPosition.y,5,"fair-face-"+p.Key);foreach(var p in fairProps)if(p.Value.gameObject.activeSelf)add(p.Value.rectTransform,p.Value.rectTransform.anchoredPosition.y,3,"fair-prop-"+p.Key);foreach(var p in fairBadges)if(p.Value.gameObject.activeSelf)add(p.Value,p.Value.anchoredPosition.y,6,"fair-badge-"+p.Key);foreach(var hit in fairHotspots)add(hit.rect,-10000,9,"fair-hit-"+hit.rect.name);}
        void ResetFunfair(){fairTiles.Clear();fairCups.Clear();fairPeople.Clear();fairProps.Clear();fairFaces.Clear();fairBadges.Clear();fairHotspots.Clear();fairPanelItems.Clear();fairFlowers.Clear();fairPaintCells.Clear();fairPositions.Clear();fairHands.Clear();fairPaintVersions.Clear();foreach(var t in fairPaintTextures.Values)Destroy(t);fairPaintTextures.Clear();if(fairMusic)Destroy(fairMusic);if(fairEffects)Destroy(fairEffects);fairClock.Reset();fairVisible=false;fairMusicKey="";fairSeenPaint=fairSeenCatch=-1;fairPanelKind="";fairPending="";fairSeenPhoto=-1;fairPhotoTexture=null;funfairRead=null;funfairDispatch=null;}
    }
}
