import { ref, readonly } from "vue";
// Composables
import useMessage from "@/composables/useMessage";

/**
 * Wraps an async function with reactive loading, error, and data state.
 * Eliminates try/catch/finally boilerplate in components.
 *
 * @example
 * const { data, isLoading, error, execute } = useAsyncState(
 *   (id: string) => getUser(id).then(r => r.data)
 * );
 */

interface UseAsyncStateOptions {
  showErrorToast?: boolean;
  onError?: (error: unknown) => void;
  successMessage?: string;
}

const useAsyncState = <T, A extends unknown[]>(
  asyncFunction: (...args: A) => Promise<T>,
  options?: UseAsyncStateOptions,
) => {
  const data = ref<T | null>(null);
  const isLoading = ref<boolean>(false);
  const error = ref<string | null>(null);

  if (!options) options = {};
  const { showErrorToast, onError, successMessage } = options;

  const { showError, showSuccess } = useMessage();

  // Wraps the async function with loading and error handling
  const execute = async (...args: A): Promise<void | null> => {
    isLoading.value = true;
    error.value = null;

    try {
      data.value = await asyncFunction(...args);

      // Show success message if provided
      if (successMessage) showSuccess(successMessage);

      return data.value;
    } catch (errorMessage: unknown) {
      console.error("Async function error:", errorMessage);

      // Show error message if provided
      if (typeof errorMessage === "string") {
        if (onError) {
          onError(errorMessage);
        } else if (showErrorToast) {
          showError(errorMessage);
        }
      }
      return null;
    } finally {
      isLoading.value = false;
    }
  };

  return {
    execute,
    isLoading: readonly(isLoading),
    data,
    error,
  };
};

export default useAsyncState;
