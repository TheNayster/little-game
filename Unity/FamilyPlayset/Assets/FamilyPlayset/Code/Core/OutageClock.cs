using System;
namespace LittleWeeps.Core
{
    // Count observed foreground time, not wall time spent asleep/backgrounded.
    public sealed class OutageClock
    {
        public const double DelaySeconds=10;
        private double elapsed, previous=-1;
        private bool observing;
        public bool Tick(double now,bool foreground,bool disconnected)
        {
            if(double.IsNaN(now) || double.IsInfinity(now) || now<0)throw new ArgumentOutOfRangeException(nameof(now));
            if(!disconnected)elapsed=0;
            else if(foreground && observing && previous>=0)elapsed+=Math.Max(0,Math.Min(.25,now-previous));
            previous=now;observing=foreground && disconnected;
            return disconnected && foreground && elapsed>=DelaySeconds;
        }
    }
}
