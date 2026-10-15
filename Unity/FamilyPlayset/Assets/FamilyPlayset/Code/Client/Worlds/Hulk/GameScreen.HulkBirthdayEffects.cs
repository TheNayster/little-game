using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private readonly Dictionary<string,AudioSource> birthdayEffects=new Dictionary<string,AudioSource>();
        private readonly Dictionary<string,string> birthdayEffectSeen=new Dictionary<string,string>();
        private bool birthdayEffectsReady;
        private void BirthdayEffect(string key)
        {
            if(applicationPaused || musicMuted || !birthdayEffects.TryGetValue(key,out var source))return;
            source.clip=BirthdayArt.Audio(key);source.volume=.4f*ForegroundDucking;source.Play();
        }
        private void TickBirthdayEffects(bool visible)
        {
            if(!visible || applicationPaused){foreach(var source in birthdayEffects.Values)source.Stop();birthdayEffectSeen.Clear();birthdayEffectsReady=false;return;}
            if(birthdayEffects.Count==0)foreach(var entry in BirthdayArt.Data.effects??Array.Empty<BirthdayArt.AudioEntry>()){
                var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;birthdayEffects.Add(entry.key,source);
            }
            bool New(string key,string value){bool changed=birthdayEffectsReady && birthdayEffectSeen.TryGetValue(key,out var old) && old!=value;birthdayEffectSeen[key]=value;return changed;}
            var b=Birthday;
            foreach(var m in b.members){
                if(New(m.actor+"/station",m.station) && m.actor==Actor){
                    if(m.station=="zipline")BirthdayEffect("zip-mount");
                    if(m.station=="web")BirthdayEffect("web");
                    if(m.station=="throw")BirthdayEffect("throw");
                }
                if(New(m.actor+"/landing",m.landings.ToString()) && m.actor==Actor)BirthdayEffect("jump-impact");
                if(New(m.actor+"/route",m.route.ToString()) && m.actor==Actor && m.route<0)BirthdayEffect("zip-dismount");
            }
            if(New("gift",b.giftAt.ToString()) && b.giftAt>=0)BirthdayEffect("gift-open");
            if(New("stickers",b.stickers.Length.ToString()) && b.stickers.Length>0)BirthdayEffect("cake-sticker");
            if(New("bubbles",b.bubblesPopped.ToString()))BirthdayEffect("bubble-pop");
            if(New("rescues",b.troublesStopped.ToString()))BirthdayEffect("jump-impact");
            if(birthdayEffects.TryGetValue("zip-loop",out var loop)){
                bool ride=BirthdayOwn?.station=="zipline" && !applicationPaused && !musicMuted;
                if(ride){loop.clip=BirthdayArt.Audio("zip-loop");loop.loop=true;loop.volume=.22f*ForegroundDucking;if(!loop.isPlaying)loop.Play();}
                else if(loop.isPlaying)loop.Stop();
            }
            birthdayEffectsReady=true;
        }
        private void ResetBirthdayEffects(){foreach(var source in birthdayEffects.Values)Destroy(source);birthdayEffects.Clear();birthdayEffectSeen.Clear();birthdayEffectsReady=false;}
    }
}
