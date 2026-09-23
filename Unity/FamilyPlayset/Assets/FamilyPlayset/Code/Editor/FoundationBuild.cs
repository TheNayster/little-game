using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class FoundationBuild
    {
        public const string WindowsProfilePath = "Assets/BuildProfiles/Windows Foundation.asset";
        public static void Windows() => Build(BuildTarget.StandaloneWindows64, "Windows", "LittleWeeps.exe");
        public static void IOS() => Build(BuildTarget.iOS, "iOS", "Xcode");

        private static void Build(BuildTarget target, string platform, string artifact)
        {
            if (!File.Exists(FoundationSetup.ScenePath)) throw new InvalidOperationException("Run FoundationSetup.Create first.");
            var number = 1;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == "-familyBuildNumber") number = int.Parse(args[i + 1]);
            if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
            PlayerSettings.bundleVersion = "0.0." + number;
            PlayerSettings.Android.bundleVersionCode = number;
            PlayerSettings.iOS.buildNumber = number.ToString();
            if (target == BuildTarget.iOS)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            }
            if (target == BuildTarget.StandaloneWindows64)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
                PlayerSettings.defaultScreenWidth = 1200;
                PlayerSettings.defaultScreenHeight = 800;
            }
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));
            var folder = Path.Combine(root, "Builds", platform, "G1-" + PlayerSettings.bundleVersion);
            var output = Path.Combine(folder, artifact);
            if (File.Exists(output) || Directory.Exists(output)) throw new IOException("Build output already exists. Use a fresh build number; do not overwrite evidence.");
            Directory.CreateDirectory(folder);
            BuildReport report;
            if (target == BuildTarget.StandaloneWindows64)
            {
                var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(WindowsProfilePath);
                if (profile == null) throw new InvalidOperationException("Missing saved Windows build profile.");
                profile.overrideGlobalScenes = true;
                profile.scenes = new[] { new EditorBuildSettingsScene(FoundationSetup.ScenePath, true) };
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
                report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
                { buildProfile = profile, locationPathName = output, options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport });
            }
            else report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { FoundationSetup.ScenePath },
                target = target,
                locationPathName = output,
                options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport
            });
            var summary = report.summary;
            File.WriteAllText(Path.Combine(folder, "build-summary.json"), JsonUtility.ToJson(new Evidence
            {
                platform = platform, version = PlayerSettings.bundleVersion, unity = Application.unityVersion,
                result = summary.result.ToString(), output = output, utc = DateTime.UtcNow.ToString("O"),
                errors = summary.totalErrors, warnings = summary.totalWarnings,
                development = (summary.options & BuildOptions.Development) != 0,
                profile = target == BuildTarget.StandaloneWindows64 ? WindowsProfilePath : "Explicit iOS target"
            }, true));
            if (summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed; inspect the log and build-summary.json.");
            Debug.Log("LITTLE_WEEPS_BUILD succeeded: " + output);
        }

        [Serializable] private sealed class Evidence
        { public string platform, version, unity, result, output, utc, profile; public int errors, warnings; public bool development; }
    }
}
