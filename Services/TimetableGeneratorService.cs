using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    /// <summary>
    /// Generates a conflict-free weekly timetable for all course assignments
    /// of a given semester using a Round Robin distribution strategy:
    ///   - Time slots rotate through a circular pointer
    ///   - Rooms rotate through a separate circular pointer
    /// A slot is accepted only if it causes no lecturer conflict and no
    /// room conflict among already-scheduled assignments for the same week.
    ///
    /// Saturday is excluded (AUCA is Adventist, Sabbath is a rest day).
    /// Time slots are ordered by time-of-day FIRST, then day-of-week, so a
    /// small number of assignments spreads across all six teaching days
    /// before piling up on one day.
    /// </summary>
    public class TimetableGeneratorService : ITimetableGeneratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimetableGeneratorService> _logger;

        // Teaching week: Mon-Fri + Sun. Saturday is intentionally excluded.
        private static readonly string[] WeekDays =
        {
            "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SUNDAY"
        };

        public TimetableGeneratorService(
            ApplicationDbContext context,
            ILogger<TimetableGeneratorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TimetableGenerationResult> GenerateAsync(GenerateTimetableDto dto)
        {
            var semester = await _context.Semesters.FindAsync(dto.SemesterId)
                ?? throw new Exception("Semester not found");

            if (dto.StartHour < 0 || dto.EndHour > 24 || dto.StartHour >= dto.EndHour)
                throw new Exception("Invalid daily time window");
            if (dto.SlotDurationMinutes < 30 || dto.SlotDurationMinutes > 240)
                throw new Exception("Slot duration must be between 30 and 240 minutes");

            // Inputs for the Round Robin scheduler
            var assignments = await _context.CourseAssignments
                .Include(ca => ca.Lecturer)
                .Include(ca => ca.Course)
                .Where(ca => ca.SemesterId == dto.SemesterId)
                .OrderBy(ca => ca.CourseId)
                .ThenBy(ca => ca.LecturerId)
                .ToListAsync();

            var rooms = await _context.Rooms
                .Where(r => r.Status == RoomStatus.AVAILABLE
                         || r.Status == RoomStatus.OCCUPIED  // OCCUPIED may be a live class; still allow for weekly schedule
                         || r.Status == RoomStatus.RESERVED)
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (!assignments.Any())
                throw new Exception("No course assignments exist for this semester. Assign courses to lecturers first.");
            if (!rooms.Any())
                throw new Exception("No rooms available for scheduling.");

            // Optionally wipe prior schedules for this semester
            if (dto.ReplaceExisting)
            {
                var old = await _context.LectureSchedules
                    .Where(s => s.SemesterId == dto.SemesterId)
                    .ToListAsync();
                if (old.Any())
                {
                    _context.LectureSchedules.RemoveRange(old);
                    await _context.SaveChangesAsync();
                }
            }

            // Build the ordered list of (day, startTime) slots for the week
            var timeSlots = BuildTimeSlots(dto.StartHour, dto.EndHour, dto.SlotDurationMinutes);
            if (!timeSlots.Any())
                throw new Exception("No time slots could be built from the given parameters.");

            // Round Robin pointers: advance independently of each other
            int timeSlotPointer = 0;
            int roomPointer = 0;

            // Bookkeeping to detect conflicts during generation
            // Key: (DayOfWeek, StartTime). Value: sets of LecturerIds / RoomIds already booked at that slot.
            var bookedLecturers = new Dictionary<(string Day, TimeSpan Start), HashSet<int>>();
            var bookedRooms = new Dictionary<(string Day, TimeSpan Start), HashSet<int>>();

            var result = new TimetableGenerationResult
            {
                SemesterId = semester.Id,
                SemesterName = semester.Name,
                TotalAssignments = assignments.Count
            };

            var newSchedules = new List<LectureSchedule>();

            foreach (var assignment in assignments)
            {
                var placed = false;
                var triedSlots = 0;
                var totalSlots = timeSlots.Count;

                // Try each time slot in Round Robin order until we find one that fits
                while (triedSlots < totalSlots && !placed)
                {
                    var slot = timeSlots[timeSlotPointer];
                    timeSlotPointer = (timeSlotPointer + 1) % totalSlots;
                    triedSlots++;

                    var key = (slot.Day, slot.Start);
                    var lecturersAtSlot = bookedLecturers.GetValueOrDefault(key) ?? new HashSet<int>();
                    if (lecturersAtSlot.Contains(assignment.LecturerId))
                        continue; // lecturer already teaching at this time — try next slot

                    // Try each room in Round Robin order for this slot
                    var roomsAtSlot = bookedRooms.GetValueOrDefault(key) ?? new HashSet<int>();
                    var triedRooms = 0;
                    while (triedRooms < rooms.Count && !placed)
                    {
                        var room = rooms[roomPointer];
                        roomPointer = (roomPointer + 1) % rooms.Count;
                        triedRooms++;

                        if (roomsAtSlot.Contains(room.Id))
                            continue; // room already booked at this time — try next room

                        // Slot + room is free — place the assignment here
                        var endTime = slot.Start.Add(TimeSpan.FromMinutes(dto.SlotDurationMinutes));

                        var schedule = new LectureSchedule
                        {
                            LecturerId = assignment.LecturerId,
                            DayOfWeek = slot.Day,
                            StartTime = slot.Start,
                            EndTime = endTime,
                            CourseCode = assignment.Course?.CourseCode,
                            CourseName = assignment.Course?.CourseName,
                            RoomNumber = room.RoomNumber,
                            SemesterId = semester.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        newSchedules.Add(schedule);

                        // Record the booking
                        if (!bookedLecturers.ContainsKey(key))
                            bookedLecturers[key] = new HashSet<int>();
                        bookedLecturers[key].Add(assignment.LecturerId);

                        if (!bookedRooms.ContainsKey(key))
                            bookedRooms[key] = new HashSet<int>();
                        bookedRooms[key].Add(room.Id);

                        result.Scheduled.Add(new GeneratedScheduleEntry
                        {
                            LecturerId = assignment.LecturerId,
                            LecturerName = assignment.Lecturer?.Name ?? string.Empty,
                            CourseCode = assignment.Course?.CourseCode ?? string.Empty,
                            CourseName = assignment.Course?.CourseName ?? string.Empty,
                            RoomNumber = room.RoomNumber,
                            DayOfWeek = slot.Day,
                            StartTime = slot.Start.ToString(@"hh\:mm"),
                            EndTime = endTime.ToString(@"hh\:mm")
                        });

                        placed = true;
                    }
                }

                if (!placed)
                {
                    result.Unscheduled.Add(new UnscheduledAssignment
                    {
                        AssignmentId = assignment.Id,
                        LecturerName = assignment.Lecturer?.Name ?? string.Empty,
                        CourseCode = assignment.Course?.CourseCode ?? string.Empty,
                        CourseName = assignment.Course?.CourseName ?? string.Empty,
                        Reason = "No conflict-free slot/room combination available. Expand time window, add more rooms, or reduce course load."
                    });
                }
            }

            // Persist generated schedules and attach their IDs to the response
            if (newSchedules.Any())
            {
                _context.LectureSchedules.AddRange(newSchedules);
                await _context.SaveChangesAsync();

                // Map saved IDs back to the result entries (same order)
                for (int i = 0; i < newSchedules.Count && i < result.Scheduled.Count; i++)
                {
                    result.Scheduled[i].ScheduleId = newSchedules[i].Id;
                }
            }

            result.ScheduledCount = result.Scheduled.Count;
            result.UnscheduledCount = result.Unscheduled.Count;

            _logger.LogInformation(
                "Timetable generated for semester {SemesterId}: {Scheduled}/{Total} scheduled, {Unscheduled} unscheduled.",
                semester.Id, result.ScheduledCount, result.TotalAssignments, result.UnscheduledCount);

            return result;
        }

        /// <summary>
        /// Build the ordered list of (day, startTime) slots the Round Robin
        /// pointer will cycle through.
        ///
        /// Crucial detail: the OUTER loop is time-of-day and the INNER loop is
        /// day-of-week. That makes the first few slots [(Mon 08:00), (Tue 08:00),
        /// (Wed 08:00), ..., (Sun 08:00), (Mon 08:50), (Tue 08:50), ...] so a
        /// handful of assignments spread out across all teaching days before
        /// piling onto one day. If we looped day-first we would fill all of
        /// Monday before even trying Tuesday.
        /// </summary>
        private static List<(string Day, TimeSpan Start)> BuildTimeSlots(
            int startHour, int endHour, int slotMinutes)
        {
            var slots = new List<(string, TimeSpan)>();
            for (var t = TimeSpan.FromHours(startHour);
                 t + TimeSpan.FromMinutes(slotMinutes) <= TimeSpan.FromHours(endHour);
                 t = t.Add(TimeSpan.FromMinutes(slotMinutes)))
            {
                foreach (var day in WeekDays)
                {
                    slots.Add((day, t));
                }
            }
            return slots;
        }
    }
}
