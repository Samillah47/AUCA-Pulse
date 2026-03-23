import { motion } from 'framer-motion';
import { cn } from '../../utils/helpers';

/**
 * Reusable Card Component
 * Variants: default, outlined, elevated, glass
 * Features: hover effects, animations, custom padding
 */
const Card = ({
  children,
  variant = 'default',
  padding = 'md',
  hover = false,
  clickable = false,
  className,
  onClick,
  ...props
}) => {
  const baseStyles = 'rounded-xl transition-all duration-300';

  const variants = {
    default: 'bg-white dark:bg-dark-900 border border-gray-200 dark:border-dark-700',
    outlined: 'border-2 border-gray-200 dark:border-dark-700 bg-transparent',
    elevated: 'bg-white dark:bg-dark-900 shadow-lg',
    glass: 'glass shadow-sm',
  };

  const paddings = {
    none: '',
    sm: 'p-4',
    md: 'p-6',
    lg: 'p-8',
  };

  const hoverStyles = hover || clickable
    ? 'card-hover cursor-pointer'
    : '';

  const cardClasses = cn(
    baseStyles,
    variants[variant],
    paddings[padding],
    hoverStyles,
    className
  );

  if (clickable || onClick) {
    return (
      <motion.div
        className={cardClasses}
        onClick={onClick}
        whileHover={{ y: -4 }}
        whileTap={{ scale: 0.98 }}
        {...props}
      >
        {children}
      </motion.div>
    );
  }

  return (
    <div className={cardClasses} onClick={onClick} {...props}>
      {children}
    </div>
  );
};

// Card Sub-components
Card.Header = ({ children, className, ...props }) => (
  <div className={cn('mb-4 pb-4 border-b border-gray-200 dark:border-dark-700', className)} {...props}>
    {children}
  </div>
);

Card.Body = ({ children, className, ...props }) => (
  <div className={cn('', className)} {...props}>
    {children}
  </div>
);

Card.Footer = ({ children, className, ...props }) => (
  <div className={cn('mt-4 pt-4 border-t border-gray-200 dark:border-dark-700', className)} {...props}>
    {children}
  </div>
);

Card.Title = ({ children, className, ...props }) => (
  <h3 className={cn('text-xl font-semibold text-gray-900 dark:text-gray-100', className)} {...props}>
    {children}
  </h3>
);

Card.Description = ({ children, className, ...props }) => (
  <p className={cn('text-sm text-gray-600 dark:text-gray-400', className)} {...props}>
    {children}
  </p>
);

export default Card;