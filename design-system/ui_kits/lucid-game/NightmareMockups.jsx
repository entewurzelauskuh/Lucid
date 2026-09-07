const { MistPanel, Button, Icon, TimerArc, CooldownRing, PaletteTile, ConnectorNet,
  SleeperMarker, RejectionLabel, CostBadge, ToastStack, MoonLives } = window.DesignSystem_427bea;

const NM_CATS = [['cat-connector', 'Connectors'], ['cat-vertical', 'Vertical'], ['cat-chicane', 'Chicanes'], ['cat-mob', 'Mobs']];

/* Budget lives TOP LEFT (UI.md §8): the number, the trickle ring, the rate. */
function BudgetCluster({ budget, trickle = 0.5, paused }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
      <CooldownRing progress={trickle} size={58}>
        <svg width="15" height="15" viewBox="0 0 8 8" aria-hidden="true"><path d="M4 0 8 4 4 8 0 4Z" fill="var(--exit-500)" /></svg>
      </CooldownRing>
      <div style={{ lineHeight: 1 }}>
        <div style={{ font: '400 var(--fs-title)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-1)', textShadow: '0 1px 12px rgba(7,11,18,.9)' }}>{budget}</div>
        <div style={{ marginTop: 5, font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-3)' }}>{window.RULES.trickle}</div>
        {paused && <div style={{ marginTop: 5, font: '500 var(--fs-micro)/1 var(--font-body)', color: 'var(--danger-300)' }}>Building paused</div>}
      </div>
    </div>
  );
}

/* One Sleeper row (UI.md §8): chip + number, name, status, health bar, moons,
   "depth 7 · exit 11", last event. Expanding shows that dream's live state. */
function SleeperRow({ index, name, status, hp, lives, depth, exit, event, selected, expanded, mobs, jammed, onClick }) {
  const out = status === 'awake' || status === 'consumed';
  return (
    <div onClick={onClick} style={{
      padding: 'var(--pad-row)', borderRadius: 'var(--r-2)', cursor: 'pointer',
      border: 'var(--bw-hair) solid ' + (selected ? 'var(--exit-500)' : out ? 'var(--line-faint)' : 'var(--line)'),
      background: selected ? 'color-mix(in srgb, var(--exit-500) 10%, transparent)' : 'color-mix(in srgb, var(--ink-900) 32%, transparent)',
      opacity: out ? 0.62 : 1
    }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--sp-3)' }}>
        <SleeperMarker index={index} name={name} status={out ? status : 'in-dream'} />
        {!out && <MoonLives lives={lives} max={window.RULES.lives} size={15} />}
      </div>
      {!out && (
        <>
          <div style={{ marginTop: 'var(--sp-3)', display: 'flex', alignItems: 'center', gap: 'var(--sp-3)' }}>
            <span style={{ flex: 1, height: 3, borderRadius: 2, background: 'var(--line-faint)', position: 'relative' }}>
              <span style={{ position: 'absolute', inset: '0 auto 0 0', width: hp + '%', background: hp <= 30 ? 'var(--danger-300)' : 'var(--fog-300)', borderRadius: 2 }} />
            </span>
            <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: hp <= 30 ? 'var(--danger-300)' : 'var(--fg-3)', width: 30, textAlign: 'right' }}>{hp}</span>
          </div>
          <div style={{ marginTop: 6, font: '400 var(--fs-micro)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: depth === exit ? 'var(--exit-500)' : 'var(--fg-3)' }}>
            depth {depth} · exit {exit}
          </div>
        </>
      )}
      {event && <div style={{ marginTop: 6, font: '400 var(--fs-micro)/1.3 var(--font-body)', color: 'var(--fg-4)' }}>{event}</div>}
      {expanded && !out && (
        <div style={{ marginTop: 'var(--sp-4)', paddingTop: 'var(--sp-3)', borderTop: '1px solid var(--line-faint)', display: 'flex', flexDirection: 'column', gap: 5 }}>
          <div style={{ font: '500 11px/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-4)' }}>In this dream</div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-2)' }}>
            <Icon name="cat-mob" size={15} />{mobs} Shades alive
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-2)' }}>
            <Icon name="weak-point" size={15} />{jammed} traps jammed
          </div>
        </div>
      )}
    </div>
  );
}

