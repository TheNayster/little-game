using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LittleWeeps.EditorTools
{
    public static class FamilyMobileBuild
    {
        public static void IOS()
        {
#if UNITY_IOS
            const string scenePath="Assets/FamilyPlayset/Scenes/NetworkProbe.unity";
            const string profilePath="Assets/BuildProfiles/iPad Family LAN.asset";
            if(!File.Exists(scenePath))throw new InvalidOperationException("Qualified shared scene is required.");
            var args=Environment.GetCommandLineArgs();var at=Array.IndexOf(args,"-familyBuildNumber");
            if(at<0 || at+1>=args.Length || !int.TryParse(args[at+1],out var number) || number<71)throw new ArgumentException("Fresh family LAN build number required.");
            if(!File.Exists(profilePath) && !AssetDatabase.CopyAsset(SoloBuild.IOSProfilePath,profilePath))throw new IOException("Cannot create iPad family profile.");
            var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);profile.name="iPad Family LAN";
            profile.overrideGlobalScenes=true;profile.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
            EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
            FoundationBuild.ValidateProfile(profile,BuildTarget.iOS,false,scenePath);
            var plugin=(PluginImporter)AssetImporter.GetAtPath("Assets/Plugins/iOS/LittleWeepsFamily.mm");
            plugin.SetCompatibleWithAnyPlatform(false);plugin.SetCompatibleWithEditor(false);plugin.SetCompatibleWithPlatform(BuildTarget.iOS,true);plugin.SaveAndReimport();
            PlayerSettings.bundleVersion="0.0."+number;PlayerSettings.iOS.buildNumber=number.ToString();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;PlayerSettings.iOS.appleEnableAutomaticSigning=true;
            if(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS)!="com.littleweeps.familyplayset")throw new InvalidOperationException("Preserve the family's existing app identity.");
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",".."));
            var folder=Path.Combine(root,"Builds","iOSFamilyLAN","G3-0.0."+number);
            if(Directory.Exists(folder))throw new IOException("Use a fresh build number.");Directory.CreateDirectory(folder);
            var output=Path.Combine(folder,"Xcode");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions{buildProfile=profile,locationPathName=output,options=BuildOptions.StrictMode|BuildOptions.DetailedBuildReport});
            if(report.summary.result!=BuildResult.Succeeded || (report.summary.options&BuildOptions.Development)!=0)throw new InvalidOperationException("iPad export failed.");
            var plistPath=Path.Combine(output,"Info.plist");var plist=new PlistDocument();plist.ReadFromFile(plistPath);
            plist.root.SetString("NSLocalNetworkUsageDescription","Little Weeps finds your family's game nearby so you can play together.");
            plist.root.CreateArray("NSBonjourServices").AddString(WindowsService);
            plist.WriteToFile(plistPath);
            var pbxPath=PBXProject.GetPBXProjectPath(output);var project=new PBXProject();project.ReadFromFile(pbxPath);
            var framework=project.GetUnityFrameworkTargetGuid();project.AddFrameworkToProject(framework,"Security.framework",false);
            var source=project.FindFileGuidByProjectPath("Libraries/Plugins/iOS/LittleWeepsFamily.mm");
            if(string.IsNullOrEmpty(source))throw new InvalidOperationException("Native family plugin missing from Xcode project.");
            project.SetCompileFlagsForFile(framework,source,new System.Collections.Generic.List<string>{"-fobjc-arc"});project.WriteToFile(pbxPath);
            File.WriteAllText(Path.Combine(folder,"build-summary.json"),JsonUtility.ToJson(new Evidence{version=PlayerSettings.bundleVersion,unity=Application.unityVersion,profile=profilePath,output=output,
                result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,utc=DateTime.UtcNow.ToString("O")},true));
#else
            throw new InvalidOperationException("Launch Unity with -buildTarget iOS.");
#endif
        }
        private const string WindowsService="_lw-playset._udp";
        [Serializable] private sealed class Evidence
        {
            public string version,unity,profile,output,result,utc;
            public string platform="iOS",signing="Unsigned Xcode export; native compile/sign and physical networking pending";
            public bool development=false;
            public int errors,warnings;
        }
    }
}
