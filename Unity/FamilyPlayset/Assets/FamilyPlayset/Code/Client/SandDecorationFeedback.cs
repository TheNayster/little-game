using System;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // One effect per tower follows its latest authoritative decoration. Replacement
    // supersedes the old prop; redundant commands and reconnect baselines never replay.
    public sealed class SandDecorationFeedback
    {
        public const float Duration=1f, WiggleDuration=.4f;
        private readonly int[] previous=new int[4], values=new int[4], events=new int[4];
        private readonly float[] remaining=new float[4], wiggles=new float[2];
        private int round=-1;private bool observing;
        public int[] Events=>(int[])events.Clone();
        public float Remaining(int i)=>remaining[i];
        public int Value(int i)=>values[i];
        public float Wiggle(int i)=>wiggles[i];
        public void Reject(string operation,int lesson)
        {if(observing && round==lesson && (operation=="flag" || operation=="shell"))wiggles[operation=="flag"?0:1]=WiggleDuration;}
        public void Observe(SandpitState state,bool active,float elapsed)
        {
            var baseline=!observing || state==null || round!=state.round || !active;
            for(var i=0;i<2;i++)wiggles[i]=baseline?0:Math.Max(0,wiggles[i]-elapsed);
            for(var i=0;i<4;i++){
                remaining[i]=baseline?0:Math.Max(0,remaining[i]-elapsed);
                var m=state?.moulds[i];var current=m!=null && m.built?m.decoration:0;
                if(!baseline && current>0 && current!=previous[i]){values[i]=current;remaining[i]=Duration;events[i]++;}
                if(current==0 || current!=values[i])remaining[i]=0;
                previous[i]=current;
            }
            round=state?.round ?? -1;observing=active && state!=null;
        }
    }
}
