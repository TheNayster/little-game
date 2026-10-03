using System.Linq;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        // Stage 2 retains saved flags/shells directly in SandShape. New decoration
        // placement is deferred; do not expose the old fixed-four tool UI.
        public int[] SandpitDecorationEffectValues=>new int[LittleWeeps.Core.DaycareSandpit.MaxPieces];
        public int[] SandpitDecorationEvents=>new int[LittleWeeps.Core.DaycareSandpit.MaxPieces];
        public float[] SandpitDecorationEffects=>new float[LittleWeeps.Core.DaycareSandpit.MaxPieces];
        public int[] SandpitDecorationVisuals=>sandShapes.Select(s=>s.decoration).ToArray();
        public bool[] SandpitDecorationHidden=>sandShapes.Select(s=>s.hideDecoration).ToArray();
        public float[] SandpitDecorationWiggles=>new float[2];
        private void ResetSandDecoration(){}
    }
}
