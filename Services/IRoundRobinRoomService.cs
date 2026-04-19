using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IRoundRobinRoomService
    {
        /// <summary>
        /// Picks the next available room using a Round Robin cycle across all AVAILABLE
        /// rooms (optionally filtered by type), reserves it for the given user, and
        /// returns the reserved room. Returns null if no room is available.
        /// </summary>
        Task<RoomResponse?> AssignNextAvailableRoomAsync(int userId, RoomType? type, int durationMinutes);
    }
}
