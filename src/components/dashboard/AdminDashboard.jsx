import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Users, CheckCircle, BarChart2, AlertTriangle, DoorOpen, Bell, Shield, UserCheck } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import Card from '../common/Card';
import Button from '../common/Button';
import LoadingSpinner from '../common/LoadingSpinner';
import { adminService, notificationService } from '../../services';
import { toast } from 'react-hot-toast';
import { useAuthStore } from '../../store/authStore';
import { useQuery } from '@tanstack/react-query';


const AdminDashboard = () => {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [stats, setStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch unread notification count
  const { data: unreadCount = 0 } = useQuery({
    queryKey: ['unreadNotifications', user?.id],
    queryFn: () => notificationService.getUnreadCount(user?.id),
    enabled: !!user?.id,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await adminService.getDashboardStats();
        setStats(data);
      } catch (error) {
        console.error('Dashboard error:', error);
        
        // Check for specific error status
        if (error.response?.status === 403) {
          setError('You do not have permission to access admin features. Please ensure you are logged in as an admin.');
        } else if (error.response?.status === 401) {
          toast.error('Your session has expired. Please log in again.');
          navigate('/login');
        } else {
          setError('Failed to load dashboard statistics. Please try again later.');
          toast.error('Failed to load dashboard statistics.');
        }
      } finally {
        setIsLoading(false);
      }
    };
    fetchStats();
  }, [navigate]);

  const quickActions = [
    {
      icon: <Users className="w-8 h-8 text-secondary-600" />,
      title: 'User Management',
      description: 'Manage all registered users, roles, and permissions.',
      action: () => navigate('/admin/users'),
      buttonText: 'Manage Users',
      variant: 'primary',
      gradient: 'from-secondary-500 to-secondary-600'
    },
    {
      icon: <CheckCircle className="w-8 h-8 text-yellow-600" />,
      title: 'Verification Requests',
      description: 'Approve or reject new user registration requests.',
      action: () => navigate('/admin/verifications'),
      buttonText: 'Review Requests',
      variant: 'secondary',
      gradient: 'from-yellow-500 to-yellow-600',
      badge: stats?.pendingVerifications > 0 ? stats.pendingVerifications : null
    },
    {
      icon: <Shield className="w-8 h-8 text-blue-600" />,
      title: 'Password Resets',
      description: 'Review and approve password reset requests.',
      action: () => navigate('/admin/password-resets'),
      buttonText: 'Manage Resets',
      variant: 'secondary',
      gradient: 'from-blue-500 to-blue-600'
    }
  ];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className="space-y-8"
    >
      {/* Welcome Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-h1 bg-gradient-to-r from-primary-600 to-secondary-600 bg-clip-text text-transparent">
            Admin Dashboard 🛡️
          </h1>
          <p className="text-body-lg text-gray-600 dark:text-gray-300 mt-2">
            System overview and management
          </p>
        </div>
        <button onClick={() => navigate('/notifications')} className="btn-secondary-modern flex items-center gap-2">
          <Bell className="w-5 h-5" />
          <span>Notifications</span>
          {unreadCount > 0 && (
            <span className="badge badge-pending ml-2">{unreadCount}</span>
          )}
        </button>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <p className="text-red-800 dark:text-red-200">{error}</p>
          <p className="text-sm text-red-600 dark:text-red-300 mt-2">
            Debug: Ensure the backend API is running and your user account has the ADMIN role.
          </p>
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Users */}
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <Users className="w-6 h-6 text-primary-600" />
            </div>
            <span className="badge badge-available">Active</span>
          </div>
          {isLoading ? (
            <div className="h-10 w-20 bg-gray-200 dark:bg-dark-700 rounded-md animate-pulse mt-4" />
          ) : (
            <div className="stat-value">{stats?.totalUsers ?? '0'}</div>
          )}
          <div className="stat-label">Total Users</div>
          <div className="stat-trend-up flex items-center gap-1 mt-1">
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M12 7a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0V8.414l-4.293 4.293a1 1 0 01-1.414 0L8 10.414l-4.293 4.293a1 1 0 01-1.414-1.414l5-5a1 1 0 011.414 0L11 10.586 14.586 7H12z" clipRule="evenodd" />
            </svg>
            Growing
          </div>
        </div>

        {/* Pending Verifications */}
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <CheckCircle className="w-6 h-6 text-yellow-600" />
            </div>
            {stats?.pendingVerifications > 0 && (
              <span className="badge badge-pending">Pending</span>
            )}
          </div>
          {isLoading ? (
            <div className="h-10 w-20 bg-gray-200 dark:bg-dark-700 rounded-md animate-pulse mt-4" />
          ) : (
            <div className="stat-value">{stats?.pendingVerifications ?? '0'}</div>
          )}
          <div className="stat-label">Pending Verifications</div>
          {stats?.pendingVerifications > 0 && (
            <div className="text-xs text-yellow-600 dark:text-yellow-400 mt-1">
              Requires attention
            </div>
          )}
        </div>

        {/* Occupied Rooms */}
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <DoorOpen className="w-6 h-6 text-green-600" />
            </div>
            <span className="badge badge-occupied">In Use</span>
          </div>
          {isLoading ? (
            <div className="h-10 w-20 bg-gray-200 dark:bg-dark-700 rounded-md animate-pulse mt-4" />
          ) : (
            <div className="stat-value">{stats?.occupiedRooms ?? '0'}</div>
          )}
          <div className="stat-label">Occupied Rooms</div>
          <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Currently in use
          </div>
        </div>

        {/* System Health */}
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <BarChart2 className="w-6 h-6 text-blue-600" />
            </div>
            <span className="badge badge-available">Healthy</span>
          </div>
          {isLoading ? (
            <div className="h-10 w-20 bg-gray-200 dark:bg-dark-700 rounded-md animate-pulse mt-4" />
          ) : (
            <div className="stat-value">{stats?.systemHealth ?? '100'}%</div>
          )}
          <div className="stat-label">System Health</div>
          <div className="stat-trend-up flex items-center gap-1 mt-1">
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M12 7a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0V8.414l-4.293 4.293a1 1 0 01-1.414 0L8 10.414l-4.293 4.293a1 1 0 01-1.414-1.414l5-5a1 1 0 011.414 0L11 10.586 14.586 7H12z" clipRule="evenodd" />
            </svg>
            Optimal
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div>
        <div className="section-header">
          <h2 className="section-title">
            <UserCheck className="w-6 h-6 text-primary-600" />
            Quick Actions
          </h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {quickActions.map((action, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: index * 0.1 }}
            >
              <div className="card-interactive group">
                <div className="p-6 flex flex-col items-center text-center">
                  {/* Icon with gradient background */}
                  <div className={`icon-container-lg mb-4 bg-gradient-to-br ${action.gradient} bg-opacity-10 group-hover:scale-110 transition-transform duration-300 relative`}>
                    {action.icon}
                    {action.badge && (
                      <span className="absolute -top-2 -right-2 w-6 h-6 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center">
                        {action.badge}
                      </span>
                    )}
                  </div>

                  <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                    {action.title}
                  </h3>
                  
                  <p className="text-sm text-gray-600 dark:text-gray-300 mb-6 flex-grow">
                    {action.description}
                  </p>

                  <button
                    onClick={action.action}
                    className={`btn-${action.variant}-modern w-full`}
                  >
                    {action.buttonText}
                  </button>
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>

      {/* Admin Tip */}
      <div className="card-premium p-6 bg-gradient-to-br from-purple-50 to-pink-50 dark:from-purple-900/10 dark:to-pink-900/10 border-purple-200 dark:border-purple-800">
        <div className="flex items-start gap-4">
          <div className="icon-container flex-shrink-0">
            <svg className="w-6 h-6 text-purple-600" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
            </svg>
          </div>
          <div className="flex-grow">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              🛡️ Admin Tip
            </h3>
            <p className="text-gray-700 dark:text-gray-300">
              Regularly review pending verifications and password reset requests to ensure smooth user onboarding and account recovery. Timely approvals improve user satisfaction.
            </p>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default AdminDashboard;
