using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return Ok(new { message = "Notification created successfully", data = notification });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(new { message = "Notification not found" });

            return Ok(new { data = notification });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotificationsByUserId(int userId)
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(new { data = notifications });
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUnreadNotificationsByUserId(int userId)
        {
            var notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return Ok(new { data = notifications });
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result)
                return NotFound(new { message = "Notification not found" });

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            if (!result)
                return NotFound(new { message = "Notification not found" });

            return Ok(new { message = "Notification deleted successfully" });
        }
    }
}
