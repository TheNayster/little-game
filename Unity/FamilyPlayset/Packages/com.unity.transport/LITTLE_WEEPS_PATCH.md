# Little Weeps local transport patch 1

Base: Unity Transport **2.7.4**, registry revision `4ab5f764fea8cb22683a635175bcb13fcd0b23de`. All original notices and licenses remain. This embedded project package overrides the registry dependency of the same name; it is not an official Unity release or a change to the editor installation.

Modified vendor file: `Runtime/UDPNetworkInterface.cs` only. On a completed receive that failed, contained no data or exceeded the receive capacity, return its acquired buffer before discarding it. Successful datagrams keep the existing queue/clear ownership path; unfinished receives keep their buffers. The socket recreation and error policy are unchanged.

Why: `ScheduleAllReceives` acquires pool slots. Discarded completions in the unpatched code neither enqueue their slots for later clearing nor release them. Enough completions exhaust the receive pool while the socket remains valid. A crashed Windows peer can expose failed receives; empty/oversized datagrams exercise the other discard path directly.

Qualification and retained failures: `docs/implementation/g3-rejoin-recovery-2026-09-24.md` in the game repository. Original file hashes: `docs/implementation/evidence/rejoin-recovery-2026-09-24/upstream-transport-2.7.4-files.json`.

Retire this patch when an upstream version fixes and passes the same discard/hard-departure tests. Review any upstream change to receive-buffer ownership before rebasing; do not blindly apply the diff to another version. Mobile/IL2CPP qualification remains required.
