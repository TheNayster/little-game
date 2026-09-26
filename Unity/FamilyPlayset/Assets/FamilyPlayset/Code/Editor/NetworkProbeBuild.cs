using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class NetworkProbeBuild
    {
        public static void Windows()
        {
            const string scenePath="Assets/FamilyPlayset/Scenes/NetworkProbe.unity";
            if(!File.Exists(scenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Windows Network Probe",typeof(LittleWeeps.NetworkProbe.NetworkProbe));
                EditorSceneManager.SaveScene(scene,scenePath);
            }
            var args=Environment.GetCommandLineArgs();var at=Array.IndexOf(args,"-familyBuildNumber");
            if(at<0 || at+1>=args.Length || !int.TryParse(args[at+1],out var number) || number<46)throw new ArgumentException("Explicit fresh network build number required.");
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",".."));
            var folder=Path.Combine(root,"Builds","NetworkProbe","G3-0.0."+number);
            if(Directory.Exists(folder))throw new IOException("Use a fresh build number.");Directory.CreateDirectory(folder);
            PlayerSettings.bundleVersion="0.0."+number;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Server,ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            var records=new List<Evidence>();
            foreach(var server in new[]{true,false})
            {
                var name=server?"PC Server Network Probe":"Windows Network Probe";var role=server?"Server":"Client";
                var profilePath="Assets/BuildProfiles/"+name+".asset";
                if(!File.Exists(profilePath) && !AssetDatabase.CopyAsset(server?FoundationBuild.ServerProfilePath:FoundationBuild.WindowsProfilePath,profilePath))throw new IOException("Cannot create profile.");
                var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);profile.name=name;
                profile.overrideGlobalScenes=true;profile.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
                EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
                FoundationBuild.ValidateProfile(profile,BuildTarget.StandaloneWindows64,server,scenePath);
                var output=Path.Combine(folder,role,"LittleWeepsNetwork.exe");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions{buildProfile=profile,locationPathName=output,options=BuildOptions.StrictMode|BuildOptions.DetailedBuildReport});
                var summary=report.summary;
                if(summary.result!=BuildResult.Succeeded || summary.platform!=BuildTarget.StandaloneWindows64 || (summary.options&BuildOptions.Development)!=0 || summary.GetSubtarget<StandaloneBuildSubtarget>()!=(server?StandaloneBuildSubtarget.Server:StandaloneBuildSubtarget.Player))throw new InvalidOperationException("Network build failed or target mismatch.");
                records.Add(new Evidence{role=role,version=PlayerSettings.bundleVersion,unity=Application.unityVersion,profile=profilePath,result=summary.result.ToString(),output=output,errors=summary.totalErrors,warnings=summary.totalWarnings,dedicatedServer=server});
            }
            File.WriteAllText(Path.Combine(folder,"build-summary.json"),JsonUtility.ToJson(new Manifest{utc=DateTime.UtcNow.ToString("O"),builds=records.ToArray()},true));
        }
        [Serializable] private sealed class Manifest{public string utc;public int contract=9,content=Core.WorldLayout.Content,schema=Core.WorldLayout.Schema;public bool gardenPresentation=true;public Evidence[] builds;}
        [Serializable] private sealed class Evidence{public string role,version,unity,profile,result,output;public int errors,warnings;public bool dedicatedServer;}
    }
}
