import { ref, readonly } from "vue";
// Composables
import useMessage from "@/composables/useMessage";

interface UseAsyncActionOptions {
  showErrorToast?: boolean;
  onError?: (error: unknown) => void;
  successMessage?: string;
}

/**
 * Wraps an async operation with automatic loading state and error handling.
 * Reduces boilerplate for common async patterns in components.
 *
 * @example
 * const { isLoading, execute } = useAsyncAction(
 *   (e.values as LoginRequest) => login(values)
 * );
 */

const useAsyncAction = <A extends unknown[]>(
  asyncFunction: (...args: A) => Promise<void>,
  options?: UseAsyncActionOptions,
) => {
  const isLoading = ref<boolean>(false);
  const { showError, showSuccess } = useMessage();

  if (!options) options = {};
  const { showErrorToast, onError, successMessage } = options;

  // Wraps the async function with loading and error handling
  const execute = async (...args: A): Promise<boolean> => {
    isLoading.value = true;

    try {
      await asyncFunction(...args);

      // Show success message if provided
      if (successMessage) showSuccess(successMessage);
      return true;
    } catch (errorMessage: unknown) {
      console.error("Async function action error:", errorMessage);

      // Show error message if provided
      if (typeof errorMessage === "string") {
        if (onError) {
          onError(errorMessage);
        } else if (showErrorToast) {
          showError(errorMessage);
        }
      }
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  return { isLoading: readonly(isLoading), execute };
};

export default useAsyncAction;
