using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class PasswordResetRequestService : IPasswordResetRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public PasswordResetRequestService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<PasswordResetRequestResponse> CreateRequestAsync(CreatePasswordResetRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found with provided credentials");

            var token = Guid.NewGuid().ToString();
            var request = new PasswordResetRequest
            {
                UserId = dto.UserId,
                Token = token,
                Status = RequestStatus.PENDING,
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetRequests.Add(request);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(
                dto.Email,
                user.Name,
                token
            );

            return MapToResponse(request);
        }

        public async Task<PasswordResetRequestResponse?> GetRequestByIdAsync(int id)
        {
            var request = await _context.PasswordResetRequests.FindAsync(id);
            return request == null ? null : MapToResponse(request);
        }

        public async Task<PasswordResetRequestResponse?> GetRequestByTokenAsync(string token)
        {
            var request = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Token == token);
            return request == null ? null : MapToResponse(request);
        }

        public async Task<List<PasswordResetRequestResponse>> GetAllRequestsAsync()
        {
            var requests = await _context.PasswordResetRequests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<PasswordResetRequestResponse>> GetRequestsByStatusAsync(RequestStatus status)
        {
            var requests = await _context.PasswordResetRequests
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<PasswordResetRequestResponse>> GetRequestsByUserIdAsync(int userId)
        {
            var requests = await _context.PasswordResetRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<PasswordResetRequestResponse?> UpdateRequestStatusAsync(int id, UpdatePasswordResetRequestDto dto, int reviewedBy)
        {
            var request = await _context.PasswordResetRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return null;

            request.Status = dto.Status;
            request.ReviewedBy = reviewedBy;
            request.ReviewedAt = DateTime.UtcNow;
            request.RejectionReason = dto.RejectionReason;

            await _context.SaveChangesAsync();

            if (dto.Status == RequestStatus.APPROVED)
            {
                await _emailService.SendPasswordResetEmailAsync(
                    request.User.Email,
                    request.User.Name,
                    request.Token
                );
            }
            else if (dto.Status == RequestStatus.REJECTED)
            {
                await _emailService.SendPasswordResetRejectionEmailAsync(
                    request.User.Email,
                    request.User.Name,
                    dto.RejectionReason ?? "No reason provided"
                );
            }

            return MapToResponse(request);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var request = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Token == token && r.Status == RequestStatus.APPROVED);

            if (request == null) return false;
            if (request.ExpiryDate < DateTime.UtcNow) return false;

            return true;
        }

        public async Task<bool> DeleteRequestAsync(int id)
        {
            var request = await _context.PasswordResetRequests.FindAsync(id);
            if (request == null) return false;

            _context.PasswordResetRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PasswordResetRequestResponse MapToResponse(PasswordResetRequest request)
        {
            return new PasswordResetRequestResponse
            {
                Id = request.Id,
                UserId = request.UserId,
                Token = request.Token,
                Status = request.Status,
                ExpiryDate = request.ExpiryDate,
                ReviewedBy = request.ReviewedBy,
                ReviewedAt = request.ReviewedAt,
                RejectionReason = request.RejectionReason,
                CreatedAt = request.CreatedAt
            };
        }
    }
}
