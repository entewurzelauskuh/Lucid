import React from 'react';
import { Icon } from '../core/Icon.jsx';
import { CostBadge } from '../core/CostBadge.jsx';

/* Six faces in a cross net: top, west, north, east, south, bottom.
   A filled dot is a connector; a dashed empty square is a wall. */
export function ConnectorNet({ mask = '010100', size = 42, style }) {
  const cell = size / 3.4, gap = 1.5;
  const pos = [[1, 0], [0, 1], [1, 1], [2, 1], [1, 2], [1, 3]];
  return (
    <svg width={cell * 3 + gap * 2} height={cell * 4 + gap * 3} style={{ display: 'block', overflow: 'visible', ...style }} aria-label="connector faces">
      {pos.map(([cx, cy], i) => {
        const on = mask[i] === '1';
        const x = cx * (cell + gap), y = cy * (cell + gap);
        return (
          <g key={i}>
            <rect x={x} y={y} width={cell} height={cell} rx="1.5" fill="none" stroke="currentColor"
              strokeWidth="1" strokeOpacity={on ? 0.9 : 0.3} strokeDasharray={on ? undefined : '2 2'} />
            {on && <circle cx={x + cell / 2} cy={y + cell / 2} r={cell * 0.22} fill="currentColor" />}
          </g>
        );
      })}
    </svg>
  );
}

export function PaletteTile({ name, cost, hotkey, mask, category = 'cat-connector', selected, budget, disabled, onClick, style, ...rest }) {
  const off = disabled || (budget != null && budget < cost);
  return (
    <button type="button" onClick={off ? undefined : onClick} title={name} style={{
      display: 'flex', flexDirection: 'column', gap: 'var(--sp-3)', alignItems: 'stretch',
      padding: 'var(--sp-3)', width: '100%', textAlign: 'left', cursor: off ? 'not-allowed' : 'pointer',
      background: selected ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 32%, transparent)',
      border: 'var(--bw-hair) solid ' + (selected ? 'var(--exit-500)' : 'var(--line)'),
      borderRadius: 'var(--r-3)', opacity: off ? 0.45 : 1,
      transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)', ...style
    }} {...rest}>
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 'var(--sp-3)' }}>
        <span style={{ color: selected ? 'var(--exit-300)' : 'var(--fg-2)' }}><Icon name={category} size={26} /></span>
        <span style={{ color: selected ? 'var(--exit-500)' : 'var(--fg-3)' }}><ConnectorNet mask={mask} size={34} /></span>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--sp-2)' }}>
        <span style={{ font: '400 var(--fs-body)/1.15 var(--font-body)', color: 'var(--text-strong)' }}>{name}</span>
        {hotkey != null && <kbd style={{ font: '500 11px/1 var(--font-body)', color: 'var(--fg-3)', border: 'var(--bw-hair) solid var(--line)', borderRadius: 'var(--r-1)', padding: '2px 5px' }}>{hotkey}</kbd>}
      </div>
      <CostBadge cost={cost} budget={budget} size="sm" style={{ alignSelf: 'flex-start' }} />
    </button>
  );
}
