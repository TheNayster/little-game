using UnityEngine;
namespace LittleWeeps.Client
{
    // Bounded idle portrait; selecting a player loads the full animation art.
    public sealed class CharacterMenuArt : ScriptableObject
    {
        public Texture2D texture;
        public Vector2 pivot,size;
        public string sourceHash;
    }
}
