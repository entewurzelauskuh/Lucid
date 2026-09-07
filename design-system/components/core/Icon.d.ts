export type IconName =
  | 'door-attached' | 'door-fog' | 'door-exit' | 'door-solid'
  | 'power-dark' | 'power-fog' | 'power-molasses' | 'power-trigger' | 'power-possess' | 'power-target'
  | 'cat-connector' | 'cat-vertical' | 'cat-chicane' | 'cat-mob' | 'cat-gimmick'
  | 'ready' | 'unready' | 'role-nightmare' | 'role-sleeper' | 'moon' | 'crown'
  | 'weak-point' | 'depth';

export interface IconProps {
  /** Which glyph. Mirrors a file in assets/icons/. */
  name: IconName;
  /** Box size in px. The grid is 24; use 16, 20, 24, 32, 48. */
  size?: number;
  /** Stroke weight. 1.5 at 24px; raise to 2 only above 40px. */
  strokeWidth?: number;
  /** Overrides currentColor. */
  color?: string;
  style?: React.CSSProperties;
  title?: string;
}
export declare function Icon(props: IconProps): JSX.Element | null;
