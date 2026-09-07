The Lobby player list row — role choice and ready state in one line.

```jsx
<PlayerRow name="Anna" host role="nightmare" ready self onRole={setRole} onReady={setReady} />
<PlayerRow name="Ben" role="sleeper" />
```

Ready is green + a filled check ring; waiting is a dashed ring — shape carries it, not hue. Only the local player's row is interactive; everyone else's is a read-out.
