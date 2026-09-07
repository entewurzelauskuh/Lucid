export interface DamageArcProps {
  /** Which screen edge the hit came from. */
  from?: 'left' | 'right' | 'top' | 'bottom' | number;
  /** 0..1; fades out over --dur-base. */
  intensity?: number;
  style?: React.CSSProperties;
}
export declare function DamageArc(props: DamageArcProps): JSX.Element;
