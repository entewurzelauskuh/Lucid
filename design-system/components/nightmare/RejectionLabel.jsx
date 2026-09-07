import React from 'react';

export const REJECTIONS = ['Door is solid', "Doesn't fit here", 'Would trap {name}', 'Not enough budget ({have} / {cost})', 'Not a door'];

export function RejectionLabel({ reason, style, ...rest }) {
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: 'var(--sp-3)',
      padding: '6px var(--sp-4)', borderRadius: 'var(--r-1)',
      background: 'color-mix(in srgb, var(--ink-900) 82%, transparent)',
      border: 'var(--bw-hair) solid var(--danger-500)',
      boxShadow: 'var(--glow-danger)',
      font: '500 var(--fs-body)/1 var(--font-body)', color: 'var(--danger-300)',
      whiteSpace: 'nowrap', ...style
    }} {...rest}>
      <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"><path d="M3 3l8 8M11 3l-8 8" /></svg>
      {reason}
    </span>
  );
}
