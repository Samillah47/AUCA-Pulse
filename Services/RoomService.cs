using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomService> _logger;

        public RoomService(ApplicationDbContext context, ILogger<RoomService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RoomResponse> CreateRoomAsync(CreateRoomDto request)
        {
            // Check if room number already exists
            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == request.RoomNumber);
            if (existingRoom != null)
            {
                throw new Exception($"Room with number {request.RoomNumber} already exists");
            }

            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                RoomName = request.RoomName,
                Capacity = request.Capacity,
                Building = request.Building,
                Floor = request.Floor,
                RoomType = request.RoomType,
                Status = RoomStatus.AVAILABLE,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return await GetRoomByIdAsync(room.Id) ?? throw new Exception("Failed to create room");
        }

        public async Task<RoomResponse?> GetRoomByIdAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null) return null;
            var busy = await GetRoomNumbersWithActiveClassAsync();
            return MapToResponse(room, busy);
        }

        /// <summary>
        /// Returns the set of RoomNumbers that currently have an active class
        /// (i.e. a LectureSchedule entry matching today, inside the active
        /// semester window, with the current time falling between StartTime
        /// and EndTime). These rooms are considered OCCUPIED even if their
        /// stored Room.Status is AVAILABLE.
        /// </summary>
        private async Task<HashSet<string>> GetRoomNumbersWithActiveClassAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentDay = now.DayOfWeek.ToString().ToUpperInvariant();
            var currentTime = now.TimeOfDay;

            var roomNumbers = await _context.LectureSchedules
                .Include(s => s.Semester)
                .Where(s => !string.IsNullOrEmpty(s.RoomNumber)
                         && s.DayOfWeek.ToUpper() == currentDay
                         && s.StartTime <= currentTime
                         && s.EndTime >= currentTime
                         && s.Semester != null
                         && s.Semester.StartDate <= today
                         && s.Semester.EndDate >= today)
                .Select(s => s.RoomNumber!)
                .Distinct()
                .ToListAsync();

            return new HashSet<string>(roomNumbers, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<List<RoomResponse>> GetAllRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            var busyRoomNumbers = await GetRoomNumbersWithActiveClassAsync();
            return rooms.Select(r => MapToResponse(r, busyRoomNumbers)).ToList();
        }

        public async Task<List<RoomResponse>> GetRoomsByStatusAsync(RoomStatus status)
        {
            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .Where(r => r.Status == status)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            var busyRoomNumbers = await GetRoomNumbersWithActiveClassAsync();
            return rooms.Select(r => MapToResponse(r, busyRoomNumbers)).ToList();
        }

        public async Task<List<RoomResponse>> GetRoomsByTypeAsync(RoomType type)
        {
            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .Where(r => r.RoomType == type)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            var busyRoomNumbers = await GetRoomNumbersWithActiveClassAsync();
            return rooms.Select(r => MapToResponse(r, busyRoomNumbers)).ToList();
        }

        public async Task<List<RoomResponse>> GetAvailableRoomsAsync()
        {
            // "Available" means Status=AVAILABLE in the DB AND no active class
            // in this room right now (via the timetable).
            var busyRoomNumbers = await GetRoomNumbersWithActiveClassAsync();

            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .Where(r => r.Status == RoomStatus.AVAILABLE)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            return rooms
                .Where(r => !busyRoomNumbers.Contains(r.RoomNumber))
                .Select(r => MapToResponse(r, busyRoomNumbers))
                .ToList();
        }

        public async Task<RoomResponse?> UpdateRoomAsync(int roomId, CreateRoomDto request)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            // Check if new room number conflicts with existing room
            if (room.RoomNumber != request.RoomNumber)
            {
                var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == request.RoomNumber);
                if (existingRoom != null)
                {
                    throw new Exception($"Room with number {request.RoomNumber} already exists");
                }
            }

            room.RoomNumber = request.RoomNumber;
            room.RoomName = request.RoomName;
            room.Capacity = request.Capacity;
            room.Building = request.Building;
            room.Floor = request.Floor;
            room.RoomType = request.RoomType;

            await _context.SaveChangesAsync();

            return await GetRoomByIdAsync(roomId);
        }

        public async Task<RoomResponse?> OccupyRoomAsync(int roomId, int lecturerId, OccupyRoomDto request)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            if (room.Status == RoomStatus.OCCUPIED)
            {
                throw new Exception("This room is already occupied by someone else.");
            }

            if (room.Status == RoomStatus.MAINTENANCE)
            {
                throw new Exception("This room is currently under maintenance.");
            }

            // Verify the user exists (lecturer, staff, or admin — anyone who reached this endpoint)
            var user = await _context.Users.FindAsync(lecturerId);
            if (user == null)
            {
                throw new Exception("We couldn't verify your account. Please sign in again.");
            }

            // Normalise Kind for PostgreSQL
            var occupiedUntilUtc = request.OccupiedUntil.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.OccupiedUntil, DateTimeKind.Utc)
                : request.OccupiedUntil.ToUniversalTime();

            var nowUtc = DateTime.UtcNow;

            if (occupiedUntilUtc <= nowUtc)
            {
                throw new Exception("Please choose an end time in the future.");
            }

            room.Status = RoomStatus.OCCUPIED;
            room.CurrentLecturerId = lecturerId;
            room.OccupiedAt = nowUtc;
            room.OccupiedUntil = occupiedUntilUtc;
            room.CourseInfo = request.CourseInfo;
            room.UpdatedAt = nowUtc;

            // Get current semester
            var currentSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);

            // Also create a lecture schedule entry for audit/history
            var schedule = new LectureSchedule
            {
                LecturerId = lecturerId,
                RoomNumber = room.RoomNumber,
                DayOfWeek = nowUtc.DayOfWeek.ToString().ToUpper(),
                StartTime = nowUtc.TimeOfDay,
                EndTime = occupiedUntilUtc.TimeOfDay,
                CourseName = !string.IsNullOrWhiteSpace(request.CourseInfo) ? request.CourseInfo : "Manual Occupation / Room Reservation",
                SemesterId = currentSemester?.Id,
                CreatedAt = nowUtc
            };
            _context.LectureSchedules.Add(schedule);

            await _context.SaveChangesAsync();

            return await GetRoomByIdAsync(roomId);
        }

        public async Task<RoomResponse?> ReleaseRoomAsync(int roomId, int lecturerId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            // Allow release for either OCCUPIED (manual) or RESERVED (auto-assigned) rooms
            if (room.Status != RoomStatus.OCCUPIED && room.Status != RoomStatus.RESERVED)
            {
                throw new Exception("This room is not currently occupied or reserved.");
            }

            if (room.CurrentLecturerId != lecturerId)
            {
                throw new Exception("You can only release a room you currently hold.");
            }

            room.Status = RoomStatus.AVAILABLE;
            room.CurrentLecturerId = null;
            room.OccupiedAt = null;
            room.OccupiedUntil = null;
            room.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetRoomByIdAsync(roomId);
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            if (room.Status == RoomStatus.OCCUPIED)
            {
                throw new Exception("Cannot delete occupied room. Release it first.");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }

        private RoomResponse MapToResponse(Room room, HashSet<string>? busyRoomNumbers = null)
        {
            var isScheduled = busyRoomNumbers != null
                && !string.IsNullOrEmpty(room.RoomNumber)
                && busyRoomNumbers.Contains(room.RoomNumber);

            // A room shown as AVAILABLE but currently holding a scheduled class
            // must appear OCCUPIED in the UI. Keep the stored Status untouched;
            // only override what the caller sees.
            var effectiveStatus = (isScheduled && room.Status == RoomStatus.AVAILABLE)
                ? RoomStatus.OCCUPIED.ToString()
                : room.Status.ToString();

            return new RoomResponse
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomName = room.RoomName,
                Capacity = room.Capacity,
                Building = room.Building,
                Floor = room.Floor,
                RoomType = room.RoomType.ToString(),
                Status = effectiveStatus,
                CurrentLecturerId = room.CurrentLecturerId,
                CurrentLecturerName = room.CurrentLecturer?.Name,
                OccupiedAt = room.OccupiedAt,
                OccupiedUntil = room.OccupiedUntil,
                CourseInfo = room.CourseInfo,
                CreatedAt = room.CreatedAt
            };
        }
    }
}
