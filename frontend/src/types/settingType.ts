export type SettingView = "personal" | "appearance";
export type theme = "light" | "dark" | "system";

export interface CardItem {
  icon: string;
  src: string;
  view: theme;
}
