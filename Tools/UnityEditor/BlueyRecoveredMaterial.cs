// BLU-01: serialized data reader for the isolated Bluey asset bake only.
// No recovered gameplay methods execute in the probe.
using UnityEngine;
namespace LegoDuplo.Materials
{
    public sealed class LegoBrickMaterial : MonoBehaviour
    {
        public int preset;
        public Color color=Color.white,primaryDecalColor=Color.white,secondaryDecalColor=Color.white;
        public Texture2D decal,secondaryDecal,shadowMap,bumpMap;
        public Vector2 decalTiling=Vector2.one,decalOffset,secondaryDecalTiling=Vector2.one,secondaryDecalOffset;
    }
}
