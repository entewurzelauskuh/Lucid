/* @ds-bundle: {"format":4,"namespace":"DesignSystem_427bea","components":[{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"CostBadge","sourcePath":"components/core/CostBadge.jsx"},{"name":"ICONS","sourcePath":"components/core/Icon.jsx"},{"name":"Icon","sourcePath":"components/core/Icon.jsx"},{"name":"MistPanel","sourcePath":"components/core/MistPanel.jsx"},{"name":"Slider","sourcePath":"components/core/Slider.jsx"},{"name":"Toggle","sourcePath":"components/core/Toggle.jsx"},{"name":"Toast","sourcePath":"components/feedback/Toast.jsx"},{"name":"ToastStack","sourcePath":"components/feedback/Toast.jsx"},{"name":"Crosshair","sourcePath":"components/hud/Crosshair.jsx"},{"name":"DamageArc","sourcePath":"components/hud/DamageArc.jsx"},{"name":"HealthRing","sourcePath":"components/hud/HealthRing.jsx"},{"name":"MoonLives","sourcePath":"components/hud/MoonLives.jsx"},{"name":"TimerArc","sourcePath":"components/hud/TimerArc.jsx"},{"name":"PlayerRow","sourcePath":"components/lobby/PlayerRow.jsx"},{"name":"ScoreboardRow","sourcePath":"components/lobby/ScoreboardRow.jsx"},{"name":"CooldownRing","sourcePath":"components/nightmare/CooldownRing.jsx"},{"name":"ConnectorNet","sourcePath":"components/nightmare/PaletteTile.jsx"},{"name":"PaletteTile","sourcePath":"components/nightmare/PaletteTile.jsx"},{"name":"REJECTIONS","sourcePath":"components/nightmare/RejectionLabel.jsx"},{"name":"RejectionLabel","sourcePath":"components/nightmare/RejectionLabel.jsx"},{"name":"SLEEPER_COLORS","sourcePath":"components/nightmare/SleeperMarker.jsx"},{"name":"SleeperMarker","sourcePath":"components/nightmare/SleeperMarker.jsx"}],"sourceHashes":{"components/core/Button.jsx":"b8ebd74d7c78","components/core/CostBadge.jsx":"4e5ec1fa2558","components/core/Icon.jsx":"ceb66648a0ad","components/core/MistPanel.jsx":"b93efec39ddb","components/core/Slider.jsx":"41fbf3ee6a0c","components/core/Toggle.jsx":"38615837a464","components/feedback/Toast.jsx":"1a773f23c9fb","components/hud/Crosshair.jsx":"23888b824b16","components/hud/DamageArc.jsx":"15f3299e97a7","components/hud/HealthRing.jsx":"d46bb62b58c1","components/hud/MoonLives.jsx":"9bcf25091a1c","components/hud/TimerArc.jsx":"50f2b3e97f3f","components/lobby/PlayerRow.jsx":"aceab515c314","components/lobby/ScoreboardRow.jsx":"860d09a2b252","components/nightmare/CooldownRing.jsx":"4042fc6c5c92","components/nightmare/PaletteTile.jsx":"3cda45486169","components/nightmare/RejectionLabel.jsx":"7faab88cf237","components/nightmare/SleeperMarker.jsx":"6f75f7360bf6","ui_kits/lucid-game/LobbyScreen.jsx":"990869de7563","ui_kits/lucid-game/NightmareMockups.jsx":"a0d5925fae3c","ui_kits/lucid-game/NightmareView.jsx":"23899ed004c7","ui_kits/lucid-game/OptionsScreen.jsx":"f12dc160c692","ui_kits/lucid-game/PauseScreen.jsx":"e384140950cb","ui_kits/lucid-game/ResultsScreen.jsx":"53daf1b77e15","ui_kits/lucid-game/RoundStartScreen.jsx":"15dadd8556ca","ui_kits/lucid-game/Shared.jsx":"710ce1e383e9","ui_kits/lucid-game/SleeperHud.jsx":"168ee2221bf4","ui_kits/lucid-game/SleeperMockups.jsx":"de0d3cd26c12","ui_kits/lucid-game/SpectatorScreen.jsx":"6ecc99d44fc2","ui_kits/lucid-game/TitleScreen.jsx":"069cc1da4ac0"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.DesignSystem_427bea = window.DesignSystem_427bea || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/core/CostBadge.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function CostBadge({
  cost,
  affordable = true,
  budget,
  size = 'md',
  style,
  ...rest
}) {
  const ok = affordable && (budget == null || budget >= cost);
  const s = size === 'sm';
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: s ? 3 : 'var(--sp-2)',
      padding: s ? '2px 5px' : '3px 7px',
      borderRadius: 'var(--r-1)',
      border: 'var(--bw-hair) solid ' + (ok ? 'var(--line)' : 'var(--danger-500)'),
      background: ok ? 'color-mix(in srgb, var(--ink-900) 45%, transparent)' : 'color-mix(in srgb, var(--danger-700) 35%, transparent)',
      font: '500 ' + (s ? '11px' : 'var(--fs-micro)') + '/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: ok ? 'var(--exit-500)' : 'var(--danger-300)',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: s ? 7 : 8,
    height: s ? 7 : 8,
    viewBox: "0 0 8 8",
    "aria-hidden": "true"
  }, /*#__PURE__*/React.createElement("path", {
    d: "M4 0 8 4 4 8 0 4Z",
    fill: "currentColor"
  })), cost);
}
Object.assign(__ds_scope, { CostBadge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/CostBadge.jsx", error: String((e && e.message) || e) }); }

// components/core/Icon.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const FRAME = '<path d="M4 21h16"/><path d="M6.5 21V8a5.5 5.5 0 0 1 11 0v13"/>';
const MIST = 'c1.2-.9 2.3.9 3.5 0s2.3-.9 3.5 0';

/* Geometry is duplicated from assets/icons/*.svg so components stay
   self-contained. The files on disk are the export path for Unity. */
const ICONS = {
  'door-attached': FRAME + '<path d="M9.5 21V9.5h5V21"/>',
  'door-fog': FRAME + '<path d="M8.5 11.5' + MIST + '" stroke-dasharray="0.1 3.2"/><path d="M8.5 15' + MIST + '" stroke-dasharray="0.1 3.2"/><path d="M8.5 18.5' + MIST + '" stroke-dasharray="0.1 3.2"/>',
  'door-exit': FRAME + '<path d="M12 9.5v9"/><path d="M2.2 12h1.6M20.2 12h1.6M5 5.5l1.1 1.1M17.9 6.6L19 5.5M12 1.8v1.6"/>',
  'door-solid': FRAME + '<path d="M7 18.5 13.5 12M7 13 12 8M9 21l8.5-8.5M14 21l3.5-3.5"/>',
  'power-dark': '<circle cx="12" cy="12" r="4.5"/><path d="M12 3v2.2M4.6 6.6 6.2 8.2M3 12h2.2"/><path d="M4 20 20 4"/>',
  'power-fog': '<path d="M4 9h13M7 13h13M4 17h11"/>',
  'power-molasses': '<path d="M7 3h10M7 21h10M8 3v3.2L12 12l4-5.8V3M8 21v-3.2L12 12l4 5.8V21"/>',
  'power-trigger': '<rect x="4.5" y="14" width="15" height="5.5" rx="1"/><path d="M12 4v6.5M9 8l3 3 3-3"/>',
  'power-possess': '<path d="M2.5 12S6 6.5 12 6.5 21.5 12 21.5 12S18 17.5 12 17.5 2.5 12 2.5 12Z"/><circle cx="12" cy="12" r="2.6"/><path d="M12 2v2.6"/>',
  'power-target': '<path d="M4 8.5V5.5A1.5 1.5 0 0 1 5.5 4h3M15.5 4h3A1.5 1.5 0 0 1 20 5.5v3M20 15.5v3a1.5 1.5 0 0 1-1.5 1.5h-3M8.5 20h-3A1.5 1.5 0 0 1 4 18.5v-3"/><circle cx="12" cy="12" r="1.6" fill="currentColor" stroke="none"/>',
  'cat-connector': '<path d="M2.5 9.5h19M2.5 14.5h19M9.5 2.5v19M14.5 2.5v19"/>',
  'cat-vertical': '<path d="M8 2.5v19M16 2.5v19M8 7h8M8 12h8M8 17h8"/>',
  'cat-chicane': '<path d="M3 20h5v-6h8V8h5"/><path d="M12 4v3M12 17v3"/>',
  'cat-mob': '<path d="M5 19V11a7 7 0 0 1 14 0v8c-1.2 0-1.6-1.4-2.8-1.4S14.4 19 13.2 19s-1.6-1.4-2.8-1.4S8.8 19 7.6 19 6.2 19 5 19Z"/><circle cx="9.6" cy="11" r="1" fill="currentColor" stroke="none"/><circle cx="14.4" cy="11" r="1" fill="currentColor" stroke="none"/>',
  'cat-gimmick': '<path d="M12 3v18M4 7.5l16 9M20 7.5l-16 9"/>',
  'ready': '<circle cx="12" cy="12" r="9"/><path d="M7.8 12.4 10.7 15.3 16.4 9.2"/>',
  'unready': '<circle cx="12" cy="12" r="9" stroke-dasharray="2.2 3"/>',
  'role-nightmare': '<path d="M2.5 10.5S6 5.5 12 5.5s9.5 5 9.5 5S18 15.5 12 15.5 2.5 10.5 2.5 10.5Z"/><circle cx="12" cy="10.5" r="2.4"/><path d="M4 19c1.4-1 2.7.9 4 0s2.6.9 4 0 2.6.9 4 0"/>',
  'role-sleeper': '<path d="M16.5 3.2A9 9 0 1 0 16.5 20.8 9.6 9.6 0 0 1 16.5 3.2Z"/>',
  'moon': '<path d="M16.5 3.2A9 9 0 1 0 16.5 20.8 9.6 9.6 0 0 1 16.5 3.2Z"/>',
  'crown': '<path d="M3 18h18M4 18 3 7l5 4 4-6 4 6 5-4-1 11"/>',
  'weak-point': '<circle cx="12" cy="12" r="8" stroke-dasharray="4.5 3"/><circle cx="12" cy="12" r="1.8" fill="currentColor" stroke="none"/>',
  'depth': '<path d="M12 3v14M7 12l5 5 5-5M4 21h16"/>'
};
function Icon({
  name,
  size = 24,
  strokeWidth = 1.5,
  color,
  style,
  title,
  ...rest
}) {
  const body = ICONS[name];
  if (!body) return null;
  return /*#__PURE__*/React.createElement("svg", _extends({
    viewBox: "0 0 24 24",
    width: size,
    height: size,
    fill: "none",
    stroke: "currentColor",
    strokeWidth: strokeWidth,
    strokeLinecap: "round",
    strokeLinejoin: "round",
    role: "img",
    "aria-label": title || name,
    style: {
      color: color || 'currentColor',
      flex: '0 0 auto',
      display: 'block',
      ...style
    },
    dangerouslySetInnerHTML: {
      __html: body
    }
  }, rest));
}
Object.assign(__ds_scope, { ICONS, Icon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Icon.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Button({
  children,
  variant = 'primary',
  size = 'md',
  icon,
  hotkey,
  disabled,
  reason,
  full,
  onClick,
  style,
  ...rest
}) {
  const off = disabled || !!reason;
  const primary = variant === 'primary';
  const btn = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 'var(--sp-3)',
    minHeight: size === 'lg' ? 56 : 'var(--hit-min)',
    padding: size === 'lg' ? '0 var(--sp-7)' : '0 var(--sp-6)',
    width: full ? '100%' : undefined,
    font: (primary ? '500 ' : '400 ') + (size === 'lg' ? 'var(--fs-heading)' : 'var(--fs-body)') + '/1 var(--font-body)',
    letterSpacing: '0.02em',
    color: off ? 'var(--fg-4)' : primary ? 'var(--exit-300)' : 'var(--fg-2)',
    background: off ? 'transparent' : primary ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
    border: 'var(--bw-hair) solid ' + (off ? 'var(--line-faint)' : primary ? 'var(--exit-500)' : 'var(--line-strong)'),
    borderRadius: 'var(--r-2)',
    cursor: off ? 'not-allowed' : 'pointer',
    transition: 'background var(--dur-fast) var(--ease-out), color var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)',
    textAlign: 'center',
    ...style
  };
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    disabled: off,
    onClick: off ? undefined : onClick,
    style: btn,
    onMouseEnter: e => {
      if (off) return;
      e.currentTarget.style.background = primary ? 'color-mix(in srgb, var(--exit-500) 22%, transparent)' : 'color-mix(in srgb, var(--mist-500) 60%, transparent)';
      if (primary) e.currentTarget.style.boxShadow = 'var(--glow-exit)';
    },
    onMouseLeave: e => {
      if (off) return;
      e.currentTarget.style.background = primary ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent';
      e.currentTarget.style.boxShadow = 'none';
    }
  }, rest), icon && /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 20
  }), /*#__PURE__*/React.createElement("span", null, reason || children), hotkey && /*#__PURE__*/React.createElement("kbd", {
    style: {
      font: '500 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)',
      border: 'var(--bw-hair) solid var(--line)',
      borderRadius: 'var(--r-1)',
      padding: '3px 6px',
      marginLeft: 'var(--sp-2)'
    }
  }, hotkey));
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/MistPanel.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function MistPanel({
  title,
  aside,
  tone = 'panel',
  pad,
  edge = 'all',
  children,
  style,
  ...rest
}) {
  /* 'panel' is the only translucent tone, and it is reserved for surfaces that
     appear and vanish over the dream — toasts, hover peeks, the reveal card.
     Permanent chrome ('chrome') and full screens ('sunken') are opaque, because
     USS has no backdrop-filter and a 72 % fill over a bright dream loses its
     text contrast. See unity/CLAUDE-CODE-UI-GUIDE.md §2. */
  const opaque = tone === 'sunken' || tone === 'chrome';
  const fill = opaque ? 'var(--ink-800)' : 'color-mix(in srgb, var(--mist-600) 72%, transparent)';
  const panelShell = {
    position: 'relative',
    display: 'flex',
    flexDirection: 'column',
    background: fill,
    backdropFilter: opaque ? 'none' : 'blur(var(--blur-panel))',
    WebkitBackdropFilter: opaque ? 'none' : 'blur(var(--blur-panel))',
    border: 'var(--bw-hair) solid var(--border-panel)',
    borderTopWidth: edge === 'bottom' ? 'var(--bw-hair)' : edge === 'top' ? 0 : undefined,
    borderRadius: edge === 'all' ? 'var(--r-2)' : 0,
    boxShadow: tone === 'sunken' ? 'none' : 'var(--shadow-panel)',
    padding: pad != null ? pad : 'var(--pad-panel)',
    color: 'var(--text-body)',
    font: '400 var(--fs-body)/var(--lh-body) var(--font-body)',
    ...style
  };
  return /*#__PURE__*/React.createElement("div", _extends({
    style: panelShell
  }, rest), (title || aside) && /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      justifyContent: 'space-between',
      gap: 'var(--sp-4)',
      marginBottom: 'var(--sp-5)'
    }
  }, title && /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      font: '600 var(--fs-heading)/var(--lh-snug) var(--font-display)',
      color: 'var(--text-strong)',
      letterSpacing: '0.01em'
    }
  }, title), aside && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-micro)/1 var(--font-body)',
      letterSpacing: 'var(--ls-caps)',
      textTransform: 'uppercase',
      color: 'var(--fg-3)'
    }
  }, aside)), children);
}
Object.assign(__ds_scope, { MistPanel });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/MistPanel.jsx", error: String((e && e.message) || e) }); }

// components/core/Slider.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Slider({
  label,
  value = 0,
  min = 0,
  max = 100,
  step = 1,
  unit,
  format,
  disabled,
  onChange,
  style,
  ...rest
}) {
  const pct = Math.max(0, Math.min(100, (value - min) / (max - min) * 100));
  const shown = format ? format(value) : value + (unit ? ' ' + unit : '');
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-3)',
      minHeight: 'var(--hit-min)',
      justifyContent: 'center',
      opacity: disabled ? 0.5 : 1,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'baseline',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1.2 var(--font-body)',
      color: 'var(--text-strong)'
    }
  }, label), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-body)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--exit-500)'
    }
  }, shown)), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height: 20,
      display: 'flex',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 'auto 0',
      height: 3,
      borderRadius: 'var(--r-full)',
      background: 'color-mix(in srgb, var(--mist-400) 70%, transparent)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 0,
      width: pct + '%',
      height: 3,
      borderRadius: 'var(--r-full)',
      background: 'var(--exit-500)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: pct + '%',
      width: 14,
      height: 14,
      marginLeft: -7,
      borderRadius: 'var(--r-full)',
      background: 'var(--exit-300)',
      boxShadow: 'var(--glow-exit)'
    }
  }), /*#__PURE__*/React.createElement("input", {
    type: "range",
    min: min,
    max: max,
    step: step,
    value: value,
    disabled: disabled,
    onChange: e => onChange && onChange(Number(e.target.value)),
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      margin: 0,
      opacity: 0,
      cursor: disabled ? 'not-allowed' : 'pointer'
    }
  })));
}
Object.assign(__ds_scope, { Slider });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Slider.jsx", error: String((e && e.message) || e) }); }

