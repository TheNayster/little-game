using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly RectTransform[] surpriseProps=new RectTransform[2],surpriseLeaves=new RectTransform[5],surpriseButterflies=new RectTransform[3];
        private RectTransform surpriseBird;
        private readonly int[] surpriseSeen={-1,-1};
        private readonly double[] surpriseSampleAge={10,10};
        private readonly float[] surpriseSampleAt=new float[2];
        private AudioSource surpriseVoice;
        private float surpriseNextSound;
        private int surpriseSounds;
        public int SurpriseSounds=>surpriseSounds;
        public int SurpriseObjects=>surpriseProps[0]==null?0:surpriseProps[0].GetComponentsInChildren<Transform>(true).Length+surpriseProps[1].GetComponentsInChildren<Transform>(true).Length;
        private void BuildElephantSurprises()
        {
            // Original editable geometry: SourceArt/Zoo/Playable/elephant-surprises.svg.
            // All actors are allocated once, with no listeners/timers per event.
            for(var i=0;i<2;i++){
                var index=i;var root=surpriseProps[i]=ZooObject(i==0?"Elephant leafy visitor":"Elephant butterfly flowers");
                Panel(root,"Ground shade",Vector2.zero,new Vector2(146,19),new Color(.28f,.37f,.23f,.2f),false,true);
                HomeHit(root,i==0?"Rustling leaves":"Butterfly flowers",new Vector2(0,48),new Vector2(174,130),()=>{
                    if(ZooMapOpen || ActionPending || applicationPaused || Shared && !shared.Connected)return;
                    if(Zoo.surpriseAge[index]<ZooLayout.SurpriseReset)return;
                    // No walking, tool selection, or feeding-lease changes.
                    if(Shared)SubmitShared(SoloAction.Zoo,"",index==0?ZooLayout.BirdSurprise:ZooLayout.ButterflySurprise,"surprise",0,0,_=>{});
                    else Command(SoloAction.Zoo,target:index==0?ZooLayout.BirdSurprise:ZooLayout.ButterflySurprise,value:"surprise");
                },false);
            }
            surpriseBird=Rect(surpriseProps[0],"Hidden garden bird",new Vector2(0,38),Vector2.zero);
            Panel(surpriseBird,"Bird outline",Vector2.zero,new Vector2(51,47),new Color(.22f,.35f,.34f),false,true);
            Panel(surpriseBird,"Bird body",new Vector2(0,1),new Vector2(44,40),new Color(.46f,.74f,.73f),false,true);
            Panel(surpriseBird,"Cream tummy",new Vector2(-3,-8),new Vector2(25,18),new Color(.98f,.9f,.66f),false,true);
            Panel(surpriseBird,"Eye",new Vector2(10,9),new Vector2(6,7),Ink,false,true);
            var beak=Plain(surpriseBird,"Golden beak",new Vector2(26,3),new Vector2(13,11),new Color(.98f,.72f,.36f));beak.rectTransform.localRotation=Quaternion.Euler(0,0,35);
            for(var i=0;i<5;i++){
                var leaf=surpriseLeaves[i]=Rect(surpriseProps[0],"Visitor leaf "+i,new Vector2((i-2)*25,25+(i%2)*13),Vector2.zero);
                Panel(leaf,"Leaf outline",Vector2.zero,new Vector2(49,73),new Color(.26f,.43f,.26f),false,true);
                Panel(leaf,"Leaf green",Vector2.zero,new Vector2(42,65),new Color(.45f+i*.015f,.65f,.32f),false,true);
                Plain(leaf,"Leaf vein",Vector2.zero,new Vector2(3,48),new Color(.72f,.79f,.44f));
            }
            for(var i=0;i<5;i++){
                var x=(i-2)*27;var y=37+(i%2)*15;
                Plain(surpriseProps[1],"Flower stem "+i,new Vector2(x,y/2),new Vector2(4,y),new Color(.32f,.53f,.3f));
                for(var j=0;j<5;j++)Panel(surpriseProps[1],"Petal "+i+" "+j,new Vector2(x+Mathf.Cos(j*1.2566f)*11,y+Mathf.Sin(j*1.2566f)*11),new Vector2(19,22),i%2==0?new Color(.88f,.67f,.74f):new Color(.97f,.84f,.58f),false,true);
                Panel(surpriseProps[1],"Flower heart "+i,new Vector2(x,y),Vector2.one*12,new Color(.72f,.49f,.23f),false,true);
            }
            for(var i=0;i<3;i++){
                var b=surpriseButterflies[i]=Rect(surpriseProps[1],"Butterfly "+i,Vector2.zero,Vector2.zero);
                for(var j=0;j<2;j++){
                    var wing=Panel(b,"Outlined wing "+j,new Vector2(j==0?-12:12,3),new Vector2(26,32),new Color(.4f,.33f,.32f),false,true);
                    Panel(wing.transform,"Wing color",Vector2.zero,new Vector2(21,27),i==0?new Color(.97f,.79f,.45f):i==1?new Color(.72f,.69f,.88f):new Color(.91f,.63f,.67f),false,true);
                }
                Panel(b,"Butterfly body",Vector2.zero,new Vector2(6,29),new Color(.4f,.33f,.32f),false,true);
            }
        }
        private void TickElephantSurprises(ZooState z,bool visible)
        {
            var now=Time.realtimeSinceStartup;
            for(var i=0;i<2;i++){
                var root=surpriseProps[i];root.gameObject.SetActive(visible);
                root.anchoredPosition=ToBoard(i==0?480:1920,440);root.localScale=Vector3.one*sceneScale;
                if(!visible){surpriseSeen[i]=-1;continue;}
                if(surpriseSeen[i]!=z.surpriseSequence[i] || surpriseSampleAge[i]!=z.surpriseAge[i]){
                    if(surpriseSeen[i]>=0 && surpriseSeen[i]!=z.surpriseSequence[i] && z.surpriseAge[i]<.8 && !applicationPaused && (!Shared || shared.Connected))PlaySurpriseSound(i);
                    surpriseSeen[i]=z.surpriseSequence[i];surpriseSampleAge[i]=z.surpriseAge[i];surpriseSampleAt[i]=now;
                }
                var age=z.surpriseAge[i]+(Shared?Math.Max(0,now-surpriseSampleAt[i]):0);
                var active=z.surpriseSequence[i]>0 && age<6;
                if(i==0){
                    var reveal=active?Mathf.Sin(Mathf.Clamp01((float)age/.55f)*Mathf.PI/2)*Mathf.Clamp01((float)(4.8-age)/.7f):0;
                    surpriseBird.gameObject.SetActive(reveal>0);
                    surpriseBird.anchoredPosition=new Vector2(0,36+reveal*55);
                    surpriseBird.localRotation=Quaternion.Euler(0,0,reveal*(z.surpriseVariation[i]==0?-1:1)*Mathf.Sin((float)age*3)*9);
                    for(var j=0;j<5;j++)surpriseLeaves[j].localRotation=Quaternion.Euler(0,0,(j-2)*9+reveal*(j-2)*15+(active?Mathf.Sin((float)age*8)*2:Mathf.Repeat(now,16)<1.2f?Mathf.Sin(now*5)*2:0));
                }else{
                    root.localRotation=Quaternion.Euler(0,0,active?Mathf.Sin((float)age*3)*1.5f:0);
                    for(var j=0;j<3;j++){
                        var b=surpriseButterflies[j];b.gameObject.SetActive(active);
                        if(!active)continue;
                        // Local arcs remain beside the flowers, above tools and faces.
                        var t=Mathf.Clamp01((float)age/2.8f);var returning=Mathf.Clamp01((float)(age-4.4)/1.6f);
                        var side=(j-1)*47;var lift=Mathf.Sin(t*Mathf.PI/2)*68*(1-returning);
                        b.anchoredPosition=new Vector2(side*(1+Mathf.Sin(t*Mathf.PI)*.4f),50+lift+j*11);
                        b.localScale=new Vector3(age>2.8 && age<4.4?1:.7f+.25f*Mathf.Sin((float)age*10+j),1,1)*Mathf.Clamp01((float)(6-age)/.65f);
                    }
                }
            }
        }
        private void PlaySurpriseSound(int index)
        {
            if(ZooEffectGain==0 || Time.realtimeSinceStartup<surpriseNextSound)return;
            var clip=ZooClip("Worlds/Zoo/Audio/"+(index==0?"visitor-chirp":"flower-rustle"));if(clip==null)return;
            if(surpriseVoice==null){surpriseVoice=Board.gameObject.AddComponent<AudioSource>();surpriseVoice.playOnAwake=false;surpriseVoice.spatialBlend=0;}
            surpriseVoice.clip=clip;surpriseVoice.volume=.1f*ZooEffectGain;surpriseVoice.Play();surpriseSounds++;surpriseNextSound=Time.realtimeSinceStartup+1;
        }
        private void ResetSurpriseObservation(){for(var i=0;i<2;i++)surpriseSeen[i]=-1;if(surpriseVoice!=null)surpriseVoice.Stop();}
    }
}
