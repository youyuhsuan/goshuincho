declare global {
  interface HTMLElement {
    _onEnter?: () => void;
    _onLeave?: () => void;
    _onClick?: (e: MouseEvent) => void;
  }
}

export interface Impression {
  id: number;
  x: number;
  y: number;
}
