using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Response
{
    public class PasswordResetRequestResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
