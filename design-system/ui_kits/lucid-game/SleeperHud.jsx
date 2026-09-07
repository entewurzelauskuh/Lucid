const { Icon, TimerArc, HealthRing, MoonLives, Crosshair, DamageArc, ToastStack, MistPanel, ScoreboardRow } = window.DesignSystem_427bea;

const HUD_STATES = {
  running: { seconds: 192, hp: 82, lives: 1, dark: 0, cross: 'idle',
    toasts: [{ text: 'A door hardened', icon: 'door-solid' }, { text: 'The exit moved', icon: 'door-exit', tone: 'exit' }] },
  dark:    { seconds: 148, hp: 64, lives: 1, dark: 0.72, cross: 'mob', effect: { icon: 'power-dark', text: 'Dark' },
    toasts: [{ text: 'Dark', icon: 'power-dark', tone: 'effect' }, { text: 'Cara was consumed', tone: 'danger' }] },
  last30:  { seconds: 27, hp: 38, lives: 1, dark: 0.15, cross: 'weak-point', progress: 0.55, damage: 'left',
    toasts: [{ text: 'Dawn in 0:30', icon: 'door-exit', tone: 'exit' }, { text: 'You lost a life — 1 moon left', tone: 'danger' }] }
};

function EffectChip({ icon, text }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', padding: '8px var(--sp-5)', borderRadius: 'var(--r-full)',
      background: 'color-mix(in srgb, var(--ink-900) 70%, transparent)', border: 'var(--bw-hair) solid var(--fog-500)',
      font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fog-300)', letterSpacing: '0.04em' }}>
      <Icon name={icon} size={18} />{text}
    </div>
  );
}

function DepthReadout({ depth = 7, best = 11 }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', textShadow: '0 1px 10px rgba(7,11,18,.95)' }}>
      <span style={{ color: 'var(--fg-3)' }}><Icon name="depth" size={20} /></span>
      <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)' }}>Depth {depth}</span>
      <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>deepest {best}</span>
    </div>
  );
}

function SleeperHud({ state = 'running', showTab }) {
  const s = HUD_STATES[state];
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="maze" dark={s.dark} />
      {s.damage && <DamageArc from={s.damage} intensity={0.85} />}

      {/* centre */}
      <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)' }}>
        <Crosshair state={s.cross} progress={s.progress || 0} size={s.cross === 'weak-point' ? 64 : 48} />
      </div>

      {/* top centre — dawn */}
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-6)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={s.seconds} total={300} size={190} phase={s.seconds <= 30 ? 'Dawn in 0:' + String(s.seconds).padStart(2, '0') : 'Dawn'} />
      </div>

      {/* top right — toasts */}
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', top: 'var(--hud-margin)' }}>
        <ToastStack toasts={s.toasts} />
      </div>

      {/* top left — effect chips */}
      {s.effect && (
        <div style={{ position: 'absolute', left: 'var(--hud-margin)', top: 'var(--hud-margin)' }}>
          <EffectChip {...s.effect} />
        </div>
      )}

      {/* bottom left — health, lives, depth */}
      <div style={{ position: 'absolute', left: 'var(--hud-margin)', bottom: 'var(--hud-margin)' }}>
        <ClusterScrim corner="bottom left">
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-6)' }}>
            <HealthRing hp={s.hp} size={84} />
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-4)' }}>
              <MoonLives lives={s.lives} max={1} size={26} />
              <DepthReadout depth={state === 'last30' ? 11 : 7} best={11} />
            </div>
          </div>
        </ClusterScrim>
      </div>

      {/* bottom right — what the keys do */}
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', bottom: 'var(--hud-margin)' }}>
        <ClusterScrim corner="bottom right">
          <div style={{ display: 'flex', gap: 'var(--sp-6)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)', textShadow: '0 1px 8px rgba(7,11,18,.95)' }}>
            <span><kbd style={{ color: 'var(--fg-1)' }}>LMB</kbd> jam</span>
            <span><kbd style={{ color: 'var(--fg-1)' }}>Shift</kbd> sprint</span>
            <span><kbd style={{ color: 'var(--fg-1)' }}>Tab</kbd> who is left</span>
          </div>
        </ClusterScrim>
      </div>

      {/* Tab overlay. No blur: the round is still running behind this, and a
          frozen blurred frame would be a lie. Held only while Tab is down. */}
      {showTab && (
        <Scrim blur={false}>
          <MistPanel tone="chrome" title="Who is left" aside="hold tab" style={{ width: 720 }}>
            <ScoreboardRow header rank="#" name="Player" score="Depth" />
            <ScoreboardRow rank={1} index={1} name="Anna" status="in the dream" score={11} />
            <ScoreboardRow rank={2} index={2} name="Ben" status="in the dream" score={8} />
            <ScoreboardRow rank={3} index={3} name="Cara" status="consumed" score={6} />
            <ScoreboardRow rank={4} index={4} name="Dev" status="awake" score={13} />
          </MistPanel>
        </Scrim>
      )}
    </div>
  );
}
Object.assign(window, { SleeperHud, EffectChip, DepthReadout });
