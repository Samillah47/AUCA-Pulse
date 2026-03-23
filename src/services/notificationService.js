import api from './mockApi';

const notificationService = {
  // Get all notifications for a user
  getNotifications: async (userId) => {
    if (!userId) throw new Error('User ID is required');
    const response = await api.get(`/notifications?userId=${userId}`);
    return response.data;
  },

  // Get unread count for a user
  getUnreadCount: async (userId) => {
    if (!userId) throw new Error('User ID is required');
    const response = await api.get(`/notifications/unread-count?userId=${userId}`);
    return response.data.count || 0; // Extract count from response
  },

  // Mark all as read for a user
  markAllAsRead: async (userId) => {
    if (!userId) throw new Error('User ID is required');
    const response = await api.put(`/notifications/read/all?userId=${userId}`);
    return response.data;
  },

  // Mark a single notification as read
  markAsRead: async (notificationId, userId) => {
    if (!userId) throw new Error('User ID is required');
    const response = await api.put(`/notifications/${notificationId}/read?userId=${userId}`);
    return response.data;
  }
};

export default notificationService;
