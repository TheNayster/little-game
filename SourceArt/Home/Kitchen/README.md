# Interactive kitchen artwork

Built-in imagegen created a clean version of the existing kitchen architecture, separate appliance interiors and door states, a table with four seats, and food/ingredient/utensil atlases and a separate food-only toppings atlas. Original PNGs are retained unchanged here and copied into Unity Resources/Kitchen; the architecture uses the versioned Resources/Scenery/home-kitchen-working.png.

The manifest records generation briefs and original output paths. Atlas cells are selected by Unity Sprite rectangles, with no raster rewriting. Furniture fronts reuse registered geometry. Recipe artwork is a target illustration; persistent ingredients, steps and portions remain authoritative game state. Physical device/art acceptance is separate from asset generation.

The September 27 ease-of-use correction adds `cooking-layers.png`: raw dough, baked crust, sauce, melted cheese, an empty board and an ingredient bowl. [Generation prompt and provenance](cooking-layers.manifest.json) record the built-in imagegen output. The original RGBA file is unchanged in source and runtime copies; colored RGB beneath transparent pixels is not a visible background. Pizza layers now retain the child's actual topping arrangement instead of replacing it with a finished preset.

The staged chocolate cake adds `cake-stages.png`: empty bowl, chocolate batter, empty tin, plain sponge layer, icing and spoon. [Exact prompt and provenance](cake-stages.manifest.json) identify the built-in imagegen output. Source and runtime copies preserve the original RGBA bytes. Runtime selects inspected component rectangles and nine icing cells, then places them according to the persistent preparation state. The same layers and additions appear on the world dish and portions.
