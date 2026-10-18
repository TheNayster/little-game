using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    // Include the same checked local packs as the PC release without importing
    // recovered media into Assets or committing it to the source repository.
    public sealed class RecoveredMobileContent : BuildPlayerProcessor
    {
        [Serializable] private sealed class Catalog { public Pack[] packs; }
        [Serializable] private sealed class Pack { public string world,source,receipt,receiptSha256; }
        [Serializable] private sealed class Entry { public string path,sha256; }
        [Serializable] private sealed class Entries { public Entry[] files; }
        private static string Hash(string path)
        {
            using var sha=SHA256.Create();using var input=File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
        }
        public override void PrepareForBuild(BuildPlayerContext context)
        {
            var target=context.BuildPlayerOptions.target;
            if(target!=BuildTarget.Android && target!=BuildTarget.iOS)return;
            if(PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target)))!="com.littleweeps.familyplayset")return;
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
            var catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(root,"Tools/Build/recovered-mobile-packs.json")));
            if(catalog?.packs==null || catalog.packs.Length!=3)throw new BuildFailedException("Three recovered world packs are required.");
            foreach(var pack in catalog.packs)
            {
                var source=Path.GetFullPath(Path.Combine(root,pack.source));
                var receipt=Path.GetFullPath(Path.Combine(root,pack.receipt));
                if(Hash(receipt)!=pack.receiptSha256)throw new BuildFailedException("Recovered content receipt changed: "+pack.world);
                var entries=JsonUtility.FromJson<Entries>("{\"files\":"+File.ReadAllText(receipt)+"}").files;
                foreach(var entry in entries)
                {
                    var file=Path.GetFullPath(Path.Combine(source,entry.path));
                    if(!file.StartsWith(source+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase) || Hash(file)!=entry.sha256)
                        throw new BuildFailedException("Recovered content changed: "+pack.world+"/"+entry.path);
                }
                context.AddAdditionalPathToStreamingAssets(source,pack.world);
                Debug.Log("Packaged recovered mobile world "+pack.world+": "+entries.Length+" verified files.");
            }
        }
    }
}
