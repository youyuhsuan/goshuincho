const debounce = <T extends (...args: any[]) => any>(
  fnc: T,
  delay: number = 500,
) => {
  let timer: ReturnType<typeof setTimeout> | null = null;

  return (...arg: Parameters<T>) => {
    if (timer) clearTimeout(timer);
    timer = setTimeout(() => {
      fnc(...arg);
    }, delay);
  };
};

export default debounce;
