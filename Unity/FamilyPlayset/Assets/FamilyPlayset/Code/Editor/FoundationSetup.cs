using System.IO;
using LittleWeeps.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleWeeps.EditorTools
{
    public static class FoundationSetup
    {
        public const string ScenePath = "Assets/FamilyPlayset/Scenes/Bootstrap.unity";

        [MenuItem("Little Weeps/Set up foundation scene")]
        public static void Create()
        {
            PlayerSettings.companyName = "Little Weeps";
            PlayerSettings.productName = "Little Weeps";
            PlayerSettings.bundleVersion = "0.0.1";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.littleweeps.familyplayset");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.littleweeps.familyplayset");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            EditorSettings.serializationMode = SerializationMode.ForceText;

            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.89f, 0.95f, 0.93f);
                camera.transform.position = new Vector3(0, 0, -10);
                new GameObject("Foundation", typeof(FoundationScreen));
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("LITTLE_WEEPS_SETUP complete: Bootstrap scene and initial identities configured.");
        }
    }
}