// components/core/Toggle.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Toggle({
  label,
  hint,
  checked,
  disabled,
  onChange,
  style,
  ...rest
}) {
  const row = {
    display: 'flex',
    alignItems: 'center',
    gap: 'var(--sp-5)',
    justifyContent: 'space-between',
    minHeight: 'var(--hit-min)',
    cursor: disabled ? 'not-allowed' : 'pointer',
    opacity: disabled ? 0.5 : 1,
    ...style
  };
  return /*#__PURE__*/React.createElement("label", _extends({
    style: row
  }, rest), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 2
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1.2 var(--font-body)',
      color: 'var(--text-strong)'
    }
  }, label), hint && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1.3 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, hint)), /*#__PURE__*/React.createElement("span", {
    onClick: () => !disabled && onChange && onChange(!checked),
    style: {
      position: 'relative',
      flex: '0 0 auto',
      width: 46,
      height: 24,
      borderRadius: 'var(--r-full)',
      background: checked ? 'color-mix(in srgb, var(--exit-500) 22%, transparent)' : 'color-mix(in srgb, var(--mist-400) 55%, transparent)',
      border: 'var(--bw-hair) solid ' + (checked ? 'var(--exit-500)' : 'var(--line)'),
      transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      top: 3,
      left: checked ? 25 : 3,
      width: 16,
      height: 16,
      borderRadius: 'var(--r-full)',
      background: checked ? 'var(--exit-300)' : 'var(--fg-3)',
      boxShadow: checked ? 'var(--glow-exit)' : 'none',
      transition: 'left var(--dur-fast) var(--ease-out), background var(--dur-fast) var(--ease-out)'
    }
  })));
}
Object.assign(__ds_scope, { Toggle });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Toggle.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Toast.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const TONES = {
  event: 'var(--fg-1)',
  exit: 'var(--exit-300)',
  danger: 'var(--danger-300)',
  effect: 'var(--fog-300)'
};
function Toast({
  text,
  icon,
  tone = 'event',
  index = 0,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '10px var(--sp-4)',
      borderRadius: 'var(--r-2)',
      background: 'color-mix(in srgb, var(--mist-600) 72%, transparent)',
      backdropFilter: 'blur(var(--blur-panel))',
      WebkitBackdropFilter: 'blur(var(--blur-panel))',
      border: 'var(--bw-hair) solid var(--border-panel)',
      borderLeft: '2px solid ' + TONES[tone],
      boxShadow: 'var(--shadow-panel)',
      font: '400 var(--fs-body)/1.25 var(--font-body)',
      color: 'var(--fg-1)',
      opacity: 1 - index * 0.22,
      whiteSpace: 'nowrap',
      ...style
    }
  }, rest), icon && /*#__PURE__*/React.createElement("span", {
    style: {
      color: TONES[tone]
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: icon,
    size: 18
  })), text);
}
function ToastStack({
  toasts = [],
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'flex-end',
      gap: 'var(--sp-3)',
      ...style
    }
  }, rest), toasts.slice(0, 3).map((t, i) => /*#__PURE__*/React.createElement(Toast, _extends({
    key: t.id != null ? t.id : i
  }, t, {
    index: i
  }))));
}
Object.assign(__ds_scope, { Toast, ToastStack });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Toast.jsx", error: String((e && e.message) || e) }); }

// components/hud/Crosshair.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Crosshair({
  state = 'idle',
  progress = 0,
  size = 56,
  style,
  ...rest
}) {
  const weak = state === 'weak-point';
  const mob = state === 'mob';
  const r = size / 2 - 8;
  const c = 2 * Math.PI * r;
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      width: size,
      height: size,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size,
    style: {
      display: 'block',
      overflow: 'visible'
    }
  }, /*#__PURE__*/React.createElement("g", {
    transform: 'translate(' + size / 2 + ',' + size / 2 + ')'
  }, weak && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: "var(--line)",
    strokeWidth: "var(--bw-ring-thin)"
  }), /*#__PURE__*/React.createElement("g", {
    transform: "rotate(-90)"
  }, /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: "var(--exit-300)",
    strokeWidth: "var(--bw-ring-thin)",
    strokeLinecap: "round",
    strokeDasharray: c * Math.max(0, 1 - progress) + ' ' + c,
    style: {
      filter: 'drop-shadow(0 0 6px rgba(255,246,222,.7))',
      transition: 'stroke-dasharray var(--dur-instant) linear'
    }
  })), /*#__PURE__*/React.createElement("path", {
    d: "M0 -4 v8 M-4 0 h8",
    stroke: "var(--exit-300)",
    strokeWidth: "1.5",
    strokeLinecap: "round"
  })), !weak && /*#__PURE__*/React.createElement("circle", {
    r: mob ? 3 : 2,
    fill: mob ? 'var(--fg-1)' : 'var(--fg-2)',
    style: {
      filter: mob ? 'drop-shadow(0 0 7px rgba(242,246,252,.9))' : 'none',
      transition: 'r var(--dur-fast) var(--ease-out)'
    }
  }))));
}
Object.assign(__ds_scope, { Crosshair });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/hud/Crosshair.jsx", error: String((e && e.message) || e) }); }

// components/hud/DamageArc.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const ORIGIN = {
  left: 0,
  right: 180,
  top: 270,
  bottom: 90
};
function DamageArc({
  from = 'left',
  intensity = 1,
  style,
  ...rest
}) {
  const deg = typeof from === 'number' ? from : ORIGIN[from] || 0;
  return /*#__PURE__*/React.createElement("div", _extends({
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      inset: 0,
      pointerEvents: 'none',
      background: 'radial-gradient(120% 90% at ' + (deg === 180 ? '100%' : deg === 0 ? '0%' : '50%') + ' ' + (deg === 270 ? '0%' : deg === 90 ? '100%' : '50%') + ', color-mix(in srgb, var(--danger-500) ' + Math.round(38 * intensity) + '%, transparent) 0%, transparent 42%)',
      opacity: intensity,
      transition: 'opacity var(--dur-base) var(--ease-out)',
      ...style
    }
  }, rest));
}
Object.assign(__ds_scope, { DamageArc });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/hud/DamageArc.jsx", error: String((e && e.message) || e) }); }

// components/hud/HealthRing.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function HealthRing({
  hp = 100,
  max = 100,
  regen,
  size = 76,
  showNumber,
  style,
  ...rest
}) {
  const pct = Math.max(0, Math.min(1, hp / max));
  const r = size / 2 - 5;
  const c = 2 * Math.PI * r;
  const low = pct <= 0.3;
  const col = low ? 'var(--danger-300)' : 'var(--fog-300)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      width: size,
      height: size,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size,
    style: {
      display: 'block'
    }
  }, /*#__PURE__*/React.createElement("g", {
    transform: 'translate(' + size / 2 + ',' + size / 2 + ') rotate(-90)'
  }, /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: "var(--line-faint)",
    strokeWidth: "var(--bw-ring)"
  }), /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: col,
    strokeWidth: "var(--bw-ring)",
    strokeLinecap: "round",
    strokeDasharray: c * pct + ' ' + c,
    style: {
      opacity: 0.35 + 0.65 * pct,
      filter: 'drop-shadow(0 0 ' + (4 + 8 * pct) + 'px ' + (low ? 'rgba(255,156,144,.55)' : 'rgba(195,212,234,.45)') + ')',
      transition: 'stroke-dasharray var(--dur-base) var(--ease-out)',
      animation: regen ? 'lucid-shimmer 1400ms var(--ease-in-out) infinite' : 'none'
    }
  }))), showNumber && /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      font: '400 var(--fs-body)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: col,
      textShadow: '0 1px 8px rgba(7,11,18,.9)'
    }
  }, Math.round(hp)));
}
Object.assign(__ds_scope, { HealthRing });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/hud/HealthRing.jsx", error: String((e && e.message) || e) }); }

// components/hud/MoonLives.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function MoonLives({
  lives = 1,
  max,
  size = 22,
  style,
  ...rest
}) {
  const total = max != null ? max : lives;
  const cells = [];
  for (let i = 0; i < total; i++) {
    const alive = i < lives;
    const last = alive && i === lives - 1;
    cells.push(/*#__PURE__*/React.createElement("span", {
      key: i,
      style: {
        display: 'block',
        color: alive ? 'var(--fg-1)' : 'var(--fg-4)',
        opacity: alive ? 1 : 0.4,
        filter: last ? 'drop-shadow(0 0 7px rgba(242,246,252,.75))' : 'none',
        animation: last ? 'lucid-breathe 2600ms var(--ease-in-out) infinite' : 'none'
      }
    }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
      name: "moon",
      size: size,
      strokeWidth: alive ? 1.5 : 1
    })));
  }
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      gap: 'var(--sp-3)',
      alignItems: 'center',
      ...style
    }
  }, rest), cells);
}
Object.assign(__ds_scope, { MoonLives });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/hud/MoonLives.jsx", error: String((e && e.message) || e) }); }

// components/hud/TimerArc.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const mmss = s => Math.floor(Math.max(0, s) / 60) + ':' + String(Math.floor(Math.max(0, s) % 60)).padStart(2, '0');
function TimerArc({
  seconds = 0,
  total = 300,
  label,
  size = 148,
  urgent,
  phase,
  style,
  ...rest
}) {
  const left = Math.max(0, seconds);
  const hot = urgent != null ? urgent : left <= 30;
  const filled = total > 0 ? Math.min(1, 1 - left / total) : 0;
  const r = size / 2 - 8;
  const c = 2 * Math.PI * r;
  const stroke = hot ? 'var(--danger-300)' : 'var(--exit-500)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      width: size,
      height: size / 2 + 34,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size / 2 + 6,
    viewBox: '0 0 ' + size + ' ' + (size / 2 + 6),
    style: {
      position: 'absolute',
      top: 0,
      left: 0
    }
  }, /*#__PURE__*/React.createElement("g", {
    transform: 'translate(' + size / 2 + ',' + (size / 2 + 2) + ')'
  }, /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: "var(--line-faint)",
    strokeWidth: "var(--bw-ring)",
    strokeDasharray: c / 2 + ' ' + c,
    transform: "rotate(180)"
  }), /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: stroke,
    strokeWidth: "var(--bw-ring)",
    strokeLinecap: "butt",
    strokeDasharray: c / 2 * filled + ' ' + c,
    transform: "rotate(180)",
    style: {
      filter: hot ? 'drop-shadow(0 0 6px rgba(255,156,144,.6))' : 'none',
      animation: hot ? 'lucid-pulse var(--pulse-timer) var(--ease-in-out) infinite' : 'none'
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: size / 2 - 42,
      textAlign: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-title)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      letterSpacing: '0.02em',
      color: hot ? 'var(--danger-300)' : 'var(--fg-1)',
      textShadow: '0 1px 12px rgba(7,11,18,.9)'
    }
  }, mmss(left)), (phase || label) && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 6,
      font: '500 var(--fs-micro)/1 var(--font-body)',
      letterSpacing: 'var(--ls-caps)',
      textTransform: 'uppercase',
      color: 'var(--fg-3)',
      textShadow: '0 1px 8px rgba(7,11,18,.9)'
    }
  }, phase || label)));
}
Object.assign(__ds_scope, { TimerArc });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/hud/TimerArc.jsx", error: String((e && e.message) || e) }); }

// components/lobby/PlayerRow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function PlayerRow({
  name,
  host,
  role,
  ready,
  self,
  onRole,
  onReady,
  style,
  ...rest
}) {
  const card = active => ({
    display: 'flex',
    alignItems: 'center',
    gap: 'var(--sp-3)',
    padding: '10px var(--sp-4)',
    minHeight: 'var(--hit-min)',
    borderRadius: 'var(--r-2)',
    cursor: self ? 'pointer' : 'default',
    background: active ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 30%, transparent)',
    border: 'var(--bw-hair) solid ' + (active ? 'var(--exit-500)' : 'var(--line)'),
    color: active ? 'var(--exit-300)' : 'var(--fg-3)',
    font: '400 var(--fs-body)/1 var(--font-body)',
    transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)'
  });
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)',
      padding: 'var(--pad-row)',
      borderRadius: 'var(--r-2)',
      background: self ? 'color-mix(in srgb, var(--mist-500) 45%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (self ? 'var(--line)' : 'transparent'),
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 44,
      height: 44,
      flex: '0 0 auto',
      borderRadius: 'var(--r-1)',
      background: 'var(--ink-800)',
      border: 'var(--bw-hair) solid var(--line)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      font: '300 var(--fs-heading)/1 var(--font-display)',
      color: 'var(--fg-3)'
    }
  }, (name || '?').slice(0, 1)), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: '1 1 auto',
      minWidth: 0,
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1.2 var(--font-body)',
      color: 'var(--text-strong)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, name), host && /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--exit-500)'
    },
    title: "Host"
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "crown",
    size: 16
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-3)',
      flex: '0 0 auto'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: card(role === 'nightmare'),
    onClick: self && onRole ? () => onRole('nightmare') : undefined
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "role-nightmare",
    size: 20
  }), " Nightmare"), /*#__PURE__*/React.createElement("div", {
    style: card(role === 'sleeper'),
    onClick: self && onRole ? () => onRole('sleeper') : undefined
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: "role-sleeper",
    size: 20
  }), " Sleeper")), /*#__PURE__*/React.createElement("div", {
    onClick: self && onReady ? () => onReady(!ready) : undefined,
    title: ready ? 'Ready' : 'Not ready',
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      flex: '0 0 auto',
      width: 116,
      padding: '8px var(--sp-4)',
      minHeight: 'var(--hit-min)',
      boxSizing: 'border-box',
      borderRadius: 'var(--r-2)',
      cursor: self ? 'pointer' : 'default',
      border: 'var(--bw-hair) solid ' + (ready ? 'var(--sleeper-3)' : 'var(--line-faint)'),
      background: ready ? 'color-mix(in srgb, var(--sleeper-3) 16%, transparent)' : 'transparent',
      color: ready ? '#7ee0bd' : 'var(--fg-3)',
      font: '400 var(--fs-body)/1 var(--font-body)'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: ready ? 'ready' : 'unready',
    size: 20
  }), ready ? 'Ready' : 'Waiting'));
}
Object.assign(__ds_scope, { PlayerRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/lobby/PlayerRow.jsx", error: String((e && e.message) || e) }); }

// components/lobby/ScoreboardRow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function ScoreboardRow({
  rank,
  name,
  index,
  score,
  delta,
  rounds,
  woke,
  consumed,
  status,
  header,
  highlight,
  style,
  ...rest
}) {
  const cell = {
    font: '400 var(--fs-heading)/1.2 var(--font-body)',
    fontVariantNumeric: 'tabular-nums',
    color: 'var(--fg-2)',
    textAlign: 'right',
    flex: '0 0 auto',
    whiteSpace: 'nowrap'
  };
  const head = {
    ...cell,
    font: '500 var(--fs-micro)/1 var(--font-body)',
    letterSpacing: 'var(--ls-caps)',
    textTransform: 'uppercase',
    color: 'var(--fg-3)'
  };
  const c = header ? head : cell;
  const col = index ? 'var(--sleeper-' + index + ')' : 'var(--fg-3)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)',
      padding: header ? '0 var(--sp-4) var(--sp-3)' : 'var(--sp-3) var(--sp-4)',
      borderTop: header ? 'none' : 'var(--bw-hair) solid var(--line-faint)',
      background: highlight ? 'color-mix(in srgb, var(--mist-500) 50%, transparent)' : 'transparent',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      minWidth: 26,
      textAlign: 'left',
      color: 'var(--fg-3)'
    }
  }, rank), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: '1 1 auto',
      minWidth: 0,
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)'
    }
  }, !header && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": !index,
    style: {
      width: 32,
      height: 32,
      flex: '0 0 auto',
      boxSizing: 'border-box',
      borderRadius: 'var(--r-full)',
      textAlign: 'center',
      font: '600 var(--fs-heading)/28px var(--font-body)',
      ...(index ? {
        border: '2px solid ' + col,
        background: col,
        color: '#07110b'
      } : null)
    }
  }, index || ''), /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      textAlign: 'left',
      color: header ? 'var(--fg-3)' : 'var(--text-strong)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, name), status && !header && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, status)), rounds != null && /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      minWidth: 124
    }
  }, rounds), woke != null && /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      minWidth: 64
    }
  }, woke), consumed != null && /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      minWidth: 92
    }
  }, consumed), /*#__PURE__*/React.createElement("span", {
    style: {
      ...c,
      minWidth: 150,
      color: header ? 'var(--fg-3)' : 'var(--fg-1)',
      display: 'flex',
      justifyContent: 'flex-end',
      alignItems: 'baseline',
      gap: 6
    }
  }, score, delta != null && !header && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-heading)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: delta > 0 ? 'var(--exit-500)' : 'var(--fg-4)'
    }
  }, delta > 0 ? '+' + delta : delta)));
}
Object.assign(__ds_scope, { ScoreboardRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/lobby/ScoreboardRow.jsx", error: String((e && e.message) || e) }); }

// components/nightmare/CooldownRing.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function CooldownRing({
  progress = 0,
  size = 56,
  thickness,
  tone = 'exit',
  label,
  children,
  style,
  ...rest
}) {
  const r = size / 2 - 3;
  const c = 2 * Math.PI * r;
  const ready = progress <= 0;
  const col = tone === 'danger' ? 'var(--danger-300)' : tone === 'fog' ? 'var(--fog-300)' : 'var(--exit-500)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      width: size,
      height: size,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size,
    style: {
      position: 'absolute',
      inset: 0,
      display: 'block'
    }
  }, /*#__PURE__*/React.createElement("g", {
    transform: 'translate(' + size / 2 + ',' + size / 2 + ') rotate(-90)'
  }, /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: "var(--line-faint)",
    strokeWidth: thickness || 'var(--bw-ring-thin)'
  }), /*#__PURE__*/React.createElement("circle", {
    r: r,
    fill: "none",
    stroke: ready ? col : 'var(--exit-700)',
    strokeWidth: thickness || 'var(--bw-ring-thin)',
    strokeLinecap: "butt",
    strokeDasharray: c * (ready ? 1 : progress) + ' ' + c,
    style: {
      opacity: ready ? 1 : 0.85,
      transition: 'stroke-dasharray var(--dur-base) linear'
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 1,
      color: ready ? 'var(--fg-1)' : 'var(--fg-4)'
    }
  }, children, label && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 11px/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'inherit'
    }
  }, label)));
}
Object.assign(__ds_scope, { CooldownRing });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/nightmare/CooldownRing.jsx", error: String((e && e.message) || e) }); }

