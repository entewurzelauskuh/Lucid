# Lucid — concept art prompts for Gemini

Ten prompts, each **fully self-contained** — paste any one straight into Gemini with no
setup. Every prompt repeats the style DNA and the world rules on purpose, because the
model has no access to this project.

Two are **UI composites** (Sleeper and Nightmare perspectives), eight are **pure
in-game situations** with the interface deliberately excluded.

## Before you start

- Ask for **16:9**. If your Gemini session lets you set an aspect ratio, do it there too.
- The blocks marked *keep verbatim* are doing real work: the painterly-pass description
  and the door-state vocabulary are what make separate images look like one game.
- If a result drifts toward horror-poster or gore, add `quiet, melancholy, no gore, no
  jump-scare framing` — the dream is not necessarily scary.
- If theme rooms come out matching each other, add `the two rooms should NOT match in
  style — only in scale, lighting and colour grade`.
- On the two UI prompts, the most common failure is soft or garbled type. Re-roll rather
  than accept it, and if it persists, ask for the world first and the interface as a
  second pass over it.
- Nothing here needs a logo. Lucid has no mark; the name is set in type.

---

## 1 · UI — Sleeper HUD, first person

First-person view from inside an 8-metre cube room in a dream maze — a castle-dungeon skin of damp stone, a low stone ceiling, one open passage ahead and, on the left wall, a doorway filled with a sheet of dark grey drifting mist. Held low in frame, the player's hands grip a **Nightlight**: a stubby brass flashlight that doubles as a gun, throwing a soft cone of pale light into the room.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**Now composite a crisp game HUD over that painted world** — the world is soft and painted, the interface is razor-sharp, as if drawn after all the blur. Everything in the humanist sans typeface Inter, thin 1-pixel light-blue-grey borders, no drop shadows, no rounded bubbles:
- **Top centre:** the time `3:12` in large light numerals under a thin semicircular arc that is partly filled with white-gold, and beneath it the small letterspaced uppercase words THE SLEEPERS ARE RUNNING.
- **Bottom left:** a thin glowing circular ring about two-thirds full in pale blue-grey (health), a single small crescent-moon outline beside it (one life), and the small text `depth 9 · exit 11`.
- **Top right:** two stacked translucent blue-black notification strips with thin light borders and a coloured left edge, reading `A door hardened` and `The exit moved`.
- **Screen centre:** a single tiny pale dot as a crosshair. Nothing else.
- **Bottom right:** three faint key hints.

No panel sits behind the corner clusters — just a very soft dark radial wash under each so the text stays legible over the room. Keep all type sharp and truly legible.

---

## 2 · UI — the Nightmare's god view

A god's-eye, near-top-down 2.5-dimensional view onto a maze being built out of uniform 8-metre cube rooms on a grid, seen from high above and slightly angled, as if looking down into an open-topped dollhouse. The cubes are rendered as a painted lattice of small furnished rooms in mismatched themes — a subway platform beside a castle corridor beside a child's bedroom — connected by short passages, floating in soft blue-black darkness. Some cubes are lit from within; the grid fades into mist at its frontier. On the outer edge of the maze, a handful of doorways glow dark grey (closed), and exactly one blazes bright white-gold.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**Now composite a crisp builder interface over it** — the painted world is soft, the interface is razor-sharp, drawn after all the blur. Typeface Inter, thin 1-pixel light borders, small 2–3 pixel corner radii, no glossy buttons:
- **Left edge:** a full-height opaque blue-black dock, about a sixth of the frame wide, holding a grid of small square cards. Each card has a thin line icon, a short name, a tiny cost badge, and a small diagram of an unfolded cube — a cross of six little squares, some with a filled dot (a doorway), some empty and dashed (a wall).
- **Right edge:** a narrower full-height opaque blue-black dock listing three players, each a small coloured circular chip with a number inside — orange #E69F00 marked 1, sky blue #56B4E9 marked 2, pink #CC79A7 marked 4 — a name beside it in light type, a thin health bar and a small crescent moon.
- **Top left:** a large numeral `12` beside a thin partly-filled ring, with tiny text beneath.
- **Top centre:** the time `2:28` under a thin partly-filled semicircular arc.
- **Bottom centre:** a horizontal opaque bar of five thin-line circular icon buttons, each ringed and captioned with a small cost.
- **In the world itself:** small coloured markers with numbers sit on three of the cubes, each with a little triangular wedge showing which way that player faces. One empty grid cell glows translucent green — a legal placement — with a thin green outline.

Keep every piece of type sharp and legible.

---

## 3 · The Nightmare as a light above the puzzle room

A single 8-metre cube room seen from high above and slightly to the side, its ceiling removed so we look straight down into it like an open dollhouse box. The room is a warm, over-furnished child's bedroom gone strange in dream logic: an enormous wardrobe, a rug, a toy chest, a chest of drawers — but the furniture has become a **jump-and-run puzzle**, arranged as a route across the room. Drawers jut out at climbable heights, a wooden plank bridges a gap, two low platforms drift slowly over a pit of upturned floorboard spikes at the room's centre. A small human figure — the **Sleeper** — stands on the chest of drawers, mid-decision, tiny against the room, a faint cone of light from the flashlight in their hands sweeping the gap ahead. On the far wall, a doorway is filled with a sheet of dark grey drifting mist.

