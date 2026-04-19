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
                .Include(ca => ca.Group)
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

            // Each credit is worth one base slot: a 3-credit course occupies
            // 3 consecutive time slots on the same day. We check the whole
            // run for lecturer / room conflicts before placing.
            var daySlotEnd = TimeSpan.FromHours(dto.EndHour);

            foreach (var assignment in assignments)
            {
                var credits = Math.Max(1, assignment.Course?.Credits ?? 1);
                var totalSpan = TimeSpan.FromMinutes(dto.SlotDurationMinutes * credits);

                var placed = false;
                var triedSlots = 0;
                var totalSlots = timeSlots.Count;

                while (triedSlots < totalSlots && !placed)
                {
                    var slot = timeSlots[timeSlotPointer];
                    timeSlotPointer = (timeSlotPointer + 1) % totalSlots;
                    triedSlots++;

                    // Build the list of N consecutive (day, time) slots this
                    // placement would occupy. All must fit in the daily window
                    // and be free of the lecturer.
                    var runStart = slot.Start;
                    var runEnd = runStart.Add(totalSpan);
                    if (runEnd > daySlotEnd) continue;

                    var runKeys = new List<(string, TimeSpan)>(credits);
                    var lecturerClash = false;
                    for (int i = 0; i < credits; i++)
                    {
                        var t = runStart.Add(TimeSpan.FromMinutes(dto.SlotDurationMinutes * i));
                        var key = (slot.Day, t);
                        runKeys.Add(key);
                        if ((bookedLecturers.GetValueOrDefault(key) ?? new HashSet<int>()).Contains(assignment.LecturerId))
                        {
                            lecturerClash = true;
                            break;
                        }
                    }
                    if (lecturerClash) continue;

                    // Try each room in Round Robin order. A room fits only if
                    // ALL N consecutive slots are free for that room.
                    var triedRooms = 0;
                    while (triedRooms < rooms.Count && !placed)
                    {
                        var room = rooms[roomPointer];
                        roomPointer = (roomPointer + 1) % rooms.Count;
                        triedRooms++;

                        var roomClash = runKeys.Any(k =>
                            (bookedRooms.GetValueOrDefault(k) ?? new HashSet<int>()).Contains(room.Id));
                        if (roomClash) continue;

                        // Place: one LectureSchedule row spanning the full run.
                        var schedule = new LectureSchedule
                        {
                            LecturerId = assignment.LecturerId,
                            DayOfWeek = slot.Day,
                            StartTime = runStart,
                            EndTime = runEnd,
                            CourseCode = assignment.Course?.CourseCode,
                            CourseName = assignment.Course?.CourseName,
                            RoomNumber = room.RoomNumber,
                            GroupName = assignment.Group?.Name,
                            SemesterId = semester.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        newSchedules.Add(schedule);

                        // Mark EVERY consecutive base slot as booked so the
                        // next round-robin candidate can't overlap us.
                        foreach (var k in runKeys)
                        {
                            if (!bookedLecturers.ContainsKey(k))
                                bookedLecturers[k] = new HashSet<int>();
                            bookedLecturers[k].Add(assignment.LecturerId);

                            if (!bookedRooms.ContainsKey(k))
                                bookedRooms[k] = new HashSet<int>();
                            bookedRooms[k].Add(room.Id);
                        }

                        result.Scheduled.Add(new GeneratedScheduleEntry
                        {
                            LecturerId = assignment.LecturerId,
                            LecturerName = assignment.Lecturer?.Name ?? string.Empty,
                            CourseCode = assignment.Course?.CourseCode ?? string.Empty,
                            CourseName = assignment.Course?.CourseName ?? string.Empty,
                            RoomNumber = room.RoomNumber,
                            GroupName = assignment.Group?.Name,
                            DayOfWeek = slot.Day,
                            StartTime = runStart.ToString(@"hh\:mm"),
                            EndTime = runEnd.ToString(@"hh\:mm")
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
                        Reason = $"No conflict-free {credits * dto.SlotDurationMinutes}-minute window available. Expand the day, add more rooms, or reduce course load."
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
        /// Two intentional tricks so the distribution feels natural:
        ///
        /// 1. Times of day are INTERLEAVED between the morning half and the
        ///    afternoon/evening half. For a day window like 08:00-21:00 the
        ///    raw times are [08:00, 08:50, 09:40, ..., 20:10]. We split them
        ///    into early [08:00, ..., 13:30] and late [14:20, ..., 20:10] and
        ///    zip them: [08:00, 14:20, 08:50, 15:10, 09:40, 16:00, ...].
        ///    That way when the Round Robin comes back to the same day for a
        ///    second class, it lands in the afternoon instead of right after
        ///    the first one.
        ///
        /// 2. Days-of-week are the INNER loop for each time-of-day, so the
        ///    first six slots are (Mon 08:00), (Tue 08:00), (Wed 08:00),
        ///    (Thu 08:00), (Fri 08:00), (Sun 08:00) and only then does the
        ///    pointer jump to the late-morning slot. Combined with the time
        ///    interleave, a semester with 12 courses ends up with one morning
        ///    and one afternoon class per teaching day.
        /// </summary>
        private static List<(string Day, TimeSpan Start)> BuildTimeSlots(
            int startHour, int endHour, int slotMinutes)
        {
            // Enumerate all valid start times in the daily window.
            var rawTimes = new List<TimeSpan>();
            for (var t = TimeSpan.FromHours(startHour);
                 t + TimeSpan.FromMinutes(slotMinutes) <= TimeSpan.FromHours(endHour);
                 t = t.Add(TimeSpan.FromMinutes(slotMinutes)))
            {
                rawTimes.Add(t);
            }

            // Interleave: alternate between the early half and the late half.
            var interleavedTimes = new List<TimeSpan>(rawTimes.Count);
            int mid = (rawTimes.Count + 1) / 2;
            int iEarly = 0, iLate = mid;
            while (iEarly < mid || iLate < rawTimes.Count)
            {
                if (iEarly < mid) interleavedTimes.Add(rawTimes[iEarly++]);
                if (iLate < rawTimes.Count) interleavedTimes.Add(rawTimes[iLate++]);
            }

            // Cross with day-of-week (day is the inner loop so we spread
            // across all teaching days before filling a second slot on any).
            var slots = new List<(string, TimeSpan)>(interleavedTimes.Count * WeekDays.Length);
            foreach (var t in interleavedTimes)
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
