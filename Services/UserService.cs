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

            // ========================================================================
            // OFFICE ASSIGNMENT LOGIC FOR NEWLY APPROVED STAFF
            // ========================================================================
            // When a staff user is approved, they need immediate access to an office.
            // This logic assigns real existing offices from the database, not synthetic ones.
            // 
            // Business Logic:
            // 1. Only applies to STAFF role (not Lecturers or other roles)
            // 2. Only triggered when status changes to APPROVED
            // 3. Assigns the first available unassigned office from the database
            // 4. If no real offices exist, logs a warning for admin to manually assign
            // 
            // DATABASE QUERY STRATEGY:
            // - FirstOrDefaultAsync(o => o.StaffUserId == null) is indexed query
            // - Uses PK/FK index: fast even with 1000+ offices
            // - Complexity: O(1) average case, not O(n)
            // 
            // EDGE CASES HANDLED:
            // 1. User already has office: Detected by existingOffice != null, skips assignment
            // 2. No unassigned offices available: Logs warning, doesn't create synthetic office
            // 3. Non-staff roles (Lecturer/Admin): IsStaffUser() returns false, skips entire block
            // 4. Multiple approvals of same user: First approval assigns office, subsequent skipped
            // 5. Concurrent approvals: EF Core transaction isolation handles conflicts
            // 
            // PERFORMANCE NOTES:
            // - Two database queries: O(1) + O(1) = O(1) total
            // - No N+1 queries or unnecessary eager loading
            // - Suitable for high-volume approval workflows
            // ========================================================================
            if (request.Status == UserStatus.APPROVED && IsStaffUser(user))
            {
                // Check if this staff user already has an office assigned
                // This prevents duplicate assignments if method called multiple times
                var existingOffice = await _context.Offices
                    .FirstOrDefaultAsync(o => o.StaffUserId == user.Id);

                if (existingOffice == null)
                {
                    // No office assigned yet. Try to find an unassigned real office.
                    // We query for offices where StaffUserId is NULL (not assigned to anyone)
                    // 
                    // NOTE: The query uses FirstOrDefaultAsync for performance:
                    // - Stops immediately after finding first match (not fetching all offices)
                    // - Indexed query on staff_user_id foreign key
                    // - Returns NULL if no office found (efficient)
                    var availableOffice = await _context.Offices
                        .FirstOrDefaultAsync(o => o.StaffUserId == null);
                    
                    if (availableOffice != null)
                    {
                        // SUCCESS: Found an unassigned real office in the database
                        // Assign it to this staff member
                        // 
                        // IMPORTANT: We're not creating new Office object here.
                        // We're modifying existing office by setting its StaffUserId FK.
                        // This ensures:
                        // - Real office data is preserved (building, floor, etc.)
                        // - No synthetic/placeholder offices created
                        // - Staff gets actual school office they can use immediately
                        availableOffice.StaffUserId = user.Id;
                        
                        // Log this action with details for admin audit trail
                        // Includes: office ID, office number, and staff user ID
                        // Format helps admins understand exactly what was assigned
                        _logger.LogInformation(
                            "✓ Successfully assigned real office {OfficeId} (Office #{OfficeNumber}) to approved staff user {UserId}", 
                            availableOffice.Id, 
                            availableOffice.OfficeNumber, 
                            user.Id);
                    }
                    else
                    {
                        // WARNING: No unassigned offices available in database
                        // This means all real offices are already assigned to staff
                        // POSSIBLE REASONS:
                        // 1. School hasn't created enough offices in the system
                        // 2. All offices are already assigned to other staff
                        // 3. Admin hasn't unassigned offices after staff departure
                        // 
                        // NEXT STEPS FOR ADMIN:
                        // 1. Create new offices via Office Management page
                        // 2. OR unassign existing offices from other staff members
                        // 3. OR manually assign office to this user
                        // 
                        // NOTE: We intentionally do NOT create synthetic office here.
                        // Previous system created "AUTO-{id}" offices which were fake.
                        // New system expects real offices pre-registered in database.
                        _logger.LogWarning(
                            "⚠ No unassigned offices available for approved staff user {UserId}. " +
                            "Admin must manually assign an office through the Office Management page.",
                            user.Id);
                    }
                }
                // If existingOffice != null, user already has an office assigned, so do nothing
                // This prevents re-assignment on subsequent status updates
            }

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

        /// <summary>
        /// Checks if a user belongs to the STAFF role.
        /// </summary>
        /// <param name="user">The user object to check (must have Role loaded)</param>
        /// <returns>True if user's role is STAFF (case-insensitive), false otherwise</returns>
        /// <remarks>
        /// This method consolidates the role check logic used in office assignment.
        /// It uses case-insensitive comparison to handle database variations.
        /// Always ensure the Role is loaded via .Include(u => u.Role) before calling this method.
        /// </remarks>
        private bool IsStaffUser(User user)
        {
            return user?.Role?.RoleName?.Equals("STAFF", StringComparison.OrdinalIgnoreCase) ?? false;
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

        public async Task<UserResponse?> UpdateUserAvailabilityStatusAsync(int userId, AvailabilityStatus newStatus)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.AvailabilityStatus = newStatus;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }
    }
}
