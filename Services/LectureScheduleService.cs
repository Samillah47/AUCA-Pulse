using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class LectureScheduleService : ILectureScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LectureScheduleService> _logger;
        private readonly INotificationService _notificationService;

        public LectureScheduleService(
            ApplicationDbContext context,
            ILogger<LectureScheduleService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

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

        public async Task<LectureScheduleResponse> CreateScheduleAsync(int lecturerId, CreateLectureScheduleDto request)
        {
            // Verify lecturer exists
            var lecturer = await _context.Users.FindAsync(lecturerId);
            if (lecturer == null)
            {
                throw new Exception("Lecturer not found");
            }

            // Verify semester exists if provided
            if (request.SemesterId.HasValue)
            {
                var semester = await _context.Semesters.FindAsync(request.SemesterId.Value);
                if (semester == null)
                {
                    throw new Exception("Semester not found");
                }
            }

            // Check for time conflicts with lecturer's schedule
            var lecturerConflict = await _context.LectureSchedules
                .Where(s => s.LecturerId == lecturerId 
                    && s.DayOfWeek == request.DayOfWeek
                    && ((s.StartTime < request.EndTime && s.EndTime > request.StartTime)))
                .AnyAsync();

            if (lecturerConflict)
            {
                throw new Exception("Lecturer already has a class scheduled at this time");
            }

            var schedule = new LectureSchedule
            {
                LecturerId = lecturerId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CourseName = request.CourseName,
                CourseCode = request.CourseCode,
                RoomNumber = request.RoomNumber,
                SemesterId = request.SemesterId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LectureSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return await GetScheduleByIdAsync(schedule.Id) ?? throw new Exception("Failed to create schedule");
        }

        public async Task<LectureScheduleResponse?> GetScheduleByIdAsync(int scheduleId)
        {
            var schedule = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            return schedule == null ? null : MapToResponse(schedule);
        }

        public async Task<List<LectureScheduleResponse>> GetAllSchedulesAsync()
        {
            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task<List<LectureScheduleResponse>> GetSchedulesByLecturerIdAsync(int lecturerId)
        {
            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .Where(s => s.LecturerId == lecturerId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task<List<LectureScheduleResponse>> GetSchedulesBySemesterIdAsync(int semesterId)
        {
            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .Where(s => s.SemesterId == semesterId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task<List<LectureScheduleResponse>> GetSchedulesByDayAsync(string dayOfWeek)
        {
            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .Where(s => s.DayOfWeek == dayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task<List<LectureScheduleResponse>> GetSchedulesByRoomAsync(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return new();

            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .Where(s => s.RoomNumber == roomNumber)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        public async Task<LectureScheduleResponse?> UpdateScheduleAsync(int scheduleId, CreateLectureScheduleDto request)
        {
            var schedule = await _context.LectureSchedules.FindAsync(scheduleId);
            if (schedule == null) return null;

            // Verify semester exists if changed
            if (request.SemesterId.HasValue && schedule.SemesterId != request.SemesterId)
            {
                var semester = await _context.Semesters.FindAsync(request.SemesterId.Value);
                if (semester == null)
                {
                    throw new Exception("Semester not found");
                }
            }

            // Check for conflicts (excluding current schedule)
            var lecturerConflict = await _context.LectureSchedules
                .Where(s => s.Id != scheduleId
                    && s.LecturerId == schedule.LecturerId
                    && s.DayOfWeek == request.DayOfWeek
                    && ((s.StartTime < request.EndTime && s.EndTime > request.StartTime)))
                .AnyAsync();

            if (lecturerConflict)
            {
                throw new Exception("Lecturer already has a class scheduled at this time");
            }

            schedule.DayOfWeek = request.DayOfWeek;
            schedule.StartTime = request.StartTime;
            schedule.EndTime = request.EndTime;
            schedule.CourseName = request.CourseName;
            schedule.CourseCode = request.CourseCode;
            schedule.RoomNumber = request.RoomNumber;
            schedule.SemesterId = request.SemesterId;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetScheduleByIdAsync(scheduleId);
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.LectureSchedules.FindAsync(scheduleId);
            if (schedule == null) return false;

            _context.LectureSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LectureScheduleResponse?> CancelForTodayAsync(int scheduleId, int lecturerId, string? reason)
        {
            var schedule = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null) return null;
            if (schedule.LecturerId != lecturerId)
                throw new UnauthorizedAccessException("You can only cancel your own classes.");

            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            schedule.CancelledOn = today;
            schedule.CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            schedule.UpdatedAt = DateTime.UtcNow;

            // If the lecturer was holding the room, free it right now so
            // everyone else can book it.
            if (!string.IsNullOrWhiteSpace(schedule.RoomNumber))
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == schedule.RoomNumber
                                           && r.CurrentLecturerId == lecturerId);
                if (room != null)
                {
                    room.Status = RoomStatus.AVAILABLE;
                    room.CurrentLecturerId = null;
                    room.OccupiedAt = null;
                    room.OccupiedUntil = null;
                    room.CourseInfo = null;
                    room.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Alert admins so they know the room was freed and the class is off today
            var lecturerName = schedule.Lecturer?.Name ?? "A lecturer";
            var course = !string.IsNullOrWhiteSpace(schedule.CourseCode)
                ? $"{schedule.CourseCode} {schedule.CourseName}".Trim()
                : (schedule.CourseName ?? "a class");
            var timeLabel = $"{schedule.StartTime:hh\\:mm}-{schedule.EndTime:hh\\:mm}";
            var roomPart = string.IsNullOrWhiteSpace(schedule.RoomNumber) ? "" : $" in {schedule.RoomNumber}";
            var reasonPart = string.IsNullOrWhiteSpace(schedule.CancellationReason) ? "" : $" Reason: {schedule.CancellationReason}.";
            await NotifyAdminsAsync(
                title: $"Class cancelled today: {course}",
                message: $"{lecturerName} cancelled {course} ({timeLabel}{roomPart}) for today.{reasonPart}",
                link: $"/Lecturers/{lecturerId}",
                type: NotificationType.WARNING);

            return MapToResponse(schedule);
        }

        public async Task<LectureScheduleResponse?> ReinstateForTodayAsync(int scheduleId, int lecturerId)
        {
            var schedule = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null) return null;
            if (schedule.LecturerId != lecturerId)
                throw new UnauthorizedAccessException("You can only manage your own classes.");

            schedule.CancelledOn = null;
            schedule.CancellationReason = null;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var lecturerName = schedule.Lecturer?.Name ?? "A lecturer";
            var course = !string.IsNullOrWhiteSpace(schedule.CourseCode)
                ? $"{schedule.CourseCode} {schedule.CourseName}".Trim()
                : (schedule.CourseName ?? "a class");
            await NotifyAdminsAsync(
                title: $"Class reinstated today: {course}",
                message: $"{lecturerName} reinstated {course} for today.",
                link: $"/Lecturers/{lecturerId}",
                type: NotificationType.INFO);

            return MapToResponse(schedule);
        }

        public async Task<List<LectureScheduleResponse>> GetCancelledClassesAsync(DateTime? date = null)
        {
            // Normalize to a UTC day window. cancelled_on is timestamptz, so we
            // compare against [startOfDay, startOfNextDay) in UTC instead of
            // calling .Date in the LINQ query — Npgsql can't translate that.
            var baseDay = (date ?? DateTime.UtcNow).Date;
            var startUtc = DateTime.SpecifyKind(baseDay, DateTimeKind.Utc);
            var endUtc = startUtc.AddDays(1);

            var schedules = await _context.LectureSchedules
                .Include(s => s.Lecturer)
                .Include(s => s.Semester)
                .Where(s => s.CancelledOn != null
                         && s.CancelledOn >= startUtc
                         && s.CancelledOn < endUtc)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(MapToResponse).ToList();
        }

        private LectureScheduleResponse MapToResponse(LectureSchedule schedule)
        {
            return new LectureScheduleResponse
            {
                Id = schedule.Id,
                LecturerId = schedule.LecturerId,
                LecturerName = schedule.Lecturer?.Name ?? "Unknown",
                LecturerEmail = schedule.Lecturer?.Email ?? "Unknown",
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                CourseName = schedule.CourseName,
                CourseCode = schedule.CourseCode,
                RoomNumber = schedule.RoomNumber,
                GroupName = schedule.GroupName,
                SemesterId = schedule.SemesterId,
                CancelledOn = schedule.CancelledOn,
                CancellationReason = schedule.CancellationReason,
                SemesterName = schedule.Semester?.Name,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            };
        }
    }
}
