// Vue
import { ref, readonly } from "vue";
// Axios
import type { AxiosResponse } from "axios";
// Type
import type { Pagination } from "@/types/commonType";
// Composables
import useMessage from "@/composables/useMessage";

const useAsyncPaginatedState = <T, A extends unknown[]>(
  asyncFunction: (...args: A) => Promise<AxiosResponse<T[]>>,
) => {
  const data = ref<T[]>([]);
  const pagination = ref<Pagination | null>(null);

  const isLoading = ref<boolean>(false);
  const { showError } = useMessage();

  const execute = async (...args: A) => {
    isLoading.value = true;
    try {
      const result = await asyncFunction(...args);
      data.value = result.data;
      pagination.value = JSON.parse(result.headers["x-pagination"]);
    } catch (errorMessage: unknown) {
      console.error("Async paginated function error:", errorMessage);

      // Show error message if provided
      if (typeof errorMessage === "string") showError(errorMessage);
    } finally {
      isLoading.value = false;
    }
  };

  return {
    execute,
    isLoading: readonly(isLoading),
    pagination: readonly(pagination),
    data,
  };
};

export default useAsyncPaginatedState;
