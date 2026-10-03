using System;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private Worlds.Beach.SeagullFlockView gullFlock;
        public SeagullState Seagulls=>HasWorld?(Shared?shared.View.seagulls:World.ReadSeagulls()):null;
        public int VisibleSeagulls=>gullFlock?.VisibleCount??0;
        public int VisibleGullTracks=>gullFlock?.VisibleTracks??0;
        private void BuildSeagulls()
        {
            if(SceneSchema<BeachSeagulls.Schema)return;
            gullFlock=new Worlds.Beach.SeagullFlockView();
            gullFlock.Build(Board,Rect,HomePicture,
                (parent,name,pos,size,action)=>HomeHit(parent,name,pos,size,action,false),
                (parent,name,pos,size,color)=>Panel(parent,name,pos,size,color),SayHelloToSeagulls);
            TickSeagulls();
        }
        private void SayHelloToSeagulls()
        {
            void Done(SoloResult result){if(!result.Accepted){homeFeedback.text=result.Outcome=="let-birds-settle"?"Let the birds settle." : "Walk closer to the birds.";homeFeedbackUntil=Time.unscaledTime+2;}}
            if(Shared)SubmitShared(SoloAction.Beach,"","flock","hello",0,0,Done);
            else Done(Command(SoloAction.Beach,target:"flock",value:"hello"));
        }
        private void TickSeagulls()
        {
            if(gullFlock==null)return;var g=Seagulls;if(g==null)return;
            gullFlock.Tick(g,Shared?shared.View.worldId:World.WorldId,CurrentArea=="beach",sceneScale,ToBoard,Time.unscaledDeltaTime);
        }
        private void AddSeagullDepth(Action<UnityEngine.RectTransform,float,int,string> add)=>gullFlock?.AddDepth(Seagulls,ToBoard,add);
        private void ResetSeagulls(){gullFlock?.Reset();gullFlock=null;}
    }
}
