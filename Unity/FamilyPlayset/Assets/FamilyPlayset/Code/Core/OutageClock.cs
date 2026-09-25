using System;
namespace LittleWeeps.Core
{
    // Network loss detection belongs to the transport. Once it is known,
    // local play has no additional grace period or server reconciliation wait.
    public sealed class OutageClock
    {
        public bool Tick(double now,bool foreground,bool disconnected)
        {
            if(double.IsNaN(now) || double.IsInfinity(now) || now<0)throw new ArgumentOutOfRangeException(nameof(now));
            return disconnected && foreground;
        }
    }
}