/* Hover-peek on a trap cube: weak point, its manual trigger, the 6 s cooldown. */
function TrapPeek({ cube, cooldown = 0, x, y }) {
  if (!cube) return null;   // never let a missing lookup unmount the screen
  return (
    <div style={{ position: 'absolute', left: x, top: y, display: 'flex', alignItems: 'flex-start', gap: 'var(--sp-4)' }}>
      <CooldownRing progress={cooldown} size={44} tone="fog" label={cooldown > 0 ? String(Math.ceil((1 - cooldown) * window.RULES.triggerCooldown)) : 'T'}>
        <Icon name="power-trigger" size={18} />
      </CooldownRing>
      <MistPanel pad="var(--sp-4)" style={{ width: 250 }}>
        <div style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-1)' }}>{cube.name}</div>
        <div style={{ marginTop: 'var(--sp-3)', display: 'flex', alignItems: 'center', gap: 'var(--sp-3)' }}>
          <span style={{ color: 'var(--fg-3)' }}><ConnectorNet mask={cube.mask} size={30} /></span>
          <div style={{ font: '400 var(--fs-micro)/1.5 var(--font-body)', color: 'var(--fg-3)' }}>
            weak point: {cube.weak}<br />trigger: {cube.trigger}
          </div>
        </div>
        <div style={{ marginTop: 'var(--sp-3)', paddingTop: 'var(--sp-3)', borderTop: '1px solid var(--line-faint)', font: '400 var(--fs-micro)/1 var(--font-body)', color: cooldown > 0 ? 'var(--fg-4)' : 'var(--exit-500)' }}>
          {cooldown > 0 ? 'Cooling — ' + Math.ceil((1 - cooldown) * window.RULES.triggerCooldown) + ' s' : 'T to ' + cube.trigger}
        </div>
      </MistPanel>
    </div>
  );
}

