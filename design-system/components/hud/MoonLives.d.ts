export interface MoonLivesProps {
  /** Lives left. The last one glows. */
  lives: number;
  /** Lives at round start (lobby setting, 1–5); spent ones stay as dim outlines. */
  max?: number;
  size?: number;
  style?: React.CSSProperties;
}
export declare function MoonLives(props: MoonLivesProps): JSX.Element;
