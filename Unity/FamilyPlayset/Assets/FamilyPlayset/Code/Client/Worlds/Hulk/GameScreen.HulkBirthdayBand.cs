using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    [Serializable] public sealed class BirthdaySoundView {public string key;public float time,volume,pitch;public bool playing;}
    public sealed partial class GameScreen
    {
        private string birthdayBandApproach="";
        private bool birthdaySocketApproach;
        private long birthdayBandVisit=-1;
        private readonly BoatVisualClock birthdayClock=new BoatVisualClock();
        private readonly Dictionary<string,AudioSource> birthdayAudio=new Dictionary<string,AudioSource>();
        private readonly Dictionary<string,RawImage> birthdayInstruments=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Image> birthdayBandTargets=new Dictionary<string,Image>();
        private readonly List<RawImage> birthdayFloorCells=new List<RawImage>();
        private RectTransform birthdayFloorRoot,birthdayBandPlay;
        private Text birthdayBandHint,birthdayBandParticipation;
        private Button birthdayReturnBattery;
        private int[] birthdayFloorColours=Array.Empty<int>();
        private int birthdayDrawnLightStep;
        public int[] BirthdayFloorColours=>birthdayFloorColours.ToArray();
        public int BirthdayDrawnLightStep=>birthdayDrawnLightStep;
        public BirthdaySoundView[] BirthdaySounds=>birthdayAudio.Select(p=>new BirthdaySoundView{key=p.Key,time=p.Value.clip?p.Value.time:0,volume=p.Value.volume,pitch=p.Value.pitch,playing=p.Value.isPlaying}).ToArray();
        private void BeginBirthdayBand(string station)
        {
            if(Birthday.props.Any(p=>p.holder==Actor))return;
            CancelBirthdayIntent();birthdayBandApproach=station;birthdayBandVisit=ReadPlayer(Actor).visit;
        }
        private void BeginBirthdaySocket(){CancelBirthdayIntent();birthdaySocketApproach=true;birthdayBandVisit=ReadPlayer(Actor).visit;}
        private void BuildBirthdayBandUi()
        {
            var floor=BirthdayArt.Data.danceFloor;
            birthdayFloorRoot=Rect(Board,"Birthday dance floor",Vector2.zero,Vector2.zero);
            for(int i=0;i<floor.cells.Length;i++){
                var image=BirthdayPicture(birthdayFloorRoot,"Birthday dance tile "+i,Vector2.zero,Vector2.one);image.texture=BirthdayArt.Texture(floor.texture);image.uvRect=floor.cells[i].uv;
                image.raycastTarget=true;NavButton(image,()=>BeginBirthdayBand("dance"));birthdayFloorCells.Add(image);
            }
            birthdayFloorColours=new int[floor.cells.Length];
            foreach(string station in new[]{"guitar","keyboard","drums","socket"}){
                var target=Plain(Board,station=="socket"?"Birthday battery socket":"Birthday "+station,Vector2.zero,new Vector2(110,95),Color.clear,true);birthdayBandTargets.Add(station,target);
                NavButton(target,()=>{
                    if(station!="socket")BeginBirthdayBand(station);
                    else if(Birthday.props.Any(p=>p.kind=="battery" && p.holder==Actor))BeginBirthdaySocket();
                    else BirthdayTapProp("birthday-battery");
                });
            }
            foreach(string prop in new[]{"GuitarActor","DrumStickActor_right","DrumStickActor_left"})birthdayInstruments.Add(prop,BirthdayImage("Birthday instrument "+prop));
            birthdayBandPlay=BirthdayButton(Board,"Play band together",Vector2.zero,new Vector2(260,60),()=>BeginBirthdayBand("band")).transform.parent as RectTransform;
            birthdayBandHint=Label(Board,"",23,Vector2.zero,new Vector2(620,45));
            birthdayBandParticipation=BirthdayButton(birthdayPartyActions,"Birthday band participation",new Vector2(-50,0),new Vector2(240,60),()=>{
                var own=Birthday.members.Single(m=>m.actor==Actor);
                if(HulkBirthday.Instrument(own.station) || own.station=="dance")BirthdaySend("station",target:"");else BeginBirthdayBand("band");
            });
            birthdayReturnBattery=BirthdayButton(birthdayPartyActions,"Return battery",new Vector2(0,0),new Vector2(240,60),BeginBirthdaySocket).transform.parent.GetComponent<Button>();
            foreach(var row in BirthdayArt.Data.audio){var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;birthdayAudio.Add(row.key,source);}
        }
        private Vector2 BirthdayHandPoint(string body,bool left)
        {
            if(!birthdayFrames.TryGetValue(body,out var pose))return birthdayHoldPoints[body];
            var anchor=(left?pose.action.leftHand:pose.action.rightHand)[pose.frame]-pose.action.pivot;
            if(birthdayMirrors.TryGetValue(body,out var mirror) && mirror)anchor.x=-anchor.x;
            return ToBoard(birthdayPositions[body].x,birthdayPositions[body].y)+anchor*pose.action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
        }
        private string BirthdayBandBody(BirthdayState game,string station)
        {
            var member=game.members.FirstOrDefault(m=>m.station==station && birthdayFrames.ContainsKey(m.actor));if(member!=null)return member.actor;
            return game.cast.FirstOrDefault(n=>n.station==station && n.phase!="band-walk" && n.controller=="" && birthdayFrames.ContainsKey(n.id))?.id??"";
        }
        private void TickBirthdayBandUi(BirthdayState game)
        {
            if(birthdayFloorRoot==null)return;
            var own=ReadPlayer(Actor);var member=game.members.Single(m=>m.actor==Actor);int place=Array.IndexOf(game.members,member);
            var held=game.props.FirstOrDefault(p=>p.holder==Actor);
            if(birthdayBandApproach!=""){
                if(own.visit!=birthdayBandVisit || held!=null)birthdayBandApproach="";
                else {
                    string station=birthdayBandApproach=="band"?HulkBirthday.RoleInstrument(member.role):birthdayBandApproach;
                    var target=new Vector2(HulkBirthday.StageX(station,place),HulkBirthday.StageY(station,place));
                    if(Vector2.Distance(new Vector2(own.x,own.y),target)<60 && !ActionPending){string action=birthdayBandApproach;birthdayBandApproach="";destination=null;BirthdaySend(action=="band"?"band":"station",target:station);}
                    else destination=target;
                }
            }
            if(birthdaySocketApproach){
                if(own.visit!=birthdayBandVisit || held?.kind!="battery")birthdaySocketApproach=false;
                else if(Vector2.Distance(new Vector2(own.x,own.y),new Vector2(HulkBirthday.BatteryX,HulkBirthday.BatteryY))<60 && !ActionPending){birthdaySocketApproach=false;destination=null;BirthdaySend("socket");}
                else destination=new Vector2(HulkBirthday.BatteryX,HulkBirthday.BatteryY);
            }
            double clock=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,game.clock,Time.unscaledTimeAsDouble,Shared);
            bool occupied=game.members.Any(m=>HulkBirthday.Instrument(m.station) || m.station=="dance") || game.cast.Any(n=>(HulkBirthday.Instrument(n.station) || n.station=="dance") && n.phase!="band-walk");
            bool lit=game.powered && (occupied || game.bandRoundAt>=0);var floor=BirthdayArt.Data.danceFloor;
            birthdayFloorRoot.gameObject.SetActive(true);birthdayDrawnLightStep=game.lightStep;
            for(int i=0;i<floor.cells.Length;i++){
                var cell=floor.cells[i];var image=birthdayFloorCells[i];int colour=lit?HulkBirthday.FloorColour(game.lightSeed,game.lightStep,i):-1;
                birthdayFloorColours[i]=colour;image.color=colour<0?floor.offColor:floor.colors[colour];
                image.rectTransform.anchoredPosition=new Vector2((floor.worldStart+cell.pixels.center.x-cameraX)*sceneScale,(cell.pixels.center.y-400)*sceneScale);
                image.rectTransform.sizeDelta=cell.pixels.size*sceneScale;
            }
            foreach(var pair in birthdayBandTargets){
                pair.Value.gameObject.SetActive(true);string station=pair.Key;
                var at=station=="socket"?new Vector2(HulkBirthday.BatteryX,HulkBirthday.BatteryY):new Vector2(HulkBirthday.StageX(station),HulkBirthday.StageY(station));
                pair.Value.rectTransform.anchoredPosition=ToBoard(at.x,at.y)+new Vector2(0,45)*sceneScale;pair.Value.rectTransform.sizeDelta=new Vector2(110,95)*sceneScale;
            }
            foreach(var pair in birthdayInstruments){
                bool guitar=pair.Key=="GuitarActor",left=guitar || pair.Key=="DrumStickActor_right";
                string carrier=BirthdayBandBody(game,guitar?"guitar":"drums");bool holding=carrier!="";
                var prop=BirthdayArt.PropView(pair.Key,holding?"held-front":"front");var image=pair.Value;
                image.gameObject.SetActive(true);image.texture=BirthdayArt.Texture(prop.texture);image.uvRect=new Rect(0,0,1,1);
                image.rectTransform.pivot=(holding?prop.holdPivot:prop.pivot)/256;image.rectTransform.sizeDelta=Vector2.one*256*prop.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;
                image.rectTransform.anchoredPosition=holding?BirthdayHandPoint(carrier,left):guitar?ToBoard(3168,193):ToBoard(2988,166);
            }
            birthdayBandPlay.gameObject.SetActive(true);birthdayBandPlay.anchoredPosition=ToBoard(2985,249)+new Vector2(0,240)*sceneScale;birthdayBandPlay.localScale=Vector3.one*sceneScale;
            birthdayBandHint.gameObject.SetActive(true);birthdayBandHint.rectTransform.anchoredPosition=birthdayBandPlay.anchoredPosition+new Vector2(0,60)*sceneScale;
            birthdayBandHint.text=game.bandRoundAt>=0?(clock<game.bandRoundAt?"Band starts in "+Math.Max(1,(int)Math.Ceiling(game.bandRoundAt-clock)):"Happy birthday, Hulk!"):
                game.bandInviteAt>=0?"Spidey: keys  ·  Spin: guitar  ·  Gwen: drums":"Make music together";
            birthdayBandParticipation.transform.parent.gameObject.SetActive(held==null && (HulkBirthday.Instrument(member.station) || member.station=="dance" || member.station=="" && game.bandInviteAt>=0));
            birthdayBandParticipation.text=HulkBirthday.Instrument(member.station) || member.station=="dance"?"Leave band":"Join the band";
            birthdayReturnBattery.gameObject.SetActive(held?.kind=="battery");
            TickBirthdayAudio(game,clock);
        }
        private void TickBirthdayAudio(BirthdayState game,double clock)
        {
            var wanted=new Dictionary<string,(double time,float volume,float pitch,bool loop)>();
            double introAge=clock-game.introAt;
            bool song=game.bandRoundAt>=0 && clock>=game.bandRoundAt;
            if(song)wanted[game.powered?"electric":"acoustic"]=(clock-game.bandRoundAt,.45f,1,false);
            else if(game.songAt>=0){
                double beat=game.bandBeat+Math.Max(0,clock-game.clock)*(game.powered?1:.5);
                foreach(string station in new[]{"guitar","keyboard","drums"}){
                    string body=BirthdayBandBody(game,station);if(body=="")continue;
                    string role=game.members.FirstOrDefault(m=>m.actor==body)?.role??game.cast.Single(n=>n.id==body).role;
                    if(HulkBirthday.RoleInstrument(role)==station)wanted[station]=(beat,.25f*(game.powered?1:.5f),game.powered?1:.5f,true);
                }
            }else if(game.introAt>=0 && introAge>=0 && introAge<BirthdayArt.Data.audio.Single(a=>a.key=="intro").duration)wanted["intro"]=(introAge,.35f,1,false);
            else wanted["main"]=(Math.Max(0,clock-Math.Max(0,game.introAt)-11),.25f,1,true);
            foreach(var pair in birthdayAudio){
                var source=pair.Value;
                if(!wanted.TryGetValue(pair.Key,out var cue) || applicationPaused || musicMuted){if(source.isPlaying)source.Stop();continue;}
                var clip=BirthdayArt.Audio(pair.Key);
                if(!cue.loop && cue.time>=clip.length){if(source.isPlaying)source.Stop();continue;}
                float target=(float)(cue.loop?cue.time%clip.length:cue.time);target=Mathf.Clamp(target,0,clip.length-1f/clip.frequency);
                if(source.clip!=clip){source.Stop();source.clip=clip;}
                source.loop=cue.loop;source.pitch=cue.pitch;source.volume=cue.volume*ForegroundDucking;
                float error=Mathf.Abs(source.time-target);if(cue.loop)error=Mathf.Min(error,clip.length-error);
                if(!source.isPlaying || error>.18f)source.time=target;
                if(!source.isPlaying)source.Play();
            }
        }
        private void HideBirthdayBand()
        {
            birthdayBandApproach="";birthdaySocketApproach=false;
            if(birthdayFloorRoot!=null)birthdayFloorRoot.gameObject.SetActive(false);
            if(birthdayBandPlay!=null)birthdayBandPlay.gameObject.SetActive(false);if(birthdayBandHint!=null)birthdayBandHint.gameObject.SetActive(false);
            foreach(var image in birthdayInstruments.Values)image.gameObject.SetActive(false);
            foreach(var target in birthdayBandTargets.Values)target.gameObject.SetActive(false);
            foreach(var source in birthdayAudio.Values)if(source.isPlaying)source.Stop();
        }
        private void AddBirthdayBandDepth(Action<RectTransform,float,int,string> add)
        {
            if(birthdayFloorRoot!=null)add(birthdayFloorRoot,float.MaxValue,0,"birthday-floor");
            foreach(var pair in birthdayInstruments){string body=BirthdayBandBody(Birthday,pair.Key=="GuitarActor"?"guitar":"drums");float y=body!=""?birthdayPositions[body].y:193;add(pair.Value.rectTransform,ToBoard(0,y).y,3,"birthday-instrument-"+pair.Key);}
            foreach(var target in birthdayBandTargets.Values)add(target.rectTransform,-1000,4,target.name);
            if(birthdayBandPlay!=null)add(birthdayBandPlay,-1000,4,"birthday-band-start");if(birthdayBandHint!=null)add(birthdayBandHint.rectTransform,-1000,4,"birthday-band-hint");
        }
        private void ResetBirthdayBandUi()
        {
            foreach(var source in birthdayAudio.Values){source.Stop();Destroy(source);}birthdayAudio.Clear();birthdayClock.Reset();birthdayFloorCells.Clear();birthdayInstruments.Clear();birthdayBandTargets.Clear();
            birthdayFloorRoot=null;birthdayBandPlay=null;birthdayBandHint=null;birthdayBandApproach="";birthdaySocketApproach=false;birthdayBandVisit=-1;birthdayFloorColours=Array.Empty<int>();
        }
    }
}
