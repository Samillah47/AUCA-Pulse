using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty] public string Email { get; set; } = string.Empty;
        public bool Submitted { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                TempData["ErrorMessage"] = "Please enter your email address.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = JsonSerializer.Serialize(new { email = Email });
            try
            {
                await client.PostAsync($"{baseUrl}/auth/forgot-password",
                    new StringContent(body, Encoding.UTF8, "application/json"));
            }
            catch
            {
                // Intentionally silent — we always tell the user the same thing
            }

            Submitted = true;
            return Page();
        }
    }
}
