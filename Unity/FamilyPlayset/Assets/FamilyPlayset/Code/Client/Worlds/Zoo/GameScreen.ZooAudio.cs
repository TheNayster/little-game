using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly Dictionary<string,AudioClip> zooClips=new Dictionary<string,AudioClip>();
        private readonly Dictionary<string,int> zooHeard=new Dictionary<string,int>();
        private AudioSource zooVoice,zooFoley;
        private float zooNextCall,zooNextStep;
        private float zooNextWater;
        private float ZooEffectGain=>musicMuted || familyTestMuted || shared?.MutedTest==true?0:ForegroundDucking;
        public bool ZooSoundPlaying=>zooVoice!=null && zooVoice.isPlaying || zooFoley!=null && zooFoley.isPlaying;
        public int ZooAudioClipCount=>zooClips.Count;
        private string ZooVoicePath(string species)
        {
            var index=species=="tyrannosaurus"?0:species=="triceratops"?1:species=="stegosaurus"?2:species=="brachiosaurus"?3:-1;
            return index<0?"Worlds/Zoo/Audio/"+ZooCatalog.Get(species).sound:"Worlds/Home/Books/hello-dinosaurs/audio/effect-"+index;
        }
        private AudioClip ZooClip(string path)
        {
            if(!zooClips.TryGetValue(path,out var clip)){clip=WorldResources.Load<AudioClip>(path);if(clip!=null)zooClips.Add(path,clip);}
            return clip;
        }
        private void ZooPlay(string species,bool foley)
        {
            if(applicationPaused || !ZooCatalog.Trail(CurrentArea) || ZooCatalog.Get(species).area!=CurrentArea)return;
            var path=foley?(ZooCatalog.Get(species).habitat==ZooHabitat.Tank?"Worlds/Zoo/Audio/water":"Worlds/Zoo/Audio/feeding"):ZooVoicePath(species);
            var clip=ZooClip(path);if(clip==null)return;
            if(zooVoice==null){zooVoice=Board.gameObject.AddComponent<AudioSource>();zooVoice.playOnAwake=false;zooVoice.spatialBlend=0;zooVoice.priority=110;}
            if(zooFoley==null){zooFoley=Board.gameObject.AddComponent<AudioSource>();zooFoley.playOnAwake=false;zooFoley.spatialBlend=0;zooFoley.priority=130;}
            var source=foley?zooFoley:zooVoice;source.Stop();source.clip=clip;
            source.panStereo=Mathf.Clamp((ZooCatalog.Get(species).Center-cameraX)/(Board.rect.width/sceneScale),-.7f,.7f);
            source.volume=(foley?.18f:.26f)*ZooEffectGain;source.Play();
        }
        private void ZooCall(string species){ZooPlay(species,false);zooNextCall=Time.realtimeSinceStartup+8;}
        private void ZooWaterSound()
        {
            if(applicationPaused || Time.realtimeSinceStartup<zooNextWater || ZooEffectGain==0)return;
            var clip=ZooClip("Worlds/Zoo/Audio/water");if(clip==null)return;
            if(zooFoley==null){zooFoley=Board.gameObject.AddComponent<AudioSource>();zooFoley.playOnAwake=false;zooFoley.spatialBlend=0;zooFoley.priority=130;}
            zooFoley.Stop();zooFoley.clip=clip;zooFoley.volume=.18f*ZooEffectGain;zooFoley.Play();zooNextWater=Time.realtimeSinceStartup+1.2f;
        }
        private void TickZooAudio(ZooState state,string[] visible)
        {
            var now=Time.realtimeSinceStartup;
            if(visible.Length==0 || applicationPaused){ResetZooAudio();return;}
            foreach(var a in state.animals.Where(a=>visible.Contains(a.species))){
                if(!zooHeard.TryGetValue(a.species,out var sequence)){zooHeard[a.species]=a.sequence;continue;}
                if(sequence==a.sequence)continue;zooHeard[a.species]=a.sequence;
                // Joining or streaming an animal never replays its old call.
                if(a.age>1.5)continue;
                if(a.phase==ZooPhase.Splash){ZooWaterSound();}
                else if(a.phase==ZooPhase.Notice && now>=zooNextCall){ZooPlay(a.species,false);zooNextCall=now+8;}
                else if(a.phase==ZooPhase.Eat){ZooPlay(a.species,true);}
                else if(a.phase==ZooPhase.Browse && now>=zooNextCall){ZooPlay(a.species,false);zooNextCall=now+18;}
            }
            var nearest=state.animals.Where(a=>visible.Contains(a.species) && (a.phase==ZooPhase.Wander || a.phase==ZooPhase.Approach)).OrderBy(a=>Math.Abs(ZooLayout.Point(a).X-cameraX)).FirstOrDefault();
            if(nearest!=null && now>=zooNextStep && (zooFoley==null || !zooFoley.isPlaying)){ZooPlay(nearest.species,true);zooNextStep=now+1.8f;}
            if(zooVoice!=null)zooVoice.volume=.26f*ZooEffectGain;
            if(zooFoley!=null)zooFoley.volume=.18f*ZooEffectGain;
            if(surpriseVoice!=null)surpriseVoice.volume=.1f*ZooEffectGain;
            var wanted=visible.Select(ZooVoicePath).Concat(new[]{"Worlds/Zoo/Audio/water","Worlds/Zoo/Audio/feeding"}).Concat(visible.Contains("elephant")?new[]{"Worlds/Zoo/Audio/visitor-chirp","Worlds/Zoo/Audio/flower-rustle"}:Array.Empty<string>()).ToArray();
            foreach(var path in zooClips.Keys.Where(p=>!wanted.Contains(p)).ToArray()){
                var clip=zooClips[path];
                if(zooVoice!=null && zooVoice.clip==clip){zooVoice.Stop();zooVoice.clip=null;}
                if(surpriseVoice!=null && surpriseVoice.clip==clip){surpriseVoice.Stop();surpriseVoice.clip=null;}
                if(zooFoley!=null && zooFoley.clip==clip){zooFoley.Stop();zooFoley.clip=null;}
                Resources.UnloadAsset(clip);zooClips.Remove(path);
            }
            foreach(var key in zooHeard.Keys.Where(k=>!visible.Contains(k)).ToArray())zooHeard.Remove(key);
        }
        private void ResetZooAudio()
        {
            if(surpriseVoice!=null){surpriseVoice.Stop();surpriseVoice.clip=null;}
            if(zooVoice!=null){zooVoice.Stop();zooVoice.clip=null;}
            if(zooFoley!=null){zooFoley.Stop();zooFoley.clip=null;}
            foreach(var clip in zooClips.Values)if(clip!=null)Resources.UnloadAsset(clip);zooClips.Clear();zooHeard.Clear();
        }
    }
}
