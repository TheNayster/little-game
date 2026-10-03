using System;

namespace LittleWeeps.Core
{
    // A test deadline is not a world lifetime. Player counts and idle activity
    // clocks must never decide whether a persistent home authority exits.
    public static class SessionLifetime
    {
        public static bool Expired(double elapsed, bool interactive, bool persistentServer)
        {
            if(double.IsNaN(elapsed) || double.IsInfinity(elapsed) || elapsed<0)
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            return !persistentServer && elapsed>(interactive?7200:240);
        }
    }
}
