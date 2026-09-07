export interface RejectionLabelProps {
  /**
   * Verbatim from the UI.md §14 placement glossary — one of
   * "Door is solid" | "Doesn't fit here" | "Would trap {name}" |
   * "Not enough budget ({have} / {cost})" | "Not a door".
   */
  reason: string;
  style?: React.CSSProperties;
}
export declare function RejectionLabel(props: RejectionLabelProps): JSX.Element;
export declare const REJECTIONS: string[];
