using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Client;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    public sealed class NetworkGardenSession : MonoBehaviour,IGardenSession
    {
        private NetworkProbe probe;
        private readonly GardenCommandQueue queue=new GardenCommandQueue();
        private readonly Queue<(float time,NetworkProbe.State state)> replies=new Queue<(float,NetworkProbe.State)>();
        private float nextSend,nextPose,pendingSince;
        private long poseTick;
        private readonly Dictionary<string,MotionBuffer> tracks=new Dictionary<string,MotionBuffer>();
        private readonly Dictionary<string,MotionBuffer> toyTracks=new Dictionary<string,MotionBuffer>();
        private WalkInput walking=new WalkInput();
        private long walkSequence,lastChangedSequence;
        private double nextWalkSend,lastMotionReceived;
        private string ownGeneration;
        private Vector2 anticipated;
        // Leave room for the 50 ms snapshot spacing plus the tested 80 ms
        // delay, 25 ms jitter and a missed snapshot. Own-player anticipation
        // stays immediate; only other players/held props use this history.
        public const double InterpolationDelay=.18;
        public void Initialize(NetworkProbe source)
        {probe=source;probe.Received+=Receive;probe.MotionReceived+=CapturePositions;probe.LostConnection+=Disconnected;}
        public string Actor=>probe.Settings.profile;
        public string PreferenceScope=>"shared.lab."+probe.Settings.runId+"."+Actor+".";
        public bool Connected=>probe.ConnectedToServer;
        public bool Busy=>queue.Busy;
        public bool MutedTest=>probe.Settings.verifyGarden;
        public string Status=>Connected?(Busy?"Finishing your move…":"Playing together"):"Connection stopped — close this window and rejoin";
        public SoloSnapshot View=>probe.Latest?.view;
        public string[] Players=>probe.Latest?.connected??Array.Empty<string>();
        public long ViewSequence=>probe.Latest?.sequence??0;
        public bool Submit(SoloCommand command,Action<SoloResult> complete)
        {
            if(!Connected){complete?.Invoke(new SoloResult(false,"disconnected",0));return false;}
            // Capture the visit at input time, never while rebasing a retry.
            var player=View.players.First(p=>p.id==Actor);command.zone=player.zone;command.visit=player.visit;
            return queue.Enqueue(command,complete);
        }
        private void Receive(NetworkProbe.State state)
        {
            CapturePositions();
            foreach(var pose in probe.Latest?.poses??Array.Empty<NetworkProbe.DragPose>())
            {
                if(!toyTracks.TryGetValue(pose.item,out var track))toyTracks[pose.item]=track=new MotionBuffer();
                track.Add(pose.lease,state.time,pose.x,pose.y);
            }
            foreach(var key in toyTracks.Keys.ToArray())if(!View.toys.Any(t=>t.id==key && !string.IsNullOrEmpty(t.holder)))toyTracks.Remove(key);
            if(string.IsNullOrEmpty(state.requestId))return;
            // Deterministic delayed-ack test, enabled only in isolated test configs.
            replies.Enqueue((Time.realtimeSinceStartup+(MutedTest ? .25f : 0),state));
        }
        private void Update()
        {
            if(probe==null)return;
            var now=Time.realtimeSinceStartup;
            if(Connected && View!=null)
            {
                if(ownGeneration==null)CapturePositions();
                if(now>=nextWalkSend){nextWalkSend=now+.05;SendWalking(false);}
                var dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
                var p=View.players.First(v=>v.id==Actor);
                // Local anticipation is visual only; no local world mutation or
                // inventory prediction. Full input rollback/replay is not used.
                if(now-lastMotionReceived<.5)
                {
                    var next=Walking.Step(anticipated.x,anticipated.y,walking,dt);anticipated=new Vector2(next.X,next.Y);
                    if(probe.InputAck(Actor)>=lastChangedSequence)
                    {
                        var age=(float)Math.Max(0,Math.Min(.2,probe.ServerClock-probe.PositionTime(Actor)));
                        var forecast=Walking.Step(p.x,p.y,walking,age);var target=new Vector2(forecast.X,forecast.Y);
                        anticipated=Vector2.Distance(anticipated,target)>90?target:Vector2.Lerp(anticipated,target,1-Mathf.Exp(-12*dt));
                    }
                }
                else anticipated=Vector2.Lerp(anticipated,new Vector2(p.x,p.y),1-Mathf.Exp(-12*dt));
            }
            while(replies.Count>0 && replies.Peek().time<=now)
            {
                var state=replies.Dequeue().state;
                queue.Complete(state.requestId,new SoloResult(state.accepted,state.outcome,state.view.revision,state.duplicate));
            }
            if(!Connected || View==null || now<nextSend)return;
            nextSend=now+.05f;
            var command=queue.Take(View.revision);
            if(command!=null){pendingSince=now;probe.Submit(command);}
            // Never replay an uncertain command under a new request ID. Surface
            // the stopped connection and let server departure cleanup run.
            if(queue.PendingId!=null && now-pendingSince>12)probe.DisconnectGuest();
        }
        public void Preview(string item,Vector2 point)
        {
            if(!Connected || Time.realtimeSinceStartup<nextPose)return;
            var pose=probe.Latest?.poses?.FirstOrDefault(p=>p.item==item && p.actor==Actor);
            if(pose==null)return;
            nextPose=Time.realtimeSinceStartup+.1f;
            probe.Preview(new NetworkProbe.DragPose{actor=Actor,item=item,lease=pose.lease,x=point.x,y=point.y,tick=++poseTick});
        }
        public bool TryPreview(string item,out Vector2 point)
        {
            var pose=probe.Latest?.poses?.FirstOrDefault(p=>p.item==item);
            point=pose==null?Vector2.zero:new Vector2(pose.x,pose.y);
            if(pose!=null && toyTracks.TryGetValue(item,out var track)){var p=track.Sample(probe.ServerClock-InterpolationDelay);point=new Vector2(p.X,p.Y);}
            return pose!=null;
        }
        public void Walk(WalkMode mode,float x=0,float y=0)
        {
            if(!Connected || View==null)return;
            var p=View.players.First(v=>v.id==Actor);
            if(walking.mode==mode && walking.x==x && walking.y==y && walking.zone==p.zone && walking.visit==p.visit)return;
            walking=new WalkInput{actor=Actor,zone=p.zone,visit=p.visit,mode=mode,x=x,y=y};
            lastChangedSequence=walkSequence+1;SendWalking(mode==WalkMode.Stop);
        }
        private void SendWalking(bool reliable)
        {
            var p=View.players.First(v=>v.id==Actor);
            walking.actor=Actor;walking.zone=p.zone;walking.visit=p.visit;walking.sequence=++walkSequence;
            probe.SendWalk(walking,reliable);
        }
        private void CapturePositions()
        {
            if(View==null)return;
            lastMotionReceived=Time.realtimeSinceStartupAsDouble;
            foreach(var id in tracks.Keys.ToArray())if(!Players.Contains(id))tracks.Remove(id);
            foreach(var p in View.players.Where(p=>Players.Contains(p.id)))
            {
                var generation=p.zone+":"+p.visit;
                if(!tracks.TryGetValue(p.id,out var track))tracks[p.id]=track=new MotionBuffer();
                track.Add(generation,probe.PositionTime(p.id),p.x,p.y);
                if(p.id==Actor && generation!=ownGeneration)
                {ownGeneration=generation;anticipated=new Vector2(p.x,p.y);walking=new WalkInput{actor=Actor,zone=p.zone,visit=p.visit,mode=WalkMode.Stop};lastChangedSequence=walkSequence+1;}
            }
        }
        public Vector2 VisualPosition(string actor)
        {
            var p=View.players.First(v=>v.id==actor);
            if(actor==Actor && Connected && ownGeneration==p.zone+":"+p.visit)return anticipated;
            if(tracks.TryGetValue(actor,out var track)){var point=track.Sample(probe.ServerClock-InterpolationDelay);return new Vector2(point.X,point.Y);}
            return new Vector2(p.x,p.y);
        }
        private void Disconnected(){replies.Clear();queue.Disconnect();tracks.Clear();toyTracks.Clear();ownGeneration=null;}
        private void OnDestroy(){if(probe!=null){probe.Received-=Receive;probe.MotionReceived-=CapturePositions;probe.LostConnection-=Disconnected;}}
    }
}