/* The lattice plan. cells are [x, y] on the current layer. */
function Lattice({ cells, fogDoors = [], exits = [], solids = [], markers = [], ghost, layer = 0, trap, children }) {
  const S = 76, ox = 470, oy = 130;
  const px = (c) => ox + c * S, py = (c) => oy + c * S;
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      <DreamView tone="god" />
      <div style={{ position: 'absolute', inset: 0, opacity: 0.45, backgroundImage: 'linear-gradient(rgba(147,167,196,.08) 1px, transparent 1px), linear-gradient(90deg, rgba(147,167,196,.08) 1px, transparent 1px)', backgroundSize: S + 'px ' + S + 'px', backgroundPosition: ox + 'px ' + oy + 'px' }} />
      <svg style={{ position: 'absolute', inset: 0, width: '100%', height: '100%' }}>
        {cells.map(([cx, cy, kind], i) => (
          <g key={i}>
            <rect x={px(cx) + 3} y={py(cy) + 3} width={S - 6} height={S - 6} rx="3"
              fill={kind === 'start' ? 'rgba(58,74,99,.62)' : 'rgba(46,65,96,.5)'}
              stroke={kind === 'start' ? 'var(--fog-300)' : 'rgba(195,212,234,.3)'} strokeWidth={kind === 'start' ? 1.6 : 1} />
            {kind === 'start' && <text x={px(cx) + S / 2} y={py(cy) + S / 2 + 4} textAnchor="middle" style={{ font: '400 11px var(--font-body)', fill: 'var(--fg-3)' }}>start</text>}
          </g>
        ))}
        {/* solidified doors — hatched wall, no hue needed */}
        {solids.map(([cx, cy, side], i) => {
          const h = side === 'n' || side === 's';
          const w = h ? S - 22 : 5, hh = h ? 5 : S - 22;
          const dx = side === 'e' ? S - 6 : side === 'w' ? 1 : 11;
          const dy = side === 's' ? S - 6 : side === 'n' ? 1 : 11;
          return <rect key={'s' + i} x={px(cx) + dx} y={py(cy) + dy} width={w} height={hh} fill="var(--door-solid)" />;
        })}
        {/* fog doors: dark, matte, dashed */}
        {fogDoors.map(([cx, cy], i) => (
          <rect key={'f' + i} x={px(cx) + 14} y={py(cy) + 14} width={S - 28} height={S - 28} rx="2"
            fill="rgba(92,114,149,.26)" stroke="var(--fog-500)" strokeWidth="1.5" strokeDasharray="4 3" />
        ))}
        {/* exits: bright, radiant */}
        {exits.map(([cx, cy], i) => (
          <g key={'e' + i} transform={'translate(' + (px(cx) + S / 2) + ',' + (py(cy) + S / 2) + ')'}>
            <circle r="30" fill="rgba(255,246,222,.12)" />
            <rect x="-16" y="-16" width="32" height="32" rx="2" fill="rgba(255,246,222,.72)" stroke="var(--exit-300)" strokeWidth="2"
              style={{ filter: 'drop-shadow(0 0 16px rgba(255,246,222,.85))' }} />
          </g>
        ))}
        {ghost && (
          <rect x={px(ghost[0]) + 3} y={py(ghost[1]) + 3} width={S - 6} height={S - 6} rx="3"
            fill={ghost[2] === 'reject' ? 'rgba(217,69,58,.3)' : 'rgba(0,158,115,.3)'}
            stroke={ghost[2] === 'reject' ? 'var(--danger-500)' : 'var(--sleeper-3)'} strokeWidth="2"
            strokeDasharray={ghost[2] === 'reject' ? '6 4' : undefined} />
        )}
      </svg>
      {markers.map((m, i) => (
        <div key={i} style={{ position: 'absolute', left: px(m.x) + S / 2 - 42, top: py(m.y) + S / 2 - 17 }}>
          <SleeperMarker index={m.index} name={m.name} facing={m.facing} selected={m.selected} />
        </div>
      ))}
      {trap && (
        <>
          <div style={{ position: 'absolute', left: px(trap.x) + 18, top: py(trap.y) + 18, width: S - 36, height: S - 36, borderRadius: 2, border: '1.5px solid var(--fog-300)', background: 'rgba(143,166,200,.14)' }} />
          <TrapPeek cube={trap.cube} cooldown={trap.cooldown} x={px(trap.x) + S + 10} y={py(trap.y) - 4} />
        </>
      )}
      {/* layer cut-away, PgUp / PgDn */}
      <div style={{ position: 'absolute', left: 'var(--sp-6)', top: '50%', transform: 'translateY(-50%)', display: 'flex', flexDirection: 'column-reverse', gap: 'var(--sp-2)', alignItems: 'center' }}>
        <span style={{ font: '400 11px/1 var(--font-body)', color: 'var(--fg-4)', marginTop: 6 }}>PgDn</span>
        {[-1, 0, 1, 2].map(l => (
          <div key={l} style={{ width: 42, height: 34, display: 'flex', alignItems: 'center', justifyContent: 'center', borderRadius: 'var(--r-1)',
            border: 'var(--bw-hair) solid ' + (l === layer ? 'var(--exit-500)' : 'var(--line-faint)'),
            background: l === layer ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 45%, transparent)',
            font: '400 var(--fs-micro)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: l === layer ? 'var(--exit-300)' : 'var(--fg-4)' }}>{l > 0 ? '+' + l : l}</div>
        ))}
        <span style={{ font: '400 11px/1 var(--font-body)', color: 'var(--fg-4)', marginBottom: 6 }}>PgUp</span>
      </div>
      {children}
    </div>
  );
}

