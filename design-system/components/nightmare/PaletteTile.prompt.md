The palette's cube card, and `ConnectorNet` on its own wherever six faces need showing (hover cards, docs, the cube pipeline's previews).

```jsx
<PaletteTile name="Cross" cost={1} hotkey={4} mask="011110" category="cat-connector" selected />
<PaletteTile name="Nest" cost={4} hotkey={7} mask="010100" category="cat-mob" budget={3} />
<ConnectorNet mask="100001" size={48} />
```

The net is a cross-shaped unfolded cube: column of top / north / south / bottom with west and east flanking the middle. Filled dot = doorway, dashed square = wall.
