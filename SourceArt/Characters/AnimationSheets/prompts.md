# Generation prompts — built-in imagegen

Two separate calls used `transparent_background: true`. Both outputs were inspected and retained without pixel changes.

## Shared animation-sheet specification

Production 2D game animation sprite sheet, 4 columns by 4 rows, exactly 16 separate full-body drawings on a truly transparent background. Every frame faces RIGHT in the same three-quarter view, same body scale and proportions, aligned to the same baseline within its equally sized cell. Generous transparent gutters; no cropping, no text, no labels, no grid, no shadows, no backdrop, no detached parts. Crisp clean alpha edges, no blue/white fringe. Preserve the appealing rounded rectangular silhouette, large expressive oval eyes, projecting muzzle, outlined flat-color children's TV illustration of the provided Bluey sheet. This is for actual sprite-frame animation so frame consistency matters more than decorative presentation.

ROW 1: neutral standing smiling; identical standing with both eyes closed for a blink; waving with right hand raised; waving with hand tilted.

ROW 2: WALK frames 1–4, a relaxed complete walking cycle: right foot forward contact/left back; lowered down pose with weight over right foot; passing pose with left foot lifting past planted right foot; rising pose with left foot swinging forward.

ROW 3: WALK frames 5–8: left foot forward contact/right back; lowered down pose with weight over left foot; passing pose with right foot lifting past planted left foot; rising pose with right foot swinging forward. Distinct alternating foot positions; arms swing opposite legs. Feet do NOT keep repeating the same crossed position.

ROW 4: sitting on an invisible seat with bottom supported and knees/feet forward; carry-ready standing with near hand forward at waist holding NOTHING; happy dance with arms up leaning left; happy dance with arms up leaning right.

No visible props or furniture. Match the given selected character style, not the simpler redrawn game art.

## Bluey prefix

Edit target: the supplied user-selected Bluey sheet. Keep Bluey's existing face, eye shapes, muzzle, markings, outline weight, colors, body proportions and appealing art exactly consistent. Retain the look of its idle/blink/wave drawings. Repair the transparency fringe and replace the repetitive walk poses with a usable complete eight-frame gait; add the home-action drawings described below. This is an animation-ready adaptation of the SAME art, not a redesign.

Input: `SelectedReference/bluey-movement-sheet.png` (relative to the parent directory). Output generation identifier: `exec-0a92cec2-b672-4dbe-bf03-ec008ac6aa14.png`.

## Bingo prefix

Create BINGO's matching animation sheet. Image 1 is the chosen ART STYLE and exact animation-sheet design reference. Image 2 supplies BINGO'S identity: orange/peach red-heeler fur, cream eyebrows/muzzle/chest/paws/tail tip, orange ear tips and face patches, dark brown nose, warm brown outlines. Bingo is Bluey's shorter younger sister, cute compact body, never a recolored Bluey with navy patches. Match image 1's polished friendly three-quarter character rendering and line quality while keeping Bingo recognizable from image 2. Bingo alone in every cell; no Bluey.

Inputs: the selected Bluey sheet and local `LocalData/CharacterArt/bingo-official-reference.png`, sourced earlier from the maintained official character catalog. Output generation identifier: `exec-0ed556ba-3e86-47f1-b88a-49c35c59f263.png`.

## Inspection limits

The prompts describe intent, not proof that every anatomical phase is perfect. Generated walking poses have repeated silhouettes and need full-cycle review. Facing is a mirrored three-quarter drawing rather than a complete asymmetric turnaround. Record actual native checks and user feedback separately from prompt requirements.
