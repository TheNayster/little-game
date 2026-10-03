using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RawImage sandPitArt,sandScenery;
        private readonly Dictionary<string,SandMould> sandPresented=new Dictionary<string,SandMould>();
        private readonly List<SandShape> sandAcceptedEffects=new List<SandShape>();
        private readonly List<(Button button,SandShape badge)> sandCardBadges=new List<(Button,SandShape)>();
        private readonly string[] sandAcceptedIds=new string[DaycareSandpit.MaxPieces];
        private readonly float[] sandAcceptedUntil=new float[DaycareSandpit.MaxPieces];
        private readonly Dictionary<string,AudioClip> sandSounds=new Dictionary<string,AudioClip>();
        private readonly AudioSource[] sandSpeakers=new AudioSource[2];
        private int sandPresentationEpoch=-1,sandHeardReaction=-1,sandAudioEvents,sandVisualEvents;
        private bool sandPresenting;private float sandSoundAt,sandInvalidUntil;private Vector2 sandToyPrevious;
        public int SandPresentationEvents=>sandVisualEvents;
        public int SandAudioEvents=>sandAudioEvents;
        public int SandActiveVoices=>sandSpeakers.Count(s=>s!=null && s.isPlaying && s.volume>0);
        public int SandActiveEffects=>sandAcceptedUntil.Count(t=>t>Time.unscaledTime);
        public bool SandIllustrated=>sandPitArt!=null && sandPitArt.texture!=null && SandArt.Atlas!=null;
        public bool SandPhoneLayout=>safe.rect.width/safe.rect.height>=1.6f;
        private void BuildSandPresentation()
        {
            foreach(var button in sandMouldButtons.Concat(sandDecorCards)){
                var badge=Rect(button.transform,"Picture choice state",new Vector2(47,30),new Vector2(28,28)).gameObject.AddComponent<SandShape>();
                badge.rectTransform.localScale=Vector3.one*.32f;badge.raycastTarget=false;sandCardBadges.Add((button,badge));
            }
            for(var i=0;i<DaycareSandpit.MaxPieces;i++){
                var effect=Rect(sandFloor,"Accepted sand placement response "+i,Vector2.zero,new Vector2(300,330)).gameObject.AddComponent<SandShape>();
                effect.kind="accepted-spark";effect.raycastTarget=false;sandAcceptedEffects.Add(effect);
            }
            foreach(var id in new[]{"scoop","pour","reveal","decorate"})sandSounds[id]=WorldResources.Load<AudioClip>("Worlds/Daycare/SandcastleClub/fx-"+id);
            sandSounds["dinosaur"]=WorldResources.Load<AudioClip>("Worlds/Dinosaur/Audio/tyrannosaurus");
        }
        private void LayoutSandPresentation()
        {
            var phone=SandPhoneLayout;
            sandBuildView.sizeDelta=new Vector2(phone?1460:1210,740);
            sandBuildView.localScale=Vector3.one*Mathf.Min(1,safe.rect.width/(phone?1490:1240),safe.rect.height/770);
            sandScenery.rectTransform.sizeDelta=safe.rect.size/sandBuildView.localScale.x;
            // The phone uses its width for play and a longer picture shelf. Only
            // the local projection stretches; saved coordinates remain untouched.
            sandFloor.localScale=new Vector3(phone?1.14f:1,1,1);
            sandPitArt.rectTransform.sizeDelta=new Vector2(phone?1200:1060,560);
            for(var i=0;i<4;i++)((RectTransform)sandMouldButtons[i].transform).anchoredPosition=new Vector2((phone?-560:-435)+i*150,-318);
            for(var i=0;i<6;i++)((RectTransform)sandDecorCards[i].transform).anchoredPosition=new Vector2((phone?-560:-435)+i*150,-318);
            sandBuildView.Find("Build").GetComponent<RectTransform>().anchoredPosition=new Vector2(phone?-565:-440,-234);
            sandBuildView.Find("Decorate").GetComponent<RectTransform>().anchoredPosition=new Vector2(phone?-370:-245,-234);
            sandBuildView.Find("Optional teacher help").GetComponent<RectTransform>().anchoredPosition=new Vector2(phone?-668:-526,322);
            sandBuildView.Find("Leave sandpit").GetComponent<RectTransform>().anchoredPosition=new Vector2(phone?668:526,322);
            for(var i=0;i<4;i++){
                var root=sandBuilders[i].root;root.anchoredPosition=new Vector2(i<2?(phone?-663:-560):(phone?663:560),i%2==0?115:-95);
                root.localScale=Vector3.one*(phone?.78f:.7f);
            }
        }
        private void SandSound(string id)
        {
            if(Time.unscaledTime<sandSoundAt || applicationPaused || musicMuted || !Narration.VoiceEnabled || familyTestMuted || shared?.MutedTest==true)return;
            sandSounds.TryGetValue(id,out var clip);
            if(clip==null){clip=WorldResources.Load<AudioClip>(id=="dinosaur"?"Worlds/Dinosaur/Audio/tyrannosaurus":"Worlds/Daycare/SandcastleClub/fx-"+id);sandSounds[id]=clip;}
            if(clip==null)return;
            // No PlayOneShot pile-up: two reusable voices, free slot or drop.
            var slot=Array.FindIndex(sandSpeakers,s=>s==null || !s.isPlaying);if(slot<0)return;
            var speaker=sandSpeakers[slot];if(speaker==null){speaker=gameObject.AddComponent<AudioSource>();speaker.playOnAwake=false;speaker.loop=false;speaker.spatialBlend=0;sandSpeakers[slot]=speaker;}
            speaker.clip=clip;speaker.volume=.22f*ForegroundDucking;speaker.Play();sandSoundAt=Time.unscaledTime+.14f;sandAudioEvents++;
        }
        private void TickSandPresentation(SandpitState g,bool joined)
        {
            var baseline=!sandPresenting || !joined || applicationPaused || sandPresentationEpoch!=g.round;
            var now=Time.unscaledTime;
            for(var i=0;i<sandCardBadges.Count;i++){
                var pair=sandCardBadges[i];var selected=i<4?sandPlacing && sandMould==new[]{"round","square","wall","gate"}[i]:i==9?sandPlayMode=="toy":sandPlayMode=="decorate" && sandDecorKind==new[]{"flag","shell","pebble","door","window"}[i-4];
                pair.badge.gameObject.SetActive(selected || !pair.button.interactable || sandpitSending);
                pair.badge.kind=sandpitSending?"pending":!pair.button.interactable?"cancel":"confirm";pair.badge.presentationTime=now;pair.badge.SetVerticesDirty();
            }
            for(var i=0;i<g.moulds.Length;i++){
                var m=g.moulds[i];if(sandAcceptedIds[i]!=m.id)sandAcceptedUntil[i]=0;sandAcceptedIds[i]=m.id;sandPresented.TryGetValue(m.id,out var p);
                if(!baseline){
                    string sound=null;
                    if(p==null)sound="decorate";
                    else if(!p.built && m.built)sound="reveal";
                    else if(!p.wet && m.wet)sound="pour";
                    else if(m.scoops>p.scoops)sound="scoop";
                    else if(m.x!=p.x || m.y!=p.y || !m.attachments.Select(a=>a.slot+":"+a.kind).SequenceEqual(p.attachments.Select(a=>a.slot+":"+a.kind)))sound="decorate";
                    if(sound!=null){SandSound(sound);if(sound=="decorate"){sandAcceptedUntil[i]=now+.55f;sandVisualEvents++;}}
                }
                sandPresented[m.id]=m.Copy();
            }
            foreach(var id in sandPresented.Keys.Where(id=>!g.moulds.Any(m=>m.id==id)).ToArray())sandPresented.Remove(id);
            if(!baseline && (g.toy.reaction!=sandHeardReaction || g.toy.placed && sandToyPrevious!=new Vector2(g.toy.x,g.toy.y)))SandSound("dinosaur");
            sandHeardReaction=g.toy.reaction;sandToyPrevious=new Vector2(g.toy.x,g.toy.y);
            for(var i=0;i<sandAcceptedEffects.Count;i++){
                if(baseline)sandAcceptedUntil[i]=0;
                var effect=sandAcceptedEffects[i];effect.gameObject.SetActive(joined && i<g.moulds.Length && sandAcceptedUntil[i]>now);
                if(effect.gameObject.activeSelf){effect.rectTransform.anchoredPosition=SandPoint(g.moulds[i].x,g.moulds[i].y);effect.progress=1-(sandAcceptedUntil[i]-now)/.55f;effect.SetVerticesDirty();effect.rectTransform.SetAsLastSibling();}
            }
            sandPreview.wiggle=sandInvalidUntil>now?Mathf.Sin((sandInvalidUntil-now)*25)*6:0;
            if(baseline || musicMuted || !Narration.VoiceEnabled)foreach(var s in sandSpeakers)if(s!=null)s.Stop();
            sandPresentationEpoch=g.round;sandPresenting=joined && !applicationPaused;
        }
        private void ResetSandPresentation()
        {
            foreach(var s in sandSpeakers)if(s!=null){s.Stop();Destroy(s);}Array.Clear(sandSpeakers,0,2);
            sandSounds.Clear();sandPresented.Clear();sandAcceptedEffects.Clear();sandCardBadges.Clear();Array.Clear(sandAcceptedUntil,0,sandAcceptedUntil.Length);
            sandPresenting=false;sandPresentationEpoch=sandHeardReaction=-1;sandPitArt=sandScenery=null;SandArt.Release();
        }
    }
}
