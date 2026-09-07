A Sleeper's identity anywhere it appears: god-view marker, Sleeper panel row, spectator tab, scoreboard, Results card.

```jsx
<SleeperMarker index={2} name="Anna" facing={215} selected />
<SleeperMarker index={3} name="Ben" status="awake" />
<SleeperMarker index={1} name="Cara" scale="lg" />   {/* Results, scoreboards */}
```

Colour, number and name always travel together (UI.md §1.5). Use `scale="lg"` on Results and anywhere read over a screen share — at the default `md` the number sits at `--fs-micro`, which is under the 22px value floor. `shape` is off by default and comes from the Options accessibility toggle; when on, each index also gets its own corner radius.
