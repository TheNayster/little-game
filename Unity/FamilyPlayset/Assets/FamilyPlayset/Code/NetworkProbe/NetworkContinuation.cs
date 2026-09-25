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
        [Serializable] private sealed class InterruptedActions
        {public int version=1;public string family,authority,world,profile,epoch;public SoloCommand[] commands;}
        public void RememberInterrupted(SoloCommand[] commands)
        {
            if(pairing==null || commands.Length==0)return;
            try
            {
                // These are unresolved intentions, not accepted events. Retain
                // their original IDs for future reconciliation; never replay.
                var value=new InterruptedActions{family=pairing.familyId,authority=pairing.authorityId,world=config.runId,
                    profile=config.profile,epoch=epoch,commands=commands};
                var payload=JsonUtility.ToJson(value);
                var path=Path.Combine(root,"client-adventures",config.profile,"interrupted",Guid.NewGuid().ToString("N")+".save");
                var archive=new CheckpointStore(path,text=>text==payload);archive.Save(payload);
                if(archive.Load().Payload!=payload)throw new IOException("Interrupted actions not verified.");
            }
            catch(Exception e){Debug.LogWarning("Interrupted action archive unavailable: "+e.Message);}
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
            if(localOnly || reconnectBlocked || familyScreen.AdventureId!="" || !familyScreen.CanChangeSession || familyScreen.MenuOpen || Time.realtimeSinceStartupAsDouble<nextContinuationAttempt)return;
            nextContinuationAttempt=Time.realtimeSinceStartupAsDouble+5;
            try
            {
                var basis=OpenRecoveryReplica().Last;
                // An old-server rendered view is not a complete checkpoint. A
                // new install with none keeps ordinary solo play available.
                if(basis==null){WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="no-checkpoint"});return;}
                if(familyScreen.TryContinue(basis))
                {
                    presentationStarted=true;
                    WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="local-continuation",branch=familyScreen.AdventureId,
                        baseEpoch=basis.epoch,baseCheckpoint=basis.checkpoint,baseRevision=basis.snapshot.revision});
                    TraceConnection("local-continuation",0,"verified-checkpoint");
                }
            }
            catch(Exception e)
            {Debug.LogWarning("Local recovery unavailable: "+e.Message);WriteJson(Path.Combine(output,"continuation-evidence.json"),new ContinuationEvidence{status="checkpoint-blocked"});}
        }
    }
}