/* The full Nightmare chrome, corrected to UI.md §8's region table. */
function NightmareChrome({ phase, seconds, urgent, budget, trickle, paused, cubes, cat, pick, toasts, sleepers, target, effects, children, rejection }) {
  const shown = window.CUBES.filter(c => c.category === cat);
  return (
    <div style={{ position: 'absolute', inset: 0 }}>
      {children}

      {/* TOP LEFT — budget */}
      <div style={{ position: 'absolute', left: 356, top: 'var(--sp-6)' }}>
        <ClusterScrim corner="top left"><BudgetCluster budget={budget} trickle={trickle} paused={paused} /></ClusterScrim>
      </div>

      {/* TOP CENTRE — dawn timer and phase banner */}
      <div style={{ position: 'absolute', left: '50%', top: 'var(--sp-5)', transform: 'translateX(-50%)' }}>
        <TimerArc seconds={seconds} total={window.RULES.dawn} size={180} phase={phase} urgent={urgent} />
      </div>

      {/* TOP RIGHT — toasts */}
      <div style={{ position: 'absolute', right: 340, top: 'var(--sp-6)', marginRight: 'var(--sp-6)' }}>
        <ToastStack toasts={toasts} />
      </div>

      {/* LEFT — palette */}
      <MistPanel tone="chrome" pad="var(--sp-5)" style={{ position: 'absolute', left: 0, top: 0, bottom: 0, width: 340, borderLeft: 0, borderTop: 0, borderBottom: 0, borderRadius: 0 }}>
        <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Palette</MicroLabel>
        <div style={{ display: 'flex', gap: 'var(--sp-2)', marginBottom: 'var(--sp-4)' }}>
          {['Core', 'Attic'].map((p, i) => (
            <div key={p} style={{ flex: 1, padding: '8px 0', textAlign: 'center', borderRadius: 'var(--r-1)',
              background: i === 0 ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'transparent',
              border: 'var(--bw-hair) solid ' + (i === 0 ? 'var(--line-strong)' : 'var(--line-faint)'),
              font: '400 var(--fs-body)/1 var(--font-body)', color: i === 0 ? 'var(--fg-1)' : 'var(--fg-3)' }}>{p}</div>
          ))}
        </div>
        <div style={{ display: 'flex', gap: 'var(--sp-2)', flexWrap: 'wrap', marginBottom: 'var(--sp-4)' }}>
          {NM_CATS.map(([c, label]) => (
            <div key={c} style={{ display: 'flex', alignItems: 'center', gap: 5, padding: '5px 8px', borderRadius: 'var(--r-1)',
              background: cat === c ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
              border: 'var(--bw-hair) solid ' + (cat === c ? 'var(--exit-500)' : 'var(--line-faint)'),
              color: cat === c ? 'var(--exit-300)' : 'var(--fg-3)', font: '400 11px/1 var(--font-body)' }}>
              <Icon name={c} size={16} />{label}
            </div>
          ))}
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--sp-3)', alignContent: 'start', overflow: 'hidden' }}>
          {shown.slice(0, 8).map((c, i) => (
            <PaletteTile key={c.name} name={c.name} cost={c.cost} mask={c.mask} category={c.category}
              hotkey={i + 1} budget={budget} selected={pick === c.name} />
          ))}
        </div>
        <div style={{ flex: 1 }} />
        <div style={{ font: '400 11px/1.5 var(--font-body)', color: 'var(--fg-4)' }}>
          1–9 select · R rotate · Esc cancel · Ctrl+Tab category
        </div>
      </MistPanel>

      {/* RIGHT — Sleeper panel */}
      <MistPanel tone="chrome" pad="var(--sp-5)" style={{ position: 'absolute', right: 0, top: 0, bottom: 0, width: 320, borderRight: 0, borderTop: 0, borderBottom: 0, borderRadius: 0 }}>
        <MicroLabel style={{ marginBottom: 'var(--sp-4)' }}>Sleepers</MicroLabel>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--sp-3)' }}>
          {sleepers.map(s => <SleeperRow key={s.index} {...s} selected={target === s.index} />)}
        </div>
        <div style={{ flex: 1 }} />
        <div style={{ font: '400 11px/1.5 var(--font-body)', color: 'var(--fg-4)' }}>
          Click a row to target that dream · F focus · Home start cube
        </div>
      </MistPanel>

      {/* BOTTOM CENTRE — powers bar */}
      <div style={{ position: 'absolute', left: 340, right: 320, bottom: 0, display: 'flex', justifyContent: 'center' }}>
        <MistPanel tone="chrome" edge="bottom" pad="var(--sp-4) var(--sp-6)" style={{ flexDirection: 'row', alignItems: 'center', gap: 'var(--sp-6)', borderBottom: 0 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-3)', paddingRight: 'var(--sp-5)', borderRight: '1px solid var(--line-faint)' }}>
            <Icon name="power-target" size={20} color="var(--fg-3)" />
            <div style={{ lineHeight: 1.25 }}>
              <div style={{ font: '500 11px/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-4)' }}>Target</div>
              <div style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: target ? 'var(--exit-300)' : 'var(--fg-1)', marginTop: 4 }}>
                {target ? sleepers.find(s => s.index === target).name + "'s dream" : 'Everyone'}
              </div>
            </div>
            <kbd style={{ font: '500 11px/1 var(--font-body)', color: 'var(--fg-3)', border: '1px solid var(--line)', borderRadius: 'var(--r-1)', padding: '2px 5px' }}>Tab</kbd>
          </div>
          {window.EFFECTS.map(e => {
            const st = effects[e.name] || {};
            const prog = st.cooldown || 0;
            const poor = budget < e.cost;
            return (
              <div key={e.name} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6, opacity: st.active ? 1 : 1 }}>
                <CooldownRing progress={prog} size={58} tone={st.active ? 'fog' : 'exit'}
                  label={prog > 0 ? String(Math.ceil((1 - prog) * e.cooldown)) : e.key}>
                  <Icon name={e.icon} size={22} />
                </CooldownRing>
                <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: st.active ? 'var(--fog-300)' : prog > 0 || poor ? 'var(--fg-4)' : 'var(--fg-2)' }}>
                  {st.active ? e.name + ' ' + st.active + 's' : e.name}
                </span>
                <CostBadge cost={e.cost} budget={budget} size="sm" />
              </div>
            );
          })}
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', paddingLeft: 'var(--sp-5)', borderLeft: '1px solid var(--line-faint)' }}>
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 6 }}>
              <CooldownRing progress={0} size={58}><Icon name="power-possess" size={22} /></CooldownRing>
              <span style={{ font: '400 var(--fs-micro)/1 var(--font-body)', color: 'var(--fg-2)' }}>Possess</span>
              <span style={{ font: '400 11px/1 var(--font-body)', color: 'var(--fg-4)' }}>free · P</span>
            </div>
            <div style={{ font: '400 11px/1.5 var(--font-body)', color: 'var(--fg-4)', maxWidth: 118 }}>
              Click a mob marker, then P. Building stops while you are in there.
            </div>
          </div>
        </MistPanel>
      </div>
      {rejection}
    </div>
  );
}

