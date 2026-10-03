using System;
using LittleWeeps.Core;

namespace LittleWeeps.Client
{
    // Observe shared fill, never a speculative tap or command reply. Re-entry is a baseline.
    public sealed class SandScoopFeedback
    {
        public const float Duration = .75f;
        private readonly int[] counts = new int[DaycareSandpit.MaxPieces];
        private readonly float[] remaining = new float[DaycareSandpit.MaxPieces];
        private readonly int[] events = new int[DaycareSandpit.MaxPieces];
        private int round = -1;
        private bool observing;
        public int[] Events => (int[])events.Clone();
        public float Remaining(int index) => remaining[index];
        public void Observe(SandpitState state, bool active, float elapsed)
        {
            var baseline = !observing || state == null || round != state.round || !active;
            for (var i = 0; i < DaycareSandpit.MaxPieces; i++)
            {
                remaining[i] = baseline ? 0 : Math.Max(0, remaining[i] - elapsed);
                var count = state == null || i>=state.moulds.Length ? 0 : state.moulds[i].scoops;
                if (!baseline && count > counts[i])
                {
                    remaining[i] = Duration;
                    events[i] += count - counts[i];
                }
                counts[i] = count;
            }
            round = state == null ? -1 : state.round;
            observing = active && state != null;
        }
    }
}
