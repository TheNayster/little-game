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
    public sealed class GameCharacterBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            const string destination = "Assets/FamilyPlayset/Resources/CharacterArt";
            const string importedContract = "Assets/FamilyPlayset/Art/Characters/animation-contract.json";
            Directory.CreateDirectory(destination); AssetDatabase.Refresh();
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            var sourceContract = Path.Combine(root, "SourceArt/Characters/AnimationSheets/animation-contract.json");
            if (Hash(sourceContract) != Hash(importedContract)) throw new BuildFailedException("Reimport changed character sheet contract.");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(importedContract));
            foreach (var character in manifest.characters)
            {
                var directory = "Assets/FamilyPlayset/Art/Characters/" + character.id;
                var sheetPath = directory + "/actions.png";
                if (Hash(Path.Combine(root, character.source)) != character.sourceSha256 || Hash(sheetPath) != character.sourceSha256)
                    throw new BuildFailedException("Reimport changed character sheet: " + character.id);
                var importer = (TextureImporter)AssetImporter.GetAtPath(sheetPath);
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
                importer.isReadable = false; importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048; importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
                if (texture.width != character.width || texture.height != character.height || character.frames.Length != 16)
                    throw new BuildFailedException("Character sheet layout changed: " + character.id);
                foreach (var frame in character.frames)
                    if (frame.pixels.width <= 0 || frame.pixels.height <= 0 || frame.pixels.Value.xMin < 0 || frame.pixels.Value.yMin < 0 ||
                        frame.pixels.Value.xMax > texture.width || frame.pixels.Value.yMax > texture.height)
                        throw new BuildFailedException("Invalid sheet rectangle: " + frame.name);
                var walk = character.walk;
                var walkPath = directory + "/gentle-walk.png";
                if (walk == null || Hash(Path.Combine(root, walk.source)) != walk.sourceSha256 || Hash(walkPath) != walk.sourceSha256)
                    throw new BuildFailedException("Reimport changed walking sheet: " + character.id);
                var walkImporter = (TextureImporter)AssetImporter.GetAtPath(walkPath);
                walkImporter.textureType = TextureImporterType.Default;
                walkImporter.mipmapEnabled = false; walkImporter.alphaIsTransparency = true;
                walkImporter.isReadable = false; walkImporter.npotScale = TextureImporterNPOTScale.None;
                walkImporter.textureCompression = TextureImporterCompression.Uncompressed;
                walkImporter.maxTextureSize = 2048; walkImporter.filterMode = FilterMode.Bilinear;
                walkImporter.wrapMode = TextureWrapMode.Clamp; walkImporter.SaveAndReimport();
                var walkTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(walkPath);
                if (walkTexture.width != walk.width || walkTexture.height != walk.height || walk.frames.Length != 8 || walk.referenceHeight <= 0)
                    throw new BuildFailedException("Walking sheet layout changed: " + character.id);
                foreach (var frame in walk.frames)
                    if (frame.pixels.width <= 0 || frame.pixels.height <= 0 || frame.pixels.Value.xMin < 0 || frame.pixels.Value.yMin < 0 ||
                        frame.pixels.Value.xMax > walkTexture.width || frame.pixels.Value.yMax > walkTexture.height)
                        throw new BuildFailedException("Invalid walk rectangle: " + frame.name);
                var path = destination + "/" + character.id + ".asset";
                var art = AssetDatabase.LoadAssetAtPath<CharacterArt>(path);
                if (art == null) { art = ScriptableObject.CreateInstance<CharacterArt>(); AssetDatabase.CreateAsset(art, path); }
                art.characterId = character.id; art.displayName = character.displayName;
                art.scale = character.scale; art.sheet = texture; art.frames = character.frames;
                art.sheetSha256 = character.sourceSha256; art.referenceHeight = character.referenceHeight;
                art.walkSheet = walkTexture; art.walkFrames = walk.frames;
                art.walkSheetSha256 = walk.sourceSha256; art.walkReferenceHeight = walk.referenceHeight;
                art.shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(directory + "/ground-shadow.png");
                // Keep old studies editable without preloading their textures.
                art.layers = null; art.profileLayers = null;
                EditorUtility.SetDirty(art);
                BuildPortrait(character,texture);
            }
            AssetDatabase.SaveAssets();
        }
        private static void BuildPortrait(Character character,Texture2D source)
        {
            const string folder="Assets/FamilyPlayset/Resources/CharacterMenu";
            if(!Directory.Exists(folder)){Directory.CreateDirectory(folder);AssetDatabase.Refresh();}
            var path=folder+"/"+character.id+".asset";
            var art=AssetDatabase.LoadAssetAtPath<CharacterMenuArt>(path);
            if(art!=null && art.sourceHash==character.sourceSha256 && art.texture!=null)return;
            if(art==null){art=ScriptableObject.CreateInstance<CharacterMenuArt>();AssetDatabase.CreateAsset(art,path);}
            if(art.texture!=null)UnityEngine.Object.DestroyImmediate(art.texture,true);
            var drawing=character.frames[0];var crop=drawing.pixels.Value;
            var factor=Mathf.Min(1,256/Mathf.Max(crop.width,crop.height));
            var width=Mathf.Max(1,Mathf.RoundToInt(crop.width*factor));var height=Mathf.Max(1,Mathf.RoundToInt(crop.height*factor));
            var target=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var previous=RenderTexture.active;
            try{
                Graphics.Blit(source,target,new Vector2(crop.width/source.width,crop.height/source.height),new Vector2(crop.x/source.width,1-crop.yMax/source.height));
                RenderTexture.active=target;
                var portrait=new Texture2D(width,height,TextureFormat.RGBA32,false);portrait.name=character.id+" portrait";
                portrait.ReadPixels(new Rect(0,0,width,height),0,0);portrait.Apply(false,true);portrait.filterMode=FilterMode.Bilinear;portrait.wrapMode=TextureWrapMode.Clamp;
                AssetDatabase.AddObjectToAsset(portrait,art);art.texture=portrait;
                art.pivot=new Vector2((drawing.ground.x-crop.x)/crop.width,(crop.yMax-drawing.ground.y)/crop.height);
                art.size=crop.size*(180/character.referenceHeight)*character.scale;art.sourceHash=character.sourceSha256;EditorUtility.SetDirty(art);
            }finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);}
        }
        private static string Hash(string path)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        [Serializable] private sealed class Manifest { public Character[] characters; }
        [Serializable] private sealed class Character
        {
            public string id, displayName, source, sourceSha256;
            public int width, height;
            public float scale, referenceHeight;
            public CharacterArt.SheetFrame[] frames;
            public WalkSheet walk;
        }
        [Serializable] private sealed class WalkSheet
        {
            public string source, sourceSha256;
            public int width, height;
            public float referenceHeight;
            public CharacterArt.SheetFrame[] frames;
        }
    }
}
