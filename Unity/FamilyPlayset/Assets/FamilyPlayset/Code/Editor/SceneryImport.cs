using UnityEditor;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public sealed class SceneryImport:AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/FamilyPlayset/Resources/Scenery/"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
            importer.isReadable=false;importer.maxTextureSize=4096;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Compressed;
            foreach(var platform in new[]{"Android","iPhone"})
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=platform,overridden=true,maxTextureSize=4096,format=TextureImporterFormat.ASTC_6x6});
        }
    }
}
