import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import {
  LayoutDashboard,
  DoorOpen,
  Users,
  Building2,
  Bell,
  User,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
  CheckCircle,
  Calendar,
  Menu,
  KeyRound,
} from 'lucide-react';
import { cn } from '../../utils/helpers';
import { ROLES } from '../../utils/constants';
import { useAuthStore } from '../../store/authStore'; // Added import
import { useQueryClient } from '@tanstack/react-query'; // Added import

/**
 * Reusable Sidebar Component
 * Features: collapsible, role-based navigation, active states
 */
const Sidebar = ({ isCollapsed, setIsCollapsed, user }) => {
  const navigate = useNavigate();
  const logoutAuthStore = useAuthStore((state) => state.logout); // Get logout action from store
  const queryClient = useQueryClient(); // Get query client

  const handleLogout = () => {
    queryClient.clear(); // Clear React Query cache
    logoutAuthStore(); // Clear auth store state and localStorage
    navigate('/login'); // Navigate to login page
  };

  // Navigation items based on role
  const getNavigationItems = () => {
    const baseItems = [
      {
        label: 'Dashboard',
        icon: LayoutDashboard,
        path: '/dashboard',
        roles: [ROLES.STUDENT, ROLES.LECTURER, ROLES.STAFF, ROLES.ADMIN],
      },
      {
        label: 'Rooms',
        icon: DoorOpen,
        path: '/rooms',
        roles: [ROLES.STUDENT, ROLES.LECTURER, ROLES.STAFF, ROLES.ADMIN],
      },
      {
        label: 'Lecturers',
        icon: Users,
        path: '/lecturers',
        roles: [ROLES.STUDENT, ROLES.LECTURER, ROLES.STAFF, ROLES.ADMIN],
      },
    ];

    const roleSpecificItems = {
      [ROLES.LECTURER]: [
        {
          label: 'My Schedule',
          icon: Calendar,
          path: '/schedule',
        },
      ],
      [ROLES.STAFF]: [
        {
          label: 'My Office',
          icon: Building2,
          path: '/office',
        },
      ],
      [ROLES.ADMIN]: [
        {
          label: 'Verifications',
          icon: CheckCircle,
          path: '/admin/verifications',
          badge: user?.pendingCount || 0,
        },
        {
          label: 'User Management',
          icon: Users,
          path: '/admin/users',
        },
        {
          label: 'Password Resets',
          icon: KeyRound,
          path: '/admin/password-resets',
        },
      ],
    };

    const userItems = [
      {
        label: 'Notifications',
        icon: Bell,
        path: '/notifications',
        badge: user?.unreadNotifications || 0,
        roles: [ROLES.STUDENT, ROLES.LECTURER, ROLES.STAFF, ROLES.ADMIN],
      },
      {
        label: 'Profile',
        icon: User,
        path: '/profile',
        roles: [ROLES.STUDENT, ROLES.LECTURER, ROLES.STAFF, ROLES.ADMIN],
      },
    ];

    const filteredBase = baseItems.filter((item) =>
      item.roles.includes(user?.role)
    );

    const roleItems = roleSpecificItems[user?.role] || [];

    const filteredUserItems = userItems.filter((item) =>
      item.roles.includes(user?.role)
    );

    return [...filteredBase, ...roleItems, ...filteredUserItems];
  };

  const navigationItems = getNavigationItems();

  return (
    <>
      {/* Sidebar */}
      <motion.aside
        initial={false}
        animate={{
          width: isCollapsed ? '80px' : '280px',
        }}
        className={cn(
          'fixed left-0 top-0 h-screen bg-white dark:bg-dark-900',
          'border-r border-gray-200 dark:border-dark-700',
          'flex flex-col z-40 transition-all duration-300'
        )}
      >
        {/* Logo Section */}
        <div className="h-16 flex items-center justify-between px-4 border-b border-gray-200 dark:border-dark-700">
          <AnimatePresence mode="wait">
            {!isCollapsed ? (
              <motion.div
                key="full-logo"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="flex items-center gap-3"
              >
                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary-600 to-secondary-600 flex items-center justify-center">
                  <span className="text-white font-bold text-lg">AC</span>
                </div>
                <div>
                  <h1 className="font-display font-bold text-lg gradient-text">
                    ACPulse
                  </h1>
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    AUCA Campus
                  </p>
                </div>
              </motion.div>
            ) : (
              <motion.div
                key="collapsed-logo"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary-600 to-secondary-600 flex items-center justify-center mx-auto"
              >
                <span className="text-white font-bold text-lg">AC</span>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Toggle Button - Only show on desktop */}
          <button
            onClick={() => setIsCollapsed(!isCollapsed)}
            className={cn(
              'hidden lg:flex p-1.5 rounded-lg',
              'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300',
              'hover:bg-gray-100 dark:hover:bg-dark-800',
              'transition-colors',
              isCollapsed && 'mx-auto'
            )}
          >
            {isCollapsed ? (
              <ChevronRight className="w-5 h-5" />
            ) : (
              <ChevronLeft className="w-5 h-5" />
            )}
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto custom-scrollbar p-4 space-y-1">
          {navigationItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 px-3 py-2.5 rounded-lg',
                  'text-gray-700 dark:text-gray-300',
                  'hover:bg-gray-100 dark:hover:bg-dark-800',
                  'transition-all duration-200 group relative',
                  isActive &&
                    'bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400 font-medium',
                  isCollapsed && 'justify-center'
                )
              }
            >
              {({ isActive }) => (
                <>
                  <item.icon
                    className={cn(
                      'w-5 h-5 flex-shrink-0',
                      isActive
                        ? 'text-primary-600 dark:text-primary-400'
                        : 'text-gray-500 dark:text-gray-400 group-hover:text-gray-700 dark:group-hover:text-gray-300'
                    )}
                  />

                  {!isCollapsed && (
                    <>
                      <span className="flex-1">{item.label}</span>
                      {item.badge > 0 && (
                        <span className="px-2 py-0.5 text-xs font-medium bg-red-500 text-white rounded-full">
                          {item.badge > 99 ? '99+' : item.badge}
                        </span>
                      )}
                    </>
                  )}

                  {/* Tooltip for collapsed state */}
                  {isCollapsed && (
                    <div className="absolute left-full ml-2 px-2 py-1 bg-gray-900 dark:bg-gray-700 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity whitespace-nowrap z-50">
                      {item.label}
                      {item.badge > 0 && ` (${item.badge})`}
                    </div>
                  )}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        {/* User Section */}
        <div className="p-4 border-t border-gray-200 dark:border-dark-700">
          <button
            onClick={handleLogout}
            className={cn(
              'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg',
              'text-red-600 dark:text-red-400',
              'hover:bg-red-50 dark:hover:bg-red-900/20',
              'transition-colors group',
              isCollapsed && 'justify-center'
            )}
          >
            <LogOut className="w-5 h-5 flex-shrink-0" />
            {!isCollapsed && <span>Logout</span>}

            {/* Tooltip for collapsed state */}
            {isCollapsed && (
              <div className="absolute left-full ml-2 px-2 py-1 bg-gray-900 dark:bg-gray-700 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity whitespace-nowrap">
                Logout
              </div>
            )}
          </button>
        </div>
      </motion.aside>
    </>
  );
};

export default Sidebar;