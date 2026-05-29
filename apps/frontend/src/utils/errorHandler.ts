/**
 * Centralized Error Handler
 * Provides consistent error handling across the application
 */

import { getErrorMessage } from '../api/client/axiosClient';
import { translateUiString } from '../i18n/runtime';
import {
    dismissToast as dismissToastHelper,
    showToastError,
    showToastInfo,
    showToastLoading,
    showToastSuccess,
    showToastWarning,
} from './toast';

/**
 * Handle API errors with toast notifications
 * @param error - The error object from API call
 * @param customMessage - Optional custom message to display
 * @returns The error message string
 */
export const handleApiError = (error: unknown, customMessage?: string): string => {
    const message = translateUiString(customMessage || getErrorMessage(error));

    // Show toast notification
    showToastError(message);

    // Log to console in development
    if (import.meta.env.DEV) {
        console.error('API Error:', error);
    }

    // TODO: Send to error tracking service (Sentry, LogRocket, etc.)
    // trackError(error, { customMessage });

    return message;
};

/**
 * Handle success messages with toast
 * @param message - Success message to display
 */
export const showSuccess = (message: string) => {
    showToastSuccess(message);
};

/**
 * Handle info messages with toast
 * @param message - Info message to display
 */
export const showInfo = (message: string) => {
    showToastInfo(message);
};

/**
 * Handle warning messages with toast
 * @param message - Warning message to display
 */
export const showWarning = (message: string) => {
    showToastWarning(message);
};

/**
 * Show loading toast
 * @param message - Loading message
 * @returns Toast ID for dismissing later
 */
export const showLoading = (message: string = 'Loading...') => {
    return showToastLoading(message);
};

/**
 * Dismiss a specific toast or all toasts
 * @param toastId - Optional toast ID to dismiss specific toast
 */
export const dismissToast = (toastId?: string) => {
    return dismissToastHelper(toastId);
};

export default {
    error: handleApiError,
    success: showSuccess,
    info: showInfo,
    warning: showWarning,
    loading: showLoading,
    dismiss: dismissToast,
};
