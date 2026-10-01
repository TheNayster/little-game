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
            var legacy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));legacy.schema=28;legacy.hideAndSeek.count=20;legacy.hideAndSeek.phase=HidePhase.Counting;
            legacy.hideAndSeek.hiders[0].mode=HiderMode.Preparing;legacy.hideAndSeek.hiders[0].preparation=20;
            SoloWorld.Validate(legacy);var migrated=SoloWorld.WithHideAndSeek(SoloWorld.Restore(legacy));
            if(migrated.Schema!=HideAndSeek.HidingWindowSchema || migrated.ReadHideAndSeek().phase!=HidePhase.Idle)throw new InvalidOperationException("Legacy hiding migration failed.");
            for(var i=0;i<HideAndSeek.SlotX.Length;i++)
            {
                s.hideAndSeek.phase=HidePhase.Inspecting;s.hideAndSeek.target=i;s.hideAndSeek.age=1.4999999999999;s.hideAndSeek.count=9.999999999;
                s.hideAndSeek.hiders[0].mode=HiderMode.Hidden;s.hideAndSeek.hiders[0].slot=i;
                s.players[0].x=HideAndSeek.SlotX[i];s.players[0].y=HideAndSeek.HiddenY(i);
                var copy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));SoloWorld.Validate(copy);
                var restored=SoloWorld.Restore(copy);if(restored.ReadHideAndSeek().phase!=HidePhase.Idle || restored.ReadPlayer("one").y!=50)throw new InvalidOperationException("Hiding restore failed.");
            }
            // Shared cover occupancy adds no fields; old unique-occupant saves
            // and new four-occupant snapshots use the same JSON shape.
            s.hideAndSeek.phase=HidePhase.Counting;s.hideAndSeek.count=10;s.hideAndSeek.target=-1;
            for(var i=0;i<4;i++){s.hideAndSeek.hiders[i].mode=HiderMode.Hidden;s.hideAndSeek.hiders[i].slot=5;s.players[i].x=HideAndSeek.SlotX[5];s.players[i].y=HideAndSeek.HiddenY(5);}
            var sharing=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s));SoloWorld.Validate(sharing);
            if(sharing.hideAndSeek.hiders.Length!=4 || sharing.hideAndSeek.hiders[3].slot!=5)throw new InvalidOperationException("Shared cover JSON lost occupants.");
            var reopened=SoloWorld.Restore(sharing);SoloWorld.Validate(reopened.Snapshot());
            foreach(var p in reopened.ReadPlayers())if(p.y!=50)throw new InvalidOperationException("Shared-cover reopen left stale hidden placement.");
            Debug.Log("Hide-and-seek Unity JSON: old schema and legacy 20-second state plus ten hidden-slot round trips passed.");
        }
    }
}
