using System;
using UnityEngine;
using LittleWeeps.Client;
using LittleWeeps.Core;
namespace LittleWeeps.EditorTools
{
    public static class NpcPresentationTests
    {
        private static void Check(bool ok,string reason){if(!ok)throw new InvalidOperationException("NPC: "+reason);}
        public static void Run()
        {
            // Uneven authority samples must not stop/start rendering each tick.
            var clock=new NpcPresentationClock();double authority=0,prior=0;
            for(var i=0;i<240;i++)
            {
                var now=i/60d;if(i%6==0)authority=now;
                var shown=clock.Step(authority,"round1",true,now,1/60f);
                if(i>1)Check(shown>=prior && shown-prior<=1.101/60,"packet-time jump");prior=shown;
            }
            Check(clock.Step(authority-.15,"round1",true,4,0)>=prior,"stale packet reversed motion");
            var stopped=clock.Step(authority,"round1",true,10,.1f);
            for(var i=0;i<60;i++)stopped=clock.Step(authority,"round1",true,10+i*.1,.1f);
            Check(stopped<=authority+.25001,"unbounded prediction after loss");
            Check(30-clock.Step(30,"round1",true,40,1/60f)<.3,"reconnect replays stale route time");
            Check(clock.Step(0,"round2",true,20,0)==0,"replay continuity");
            var motion=new NpcLocomotion();motion.Step(Vector2.zero,"test",false,180,1/60f);
            for(var i=0;i<120;i++){var f=motion.Step(new Vector2(300,0),"test",false,180,1/60f);Check(f.Speed<=180.01,"travel exceeds NPC speed");}
            for(var i=0;i<120;i++)motion.Step(new Vector2(300,0),"test",false,180,1/60f);
            for(var i=0;i<30;i++)Check(motion.Step(new Vector2(300,0),"test",false,180,1/60f).Pose==CharacterPose.Idle,"arrival flicker");
            // Exercise every prepared character, including Jean-Luc from the
            // user's screenshot, with the real sprite presenter at NPC speed.
            var count=0;
            foreach(var entry in PlayableCharacters.All)
            {
                var art=LittleWeeps.Client.WorldResources.Load<CharacterArt>("Shared/Characters/Art/"+entry.ArtId);
                var root=new GameObject("NPC gait check",typeof(RectTransform));var view=root.AddComponent<CharacterSheetView>();view.Configure(art,null);
                Check(view.EffectiveWalkStride==CharacterSheetView.WalkStride,"player cadence changed");view.NpcWorldScale=.8f;
                var drawings=new System.Collections.Generic.HashSet<int>();var longest=0;var hold=0;var last=-1;
                for(var i=0;i<240;i++)
                {
                    view.Present(new CharacterFrame(CharacterPose.Walk,40,false,travel:new Vector2(40/60f,0)),1/60f);
                    // Model sheets deliberately duplicate each of four poses.
                    var index=art.walkFrames[view.FrameIndex-4].pixels.Value;
                    var id=index.GetHashCode();drawings.Add(id);hold=id==last?hold+1:1;last=id;longest=Math.Max(longest,hold);
                }
                Check(drawings.Count>=4,"stalled walking frames for "+entry.ArtId);
                Check(longest/60f<.7f,"walking pose held too long for "+entry.ArtId);
                UnityEngine.Object.DestroyImmediate(root);count++;
            }
            var teacherRoot=new GameObject("Teacher gait check",typeof(RectTransform));
            var raw=new GameObject("Teacher picture",typeof(RectTransform),typeof(UnityEngine.UI.RawImage)).GetComponent<UnityEngine.UI.RawImage>();raw.transform.SetParent(teacherRoot.transform,false);
            var teacher=teacherRoot.AddComponent<TeacherWalkView>();teacher.Configure(raw,LittleWeeps.Client.WorldResources.Load<Texture2D>("Worlds/Daycare/Shared/calypso-poses"));
            var frames=new System.Collections.Generic.HashSet<int>();
            for(var i=0;i<120;i++){teacher.Present(new CharacterFrame(CharacterPose.Walk,180,true,travel:new Vector2(-3,0)),2,1/60f,1);Check(teacher.Walking && teacher.Floor==Vector2.zero,"teacher glides or floor shifts");frames.Add(teacher.Drawing);}
            Check(frames.Count==8,"teacher missing walk steps");teacher.Present(new CharacterFrame(CharacterPose.Idle,0,true),2,1/60f,1);Check(!teacher.Walking && teacher.Drawing==2,"teacher does not resume reading");UnityEngine.Object.DestroyImmediate(teacherRoot);
            Debug.Log("NPC_PRESENTATION_PASS: jitter/loss/replay, bounded movement and settled arrival, "+count+" character gaits; player stride unchanged.");
        }
    }
}
