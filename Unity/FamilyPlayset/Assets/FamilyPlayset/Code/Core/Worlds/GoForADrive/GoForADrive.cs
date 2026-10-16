using System;
using System.Globalization;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class GoForADrive
    {
        public const string Zone="go-for-a-drive";
        public const int Schema=57;
        public static readonly int[] Difficulty={1,2,2,2,3,3,3,4};
        public static readonly string[] Stops={"toilet","petrol","roadworks"};
        public static readonly int[] Overtakes={2,4,6,6,12,18};
        public static int RoadTarget(DriveState d)=>Overtakes[Math.Min(5,d.trip*3+d.stopIndex)];
        public static readonly string[] Poses={"idle","carry","throw","balance","sit","drive","wave","cheer","jump","double-jump","fuel","toilet","cow","rubble","build","smash"};
        public static bool Finite(double n)=>!double.IsNaN(n) && !double.IsInfinity(n);
        public static bool BoardPoint(float x,float y)=>Finite(x) && Finite(y) && x>=0 && x<=1000 && y>=0 && y<=600;
        public static DriveState Create(string[] actors)=>new DriveState{members=actors.Select(a=>new DriveMember{actor=a}).ToArray(),items=Packing(1,0)};
        // The source spawner supplies six three-brick sets; later waves append sets.
        public static DriveItem[] Packing(int waves,int set)=>Enumerable.Range(0,waves*3).Select(i=>new DriveItem{id="luggage-"+i,kind="packing-"+((set+i/3)%6)+"-"+(i%3),x=170+i%3*120,y=170+i/3*35}).ToArray();
    }

    public sealed partial class GameWorld
    {
        public DriveState ReadDrive()=>state.drive?.Copy();
        // Board placement and the signed balancing lever have distinct bounds
        // from the walking floor. Other commands keep normal area validation.
        private static bool DriveInputPoint(SoloCommand c)=>c.zone==GoForADrive.Zone && (c.value=="balance"?GoForADrive.Finite(c.x) && c.x>=-1 && c.x<=1 && c.y==0:GoForADrive.BoardPoint(c.x,c.y));
        private static void NormalizeDrive(SoloSnapshot s)
        {
            var d=s?.drive;
            if(s!=null && s.schema<GoForADrive.Schema && d!=null && (d.members==null || d.members.Length==0) && (d.items==null || d.items.Length==0))s.drive=null;
            else if(d!=null && d.format==1){
                // Preserve checkpoints made by the earlier concurrent candidate.
                // Only the new shared-hand transient fields need initialization.
                d.format=2;d.windowSpeed=Math.Max(3,d.windowSpeed);d.windowEligible=true;d.windowCollected=false;
                d.armHolder=d.armActor="";d.armHeight=d.armDragHeight=d.windowDistance=0;d.armJumpStage=0;
            }
        }
        private static void ValidateDrive(SoloSnapshot s)
        {
            var d=s.drive;if(d==null && s.schema<GoForADrive.Schema)return;
            bool Actor(string id)=>s.players.Any(p=>p.id==id);
            bool Time(double t)=>GoForADrive.Finite(t) && t>=0;
            if(d==null || d.members==null || d.format!=2 || d.trip<0 || d.stage<0 || d.stage>3 || d.packingWave<1 || d.packingWave>4 || d.packingSet<0 || d.packingSet>5 || d.difficulty<0 || d.difficulty>=GoForADrive.Difficulty.Length ||
                d.lane<0 || d.lane>2 || d.nomadLane<0 || d.nomadLane>2 || d.passed<0 || d.passed>18 || d.roadStep<0 || d.stopIndex<0 || d.stopIndex>3 || d.stopProgress<0 || d.stopProgress>4 ||
                d.stars<0 || d.obstacle<0 || d.obstacleKind<0 || d.obstacleKind>14 || d.brickSerial<0 || d.buildRevision<0 || d.statues<0 || d.smashes<0 || d.photos<0 || d.doorTaps<0 || d.cowTaps<0 ||
                !Time(d.clock) || !Time(d.stageAt) || d.stageAt>d.clock || !Time(d.roadAt) || d.roadAt>d.clock || !Time(d.obstacleAt) || d.obstacleAt>d.clock || !Time(d.navigatorUntil) ||
                !GoForADrive.Finite(d.eventAt) || d.eventAt< -1 || d.eventAt>d.clock || new[]{d.balancedAt,d.unstableAt,d.smashAt}.Any(t=>!GoForADrive.Finite(t) || t< -1 || t>d.clock) || !GoForADrive.Finite(d.balance) || Math.Abs(d.balance)>1 || !GoForADrive.Finite(d.balanceLever) || Math.Abs(d.balanceLever)>1 ||
                d.navigator==null || d.navigator!="" && !Actor(d.navigator) || d.stop==null || d.stop!="" && !GoForADrive.Stops.Contains(d.stop) || d.windowWinner==null || d.windowWinner!="" && !Actor(d.windowWinner) || d.lastActor==null || d.lastActor!="" && !Actor(d.lastActor) || d.eventKind==null || d.hoseHolder==null || d.hoseHolder!="" && !d.members.Any(m=>m.actor==d.hoseHolder && m.joined) || !GoForADrive.Finite(d.fuel) || d.fuel<0 || d.fuel>1 ||
                d.members==null || !d.members.Select(m=>m?.actor).OrderBy(a=>a).SequenceEqual(s.players.Select(p=>p.id).OrderBy(a=>a)) ||
                d.members.Any(m=>m==null || m.pose==null || !GoForADrive.Poses.Contains(m.pose) || m.held==null || !Time(m.at) || m.at>d.clock || !Time(m.nextAction) || !Time(m.jumpUntil) || m.contributions<0 || m.readyTrip< -1 || m.readyTrip>d.trip || m.joined && m.declined || m.joined && !s.players.Any(p=>p.id==m.actor && p.zone==GoForADrive.Zone)) ||
                d.items==null || d.items.Length!=d.packingWave*3 || d.items.Where((p,i)=>p==null || p.id!="luggage-"+i || p.kind!="packing-"+((d.packingSet+i/3)%6)+"-"+(i%3) || p.holder==null || p.holder!="" && !d.members.Any(m=>m.actor==p.holder && m.joined && m.held==p.id) || p.slot< -1 || p.slot>=12 || p.holder!="" && p.slot>=0 || p.revision<0 || !GoForADrive.BoardPoint(p.x,p.y) || !Time(p.at) || p.at>d.clock).Any() ||
                d.items.Where(p=>p.slot>=0).GroupBy(p=>p.slot).Any(g=>g.Count()>1) || d.members.Any(m=>m.held!="" && !d.items.Any(p=>p.id==m.held && p.holder==m.actor)) ||
                d.bricks==null || d.bricks.Length>300 || d.bricks.Any(b=>b==null || !Id(b.id) || b.holder==null || b.holder!="" && !d.members.Any(m=>m.actor==b.holder && m.joined) || b.variant<0 || b.variant>47 || b.turn<0 || b.turn>3 || b.revision<0 || !GoForADrive.BoardPoint(b.x,b.y) || !Time(b.at) || b.at>d.clock || b.snapped && b.holder!="") || d.bricks.Select(b=>b.id).Distinct().Count()!=d.bricks.Length || d.bricks.Where(b=>b.holder!="").GroupBy(b=>b.holder).Any(g=>g.Count()>1) ||
                d.stickers==null || d.stickers.Length!=10 || d.stickers.Any(v=>v<0 || v>4) || !Time(d.windowSpeed) || d.windowSpeed<3 || !GoForADrive.Finite(d.windowDistance) || d.windowDistance<0 || !GoForADrive.Finite(d.armHeight) || d.armHeight<0 || d.armHeight>300 || !GoForADrive.Finite(d.armDragHeight) || d.armDragHeight<0 || d.armDragHeight>1 || d.armJumpStage<0 || d.armJumpStage>2 || !Time(d.armJumpAt) || d.armJumpAt>d.clock || !Time(d.armJumpUntil) || !Time(d.armDragUntil) || d.armHolder==null || d.armHolder!="" && !d.members.Any(m=>m.actor==d.armHolder && m.joined) || d.armActor==null || d.armActor!="" && !Actor(d.armActor))throw new InvalidOperationException("Invalid Go for a Drive world.");
        }
        private static void RestoreDrive(SoloSnapshot s)
        {
            var d=s.drive;if(d==null)return;
            foreach(var m in d.members){m.joined=false;m.declined=false;m.held="";m.pose="idle";m.jumpUntil=0;m.readyTrip=-1;m.nextAction=d.clock;}
            foreach(var p in d.items.Where(p=>p.holder!="")){p.holder="";p.revision++;}
            foreach(var b in d.bricks.Where(b=>b.holder!="")){b.holder="";b.revision++;}
            d.navigator="";d.navigatorUntil=d.clock;d.hoseHolder="";
            // Resume the saved checkpoint without counting time spent with no players.
            d.roadAt=d.obstacleAt=d.clock;d.windowWinner=d.armHolder="";d.balancedAt=-1;d.armHeight=d.armDragHeight=d.windowDistance=0;d.armJumpUntil=d.armDragUntil=d.clock;d.armJumpStage=0;
            if(d.smashing)DriveScatter(d);d.balanceLever=0;d.unstableAt=-1;
        }
        private void DrivePose(DriveMember m,string pose)
        {m.pose=pose;m.at=state.drive.clock;}
        private void DriveEvent(string kind,string actor)
        {state.drive.eventKind=kind;state.drive.lastActor=actor;state.drive.eventAt=state.drive.clock;}
        public void ReleaseDrive(string actor)
        {
            var d=state.drive;if(d==null)return;var m=d.members.FirstOrDefault(p=>p.actor==actor);if(m==null)return;
            bool changed=m.joined || m.held!="" || d.navigator==actor || d.bricks.Any(b=>b.holder==actor);
            m.joined=false;m.held="";m.pose="idle";m.jumpUntil=0;m.readyTrip=-1;
            foreach(var p in d.items.Where(p=>p.holder==actor)){p.holder="";p.revision++;}
            foreach(var b in d.bricks.Where(b=>b.holder==actor)){b.holder="";b.revision++;}
            if(d.navigator==actor){d.navigator="";d.navigatorUntil=d.clock;}
            if(d.hoseHolder==actor){d.hoseHolder="";changed=true;}
            if(d.armHolder==actor){d.armHolder="";changed=true;}
            if(changed)state.revision++;
        }
        private void EnterDrive(string actor)
        {
            var d=state.drive;if(d==null)return;var m=d.members.Single(p=>p.actor==actor);
            m.declined=false;m.joined=d.begun;DrivePose(m,d.stage==0?"idle":"sit");
        }
        private string DriveOperation(SoloCommand c,SoloPlayer player,string[] connected)
        {
            var d=state.drive;if(d==null || player.zone!=GoForADrive.Zone)return "come-to-the-car";
            var m=d.members.Single(p=>p.actor==player.id);
            bool Number(string value,int max,out int result)=>int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out result) && result>=0 && result<=max;
            bool OwnRevision(int revision)=>c.target==revision.ToString(CultureInfo.InvariantCulture);
            if(c.value=="leave"){ReleaseDrive(player.id);m.declined=true;return null;}
            if(c.value=="join" || c.value=="start"){
                if(c.value=="start" && !d.begun){d.begun=true;foreach(var other in d.members.Where(a=>!a.declined && state.players.Any(p=>p.id==a.actor && p.zone==GoForADrive.Zone && (connected==null || connected.Contains(p.id)))))other.joined=true;DriveEvent("start",player.id);}
                m.declined=false;m.joined=true;return null;
            }
            if(!m.joined || m.declined)return "join-the-trip";
            if(c.value=="next"){
                if(d.members.Any(a=>a.held!="") || d.bricks.Any(b=>b.holder!=""))return "put-down-before-travelling";
                if(d.stage==0 && !d.packingClear || d.stage==1 && d.stopIndex<3)return "finish-this-stop";
                if(d.stage>=3)return "already-at-the-big-peanut";
                d.stage++;d.stageAt=d.clock;d.roadAt=d.obstacleAt=d.clock;d.windowWinner="";
                foreach(var a in d.members.Where(a=>a.joined))DrivePose(a,"cheer");
                if(d.stage==3 && d.bricks.Length==0)DriveStatue(d);
                DriveEvent("travel",player.id);return null;
            }
            if(c.value=="replay"){
                if(d.stage!=3 || d.members.Any(a=>a.held!="") || d.bricks.Any(b=>b.holder!=""))return "finish-building-first";
                m.readyTrip=d.trip;
                // Reset is shared consent, unlike an independent Leave/Return.
                var participants=d.members.Where(a=>a.joined && !a.declined && (connected==null || connected.Contains(a.actor))).ToArray();
                if(participants.Any(a=>a.readyTrip!=d.trip))return null;
                d.trip++;d.stage=0;d.stageAt=d.clock;d.packingClear=false;d.packingWave=1;d.packingSet=(d.packingSet+1)%6;
                d.difficulty=d.difficulty==7?4:d.difficulty+1;d.items=GoForADrive.Packing(1,d.packingSet);
                d.passed=d.stopIndex=d.stopProgress=0;d.stop="";d.stopDone=false;d.nomadPassed=false;d.armDropped=false;d.navigator=d.hoseHolder="";d.fuel=0;d.balance=d.balanceLever=0;d.balancedAt=d.unstableAt=-1;
                foreach(var a in d.members){a.held="";a.readyTrip=-1;DrivePose(a,"idle");}
                DriveEvent("replay",player.id);return null;
            }
            if(c.value=="wave" || c.value=="door" || c.value=="photo" || c.value=="horn"){
                if(d.clock<m.nextAction)return "wait-a-moment";m.nextAction=d.clock+.7;DrivePose(m,"wave");m.contributions++;
                if(c.value=="door")d.doorTaps++;if(c.value=="photo")d.photos++;DriveEvent(c.value,player.id);return null;
            }
            if(d.stage==0)return DrivePacking(c,m);
            if(d.stage==1)return DriveRoad(c,m,Number);
            if(d.stage==2){
                if(c.value=="hand-release"){if(d.armHolder==m.actor)d.armHolder="";return null;}
                if(c.value=="hand"){
                    if(!GoForADrive.Finite(c.y) || c.y<0 || c.y>1)return "move-the-window-hand";
                    if(d.armHolder!="" && d.armHolder!=m.actor && d.armDragUntil>d.clock)return "friend-is-guiding-the-hand";
                    d.armHolder=d.armActor=m.actor;d.armDragUntil=d.clock+.75;d.armDragHeight=c.y;d.armDropped=true;m.contributions++;return null;
                }
                if(c.value!="jump")return "try-the-window-hand";
                if(d.armHolder!="" && d.armDragUntil>d.clock)return "release-the-hand-to-jump";
                if(d.clock<m.nextAction)return "wait-a-moment";
                bool second=d.armJumpUntil>d.clock;if(second && d.armJumpStage==2)return "already-double-jumping";
                DrivePose(m,second?"double-jump":"jump");d.armActor=m.actor;d.armJumpAt=d.clock;d.armJumpStage=second?2:1;d.armJumpUntil=d.clock+(second?1.5:1.1);m.jumpUntil=d.armJumpUntil;m.nextAction=d.clock+.15;m.contributions++;d.armDropped=true;DriveEvent(m.pose,player.id);return null;
            }
            return DriveBuilding(c,m,Number,OwnRevision);
        }
        private delegate bool DriveNumber(string value,int max,out int result);
        private delegate bool DriveRevision(int revision);
        private string DrivePacking(SoloCommand c,DriveMember m)
        {
            var d=state.drive;var item=d.items.SingleOrDefault(p=>p.id==c.item);
            if(c.value=="pack-claim"){
                if(item==null || item.holder!="" || m.held!="" || d.packingClear)return "item-in-use";
                if(c.target!=item.revision.ToString(CultureInfo.InvariantCulture))return "item-changed";
                item.slot=-1;item.holder=m.actor;item.revision++;m.held=item.id;d.balancedAt=-1;DrivePose(m,"carry");return null;
            }
            if(c.value=="pack-place" || c.value=="pack-drop"){
                if(item==null || item.holder!=m.actor || m.held!=item.id)return "pick-up-the-item";
                if(c.target!=item.revision.ToString(CultureInfo.InvariantCulture))return "item-changed";
                if(c.value=="pack-place"){
                    if(!GoForADrive.Finite(c.x) || c.x!=Math.Floor(c.x) || c.x<0 || c.x>11 || d.items.Any(p=>p!=item && p.slot==(int)c.x))return "choose-a-free-car-peg";
                    item.slot=(int)c.x;item.x=600+item.slot%3*80;item.y=220+item.slot/3*45;DrivePose(m,"throw");
                }else{item.slot=-1;item.x=230+Array.IndexOf(d.members,m)*70;item.y=180;DrivePose(m,"idle");}
                item.holder="";item.revision++;item.at=d.clock;m.held="";m.contributions++;d.balancedAt=-1;DriveEvent(c.value,m.actor);return null;
            }
            if(c.value=="balance"){
                if(!GoForADrive.Finite(c.x) || c.x< -1 || c.x>1)return "steady-the-car";
                d.balanceLever=Math.Clamp(d.balanceLever+c.x*.2f,-1,1);DrivePose(m,"balance");DriveEvent("balance",m.actor);return null;
            }
            return "choose-a-packing-action";
        }
        private string DriveRoad(SoloCommand c,DriveMember m,DriveNumber number)
        {
            var d=state.drive;
            if(c.value=="lane"){
                if(d.stop!="" || !number(c.item,2,out var lane))return "choose-a-road-lane";
                if(d.navigator!="" && d.navigator!=m.actor && d.navigatorUntil>d.clock)return "friend-is-guiding-dad";
                d.navigator=m.actor;d.navigatorUntil=d.clock+1.2;d.lane=lane;m.contributions++;DrivePose(m,"drive");return null;
            }
            if(c.value=="boost"){
                if(d.stop!="" || d.stopIndex>=3 || d.clock<m.nextAction)return "wait-a-moment";
                m.nextAction=d.clock+.7;DrivePose(m,"cheer");m.contributions++;DriveEvent("boost",m.actor);
                if(!d.nomadPassed && d.lane!=d.nomadLane){d.nomadPassed=true;d.passed=Math.Min(18,d.passed+1);d.roadAt=d.clock;}
                return null;
            }
            if(d.stop=="")return "guide-dad-or-cheer";
            if(c.value=="continue"){
                if(!d.stopDone)return "finish-the-stop";
                if(d.members.Any(a=>a.pose=="toilet" && d.clock-a.at<2))return "friend-is-using-the-toilet";
                d.stop="";d.stopIndex++;d.passed=0;d.stopProgress=0;d.stopDone=false;d.nomadPassed=false;d.roadAt=d.clock;d.fuel=0;d.hoseHolder="";DriveEvent("continue",m.actor);return null;
            }
            if(c.value=="cow"){if(d.clock<m.nextAction)return "wait-a-moment";d.cowTaps++;DrivePose(m,"cow");m.nextAction=d.clock+.7;DriveEvent("cow",m.actor);return null;}
            if(d.stop=="petrol" && c.value=="fuel"){
                if(d.hoseHolder==m.actor){d.hoseHolder="";DrivePose(m,"idle");return null;}
                if(d.hoseHolder!="")return "friend-is-holding-the-nozzle";
                if(d.fuel>=1)return "tank-is-full";d.hoseHolder=m.actor;DrivePose(m,"fuel");DriveEvent("fuel",m.actor);return null;
            }
            if(d.clock<m.nextAction)return "wait-a-moment";
            if(d.stop=="toilet" && c.value=="toilet"){
                if(d.members.Any(a=>a.pose=="toilet" && d.clock-a.at<2))return "toilet-in-use";
                m.nextAction=d.clock+2;m.contributions++;DrivePose(m,"toilet");DriveEvent("toilet",m.actor);return null;
            }
            if(d.stop=="roadworks" && c.value=="rubble"){
                d.stopProgress=Math.Min(4,d.stopProgress+1);d.stopDone=d.stopProgress==4 && d.trafficGreen;m.nextAction=d.clock+.75;m.contributions++;DrivePose(m,"rubble");DriveEvent("rubble",m.actor);return null;
            }
            if(d.stop=="roadworks" && c.value=="light"){d.trafficGreen=!d.trafficGreen;d.stopDone=d.stopProgress==4 && d.trafficGreen;DriveEvent("light",m.actor);return null;}
            return "choose-the-stop-action";
        }
        private static void DriveStatue(DriveState d)
        {
            d.bricks=Enumerable.Range(0,12).Select(i=>new DriveBrick{id="drive-brick-"+(++d.brickSerial),variant=i,x=180+i%6*95,y=140+i/6*65,at=d.clock}).ToArray();d.statueBuilt=false;d.buildRevision++;
        }
        private string DriveBuilding(SoloCommand c,DriveMember m,DriveNumber number,DriveRevision revision)
        {
            var d=state.drive;var brick=d.bricks.SingleOrDefault(b=>b.id==c.item);
            if(d.smashing)return "car-is-tumbling-the-statue";
            if(c.value=="brick-claim"){
                if(brick==null || brick.holder!="" || d.bricks.Any(b=>b.holder==m.actor) || !revision(brick.revision))return "brick-in-use-or-changed";
                brick.holder=m.actor;if(brick.snapped)d.statueBuilt=false;brick.snapped=false;brick.revision++;DrivePose(m,"carry");return null;
            }
            if(c.value=="brick-place" || c.value=="brick-turn" || c.value=="brick-drop"){
                if(brick==null || brick.holder!=m.actor || !revision(brick.revision))return "brick-in-use-or-changed";
                if(c.value=="brick-turn")brick.turn=(brick.turn+1)%4;
                else{
                    if(!GoForADrive.BoardPoint(c.x,c.y))return "place-on-the-building-board";
                    brick.x=c.x;brick.y=c.y;brick.holder="";
                    brick.snapped=!d.freeBuild && brick.variant<12 && Math.Abs(c.x-(GoForADriveLayout.StatueX[brick.variant]))<80 && Math.Abs(c.y-(GoForADriveLayout.StatueY[brick.variant]))<80;
                    if(brick.snapped){brick.x=GoForADriveLayout.StatueX[brick.variant];brick.y=GoForADriveLayout.StatueY[brick.variant];brick.turn=0;}
                    m.contributions++;DrivePose(m,"build");
                }
                brick.at=d.clock;brick.revision++;d.buildRevision++;
                if(!d.statueBuilt && Enumerable.Range(0,12).All(i=>d.bricks.Any(b=>b.variant==i && b.snapped))){d.statueBuilt=true;d.statues++;DriveEvent("statue",m.actor);}
                return null;
            }
            if(c.value=="brick-new"){
                if(!d.freeBuild || !number(c.item,47,out var variant) || d.bricks.Length>=300 || d.bricks.Any(b=>b.holder==m.actor))return "choose-an-available-brick";
                d.bricks=d.bricks.Concat(new[]{new DriveBrick{id="drive-brick-"+(++d.brickSerial),variant=variant,holder=m.actor,x=320,y=200,at=d.clock}}).ToArray();d.buildRevision++;DrivePose(m,"carry");return null;
            }
            if(c.value=="free-build"){if(d.bricks.Any(b=>b.holder!=""))return "put-down-before-changing-building-mode";d.freeBuild=!d.freeBuild;return null;}
            if(c.value=="smash"){
                if(d.bricks.Any(b=>b.holder!=""))return "friend-is-holding-a-brick";
                d.smashing=true;d.smashAt=d.clock;d.smashes++;DrivePose(m,"smash");DriveEvent("smash",m.actor);return null;
            }
            return "choose-a-building-action";
        }
        private static void DriveScatter(DriveState d)
        {
            foreach(var b in d.bricks){b.snapped=false;b.x=150+b.variant%6*90;b.y=120+b.variant/6%4*60;b.turn=(b.turn+1)%4;b.revision++;b.at=d.clock;}
            d.statueBuilt=false;d.smashing=false;d.buildRevision++;
        }
        private static string DriveStamp(DriveState d)=>string.Join("|",d.stage,d.packingWave,d.packingClear,d.roadStep,d.passed,d.stop,d.stopDone,d.obstacle,d.windowEligible,d.windowCollected,d.stars,d.buildRevision,d.eventAt,string.Join("",d.members.Select(m=>m.joined?"1":"0")));
        private bool AdvanceDrive(double seconds,string[] active,out bool visible)
        {
            visible=false;var d=state.drive;if(d==null)return false;bool changed=false;var before=DriveStamp(d);
            foreach(var m in d.members){
                bool here=state.players.Any(p=>p.id==m.actor && p.zone==GoForADrive.Zone && (active==null || active.Contains(p.id)));
                if(!here && m.joined){ReleaseDrive(m.actor);changed=true;}
                else if(here && d.begun && !m.declined && !m.joined){m.joined=true;DrivePose(m,d.stage==0?"idle":"sit");changed=true;}
            }
            if(!d.members.Any(m=>m.joined)){visible=changed;return changed;}
            d.clock+=seconds;AmbientChanged=true;
            if(d.smashing && d.clock-d.smashAt>=1)DriveScatter(d);
            foreach(var m in d.members.Where(m=>m.joined && d.clock-m.at>2 && m.held=="" && !d.bricks.Any(b=>b.holder==m.actor)))m.pose=d.stage==0 || d.stage==3?"idle":"sit";
            if(d.navigator!="" && d.clock>=d.navigatorUntil){d.navigator="";changed=true;}
            if(d.stage==0 && !d.packingClear){
                var packed=d.items.Where(p=>p.slot>=0).ToArray();
                // Project the original attachment-position balancing into three
                // pictured roof columns. The lever lets siblings steady one car.
                float target=Math.Clamp((packed.Length==0?0:packed.Sum(p=>p.slot%3-1)/(float)packed.Length)*1.05f+d.balanceLever,-1,1);
                d.balance+=(target-d.balance)*Math.Min(1,(float)seconds*2);
                if(Math.Abs(d.balance)>.92){
                    if(d.unstableAt<0){d.unstableAt=d.clock;DriveEvent("balance",d.members.First(m=>m.joined).actor);}
                    else if(d.clock-d.unstableAt>2){var outer=packed.OrderByDescending(p=>p.slot).FirstOrDefault();if(outer!=null){outer.slot=-1;outer.x=250;outer.y=175;outer.revision++;outer.at=d.clock;}d.unstableAt=-1;d.balancedAt=-1;DriveEvent("buckaroo",d.members.First(m=>m.joined).actor);}
                }else d.unstableAt=-1;
                if(!d.items.All(p=>p.slot>=0) || Math.Abs(d.balance)>.1){d.balancedAt=-1;}
                else if(d.balancedAt<0)d.balancedAt=d.clock;
                else if(d.clock-d.balancedAt>=1.2){
                    if(d.packingWave>=GoForADrive.Difficulty[d.difficulty]){d.packingClear=true;DriveEvent("packed",d.members.First(m=>m.joined).actor);}
                    else{d.packingWave++;var items=GoForADrive.Packing(d.packingWave,d.packingSet);Array.Copy(d.items,items,d.items.Length);d.items=items;d.balancedAt=-1;}
                    changed=true;
                }
            }
            if(d.stage==1 && d.stop=="" && d.stopIndex<3){
                if(d.passed>=GoForADrive.RoadTarget(d)){d.stop=GoForADrive.Stops[d.stopIndex];d.stopProgress=0;d.stopDone=false;DriveEvent("stop",d.members.First(m=>m.joined).actor);changed=true;}
                else if(d.nomadPassed && d.clock-d.roadAt>=1.5){d.nomadPassed=false;d.roadStep++;d.nomadLane=d.roadStep%3;d.roadAt=d.clock;changed=true;}
                else if(!d.nomadPassed && d.passed>0 && d.clock-d.roadAt>=6){d.passed--;d.roadStep++;d.nomadLane=d.roadStep%3;d.roadAt=d.clock;DriveEvent("overtaken",d.members.First(m=>m.joined).actor);changed=true;}
            }
            if(d.stage==1 && d.stop=="petrol" && d.hoseHolder!=""){
                d.fuel=Math.Min(1,d.fuel+(float)(seconds/4.5));if(d.fuel>=1){d.stopDone=true;DriveEvent("filled",d.hoseHolder);d.hoseHolder="";}changed=true;
            }
            if(d.stage==1 && d.stop=="toilet" && d.members.Any(m=>m.joined && m.pose=="toilet" && d.clock-m.at>=1.8)){d.stopDone=true;changed=true;}
            if(d.stage==2 && d.armDropped){
                if(d.armHolder!="" && d.armDragUntil<=d.clock)d.armHolder="";
                if(d.armHolder!="")d.armHeight=d.armDragHeight*300;
                else if(d.armJumpUntil>d.clock)d.armHeight=(float)Math.Sin(Math.Clamp((d.clock-d.armJumpAt)/(d.armJumpUntil-d.armJumpAt),0,1)*Math.PI)*(d.armJumpStage==2?300:200);
                else{d.armJumpStage=0;d.armHeight=Math.Max(0,d.armHeight-(float)seconds*500);}
                var pattern=GoForADriveWindowLayout.Patterns[d.obstacleKind];d.windowDistance+=(float)(seconds*d.windowSpeed*GoForADriveWindowLayout.PixelsPerUnit);
                float root=GoForADriveWindowLayout.ArmX+pattern.span*GoForADriveWindowLayout.PixelsPerUnit*.5f-d.windowDistance;
                float localX=GoForADriveWindowLayout.ArmX-root,localY=GoForADriveWindowLayout.HandY+d.armHeight-GoForADriveWindowLayout.Floor;
                var winner=d.members.FirstOrDefault(m=>m.actor==d.armActor && m.joined)??d.members.First(m=>m.joined);
                if(d.windowEligible && pattern.obstacle.Contains(localX,localY)){d.windowEligible=false;d.windowSpeed=Math.Max(3,d.windowSpeed-.5);DriveEvent("window-bump",winner.actor);}
                if(d.windowEligible && !d.windowCollected && pattern.reward.Contains(localX,localY)){
                    d.windowCollected=true;d.windowWinner=winner.actor;d.stickers[d.stars%10]=Array.IndexOf(d.members,winner)+1;d.stars++;winner.contributions++;if(d.stars%10==0)d.windowSpeed+=.5;DriveEvent("star",winner.actor);
                }
                if(d.windowDistance>=pattern.span*GoForADriveWindowLayout.PixelsPerUnit){d.obstacle++;d.obstacleKind=d.obstacle%15;d.obstacleAt=d.clock;d.windowDistance=0;d.windowEligible=true;d.windowCollected=false;}
                changed=true;
            }
            // Continuous clocks/fill/scrolling use the existing ambient path.
            // Only discrete transitions invalidate commands and request a save.
            visible=before!=DriveStamp(d);return true;
        }
    }
}
