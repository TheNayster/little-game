using System;
using System.IO;
using LittleWeeps.Client;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class SoloBuild
    {
        public const string ScenePath="Assets/FamilyPlayset/Scenes/SoloPrototype.unity";
        public const string ProfilePath="Assets/BuildProfiles/Windows Solo Prototype.asset";
        public const string AndroidProfilePath="Assets/BuildProfiles/Android Solo Prototype.asset";
        public static void Windows()=>Build(false);
        public static void Android()=>Build(true);
        private static void Build(bool android)
        {
            var target=android?BuildTarget.Android:BuildTarget.StandaloneWindows64;
            var profilePath=android?AndroidProfilePath:ProfilePath;
            if(!File.Exists(ScenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.95f,.94f,.86f);camera.transform.position=new Vector3(0,0,-10);
                new GameObject("Solo Play",typeof(SoloScreen));EditorSceneManager.SaveScene(scene,ScenePath);
            }
            if(!File.Exists(profilePath) && !AssetDatabase.CopyAsset(android?FoundationBuild.AndroidProfilePath:FoundationBuild.WindowsProfilePath,profilePath))throw new IOException("Cannot create solo Build Profile.");
            var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);profile.name=android?"Android Solo Prototype":"Windows Solo Prototype";
            profile.overrideGlobalScenes=true;profile.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
            FoundationBuild.ValidateProfile(profile,target,false,ScenePath);
            var number=23;var args=Environment.GetCommandLineArgs();for(var i=0;i<args.Length-1;i++)if(args[i]=="-familyBuildNumber")number=int.Parse(args[i+1]);
            PlayerSettings.bundleVersion="0.0."+number;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            if(android)
            {
                if(PlayerSettings.Android.useCustomKeystore)throw new InvalidOperationException("Do not replace family signing settings for an emulator probe.");
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
                PlayerSettings.Android.bundleVersionCode=number;
                EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            }
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",".."));var folder=Path.Combine(root,"Builds",android?"AndroidSolo":"WindowsSolo","G2-0.0."+number);
            if(Directory.Exists(folder))throw new IOException("Use a fresh solo build number.");Directory.CreateDirectory(folder);
            var output=Path.Combine(folder,android?"LittleWeepsSolo.apk":"LittleWeepsSolo.exe");
            using var androidTools=android?AndroidFoundationTools.Configure():null;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions{buildProfile=profile,locationPathName=output,options=BuildOptions.StrictMode|BuildOptions.DetailedBuildReport});
            var summary=report.summary;
            File.WriteAllText(Path.Combine(folder,"build-summary.json"),JsonUtility.ToJson(new Evidence{version=PlayerSettings.bundleVersion,unity=Application.unityVersion,profile=profilePath,
                platform=android?"Android":"Windows",signing=android?"Default Android debug certificate; isolated emulator probe only":"Not applicable",
                result=summary.result.ToString(),output=output,utc=DateTime.UtcNow.ToString("O"),errors=summary.totalErrors,warnings=summary.totalWarnings,development=(summary.options&BuildOptions.Development)!=0},true));
            if(summary.result!=BuildResult.Succeeded || (summary.options&BuildOptions.Development)!=0)throw new Exception("Solo build failed.");
        }
        [Serializable] private sealed class Evidence{public string version,unity,profile,result,output,utc,platform,signing;public int errors,warnings;public bool development;}
    }
}
