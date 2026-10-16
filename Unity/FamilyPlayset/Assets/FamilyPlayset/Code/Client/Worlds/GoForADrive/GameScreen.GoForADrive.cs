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
        private RectTransform driveRoot,driveControls,drivePalette;
        private RawImage driveBackdrop,driveCar,driveNomad,drivePeanut,driveObstacle,driveReward,driveWindow,driveDad,driveCow,driveStop,driveNozzle;
        private Image driveHose;
        private Text driveTitle,driveHint,driveProgress;
        private readonly Dictionary<string,RawImage> driveActors=new Dictionary<string,RawImage>(),driveProps=new Dictionary<string,RawImage>(),driveBricks=new Dictionary<string,RawImage>(),driveHands=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Text> driveNames=new Dictionary<string,Text>();
        private readonly Dictionary<string,Vector2> driveHandPoints=new Dictionary<string,Vector2>(),driveFeet=new Dictionary<string,Vector2>();
        private readonly List<RawImage> driveStars=new List<RawImage>();
        private readonly List<Image> drivePegs=new List<Image>();
        private readonly List<RawImage> driveBuildTargets=new List<RawImage>(),drivePalettePictures=new List<RawImage>();
        private readonly Dictionary<string,Button> driveButtons=new Dictionary<string,Button>();
        private string driveControlKey="",driveOutcome="",driveFeedback="";
        private float driveFeedbackUntil;
        private double driveSeenEvent=-2;
        private int driveVisible,drivePalettePage,driveMusicStage=-1;
        private AudioSource driveMusic,driveEffects;
        private bool driveWasVisible;
        private readonly BoatVisualClock driveClock=new BoatVisualClock();
        private static readonly string[] DriveRoles={"bluey","bingo","chilli","bandit"};
        private DriveState Drive=>Shared?shared.View.drive:World.ReadDrive();
        public DriveState DriveView=>HasWorld?Drive?.Copy():null;
        public int VisibleDrivePlayers=>CurrentArea==GoForADrive.Zone?driveVisible:0;
        public string DriveLastOutcome=>driveOutcome;
        private static Vector2 DrivePoint(float x,float y)=>new Vector2((x-500)*1.08f,(y-300)*.88f+12);
        private RawImage DriveImage(string name,Transform parent=null)
        {
            var image=Rect(parent?parent:driveRoot,name,Vector2.zero,Vector2.one).gameObject.AddComponent<RawImage>();image.raycastTarget=false;return image;
        }
        private static void DrivePicture(RawImage image,string key,Vector2 position,Vector2 size)
        {
            var texture=GoForADriveArt.Picture(key);image.gameObject.SetActive(texture);if(!texture)return;
            float scale=Mathf.Min(size.x/texture.width,size.y/texture.height);
            image.texture=texture;image.rectTransform.anchoredPosition=position;image.rectTransform.sizeDelta=new Vector2(texture.width,texture.height)*scale;image.uvRect=new Rect(0,0,1,1);
        }
        private static void DriveHeldPicture(RawImage image,string key,Vector2 hand,Vector2 size)
        {
            DrivePicture(image,key,hand,size);var entry=GoForADriveArt.PictureInfo(key);if(entry==null || !image.texture)return;
            float scale=image.rectTransform.sizeDelta.x/image.texture.width;
            image.rectTransform.anchoredPosition=hand+(new Vector2(image.texture.width,image.texture.height)*.5f-entry.holdPivot)*scale;
        }
        private static void DriveStatuePicture(RawImage image,int variant)
        {
            var row=GoForADriveArt.Data.statueLayout.rows.Single(r=>r.index==variant);var b=row.bounds;
            DrivePicture(image,"brick-"+variant,new Vector2((b.x+b.width*.5f-.5f)*1120,(.5f-b.y-b.height*.5f)*630),new Vector2(b.width*1120,b.height*630));
        }
        private void BuildDrive()
        {
            if(SceneSchema<GoForADrive.Schema || !GoForADriveArt.Available)return;
            driveRoot=Panel(safe,"Go for a Drive world",new Vector2(0,5),new Vector2(1120,630),Color.white,true).rectTransform;
            driveRoot.gameObject.AddComponent<RectMask2D>();driveBackdrop=DriveImage("Drive recovered scenery");driveBackdrop.rectTransform.sizeDelta=new Vector2(1120,630);
            driveBackdrop.raycastTarget=true;driveBackdrop.gameObject.AddComponent<DriveSurface>().clicked=point=>{
                if(Drive.stage!=3 || !Drive.freeBuild)return;var held=Drive.bricks.SingleOrDefault(b=>b.holder==Actor);
                if(held!=null)DriveSend("brick-place",held.id,held.revision.ToString(),Mathf.Clamp(point.x/1.08f+500,0,1000),Mathf.Clamp((point.y-12)/.88f+300,0,600));
            };
            driveCar=DriveImage("Drive Heeler car");driveNomad=DriveImage("Drive Grey Nomads");driveCow=DriveImage("Drive roadside cow");drivePeanut=DriveImage("Drive Big Peanut");driveObstacle=DriveImage("Drive window obstacle");driveReward=DriveImage("Drive physical star reward");driveDad=DriveImage("Drive Dad assistance");driveStop=DriveImage("Drive roadside fixture");
            foreach(var member in Drive.members){
                string id=member.actor;driveActors[id]=DriveImage("Drive character "+id);
                driveNames[id]=Label(driveRoot,"",17,Vector2.zero,new Vector2(96,28));
                var hand=DriveImage("Drive window hand "+id);driveHands[id]=hand;
                if(id==Actor){hand.raycastTarget=true;var input=hand.gameObject.AddComponent<DriveHandInput>();input.surface=driveRoot;input.tapped=()=>DriveSend("jump");input.moved=y=>DriveSend("hand",y:y);input.released=()=>DriveSend("hand-release");}
            }
            driveHose=Plain(driveRoot,"Drive attached hose",Vector2.zero,new Vector2(1,4),new Color(.18f,.22f,.24f));driveNozzle=DriveImage("Drive original nozzle");
            driveWindow=DriveImage("Drive source window frame");
            for(int i=0;i<12;i++){
                int peg=i;var image=Panel(driveRoot,"Drive car peg "+i,DrivePoint(600+i%3*80,345+i/3*45),Vector2.one*38,new Color(.92f,.98f,1,.7f),true,true);
                NavButton(image,()=>{var m=Drive.members.Single(a=>a.actor==Actor);var p=Drive.items.SingleOrDefault(a=>a.id==m.held);if(p!=null)DriveSend("pack-place",p.id,p.revision.ToString(),peg);});drivePegs.Add(image);
            }
            for(int i=0;i<10;i++)driveStars.Add(DriveImage("Drive common sticker "+i));
            for(int i=0;i<12;i++){
                int slot=i;var target=DriveImage("Drive statue place "+i);target.raycastTarget=true;NavButton(target,()=>{
                    var held=Drive.bricks.SingleOrDefault(b=>b.holder==Actor);
                    if(held!=null)DriveSend("brick-place",held.id,held.revision.ToString(),GoForADriveLayout.StatueX[slot],GoForADriveLayout.StatueY[slot]);
                });driveBuildTargets.Add(target);
            }
            drivePalette=Panel(driveRoot,"Drive original brick palette",new Vector2(-350,15),new Vector2(300,275),new Color(.97f,.99f,1,.94f),true).rectTransform;
            for(int i=0;i<12;i++){
                int item=i;var picture=DriveImage("Drive palette item "+i,drivePalette);picture.raycastTarget=true;
                NavButton(picture,()=>DriveSend("brick-new",(12+drivePalettePage*12+item).ToString()));drivePalettePictures.Add(picture);
            }
            Button(drivePalette,"<",new Vector2(-110,110),new Vector2(48,40),()=>drivePalettePage=(drivePalettePage+2)%3,Color.white);
            Button(drivePalette,">",new Vector2(110,110),new Vector2(48,40),()=>drivePalettePage=(drivePalettePage+1)%3,Color.white);
            driveTitle=Label(driveRoot,"",25,new Vector2(0,282),new Vector2(860,40));
            driveProgress=Label(driveRoot,"",21,new Vector2(0,242),new Vector2(900,36));
            driveHint=Label(driveRoot,"",18,new Vector2(0,-200),new Vector2(1040,38));
            Panel(driveRoot,"Drive lower control dock",new Vector2(0,-258),new Vector2(1120,114),new Color(.86f,.94f,.77f));
            driveControls=Rect(driveRoot,"Drive pictured controls",new Vector2(0,-267),new Vector2(1080,76));
            driveMusic=gameObject.AddComponent<AudioSource>();driveMusic.playOnAwake=false;driveMusic.loop=true;driveMusic.volume=.19f;
            driveEffects=gameObject.AddComponent<AudioSource>();driveEffects.playOnAwake=false;
            driveRoot.gameObject.SetActive(false);
        }
        private void DriveSend(string op,string item="",string target="",float x=0,float y=0)
        {
            if(ActionPending || CurrentArea!=GoForADrive.Zone)return;destination=null;if(Shared)shared.Walk(WalkMode.Stop);
            void Done(SoloResult result){driveOutcome=result.Outcome;if(!result.Accepted){driveFeedback=result.Outcome.Replace('-',' ');driveFeedbackUntil=Time.unscaledTime+2.5f;}Render();}
            if(Shared)SubmitShared(SoloAction.Drive,item,target,op,x,y,Done);
            else Done(Command(SoloAction.Drive,item:item,target:target,value:op,x:x,y:y));
        }
        private void DriveSound(string key)
        {if(driveEffects)StartCoroutine(GoForADriveArt.LoadAudio(key,clip=>{if(driveEffects && clip)driveEffects.PlayOneShot(clip,.5f);}));}
        private void DriveBuildControls(DriveState d)
        {
            var local=d.members.Single(m=>m.actor==Actor);
            string key=d.stage+"|"+d.stop+"|"+d.begun+"|"+local.joined+"|"+d.freeBuild;
            if(key==driveControlKey)return;driveControlKey=key;
            foreach(Transform child in driveControls)Destroy(child.gameObject);driveButtons.Clear();
            var choices=new List<(string id,string label,string icon,Action click)>();
            void Add(string id,string label,string icon,Action click)=>choices.Add((id,label,icon,click));
            if(!local.joined){Add(d.begun?"join":"start",d.begun?"Join trip":"Start trip","car",()=>DriveSend(d.begun?"join":"start"));}
            else{
                if(d.stage==0){
                    Add("balance-left","Steady left","car",()=>DriveSend("balance",x:-1));Add("balance-right","Steady right","car",()=>DriveSend("balance",x:1));
                    Add("drop","Put down","packing-0-0",()=>{var p=Drive.items.SingleOrDefault(v=>v.holder==Actor);if(p!=null)DriveSend("pack-drop",p.id,p.revision.ToString());});
                    Add("door","Door","house",()=>DriveSend("door"));Add("next","Go!","car",()=>DriveSend("next"));
                }else if(d.stage==1){
                    if(d.stop==""){
                        for(int i=0;i<3;i++){int lane=i;Add("lane-"+i,new[]{"Near lane","Middle lane","Far lane"}[i],"car",()=>DriveSend("lane",lane.ToString()));}
                        Add("boost","Let's go!","nomad",()=>DriveSend("boost"));Add("horn","Beep beep","car",()=>DriveSend("horn"));Add("next","Window play","window-frame",()=>DriveSend("next"));
                    }else{
                        string op=d.stop=="petrol"?"fuel":d.stop=="toilet"?"toilet":"rubble";
                        Add(op,d.stop=="petrol"?"Nozzle":d.stop=="toilet"?"Toilet":"Clear rubble",d.stop=="petrol"?"fuel-icon":d.stop=="toilet"?"toilet":"rubble",()=>DriveSend(op));
                        if(d.stop=="roadworks")Add("light","Traffic light","traffic-red",()=>DriveSend("light"));
                        Add("cow","Say hello","cow",()=>DriveSend("cow"));Add("continue","Back in car","car",()=>DriveSend("continue"));
                    }
                }else if(d.stage==2){Add("jump","Jump / jump again","hand-up",()=>DriveSend("jump"));Add("next","Big Peanut","peanut",()=>DriveSend("next"));}
                else{
                    Add("turn","Turn brick","brick-0",()=>{var b=Drive.bricks.SingleOrDefault(v=>v.holder==Actor);if(b!=null)DriveSend("brick-turn",b.id,b.revision.ToString());});
                    Add("free-build",d.freeBuild?"Build Peanut":"Free build","peanut",()=>DriveSend("free-build"));
                    Add("smash","Tumble!","car",()=>DriveSend("smash"));Add("replay","Another trip","car",()=>DriveSend("replay"));
                }
                Add("wave","Wave","bluey-wave",()=>DriveSend("wave"));Add("leave","Leave trip","car",()=>DriveSend("leave"));
            }
            float width=Math.Min(140,1060f/choices.Count);
            for(int i=0;i<choices.Count;i++){
                var choice=choices[i];var text=Button(driveControls,choice.label,new Vector2((i-(choices.Count-1)*.5f)*width,0),new Vector2(width-7,74),()=>choice.click(),new Color(.94f,.98f,1));
                text.transform.parent.name="Drive "+choice.id;text.fontSize=15;text.rectTransform.anchoredPosition=new Vector2(0,-21);text.rectTransform.sizeDelta=new Vector2(width-9,25);
                var icon=DriveImage("Drive picture "+choice.id,text.transform.parent);DrivePicture(icon,choice.icon,new Vector2(0,12),new Vector2(60,38));
                driveButtons[choice.id]=text.transform.parent.GetComponent<Button>();
            }
        }
        private Vector2 DrawDriveActor(RawImage image,string role,string pose,Vector2 position,double clock,float size=145)
        {
            string action=pose=="sit"?"sitting":pose=="drive"?"driving":pose=="cheer"?"happy":pose=="wave" || pose=="cow"?"wave":pose=="jump" || pose=="double-jump"?"happy":pose;
            var strip=GoForADriveArt.Action(role,action)??GoForADriveArt.Action(role,"idle");image.gameObject.SetActive(strip!=null);if(strip==null)return position;
            int frame=strip.frames==1?0:(int)(Math.Max(0,clock)/Math.Max(.05,strip.duration)*strip.frames)%strip.frames;
            float scale=size/strip.size;
            image.texture=GoForADriveArt.Texture(strip.texture);image.uvRect=BirthdayArt.Uv(strip,frame);image.rectTransform.anchoredPosition=position+(Vector2.one*(strip.size*.5f)-strip.pivot)*scale;image.rectTransform.sizeDelta=Vector2.one*size;
            return position+(strip.hold[frame]-strip.pivot)*scale;
        }
        private void TickDrive()
        {
            if(!driveRoot)return;bool visible=CurrentArea==GoForADrive.Zone;driveRoot.gameObject.SetActive(visible && !CharactersOpen && !WorldLoading);
            if(visible!=driveWasVisible){
                driveWasVisible=visible;
                if(visible)driveMusicStage=-1;
                else{driveMusic.Stop();driveVisible=0;}
            }
            if(!visible)return;
            characterVisual.gameObject.SetActive(false);foreach(var friend in friends.Values)friend.view.gameObject.SetActive(false);
            if(miniGamesButton)miniGamesButton.gameObject.SetActive(false);if(stick)stick.gameObject.SetActive(false);
            var d=Drive;if(d==null)return;DriveBuildControls(d);
            double visualClock=driveClock.Sample(Shared?shared.View.worldId:World.WorldId,d.clock,Time.realtimeSinceStartupAsDouble,Shared);
            if(Math.Abs(visualClock-d.clock)>1){driveClock.Reset();visualClock=d.clock;}
            if(driveMusicStage!=d.stage){driveMusicStage=d.stage;int stage=d.stage;StartCoroutine(GoForADriveArt.LoadAudio(new[]{"music-packing","music-driving","music-window","music-building"}[stage],clip=>{if(driveMusic && CurrentArea==GoForADrive.Zone && driveMusicStage==stage){driveMusic.clip=clip;driveMusic.Play();}}));}
            string[] stages={"Pack the car","A drive with Dad","Window games","Build a Big Peanut"};driveTitle.text=stages[d.stage];
            string[] backgrounds={"packing-background","driving-background","window-background","building-background"};
            driveBackdrop.texture=GoForADriveArt.Picture(backgrounds[d.stage]);driveBackdrop.uvRect=new Rect(0,0,1,1);
            driveProgress.text=!d.begun?"One car. One adventure together.":d.stage==0?"Packed "+d.items.Count(p=>p.slot>=0)+" / "+d.items.Length+"  |  Wave "+d.packingWave+" / "+GoForADrive.Difficulty[d.difficulty]:d.stage==1?(d.stop==""?"Nomads passed "+d.passed+" / "+GoForADrive.RoadTarget(d)+"  |  Stops "+d.stopIndex+" / 3":d.stop=="petrol"?"Fuel "+Mathf.RoundToInt(d.fuel*100)+"%":d.stopDone?"Ready to go!":d.stop=="roadworks"?"Clear the rubble and watch the light":"A little stop"):d.stage==2?"Shared stars "+d.stars:d.freeBuild?"Build something together":d.statueBuilt?"You built the Big Peanut!":"Peanut pieces "+d.bricks.Count(b=>b.snapped)+" / 12";
            driveHint.text=!d.begun?"Start invites everyone here to join the same trip.":d.members.Single(m=>m.actor==Actor).joined?(d.stage==0?d.packingClear?"All packed! Let's go with Dad.":"Pick an item, then choose a free peg on the car.":d.stage==1?d.stop==""?"Choose a clear lane and cheer Dad past the Grey Nomads.":d.stop=="petrol"?"Attach the nozzle to fill. Tap again to put it back.":"Help at the stop; the family's trip waits here.":d.stage==2?"Share one hand: tap to jump, tap again higher, or drag it over the animals.":"Pick up a piece, then tap its matching place or a free building space."):"Your family can keep playing. Join again whenever you like.";
            if(Time.unscaledTime<driveFeedbackUntil)driveHint.text=driveFeedback;
            if(driveButtons.TryGetValue("next",out var next))next.interactable=d.stage==0?d.packingClear:d.stage==1?d.stopIndex==3:true;
            if(driveButtons.TryGetValue("continue",out var resume))resume.interactable=d.stopDone;
            if(driveButtons.TryGetValue("replay",out var replay)){
                bool ready=d.members.Single(m=>m.actor==Actor).readyTrip==d.trip;replay.GetComponentInChildren<Text>().text=ready?"Waiting for family":"Another trip";
            }
            float carX=d.stage==0?680:360;
            if(d.stage==3 && d.smashing)carX=Mathf.Lerp(250,510,Mathf.Clamp01((float)(d.clock-d.smashAt)));
            DrivePicture(driveCar,"car",DrivePoint(carX,d.stage==1?145+d.lane*50:230),new Vector2(390,300));driveCar.gameObject.SetActive(d.stage!=2);driveCar.rectTransform.localEulerAngles=new Vector3(0,0,d.stage==0?d.balance*12:d.smashing?Mathf.Sin((float)(d.clock-d.smashAt)*Mathf.PI)*10:0);
            DrivePicture(driveNomad,d.roadStep%4==0?"nomad":"traffic-"+(d.roadStep%4-1),DrivePoint(800-(float)Math.Min(1,(d.clock-d.roadAt)/1.5)*(d.nomadPassed?850:0),145+d.nomadLane*50),new Vector2(265,210));driveNomad.gameObject.SetActive(d.stage==1 && d.stop=="" && d.stopIndex<3);
            DrivePicture(driveCow,"cow",DrivePoint(790,240),new Vector2(150,145));driveCow.gameObject.SetActive(d.stage==1 && d.stop!="");
            DrivePicture(driveStop,d.stop=="petrol"?"petrol-stop":d.stop=="toilet"?"toilet":"rubble",DrivePoint(730,270),new Vector2(260,270));driveStop.gameObject.SetActive(d.stage==1 && d.stop!="");
            drivePeanut.gameObject.SetActive(false);
            DrivePicture(driveWindow,"window-frame",Vector2.zero,new Vector2(1040,530));driveWindow.gameObject.SetActive(d.stage==2);
            // The original ground ends above the recovered camera edge. A native
            // bottom control dock covers that finite boundary without shifting snap anchors.
            driveControls.GetComponent<RectTransform>().SetAsLastSibling();
            var own=d.members.Single(m=>m.actor==Actor);
            for(int i=0;i<drivePegs.Count;i++)drivePegs[i].gameObject.SetActive(d.stage==0 && own.held!="" && !d.items.Any(p=>p.slot==i));
            driveVisible=0;
            for(int i=0;i<d.members.Length;i++){
                var m=d.members[i];bool shown=ReadPlayer(m.actor).zone==GoForADrive.Zone && (Shared?shared.Players.Contains(m.actor):m.actor==Actor);var person=driveActors[m.actor];person.gameObject.SetActive(shown);driveNames[m.actor].gameObject.SetActive(shown);driveHands[m.actor].gameObject.SetActive(shown && d.stage==2);if(!shown)continue;driveVisible++;
                Vector2 point=d.stage==0?DrivePoint(190+i*110,200):d.stage==1?(d.stop==""?DrivePoint(270+i*60,130+d.lane*50):DrivePoint(190+i*100,180)):d.stage==2?new Vector2(-395+i*76,-145):DrivePoint(d.freeBuild?new[]{680,775,870,965}[i]:new[]{100,280,740,900}[i],180);
                if(d.stage==1 && (m.pose=="fuel" || m.pose=="toilet" || m.pose=="rubble"))point=DrivePoint(610+i*30,220);
                if(m.pose=="jump" || m.pose=="double-jump")point.y+=Mathf.Sin(Mathf.Clamp01((float)((d.clock-m.at)/1.25))*Mathf.PI)*35;
                var luggage=d.items.SingleOrDefault(p=>p.holder==m.actor);var carried=d.bricks.SingleOrDefault(b=>b.holder==m.actor);
                if(d.stage==0 && luggage!=null)point=DrivePoint(luggage.x,luggage.y-30);
                if(d.stage==0 && m.pose=="throw" && d.clock-m.at<1.8)point=DrivePoint(550+i*30,200);
                if(d.stage==3 && carried!=null)point=DrivePoint(carried.x,Mathf.Min(carried.y,200));
                if(!driveFeet.TryGetValue(m.actor,out var foot))foot=point;
                bool approaching=Vector2.Distance(foot,point)>3;
                point=Vector2.MoveTowards(foot,point,Time.unscaledDeltaTime*420);driveFeet[m.actor]=point;
                string pose=m.pose;
                if(d.clock-m.at>1.8 && !d.items.Any(p=>p.holder==m.actor) && !d.bricks.Any(b=>b.holder==m.actor) && d.hoseHolder!=m.actor)pose="idle";
                if(d.stage==1 && d.stop=="")pose=i==3?"drive":"sit";
                else if(approaching)pose=luggage!=null || carried!=null?"carry":"walk";
                else if(pose=="carry")pose="idle";
                driveHandPoints[m.actor]=DrawDriveActor(person,DriveRoles[i],m.joined?pose:"idle",point,visualClock-m.at,d.stage==1?120:150);
                var label=driveNames[m.actor];label.text=(m.actor==Actor?"You":"Player "+(i+1))+(m.joined?"":" | resting");label.rectTransform.anchoredPosition=point+new Vector2(0,-70);
                var hand=driveHands[m.actor];DrivePicture(hand,d.armHeight>25?"hand-up":"hand-down",new Vector2(GoForADriveWindowLayout.ArmX,-60+d.armHeight),new Vector2(110,130));hand.gameObject.SetActive(d.stage==2 && m.actor==Actor);
            }
            bool dadNeeded=d.stage!=2 && !d.members.Where((m,i)=>i==3).Any(m=>ReadPlayer(m.actor).zone==GoForADrive.Zone && (Shared?shared.Players.Contains(m.actor):m.actor==Actor));
            if(dadNeeded)DrawDriveActor(driveDad,"bandit",d.stage==1?(d.stop==""?"drive":"sitting"):d.packingClear?"happy":"idle",d.stage==1?DrivePoint(270,130+d.lane*50):DrivePoint(800,180),d.clock,145);else driveDad.gameObject.SetActive(false);
            foreach(var prop in d.items){
                if(!driveProps.TryGetValue(prop.id,out var image)){image=DriveImage("Drive luggage "+prop.id);image.raycastTarget=true;string id=prop.id;NavButton(image,()=>{var p=Drive.items.Single(a=>a.id==id);DriveSend("pack-claim",id,p.revision.ToString());});driveProps.Add(prop.id,image);}
                Vector2 position=DrivePoint(prop.x,prop.y+(prop.slot>=0?125:0));
                var thrower=d.members.SingleOrDefault(m=>m.pose=="throw" && Math.Abs(m.at-prop.at)<.01);
                if(prop.slot>=0 && thrower!=null && d.clock-prop.at<.45 && driveHandPoints.TryGetValue(thrower.actor,out var from)){float t=Mathf.Clamp01((float)(visualClock-prop.at)/.45f);position=Vector2.Lerp(from,position,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*55;}if(prop.holder!="" && driveHandPoints.TryGetValue(prop.holder,out var hand))position=hand;
                if(prop.holder!="")DriveHeldPicture(image,prop.kind,position,Vector2.one*80);else DrivePicture(image,prop.kind,position,Vector2.one*80);image.gameObject.SetActive(d.stage==0);image.raycastTarget=prop.holder=="" && !d.packingClear;
            }
            foreach(var id in driveProps.Keys.Where(id=>!d.items.Any(p=>p.id==id)).ToArray()){Destroy(driveProps[id].gameObject);driveProps.Remove(id);}
            RenderDriveBuilding(d);
            var pattern=GoForADriveWindowLayout.Patterns[d.obstacleKind];float distance=d.windowDistance+(float)(Math.Max(0,visualClock-d.clock)*d.windowSpeed*GoForADriveWindowLayout.PixelsPerUnit);float patternRoot=GoForADriveWindowLayout.ArmX+pattern.span*GoForADriveWindowLayout.PixelsPerUnit*.5f-distance;
            var patternPoint=new Vector2(patternRoot+(pattern.width*.5f-pattern.pivotX)*pattern.scale,GoForADriveWindowLayout.Floor+(pattern.height*.5f-pattern.pivotY)*pattern.scale);
            var patternSize=new Vector2(pattern.width,pattern.height)*pattern.scale;
            DrivePicture(driveObstacle,"window-pattern-"+d.obstacleKind+"-obstacles",patternPoint,patternSize);driveObstacle.gameObject.SetActive(d.stage==2 && d.armDropped);
            DrivePicture(driveReward,"window-pattern-"+d.obstacleKind+"-reward",patternPoint,patternSize);driveReward.gameObject.SetActive(d.stage==2 && d.armDropped && d.windowEligible && !d.windowCollected);
            driveObstacle.rectTransform.localEulerAngles=new Vector3(0,0,!d.windowEligible && d.clock-d.eventAt<.7?Mathf.Sin((float)(d.clock-d.eventAt)*15)*2:0);
            bool nozzle=d.stage==1 && d.stop=="petrol" && d.hoseHolder!="" && driveHandPoints.TryGetValue(d.hoseHolder,out _);
            driveNozzle.gameObject.SetActive(nozzle);driveHose.gameObject.SetActive(nozzle);
            if(nozzle){var hand=driveHandPoints[d.hoseHolder];DriveHeldPicture(driveNozzle,"nozzle",hand,new Vector2(90,100));var start=DrivePoint(730,220);var delta=hand-start;driveHose.rectTransform.anchoredPosition=(start+hand)*.5f;driveHose.rectTransform.sizeDelta=new Vector2(delta.magnitude,4);driveHose.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
            for(int i=0;i<driveStars.Count;i++){var star=driveStars[i];DrivePicture(star,"star",new Vector2(-250+i*55,185),Vector2.one*40);star.gameObject.SetActive(d.stage==2 && d.stickers[i]>0);}
            driveControls.SetAsLastSibling();driveHint.transform.SetAsLastSibling();
            if(d.eventAt!=driveSeenEvent){driveSeenEvent=d.eventAt;if(d.eventAt>=0)DriveSound(d.eventKind);}
        }
        private void RenderDriveBuilding(DriveState d)
        {
            drivePalette.gameObject.SetActive(d.stage==3 && d.freeBuild);
            for(int i=0;i<drivePalettePictures.Count;i++)DrivePicture(drivePalettePictures[i],"brick-"+(12+drivePalettePage*12+i),new Vector2((i%4-1.5f)*68,55-i/4*72),new Vector2(62,58));
            for(int i=0;i<driveBuildTargets.Count;i++){
                var target=driveBuildTargets[i];DriveStatuePicture(target,i);target.color=new Color(1,1,1,.25f);target.gameObject.SetActive(d.stage==3 && !d.freeBuild && !d.statueBuilt && !d.bricks.Any(b=>b.variant==i && b.snapped));
            }
            var held=d.bricks.SingleOrDefault(b=>b.holder==Actor);
            foreach(var brick in d.bricks){
                if(!driveBricks.TryGetValue(brick.id,out var image)){
                    string id=brick.id;image=DriveImage("Drive building brick "+id);image.raycastTarget=true;NavButton(image,()=>{var b=Drive.bricks.Single(p=>p.id==id);DriveSend("brick-claim",id,b.revision.ToString());});driveBricks.Add(id,image);
                }
                Vector2 point=brick.holder!="" && driveHandPoints.TryGetValue(brick.holder,out var hand)?hand:DrivePoint(brick.x,brick.y);
                if(brick.snapped && brick.variant<12)DriveStatuePicture(image,brick.variant);
                else if(brick.holder!="")DriveHeldPicture(image,"brick-"+brick.variant,point,new Vector2(82,67));else DrivePicture(image,"brick-"+brick.variant,point,new Vector2(82,67));
                image.rectTransform.localEulerAngles=new Vector3(0,0,brick.turn*90);image.gameObject.SetActive(d.stage==3);image.raycastTarget=brick.holder=="" && held==null;
            }
            for(int i=0;i<driveBuildTargets.Count;i++){
                var target=driveBuildTargets[i];target.raycastTarget=held!=null && held.variant==i && !d.freeBuild;
                if(target.raycastTarget){target.transform.SetAsLastSibling();target.color=new Color(1,1,1,.5f);}
            }
            foreach(var id in driveBricks.Keys.Where(id=>!d.bricks.Any(b=>b.id==id)).ToArray()){Destroy(driveBricks[id].gameObject);driveBricks.Remove(id);}
        }
        private void ResetDrive()
        {
            driveRoot=null;driveBackdrop=driveCar=driveNomad=drivePeanut=driveObstacle=driveReward=driveWindow=driveDad=driveCow=driveStop=driveNozzle=null;driveHose=null;
            driveActors.Clear();driveProps.Clear();driveBricks.Clear();driveHands.Clear();driveNames.Clear();driveHandPoints.Clear();driveFeet.Clear();driveStars.Clear();drivePegs.Clear();driveBuildTargets.Clear();drivePalettePictures.Clear();driveButtons.Clear();drivePalette=null;driveControlKey="";driveWasVisible=false;driveSeenEvent=-2;driveVisible=drivePalettePage=0;driveMusicStage=-1;driveClock.Reset();
            if(driveMusic)Destroy(driveMusic);if(driveEffects)Destroy(driveEffects);driveMusic=driveEffects=null;GoForADriveArt.Release();
        }
    }
}
