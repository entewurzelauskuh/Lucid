/**
 * A cube card in the Nightmare's palette: category icon, name, cost, hotkey and
 * a mini cube net showing which of the six faces are connectors.
 * @startingPoint section="Lucid" subtitle="Cube palette tiles with connector nets" viewport="700x260"
 */
export interface PaletteTileProps {
  /** Cube type name, e.g. 'Ladder shaft'. */
  name: string;
  /** Budget points. */
  cost: number;
  /** 1–9 for the visible cards. */
  hotkey?: number | string;
  /**
   * Six characters, '1' = connector, in net order:
   * top, west, north, east, south, bottom. 'Straight' is '010100'.
   */
  mask?: string;
  /** Category icon name. */
  category?: 'cat-connector' | 'cat-vertical' | 'cat-chicane' | 'cat-mob' | 'cat-gimmick';
  selected?: boolean;
  /** Current budget; below cost the tile dims and its badge turns danger. */
  budget?: number;
  disabled?: boolean;
  onClick?: () => void;
  style?: React.CSSProperties;
}
export interface ConnectorNetProps {
  mask?: string;
  size?: number;
  style?: React.CSSProperties;
}
export declare function PaletteTile(props: PaletteTileProps): JSX.Element;
export declare function ConnectorNet(props: ConnectorNetProps): JSX.Element;
