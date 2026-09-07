const { useState, useEffect, useRef } = React;
const { MistPanel, Button, Toggle, Slider, CostBadge, Icon, TimerArc, HealthRing, MoonLives,
  Crosshair, DamageArc, CooldownRing, PaletteTile, ConnectorNet, SleeperMarker, RejectionLabel,
  PlayerRow, ScoreboardRow, Toast, ToastStack } = window.DesignSystem_427bea;

/* A neutral, blurred stand-in for the painterly 3D view. Geometry and gradient
   only — never an illustration (UI.md §15, and the brief's asset constraint). */
function DreamView({ tone = 'maze', dark = 0, children, style }) {
  const beds = {
    maze:   'radial-gradient(70% 90% at 30% 30%, #35465f 0%, transparent 60%), radial-gradient(60% 70% at 78% 62%, #2a3b56 0%, transparent 65%), linear-gradient(160deg, #1b2739 0%, #0b111c 100%)',
    god:    'radial-gradient(90% 90% at 50% 42%, #2c3d58 0%, transparent 62%), linear-gradient(180deg, #16202f 0%, #090e18 100%)',
    bedroom:'radial-gradient(45% 60% at 50% 58%, #3a4a63 0%, transparent 62%), linear-gradient(180deg, #121b2a 0%, #070b12 100%)',
    void:   'linear-gradient(180deg, #10182580 0%, #070b12 100%)'
  };
  return (
    <div style={{ position: 'absolute', inset: 0, overflow: 'hidden', background: beds[tone], ...style }}>
      <div style={{ position: 'absolute', inset: '-8%', filter: 'blur(46px)', opacity: 0.85 }}>
        <div style={{ position: 'absolute', left: '12%', top: '18%', width: 420, height: 420, borderRadius: 24, background: 'rgba(120,146,186,.16)', transform: 'rotate(18deg)' }} />
        <div style={{ position: 'absolute', left: '52%', top: '46%', width: 560, height: 340, borderRadius: 40, background: 'rgba(96,124,166,.14)', transform: 'rotate(-9deg)' }} />
        <div style={{ position: 'absolute', left: '30%', top: '62%', width: 300, height: 300, borderRadius: '50%', background: 'rgba(150,176,214,.1)' }} />
      </div>
      <div style={{ position: 'absolute', inset: 0, opacity: 0.35, backgroundImage: 'repeating-linear-gradient(0deg, rgba(255,255,255,.02) 0 1px, transparent 1px 3px)' }} />
      {dark > 0 && <div style={{ position: 'absolute', inset: 0, background: 'rgba(4,6,11,' + dark + ')' }} />}
      <div style={{ position: 'absolute', inset: 0, boxShadow: 'inset 0 0 240px 60px rgba(7,11,18,.85)' }} />
      {children}
    </div>
  );
}

/* Full-screen scrim behind a modal surface. */
function Scrim({ children, blur = true }) {
  return (
    <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center',
      background: 'var(--surface-scrim)', backdropFilter: blur ? 'blur(var(--blur-scrim))' : 'none', WebkitBackdropFilter: blur ? 'blur(var(--blur-scrim))' : 'none' }}>
      {children}
    </div>
  );
}

/* HUD cluster scrim: the "light chrome" answer — a faint radial wash under a
   corner cluster instead of a panel, so the world stays visible. */
function ClusterScrim({ corner = 'bottom left', children, style }) {
  return (
    <div style={{ position: 'relative', padding: 'var(--sp-5)', margin: 'calc(var(--sp-5) * -1)', ...style }}>
      <div aria-hidden="true" style={{ position: 'absolute', inset: '-24px', background: 'radial-gradient(60% 70% at ' + corner + ', rgba(7,11,18,.62) 0%, transparent 72%)', pointerEvents: 'none' }} />
      <div style={{ position: 'relative' }}>{children}</div>
    </div>
  );
}

/* The phase banner used at round start and by the Nightmare. */
function PhaseBanner({ text, sub }) {
  return (
    <div style={{ textAlign: 'center', textShadow: '0 2px 22px rgba(7,11,18,.95)' }}>
      <div style={{ font: '300 var(--fs-title)/1.1 var(--font-display)', color: 'var(--fg-1)', letterSpacing: '0.03em' }}>{text}</div>
      {sub && <div style={{ marginTop: 6, font: '400 var(--fs-body)/1 var(--font-body)', color: 'var(--fg-3)' }}>{sub}</div>}
    </div>
  );
}

