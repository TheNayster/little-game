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
        public static void Android() => Build(BuildTarget.Android, "Android", "LittleWeeps.apk");
        public static void WindowsServer() => Build(BuildTarget.StandaloneWindows64, "WindowsServer", "LittleWeepsServer.exe");

        private static void Build(BuildTarget target, string platform, string artifact)
        {
            var server = platform == "WindowsServer";
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install the matching Unity platform module before building " + platform + ".");
            if (server && !Directory.Exists(Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "windowsstandalonesupport", "Variations", "win64_server_nondevelopment_mono")))
                throw new InvalidOperationException("Install Windows Dedicated Server support for this exact Unity editor first.");
            FoundationServerSetup.Create();
            var scenePath = server ? FoundationServerSetup.ScenePath : FoundationSetup.ScenePath;
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
                PlayerSettings.SetScriptingBackend(server ? NamedBuildTarget.Server : NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
                PlayerSettings.defaultScreenWidth = 1200;
                PlayerSettings.defaultScreenHeight = 800;
            }
            if (target == BuildTarget.Android)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                // Build-only probe. A stable private family signing key is a separate release gate.
                PlayerSettings.Android.useCustomKeystore = false;
            }
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));
            var folder = Path.Combine(root, "Builds", platform, "G1-" + PlayerSettings.bundleVersion);
            var output = Path.Combine(folder, artifact);
            if (File.Exists(output) || Directory.Exists(output)) throw new IOException("Build output already exists. Use a fresh build number; do not overwrite evidence.");
            Directory.CreateDirectory(folder);
            BuildReport report;
            if (target == BuildTarget.StandaloneWindows64 && !server)
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
                scenes = new[] { scenePath },
                target = target,
                subtarget = server ? (int)StandaloneBuildSubtarget.Server : 0,
                locationPathName = output,
                options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport
            });
            var summary = report.summary;
            if (summary.platform != target) throw new InvalidOperationException("Built platform does not match the requested target.");
            File.WriteAllText(Path.Combine(folder, "build-summary.json"), JsonUtility.ToJson(new Evidence
            {
                platform = platform, version = PlayerSettings.bundleVersion, unity = Application.unityVersion,
                result = summary.result.ToString(), output = output, utc = DateTime.UtcNow.ToString("O"),
                errors = summary.totalErrors, warnings = summary.totalWarnings,
                development = (summary.options & BuildOptions.Development) != 0,
                profile = target == BuildTarget.StandaloneWindows64 && !server ? WindowsProfilePath : "Explicit " + platform + " build configuration",
                scene = scenePath, dedicatedServer = server,
                signing = target == BuildTarget.Android ? "Default Android debug certificate; build-only probe, not family release" : "Not applicable or platform managed"
            }, true));
            if (summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed; inspect the log and build-summary.json.");
            Debug.Log("LITTLE_WEEPS_BUILD succeeded: " + output);
        }

        [Serializable] private sealed class Evidence
        { public string platform, version, unity, result, output, utc, profile, scene, signing; public int errors, warnings; public bool development, dedicatedServer; }
    }
}
