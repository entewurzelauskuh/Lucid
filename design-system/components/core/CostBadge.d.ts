export interface CostBadgeProps {
  /** Budget points. */
  cost: number;
  /** Force the unaffordable state. */
  affordable?: boolean;
  /** Current budget; below cost the badge turns danger. */
  budget?: number;
  size?: 'sm' | 'md';
  style?: React.CSSProperties;
}
export declare function CostBadge(props: CostBadgeProps): JSX.Element;
