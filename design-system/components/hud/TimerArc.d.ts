/**
 * The dawn timer: mm:ss under a thin arc that fills toward dawn, pulsing in the
 * last 30 s. The head-start countdown is the same element.
 * @startingPoint section="Lucid" subtitle="Timer arc, health ring, moons, crosshair" viewport="700x260"
 */
export interface TimerArcProps {
  /** Seconds remaining. */
  seconds: number;
  /** Round length, so the arc knows how full it is. */
  total?: number;
  /** Phase line under the numerals, verbatim from the glossary. */
  phase?: string;
  label?: string;
  size?: number;
  /** Forces the urgent state; defaults to seconds <= 30. */
  urgent?: boolean;
  style?: React.CSSProperties;
}
export declare function TimerArc(props: TimerArcProps): JSX.Element;
