/// <reference types="vite/client" />

// Module declaration for Vue Single File Components
// Enables TypeScript to recognize .vue file imports
declare module "*.vue" {
  import type { DefineComponent } from "vue";

  const component: DefineComponent<object, object, unknown>;
  export default component;
}
