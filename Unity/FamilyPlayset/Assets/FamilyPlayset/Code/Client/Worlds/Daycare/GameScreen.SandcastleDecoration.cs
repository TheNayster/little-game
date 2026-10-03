using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private const float SandFlagX=4490, SandShellX=4670, SandDecorationY=180;
        private SandDecorationFeedback sandDecorationFeedback=new SandDecorationFeedback();
        private readonly List<SandShape> sandDecorationObjects=new List<SandShape>(),sandDecorationEffects=new List<SandShape>();
        public int[] SandpitDecorationEffectValues=>Enumerable.Range(0,4).Select(sandDecorationFeedback.Value).ToArray();
        public int[] SandpitDecorationEvents=>sandDecorationFeedback.Events;
        public float[] SandpitDecorationEffects=>Enumerable.Range(0,4).Select(sandDecorationFeedback.Remaining).ToArray();
        public int[] SandpitDecorationVisuals=>sandShapes.Select(s=>s.decoration).ToArray();
        public bool[] SandpitDecorationHidden=>sandShapes.Select(s=>s.hideDecoration).ToArray();
        public float[] SandpitDecorationWiggles=>Enumerable.Range(0,2).Select(sandDecorationFeedback.Wiggle).ToArray();
        private void BuildSandDecoration()
        {
            for(var i=0;i<2;i++){
                var op=i==0?"flag":"shell";var root=Rect(Board,"Sandpit "+op+" object",Vector2.zero,new Vector2(180,140));
                var shape=root.gameObject.AddComponent<SandShape>();shape.kind=op+"-object";shape.raycastTarget=false;sandDecorationObjects.Add(shape);
                HomeHit(root,i==0?"Decorate with flag":"Decorate with shell",new Vector2(0,35),new Vector2(160,130),()=>RequestSandTool(op),false);
            }
            for(var i=0;i<4;i++){
                var effect=Rect(Board,"Confirmed tower decoration "+(i+1),Vector2.zero,new Vector2(1400,600)).gameObject.AddComponent<SandShape>();
                effect.kind="decoration-place";effect.raycastTarget=false;sandDecorationEffects.Add(effect);
            }
        }
        private void TickSandDecoration(SandpitState game,bool joined)
        {
            sandDecorationFeedback.Observe(game,joined && !applicationPaused,Time.unscaledDeltaTime);
            for(var i=0;i<2;i++){
                var shape=sandDecorationObjects[i];shape.gameObject.SetActive(joined);shape.rectTransform.anchoredPosition=ToBoard(i==0?SandFlagX:SandShellX,SandDecorationY);shape.rectTransform.localScale=Vector3.one*sceneScale;
                var left=sandDecorationFeedback.Wiggle(i);shape.wiggle=left>0?Mathf.Sin((1-left/SandDecorationFeedback.WiggleDuration)*Mathf.PI*4)*5:0;shape.SetVerticesDirty();
            }
            for(var i=0;i<4;i++){
                var effect=sandDecorationEffects[i];var active=joined && sandDecorationFeedback.Remaining(i)>0;
                effect.gameObject.SetActive(active);effect.rectTransform.anchoredPosition=sandPlaces[i].anchoredPosition;effect.rectTransform.localScale=Vector3.one*sceneScale;
                effect.decoration=sandDecorationFeedback.Value(i);effect.progress=1-sandDecorationFeedback.Remaining(i)/SandDecorationFeedback.Duration;
                var at=DaycareSandpit.Place(i);var h=DaycareSandpit.Capacity(i)==3?105:75;
                effect.source=new Vector2((effect.decoration==1?SandFlagX:SandShellX)-at.X,(SandDecorationY-at.Y)*.45f-16+(effect.decoration==2?29:0));
                effect.target=effect.decoration==1?new Vector2(0,h+10):new Vector2(27,h+26);effect.SetVerticesDirty();
                // Handoff to the one permanent authority-driven prop when the moving
                // object arrives. Leaving/resetting always restores final-state drawing.
                sandShapes[i].hideDecoration=active && effect.progress<.76f;sandShapes[i].SetVerticesDirty();
            }
        }
        private void AddSandDecorationDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var shape in sandDecorationObjects)if(shape.gameObject.activeSelf)add(shape.rectTransform,shape.rectTransform.anchoredPosition.y,3,"sand-decoration-object");
            for(var i=0;i<sandDecorationEffects.Count;i++)if(sandDecorationEffects[i].gameObject.activeSelf)add(sandDecorationEffects[i].rectTransform,sandDecorationObjects[0].rectTransform.anchoredPosition.y,5,"sand-decoration-flight-"+i);
        }
        private void ResetSandDecoration(){sandDecorationObjects.Clear();sandDecorationEffects.Clear();sandDecorationFeedback=new SandDecorationFeedback();}
    }
}
