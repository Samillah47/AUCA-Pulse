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
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == verificationRequest.UserId);
                if (user != null)
                {
                    user.Status = UserStatus.APPROVED;
                    user.UpdatedAt = DateTime.UtcNow;

                    // ====================================================================
                    // STAFF OFFICE ASSIGNMENT DURING VERIFICATION APPROVAL
                    // ====================================================================
                    // When a staff user's verification request is APPROVED, they should
                    // immediately get access to an office for the "My Office" feature.
                    // This mirrors the logic in UserService.UpdateUserStatusAsync()
                    // to keep staff onboarding consistent across approval workflows.
                    //
                    // NOTE: This is a PARALLEL workflow to UserService.UpdateUserStatusAsync()
                    // Both methods handle office assignment because staff can be approved via:
                    // - Verification Request flow (here) OR
                    // - Direct user status update (UserService)
                    //
                    // Current implementation assigns REAL offices from the database,
                    // not synthetic placeholder offices. This ensures staff get actual
                    // school offices that were pre-registered in the system.
                    // ====================================================================
                    if (user.Role.RoleName.Equals("STAFF", StringComparison.OrdinalIgnoreCase))
                    {
                        // First, check if this staff member already has an office assigned
                        var existingOffice = await _context.Offices
                            .FirstOrDefaultAsync(o => o.StaffUserId == user.Id);

                        if (existingOffice == null)
                        {
                            // No office assigned yet. Search database for an unassigned real office.
                            // We look for offices with StaffUserId = NULL (not currently assigned to anyone)
                            var availableOffice = await _context.Offices
                                .FirstOrDefaultAsync(o => o.StaffUserId == null);

                            if (availableOffice != null)
                            {
                                // SUCCESS: Found an unassigned real office in the database
                                // Assign it to this newly verified staff member
                                availableOffice.StaffUserId = user.Id;

                                // Log this important action with full details for audit trail
                                _logger.LogInformation(
                                    "✓ Successfully assigned real office {OfficeId} (Office #{OfficeNumber}) to verified staff user {UserId}",
                                    availableOffice.Id,
                                    availableOffice.OfficeNumber,
                                    user.Id);
                            }
                            else
                            {
                                // WARNING: No unassigned offices available in database
                                // All real offices are already assigned to other staff members
                                // Admin intervention required to add more offices or unassign existing ones
                                _logger.LogWarning(
                                    "⚠ No unassigned offices available for verified staff user {UserId}. " +
                                    "Admin must manually assign an office through the Office Management page.",
                                    user.Id);
                            }
                        }
                        // If existingOffice != null, user already has an office, so do nothing
                    }
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
