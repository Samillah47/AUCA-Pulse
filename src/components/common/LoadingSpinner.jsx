import { Loader2 } from 'lucide-react';
import { cn } from '../../utils/helpers';

/**
 * Reusable Loading Spinner Component
 */
const LoadingSpinner = ({
  size = 'md',
  variant = 'primary',
  fullScreen = false,
  text,
  className,
}) => {
  const sizes = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
    xl: 'w-16 h-16',
  };

  const variants = {
    primary: 'text-primary-600 dark:text-primary-400',
    secondary: 'text-secondary-600 dark:text-secondary-400',
    white: 'text-white',
    gray: 'text-gray-600 dark:text-gray-400',
  };

  const spinnerContent = (
    <div className="flex flex-col items-center justify-center gap-3">
      <Loader2 className={cn('animate-spin', sizes[size], variants[variant])} />
      {text && (
        <p className="text-sm text-gray-600 dark:text-gray-400 animate-pulse">
          {text}
        </p>
      )}
    </div>
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-white/80 dark:bg-dark-950/80 backdrop-blur-sm">
        {spinnerContent}
      </div>
    );
  }

  return (
    <div className={cn('flex items-center justify-center p-8', className)}>
      {spinnerContent}
    </div>
  );
};

export default LoadingSpinner;