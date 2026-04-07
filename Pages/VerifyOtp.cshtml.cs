using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class VerifyOtpModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VerifyOtpModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "OTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string OtpCode { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet(string? email)
        {
            // Get email from query parameter or session
            Email = email ?? HttpContext.Session.GetString("Email") ?? string.Empty;

            if (string.IsNullOrEmpty(Email))
            {
                return RedirectToPage("/Login");
            }

            HttpContext.Session.SetString("Email", Email);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Email = HttpContext.Session.GetString("Email") ?? string.Empty;

            if (string.IsNullOrEmpty(Email))
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var verifyRequest = new
                {
                    email = Email,
                    otpCode = OtpCode
                };

                var json = JsonSerializer.Serialize(verifyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Auth/verify-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var data = result.GetProperty("data");

                    // Store authentication data in session
                    HttpContext.Session.SetString("Token", data.GetProperty("token").GetString() ?? string.Empty);
                    HttpContext.Session.SetString("UserId", data.GetProperty("userId").ToString());
                    HttpContext.Session.SetString("UserName", data.GetProperty("name").GetString() ?? string.Empty);
                    HttpContext.Session.SetString("UserRole", data.GetProperty("role").GetString() ?? string.Empty);
                    HttpContext.Session.SetString("Email", Email);

                    // Redirect to dashboard
                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    ErrorMessage = error.TryGetProperty("message", out var msg)
                        ? msg.GetString()
                        : "Invalid OTP code. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }
    }
}
