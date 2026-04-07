using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Response
{
    public class OfficeResponse
    {
        public int Id { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeNumber { get; set; } = string.Empty;
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Department { get; set; }
        public string? PhoneExtension { get; set; }
        public int? StaffUserId { get; set; }
        public string? StaffUserName { get; set; }
        public string? StaffUserEmail { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public TimeSpan? RegularOpenTime { get; set; }
        public TimeSpan? RegularCloseTime { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
