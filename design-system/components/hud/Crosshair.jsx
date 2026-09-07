import React from 'react';

export function Crosshair({ state = 'idle', progress = 0, size = 56, style, ...rest }) {
  const weak = state === 'weak-point';
  const mob = state === 'mob';
  const r = size / 2 - 8;
  const c = 2 * Math.PI * r;
  return (
    <div style={{ position: 'relative', width: size, height: size, ...style }} {...rest}>
      <svg width={size} height={size} style={{ display: 'block', overflow: 'visible' }}>
        <g transform={'translate(' + size / 2 + ',' + size / 2 + ')'}>
          {weak && (
            <>
              <circle r={r} fill="none" stroke="var(--line)" strokeWidth="var(--bw-ring-thin)" />
              <g transform="rotate(-90)">
                <circle r={r} fill="none" stroke="var(--exit-300)" strokeWidth="var(--bw-ring-thin)" strokeLinecap="round"
                  strokeDasharray={c * Math.max(0, 1 - progress) + ' ' + c}
                  style={{ filter: 'drop-shadow(0 0 6px rgba(255,246,222,.7))', transition: 'stroke-dasharray var(--dur-instant) linear' }} />
              </g>
              <path d="M0 -4 v8 M-4 0 h8" stroke="var(--exit-300)" strokeWidth="1.5" strokeLinecap="round" />
            </>
          )}
          {!weak && <circle r={mob ? 3 : 2} fill={mob ? 'var(--fg-1)' : 'var(--fg-2)'} style={{ filter: mob ? 'drop-shadow(0 0 7px rgba(242,246,252,.9))' : 'none', transition: 'r var(--dur-fast) var(--ease-out)' }} />}
        </g>
      </svg>
    </div>
  );
}
