import type { Shrine } from "@/types/shrinesType";

/**
 * Formats a shrine's address into a readable string.
 *
 * @param shrine - The shrine object containing address fields
 * @returns A dot-separated address string, omitting any empty values
 *
 * @example
 * formatAddress({ region: '関東', prefecture: '東京都', city: '渋谷区' })
 * // => '関東 · 東京都 · 渋谷区'
 *
 * formatAddress({ region: '関東', prefecture: '東京都', city: null })
 * // => '関東 · 東京都'
 */
export const formatAddress = (shrine: Shrine) => {
  return [shrine.region, shrine.prefecture, shrine.city]
    .filter(Boolean)
    .join(" · ");
};
