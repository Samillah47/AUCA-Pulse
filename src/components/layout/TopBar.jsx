import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Sun, Moon, Bell, User, LogOut, ChevronDown, Search, Menu } from 'lucide-react';
import { useUIStore } from '../../store';
import { useAuthStore } from '../../store';
import { cn } from '../../utils/helpers';
import { Avatar } from '../common';

const TopBar = ({ toggleMobileSidebar }) => {
  const navigate = useNavigate();
  const { theme, toggleTheme, openGlobalSearch } = useUIStore();
  const { user, logout } = useAuthStore();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  // Handle Ctrl+K for search
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.ctrlKey && e.key === 'k') {
        e.preventDefault();
        openGlobalSearch();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [openGlobalSearch]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-30 w-full bg-white/80 dark:bg-dark-900/80 backdrop-blur-lg backdrop-saturate-150 border-b border-gray-200/50 dark:border-dark-700/50">
      <div className="flex items-center justify-between h-16 px-4 sm:px-6 lg:px-8">
        
        {/* Left side: Mobile menu & Global Search */}
        <div className="flex items-center gap-2">
            {/* Mobile Sidebar Toggle */}
            <button
            onClick={toggleMobileSidebar}
            className="lg:hidden p-2 rounded-full text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-dark-800"
            >
                <Menu className="w-6 h-6" />
            </button>

            {/* Global Search Button */}
            <button
                onClick={openGlobalSearch}
                className="flex items-center gap-2 p-2 w-full max-w-xs rounded-lg text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-dark-800 transition-colors"
            >
                <Search className="w-5 h-5" />
                <span className="text-sm hidden md:block">Search...</span>
                <kbd className="hidden md:inline-flex items-center px-2 py-1 text-xs font-sans font-medium text-gray-500 bg-gray-100 dark:bg-dark-700 rounded border">
                Ctrl+K
                </kbd>
            </button>
        </div>


        {/* Right side controls */}
        <div className="flex items-center gap-4">
          {/* Theme Toggle */}
          <button
            onClick={toggleTheme}
            className="p-2 rounded-full text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-dark-800 transition-colors"
            aria-label="Toggle theme"
          >
            <AnimatePresence mode="wait" initial={false}>
              <motion.div
                key={theme}
                initial={{ y: -20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                exit={{ y: 20, opacity: 0 }}
                transition={{ duration: 0.2 }}
              >
                {theme === 'light' ? (
                  <Moon className="w-5 h-5" />
                ) : (
                  <Sun className="w-5 h-5" />
                )}
              </motion.div>
            </AnimatePresence>
          </button>

          {/* Notifications */}
          <button
            onClick={() => navigate('/notifications')}
            className="p-2 relative rounded-full text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-dark-800 transition-colors"
            aria-label="Notifications"
          >
            <Bell className="w-5 h-5" />
            { user?.unreadNotifications > 0 && 
              <span className="absolute top-2 right-2 block h-2 w-2 rounded-full bg-primary-600 ring-2 ring-white dark:ring-dark-900" />
            }
          </button>

          {/* Profile Dropdown */}
          <div className="relative">
            <button
              onClick={() => setIsDropdownOpen(!isDropdownOpen)}
              className="flex items-center gap-2"
            >
              <Avatar name={user?.name} src={user?.profilePicture} size="md" />
              <div className="hidden md:flex flex-col items-start">
                <span className="text-sm font-medium">{user?.name}</span>
                <span className="text-xs text-gray-500 dark:text-gray-400 capitalize">{user?.role?.toLowerCase()}</span>
              </div>
              <ChevronDown
                className={cn(
                  'w-4 h-4 text-gray-500 transition-transform',
                  isDropdownOpen && 'rotate-180'
                )}
              />
            </button>

            {/* Dropdown Menu */}
            <AnimatePresence>
              {isDropdownOpen && (
                <motion.div
                  initial={{ opacity: 0, y: -10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -10 }}
                  className="absolute right-0 mt-2 w-48 bg-white dark:bg-dark-800 rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 overflow-hidden z-10"
                  onMouseLeave={() => setIsDropdownOpen(false)}
                >
                  <div className="py-1">
                    <button
                      onClick={() => {
                        navigate('/profile');
                        setIsDropdownOpen(false);
                      }}
                      className="w-full text-left flex items-center gap-3 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-dark-700"
                    >
                      <User className="w-4 h-4" />
                      <span>My Profile</span>
                    </button>
                    <button
                      onClick={() => {
                        handleLogout();
                        setIsDropdownOpen(false);
                      }}
                      className="w-full text-left flex items-center gap-3 px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20"
                    >
                      <LogOut className="w-4 h-4" />
                      <span>Logout</span>
                    </button>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </div>
      </div>
    </header>
  );
};

export default TopBar;
