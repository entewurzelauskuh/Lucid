# UI Toolkit limits, and what was decided about each

Four things Unity 6's UI Toolkit cannot do that shaped the interface. Each has a
decision, not a workaround, and whoever next tries the obvious thing will
rediscover the limit — this page is so they find the decision first. The
reasoning in full is in `design-system/unity/CLAUDE-CODE-UI-GUIDE.md` §2–§4;
the visual consequences are in `docs/UI.md` §15.

| Limit | What it breaks | Decision |
|---|---|---|
| **No `backdrop-filter`**, and none can be added — the Screen Space Overlay composites after the camera, so a panel has nothing to sample | "Translucent mist panels" as a blanket rule: over a busy god view a 72 % fill reads straight through | Translucency by *lifetime*: permanent chrome opaque, small transient surfaces at 72 %, modal scrims either one frozen-and-blurred frame (Pause, Options, Round start) or live-and-sharp (Tab overlay, target selector). No custom URP feature for the UI |
| **No OpenType feature switches** — `tnum` and `lnum` cannot be requested from a style | A proportional dawn timer jitters every second; Cormorant's default zero is O-shaped | Features frozen into two extra font files offline (`pyftfeatfreeze`), selected with `.tabular` and `.lining`. Four files, not two |
| **No dash array in Painter2D, no `border-style` in USS** | Dashed and dotted borders of any kind | One 9-sliced `dash-border.png` for dashed boxes; the connector net marks a wall by a fainter stroke and a missing dot, which reads better at 12 px than dashes would |
| **No `@keyframes`** | Any looping animation in USS | The four pulses (`design-system/guidelines/motion.md`) are small C# tweens on `style.opacity`. Nothing else pulses |

Also true and worth knowing before writing USS: no `calc()`, no `color-mix()`,
no `em` for letter-spacing, no pseudo-element content, no web fonts by URL, and
durations are seconds rather than milliseconds. Every alpha in the tokens file is
therefore a written-out `rgba()` literal, and every translucent value that is
needed at a new alpha is a new named token rather than a computation.

Rings and arcs are the one thing USS cannot express at all; they are a single
`VisualElement` subclass drawing with `Painter2D.Arc` and reading its colours from
`--ring-*` custom properties, so a stylesheet still configures them.