// components/nightmare/PaletteTile.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Six faces in a cross net: top, west, north, east, south, bottom.
   A filled dot is a connector; a dashed empty square is a wall. */
function ConnectorNet({
  mask = '010100',
  size = 42,
  style
}) {
  const cell = size / 3.4,
    gap = 1.5;
  const pos = [[1, 0], [0, 1], [1, 1], [2, 1], [1, 2], [1, 3]];
  return /*#__PURE__*/React.createElement("svg", {
    width: cell * 3 + gap * 2,
    height: cell * 4 + gap * 3,
    style: {
      display: 'block',
      overflow: 'visible',
      ...style
    },
    "aria-label": "connector faces"
  }, pos.map(([cx, cy], i) => {
    const on = mask[i] === '1';
    const x = cx * (cell + gap),
      y = cy * (cell + gap);
    return /*#__PURE__*/React.createElement("g", {
      key: i
    }, /*#__PURE__*/React.createElement("rect", {
      x: x,
      y: y,
      width: cell,
      height: cell,
      rx: "1.5",
      fill: "none",
      stroke: "currentColor",
      strokeWidth: "1",
      strokeOpacity: on ? 0.9 : 0.3,
      strokeDasharray: on ? undefined : '2 2'
    }), on && /*#__PURE__*/React.createElement("circle", {
      cx: x + cell / 2,
      cy: y + cell / 2,
      r: cell * 0.22,
      fill: "currentColor"
    }));
  }));
}
function PaletteTile({
  name,
  cost,
  hotkey,
  mask,
  category = 'cat-connector',
  selected,
  budget,
  disabled,
  onClick,
  style,
  ...rest
}) {
  const off = disabled || budget != null && budget < cost;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: off ? undefined : onClick,
    title: name,
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-3)',
      alignItems: 'stretch',
      padding: 'var(--sp-3)',
      width: '100%',
      textAlign: 'left',
      cursor: off ? 'not-allowed' : 'pointer',
      background: selected ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 32%, transparent)',
      border: 'var(--bw-hair) solid ' + (selected ? 'var(--exit-500)' : 'var(--line)'),
      borderRadius: 'var(--r-3)',
      opacity: off ? 0.45 : 1,
      transition: 'background var(--dur-fast) var(--ease-out), border-color var(--dur-fast) var(--ease-out)',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'flex-start',
      justifyContent: 'space-between',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: selected ? 'var(--exit-300)' : 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Icon, {
    name: category,
    size: 26
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      color: selected ? 'var(--exit-500)' : 'var(--fg-3)'
    }
  }, /*#__PURE__*/React.createElement(ConnectorNet, {
    mask: mask,
    size: 34
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: 'var(--sp-2)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1.15 var(--font-body)',
      color: 'var(--text-strong)'
    }
  }, name), hotkey != null && /*#__PURE__*/React.createElement("kbd", {
    style: {
      font: '500 11px/1 var(--font-body)',
      color: 'var(--fg-3)',
      border: 'var(--bw-hair) solid var(--line)',
      borderRadius: 'var(--r-1)',
      padding: '2px 5px'
    }
  }, hotkey)), /*#__PURE__*/React.createElement(__ds_scope.CostBadge, {
    cost: cost,
    budget: budget,
    size: "sm",
    style: {
      alignSelf: 'flex-start'
    }
  }));
}
Object.assign(__ds_scope, { ConnectorNet, PaletteTile });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/nightmare/PaletteTile.jsx", error: String((e && e.message) || e) }); }

// components/nightmare/RejectionLabel.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const REJECTIONS = ['Door is solid', "Doesn't fit here", 'Would trap {name}', 'Not enough budget ({have} / {cost})', 'Not a door'];
function RejectionLabel({
  reason,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '6px var(--sp-4)',
      borderRadius: 'var(--r-1)',
      background: 'color-mix(in srgb, var(--ink-900) 82%, transparent)',
      border: 'var(--bw-hair) solid var(--danger-500)',
      boxShadow: 'var(--glow-danger)',
      font: '500 var(--fs-body)/1 var(--font-body)',
      color: 'var(--danger-300)',
      whiteSpace: 'nowrap',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("svg", {
    width: "14",
    height: "14",
    viewBox: "0 0 14 14",
    "aria-hidden": "true",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: "1.5",
    strokeLinecap: "round"
  }, /*#__PURE__*/React.createElement("path", {
    d: "M3 3l8 8M11 3l-8 8"
  })), reason);
}
Object.assign(__ds_scope, { REJECTIONS, RejectionLabel });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/nightmare/RejectionLabel.jsx", error: String((e && e.message) || e) }); }

// components/nightmare/SleeperMarker.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const SLEEPER_COLORS = ['var(--sleeper-1)', 'var(--sleeper-2)', 'var(--sleeper-3)', 'var(--sleeper-4)'];
const SHAPES = ['0', '4', '12', '50'];

/* 'md' is for markers and rows layered over the dream, where the chip must stay
   small. 'lg' is for Results and any scoreboard read over a screen share: every
   VALUE goes to --fs-heading or above, which is the 22px floor in §Minimum
   scales. Static column labels may stay smaller — they name a column, they are
   not a value anyone reads off. */
const SCALES = {
  md: {
    box: 34,
    num: 'var(--fs-micro)',
    name: 'var(--fs-body)',
    sub: 'var(--fs-micro)'
  },
  lg: {
    box: 46,
    num: 'var(--fs-heading)',
    name: 'var(--fs-title)',
    sub: 'var(--fs-body)'
  }
};
function SleeperMarker({
  index = 1,
  name,
  facing,
  selected,
  status = 'in-dream',
  shape,
  scale = 'md',
  size,
  style,
  ...rest
}) {
  const sc = SCALES[scale] || SCALES.md;
  size = size != null ? size : sc.box;
  const col = SLEEPER_COLORS[(index - 1) % 4];
  const out = status !== 'in-dream';
  const radius = shape ? SHAPES[(index - 1) % 4] + (SHAPES[(index - 1) % 4] === '50' ? '%' : 'px') : 'var(--r-full)';
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'relative',
      width: size,
      height: size,
      flex: '0 0 auto'
    }
  }, facing != null && /*#__PURE__*/React.createElement("svg", {
    width: size,
    height: size,
    style: {
      position: 'absolute',
      inset: 0,
      transform: 'rotate(' + facing + 'deg)'
    },
    "aria-hidden": "true"
  }, /*#__PURE__*/React.createElement("path", {
    d: 'M' + size / 2 + ' 0 l4.5 7 h-9 Z',
    fill: col,
    opacity: out ? 0.4 : 1
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: scale === 'lg' ? 6 : 5,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: radius,
      background: out ? 'transparent' : col,
      border: '2px solid ' + col,
      opacity: out ? 0.5 : 1,
      boxShadow: selected ? '0 0 0 2px var(--exit-300), 0 0 14px rgba(255,246,222,.55)' : '0 1px 6px rgba(7,11,18,.8)',
      font: '600 ' + sc.num + '/1 var(--font-body)',
      color: out ? col : '#07110b'
    }
  }, index)), name && /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      lineHeight: 1.15
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 ' + sc.name + '/1.15 var(--font-body)',
      color: out ? 'var(--fg-3)' : 'var(--text-strong)',
      textShadow: '0 1px 6px rgba(7,11,18,.9)'
    }
  }, name), out && /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 ' + sc.sub + '/1.2 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, status === 'awake' ? 'awake' : 'consumed')));
}
Object.assign(__ds_scope, { SLEEPER_COLORS, SleeperMarker });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/nightmare/SleeperMarker.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/LobbyScreen.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  MistPanel,
  Button,
  Toggle,
  Slider,
  Icon,
  PlayerRow,
  ScoreboardRow
} = window.DesignSystem_427bea;
function LobbyScreen({
  onStart,
  onBack
}) {
  const [role, setRole] = React.useState('nightmare');
  const [ready, setReady] = React.useState(false);
  const [headStart, setHeadStart] = React.useState(30);
  const [dawn, setDawn] = React.useState(300);
  const [hints, setHints] = React.useState(true);
  const others = [{
    name: 'Ben',
    role: 'sleeper',
    ready: true
  }, {
    name: 'Cara',
    role: 'sleeper',
    ready: true
  }, {
    name: 'Dev',
    role: 'sleeper',
    ready: false
  }];
  const allReady = ready && others.every(o => o.ready);
  const reason = !ready ? 'Ready up to start' : !allReady ? 'Waiting for Dev to ready up' : null;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "void"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      padding: 'var(--sp-8)',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      justifyContent: 'space-between'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '300 var(--fs-title)/1 var(--font-display)',
      letterSpacing: '0.2em',
      color: 'var(--fg-1)'
    }
  }, "LUCID"), /*#__PURE__*/React.createElement(MicroLabel, null, "Lobby \xB7 4 of 5")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "Invite code"), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-heading)/1 var(--font-body)',
      letterSpacing: '0.2em',
      color: 'var(--exit-500)',
      padding: '8px var(--sp-5)',
      border: 'var(--bw-hair) solid var(--line-strong)',
      borderRadius: 'var(--r-2)'
    }
  }, "MOTH-914"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    hotkey: "Esc",
    onClick: onBack
  }, "Leave"))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: 'flex',
      gap: 'var(--sp-6)',
      minHeight: 0
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    title: "Who is dreaming",
    aside: "pick a role",
    style: {
      flex: '1 1 auto'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-2)'
    }
  }, /*#__PURE__*/React.createElement(PlayerRow, {
    name: "Anna",
    host: true,
    self: true,
    role: role,
    ready: ready,
    onRole: setRole,
    onReady: setReady
  }), others.map(o => /*#__PURE__*/React.createElement(PlayerRow, _extends({
    key: o.name
  }, o))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)',
      padding: 'var(--pad-row)',
      border: '1px dashed var(--line-faint)',
      borderRadius: 'var(--r-2)',
      minHeight: 68
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "One seat free \u2014 share MOTH-914"))), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Session leaderboard"), /*#__PURE__*/React.createElement(ScoreboardRow, {
    header: true,
    rank: "#",
    name: "Player",
    rounds: "As Nightmare",
    woke: "Woke",
    consumed: "Consumed",
    score: "Score"
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 1,
    index: 2,
    name: "Ben",
    rounds: 1,
    woke: 3,
    consumed: 1,
    score: 612
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 2,
    index: 1,
    name: "Anna",
    rounds: 2,
    woke: 2,
    consumed: 2,
    score: 430,
    highlight: true
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 3,
    index: 3,
    name: "Cara",
    rounds: 0,
    woke: 1,
    consumed: 3,
    score: 205
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 4,
    index: 4,
    name: "Dev",
    rounds: 0,
    woke: 1,
    consumed: 3,
    score: 188
  })), /*#__PURE__*/React.createElement(MistPanel, {
    title: "The dream",
    aside: "host only",
    style: {
      flex: '0 0 420px'
    }
  }, /*#__PURE__*/React.createElement(Slider, {
    label: "Head start",
    value: headStart,
    min: 5,
    max: 120,
    unit: "s",
    onChange: setHeadStart
  }), /*#__PURE__*/React.createElement(Slider, {
    label: "Dawn",
    value: dawn,
    min: 120,
    max: 900,
    step: 30,
    onChange: setDawn,
    format: s => Math.floor(s / 60) + ':' + String(s % 60).padStart(2, '0')
  }), /*#__PURE__*/React.createElement(Slider, {
    label: "Sleeper lives",
    value: 1,
    min: 1,
    max: 5,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Packs"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-2)'
    }
  }, /*#__PURE__*/React.createElement(Toggle, {
    label: "Core",
    hint: "Connectors, shafts, chicanes",
    checked: true,
    disabled: true
  }), /*#__PURE__*/React.createElement(Toggle, {
    label: "Attic",
    hint: "Dust, low ceilings, a nest",
    checked: true,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Toggle, {
    label: "Waterworks",
    hint: "Molasses runs slower here",
    checked: false,
    onChange: () => {}
  })), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(Toggle, {
    label: "Hint cards",
    hint: "Shown for the first three rounds",
    checked: hints,
    onChange: setHints
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    full: true,
    reason: reason,
    onClick: onStart,
    style: {
      marginTop: 'var(--sp-6)'
    }
  }, "Start the dream")))));
}
Object.assign(window, {
  LobbyScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/LobbyScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/NightmareMockups.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  MistPanel,
  Button,
  Icon,
  TimerArc,
  CooldownRing,
  PaletteTile,
  ConnectorNet,
  SleeperMarker,
  RejectionLabel,
  CostBadge,
  ToastStack,
  MoonLives
} = window.DesignSystem_427bea;
const NM_CATS = [['cat-connector', 'Connectors'], ['cat-vertical', 'Vertical'], ['cat-chicane', 'Chicanes'], ['cat-mob', 'Mobs']];

/* Budget lives TOP LEFT (UI.md §8): the number, the trickle ring, the rate. */
function BudgetCluster({
  budget,
  trickle = 0.5,
  paused
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: trickle,
    size: 58
  }, /*#__PURE__*/React.createElement("svg", {
    width: "15",
    height: "15",
    viewBox: "0 0 8 8",
    "aria-hidden": "true"
  }, /*#__PURE__*/React.createElement("path", {
    d: "M4 0 8 4 4 8 0 4Z",
    fill: "var(--exit-500)"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      lineHeight: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-title)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)',
      textShadow: '0 1px 12px rgba(7,11,18,.9)'
    }
  }, budget), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 5,
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, window.RULES.trickle), paused && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 5,
      font: '500 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--danger-300)'
    }
  }, "Building paused")));
}

/* One Sleeper row (UI.md §8): chip + number, name, status, health bar, moons,
   "depth 7 · exit 11", last event. Expanding shows that dream's live state. */
