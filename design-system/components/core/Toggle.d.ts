export interface ToggleProps {
  label: string;
  /** One line under the label; the setting's consequence, not its restatement. */
  hint?: string;
  checked?: boolean;
  disabled?: boolean;
  onChange?: (next: boolean) => void;
  style?: React.CSSProperties;
}
export declare function Toggle(props: ToggleProps): JSX.Element;
