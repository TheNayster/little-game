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
        public void Initialize(NetworkProbe source)
        {probe=source;probe.Received+=Receive;probe.LostConnection+=Disconnected;}
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
            return queue.Enqueue(command,complete);
        }
        private void Receive(NetworkProbe.State state)
        {
            if(string.IsNullOrEmpty(state.requestId))return;
            // Deterministic delayed-ack test, enabled only in isolated test configs.
            replies.Enqueue((Time.realtimeSinceStartup+(MutedTest ? .25f : 0),state));
        }
        private void Update()
        {
            if(probe==null)return;
            var now=Time.realtimeSinceStartup;
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
            point=pose==null?Vector2.zero:new Vector2(pose.x,pose.y);return pose!=null;
        }
        private void Disconnected(){replies.Clear();queue.Disconnect();}
        private void OnDestroy(){if(probe!=null){probe.Received-=Receive;probe.LostConnection-=Disconnected;}}
    }
}
