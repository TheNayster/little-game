using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LittleWeeps.Core;
namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private Texture2D wheelsTexture, riderTexture;
        private readonly System.Collections.Generic.List<Sprite> wheelsSprites=new System.Collections.Generic.List<Sprite>();
        private readonly float[] wheelPhase=new float[8];
        private readonly RectTransform[] wheelVehicles=new RectTransform[8];
        private readonly GameObject[] wheelTargets=new GameObject[8];
        private readonly bool[] wheelLeft=new bool[8];
        private readonly float[] wheelLastX=new float[8];
        private GameObject wheelsExit;
        private void BuildWheels()
        {
            wheelsTexture=Resources.Load<Texture2D>("ParkArt/bikes-scooters");
            if(wheelsTexture==null)throw new InvalidOperationException("Missing park vehicle artwork");
            riderTexture=Resources.Load<Texture2D>("ParkArt/rider-poses");
            if(riderTexture==null)throw new InvalidOperationException("Missing riding poses");
            for(var i=0;i<8;i++){
                var slot=i;var root=Rect(Board,"Park vehicle "+i,Vector2.zero,Vector2.zero);wheelVehicles[i]=root;
                var bike=i<4;var unit=bike?220f/940:200f/790;
                // Separate saddle and handlebar pieces let this child-sized
                // equipment fit the rider's real seat, hand and foot landmarks.
                if(bike){WheelsPart(root,"Bike frame",new Rect(8,450,940,385),478,unit,0);
                    WheelsPart(root,"Saddle",new Rect(230,340,270,122),478,unit,85);
                    WheelsPart(root,"Handlebars",new Rect(535,150,300,305),478,unit,165);}
                else {WheelsPart(root,"Scooter deck and wheels",new Rect(985,500,790,335),1380,unit,0);
                    WheelsPart(root,"Scooter handlebars",new Rect(1430,80,345,200),1380,unit,320);}
                HomeHit(root,(bike?"Ride bike ":"Ride scooter ")+(i%4+1),new Vector2(0,65),new Vector2(230,170),()=>{destination=null;shared?.Walk(WalkMode.Stop);HomeAction(SoloAction.UseFixture,ParkWheels.Id(slot));});
                wheelTargets[i]=root.GetComponentInChildren<Button>().gameObject;wheelLastX[i]=ParkWheels.ParkX(i);
            }
            wheelsExit=Rect(safe,"Riding controls",new Vector2(0,-safe.rect.height/2+65),new Vector2(640,72)).gameObject;
            Button(wheelsExit.transform,"Get off",new Vector2(195,0),new Vector2(220,70),()=>{destination=null;stickDirection=Vector2.zero;shared?.Walk(WalkMode.Stop);HomeAction(SoloAction.LeaveFixture,"");},Cream);
            Label(wheelsExit.transform,"Ride left or right",24,new Vector2(-140,0),new Vector2(360,60));
        }
        private void WheelsPart(RectTransform root,string name,Rect rect,float center,float unit,float down)
        {
            var sx=wheelsTexture.width/1784f;var sy=wheelsTexture.height/892f;
            var sprite=Sprite.Create(wheelsTexture,new Rect(rect.x*sx,(892-rect.y-rect.height)*sy,rect.width*sx,rect.height*sy),new Vector2(.5f,.5f));wheelsSprites.Add(sprite);
            HomePicture(root,name,new Vector2((rect.center.x-center)*unit,(835-rect.center.y-down)*unit),rect.size*unit,sprite);
        }
        private void ResetWheels()
        {
            foreach(var sprite in wheelsSprites)if(sprite!=null)Destroy(sprite);wheelsSprites.Clear();Array.Clear(wheelPhase,0,8);Array.Clear(wheelVehicles,0,8);Array.Clear(wheelTargets,0,8);Array.Clear(wheelLeft,0,8);
            if(wheelsTexture!=null)Resources.UnloadAsset(wheelsTexture);wheelsTexture=null;if(riderTexture!=null)Resources.UnloadAsset(riderTexture);riderTexture=null;wheelsExit=null;
        }
        private void PresentWheels()
        {
            if(wheelsExit==null || !HasWorld)return;var list=ReadPlayersForWheels();
            wheelsExit.SetActive(CurrentArea=="park" && !MenuOpen && ParkWheels.Usable(ReadPlayer(Actor).fixture));
            for(var i=0;i<8;i++){
                var root=wheelVehicles[i];var rider=list.FirstOrDefault(p=>p.zone=="park" && p.fixture==ParkWheels.Id(i));
                root.gameObject.SetActive(CurrentArea=="park" && !WorldLoading);if(!root.gameObject.activeSelf)continue;
                // A rider's moving hitbox must not block an available parked
                // vehicle underneath it. Dismount uses the explicit exit button.
                wheelTargets[i].SetActive(rider==null);
                var point=rider==null?new Vector2(ParkWheels.ParkX(i),ParkWheels.Lane(ParkWheels.Id(i))):Shared && shared.Connected?shared.VisualPosition(rider.id):new Vector2(rider.x,rider.y);
                var delta=point.x-wheelLastX[i];if(Mathf.Abs(delta)>.05f)wheelLeft[i]=delta<0;wheelLastX[i]=point.x;
                if(rider!=null && Mathf.Abs(delta)<100)wheelPhase[i]+=Mathf.Abs(delta)/170;
                var size=1f;GameCharacterVisual visual=null;RectTransform body=null;
                if(rider!=null){if(rider.id==Actor){visual=characterVisual;body=avatar;}else if(friends.TryGetValue(rider.id,out var friend)){visual=friend.view;body=friend.root;}}
                if(visual!=null)size=visual.ActiveView.SupportScale;
                root.anchoredPosition=ToBoard(point.x,point.y);root.localScale=new Vector3((wheelLeft[i]?-1:1)*sceneScale*size,sceneScale*size,1);
                // Character and vehicle use the same displayed point. Never read
                // a raw server position to reset an already interpolated rider.
                if(body!=null && body.gameObject.activeSelf){
                    var authored=rider.outfit=="" && (rider.avatar=="blue-pup" || rider.avatar=="orange-pup");
                    var seat=new Vector2(-26.45f,72.55f);var deck=new Vector2(0,26.58f);
                    var offset=i<4?(authored?seat:new Vector2(seat.x,seat.y-38)):deck;if(wheelLeft[i])offset.x=-offset.x;
                    body.anchoredPosition=ToBoard(point.x,point.y)+offset*sceneScale*size;
                    visual.PresentSupported(new CharacterFrame(i<4?CharacterPose.Sit:CharacterPose.Idle,0,wheelLeft[i],1),Time.unscaledDeltaTime);
                    if(authored){var row=rider.avatar=="orange-pup"?1:0;var frame=((int)wheelPhase[i])%2;var col=i<4?frame:2+frame;
                        var cw=riderTexture.width/4f;var ch=riderTexture.height/2f;var cell=new Rect(col*cw,row*ch,cw,ch);
                        var contact=i<4?new Vector2(190,328):new Vector2(frame==0?245:265,428);
                        contact=cell.position+new Vector2(contact.x*cw/446,contact.y*ch/446);
                        visual.ActiveView.PresentRiding(riderTexture,cell,contact,ch*410/446,wheelLeft[i]);
                    }
                }
            }
            SortDepth();
        }
        private SoloPlayer[] ReadPlayersForWheels()=>Shared?shared.View.players:World.ReadPlayers();
        private float WheelsGround(string id)=>wheelVehicles[ParkWheels.Index(id)].anchoredPosition.y;
        private void AddWheelsDepth(Action<RectTransform,float,int,string> add)
        {for(var i=0;i<8;i++)if(wheelVehicles[i]!=null)add(wheelVehicles[i],wheelVehicles[i].anchoredPosition.y,0,ParkWheels.Id(i));}
    }
}