function SleeperRow({
  index,
  name,
  status,
  hp,
  lives,
  depth,
  exit,
  event,
  selected,
  expanded,
  mobs,
  jammed,
  onClick
}) {
  const out = status === 'awake' || status === 'consumed';
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      padding: 'var(--pad-row)',
      borderRadius: 'var(--r-2)',
      cursor: 'pointer',
      border: 'var(--bw-hair) solid ' + (selected ? 'var(--exit-500)' : out ? 'var(--line-faint)' : 'var(--line)'),
      background: selected ? 'color-mix(in srgb, var(--exit-500) 10%, transparent)' : 'color-mix(in srgb, var(--ink-900) 32%, transparent)',
      opacity: out ? 0.62 : 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: index,
    name: name,
    status: out ? status : 'in-dream'
  }), !out && /*#__PURE__*/React.createElement(MoonLives, {
    lives: lives,
    max: window.RULES.lives,
    size: 15
  })), !out && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-3)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      height: 3,
      borderRadius: 2,
      background: 'var(--line-faint)',
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: '0 auto 0 0',
      width: hp + '%',
      background: hp <= 30 ? 'var(--danger-300)' : 'var(--fog-300)',
      borderRadius: 2
    }
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: hp <= 30 ? 'var(--danger-300)' : 'var(--fg-3)',
      width: 30,
      textAlign: 'right'
    }
  }, hp)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 6,
      font: '400 var(--fs-micro)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: depth === exit ? 'var(--exit-500)' : 'var(--fg-3)'
    }
  }, "depth ", depth, " \xB7 exit ", exit)), event && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 6,
      font: '400 var(--fs-micro)/1.3 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, event), expanded && !out && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-4)',
      paddingTop: 'var(--sp-3)',
      borderTop: '1px solid var(--line-faint)',
      display: 'flex',
      flexDirection: 'column',
      gap: 5
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '500 11px/1 var(--font-body)',
      letterSpacing: 'var(--ls-caps)',
      textTransform: 'uppercase',
      color: 'var(--fg-4)'
    }
  }, "In this dream"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "cat-mob",
    size: 15
  }), mobs, " Shades alive"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "weak-point",
    size: 15
  }), jammed, " traps jammed")));
}

/* Hover-peek on a trap cube: weak point, its manual trigger, the 6 s cooldown. */
function TrapPeek({
  cube,
  cooldown = 0,
  x,
  y
}) {
  if (!cube) return null; // never let a missing lookup unmount the screen
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: x,
      top: y,
      display: 'flex',
      alignItems: 'flex-start',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: cooldown,
    size: 44,
    tone: "fog",
    label: cooldown > 0 ? String(Math.ceil((1 - cooldown) * window.RULES.triggerCooldown)) : 'T'
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-trigger",
    size: 18
  })), /*#__PURE__*/React.createElement(MistPanel, {
    pad: "var(--sp-4)",
    style: {
      width: 250
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-1)'
    }
  }, cube.name), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-3)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--fg-3)'
    }
  }, /*#__PURE__*/React.createElement(ConnectorNet, {
    mask: cube.mask,
    size: 30
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-micro)/1.5 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "weak point: ", cube.weak, /*#__PURE__*/React.createElement("br", null), "trigger: ", cube.trigger)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-3)',
      paddingTop: 'var(--sp-3)',
      borderTop: '1px solid var(--line-faint)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: cooldown > 0 ? 'var(--fg-4)' : 'var(--exit-500)'
    }
  }, cooldown > 0 ? 'Cooling — ' + Math.ceil((1 - cooldown) * window.RULES.triggerCooldown) + ' s' : 'T to ' + cube.trigger)));
}

/* The lattice plan. cells are [x, y] on the current layer. */
function Lattice({
  cells,
  fogDoors = [],
  exits = [],
  solids = [],
  markers = [],
  ghost,
  layer = 0,
  trap,
  children
}) {
  const S = 76,
    ox = 470,
    oy = 130;
  const px = c => ox + c * S,
    py = c => oy + c * S;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "god"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      opacity: 0.45,
      backgroundImage: 'linear-gradient(rgba(147,167,196,.08) 1px, transparent 1px), linear-gradient(90deg, rgba(147,167,196,.08) 1px, transparent 1px)',
      backgroundSize: S + 'px ' + S + 'px',
      backgroundPosition: ox + 'px ' + oy + 'px'
    }
  }), /*#__PURE__*/React.createElement("svg", {
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%'
    }
  }, cells.map(([cx, cy, kind], i) => /*#__PURE__*/React.createElement("g", {
    key: i
  }, /*#__PURE__*/React.createElement("rect", {
    x: px(cx) + 3,
    y: py(cy) + 3,
    width: S - 6,
    height: S - 6,
    rx: "3",
    fill: kind === 'start' ? 'rgba(58,74,99,.62)' : 'rgba(46,65,96,.5)',
    stroke: kind === 'start' ? 'var(--fog-300)' : 'rgba(195,212,234,.3)',
    strokeWidth: kind === 'start' ? 1.6 : 1
  }), kind === 'start' && /*#__PURE__*/React.createElement("text", {
    x: px(cx) + S / 2,
    y: py(cy) + S / 2 + 4,
    textAnchor: "middle",
    style: {
      font: '400 11px var(--font-body)',
      fill: 'var(--fg-3)'
    }
  }, "start"))), solids.map(([cx, cy, side], i) => {
    const h = side === 'n' || side === 's';
    const w = h ? S - 22 : 5,
      hh = h ? 5 : S - 22;
    const dx = side === 'e' ? S - 6 : side === 'w' ? 1 : 11;
    const dy = side === 's' ? S - 6 : side === 'n' ? 1 : 11;
    return /*#__PURE__*/React.createElement("rect", {
      key: 's' + i,
      x: px(cx) + dx,
      y: py(cy) + dy,
      width: w,
      height: hh,
      fill: "var(--door-solid)"
    });
  }), fogDoors.map(([cx, cy], i) => /*#__PURE__*/React.createElement("rect", {
    key: 'f' + i,
    x: px(cx) + 14,
    y: py(cy) + 14,
    width: S - 28,
    height: S - 28,
    rx: "2",
    fill: "rgba(92,114,149,.26)",
    stroke: "var(--fog-500)",
    strokeWidth: "1.5",
    strokeDasharray: "4 3"
  })), exits.map(([cx, cy], i) => /*#__PURE__*/React.createElement("g", {
    key: 'e' + i,
    transform: 'translate(' + (px(cx) + S / 2) + ',' + (py(cy) + S / 2) + ')'
  }, /*#__PURE__*/React.createElement("circle", {
    r: "30",
    fill: "rgba(255,246,222,.12)"
  }), /*#__PURE__*/React.createElement("rect", {
    x: "-16",
    y: "-16",
    width: "32",
    height: "32",
    rx: "2",
    fill: "rgba(255,246,222,.72)",
    stroke: "var(--exit-300)",
    strokeWidth: "2",
    style: {
      filter: 'drop-shadow(0 0 16px rgba(255,246,222,.85))'
    }
  }))), ghost && /*#__PURE__*/React.createElement("rect", {
    x: px(ghost[0]) + 3,
    y: py(ghost[1]) + 3,
    width: S - 6,
    height: S - 6,
    rx: "3",
    fill: ghost[2] === 'reject' ? 'rgba(217,69,58,.3)' : 'rgba(0,158,115,.3)',
    stroke: ghost[2] === 'reject' ? 'var(--danger-500)' : 'var(--sleeper-3)',
    strokeWidth: "2",
    strokeDasharray: ghost[2] === 'reject' ? '6 4' : undefined
  })), markers.map((m, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      position: 'absolute',
      left: px(m.x) + S / 2 - 42,
      top: py(m.y) + S / 2 - 17
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: m.index,
    name: m.name,
    facing: m.facing,
    selected: m.selected
  }))), trap && /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: px(trap.x) + 18,
      top: py(trap.y) + 18,
      width: S - 36,
      height: S - 36,
      borderRadius: 2,
      border: '1.5px solid var(--fog-300)',
      background: 'rgba(143,166,200,.14)'
    }
  }), /*#__PURE__*/React.createElement(TrapPeek, {
    cube: trap.cube,
    cooldown: trap.cooldown,
    x: px(trap.x) + S + 10,
    y: py(trap.y) - 4
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--sp-6)',
      top: '50%',
      transform: 'translateY(-50%)',
      display: 'flex',
      flexDirection: 'column-reverse',
      gap: 'var(--sp-2)',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 11px/1 var(--font-body)',
      color: 'var(--fg-4)',
      marginTop: 6
    }
  }, "PgDn"), [-1, 0, 1, 2].map(l => /*#__PURE__*/React.createElement("div", {
    key: l,
    style: {
      width: 42,
      height: 34,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: 'var(--r-1)',
      border: 'var(--bw-hair) solid ' + (l === layer ? 'var(--exit-500)' : 'var(--line-faint)'),
      background: l === layer ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 45%, transparent)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: l === layer ? 'var(--exit-300)' : 'var(--fg-4)'
    }
  }, l > 0 ? '+' + l : l)), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 11px/1 var(--font-body)',
      color: 'var(--fg-4)',
      marginBottom: 6
    }
  }, "PgUp")), children);
}

/* The full Nightmare chrome, corrected to UI.md §8's region table. */
function NightmareChrome({
  phase,
  seconds,
  urgent,
  budget,
  trickle,
  paused,
  cubes,
  cat,
  pick,
  toasts,
  sleepers,
  target,
  effects,
  children,
  rejection
}) {
  const shown = window.CUBES.filter(c => c.category === cat);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, children, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 356,
      top: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "top left"
  }, /*#__PURE__*/React.createElement(BudgetCluster, {
    budget: budget,
    trickle: trickle,
    paused: paused
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-5)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: seconds,
    total: window.RULES.dawn,
    size: 180,
    phase: phase,
    urgent: urgent
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 340,
      top: 'var(--sp-6)',
      marginRight: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: toasts
  })), /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    pad: "var(--sp-5)",
    style: {
      position: 'absolute',
      left: 0,
      top: 0,
      bottom: 0,
      width: 340,
      borderLeft: 0,
      borderTop: 0,
      borderBottom: 0,
      borderRadius: 0
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Palette"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-2)',
      marginBottom: 'var(--sp-4)'
    }
  }, ['Core', 'Attic'].map((p, i) => /*#__PURE__*/React.createElement("div", {
    key: p,
    style: {
      flex: 1,
      padding: '8px 0',
      textAlign: 'center',
      borderRadius: 'var(--r-1)',
      background: i === 0 ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (i === 0 ? 'var(--line-strong)' : 'var(--line-faint)'),
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: i === 0 ? 'var(--fg-1)' : 'var(--fg-3)'
    }
  }, p))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-2)',
      flexWrap: 'wrap',
      marginBottom: 'var(--sp-4)'
    }
  }, NM_CATS.map(([c, label]) => /*#__PURE__*/React.createElement("div", {
    key: c,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 5,
      padding: '5px 8px',
      borderRadius: 'var(--r-1)',
      background: cat === c ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (cat === c ? 'var(--exit-500)' : 'var(--line-faint)'),
      color: cat === c ? 'var(--exit-300)' : 'var(--fg-3)',
      font: '400 11px/1 var(--font-body)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: c,
    size: 16
  }), label))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--sp-3)',
      alignContent: 'start',
      overflow: 'hidden'
    }
  }, shown.slice(0, 8).map((c, i) => /*#__PURE__*/React.createElement(PaletteTile, {
    key: c.name,
    name: c.name,
    cost: c.cost,
    mask: c.mask,
    category: c.category,
    hotkey: i + 1,
    budget: budget,
    selected: pick === c.name
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 11px/1.5 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "1\u20139 select \xB7 R rotate \xB7 Esc cancel \xB7 Ctrl+Tab category")), /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    pad: "var(--sp-5)",
    style: {
      position: 'absolute',
      right: 0,
      top: 0,
      bottom: 0,
      width: 320,
      borderRight: 0,
      borderTop: 0,
      borderBottom: 0,
      borderRadius: 0
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Sleepers"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-3)'
    }
  }, sleepers.map(s => /*#__PURE__*/React.createElement(SleeperRow, _extends({
    key: s.index
  }, s, {
    selected: target === s.index
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 11px/1.5 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "Click a row to target that dream \xB7 F focus \xB7 Home start cube")), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 340,
      right: 320,
      bottom: 0,
      display: 'flex',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    edge: "bottom",
    pad: "var(--sp-4) var(--sp-6)",
    style: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 'var(--sp-6)',
      borderBottom: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      paddingRight: 'var(--sp-5)',
      borderRight: '1px solid var(--line-faint)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-target",
    size: 20,
    color: "var(--fg-3)"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      lineHeight: 1.25
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '500 11px/1 var(--font-body)',
      letterSpacing: 'var(--ls-caps)',
      textTransform: 'uppercase',
      color: 'var(--fg-4)'
    }
  }, "Target"), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: target ? 'var(--exit-300)' : 'var(--fg-1)',
      marginTop: 4
    }
  }, target ? sleepers.find(s => s.index === target).name + "'s dream" : 'Everyone')), /*#__PURE__*/React.createElement("kbd", {
    style: {
      font: '500 11px/1 var(--font-body)',
      color: 'var(--fg-3)',
      border: '1px solid var(--line)',
      borderRadius: 'var(--r-1)',
      padding: '2px 5px'
    }
  }, "Tab")), window.EFFECTS.map(e => {
    const st = effects[e.name] || {};
    const prog = st.cooldown || 0;
    const poor = budget < e.cost;
    return /*#__PURE__*/React.createElement("div", {
      key: e.name,
      style: {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 6,
        opacity: st.active ? 1 : 1
      }
    }, /*#__PURE__*/React.createElement(CooldownRing, {
      progress: prog,
      size: 58,
      tone: st.active ? 'fog' : 'exit',
      label: prog > 0 ? String(Math.ceil((1 - prog) * e.cooldown)) : e.key
    }, /*#__PURE__*/React.createElement(Icon, {
      name: e.icon,
      size: 22
    })), /*#__PURE__*/React.createElement("span", {
      style: {
        font: '400 var(--fs-micro)/1 var(--font-body)',
        color: st.active ? 'var(--fog-300)' : prog > 0 || poor ? 'var(--fg-4)' : 'var(--fg-2)'
      }
    }, st.active ? e.name + ' ' + st.active + 's' : e.name), /*#__PURE__*/React.createElement(CostBadge, {
      cost: e.cost,
      budget: budget,
      size: "sm"
    }));
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      paddingLeft: 'var(--sp-5)',
      borderLeft: '1px solid var(--line-faint)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: 0,
    size: 58
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-possess",
    size: 22
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, "Possess"), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 11px/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "free \xB7 P")), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 11px/1.5 var(--font-body)',
      color: 'var(--fg-4)',
      maxWidth: 118
    }
  }, "Click a mob marker, then P. Building stops while you are in there.")))), rejection);
}

/* ---------------------------------------------------------------- 1 of 3 */
function NightmareHeadStart() {
  return /*#__PURE__*/React.createElement(NightmareChrome, {
    phase: "The Sleepers stir in 0:18",
    seconds: 288,
    urgent: false,
    budget: 11,
    trickle: 0.75,
    cat: "cat-connector",
    pick: "Straight",
    toasts: [],
    target: null,
    effects: {},
    sleepers: [{
      index: 1,
      name: 'Anna',
      hp: 100,
      lives: 1,
      depth: 0,
      exit: 2,
      event: 'in the bedroom'
    }, {
      index: 2,
      name: 'Ben',
      hp: 100,
      lives: 1,
      depth: 0,
      exit: 2,
      event: 'in the bedroom'
    }, {
      index: 3,
      name: 'Cara',
      hp: 100,
      lives: 1,
      depth: 0,
      exit: 2,
      event: 'in the bedroom'
    }, {
      index: 4,
      name: 'Dev',
      hp: 100,
      lives: 1,
      depth: 0,
      exit: 2,
      event: 'in the bedroom'
    }]
  }, /*#__PURE__*/React.createElement(Lattice, {
    cells: [[5, 4, 'start'], [6, 4], [6, 3]],
    fogDoors: [[7, 4], [5, 3]],
    exits: [[6, 2]],
    ghost: [7, 3, 'valid'],
    markers: [],
    layer: 0
  }));
}

