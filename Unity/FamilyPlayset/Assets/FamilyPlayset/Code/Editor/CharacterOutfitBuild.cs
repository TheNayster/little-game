using System;
using System.IO;
using System.Security.Cryptography;
using LittleWeeps.Client;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public sealed class CharacterOutfitBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder=>10;
        public void OnPreprocessBuild(BuildReport report)
        {
            const string imported="Assets/FamilyPlayset/Art/Outfits/Dinosaur/animation-contract.json";
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
            var source=Path.Combine(root,"SourceArt/Characters/Outfits/Dinosaur/animation-contract.json");
            if(Hash(source)!=Hash(imported))throw new BuildFailedException("Reimport changed outfit contract.");
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(imported));
            var directory="Assets/FamilyPlayset/Resources/CharacterOutfitArt/"+manifest.outfit;
            Directory.CreateDirectory(directory);AssetDatabase.Refresh();
            foreach(var c in manifest.characters)
            {
                var path="Assets/FamilyPlayset/Art/Outfits/Dinosaur/"+c.id+"/actions.png";
                if(Hash(Path.Combine(root,c.source))!=c.sourceSha256 || Hash(path)!=c.sourceSha256)
                    throw new BuildFailedException("Changed costume sheet: "+c.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                importer.isReadable=false;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if(texture.width!=c.width || texture.height!=c.height || c.frames.Length!=16 || c.walk.frames.Length!=8 || c.referenceHeight<=0)
                    throw new BuildFailedException("Invalid outfit atlas: "+c.id);
                foreach(var f in c.frames)if(f.pixels.width<=0 || f.pixels.height<=0 || f.pixels.Value.xMin<0 || f.pixels.Value.yMin<0 || f.pixels.Value.xMax>texture.width || f.pixels.Value.yMax>texture.height)
                    throw new BuildFailedException("Invalid outfit frame: "+c.id+"/"+f.name);
                var assetPath=directory+"/"+c.id+".asset";
                var art=AssetDatabase.LoadAssetAtPath<CharacterArt>(assetPath);
                if(art==null){art=ScriptableObject.CreateInstance<CharacterArt>();AssetDatabase.CreateAsset(art,assetPath);}
                art.characterId=c.id;art.displayName=c.displayName;art.scale=c.scale;
                art.sheet=art.walkSheet=texture;art.frames=c.frames;art.walkFrames=c.walk.frames;
                art.sheetSha256=art.walkSheetSha256=c.sourceSha256;
                art.referenceHeight=art.walkReferenceHeight=c.referenceHeight;
                art.shadowSprite=Resources.Load<CharacterArt>("CharacterArt/"+c.id).shadowSprite;
                EditorUtility.SetDirty(art);
            }
            AssetDatabase.SaveAssets();
        }
        private static string Hash(string path){using var h=SHA256.Create();return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        [Serializable] private sealed class Manifest {public string outfit;public Entry[] characters;}
        [Serializable] private sealed class Entry
        {public string id,displayName,source,sourceSha256;public int width,height;public float scale,referenceHeight;public CharacterArt.SheetFrame[] frames;public Walk walk;}
        [Serializable] private sealed class Walk {public CharacterArt.SheetFrame[] frames;}
    }
}
