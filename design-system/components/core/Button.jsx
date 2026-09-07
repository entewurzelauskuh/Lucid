import React from 'react';
import { Icon } from './Icon.jsx';

export function Button({ children, variant = 'primary', size = 'md', icon, hotkey, disabled, reason, full, onClick, style, ...rest }) {
  const off = disabled || !!reason;
  const primary = variant === 'primary';
  const btn = {
    display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 'var(--sp-3)',
    minHeight: size === 'lg' ? 56 : 'var(--hit-min)',
    padding: size === 'lg' ? '0 var(--sp-7)' : '0 var(--sp-6)',
    width: full ? '100%' : undefined,
    font: (primary ? '500 ' : '400 ') + (size === 'lg' ? 'var(--fs-heading)' : 'var(--fs-body)') + '/1 var(--font-body)',
    letterSpacing: '0.02em',
    color: off ? 'var(--fg-4)' : primary ? 'var(--exit-300)' : 'var(--fg-2)',
    background: off ? 'transparent' : primary ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
    border: 'var(--bw-hair) solid ' + (off ? 'var(--line-faint)' : primary ? 'var(--exit-500)' : 'var(--line-strong)'),
    borderRadius: 'var(--r-2)',
    cursor: off ? 'not-allowed' : 'pointer',
    transition: 'background var(--dur-fast) var(--ease-out), color var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)',
    textAlign: 'center', ...style
  };
  return (
    <button type="button" disabled={off} onClick={off ? undefined : onClick} style={btn}
      onMouseEnter={e => { if (off) return; e.currentTarget.style.background = primary ? 'color-mix(in srgb, var(--exit-500) 22%, transparent)' : 'color-mix(in srgb, var(--mist-500) 60%, transparent)'; if (primary) e.currentTarget.style.boxShadow = 'var(--glow-exit)'; }}
      onMouseLeave={e => { if (off) return; e.currentTarget.style.background = primary ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent'; e.currentTarget.style.boxShadow = 'none'; }}
      {...rest}>
      {icon && <Icon name={icon} size={20} />}
      <span>{reason || children}</span>
      {hotkey && <kbd style={{ font: '500 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)', border: 'var(--bw-hair) solid var(--line)', borderRadius: 'var(--r-1)', padding: '3px 6px', marginLeft: 'var(--sp-2)' }}>{hotkey}</kbd>}
    </button>
  );
}
