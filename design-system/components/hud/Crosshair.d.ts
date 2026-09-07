export interface CrosshairProps {
  /** 'idle' a small dot; 'mob' brightens it; 'weak-point' grows the ring. */
  state?: 'idle' | 'mob' | 'weak-point';
  /** 0..1 of the weak point's HP already taken; the ring drains as it rises. */
  progress?: number;
  size?: number;
  style?: React.CSSProperties;
}
export declare function Crosshair(props: CrosshairProps): JSX.Element;
