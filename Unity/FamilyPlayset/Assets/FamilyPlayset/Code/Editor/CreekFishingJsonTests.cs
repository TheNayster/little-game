using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class CreekFishingJsonTests
    {
        public static void Run()
        {
            var old=SoloWorld.WithCreekBoats(SoloWorld.Create("one","two","three","four"));
            var legacy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));
            var w=SoloWorld.WithCreekFishing(SoloWorld.Restore(legacy));var p=w.ReadPlayer("one");
            if(!w.Apply(new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=SoloAction.CreekFishing,value="start-feeding"}).Accepted)throw new InvalidOperationException("Native creek feeding failed.");
            var saved=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(w.Snapshot()));SoloWorld.Validate(saved);
            if(saved.creekFishing.fish.Length!=20 || saved.creekFishing.food.Length!=1 || saved.pond.fish.Length!=8 || saved.creekBoats.boats.Length!=4)throw new InvalidOperationException("Unity JSON lost distinct habitats.");
            var reopened=SoloWorld.Restore(saved);
            if(reopened.WorldId!=old.WorldId || reopened.ReadCreekFishing().rods.Any(r=>r.mode!=PondMode.None) || reopened.ReadCreekFishing().food.Length!=0)throw new InvalidOperationException("Creek recovery retained temporary participation.");
            Debug.Log("Creek fishing JSON passed: legacy upgrade, two habitats, boats and lease recovery.");
        }
    }
}
