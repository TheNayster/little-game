# Planned host handoff model

> Historical experiment. Device hosting (G4/AUTO-02) was retired by the user on September 25. These tests are retained for evidence and possible dedicated-server recovery reference; they are not the active implementation queue. See [current decisions](../../docs/current-decisions.md).

This executable is a **design experiment**, outside the shipping Unity project.
It models one cooperative handoff between an existing writer and a prepared
replica. It does not start a server or alter any family data.

Run from the game root, choosing a new ignored evidence directory:

```powershell
dotnet run --project Tools/HostingModel.Tests/HostingModel.Tests.csproj --configuration Release -- LocalData/Verification/hosting-model-next
```

The model assumes authenticated message senders, one serialized controller per
device, a complete validated final checkpoint, and atomic persistent journals.
`ModelDisk` simulates those journal boundaries using serialized records, including
errors before commit and after a commit whose acknowledgment was lost. It is not
an actual filesystem durability or cryptographic test. The checkpoint payload is
opaque fixture data: completeness is a precondition here, not a replacement for
the real `RecoveryRecord.Validate` and full-world restore checks.

The safety invariant is **at most one writer for this planned transfer**. The
source first freezes, the successor stages the final checkpoint, and the source
durably retires before issuing a grant bound to that exact checkpoint/recipient.
The successor persists the grant and restores its world before serving. Lost
messages may prevent progress until retransmitted. Retired sources cannot resume
writing after a restart. A write error disables the local writer until its journal
is reloaded, covering the ambiguous-commit case.

This is intentionally NOT hard-loss election or partition consensus. No timeout
turns a replica into the old shared authority. Private solo continuation already
exists in the game; advertising a new mobile branch, choosing it after discovery,
and automatic reunion remain the subsequent G4 work. Multiple successive
transfers, authenticated cancel/reprepare, source epochs chained across branches,
real disk/key storage, network retries/backpressure, payload validation and
foreground background adapters must be implemented and tested before production
integration.

See the project's September 25 hosting/animation preparation report for sources,
the native integration sequence, and the distinction between an existing stable
session, orderly transfer, and a partitioned local branch.
