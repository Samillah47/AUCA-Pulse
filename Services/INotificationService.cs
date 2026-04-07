using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationDto dto);
        Task<NotificationResponse?> GetNotificationByIdAsync(int id);
        Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(int userId);
        Task<List<NotificationResponse>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
