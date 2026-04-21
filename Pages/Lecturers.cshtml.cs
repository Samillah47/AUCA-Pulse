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

        public IConfiguration Configuration => _configuration;
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
            var currentUserId = HttpContext.Session.GetString("UserId");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                // Get all staff (lecturers and staff members)
                var response = await client.GetAsync($"{apiUrl}/User/staff");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var allStaff = JsonSerializer.Deserialize<List<LecturerDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<LecturerDto>();

                    // Filter out the current user and apply search filter
                    Lecturers = allStaff.Where(l => l.Id.ToString() != currentUserId).ToList();

                    if (!string.IsNullOrEmpty(search))
                    {
                        Lecturers = Lecturers.Where(l =>
                            (l.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (l.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (l.Department?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                    }

                    // Use the merged lecturer-locations endpoint — it combines
                    // active LectureSchedule (for IN_CLASS), latest LecturerStatus,
                    // and Office availability, and falls back to AVAILABLE for
                    // approved lecturers. One call instead of N.
                    var statusByLecturerId = new Dictionary<int, string>();
                    try
                    {
                        var locRes = await client.GetAsync($"{apiUrl}/lecturer-locations");
                        if (locRes.IsSuccessStatusCode)
                        {
                            var locContent = await locRes.Content.ReadAsStringAsync();
                            var locs = JsonSerializer.Deserialize<List<LecturerLocationMini>>(locContent,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                            foreach (var l in locs)
                            {
                                if (!string.IsNullOrWhiteSpace(l.Status))
                                    statusByLecturerId[l.LecturerId] = l.Status!;
                            }
                        }
                    }
                    catch { /* fall through — each row still renders fine */ }

                    foreach (var lecturer in Lecturers)
                    {
                        if (statusByLecturerId.TryGetValue(lecturer.Id, out var status))
                        {
                            lecturer.CurrentStatus = status;
                            continue;
                        }

                        // STAFF members aren't returned by lecturer-locations (it
                        // only considers LECTURERs). For them, fall back to the
                        // single-user status endpoint or default to AVAILABLE.
                        try
                        {
                            var statusResponse = await client.GetAsync($"{apiUrl}/LecturerStatus/lecturer/{lecturer.Id}/current");
                            if (statusResponse.IsSuccessStatusCode)
                            {
                                var statusContent = await statusResponse.Content.ReadAsStringAsync();
                                var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent);
                                if (statusData.TryGetProperty("status", out var statusProp))
                                    lecturer.CurrentStatus = statusProp.GetString();
                                else if (statusData.TryGetProperty("Status", out var statusPropCap))
                                    lecturer.CurrentStatus = statusPropCap.GetString();
                            }
                            if (string.IsNullOrWhiteSpace(lecturer.CurrentStatus))
                                lecturer.CurrentStatus = "AVAILABLE";
                        }
                        catch
                        {
                            lecturer.CurrentStatus = "AVAILABLE";
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

    // Minimal shape of LecturerLocationResponse just for status lookups on this page
    internal class LecturerLocationMini
    {
        public int LecturerId { get; set; }
        public string? Status { get; set; }
    }
}
