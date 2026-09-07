import React from 'react';

export function CooldownRing({ progress = 0, size = 56, thickness, tone = 'exit', label, children, style, ...rest }) {
  const r = size / 2 - 3;
  const c = 2 * Math.PI * r;
  const ready = progress <= 0;
  const col = tone === 'danger' ? 'var(--danger-300)' : tone === 'fog' ? 'var(--fog-300)' : 'var(--exit-500)';
  return (
    <div style={{ position: 'relative', width: size, height: size, ...style }} {...rest}>
      <svg width={size} height={size} style={{ position: 'absolute', inset: 0, display: 'block' }}>
        <g transform={'translate(' + size / 2 + ',' + size / 2 + ') rotate(-90)'}>
          <circle r={r} fill="none" stroke="var(--line-faint)" strokeWidth={thickness || 'var(--bw-ring-thin)'} />
          <circle r={r} fill="none" stroke={ready ? col : 'var(--exit-700)'} strokeWidth={thickness || 'var(--bw-ring-thin)'} strokeLinecap="butt"
            strokeDasharray={c * (ready ? 1 : progress) + ' ' + c}
            style={{ opacity: ready ? 1 : 0.85, transition: 'stroke-dasharray var(--dur-base) linear' }} />
        </g>
      </svg>
      <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 1, color: ready ? 'var(--fg-1)' : 'var(--fg-4)' }}>
        {children}
        {label && <span style={{ font: '500 11px/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'inherit' }}>{label}</span>}
      </div>
    </div>
  );
}
