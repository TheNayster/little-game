using System;
using System.IO;
using LittleWeeps.Adapters;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    public sealed partial class NetworkProbe
    {
        private readonly OutageClock outageClock=new OutageClock();
        private string requestedAdventure;
        private double nextContinuationAttempt;
        private LocalViewOrigin localStart;
        private void RememberVisibleLocalStart()
        {
            if(localStart!=null || familyScreen==null || !familyScreen.Shared || Latest==null)return;
            var snapshot=familyScreen.CaptureVisibleLocalStart();
            if(snapshot==null)return;
            localStart=new LocalViewOrigin{family=pairing.familyId,authority=pairing.authorityId,
                world=config.runId,epoch=Latest.epoch,snapshot=snapshot};
            nextContinuationAttempt=0;
        }
        [Serializable] private sealed class InterruptedActions
        {public int version=1;public string family,authority,world,profile,epoch;public SoloCommand[] commands;}
        private sealed class PendingArchive {public string path,payload;}
        private readonly System.Collections.Generic.Queue<PendingArchive> interruptedArchives=new System.Collections.Generic.Queue<PendingArchive>();
        private double nextArchiveAttempt;
        public int PendingInterruptedArchives=>interruptedArchives.Count;
        public void RememberInterrupted(SoloCommand[] commands)
        {
            if(pairing==null || commands.Length==0)return;
            // Serialize before the live command queue is cleared. Retrying uses
            // the same file and request IDs; this never resubmits game actions.
            var value=new InterruptedActions{family=pairing.familyId,authority=pairing.authorityId,world=config.runId,
                profile=config.profile,epoch=epoch,commands=commands};
            interruptedArchives.Enqueue(new PendingArchive{
                path=Path.Combine(root,"client-adventures",config.profile,"interrupted",Guid.NewGuid().ToString("N")+".save"),
                payload=JsonUtility.ToJson(value)});
            nextArchiveAttempt=0;TickInterruptedArchives();
        }
        private void TickInterruptedArchives()
        {
            if(interruptedArchives.Count==0 || Time.realtimeSinceStartupAsDouble<nextArchiveAttempt)return;
            nextArchiveAttempt=Time.realtimeSinceStartupAsDouble+2;
            try
            {
                var pending=interruptedArchives.Peek();
                var archive=new CheckpointStore(pending.path,text=>text==pending.payload);
                archive.Save(pending.payload);
                if(archive.Load().Payload!=pending.payload)throw new IOException("Interrupted actions not verified.");
                interruptedArchives.Dequeue();
            }
            catch(Exception e){Debug.LogWarning("Interrupted actions retained in memory; archive will retry: "+e.Message);}
        }
        [Serializable] private sealed class ContinuationEvidence
        {public string status,branch,baseEpoch;public long baseCheckpoint,baseRevision;}
        private void ConfigureContinuation()
        {
            try
            {
                var library=new ContinuationLibrary(Path.Combine(root,"client-adventures",config.profile),pairing.familyId,pairing.authorityId,config.runId,config.profile,
                    text=>JsonUtility.FromJson<ContinuationRecord>(text),value=>JsonUtility.ToJson(value),
                    text=>JsonUtility.FromJson<ContinuationSelection>(text),value=>JsonUtility.ToJson(value));
                familyScreen.ConfigureAdventures(library,id=>requestedAdventure=id);
            }
            catch(Exception e)
            {Debug.LogWarning("Saved adventures unavailable: "+e.Message);WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="store-blocked"});}
        }
        private void TryLocalContinuation()
        {
            if(localOnly || reconnectBlocked || familyScreen.AdventureId!="" || !familyScreen.CanChangeSession ||
                (familyScreen.MenuOpen && !familyScreen.RecoveringDisconnected) || Time.realtimeSinceStartupAsDouble<nextContinuationAttempt)return;
            nextContinuationAttempt=Time.realtimeSinceStartupAsDouble+5;
            try
            {
                // A private continuation starts from the last received/displayed
                // view. It never requires or impersonates a host-recovery replica.
                if(localStart==null)
                {
                    var returned=familyScreen.RecoveringDisconnected && familyScreen.TryReturnToLocal();
                    WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status=returned?"solo-without-view":"no-view"});
                    return;
                }
                if(familyScreen.TryContinue(localStart))
                {
                    presentationStarted=true;
                    WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="local-continuation",branch=familyScreen.AdventureId,
                        baseEpoch=localStart.epoch,baseCheckpoint=0,baseRevision=localStart.snapshot.revision});
                    TraceConnection("local-continuation",0,"private-visible-state");
                }
            }
            catch(Exception e)
            {Debug.LogWarning("Local recovery unavailable: "+e.Message);WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="checkpoint-blocked"});}
        }
    }
}
