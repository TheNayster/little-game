# Creek Boat Artwork

Built-in image_gen generated the transparent atlas for the creek boat playset on September 30, 2026. The game consumes an identical copy at `Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/CreekBoatArt/boats.png`. No official game artwork was copied.

[Original atlas](boats-source.png) · [Exact generation prompt](imagegen-prompt.txt) · [Editable sprite layout](sprite-layout.json) · [Research and implementation](../../../docs/implementation/creek-boats-2026-09-30.md)

The atlas remains editable as a raster source. The layout defines six independent parts: leaf hull, bark hull, paper hull, dock, leaf passenger and flower. Unity builds independent layers from those rectangles and adds water ripples separately. Frame bounds use top-left pixel coordinates in the original 1536 by 1024 atlas; the runtime converts them to Unity's bottom-left coordinates. No resampling, recolouring or alpha cleanup was applied to the source image. The four pastel variants tint the hull only at runtime; passenger and flower pixels remain intact.

The sprites use a gentle side view, organic outlines and warm painted surfaces to fit the existing Australian bush scenery. The leaf and bark boats use twig masts and leaf sails; the paper boat retains a folded silhouette. Generated artwork is a prepared game asset, not a claim of family visual approval.
