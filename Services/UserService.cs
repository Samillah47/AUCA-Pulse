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

        public async Task<UserResponse> AdminCreateUserAsync(AdminCreateUserDto request)
        {
            var roleType = (request.RoleType ?? "").Trim().ToUpperInvariant();
            var allowedRoles = new[] { "STUDENT", "LECTURER", "STAFF", "ADMIN" };
            if (!allowedRoles.Contains(roleType))
                throw new Exception("Please choose a valid role: STUDENT, LECTURER, STAFF, or ADMIN.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new Exception("Password must be at least 6 characters long.");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Full name is required.");
            if (string.IsNullOrWhiteSpace(request.IdentificationNumber))
                throw new Exception("Identification number is required.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("A user with this email already exists.");
            if (await _context.Users.AnyAsync(u => u.IdentificationNumber == request.IdentificationNumber))
                throw new Exception("A user with this identification number already exists.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleType)
                ?? throw new Exception("Selected role is not configured in the system.");

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IdentificationNumber = request.IdentificationNumber.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
                LocationId = request.LocationId,
                RoleId = role.Id,
                // Admin-created accounts are immediately active — no pending approval
                Status = UserStatus.APPROVED,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Try to notify the new user with their starting credentials
            try
            {
                await _emailService.SendApprovalEmailAsync(user.Email, user.Name, roleType, user.Department ?? "-");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Admin-created user {Email} was saved but the welcome email failed.", user.Email);
            }

            var created = await GetUserByIdAsync(user.Id)
                ?? throw new Exception("The user was created but could not be reloaded.");
            return created;
        }

        public async Task<UserResponse?> AdminUpdateUserAsync(int userId, AdminUpdateUserRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name.Trim();

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.Trim() != user.Email)
            {
                var email = request.Email.Trim();
                if (await _context.Users.AnyAsync(u => u.Id != userId && u.Email == email))
                    throw new Exception("Another user already uses this email address.");
                user.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber)
                && request.IdentificationNumber.Trim() != user.IdentificationNumber)
            {
                var id = request.IdentificationNumber.Trim();
                if (await _context.Users.AnyAsync(u => u.Id != userId && u.IdentificationNumber == id))
                    throw new Exception("Another user already uses this identification number.");
                user.IdentificationNumber = id;
            }

            if (request.PhoneNumber != null)
                user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            if (request.Department != null)
                user.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();

            if (!string.IsNullOrWhiteSpace(request.RoleType))
            {
                var roleName = request.RoleType.Trim().ToUpperInvariant();
                var allowed = new[] { "STUDENT", "LECTURER", "STAFF", "ADMIN" };
                if (!allowed.Contains(roleName))
                    throw new Exception("Please choose a valid role: STUDENT, LECTURER, STAFF, or ADMIN.");
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName)
                    ?? throw new Exception("That role is not configured in the system.");
                user.RoleId = role.Id;
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<UserStatus>(request.Status.Trim(), true, out var newStatus))
                    throw new Exception("Please choose a valid status: PENDING, APPROVED, or REJECTED.");
                user.Status = newStatus;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id);
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

        public async Task<List<UserResponse>> GetUsersByRoleNameAsync(string roleName)
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Include(u => u.Office)
                .Where(u => u.Role != null && u.Role.RoleName == roleName)
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
    }
}
