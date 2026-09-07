const { MistPanel, Icon, TimerArc, HealthRing, MoonLives, Crosshair, DamageArc, ToastStack } = window.DesignSystem_427bea;

/* A doorway in the world, drawn as geometry: the mist sheet is the fill. */
function Doorway({ state = 'fog', w = 300, h = 430, x, y, label, countdown }) {
  const skin = {
    fog:      { fill: 'radial-gradient(60% 70% at 50% 45%, rgba(92,114,149,.5) 0%, rgba(35,48,70,.72) 100%)', edge: 'var(--fog-700)', glow: 'none' },
    exit:     { fill: 'radial-gradient(52% 58% at 50% 46%, rgba(255,251,240,.88) 0%, rgba(255,238,198,.42) 46%, rgba(255,227,163,.14) 100%)', edge: 'rgba(255,246,222,.75)', glow: '0 0 120px 34px rgba(255,246,222,.34)' },
    solid:    { fill: 'repeating-linear-gradient(45deg, rgba(74,85,104,.9) 0 6px, rgba(58,68,84,.9) 6px 12px)', edge: 'var(--door-solid)', glow: 'none' },
    attached: { fill: 'linear-gradient(180deg, rgba(10,15,24,.92) 0%, rgba(18,26,40,.8) 100%)', edge: 'rgba(195,212,234,.34)', glow: 'none' }
  }[state];
  return (
    <div style={{ position: 'absolute', left: x, top: y, width: w, height: h }}>
      <div style={{ position: 'absolute', inset: 0, borderRadius: '4px 4px 0 0', border: '2px solid ' + skin.edge, background: skin.fill, boxShadow: skin.glow }} />
      {countdown && (
        <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 10 }}>
          <span style={{ font: '300 96px/1 var(--font-display)', fontVariantNumeric: 'lining-nums tabular-nums', fontFeatureSettings: 'var(--numeric)', color: 'var(--fg-1)', textShadow: '0 0 40px rgba(7,11,18,.9)' }}>{countdown}</span>
          <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', letterSpacing: '0.1em', color: 'var(--fg-2)' }}>{label}</span>
        </div>
      )}
      {!countdown && label && (
        <div style={{ position: 'absolute', left: '50%', bottom: -34, transform: 'translateX(-50%)', font: '400 var(--fs-micro)/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-3)', whiteSpace: 'nowrap' }}>{label}</div>
      )}
    </div>
  );
}

/* Round-start hint cards — first three rounds only (UI.md §5). */
function HintCards() {
  const cards = [
    ['door-fog', 'grey mist', 'closed for now', 'var(--fog-500)'],
    ['door-exit', 'white light', 'the way out', 'var(--exit-300)'],
    ['door-solid', 'hardened wall', "you've been here", 'var(--door-solid)']
  ];
  return (
    <div style={{ display: 'flex', gap: 'var(--sp-5)' }}>
      {cards.map(c => (
        <MistPanel key={c[1]} pad="var(--sp-5)" style={{ width: 250, alignItems: 'center', textAlign: 'center' }}>
          <Icon name={c[0]} size={44} color={c[3]} strokeWidth={1.3} />
          <div style={{ marginTop: 'var(--sp-4)', font: '400 var(--fs-heading)/1.1 var(--font-body)', color: 'var(--fg-1)' }}>{c[1]}</div>
          <div style={{ marginTop: 4, font: '400 var(--fs-body)/1.3 var(--font-body)', color: 'var(--fg-3)' }}>{c[2]}</div>
        </MistPanel>
      ))}
    </div>
  );
}

/* Molasses readout: the viscous vignette plus the 70 % icon (UI.md §6). */
function MolassesOverlay({ secondsLeft }) {
  return (
    <React.Fragment>
      <div aria-hidden="true" style={{ position: 'absolute', inset: 0, pointerEvents: 'none',
        background: 'radial-gradient(70% 60% at 50% 50%, transparent 30%, rgba(60,44,20,.42) 100%)',
        boxShadow: 'inset 0 0 200px 60px rgba(48,36,16,.5)' }} />
      <div style={{ position: 'absolute', left: 'var(--hud-margin)', top: 'var(--hud-margin)', display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', padding: '8px var(--sp-5)', borderRadius: 'var(--r-full)',
          background: 'color-mix(in srgb, var(--ink-900) 72%, transparent)', border: 'var(--bw-hair) solid var(--fog-500)', color: 'var(--fog-300)' }}>
          <Icon name="power-molasses" size={20} />
          <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums' }}>70 %</span>
        </div>
        <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)', textShadow: '0 1px 10px rgba(7,11,18,.95)' }}>{secondsLeft} s left</span>
      </div>
    </React.Fragment>
  );
}

/* Dark dims the HUD (UI.md §6) but health and lives carry rules, so the floor is
   0.6 — at 0.32 they measured ~1.7:1 and were effectively erased, which §1.3
   forbids. Only the non-rule key hints go lower. */
