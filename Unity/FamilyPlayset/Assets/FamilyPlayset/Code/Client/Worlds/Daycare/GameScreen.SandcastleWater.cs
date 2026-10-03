using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly List<SandShape> sandWaterEffects=new List<SandShape>();
        private SandWaterFeedback sandWaterFeedback=new SandWaterFeedback();
        public int[] SandpitWaterEvents=>sandWaterFeedback.Events;
        public float[] SandpitWaterEffects=>Enumerable.Range(0,DaycareSandpit.MaxPieces).Select(sandWaterFeedback.Remaining).ToArray();
        public bool[] SandpitWetVisuals=>sandShapes.Select(s=>s.wet).ToArray();
        private void BuildSandWater()
        {
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var effect=Rect(sandFloor,"Accepted sand water "+(i+1),Vector2.zero,new Vector2(1400,600)).gameObject.AddComponent<SandShape>();
                effect.kind="water-pour";effect.raycastTarget=false;sandWaterEffects.Add(effect);
            }
        }
        private void TickSandWater(SandpitState game,bool joined)
        {
            sandWaterFeedback.Observe(game,joined && !applicationPaused,Time.unscaledDeltaTime);
            for(var i=0;i<sandWaterEffects.Count;i++){
                var effect=sandWaterEffects[i];effect.gameObject.SetActive(joined && i<game.moulds.Length && sandWaterFeedback.Remaining(i)>0);
                if(i>=game.moulds.Length)continue;
                effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*.55f;
                effect.source=new Vector2(-70,-25);effect.target=new Vector2(0,game.moulds[i].capacity==3?75:55);
                effect.progress=1-sandWaterFeedback.Remaining(i)/SandWaterFeedback.Duration;effect.SetVerticesDirty();
            }
        }
        private void ResetSandWater(){sandWaterEffects.Clear();sandWaterFeedback=new SandWaterFeedback();}
    }
}
