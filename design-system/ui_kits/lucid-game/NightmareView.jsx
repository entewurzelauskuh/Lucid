const { MistPanel, Button, Icon, TimerArc, CooldownRing, PaletteTile, ConnectorNet,
  SleeperMarker, RejectionLabel, CostBadge, ToastStack, Toggle } = window.DesignSystem_427bea;

/* NOTE: in-browser Babel downcompiles a top-level const in these classic
   scripts to a var, which lands on window. Every module-level name here is
   therefore file-prefixed, and shared game data lives ONLY in Shared.jsx
   (window.CUBES / EFFECTS / RULES) so there is one source of truth. */
const NV_PACKS = ['Core', 'Attic', 'Waterworks'];
const NV_CATS = [['cat-connector', 'Connectors'], ['cat-vertical', 'Vertical'], ['cat-chicane', 'Chicanes'], ['cat-mob', 'Mobs'], ['cat-gimmick', 'Gimmicks']];

/* Budget: the number, the rate, and a trickle ring that fills to the next point. */
function BudgetMeter({ budget = 12, trickle = 0.6, rate = '1 per 4 s' }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
      <CooldownRing progress={trickle} size={54}>
        <svg width="14" height="14" viewBox="0 0 8 8" aria-hidden="true"><path d="M4 0 8 4 4 8 0 4Z" fill="var(--exit-500)" /></svg>
      </CooldownRing>
      <div style={{ lineHeight: 1 }}>
        <div style={{ font: '400 var(--fs-title)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)' }}>{budget}</div>
        <div style={{ marginTop: 5, font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)' }}>{rate}</div>
      </div>
    </div>
  );
}

/* The god view: a flat plan of the maze, drawn from geometry only. */
function GodView({ layer = 2, ghost, rejected, possessing, selected }) {
  const cells = [[3,2],[4,2],[5,2],[5,3],[5,4],[6,4],[7,4],[7,3],[4,5],[4,6],[3,6],[2,6],[7,5],[8,5],[8,6],[6,2]];
  const S = 74, ox = 500, oy = 150;
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="god" dark={possessing ? 0.4 : 0} />
      <div style={{ position: 'absolute', inset: 0, opacity: 0.5, backgroundImage: 'linear-gradient(rgba(147,167,196,.09) 1px, transparent 1px), linear-gradient(90deg, rgba(147,167,196,.09) 1px, transparent 1px)', backgroundSize: S + 'px ' + S + 'px', backgroundPosition: ox + 'px ' + oy + 'px' }} />
      <svg style={{ position: 'absolute', inset: 0, width: '100%', height: '100%' }}>
        {cells.map(([cx, cy], i) => (
          <rect key={i} x={ox + cx * S + 3} y={oy + cy * S + 3} width={S - 6} height={S - 6} rx="3"
            fill="rgba(46,65,96,.5)" stroke="rgba(195,212,234,.3)" strokeWidth="1" />
        ))}
        {/* fog doors: dark matte squares on the frontier */}
        {[[8,4],[2,5],[9,6],[3,7]].map(([cx, cy], i) => (
          <rect key={'f' + i} x={ox + cx * S + 12} y={oy + cy * S + 12} width={S - 24} height={S - 24} rx="2"
            fill="rgba(92,114,149,.28)" stroke="var(--fog-500)" strokeWidth="1.5" strokeDasharray="4 3" />
        ))}
        {/* the exit: bright, radiant */}
        <g transform={'translate(' + (ox + 9 * S + S / 2) + ',' + (oy + 2 * S + S / 2) + ')'}>
          <circle r="26" fill="rgba(255,246,222,.12)" />
          <rect x="-14" y="-14" width="28" height="28" rx="2" fill="rgba(255,246,222,.7)" stroke="var(--exit-300)" strokeWidth="2" style={{ filter: 'drop-shadow(0 0 14px rgba(255,246,222,.8))' }} />
        </g>
        {/* ghost cube */}
        {ghost && (
          <rect x={ox + 8 * S + 3} y={oy + 4 * S + 3} width={S - 6} height={S - 6} rx="3"
            fill={rejected ? 'rgba(217,69,58,.28)' : 'rgba(0,158,115,.28)'}
            stroke={rejected ? 'var(--danger-500)' : 'var(--sleeper-3)'} strokeWidth="2" />
        )}
      </svg>
      {/* Sleeper markers projected into the overlay */}
      {[[4, 5, 1, 'Anna', 90], [6, 4, 2, 'Ben', 200], [3, 6, 4, 'Dev', 315]].map(([cx, cy, i, n, deg]) => (
        <div key={i} style={{ position: 'absolute', left: ox + cx * S + S / 2 - 40, top: oy + cy * S + S / 2 - 17 }}>
          <SleeperMarker index={i} name={n} facing={deg} selected={selected === i} />
        </div>
      ))}
      {/* a placed trap with its own cooldown ring */}
      <div style={{ position: 'absolute', left: ox + 5 * S + 14, top: oy + 3 * S + 14 }}>
        <CooldownRing progress={0.35} size={34} tone="fog"><Icon name="power-trigger" size={15} /></CooldownRing>
      </div>
      {ghost && rejected && (
        <div style={{ position: 'absolute', left: ox + 8 * S - 40, top: oy + 5 * S + 6 }}>
          <RejectionLabel reason="Would trap Ben" />
        </div>
      )}
      {/* layer cut-away */}
      <div style={{ position: 'absolute', left: 'var(--sp-6)', top: '50%', transform: 'translateY(-50%)', display: 'flex', flexDirection: 'column-reverse', gap: 'var(--sp-2)' }}>
        {[0, 1, 2, 3].map(l => (
          <div key={l} style={{ width: 40, height: 34, display: 'flex', alignItems: 'center', justifyContent: 'center', borderRadius: 'var(--r-1)',
            border: 'var(--bw-hair) solid ' + (l === layer ? 'var(--exit-500)' : 'var(--line-faint)'),
            background: l === layer ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 45%, transparent)',
            font: '400 var(--fs-micro)/1 var(--font-body)', color: l === layer ? 'var(--exit-300)' : 'var(--fg-4)' }}>{l}</div>
        ))}
      </div>
    </div>
  );
}

