using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class VerificationRequestService : IVerificationRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VerificationRequestService> _logger;

        public VerificationRequestService(ApplicationDbContext context, ILogger<VerificationRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VerificationRequestResponse> CreateRequestAsync(int userId, CreateVerificationRequestDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var verificationRequest = new VerificationRequest
            {
                UserId = userId,
                SubmittedId = request.SubmittedId,
                RequestType = request.RequestType,
                Status = VerificationStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _context.VerificationRequests.Add(verificationRequest);
            await _context.SaveChangesAsync();

            return await GetRequestByIdAsync(verificationRequest.Id) ?? throw new Exception("Failed to create request");
        }

        public async Task<VerificationRequestResponse?> GetRequestByIdAsync(int requestId)
        {
            var request = await _context.VerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            return request == null ? null : MapToResponse(request);
        }

        public async Task<List<VerificationRequestResponse>> GetAllRequestsAsync()
        {
            var requests = await _context.VerificationRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<VerificationRequestResponse>> GetRequestsByUserIdAsync(int userId)
        {
            var requests = await _context.VerificationRequests
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<VerificationRequestResponse>> GetRequestsByStatusAsync(VerificationStatus status)
        {
            var requests = await _context.VerificationRequests
                .Include(r => r.User)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<VerificationRequestResponse?> UpdateRequestStatusAsync(int requestId, int adminId, UpdateVerificationRequestDto request)
        {
            var verificationRequest = await _context.VerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (verificationRequest == null) return null;

            verificationRequest.Status = request.Status;
            verificationRequest.RejectionReason = request.RejectionReason;
            verificationRequest.ReviewedBy = adminId;
            verificationRequest.ReviewedAt = DateTime.UtcNow;

            // If approved, also update user status
            if (request.Status == VerificationStatus.APPROVED)
            {
                var user = await _context.Users.FindAsync(verificationRequest.UserId);
                if (user != null)
                {
                    user.Status = UserStatus.APPROVED;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (request.Status == VerificationStatus.REJECTED)
            {
                var user = await _context.Users.FindAsync(verificationRequest.UserId);
                if (user != null)
                {
                    user.Status = UserStatus.REJECTED;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Create notification for user
            var notification = new Notification
            {
                UserId = verificationRequest.UserId,
                Type = request.Status == VerificationStatus.APPROVED ? NotificationType.APPROVAL : NotificationType.REJECTION,
                Title = $"Verification Request {request.Status}",
                Message = $"Your {verificationRequest.RequestType} verification request has been {request.Status.ToString().ToLower()}. {request.RejectionReason ?? ""}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return await GetRequestByIdAsync(requestId);
        }

        public async Task<bool> DeleteRequestAsync(int requestId)
        {
            var request = await _context.VerificationRequests.FindAsync(requestId);
            if (request == null) return false;

            _context.VerificationRequests.Remove(request);
            await _context.SaveChangesAsync();

            return true;
        }

        private VerificationRequestResponse MapToResponse(VerificationRequest request)
        {
            // Get reviewer name if exists
            string? reviewerName = null;
            if (request.ReviewedBy.HasValue)
            {
                var reviewer = _context.Users.Find(request.ReviewedBy.Value);
                reviewerName = reviewer?.Name;
            }

            return new VerificationRequestResponse
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = request.User?.Name ?? "Unknown",
                UserEmail = request.User?.Email ?? "Unknown",
                SubmittedId = request.SubmittedId,
                RequestType = request.RequestType.ToString(),
                Status = request.Status.ToString(),
                ReviewedBy = request.ReviewedBy,
                ReviewedByName = reviewerName,
                RejectionReason = request.RejectionReason,
                CreatedAt = request.CreatedAt,
                ReviewedAt = request.ReviewedAt
            };
        }
    }
}
