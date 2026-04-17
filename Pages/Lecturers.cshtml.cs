using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class LecturersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LecturersModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<LecturerDto> Lecturers { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? search)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            SearchQuery = search;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                // Get all lecturers
                var response = await client.GetAsync($"{apiUrl}/User/lecturers");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Lecturers = JsonSerializer.Deserialize<List<LecturerDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<LecturerDto>();

                    // Apply search filter
                    if (!string.IsNullOrEmpty(search))
                    {
                        Lecturers = Lecturers.Where(l =>
                            (l.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (l.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (l.Department?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                    }

                    // Get current status for each lecturer
                    foreach (var lecturer in Lecturers)
                    {
                        try
                        {
                            var statusResponse = await client.GetAsync($"{apiUrl}/LecturerStatus/lecturer/{lecturer.Id}/current");
                            if (statusResponse.IsSuccessStatusCode)
                            {
                                var statusContent = await statusResponse.Content.ReadAsStringAsync();
                                var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent);
                                if (statusData.TryGetProperty("status", out var statusProp))
                                {
                                    lecturer.CurrentStatus = statusProp.GetString();
                                }
                                else if (statusData.TryGetProperty("Status", out var statusPropCap))
                                {
                                    lecturer.CurrentStatus = statusPropCap.GetString();
                                }
                            }
                        }
                        catch
                        {
                            lecturer.CurrentStatus = "UNAVAILABLE";
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to load lecturers. Status: {response.StatusCode}. Error: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }
    }

    public class LecturerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? CurrentStatus { get; set; }
    }
}
