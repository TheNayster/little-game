# Shared Home books: original-story phone preview

September 26, 2026. BOOK-01 development on `codex/home-reading`, following the [reader research](home-reading-quiet-play-research-2026-09-26.md) and [voice research and selection](kids-narrator-research-2026-09-26.md).

**September 27 iPad rollout:** both iPads now run 171 with the existing six-book preview. In-place installation retained all prior documents/preferences, and both devices wrote runtime version 171. The user reports silent books; listening acceptance remains open while audio controls/playback are checked. [Current rollout and evidence](chocolate-cake-flow-2026-09-27.html#device-delivery).

## Requested preview

The user requested the original drafts on the phone before deciding whether to replace them. Build 147 delivered four books. The user reported clipped animals and story panels, small covers/pages, and two missing books. The next preview completes the original six, enlarges the shared rack and chooser, and replaces the small framed page with a screen-sized illustration and translucent controls, following the supplied tablet example. Full spoken story words appear on each page. Read to me supports pause/resume, replay, deliberate sounds and automatic page turning; reading never starts merely by opening a book.

| Preview title | Pages | Contents |
| --- | --- | --- |
| Hello, Dinosaurs! | 14 | Twelve animals including T. rex, Brontosaurus, Triceratops, Spinosaurus, Pteranodon and Quetzalcoatlus; individual name cues and imaginative calls |
| Big Trucks, Little Bridge | 8 | Building a shared bridge |
| Rocket to the Moon | 8 | Ari and Pip's pretend moon trip |
| The Fairy Garden Surprise | 8 | Lila's patient care for a plant |
| The Princess and the Lost Star | 8 | Nora returns a child's parade lantern |
| The Mermaid and the Rainbow Shell | 8 | Mira helps a hermit crab and removes litter |

These remain drafts for review. The separately requested dinosaurs/reptiles/vehicles/Hello Kitty/Tangled/unicorn subjects remain recorded; this preview does not silently approve or cancel that later selection. All four family profiles can read every title.

## Applied systems and art corrections

Six persistent book objects occupy the downstairs living-room rack. One physical copy has one holder; reading uses independent local page/audio cursors and per-profile, per-title durable bookmarks. Books can be carried into rooms. No global reading lock is introduced. Explicit narration intent rejects late media loads after pause, page turn, close or lifecycle interruption. Resources are released between titles; voice and effects have separate bounded sources and effects duck during speech.

Build 147 introduced schema 10/content 11 with four copies. The six-book preview uses schema 11/content 12/Windows contract 13. Migration preserves existing identities, placements, storage and holders while adding only missing copies at free rack positions. Schema-10 saves still validate before upgrading. No phone/iPad becomes a server and private offline work is never merged into shared authority.

The original dinosaur silhouettes crossed uniform atlas-cell boundaries. Replacement silhouettes have measured alpha bounds and generous empty cell margins. Each sprite uses its complete measured rectangle rather than cutting tails or wings at an assumed grid line. Story sheets also use measured panel boundaries to avoid showing neighboring pages. Wider compositions preserve main subjects; the reader fits complete pictures without stretching or cropping to fill a different device aspect. Some aspect ratios leave narrow margins.

The rack is larger, book covers increase from 90×115 to 130×170 world units, and all six chooser covers are larger than the previous four-book chooser. Correctly registered foreground rails replace an invalid two-point front mask. The reader's illustration occupies the available screen and controls overlay it at 30% panel opacity; the spoken text uses its own translucent panel and a bounded readable font size. A Unity build check measures every actual paragraph at the narrower 4:3 landscape width.

Audio was generated on the user's PC: Qwen3-TTS Base with the explicitly selected Rapunzel speaking reference; MMAudio generated gentle illustrative effects. No extinct animal recording is claimed to be authentic. The private reference recording and model caches remain ignored. Generated narration, prompts, seeds, model hashes and mastering records are retained under `SourceAudio`; editable art, generation prompts, hashes and crop metadata are under `SourceArt/Books`.

## Verification and limits

- 154 core checks passed, including additive migration from the installed four-book schema, independent four-reader cursors, stale playback cancellation and exclusive physical holding/storage.
- Six titles, 54 pages and 83 installed narration/name/effect WAVs checked against their generated source bytes; measured art rectangles stay in bounds.
- Android 147 was installed over 138 with matching package/signing identity and exact installed APK hash; all 16 save records remained. Fifteen were byte-identical and the active world migrated without losing previous object/profile identities. Physical reader playback worked; visual inspection and user feedback identified the issues fixed here.
- Android 148 compiled and signed successfully but was not installed because the user then requested the full-page layout, translucent controls and full story text. The next build must contain those newer changes.

**Android 149 installed:** the fresh signed release replaced 147 in place; exact installed APK/signing matched. All 16 save records remain, 15 byte-identical. The active world retained all previous object/profile identities and added two books (schema 10→11). The physical Mermaid page shows full-screen art, complete spoken text and translucent controls; no Unity/Android runtime errors were observed. Other page visual checks were not completed while the user was interacting. The Unity build check measured all 54 paragraphs successfully at 4:3 (24pt; maximum measured height 55). [Sanitized update evidence](evidence/home-books149-2026-09-26/android-update.json) · [Actual phone reader](evidence/home-books149-2026-09-26/full-page-reader.png). Development checkpoint remains on `codex/home-reading` until the outstanding native qualification is complete. A10 iPad performance, sustained mixed-device reading, native four-client reader qualification, full production recovery, narration pronunciation/listening acceptance and final story selection remain open. The PC server/iPads/iPhone have not been updated by this preview; the phone uses private solo while their content versions differ. Home, books and the wider room phases are not declared complete.
