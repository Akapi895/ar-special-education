import toast from 'react-hot-toast';
import { translateUiString } from '../i18n/runtime';

export const toastStyles = {
  duration: 4000,
  style: {
    background: 'var(--toast-bg)',
    color: 'var(--toast-text)',
    border: '1px solid var(--toast-border)',
    borderRadius: '12px',
    padding: '16px',
    boxShadow: '0 16px 40px rgba(15, 23, 42, 0.18)',
  },
};

const withTranslation = (message: string) => translateUiString(message);

export const showToastError = (message: string) => {
  toast.error(withTranslation(message), {
    ...toastStyles,
    duration: 5000,
    icon: '⚠️',
  });
};

export const showToastSuccess = (message: string) => {
  toast.success(withTranslation(message), {
    ...toastStyles,
    duration: 3000,
    icon: '✅',
  });
};

export const showToastInfo = (message: string) => {
  toast(withTranslation(message), {
    ...toastStyles,
    duration: 3000,
    icon: 'ℹ️',
  });
};

export const showToastWarning = (message: string) => {
  toast(withTranslation(message), {
    ...toastStyles,
    duration: 4000,
    icon: '⚠️',
    style: {
      ...toastStyles.style,
      border: '1px solid #f59e0b',
    },
  });
};

export const showToastLoading = (message: string = 'Loading...') => {
  return toast.loading(withTranslation(message), toastStyles);
};

export const dismissToast = (toastId?: string) => {
  if (toastId) {
    toast.dismiss(toastId);
    return;
  }

  toast.dismiss();
};

export const toastTheme = {
  position: 'top-right' as const,
  toastOptions: {
    ...toastStyles,
    success: {
        iconTheme: {
          primary: '#6c9aee',
          secondary: '#fff',
        },
        style: {
          ...toastStyles.style,
          border: '1px solid #6c9aee',
        },
    },
    error: {
      iconTheme: {
        primary: '#6c9aee',
        secondary: '#fff',
      },
      style: {
        ...toastStyles.style,
        border: '1px solid #6c9aee',
      },
    },
  },
};

export default {
  error: showToastError,
  success: showToastSuccess,
  info: showToastInfo,
  warning: showToastWarning,
  loading: showToastLoading,
  dismiss: dismissToast,
};
