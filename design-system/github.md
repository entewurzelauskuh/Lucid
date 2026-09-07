repo: entewurzelauskuh/Lucid
branch: main
path: docs/

## Last sync

date: 2026-09-05T10:32:00Z

### Updated in this project
- Resolved the three UI Toolkit limitations as decisions: translucency allocated by surface lifetime, OpenType features frozen into two extra font files, one 9-sliced sprite for dashed boxes
- Split modal scrims into frozen and live, which removes any need for a custom URP render feature in the UI
- Added `MistPanel` tone `chrome` and made the Nightmare docks and powers bar opaque
- Added six annotated in-game mockups and the Claude Code implementation guide with `LucidRing`, `LucidConnectorNet` and `LucidStrings`

*No commit recorded — the tree was read by ref `main`, not at a known commit sha.*

### Pending upstream

`unity/DOCS-CHANGE-REQUEST.md` listed fourteen edits `docs/` needed. Applied by
the repository in #85 (2026-09-07); Dark's two dim levels and the general
readability clause are held as *proposed*. §15's translucency rule turned out to be
`[D]`, not `[S]`.

## Screen map

| Screen / artefact | Built from |
|---|---|
| Title | docs/UI.md §3, §15 |
| Lobby | docs/UI.md §4; docs/SPEC.md §6, §12 |
| Round start | docs/UI.md §5; docs/SPEC.md §11 |
| Sleeper HUD (running, Dark, last 30 s, Tab) | docs/UI.md §6; docs/SPEC.md §9, §10 |
| Spectator | docs/UI.md §7 |
| Nightmare view (idle, placing, possession) | docs/UI.md §8; docs/SPEC.md §7, §10 |
| Mockups N1–N3 (head start, out of budget, dawn) | docs/UI.md §8; docs/SPEC.md §7, §8, §10, §11 |
| Mockups S1–S3 (bedroom, molasses, dark) | docs/UI.md §5, §6; docs/SPEC.md §8, §9, §10 |
| Results (1920 + 1280) | docs/UI.md §9; docs/SPEC.md §12 |
| Pause | docs/UI.md §10 |
| Options | docs/UI.md §11 |
| Palette cube set, costs, masks | docs/SPEC.md §8 |
| Powers, costs, cooldowns | docs/SPEC.md §10 |
| Tokens, type, colour | docs/UI.md §15; docs/SPEC.md §15 |
| unity/Runtime/LucidStrings.cs | docs/UI.md §14 (verbatim) |
| unity/CLAUDE-CODE-UI-GUIDE.md | docs/UI.md §1, §5–§11, §15; docs/SPEC.md §14 |
| unity/DOCS-CHANGE-REQUEST.md | docs/UI.md §1, §6, §9, §11, §14, §15; docs/SPEC.md §7, §15; CLAUDE.md rule 5; docs/DECISIONS.md |
| Icon language | docs/UI.md §15; docs/SPEC.md §7, §8, §10 |
| Asset checklist | CLAUDE.md rule 5 |

## Sync history

- 2026-09-05T09:45Z — full re-read of `docs/UI.md` and `docs/SPEC.md`; corrected lives, budget placement and every glossary string; replaced the invented cube palette with the real MVP set
- 2026-09-03 — first build from the docs: tokens, 18 components, icon language, ten screens
