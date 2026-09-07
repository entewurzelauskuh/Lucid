Screen centre. A dot; over a weak point it grows a ring that drains as the part takes damage (about 6 shots, SPEC §9).

```jsx
<Crosshair state="weak-point" progress={0.5} />
<Crosshair state="mob" />
```

The ring is the jam progress bar — the only place the Sleeper learns how close the trap is to dying, so it never fades or animates decoratively.
