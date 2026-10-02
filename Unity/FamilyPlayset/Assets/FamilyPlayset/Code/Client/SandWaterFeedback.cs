using System;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // Water is idempotent. Only an observed dry-to-wet transition starts a pour,
    // including a sibling's transition; neither taps nor command replies predict it.
    public sealed class SandWaterFeedback
    {
        public const float Duration = 1.25f;
        private readonly bool[] wet = new bool[4];
        private readonly float[] remaining = new float[4];
        private readonly int[] events = new int[4];
        private int round = -1;
        private bool observing;
        public int[] Events => (int[])events.Clone();
        public float Remaining(int index) => remaining[index];
        public void Observe(SandpitState state, bool active, float elapsed)
        {
            var baseline = !observing || state == null || round != state.round || !active;
            for (var i = 0; i < 4; i++)
            {
                remaining[i] = baseline ? 0 : Math.Max(0, remaining[i] - elapsed);
                var current = state != null && state.moulds[i].wet;
                if (!baseline && current && !wet[i])
                {
                    remaining[i] = Duration;
                    events[i]++;
                }
                // A reset/removal must not leave water pouring into a dry mould.
                if (!current) remaining[i] = 0;
                wet[i] = current;
            }
            round = state == null ? -1 : state.round;
            observing = active && state != null;
        }
    }
}
