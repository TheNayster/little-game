#if MARKETS_ISOLATED
// Explicit opt-in rule-only check while another world's shared integration is
// incomplete. Default checks compile the actual complete production Core tree.
namespace LittleWeeps.Core
{
    public sealed class SoloPlayer { public string id, zone; }
}
#endif
