import { motion } from 'framer-motion';
import { User } from 'lucide-react';
import { cn, getInitials, getAvatarColor } from '../../utils/helpers';

/**
 * Reusable Avatar Component
 * Features: image, initials, fallback, status indicator
 */
const Avatar = ({
  src,
  alt,
  name,
  size = 'md',
  status,
  className,
  onClick,
  ...props
}) => {
  const sizes = {
    xs: 'w-6 h-6 text-xs',
    sm: 'w-8 h-8 text-sm',
    md: 'w-10 h-10 text-base',
    lg: 'w-12 h-12 text-lg',
    xl: 'w-16 h-16 text-xl',
    '2xl': 'w-20 h-20 text-2xl',
  };

  const statusSizes = {
    xs: 'w-1.5 h-1.5',
    sm: 'w-2 h-2',
    md: 'w-2.5 h-2.5',
    lg: 'w-3 h-3',
    xl: 'w-3.5 h-3.5',
    '2xl': 'w-4 h-4',
  };

  const statusColors = {
    online: 'bg-green-500',
    offline: 'bg-gray-400',
    busy: 'bg-red-500',
    away: 'bg-yellow-500',
  };

  const initials = getInitials(name || alt);
  const avatarColor = getAvatarColor(name || alt);

  const AvatarWrapper = onClick ? motion.button : 'div';

  const motionProps = onClick 
    ? {
        whileHover: { scale: 1.05 },
        whileTap: { scale: 0.95 },
      }
    : {};

  return (
    <AvatarWrapper
      className={cn(
        'relative inline-flex items-center justify-center rounded-full overflow-hidden flex-shrink-0',
        sizes[size],
        onClick && 'cursor-pointer hover:opacity-80 transition-opacity',
        className
      )}
      onClick={onClick}
      {...motionProps}
      {...props}
    >
      {src ? (
        <img
          src={src}
          alt={alt || name}
          className="w-full h-full object-cover"
          onError={(e) => {
            e.target.style.display = 'none';
          }}
        />
      ) : initials ? (
        <div className={cn('w-full h-full flex items-center justify-center font-semibold text-white', avatarColor)}>
          {initials}
        </div>
      ) : (
        <div className="w-full h-full flex items-center justify-center bg-gray-200 dark:bg-dark-700">
          <User className="w-1/2 h-1/2 text-gray-400 dark:text-dark-400" />
        </div>
      )}

      {/* Status Indicator */}
      {status && (
        <span
          className={cn(
            'absolute bottom-0 right-0 rounded-full border-2 border-white dark:border-dark-900',
            statusSizes[size],
            statusColors[status]
          )}
        />
      )}
    </AvatarWrapper>
  );
};

export default Avatar;