import React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import notificationService from '../services/notificationService';
import LoadingSpinner from '../components/common/LoadingSpinner';
import EmptyState from '../components/common/EmptyState';
import { Bell, Archive } from 'lucide-react';
import { formatRelativeTime, cn } from '../utils/helpers';
import Button from '../components/common/Button';
import { toast } from 'react-hot-toast';
import { useAuthStore } from '../store/authStore';

const NotificationItem = ({ notification, onMarkAsRead }) => {
    const navigate = useNavigate();

    // Handle navigation if a link is provided in the notification
    const handleNotificationClick = () => {
        if (notification.link) {
            navigate(notification.link);
        }
    };

    return (
        <div
            className={cn(
                'p-4 flex items-start gap-4 border-b dark:border-dark-700',
                !notification.isRead && 'bg-primary-50 dark:bg-primary-900/10',
                notification.link && 'cursor-pointer hover:bg-gray-50 dark:hover:bg-dark-700/50 transition-colors'
            )}
            onClick={handleNotificationClick}
        >
            <div className="w-8 h-8 bg-primary-100 dark:bg-primary-900/20 rounded-full flex items-center justify-center flex-shrink-0">
                <Bell className="w-5 h-5 text-primary-600 dark:text-primary-400" />
            </div>
            <div className="flex-1">
                <p className="text-gray-800 dark:text-gray-200">{notification.message}</p>
                <span className="text-sm text-gray-500 dark:text-gray-400">
                    {formatRelativeTime(notification.createdAt)}
                </span>
            </div>
            {!notification.isRead && (
                <Button 
                    size="sm" 
                    variant="ghost" 
                    onClick={(e) => {
                        e.stopPropagation(); // Prevent navigation when marking as read
                        onMarkAsRead(notification.id);
                    }}
                >
                    Mark as read
                </Button>
            )}
        </div>
    );
};


const Notifications = () => {
    const queryClient = useQueryClient();
    const user = useAuthStore((state) => state.user);
    const userId = user?.userId;

    const { data: notifications, isLoading, error } = useQuery({
        queryKey: ['notifications', userId],
        queryFn: () => notificationService.getNotifications(),
        enabled: !!userId
    });

    const markAsReadMutation = useMutation({
        mutationFn: (notificationId) => notificationService.markAsRead(notificationId),
        onSuccess: () => {
            queryClient.invalidateQueries(['notifications', userId]);
        }
    });

    const markAllAsReadMutation = useMutation({
        mutationFn: () => notificationService.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries(['notifications', userId]);
            toast.success('All notifications marked as read.');
        }
    });

    const handleMarkAsRead = (id) => {
        markAsReadMutation.mutate(id);
    }

    const handleMarkAllAsRead = () => {
        markAllAsReadMutation.mutate();
    }

    // Determine if there are any unread notifications.
    const hasUnreadNotifications = notifications?.some(n => !n.isRead);

  return (
    <div className="bg-white dark:bg-dark-800 rounded-lg shadow-md">
       <div className="p-4 border-b dark:border-dark-700 flex justify-between items-center">
        <h1 className="text-xl font-bold">Notifications</h1>
        <Button 
            variant="outline" 
            onClick={handleMarkAllAsRead}
            disabled={markAllAsReadMutation.isLoading || !hasUnreadNotifications}
        >
            Mark All as Read
        </Button>
       </div>

       {isLoading && <div className="p-8 flex justify-center"><LoadingSpinner /></div>}
       {!isLoading && notifications?.length === 0 && (
           <EmptyState title="No Notifications" message="You're all caught up!" icon={Archive} />
       )}
       {!isLoading && error && notifications?.length > 0 && ( // Display error only if notifications are NOT empty (i.e., some loaded but others failed)
           <EmptyState title="Error" message={error.message} icon={Bell} />
       )}

       {!isLoading && !error && notifications?.length > 0 && (
           <div>
            {notifications?.map(notification => (
                <NotificationItem key={notification.id} notification={notification} onMarkAsRead={handleMarkAsRead} />
            ))}
           </div>
       )}
    </div>
  );
};

export default Notifications;
