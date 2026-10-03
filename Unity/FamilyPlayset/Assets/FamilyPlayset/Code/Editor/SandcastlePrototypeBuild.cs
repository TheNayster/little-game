using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using LittleWeeps.Client;

namespace LittleWeeps.EditorTools
{
    public static class SandcastlePrototypeBuild
    {
        public const string Scene="Assets/FamilyPlayset/Scenes/SandcastleVisualPrototype.unity";
        [MenuItem("Little Weeps/Sandcastle/Open isolated visual prototype")]
        public static void Open()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!File.Exists(Scene))Create();else EditorSceneManager.OpenScene(Scene);
        }
        private static void Create()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Isolated Sandcastle prototype",typeof(SandcastlePrototype));
            EditorSceneManager.SaveScene(scene,Scene);
        }
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach(var name in new[]{"empty-pit","pieces"}){var texture=AssetImporter.GetAtPath("Assets/FamilyPlayset/Resources/Worlds/Daycare/SandcastleClub/Prototype/"+name+".png") as TextureImporter;
            texture.alphaIsTransparency=true;texture.mipmapEnabled=false;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.wrapMode=TextureWrapMode.Clamp;texture.SaveAndReimport();}
            Create();
            // Validate inverse projection over the entire unchanged authority domain.
            for(var x=DaycareLeft();x<=4800;x+=5)for(var y=70;y<=540;y+=5){var p=SandcastleProjection.Inverse(SandcastleProjection.Project(x,y));if(Vector2.Distance(p,new Vector2(x,y))>.01f)throw new Exception("Projection inverse mismatch");}
            var args=Environment.GetCommandLineArgs();var i=Array.IndexOf(args,"-prototypeOutput");if(i<0)throw new Exception("Fresh explicit prototype output required");
            var output=Path.GetFullPath(args[i+1]);if(File.Exists(output))throw new IOException("Refuse stale prototype output");Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=output,target=BuildTarget.StandaloneWindows64,subtarget=(int)StandaloneBuildSubtarget.Player,options=BuildOptions.StrictMode});
            var s=report.summary;File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"build-result.json"),"{\"result\":\""+s.result+"\",\"errors\":"+s.totalErrors+",\"warnings\":"+s.totalWarnings+",\"projectionSamples\":13965}");
            if(s.result!=BuildResult.Succeeded)throw new Exception("Prototype build failed");
        }
        private static int DaycareLeft()=>4070;
    }
}
