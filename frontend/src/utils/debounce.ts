const debounce = <T extends (...args: unknown[]) => unknown>(
  fnc: T,
  delay: number = 500,
): ((...args: Parameters<T>) => void) => {
  let timer: ReturnType<typeof setTimeout> | null = null;

  return (...arg: Parameters<T>) => {
    if (timer) clearTimeout(timer);
    timer = setTimeout(() => {
      fnc(...arg);
    }, delay);
  };
};

export default debounce;
