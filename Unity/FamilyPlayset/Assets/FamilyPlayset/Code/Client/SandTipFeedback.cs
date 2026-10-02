using System;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // Only authority can remove a full dry fill or change unbuilt to built.
    // Inspect the resulting state first: a water/tip race must reveal, never crumble.
    public sealed class SandTipFeedback
    {
        public const float Duration=1.9f, WiggleDuration=.4f;
        private readonly SandMould[] previous=new SandMould[4];
        private readonly float[] remaining=new float[4];
        private readonly string[] outcomes=new string[4];
        private readonly int[] events=new int[4];
        private int round=-1;private bool observing;
        public int[] Events=>(int[])events.Clone();
        public float Remaining(int i)=>remaining[i];
        public string Outcome(int i)=>outcomes[i];
        public void Underfilled(int i,int lesson)
        {if(observing && round==lesson && remaining[i]==0){outcomes[i]="underfilled";remaining[i]=WiggleDuration;}}
        public void Observe(SandpitState state,bool active,float elapsed)
        {
            var baseline=!observing || state==null || round!=state.round || !active;
            for(var i=0;i<4;i++){
                remaining[i]=baseline?0:Math.Max(0,remaining[i]-elapsed);
                var m=state?.moulds[i];var p=previous[i];
                string result=null;
                if(!baseline && p!=null && m!=null){
                    if(!p.built && m.built)result="reveal";
                    else if(!p.built && !m.built && !m.wet && p.scoops>0 && m.scoops==0)result="collapse";
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
