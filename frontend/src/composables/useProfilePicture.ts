import { ref, watch } from "vue";

const useProfilePicture = (pictureUrl: () => string | undefined) => {
  const isPictureReady = ref<boolean>(false);
  const hasError = ref<boolean>(false);

  watch(
    pictureUrl,
    (url) => {
      isPictureReady.value = false;
      hasError.value = false;

      if (!url) return;
      const img = new Image();

      img.onload = () => {
        isPictureReady.value = true;
      };
      img.onerror = () => {
        hasError.value = true;
      };
      img.src = url;
    },
    { immediate: true },
  );

  return { isPictureReady, hasError };
};

export default useProfilePicture;
