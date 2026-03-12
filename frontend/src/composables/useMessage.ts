import { useToast } from "primevue/usetoast";

// Default toast display duration in milliseconds
const LIFE = 3000;

// Toast message composable
const useMessage = () => {
  const toast = useToast();

  // Show success toast message
  const showSuccess = (detail: string, summary = "Success") => {
    toast.add({
      severity: "success",
      summary,
      detail,
      life: LIFE,
    });
  };

  // Show error toast message
  const showError = (detail: string, summary = "Error") => {
    toast.add({
      severity: "error",
      summary,
      detail,
      life: LIFE,
    });
  };

  // Show warning toast message
  const showWarning = (detail: string, summary = "Warning") => {
    toast.add({
      severity: "warn",
      summary,
      detail,
      life: LIFE,
    });
  };

  // Show info toast message
  const showInfo = (detail: string, summary = "Info") => {
    toast.add({
      severity: "info",
      summary,
      detail,
      life: LIFE,
    });
  };

  return {
    showError,
    showSuccess,
    showWarning,
    showInfo,
  };
};

export default useMessage;
