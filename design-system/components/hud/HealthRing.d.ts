export interface HealthRingProps {
  /** Current HP. */
  hp: number;
  /** Max HP; 100 by default (SPEC §9). */
  max?: number;
  /** Regen shimmer, 4 s after the last hit. */
  regen?: boolean;
  size?: number;
  /** The number is hover-hold only (UI.md §6) — off by default. */
  showNumber?: boolean;
  style?: React.CSSProperties;
}
export declare function HealthRing(props: HealthRingProps): JSX.Element;
