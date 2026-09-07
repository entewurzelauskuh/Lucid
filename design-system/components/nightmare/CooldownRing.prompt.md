Cooldown state for the three global effects (30 s per dream), manual triggers (6 s per trap per dream) and the budget trickle point.

```jsx
<CooldownRing progress={0} label="Q"><Icon name="power-dark" size={22} /></CooldownRing>
<CooldownRing progress={0.4} label="18"><Icon name="power-molasses" size={22} /></CooldownRing>
```

Recovering rings dim their contents to `--fg-4`, so "can I press this" is legible without reading the number.
