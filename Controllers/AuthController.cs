using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var result = await _authService.VerifyOtpAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"OTP verification error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request.Email);
                return Ok(new { message = "Password reset request submitted successfully. It is now pending admin approval." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("password-reset-otp")]
        public async Task<IActionResult> RequestPasswordResetOtp([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.RequestPasswordResetOtpAsync(request.Email);
                return Ok(new { message = "OTP sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset OTP error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var token = await _authService.VerifyPasswordResetOtpAsync(request.Email, request.Otp);
                return Ok(new { message = "OTP verified successfully.", token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verify reset OTP error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Token, request.Password);
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
