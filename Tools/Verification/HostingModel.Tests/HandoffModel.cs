using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LittleWeeps.HostingModel;

// Executable protocol design, deliberately outside Assets. This is not the
// shipping host controller, a network transport, or a consensus implementation.
public enum Phase { Replica, Serving, Frozen, Staged, Retired }
public enum WriteFault { None, BeforeCommit, AfterCommit }
public sealed record Checkpoint(string Family, string World, string Epoch,
    long Revision, bool CompleteAuthorityState, string Payload, string Digest)
{
    public static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public bool Valid => CompleteAuthorityState && Revision >= 0 && Payload.Length > 0 && Digest == Hash(Payload);
}
public sealed record Transfer(string Id, string Source, string Target, string NextEpoch, Checkpoint State);
public sealed record Journal(string Device, string Family, string World, string Epoch, Phase Phase,
    Transfer? Transfer = null);

// Simulates the required atomic durable-write contract, including the ambiguous
// "committed but caller saw an error" boundary. Real disk/Keychain is future work.
public sealed class ModelDisk
{
    public string Bytes { get; private set; }
    public WriteFault NextFault { get; set; }
    public ModelDisk(Journal initial) { Bytes = JsonSerializer.Serialize(initial); }
    public Journal Read() => JsonSerializer.Deserialize<Journal>(Bytes)!;
    public void Write(Journal next)
    {
        var fault = NextFault; NextFault = WriteFault.None;
        if (fault == WriteFault.BeforeCommit) throw new IOException("Injected failure before atomic commit");
        Bytes = JsonSerializer.Serialize(next);
        if (fault == WriteFault.AfterCommit) throw new IOException("Injected lost durable-write acknowledgment");
    }
}

public sealed class HandoffNode
{
    public ModelDisk Disk { get; }
    public Journal State { get; private set; }
    public bool Faulted { get; private set; }
    public bool Foreground { get; set; } = true;
    public bool Restored { get; private set; }
    public bool CanWrite => !Faulted && Foreground && Restored && State.Phase == Phase.Serving;
    public HandoffNode(ModelDisk disk) { Disk = disk; State = disk.Read(); }
    public HandoffNode Restart() => new(Disk);
    private void Save(Journal next)
    {
        try { Disk.Write(next); State = next; }
        catch (IOException) { Faulted = true; Restored = false; throw; }
    }
    public void RestoreWorld(bool succeeded)
    {
        Restored = succeeded && !Faulted && State.Phase == Phase.Serving;
    }
    private bool Matches(Transfer t) => t.State.Valid && t.State.Family == State.Family && t.State.World == State.World
        && t.Source != t.Target && !string.IsNullOrWhiteSpace(t.Id) && !string.IsNullOrWhiteSpace(t.NextEpoch)
        && t.NextEpoch != t.State.Epoch;
    private bool Usable => !Faulted && Foreground;
    public Transfer Prepare(string target, Checkpoint frozenFinalState, string id, string nextEpoch)
    {
        if (!CanWrite) throw new InvalidOperationException("Only the current restored foreground writer can prepare");
        var t = new Transfer(id, State.Device, target, nextEpoch, frozenFinalState);
        if (!Matches(t) || string.IsNullOrWhiteSpace(target) || frozenFinalState.Epoch != State.Epoch)
            throw new InvalidOperationException("Wrong final checkpoint or transfer identity");
        // Production adapter must freeze gameplay mutations BEFORE obtaining
        // this final state. The model treats Prepare as one serialized event.
        Save(State with { Phase = Phase.Frozen, Transfer = t }); return t;
    }
    public bool Stage(Transfer t, string authenticatedSource)
    {
        if (!Usable || !Matches(t) || authenticatedSource != t.Source || t.Target != State.Device
            || State.Epoch != t.State.Epoch || (State.Phase != Phase.Replica && State.Phase != Phase.Staged)) return false;
        // An already staged transfer cannot be silently replaced. An explicit
        // authenticated cancel/reprepare exchange is outside this first model.
        if (State.Phase == Phase.Staged) return State.Transfer == t;
        Save(State with { Phase = Phase.Staged, Transfer = t }); return true;
    }
    public Transfer? Ready() => Usable && State.Phase == Phase.Staged ? State.Transfer : null;
    public Transfer? FenceAndGrant(Transfer ready, string authenticatedTarget)
    {
        if (!Usable || State.Phase != Phase.Frozen || State.Transfer != ready || authenticatedTarget != ready.Target) return null;
        // No grant leaves this method until the old writer's retirement is
        // durable. A failed/ambiguous write leaves this process unable to write.
        Save(State with { Phase = Phase.Retired }); Restored = false; return ready;
    }
    public Transfer? RetryGrant() => Usable && State.Phase == Phase.Retired ? State.Transfer : null;
    public bool AcceptGrant(Transfer grant, string authenticatedSource)
    {
        if (!Usable || !Matches(grant) || authenticatedSource != grant.Source || grant.Target != State.Device
            || State.Transfer != grant) return false;
        if (State.Phase == Phase.Serving) return State.Epoch == grant.NextEpoch;
        if (State.Phase != Phase.Staged || State.Epoch != grant.State.Epoch) return false;
        Save(State with { Phase = Phase.Serving, Epoch = grant.NextEpoch });
        Restored = false; return true;
    }
    public bool CancelBeforeGrant()
    {
        if (!Usable || State.Phase != Phase.Frozen) return false;
        Save(State with { Phase = Phase.Serving, Transfer = null }); return true;
    }
}
