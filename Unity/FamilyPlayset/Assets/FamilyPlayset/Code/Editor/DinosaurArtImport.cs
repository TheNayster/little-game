using UnityEditor;
namespace LittleWeeps.EditorTools
{
    public sealed class DinosaurArtImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/DinosaurWorld/") && !assetPath.EndsWith("/WorldMenu/dinosaur-world.png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=assetPath.Contains("/WorldMenu/")?TextureImporterType.Sprite:TextureImporterType.Default;
            if(assetPath.Contains("/WorldMenu/"))t.spriteImportMode=SpriteImportMode.Single;
            t.textureShape=TextureImporterShape.Texture2D;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=assetPath.Contains("/WorldMenu/")?512:2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
