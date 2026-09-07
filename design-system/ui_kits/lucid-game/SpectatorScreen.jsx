const { Icon, TimerArc, HealthRing, MoonLives, ToastStack, SleeperMarker, Button } = window.DesignSystem_427bea;

function SpectatorScreen() {
  const [watching, setWatching] = React.useState(2);
  const alive = [{ index: 2, name: 'Ben' }, { index: 4, name: 'Dev' }];
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="maze" dark={0.1} />
      <div style={{ position: 'absolute', inset: 0, boxShadow: 'inset 0 0 0 3px color-mix(in srgb, var(--sleeper-' + watching + ') 55%, transparent)', pointerEvents: 'none' }} />

      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-6)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={148} total={300} size={190} phase="Dawn" />
      </div>
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', top: 'var(--hud-margin)' }}>
        <ToastStack toasts={[{ text: 'Cara was consumed', tone: 'danger' }, { text: 'The exit moved', icon: 'door-exit', tone: 'exit' }]} />
      </div>

      <div style={{ position: 'absolute', left: 'var(--hud-margin)', top: 'var(--hud-margin)' }}>
        <ClusterScrim corner="top left">
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
            <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', color: 'var(--fg-1)' }}>You're awake. Watch the others.</span>
            <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>watching</span>
            <SleeperMarker index={watching} name={alive.find(a => a.index === watching).name} />
          </div>
        </ClusterScrim>
      </div>

      {/* the watched Sleeper's own vitals, quoted rather than owned */}
      <div style={{ position: 'absolute', left: 'var(--hud-margin)', bottom: 'var(--hud-margin)' }}>
        <ClusterScrim corner="bottom left">
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-6)', opacity: 0.85 }}>
            <HealthRing hp={54} size={72} />
            <MoonLives lives={2} max={3} size={24} />
            <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', color: 'var(--fg-2)', textShadow: '0 1px 10px rgba(7,11,18,.95)' }}>Depth 8</span>
          </div>
        </ClusterScrim>
      </div>

      <div style={{ position: 'absolute', left: '50%', bottom: 'var(--hud-margin)', transform: 'translateX(-50%)' }}>
        <ClusterScrim corner="bottom center">
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
            {alive.map(a => (
              <button key={a.index} type="button" onClick={() => setWatching(a.index)} style={{
                display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', padding: '8px var(--sp-5)', minHeight: 'var(--hit-min)',
                cursor: 'pointer', borderRadius: 'var(--r-2)',
                background: watching === a.index ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'color-mix(in srgb, var(--ink-900) 55%, transparent)',
                border: 'var(--bw-hair) solid ' + (watching === a.index ? 'var(--line-strong)' : 'var(--line)')
              }}>
                <SleeperMarker index={a.index} name={a.name} size={28} />
              </button>
            ))}
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', padding: '8px var(--sp-5)', minHeight: 'var(--hit-min)', borderRadius: 'var(--r-2)', background: 'color-mix(in srgb, var(--ink-900) 55%, transparent)', border: 'var(--bw-hair) solid var(--line)', color: 'var(--fg-2)' }}>
              <Icon name="role-nightmare" size={20} />
              <span style={{ font: '400 var(--fs-body)/1 var(--font-body)' }}>God view</span>
            </div>
            <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-4)', marginLeft: 'var(--sp-3)' }}>← → to change</span>
          </div>
        </ClusterScrim>
      </div>
    </div>
  );
}
Object.assign(window, { SpectatorScreen });