/* ---------------------------------------------------------------- 1 of 3 */
function NightmareHeadStart() {
  return (
    <NightmareChrome
      phase="The Sleepers stir in 0:18" seconds={288} urgent={false}
      budget={11} trickle={0.75} cat="cat-connector" pick="Straight"
      toasts={[]} target={null}
      effects={{}}
      sleepers={[
        { index: 1, name: 'Anna', hp: 100, lives: 1, depth: 0, exit: 2, event: 'in the bedroom' },
        { index: 2, name: 'Ben', hp: 100, lives: 1, depth: 0, exit: 2, event: 'in the bedroom' },
        { index: 3, name: 'Cara', hp: 100, lives: 1, depth: 0, exit: 2, event: 'in the bedroom' },
        { index: 4, name: 'Dev', hp: 100, lives: 1, depth: 0, exit: 2, event: 'in the bedroom' }
      ]}>
      <Lattice
        cells={[[5, 4, 'start'], [6, 4], [6, 3]]}
        fogDoors={[[7, 4], [5, 3]]}
        exits={[[6, 2]]}
        ghost={[7, 3, 'valid']}
        markers={[]}
        layer={0} />
    </NightmareChrome>
  );
}

/* ---------------------------------------------------------------- 2 of 3 */
function NightmareRejected() {
  return (
    <NightmareChrome
      phase="The Sleepers are running" seconds={166} urgent={false}
      budget={3} trickle={0.25} cat="cat-chicane" pick="Crusher"
      target={1}
      toasts={[
        { text: 'Not enough budget (3 / 4)', tone: 'danger' },
        { text: 'Anna hardened 3 doors in the Cross', icon: 'door-solid' }
      ]}
      effects={{ Molasses: { cooldown: 0.55 }, Dark: { cooldown: 0 }, Fog: { cooldown: 0 } }}
      sleepers={[
        { index: 1, name: 'Anna', hp: 88, lives: 1, depth: 11, exit: 11, event: 'at an exit door' },
        { index: 2, name: 'Ben', hp: 45, lives: 1, depth: 7, exit: 11, event: 'jamming a latch' },
        { index: 3, name: 'Cara', status: 'consumed', event: 'consumed at 2:40' },
        { index: 4, name: 'Dev', hp: 100, lives: 1, depth: 4, exit: 11, event: 'in the Little maze' }
      ]}>
      <Lattice
        cells={[[5, 4, 'start'], [6, 4], [7, 4], [7, 3], [8, 3], [8, 2], [9, 2], [6, 5], [6, 6], [7, 6], [5, 3], [4, 3]]}
        solids={[[6, 4, 'n'], [7, 4, 'n'], [7, 3, 'w'], [8, 3, 'n'], [6, 5, 'e'], [5, 4, 'n']]}
        fogDoors={[[4, 4], [8, 6], [3, 3]]}
        exits={[[10, 2]]}
        ghost={[9, 3, 'reject']}
        markers={[
          { x: 9, y: 2, index: 1, name: 'Anna', facing: 45, selected: true },
          { x: 7, y: 6, index: 2, name: 'Ben', facing: 90 },
          { x: 4, y: 3, index: 4, name: 'Dev', facing: 270 }
        ]}
        layer={0} />
      <div style={{ position: 'absolute', left: 1090, top: 400 }}>
        <RejectionLabel reason="Not enough budget (3 / 4)" />
      </div>
    </NightmareChrome>
  );
}

