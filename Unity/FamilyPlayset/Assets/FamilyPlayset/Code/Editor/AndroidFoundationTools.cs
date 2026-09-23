using System;
using System.IO;

namespace LittleWeeps.EditorTools
{
    public static class AndroidFoundationTools
    {
        // Use Unity's public API; restore user preferences after this project's build.
        public static IDisposable Configure()
        {
#if UNITY_ANDROID
            var args = Environment.GetCommandLineArgs();
            string root = null;
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == "-familyAndroidToolchain") root = Path.GetFullPath(args[i + 1]);
            if (root == null) throw new InvalidOperationException("Supply the verified Android toolchain path through Build-Foundation.ps1.");
            if (!File.ReadAllText(Path.Combine(root, "NDK", "source.properties")).Contains("27.2.12479018"))
                throw new InvalidOperationException("This Unity patch requires the verified NDK r27c toolchain.");
            var scope = new RestorePaths();
            try
            {
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = Path.Combine(root, "OpenJDK");
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = Path.Combine(root, "SDK");
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = Path.Combine(root, "NDK");
                return scope;
            }
            catch { scope.Dispose(); throw; }
#else
            throw new InvalidOperationException("Start Unity with -buildTarget Android to configure Android tools.");
#endif
        }

#if UNITY_ANDROID
        // Regression check for the first Android build: bundled tools can be absent.
        public static void VerifyRestoration()
        {
            using var original = new RestorePaths();
            UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = null;
            UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = null;
            UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = null;
            var bundledJdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
            var bundledSdk = UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath;
            var bundledNdk = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath;
            using (Configure()) { }
            RequireSame(bundledJdk, UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
            RequireSame(bundledSdk, UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath);
            RequireSame(bundledNdk, UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath);
            using (Configure())
            {
                var customJdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
                var customSdk = UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath;
                var customNdk = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath;
                using (Configure()) { }
                RequireSame(customJdk, UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
                RequireSame(customSdk, UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath);
                RequireSame(customNdk, UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath);
            }
            UnityEngine.Debug.Log("LITTLE_WEEPS_ANDROID_TOOLS_PASS: bundled defaults and custom paths restored.");
        }

        private static void RequireSame(string expected, string actual)
        {
            if (!string.Equals(Path.GetFullPath(expected), Path.GetFullPath(actual), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Android External Tools restoration mismatch.");
        }

        private sealed class RestorePaths : IDisposable
        {
            private readonly string jdk = RestoreValue(UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath, "OpenJDK");
            private readonly string sdk = RestoreValue(UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath, "SDK");
            private readonly string ndk = RestoreValue(UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath, "NDK");

            private static string RestoreValue(string path, string folder)
            {
                var bundled = Path.Combine(UnityEditor.EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer", folder);
                // The getter resolves the bundled path even when it doesn't exist.
                // Unity documents null as the setter for "Installed with Unity".
                return string.IsNullOrEmpty(path) || string.Equals(Path.GetFullPath(path), Path.GetFullPath(bundled), StringComparison.OrdinalIgnoreCase)
                    ? null : path;
            }
            public void Dispose()
            {
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = jdk;
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = sdk;
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = ndk;
            }
        }
#endif
    }
}
