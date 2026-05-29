import { type KeyboardEvent as ReactKeyboardEvent, type ReactNode, useEffect, useId, useRef } from 'react';
import { X } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { translateUiString } from '../../i18n/runtime';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  children: ReactNode;
  title?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  showCloseButton?: boolean;
  closeOnBackdrop?: boolean;
}

const Modal = ({
  isOpen,
  onClose,
  children,
  title,
  size = 'md',
  showCloseButton = true,
  closeOnBackdrop = true,
}: ModalProps) => {
  useTranslation();
  const modalId = useId();
  const dialogRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const previousActiveElementRef = useRef<HTMLElement | null>(null);

  // Đóng modal khi nhấn ESC
  useEffect(() => {
    const handleEscape = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  // Ngăn scroll body khi modal mở
  useEffect(() => {
    if (!isOpen) {
      previousActiveElementRef.current?.focus();
      return;
    }

    previousActiveElementRef.current = document.activeElement as HTMLElement | null;
    window.requestAnimationFrame(() => {
      closeButtonRef.current?.focus();
    });
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) {
      document.body.style.overflow = 'unset';
      return;
    }

    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  const translatedTitle = translateUiString(title);
  const titleId = translatedTitle ? `${modalId}-title` : undefined;

  const sizeStyles = {
    sm: 'max-w-md',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl',
    full: 'max-w-7xl',
  };

  const handleKeyDown = (event: ReactKeyboardEvent<HTMLDivElement>) => {
    if (event.key !== 'Tab' || !dialogRef.current) {
      return;
    }

    const focusableElements = dialogRef.current.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const focusable = Array.from(focusableElements).filter((element) => !element.hasAttribute('disabled'));

    if (focusable.length === 0) {
      event.preventDefault();
      return;
    }

    const firstFocusable = focusable[0];
    const lastFocusable = focusable[focusable.length - 1];

    if (event.shiftKey && document.activeElement === firstFocusable) {
      event.preventDefault();
      lastFocusable.focus();
    } else if (!event.shiftKey && document.activeElement === lastFocusable) {
      event.preventDefault();
      firstFocusable.focus();
    }
  };

  const handleBackdropClick = () => {
    if (closeOnBackdrop) {
      onClose();
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fade-in">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/60 backdrop-blur-md animate-fade-in"
        onClick={handleBackdropClick}
      />

      {/* Modal Content */}
      <div 
        ref={dialogRef}
        onKeyDown={handleKeyDown}
        className={`
          relative w-full rounded-2xl bg-[var(--app-surface-strong)] shadow-strong 
          ${sizeStyles[size]} max-h-[90vh] overflow-hidden
          animate-scale-in border border-[var(--app-border)]
        `}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
      >
        {/* Header */}
        {(translatedTitle || showCloseButton) && (
          <div className="flex items-center justify-between border-b border-[var(--app-border)] bg-[var(--app-bg)] px-6 py-4">
            {translatedTitle && (
              <h2 id={titleId} className="flex items-center gap-2 text-xl font-bold text-[var(--app-text)]">
                <div className="w-1 h-6 bg-gradient-accent rounded-full" />
                {translatedTitle}
              </h2>
            )}
            {showCloseButton && (
              <button
                ref={closeButtonRef}
                onClick={onClose}
                className="group ml-auto rounded-xl p-2 transition-all duration-200 active:scale-95 hover:bg-[var(--app-surface)]"
                aria-label="Đóng hộp thoại"
              >
                <X className="h-5 w-5 text-[var(--app-text-muted)] transition-all duration-200 group-hover:rotate-90 group-hover:text-[var(--app-text)]" />
              </button>
            )}
          </div>
        )}

        {/* Body */}
        <div className="max-h-[calc(90vh-8rem)] overflow-y-auto px-6 py-5 scrollbar-hide">
          {children}
        </div>
      </div>
    </div>
  );
};

export default Modal;
