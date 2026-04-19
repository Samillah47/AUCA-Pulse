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

        public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
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
