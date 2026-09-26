# Restrained walking prompt set — September 26, 2026

Mode: built-in image generation, transparent output. Two separate requests use the existing Bluey/Bingo action sheets as appearance references. PNG output bytes are retained unchanged in `bluey-gentle-walk.png` and `bingo-gentle-walk.png`; idle/home actions retain their original sheets.

## Exact shared prompt

Use case: precise-object-edit. Asset type: transparent sprite animation atlas for a Unity game. Use the reference as the exact character appearance/style anchor. Generate ONLY an eight-frame relaxed walking cycle, arranged as exactly FOUR columns by TWO rows, landscape atlas. All eight full bodies face screen RIGHT in the same three-quarter view as the reference. The user loves the appearance but complains the arms and legs swing far too much. Make this a quiet casual stroll: feet remain close under hips, tiny forward/back steps, very low foot clearance; hands hang almost straight down alongside hips with a SMALL relaxed swing (about one quarter of the reference's broad motion), no marching, punching, elbow pumping or high knees. Keep the same recognizable facial features, colors, proportions, clean illustrated outlines and friendly small smile. Neutral steady head and torso, no wobble. Exactly 8 ordered consecutive frames, near leg forward contact, small down pose, near leg passing, small lift, far leg forward contact, small down, far leg passing, small lift, cycling naturally into first frame. Near and far limbs alternate clearly with tiny opposite arm swing; do not repeat the same leg forward in both halves. Identical scale/camera/proportions, fixed grid with generous transparent margin, level ground baseline inside every cell. No crop, overlap, floor/shadow, labels, text, props or backgrounds. True clean transparent alpha.

## Character suffix

For each character append: “Character: bluey.” or “Character: bingo.”, then: “The provided image is the existing action sheet, preserve this character's appearance; produce only the quieter 8 walking poses.”

Bluey generation: `exec-c15af0ea-dd36-449a-a983-a32e4451c8fd.png`. Bingo generation: `exec-499c63bd-beaf-48d9-91fc-4408aabdb9ae.png`. Each atlas uses four columns and two rows. Source hashes and frame registration live in `animation-contract.json`.

Prompt requirements are intent, not acceptance evidence. The actual sheets still need motion review for limb alternation, support, overlap and loop seams. Smaller limb excursions and slower playback do not by themselves establish biomechanical accuracy or planted feet. See the third online investigation in `docs/implementation/walk-animation-research-2026-09-26.md` and the delivery report.
