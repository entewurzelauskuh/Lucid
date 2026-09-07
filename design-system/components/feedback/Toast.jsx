import React from 'react';
import { Icon } from '../core/Icon.jsx';

const TONES = {
  event: 'var(--fg-1)', exit: 'var(--exit-300)',
  danger: 'var(--danger-300)', effect: 'var(--fog-300)'
};

export function Toast({ text, icon, tone = 'event', index = 0, style, ...rest }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 'var(--sp-3)',
      padding: '10px var(--sp-4)', borderRadius: 'var(--r-2)',
      background: 'color-mix(in srgb, var(--mist-600) 72%, transparent)',
      backdropFilter: 'blur(var(--blur-panel))', WebkitBackdropFilter: 'blur(var(--blur-panel))',
      border: 'var(--bw-hair) solid var(--border-panel)',
      borderLeft: '2px solid ' + TONES[tone],
      boxShadow: 'var(--shadow-panel)',
      font: '400 var(--fs-body)/1.25 var(--font-body)', color: 'var(--fg-1)',
      opacity: 1 - index * 0.22, whiteSpace: 'nowrap', ...style
    }} {...rest}>
      {icon && <span style={{ color: TONES[tone] }}><Icon name={icon} size={18} /></span>}
      {text}
    </div>
  );
}

export function ToastStack({ toasts = [], style, ...rest }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 'var(--sp-3)', ...style }} {...rest}>
      {toasts.slice(0, 3).map((t, i) => <Toast key={t.id != null ? t.id : i} {...t} index={i} />)}
    </div>
  );
}
