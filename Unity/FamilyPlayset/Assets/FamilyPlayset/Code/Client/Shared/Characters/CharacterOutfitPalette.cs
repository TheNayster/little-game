using UnityEngine;
namespace LittleWeeps.Client
{
    public static class CharacterOutfitPalette
    {
        public static Color Cloth(string id)=>id=="pink"?new Color(.94f,.39f,.66f):id=="blue"?new Color(.22f,.63f,.94f):
            id=="red"?new Color(.9f,.25f,.23f):new Color(.38f,.70f,.28f);
    }
}
