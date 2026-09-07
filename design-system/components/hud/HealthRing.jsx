import React from 'react';

export function HealthRing({ hp = 100, max = 100, regen, size = 76, showNumber, style, ...rest }) {
  const pct = Math.max(0, Math.min(1, hp / max));
  const r = size / 2 - 5;
  const c = 2 * Math.PI * r;
  const low = pct <= 0.3;
  const col = low ? 'var(--danger-300)' : 'var(--fog-300)';
  return (
    <div style={{ position: 'relative', width: size, height: size, ...style }} {...rest}>
      <svg width={size} height={size} style={{ display: 'block' }}>
        <g transform={'translate(' + size / 2 + ',' + size / 2 + ') rotate(-90)'}>
          <circle r={r} fill="none" stroke="var(--line-faint)" strokeWidth="var(--bw-ring)" />
          <circle r={r} fill="none" stroke={col} strokeWidth="var(--bw-ring)" strokeLinecap="round"
            strokeDasharray={c * pct + ' ' + c}
            style={{ opacity: 0.35 + 0.65 * pct, filter: 'drop-shadow(0 0 ' + (4 + 8 * pct) + 'px ' + (low ? 'rgba(255,156,144,.55)' : 'rgba(195,212,234,.45)') + ')', transition: 'stroke-dasharray var(--dur-base) var(--ease-out)', animation: regen ? 'lucid-shimmer 1400ms var(--ease-in-out) infinite' : 'none' }} />
        </g>
      </svg>
      {showNumber && <span style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', font: '400 var(--fs-body)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: col, textShadow: '0 1px 8px rgba(7,11,18,.9)' }}>{Math.round(hp)}</span>}
    </div>
  );
}
