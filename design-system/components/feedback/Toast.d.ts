/**
 * A four-second line of copy in the top-right corner; the way every state
 * change that matters gets said out loud (UI.md §1.3).
 * @startingPoint section="Lucid" subtitle="Toast stack, rejection labels" viewport="700x220"
 */
export interface ToastProps {
  /** Verbatim from the UI.md §14 glossary. Never improvised. */
  text: string;
  /** Optional Icon name — door state or power. */
  icon?: string;
  /** Left-edge accent: exit for doors and waking, danger for consumed. */
  tone?: 'event' | 'exit' | 'danger' | 'effect';
  /** Position in the stack, 0 on top; older ones fade. */
  index?: number;
  style?: React.CSSProperties;
}
export interface ToastStackProps {
  /** Newest first. Only the first three render (UI.md §6). */
  toasts: ToastProps[];
  style?: React.CSSProperties;
}
export declare function Toast(props: ToastProps): JSX.Element;
export declare function ToastStack(props: ToastStackProps): JSX.Element;
