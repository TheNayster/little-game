using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private SandTipFeedback sandTipFeedback=new SandTipFeedback();
        private readonly List<SandShape> sandTipEffects=new List<SandShape>();
        public int[] SandpitTipEvents=>sandTipFeedback.Events;
        public float[] SandpitTipEffects=>Enumerable.Range(0,4).Select(sandTipFeedback.Remaining).ToArray();
        public string[] SandpitTipOutcomes=>Enumerable.Range(0,4).Select(sandTipFeedback.Outcome).ToArray();
        public bool[] SandpitBuiltVisuals=>sandShapes.Select(s=>s.built).ToArray();
        public float[] SandpitTowerReveal=>sandShapes.Select(s=>s.towerReveal).ToArray();
        private void TouchSandMould(int i)
        {if(SandpitOwn && i==sandpitSelected && !SandpitGame.moulds[i].built)RequestSandTool("tip");else SelectSandMould(i);}
        private void BuildSandTip()
        {
            for(var i=0;i<4;i++){
                var effect=Rect(Board,"Confirmed mould tip "+(i+1),Vector2.zero,new Vector2(360,440)).gameObject.AddComponent<SandShape>();
                effect.kind="mould-tip";effect.raycastTarget=false;sandTipEffects.Add(effect);
            }
        }
        private void TickSandTip(SandpitState game,bool joined)
        {
            var before=sandTipFeedback.Events;
            sandTipFeedback.Observe(game,joined && !applicationPaused,Time.unscaledDeltaTime);
            for(var i=0;i<4;i++){
                var effect=sandTipEffects[i];var result=sandTipFeedback.Outcome(i);var active=joined && sandTipFeedback.Remaining(i)>0;
                effect.gameObject.SetActive(active && result!="underfilled");effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*sceneScale;
                effect.capacity=DaycareSandpit.Capacity(i);effect.tipOutcome=result;effect.progress=1-sandTipFeedback.Remaining(i)/(result=="underfilled"?SandTipFeedback.WiggleDuration:SandTipFeedback.Duration);effect.SetVerticesDirty();
                // Animate the existing authoritative tower, never a second temporary tower.
                sandShapes[i].towerReveal=active && result=="reveal"?Mathf.Clamp01((effect.progress-.52f)/.3f):1;
                sandShapes[i].hideBucket=active && result!="underfilled";
                sandShapes[i].wiggle=active && result=="underfilled"?Mathf.Sin(effect.progress*Mathf.PI*4)*5*(1-effect.progress):0;
                sandShapes[i].SetVerticesDirty();
                if(sandTipFeedback.Events[i]>before[i] && result=="collapse" && i==sandpitSelected){Narration.Speak("sand-crumble");homeFeedback.text="Dry sand crumbles. Try a little water!";homeFeedbackUntil=Time.unscaledTime+4;}
            }
        }
        private void AddSandTipDepth(Action<RectTransform,float,int,string> add)
        {for(var i=0;i<sandTipEffects.Count;i++)if(sandTipEffects[i].gameObject.activeSelf)add(sandTipEffects[i].rectTransform,sandPlaces[i].anchoredPosition.y,5,"sand-tip-"+i);}
        private void ResetSandTip(){sandTipEffects.Clear();sandTipFeedback=new SandTipFeedback();}
    }
}
