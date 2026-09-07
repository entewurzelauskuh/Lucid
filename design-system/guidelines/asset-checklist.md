# Asset production checklist

Everything the UI needs, what format it is, who makes it with what, and how long.
The constraint is one person with vector tools over a weekend; anything that
breaks that is flagged **⚠ OVER BUDGET** with a way to cut it.

Licences: CC0 or CC-BY, and OFL for fonts only (CLAUDE.md rule 5); one row per file in
`THIRD_PARTY_NOTICES.md`. Anything made in-house for the UI is project work under
CC-BY-4.0 (docs/SPEC.md §18), not CC0.

## Fonts — 7 files, ~30 min

| File | Format | Source | Licence | Effort |
|---|---|---|---|---|
| `CormorantGaramond-Light.ttf`, `-SemiBold.ttf` | TTF | Google Fonts | OFL 1.1 | 10 min |
| `Inter-Regular.ttf`, `-Medium.ttf`, `-SemiBold.ttf` | TTF | Google Fonts / rsms.me | OFL 1.1 | 10 min |
| `LucidInter-Tabular.ttf` | TTF | `pyftfeatfreeze -f tnum` on Inter-Regular | OFL 1.1, modified | 10 min |
| `LucidCormorant-Lining.ttf` | TTF | `pyftfeatfreeze -f lnum,tnum` on Cormorant Light | OFL 1.1, modified | 10 min |
| `FontAsset`s + one `FontDefinition` per face | Unity asset | Unity font importer | — | 20 min |

Unity exposes no OpenType switches, so tabular and lining figures have to be
frozen into their own files; the `.tabular` and `.lining` USS classes select
them. Check each family for a Reserved Font Name and rename if present — the
`Lucid` prefix does that unconditionally. Ledger the modification.

Subset to Latin + the glyphs actually used before shipping; both families are
large and the UI uses a fraction of them.

## Icons — 29 SVGs, ~4 h total

All on a 24 px grid, 1.5 px stroke, `currentColor`, no fills except two dots.
Already drawn in `assets/icons/`; the work below is exporting and importing.

| Group | Files | Tool | Effort |
|---|---|---|---|
| Door states | `door-attached`, `door-fog`, `door-exit`, `door-solid` | Inkscape / Figma | 45 min |
| Powers & effects | `power-dark`, `power-fog`, `power-molasses`, `power-trigger`, `power-possess`, `power-target` | Inkscape / Figma | 75 min |
| Pack categories | `cat-connector`, `cat-vertical`, `cat-chicane`, `cat-mob`, `cat-gimmick` | Inkscape / Figma | 50 min |
| State | `ready`, `unready`, `role-nightmare`, `role-sleeper`, `moon`, `crown`, `weak-point`, `depth` | Inkscape / Figma | 60 min |
| Connector nets | `net-straight`, `-corner`, `-tee`, `-cross`, `-drop`, `-ladder` | script-generated from the 6-bit mask | 20 min |

**Import note.** Unity's SVG importer (com.unity.vectorgraphics) is the cheap
path; the fallback is a single 2048² SDF sprite atlas exported at 4× and tinted
with `-unity-background-image-tint-color`, which is what the USS above assumes.
The nets are *generated*, not drawn — one script takes the same six-bit mask the
cube data already uses, so a new cube type gets its glyph for free.

## Procedural / code — no files, ~2 days

These are the pieces that would be images in another project and are not here.

| Piece | How | Effort |
|---|---|---|
| Timer arc, health ring, cooldown ring, weak-point ring | one `RingElement : VisualElement` with `generateVisualContent` + `Painter2D.Arc`, driven by the `--ring-*` USS properties | 4 h |
| Mist panel fill | flat `rgba()` background; **no blur** | 0 |
| Frozen modal blur | one `Camera.Render` into an RT on modal open, two-pass blit blur, set as the scrim's background image. Only Pause, Options and Round start; no render feature, no per-frame cost | 1.5 h |
| Damage arc | radial-gradient texture, 512², one file, tinted red | 30 min |
| Cluster scrim | same radial-gradient texture, tinted near-black | 0 (reuse) |
| Noise / grain | 256² tiling CC0 blue-noise PNG at 4 % opacity | 20 min |
| Sleeper marker, ghost cube, world markers | UI Toolkit elements positioned from `WorldToScreenPoint` | 4 h |

## Textures — 3 files, ~1 h

| File | Size | Made with | Effort |
|---|---|---|---|
| `radial-soft.png` | 512² | any gradient tool, greyscale, tinted in USS | 20 min |
| `noise-blue.png` | 256² tiling | CC0 blue-noise, or one shader bake | 20 min |
| `hatch-45.png` | 64² tiling | 8 lines in a vector tool — the solid-door high-contrast fill | 20 min |
| `dash-border.png` | 32² 9-slice | 4 dashed edges in a vector tool — every dashed box in the UI, since USS has no `border-style` | 20 min |

## Not in the budget

| Piece | Why | Do this instead |
|---|---|---|
| ⚠ A logo or brand mark | needs an illustrator and iteration nobody here has | **Already the plan:** LUCID is set in Cormorant Garamond Light at 0.22em tracking. It works. Revisit only if someone volunteers. |
| ⚠ Per-cube illustrated cards | 40+ drawings, and they go stale every time a cube changes | The connector net plus a category icon, generated from cube data — implemented |
| ⚠ Character portraits / avatars | photo or illustration work, and four Sleepers need four | First initial in the display serif on a flat tile — implemented |
| ⚠ Live per-panel blur | a full-screen blur pass every frame plus per-panel UV offsetting, to fake `backdrop-filter` | Opaque docks. Translucency is kept only for small transient surfaces, and modals get one frozen blurred frame. |
| ⚠ Illustrated backgrounds on Title / Results | the brief forbids it and it would date badly | The blurred 3D view, or flat mist |

**Total for a working UI: roughly one weekend** — half a day of fonts and icons,
one day of ring elements and world markers, half a day of textures and assembly.
The blur pass and anything in the table above are the cuts if it runs long.
