Top-right feed for both roles and for spectators, so the audience follows the story.

```jsx
<ToastStack toasts={[
  { text: 'The exit moved', icon: 'door-exit', tone: 'exit' },
  { text: 'Ben was consumed', tone: 'danger' },
  { text: 'Molasses — don\'t jump', icon: 'power-molasses', tone: 'effect' }
]} />
```

Three at a time, four seconds each, newest on top, older ones stepping down in opacity. Strings come from the glossary verbatim — a toast is a rule made visible, so it is never reworded per screen.
