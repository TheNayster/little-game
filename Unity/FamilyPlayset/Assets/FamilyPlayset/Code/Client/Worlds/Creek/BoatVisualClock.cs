using System;
namespace LittleWeeps.Client
{
    // Shared boat snapshots are sparse. Advance the drawing between receipts,
    // and correct small timing differences by changing speed instead of position.
    // This clock never launches, docks or saves a boat; the authority owns that.
    public sealed class BoatVisualClock
    {
        private string world;
        private bool initialized,predicting;
        private double sample,received,previous,display;
        public double SampleTime=>previous;
        public double Sample(string worldId,double authority,double now,bool predict)
        {
            if(!initialized || world!=worldId || predicting!=predict || authority<sample || now<previous || !predict)
            {
                initialized=true;world=worldId;predicting=predict;sample=display=authority;received=previous=now;return display;
            }
            if(authority!=sample){sample=authority;received=now;}
            var elapsed=now-previous;previous=now;
            var expected=sample+now-received;
            // A repeated Render in the same frame must not advance twice. Never
            // clamp to a short snapshot horizon: that caused periodic freezing.
            var error=expected-(display+elapsed);
            display+=elapsed*Math.Max(.9,Math.Min(1.1,1+error*.25));
            return display;
        }
        public void Reset(){initialized=false;world=null;}
    }
}
