using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private float nextBookAudioObservation;
        private readonly Queue<string> bookAudioEvents=new Queue<string>();
        private readonly float[] bookOutput=new float[256];
        [Serializable] private sealed class BookAudioEvidence
        {
            public string build,utc,title,clip,loadState,nativeSession;
            public int page,sample,clipSamples,sampleRate,listeners;
            public bool requested,playing,voiceEnabled,sourceMuted,listenerPaused,applicationPaused;
            public float sourceVolume,listenerVolume,outputPeak;
            public double dspTime;
            public string[] events;
        }
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern IntPtr LWBookAudioState();
        [DllImport("__Internal")] private static extern void LWFamilyFree(IntPtr text);
#endif
        private void ObserveBookAudio()
        {
            if(Time.unscaledTime<nextBookAudioObservation || !(bookAwaitingSpeech || BookSpeaking || BookEffectPlaying))return;
            nextBookAudioObservation=Time.unscaledTime+1;RecordBookAudio(null);
        }
        private void RecordBookAudio(string phase)
        {
            // Local, bounded diagnostics replace debugger attachment during
            // family play. No microphone, recording, credentials or world data.
            try
            {
                if(phase!=null){if(bookAudioEvents.Count>=24)bookAudioEvents.Dequeue();bookAudioEvents.Enqueue(DateTime.UtcNow.ToString("O")+" "+phase);}
                var native="";
#if UNITY_IOS && !UNITY_EDITOR
                var pointer=LWBookAudioState();if(pointer!=IntPtr.Zero){try{native=Marshal.PtrToStringUTF8(pointer);}finally{LWFamilyFree(pointer);}}
#endif
                var source=BookEffectPlaying && !BookSpeaking?readerEffects:readerVoice;var clip=source?.clip;var peak=0f;
                if(source!=null && source.isPlaying){source.GetOutputData(bookOutput,0);foreach(var value in bookOutput)peak=Mathf.Max(peak,Mathf.Abs(value));}
                var data=new BookAudioEvidence{build=Application.version,utc=DateTime.UtcNow.ToString("O"),title=bookContent?.id??"",page=reader.Page,
                    requested=reader.Playing,playing=source!=null && source.isPlaying,voiceEnabled=Narration!=null && Narration.VoiceEnabled,
                    clip=clip?.name??"",loadState=clip==null?"none":clip.loadState.ToString(),sample=source==null?0:source.timeSamples,clipSamples=clip==null?0:clip.samples,
                    sampleRate=AudioSettings.outputSampleRate,dspTime=AudioSettings.dspTime,sourceVolume=source==null?0:source.volume,sourceMuted=source!=null && source.mute,
                    listenerVolume=AudioListener.volume,listenerPaused=AudioListener.pause,listeners=FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,
                    applicationPaused=applicationPaused,outputPeak=peak,nativeSession=native,events=bookAudioEvents.ToArray()};
                var folder=Path.Combine(Application.persistentDataPath,"BookAudio");Directory.CreateDirectory(folder);
                var path=Path.Combine(folder,"last.json");File.WriteAllText(path+".pending",JsonUtility.ToJson(data,true));
                if(File.Exists(path))File.Replace(path+".pending",path,null);else File.Move(path+".pending",path);
            }
            catch(IOException){ /* Diagnostics never interrupt reading or play. */ }
        }
    }
}
