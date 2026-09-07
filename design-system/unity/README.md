# unity/ — paste-ready implementation

Everything in here is written against what USS actually supports. Start with
**[CLAUDE-CODE-UI-GUIDE.md](CLAUDE-CODE-UI-GUIDE.md)** — it is the design
authority for the Unity implementation and tells you what may not be changed.

Then read **[DOCS-CHANGE-REQUEST.md](DOCS-CHANGE-REQUEST.md)** *before touching
`docs/`*: fourteen edits the repo's own documentation needed for the
implementation to be consistent with itself. Applied in #85; two items (Dark's dim
levels and the general readability clause) are held as *proposed* until the owner
rules. The rule it called `[S]` and unhonourable was `[D]`.

| File | What |
|---|---|
| `CLAUDE-CODE-UI-GUIDE.md` | The guide. Read first. |
| `DOCS-CHANGE-REQUEST.md` | Edits the repo's `docs/` need. Read second. |
| `lucid-tokens.uss` | Every token as a USS custom property |
| `lucid-components.uss` | A class block per component, with states |
| `uxml/*.uxml` | A structure-only skeleton per screen — **a shape, not a source**: they carry inline `style=` attributes, twenty-two strings that are not in `docs/UI.md`, and five element types that do not exist (`TimerArc`, `HealthRing`, `Crosshair`, `CooldownRing`, `PowerButton` — each is a `LucidRing` with a class, or a `Button`); the guide's §0 forbids the first two in the real screens and §4 explains the third. Take the hierarchy; take styling from the two stylesheets and every string from `LucidStrings` |
| `Runtime/LucidRing.cs` | Every ring and arc — the one thing USS cannot draw |
| `Runtime/LucidConnectorNet.cs` | The six-face cube net, generated from the mask |
| `Runtime/LucidStrings.cs` | `docs/UI.md` §14 transcribed; the only source of copy |

Drop `Styles/`, `Screens/` and `Runtime/` under `Assets/_Lucid/Runtime/UI/` per
the layout in §1 of the guide — inside `Lucid.Runtime`, because this project's
assemblies cannot reference a folder outside them. The C# needs no packages beyond UI Toolkit itself.

## Resolved UI Toolkit limitations

Four things UI Toolkit cannot do. Each has a decision, not a workaround; the
reasoning is in §2, §3 and §4 of the guide.

- **No `backdrop-filter`, and it cannot be added** (the Screen Space Overlay
  composites after the camera). **Decision:** translucency is allocated by
  lifetime. Permanent docks and bars are opaque `--ink-800`; only small
  transient surfaces stay at 72 %. Modal scrims split into `.scrim--frozen` (one
  captured, blurred frame — Pause, Options, Round start) and `.scrim--live`
  (never blurred — Tab overlay, target selector). **No custom URP render feature
  is needed for the UI.**
- **No OpenType feature switches** — no `tnum`, no `lnum`. **Decision:** freeze
  them into two extra source files offline with `pyftfeatfreeze`, then select
  them with the `.tabular` and `.lining` classes. Prose keeps proportional
  figures. Command lines are in guide §3.
- **No dash array in Painter2D and no `border-style` in USS.** **Decision:** one
  9-sliced `dash-border.png` for dashed boxes; the connector net keeps its
  stroke-alpha-plus-dot fallback, which reads better small anyway.

Also worth knowing: **no `@keyframes`.** The four pulses in `guidelines/motion.md`
are small C# tweens on `style.opacity`.
