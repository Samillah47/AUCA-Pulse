using AUCAPulse.Data;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    /// <summary>
    /// Singleton service that distributes ad-hoc room reservations fairly across all
    /// AVAILABLE rooms using a Round Robin cycle. The cycle pointer is preserved
    /// between requests to prevent the same room from being picked repeatedly.
    ///
    /// Registered as Singleton — uses IServiceScopeFactory to resolve a scoped
    /// ApplicationDbContext per request, avoiding the Singleton-consumes-Scoped trap.
    /// Pointer access is guarded by a lock for thread safety.
    /// </summary>
    public class RoundRobinRoomService : IRoundRobinRoomService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoundRobinRoomService> _logger;

        private int _lastIndex = -1;
        private readonly object _pointerLock = new();

        public RoundRobinRoomService(
            IServiceScopeFactory scopeFactory,
            ILogger<RoundRobinRoomService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<RoomResponse?> AssignNextAvailableRoomAsync(
            int userId, RoomType? type, int durationMinutes)
        {
            if (durationMinutes < 15 || durationMinutes > 480)
                throw new Exception("Duration must be between 15 and 480 minutes");

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Verify the user exists
            var user = await context.Users.FindAsync(userId)
                ?? throw new Exception("User not found");

            // Query currently-available rooms (optionally filtered by type)
            var query = context.Rooms.Where(r => r.Status == RoomStatus.AVAILABLE);
            if (type.HasValue)
                query = query.Where(r => r.RoomType == type.Value);

            var available = await query
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (!available.Any())
                return null;

            // Advance the Round Robin pointer (thread-safe)
            int pickedIndex;
            lock (_pointerLock)
            {
                _lastIndex = (_lastIndex + 1) % available.Count;
                pickedIndex = _lastIndex;
            }

            var selected = available[pickedIndex];

            // Reserve the room for the user
            selected.Status = RoomStatus.RESERVED;
            selected.CurrentLecturerId = userId;
            selected.OccupiedAt = DateTime.UtcNow;
            selected.OccupiedUntil = DateTime.UtcNow.AddMinutes(durationMinutes);
            selected.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Round Robin assigned room {RoomNumber} (index {Index}/{Total}) to user {UserId}",
                selected.RoomNumber, pickedIndex, available.Count, userId);

            return new RoomResponse
            {
                Id = selected.Id,
                RoomNumber = selected.RoomNumber,
                RoomName = selected.RoomName,
                Capacity = selected.Capacity,
                Building = selected.Building,
                Floor = selected.Floor,
                RoomType = selected.RoomType.ToString(),
                Status = selected.Status.ToString(),
                CurrentLecturerId = selected.CurrentLecturerId,
                CurrentLecturerName = user.Name,
                OccupiedAt = selected.OccupiedAt,
                OccupiedUntil = selected.OccupiedUntil,
                CreatedAt = selected.CreatedAt
            };
        }
    }
}
