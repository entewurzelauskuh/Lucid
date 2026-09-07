import React from 'react';

export function CostBadge({ cost, affordable = true, budget, size = 'md', style, ...rest }) {
  const ok = affordable && (budget == null || budget >= cost);
  const s = size === 'sm';
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: s ? 3 : 'var(--sp-2)',
      padding: s ? '2px 5px' : '3px 7px', borderRadius: 'var(--r-1)',
      border: 'var(--bw-hair) solid ' + (ok ? 'var(--line)' : 'var(--danger-500)'),
      background: ok ? 'color-mix(in srgb, var(--ink-900) 45%, transparent)' : 'color-mix(in srgb, var(--danger-700) 35%, transparent)',
      font: '500 ' + (s ? '11px' : 'var(--fs-micro)') + '/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: ok ? 'var(--exit-500)' : 'var(--danger-300)', ...style
    }} {...rest}>
      <svg width={s ? 7 : 8} height={s ? 7 : 8} viewBox="0 0 8 8" aria-hidden="true"><path d="M4 0 8 4 4 8 0 4Z" fill="currentColor" /></svg>
      {cost}
    </span>
  );
}
