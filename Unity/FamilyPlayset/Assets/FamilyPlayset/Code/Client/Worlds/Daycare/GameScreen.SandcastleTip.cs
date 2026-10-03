using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private SandTipFeedback sandTipFeedback=new SandTipFeedback();
        private readonly List<SandShape> sandTipEffects=new List<SandShape>();
        public int[] SandpitTipEvents=>sandTipFeedback.Events;
        public float[] SandpitTipEffects=>Enumerable.Range(0,DaycareSandpit.MaxPieces).Select(sandTipFeedback.Remaining).ToArray();
        public string[] SandpitTipOutcomes=>Enumerable.Range(0,DaycareSandpit.MaxPieces).Select(sandTipFeedback.Outcome).ToArray();
        public bool[] SandpitBuiltVisuals=>sandShapes.Select(s=>s.built).ToArray();
        public float[] SandpitTowerReveal=>sandShapes.Select(s=>s.towerReveal).ToArray();
        private void TouchSandMould(int i)
        {if(sandPlacing)return;if(i>=SandpitGame.moulds.Length)return;if(SandpitOwn && i==sandpitSelected && !SandpitGame.moulds[i].built)RequestSandTool("tip");else SelectSandMould(i);}
        private void BuildSandTip()
        {
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var effect=Rect(sandFloor,"Confirmed mould tip "+(i+1),Vector2.zero,new Vector2(360,440)).gameObject.AddComponent<SandShape>();
                effect.kind="mould-tip";effect.raycastTarget=false;sandTipEffects.Add(effect);
            }
        }
        private void TickSandTip(SandpitState game,bool joined)
        {
            sandTipFeedback.Observe(game,joined && !applicationPaused,Time.unscaledDeltaTime);
            for(var i=0;i<sandTipEffects.Count;i++){
                var effect=sandTipEffects[i];var result=sandTipFeedback.Outcome(i);var active=joined && i<game.moulds.Length && sandTipFeedback.Remaining(i)>0;
                var cue=result=="underfilled" || result=="dry";effect.gameObject.SetActive(active && !cue);
                if(i>=game.moulds.Length)continue;
                effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one;
                effect.mould=game.moulds[i].shape;effect.orientation=game.moulds[i].orientation;effect.capacity=game.moulds[i].capacity;effect.tipOutcome=result;effect.progress=1-sandTipFeedback.Remaining(i)/(cue?SandTipFeedback.WiggleDuration:SandTipFeedback.Duration);effect.SetVerticesDirty();
                sandShapes[i].towerReveal=active && result=="reveal"?Mathf.Clamp01((effect.progress-.52f)/.3f):1;
                sandShapes[i].hideBucket=active && !cue;
                sandShapes[i].wiggle=active && cue?Mathf.Sin(effect.progress*Mathf.PI*4)*5*(1-effect.progress):0;

                sandShapes[i].SetVerticesDirty();
            }
        }
        private void ResetSandTip(){sandTipEffects.Clear();sandTipFeedback=new SandTipFeedback();}
    }
}
