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
        private sealed class RestorePaths : IDisposable
        {
            private readonly string jdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
            private readonly string sdk = UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath;
            private readonly string ndk = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath;
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
