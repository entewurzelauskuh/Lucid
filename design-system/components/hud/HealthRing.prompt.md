Bottom-left health: a soft glow ring that dims as health drops. No number unless the player hover-holds.

```jsx
<HealthRing hp={64} regen />
<HealthRing hp={22} showNumber />   {/* <=30% turns danger */}
```

Both the arc length and the glow fall away together, so it reads at a glance and without hue.
