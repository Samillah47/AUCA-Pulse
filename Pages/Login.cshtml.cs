using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("Token") != null)
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = "http://localhost:5113/api";
                }

                var loginRequest = new
                {
                    email = Email,
                    password = Password
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{baseUrl}/Auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Store email in session for OTP verification
                    HttpContext.Session.SetString("Email", Email);
                    
                    // Redirect to OTP verification
                    return RedirectToPage("/VerifyOtp", new { email = Email });
                }
                else
                {
                    var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    ErrorMessage = error.TryGetProperty("message", out var msg) 
                        ? msg.GetString() 
                        : "Login failed. Please check your credentials.";
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
