import { forwardRef, useState } from 'react';
import { Eye, EyeOff, AlertCircle } from 'lucide-react';
import { cn } from '../../utils/helpers';

/**
 * Reusable Input Component
 * Supports: text, email, password, number, textarea
 * Features: error states, icons, character count
 */
const Input = forwardRef(({
  label,
  type = 'text',
  error,
  helperText,
  leftIcon,
  rightIcon,
  placeholder,
  disabled = false,
  fullWidth = true,
  className,
  rows = 4,
  maxLength,
  showCharCount = false,
  value,
  onChange,
  ...props
}, ref) => {
  const [showPassword, setShowPassword] = useState(false);
  const [charCount, setCharCount] = useState(value?.length || 0);

  const isTextarea = type === 'textarea';
  const isPassword = type === 'password';
  const inputType = isPassword ? (showPassword ? 'text' : 'password') : type;

  const handleChange = (e) => {
    setCharCount(e.target.value.length);
    if (onChange) onChange(e);
  };

  const baseInputStyles = cn(
    'input-base',
    leftIcon && 'pl-10',
    (rightIcon || isPassword) && 'pr-10',
    error && 'border-red-500 focus:ring-red-500 dark:border-red-500',
    disabled && 'bg-gray-100 cursor-not-allowed dark:bg-dark-900',
    fullWidth && 'w-full',
    className
  );

  const InputElement = isTextarea ? 'textarea' : 'input';

  return (
    <div className={cn('space-y-1', fullWidth && 'w-full')}>
      {label && (
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          {label}
        </label>
      )}

      <div className="relative">
        {/* Left Icon */}
        {leftIcon && (
          <div className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-dark-400">
            {leftIcon}
          </div>
        )}

        {/* Input Field */}
        <InputElement
          ref={ref}
          type={inputType}
          placeholder={placeholder}
          disabled={disabled}
          className={baseInputStyles}
          value={value}
          onChange={handleChange}
          maxLength={maxLength}
          {...(isTextarea && { rows })}
          {...props}
        />

        {/* Right Icon / Password Toggle */}
        {isPassword ? (
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:text-dark-400 dark:hover:text-dark-300 transition-colors"
            tabIndex={-1}
          >
            {showPassword ? (
              <EyeOff className="w-5 h-5" />
            ) : (
              <Eye className="w-5 h-5" />
            )}
          </button>
        ) : rightIcon ? (
          <div className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-dark-400">
            {rightIcon}
          </div>
        ) : null}
      </div>

      {/* Error Message */}
      {error && (
        <div className="flex items-center gap-1 text-sm text-red-600 dark:text-red-400 animate-slide-down">
          <AlertCircle className="w-4 h-4" />
          <span>{error}</span>
        </div>
      )}

      {/* Helper Text / Character Count */}
      {(helperText || showCharCount) && (
        <div className="flex items-center justify-between text-xs text-gray-500 dark:text-dark-400">
          <span>{helperText}</span>
          {showCharCount && maxLength && (
            <span className={cn(
              'font-medium',
              charCount > maxLength * 0.9 && 'text-orange-500',
              charCount >= maxLength && 'text-red-500'
            )}>
              {charCount}/{maxLength}
            </span>
          )}
        </div>
      )}
    </div>
  );
});

Input.displayName = 'Input';

export default Input;