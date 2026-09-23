using System;
using System.IO;
using UnityEngine;

namespace LittleWeeps.Runtime
{
    // G1-only verification namespace. Automated checks never reset family data.
    public static class FoundationRun
    {
        public static readonly string Mode;
        public static readonly string RunId;
        public static bool Automated => RunId != null;
        public static string EvidenceDirectory => Path.Combine(Application.persistentDataPath, "Verification", RunId);
        static FoundationRun()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-foundationVerify") Mode = args[i + 1];
                if (args[i] == "-foundationRun" && Guid.TryParse(args[i + 1], out var id)) RunId = id.ToString("N");
            }
            if (Mode != "seed" && Mode != "resume" && Mode != "update") { Mode = null; RunId = null; }
            if (Mode == null) RunId = null;
#endif
        }
        public static string Key(string suffix) => Automated
            ? "foundation.verify." + RunId + "." + suffix : "foundation." + suffix;
    }
}
