Budget price on a cube card or an effect button. The diamond glyph is the budget point.

```jsx
<CostBadge cost={4} budget={3} />   {/* unaffordable: red edge */}
<CostBadge cost={1} size="sm" />
```

Unaffordable is edge + fill + text colour together, so it survives a colour-blind viewer; the placement rejection still spells out "Not enough budget (3 / 4)".
