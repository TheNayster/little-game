using System;
using LittleWeeps.Adapters;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly BackgroundSave localSave=new BackgroundSave();
        private void SaveFailed(Exception error)
        {
            dirty=true;
            if(saveLabel!=null)saveLabel.text="Couldn't save yet. Please ask a grown-up.";
            Debug.LogWarning("Solo checkpoint: "+error.Message);
        }
        private void SavedLabel()
        {
            if(saveLabel!=null)saveLabel.text=continuation!=null?"Your adventure is saved on this device":
                "Saved on this device · solo prototype · "+Application.version;
        }
        private void FinishBackgroundSave(bool wait)
        {
            if(!localSave.Pending || !localSave.Complete(wait,out var error))return;
            if(error!=null)SaveFailed(error);else if(!dirty)SavedLabel();
        }
        private void SaveDuringPlay()
        {
            if(!dirty || localSave.Pending || World==null || store==null)return;
            var started=System.Diagnostics.Stopwatch.GetTimestamp();
            try
            {
                // Snapshot() copies plain data. The worker never reads the live
                // world, UI, preferences or a mutable continuation record.
                var snapshot=World.Snapshot();var targetStore=store;var library=adventures;
                var record=continuation==null?null:new ContinuationRecord{
                    version=continuation.version,id=continuation.id,actor=continuation.actor,
                    createdUtcTicks=continuation.createdUtcTicks,basis=continuation.basis,snapshot=snapshot};
                // The basis is immutable after creation. The detached record's
                // snapshot is owned exclusively by this commit.
                localSave.Start(()=>{if(record!=null)library.Save(record,snapshot);else targetStore.Save(JsonUtility.ToJson(snapshot));});
                dirty=false;
                if(saveLabel!=null)saveLabel.text="Saving on this device…";
            }
            catch(Exception e){SaveFailed(e);}
            finally{RecordSaveWork(started);}
        }
    }
}
