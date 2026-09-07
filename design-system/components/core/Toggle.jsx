import React from 'react';

export function Toggle({ label, hint, checked, disabled, onChange, style, ...rest }) {
  const row = { display: 'flex', alignItems: 'center', gap: 'var(--sp-5)', justifyContent: 'space-between', minHeight: 'var(--hit-min)', cursor: disabled ? 'not-allowed' : 'pointer', opacity: disabled ? 0.5 : 1, ...style };
  return (
    <label style={row} {...rest}>
      <span style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <span style={{ font: '400 var(--fs-body)/1.2 var(--font-body)', color: 'var(--text-strong)' }}>{label}</span>
        {hint && <span style={{ font: '400 var(--fs-micro)/1.3 var(--font-body)', color: 'var(--fg-3)' }}>{hint}</span>}
      </span>
      <span onClick={() => !disabled && onChange && onChange(!checked)} style={{
        position: 'relative', flex: '0 0 auto', width: 46, height: 24, borderRadius: 'var(--r-full)',
        background: checked ? 'color-mix(in srgb, var(--exit-500) 22%, transparent)' : 'color-mix(in srgb, var(--mist-400) 55%, transparent)',
        border: 'var(--bw-hair) solid ' + (checked ? 'var(--exit-500)' : 'var(--line)'),
        transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)'
      }}>
        <span style={{ position: 'absolute', top: 3, left: checked ? 25 : 3, width: 16, height: 16, borderRadius: 'var(--r-full)', background: checked ? 'var(--exit-300)' : 'var(--fg-3)', boxShadow: checked ? 'var(--glow-exit)' : 'none', transition: 'left var(--dur-fast) var(--ease-out), background var(--dur-fast) var(--ease-out)' }} />
      </span>
    </label>
  );
}
