using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        [Serializable] private struct PlayFrame
        {
            public int frame,collections;
            public double time,frameMs,saveMs,networkMs;
            public bool shared,moving;
            public float x,y;
        }
        [Serializable] private sealed class PlayCapture
        {
            public string build,utc;
            public PlayFrame[] frames;
        }
        // Fixed memory only during play. Export detached plain data after a
        // menu/pause request; profiling must not add its own periodic disk hitch.
        private readonly PlayFrame[] playFrames=new PlayFrame[4096];
        private int playFrameCount,lastMeasuredFrame=-1;
        private Task playExport;
        private string playExportPath;
        private ref PlayFrame CurrentPlayFrame()
        {
            if(lastMeasuredFrame!=Time.frameCount)
            {
                lastMeasuredFrame=Time.frameCount;
                playFrames[playFrameCount%playFrames.Length]=new PlayFrame{frame=Time.frameCount,
                    time=Time.realtimeSinceStartupAsDouble,frameMs=Time.unscaledDeltaTime*1000,
                    collections=GC.CollectionCount(0)};
                playFrameCount++;
            }
            return ref playFrames[(playFrameCount-1)%playFrames.Length];
        }
        public void RecordNetworkWork(long started)
        {CurrentPlayFrame().networkMs+=(Stopwatch.GetTimestamp()-started)*1000.0/Stopwatch.Frequency;}
        private void RecordSaveWork(long started)
        {CurrentPlayFrame().saveMs+=(Stopwatch.GetTimestamp()-started)*1000.0/Stopwatch.Frequency;}
        private void RecordPlayFrame()
        {
            if(!Ready)return;
            ref var sample=ref CurrentPlayFrame();
            sample.shared=Shared;sample.moving=!MenuOpen && (destination.HasValue || stickDirection.sqrMagnitude>.0001f);
            var p=ReadPlayer(Actor);sample.x=p.x;sample.y=p.y;
        }
        public Task ExportPlayPerformance()
        {
            if(playExport!=null && !playExport.IsCompleted)return playExport;
            if(playExportPath==null)playExportPath=Path.Combine(Application.persistentDataPath,"Performance","walking-latest.json");
            var count=Math.Min(playFrameCount,playFrames.Length);var copy=new PlayFrame[count];
            for(var i=0;i<count;i++)copy[i]=playFrames[(playFrameCount-count+i)%playFrames.Length];
            var capture=new PlayCapture{build=Application.version,utc=DateTime.UtcNow.ToString("O"),frames=copy};
            var path=playExportPath;
            return playExport=Task.Run(()=>
            {
                // JsonUtility supports background threads for detached plain
                // objects. This disposable trace never writes a game save.
                try{Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(capture));}
                catch(IOException){}catch(UnauthorizedAccessException){}
            });
        }
    }
}
