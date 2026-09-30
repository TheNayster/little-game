using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly Dictionary<string,RectTransform> parkObjects=new Dictionary<string,RectTransform>();
        private readonly List<Sprite> parkSprites=new List<Sprite>();
        private readonly RectTransform[] parkSwings=new RectTransform[4],parkHandles=new RectTransform[4];
        private readonly List<Image> parkDrops=new List<Image>();
        private Texture2D parkTexture;
        private GameObject parkControls;
        private RectTransform parkRail;
        private double parkDisplayClock,parkSampleClock=-1;
        private string parkClockWorld="";
        private ParkState Park=>HasWorld?(Shared?shared.View.park:World.ReadPark()):null;

        // The generated pieces have individual alpha bounds, rather than equal
        // cell bounds. Crop in-engine without creating duplicate texture copies.
        private Sprite ParkSprite(int index)
        {
            var bounds=new[]{new Rect(34,43,453,410),new Rect(486,109,441,307),new Rect(1025,19,192,420),new Rect(1334,266,427,130),
                new Rect(83,570,292,188),new Rect(587,515,200,285),new Rect(895,554,422,242),new Rect(1333,566,432,228)};
            var b=bounds[index];var sx=parkTexture.width/1784f;var sy=parkTexture.height/892f;
            var sprite=Sprite.Create(parkTexture,new Rect(b.x*sx,(892-b.y-b.height)*sy,b.width*sx,b.height*sy),new Vector2(.5f,.5f));
            parkSprites.Add(sprite);return sprite;
        }
        private RectTransform ParkObject(string station)
        {var r=Rect(Board,"Park "+station,Vector2.zero,Vector2.zero);parkObjects.Add(station,r);return r;}
        private void ParkHit(Transform root,string name,Vector2 pos,Vector2 size,string station)
        {
            HomeHit(root,name,pos,size,()=>{
                var own=ReadPlayer(Actor);var snapshot=Shared?shared.View:World.Snapshot();
                if(ParkPlay.Station(own.fixture)==station){HomeAction(SoloAction.LeaveFixture,own.fixture);return;}
                var slot=Enumerable.Range(0,4).Select(i=>ParkPlay.Slot(station,i)).FirstOrDefault(id=>!snapshot.players.Any(p=>p.zone=="park" && p.fixture==id));
                if(slot==null){homeFeedback.text="All four spots are busy.";homeFeedbackUntil=Time.unscaledTime+2;return;}
                destination=null;HomeAction(SoloAction.UseFixture,slot);
            });
        }
        private void BuildPark()
        {
            if(SceneSchema<ParkPlay.Schema)return;
            parkTexture=Resources.Load<Texture2D>("ParkArt/equipment");
            var art=Enumerable.Range(0,8).Select(ParkSprite).ToArray();
            var slide=ParkObject("slide");
            HomePicture(slide,"Playhouse and slide",new Vector2(0,210),new Vector2(680,615),art[0]);
            // Register the front railing to the same atlas and full-structure scale.
            parkRail=Rect(Board,"Park playhouse front rail",Vector2.zero,Vector2.zero);
            var railSprite=Sprite.Create(parkTexture,new Rect(170*parkTexture.width/1784f,(892-194-118)*parkTexture.height/892f,141*parkTexture.width/1784f,118*parkTexture.height/892f),new Vector2(.5f,.5f));parkSprites.Add(railSprite);
            HomePicture(parkRail,"Front railing",new Vector2(-30,205),new Vector2(211.5f,177),railSprite);
            ParkHit(slide,"Climb and slide",new Vector2(-205,100),new Vector2(225,220),"slide");
            ParkHit(slide,"Playhouse platform",new Vector2(-55,380),new Vector2(220,140),"slide");
            var swings=ParkObject("swing");
            for(var i=0;i<2;i++)HomePicture(swings,"Swing frame "+i,new Vector2((i-.5f)*360,250),new Vector2(455,318),art[1]);
            for(var i=0;i<4;i++)
            {
                parkSwings[i]=Rect(Board,"Park swing seat "+i,Vector2.zero,Vector2.zero);
                var image=HomePicture(parkSwings[i],"Chains and seat",new Vector2(0,-158),new Vector2(145,316),art[2]);
                var slot=ParkPlay.Slot("swing",i);
                HomeHit(parkSwings[i],"Ride swing "+(i+1),new Vector2(0,-265),new Vector2(125,155),()=>{destination=null;UseHome(slot);});
            }
            var round=ParkObject("roundabout");
            HomePicture(round,"Rotating platform",new Vector2(0,45),new Vector2(550,168),art[3]);
            ParkHit(round,"Ride merry-go-round",new Vector2(0,85),new Vector2(510,210),"roundabout");
            for(var i=0;i<4;i++){parkHandles[i]=Rect(Board,"Park roundabout handle "+i,Vector2.zero,Vector2.zero);HomePicture(parkHandles[i],"Handle and seat",new Vector2(0,20),new Vector2(120,78),art[4]);}
            var fountain=ParkObject("fountain");
            HomePicture(fountain,"Water fountain",new Vector2(0,105),new Vector2(178,254),art[5]);
            HomeHit(fountain,"Fountain water",new Vector2(0,130),new Vector2(180,230),()=>HomeAction(SoloAction.Park,"", "water"));
            for(var i=0;i<10;i++){var drop=Panel(fountain,"Water drop "+i,Vector2.zero,new Vector2(8,13),new Color(.46f,.8f,.96f),false,true);drop.raycastTarget=false;parkDrops.Add(drop);}
            var bench=ParkObject("bench");HomePicture(bench,"Bench",new Vector2(0,82),new Vector2(470,270),art[6]);
            ParkHit(bench,"Sit on bench",new Vector2(0,95),new Vector2(460,230),"bench");
            var picnic=ParkObject("picnic");HomePicture(picnic,"Picnic table",new Vector2(0,68),new Vector2(480,254),art[7]);
            ParkHit(picnic,"Sit for picnic",new Vector2(0,65),new Vector2(475,200),"picnic");
            parkControls=Rect(safe,"Park ride controls",new Vector2(0,-safe.rect.height/2+75),new Vector2(530,75)).gameObject;
            Button(parkControls.transform,"Get off",new Vector2(-145,0),new Vector2(220,70),()=>HomeAction(SoloAction.LeaveFixture,""),Cream);
            Button(parkControls.transform,"Stop / turn",new Vector2(125,0),new Vector2(260,70),()=>HomeAction(SoloAction.Park,"",Park.targetSpeed==0?"turn":"stop"),Cream).transform.parent.name="Roundabout speed";
            BuildWheels();TickPark();
        }
        private void ResetPark()
        {
            ResetWheels();
            foreach(var sprite in parkSprites)Destroy(sprite);parkSprites.Clear();
            if(parkTexture!=null)Resources.UnloadAsset(parkTexture);parkTexture=null;parkObjects.Clear();parkDrops.Clear();
            Array.Clear(parkSwings,0,4);Array.Clear(parkHandles,0,4);parkControls=null;parkRail=null;parkSampleClock=-1;parkClockWorld="";
        }
        private void TickPark()
        {
            var park=Park;if(park==null || parkObjects.Count==0)return;
            var players=Shared?shared.View.players:World.ReadPlayers();var worldId=Shared?shared.View.worldId:World.WorldId;
            if(parkClockWorld!=worldId || park.clock<parkSampleClock){parkClockWorld=worldId;parkDisplayClock=park.clock;}
            // A bounded presentation clock bridges 20 Hz samples. It neither
            // drives the authority nor resets ride phase on each reliable state.
            parkSampleClock=park.clock;
            parkDisplayClock=Math.Max(park.clock,Math.Min(park.clock+.12,parkDisplayClock+Math.Min(.1,Time.unscaledDeltaTime)));
            var visible=CurrentArea=="park";
            foreach(var pair in parkObjects)
            {var x=pair.Key=="fountain"?ParkPlay.FountainX:ParkPlay.Center(pair.Key);var y=pair.Key=="fountain"?ParkPlay.FountainY:pair.Key=="roundabout"?180:220;
                pair.Value.gameObject.SetActive(visible);pair.Value.anchoredPosition=ToBoard(x,y);pair.Value.localScale=Vector3.one*sceneScale;}
            parkRail.gameObject.SetActive(visible);parkRail.anchoredPosition=parkObjects["slide"].anchoredPosition;parkRail.localScale=Vector3.one*sceneScale;
            for(var i=0;i<4;i++)
            {
                var slot=ParkPlay.Slot("swing",i);var rider=players.FirstOrDefault(p=>p.zone=="park" && p.fixture==slot);
                var angle=rider==null?0:ParkPlay.SwingAngle(parkDisplayClock-rider.rideStarted,i);
                var root=parkSwings[i];root.gameObject.SetActive(visible);root.anchoredPosition=ToBoard(ParkPlay.X(slot),220)+new Vector2(0,386*sceneScale);
                root.localScale=Vector3.one*sceneScale;root.localRotation=Quaternion.Euler(0,0,(float)(angle*180/Math.PI));
                var handle=parkHandles[i];var position=RoundPoint(i);handle.gameObject.SetActive(visible);handle.anchoredPosition=ToBoard(ParkPlay.Center("roundabout"),180)+position*sceneScale;handle.localScale=Vector3.one*sceneScale;
            }
            foreach(var drop in parkDrops)drop.gameObject.SetActive(visible && parkDisplayClock<park.waterUntil);
            for(var i=0;i<parkDrops.Count;i++){var t=(float)((parkDisplayClock*1.1+i*.1)%1);parkDrops[i].rectTransform.anchoredPosition=new Vector2(30+t*48,208+80*t-100*t*t);}
            var own=ReadPlayer(Actor);parkControls.SetActive(visible && ParkPlay.Usable(own.fixture) && !MenuOpen);
            parkControls.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-safe.rect.height/2+75);
            parkControls.transform.Find("Roundabout speed").gameObject.SetActive(ParkPlay.Station(own.fixture)=="roundabout");
        }
        private Vector2 RoundPoint(int seat)
        {var angle=ParkPlay.TurnAngle(Park,Math.Max(0,parkDisplayClock-Park.clock))+seat*Math.PI/2;return new Vector2((float)Math.Cos(angle)*185,50+(float)Math.Sin(angle)*55);}
        private void PresentPark()
        {
            if(CurrentArea!="park" || Park==null)return;
            void Ride(string actor,RectTransform root,GameCharacterVisual visual)
            {
                var p=ReadPlayer(actor);var station=ParkPlay.Station(p.fixture);if(station=="")return;
                var age=Math.Max(0,parkDisplayClock-p.rideStarted);var seat=ParkPlay.Seat(p.fixture,station);
                var offset=Vector2.zero;var pose=CharacterPose.Sit;var left=false;var tilt=0f;
                if(station=="swing"){
                    var angle=ParkPlay.SwingAngle(age,seat);offset=new Vector2((float)Math.Sin(angle)*290,386-(float)Math.Cos(angle)*290-2);tilt=(float)(angle*180/Math.PI);}
                else if(station=="roundabout")offset=RoundPoint(seat)+new Vector2(ParkPlay.Center(station)-p.x,-11);
                else if(station=="slide")
                {
                    var c=ParkPlay.Center("slide")-p.x;age=Math.Max(0,age-seat*.35);
                    // Contact points measured on the actual playhouse atlas:
                    // ladder foot/top, platform lip, slide lip and landing.
                    // A cubic route follows its curved mint slide, not open air.
                    if(age<2){var t=(float)(age/2);offset=new Vector2(c-295+t*142,-46+t*210);pose=CharacterPose.Walk;}
                    else if(age<2.6){var t=(float)((age-2)/.6);offset=new Vector2(c-153+219*t,164+7*t);pose=CharacterPose.Walk;}
                    else{
                        var t=Mathf.Clamp01((float)((age-2.6)/2.4));var u=1-t;
                        var point=u*u*u*new Vector2(66,171)+3*u*u*t*new Vector2(130,176)+3*u*t*t*new Vector2(168,-34)+t*t*t*new Vector2(294,-46);
                        offset=point+new Vector2(c,0);tilt=-12*Mathf.Sin(t*Mathf.PI);
                    }
                }
                else offset=new Vector2(0,station=="bench"?64:74);
                root.anchoredPosition=ToBoard(p.x,p.y)+offset*sceneScale;
                visual.PresentSupported(new CharacterFrame(pose,pose==CharacterPose.Walk?Walking.Speed:0,left,(float)age,travel:new Vector2((float)age*Walking.Speed,0)),Time.unscaledDeltaTime,tilt);
            }
            Ride(Actor,avatar,characterVisual);
            foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)Ride(friend.Key,friend.Value.root,friend.Value.view);
            SortDepth();
        }
        private float ParkPlayerGround(string fixture)
        {
            var station=ParkPlay.Station(fixture);
            // Use the identical support key for body and foreground. Recreating
            // it from differently rounded body coordinates can flip depth order.
            if(station=="roundabout")return parkHandles[ParkPlay.Seat(fixture,station)].anchoredPosition.y-50*sceneScale;
            return parkObjects[station].anchoredPosition.y;
        }
        private void AddParkDepth(Action<RectTransform,float,int,string> add)
        {
            if(parkRail!=null)add(parkRail,parkRail.anchoredPosition.y,2,"park-slide-rail");
            foreach(var pair in parkObjects)add(pair.Value,pair.Value.anchoredPosition.y,0,"park-"+pair.Key);
            for(var i=0;i<4;i++){
                if(parkSwings[i]!=null)add(parkSwings[i],ToBoard(0,220).y,0,"park-swing-"+i);
                if(parkHandles[i]!=null)add(parkHandles[i],parkHandles[i].anchoredPosition.y-50*sceneScale,2,"park-round-"+i);}
        }
    }
}
