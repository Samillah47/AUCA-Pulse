using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResetPasswordModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)] public string? Token { get; set; }
        [BindProperty] public string NewPassword { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["ErrorMessage"] = "This reset link is missing its token. Please request a new one.";
                return RedirectToPage("/ForgotPassword");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["ErrorMessage"] = "This reset link is no longer valid. Please request a new one.";
                return RedirectToPage("/ForgotPassword");
            }
            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Your new password must be at least 6 characters long.";
                return Page();
            }
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "The two passwords you entered don't match.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                var body = JsonSerializer.Serialize(new { token = Token, newPassword = NewPassword });
                var response = await client.PostAsync($"{baseUrl}/auth/reset-password",
                    new StringContent(body, Encoding.UTF8, "application/json"));

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Your password has been updated. Please sign in with your new password.";
                    return RedirectToPage("/Login");
                }

                TempData["ErrorMessage"] = ExtractMessage(content);
                return Page();
            }
            catch
            {
                TempData["ErrorMessage"] = "We couldn't reach the server. Please try again in a moment.";
                return Page();
            }
        }

        private static string ExtractMessage(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var m))
                {
                    var msg = m.GetString();
                    if (!string.IsNullOrWhiteSpace(msg)) return msg;
                }
            }
            catch { }
            return "We couldn't reset your password. Please request a new reset link.";
        }
    }
}
