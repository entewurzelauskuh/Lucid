import React from 'react';

export function MistPanel({ title, aside, tone = 'panel', pad, edge = 'all', children, style, ...rest }) {
  /* 'panel' is the only translucent tone, and it is reserved for surfaces that
     appear and vanish over the dream — toasts, hover peeks, the reveal card.
     Permanent chrome ('chrome') and full screens ('sunken') are opaque, because
     USS has no backdrop-filter and a 72 % fill over a bright dream loses its
     text contrast. See unity/CLAUDE-CODE-UI-GUIDE.md §2. */
  const opaque = tone === 'sunken' || tone === 'chrome';
  const fill = opaque ? 'var(--ink-800)' : 'color-mix(in srgb, var(--mist-600) 72%, transparent)';
  const panelShell = {
    position: 'relative', display: 'flex', flexDirection: 'column',
    background: fill,
    backdropFilter: opaque ? 'none' : 'blur(var(--blur-panel))',
    WebkitBackdropFilter: opaque ? 'none' : 'blur(var(--blur-panel))',
    border: 'var(--bw-hair) solid var(--border-panel)',
    borderTopWidth: edge === 'bottom' ? 'var(--bw-hair)' : edge === 'top' ? 0 : undefined,
    borderRadius: edge === 'all' ? 'var(--r-2)' : 0,
    boxShadow: tone === 'sunken' ? 'none' : 'var(--shadow-panel)',
    padding: pad != null ? pad : 'var(--pad-panel)',
    color: 'var(--text-body)',
    font: '400 var(--fs-body)/var(--lh-body) var(--font-body)',
    ...style
  };
  return (
    <div style={panelShell} {...rest}>
      {(title || aside) && (
        <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', gap: 'var(--sp-4)', marginBottom: 'var(--sp-5)' }}>
          {title && <h2 style={{ margin: 0, font: '600 var(--fs-heading)/var(--lh-snug) var(--font-display)', color: 'var(--text-strong)', letterSpacing: '0.01em' }}>{title}</h2>}
          {aside && <span style={{ font: '500 var(--fs-micro)/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-3)' }}>{aside}</span>}
        </header>
      )}
      {children}
    </div>
  );
}
