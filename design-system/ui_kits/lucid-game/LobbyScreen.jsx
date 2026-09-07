const { MistPanel, Button, Toggle, Slider, Icon, PlayerRow, ScoreboardRow } = window.DesignSystem_427bea;

function LobbyScreen({ onStart, onBack }) {
  const [role, setRole] = React.useState('nightmare');
  const [ready, setReady] = React.useState(false);
  const [headStart, setHeadStart] = React.useState(30);
  const [dawn, setDawn] = React.useState(300);
  const [hints, setHints] = React.useState(true);
  const others = [
    { name: 'Ben', role: 'sleeper', ready: true },
    { name: 'Cara', role: 'sleeper', ready: true },
    { name: 'Dev', role: 'sleeper', ready: false }
  ];
  const allReady = ready && others.every(o => o.ready);
  const reason = !ready ? 'Ready up to start' : !allReady ? 'Waiting for Dev to ready up' : null;
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="void" />
      <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', padding: 'var(--sp-8)', gap: 'var(--sp-6)' }}>
        <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between' }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 'var(--sp-6)' }}>
            <span style={{ font: '300 var(--fs-title)/1 var(--font-display)', letterSpacing: '0.2em', color: 'var(--fg-1)' }}>LUCID</span>
            <MicroLabel>Lobby · 4 of 5</MicroLabel>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)' }}>
            <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>Invite code</span>
            <span style={{ font: '500 var(--fs-heading)/1 var(--font-body)', letterSpacing: '0.2em', color: 'var(--exit-500)', padding: '8px var(--sp-5)', border: 'var(--bw-hair) solid var(--line-strong)', borderRadius: 'var(--r-2)' }}>MOTH-914</span>
            <Button variant="secondary" hotkey="Esc" onClick={onBack}>Leave</Button>
          </div>
        </header>
        <div style={{ flex: 1, display: 'flex', gap: 'var(--sp-6)', minHeight: 0 }}>
          <MistPanel title="Who is dreaming" aside="pick a role" style={{ flex: '1 1 auto' }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-2)' }}>
              <PlayerRow name="Anna" host self role={role} ready={ready} onRole={setRole} onReady={setReady} />
              {others.map(o => <PlayerRow key={o.name} {...o} />)}
              <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)', padding: 'var(--pad-row)', border: '1px dashed var(--line-faint)', borderRadius: 'var(--r-2)', minHeight: 68 }}>
                <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-4)' }}>One seat free — share MOTH-914</span>
              </div>
            </div>
            <Divider />
            <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Session leaderboard</MicroLabel>
            <ScoreboardRow header rank="#" name="Player" rounds="As Nightmare" woke="Woke" consumed="Consumed" score="Score" />
            <ScoreboardRow rank={1} index={2} name="Ben" rounds={1} woke={3} consumed={1} score={612} />
            <ScoreboardRow rank={2} index={1} name="Anna" rounds={2} woke={2} consumed={2} score={430} highlight />
            <ScoreboardRow rank={3} index={3} name="Cara" rounds={0} woke={1} consumed={3} score={205} />
            <ScoreboardRow rank={4} index={4} name="Dev" rounds={0} woke={1} consumed={3} score={188} />
          </MistPanel>
          <MistPanel title="The dream" aside="host only" style={{ flex: '0 0 420px' }}>
            <Slider label="Head start" value={headStart} min={5} max={120} unit="s" onChange={setHeadStart} />
            <Slider label="Dawn" value={dawn} min={120} max={900} step={30} onChange={setDawn} format={s => Math.floor(s / 60) + ':' + String(s % 60).padStart(2, '0')} />
            <Slider label="Sleeper lives" value={1} min={1} max={5} onChange={() => {}} />
            <Divider />
            <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Packs</MicroLabel>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-2)' }}>
              <Toggle label="Core" hint="Connectors, shafts, chicanes" checked disabled />
              <Toggle label="Attic" hint="Dust, low ceilings, a nest" checked onChange={() => {}} />
              <Toggle label="Waterworks" hint="Molasses runs slower here" checked={false} onChange={() => {}} />
            </div>
            <Divider />
            <Toggle label="Hint cards" hint="Shown for the first three rounds" checked={hints} onChange={setHints} />
            <div style={{ flex: 1 }} />
            <Button variant="primary" size="lg" full reason={reason} onClick={onStart} style={{ marginTop: 'var(--sp-6)' }}>Start the dream</Button>
          </MistPanel>
        </div>
      </div>
    </div>
  );
}
Object.assign(window, { LobbyScreen });
