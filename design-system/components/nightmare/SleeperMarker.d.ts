export interface SleeperMarkerProps {
  /** 1–4. Picks the colour-blind-safe colour AND the number shown inside. */
  index: 1 | 2 | 3 | 4;
  /** Steam name beside the chip. Colour is never used without number + name. */
  name?: string;
  /** Degrees clockwise from north; draws the facing wedge. Omit off-world. */
  facing?: number;
  /** Current power target or selected row: white-gold halo. */
  selected?: boolean;
  status?: 'in-dream' | 'awake' | 'consumed';
  /** Accessibility option "colour-blind marker shapes": varies the chip outline. */
  shape?: boolean;
  /**
   * 'md' (default) for markers and rows layered over the dream, where the chip
   * must stay small. 'lg' for Results and anything read over a screen share:
   * the number goes to --fs-heading and the name to --fs-title, so no value
   * falls under the 22px floor.
   */
  scale?: 'md' | 'lg';
  /** Overrides the scale's chip box size. */
  size?: number;
  style?: React.CSSProperties;
}
export declare function SleeperMarker(props: SleeperMarkerProps): JSX.Element;
export declare const SLEEPER_COLORS: string[];
