import { type ButtonHTMLAttributes, type ReactNode } from 'react';
import { Loader2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { translateUiString } from '../../i18n/runtime';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  variant?: 'primary' | 'secondary' | 'danger' | 'outline' | 'success' | 'warning' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  loading?: boolean;
  icon?: ReactNode;
}

const Button = ({
  children,
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  loading = false,
  icon,
  className = '',
  disabled,
  ...props
}: ButtonProps) => {
  const { t } = useTranslation();
  const baseStyles = 'inline-flex items-center justify-center gap-2 font-semibold rounded-xl transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed active:scale-95 btn-ripple shadow-soft hover:shadow-medium';
  
  const variantStyles = {
    primary: 'bg-gradient-primary text-white hover:brightness-105 hover:shadow-glow-accent focus:ring-accent-400 border border-transparent',
    secondary: 'bg-[var(--app-surface)] text-[var(--app-text)] hover:bg-[var(--app-surface-strong)] focus:ring-gray-300 border border-[var(--app-border)]',
    danger: 'bg-gradient-danger text-white hover:brightness-105 hover:shadow-glow focus:ring-danger-400 border border-transparent',
    success: 'bg-gradient-success text-white hover:brightness-105 hover:shadow-glow-success focus:ring-success-400 border border-transparent',
    warning: 'bg-gradient-warning text-gray-900 hover:brightness-105 hover:shadow-glow focus:ring-warning-400 border border-transparent',
    outline: 'border-2 border-accent-500 text-accent-600 bg-[var(--app-surface)] hover:bg-accent-500 hover:text-white focus:ring-accent-400',
    ghost: 'text-[var(--app-text-muted)] hover:bg-[var(--app-surface)] hover:text-[var(--app-text)] focus:ring-gray-300 border border-transparent',
  };

  const sizeStyles = {
    sm: 'px-3 py-1.5 text-sm min-h-[32px]',
    md: 'px-5 py-2.5 text-base min-h-[40px]',
    lg: 'px-7 py-3.5 text-lg min-h-[48px]',
  };

  const widthStyle = fullWidth ? 'w-full' : '';
  const translatedChildren = typeof children === 'string' ? translateUiString(children) : children;

  return (
    <button
      className={`${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${widthStyle} ${className}`}
      disabled={disabled || loading}
      {...props}
    >
      {loading ? (
        <>
          <Loader2 className="w-4 h-4 animate-spin" />
          <span>{translateUiString(t('common.loading', { defaultValue: 'Loading...' }))}</span>
        </>
      ) : (
        <>
          {icon && <span className="shrink-0">{icon}</span>}
          {translatedChildren}
        </>
      )}
    </button>
  );
};

export default Button;
