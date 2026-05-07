declare global {
  interface HTMLElement {
    _onEnter?: () => void;
    _onLeave?: () => void;
  }
}
