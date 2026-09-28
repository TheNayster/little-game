using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class HideAndSeekJsonTests
    {
        public static void Run()
        {
            var old=SoloWorld.WithMarbleRamps(SoloWorld.Create("one","two","three","four"));
            SoloWorld.Validate(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot())));
            var w=SoloWorld.WithHideAndSeek(old);var s=w.Snapshot();
            for(var i=0;i<6;i++)
            {
                s.hideAndSeek.phase=HidePhase.Inspecting;s.hideAndSeek.target=i;s.hideAndSeek.age=1.4999999999999;s.hideAndSeek.count=19.999999999;
                s.hideAndSeek.hiders[0].mode=HiderMode.Hidden;s.hideAndSeek.hiders[0].slot=i;
                s.players[0].x=HideAndSeek.SlotX[i];s.players[0].y=320;
                var copy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));SoloWorld.Validate(copy);
                var restored=SoloWorld.Restore(copy);if(restored.ReadHideAndSeek().phase!=HidePhase.Idle || restored.ReadPlayer("one").y!=50)throw new InvalidOperationException("Hiding restore failed.");
            }
            Debug.Log("Hide-and-seek Unity JSON: old schema plus six hidden-slot round trips passed.");
        }
    }
}
