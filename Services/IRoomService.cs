using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IRoomService
    {
        Task<RoomResponse> CreateRoomAsync(CreateRoomDto request);
        Task<RoomResponse?> GetRoomByIdAsync(int roomId);
        Task<List<RoomResponse>> GetAllRoomsAsync();
        Task<List<RoomResponse>> GetRoomsByStatusAsync(RoomStatus status);
        Task<List<RoomResponse>> GetRoomsByTypeAsync(RoomType type);
        Task<List<RoomResponse>> GetAvailableRoomsAsync();
        Task<RoomResponse?> UpdateRoomAsync(int roomId, CreateRoomDto request);
        Task<RoomResponse?> OccupyRoomAsync(int roomId, int lecturerId, OccupyRoomDto request);
        Task<RoomResponse?> ReleaseRoomAsync(int roomId, int lecturerId);
        Task<bool> DeleteRoomAsync(int roomId);
    }
}
