// Store
import useSettingStore from "@/stores/setting.store";

const vCursorStamp = {
  mounted(el: HTMLElement, _binding: { value?: unknown }) {
    const settingStore = useSettingStore();

    el._onEnter = () => {
      settingStore.cursor.type = "stamp";
    };

    el._onLeave = () => {
      settingStore.cursor.type = "dot";
    };

    el._onClick = () => {
      settingStore.cursor.clickId++;
    };

    el.addEventListener("mouseenter", el._onEnter);
    el.addEventListener("mouseleave", el._onLeave);
    el.addEventListener("click", el._onClick);
  },

  unmounted(el: HTMLElement) {
    const settingStore = useSettingStore();
    settingStore.cursor.type = "dot";

    if (el._onEnter) el.removeEventListener("mouseenter", el._onEnter);
    if (el._onLeave) el.removeEventListener("mouseleave", el._onLeave);
    if (el._onClick) el.removeEventListener("click", el._onClick);
  },
};

export default vCursorStamp;
