using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> CreateAppointmentAsync(int studentUserId, CreateAppointmentDto request);
        Task<List<AppointmentResponse>> GetAppointmentsByStaffAsync(int staffUserId);
        Task<List<AppointmentResponse>> GetAppointmentsByStudentAsync(int studentUserId);
        Task<AppointmentResponse?> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto request, int? actingStaffUserId = null);
        Task<bool> DeleteAppointmentAsync(int appointmentId);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;
        private readonly INotificationService _notificationService;

        public AppointmentService(
            ApplicationDbContext context,
            ILogger<AppointmentService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<AppointmentResponse> CreateAppointmentAsync(int studentUserId, CreateAppointmentDto request)
        {
            var student = await _context.Users.FindAsync(studentUserId);
            if (student == null)
                throw new Exception("Student not found");

            var staff = await _context.Users.FindAsync(request.StaffUserId);
            if (staff == null)
                throw new Exception("Staff member not found");

            var normalizedAppointmentDate = NormalizeToUtc(request.AppointmentDate);

            var duplicateExists = await _context.Appointments.AnyAsync(a =>
                a.StudentUserId == studentUserId &&
                a.StaffUserId == request.StaffUserId &&
                a.AppointmentDate == normalizedAppointmentDate);

            if (duplicateExists)
            {
                throw new InvalidOperationException("You already requested this exact appointment slot with this staff member.");
            }

            var appointment = new Appointment
            {
                StudentUserId = studentUserId,
                StaffUserId = request.StaffUserId,
                AppointmentDate = normalizedAppointmentDate,
                Reason = request.Reason,
                Status = AppointmentStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Appointment created: Student {studentUserId} requested appointment with Staff {request.StaffUserId}");

            // Notify the staff / lecturer that a new appointment request has arrived
            try
            {
                var localWhen = normalizedAppointmentDate.ToLocalTime().ToString("MMM d, HH:mm");
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = request.StaffUserId,
                    Title = "New appointment request",
                    Message = $"{student.Name} requested an appointment on {localWhen}. " +
                              (string.IsNullOrWhiteSpace(request.Reason) ? "" : $"Reason: {request.Reason}"),
                    Type = NotificationType.INFO
                });
            }
            catch (Exception ex)
            {
                // Don't fail the booking if the notification write fails
                _logger.LogWarning(ex, "Appointment {AppointmentId} saved but new-request notification to user {StaffId} failed",
                    appointment.Id, request.StaffUserId);
            }

            return await GetAppointmentResponseAsync(appointment);
        }

        public async Task<List<AppointmentResponse>> GetAppointmentsByStaffAsync(int staffUserId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.StudentUser)
                .Include(a => a.StaffUser)
                .Where(a => a.StaffUserId == staffUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return appointments.Select(MapToResponse).ToList();
        }

        public async Task<List<AppointmentResponse>> GetAppointmentsByStudentAsync(int studentUserId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.StudentUser)
                .Include(a => a.StaffUser)
                .Where(a => a.StudentUserId == studentUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return appointments.Select(MapToResponse).ToList();
        }

        public async Task<AppointmentResponse?> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto request, int? actingStaffUserId = null)
        {
            var appointment = await _context.Appointments
                .Include(a => a.StudentUser)
                .Include(a => a.StaffUser)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return null;

            // Staff can only update appointments assigned to them.
            if (actingStaffUserId.HasValue && appointment.StaffUserId != actingStaffUserId.Value)
            {
                throw new UnauthorizedAccessException("You can only update appointments assigned to you.");
            }

            if (Enum.TryParse<AppointmentStatus>(request.Status, true, out var status))
            {
                appointment.Status = status;

                if (status == AppointmentStatus.REJECTED && !string.IsNullOrWhiteSpace(request.Reason))
                {
                    var note = $"Staff response: {request.Reason.Trim()}";
                    appointment.Reason = string.IsNullOrWhiteSpace(appointment.Reason)
                        ? note
                        : $"{appointment.Reason}\n\n{note}";
                }

                appointment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Appointment {appointmentId} status updated to {status}");

                // Notify the student that their appointment was reviewed
                try
                {
                    var localWhen = appointment.AppointmentDate.ToLocalTime().ToString("MMM d, HH:mm");
                    var staffName = appointment.StaffUser?.Name ?? "Staff member";
                    var title = status switch
                    {
                        AppointmentStatus.APPROVED  => "Appointment approved",
                        AppointmentStatus.REJECTED  => "Appointment rejected",
                        AppointmentStatus.COMPLETED => "Appointment completed",
                        AppointmentStatus.CANCELLED => "Appointment cancelled",
                        _ => $"Appointment {status}"
                    };
                    var body = status switch
                    {
                        AppointmentStatus.APPROVED =>
                            $"{staffName} approved your appointment on {localWhen}.",
                        AppointmentStatus.REJECTED =>
                            $"{staffName} rejected your appointment on {localWhen}." +
                            (!string.IsNullOrWhiteSpace(request.Reason) ? $" Reason: {request.Reason.Trim()}" : ""),
                        AppointmentStatus.COMPLETED =>
                            $"Your appointment with {staffName} on {localWhen} is marked as completed.",
                        AppointmentStatus.CANCELLED =>
                            $"Your appointment with {staffName} on {localWhen} was cancelled.",
                        _ => $"Your appointment with {staffName} on {localWhen} is now {status}."
                    };
                    var type = status switch
                    {
                        AppointmentStatus.APPROVED  => NotificationType.APPROVAL,
                        AppointmentStatus.REJECTED  => NotificationType.REJECTION,
                        AppointmentStatus.COMPLETED => NotificationType.SUCCESS,
                        _ => NotificationType.INFO
                    };
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = appointment.StudentUserId,
                        Title = title,
                        Message = body,
                        Type = type
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Appointment {AppointmentId} updated but status-change notification failed",
                        appointmentId);
                }

                return MapToResponse(appointment);
            }

            throw new Exception("Invalid appointment status");
        }

        public async Task<bool> DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Appointment {appointmentId} deleted");
            return true;
        }

        // PostgreSQL timestamp with time zone expects UTC values.
        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
            };
        }

        private async Task<AppointmentResponse> GetAppointmentResponseAsync(Appointment appointment)
        {
            var student = await _context.Users.FindAsync(appointment.StudentUserId);
            var staff = await _context.Users.FindAsync(appointment.StaffUserId);

            return new AppointmentResponse
            {
                Id = appointment.Id,
                StudentUserId = appointment.StudentUserId,
                StudentName = student?.Name ?? "Unknown",
                StudentEmail = student?.Email ?? "Unknown",
                StaffUserId = appointment.StaffUserId,
                StaffName = staff?.Name ?? "Unknown",
                StaffEmail = staff?.Email ?? "Unknown",
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        private AppointmentResponse MapToResponse(Appointment appointment)
        {
            return new AppointmentResponse
            {
                Id = appointment.Id,
                StudentUserId = appointment.StudentUserId,
                StudentName = appointment.StudentUser?.Name ?? "Unknown",
                StudentEmail = appointment.StudentUser?.Email ?? "Unknown",
                StaffUserId = appointment.StaffUserId,
                StaffName = appointment.StaffUser?.Name ?? "Unknown",
                StaffEmail = appointment.StaffUser?.Email ?? "Unknown",
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }
    }
}
