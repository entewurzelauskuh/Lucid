const { MistPanel, Icon, TimerArc, SleeperMarker } = window.DesignSystem_427bea;

function RoundStartScreen({ stage = 'reveal' }) {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="bedroom" dark={0.35} />
      <Scrim>
        {stage === 'count' ? (
          <div style={{ font: '300 220px/1 var(--font-display)', fontVariantNumeric: 'lining-nums tabular-nums', fontFeatureSettings: 'var(--numeric)', color: 'var(--fg-1)', textShadow: '0 0 100px rgba(195,212,234,.35)' }}>2</div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 'var(--sp-8)' }}>
            <MistPanel pad="var(--sp-8)" style={{ width: 620, alignItems: 'center', textAlign: 'center', borderRadius: 'var(--r-3)' }}>
              <MicroLabel>Round 3</MicroLabel>
              <div style={{ margin: 'var(--sp-6) 0 var(--sp-5)', color: 'var(--exit-300)' }}><Icon name="role-nightmare" size={72} strokeWidth={1.1} /></div>
              <div style={{ font: '300 var(--fs-title)/1.2 var(--font-display)', color: 'var(--fg-2)' }}>Tonight's Nightmare is…</div>
              <div style={{ font: '600 var(--fs-display)/1.1 var(--font-display)', color: 'var(--exit-300)', textShadow: '0 0 60px rgba(255,227,163,.35)' }}>Anna</div>
              <div style={{ marginTop: 'var(--sp-6)', font: '400 var(--fs-body)/1.5 var(--font-body)', color: 'var(--fg-3)', maxWidth: 400 }}>
                You are a Sleeper. Find the deepest fog door before dawn.
              </div>
            </MistPanel>
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-7)' }}>
              {PLAYERS.slice(1).map(p => <SleeperMarker key={p.index} index={p.index} name={p.name} />)}
            </div>
            <TimerArc seconds={300} total={300} size={190} phase="The Sleepers stir in 0:30" urgent={false} />
          </div>
        )}
      </Scrim>
    </div>
  );
}
Object.assign(window, { RoundStartScreen });
