using System;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // Only an authoritative unbuilt-to-built transition reveals a tower.
    // Rejected dry/underfilled tips cue the next action without removing fill.
    public sealed class SandTipFeedback
    {
        public const float Duration=1.9f, WiggleDuration=.4f;
        private readonly SandMould[] previous=new SandMould[DaycareSandpit.MaxPieces];
        private readonly float[] remaining=new float[DaycareSandpit.MaxPieces];
        private readonly string[] outcomes=new string[DaycareSandpit.MaxPieces];
        private readonly int[] events=new int[DaycareSandpit.MaxPieces];
        private int round=-1;private bool observing;
        public int[] Events=>(int[])events.Clone();
        public float Remaining(int i)=>remaining[i];
        public string Outcome(int i)=>outcomes[i];
        public void Underfilled(int i,int lesson)=>Cue(i,lesson,"underfilled");
        public void Cue(int i,int lesson,string outcome)
        {if(i>=0 && i<remaining.Length && observing && round==lesson && remaining[i]==0){outcomes[i]=outcome;remaining[i]=WiggleDuration;}}
        public void Observe(SandpitState state,bool active,float elapsed)
        {
            var baseline=!observing || state==null || round!=state.round || !active;
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                remaining[i]=baseline?0:Math.Max(0,remaining[i]-elapsed);
                var m=state!=null && i<state.moulds.Length?state.moulds[i]:null;var p=previous[i];
                string result=null;
                if(!baseline && p!=null && m!=null && p.id==m.id){
                    if(!p.built && m.built)result="reveal";
                    
                }
                if(result!=null){outcomes[i]=result;remaining[i]=Duration;events[i]++;}
                // Refilling or watering after a dry reset takes priority over its old effect.
                if(outcomes[i]=="collapse" && m!=null && (m.scoops>0 || m.wet || m.built))remaining[i]=0;
                if(remaining[i]==0)outcomes[i]=null;
                previous[i]=m?.Copy();
            }
            round=state?.round ?? -1;observing=active && state!=null;
        }
    }
}
