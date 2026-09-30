# Zoo layout design

[zoo-layout.json](zoo-layout.json) is an editable **design proposal**, not a Unity scene or live server configuration. It assigns every starter species to one exhibit, one food bucket and four shared offer places, and records the source behind its habitat/routine brief. Food portions are simplified game representations, not husbandry instructions.

[zoo-layout.svg](zoo-layout.svg) shows the proposed entrance, four trails, all sixteen exhibits and bidirectional connections. The map describes travel connections; playable scenes retain the existing side-on scrolling view. Exact path polygons, animal bounds, gate positions, feeding distances and animation timing require final art and focused playtesting.

The proposed 2400-by-800 logical panels reuse the verified project's panel convention. Four panels per trail require new zoo bounds of 0 through 9600; those bounds are not implemented. The hub contributes one additional panel. No seventeen-panel runtime or performance result is claimed.

Regenerate the diagram with `python Tools/Render-ZooLayout.py`. The renderer checks roster uniqueness, panel assignments, feeding places and the travel graph before writing the SVG. It validates this design data only.

The integration rationale, local code evidence, source limitations, animation production plan and shared feeding contract are in [the zoo research](../../../docs/implementation/zoo-world-research-2026-09-30.md).
