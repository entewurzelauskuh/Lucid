import React from 'react';

export const SLEEPER_COLORS = ['var(--sleeper-1)', 'var(--sleeper-2)', 'var(--sleeper-3)', 'var(--sleeper-4)'];
const SHAPES = ['0', '4', '12', '50'];

/* 'md' is for markers and rows layered over the dream, where the chip must stay
   small. 'lg' is for Results and any scoreboard read over a screen share: every
   VALUE goes to --fs-heading or above, which is the 22px floor in §Minimum
   scales. Static column labels may stay smaller — they name a column, they are
   not a value anyone reads off. */
const SCALES = {
  md: { box: 34, num: 'var(--fs-micro)', name: 'var(--fs-body)', sub: 'var(--fs-micro)' },
  lg: { box: 46, num: 'var(--fs-heading)', name: 'var(--fs-title)', sub: 'var(--fs-body)' }
};

export function SleeperMarker({ index = 1, name, facing, selected, status = 'in-dream', shape, scale = 'md', size, style, ...rest }) {
  const sc = SCALES[scale] || SCALES.md;
  size = size != null ? size : sc.box;
  const col = SLEEPER_COLORS[(index - 1) % 4];
  const out = status !== 'in-dream';
  const radius = shape ? SHAPES[(index - 1) % 4] + (SHAPES[(index - 1) % 4] === '50' ? '%' : 'px') : 'var(--r-full)';
  return (
    <div style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--sp-3)', ...style }} {...rest}>
      <span style={{ position: 'relative', width: size, height: size, flex: '0 0 auto' }}>
        {facing != null && (
          <svg width={size} height={size} style={{ position: 'absolute', inset: 0, transform: 'rotate(' + facing + 'deg)' }} aria-hidden="true">
            <path d={'M' + size / 2 + ' 0 l4.5 7 h-9 Z'} fill={col} opacity={out ? 0.4 : 1} />
          </svg>
        )}
        <span style={{
          position: 'absolute', inset: scale === 'lg' ? 6 : 5, display: 'flex', alignItems: 'center', justifyContent: 'center',
          borderRadius: radius, background: out ? 'transparent' : col,
          border: '2px solid ' + col, opacity: out ? 0.5 : 1,
          boxShadow: selected ? '0 0 0 2px var(--exit-300), 0 0 14px rgba(255,246,222,.55)' : '0 1px 6px rgba(7,11,18,.8)',
          font: '600 ' + sc.num + '/1 var(--font-body)', color: out ? col : '#07110b'
        }}>{index}</span>
      </span>
      {name && (
        <span style={{ display: 'flex', flexDirection: 'column', lineHeight: 1.15 }}>
          <span style={{ font: '400 ' + sc.name + '/1.15 var(--font-body)', color: out ? 'var(--fg-3)' : 'var(--text-strong)', textShadow: '0 1px 6px rgba(7,11,18,.9)' }}>{name}</span>
          {out && <span style={{ font: '400 ' + sc.sub + '/1.2 var(--font-body)', color: 'var(--fg-3)' }}>{status === 'awake' ? 'awake' : 'consumed'}</span>}
        </span>
      )}
    </div>
  );
}
