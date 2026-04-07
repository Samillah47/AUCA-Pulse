using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateRoomDto
    {
        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string RoomName { get; set; } = string.Empty;

        public int? Capacity { get; set; }

        [MaxLength(255)]
        public string? Building { get; set; }

        [MaxLength(50)]
        public string? Floor { get; set; }

        [Required]
        public RoomType RoomType { get; set; }
    }
}
