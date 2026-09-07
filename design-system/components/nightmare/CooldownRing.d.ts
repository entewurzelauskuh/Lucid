/**
 * A ring that fills as a cooldown returns. Wraps power buttons, trap triggers
 * and the budget trickle point.
 * @startingPoint section="Lucid" subtitle="Powers bar, cooldown rings, cost badges" viewport="700x200"
 */
export interface CooldownRingProps {
  /** 0 = ready (full bright ring), 0.4 = 40% recovered. */
  progress?: number;
  size?: number;
  thickness?: number | string;
  tone?: 'exit' | 'fog' | 'danger';
  /** Seconds remaining, e.g. '18'. Omit when ready. */
  label?: string;
  /** Usually an <Icon />. */
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function CooldownRing(props: CooldownRingProps): JSX.Element;
