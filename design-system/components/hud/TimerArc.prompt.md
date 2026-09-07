Top-centre dawn timer for both roles, and the head-start countdown.

```jsx
<TimerArc seconds={192} total={300} phase="The Sleepers are running" />
<TimerArc seconds={28} total={300} phase="Dawn in 0:28" />
```

Needs the `lucid-pulse` keyframe (see guidelines/motion.md). The digits never animate their size — only the arc and its glow pulse, so the number stays readable.