const Divider = ({ style }) => <div style={{ height: 1, background: 'var(--line-faint)', margin: 'var(--sp-5) 0', ...style }} />;

const MicroLabel = ({ children, style }) => (
  <div style={{ font: '500 var(--fs-micro)/1 var(--font-body)', letterSpacing: 'var(--ls-caps)', textTransform: 'uppercase', color: 'var(--fg-3)', ...style }}>{children}</div>
);

const PLAYERS = [
  { name: 'Anna', index: 1 }, { name: 'Ben', index: 2 },
  { name: 'Cara', index: 3 }, { name: 'Dev', index: 4 }
];

/* ---- Real values. SPEC §8 (cubes), §9 (Sleepers), §10 (powers), §12 (scoring) ---- */

/* Connector masks in net order: top, west, north, east, south, bottom. */
const CUBES = [
  { name: 'Straight',        cost: 1, mask: '010100', category: 'cat-connector' },
  { name: 'Corner',          cost: 1, mask: '011000', category: 'cat-connector' },
  { name: 'T',               cost: 1, mask: '011100', category: 'cat-connector' },
  { name: 'Cross',           cost: 1, mask: '011110', category: 'cat-connector' },
  { name: 'Drop',            cost: 1, mask: '010001', category: 'cat-vertical', note: 'One-way down — a funnel' },
  { name: 'Landing',         cost: 1, mask: '010101', category: 'cat-vertical' },
  { name: 'Ladder shaft',    cost: 2, mask: '100001', category: 'cat-vertical', climbable: true },
  { name: 'Stairwell',       cost: 2, mask: '110000', category: 'cat-vertical', climbable: true },
  { name: 'Spike Pit',       cost: 2, mask: '010100', category: 'cat-chicane' },
  { name: 'Gap',             cost: 2, mask: '010100', category: 'cat-chicane' },
  { name: 'Vent',            cost: 2, mask: '010100', category: 'cat-chicane' },
  { name: 'Little maze',     cost: 3, mask: '010100', category: 'cat-chicane' },
  { name: 'Moving platforms',cost: 3, mask: '010100', category: 'cat-chicane' },
  { name: 'Trapdoor',        cost: 3, mask: '010101', category: 'cat-chicane', weak: 'latch', trigger: 'drop it now' },
  { name: 'Timed spikes',    cost: 3, mask: '010100', category: 'cat-chicane', weak: 'control box', trigger: 'fire now, off-rhythm' },
  { name: 'Pendulum',        cost: 4, mask: '010100', category: 'cat-chicane', weak: 'chain', trigger: 'hold' },
  { name: 'Crusher',         cost: 4, mask: '010100', category: 'cat-chicane', weak: 'hydraulic line', trigger: 'slam now' },
  { name: 'Turret',          cost: 4, mask: '010100', category: 'cat-chicane', weak: 'core', trigger: 'possess to aim by hand' },
  { name: 'Nest',            cost: 4, mask: '010100', category: 'cat-mob', weak: 'the nest', trigger: 'spawn a wave now' }
];

/* SPEC §10: one 30 s cooldown per power per dream. */
const EFFECTS = [
  { key: 'Q', icon: 'power-dark',     name: 'Dark',     cost: 3, duration: 8,  cooldown: 30 },
  { key: 'W', icon: 'power-fog',      name: 'Fog',      cost: 2, duration: 10, cooldown: 30 },
  { key: 'E', icon: 'power-molasses', name: 'Molasses', cost: 4, duration: 6,  cooldown: 30 }
];

const RULES = {
  headStart: 30, dawn: 300, lives: 1, hp: 100,
  budgetStart: 12, trickle: '1 per 4 s',
  jamShots: 6, triggerCooldown: 6
};

Object.assign(window, { DreamView, Scrim, ClusterScrim, PhaseBanner, Divider, MicroLabel, PLAYERS, CUBES, EFFECTS, RULES });
