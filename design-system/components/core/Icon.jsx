import React from 'react';

const FRAME = '<path d="M4 21h16"/><path d="M6.5 21V8a5.5 5.5 0 0 1 11 0v13"/>';
const MIST = 'c1.2-.9 2.3.9 3.5 0s2.3-.9 3.5 0';

/* Geometry is duplicated from assets/icons/*.svg so components stay
   self-contained. The files on disk are the export path for Unity. */
export const ICONS = {
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

export function Icon({ name, size = 24, strokeWidth = 1.5, color, style, title, ...rest }) {
  const body = ICONS[name];
  if (!body) return null;
  return (
    <svg viewBox="0 0 24 24" width={size} height={size} fill="none" stroke="currentColor"
      strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" role="img"
      aria-label={title || name} style={{ color: color || 'currentColor', flex: '0 0 auto', display: 'block', ...style }}
      dangerouslySetInnerHTML={{ __html: body }} {...rest} />
  );
}
