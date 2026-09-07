const { MistPanel, Button, Icon, SleeperMarker, ScoreboardRow } = window.DesignSystem_427bea;

/* ============================ one source of truth ============================
   Every module-level name here is RS_-prefixed: in-browser Babel downcompiles a
   top-level const to a var on window, and a generic name like DAWN or OUTCOME
   would collide with another screen file loaded after this one.

   The cards, the leaderboard deltas and the badges all derive from SESSION
   below, with SPEC §12 applied in code, so they cannot disagree. Previously
   these were three hand-written literal blocks and three of four players had
   contradictory numbers.

   SPEC §12:  a Sleeper who wakes scores 100 + remaining seconds; consumed is 0;
              the Nightmare scores 100 per consumed Sleeper.
   The session is round 3 of 5 players, so exactly 3 Nightmare stints have been
   played and every player's woke + consumed equals 3 − their Nightmare stints. */

const RS_DAWN = 300;          /* 5:00, the lobby default */
const RS_ROUND_NO = 3;

const rsSecs = t => { const p = t.split(':').map(Number); return p[0] * 60 + p[1]; };
const rsWakeScore = t => 100 + (RS_DAWN - rsSecs(t));
const rsClock = s => Math.floor(s / 60) + ':' + String(s % 60).padStart(2, '0');

/* This round's Sleepers, in outcome order: woke first, then consumed. */
const RS_SLEEPERS = [
  { index: 2, name: 'Ben',  wokeAt: '4:12', depth: 13, before: 612, asNightmare: 1, woke: 2, consumed: 0 },
  { index: 4, name: 'Dev',  wokeAt: '4:48', depth: 11, before: 196, asNightmare: 0, woke: 2, consumed: 1 },
  { index: 3, name: 'Cara', consumedAt: '2:40', depth: 6, before: 205, asNightmare: 0, woke: 1, consumed: 2 },
  { index: 1, name: 'Anna', byDawn: true, depth: 9, before: 245, asNightmare: 1, woke: 1, consumed: 1 }
];

const RS_NIGHTMARE = { name: 'Mara', cubes: 34, deepest: 14, before: 342, asNightmare: 1, woke: 1, consumed: 1 };

/* Derived — never hand-written. */
const RS_consumedThisRound = RS_SLEEPERS.filter(s => !s.wokeAt).length;
const RS_scored = RS_SLEEPERS.map(s => ({ ...s, delta: s.wokeAt ? rsWakeScore(s.wokeAt) : 0 }));
const RS_nightmare = { ...RS_NIGHTMARE, delta: 100 * RS_consumedThisRound };

const RS_LEADERBOARD = [...RS_scored, RS_nightmare]
  .map(p => ({ ...p, total: p.before + p.delta }))
  .sort((a, b) => b.total - a.total);

/* UI.md §9: the title is exactly one of three strings. */
const RS_OUTCOME = RS_SLEEPERS.some(s => s.byDawn)
  ? { line: 'Dawn.', tone: 'var(--exit-300)', icon: 'door-exit' }
  : RS_consumedThisRound === RS_SLEEPERS.length
    ? { line: 'Consumed', tone: 'var(--danger-300)', icon: 'role-nightmare' }
    : { line: 'Everyone woke up', tone: 'var(--exit-300)', icon: 'door-exit' };

const RS_fastestWake = RS_scored.filter(s => s.wokeAt).sort((a, b) => rsSecs(a.wokeAt) - rsSecs(b.wokeAt))[0];

const RS_BADGES = [
  ['fastest wake', RS_fastestWake.name + ' · ' + RS_fastestWake.wokeAt],
  ['longest survived', RS_SLEEPERS.some(s => s.byDawn) ? RS_SLEEPERS.find(s => s.byDawn).name + ' · to dawn' : RS_fastestWake.name],
  ['most cubes built', RS_nightmare.name + ' · ' + RS_nightmare.cubes]
];

