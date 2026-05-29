import i18n from 'i18next';
import toast from 'react-hot-toast';
import { initReactI18next } from 'react-i18next';

import { resources } from './resources';
import { translateUiString } from './runtime';

void i18n.use(initReactI18next).init({
  resources,
  lng: 'vi',
  fallbackLng: 'vi',
  interpolation: {
    escapeValue: false,
  },
});

let runtimePatched = false;

export const installLocalizationRuntimePatches = () => {
  if (runtimePatched || typeof window === 'undefined') {
    return;
  }

  const originalAlert = window.alert.bind(window);
  window.alert = (message?: string) => {
    originalAlert(translateUiString(message));
  };

  const originalSuccess = toast.success.bind(toast);
  const originalError = toast.error.bind(toast);
  const originalLoading = toast.loading.bind(toast);
  const originalToast = toast.bind(toast);

  toast.success = ((message, options) =>
    originalSuccess(typeof message === 'string' ? translateUiString(message) : message, options)) as typeof toast.success;
  toast.error = ((message, options) =>
    originalError(typeof message === 'string' ? translateUiString(message) : message, options)) as typeof toast.error;
  toast.loading = ((message, options) =>
    originalLoading(typeof message === 'string' ? translateUiString(message) : message, options)) as typeof toast.loading;
  Object.assign(originalToast, toast);

  runtimePatched = true;
};

export default i18n;