/* ---------------------------------------------------------------- 2 of 3 */
function NightmareRejected() {
  return /*#__PURE__*/React.createElement(NightmareChrome, {
    phase: "The Sleepers are running",
    seconds: 166,
    urgent: false,
    budget: 3,
    trickle: 0.25,
    cat: "cat-chicane",
    pick: "Crusher",
    target: 1,
    toasts: [{
      text: 'Not enough budget (3 / 4)',
      tone: 'danger'
    }, {
      text: 'Anna hardened 3 doors in the Cross',
      icon: 'door-solid'
    }],
    effects: {
      Molasses: {
        cooldown: 0.55
      },
      Dark: {
        cooldown: 0
      },
      Fog: {
        cooldown: 0
      }
    },
    sleepers: [{
      index: 1,
      name: 'Anna',
      hp: 88,
      lives: 1,
      depth: 11,
      exit: 11,
      event: 'at an exit door'
    }, {
      index: 2,
      name: 'Ben',
      hp: 45,
      lives: 1,
      depth: 7,
      exit: 11,
      event: 'jamming a latch'
    }, {
      index: 3,
      name: 'Cara',
      status: 'consumed',
      event: 'consumed at 2:40'
    }, {
      index: 4,
      name: 'Dev',
      hp: 100,
      lives: 1,
      depth: 4,
      exit: 11,
      event: 'in the Little maze'
    }]
  }, /*#__PURE__*/React.createElement(Lattice, {
    cells: [[5, 4, 'start'], [6, 4], [7, 4], [7, 3], [8, 3], [8, 2], [9, 2], [6, 5], [6, 6], [7, 6], [5, 3], [4, 3]],
    solids: [[6, 4, 'n'], [7, 4, 'n'], [7, 3, 'w'], [8, 3, 'n'], [6, 5, 'e'], [5, 4, 'n']],
    fogDoors: [[4, 4], [8, 6], [3, 3]],
    exits: [[10, 2]],
    ghost: [9, 3, 'reject'],
    markers: [{
      x: 9,
      y: 2,
      index: 1,
      name: 'Anna',
      facing: 45,
      selected: true
    }, {
      x: 7,
      y: 6,
      index: 2,
      name: 'Ben',
      facing: 90
    }, {
      x: 4,
      y: 3,
      index: 4,
      name: 'Dev',
      facing: 270
    }],
    layer: 0
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 1090,
      top: 400
    }
  }, /*#__PURE__*/React.createElement(RejectionLabel, {
    reason: "Not enough budget (3 / 4)"
  })));
}

/* ---------------------------------------------------------------- 3 of 3 */
function NightmareDawn() {
  const trapdoor = (window.CUBES || []).find(c => c.name === 'Trapdoor');
  return /*#__PURE__*/React.createElement(NightmareChrome, {
    phase: "Dawn in 0:24",
    seconds: 24,
    urgent: true,
    budget: 16,
    trickle: 0.4,
    cat: "cat-mob",
    pick: "Nest",
    target: 2,
    toasts: [{
      text: 'Dev was consumed',
      tone: 'danger'
    }, {
      text: 'Anna woke up',
      icon: 'door-exit',
      tone: 'exit'
    }, {
      text: 'Molasses — don\'t jump',
      icon: 'power-molasses',
      tone: 'effect'
    }],
    effects: {
      Molasses: {
        active: 4
      },
      Dark: {
        cooldown: 0.3
      },
      Fog: {
        cooldown: 0
      }
    },
    sleepers: [{
      index: 2,
      name: 'Ben',
      hp: 32,
      lives: 1,
      depth: 13,
      exit: 14,
      event: 'one door from the exit',
      expanded: true,
      mobs: 2,
      jammed: 1
    }, {
      index: 1,
      name: 'Anna',
      status: 'awake',
      event: 'woke at 4:12'
    }, {
      index: 3,
      name: 'Cara',
      status: 'consumed',
      event: 'consumed at 2:40'
    }, {
      index: 4,
      name: 'Dev',
      status: 'consumed',
      event: 'consumed at 4:36'
    }]
  }, /*#__PURE__*/React.createElement(Lattice, {
    cells: [[5, 4, 'start'], [6, 4], [7, 4], [7, 3], [8, 3], [8, 2], [9, 2], [10, 2], [10, 3], [10, 4], [6, 5], [6, 6], [7, 6], [5, 3]],
    solids: [[6, 4, 'n'], [7, 4, 'n'], [7, 3, 'w'], [8, 3, 'n'], [9, 2, 'n'], [10, 2, 'e'], [6, 5, 'e'], [5, 4, 'n'], [10, 3, 'w']],
    fogDoors: [[4, 4], [8, 6]],
    exits: [[11, 4]],
    markers: [{
      x: 10,
      y: 4,
      index: 2,
      name: 'Ben',
      facing: 90,
      selected: true
    }],
    trap: {
      x: 10,
      y: 3,
      cube: trapdoor,
      cooldown: 0
    },
    layer: 0
  }));
}
Object.assign(window, {
  NightmareHeadStart,
  NightmareRejected,
  NightmareDawn,
  NightmareChrome,
  Lattice,
  SleeperRow,
  TrapPeek,
  BudgetCluster
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/NightmareMockups.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/NightmareView.jsx
try { (() => {
const {
  MistPanel,
  Button,
  Icon,
  TimerArc,
  CooldownRing,
  PaletteTile,
  ConnectorNet,
  SleeperMarker,
  RejectionLabel,
  CostBadge,
  ToastStack,
  Toggle
} = window.DesignSystem_427bea;

/* NOTE: in-browser Babel downcompiles a top-level const in these classic
   scripts to a var, which lands on window. Every module-level name here is
   therefore file-prefixed, and shared game data lives ONLY in Shared.jsx
   (window.CUBES / EFFECTS / RULES) so there is one source of truth. */
const NV_PACKS = ['Core', 'Attic', 'Waterworks'];
const NV_CATS = [['cat-connector', 'Connectors'], ['cat-vertical', 'Vertical'], ['cat-chicane', 'Chicanes'], ['cat-mob', 'Mobs'], ['cat-gimmick', 'Gimmicks']];

/* Budget: the number, the rate, and a trickle ring that fills to the next point. */
function BudgetMeter({
  budget = 12,
  trickle = 0.6,
  rate = '1 per 4 s'
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: trickle,
    size: 54
  }, /*#__PURE__*/React.createElement("svg", {
    width: "14",
    height: "14",
    viewBox: "0 0 8 8",
    "aria-hidden": "true"
  }, /*#__PURE__*/React.createElement("path", {
    d: "M4 0 8 4 4 8 0 4Z",
    fill: "var(--exit-500)"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      lineHeight: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-title)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)'
    }
  }, budget), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 5,
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, rate)));
}

/* The god view: a flat plan of the maze, drawn from geometry only. */
function GodView({
  layer = 2,
  ghost,
  rejected,
  possessing,
  selected
}) {
  const cells = [[3, 2], [4, 2], [5, 2], [5, 3], [5, 4], [6, 4], [7, 4], [7, 3], [4, 5], [4, 6], [3, 6], [2, 6], [7, 5], [8, 5], [8, 6], [6, 2]];
  const S = 74,
    ox = 500,
    oy = 150;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "god",
    dark: possessing ? 0.4 : 0
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      opacity: 0.5,
      backgroundImage: 'linear-gradient(rgba(147,167,196,.09) 1px, transparent 1px), linear-gradient(90deg, rgba(147,167,196,.09) 1px, transparent 1px)',
      backgroundSize: S + 'px ' + S + 'px',
      backgroundPosition: ox + 'px ' + oy + 'px'
    }
  }), /*#__PURE__*/React.createElement("svg", {
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%'
    }
  }, cells.map(([cx, cy], i) => /*#__PURE__*/React.createElement("rect", {
    key: i,
    x: ox + cx * S + 3,
    y: oy + cy * S + 3,
    width: S - 6,
    height: S - 6,
    rx: "3",
    fill: "rgba(46,65,96,.5)",
    stroke: "rgba(195,212,234,.3)",
    strokeWidth: "1"
  })), [[8, 4], [2, 5], [9, 6], [3, 7]].map(([cx, cy], i) => /*#__PURE__*/React.createElement("rect", {
    key: 'f' + i,
    x: ox + cx * S + 12,
    y: oy + cy * S + 12,
    width: S - 24,
    height: S - 24,
    rx: "2",
    fill: "rgba(92,114,149,.28)",
    stroke: "var(--fog-500)",
    strokeWidth: "1.5",
    strokeDasharray: "4 3"
  })), /*#__PURE__*/React.createElement("g", {
    transform: 'translate(' + (ox + 9 * S + S / 2) + ',' + (oy + 2 * S + S / 2) + ')'
  }, /*#__PURE__*/React.createElement("circle", {
    r: "26",
    fill: "rgba(255,246,222,.12)"
  }), /*#__PURE__*/React.createElement("rect", {
    x: "-14",
    y: "-14",
    width: "28",
    height: "28",
    rx: "2",
    fill: "rgba(255,246,222,.7)",
    stroke: "var(--exit-300)",
    strokeWidth: "2",
    style: {
      filter: 'drop-shadow(0 0 14px rgba(255,246,222,.8))'
    }
  })), ghost && /*#__PURE__*/React.createElement("rect", {
    x: ox + 8 * S + 3,
    y: oy + 4 * S + 3,
    width: S - 6,
    height: S - 6,
    rx: "3",
    fill: rejected ? 'rgba(217,69,58,.28)' : 'rgba(0,158,115,.28)',
    stroke: rejected ? 'var(--danger-500)' : 'var(--sleeper-3)',
    strokeWidth: "2"
  })), [[4, 5, 1, 'Anna', 90], [6, 4, 2, 'Ben', 200], [3, 6, 4, 'Dev', 315]].map(([cx, cy, i, n, deg]) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      position: 'absolute',
      left: ox + cx * S + S / 2 - 40,
      top: oy + cy * S + S / 2 - 17
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: i,
    name: n,
    facing: deg,
    selected: selected === i
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: ox + 5 * S + 14,
      top: oy + 3 * S + 14
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: 0.35,
    size: 34,
    tone: "fog"
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-trigger",
    size: 15
  }))), ghost && rejected && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: ox + 8 * S - 40,
      top: oy + 5 * S + 6
    }
  }, /*#__PURE__*/React.createElement(RejectionLabel, {
    reason: "Would trap Ben"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--sp-6)',
      top: '50%',
      transform: 'translateY(-50%)',
      display: 'flex',
      flexDirection: 'column-reverse',
      gap: 'var(--sp-2)'
    }
  }, [0, 1, 2, 3].map(l => /*#__PURE__*/React.createElement("div", {
    key: l,
    style: {
      width: 40,
      height: 34,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: 'var(--r-1)',
      border: 'var(--bw-hair) solid ' + (l === layer ? 'var(--exit-500)' : 'var(--line-faint)'),
      background: l === layer ? 'color-mix(in srgb, var(--exit-500) 14%, transparent)' : 'color-mix(in srgb, var(--ink-900) 45%, transparent)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: l === layer ? 'var(--exit-300)' : 'var(--fg-4)'
    }
  }, l))));
}

