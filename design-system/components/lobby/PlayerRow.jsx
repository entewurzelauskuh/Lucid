import React from 'react';
import { Icon } from '../core/Icon.jsx';

export function PlayerRow({ name, host, role, ready, self, onRole, onReady, style, ...rest }) {
  const card = active => ({
    display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', padding: '10px var(--sp-4)',
    minHeight: 'var(--hit-min)', borderRadius: 'var(--r-2)', cursor: self ? 'pointer' : 'default',
    background: active ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 30%, transparent)',
    border: 'var(--bw-hair) solid ' + (active ? 'var(--exit-500)' : 'var(--line)'),
    color: active ? 'var(--exit-300)' : 'var(--fg-3)',
    font: '400 var(--fs-body)/1 var(--font-body)',
    transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)'
  });
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 'var(--sp-5)', padding: 'var(--pad-row)',
      borderRadius: 'var(--r-2)',
      background: self ? 'color-mix(in srgb, var(--mist-500) 45%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (self ? 'var(--line)' : 'transparent'), ...style
    }} {...rest}>
      <div style={{ width: 44, height: 44, flex: '0 0 auto', borderRadius: 'var(--r-1)', background: 'var(--ink-800)', border: 'var(--bw-hair) solid var(--line)', display: 'flex', alignItems: 'center', justifyContent: 'center', font: '300 var(--fs-heading)/1 var(--font-display)', color: 'var(--fg-3)' }}>{(name || '?').slice(0, 1)}</div>
      <div style={{ flex: '1 1 auto', minWidth: 0, display: 'flex', alignItems: 'center', gap: 'var(--sp-3)' }}>
        <span style={{ font: '400 var(--fs-heading)/1.2 var(--font-body)', color: 'var(--text-strong)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{name}</span>
        {host && <span style={{ color: 'var(--exit-500)' }} title="Host"><Icon name="crown" size={16} /></span>}
      </div>
      <div style={{ display: 'flex', gap: 'var(--sp-3)', flex: '0 0 auto' }}>
        <div style={card(role === 'nightmare')} onClick={self && onRole ? () => onRole('nightmare') : undefined}>
          <Icon name="role-nightmare" size={20} /> Nightmare
        </div>
        <div style={card(role === 'sleeper')} onClick={self && onRole ? () => onRole('sleeper') : undefined}>
          <Icon name="role-sleeper" size={20} /> Sleeper
        </div>
      </div>
      <div onClick={self && onReady ? () => onReady(!ready) : undefined} title={ready ? 'Ready' : 'Not ready'} style={{
        display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', flex: '0 0 auto', width: 116,
        padding: '8px var(--sp-4)', minHeight: 'var(--hit-min)', boxSizing: 'border-box', borderRadius: 'var(--r-2)',
        cursor: self ? 'pointer' : 'default',
        border: 'var(--bw-hair) solid ' + (ready ? 'var(--sleeper-3)' : 'var(--line-faint)'),
        background: ready ? 'color-mix(in srgb, var(--sleeper-3) 16%, transparent)' : 'transparent',
        color: ready ? '#7ee0bd' : 'var(--fg-3)', font: '400 var(--fs-body)/1 var(--font-body)'
      }}>
        <Icon name={ready ? 'ready' : 'unready'} size={20} />{ready ? 'Ready' : 'Waiting'}
      </div>
    </div>
  );
}
