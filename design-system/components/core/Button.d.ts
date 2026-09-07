export interface ButtonProps {
  children?: React.ReactNode;
  /** primary = white-gold, one per screen; secondary = hairline. */
  variant?: 'primary' | 'secondary';
  /** 'lg' for Title and Results, 'md' everywhere else. */
  size?: 'md' | 'lg';
  /** An Icon name rendered before the label. */
  icon?: string;
  /** Key cap shown after the label, e.g. 'Esc'. */
  hotkey?: string;
  disabled?: boolean;
  /**
   * Why the button cannot be used — REPLACES the label and disables it.
   * "Never hide a rule" (UI.md §1.3): Start says "Nobody picked Nightmare".
   */
  reason?: string;
  full?: boolean;
  onClick?: () => void;
  style?: React.CSSProperties;
}
export declare function Button(props: ButtonProps): JSX.Element;
