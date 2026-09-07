const { MistPanel, Button, Icon } = window.DesignSystem_427bea;

function TitleScreen({ onHost, onOptions }) {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="bedroom" />
      {/* the bedroom's fog door, breathing, behind the wordmark */}
      <div aria-hidden="true" style={{ position: 'absolute', left: 0, right: 0, top: 0, height: 500, overflow: 'hidden', pointerEvents: 'none', maskImage: 'linear-gradient(180deg, #000 0%, #000 62%, transparent 96%)', WebkitMaskImage: 'linear-gradient(180deg, #000 0%, #000 62%, transparent 96%)' }}>
        <div style={{ position: 'absolute', left: '50%', top: 96, transform: 'translateX(-50%)', color: 'var(--fog-700)', opacity: 0.34, animation: 'lucid-breathe var(--pulse-door) var(--ease-in-out) infinite' }}>
          <Icon name="door-fog" size={620} strokeWidth={0.3} />
        </div>
      </div>
      <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 'var(--sp-9)' }}>
        <div style={{ textAlign: 'center' }}>
          <h1 style={{ margin: 0, font: '300 128px/1 var(--font-display)', letterSpacing: '0.24em', paddingLeft: '0.24em', color: 'var(--fg-1)', textShadow: '0 0 90px rgba(195,212,234,.4)' }}>LUCID</h1>
          <p style={{ margin: 'var(--sp-5) 0 0', font: '300 var(--fs-heading)/1 var(--font-display)', letterSpacing: '0.1em', color: 'var(--fg-3)' }}>One builds the dream. The rest have to wake up.</p>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-4)', width: 340 }}>
          <Button variant="primary" size="lg" full onClick={onHost}>Host a dream</Button>
          <Button variant="secondary" size="lg" full>Join</Button>
          <Button variant="secondary" size="lg" full>Sandbox</Button>
          <Button variant="secondary" size="lg" full onClick={onOptions}>Options</Button>
          <Button variant="secondary" size="lg" full>Quit</Button>
        </div>
      </div>
      <div style={{ position: 'absolute', left: 'var(--hud-margin)', bottom: 'var(--sp-6)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-4)' }}>v0.6.0-dev · MIT · Unity 6</div>
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', bottom: 'var(--sp-6)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-4)' }}>lucid.game</div>
    </div>
  );
}
Object.assign(window, { TitleScreen });
