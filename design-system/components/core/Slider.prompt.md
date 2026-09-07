Numeric setting in the Lobby and Options; the value is always shown as a number, never inferred from the handle.

```jsx
<Slider label="Head start" value={30} min={5} max={120} unit="s" onChange={set} />
<Slider label="Dawn" value={300} min={120} max={900} step={30} format={s => Math.floor(s/60)+':'+String(s%60).padStart(2,'0')} />
```
