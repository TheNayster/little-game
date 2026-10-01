using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools
{
    public static class ShoreJsonTests
    {
        public static void Run()
        {
            var legacy=SoloWorld.WithSeagulls(SoloWorld.Create("one","two","three","four"));
            var reopened=SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(legacy.Snapshot())));
            var w=SoloWorld.WithShore(reopened);var s=w.Snapshot();
            s.shore.clock=10;s.shore.nextVisit=35;s.shore.visitStarted=8;s.shore.visits=1;s.shore.visitX=1200;
            s.shore.prints=new[]{new SandPrint{actor="one",x=1000,y=200,born=9,washed=-1},new SandPrint{actor="two",x=1200,y=480,born=9,washed=9.9}};
            s.shore.ripples=new[]{new ShoreRipple{actor="three",x=1200,y=480,born=9}};
            foreach(SeaVisitor type in Enum.GetValues(typeof(SeaVisitor))){
                s.shore.visitor=type;var restored=SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(JsonUtility.ToJson(s)));var g=restored.ReadShore();
                if(g.visitor!=type || g.random!=s.shore.random || g.nextVisit!=35 || g.prints.Length!=2 || g.prints[0].washed!=-1 || g.ripples.Length!=1 || restored.ReadToys().Length!=legacy.ReadToys().Length)throw new InvalidOperationException("Shore Unity JSON retention failed.");
            }
            Debug.Log("Beach shore Unity JSON: additive migration, RNG, all three sightings, prints/ripples and previous objects retained.");
        }
    }
}