**Directly above the open room hangs the Nightmare: not a body, but a presence** — a single cold, pale, unblinking light suspended in the black above the walls, like a bare bulb or a low moon, throwing the Sleeper's long shadow across the floor and rimming every piece of furniture from directly overhead. It watches. It has no face.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 4 · The start cube: the bedroom before the mist drops

First-person view standing in the middle of a small, tender, ordinary child's bedroom at night — the dreamer's own bedroom, rendered as an 8-metre cube room. A single bed with rumpled sheets, a lamp, a wardrobe, a rug, curtains stirring at a window that looks onto nothing but soft blue-black void. The room is the only warm-ish place in the dream, lit low and blue.

**There is exactly one door, and it is sealed.** The doorway — 2.5 m wide, 3 m high, centred on the far wall at floor level — is filled floor-to-lintel with a thick sheet of slowly drifting dark grey-blue mist, layered and translucent, faintly lit from within, absolutely impassable. It is the most important object in the frame. Faint cold light spills around its frame from whatever lies beyond.

Held low in frame, the player's hands grip a stubby brass flashlight that doubles as a gun, its beam catching motes in the air.

The mood is the held breath before a run: quiet, safe, and about to stop being safe.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 5 · The exit, at the deepest edge of the dream

First-person view down a long, cold, mismatched corridor of 8-metre cube rooms — a hospital cube of green tile giving way to a subway cube of dirty cream brick giving way to a stretch of dark forest floor with real trees pushing through the tiles, all at exactly the same ceiling height, all under one unifying painted haze. The corridor recedes away from the camera into blue-black darkness.

**At the far end, one doorway is blazing.** Where the other doorways along the corridor are filled with sheets of dark grey drifting mist, this one is filled with the same drifting mist sheet turned incandescent white-gold #FFF6DE — the way out, the deepest edge of the dream. It is the single warm light source in the entire image, and it throws long converging highlights down the wet floor and along both walls toward the camera, catching every doorframe on the way. Its glow is soft-bloomed and slightly overexposed at the core.

Held low in frame, the player's hands grip a stubby brass flashlight-gun; its own pale beam is almost irrelevant against that light.

Overwhelming feeling: relief that is still very far away.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 6 · Dark — the flashlight is the only light left

First-person view in near-total blackness inside an 8-metre cube room of an office-at-night skin — cubicle partitions, a photocopier, a suspended ceiling — but every light in the dream has just gone out. The frame is dominated by darkness: #070b12 crushing in from all edges.

**The only illumination is the Nightlight**, the stubby brass flashlight-gun held low in frame, whose narrow pale cone reveals just a few metres of carpet, one cubicle edge, and a scatter of drifting dust motes. Beyond the cone, shapes are only barely inferable — a suggestion of a desk, a doorway, something upright that may or may not be furniture.

**Crucially, two things still glow in the dark on their own:** a doorway to the left filled with a sheet of dark grey-blue drifting mist that emits its own faint cold luminescence, and — much further away, small, through a passage — a second doorway filled with mist glowing bright white-gold. Those two lights are the only navigation left. Everything else is guesswork.

Claustrophobic, disorienting, and quiet.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 7 · Molasses — the jump that is going to fall short

A dramatic third-person view from the side, camera low and close, of a small human figure — the **Sleeper** — caught in mid-air over a chasm inside an 8-metre cube room. The room is a giant's kitchen skin at dream scale: the floor is a colossal butcher-block worktop, the gap is the void between two enormous countertops, and far below is soft blue-black nothing. A stubby brass flashlight-gun is still gripped in one hand, its beam swinging uselessly across the far ledge.

**The jump is going to fall short, and the image should make that obvious** — the arc is too flat, the far edge is just beyond the toes, and the figure's posture is already turning from leap into reach.

**Everything about the air reads as viscous.** The Sleeper moves as though through syrup: a faint warm-amber-brown treacle-like haze thickens the frame edges and pools around the limbs, trailing in slow sluggish streaks behind the body, catching the light in heavy strands. Motion blur is long, slow and gluey rather than fast. The world drags.

Across the chasm, on the far wall, a doorway filled with dark grey drifting mist waits — close, and unreachable.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 8 · A Shade wave, on a subway platform

First-person view on a derelict subway platform rendered as an 8-metre cube room — cream tiled walls, a strip of dirty floor, the black mouth of a tunnel, flickering fluorescent tubes overhead. Held low in frame, the player's hands grip a stubby brass flashlight-gun, its beam swung hard to the right and catching what is coming.

**Three Shades are closing.** A Shade is a humanoid melee chaser made of accumulated darkness — roughly person-shaped and person-sized but unfinished, its edges dissolving into smoke and its limbs a little too long, with no face and only two faint cold pinpricks where eyes would be. They move fast and low, slightly slower than a running human. The nearest is close enough that the flashlight beam blows out its shoulder into pale smear; the second is mid-stride behind it; the third is still only a silhouette in the tunnel mouth.

