using System;
using System.Collections.Generic;
using System.IO;
using LittleWeeps.Client;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.NetworkProbe
{
    // Test the actual sheet player, not obsolete hidden procedural limbs.
    public static class WalkAnimationVerification
    {
        [Serializable] private sealed class Result
        { public bool passed; public string build; public List<Trial> trials=new List<Trial>(); public string[] checks; }
        [Serializable] private sealed class Trial
        { public string character,sourceHash,walkSourceHash; public int fps,distinctWalkFrames; public bool faceLeft; public float phase; }
        public static void Run(Transform parent,string output)
        {
            var result=new Result{build=Application.version};
            void Check(bool yes,string message){if(!yes)throw new InvalidOperationException("Sheet walk check: "+message);}
            foreach(var id in new[]{"blue-pup","orange-pup"})foreach(var left in new[]{false,true})
            {
                float expected=-1;
                foreach(var fps in new[]{30,60,120})
                {
                    var go=new GameObject("Isolated sheet check",typeof(RectTransform));go.transform.SetParent(parent,false);
                    var rect=(RectTransform)go.transform;rect.anchoredPosition=new Vector2(10000,0);
                    try
                    {
                        var adapter=go.AddComponent<GameCharacterVisual>();adapter.Select(id);
                        var view=adapter.ActiveView;
                        Check(view.FrameCount==16 && view.SourceHash.Length==64,"missing prepared artwork");
                        Check(go.GetComponentsInChildren<CharacterView>().Length==0,"legacy redraw still visible");
                        Check(go.GetComponentsInChildren<RawImage>().Length==1,"sheet drawing absent or duplicated");
                        var step=1f/fps;var point=Vector2.zero;var direction=left?-1:1;var seen=new HashSet<int>();
                        adapter.Present(point,"sheet-check",false,step);
                        for(var i=0;i<fps*2;i++)
                        {
                            point.x+=direction*Core.Walking.Speed*step;rect.anchoredPosition=new Vector2(10000+point.x,0);
                            var root=rect.anchoredPosition;adapter.Present(point,"sheet-check",false,step);
                            Check(rect.anchoredPosition==root,"animation changed player coordinates");
                            Check(view.FrameIndex>=4 && view.FrameIndex<12,"wrong walk drawing");seen.Add(view.FrameIndex);
                            Check(view.IsWalkDrawing && view.WalkSourceHash.Length==64,"old exaggerated walk is still active");
                            Check(left?view.FacingSign<0:view.FacingSign>0,"art faces away from travel");
                        }
                        var trial=new Trial{character=view.CharacterId,sourceHash=view.SourceHash,walkSourceHash=view.WalkSourceHash,fps=fps,faceLeft=left,
                            phase=view.WalkPhase,distinctWalkFrames=seen.Count};
                        Check(seen.Count==8,"walk omitted authored frames");
                        if(expected<0)expected=trial.phase;
                        Check(Mathf.Abs(Mathf.DeltaAngle(expected*360,trial.phase*360))<.08f,"render rate changed gait phase");
                        adapter.Present(point,"sheet-check",true,step);
                        Check(view.FrameIndex==13 && view.WalkWeight==0,"stopped carry lost its pose");
                        Check(!view.IsWalkDrawing,"carry did not restore original action artwork");
                        adapter.Present(point,"sheet-check",false,step);
                        Check(view.FrameIndex<2,"stop failed to return to idle");
                        point.x+=1500;adapter.Present(point,"sheet-check",false,step);
                        Check(view.WalkPhase==0 && view.WalkWeight==0,"teleport produced a false walk");
                        foreach(var pose in new[]{CharacterPose.Sit,CharacterPose.Bounce,CharacterPose.Dance,CharacterPose.Wave})
                        {
                            adapter.PresentFrame(new CharacterFrame(pose,0,left,.7f),step);
                            var correct=pose==CharacterPose.Sit?view.FrameIndex==12:pose==CharacterPose.Dance?view.FrameIndex>=14:
                                pose==CharacterPose.Wave?view.FrameIndex==2 || view.FrameIndex==3:view.FrameIndex==0 || view.FrameIndex==2;
                            Check(correct,"home action lost selected sheet artwork");
                            Check(!view.IsWalkDrawing,"home action incorrectly uses walk atlas");
                        }
                        var savedRoot=rect.anchoredPosition;
                        adapter.Select(id=="blue-pup"?"orange-pup":"blue-pup");
                        Check(rect.anchoredPosition==savedRoot && adapter.Frame.Pose==CharacterPose.Wave,"switch lost current action or root");
                        Check(go.GetComponentsInChildren<RawImage>().Length==1,"switch rendered both characters");
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
            result.checks=new[]{"selected Bluey/Bingo sheets render with no legacy redraw","all eight walk frames in both directions at 30/60/120 FPS",
                "equal distance retains phase and animation never changes player root","idle/carry/sit/bounce/dance/wave use prepared frames",
                "switch preserves action, coordinates and one visible drawing","teleport reset and facing hysteresis",
                "relaxed walk atlas only while moving, original home poses retained"};
            File.WriteAllText(Path.Combine(output,"walk-checks.json"),JsonUtility.ToJson(result,true));
        }
    }
}
