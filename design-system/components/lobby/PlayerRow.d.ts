/**
 * One player in the Lobby's left column: avatar, name, host crown, the two role
 * cards, and the ready check.
 * @startingPoint section="Lucid" subtitle="Lobby player rows and scoreboard rows" viewport="700x300"
 */
export interface PlayerRowProps {
  name: string;
  /** Draws the host crown. */
  host?: boolean;
  /** Which role card is lit. Cleared when the lobby reopens (UI.md §4). */
  role?: 'nightmare' | 'sleeper' | null;
  ready?: boolean;
  /** This is the local player: the row is highlighted and its controls live. */
  self?: boolean;
  onRole?: (role: 'nightmare' | 'sleeper') => void;
  onReady?: (next: boolean) => void;
  style?: React.CSSProperties;
}
export declare function PlayerRow(props: PlayerRowProps): JSX.Element;
