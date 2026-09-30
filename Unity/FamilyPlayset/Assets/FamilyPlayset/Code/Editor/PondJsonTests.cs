using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class PondJsonTests
    {
        public static void Run()
        {
            // Clear a cached automatic cubemap import before validating the new panorama.
            UnityEditor.AssetDatabase.ImportAsset("Assets/FamilyPlayset/Resources/PondArt/garden-tree-clean.png",UnityEditor.ImportAssetOptions.ForceUpdate);
            if(Resources.Load<Texture2D>("PondArt/garden-tree-clean")==null)throw new InvalidOperationException("Clean garden texture is not imported as 2D.");
            var old=SoloWorld.WithOutfits(SoloWorld.Create("one","two","three","four"));
            var legacy=JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old.Snapshot()));SoloWorld.Validate(legacy);
            var w=SoloWorld.WithPond(SoloWorld.Restore(legacy));var p=w.ReadPlayer("one");
            var result=w.Apply(new SoloCommand{actor=p.id,requestId=Guid.NewGuid().ToString("N"),zone=p.zone,visit=p.visit,expectedRevision=w.Revision,action=SoloAction.Pond,value="start-feeding",x=PondFishing.X,y=100});
            if(!result.Accepted)throw new InvalidOperationException("Native pond start failed.");
            var json=JsonUtility.ToJson(w.Snapshot());var s=JsonUtility.FromJson<SoloSnapshot>(json);SoloWorld.Validate(s);
            if(s.pond.rods[0].mode!=PondMode.Feeding || s.pond.food.Length!=1 || s.pond.fish.Length!=8)throw new InvalidOperationException("Unity JSON lost pond records.");
            var restored=SoloWorld.Restore(s).ReadPond();
            if(restored.rods[0].mode!=PondMode.None || restored.food.Length!=0 || restored.fish.Length!=8)throw new InvalidOperationException("Native pond recovery retained a temporary lease.");
            Debug.Log("Pond JSON checks passed: old-save upgrade, shared fish/food fields and temporary lease recovery.");
        }
    }
}
