# Dinosaur hammer, bubble lab and liquid colors

September 28, 2026. The user liked the simpler science preview, requested a small hammer to break ice off the frozen dinosaur, and asked for researched bubble-making and liquid-color labs. This pass updates the browser playground. It does not build or deploy Unity, Android, iPad or the family server.

[Open the playground](home-science-playground.html) · [Previous child-flow research](science-kids-flow-2026-09-28.html)

## Research used in the implementation

| Source reviewed | Finding and application |
|---|---|
| [Exploratorium: Soap](https://annex.exploratorium.edu/ronh/bubbles/soap.html) | Soap reduces water's surface tension and helps a film last. The bubble lab therefore needs water, soap, mixing and a dipped wand; clicking Blow on an empty or unprepared setup cannot create bubbles. We use illustrative portions, not a real recipe or quantitative surface-tension solver. |
| [Exploratorium: The shape of bubbles](https://annex.exploratorium.edu/ronh/bubbles/shape_of_bubbles.html) | A free bubble tends toward a sphere because that shape minimizes its surface area for a given enclosed volume. Both round and square wands release round bubbles. |
| [Exploratorium: Bubble colors](https://annex.exploratorium.edu/ronh/bubbles/bubble_colors.html) | Thin-film interference explains shifting bubble colors. The visual uses translucent iridescent arcs and highlights; it does not suggest that dye produces a solid rainbow bubble. |
| [PBS: Experiment with walking water](https://www.pbs.org/parents/crafts-and-experiments/experiment-with-walking-water) | Colored water from different vessels combines in a receiving jar; the activity invites prediction and observation of the resulting colors. Applied as three obvious source bottles and one central receiving beaker. Paper-towel capillary transport is not implemented in this slice. |
| [PBS family science guide: Play Together](https://static.pbslearningmedia.org/media/media_files/00eeccf4-8cc2-44cd-aaf3-942771df089f/5f2da456-9574-4772-8094-57efcec32511.pdf) | The publisher describes Curious George's Mix and Paint as combining colors to create new ones and exploring brown. This supports free experimentation without compulsory matching. The historical game was not run or presented as a currently tested product. |
| [Toca Boca Jr](https://www.tocaboca.com/toca-boca-jr) | The publisher's descriptions emphasize exploration and intuitive physical controls. Alongside the earlier child-flow research, this informs tappable apparatus, large picture alternatives and brief optional narration. This is a design inference, not evidence of child usability for our prototype. |

The toy hammer is the user's requested interaction. It chips material locally; it is not represented as heat or as a simulation of real fossil preparation. Warm/cool-water exploration remains available on the same ice.

## What is playable

| Tracking ID | Implementation |
|---|---|
| SP-02 — Dinosaur ice rescue | A small hammer is the default tool. Tap or brush across the ice: nearby cells crack, then break off with bounded fragments and a short hammer swing. The large Chip button reaches the next remaining piece when precise tapping is difficult. Water remains a selectable tool. Once freed, the dinosaur can be moved around the tray. |
| SP-12 — Bubble lab | Expands the existing Bubble garden instead of adding a duplicate destination. Water → Soap → Stir → Dip → Blow. Choose a big bubble or several small ones, round/square wands, and tap to pop. Mixture and wand film are consumed; the lab offers a fresh batch when depleted. Starting a batch does not erase bubbles still flying. Wind/air adjustments remain optional. |
| SP-15 — Liquid color lab | New separate destination. Tap red, yellow and blue bottles or their large picture buttons. Each tap pours one portion; streams and swirls settle into the resulting color. Equal red/yellow makes orange, yellow/blue green, red/blue purple. Amount ratios vary the shade; clear water under More to try lightens/dilutes it. Similar amounts of all three make earthy brown. The beaker holds at most twelve portions. Again and Undo restart remain local to this experiment. |

There are now **15 activities and 60 retained workspaces across four players**. SP-15 extends LAB-01; it does not replace the original SCI-04 additive-light station or SCI-09 reaction activities. Mixing colored liquids and mixing RGB lights retain different rules.

The liquid renderer uses an explicitly authored RYB pigment approximation with interpolation between reviewed color anchors. It preserves portion ratios and dilution, but it is not a measured food-dye spectral simulation. All-three mixtures with strongly uneven proportions are labeled Mixed colors rather than promising every mixture is brown. Mixing is physical blending, with no invented fizz reaction.

The same simple entry pattern remains: at most three large main tools, optional extras closed on entry, tap-to-hear help and no score or compulsory quiz. The three color choices receive equal emphasis. Screen-reader canvas descriptions now include the current observable experiment state.

## Save upgrade and four-player isolation

Browser save schema is **2**, using the established `little-weeps-science-playground-v1` storage key to find earlier play. A validated version-1 record is copied into a migrated record; the original serialized record is preserved under `little-weeps-science-playground-v1-before-v2` before an upgraded save is written. Malformed/future records are rejected and left untouched.

All 56 prior workspaces, selected activities, partially removed ice, existing bubble film, drawings and optional family-chain participation are retained. New hammer fields default safely; old bubble setups receive prepared mixture because the earlier activity already assumed a ready soap bowl. Each player gets a distinct empty color beaker. Current-version upgrades are idempotent. No production game save or enrollment schema changes.

Restart and undo continue to affect only the chosen player's selected experiment. Fragment, bubble and pop-effect counts are bounded. Gentle effects omit the hammer swing, flying fragments, color swirls and pop rays while preserving the visible result.

## Validation and evidence

- [12 new mechanism/migration groups](evidence/science-hammer-labs-2026-09-28/hammer-lab-checks.json): spatial chipping versus heat, complete rescue, retained melting, bubble preparation/depletion/size/shape, color pairs/ratios/dilution/capacity, all four players, version-1 preservation, idempotence and corrupt/future rejection.
- [20 model groups](evidence/science-hammer-labs-2026-09-28/model-checks.json) and [15 flow/audio groups](evidence/science-hammer-labs-2026-09-28/flow-checks.json) passed with the expanded catalog. Existing harness date fields describe their original creation date; these evidence files were generated for this September 28 pass.
- [Browser evidence](evidence/science-hammer-labs-2026-09-28/browser-checks.json): direct ice taps, full rescue and toy movement; Water/Soap/Stir/Dip/Blow; big bubbles and square wand; direct red/yellow bottle taps made orange; separate players made green, purple and brown; reload preserved the orange mixture; clear water increased the retained volume; one-player restart and undo preserved siblings. The new orange hint loaded and started playing. Human voice-quality acceptance is not claimed.
- Tablet four-workspace and compact landscape views were visually inspected; 390- and 320-pixel widths received overflow/target checks. Existing browser save loaded successfully through the upgrade. Pure tests provide the complete old-record comparison; browser checks do not inspect private local storage directly.
- [Hammer review](evidence/science-hammer-labs-2026-09-28/hammer.png), [bubble lab](evidence/science-hammer-labs-2026-09-28/bubble-lab.png), [color lab](evidence/science-hammer-labs-2026-09-28/color-lab.png), [four independent mixtures](evidence/science-hammer-labs-2026-09-28/four-color-labs.png).

Forty-nine local draft hint files now include the hammer, bubble preparation and color reactions. [Voice/scripts manifest](science-playground/hints/manifest.json): Microsoft Zira Desktop, generated on this PC with the existing reproducible script. These remain prototype narration, not accepted final voices or book replacements.

Child playtesting, final apparatus art/audio, physical multitouch/A10 performance, Unity integration and authoritative shared-world/device qualification remain open. Android remains last verified at 188; there was no phone update. Keep the branch's existing integration hold.

**Next bounded task:** review these three interactions, then detail the selected lab before game integration. Preserve all fifteen prototype concepts, the original nine Home science stations and the wider Home backlog.
