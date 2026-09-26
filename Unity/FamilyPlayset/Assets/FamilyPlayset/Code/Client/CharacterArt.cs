using System;
using UnityEngine;

namespace LittleWeeps.Client
{
    // Build-time references keep the same exported art in the native workshop
    // and the game's overlay canvas, with no runtime file or network dependency.
    public sealed class CharacterArt : ScriptableObject
    {
        public string characterId, displayName;
        public float scale, groundX, groundY;
        public Layer[] layers;
        public Layer[] profileLayers;
        [Serializable] public sealed class Layer
        {
            public string name;
            public Sprite sprite;
            public float left, top, width, height, pivotX, pivotY;
        }
    }
}
