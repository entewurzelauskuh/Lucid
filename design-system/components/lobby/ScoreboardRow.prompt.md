The session leaderboard in the Lobby, the Tab overlay during a round, and the deltas on Results.

```jsx
<ScoreboardRow header rank="#" name="Player" rounds="As Nightmare" woke="Woke" consumed="Consumed" score="Score" />
<ScoreboardRow rank={1} index={2} name="Anna" rounds={2} woke={3} consumed={1} score={612} delta={148} />
```

Every **value** — rank, number chip, name, counts, score, delta, status — is `--fs-heading` (22px) or larger and tabular, because this row has to survive a 720p screen share at 0.667×. The `header` row is the one exception: static column labels sit at `--fs-micro` in caps, since they name a column rather than carry a value anyone reads off.

The score column is `minWidth`, never a fixed `width`. It is flex-end aligned, so a fixed width lets a long score plus delta escape leftward over the Consumed column while the box itself stays put — which no box-width measurement will catch. Scores sum across rounds (SPEC §12), so four and five digits are normal; if you restyle this row, re-derive that minimum from the longest score plus the longest delta at the current type size.
