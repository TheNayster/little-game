using System;
using System.Collections.Generic;
using System.IO;
using LittleWeeps.Client;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace LittleWeeps.EditorTools
{
    public static class CharacterWorkshopBuild
    {
        private const string ArtRoot = "Assets/FamilyPlayset/Art/Characters";
        public const string ScenePath = "Assets/FamilyPlayset/Scenes/CharacterWorkshop.unity";

        [MenuItem("Little Weeps/Character Workshop/Rebuild isolated scene")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bluey = CreateVisual("bluey");
            var bingo = CreateVisual("bingo");
            var actorRoot = new GameObject("Workshop movement root");
            actorRoot.transform.position = new Vector3(-1.4f, -1.6f, 0);
            var actor = (GameObject)PrefabUtility.InstantiatePrefab(bluey, actorRoot.transform);
            var friendRoot = new GameObject("Depth reference root");
            friendRoot.transform.position = new Vector3(1.3f, -1.6f, 0);
            var friend = (GameObject)PrefabUtility.InstantiatePrefab(bingo, friendRoot.transform);
            var camera = new GameObject("Workshop Camera", typeof(Camera)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 4;
            camera.transform.position = new Vector3(1.1f, 1.2f, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.89f,.94f,.89f);
            var workshop = new GameObject("Character Workshop", typeof(CharacterWorkshop)).GetComponent<CharacterWorkshop>();
            workshop.character = actor.GetComponent<CharacterView>(); workshop.companion = friend.GetComponent<CharacterView>(); workshop.viewCamera = camera;
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("CHARACTER_WORKSHOP_PROJECT=" + Path.GetFullPath(Path.Combine(Application.dataPath, "..")));
        }

        private static GameObject CreateVisual(string characterId)
        {
            var Art = ArtRoot + "/" + characterId;
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Art + "/import-manifest.json"));
            foreach (var layer in manifest.layers)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Art + "/" + layer.name + ".png");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }
            var visual = new GameObject(manifest.displayName + " visual");
            visual.transform.localScale = Vector3.one * manifest.scale;
            var view = visual.AddComponent<CharacterView>();
            view.characterId = manifest.assetId; view.displayName = manifest.displayName;
            view.sorting = visual.AddComponent<SortingGroup>();
            view.facing = Joint("Facing", visual.transform, Vector2.zero);
            var joints = new Dictionary<string, Transform>();
            var renderers = new Dictionary<string, SpriteRenderer>();
            var order = 0;
            foreach (var layer in manifest.layers)
            {
                var pivot = new Vector2(layer.pivotX, layer.pivotY);
                var parent = view.facing;
                var local = new Vector2((pivot.x - 220) / 100, (454 - pivot.y) / 100);
                if (layer.name.StartsWith("eyes-") || layer.name == "muzzle") { parent = joints["head"]; local = Vector2.zero; }
                if (layer.name == "held-prop")
                {
                    var hand = new Vector3((pivot.x - 220) / 100, (454 - pivot.y) / 100, 0);
                    view.handAnchor = Joint("Hand anchor", joints["arm-near"], hand - joints["arm-near"].localPosition);
                    parent = view.handAnchor; local = Vector2.zero;
                }
                var joint = Joint(layer.name, parent, local);
                joints.Add(layer.name, joint);
                var spriteObject = new GameObject("Sprite", typeof(SpriteRenderer));
                spriteObject.transform.SetParent(joint, false);
                spriteObject.transform.localPosition = new Vector3((layer.left + layer.width / 2f - pivot.x) / 100,
                    (pivot.y - layer.top - layer.height / 2f) / 100, 0);
                var sprite = spriteObject.GetComponent<SpriteRenderer>();
                sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "/" + layer.name + ".png");
                if (sprite.sprite == null) throw new InvalidOperationException("Missing imported layer " + layer.name);
                sprite.sortingOrder = order++;
                renderers.Add(layer.name, sprite);
            }
            view.body = joints["body"]; view.head = joints["head"]; view.tail = joints["tail"];
            view.armFar = joints["arm-far"]; view.armNear = joints["arm-near"];
            view.footFar = joints["foot-far"]; view.footNear = joints["foot-near"];
            view.eyesOpen = renderers["eyes-open"]; view.eyesClosed = renderers["eyes-closed"];
            view.heldProp = joints["held-prop"]; view.heldProp.gameObject.SetActive(false); view.eyesClosed.enabled = false;
            var prefab = PrefabUtility.SaveAsPrefabAsset(visual, Art + "/" + manifest.displayName + "View.prefab");
            UnityEngine.Object.DestroyImmediate(visual);
            return prefab;
        }

        public static void Windows()
        {
            Create();
            var args = Environment.GetCommandLineArgs(); string folder = null;
            for (var i = 0; i < args.Length - 1; i++) if (args[i] == "-characterOutput") folder = args[i + 1];
            if (string.IsNullOrEmpty(folder) || Directory.Exists(folder)) throw new IOException("Specify a fresh -characterOutput folder.");
            Directory.CreateDirectory(folder);
            // A separate product name keeps even Unity's window preferences
            // outside the family's app. Restore project settings after the build.
            var product = PlayerSettings.productName;
            var background = PlayerSettings.runInBackground;
            BuildReport report;
            try
            {
                PlayerSettings.productName = "Little Weeps Character Workshop";
                PlayerSettings.runInBackground = true;
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64,
                    subtarget = (int)StandaloneBuildSubtarget.Player,
                    locationPathName = Path.Combine(folder, "LittleWeepsCharacterWorkshop.exe"),
                    options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport
                });
            }
            finally { PlayerSettings.productName = product; PlayerSettings.runInBackground = background; AssetDatabase.SaveAssets(); }
            var summary = report.summary;
            File.WriteAllText(Path.Combine(folder, "build-summary.json"), JsonUtility.ToJson(new BuildEvidence {
                result = summary.result.ToString(), unity = Application.unityVersion,
                project = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                scene = ScenePath, familyVersionUnchanged = PlayerSettings.bundleVersion,
                errors = summary.totalErrors, warnings = summary.totalWarnings,
                development = (summary.options & BuildOptions.Development) != 0
            }, true));
            if (summary.result != BuildResult.Succeeded) throw new Exception("Character workshop build failed.");
        }

        private static Transform Joint(string name, Transform parent, Vector2 local)
        { var joint = new GameObject(name).transform; joint.SetParent(parent, false); joint.localPosition = local; return joint; }
        [Serializable] private sealed class Manifest { public Layer[] layers; public string assetId, displayName; public float scale; }
        [Serializable] private sealed class Layer { public string name; public int left, top, width, height; public float pivotX, pivotY; }
        [Serializable] private sealed class BuildEvidence { public string result, unity, project, scene, familyVersionUnchanged; public int errors, warnings; public bool development; }
    }
}