/* Live play behind it: scrim only, never a frozen blurred frame. */
function TargetSelector({
  onPick,
  onClose
}) {
  return /*#__PURE__*/React.createElement(Scrim, {
    blur: false
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    title: "Who feels it",
    aside: "tab \xB7 click \xB7 0",
    style: {
      width: 560
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-3)'
    }
  }, [[1, 'Anna', 'depth 11'], [2, 'Ben', 'depth 8'], [4, 'Dev', 'depth 6']].map(([i, n, d]) => /*#__PURE__*/React.createElement("button", {
    key: i,
    type: "button",
    onClick: () => onPick && onPick(i),
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: 'var(--sp-4)',
      padding: 'var(--pad-row)',
      minHeight: 'var(--hit-min)',
      cursor: 'pointer',
      textAlign: 'left',
      background: 'color-mix(in srgb, var(--ink-900) 34%, transparent)',
      border: 'var(--bw-hair) solid var(--line)',
      borderRadius: 'var(--r-2)'
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: i,
    name: n
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, d))), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => onPick && onPick('all'),
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      padding: 'var(--pad-row)',
      minHeight: 'var(--hit-min)',
      cursor: 'pointer',
      textAlign: 'left',
      background: 'color-mix(in srgb, var(--exit-500) 12%, transparent)',
      border: 'var(--bw-hair) solid var(--exit-500)',
      borderRadius: 'var(--r-2)',
      color: 'var(--exit-300)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-target",
    size: 22
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)'
    }
  }, "Everyone"), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: 'auto',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "0"))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-5)',
      display: 'flex',
      justifyContent: 'flex-end'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    hotkey: "Esc",
    onClick: onClose
  }, "Cancel"))));
}
function PossessionOverlay({
  onRelease
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "maze",
    dark: 0.25
  }), /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      inset: 0,
      boxShadow: 'inset 0 0 220px 40px rgba(204,121,167,.28), inset 0 0 0 2px rgba(204,121,167,.5)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--hud-margin)',
      transform: 'translateX(-50%)',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      padding: '10px var(--sp-6)',
      borderRadius: 'var(--r-full)',
      background: 'color-mix(in srgb, var(--ink-900) 74%, transparent)',
      border: 'var(--bw-hair) solid var(--sleeper-4)',
      color: '#f0c4dc'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-possess",
    size: 20
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)'
    }
  }, "Possessing a Shade in Anna's dream \u2014 P to let go"))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: '50%',
      transform: 'translate(-50%,-50%)'
    }
  }, /*#__PURE__*/React.createElement(Crosshair, {
    state: "mob",
    size: 48
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom left"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)'
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: 0.7,
    size: 64,
    tone: "danger",
    label: "9s"
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-possess",
    size: 22
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '400 var(--fs-body)/1.4 var(--font-body)',
      color: 'var(--fg-2)',
      textShadow: '0 1px 10px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)'
    }
  }, "16"), " budget", /*#__PURE__*/React.createElement("br", null), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--danger-300)',
      fontSize: 'var(--fs-micro)'
    }
  }, "Building paused"))))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    hotkey: "P",
    onClick: onRelease
  }, "Let go")));
}
function NightmareView({
  mode = 'idle',
  onMode
}) {
  const [pack, setPack] = React.useState('Core');
  const [cat, setCat] = React.useState('cat-connector');
  const [pick, setPick] = React.useState('Straight');
  const [target, setTarget] = React.useState(false);
  const budget = mode === 'placing' ? 3 : 12;
  const shown = (window.CUBES || []).filter(c => cat === 'all' || c.category === cat);
  if (mode === 'possession') return /*#__PURE__*/React.createElement(PossessionOverlay, {
    onRelease: () => onMode && onMode('idle')
  });
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(GodView, {
    ghost: mode === 'placing',
    rejected: mode === 'placing',
    selected: target ? 1 : null
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 0,
      top: 0,
      bottom: 0,
      width: 340,
      display: 'flex'
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    edge: "top",
    pad: "var(--sp-5)",
    style: {
      flex: 1,
      borderLeft: 0,
      borderTop: 0,
      borderBottom: 0,
      borderRadius: 0
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Palette"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-2)',
      marginBottom: 'var(--sp-4)'
    }
  }, NV_PACKS.map(p => /*#__PURE__*/React.createElement("button", {
    key: p,
    type: "button",
    onClick: () => setPack(p),
    style: {
      flex: 1,
      padding: '8px 0',
      cursor: 'pointer',
      borderRadius: 'var(--r-1)',
      background: pack === p ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (pack === p ? 'var(--line-strong)' : 'var(--line-faint)'),
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: pack === p ? 'var(--fg-1)' : 'var(--fg-3)'
    }
  }, p))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-2)',
      flexWrap: 'wrap',
      marginBottom: 'var(--sp-4)'
    }
  }, NV_CATS.map(([c, label]) => /*#__PURE__*/React.createElement("button", {
    key: c,
    type: "button",
    title: label,
    onClick: () => setCat(c),
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 5,
      padding: '5px 8px',
      cursor: 'pointer',
      borderRadius: 'var(--r-1)',
      background: cat === c ? 'color-mix(in srgb, var(--exit-500) 12%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (cat === c ? 'var(--exit-500)' : 'var(--line-faint)'),
      color: cat === c ? 'var(--exit-300)' : 'var(--fg-3)',
      font: '400 11px/1 var(--font-body)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: c,
    size: 16
  }), label))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 'var(--sp-3)',
      overflow: 'auto'
    }
  }, shown.slice(0, 9).map((c, i) => /*#__PURE__*/React.createElement(PaletteTile, {
    key: c.name,
    name: c.name,
    cost: c.cost,
    mask: c.mask,
    category: c.category,
    hotkey: i + 1,
    budget: budget,
    selected: pick === c.name,
    onClick: () => setPick(c.name)
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between'
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, null, "Budget"), /*#__PURE__*/React.createElement(BudgetMeter, {
    budget: budget
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 0,
      top: 0,
      bottom: 0,
      width: 320,
      display: 'flex'
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    pad: "var(--sp-5)",
    style: {
      flex: 1,
      borderRight: 0,
      borderTop: 0,
      borderBottom: 0,
      borderRadius: 0
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-4)'
    }
  }, "Sleepers"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-3)'
    }
  }, [[1, 'Anna', 11, 82, 3], [2, 'Ben', 8, 54, 2], [4, 'Dev', 6, 100, 3]].map(([i, n, d, hp, lives]) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      padding: 'var(--pad-row)',
      borderRadius: 'var(--r-2)',
      border: 'var(--bw-hair) solid var(--line)',
      background: 'color-mix(in srgb, var(--ink-900) 32%, transparent)'
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: i,
    name: n
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-3)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "depth ", d), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      height: 3,
      borderRadius: 2,
      background: 'var(--line-faint)',
      position: 'relative'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'absolute',
      inset: '0 auto 0 0',
      width: hp + '%',
      background: hp <= 30 ? 'var(--danger-300)' : 'var(--fog-300)',
      borderRadius: 2
    }
  })), /*#__PURE__*/React.createElement(MoonLives, {
    lives: lives,
    max: 3,
    size: 13
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--pad-row)',
      borderRadius: 'var(--r-2)',
      border: '1px dashed var(--line-faint)'
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: 3,
    name: "Cara",
    status: "consumed"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(MicroLabel, {
    style: {
      marginBottom: 'var(--sp-3)'
    }
  }, "This dream"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      font: '400 var(--fs-body)/1.8 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "Cubes placed"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)'
    }
  }, "34")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      font: '400 var(--fs-body)/1.8 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "Doors hardened"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)'
    }
  }, "3")))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-5)',
      transform: 'translateX(-50%)',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 'var(--sp-3)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 148,
    total: 300,
    size: 180,
    phase: "The Sleepers are running"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 340,
      top: 'var(--sp-6)',
      marginRight: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: mode === 'placing' ? [{
      text: 'Not enough budget (3 / 4)',
      tone: 'danger'
    }] : [{
      text: 'Ben found the exit',
      icon: 'door-exit',
      tone: 'exit'
    }, {
      text: 'Cara was consumed',
      tone: 'danger'
    }]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 340,
      right: 320,
      bottom: 0,
      display: 'flex',
      justifyContent: 'center'
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    edge: "bottom",
    pad: "var(--sp-4) var(--sp-6)",
    style: {
      display: 'flex',
      flexDirection: 'row',
      alignItems: 'center',
      gap: 'var(--sp-6)',
      borderBottom: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)'
    }
  }, [['power-dark', 'Dark', 'Q', 0], ['power-fog', 'Fog', 'W', 0.62], ['power-molasses', 'Molasses', 'E', 0.2]].map(([ic, label, key, prog]) => /*#__PURE__*/React.createElement("div", {
    key: label,
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: prog,
    size: 58,
    label: prog > 0 ? String(Math.round(prog * 30)) : key
  }, /*#__PURE__*/React.createElement(Icon, {
    name: ic,
    size: 22
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: prog > 0 ? 'var(--fg-4)' : 'var(--fg-2)'
    }
  }, label), /*#__PURE__*/React.createElement(CostBadge, {
    cost: 2,
    budget: budget,
    size: "sm"
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1,
      alignSelf: 'stretch',
      background: 'var(--line-faint)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)'
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => setTarget(true),
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 6,
      background: 'none',
      border: 0,
      cursor: 'pointer',
      padding: 0
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: 0,
    size: 58,
    tone: "fog"
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-target",
    size: 22
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, "Who feels it"), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 11px/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "Tab \xB7 0")), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => onMode && onMode('possession'),
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 6,
      background: 'none',
      border: 0,
      cursor: 'pointer',
      padding: 0
    }
  }, /*#__PURE__*/React.createElement(CooldownRing, {
    progress: 0,
    size: 58
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-possess",
    size: 22
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, "Possess"), /*#__PURE__*/React.createElement(CostBadge, {
    cost: 3,
    budget: budget,
    size: "sm"
  }))))), target && /*#__PURE__*/React.createElement(TargetSelector, {
    onPick: () => setTarget(false),
    onClose: () => setTarget(false)
  }));
}
Object.assign(window, {
  NightmareView,
  GodView,
  TargetSelector,
  PossessionOverlay,
  BudgetMeter
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/NightmareView.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/OptionsScreen.jsx
try { (() => {
const {
  MistPanel,
  Button,
  Toggle,
  Slider,
  Icon,
  SleeperMarker
} = window.DesignSystem_427bea;
const TABS = ['Game', 'Video', 'Audio', 'Controls', 'Accessibility'];
function OptionsScreen({
  onBack
}) {
  const [tab, setTab] = React.useState('Accessibility');
  const [shapes, setShapes] = React.useState(false);
  const [contrast, setContrast] = React.useState(true);
  const [shake, setShake] = React.useState(30);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--ink-800)'
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "void"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      padding: 'var(--sp-8)',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'baseline',
      justifyContent: 'space-between'
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      font: '300 var(--fs-title)/1 var(--font-display)',
      letterSpacing: '0.1em',
      color: 'var(--fg-1)'
    }
  }, "Options"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    hotkey: "Esc",
    onClick: onBack
  }, "Back")), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: 'flex',
      gap: 'var(--sp-6)',
      minHeight: 0
    }
  }, /*#__PURE__*/React.createElement("nav", {
    style: {
      flex: '0 0 240px',
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-2)'
    }
  }, TABS.map(t => /*#__PURE__*/React.createElement("button", {
    key: t,
    type: "button",
    onClick: () => setTab(t),
    style: {
      textAlign: 'left',
      padding: 'var(--sp-4) var(--sp-5)',
      minHeight: 'var(--hit-min)',
      cursor: 'pointer',
      borderRadius: 'var(--r-2)',
      background: tab === t ? 'color-mix(in srgb, var(--mist-500) 65%, transparent)' : 'transparent',
      border: 'var(--bw-hair) solid ' + (tab === t ? 'var(--line-strong)' : 'transparent'),
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: tab === t ? 'var(--fg-1)' : 'var(--fg-3)'
    }
  }, t))), /*#__PURE__*/React.createElement(MistPanel, {
    title: tab,
    style: {
      flex: 1,
      overflow: 'auto'
    }
  }, tab === 'Accessibility' ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(Toggle, {
    label: "Colour-blind marker shapes",
    hint: "Each Sleeper also gets their own outline shape",
    checked: shapes,
    onChange: setShapes
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-5)',
      padding: 'var(--sp-5) 0 var(--sp-6)'
    }
  }, [1, 2, 3, 4].map(i => /*#__PURE__*/React.createElement(SleeperMarker, {
    key: i,
    index: i,
    name: ['Anna', 'Ben', 'Cara', 'Dev'][i - 1],
    shape: shapes
  }))), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(Toggle, {
    label: "High-contrast doors",
    hint: "A faint hatch on fog, rays on exits",
    checked: contrast,
    onChange: setContrast
  }), /*#__PURE__*/React.createElement(Toggle, {
    label: "Larger HUD text",
    hint: "Everything one step up the scale",
    checked: false,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Toggle, {
    label: "Reduce motion",
    hint: "Pulses hold still; nothing else changes",
    checked: false,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(Slider, {
    label: "Screen shake",
    value: shake,
    min: 0,
    max: 100,
    unit: "%",
    onChange: setShake
  }), /*#__PURE__*/React.createElement(Slider, {
    label: "Subtitle size",
    value: 16,
    min: 12,
    max: 28,
    unit: "px",
    onChange: () => {}
  })) : tab === 'Game' ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(Toggle, {
    label: "Hint cards",
    hint: "Shown for the first three rounds",
    checked: true,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Toggle, {
    label: "Show the build string on Results",
    checked: true,
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Divider, null), /*#__PURE__*/React.createElement(Slider, {
    label: "Field of view",
    value: 95,
    min: 70,
    max: 110,
    unit: "\xB0",
    onChange: () => {}
  }), /*#__PURE__*/React.createElement(Slider, {
    label: "Mouse sensitivity",
    value: 42,
    min: 1,
    max: 100,
    onChange: () => {}
  })) : /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 'var(--sp-8) 0',
      textAlign: 'center',
      font: '400 var(--fs-body)/1.5 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, tab, " settings follow the same two controls \u2014 Toggle and Slider \u2014 in the same order as the docs list them.")))));
}
Object.assign(window, {
  OptionsScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/OptionsScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/PauseScreen.jsx
try { (() => {
const {
  MistPanel,
  Button,
  Icon
} = window.DesignSystem_427bea;
function PauseScreen({
  under = 'hud',
  onResume,
  onOptions,
  onLobby
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: under === 'god' ? 'god' : 'maze'
  }), /*#__PURE__*/React.createElement(Scrim, null, /*#__PURE__*/React.createElement(MistPanel, {
    pad: "var(--sp-8)",
    style: {
      width: 460,
      borderRadius: 'var(--r-3)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: 'center',
      marginBottom: 'var(--sp-7)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '300 var(--fs-title)/1 var(--font-display)',
      letterSpacing: '0.14em',
      color: 'var(--fg-1)'
    }
  }, "Paused"), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-3)',
      font: '400 var(--fs-body)/1.4 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "The dream keeps going \u2014 only your screen is still.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    full: true,
    onClick: onResume,
    hotkey: "Esc"
  }, "Back to the dream"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    full: true,
    onClick: onOptions
  }, "Options"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    full: true,
    onClick: onLobby
  }, "Leave"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    full: true
  }, "Quit to desktop")), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 'var(--sp-6) 0 0',
      font: '400 var(--fs-micro)/1.5 var(--font-body)',
      color: 'var(--fg-4)',
      textAlign: 'center'
    }
  }, "Leaving as the Nightmare ends the round: \u201CThe dream will collapse for all Sleepers.\u201D"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'none'
    }
  }))));
}
Object.assign(window, {
  PauseScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/PauseScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/ResultsScreen.jsx
try { (() => {
const {
  MistPanel,
  Button,
  Icon,
  SleeperMarker,
  ScoreboardRow
} = window.DesignSystem_427bea;

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

const RS_DAWN = 300; /* 5:00, the lobby default */
const RS_ROUND_NO = 3;
const rsSecs = t => {
  const p = t.split(':').map(Number);
  return p[0] * 60 + p[1];
};
const rsWakeScore = t => 100 + (RS_DAWN - rsSecs(t));
const rsClock = s => Math.floor(s / 60) + ':' + String(s % 60).padStart(2, '0');

/* This round's Sleepers, in outcome order: woke first, then consumed. */
const RS_SLEEPERS = [{
  index: 2,
  name: 'Ben',
  wokeAt: '4:12',
  depth: 13,
  before: 612,
  asNightmare: 1,
  woke: 2,
  consumed: 0
}, {
  index: 4,
  name: 'Dev',
  wokeAt: '4:48',
  depth: 11,
  before: 196,
  asNightmare: 0,
  woke: 2,
  consumed: 1
}, {
  index: 3,
  name: 'Cara',
  consumedAt: '2:40',
  depth: 6,
  before: 205,
  asNightmare: 0,
  woke: 1,
  consumed: 2
}, {
  index: 1,
  name: 'Anna',
  byDawn: true,
  depth: 9,
  before: 245,
  asNightmare: 1,
  woke: 1,
  consumed: 1
}];
const RS_NIGHTMARE = {
  name: 'Mara',
  cubes: 34,
  deepest: 14,
  before: 342,
  asNightmare: 1,
  woke: 1,
  consumed: 1
};

/* Derived — never hand-written. */
const RS_consumedThisRound = RS_SLEEPERS.filter(s => !s.wokeAt).length;
const RS_scored = RS_SLEEPERS.map(s => ({
  ...s,
  delta: s.wokeAt ? rsWakeScore(s.wokeAt) : 0
}));
const RS_nightmare = {
  ...RS_NIGHTMARE,
  delta: 100 * RS_consumedThisRound
};
const RS_LEADERBOARD = [...RS_scored, RS_nightmare].map(p => ({
  ...p,
  total: p.before + p.delta
})).sort((a, b) => b.total - a.total);

/* UI.md §9: the title is exactly one of three strings. */
const RS_OUTCOME = RS_SLEEPERS.some(s => s.byDawn) ? {
  line: 'Dawn.',
  tone: 'var(--exit-300)',
  icon: 'door-exit'
} : RS_consumedThisRound === RS_SLEEPERS.length ? {
  line: 'Consumed',
  tone: 'var(--danger-300)',
  icon: 'role-nightmare'
} : {
  line: 'Everyone woke up',
  tone: 'var(--exit-300)',
  icon: 'door-exit'
};
const RS_fastestWake = RS_scored.filter(s => s.wokeAt).sort((a, b) => rsSecs(a.wokeAt) - rsSecs(b.wokeAt))[0];
const RS_BADGES = [['fastest wake', RS_fastestWake.name + ' · ' + RS_fastestWake.wokeAt], ['longest survived', RS_SLEEPERS.some(s => s.byDawn) ? RS_SLEEPERS.find(s => s.byDawn).name + ' · to dawn' : RS_fastestWake.name], ['most cubes built', RS_nightmare.name + ' · ' + RS_nightmare.cubes]];

/* One card per Sleeper: the story of their round in three lines. */
function PlayerCard({
  p,
  compact
}) {
  const woke = !!p.wokeAt;
  const outcome = woke ? 'Woke at ' + p.wokeAt : p.byDawn ? 'Consumed by dawn' : 'Consumed at ' + p.consumedAt;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0,
      padding: compact ? 'var(--sp-4)' : 'var(--sp-5)',
      borderRadius: 'var(--r-3)',
      background: 'color-mix(in srgb, var(--ink-900) 40%, transparent)',
      border: 'var(--bw-hair) solid ' + (woke ? 'color-mix(in srgb, var(--exit-500) 55%, transparent)' : 'var(--line)')
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: p.index,
    name: p.name,
    scale: "lg",
    size: compact ? 38 : 46
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: compact ? 'var(--sp-3)' : 'var(--sp-4)',
      display: 'flex',
      alignItems: 'baseline',
      gap: 'var(--sp-3)',
      flexWrap: 'wrap'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 ' + (compact ? 'var(--fs-heading)' : 'var(--fs-title)') + '/1.1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: woke ? 'var(--exit-300)' : 'var(--fg-3)'
    }
  }, outcome), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-heading)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: p.delta > 0 ? 'var(--exit-500)' : 'var(--fg-4)'
    }
  }, p.delta > 0 ? '+' + p.delta : '0')), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 6,
      font: '400 var(--fs-heading)/1.3 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-3)'
    }
  }, "deepest door ", p.depth));
}
function ResultsScreen({
  compact,
  onLobby
}) {
  const pad = compact ? 'var(--sp-6)' : 'var(--sp-8)';
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'var(--ink-800)'
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "void"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      padding: pad,
      gap: compact ? 'var(--sp-5)' : 'var(--sp-7)'
    }
  }, /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'flex-end',
      justifyContent: 'space-between',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-5)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: RS_OUTCOME.tone
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: RS_OUTCOME.icon,
    size: compact ? 44 : 60,
    strokeWidth: 1.2
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(MicroLabel, null, "Round ", RS_ROUND_NO, " \xB7 dawn at ", rsClock(RS_DAWN)), /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: '8px 0 0',
      font: '300 ' + (compact ? '46px' : 'var(--fs-display)') + '/1 var(--font-display)',
      color: RS_OUTCOME.tone
    }
  }, RS_OUTCOME.line))), /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: 'right'
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, null, "The Nightmare"), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 8,
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      justifyContent: 'flex-end'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 ' + (compact ? 'var(--fs-title)' : '46px') + '/1 var(--font-display)',
      color: 'var(--fg-1)'
    }
  }, RS_nightmare.name), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '500 var(--fs-title)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--exit-500)'
    }
  }, "+", RS_nightmare.delta)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 8,
      font: '400 var(--fs-heading)/1.35 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-2)'
    }
  }, RS_consumedThisRound, " Sleepers consumed \xB7 ", RS_nightmare.cubes, " cubes placed \xB7 deepest ", RS_nightmare.deepest))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: compact ? 'var(--sp-4)' : 'var(--sp-5)'
    }
  }, RS_scored.map(p => /*#__PURE__*/React.createElement(PlayerCard, {
    key: p.index,
    p: p,
    compact: compact
  }))), /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    pad: compact ? 'var(--sp-4)' : 'var(--sp-5)',
    style: {
      flex: 1,
      minHeight: 0
    }
  }, /*#__PURE__*/React.createElement(ScoreboardRow, {
    header: true,
    rank: "#",
    name: "Player",
    rounds: "As Nightmare",
    woke: "Woke",
    consumed: "Consumed",
    score: "Score"
  }), RS_LEADERBOARD.map((p, i) => /*#__PURE__*/React.createElement(ScoreboardRow, {
    key: p.name,
    rank: i + 1,
    index: p.index,
    name: p.name,
    rounds: p.asNightmare,
    woke: p.woke,
    consumed: p.consumed,
    score: p.total,
    delta: p.delta > 0 ? p.delta : null,
    highlight: p.name === RS_nightmare.name
  }))), /*#__PURE__*/React.createElement("footer", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-6)'
    }
  }, RS_BADGES.map(b => /*#__PURE__*/React.createElement("div", {
    key: b[0]
  }, /*#__PURE__*/React.createElement(MicroLabel, null, b[0]), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 5,
      font: '400 var(--fs-heading)/1 var(--font-body)',
      color: 'var(--fg-2)'
    }
  }, b[1])))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "auto in 10 s"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: compact ? 'md' : 'lg'
  }, "Save replay"), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: compact ? 'md' : 'lg',
    onClick: onLobby
  }, "Back to lobby")))));
}
Object.assign(window, {
  ResultsScreen,
  PlayerCard
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/ResultsScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/RoundStartScreen.jsx
try { (() => {
const {
  MistPanel,
  Icon,
  TimerArc,
  SleeperMarker
} = window.DesignSystem_427bea;
function RoundStartScreen({
  stage = 'reveal'
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "bedroom",
    dark: 0.35
  }), /*#__PURE__*/React.createElement(Scrim, null, stage === 'count' ? /*#__PURE__*/React.createElement("div", {
    style: {
      font: '300 220px/1 var(--font-display)',
      fontVariantNumeric: 'lining-nums tabular-nums',
      fontFeatureSettings: 'var(--numeric)',
      color: 'var(--fg-1)',
      textShadow: '0 0 100px rgba(195,212,234,.35)'
    }
  }, "2") : /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 'var(--sp-8)'
    }
  }, /*#__PURE__*/React.createElement(MistPanel, {
    pad: "var(--sp-8)",
    style: {
      width: 620,
      alignItems: 'center',
      textAlign: 'center',
      borderRadius: 'var(--r-3)'
    }
  }, /*#__PURE__*/React.createElement(MicroLabel, null, "Round 3"), /*#__PURE__*/React.createElement("div", {
    style: {
      margin: 'var(--sp-6) 0 var(--sp-5)',
      color: 'var(--exit-300)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "role-nightmare",
    size: 72,
    strokeWidth: 1.1
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '300 var(--fs-title)/1.2 var(--font-display)',
      color: 'var(--fg-2)'
    }
  }, "Tonight's Nightmare is\u2026"), /*#__PURE__*/React.createElement("div", {
    style: {
      font: '600 var(--fs-display)/1.1 var(--font-display)',
      color: 'var(--exit-300)',
      textShadow: '0 0 60px rgba(255,227,163,.35)'
    }
  }, "Anna"), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-6)',
      font: '400 var(--fs-body)/1.5 var(--font-body)',
      color: 'var(--fg-3)',
      maxWidth: 400
    }
  }, "You are a Sleeper. Find the deepest fog door before dawn.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-7)'
    }
  }, PLAYERS.slice(1).map(p => /*#__PURE__*/React.createElement(SleeperMarker, {
    key: p.index,
    index: p.index,
    name: p.name
  }))), /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 300,
    total: 300,
    size: 190,
    phase: "The Sleepers stir in 0:30",
    urgent: false
  }))));
}
Object.assign(window, {
  RoundStartScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/RoundStartScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/Shared.jsx
try { (() => {
const {
  useState,
  useEffect,
  useRef
} = React;
const {
  MistPanel,
  Button,
  Toggle,
  Slider,
  CostBadge,
  Icon,
  TimerArc,
  HealthRing,
  MoonLives,
  Crosshair,
  DamageArc,
  CooldownRing,
  PaletteTile,
  ConnectorNet,
  SleeperMarker,
  RejectionLabel,
  PlayerRow,
  ScoreboardRow,
  Toast,
  ToastStack
} = window.DesignSystem_427bea;

/* A neutral, blurred stand-in for the painterly 3D view. Geometry and gradient
   only — never an illustration (UI.md §15, and the brief's asset constraint). */
function DreamView({
  tone = 'maze',
  dark = 0,
  children,
  style
}) {
  const beds = {
    maze: 'radial-gradient(70% 90% at 30% 30%, #35465f 0%, transparent 60%), radial-gradient(60% 70% at 78% 62%, #2a3b56 0%, transparent 65%), linear-gradient(160deg, #1b2739 0%, #0b111c 100%)',
    god: 'radial-gradient(90% 90% at 50% 42%, #2c3d58 0%, transparent 62%), linear-gradient(180deg, #16202f 0%, #090e18 100%)',
    bedroom: 'radial-gradient(45% 60% at 50% 58%, #3a4a63 0%, transparent 62%), linear-gradient(180deg, #121b2a 0%, #070b12 100%)',
    void: 'linear-gradient(180deg, #10182580 0%, #070b12 100%)'
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      overflow: 'hidden',
      background: beds[tone],
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: '-8%',
      filter: 'blur(46px)',
      opacity: 0.85
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '12%',
      top: '18%',
      width: 420,
      height: 420,
      borderRadius: 24,
      background: 'rgba(120,146,186,.16)',
      transform: 'rotate(18deg)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '52%',
      top: '46%',
      width: 560,
      height: 340,
      borderRadius: 40,
      background: 'rgba(96,124,166,.14)',
      transform: 'rotate(-9deg)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '30%',
      top: '62%',
      width: 300,
      height: 300,
      borderRadius: '50%',
      background: 'rgba(150,176,214,.1)'
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      opacity: 0.35,
      backgroundImage: 'repeating-linear-gradient(0deg, rgba(255,255,255,.02) 0 1px, transparent 1px 3px)'
    }
  }), dark > 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      background: 'rgba(4,6,11,' + dark + ')'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      boxShadow: 'inset 0 0 240px 60px rgba(7,11,18,.85)'
    }
  }), children);
}

