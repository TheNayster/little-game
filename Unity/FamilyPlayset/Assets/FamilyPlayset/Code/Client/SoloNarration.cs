using System.Collections.Generic;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed class SoloNarration : MonoBehaviour
    {
        private readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        private AudioSource speaker;
        public bool Ready {get;private set;}
        public void Initialize(bool muted)
        {
            speaker=gameObject.AddComponent<AudioSource>();speaker.playOnAwake=false;speaker.spatialBlend=0;speaker.volume=muted?0:.55f;
            Ready=true;
            foreach(var id in new[]{"garden","cleanup","freeplay","tapwalk","joystick"})
            {
                var clip=Resources.Load<AudioClip>("SoloNarration/en-US/"+id);
                if(clip==null || clip.samples<=0 || clip.length<.5f){Ready=false;continue;}
                clips.Add(id,clip);
            }
        }
        public void Speak(string id){if(speaker==null)return;speaker.Stop();if(clips.TryGetValue(id,out var clip)){speaker.clip=clip;speaker.Play();}}
        public void Stop(){if(speaker!=null)speaker.Stop();}
        private void OnApplicationPause(bool paused){if(paused)Stop();}
        private void OnApplicationFocus(bool focused){if(!focused)Stop();}
    }
}
