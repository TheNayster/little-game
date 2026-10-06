using System;
using System.Collections.Generic;
using System.Linq;

namespace LittleWeeps.Client
{
    // Observes authority state, never creates a gameplay event or delays a round.
    public sealed class RevealReactions
    {
        public const float Duration=2.2f,SurpriseSeconds=.8f;
        public readonly struct Occupant
        {
            public readonly string actor;public readonly int slot;public readonly bool found;
            public Occupant(string actor,int slot,bool found){this.actor=actor;this.slot=slot;this.found=found;}
        }
        public sealed class Reaction
        {
            public string actor;public int slot,variant,index,count;public float start;
        }
        private string continuity;
        private readonly Dictionary<string,Occupant> previous=new Dictionary<string,Occupant>();
        private readonly Dictionary<string,Reaction> active=new Dictionary<string,Reaction>();
        private readonly HashSet<string> consumed=new HashSet<string>();
        public int Events {get;private set;}
        public Reaction[] Active(float now)=>active.Values.Where(r=>now-r.start<Duration).OrderBy(r=>r.actor,StringComparer.Ordinal).ToArray();
        public void Cancel(string actor)=>active.Remove(actor);
        public void Reset(){continuity=null;previous.Clear();active.Clear();consumed.Clear();}
        public bool Observe(string key,int round,IEnumerable<Occupant> occupants,float now,bool ready)
        {
            if(!ready){Reset();return false;}
            var list=occupants.ToArray();var fresh=continuity!=key;
            if(fresh){Reset();continuity=key;}
            var changed=false;
            foreach(var current in list){
                if(current.found && consumed.Add(current.actor) && !fresh && previous.TryGetValue(current.actor,out var prior) && prior.slot>=0 && !prior.found){
                    var peers=previous.Values.Where(o=>o.slot==prior.slot).OrderBy(o=>o.actor,StringComparer.Ordinal).ToArray();
                    active[current.actor]=new Reaction{actor=current.actor,slot=prior.slot,variant=Variant(round,current.actor),index=Array.FindIndex(peers,o=>o.actor==current.actor),count=peers.Length,start=now};
                    Events++;changed=true;
                }
            }
            previous.Clear();foreach(var o in list)previous[o.actor]=o;
            foreach(var id in active.Keys.ToArray())if(now-active[id].start>=Duration || !previous.TryGetValue(id,out var o) || !o.found)active.Remove(id);
            return changed;
        }
        public static int Variant(int round,string actor)
        {
            // Explicit stable hash: runtime string hashes differ across clients.
            uint hash=2166136261;unchecked{foreach(var c in round+"/"+actor){hash^=c;hash*=16777619;}}return (int)(hash%3);
        }
    }
}
