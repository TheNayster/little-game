using UnityEditor;
namespace LittleWeeps.EditorTools
{
    public sealed class BathroomArtImport:AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/Worlds/Home/Bathroom/") && !assetPath.EndsWith("/Scenery/home-bathroom.png"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;
            t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Compressed;
        }
    }
}