/* Full-screen scrim behind a modal surface. */
function Scrim({
  children,
  blur = true
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'var(--surface-scrim)',
      backdropFilter: blur ? 'blur(var(--blur-scrim))' : 'none',
      WebkitBackdropFilter: blur ? 'blur(var(--blur-scrim))' : 'none'
    }
  }, children);
}

/* HUD cluster scrim: the "light chrome" answer — a faint radial wash under a
   corner cluster instead of a panel, so the world stays visible. */
function ClusterScrim({
  corner = 'bottom left',
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      padding: 'var(--sp-5)',
      margin: 'calc(var(--sp-5) * -1)',
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      inset: '-24px',
      background: 'radial-gradient(60% 70% at ' + corner + ', rgba(7,11,18,.62) 0%, transparent 72%)',
      pointerEvents: 'none'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative'
    }
  }, children));
}

/* The phase banner used at round start and by the Nightmare. */
function PhaseBanner({
  text,
  sub
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: 'center',
      textShadow: '0 2px 22px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      font: '300 var(--fs-title)/1.1 var(--font-display)',
      color: 'var(--fg-1)',
      letterSpacing: '0.03em'
    }
  }, text), sub && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 6,
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, sub));
}
const Divider = ({
  style
}) => /*#__PURE__*/React.createElement("div", {
  style: {
    height: 1,
    background: 'var(--line-faint)',
    margin: 'var(--sp-5) 0',
    ...style
  }
});
const MicroLabel = ({
  children,
  style
}) => /*#__PURE__*/React.createElement("div", {
  style: {
    font: '500 var(--fs-micro)/1 var(--font-body)',
    letterSpacing: 'var(--ls-caps)',
    textTransform: 'uppercase',
    color: 'var(--fg-3)',
    ...style
  }
}, children);
const PLAYERS = [{
  name: 'Anna',
  index: 1
}, {
  name: 'Ben',
  index: 2
}, {
  name: 'Cara',
  index: 3
}, {
  name: 'Dev',
  index: 4
}];

/* ---- Real values. SPEC §8 (cubes), §9 (Sleepers), §10 (powers), §12 (scoring) ---- */

/* Connector masks in net order: top, west, north, east, south, bottom. */
const CUBES = [{
  name: 'Straight',
  cost: 1,
  mask: '010100',
  category: 'cat-connector'
}, {
  name: 'Corner',
  cost: 1,
  mask: '011000',
  category: 'cat-connector'
}, {
  name: 'T',
  cost: 1,
  mask: '011100',
  category: 'cat-connector'
}, {
  name: 'Cross',
  cost: 1,
  mask: '011110',
  category: 'cat-connector'
}, {
  name: 'Drop',
  cost: 1,
  mask: '010001',
  category: 'cat-vertical',
  note: 'One-way down — a funnel'
}, {
  name: 'Landing',
  cost: 1,
  mask: '010101',
  category: 'cat-vertical'
}, {
  name: 'Ladder shaft',
  cost: 2,
  mask: '100001',
  category: 'cat-vertical',
  climbable: true
}, {
  name: 'Stairwell',
  cost: 2,
  mask: '110000',
  category: 'cat-vertical',
  climbable: true
}, {
  name: 'Spike Pit',
  cost: 2,
  mask: '010100',
  category: 'cat-chicane'
}, {
  name: 'Gap',
  cost: 2,
  mask: '010100',
  category: 'cat-chicane'
}, {
  name: 'Vent',
  cost: 2,
  mask: '010100',
  category: 'cat-chicane'
}, {
  name: 'Little maze',
  cost: 3,
  mask: '010100',
  category: 'cat-chicane'
}, {
  name: 'Moving platforms',
  cost: 3,
  mask: '010100',
  category: 'cat-chicane'
}, {
  name: 'Trapdoor',
  cost: 3,
  mask: '010101',
  category: 'cat-chicane',
  weak: 'latch',
  trigger: 'drop it now'
}, {
  name: 'Timed spikes',
  cost: 3,
  mask: '010100',
  category: 'cat-chicane',
  weak: 'control box',
  trigger: 'fire now, off-rhythm'
}, {
  name: 'Pendulum',
  cost: 4,
  mask: '010100',
  category: 'cat-chicane',
  weak: 'chain',
  trigger: 'hold'
}, {
  name: 'Crusher',
  cost: 4,
  mask: '010100',
  category: 'cat-chicane',
  weak: 'hydraulic line',
  trigger: 'slam now'
}, {
  name: 'Turret',
  cost: 4,
  mask: '010100',
  category: 'cat-chicane',
  weak: 'core',
  trigger: 'possess to aim by hand'
}, {
  name: 'Nest',
  cost: 4,
  mask: '010100',
  category: 'cat-mob',
  weak: 'the nest',
  trigger: 'spawn a wave now'
}];

/* SPEC §10: one 30 s cooldown per power per dream. */
const EFFECTS = [{
  key: 'Q',
  icon: 'power-dark',
  name: 'Dark',
  cost: 3,
  duration: 8,
  cooldown: 30
}, {
  key: 'W',
  icon: 'power-fog',
  name: 'Fog',
  cost: 2,
  duration: 10,
  cooldown: 30
}, {
  key: 'E',
  icon: 'power-molasses',
  name: 'Molasses',
  cost: 4,
  duration: 6,
  cooldown: 30
}];
const RULES = {
  headStart: 30,
  dawn: 300,
  lives: 1,
  hp: 100,
  budgetStart: 12,
  trickle: '1 per 4 s',
  jamShots: 6,
  triggerCooldown: 6
};
Object.assign(window, {
  DreamView,
  Scrim,
  ClusterScrim,
  PhaseBanner,
  Divider,
  MicroLabel,
  PLAYERS,
  CUBES,
  EFFECTS,
  RULES
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/Shared.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/SleeperHud.jsx
try { (() => {
const {
  Icon,
  TimerArc,
  HealthRing,
  MoonLives,
  Crosshair,
  DamageArc,
  ToastStack,
  MistPanel,
  ScoreboardRow
} = window.DesignSystem_427bea;
const HUD_STATES = {
  running: {
    seconds: 192,
    hp: 82,
    lives: 1,
    dark: 0,
    cross: 'idle',
    toasts: [{
      text: 'A door hardened',
      icon: 'door-solid'
    }, {
      text: 'The exit moved',
      icon: 'door-exit',
      tone: 'exit'
    }]
  },
  dark: {
    seconds: 148,
    hp: 64,
    lives: 1,
    dark: 0.72,
    cross: 'mob',
    effect: {
      icon: 'power-dark',
      text: 'Dark'
    },
    toasts: [{
      text: 'Dark',
      icon: 'power-dark',
      tone: 'effect'
    }, {
      text: 'Cara was consumed',
      tone: 'danger'
    }]
  },
  last30: {
    seconds: 27,
    hp: 38,
    lives: 1,
    dark: 0.15,
    cross: 'weak-point',
    progress: 0.55,
    damage: 'left',
    toasts: [{
      text: 'Dawn in 0:30',
      icon: 'door-exit',
      tone: 'exit'
    }, {
      text: 'You lost a life — 1 moon left',
      tone: 'danger'
    }]
  }
};
function EffectChip({
  icon,
  text
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '8px var(--sp-5)',
      borderRadius: 'var(--r-full)',
      background: 'color-mix(in srgb, var(--ink-900) 70%, transparent)',
      border: 'var(--bw-hair) solid var(--fog-500)',
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fog-300)',
      letterSpacing: '0.04em'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: icon,
    size: 18
  }), text);
}
function DepthReadout({
  depth = 7,
  best = 11
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      textShadow: '0 1px 10px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--fg-3)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "depth",
    size: 20
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: 'var(--fg-1)'
    }
  }, "Depth ", depth), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "deepest ", best));
}
function SleeperHud({
  state = 'running',
  showTab
}) {
  const s = HUD_STATES[state];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "maze",
    dark: s.dark
  }), s.damage && /*#__PURE__*/React.createElement(DamageArc, {
    from: s.damage,
    intensity: 0.85
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: '50%',
      transform: 'translate(-50%,-50%)'
    }
  }, /*#__PURE__*/React.createElement(Crosshair, {
    state: s.cross,
    progress: s.progress || 0,
    size: s.cross === 'weak-point' ? 64 : 48
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-6)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: s.seconds,
    total: 300,
    size: 190,
    phase: s.seconds <= 30 ? 'Dawn in 0:' + String(s.seconds).padStart(2, '0') : 'Dawn'
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      top: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: s.toasts
  })), s.effect && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      top: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(EffectChip, s.effect)), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom left"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(HealthRing, {
    hp: s.hp,
    size: 84
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(MoonLives, {
    lives: s.lives,
    max: 1,
    size: 26
  }), /*#__PURE__*/React.createElement(DepthReadout, {
    depth: state === 'last30' ? 11 : 7,
    best: 11
  }))))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom right"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-6)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)',
      textShadow: '0 1px 8px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "LMB"), " jam"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "Shift"), " sprint"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "Tab"), " who is left")))), showTab && /*#__PURE__*/React.createElement(Scrim, {
    blur: false
  }, /*#__PURE__*/React.createElement(MistPanel, {
    tone: "chrome",
    title: "Who is left",
    aside: "hold tab",
    style: {
      width: 720
    }
  }, /*#__PURE__*/React.createElement(ScoreboardRow, {
    header: true,
    rank: "#",
    name: "Player",
    score: "Depth"
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 1,
    index: 1,
    name: "Anna",
    status: "in the dream",
    score: 11
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 2,
    index: 2,
    name: "Ben",
    status: "in the dream",
    score: 8
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 3,
    index: 3,
    name: "Cara",
    status: "consumed",
    score: 6
  }), /*#__PURE__*/React.createElement(ScoreboardRow, {
    rank: 4,
    index: 4,
    name: "Dev",
    status: "awake",
    score: 13
  }))));
}
Object.assign(window, {
  SleeperHud,
  EffectChip,
  DepthReadout
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/SleeperHud.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/SleeperMockups.jsx
try { (() => {
const {
  MistPanel,
  Icon,
  TimerArc,
  HealthRing,
  MoonLives,
  Crosshair,
  DamageArc,
  ToastStack
} = window.DesignSystem_427bea;

/* A doorway in the world, drawn as geometry: the mist sheet is the fill. */
function Doorway({
  state = 'fog',
  w = 300,
  h = 430,
  x,
  y,
  label,
  countdown
}) {
  const skin = {
    fog: {
      fill: 'radial-gradient(60% 70% at 50% 45%, rgba(92,114,149,.5) 0%, rgba(35,48,70,.72) 100%)',
      edge: 'var(--fog-700)',
      glow: 'none'
    },
    exit: {
      fill: 'radial-gradient(52% 58% at 50% 46%, rgba(255,251,240,.88) 0%, rgba(255,238,198,.42) 46%, rgba(255,227,163,.14) 100%)',
      edge: 'rgba(255,246,222,.75)',
      glow: '0 0 120px 34px rgba(255,246,222,.34)'
    },
    solid: {
      fill: 'repeating-linear-gradient(45deg, rgba(74,85,104,.9) 0 6px, rgba(58,68,84,.9) 6px 12px)',
      edge: 'var(--door-solid)',
      glow: 'none'
    },
    attached: {
      fill: 'linear-gradient(180deg, rgba(10,15,24,.92) 0%, rgba(18,26,40,.8) 100%)',
      edge: 'rgba(195,212,234,.34)',
      glow: 'none'
    }
  }[state];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: x,
      top: y,
      width: w,
      height: h
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      borderRadius: '4px 4px 0 0',
      border: '2px solid ' + skin.edge,
      background: skin.fill,
      boxShadow: skin.glow
    }
  }), countdown && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '300 96px/1 var(--font-display)',
      fontVariantNumeric: 'lining-nums tabular-nums',
      fontFeatureSettings: 'var(--numeric)',
      color: 'var(--fg-1)',
      textShadow: '0 0 40px rgba(7,11,18,.9)'
    }
  }, countdown), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      letterSpacing: '0.1em',
      color: 'var(--fg-2)'
    }
  }, label)), !countdown && label && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      bottom: -34,
      transform: 'translateX(-50%)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      letterSpacing: 'var(--ls-caps)',
      textTransform: 'uppercase',
      color: 'var(--fg-3)',
      whiteSpace: 'nowrap'
    }
  }, label));
}

