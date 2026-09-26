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
        public Texture2D sheet;
        public Texture2D walkSheet;
        public Sprite shadowSprite;
        public string sheetSha256;
        public string walkSheetSha256;
        public float referenceHeight;
        public float walkReferenceHeight;
        public SheetFrame[] frames;
        public SheetFrame[] walkFrames;
        [Serializable] public sealed class SheetFrame
        {
            public string name;
            // Pixel coordinates use the authored sheet's top-left origin.
            public PixelRect pixels;
            public Vector2 ground;
        }
        [Serializable] public sealed class PixelRect
        {
            public float x, y, width, height;
            public Rect Value => new Rect(x, y, width, height);
        }
        [Serializable] public sealed class Layer
        {
            public string name;
            public Sprite sprite;
            public float left, top, width, height, pivotX, pivotY;
        }
    }
}
