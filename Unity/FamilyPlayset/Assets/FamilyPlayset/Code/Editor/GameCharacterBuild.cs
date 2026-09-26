using System;
using System.IO;
using System.Linq;
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
            Directory.CreateDirectory(destination); AssetDatabase.Refresh();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            foreach (var id in new[] { "bluey", "bingo" })
            {
                var directory = "Assets/FamilyPlayset/Art/Characters/" + id;
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(directory + "/import-manifest.json"));
                CheckHash(Path.Combine(projectRoot, manifest.source), manifest.sourceSha256);
                CheckHash(Path.Combine(projectRoot, manifest.contract), manifest.contractSha256);
                var path = destination + "/" + id + ".asset";
                var art = AssetDatabase.LoadAssetAtPath<CharacterArt>(path);
                if (art == null) { art = ScriptableObject.CreateInstance<CharacterArt>(); AssetDatabase.CreateAsset(art, path); }
                art.characterId = id; art.displayName = manifest.displayName; art.scale = manifest.scale;
                art.groundX = manifest.groundX; art.groundY = manifest.groundY;
                art.layers = manifest.layers.Select(layer => {
                    var spritePath = directory + "/" + layer.name + ".png";
                    CheckHash(spritePath, layer.sha256);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite == null) throw new BuildFailedException("Missing sprite: " + spritePath);
                    return new CharacterArt.Layer { name = layer.name, sprite = sprite, left = layer.left, top = layer.top,
                        width = layer.width, height = layer.height, pivotX = layer.pivotX, pivotY = layer.pivotY };
                }).ToArray();
                EditorUtility.SetDirty(art);
            }
            AssetDatabase.SaveAssets();
        }
        private static void CheckHash(string path, string expected)
        {
            using var hash = SHA256.Create();
            var actual = BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
            if (actual != expected) throw new BuildFailedException("Re-export changed character source: " + path);
        }
        [Serializable] private sealed class Manifest
        {
            public string displayName, source, sourceSha256, contract, contractSha256;
            public float scale, groundX, groundY;
            public Layer[] layers;
        }
        [Serializable] private sealed class Layer
        { public string name, sha256; public float left, top, width, height, pivotX, pivotY; }
    }
}
