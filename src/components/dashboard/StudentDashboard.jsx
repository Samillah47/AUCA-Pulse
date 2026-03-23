import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import Button from '../common/Button';
import Card from '../common/Card';
import { DoorOpen, Users, Search, BookOpen, Bell, Calendar, TrendingUp } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import { useQuery } from '@tanstack/react-query';
import { notificationService } from '../../services';


const StudentDashboard = () => {
  const navigate = useNavigate();
  const { user } = useAuthStore();

  // Fetch unread notification count
  const { data: unreadCount = 0 } = useQuery({
    queryKey: ['unreadNotifications', user?.id],
    queryFn: () => notificationService.getUnreadCount(user?.id),
    enabled: !!user?.id,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  const quickActions = [
    {
      icon: <DoorOpen className="w-8 h-8 text-primary-600" />,
      title: 'Find a Room',
      description: 'Check the status of any room on campus in real-time.',
      action: () => navigate('/rooms'),
      buttonText: 'Search Rooms',
      variant: 'primary',
      gradient: 'from-primary-500 to-primary-600'
    },
    {
      icon: <Users className="w-8 h-8 text-secondary-600" />,
      title: 'Find a Lecturer',
      description: 'See if a lecturer is available in their office.',
      action: () => navigate('/lecturers'),
      buttonText: 'Search Lecturers',
      variant: 'secondary',
      gradient: 'from-secondary-500 to-secondary-600'
    },
    {
      icon: <Calendar className="w-8 h-8 text-accent-600" />,
      title: 'View Schedules',
      description: 'Check lecturer schedules and office hours.',
      action: () => navigate('/schedule'),
      buttonText: 'View Schedules',
      variant: 'secondary',
      gradient: 'from-accent-500 to-accent-600'
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
            Welcome back, {user?.name}! 👋
          </h1>
          <p className="text-body-lg text-gray-600 dark:text-gray-300 mt-2">
            Here's what's happening on campus today
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

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <DoorOpen className="w-6 h-6 text-primary-600" />
            </div>
            <span className="badge badge-available">Live</span>
          </div>
          <div className="stat-value">24</div>
          <div className="stat-label">Available Rooms</div>
          <div className="stat-trend-up flex items-center gap-1 mt-1">
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M12 7a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0V8.414l-4.293 4.293a1 1 0 01-1.414 0L8 10.414l-4.293 4.293a1 1 0 01-1.414-1.414l5-5a1 1 0 011.414 0L11 10.586 14.586 7H12z" clipRule="evenodd" />
            </svg>
            +3 from yesterday
          </div>
        </div>

        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <Users className="w-6 h-6 text-secondary-600" />
            </div>
            <span className="badge badge-available">Active</span>
          </div>
          <div className="stat-value">18</div>
          <div className="stat-label">Lecturers Available</div>
          <div className="stat-trend-up flex items-center gap-1 mt-1">
            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M12 7a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0V8.414l-4.293 4.293a1 1 0 01-1.414 0L8 10.414l-4.293 4.293a1 1 0 01-1.414-1.414l5-5a1 1 0 011.414 0L11 10.586 14.586 7H12z" clipRule="evenodd" />
            </svg>
            +2 from yesterday
          </div>
        </div>

        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="icon-container">
              <BookOpen className="w-6 h-6 text-accent-600" />
            </div>
            <span className="badge badge-teaching">Ongoing</span>
          </div>
          <div className="stat-value">12</div>
          <div className="stat-label">Classes in Session</div>
          <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Across campus
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div>
        <div className="section-header">
          <h2 className="section-title">
            <Search className="w-6 h-6 text-primary-600" />
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
                  <div className={`icon-container-lg mb-4 bg-gradient-to-br ${action.gradient} bg-opacity-10 group-hover:scale-110 transition-transform duration-300`}>
                    {action.icon}
                  </div>

                  <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                    {action.title}
                  </h3>
                  
                  <p className="text-sm text-gray-600 dark:text-gray-300 mb-6 flex-grow">
                    {action.description}
                  </p>

                  <button
                    onClick={action.action}
                    className={`btn-${action.variant}-modern w-full flex items-center justify-center gap-2`}
                  >
                    <Search className="w-4 h-4" />
                    {action.buttonText}
                  </button>
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>

      {/* Campus Tips */}
      <div className="card-premium p-6 bg-gradient-to-br from-primary-50 to-secondary-50 dark:from-primary-900/10 dark:to-secondary-900/10 border-primary-200 dark:border-primary-800">
        <div className="flex items-start gap-4">
          <div className="icon-container flex-shrink-0">
            <svg className="w-6 h-6 text-primary-600" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
            </svg>
          </div>
          <div className="flex-grow">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              💡 Campus Tip
            </h3>
            <p className="text-gray-700 dark:text-gray-300">
              Most lecturers are available in their offices between 2:00 PM - 4:00 PM. 
              Check the lecturer directory for specific office hours and locations.
            </p>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default StudentDashboard;
