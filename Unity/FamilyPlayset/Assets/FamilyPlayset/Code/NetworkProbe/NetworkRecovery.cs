using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LittleWeeps.Adapters;
using LittleWeeps.Core;
using Unity.Netcode;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    public sealed partial class NetworkProbe
    {
        private const string RecoveryMessage="littleweeps.recovery.chunk.v1", RecoveryAckMessage="littleweeps.recovery.ack.v1";
        private sealed class RecoveryPeer
        {
            public byte[] bytes;
            public string transfer, hash, durableHash;
            public int next, sent;
            public long checkpoint, revision, durableCheckpoint, durableRevision;
            public double expires, nextOffer;
            public bool awaiting;
        }
        [Serializable] private sealed class RecoveryPeerEvidence
        { public string profile, hash; public long checkpoint, revision; public int sent; }
        [Serializable] private sealed class RecoveryEvidence
        {
            public string status, epoch, hash;
            public int receivedChunks, bytes, receipts, timers;
            public long checkpoint, revision;
            public double writeMilliseconds;
            public RecoveryPeerEvidence[] peers;
        }
        private readonly Dictionary<ulong,RecoveryPeer> recoveryPeers=new Dictionary<ulong,RecoveryPeer>();
        private readonly RecoveryTransfer recoveryTransfer=new RecoveryTransfer();
        private RecoveryRecord recoveryCommitted;
        private RecoveryReplica recoveryReplica;
        private byte[] recoveryBytes;
        private long recoverySerial;
        private ulong recoveryLastPeer;
        private double nextRecoveryChunk;
        private bool recoveryInitialized;
        private int recoveryChunks;
        private string recoveryCompletedTransfer;

        private void RegisterRecoveryMessages()
        {
            network.CustomMessagingManager.RegisterNamedMessageHandler(RecoveryMessage,ReceiveRecovery);
            network.CustomMessagingManager.RegisterNamedMessageHandler(RecoveryAckMessage,ReceiveRecoveryAck);
        }
        private void ResetRecoveryTransfer(){recoveryTransfer.Reset();recoveryCompletedTransfer=null;}
        private RecoveryReplica OpenRecoveryReplica()
        {
            if(!recoveryInitialized)
            {
                recoveryInitialized=true;
                recoveryReplica=new RecoveryReplica(Path.Combine(root,"client-recovery",config.profile,"world.save"),
                    pairing?.familyId??config.runId,pairing?.authorityId??config.runId,config.runId,text=>JsonUtility.FromJson<RecoveryRecord>(text));
            }
            if(recoveryReplica==null)throw new IOException("Client recovery store needs attention.");
            return recoveryReplica;
        }
        private void CaptureRecovery(SoloSnapshot snapshot)
        {
            recoveryCommitted=new RecoveryRecord{version=1,protocol=Protocol,content=Content,family=pairing?.familyId??config.runId,
                authority=pairing?.authorityId??config.runId,world=config.runId,epoch=epoch,checkpoint=++recoverySerial,snapshot=snapshot};
            recoveryBytes=null;
        }
        private void TickRecovery(double now)
        {
            if(config.role!="server" || recoveryPeers.Count==0 || recoveryCommitted==null || now<nextRecoveryChunk)return;
            // At most one 3 KiB chunk per 50 ms globally, round-robin across
            // four peers, with one acknowledged in-flight chunk per peer.
            // Gameplay/motion ticks run first; never queue a whole large save.
            nextRecoveryChunk=now+.05;
            try
            {
                var peers=recoveryPeers.Keys.Where(id=>network.ConnectedClientsIds.Contains(id)).OrderBy(id=>id<=recoveryLastPeer).ThenBy(id=>id).ToArray();
                foreach(var id in peers)
                {
                    var peer=recoveryPeers[id];
                    if(peer.bytes!=null && now>peer.expires){peer.bytes=null;peer.awaiting=false;peer.nextOffer=now+5;}
                    if(peer.bytes==null)
                    {
                        if(now<peer.nextOffer || peer.durableCheckpoint==recoveryCommitted.checkpoint)continue;
                        if(recoveryBytes==null)recoveryBytes=Utf8.GetBytes(JsonUtility.ToJson(recoveryCommitted));
                        if(recoveryBytes.Length>RecoveryTransfer.MaxBytes)throw new InvalidDataException("Recovery budget exceeded.");
                        peer.bytes=recoveryBytes;peer.hash=RecoveryTransfer.Hash(peer.bytes);peer.transfer=Guid.NewGuid().ToString("N");
                        peer.checkpoint=recoveryCommitted.checkpoint;peer.revision=recoveryCommitted.snapshot.revision;
                        peer.next=0;peer.awaiting=false;peer.expires=now+RecoveryTransfer.Timeout;
                    }
                    if(peer.awaiting)continue;
                    Send(RecoveryMessage,id,RecoveryTransfer.Chunk(peer.bytes,peer.transfer,epoch,peer.next));
                    peer.awaiting=true;peer.sent++;recoveryLastPeer=id;break;
                }
            }
            catch(Exception)
            {
                nextRecoveryChunk=now+5;
                WriteJson(Path.Combine(output,"recovery-evidence.json"),new RecoveryEvidence{status="transfer-unavailable"});
            }
        }
        private void ReceiveRecovery(ulong sender,FastBufferReader reader)
        {
            if(config.role!="client" || sender!=NetworkManager.ServerClientId || !ConnectedToServer || applicationPaused || retryPending || localOnly || Latest?.recovery!=1)return;
            try
            {
                var part=JsonUtility.FromJson<RecoveryChunk>(Read(reader));
                var bytes=recoveryTransfer.Add(part,epoch,Time.realtimeSinceStartupAsDouble);recoveryChunks++;
                if(bytes!=null && recoveryCompletedTransfer!=part.transfer)
                {
                    var timer=System.Diagnostics.Stopwatch.StartNew();
                    var record=OpenRecoveryReplica().Commit(bytes,epoch,Latest.view.worldId,Latest.view.players.Select(p=>p.id).ToArray());
                    timer.Stop();recoveryCompletedTransfer=part.transfer;
                    WriteJson(Path.Combine(output,"recovery-evidence.json"),new RecoveryEvidence{status="durable",epoch=epoch,hash=part.hash,
                        checkpoint=record.checkpoint,revision=record.snapshot.revision,bytes=bytes.Length,receivedChunks=recoveryChunks,
                        receipts=record.snapshot.receipts.Length,timers=record.snapshot.idleTimers.Length,writeMilliseconds=timer.Elapsed.TotalMilliseconds});
                }
                // The final acknowledgement is sent only after atomic storage
                // and read-back validation, never merely on chunk receipt.
                Send(RecoveryAckMessage,NetworkManager.ServerClientId,new RecoveryAck{transfer=part.transfer,epoch=part.epoch,hash=part.hash,
                    next=recoveryTransfer.Next,durable=bytes!=null && recoveryCompletedTransfer==part.transfer});
            }
            catch(Exception)
            {
                ResetRecoveryTransfer();
                WriteJson(Path.Combine(output,"recovery-evidence.json"),new RecoveryEvidence{status="checkpoint-rejected",receivedChunks=recoveryChunks});
                // Recovery unavailability cannot blank the view or the solo
                // draft, nor take down otherwise healthy family play.
            }
        }
        private void ReceiveRecoveryAck(ulong sender,FastBufferReader reader)
        {
            if(config.role!="server" || !recoveryPeers.TryGetValue(sender,out var peer) || peer.bytes==null)return;
            try
            {
                var ack=JsonUtility.FromJson<RecoveryAck>(Read(reader));
                if(ack==null || ack.epoch!=epoch || ack.transfer!=peer.transfer || ack.hash!=peer.hash || !peer.awaiting || ack.next!=peer.next+1)return;
                var final=ack.next==RecoveryTransfer.Count(peer.bytes.Length);
                if(ack.durable!=final)return;
                peer.next=ack.next;peer.awaiting=false;
                if(!final)return;
                peer.durableCheckpoint=peer.checkpoint;peer.durableRevision=peer.revision;peer.durableHash=peer.hash;peer.bytes=null;
                peer.nextOffer=Time.realtimeSinceStartupAsDouble+5;
                WriteJson(Path.Combine(output,"recovery-evidence.json"),new RecoveryEvidence{status="replicated",epoch=epoch,
                    peers=recoveryPeers.Select(p=>new RecoveryPeerEvidence{profile=session.TryPlayer(p.Key,out var profile)?profile:"",
                        checkpoint=p.Value.durableCheckpoint,revision=p.Value.durableRevision,hash=p.Value.durableHash,sent=p.Value.sent}).ToArray()});
            }
            catch(Exception){ /* Invalid acknowledgement grants no recovery coverage. */ }
        }
    }
}
