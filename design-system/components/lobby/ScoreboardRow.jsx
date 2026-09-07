import React from 'react';
import { Icon } from '../core/Icon.jsx';

export function ScoreboardRow({ rank, name, index, score, delta, rounds, woke, consumed, status, header, highlight, style, ...rest }) {
  const cell = { font: '400 var(--fs-heading)/1.2 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-2)', textAlign: 'right', flex: '0 0 auto', whiteSpace: 'nowrap' };
  const head = { ...cell, font: '500 var(--fs-micro)/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-3)' };
  const c = header ? head : cell;
  const col = index ? 'var(--sleeper-' + index + ')' : 'var(--fg-3)';
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 'var(--sp-5)', padding: header ? '0 var(--sp-4) var(--sp-3)' : 'var(--sp-3) var(--sp-4)',
      borderTop: header ? 'none' : 'var(--bw-hair) solid var(--line-faint)',
      background: highlight ? 'color-mix(in srgb, var(--mist-500) 50%, transparent)' : 'transparent', ...style
    }} {...rest}>
      {/* --fg-3, not --fg-4: at 22px this is body text, and the highlight wash
          pushes --fg-4 to 3.67:1. WCAG's 3:1 allowance starts at 24px. */}
      <span style={{ ...c, minWidth: 26, textAlign: 'left', color: 'var(--fg-3)' }}>{rank}</span>
      <span style={{ flex: '1 1 auto', minWidth: 0, display: 'flex', alignItems: 'center', gap: 'var(--sp-3)' }}>
        {/* Always emit the chip's footprint on a data row, painted only when the
            player has a Sleeper index. The Nightmare has no number (§1.5 gives
            numbers to Sleepers only), and omitting the element would outdent
            their name by the chip + gap on every leaderboard. */}
        {!header && <span aria-hidden={!index} style={{ width: 32, height: 32, flex: '0 0 auto', boxSizing: 'border-box', borderRadius: 'var(--r-full)', textAlign: 'center', font: '600 var(--fs-heading)/28px var(--font-body)', ...(index ? { border: '2px solid ' + col, background: col, color: '#07110b' } : null) }}>{index || ''}</span>}
        <span style={{ ...c, textAlign: 'left', color: header ? 'var(--fg-3)' : 'var(--text-strong)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{name}</span>
        {status && !header && <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', color: 'var(--fg-3)' }}>{status}</span>}
      </span>
      {rounds != null && <span style={{ ...c, minWidth: 124 }}>{rounds}</span>}
      {woke != null && <span style={{ ...c, minWidth: 64 }}>{woke}</span>}
      {consumed != null && <span style={{ ...c, minWidth: 92 }}>{consumed}</span>}
      {/* minWidth, NOT width: the box is flex-end aligned, so a fixed width makes
          over-long content escape leftward over the Consumed column instead of
          growing the box. 150 holds a five-digit score plus a four-char delta. */}
      <span style={{ ...c, minWidth: 150, color: header ? 'var(--fg-3)' : 'var(--fg-1)', display: 'flex', justifyContent: 'flex-end', alignItems: 'baseline', gap: 6 }}>
        {score}
        {delta != null && !header && <span style={{ font: '500 var(--fs-heading)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: delta > 0 ? 'var(--exit-500)' : 'var(--fg-4)' }}>{delta > 0 ? '+' + delta : delta}</span>}
      </span>
    </div>
  );
}