function SleeperVitals({ hp, lives, depth, exit, dim }) {
  return (
    <div style={{ position: 'absolute', left: 'var(--hud-margin)', bottom: 'var(--hud-margin)', opacity: dim ? 0.6 : 1, transition: 'opacity var(--dur-base) var(--ease-out)' }}>
      <ClusterScrim corner="bottom left">
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-6)' }}>
          <HealthRing hp={hp} size={84} />
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-4)' }}>
            <MoonLives lives={lives} max={window.RULES.lives} size={26} />
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', textShadow: '0 1px 10px rgba(7,11,18,.95)' }}>
              <span style={{ color: 'var(--fg-3)' }}><Icon name="depth" size={20} /></span>
              <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: depth === exit ? 'var(--exit-500)' : 'var(--fg-1)' }}>depth {depth}</span>
              <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>exit {exit}</span>
            </div>
          </div>
        </div>
      </ClusterScrim>
    </div>
  );
}

function SleeperKeys({ dim }) {
  return (
    <div style={{ position: 'absolute', right: 'var(--hud-margin)', bottom: 'var(--hud-margin)', opacity: dim ? 0.32 : 1 }}>
      <ClusterScrim corner="bottom right">
        <div style={{ display: 'flex', gap: 'var(--sp-6)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)', textShadow: '0 1px 8px rgba(7,11,18,.95)' }}>
          <span><kbd style={{ color: 'var(--fg-1)' }}>LMB</kbd> Nightlight</span>
          <span><kbd style={{ color: 'var(--fg-1)' }}>Ctrl</kbd> crouch</span>
          <span><kbd style={{ color: 'var(--fg-1)' }}>Tab</kbd> who is left</span>
        </div>
      </ClusterScrim>
    </div>
  );
}

/* ---------------------------------------------------------------- 1 of 3
   The bedroom. The start cube's one door is misted until the head start ends,
   with the countdown projected onto it. Hint cards, first three rounds only. */
function SleeperBedroom() {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="bedroom" />
      <Doorway state="fog" x={810} y={290} w={300} h={430} countdown="0:12" label="the door is misted" />
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-6)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={300} total={300} size={190} phase="The Sleepers stir in 0:12" urgent={false} />
      </div>
      <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)' }}>
        <Crosshair size={48} />
      </div>
      <SleeperVitals hp={100} lives={1} depth={0} exit={2} />
      <SleeperKeys />
      <div style={{ position: 'absolute', left: '50%', bottom: 'var(--sp-9)', transform: 'translateX(-50%)' }}>
        <HintCards />
      </div>
    </div>
  );
}

/* ---------------------------------------------------------------- 2 of 3
   Molasses at 70 % while jamming a Trapdoor latch: six shots, about 1.5 s of
   standing still — which is exactly the window the Nightmare bought. */
function SleeperJamming() {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="maze" />
      <Doorway state="attached" x={250} y={330} w={230} h={340} label="attached" />
      <Doorway state="solid" x={1440} y={340} w={210} h={320} label="hardened — you've been here" />
      <MolassesOverlay secondsLeft={4} />
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-6)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={148} total={300} size={190} phase="The Sleepers are running" />
      </div>
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', top: 'var(--hud-margin)' }}>
        <ToastStack toasts={[{ text: "Molasses — don't jump", icon: 'power-molasses', tone: 'effect' }]} />
      </div>
      <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 'var(--sp-6)' }}>
        <Crosshair state="weak-point" progress={4 / window.RULES.jamShots} size={72} />
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-2)', textShadow: '0 1px 12px rgba(7,11,18,.95)' }}>
          <Icon name="weak-point" size={18} color="var(--exit-300)" />
          latch · 4 of 6
        </div>
      </div>
      <SleeperVitals hp={62} lives={1} depth={9} exit={11} />
      <SleeperKeys />
    </div>
  );
}

/* ---------------------------------------------------------------- 3 of 3
   Dark, in the last half minute, with an exit in sight. Dark puts every light
   out and dims the HUD — except the timer — and fog doors still glow. */
function SleeperDark() {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="maze" dark={0.82} />
      <div aria-hidden="true" style={{ position: 'absolute', left: '50%', top: '52%', transform: 'translate(-50%,-50%)', width: 900, height: 640,
        background: 'radial-gradient(50% 50% at 50% 50%, rgba(214,226,246,.16) 0%, transparent 70%)', pointerEvents: 'none' }} />
      <Doorway state="exit" x={835} y={318} w={250} h={370} label="the way out" />
      <Doorway state="fog" x={300} y={356} w={200} h={300} label="closed for now" />
      <DamageArc from="right" intensity={0.7} />
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-6)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={18} total={300} size={190} phase="Dawn in 0:18" />
      </div>
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', top: 'var(--hud-margin)', opacity: 0.32 }}>
        <ToastStack toasts={[
          { text: 'Dark', icon: 'power-dark', tone: 'effect' },
          { text: 'The exit moved', icon: 'door-exit', tone: 'exit' }
        ]} />
      </div>
      <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)' }}>
        <Crosshair size={48} />
      </div>
      <SleeperVitals hp={24} lives={1} depth={11} exit={11} dim />
      <SleeperKeys dim />
    </div>
  );
}

Object.assign(window, { SleeperBedroom, SleeperJamming, SleeperDark, Doorway, HintCards, MolassesOverlay, SleeperVitals, SleeperKeys });
