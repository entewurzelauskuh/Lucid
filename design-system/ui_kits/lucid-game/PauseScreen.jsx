const { MistPanel, Button, Icon } = window.DesignSystem_427bea;

function PauseScreen({ under = 'hud', onResume, onOptions, onLobby }) {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone={under === 'god' ? 'god' : 'maze'} />
      {/* Away from the game: a one-shot captured, blurred frame is honest here. */}
      <Scrim>
        <MistPanel pad="var(--sp-8)" style={{ width: 460, borderRadius: 'var(--r-3)' }}>
          <div style={{ textAlign: 'center', marginBottom: 'var(--sp-7)' }}>
            <div style={{ font: '300 var(--fs-title)/1 var(--font-display)', letterSpacing: '0.14em', color: 'var(--fg-1)' }}>Paused</div>
            <div style={{ marginTop: 'var(--sp-3)', font: '400 var(--fs-body)/1.4 var(--font-body)', color: 'var(--fg-3)' }}>
              The dream keeps going — only your screen is still.
            </div>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-4)' }}>
            <Button variant="primary" full onClick={onResume} hotkey="Esc">Back to the dream</Button>
            <Button variant="secondary" full onClick={onOptions}>Options</Button>
            <Button variant="secondary" full onClick={onLobby}>Leave</Button>
            <Button variant="secondary" full>Quit to desktop</Button>
          </div>
          <p style={{ margin: 'var(--sp-6) 0 0', font: '400 var(--fs-micro)/1.5 var(--font-body)', color: 'var(--fg-4)', textAlign: 'center' }}>
            Leaving as the Nightmare ends the round: “The dream will collapse for all Sleepers.”
          </p>
          <div style={{ display: 'none' }}>
          </div>
        </MistPanel>
      </Scrim>
    </div>
  );
}
Object.assign(window, { PauseScreen });
