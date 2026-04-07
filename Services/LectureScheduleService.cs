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

        public LectureScheduleService(ApplicationDbContext context, ILogger<LectureScheduleService> logger)
        {
            _context = context;
            _logger = logger;
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
                SemesterId = schedule.SemesterId,
                SemesterName = schedule.Semester?.Name,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            };
        }
    }
}
