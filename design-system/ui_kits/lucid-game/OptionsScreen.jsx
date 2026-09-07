const { MistPanel, Button, Toggle, Slider, Icon, SleeperMarker } = window.DesignSystem_427bea;

const TABS = ['Game', 'Video', 'Audio', 'Controls', 'Accessibility'];

function OptionsScreen({ onBack }) {
  const [tab, setTab] = React.useState('Accessibility');
  const [shapes, setShapes] = React.useState(false);
  const [contrast, setContrast] = React.useState(true);
  const [shake, setShake] = React.useState(30);
  return (
    <div style={{ position: 'absolute', inset: 0, background: 'var(--ink-800)' }}>
      <DreamView tone="void" />
      <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', padding: 'var(--sp-8)', gap: 'var(--sp-6)' }}>
        <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between' }}>
          <h1 style={{ margin: 0, font: '300 var(--fs-title)/1 var(--font-display)', letterSpacing: '0.1em', color: 'var(--fg-1)' }}>Options</h1>
          <Button variant="secondary" hotkey="Esc" onClick={onBack}>Back</Button>
        </header>
        <div style={{ flex: 1, display: 'flex', gap: 'var(--sp-6)', minHeight: 0 }}>
          <nav style={{ flex: '0 0 240px', display: 'flex', flexDirection: 'column', gap: 'var(--sp-2)' }}>
            {TABS.map(t => (
              <button key={t} type="button" onClick={() => setTab(t)} style={{
                textAlign: 'left', padding: 'var(--sp-4) var(--sp-5)', minHeight: 'var(--hit-min)', cursor: 'pointer', borderRadius: 'var(--r-2)',
                background: tab === t ? 'color-mix(in srgb, var(--mist-500) 65%, transparent)' : 'transparent',
                border: 'var(--bw-hair) solid ' + (tab === t ? 'var(--line-strong)' : 'transparent'),
                font: '400 var(--fs-body)/1 var(--font-body)', color: tab === t ? 'var(--fg-1)' : 'var(--fg-3)' }}>{t}</button>
            ))}
          </nav>
          <MistPanel title={tab} style={{ flex: 1, overflow: 'auto' }}>
            {tab === 'Accessibility' ? (
              <>
                <Toggle label="Colour-blind marker shapes" hint="Each Sleeper also gets their own outline shape" checked={shapes} onChange={setShapes} />
                <div style={{ display: 'flex', gap: 'var(--sp-5)', padding: 'var(--sp-5) 0 var(--sp-6)' }}>
                  {[1, 2, 3, 4].map(i => <SleeperMarker key={i} index={i} name={['Anna', 'Ben', 'Cara', 'Dev'][i - 1]} shape={shapes} />)}
                </div>
                <Divider />
                <Toggle label="High-contrast doors" hint="A faint hatch on fog, rays on exits" checked={contrast} onChange={setContrast} />
                <Toggle label="Larger HUD text" hint="Everything one step up the scale" checked={false} onChange={() => {}} />
                <Toggle label="Reduce motion" hint="Pulses hold still; nothing else changes" checked={false} onChange={() => {}} />
                <Divider />
                <Slider label="Screen shake" value={shake} min={0} max={100} unit="%" onChange={setShake} />
                <Slider label="Subtitle size" value={16} min={12} max={28} unit="px" onChange={() => {}} />
              </>
            ) : tab === 'Game' ? (
              <>
                <Toggle label="Hint cards" hint="Shown for the first three rounds" checked onChange={() => {}} />
                <Toggle label="Show the build string on Results" checked onChange={() => {}} />
                <Divider />
                <Slider label="Field of view" value={95} min={70} max={110} unit="°" onChange={() => {}} />
                <Slider label="Mouse sensitivity" value={42} min={1} max={100} onChange={() => {}} />
              </>
            ) : (
              <div style={{ padding: 'var(--sp-8) 0', textAlign: 'center', font: '400 var(--fs-body)/1.5 var(--font-body)', color: 'var(--fg-4)' }}>
                {tab} settings follow the same two controls — Toggle and Slider — in the same order as the docs list them.
              </div>
            )}
          </MistPanel>
        </div>
      </div>
    </div>
  );
}
Object.assign(window, { OptionsScreen });
