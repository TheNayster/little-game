using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform elephantBasket,elephantCareWait;
        private Image elephantBrushButton;
        private readonly Image[] elephantDust=new Image[3];
        private readonly VetTouchSurface[] elephantCareTouch=new VetTouchSurface[3];
        private readonly RectTransform[] elephantBrushes=new RectTransform[4];
        private readonly Vector2[] elephantPatchPoints={new Vector2(-70,202),new Vector2(22,234),new Vector2(105,160)};
        private long elephantGesture;
        private bool elephantCareSending;
        private int elephantLocalPatch;
        private float elephantLocalStrokeAt=-10;
        private readonly double[] elephantBrushSampleAge={10,10,10,10};
        private readonly float[] elephantBrushSampleAt=new float[4];
        private float elephantBrushNext;
        private int elephantCareSeen=-1,elephantCareReaction=-1,elephantCareFinishEvents;
        private bool elephantCareSeenComplete;
        public Vector2[] ElephantBrushPositions=>elephantBrushes.Where(r=>r!=null && r.gameObject.activeInHierarchy).Select(r=>r.anchoredPosition).ToArray();
        public Color[] ZooRouteColors=>new[]{zooBackButton.image.canvasRenderer.GetColor(),zooForwardButton.image.canvasRenderer.GetColor(),zooGateButton.image.canvasRenderer.GetColor()};
        public int ElephantCareFinishEvents=>elephantCareFinishEvents;
        private void DrawElephantBrush(Transform parent,Vector2 at,float scale)
        {
            var brush=Rect(parent,"Brush drawing",at,new Vector2(62,58)*scale);
            Plain(brush,"Wooden handle",new Vector2(0,19)*scale,new Vector2(14,35)*scale,new Color(.69f,.45f,.25f));
            Panel(brush,"Brush back",new Vector2(0,-2)*scale,new Vector2(59,29)*scale,new Color(.96f,.73f,.38f),false,true);
            for(var j=0;j<6;j++)Plain(brush,"Soft bristles "+j,new Vector2((j-2.5f)*8,-20)*scale,new Vector2(6,18)*scale,new Color(.9f,.83f,.65f));
        }
        private void BuildElephantCare()
        {
            // Native editable geometry follows SourceArt/Zoo/Playable/elephant-care.svg.
            elephantBasket=ZooObject("Elephant brush basket");
            Panel(elephantBasket,"Basket shadow",new Vector2(0,-5),new Vector2(105,18),new Color(.35f,.3f,.2f,.15f),false,true);
            elephantBrushButton=Panel(elephantBasket,"Choose elephant brush",new Vector2(0,70),new Vector2(116,126),Cream,true,true);
            Panel(elephantBrushButton.transform,"Woven basket",new Vector2(0,-30),new Vector2(97,45),new Color(.72f,.53f,.32f),false,true);
            DrawElephantBrush(elephantBrushButton.transform,new Vector2(0,18),1);
            NavButton(elephantBrushButton,()=>{if(CareInputReady)SendZoo("care","elephant",_=>{});});
            var put=Panel(elephantBasket,"Put elephant brush away",new Vector2(0,212),new Vector2(108,92),Cream,true,true);
            Panel(put.transform,"Empty basket",new Vector2(0,-16),new Vector2(69,29),new Color(.72f,.53f,.32f),false,true);
            var arrow=Plain(put.transform,"Down arrow",new Vector2(0,15),new Vector2(12,34),new Color(.36f,.56f,.45f));
            var tip=Plain(put.transform,"Arrow tip",new Vector2(0,0),new Vector2(24,24),new Color(.36f,.56f,.45f));tip.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            NavButton(put,()=>{if(CareInputReady)SendZoo("put-brush","elephant",_=>{});});
            elephantCareWait=Rect(elephantBasket,"Care waiting picture",new Vector2(-87,78),new Vector2(60,70));
            Panel(elephantCareWait,"Waiting face",Vector2.zero,Vector2.one*54,new Color(.8f,.88f,.74f),false,true);
            Plain(elephantCareWait,"Clock hand",new Vector2(0,8),new Vector2(5,22),Ink);
            Plain(elephantCareWait,"Clock hand across",new Vector2(8,0),new Vector2(21,5),Ink);
            var animalRoot=zooAnimals["elephant"].transform.parent;
            for(var i=0;i<3;i++){
                var patch=i;
                var area=Panel(animalRoot,"Brush elephant patch "+i,elephantPatchPoints[i],new Vector2(130,105),new Color(0,0,0,0),true,true);
                elephantDust[i]=Panel(area.transform,"Soft dust patch "+i,Vector2.zero,new Vector2(103,68),new Color(.79f,.64f,.43f,.65f),false,true);
                for(var j=0;j<3;j++)Panel(elephantDust[i].transform,"Dust speck "+j,new Vector2((j-1)*25,(j%2==0?10:-8)),new Vector2(12,8),new Color(.9f,.78f,.56f,.6f),false,true);
                var surface=area.gameObject.AddComponent<VetTouchSurface>();elephantCareTouch[i]=surface;
                surface.Stroke=(point,gesture,first)=>BrushElephantPatch(patch,point,first);
            }
            for(var i=0;i<4;i++){
                elephantBrushes[i]=Rect(animalRoot,"Helper brush "+i,Vector2.zero,Vector2.zero);
                DrawElephantBrush(elephantBrushes[i],Vector2.zero,.75f);
            }
        }
        private bool CareInputReady=>Ready && !MenuOpen && !ZooMapOpen && !TravelPending && !ActionPending && !applicationPaused;
        private void BrushElephantPatch(int patch,Vector2 point,bool first)
        {
            var z=Zoo;
            if(!CareInputReady || !first || z==null || !z.careMembers.Contains(Actor) || z.careComplete || z.animals[0].phase!=ZooPhase.Care || Time.realtimeSinceStartup<elephantBrushNext)return;
            elephantBrushNext=Time.realtimeSinceStartup+.35f;
            var session=z.careSession;var token=session+"/"+z.careMemberEpoch[Array.IndexOf(z.careMembers,Actor)]+"/"+patch+"/"+(++elephantGesture);
            // One accepted credit per distinct tap/stroke. Drag frames never send
            // commands, and a held finger cannot add progress on its own.
            elephantLocalPatch=patch;elephantLocalStrokeAt=Time.realtimeSinceStartup;elephantCareSending=Shared;
            void Done(SoloResult result){elephantCareSending=false;if(!result.Accepted)elephantLocalStrokeAt=-10;}
            if(Shared){if(!SubmitShared(SoloAction.Zoo,token,"elephant","brush",0,0,Done))elephantCareSending=false;}
            else Done(Command(SoloAction.Zoo,item:token,target:"elephant",value:"brush"));
        }
        private void TickElephantCare(ZooState z,ZooAnimal a,bool shown)
        {
            if(elephantBasket==null)return;
            var own=z.careMembers.Contains(Actor);
            var available=shown && a.phase==ZooPhase.Care && !z.careComplete && !applicationPaused && (!Shared || shared.Connected);
            elephantBasket.gameObject.SetActive(shown);elephantBasket.anchoredPosition=ToBoard(ZooLayout.BrushX,180);elephantBasket.localScale=Vector3.one*sceneScale;
            elephantBrushButton.color=own?new Color(1,.85f,.46f):Cream;
            elephantBasket.Find("Put elephant brush away").gameObject.SetActive(own);
            elephantCareWait.gameObject.SetActive(own && !available && !z.careComplete);
            if(elephantCareSeen>=0 && elephantCareSeen==z.careSession && !elephantCareSeenComplete && z.careComplete && shown && !applicationPaused && (!Shared || shared.Connected)){
                elephantCareReaction=z.careSession;elephantCareFinishEvents++;
            }
            elephantCareSeen=z.careSession;elephantCareSeenComplete=z.careComplete;
            for(var i=0;i<3;i++){
                var surface=elephantCareTouch[i];surface.gameObject.SetActive(available && z.careProgress[i]<4);
                surface.GetComponent<Image>().raycastTarget=own && !ZooMapOpen && !MenuOpen;
                elephantDust[i].color=new Color(.79f,.64f,.43f,.7f*(1-z.careProgress[i]/4f));
                // Children follow the accepted fade too; no retained opaque specks.
                foreach(Transform dot in elephantDust[i].transform)dot.GetComponent<Image>().color=new Color(.9f,.78f,.56f,.6f*(1-z.careProgress[i]/4f));
            }
            for(var i=0;i<4;i++){
                var cursor=elephantBrushes[i];cursor.gameObject.SetActive(available && i<z.careMembers.Length);
                if(!cursor.gameObject.activeSelf)continue;
                var patch=z.carePatch[i];var now=Time.realtimeSinceStartup;
                if(elephantBrushSampleAge[i]!=z.careBrushAge[i]){elephantBrushSampleAge[i]=z.careBrushAge[i];elephantBrushSampleAt[i]=now;}
                var age=z.careBrushAge[i]+(Shared?Math.Max(0,now-elephantBrushSampleAt[i]):0);
                if(z.careMembers[i]==Actor && now-elephantLocalStrokeAt<.65f){patch=elephantLocalPatch;age=now-elephantLocalStrokeAt;}
                var sweep=age<.65?Mathf.Sin((float)age/.65f*Mathf.PI*4)*20:0;
                cursor.anchoredPosition=elephantPatchPoints[Math.Max(0,patch)]+new Vector2((i-1.5f)*29+sweep,-44);
                cursor.localRotation=Quaternion.Euler(0,0,(i-1.5f)*9+sweep*.35f);
            }
        }
        private void ResetElephantCareObservation(){elephantCareSeen=-1;elephantCareReaction=-1;elephantCareSending=false;elephantLocalStrokeAt=-10;}
    }
}
