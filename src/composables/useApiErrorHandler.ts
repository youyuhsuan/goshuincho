import { AxiosError } from "axios";
// Composables
import useMessage from "@/composables/useMessage";
// Utils
import handleError from "@/utils/errorHandler";

const useApiErrorHandler = () => {
  const { showError, showWarning } = useMessage();

  const handleAndShowError = (error: unknown, context?: string) => {
    const errorMessage = handleError(error);
    const contextMessage = context
      ? `${context}: ${errorMessage}`
      : errorMessage;

    if (error instanceof AxiosError && error.response?.status === 401) {
      showWarning("Session expired. Please log in again.");
    } else {
      showError(contextMessage);
    }
  };

  return {
    handleAndShowError,
    getErrorMessage: handleError,
  };
};

export default useApiErrorHandler;
