using UnityEditor;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public sealed class PondArtImport:AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/PondArt/"))return;
            // Wide panoramas can trigger Unity's automatic cubemap detection.
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.alphaSource=TextureImporterAlphaSource.FromInput;
            t.alphaIsTransparency=true;t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Compressed;
        }
        private void OnPreprocessAudio()
        {
            if(!assetPath.Contains("/Resources/PondArt/"))return;
            var a=(AudioImporter)assetImporter;a.forceToMono=true;var settings=a.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;a.defaultSampleSettings=settings;
        }
    }
}
