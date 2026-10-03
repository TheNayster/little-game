using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private const float SandCanX=4310, SandCanY=180;
        private SandShape sandCanShape;
        private readonly List<SandShape> sandWaterEffects=new List<SandShape>();
        private SandWaterFeedback sandWaterFeedback=new SandWaterFeedback();
        public int[] SandpitWaterEvents=>sandWaterFeedback.Events;
        public float[] SandpitWaterEffects=>Enumerable.Range(0,4).Select(sandWaterFeedback.Remaining).ToArray();
        public bool[] SandpitWetVisuals=>sandShapes.Select(s=>s.wet).ToArray();

        private void BuildSandWater()
        {
            var root=Rect(Board,"Sandpit watering can",Vector2.zero,new Vector2(180,135));
            sandCanShape=root.gameObject.AddComponent<SandShape>();sandCanShape.kind="watering-can";sandCanShape.raycastTarget=false;
            HomeHit(root,"Water with watering can",new Vector2(0,30),new Vector2(160,130),()=>RequestSandTool("water"),false);
            for(var i=0;i<4;i++){
                var effect=Rect(Board,"Accepted sand water "+(i+1),Vector2.zero,new Vector2(1400,600)).gameObject.AddComponent<SandShape>();
                effect.kind="water-pour";effect.raycastTarget=false;sandWaterEffects.Add(effect);
            }
        }
        private void TickSandWater(SandpitState game,bool joined)
        {
            if(sandCanShape==null)return;
            sandWaterFeedback.Observe(game,joined && !applicationPaused,Time.unscaledDeltaTime);
            sandCanShape.gameObject.SetActive(joined);sandCanShape.rectTransform.anchoredPosition=ToBoard(SandCanX,SandCanY);sandCanShape.rectTransform.localScale=Vector3.one*sceneScale;
            for(var i=0;i<4;i++){
                var effect=sandWaterEffects[i];effect.gameObject.SetActive(joined && sandWaterFeedback.Remaining(i)>0);
                effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*sceneScale;
                var at=DaycareSandpit.Place(i);
                effect.source=new Vector2(SandCanX-at.X,(SandCanY-at.Y)*.45f-16+24);
                effect.target=new Vector2(0,DaycareSandpit.Capacity(i)==3?75:55);
                effect.progress=1-sandWaterFeedback.Remaining(i)/SandWaterFeedback.Duration;effect.SetVerticesDirty();
            }
            // The can visibly lifts, pours down into the rim and returns to its floor
            // shadow. Its existing pointer target stays fixed and never captures a drag.
            sandCanShape.pouring=sandWaterEffects.Any(e=>e.gameObject.activeSelf);sandCanShape.SetVerticesDirty();
        }
        private void AddSandWaterDepth(Action<RectTransform,float,int,string> add)
        {
            if(sandCanShape==null || !sandCanShape.gameObject.activeSelf)return;
            add(sandCanShape.rectTransform,sandCanShape.rectTransform.anchoredPosition.y,3,"sand-watering-can");
            for(var i=0;i<sandWaterEffects.Count;i++)if(sandWaterEffects[i].gameObject.activeSelf)
                add(sandWaterEffects[i].rectTransform,sandCanShape.rectTransform.anchoredPosition.y,4,"sand-water-pour-"+i);
        }
        private void ResetSandWater()
        {sandCanShape=null;sandWaterEffects.Clear();sandWaterFeedback=new SandWaterFeedback();}
    }
}
