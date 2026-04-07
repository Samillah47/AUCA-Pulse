using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Response
{
    public class LocationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LocationResponse>? Children { get; set; }
    }
}
