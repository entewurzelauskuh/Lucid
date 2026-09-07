import React from 'react';
import { Icon } from '../core/Icon.jsx';

export function MoonLives({ lives = 1, max, size = 22, style, ...rest }) {
  const total = max != null ? max : lives;
  const cells = [];
  for (let i = 0; i < total; i++) {
    const alive = i < lives;
    const last = alive && i === lives - 1;
    cells.push(
      <span key={i} style={{ display: 'block', color: alive ? 'var(--fg-1)' : 'var(--fg-4)', opacity: alive ? 1 : 0.4, filter: last ? 'drop-shadow(0 0 7px rgba(242,246,252,.75))' : 'none', animation: last ? 'lucid-breathe 2600ms var(--ease-in-out) infinite' : 'none' }}>
        <Icon name="moon" size={size} strokeWidth={alive ? 1.5 : 1} />
      </span>
    );
  }
  return <div style={{ display: 'flex', gap: 'var(--sp-3)', alignItems: 'center', ...style }} {...rest}>{cells}</div>;
}
