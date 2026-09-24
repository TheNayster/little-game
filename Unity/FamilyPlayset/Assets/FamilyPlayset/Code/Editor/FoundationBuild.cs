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
        public const string IOSProfilePath = "Assets/BuildProfiles/iPad Foundation.asset";
        public const string AndroidProfilePath = "Assets/BuildProfiles/Android Foundation.asset";
        public const string ServerProfilePath = "Assets/BuildProfiles/Windows Server Foundation.asset";
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
            var profilePath = server ? ServerProfilePath : target == BuildTarget.iOS ? IOSProfilePath :
                target == BuildTarget.Android ? AndroidProfilePath : WindowsProfilePath;
            var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
            ValidateProfile(profile, target, server, scenePath);
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
                if (PlayerSettings.Android.useCustomKeystore)
                    throw new InvalidOperationException("A custom signing key is configured. The build-only probe must not replace the family signing identity.");
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                // The family build wrapper signs a separate APK with the protected family key.
                PlayerSettings.Android.useCustomKeystore = false;
            }
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));
            var folder = Path.Combine(root, "Builds", platform, "G1-" + PlayerSettings.bundleVersion);
            var output = Path.Combine(folder, artifact);
            if (File.Exists(output) || Directory.Exists(output)) throw new IOException("Build output already exists. Use a fresh build number; do not overwrite evidence.");
            Directory.CreateDirectory(folder);
            using var androidTools = target == BuildTarget.Android ? AndroidFoundationTools.Configure() : null;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
            {
                buildProfile = profile,
                locationPathName = output,
                options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport
            });
            var summary = report.summary;
            if (summary.platform != target) throw new InvalidOperationException("Built platform does not match the requested target.");
            if ((summary.options & BuildOptions.Development) != 0)
                throw new InvalidOperationException("Foundation qualification requires a non-development build.");
            if (target == BuildTarget.StandaloneWindows64 &&
                summary.GetSubtarget<StandaloneBuildSubtarget>() != (server ? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player))
                throw new InvalidOperationException("Built Windows player/server subtarget does not match the request.");
            File.WriteAllText(Path.Combine(folder, "build-summary.json"), JsonUtility.ToJson(new Evidence
            {
                platform = platform, version = PlayerSettings.bundleVersion, unity = Application.unityVersion,
                result = summary.result.ToString(), output = output, utc = DateTime.UtcNow.ToString("O"),
                errors = summary.totalErrors, warnings = summary.totalWarnings,
                development = (summary.options & BuildOptions.Development) != 0,
                profile = profilePath,
                scene = scenePath, dedicatedServer = server,
                signing = target == BuildTarget.Android ? "Default Android debug certificate; build-only probe, not family release" : "Not applicable or platform managed"
            }, true));
            if (summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed; inspect the log and build-summary.json.");
            Debug.Log("LITTLE_WEEPS_BUILD succeeded: " + output);
        }

        public static void ValidateProfile(BuildProfile profile, BuildTarget target, bool server, string scenePath)
        {
            if (profile == null) throw new InvalidOperationException("Missing saved foundation Build Profile.");
            // Unity 6000.3 exposes no public target/settings getters on BuildProfile.
            // Read its verified serialized schema; fail closed if an editor upgrade changes it.
            using var settings = new SerializedObject(profile);
            SerializedProperty Required(string path) => settings.FindProperty(path) ??
                throw new InvalidOperationException("Build Profile schema changed: " + path);
            var expectedSubtarget = target == BuildTarget.StandaloneWindows64 ?
                (int)(server ? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player) : 0;
            if (Required("m_BuildTarget").intValue != (int)target || Required("m_Subtarget").intValue != expectedSubtarget)
                throw new InvalidOperationException("Build Profile target/subtarget mismatch: " + profile.name);
            foreach (var flag in new[] { "m_Development", "m_ConnectProfiler", "m_BuildWithDeepProfilingSupport", "m_AllowDebugging", "m_WaitForManagedDebugger" })
                if (Required("m_PlatformBuildProfile." + flag).boolValue)
                    throw new InvalidOperationException("Foundation profile must disable " + flag);
            if (Required("m_PlayerSettingsYaml.m_Settings").arraySize != 0 || Required("m_HasScriptingDefines").boolValue)
                throw new InvalidOperationException("Foundation profiles must use the shared Player Settings and scripting defines.");
            var scenes = profile.scenes;
            if (!profile.overrideGlobalScenes || scenes.Length != 1 || !scenes[0].enabled || scenes[0].path != scenePath)
                throw new InvalidOperationException("Foundation profile must explicitly contain only " + scenePath);
            if (target == BuildTarget.Android &&
                (Required("m_PlatformBuildProfile.m_ExportAsGoogleAndroidProject").boolValue ||
                 Required("m_PlatformBuildProfile.m_BuildAppBundle").boolValue))
                throw new InvalidOperationException("Android foundation profile must produce an APK.");
        }

        [Serializable] private sealed class Evidence
        { public string platform, version, unity, result, output, utc, profile, scene, signing; public int errors, warnings; public bool development, dedicatedServer; }
    }
}
