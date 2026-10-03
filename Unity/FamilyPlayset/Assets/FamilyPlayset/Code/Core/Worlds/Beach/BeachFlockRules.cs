using System;
using System.Linq;
using LittleWeeps.Core;

namespace LittleWeeps.Core.Worlds.Beach
{
    // Owns flock transitions/validation. The shared world only supplies its state and players.
    internal static class BeachFlockRules
    {
        internal static void Validate(SoloSnapshot s)
        {
            if(s.seagulls==null && s.schema<WorldLayout.Schema)return;
            // Unity can deserialize an omitted nested record as all defaults.
            if(s.schema<BeachSeagulls.Schema){
                var old=s.seagulls;if(old!=null && old.phase==0 && old.clock==0 && old.age==0 && old.calmUntil==0 && old.from==0 && old.to==0 && old.flights==0 && (old.nearby==null || old.nearby.Length==0))s.seagulls=null;
                if(s.seagulls!=null)throw new InvalidOperationException("Seagulls require schema 37.");return;
            }
            var g=s.seagulls;
            if(g==null || !Enum.IsDefined(typeof(GullPhase),g.phase) || !KeepyRules.Finite(g.clock) || g.clock<0 ||
                !KeepyRules.Finite(g.age) || g.age<0 || !KeepyRules.Finite(g.calmUntil) || g.calmUntil<0 ||
                g.from<0 || g.from>=3 || g.to<0 || g.to>=3 || g.trailFrom<0 || g.trailFrom>=3 || g.flights<0 ||
                g.phase!=GullPhase.Resting && (g.from==g.to || g.age>BeachSeagulls.Duration(g.phase)+.00001) ||
                g.nearby==null || g.nearby.Length>4 || g.nearby.Distinct().Count()!=g.nearby.Length || g.nearby.Any(id=>!s.players.Any(p=>p.id==id)))
                throw new InvalidOperationException("Invalid shared seagull flock.");
        }
        internal static void Start(SeagullState g,SoloPlayer[] watchers)
        {

            g.trailFrom=g.from;
            // Prefer the patch with most breathing room for all beach players.
            g.to=Enumerable.Range(0,3).Where(i=>i!=g.from).OrderByDescending(i=>watchers.Length==0?Math.Abs(i-g.from):watchers.Min(p=>(p.x-BeachSeagulls.Patches[i])*(p.x-BeachSeagulls.Patches[i])+(p.y-BeachSeagulls.GroundY)*(p.y-BeachSeagulls.GroundY))).ThenBy(i=>i).First();
            g.phase=GullPhase.Notice;g.age=0;g.flights=g.flights==int.MaxValue?0:g.flights+1;
        }
        internal static bool Advance(SeagullState g,double dt,SoloPlayer[] players,string[] active,out bool visible)
        {
            visible=false;if(g==null)return false;
            var watchers=players.Where(p=>p.zone=="beach" && (active==null || active.Contains(p.id))).ToArray();
            if(watchers.Length==0){var changed=g.nearby.Length>0;g.nearby=Array.Empty<string>();return changed;}
            g.clock+=dt;
            var near=watchers.Where(p=>BeachSeagulls.Near(p,g.from,BeachSeagulls.Radius)).Select(p=>p.id).OrderBy(id=>id,StringComparer.Ordinal).ToArray();
            if(g.phase==GullPhase.Resting && g.clock>=g.calmUntil && near.Any(id=>!g.nearby.Contains(id))){Start(g,watchers);visible=true;}
            g.nearby=near;
            if(g.phase==GullPhase.Resting){g.age=0;return true;}
            g.age+=dt;
            while(g.phase!=GullPhase.Resting && g.age>=BeachSeagulls.Duration(g.phase)){
                g.age-=BeachSeagulls.Duration(g.phase);visible=true;
                if(g.phase==GullPhase.Landing){g.phase=GullPhase.Resting;g.age=0;g.from=g.to;g.calmUntil=g.clock+BeachSeagulls.CalmSeconds;
                    g.nearby=watchers.Where(p=>BeachSeagulls.Near(p,g.from,BeachSeagulls.Radius)).Select(p=>p.id).OrderBy(id=>id,StringComparer.Ordinal).ToArray();}
                else g.phase++;
            }
            return true;
        }
    }
}
