using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace LittleWeeps.Client
{
    public static class RecoveredWorldContent
    {
        private static readonly Dictionary<string,string> roots=new Dictionary<string,string>();
        public static string Root(string world)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // StreamingAssets lives inside the APK on Android. The existing
            // art readers need ordinary files, so unpack only this signed pack
            // into a versioned cache, leaving saves and enrollment untouched.
            if(roots.TryGetValue(world,out var cached))return cached;
            if(world!="HulkBirthday" && world!="SpideyHQ" && world!="PeppaFunfair")throw new InvalidDataException("Unknown recovered world.");
            var root=Path.Combine(Application.persistentDataPath,"RecoveredContent",Application.version,world);
            var ready=Path.Combine(root,".complete");
            if(!File.Exists(ready))
            {
                Directory.CreateDirectory(root);
                using var input=File.OpenRead(Application.dataPath);
                using var archive=new ZipArchive(input,ZipArchiveMode.Read);
                var prefix="assets/"+world+"/";int files=0;
                foreach(var entry in archive.Entries)
                {
                    if(!entry.FullName.StartsWith(prefix,StringComparison.Ordinal) || entry.FullName.EndsWith("/",StringComparison.Ordinal))continue;
                    var path=Path.GetFullPath(Path.Combine(root,entry.FullName.Substring(prefix.Length)));
                    if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.Ordinal))throw new InvalidDataException("Invalid recovered content path.");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using(var source=entry.Open())using(var destination=File.Create(path))source.CopyTo(destination);
                    files++;
                }
                if(files==0)return root;
                if(!File.Exists(Path.Combine(root,"actor-frames.json")))throw new InvalidDataException("Incomplete recovered world: "+world);
                File.WriteAllText(ready,files.ToString());
                Debug.Log("Recovered mobile world ready: "+world+", "+files+" files.");
            }
            roots[world]=root;return root;
#else
            return Path.Combine(Application.streamingAssetsPath,world);
#endif
        }
    }
}
