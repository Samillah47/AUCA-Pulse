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

            return room == null ? null : MapToResponse(room);
        }

        public async Task<List<RoomResponse>> GetAllRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            return rooms.Select(MapToResponse).ToList();
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

            return rooms.Select(MapToResponse).ToList();
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

            return rooms.Select(MapToResponse).ToList();
        }

        public async Task<List<RoomResponse>> GetAvailableRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Include(r => r.CurrentLecturer)
                .Where(r => r.Status == RoomStatus.AVAILABLE)
                .OrderBy(r => r.Building)
                .ThenBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();

            return rooms.Select(MapToResponse).ToList();
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
                throw new Exception("Room is already occupied");
            }

            if (room.Status == RoomStatus.MAINTENANCE)
            {
                throw new Exception("Room is under maintenance");
            }

            // Verify lecturer exists
            var lecturer = await _context.Users.FindAsync(lecturerId);
            if (lecturer == null)
            {
                throw new Exception("Lecturer not found");
            }

            // Ensure UTC for PostgreSQL
            var occupiedUntilUtc = request.OccupiedUntil.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.OccupiedUntil, DateTimeKind.Utc)
                : request.OccupiedUntil.ToUniversalTime();

            room.Status = RoomStatus.OCCUPIED;
            room.CurrentLecturerId = lecturerId;
            room.OccupiedAt = DateTime.UtcNow;
            room.OccupiedUntil = occupiedUntilUtc;
            room.CourseInfo = request.CourseInfo;

            // Get current semester
            var currentSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);

            // Also create a lecture schedule entry
            var schedule = new LectureSchedule
            {
                LecturerId = lecturerId,
                RoomNumber = room.RoomNumber,
                DayOfWeek = DateTime.UtcNow.DayOfWeek.ToString().ToUpper(),
                StartTime = DateTime.UtcNow.TimeOfDay,
                EndTime = occupiedUntilUtc.TimeOfDay,
                CourseName = !string.IsNullOrWhiteSpace(request.CourseInfo) ? request.CourseInfo : "Manual Occupation / Room Reservation",
                SemesterId = currentSemester?.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.LectureSchedules.Add(schedule);

            await _context.SaveChangesAsync();

            return await GetRoomByIdAsync(roomId);
        }

        public async Task<RoomResponse?> ReleaseRoomAsync(int roomId, int lecturerId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            if (room.Status != RoomStatus.OCCUPIED)
            {
                throw new Exception("Room is not occupied");
            }

            if (room.CurrentLecturerId != lecturerId)
            {
                throw new Exception("You are not the current occupant of this room");
            }

            room.Status = RoomStatus.AVAILABLE;
            room.CurrentLecturerId = null;
            room.OccupiedAt = null;
            room.OccupiedUntil = null;

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

        private RoomResponse MapToResponse(Room room)
        {
            return new RoomResponse
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomName = room.RoomName,
                Capacity = room.Capacity,
                Building = room.Building,
                Floor = room.Floor,
                RoomType = room.RoomType.ToString(),
                Status = room.Status.ToString(),
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
