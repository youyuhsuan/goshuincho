// Store
import useSettingStore from "@/stores/setting.store";

// Increases the cursor size when the mouse enters an element
const handlMouseEnterCursor = (binding?: number) => {
  const settingStore = useSettingStore();
  settingStore.cursor.size = binding ?? 5;
};

// Resets the cursor size when the mouse leaves an element
const handlMouseLeaveCursor = (binding?: number) => {
  const settingStore = useSettingStore();
  settingStore.cursor.size = binding ?? 1;
};

// Vue custom directive that changes the cursor size on hover
const vCursorHover = {
  mounted(el: HTMLElement, binding: { value?: number }) {
    el._onEnter = () => handlMouseEnterCursor(binding.value);
    el._onLeave = () => handlMouseLeaveCursor(binding.value);

    el.addEventListener("mouseenter", el._onEnter);
    el.addEventListener("mouseleave", el._onLeave);
  },
  unmounted(el: HTMLElement) {
    if (el._onEnter) {
      el.removeEventListener("mouseenter", el._onEnter);
    }
    if (el._onLeave) {
      el.removeEventListener("mouseleave", el._onLeave);
    }
  },
};

export default vCursorHover;
