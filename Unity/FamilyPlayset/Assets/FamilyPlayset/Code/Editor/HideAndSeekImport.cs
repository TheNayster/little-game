using UnityEditor;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public sealed class HideAndSeekImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/HideAndSeek/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.FromInput;
            t.alphaIsTransparency=true;t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        }
        void OnPreprocessAudio()
        {
            if(!assetPath.Contains("/Resources/HideAndSeek/"))return;
            var a=(AudioImporter)assetImporter;a.forceToMono=true;var s=a.defaultSampleSettings;s.loadType=UnityEngine.AudioClipLoadType.DecompressOnLoad;s.compressionFormat=AudioCompressionFormat.Vorbis;s.quality=.8f;a.defaultSampleSettings=s;
        }
    }
}
