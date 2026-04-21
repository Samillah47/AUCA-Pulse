using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class UpdateStatusModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UpdateStatusModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public string SelectedStatus { get; set; } = "AVAILABLE";

        [BindProperty]
        public string? Notes { get; set; }

        public string CurrentStatus { get; set; } = "AVAILABLE";
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"];

                var response = await client.GetAsync($"{apiUrl}/LecturerStatus/my-status");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    CurrentStatus = doc.RootElement.GetProperty("status").GetString() ?? "AVAILABLE";
                    SelectedStatus = CurrentStatus;
                    Notes = doc.RootElement.TryGetProperty("notes", out var n) ? n.GetString() : "";
                }
            }
            catch { /* Fallback to default */ }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"];

                var body = new
                {
                    status = SelectedStatus,
                    notes = Notes
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/LecturerStatus", content);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Your status has been updated successfully.";
                    CurrentStatus = SelectedStatus;
                }
                else
                {
                    ErrorMessage = "Failed to update status. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }
    }
}