/* One card per Sleeper: the story of their round in three lines. */
function PlayerCard({ p, compact }) {
  const woke = !!p.wokeAt;
  const outcome = woke ? 'Woke at ' + p.wokeAt : p.byDawn ? 'Consumed by dawn' : 'Consumed at ' + p.consumedAt;
  return (
    <div style={{ flex: 1, minWidth: 0, padding: compact ? 'var(--sp-4)' : 'var(--sp-5)', borderRadius: 'var(--r-3)',
      background: 'color-mix(in srgb, var(--ink-900) 40%, transparent)',
      border: 'var(--bw-hair) solid ' + (woke ? 'color-mix(in srgb, var(--exit-500) 55%, transparent)' : 'var(--line)') }}>
      <SleeperMarker index={p.index} name={p.name} scale="lg" size={compact ? 38 : 46} />
      <div style={{ marginTop: compact ? 'var(--sp-3)' : 'var(--sp-4)', display: 'flex', alignItems: 'baseline', gap: 'var(--sp-3)', flexWrap: 'wrap' }}>
        <span style={{ font: '400 ' + (compact ? 'var(--fs-heading)' : 'var(--fs-title)') + '/1.1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: woke ? 'var(--exit-300)' : 'var(--fg-3)' }}>{outcome}</span>
        <span style={{ font: '500 var(--fs-heading)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: p.delta > 0 ? 'var(--exit-500)' : 'var(--fg-4)' }}>{p.delta > 0 ? '+' + p.delta : '0'}</span>
      </div>
      <div style={{ marginTop: 6, font: '400 var(--fs-heading)/1.3 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-3)' }}>deepest door {p.depth}</div>
    </div>
  );
}

function ResultsScreen({ compact, onLobby }) {
  const pad = compact ? 'var(--sp-6)' : 'var(--sp-8)';
  return (
    <div style={{ position: 'absolute', inset: 0, background: 'var(--ink-800)' }}>
      <DreamView tone="void" />
      <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', padding: pad, gap: compact ? 'var(--sp-5)' : 'var(--sp-7)' }}>
        <header style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', gap: 'var(--sp-6)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-5)' }}>
            <span style={{ color: RS_OUTCOME.tone }}><Icon name={RS_OUTCOME.icon} size={compact ? 44 : 60} strokeWidth={1.2} /></span>
            <div>
              <MicroLabel>Round {RS_ROUND_NO} · dawn at {rsClock(RS_DAWN)}</MicroLabel>
              <h1 style={{ margin: '8px 0 0', font: '300 ' + (compact ? '46px' : 'var(--fs-display)') + '/1 var(--font-display)', color: RS_OUTCOME.tone }}>{RS_OUTCOME.line}</h1>
            </div>
          </div>
          <div style={{ textAlign: 'right' }}>
            <MicroLabel>The Nightmare</MicroLabel>
            <div style={{ marginTop: 8, display: 'flex', alignItems: 'center', gap: 'var(--sp-4)', justifyContent: 'flex-end' }}>
              <span style={{ font: '400 ' + (compact ? 'var(--fs-title)' : '46px') + '/1 var(--font-display)', color: 'var(--fg-1)' }}>{RS_nightmare.name}</span>
              <span style={{ font: '500 var(--fs-title)/1 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--exit-500)' }}>+{RS_nightmare.delta}</span>
            </div>
            <div style={{ marginTop: 8, font: '400 var(--fs-heading)/1.35 var(--font-body)', fontVariantNumeric: 'tabular-nums', color: 'var(--fg-2)' }}>
              {RS_consumedThisRound} Sleepers consumed · {RS_nightmare.cubes} cubes placed · deepest {RS_nightmare.deepest}
            </div>
          </div>
        </header>

        <div style={{ display: 'flex', gap: compact ? 'var(--sp-4)' : 'var(--sp-5)' }}>
          {RS_scored.map(p => <PlayerCard key={p.index} p={p} compact={compact} />)}
        </div>

        <MistPanel tone="chrome" pad={compact ? 'var(--sp-4)' : 'var(--sp-5)'} style={{ flex: 1, minHeight: 0 }}>
          <ScoreboardRow header rank="#" name="Player" rounds="As Nightmare" woke="Woke" consumed="Consumed" score="Score" />
          {RS_LEADERBOARD.map((p, i) => (
            <ScoreboardRow key={p.name} rank={i + 1} index={p.index} name={p.name}
              rounds={p.asNightmare} woke={p.woke} consumed={p.consumed}
              score={p.total} delta={p.delta > 0 ? p.delta : null}
              highlight={p.name === RS_nightmare.name} />
          ))}
        </MistPanel>

        <footer style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--sp-6)' }}>
          <div style={{ display: 'flex', gap: 'var(--sp-6)' }}>
            {RS_BADGES.map(b => (
              <div key={b[0]}>
                <MicroLabel>{b[0]}</MicroLabel>
                <div style={{ marginTop: 5, font: '400 var(--fs-heading)/1 var(--font-body)', color: 'var(--fg-2)' }}>{b[1]}</div>
              </div>
            ))}
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--sp-4)' }}>
            <span style={{ font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-4)' }}>auto in 10 s</span>
            <Button variant="secondary" size={compact ? 'md' : 'lg'}>Save replay</Button>
            <Button variant="primary" size={compact ? 'md' : 'lg'} onClick={onLobby}>Back to lobby</Button>
          </div>
        </footer>
      </div>
    </div>
  );
}
Object.assign(window, { ResultsScreen, PlayerCard });
