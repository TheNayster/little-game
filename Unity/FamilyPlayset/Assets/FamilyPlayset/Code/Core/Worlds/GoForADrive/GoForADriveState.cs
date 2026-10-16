using System;
using System.Linq;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class DriveMember
    {
        public string actor,pose="idle",held="";
        public bool joined,declined;
        public double at,nextAction,jumpUntil;
        public int contributions,readyTrip=-1;
        public DriveMember Copy()=>(DriveMember)MemberwiseClone();
    }
    [Serializable] public sealed class DriveItem
    {
        public string id,kind,holder="";
        public int slot=-1,revision;
        public float x,y;
        public double at;
        public DriveItem Copy()=>(DriveItem)MemberwiseClone();
    }
    [Serializable] public sealed class DriveBrick
    {
        public string id,holder="";
        public int variant,turn,revision;
        public float x,y;
        public bool snapped;
        public double at;
        public DriveBrick Copy()=>(DriveBrick)MemberwiseClone();
    }
    [Serializable] public sealed class DriveState
    {
        public int format=2,trip,stage,packingWave=1,packingSet,difficulty,lane=1,nomadLane,roadStep,passed,stopIndex,stopProgress;
        public int stars,obstacle,obstacleKind,brickSerial,buildRevision,statues,smashes,photos,doorTaps,cowTaps;
        public bool begun,packingClear,nomadPassed,armDropped,trafficGreen,stopDone,statueBuilt,freeBuild,smashing,windowEligible=true,windowCollected;
        public string navigator="",stop="",windowWinner="",lastActor="",eventKind="",hoseHolder="",armHolder="",armActor="";
        public double clock,stageAt,eventAt=-1,roadAt,obstacleAt,navigatorUntil,balancedAt=-1,unstableAt=-1,smashAt=-1;
        public float balance,balanceLever,fuel,windowDistance,armHeight,armDragHeight;
        public double windowSpeed=3,armJumpAt,armJumpUntil,armDragUntil;
        public int armJumpStage;
        public DriveMember[] members=Array.Empty<DriveMember>();
        public DriveItem[] items=Array.Empty<DriveItem>();
        public DriveBrick[] bricks=Array.Empty<DriveBrick>();
        public int[] stickers=new int[10];
        public DriveState Copy()
        {
            var s=(DriveState)MemberwiseClone();
            s.members=members?.Select(m=>m?.Copy()).ToArray();s.items=items?.Select(i=>i?.Copy()).ToArray();
            s.bricks=bricks?.Select(b=>b?.Copy()).ToArray();s.stickers=stickers?.ToArray();return s;
        }
    }
}
