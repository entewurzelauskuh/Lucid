import React from 'react';

export function Slider({ label, value = 0, min = 0, max = 100, step = 1, unit, format, disabled, onChange, style, ...rest }) {
  const pct = Math.max(0, Math.min(100, ((value - min) / (max - min)) * 100));
  const shown = format ? format(value) : value + (unit ? ' ' + unit : '');
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-3)', minHeight: 'var(--hit-min)', justifyContent: 'center', opacity: disabled ? 0.5 : 1, ...style }} {...rest}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', gap: 'var(--sp-4)' }}>
        <span style={{ font: '400 var(--fs-body)/1.2 var(--font-body)', color: 'var(--text-strong)' }}>{label}</span>
        <span style={{ font: '500 var(--fs-body)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--exit-500)' }}>{shown}</span>
      </div>
      <div style={{ position: 'relative', height: 20, display: 'flex', alignItems: 'center' }}>
        <div style={{ position: 'absolute', inset: 'auto 0', height: 3, borderRadius: 'var(--r-full)', background: 'color-mix(in srgb, var(--mist-400) 70%, transparent)' }} />
        <div style={{ position: 'absolute', left: 0, width: pct + '%', height: 3, borderRadius: 'var(--r-full)', background: 'var(--exit-500)' }} />
        <div style={{ position: 'absolute', left: pct + '%', width: 14, height: 14, marginLeft: -7, borderRadius: 'var(--r-full)', background: 'var(--exit-300)', boxShadow: 'var(--glow-exit)' }} />
        <input type="range" min={min} max={max} step={step} value={value} disabled={disabled}
          onChange={e => onChange && onChange(Number(e.target.value))}
          style={{ position: 'absolute', inset: 0, width: '100%', margin: 0, opacity: 0, cursor: disabled ? 'not-allowed' : 'pointer' }} />
      </div>
    </div>
  );
}
