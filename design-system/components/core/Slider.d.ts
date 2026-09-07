export interface SliderProps {
  label: string;
  value: number;
  min?: number;
  max?: number;
  step?: number;
  /** Unit suffix on the value readout, e.g. 's', '%', 'cubes'. */
  unit?: string;
  /** Formats the readout instead; use for mm:ss and "1 per 4 s". */
  format?: (v: number) => string;
  disabled?: boolean;
  onChange?: (v: number) => void;
  style?: React.CSSProperties;
}
export declare function Slider(props: SliderProps): JSX.Element;
