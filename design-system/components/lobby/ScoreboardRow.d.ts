export interface ScoreboardRowProps {
  /** Position, or a column label when header. */
  rank?: string | number;
  name: string;
  /** 1–4 draws the Sleeper's numbered colour chip. Omit for the Nightmare. */
  index?: 1 | 2 | 3 | 4;
  score?: string | number;
  /** Round delta on the Results screen, e.g. 148. */
  delta?: number;
  /** "rounds as Nightmare" (SPEC §12). */
  rounds?: string | number;
  woke?: string | number;
  consumed?: string | number;
  /** Tab overlay status: 'in the dream' | 'awake' | 'consumed'. */
  status?: string;
  /** Renders as the column header row. */
  header?: boolean;
  highlight?: boolean;
  style?: React.CSSProperties;
}
export declare function ScoreboardRow(props: ScoreboardRowProps): JSX.Element;
