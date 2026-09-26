using System;
using System.Collections.Generic;
using System.IO;
using LittleWeeps.Client;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.NetworkProbe
{
    // Invoked only by the isolated release-player verification channel. Uses
    // the real character adapter/art at several render rates, not a mock rig.
    public static class WalkAnimationVerification
    {
        [Serializable] private sealed class Result
        {public bool passed;public string build;public List<Trial> trials=new List<Trial>();public string[] checks;}
        [Serializable] private sealed class Trial
        {public string character;public int fps,plantedPairs;public bool faceLeft;public float phase,maxStanceDrift,footLift;}
        public static void Run(Transform parent,string output)
        {
            var result=new Result{build=Application.version};
            void Check(bool yes,string message){if(!yes)throw new InvalidOperationException("Walk check: "+message);}
            foreach(var id in new[]{"blue-pup","orange-pup"})foreach(var left in new[]{false,true})
            {
                float expected=-1;
                foreach(var fps in new[]{30,60,120})
                {
                    var go=new GameObject("Isolated walk check",typeof(RectTransform));
                    go.transform.SetParent(parent,false);
                    // Keep the fixture out of the visible play area; world state
                    // and the actual player character are never moved by it.
                    var rect=(RectTransform)go.transform;rect.anchoredPosition=new Vector2(10000,0);
                    try
                    {
                        var adapter=go.AddComponent<GameCharacterVisual>();adapter.Select(id);
                        var view=adapter.ActiveView;
                        Check(2*Core.Walking.Speed/CharacterWalk.Stride<=3.6f,"walk cadence became a frantic run");
                        var step=1f/fps;var point=Vector2.zero;var direction=left?-1:1;
                        adapter.Present(point,"walk-check",false,step);
                        var priorFar=view.FarContact;var priorNear=view.NearContact;var previousPhase=0f;
                        var trial=new Trial{character=view.characterId,fps=fps,faceLeft=left};
                        var floorY=priorFar.y;
                        for(var i=0;i<fps*2;i++)
                        {
                            point.x+=direction*Core.Walking.Speed*step;rect.anchoredPosition=new Vector2(10000+point.x,0);
                            adapter.Present(point,"walk-check",false,step);
                            view=adapter.ActiveView;
                            Check(adapter.ProfileVisible,"horizontal walking kept the frontal drawing");
                            Check(go.GetComponentsInChildren<CharacterView>().Length==1,"two directional drawings are visible");
                            var far=view.FarContact;var near=view.NearContact;var phase=view.WalkPhase;
                            if(i>fps/3 && phase>previousPhase && phase<.49f && previousPhase>.01f)
                            {trial.maxStanceDrift=Mathf.Max(trial.maxStanceDrift,Vector3.Distance(priorFar,far));trial.plantedPairs++;}
                            if(i>fps/3 && phase>previousPhase && phase>.51f && previousPhase>.51f)
                            {trial.maxStanceDrift=Mathf.Max(trial.maxStanceDrift,Vector3.Distance(priorNear,near));trial.plantedPairs++;}
                            trial.footLift=Mathf.Max(trial.footLift,far.y-floorY,near.y-floorY);
                            priorFar=far;priorNear=near;previousPhase=phase;
                        }
                        trial.phase=view.WalkPhase;
                        var noseDirection=view.facing.TransformVector(Vector3.left).x;
                        Check(left?noseDirection<0:noseDirection>0,"artwork faces away from travel");
                        // Parent screen scale is allowed; the test drift is in
                        // native canvas world coordinates with identical roots.
                        Check(trial.plantedPairs>20 && trial.maxStanceDrift<.02f,"stance foot slid");
                        Check(trial.footLift>3,"no visible foot clearance");
                        if(expected<0)expected=trial.phase;
                        Check(Mathf.Abs(Mathf.DeltaAngle(expected*360,trial.phase*360))<.08f,"render rate changed gait phase");
                        var root=rect.anchoredPosition;
                        for(var i=0;i<fps/3;i++)adapter.Present(point,"walk-check",true,step);
                        view=adapter.ActiveView;
                        Check(view.WalkWeight==0 && rect.anchoredPosition==root,"stop moved root or failed to settle");
                        Check(!adapter.ProfileVisible,"idle failed to return to frontal art");
                        point.x+=1500;adapter.Present(point,"walk-check",false,step);
                        Check(view.WalkPhase==0 && view.WalkWeight==0,"teleport produced a giant step");
                        adapter.Present(point,"new-room",false,step);
                        Check(view.WalkWeight==0,"travel retained old stance");
                        result.trials.Add(trial);
                    }
                    finally{go.SetActive(false);UnityEngine.Object.Destroy(go);}
                }
            }
            var motion=new CharacterMotion();motion.Observe(Vector2.zero,"stable",false,false,.02f);
            motion.Observe(new Vector2(-4,0),"stable",false,false,.02f);
            for(var i=0;i<12;i++)Check(motion.Observe(new Vector2(-4+(i%2)*.03f,0),"stable",false,false,.02f).FaceLeft,"small correction flipped facing");
            Check(!motion.Observe(new Vector2(0,0),"stable",false,false,.02f).FaceLeft,"intentional reversal ignored");
            result.passed=true;
            result.checks=new[]{"Bluey/Bingo left/right profile artwork with one visible rig","30/60/120 FPS equal-distance phase and planted-foot contact","stop/carry preserves gameplay root and restores frontal idle","travel/teleport clears gait","tiny correction facing hysteresis and deliberate reversal"};
            File.WriteAllText(Path.Combine(output,"walk-checks.json"),JsonUtility.ToJson(result,true));
        }
    }
}
