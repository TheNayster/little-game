using System;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    // One outstanding operation per client. An explicit stale rejection permits
    // a new attempt; an unknown outcome never permits a new operation identity.
    public sealed class GardenCommandQueue
    {
        private sealed class Entry { public SoloCommand command; public Action<SoloResult> done; public int retries; }
        private readonly List<Entry> queue=new List<Entry>();
        private Entry pending;
        public bool Busy=>pending!=null || queue.Count>0;
        public string PendingId=>pending?.command.requestId;
        public bool Enqueue(SoloCommand command,Action<SoloResult> done)
        {
            if(command==null)throw new ArgumentNullException(nameof(command));
            // Keep only the newest unsent walking destination. Never replace an
            // in-flight operation, pickup, drop, or cancellation.
            if(command.action==SoloAction.Move)
            {
                var previous=queue.FindIndex(e=>e.command.action==SoloAction.Move);
                if(previous>=0){var old=queue[previous];queue.RemoveAt(previous);old.done?.Invoke(new SoloResult(false,"superseded",0));}
            }
            if(queue.Count>=16 && command.action!=SoloAction.CancelGrab){done?.Invoke(new SoloResult(false,"busy",0));return false;}
            var entry=new Entry{command=command,done=done};
            if(command.action==SoloAction.CancelGrab)queue.Insert(0,entry);else queue.Add(entry);
            return true;
        }
        public SoloCommand Take(long revision)
        {
            if(pending!=null || queue.Count==0)return null;
            pending=queue[0];queue.RemoveAt(0);pending.command.expectedRevision=revision;return pending.command;
        }
        public bool Complete(string requestId,SoloResult result)
        {
            if(pending==null || pending.command.requestId!=requestId)return false;
            var entry=pending;pending=null;
            if(!result.Accepted && result.Outcome=="stale-revision" && (entry.command.action==SoloAction.CancelGrab || entry.retries++<8))
            {queue.Insert(0,entry);return true;}
            entry.done?.Invoke(result);return true;
        }
        public void Disconnect()
        {
            var entries=new List<Entry>(queue);queue.Clear();
            if(pending!=null){entries.Insert(0,pending);pending=null;}
            foreach(var entry in entries)entry.done?.Invoke(new SoloResult(false,"disconnected",0));
        }
    }
}
