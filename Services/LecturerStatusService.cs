using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class LecturerStatusService : ILecturerStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LecturerStatusService> _logger;
        private readonly INotificationService _notificationService;

        public LecturerStatusService(
            ApplicationDbContext context,
            ILogger<LecturerStatusService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>Broadcast a notification to every ADMIN user.</summary>
        private async Task NotifyAdminsAsync(string title, string message, string link, NotificationType type = NotificationType.INFO)
        {
            try
            {
                var adminIds = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == "ADMIN"
                             && u.Status == UserStatus.APPROVED)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var adminId in adminIds)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = adminId,
                        Title = title,
                        Message = message,
                        Type = type,
                        Link = link
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast admin notification: {Title}", title);
            }
        }

        public async Task<LecturerStatusResponse> CreateStatusAsync(int lecturerId, CreateLecturerStatusDto request)
        {
            // Verify lecturer exists
            var lecturer = await _context.Users.FindAsync(lecturerId);
            if (lecturer == null)
            {
                throw new Exception("Lecturer not found");
            }

            var lecturerStatus = new LecturerStatus
            {
                LecturerId = lecturerId,
                Status = request.Status,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.LecturerStatuses.Add(lecturerStatus);
            await _context.SaveChangesAsync();

            // Let admins know the lecturer just changed their status
            var statusLabel = request.Status.ToString().Replace('_', ' ');
            var note = string.IsNullOrWhiteSpace(request.Notes) ? "" : $" — {request.Notes.Trim()}";
            await NotifyAdminsAsync(
                title: $"{lecturer.Name} is now {statusLabel}",
                message: $"{lecturer.Name} updated their status to {statusLabel}{note}.",
                link: $"/Lecturers/{lecturerId}",
                type: request.Status == Status.UNAVAILABLE ? NotificationType.WARNING : NotificationType.INFO);

            return await GetStatusByIdAsync(lecturerStatus.Id) ?? throw new Exception("Failed to create status");
        }

        public async Task<LecturerStatusResponse?> GetStatusByIdAsync(int statusId)
        {
            var status = await _context.LecturerStatuses
                .Include(s => s.Lecturer)
                .FirstOrDefaultAsync(s => s.Id == statusId);

            return status == null ? null : MapToResponse(status);
        }

        public async Task<List<LecturerStatusResponse>> GetAllStatusesAsync()
        {
            var statuses = await _context.LecturerStatuses
                .Include(s => s.Lecturer)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return statuses.Select(MapToResponse).ToList();
        }

        public async Task<List<LecturerStatusResponse>> GetStatusesByLecturerIdAsync(int lecturerId)
        {
            var statuses = await _context.LecturerStatuses
                .Include(s => s.Lecturer)
                .Where(s => s.LecturerId == lecturerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return statuses.Select(MapToResponse).ToList();
        }

        public async Task<LecturerStatusResponse?> GetCurrentStatusByLecturerIdAsync(int lecturerId)
        {
            var status = await _context.LecturerStatuses
                .Include(s => s.Lecturer)
                .Where(s => s.LecturerId == lecturerId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (status == null)
            {
                var lecturer = await _context.Users.FindAsync(lecturerId);
                if (lecturer != null)
                {
                    return new LecturerStatusResponse
                    {
                        LecturerId = lecturerId,
                        LecturerName = lecturer.Name,
                        LecturerEmail = lecturer.Email,
                        Status = "AVAILABLE",
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }

            return status == null ? null : MapToResponse(status);
        }

        public async Task<List<LecturerStatusResponse>> GetStatusesByStatusTypeAsync(Status status)
        {
            var statuses = await _context.LecturerStatuses
                .Include(s => s.Lecturer)
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return statuses.Select(MapToResponse).ToList();
        }

        public async Task<LecturerStatusResponse?> UpdateStatusAsync(int statusId, CreateLecturerStatusDto request)
        {
            var status = await _context.LecturerStatuses.FindAsync(statusId);
            if (status == null) return null;

            status.Status = request.Status;
            status.Notes = request.Notes;
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetStatusByIdAsync(statusId);
        }

        public async Task<bool> DeleteStatusAsync(int statusId)
        {
            var status = await _context.LecturerStatuses.FindAsync(statusId);
            if (status == null) return false;

            _context.LecturerStatuses.Remove(status);
            await _context.SaveChangesAsync();

            return true;
        }

        private LecturerStatusResponse MapToResponse(LecturerStatus status)
        {
            return new LecturerStatusResponse
            {
                Id = status.Id,
                LecturerId = status.LecturerId,
                LecturerName = status.Lecturer?.Name ?? "Unknown",
                LecturerEmail = status.Lecturer?.Email ?? "Unknown",
                Status = status.Status.ToString(),
                Notes = status.Notes,
                CreatedAt = status.CreatedAt,
                UpdatedAt = status.UpdatedAt
            };
        }
    }
}