/* Live play behind it: scrim only, never a frozen blurred frame. */
function TargetSelector({ onPick, onClose }) {
  return (
    <Scrim blur={false}>
      <MistPanel tone="chrome" title="Who feels it" aside="tab · click · 0" style={{ width: 560 }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-3)' }}>
          {[[1, 'Anna', 'depth 11'], [2, 'Ben', 'depth 8'], [4, 'Dev', 'depth 6']].map(([i, n, d]) => (
            <button key={i} type="button" onClick={() => onPick && onPick(i)} style={{
              display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--sp-4)',
              padding: 'var(--pad-row)', minHeight: 'var(--hit-min)', cursor: 'pointer', textAlign: 'left',
              background: 'color-mix(in srgb, var(--ink-900) 34%, transparent)',
              border: 'var(--bw-hair) solid var(--line)', borderRadius: 'var(--r-2)' }}>
              <SleeperMarker index={i} name={n} />
              <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>{d}</span>
            </button>
          ))}
          <button type="button" onClick={() => onPick && onPick('all')} style={{
            display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', padding: 'var(--pad-row)', minHeight: 'var(--hit-min)',
            cursor: 'pointer', textAlign: 'left', background: 'color-mix(in srgb, var(--exit-500) 12%, transparent)',
            border: 'var(--bw-hair) solid var(--exit-500)', borderRadius: 'var(--r-2)', color: 'var(--exit-300)' }}>
            <Icon name="power-target" size={22} />
            <span style={{ font: '400 var(--fs-body)/1 var(--font-body)' }}>Everyone</span>
            <span style={{ marginLeft: 'auto', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)' }}>0</span>
          </button>
        </div>
        <div style={{ marginTop: 'var(--sp-5)', display: 'flex', justifyContent: 'flex-end' }}>
          <Button variant="secondary" hotkey="Esc" onClick={onClose}>Cancel</Button>
        </div>
      </MistPanel>
    </Scrim>
  );
}

function PossessionOverlay({ onRelease }) {
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="maze" dark={0.25} />
      <div aria-hidden="true" style={{ position: 'absolute', inset: 0, boxShadow: 'inset 0 0 220px 40px rgba(204,121,167,.28), inset 0 0 0 2px rgba(204,121,167,.5)' }} />
      <div style={{ position: 'absolute', left: '50%', top: 'var(--hud-margin)', transform: 'translateX(-50%)', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 'var(--sp-4)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', padding: '10px var(--sp-6)', borderRadius: 'var(--r-full)',
          background: 'color-mix(in srgb, var(--ink-900) 74%, transparent)', border: 'var(--bw-hair) solid var(--sleeper-4)', color: '#f0c4dc' }}>
          <Icon name="power-possess" size={20} />
          <span style={{ font: '400 var(--fs-heading)/1 var(--font-body)' }}>Possessing a Shade in Anna's dream — P to let go</span>
        </div>
      </div>
      <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)' }}>
        <Crosshair state="mob" size={48} />
      </div>
      <div style={{ position: 'absolute', left: 'var(--hud-margin)', bottom: 'var(--hud-margin)' }}>
        <ClusterScrim corner="bottom left">
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)' }}>
            <CooldownRing progress={0.7} size={64} tone="danger" label="9s"><Icon name="power-possess" size={22} /></CooldownRing>
            <div style={{ font: '400 var(--fs-body)/1.4 var(--font-body)', color: 'var(--fg-2)', textShadow: '0 1px 10px rgba(7,11,18,.95)' }}>
              <span style={{ fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)' }}>16</span> budget<br />
              <span style={{ color: 'var(--danger-300)', fontSize: 'var(--fs-micro)' }}>Building paused</span>
            </div>
          </div>
        </ClusterScrim>
      </div>
      <div style={{ position: 'absolute', right: 'var(--hud-margin)', bottom: 'var(--hud-margin)' }}>
        <Button variant="secondary" hotkey="P" onClick={onRelease}>Let go</Button>
      </div>
    </div>
  );
}

