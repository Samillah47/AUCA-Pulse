using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Response
{
    public class RoomResponse
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CurrentLecturerId { get; set; }
        public string? CurrentLecturerName { get; set; }
        public DateTime? OccupiedAt { get; set; }
        public DateTime? OccupiedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