**Behind them, at the back of the room, sits the Nest they came from** — a dark clotted mass fused into the tiled corner where the wall meets the ceiling, veined and faintly pulsing, with a single brighter node at its centre.

A doorway to the left is filled with a sheet of dark grey drifting mist. There is no way out that way.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 9 · The Eye — shooting out a turret's core

First-person view in a castle-dungeon 8-metre cube room of wet black stone, iron sconces and a grated floor drain. Held low in frame, the player's hands grip a stubby brass flashlight-gun, raised and firing — a hard pale muzzle flash lighting the room in one frozen instant.

**High on the far wall, bracketed to the stone, is the Eye: a static turret built as a single great mechanical eyeball**, an iron-and-brass orb about a metre across, lidded with riveted metal plates, its lens a deep glassy pupil that glows a low danger red #D9453A. It has just fired: a slow bright bolt is crossing the room toward the camera, low and to the left, trailing a thin red streak.

**At the eye's base, where the bracket meets the orb, sits its weak point — a small exposed glass-and-copper core**, and the shot is landing there: a bright white-gold spark, a spray of glass, a crack spidering across the housing. Two earlier hits have already scarred it.

The room's other light is a doorway on the right filled with dark grey drifting mist, glowing faintly and coldly. The composition should feel like a held-still trade: stop running, stand in the open, and shoot.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## 10 · The dream seam — one doorway, two impossible rooms

A wide third-person view from behind and slightly above a small human figure — the **Sleeper** — standing exactly in the threshold between two 8-metre cube rooms that have no business touching.

**Behind and around the camera: a mundane office at night** — grey carpet tiles, cubicle partitions, a dead photocopier, the flat green glow of an exit sign, everything at human scale and slightly too tidy.

**Ahead, through the open 2.5 m × 3 m passage: a giant's kitchen at impossible scale** — a butcher-block worktop the size of a public square, a colander like a cathedral dome, a fallen wooden spoon forming a ramp, a jar of something amber lit from behind and towering out of frame. The scale break is absolute and completely unremarked; the dream simply allows it. Both rooms sit under one identical painted haze and share one blue-black colour grade, which is what makes them read as a single world rather than a collage.

**On the wall immediately behind the Sleeper, a doorway is in the act of closing forever** — a sheet of grey mist condensing, thickening and hardening into blank wall in the office's own carpet-and-plaster material, its surface faintly hatched, the last wisps being drawn into the solidifying surface. That way is gone.

Small figure, enormous frame. Wonder first, dread second.

**Style (keep verbatim):** painterly, soft-focus, dreamy — a low-strength oil-paint / Kuwahara brush-smear pass laid over realistic-ish 3D geometry, so mismatched subjects read as one continuous dream. Soft bloom, gentle depth of field that softens toward the frame edges, light film grain, faint lens distortion, vignette. Cool desaturated blue-black base: #070b12, #0d1420, #131c2e, #22314a, #2e4160. Fog and mist are grey-blue #8FA6C8 / #5C7295. The only warm light anywhere in the frame is the exit's white-gold #FFF6DE / #FFE3A3. Red #D9453A appears only as danger, sparingly. Quiet, melancholy, uncanny — not gory, not a horror poster. 16:9.

**World rules to respect:** the dream is built from uniform 8-metre cube rooms on a grid, each cube a different theme skin (childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship) but identical in scale and geometry. Doorways are centred on a cube face at floor level, 2.5 m wide and 3 m high. Every doorway is in one of four states, and each must be readable **without relying on colour**: **fog** = a sheet of dark grey drifting mist filling the opening, solid to the touch; **exit** = the same mist sheet but blazing bright white-gold, the way out; **solid** = the opening has condensed into a blank wall in the room's own material, subtly hatched; **attached** = an open passage into the next cube.

**No UI:** no text, no letters, no numbers, no HUD, no icons, no logos, no watermarks, no captions. Pure environment concept art.

---

## Coverage

| # | Situation | Shows off |
|---|---|---|
| 1 | Sleeper HUD, first person | the crisp-UI-over-painted-world rule, door states, health/lives/timer |
| 2 | Nightmare god view | the 2.5D lattice, cube palette, connector nets, player markers |
| 3 | Furniture jump puzzle, Nightmare as a light overhead | asymmetry made visual — one player as a watching presence |
| 4 | The start cube | the misted door, the moment before the run |
| 5 | The exit | the one warm light in the game, and how far away it is |
| 6 | Dark | the Nightlight as sole light source; fog doors still glow |
| 7 | Molasses | a power that kills by changing physics, not by attacking |
| 8 | Shade wave from a Nest | mobs, and why standing still is dangerous |
| 9 | The Eye and its weak point | the shoot-or-run trade at the heart of the chicanes |
| 10 | The dream seam | "anything goes", plus a door hardening behind you |

Situations deliberately left out, in case you want an eleventh: possession (the Nightmare
piloting a Shade in first person), the Crusher and Pendulum chicanes, the Trapdoor drop,
a Sleeper stranded below a one-way drop, and the moment of waking (white-out).
