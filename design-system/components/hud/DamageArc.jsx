import React from 'react';

const ORIGIN = { left: 0, right: 180, top: 270, bottom: 90 };

export function DamageArc({ from = 'left', intensity = 1, style, ...rest }) {
  const deg = typeof from === 'number' ? from : (ORIGIN[from] || 0);
  return (
    <div aria-hidden="true" style={{
      position: 'absolute', inset: 0, pointerEvents: 'none',
      background: 'radial-gradient(120% 90% at ' + (deg === 180 ? '100%' : deg === 0 ? '0%' : '50%') + ' ' + (deg === 270 ? '0%' : deg === 90 ? '100%' : '50%') + ', color-mix(in srgb, var(--danger-500) ' + Math.round(38 * intensity) + '%, transparent) 0%, transparent 42%)',
      opacity: intensity, transition: 'opacity var(--dur-base) var(--ease-out)', ...style
    }} {...rest} />
  );
}
