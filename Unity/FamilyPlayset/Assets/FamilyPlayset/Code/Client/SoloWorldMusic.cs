using System;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private WorldMusicPlayer worldMusic;
        public string WorldMusicTrack=>worldMusic==null?"":worldMusic.Track;
        public bool WorldMusicPlaying=>worldMusic!=null && worldMusic.Playing;
        public float WorldMusicVolume=>worldMusic==null?0:worldMusic.Volume;
        public int WorldMusicClipCount=>worldMusic==null?0:worldMusic.ClipCount;
        public int WorldMusicSample=>worldMusic==null?0:worldMusic.Sample;
        public float WorldMusicSignal=>worldMusic==null?0:worldMusic.Signal;
        private float ForegroundDucking=>TreasureTonePlaying?.08f:Narration!=null && Narration.Speaking ? .22f : BookDucking;

        private void TickWorldMusic()
        {
            if(worldMusic==null || !Ready)return;
            var area=CurrentArea;
            var track=area=="garden"?(ReadPlayer(Actor).x>150?"yard":"home"):
                area==TreasureHunt.Zone?"treasure":area==KingdomAdventure.Zone || area==DaycareVet.Zone?"daycare":
                area==DinosaurRides.Area?"yard":
                ZooLayout.Area(area)?(area==ZooCatalog.Aquarium?"creek":"yard"):
                area=="park" || area=="creek" || area=="beach" || area=="daycare"?area:"home";
            // Keep a margin around the veranda boundary so tiny steps don't
            // repeatedly restart both scores.
            if(area=="garden" && Mathf.Abs(ReadPlayer(Actor).x-150)<150 &&
                (worldMusic.Track=="yard" || worldMusic.Track=="home"))track=worldMusic.Track;
            var quiet=SecretRooms.Index(area)>=0;
            var gain=musicMuted || quiet?0:.42f*ForegroundDucking;
            if(HomeMusicPlaying)gain*=.08f;
            if(HomeRooms.Property(area) && area!="garden")gain*=.7f;
            worldMusic.Tick(track,gain,applicationPaused);
        }
    }

    // Two sources, at most two retained clips and one in-flight request.
    // Travel and preferences stay local; no world/save/network mutation.
    internal sealed class WorldMusicPlayer : MonoBehaviour
    {
        private readonly AudioSource[] sources=new AudioSource[2];
        private readonly string[] ids={"",""};
        private readonly float[] mix=new float[2];
        private ResourceRequest pending;
        private string pendingId="",desired="";
        private int active;
        private bool paused;
        private float gain;
        private readonly float[] meter=new float[256];
        public string Track=>ids[active];
        public bool Playing=>sources[active]!=null && sources[active].isPlaying && Volume>0;
        public float Volume=>sources[active]==null?0:sources[active].volume;
        public int ClipCount=>(sources[0]!=null && sources[0].clip!=null?1:0)+(sources[1]!=null && sources[1].clip!=null?1:0);
        public int Sample=>sources[active]==null?0:sources[active].timeSamples;
        public float Signal
        {
            get
            {
                if(!Playing)return 0;
                sources[active].GetOutputData(meter,0);
                var sum=0f;foreach(var value in meter)sum+=value*value;
                return Mathf.Sqrt(sum/meter.Length);
            }
        }
        private void Awake()
        {
            for(var i=0;i<2;i++)
            {
                sources[i]=gameObject.AddComponent<AudioSource>();sources[i].playOnAwake=false;
                sources[i].loop=true;sources[i].spatialBlend=0;sources[i].volume=0;sources[i].priority=180;
            }
        }
        public void Suspend(bool value)
        {
            if(paused==value)return;paused=value;
            foreach(var source in sources)
            {
                if(source==null)continue;
                if(value)source.Pause();else if(source.clip!=null)source.UnPause();
            }
        }
        public void Tick(string track,float targetGain,bool pause)
        {
            desired=track;Suspend(pause);if(paused)return;
            // Finish the existing transition before admitting another clip.
            // Rapid destination changes coalesce to the most recent request.
            if(pending!=null && pending.isDone)
            {
                var clip=pending.asset as AudioClip;pending=null;
                if(clip!=null)
                {
                    if(pendingId!=desired)Resources.UnloadAsset(clip);
                    else
                    {
                        active=1-active;Release(active);
                        sources[active].clip=clip;ids[active]=pendingId;mix[active]=0;sources[active].Play();
                    }
                }
            }
            var dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            // Mute is immediate; speech ducks quickly and returns gently.
            gain=targetGain==0?0:Mathf.MoveTowards(gain,targetGain,dt*(targetGain<gain?1.6f:.35f));
            for(var i=0;i<2;i++)
            {
                mix[i]=Mathf.MoveTowards(mix[i],i==active?1:0,dt/1.8f);
                sources[i].volume=gain*mix[i];
                if(i!=active && mix[i]<=0)Release(i);
            }
            if(pending==null && Track!=desired && sources[1-active].clip==null)
            {pendingId=desired;pending=Resources.LoadAsync<AudioClip>("WorldMusic/"+desired);}
        }
        private void Release(int index)
        {
            var source=sources[index];if(source==null)return;
            var clip=source.clip;source.Stop();source.clip=null;ids[index]="";mix[index]=0;
            if(clip!=null)Resources.UnloadAsset(clip);
        }
        private void OnDestroy()
        {
            Release(0);Release(1);
            // A request can finish after the screen is replaced on reconnect.
            // Release its result without starting audio on the abandoned view.
            if(pending!=null)
            {
                var request=pending;
                if(request.isDone){if(request.asset!=null)Resources.UnloadAsset(request.asset);}
                else request.completed+=_=>{if(request.asset!=null)Resources.UnloadAsset(request.asset);};
            }
            foreach(var source in sources)if(source!=null)Destroy(source);
        }
    }
}
