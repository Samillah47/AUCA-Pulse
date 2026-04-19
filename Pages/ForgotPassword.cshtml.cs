using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    [IgnoreAntiforgeryToken]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSendOtpAsync([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return new JsonResult(new { success = false, message = "Email is required" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                // Use HTTPS port from launchSettings
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7255/api";

                var json = JsonSerializer.Serialize(new { email = request.Email });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Auth/password-reset-otp", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true, message = "OTP sent to your email" });
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try {
                        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Wrong email";
                        return new JsonResult(new { success = false, message = message });
                    } catch {
                        return new JsonResult(new { success = false, message = "Wrong email" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostVerifyOtpAsync([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                return new JsonResult(new { success = false, message = "Email and OTP are required" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7255/api";

                var json = JsonSerializer.Serialize(new { email = request.Email, otp = request.Otp });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Auth/verify-reset-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var token = result.GetProperty("token").GetString();
                    return new JsonResult(new { success = true, token = token });
                }
                else
                {
                    try {
                        var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var message = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Invalid OTP";
                        return new JsonResult(new { success = false, message = message });
                    } catch {
                        return new JsonResult(new { success = false, message = "Invalid OTP" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public class EmailRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class VerifyOtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Otp { get; set; } = string.Empty;
        }
    }
}