function NightmareView({ mode = 'idle', onMode }) {
  const [pack, setPack] = React.useState('Core');
  const [cat, setCat] = React.useState('cat-connector');
  const [pick, setPick] = React.useState('Straight');
  const [target, setTarget] = React.useState(false);
  const budget = mode === 'placing' ? 3 : 12;
  const shown = (window.CUBES || []).filter(c => cat === 'all' || c.category === cat);

  if (mode === 'possession') return <PossessionOverlay onRelease={() => onMode && onMode('idle')} />;

  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <GodView ghost={mode === 'placing'} rejected={mode === 'placing'} selected={target ? 1 : null} />

      {/* left dock — palette */}
      <div style={{ position: 'absolute', left: 0, top: 0, bottom: 0, width: 340, display: 'flex' }}>
        <MistPanel tone="chrome" edge="top" pad="var(--sp-5)" style={{ flex: 1, borderLeft: 0, borderTop: 0, borderBottom: 0, borderRadius: 0 }}>
          <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Palette</MicroLabel>
          <div style={{ display: 'flex', gap: 'var(--sp-2)', marginBottom: 'var(--sp-4)' }}>
            {NV_PACKS.map(p => (
              <button key={p} type="button" onClick={() => setPack(p)} style={{
                flex: 1, padding: '8px 0', cursor: 'pointer', borderRadius: 'var(--r-1)',
                background: pack === p ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'transparent',
                border: 'var(--bw-hair) solid ' + (pack === p ? 'var(--line-strong)' : 'var(--line-faint)'),
                font: '400 var(--fs-body)/1 var(--font-body)', color: pack === p ? 'var(--fg-1)' : 'var(--fg-3)' }}>{p}</button>
            ))}
          </div>
          <div style={{ display: 'flex', gap: 'var(--sp-2)', flexWrap: 'wrap', marginBottom: 'var(--sp-4)' }}>
            {NV_CATS.map(([c, label]) => (
              <button key={c} type="button" title={label} onClick={() => setCat(c)} style={{
                display: 'flex', alignItems: 'center', gap: 5, padding: '5px 8px', cursor: 'pointer', borderRadius: 'var(--r-1)',
                background: cat === c ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
                border: 'var(--bw-hair) solid ' + (cat === c ? 'var(--exit-500)' : 'var(--line-faint)'),
                color: cat === c ? 'var(--exit-300)' : 'var(--fg-3)', font: '400 11px/1 var(--font-body)' }}>
                <Icon name={c} size={16} />{label}
              </button>
            ))}
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--sp-3)', overflow: 'auto' }}>
            {shown.slice(0, 9).map((c, i) => (
              <PaletteTile key={c.name} name={c.name} cost={c.cost} mask={c.mask} category={c.category}
                hotkey={i + 1} budget={budget} selected={pick === c.name} onClick={() => setPick(c.name)} />
            ))}
          </div>
          <div style={{ flex: 1 }} />
          <Divider />
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <MicroLabel>Budget</MicroLabel>
            <BudgetMeter budget={budget} />
          </div>
        </MistPanel>
      </div>

      {/* right dock — Sleepers */}
      <div style={{ position: 'absolute', right: 0, top: 0, bottom: 0, width: 320, display: 'flex' }}>
        <MistPanel tone="chrome" pad="var(--sp-5)" style={{ flex: 1, borderRight: 0, borderTop: 0, borderBottom: 0, borderRadius: 0 }}>
          <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Sleepers</MicroLabel>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-3)' }}>
            {[[1, 'Anna', 11, 82, 3], [2, 'Ben', 8, 54, 2], [4, 'Dev', 6, 100, 3]].map(([i, n, d, hp, lives]) => (
              <div key={i} style={{ padding: 'var(--pad-row)', borderRadius: 'var(--r-2)', border: 'var(--bw-hair) solid var(--line)', background: 'color-mix(in srgb, var(--ink-900) 32%, transparent)' }}>
                <SleeperMarker index={i} name={n} />
                <div style={{ marginTop: 'var(--sp-3)', display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)' }}>
                  <span>depth {d}</span>
                  <span style={{ flex: 1, height: 3, borderRadius: 2, background: 'var(--line-faint)', position: 'relative' }}>
                    <span style={{ position: 'absolute', inset: '0 auto 0 0', width: hp + '%', background: hp <= 30 ? 'var(--danger-300)' : 'var(--fog-300)', borderRadius: 2 }} />
                  </span>
                  <MoonLives lives={lives} max={3} size={13} />
                </div>
              </div>
            ))}
            <div style={{ padding: 'var(--pad-row)', borderRadius: 'var(--r-2)', border: '1px dashed var(--line-faint)' }}>
              <SleeperMarker index={3} name="Cara" status="consumed" />
            </div>
          </div>
          <div style={{ flex: 1 }} />
          <Divider />
          <MicroLabel style={{ marginBottom: 'var(--sp-3)' }}>This dream</MicroLabel>
          <div style={{ display: 'flex', justifyContent: 'space-between', font: '400 var(--fs-body)/1.8 var(--font-body)', color: 'var(--fg-2)' }}>
            <span>Cubes placed</span><span style={{ fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)' }}>34</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', font: '400 var(--fs-body)/1.8 var(--font-body)', color: 'var(--fg-2)' }}>
            <span>Doors hardened</span><span style={{ fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)' }}>3</span>
          </div>
        </MistPanel>
      </div>

      {/* top centre — dawn and phase */}
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-5)', transform: 'translateX(-50%)', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 'var(--sp-3)' }}>
        <TimerArc seconds={148} total={300} size={180} phase="The Sleepers are running" />
      </div>

      {/* toasts */}
      <div style={{ position: 'absolute', right: 340, top: 'var(--sp-6)', marginRight: 'var(--sp-6)' }}>
        <ToastStack toasts={mode === 'placing'
          ? [{ text: 'Not enough budget (3 / 4)', tone: 'danger' }]
          : [{ text: 'Ben found the exit', icon: 'door-exit', tone: 'exit' }, { text: 'Cara was consumed', tone: 'danger' }]} />
      </div>

      {/* bottom centre — powers bar */}
      <div style={{ position: 'absolute', left: 340, right: 320, bottom: 0, display: 'flex', justifyContent: 'center' }}>
        <MistPanel tone="chrome" edge="bottom" pad="var(--sp-4) var(--sp-6)" style={{ display: 'flex', flexDirection: 'row', alignItems: 'center', gap: 'var(--sp-6)', borderBottom: 0 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)' }}>
            {[['power-dark', 'Dark', 'Q', 0], ['power-fog', 'Fog', 'W', 0.62], ['power-molasses', 'Molasses', 'E', 0.2]].map(([ic, label, key, prog]) => (
              <div key={label} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6 }}>
                <CooldownRing progress={prog} size={58} label={prog > 0 ? String(Math.round(prog * 30)) : key}><Icon name={ic} size={22} /></CooldownRing>
                <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: prog > 0 ? 'var(--fg-4)' : 'var(--fg-2)' }}>{label}</span>
                <CostBadge cost={2} budget={budget} size="sm" />
              </div>
            ))}
          </div>
          <div style={{ width: 1, alignSelf: 'stretch', background: 'var(--line-faint)' }} />
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)' }}>
            <button type="button" onClick={() => setTarget(true)} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6, background: 'none', border: 0, cursor: 'pointer', padding: 0 }}>
              <CooldownRing progress={0} size={58} tone="fog"><Icon name="power-target" size={22} /></CooldownRing>
              <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-2)' }}>Who feels it</span>
              <span style={{ font: '400 11px/1 var(--font-body)', color: 'var(--fg-4)' }}>Tab · 0</span>
            </button>
            <button type="button" onClick={() => onMode && onMode('possession')} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6, background: 'none', border: 0, cursor: 'pointer', padding: 0 }}>
              <CooldownRing progress={0} size={58}><Icon name="power-possess" size={22} /></CooldownRing>
              <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-2)' }}>Possess</span>
              <CostBadge cost={3} budget={budget} size="sm" />
            </button>
          </div>
        </MistPanel>
      </div>

      {target && <TargetSelector onPick={() => setTarget(false)} onClose={() => setTarget(false)} />}
    </div>
  );
}
Object.assign(window, { NightmareView, GodView, TargetSelector, PossessionOverlay, BudgetMeter });
