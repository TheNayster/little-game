using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform elephantWater,elephantFloat,elephantCuriousLeaf;
        private Image elephantPump,elephantRipple;
        private readonly RectTransform[] elephantDrops=new RectTransform[12];
        private int elephantWaterSeen=-1,elephantWaterDraw=-1,elephantWaterEvents;
        private double elephantWaterSampleAge;
        private float elephantWaterSampleAt,elephantPumpNext;
        public int ElephantWaterEvents=>elephantWaterEvents;
        public int ElephantEffectObjects=>elephantWater==null?0:elephantDrops.Length;
        private void BuildElephantPlay()
        {
            // Editable UI geometry follows SourceArt/Zoo/Playable/elephant-water.svg.
            // All droplets exist once; repeated play never creates effects/timers.
            elephantWater=ZooObject("Elephant water station");
            Panel(elephantWater,"Basin shadow",new Vector2(0,-8),new Vector2(250,35),new Color(.32f,.36f,.24f,.18f),false,true);
            Panel(elephantWater,"Shallow basin",new Vector2(0,27),new Vector2(230,70),new Color(.67f,.53f,.36f),false,true);
            Panel(elephantWater,"Basin rim",new Vector2(0,53),new Vector2(246,50),new Color(.86f,.74f,.52f),false,true);
            Panel(elephantWater,"Blue water",new Vector2(0,57),new Vector2(211,31),new Color(.43f,.77f,.86f),false,true);
            elephantRipple=Panel(elephantWater,"Water ripple",new Vector2(4,59),new Vector2(70,10),new Color(.83f,.96f,.98f,.7f),false,true);
            elephantFloat=Rect(elephantWater,"Floating leaf",new Vector2(-18,62),Vector2.zero);
            var leaf=Panel(elephantFloat,"Leaf",Vector2.zero,new Vector2(49,20),new Color(.39f,.62f,.24f),false,true);
            leaf.rectTransform.localRotation=Quaternion.Euler(0,0,-15);
            Plain(elephantFloat,"Leaf vein",Vector2.zero,new Vector2(36,3),new Color(.72f,.81f,.42f));
            // A separate pump beside the left-hand pool leaves its water/leaf
            // visible and puts the whole toy away from the right-hand feeding positions.
            Plain(elephantWater,"Pump post",new Vector2(-130,75),new Vector2(30,140),new Color(.41f,.62f,.64f));
            Plain(elephantWater,"Pump spout",new Vector2(-83,120),new Vector2(106,24),new Color(.41f,.62f,.64f));
            Plain(elephantWater,"Spout end",new Vector2(-35,107),new Vector2(24,40),new Color(.41f,.62f,.64f));
            elephantPump=Panel(elephantWater,"Splash button",new Vector2(-130,166),new Vector2(144,120),new Color(.98f,.83f,.4f),true,true);
            NavButton(elephantPump,()=>{
                if(ZooMapOpen || ActionPending || applicationPaused || Time.realtimeSinceStartup<elephantPumpNext)return;
                elephantPumpNext=Time.realtimeSinceStartup+1.2f;
                SendZoo("water","elephant",_=>{});
            });
            // A drop symbol makes the one-tap action readable without text.
            Panel(elephantPump.transform,"Water drop",new Vector2(0,-3),new Vector2(33,41),new Color(.2f,.59f,.76f),false,true);
            var tip=Plain(elephantPump.transform,"Drop tip",new Vector2(0,13),new Vector2(23,23),new Color(.2f,.59f,.76f));tip.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            for(var i=0;i<elephantDrops.Length;i++)elephantDrops[i]=Panel(elephantWater,"Splash droplet "+i,Vector2.zero,new Vector2(9,15),new Color(.49f,.83f,.94f,.9f),false,true).rectTransform;
            elephantCuriousLeaf=Rect(zooAnimals["elephant"].transform.parent,"Curious leaf",Vector2.zero,Vector2.zero);
            var curious=Panel(elephantCuriousLeaf,"Investigated leaf",Vector2.zero,new Vector2(42,18),new Color(.43f,.65f,.29f),false,true);curious.rectTransform.localRotation=Quaternion.Euler(0,0,22);
        }
        private static int ElephantPlayFrame(ZooAnimal a,double age,int ordinary)
        {
            // Existing drawn ear/trunk poses articulate the action. Body scale
            // and the feeding sockets remain exactly as approved.
            if(a.phase==ZooPhase.Greet || a.phase==ZooPhase.Curious || a.phase==ZooPhase.Splash)return 4;
            return ordinary;
        }
        private void PoseElephant(ElephantArtView image,ZooAnimal a,double age)
        {
            var wave=Mathf.Sin(Mathf.Clamp01((float)(age/a.duration))*Mathf.PI);
            if(a.phase==ZooPhase.Greet)image.Pose(24*wave,4*Mathf.Sin((float)age*5)*wave);
            else if(a.phase==ZooPhase.Curious)image.Pose(-12*wave,0);
            else if(a.phase==ZooPhase.Splash)image.Pose(28*wave,2*wave);
            else if(a.owner!="" && !a.consumed && (a.phase==ZooPhase.Approach || a.phase==ZooPhase.Eat)){
                var reach=a.phase==ZooPhase.Eat?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(((float)(age/a.duration)-.65f)/.35f));
                // The original hanging tip is just ahead of the low hand.
                // A bounded downward curl reaches it without translating pixels.
                image.Pose(-18*reach,0);
            }
            else if(a.phase==ZooPhase.CareFinish && Zoo.careSession==elephantCareReaction)image.Pose(12*wave,5*Mathf.Sin((float)age*7)*wave);
            else image.Pose(0,0);
        }
        private void TickElephantPlay(ZooState z,ZooAnimal a,bool shown,double animalAge)
        {
            elephantWater.gameObject.SetActive(shown);
            elephantWater.anchoredPosition=ToBoard(ZooLayout.WaterX,ZooLayout.WaterY);elephantWater.localScale=Vector3.one*sceneScale;
            elephantCuriousLeaf.gameObject.SetActive(shown && a.phase==ZooPhase.Curious);
            var drawing=(ElephantArtView)zooAnimals["elephant"];
            var tip=drawing.TrunkTip;
            elephantCuriousLeaf.anchoredPosition=tip+new Vector2(-15,-12);
            if(elephantWaterSeen!=z.waterSequence || elephantWaterSampleAge!=z.waterAge){
                var seeded=elephantWaterSeen<0;
                if(!seeded && elephantWaterSeen!=z.waterSequence && shown && !applicationPaused && (!Shared || shared.Connected) && z.waterAge<1.2){
                    elephantWaterDraw=z.waterSequence;elephantWaterEvents++;ZooWaterSound();
                }
                elephantWaterSeen=z.waterSequence;elephantWaterSampleAge=z.waterAge;elephantWaterSampleAt=Time.realtimeSinceStartup;
            }
            var waterAge=z.waterAge+(Shared?Math.Max(0,Time.realtimeSinceStartup-elephantWaterSampleAt):0);
            var pulse=shown && !applicationPaused && elephantWaterDraw==z.waterSequence && waterAge<1.1;
            var splash=shown && !applicationPaused && a.phase==ZooPhase.Splash && animalAge>=.25 && animalAge<1.5;
            var motion=pulse?Mathf.Sin((float)waterAge*Mathf.PI/1.1f):splash?Mathf.Sin((float)(animalAge-.25)*Mathf.PI/1.25f):0;
            elephantPump.color=z.waterCooldown>0?new Color(.77f,.91f,.91f):new Color(.98f,.83f,.4f);
            elephantFloat.anchoredPosition=new Vector2(-18+motion*12,62+motion*7);elephantFloat.localRotation=Quaternion.Euler(0,0,motion*12);
            elephantRipple.rectTransform.sizeDelta=new Vector2(70+motion*110,10+motion*8);
            for(var i=0;i<elephantDrops.Length;i++){
                var drop=elephantDrops[i];drop.gameObject.SetActive(pulse || splash);if(!pulse && !splash)continue;
                var t=Mathf.Repeat((float)(pulse?waterAge:animalAge-.25)*1.7f+i*.073f,1);
                // The elephant's approved curled-trunk socket is the origin.
                // Droplets fall into the basin, never toward the camera/UI.
                var point=ZooLayout.Point(a);
                var origin=pulse?new Vector2(-35,89):new Vector2(point.X-ZooLayout.WaterX,(point.Y-ZooLayout.WaterY)*.45f)+tip;
                var end=new Vector2((i-5.5f)*15,61);
                drop.anchoredPosition=Vector2.Lerp(origin,end,t)+new Vector2(0,Mathf.Sin(t*Mathf.PI)*(pulse?24:36));
            }
        }
        private void ResetElephantPlayObservation(){elephantWaterSeen=-1;elephantWaterDraw=-1;}
    }
}
