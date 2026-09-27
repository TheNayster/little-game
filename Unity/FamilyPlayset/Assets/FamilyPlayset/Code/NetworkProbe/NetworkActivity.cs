using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LittleWeeps.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    public sealed partial class NetworkProbe
    {
        private const string ActivityMessage="littleweeps.activity.v1";
        private readonly HashSet<ulong> activityPeers=new HashSet<ulong>();
        private double nextActivitySend,nextLegacyHeat;
        private bool activityDirty;
        private long activityPackets,activityBytes;
        public event Action<double> ActivityReceived;
        // One item per datagram bounds every update below the motion budget,
        // even with four held objects and four ovens. New samples replace old
        // ones; durable actions and completed dishes still use reliable state.
        [Serializable] private sealed class ActivitySample
        {public string epoch,item,dish;public double time,heat;public DragPose pose;}
        [Serializable] private sealed class ActivityMetrics
        {public long packets,bytes;public int maxBytes;}
        private int largestActivity;

        private void TickActivity(double now)
        {
            if(config.role!="server" || now<nextActivitySend)return;nextActivitySend=now+.1;
            var view=session.View();
            var heating=view.toys.Where(t=>t.kitchen?.dish!=null && Kitchen.Next(t.kitchen.dish)=="heat" &&
                Kitchen.Slot(t.container,out var group,out _) && group=="oven").ToArray();
            foreach(var peer in network.ConnectedClientsIds)
            {
                if(!activityPeers.Contains(peer))
                {
                    // A staged rollout can retain older clients. They receive
                    // their existing full-state format at a bounded cadence.
                    if(activityDirty || heating.Length>0 && now>=nextLegacyHeat)Send(StateMessage,peer,Current());
                    continue;
                }
                foreach(var pose in poses.Values)SendActivity(peer,new ActivitySample{epoch=epoch,time=ServerClock,pose=pose});
                foreach(var toy in heating)SendActivity(peer,new ActivitySample{epoch=epoch,time=ServerClock,item=toy.id,
                    dish=toy.kitchen.dish.id,heat=toy.kitchen.dish.heat});
            }
            if(now>=nextLegacyHeat)
            {
                nextLegacyHeat=now+.5;
                WriteJson(Path.Combine(output,"activity-stats.json"),new ActivityMetrics{packets=activityPackets,bytes=activityBytes,maxBytes=largestActivity});
            }
            activityDirty=false;
        }
        private void SendActivity(ulong peer,ActivitySample sample)
        {
            var size=Utf8.GetByteCount(JsonUtility.ToJson(sample));largestActivity=Math.Max(largestActivity,size);
            Send(ActivityMessage,peer,sample,NetworkDelivery.UnreliableSequenced);activityPackets++;activityBytes+=size;
        }
        private void ReceiveActivity(ulong sender,FastBufferReader reader)
        {
            if(config.role!="client" || sender!=NetworkManager.ServerClientId || !ConnectedToServer || Latest==null || applicationPaused || retryPending || localOnly)return;
            try
            {
                var sample=JsonUtility.FromJson<ActivitySample>(Read(reader));
                if(sample==null || sample.epoch!=epoch || !KeepyRules.Finite(sample.time) || sample.time<Latest.time)return;
                if(sample.pose!=null && !string.IsNullOrEmpty(sample.pose.item))
                {
                    var p=sample.pose;
                    var toy=Latest.view.toys.FirstOrDefault(t=>t.id==p.item && t.holder==p.actor);
                    var current=Latest.poses?.FirstOrDefault(v=>v.item==p.item && v.actor==p.actor && v.lease==p.lease);
                    if(toy==null || current==null || p.tick<current.tick || !WorldLayout.Position(toy.zone,Latest.view.schema,p.x,p.y))return;
                    current.x=p.x;current.y=p.y;current.tick=p.tick;ActivityReceived?.Invoke(sample.time);
                }
                else if(!string.IsNullOrEmpty(sample.item))
                {
                    var toy=Latest.view.toys.FirstOrDefault(t=>t.id==sample.item);
                    var dish=toy?.kitchen?.dish;
                    if(dish==null || dish.id!=sample.dish || Kitchen.Next(dish)!="heat" ||
                        !Kitchen.Slot(toy.container,out var group,out _) || group!="oven" ||
                        !KeepyRules.Finite(sample.heat) || sample.heat<0 || sample.heat>Kitchen.HeatSeconds)return;
                    dish.heat=Math.Max(dish.heat,sample.heat);
                }
            }
            catch(ArgumentException){ /* A bad transient sample cannot change durable state. */ }
            catch(InvalidDataException){ }
        }
        private void PreserveActivityProgress(State incoming)
        {
            if(Latest==null || Latest.epoch!=incoming.epoch)return;
            // Reliable snapshots and disposable samples are different streams.
            // A delayed snapshot must not rewind a matching bake or drag lease.
            foreach(var toy in incoming.view.toys)
            {
                var dish=toy.kitchen?.dish;
                if(dish==null || Kitchen.Next(dish)!="heat")continue;
                var previous=Latest.view.toys.FirstOrDefault(t=>t.id==toy.id)?.kitchen?.dish;
                if(previous!=null && previous.id==dish.id) dish.heat=Math.Max(dish.heat,previous.heat);
            }
            foreach(var pose in incoming.poses??Array.Empty<DragPose>())
            {
                var previous=Latest.poses?.FirstOrDefault(p=>p.item==pose.item && p.actor==pose.actor && p.lease==pose.lease);
                if(previous==null || previous.tick<=pose.tick)continue;
                pose.x=previous.x;pose.y=previous.y;pose.tick=previous.tick;
            }
        }
    }
}
