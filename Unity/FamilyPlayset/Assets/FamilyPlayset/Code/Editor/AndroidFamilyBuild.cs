using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class AndroidFamilyBuild
    {
        public static void Build()
        {
            const string scene="Assets/FamilyPlayset/Scenes/FamilyNetwork.unity";
            const string profilePath="Assets/BuildProfiles/Android Family LAN.asset";
            if(!UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player).Any(a=>a.name=="LittleWeeps.NetworkProbe"))
                throw new InvalidOperationException("The shared client assembly must be included on Android.");
            var args=Environment.GetCommandLineArgs();var at=Array.IndexOf(args,"-familyBuildNumber");
            if(at<0 || at+1>=args.Length || !int.TryParse(args[at+1],out var number) || number<75)throw new ArgumentException("Fresh Android family LAN build number required.");
            if(!File.Exists(profilePath) && !AssetDatabase.CopyAsset(SoloBuild.AndroidProfilePath,profilePath))throw new IOException("Cannot create Android family profile.");
            var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);profile.name="Android Family LAN";
            profile.overrideGlobalScenes=true;profile.scenes=new[]{new EditorBuildSettingsScene(scene,true)};
            EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();FoundationBuild.ValidateProfile(profile,BuildTarget.Android,false,scene);
            if(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)!="com.littleweeps.familyplayset")throw new InvalidOperationException("Preserve the existing Android identity.");
            if(PlayerSettings.Android.useCustomKeystore)throw new InvalidOperationException("Signing is handled by the protected family-key tool.");
            PlayerSettings.bundleVersion="0.0."+number;PlayerSettings.Android.bundleVersionCode=number;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            // API 36 uses INTERNET for NSD/local sockets. A target-37 upgrade
            // needs its own parent permission flow and device qualification.
            PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
            PlayerSettings.Android.forceInternetPermission=true;
            PlayerSettings.Android.minifyRelease=false;
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",".."));
            var folder=Path.Combine(root,"Builds","AndroidFamilyLAN","G3-0.0."+number);
            if(Directory.Exists(folder))throw new IOException("Use a fresh build number.");Directory.CreateDirectory(folder);
            var output=Path.Combine(folder,"LittleWeeps.apk");
            using var tools=AndroidFoundationTools.Configure();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions{buildProfile=profile,locationPathName=output,options=BuildOptions.StrictMode|BuildOptions.DetailedBuildReport});
            var summary=report.summary;
            File.WriteAllText(Path.Combine(folder,"build-summary.json"),JsonUtility.ToJson(new Evidence{version=PlayerSettings.bundleVersion,unity=Application.unityVersion,profile=profilePath,
                output=output,result=summary.result.ToString(),errors=summary.totalErrors,warnings=summary.totalWarnings,utc=DateTime.UtcNow.ToString("O"),development=(summary.options&BuildOptions.Development)!=0},true));
            if(summary.result!=BuildResult.Succeeded || summary.platform!=BuildTarget.Android || (summary.options&BuildOptions.Development)!=0)throw new InvalidOperationException("Android family build failed.");
        }
        [Serializable] private sealed class Evidence
        {
            public string version,unity,profile,output,result,utc;
            public string platform="Android",signing="Unity intermediate; must be signed with the pinned family key before device installation";
            public bool development;
            public int errors,warnings;
        }
    }
}
