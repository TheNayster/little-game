using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class WaveRideJsonTests
    {
        public static void Run()
        {
            var legacy=GameWorld.WithShore(GameWorld.Create("one","two","three","four"));var old=legacy.Snapshot();
            var migrated=GameWorld.WithWaveRides(GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(old))));
            var s=migrated.Snapshot();s.players[0].zone="beach";s.shore.ride.phase=WaveRidePhase.Riding;s.shore.ride.round=1;s.shore.ride.age=8;s.shore.ride.splashes=1;
            s.shore.ride.riders=new[]{new WaveRider{actor="one",visitor=SeaVisitor.Mermaid,seat=2,returnX=1300,returnY=230,waves=1}};
            var w=GameWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));var g=w.ReadShore();
            if(g.ride.riders[0].visitor!=SeaVisitor.Mermaid || g.ride.riders[0].seat!=2 || g.ride.age!=8 || g.ride.splashes!=1 || g.random!=old.shore.random || w.ReadToys().Length!=legacy.ReadToys().Length)throw new InvalidOperationException("Wave ride Unity JSON retention failed.");
            Debug.Log("Wave ride Unity JSON: old shoreline save migration and shared ride/seat/timing/reward retention pass.");
        }
    }
}
