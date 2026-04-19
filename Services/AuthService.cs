using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Helpers;
using AUCAPulse.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public interface IAuthService
    {
        Task<Dictionary<string, object>> RegisterAsync(RegisterRequest request);
        Task<Dictionary<string, object>> LoginAsync(LoginRequest request);
        Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IEmailService emailService,
            JwtHelper jwtHelper,
            ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("Email already registered");
            }

            // Check if ID already exists
            if (await _context.Users.AnyAsync(u => u.IdentificationNumber == request.IdentificationNumber))
            {
                throw new Exception("Identification number already registered");
            }

            // Get role
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.RoleType.ToUpper());
            if (role == null)
            {
                throw new Exception("Invalid role type");
            }

            // Create user with hashed password
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IdentificationNumber = request.IdentificationNumber,
                PhoneNumber = request.PhoneNumber,
                Department = request.Department,
                RoleId = role.Id,
                LocationId = request.LocationId,
                Status = request.RoleType.ToUpper() == "ADMIN" ? UserStatus.APPROVED : UserStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create verification request ONLY if not an auto-approved admin
            if (user.Status != UserStatus.APPROVED)
            {
                RequestType requestType;
                var roleTypeUpper = request.RoleType.ToUpper();
                
                if (roleTypeUpper == "STUDENT") requestType = RequestType.STUDENT;
                else if (roleTypeUpper == "LECTURER") requestType = RequestType.LECTURER;
                else if (roleTypeUpper == "STAFF") requestType = RequestType.STAFF;
                else requestType = RequestType.STUDENT; // Default

                var verificationRequest = new VerificationRequest
                {
                    UserId = user.Id,
                    SubmittedId = request.IdentificationNumber,
                    RequestType = requestType,
                    Status = VerificationStatus.PENDING,
                    CreatedAt = DateTime.UtcNow
                };

                _context.VerificationRequests.Add(verificationRequest);
                await _context.SaveChangesAsync();
            }

            return new Dictionary<string, object>
            {
                { "message", "Registration submitted. Awaiting admin approval." },
                { "userId", user.Id }
            };
        }

        public async Task<Dictionary<string, object>> LoginAsync(LoginRequest request)
        {
            // Get user from database
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new Exception("The email or password you entered doesn't match our records.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("The email or password you entered doesn't match our records.");
            }

            // Check if user account is approved
            if (user.Status == UserStatus.PENDING)
            {
                throw new Exception("Your account is pending approval. You'll be able to sign in once an administrator approves it.");
            }
            if (user.Status == UserStatus.REJECTED)
            {
                throw new Exception("Your account has been rejected. Please contact an administrator for help.");
            }

            // Generate and save OTP
            var otp = OtpHelper.GenerateOtp();
            user.OtpCode = otp;
            user.OtpExpiry = OtpHelper.GetOtpExpiry(5);
            await _context.SaveChangesAsync();

            // Send OTP to user's email
            await _emailService.SendOtpEmailAsync(user.Email, otp);

            // Return pending status
            return new Dictionary<string, object>
            {
                { "status", "PENDING_OTP" },
                { "message", "OTP sent to email" }
            };
        }

        public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!OtpHelper.ValidateOtp(request.Otp, user.OtpCode ?? "", user.OtpExpiry))
            {
                throw new Exception("Invalid or expired OTP");
            }

            // Clear OTP
            user.OtpCode = null;
            user.OtpExpiry = null;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role.RoleName);

            return new AuthResponse
            {
                Token = token,
                Role = user.Role.RoleName,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Status = user.Status.ToString()
            };
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception($"User not found with email: {email}");
            }

            // Generate token immediately upon request
            var token = Guid.NewGuid().ToString();

            var resetRequest = new PasswordResetRequest
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(1),
                Status = RequestStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetRequests.Add(resetRequest);
            await _context.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var request = await _context.PasswordResetRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);

            if (request == null)
            {
                throw new Exception("Invalid or expired password reset token.");
            }

            // Check if already used
            if (request.Status == RequestStatus.COMPLETED)
            {
                throw new Exception("This password reset link has already been used. Please request a new one.");
            }

            // Check if rejected
            if (request.Status == RequestStatus.REJECTED)
            {
                throw new Exception("This password reset request was rejected. Please contact support.");
            }

            // Check if not approved yet
            if (request.Status != RequestStatus.APPROVED)
            {
                throw new Exception("Password reset request is pending admin approval.");
            }

            // Check expiration
            if (DateTime.UtcNow > request.ExpiryDate)
            {
                throw new Exception("This password reset link has expired. Please request a new one.");
            }

            // Reset password
            request.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            
            // Mark as completed
            request.Status = RequestStatus.COMPLETED;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}