using AUCAPulse.Data;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    /// <summary>
    /// Computes a lecturer's current whereabouts by merging three sources, in priority:
    ///   1. Active LectureSchedule entry for today's day-of-week and current time → IN_CLASS
    ///   2. Latest LecturerStatus record (if within validity window) → IN_MEETING / AWAY / AVAILABLE
    ///   3. Assigned Office availability → IN_OFFICE when OPEN
    ///   4. Fallback → AVAILABLE for approved lecturers, UNKNOWN otherwise
    /// </summary>
    public class LecturerLocationService : ILecturerLocationService
    {
        private readonly ApplicationDbContext _context;

        public LecturerLocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LecturerLocationResponse?> GetForLecturerAsync(int lecturerId)
        {
            var lecturer = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == lecturerId);

            if (lecturer == null || lecturer.Role?.RoleName != "LECTURER")
                return null;

            return await ComputeLocationAsync(lecturer);
        }

        public async Task<List<LecturerLocationResponse>> GetForAllLecturersAsync()
        {
            var lecturers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "LECTURER"
                         && u.Status == UserStatus.APPROVED)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var results = new List<LecturerLocationResponse>();
            foreach (var lecturer in lecturers)
            {
                var location = await ComputeLocationAsync(lecturer);
                if (location != null) results.Add(location);
            }
            return results;
        }

        private async Task<LecturerLocationResponse> ComputeLocationAsync(User lecturer)
        {
            var result = new LecturerLocationResponse
            {
                LecturerId = lecturer.Id,
                LecturerName = lecturer.Name,
                LecturerEmail = lecturer.Email,
                Department = lecturer.Department,
                Status = "UNKNOWN",
                LocationLabel = "No status available",
                Source = "none"
            };

            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentDay = now.DayOfWeek.ToString().ToUpper(); // MONDAY, TUESDAY, ...
            var currentTime = now.TimeOfDay;

            // 1. Check active LectureSchedule for right now.
            //    A weekly schedule only counts when we're inside its semester
            //    window (StartDate <= today <= EndDate). Otherwise the schedule
            //    is historical or future and the lecturer is NOT actually in
            //    class right now — even if today happens to be the same weekday.
            var activeSchedule = await _context.LectureSchedules
                .Include(s => s.Semester)
                .Where(s => s.LecturerId == lecturer.Id
                         && s.DayOfWeek.ToUpper() == currentDay
                         && s.StartTime <= currentTime
                         && s.EndTime >= currentTime
                         && s.Semester != null
                         && s.Semester.StartDate <= today
                         && s.Semester.EndDate >= today)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (activeSchedule != null)
            {
                result.Status = "IN_CLASS";
                result.LocationLabel = string.IsNullOrWhiteSpace(activeSchedule.RoomNumber)
                    ? "In class"
                    : $"Room {activeSchedule.RoomNumber}";
                if (!string.IsNullOrWhiteSpace(activeSchedule.CourseCode) || !string.IsNullOrWhiteSpace(activeSchedule.CourseName))
                {
                    result.CourseInfo = $"{activeSchedule.CourseCode} — {activeSchedule.CourseName}".Trim(' ', '—');
                }
                result.UntilTime = activeSchedule.EndTime.ToString(@"hh\:mm");
                result.Source = "schedule";
                return result;
            }

            // 2. Latest LecturerStatus within validity window (if any)
            var latestStatus = await _context.LecturerStatuses
                .Where(ls => ls.LecturerId == lecturer.Id)
                .OrderByDescending(ls => ls.CreatedAt)
                .FirstOrDefaultAsync();

            var statusIsValid = latestStatus != null
                && (latestStatus.AvailableUntil == null || latestStatus.AvailableUntil >= now)
                && (latestStatus.AvailableFrom == null || latestStatus.AvailableFrom <= now);

            if (latestStatus != null && statusIsValid)
            {
                result.Status = latestStatus.Status.ToString();
                result.LocationLabel = !string.IsNullOrWhiteSpace(latestStatus.LocationDescription)
                    ? latestStatus.LocationDescription!
                    : latestStatus.Status.ToString().Replace('_', ' ');
                if (latestStatus.AvailableUntil.HasValue)
                    result.UntilTime = latestStatus.AvailableUntil.Value.ToLocalTime().ToString("HH:mm");
                result.Source = "status";
                return result;
            }

            // 3. Office availability
            var office = await _context.Offices
                .FirstOrDefaultAsync(o => o.StaffUserId == lecturer.Id);

            if (office != null && office.AvailabilityStatus == AvailabilityStatus.OPEN)
            {
                result.Status = "IN_OFFICE";
                result.LocationLabel = !string.IsNullOrWhiteSpace(office.OfficeName)
                    ? $"{office.OfficeName} ({office.OfficeNumber})"
                    : $"Office {office.OfficeNumber}";
                if (!string.IsNullOrWhiteSpace(office.Building))
                    result.LocationLabel += $", {office.Building}";
                result.Source = "office";
                return result;
            }

            // 4. Fallback: approved lecturers with no signal are assumed to be
            //    available rather than "unknown" — they just haven't set a
            //    status manually and have no class or open office this minute.
            if (lecturer.Status == UserStatus.APPROVED)
            {
                result.Status = "AVAILABLE";
                result.LocationLabel = "Available";
                result.Source = "default";
            }
            return result;
        }
    }
}
