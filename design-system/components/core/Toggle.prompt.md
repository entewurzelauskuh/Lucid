Binary setting in the Lobby settings column and every Options tab.

```jsx
<Toggle label="High-contrast doors" hint="A faint hatch on fog, rays on exits" checked onChange={setOn} />
```

The knob glows white-gold when on; the off state is grey-blue, never red. Read-only for non-hosts in the Lobby: pass `disabled`.
