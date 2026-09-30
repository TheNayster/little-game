using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class BathroomJsonTests
    {
        public static void Run()
        {
            UnityEditor.AssetDatabase.ImportAsset("Assets/FamilyPlayset/Resources/Scenery/home-bathroom.png",UnityEditor.ImportAssetOptions.ForceUpdate);
            UnityEditor.AssetDatabase.ImportAsset("Assets/FamilyPlayset/Resources/BathroomArt/hallway.png",UnityEditor.ImportAssetOptions.ForceUpdate);
            if(Resources.Load<Texture2D>("Scenery/home-bathroom")==null || Resources.Load<Texture2D>("BathroomArt/hallway")==null)throw new InvalidOperationException("Bathroom art must import as 2D.");
            var old=SoloWorld.WithPond(SoloWorld.Create("one","two","three","four"));
            // The integration checkout may include a newer additive migration;
            // exercise its native JSON without requiring that separate feature.
            var newer=typeof(SoloWorld).GetMethod("WithCreekFishing");if(newer!=null)old=(SoloWorld)newer.Invoke(null,new object[]{old});
            var legacy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));SoloWorld.Validate(legacy);
            var w=SoloWorld.WithBathroom(SoloWorld.Restore(legacy));var p=w.ReadPlayer("one");
            if(w.Snapshot().worldId!=legacy.worldId || w.Snapshot().toys.Length!=legacy.toys.Length)throw new InvalidOperationException("Bathroom migration lost Home state.");
            var s=w.Snapshot();s.players[0].zone=BathroomLayout.Area;s.players[0].x=950;s.players[0].y=BathroomLayout.Y;s.players[0].fixture="bath-0";
            var saved=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));SoloWorld.Validate(saved);
            var restored=SoloWorld.Restore(saved);if(restored.ReadPlayer("one").zone!=BathroomLayout.Area || restored.ReadPlayer("one").fixture!="" || saved.players[0].fixture!="bath-0")throw new InvalidOperationException("Bathroom restore changed source or retained lease.");
            Debug.Log("Bathroom JSON passed: additive room migration, old Home retention and independent temporary lease recovery.");
        }
    }
}