/* ---------------------------------------------------------------- 3 of 3 */
function NightmareDawn() {
  const trapdoor = (window.CUBES || []).find(c => c.name === 'Trapdoor');
  return (
    <NightmareChrome
      phase="Dawn in 0:24" seconds={24} urgent
      budget={16} trickle={0.4} cat="cat-mob" pick="Nest"
      target={2}
      toasts={[
        { text: 'Dev was consumed', tone: 'danger' },
        { text: 'Anna woke up', icon: 'door-exit', tone: 'exit' },
        { text: 'Molasses — don\'t jump', icon: 'power-molasses', tone: 'effect' }
      ]}
      effects={{ Molasses: { active: 4 }, Dark: { cooldown: 0.3 }, Fog: { cooldown: 0 } }}
      sleepers={[
        { index: 2, name: 'Ben', hp: 32, lives: 1, depth: 13, exit: 14, event: 'one door from the exit', expanded: true, mobs: 2, jammed: 1 },
        { index: 1, name: 'Anna', status: 'awake', event: 'woke at 4:12' },
        { index: 3, name: 'Cara', status: 'consumed', event: 'consumed at 2:40' },
        { index: 4, name: 'Dev', status: 'consumed', event: 'consumed at 4:36' }
      ]}>
      <Lattice
        cells={[[5, 4, 'start'], [6, 4], [7, 4], [7, 3], [8, 3], [8, 2], [9, 2], [10, 2], [10, 3], [10, 4], [6, 5], [6, 6], [7, 6], [5, 3]]}
        solids={[[6, 4, 'n'], [7, 4, 'n'], [7, 3, 'w'], [8, 3, 'n'], [9, 2, 'n'], [10, 2, 'e'], [6, 5, 'e'], [5, 4, 'n'], [10, 3, 'w']]}
        fogDoors={[[4, 4], [8, 6]]}
        exits={[[11, 4]]}
        markers={[{ x: 10, y: 4, index: 2, name: 'Ben', facing: 90, selected: true }]}
        trap={{ x: 10, y: 3, cube: trapdoor, cooldown: 0 }}
        layer={0} />
    </NightmareChrome>
  );
}

Object.assign(window, { NightmareHeadStart, NightmareRejected, NightmareDawn, NightmareChrome, Lattice, SleeperRow, TrapPeek, BudgetCluster });
