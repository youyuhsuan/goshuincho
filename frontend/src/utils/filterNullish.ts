/**
 * Removes all null or undefined properties from an object.
 *
 * @param obj - The object to filter
 * @returns A new object without null or undefined values
 *
 * @example
 * filterNullish({ name: 'Tokyo', lat: null, lng: undefined })
 * // => { name: 'Tokyo' }
 */
const filterNullish = <T extends object>(
  obj: T | null | undefined,
): Partial<T> => {
  if (obj == null) return {};

  return Object.fromEntries(
    Object.entries(obj).filter(([, value]) => value != null),
  ) as Partial<T>;
};

export default filterNullish;
