using UnityEditor;
namespace LittleWeeps.EditorTools
{
    public sealed class ZooArtImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/ZooArt/") && !assetPath.EndsWith("/WorldMenu/zoo.png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=assetPath.Contains("/WorldMenu/")?TextureImporterType.Sprite:TextureImporterType.Default;
            t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
