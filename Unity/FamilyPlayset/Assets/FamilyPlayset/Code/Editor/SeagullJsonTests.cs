using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class SeagullJsonTests
    {
        public static void Run()
        {
            var old=GameWorld.WithOutfits(GameWorld.Create("one","two","three","four"));
            GameWorld.Validate(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot())));
            var w=GameWorld.WithSeagulls(old);var s=w.Snapshot();
            foreach(GullPhase phase in Enum.GetValues(typeof(GullPhase))){
                s.seagulls.phase=phase;s.seagulls.from=0;s.seagulls.to=phase==GullPhase.Resting?0:2;s.seagulls.age=phase==GullPhase.Resting?0:BeachSeagulls.Duration(phase)-.001;
                var restored=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));
                if(restored.ReadSeagulls().phase!=phase || restored.ReadToys().Length!=old.ReadToys().Length)throw new InvalidOperationException("Seagull Unity JSON retention failed.");
            }
            Debug.Log("Seagull Unity JSON: legacy migration and all five phases retain objects and flock.");
        }
    }
}
