# Project boundary

- This directory is the new home for the family game described in `docs/bluey-game-research-2026-09-23.md`.
- `C:\Users\sephi\Desktop\Meeps game` is an unrelated old Unity project. Do not move, copy, merge, or modify that project as part of this game's work unless the user specifically requests it.
- The desktop Connect Unity + Blender and Connect Little Weeps shortcuts now target this root's launcher. The old project's launcher remains unmodified. Require a fresh acknowledgment of the exact new Unity project path before claiming an editor connection.
- G1 foundation setup has begun. Read `docs/implementation/g1-status.md` for actual evidence; do not claim the planned game or networking features are implemented.

## Game implementation workflow

- Treat `docs/bluey-game-research-2026-09-23.md` as the feature goal sheet and `docs/family-playset-build-guide-2026-09-23.md` as the default implementation sequence. The user's latest instructions take precedence.
- Before implementation, read the build guide's current work record and the goal-sheet sections for the task. Follow the active phase and its dependencies; later phases do not remove required features.
- Keep one bounded implementation task active, record its goal IDs and acceptance evidence, and update the work record with what exists, what passed and the next task. Do not mark a phase complete based only on code/assets being present.
- Keep platform builds, saved data, shared-world recovery and actual-device qualification in the sequence. Do not silently replace required iPad hosting with PC-only behavior or use the old project's connector as proof this new project is connected.
