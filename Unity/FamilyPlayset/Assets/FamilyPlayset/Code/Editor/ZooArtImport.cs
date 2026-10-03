using UnityEditor;
namespace LittleWeeps.EditorTools
{
    public sealed class ZooArtImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/Worlds/Zoo/Art/") && !assetPath.EndsWith("/WorldMenu/zoo.png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=assetPath.Contains("/WorldMenu/")?TextureImporterType.Sprite:TextureImporterType.Default;
            t.textureShape=TextureImporterShape.Texture2D;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
