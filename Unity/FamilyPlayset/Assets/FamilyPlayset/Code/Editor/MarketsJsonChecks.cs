using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class MarketsJsonChecks
    {
        [Serializable] private sealed class Pack { public Entry[] files; }
        [Serializable] private sealed class Entry { public string path,sha256; }
        public static void Run()
        {
            string packRoot=Path.Combine(Application.dataPath,"FamilyPlayset/Resources/Worlds/Markets");
            var pack=JsonUtility.FromJson<Pack>(File.ReadAllText(Path.Combine(packRoot,"content-receipt.json")));
            if(pack.files==null||pack.files.Length<570||!pack.files.Any(f=>f.path=="Art/pony-apple.png"))throw new Exception("Markets media receipt incomplete");
            foreach(var entry in pack.files){
                string path=Path.GetFullPath(Path.Combine(packRoot,entry.path));
                if(!path.StartsWith(Path.GetFullPath(packRoot)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||!File.Exists(path))throw new Exception("Markets media missing: "+entry.path);
                using(var sha=SHA256.Create())using(var stream=File.OpenRead(path)){
                    if(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant()!=entry.sha256)throw new Exception("Markets media changed: "+entry.path);
                }
            }
            var ponyFood=Resources.Load<Texture2D>("Worlds/Markets/Art/pony-apple");
            if(!ponyFood||ponyFood.width<16||ponyFood.height<16)throw new Exception("Markets held pony food was not imported");
            string[] actors={"one","two","three","four"};
            var world=GameWorld.WithDinosaurWorld(GameWorld.Create(actors));
            var original=world.Snapshot();
            if(original.markets==null || original.markets.members.Length!=4)throw new Exception("Markets migration failed");
            var parsed=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(original));
            GameWorld.Validate(parsed);var restored=GameWorld.Restore(parsed);
            if(restored.ReadMarkets().stations.Length!=8 || restored.ReadMarkets().ponies.Length!=2)throw new Exception("Markets JSON round-trip failed");
            var legacy=GameWorld.Create(actors).Snapshot();var upgraded=GameWorld.WithDinosaurWorld(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(legacy))));
            if(upgraded.ReadMarkets()==null || upgraded.ReadPlayer("one").avatar!=legacy.players[0].avatar)throw new Exception("Markets legacy JSON retention failed");
            var copy=GameWorld.CopySnapshot(original);copy.markets.ponies[0].body=4;
            if(world.ReadMarkets().ponies[0].body==4)throw new Exception("Markets snapshot aliases authority");
            Debug.Log("MARKETS_JSON_CHECKS passed media hashes, legacy retention, native JSON, deep copy and restore");
        }
    }
}
