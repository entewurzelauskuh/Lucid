Every panel, card and docked bar in the game; translucent mist over the 3D view, opaque only on full screens.

```jsx
<MistPanel title="Round" aside="host edits">
  <Toggle label="Hint cards" checked />
</MistPanel>
```

Three tones, and picking the right one is the whole decision:

- `panel` — the translucent one. Transient surfaces only: toasts, the trap hover-peek, the reveal card, hint cards. Small, short-lived, so a bright dream behind it cannot cost you a rule.
- `chrome` — opaque, with the hairline and outer shadow. Permanent docks and bars: the Nightmare's palette, the Sleeper panel, the powers bar.
- `sunken` — opaque, no shadow. Full screens: Options, Results.

Large permanent panels are opaque because USS has no `backdrop-filter`, so translucency cannot be rescued by blurring what is behind it. `edge="bottom"` drops the radius and the top border for a bar docked to a screen edge. Never nest a mist panel inside a mist panel — use a divider.
