/**
 * The one container in Lucid: translucent blue-black mist with a thin light
 * border, sat over the blurred 3D view.
 * @startingPoint section="Lucid" subtitle="Mist panel, buttons, toggle, slider, cost badge" viewport="700x420"
 */
export interface MistPanelProps {
  /** Optional heading, rendered in the display serif. */
  title?: string;
  /** Small uppercase micro label opposite the title (a count, a hotkey hint). */
  aside?: string;
  /**
   * 'panel'  — translucent mist, 72 %. ONLY for transient surfaces over the
   *            dream: toasts, hover peeks, the reveal card, hint cards.
   * 'chrome' — opaque --ink-800 with the hairline and outer shadow. Permanent
   *            docks and bars that sit over the view for a whole round.
   * 'sunken' — opaque --ink-800, no shadow. Full screens where no view shows.
   *
   * USS has no backdrop-filter, so translucency cannot be rescued by blur;
   * anything large and permanent is opaque by decision.
   */
  tone?: 'panel' | 'chrome' | 'sunken';
  /** Overrides --pad-panel. */
  pad?: string | number;
  /** 'all' rounds and borders every side; 'top'/'bottom' for edge-docked bars. */
  edge?: 'all' | 'top' | 'bottom';
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
export declare function MistPanel(props: MistPanelProps): JSX.Element;
