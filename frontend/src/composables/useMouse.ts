import { ref, onMounted, onUnmounted } from "vue";

const useMouse = () => {
  const x = ref<number>(0);
  const y = ref<number>(0);

  const onMove = (e: MouseEvent) => {
    x.value = e.clientX;
    y.value = e.clientY;
  };

  onMounted(() => window.addEventListener("mousemove", onMove));
  onUnmounted(() => window.removeEventListener("mousemove", onMove));

  return { x, y };
};

export default useMouse;
