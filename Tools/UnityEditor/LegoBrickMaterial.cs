using UnityEngine;

namespace Lego
{
    // Compatibility reader for serialized fields. Original method bodies were not recovered.
    public sealed class LegoBrickMaterial : MonoBehaviour
    {
        public int materialPreset;
        public Color color = Color.white;
        public Texture2D decal;
        public Vector2 decalTiling = Vector2.one;
        public Vector2 decalOffset;
        public Texture2D shadowMap, normalMap;
        public int renderQueue = -1;
    }
}
