using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public sealed class NavigationArtBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;
        public void OnPreprocessBuild(BuildReport report)
        {
            foreach(var id in new[]{"home","garden","park","creek","beach","daycare","zoo","dinosaur-world"})
            {
                var path="Assets/FamilyPlayset/Resources/Shared/UI/WorldMenu/"+id+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new BuildFailedException("Missing world picture: "+path);
                // Menus use bounded thumbnail textures, never six full-resolution
                // background paintings or a duplicate animated character atlas.
                if(importer.textureType!=TextureImporterType.Sprite || importer.maxTextureSize!=512 || importer.mipmapEnabled)
                {
                    importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                    importer.maxTextureSize=512;importer.mipmapEnabled=false;importer.isReadable=false;
                    importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
                if(AssetDatabase.LoadAssetAtPath<Sprite>(path)==null)throw new BuildFailedException("World picture was not imported as a sprite: "+id);
            }
        }
    }
}
