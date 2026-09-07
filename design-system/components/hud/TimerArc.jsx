import React from 'react';

const mmss = s => Math.floor(Math.max(0, s) / 60) + ':' + String(Math.floor(Math.max(0, s) % 60)).padStart(2, '0');

export function TimerArc({ seconds = 0, total = 300, label, size = 148, urgent, phase, style, ...rest }) {
  const left = Math.max(0, seconds);
  const hot = urgent != null ? urgent : left <= 30;
  const filled = total > 0 ? Math.min(1, 1 - left / total) : 0;
  const r = size / 2 - 8;
  const c = 2 * Math.PI * r;
  const stroke = hot ? 'var(--danger-300)' : 'var(--exit-500)';
  return (
    <div style={{ position: 'relative', width: size, height: size / 2 + 34, display: 'flex', flexDirection: 'column', alignItems: 'center', ...style }} {...rest}>
      <svg width={size} height={size / 2 + 6} viewBox={'0 0 ' + size + ' ' + (size / 2 + 6)} style={{ position: 'absolute', top: 0, left: 0 }}>
        <g transform={'translate(' + size / 2 + ',' + (size / 2 + 2) + ')'}>
          <circle r={r} fill="none" stroke="var(--line-faint)" strokeWidth="var(--bw-ring)" strokeDasharray={c / 2 + ' ' + c} transform="rotate(180)" />
          <circle r={r} fill="none" stroke={stroke} strokeWidth="var(--bw-ring)" strokeLinecap="butt"
            strokeDasharray={(c / 2) * filled + ' ' + c} transform="rotate(180)"
            style={{ filter: hot ? 'drop-shadow(0 0 6px rgba(255,156,144,.6))' : 'none', animation: hot ? 'lucid-pulse var(--pulse-timer) var(--ease-in-out) infinite' : 'none' }} />
        </g>
      </svg>
      <div style={{ marginTop: size / 2 - 42, textAlign: 'center' }}>
        <div style={{ font: '400 var(--fs-title)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', letterSpacing: '0.02em', color: hot ? 'var(--danger-300)' : 'var(--fg-1)', textShadow: '0 1px 12px rgba(7,11,18,.9)' }}>{mmss(left)}</div>
        {(phase || label) && <div style={{ marginTop: 6, font: '500 var(--fs-micro)/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-3)', textShadow: '0 1px 8px rgba(7,11,18,.9)' }}>{phase || label}</div>}
      </div>
    </div>
  );
}
