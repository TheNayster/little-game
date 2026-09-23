using System.IO;
using LittleWeeps.Server;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleWeeps.EditorTools
{
    public static class FoundationServerSetup
    {
        public const string ScenePath = "Assets/FamilyPlayset/Scenes/ServerBootstrap.unity";

        public static void Create()
        {
            if (File.Exists(ScenePath)) return;
            var previous = SceneManager.GetActiveScene();
            // Batch Unity starts with an untitled scene; it cannot add another scene to it.
            if (Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                previous = EditorSceneManager.OpenScene(FoundationSetup.ScenePath, OpenSceneMode.Single);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            new GameObject("Server Foundation", typeof(FoundationServer));
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
        }
    }
}
