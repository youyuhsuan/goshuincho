import type { SUPPORTED_LOCALES } from "@/constants/common";

export type SettingView = "personal" | "appearance";
export type theme = "light" | "dark" | "system";

export type Language = (typeof SUPPORTED_LOCALES)[number];

export interface CardItem {
  icon: string;
  src: string;
  view: theme;
}

export interface LanguageOption {
  code: Language;
  name: string;
}
