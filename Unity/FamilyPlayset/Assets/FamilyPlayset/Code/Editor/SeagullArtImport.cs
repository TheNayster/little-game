using UnityEditor;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public sealed class SeagullArtImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/BeachArt/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;
            t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;
            t.maxTextureSize=2048;t.filterMode=FilterMode.Bilinear;t.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
