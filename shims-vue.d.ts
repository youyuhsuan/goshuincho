/// <reference types="vite/client" />
/// <reference types="vite-svg-loader" />

// Module declaration for Vue Single File Components
// Enables TypeScript to recognize .vue file imports
declare module "*.vue" {
  import type { DefineComponent } from "vue";
  const component: DefineComponent<object, object, unknown>;
  export default component;
}

// SVG module declarations for vite-svg-loader
declare module "*.svg" {
  import type { DefineComponent } from "vue";
  const component: DefineComponent;
  export default component;
}

declare module "*.svg?component" {
  import { DefineComponent } from "vue";
  const component: DefineComponent;
  export default component;
}
