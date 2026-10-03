# Sandcastle Stage 5: isolated Unity prototype

Owner: DAY-01 / LEARN-01 / FAMILY-01. These are actual native Unity P4 captures, not a mockup. Production build447 is unchanged. Retain as dated review evidence; retire only under the project evidence policy.

## Open and play

Open `Unity/FamilyPlayset` in Unity6000.3.24f1. Use **Little Weeps > Sandcastle > Open isolated visual prototype**, or open `Assets/FamilyPlayset/Scenes/SandcastleVisualPrototype.unity`. Press Play. Set Game View to1024x768 (tablet) or1280x591 (phone).

The first screen is explicitly labelled **authored example castle**. Press **Empty pit** for real construction: select Round/Square/Wall/Gate; tap sand; inspect the translucent preview; Confirm (Rotate optional); Scoop three times, Water and Tip; select the visible face of a built piece; add Flag and Shell. Continue choosing shapes and building. Water may precede scoops. Invalid overlap keeps the existing creation. Leave releases only the local synthetic participant. Empty pit/Example castle intentionally create a new disposable synthetic family, not a reset of any real session.

Fresh local executable: `Builds/SandcastlePrototype/P4/SandcastlePrototype.exe`. It contains only the prototype scene. Rebuild with the editor method `LittleWeeps.EditorTools.SandcastlePrototypeBuild.Build`, passing `-prototypeOutput` with an unused output path. The ordinary production build workflow/scenes remain unchanged.

## Compare and inspect

- [Approved target](approved-target.png) - exact supplied attachment, byte-matching reference only.
- [Target beside actual tablet Unity](target-beside-unity-tablet.png).
- [Target beside actual phone Unity](target-beside-unity-phone.png).
- [Actual placement/build/decoration recording](native-p4-placement-build-decorate-silent.mp4) - 156 live timestamped frames, about25seconds, silent. Includes empty pit, mould/preview/Confirm, scoops, water, tip, continued wall construction, selecting an existing round tower and adding both props.
- [Tablet empty](native-p4-tablet-empty.png), [preview](native-p4-tablet-preview.png), [early water](native-p4-tablet-water-before-scoop.png), [full bucket](native-p4-tablet-full-bucket.png), [combined decorations](native-p4-tablet-built-combined.png), [selected existing piece](native-p4-tablet-selected-existing.png).
- [Phone gate preview](native-p4-phone-gate-preview.png), [open gate built/decorated](native-p4-phone-gate-built.png), [invalid overlapping preview](native-p4-phone-overlap-preview.png), [continued construction from recording](native-p4-phone-recorded-continued-build.png).
- [Acceptance receipt](local-acceptance.json).

## Assets and differences still for review

Separate illustrated empty wood/sand sprite and six registered castle sprites are complete prototype artwork candidates, with original transparent PNG masters and registration in `SourceArt/Daycare/Sandpit/StageFivePrototype`. Round/square, wall/gate and both orientations are separate rendered objects. Bucket/tools/flags/shells/windows, four Little Weeps characters and the small existing orange T.rex are reused assets. The reference image is never loaded by the game.

Temporary/adapted: existing Carry poses rather than custom leaning/shovelling drawings; existing blue bucket/Foley artwork rather than newly illustrated matching tools; picture trays expose four moulds and two primary decorations, with windows demonstrated in the fixture. The toy is shown in the fixture; its full move/reaction controls, other prop/edit/reset UI and networking transport are not redesigned in this bounded prototype. There is no new toy truck or star decoration. The example is12pieces, not a new saved castle or a new limit.

Technical constraints: current8x4 snapped logical lattice,16-piece cap and0/90 orientations are preserved. This is a2D trapezoid projection with depth-sorted individual billboards, not a3D physics scene. The phone keeps the same vertical framing and shows more surrounding daycare scenery; it does not reproduce the target's close foreground crop. The wooden pit is more rectangular and the generated grain more pronounced than the target. The reused cast has flatter shading than the new pit and no authored hands gripping its rim. No audio is included in the evidence recording. These differences remain visible for parent review; visual acceptance, physical touch comfort/performance and child enjoyment are unclaimed. Stage6 has not started.
