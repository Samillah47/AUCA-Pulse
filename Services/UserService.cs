using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IEmailService emailService, ILogger<UserService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserResponse?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user == null ? null : MapToUserResponse(user);
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .FirstOrDefaultAsync(u => u.Email == email);

            return user == null ? null : MapToUserResponse(user);
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToUserResponse).ToList();
        }

        public async Task<List<UserResponse>> GetUsersByStatusAsync(UserStatus status)
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .Where(u => u.Status == status)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToUserResponse).ToList();
        }

        public async Task<List<UserResponse>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .Where(u => u.RoleId == roleId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToUserResponse).ToList();
        }

        public async Task<UserResponse?> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.Name = request.Name;
            user.PhoneNumber = request.PhoneNumber;
            user.Department = request.Department;
            user.LocationId = request.LocationId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        public async Task<UserResponse?> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var oldStatus = user.Status;
            user.Status = request.Status;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification email
            if (oldStatus != request.Status)
            {
                if (request.Status == UserStatus.APPROVED)
                {
                    await _emailService.SendApprovalEmailAsync(user.Email, user.Name, user.Role.RoleName, user.Department ?? "Not specified");
                }
                else if (request.Status == UserStatus.REJECTED)
                {
                    await _emailService.SendRejectionEmailAsync(user.Email, user.Name, request.Reason ?? "No reason provided");
                }
            }

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                Status = user.Status.ToString(),
                Role = user.Role?.RoleName ?? "Unknown",
                RoleId = user.RoleId,
                LocationId = user.LocationId,
                LocationName = user.Location?.Name,
                OfficeName = user.Office?.OfficeName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