/* Round-start hint cards — first three rounds only (UI.md §5). */
function HintCards() {
  const cards = [['door-fog', 'grey mist', 'closed for now', 'var(--fog-500)'], ['door-exit', 'white light', 'the way out', 'var(--exit-300)'], ['door-solid', 'hardened wall', "you've been here", 'var(--door-solid)']];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-5)'
    }
  }, cards.map(c => /*#__PURE__*/React.createElement(MistPanel, {
    key: c[1],
    pad: "var(--sp-5)",
    style: {
      width: 250,
      alignItems: 'center',
      textAlign: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: c[0],
    size: 44,
    color: c[3],
    strokeWidth: 1.3
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 'var(--sp-4)',
      font: '400 var(--fs-heading)/1.1 var(--font-body)',
      color: 'var(--fg-1)'
    }
  }, c[1]), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 4,
      font: '400 var(--fs-body)/1.3 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, c[2]))));
}

/* Molasses readout: the viscous vignette plus the 70 % icon (UI.md §6). */
function MolassesOverlay({
  secondsLeft
}) {
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      inset: 0,
      pointerEvents: 'none',
      background: 'radial-gradient(70% 60% at 50% 50%, transparent 30%, rgba(60,44,20,.42) 100%)',
      boxShadow: 'inset 0 0 200px 60px rgba(48,36,16,.5)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      top: 'var(--hud-margin)',
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '8px var(--sp-5)',
      borderRadius: 'var(--r-full)',
      background: 'color-mix(in srgb, var(--ink-900) 72%, transparent)',
      border: 'var(--bw-hair) solid var(--fog-500)',
      color: 'var(--fog-300)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "power-molasses",
    size: 20
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums'
    }
  }, "70 %")), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)',
      textShadow: '0 1px 10px rgba(7,11,18,.95)'
    }
  }, secondsLeft, " s left")));
}

/* Dark dims the HUD (UI.md §6) but health and lives carry rules, so the floor is
   0.6 — at 0.32 they measured ~1.7:1 and were effectively erased, which §1.3
   forbids. Only the non-rule key hints go lower. */
function SleeperVitals({
  hp,
  lives,
  depth,
  exit,
  dim
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)',
      opacity: dim ? 0.6 : 1,
      transition: 'opacity var(--dur-base) var(--ease-out)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom left"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(HealthRing, {
    hp: hp,
    size: 84
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement(MoonLives, {
    lives: lives,
    max: window.RULES.lives,
    size: 26
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)',
      textShadow: '0 1px 10px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--fg-3)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "depth",
    size: 20
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      fontVariantNumeric: 'tabular-nums',
      color: depth === exit ? 'var(--exit-500)' : 'var(--fg-1)'
    }
  }, "depth ", depth), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "exit ", exit))))));
}
function SleeperKeys({
  dim
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)',
      opacity: dim ? 0.32 : 1
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom right"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 'var(--sp-6)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-3)',
      textShadow: '0 1px 8px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "LMB"), " Nightlight"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "Ctrl"), " crouch"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("kbd", {
    style: {
      color: 'var(--fg-1)'
    }
  }, "Tab"), " who is left"))));
}

/* ---------------------------------------------------------------- 1 of 3
   The bedroom. The start cube's one door is misted until the head start ends,
   with the countdown projected onto it. Hint cards, first three rounds only. */
function SleeperBedroom() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "bedroom"
  }), /*#__PURE__*/React.createElement(Doorway, {
    state: "fog",
    x: 810,
    y: 290,
    w: 300,
    h: 430,
    countdown: "0:12",
    label: "the door is misted"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-6)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 300,
    total: 300,
    size: 190,
    phase: "The Sleepers stir in 0:12",
    urgent: false
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: '50%',
      transform: 'translate(-50%,-50%)'
    }
  }, /*#__PURE__*/React.createElement(Crosshair, {
    size: 48
  })), /*#__PURE__*/React.createElement(SleeperVitals, {
    hp: 100,
    lives: 1,
    depth: 0,
    exit: 2
  }), /*#__PURE__*/React.createElement(SleeperKeys, null), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      bottom: 'var(--sp-9)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(HintCards, null)));
}

/* ---------------------------------------------------------------- 2 of 3
   Molasses at 70 % while jamming a Trapdoor latch: six shots, about 1.5 s of
   standing still — which is exactly the window the Nightmare bought. */
function SleeperJamming() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "maze"
  }), /*#__PURE__*/React.createElement(Doorway, {
    state: "attached",
    x: 250,
    y: 330,
    w: 230,
    h: 340,
    label: "attached"
  }), /*#__PURE__*/React.createElement(Doorway, {
    state: "solid",
    x: 1440,
    y: 340,
    w: 210,
    h: 320,
    label: "hardened \u2014 you've been here"
  }), /*#__PURE__*/React.createElement(MolassesOverlay, {
    secondsLeft: 4
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-6)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 148,
    total: 300,
    size: 190,
    phase: "The Sleepers are running"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      top: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: [{
      text: "Molasses — don't jump",
      icon: 'power-molasses',
      tone: 'effect'
    }]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: '50%',
      transform: 'translate(-50%,-50%)',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 'var(--sp-6)'
    }
  }, /*#__PURE__*/React.createElement(Crosshair, {
    state: "weak-point",
    progress: 4 / window.RULES.jamShots,
    size: 72
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-2)',
      textShadow: '0 1px 12px rgba(7,11,18,.95)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "weak-point",
    size: 18,
    color: "var(--exit-300)"
  }), "latch \xB7 4 of 6")), /*#__PURE__*/React.createElement(SleeperVitals, {
    hp: 62,
    lives: 1,
    depth: 9,
    exit: 11
  }), /*#__PURE__*/React.createElement(SleeperKeys, null));
}

/* ---------------------------------------------------------------- 3 of 3
   Dark, in the last half minute, with an exit in sight. Dark puts every light
   out and dims the HUD — except the timer — and fog doors still glow. */
function SleeperDark() {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "maze",
    dark: 0.82
  }), /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      left: '50%',
      top: '52%',
      transform: 'translate(-50%,-50%)',
      width: 900,
      height: 640,
      background: 'radial-gradient(50% 50% at 50% 50%, rgba(214,226,246,.16) 0%, transparent 70%)',
      pointerEvents: 'none'
    }
  }), /*#__PURE__*/React.createElement(Doorway, {
    state: "exit",
    x: 835,
    y: 318,
    w: 250,
    h: 370,
    label: "the way out"
  }), /*#__PURE__*/React.createElement(Doorway, {
    state: "fog",
    x: 300,
    y: 356,
    w: 200,
    h: 300,
    label: "closed for now"
  }), /*#__PURE__*/React.createElement(DamageArc, {
    from: "right",
    intensity: 0.7
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-6)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 18,
    total: 300,
    size: 190,
    phase: "Dawn in 0:18"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      top: 'var(--hud-margin)',
      opacity: 0.32
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: [{
      text: 'Dark',
      icon: 'power-dark',
      tone: 'effect'
    }, {
      text: 'The exit moved',
      icon: 'door-exit',
      tone: 'exit'
    }]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: '50%',
      transform: 'translate(-50%,-50%)'
    }
  }, /*#__PURE__*/React.createElement(Crosshair, {
    size: 48
  })), /*#__PURE__*/React.createElement(SleeperVitals, {
    hp: 24,
    lives: 1,
    depth: 11,
    exit: 11,
    dim: true
  }), /*#__PURE__*/React.createElement(SleeperKeys, {
    dim: true
  }));
}
Object.assign(window, {
  SleeperBedroom,
  SleeperJamming,
  SleeperDark,
  Doorway,
  HintCards,
  MolassesOverlay,
  SleeperVitals,
  SleeperKeys
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/SleeperMockups.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/SpectatorScreen.jsx
try { (() => {
const {
  Icon,
  TimerArc,
  HealthRing,
  MoonLives,
  ToastStack,
  SleeperMarker,
  Button
} = window.DesignSystem_427bea;
function SpectatorScreen() {
  const [watching, setWatching] = React.useState(2);
  const alive = [{
    index: 2,
    name: 'Ben'
  }, {
    index: 4,
    name: 'Dev'
  }];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "maze",
    dark: 0.1
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      boxShadow: 'inset 0 0 0 3px color-mix(in srgb, var(--sleeper-' + watching + ') 55%, transparent)',
      pointerEvents: 'none'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 'var(--sp-6)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(TimerArc, {
    seconds: 148,
    total: 300,
    size: 190,
    phase: "Dawn"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      top: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ToastStack, {
    toasts: [{
      text: 'Cara was consumed',
      tone: 'danger'
    }, {
      text: 'The exit moved',
      icon: 'door-exit',
      tone: 'exit'
    }]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      top: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "top left"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      color: 'var(--fg-1)'
    }
  }, "You're awake. Watch the others."), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)',
      color: 'var(--fg-3)'
    }
  }, "watching"), /*#__PURE__*/React.createElement(SleeperMarker, {
    index: watching,
    name: alive.find(a => a.index === watching).name
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      bottom: 'var(--hud-margin)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom left"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-6)',
      opacity: 0.85
    }
  }, /*#__PURE__*/React.createElement(HealthRing, {
    hp: 54,
    size: 72
  }), /*#__PURE__*/React.createElement(MoonLives, {
    lives: 2,
    max: 3,
    size: 24
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-heading)/1 var(--font-body)',
      color: 'var(--fg-2)',
      textShadow: '0 1px 10px rgba(7,11,18,.95)'
    }
  }, "Depth 8")))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      bottom: 'var(--hud-margin)',
      transform: 'translateX(-50%)'
    }
  }, /*#__PURE__*/React.createElement(ClusterScrim, {
    corner: "bottom center"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-4)'
    }
  }, alive.map(a => /*#__PURE__*/React.createElement("button", {
    key: a.index,
    type: "button",
    onClick: () => setWatching(a.index),
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '8px var(--sp-5)',
      minHeight: 'var(--hit-min)',
      cursor: 'pointer',
      borderRadius: 'var(--r-2)',
      background: watching === a.index ? 'color-mix(in srgb, var(--mist-500) 70%, transparent)' : 'color-mix(in srgb, var(--ink-900) 55%, transparent)',
      border: 'var(--bw-hair) solid ' + (watching === a.index ? 'var(--line-strong)' : 'var(--line)')
    }
  }, /*#__PURE__*/React.createElement(SleeperMarker, {
    index: a.index,
    name: a.name,
    size: 28
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 'var(--sp-3)',
      padding: '8px var(--sp-5)',
      minHeight: 'var(--hit-min)',
      borderRadius: 'var(--r-2)',
      background: 'color-mix(in srgb, var(--ink-900) 55%, transparent)',
      border: 'var(--bw-hair) solid var(--line)',
      color: 'var(--fg-2)'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "role-nightmare",
    size: 20
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-body)/1 var(--font-body)'
    }
  }, "God view")), /*#__PURE__*/React.createElement("span", {
    style: {
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-4)',
      marginLeft: 'var(--sp-3)'
    }
  }, "\u2190 \u2192 to change")))));
}
Object.assign(window, {
  SpectatorScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/SpectatorScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/lucid-game/TitleScreen.jsx
try { (() => {
const {
  MistPanel,
  Button,
  Icon
} = window.DesignSystem_427bea;
function TitleScreen({
  onHost,
  onOptions
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0
    }
  }, /*#__PURE__*/React.createElement(DreamView, {
    tone: "bedroom"
  }), /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: 'absolute',
      left: 0,
      right: 0,
      top: 0,
      height: 500,
      overflow: 'hidden',
      pointerEvents: 'none',
      maskImage: 'linear-gradient(180deg, #000 0%, #000 62%, transparent 96%)',
      WebkitMaskImage: 'linear-gradient(180deg, #000 0%, #000 62%, transparent 96%)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: '50%',
      top: 96,
      transform: 'translateX(-50%)',
      color: 'var(--fog-700)',
      opacity: 0.34,
      animation: 'lucid-breathe var(--pulse-door) var(--ease-in-out) infinite'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "door-fog",
    size: 620,
    strokeWidth: 0.3
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 'var(--sp-9)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: 'center'
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      font: '300 128px/1 var(--font-display)',
      letterSpacing: '0.24em',
      paddingLeft: '0.24em',
      color: 'var(--fg-1)',
      textShadow: '0 0 90px rgba(195,212,234,.4)'
    }
  }, "LUCID"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 'var(--sp-5) 0 0',
      font: '300 var(--fs-heading)/1 var(--font-display)',
      letterSpacing: '0.1em',
      color: 'var(--fg-3)'
    }
  }, "One builds the dream. The rest have to wake up.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 'var(--sp-4)',
      width: 340
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    full: true,
    onClick: onHost
  }, "Host a dream"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: "lg",
    full: true
  }, "Join"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: "lg",
    full: true
  }, "Sandbox"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: "lg",
    full: true,
    onClick: onOptions
  }, "Options"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: "lg",
    full: true
  }, "Quit"))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      left: 'var(--hud-margin)',
      bottom: 'var(--sp-6)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "v0.6.0-dev \xB7 MIT \xB7 Unity 6"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      right: 'var(--hud-margin)',
      bottom: 'var(--sp-6)',
      font: '400 var(--fs-micro)/1 var(--font-body)',
      color: 'var(--fg-4)'
    }
  }, "lucid.game"));
}
Object.assign(window, {
  TitleScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/lucid-game/TitleScreen.jsx", error: String((e && e.message) || e) }); }

__ds_ns.Button = __ds_scope.Button;

__ds_ns.CostBadge = __ds_scope.CostBadge;

__ds_ns.ICONS = __ds_scope.ICONS;

__ds_ns.Icon = __ds_scope.Icon;

__ds_ns.MistPanel = __ds_scope.MistPanel;

__ds_ns.Slider = __ds_scope.Slider;

__ds_ns.Toggle = __ds_scope.Toggle;

__ds_ns.Toast = __ds_scope.Toast;

__ds_ns.ToastStack = __ds_scope.ToastStack;

__ds_ns.Crosshair = __ds_scope.Crosshair;

__ds_ns.DamageArc = __ds_scope.DamageArc;

__ds_ns.HealthRing = __ds_scope.HealthRing;

__ds_ns.MoonLives = __ds_scope.MoonLives;

__ds_ns.TimerArc = __ds_scope.TimerArc;

__ds_ns.PlayerRow = __ds_scope.PlayerRow;

__ds_ns.ScoreboardRow = __ds_scope.ScoreboardRow;

__ds_ns.CooldownRing = __ds_scope.CooldownRing;

__ds_ns.ConnectorNet = __ds_scope.ConnectorNet;

__ds_ns.PaletteTile = __ds_scope.PaletteTile;

__ds_ns.REJECTIONS = __ds_scope.REJECTIONS;

__ds_ns.RejectionLabel = __ds_scope.RejectionLabel;

__ds_ns.SLEEPER_COLORS = __ds_scope.SLEEPER_COLORS;

__ds_ns.SleeperMarker = __ds_scope.SleeperMarker;

})();
