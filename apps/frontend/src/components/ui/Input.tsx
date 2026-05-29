import { type InputHTMLAttributes, forwardRef, useId, useState } from 'react';
import { Eye, EyeOff, AlertCircle, CheckCircle2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { translateUiString } from '../../i18n/runtime';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  fullWidth?: boolean;
  success?: boolean;
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ 
    label, 
    error, 
    helperText, 
    fullWidth = false, 
    success = false, 
    icon,
    iconPosition = 'left', 
    className = '', 
    type = 'text', 
    required, 
    ...props 
  }, ref) => {
    useTranslation();
    const [showPassword, setShowPassword] = useState(false);
    const widthStyle = fullWidth ? 'w-full' : '';
    const isPassword = type === 'password';
    const inputType = isPassword && showPassword ? 'text' : type;

    const hasLeftIcon = !!icon && iconPosition === 'left';
    const hasRightIcon = !!icon && iconPosition === 'right';
    
    const hasRightAccessory = isPassword || hasRightIcon || !!error || !!success;
    const translatedLabel = translateUiString(label);
    const translatedError = translateUiString(error);
    const translatedHelperText = translateUiString(helperText);
    const translatedPlaceholder = translateUiString(
      typeof props.placeholder === 'string' ? props.placeholder : undefined
    );

    const inputId = useId();
    const errorId = translatedError ? `${inputId}-error` : undefined;
    const helperId = translatedHelperText && !translatedError ? `${inputId}-helper` : undefined;
    const accessoryClass = "absolute right-0 inset-y-0 flex items-center pr-3";
    
    return (
      <div className={`${widthStyle}`}>
        {translatedLabel && (
          <label htmlFor={inputId} className="mb-2 block text-sm font-semibold text-(--app-text)">
            {translatedLabel}
            {required && <span className="text-red-500 ml-1">*</span>}
          </label>
        )}
        <div className="relative">
          
          {/* --- ICON BÊN TRÁI --- */}
          {hasLeftIcon && (
            <div className="absolute inset-y-0 left-0 flex items-center pl-3 text-(--app-text-muted) pointer-events-none">
              {icon}
            </div>
          )}
          
          <input
            id={inputId}
            ref={ref}
            type={inputType}
            className={`
              rounded-xl border bg-(--app-surface) px-4 py-2.5 text-base
              transition-all duration-200
              focus:outline-none focus:ring-2 focus:ring-accent/50 focus:border-accent
              disabled:cursor-not-allowed disabled:bg-(--app-surface) disabled:text-(--app-text-muted)
              placeholder:text-(--app-text-muted)
                
              ${hasLeftIcon ? 'pl-10' : ''}
              ${hasRightAccessory ? 'pr-12' : 'pr-4'} 
              
              ${widthStyle}
              ${className}
              ${error ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50' : success ? 'border-success-300 focus:border-success-500 focus:ring-success-500/50' : 'border-(--app-border) hover:border-slate-400'}
            `}
            placeholder={translatedPlaceholder}
            aria-invalid={!!error}
            aria-describedby={errorId || helperId}
            {...props}
          />

          {(isPassword || hasRightIcon || error || success) && (
            <div className={accessoryClass}>
            
              {/* Default password toggle (only if no custom icon) */}
              {isPassword && !icon && ( 
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="text-(--app-text-muted) transition-colors hover:text-(--app-text) focus:outline-none focus:ring-0"
                  tabIndex={-1}
                >
                  {showPassword ? (
                    <EyeOff className="w-5 h-5" />
                  ) : (
                    <Eye className="w-5 h-5" />
                  )}
                </button>
              )}
              
              {/* Custom icon on right (including toggle from PasswordInput) */}
              {hasRightIcon && (
                <div className="text-(--app-text-muted)">
                  {icon}
                </div>
              )}

              {/* Error and success icons */}
              {translatedError && !isPassword && !icon && (
                <div className="text-red-500">
                  <AlertCircle className="w-5 h-5" />
                </div>
              )}
              {success && !error && !isPassword && !icon && (
                <div className="text-success-500">
                  <CheckCircle2 className="w-5 h-5" />
                </div>
              )}
            </div>
          )}
        </div>
        
        {translatedError && (
          <div id={errorId} className="mt-1.5 flex items-center gap-1 text-sm text-red-600 animate-slide-down" role="alert">
            <AlertCircle className="w-3.5 h-3.5 shrink-0" />
            <p>{translatedError}</p>
          </div>
        )}
        {translatedHelperText && !translatedError && (
          <p id={helperId} className="mt-1.5 text-sm text-(--app-text-muted)">{translatedHelperText}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export default Input;
