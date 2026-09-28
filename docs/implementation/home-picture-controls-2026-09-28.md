# Home picture buttons and reading controls

September 28, 2026. The user accepted the hammer/bubble/color presentation and asked to check the other mini-game and reading buttons while keeping this style. This is a browser-preview consistency pass under LAB-01 and BOOK-01. The Unity client, server, installed devices and production saves are unchanged.

[Science playground](home-science-playground.html) · [Book-reader preview](home-book-playground.html)

## Accepted direction

Retain the cream, teal and pastel palette, rounded buttons, familiar object pictures, short visible labels, generous spacing and an emphasized action where a next action is useful. Keep optional tools behind More to try. Use the same picture vocabulary and actual button semantics across activities. Reading keeps full-page art fitted without cropping and translucent controls with solid legible pictures/text. Keep **Read to me** as the initial narration action; opening a title stays quiet.

This acceptance records the user's preferred direction. It does not declare every activity, story, voice or physical-device layout accepted.

## What the review found and changed

| Surface | Finding and result |
|---|---|
| Fifteen science main flows | Already use at most three large picture controls. Preserved their state-driven actions, direct manipulation, short help and existing save model. |
| Science optional tools | Had text-only buttons and sliders. Added matching pictures to every optional action and slider label, including temperature, surfaces, weights, lamps/fans, color mixing and chain pieces. Selected choices expose their state. Controls wrap at narrow widths. |
| Shared navigation | Science and Books now have consistent picture links. Leaving an activity preserves its saved work. |
| Existing game reader, source review | Has text-only overlaid controls with four actions across the bottom and additional voice/effect/auto controls at the top. The preview groups three main actions, keeps page arrows and Books fixed, and moves page/word preferences into More reading options. Existing Unity controls were inspected but not changed or physically requalified. |
| Cooking, mixing and coloring in Unity, source review | Cooking actions, the existing mixing screen, coloring page navigation and undo still need the accepted picture treatment during game integration. Existing ingredient pictures/color swatches are useful starting points. This preview does not claim those native screens were restyled. |

The reusable browser button helper is shared by main science tools, optional tools and the reader. Additional original SVG pictures extend the existing icon set; no raster art or character design was replaced.

## Reading research applied

- [Khan Academy Kids: books guide](https://khankids.zendesk.com/hc/en-us/articles/4409036780955-Learn-more-about-the-books-in-the-Khan-Academy-Kids-app) describes a picture library, Read to Me versus Read by Myself, and page arrows for independent reading. Applied as quiet opening, deliberate narration, persistent arrows and a prominent reading button. We did not run their app or copy its assets; this pass reviewed publisher documentation. Word-by-word synchronized highlighting is not implemented here.
- [W3C enhanced target guidance](https://www.w3.org/WAI/WCAG22/Understanding/target-size-enhanced.html) specifies 44 by 44 CSS-pixel targets, with larger targets helpful for frequent or sequential actions. Applied as a measured lower bound, with larger main controls and spacing. This is an accessibility reference, not a toddler usability certification.
- The previous [children's-app research](science-kids-flow-2026-09-28.html) and [book requirements](home-reading-quiet-play-research-2026-09-26.html) remain the basis for picture choices, limited competing actions, independent reading and explicit audio intent.

## Playable reading preview

All six **existing draft** books and their 54 pages reuse the current game content, measured art rectangles and recorded page/name/effect files. No new stories, voices or audio assets were generated. This does not settle or replace the separately requested dinosaurs/reptiles/vehicles/Hello Kitty/Tangled/unicorn catalog.

- Read to me becomes Pause while speaking, or Cancel while loading. Pausing preserves the position; Keep reading resumes it. Read again starts the current page over.
- Hear sound is deliberate. Dinosaur overview pictures speak their names; the sound action follows the selected animal. Page changes, title changes, closing and hiding the tab stop playback. A stale media promise owns only its old element and cannot stop or restart a newer request.
- Automatic page turning is optional and initially off. It follows completed narration and can be switched off. Manual page arrows always work; first/last boundaries do not wrap.
- Story words appear over a translucent panel. All illustrations fit their complete authored bounds; different aspect ratios can leave margins. The landscape layout is the primary book view.
- Four selectable readers retain independent per-title pages and narration positions. Their separate preview key, `little-weeps-book-controls-v1`, does not touch the science key or any Unity save. Malformed records remain untouched with a visible storage message; playback intent is never saved as active.
- Browser audio uses the existing clips and a single active channel, so names, effects and narration do not compete. This is a control demonstration, not the final native multi-device audio implementation.

## Validation and limits

- [Eight reader/content groups](evidence/home-picture-controls-2026-09-28/reader-checks.json): four-reader isolation, title isolation, boundaries, content revision handling, corrupt/future saves, stale media intent, all 54 page recordings and all authored crop bounds.
- [Fifteen flow/audio groups](evidence/home-picture-controls-2026-09-28/flow-checks.json), [twenty model groups](evidence/home-picture-controls-2026-09-28/model-checks.json) and [twelve hammer/lab groups](evidence/home-picture-controls-2026-09-28/hammer-checks.json) pass. Total: **55 test groups**.
- [Browser checks](evidence/home-picture-controls-2026-09-28/browser-checks.json) cover all fifteen main/optional science tool sets at 320 pixels, all six quiet book openings, page-turn cancellation, narration completion/pause/resume, automatic page advancement and independent saved pages after reload. Reader targets/layouts were checked at 1024×768, 740×360 and 320×640.
- [Dinosaur reader](evidence/home-picture-controls-2026-09-28/dinosaur-reader.png) and [story reader](evidence/home-picture-controls-2026-09-28/story-reader.png) are actual browser captures.

No Unity build, phone installation, live-server change or iPad qualification occurred. Browser media state is not a human listening-quality check and does not close the reported native book-sound issue. Physical multitouch, child usability, A10 performance and authoritative shared play remain open. The science prototype save schema is unchanged at 2; the production game remains at its prior versions. Keep the existing branch integration hold; do not promote this lineage to main.

**Next bounded task:** review these book controls, then carry the accepted picture controls into one native activity/reader slice with the existing save/audio and four-player contracts. Preserve all fifteen science prototypes, original science stations, cooking/coloring work, requested book subjects and the full Home backlog.
