using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class HomeModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public string FirstName { get; set; } = "there";
        public int AvailableRoomCount { get; set; }
        public int UnreadNotifications { get; set; }
        public int LecturerCount { get; set; }
        public List<TodayClassDto> TodayClasses { get; set; } = new();
        public List<AnnouncementDto> Announcements { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var fullName = HttpContext.Session.GetString("UserName") ?? "there";
            FirstName = fullName.Split(' ').FirstOrDefault() ?? fullName;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // Available rooms count
                var roomsRes = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsRes.IsSuccessStatusCode)
                {
                    var content = await roomsRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    AvailableRoomCount = doc.RootElement.EnumerateArray()
                        .Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "AVAILABLE"));
                }

                // Lecturers count
                var lecRes = await client.GetAsync($"{baseUrl}/User/lecturers");
                if (lecRes.IsSuccessStatusCode)
                {
                    var content = await lecRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    LecturerCount = doc.RootElement.GetArrayLength();
                }

                // Today's timetable — current semester, filter by current day
                var semRes = await client.GetAsync($"{baseUrl}/semester/current");
                int? semesterId = null;
                if (semRes.IsSuccessStatusCode)
                {
                    var content = await semRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl)
                        && dataEl.TryGetProperty("id", out var idEl))
                    {
                        semesterId = idEl.GetInt32();
                    }
                }

                if (semesterId.HasValue)
                {
                    var schedRes = await client.GetAsync($"{baseUrl}/LectureSchedule/semester/{semesterId.Value}");
                    if (schedRes.IsSuccessStatusCode)
                    {
                        var content = await schedRes.Content.ReadAsStringAsync();
                        var all = JsonSerializer.Deserialize<List<TodayClassDto>>(content, opts) ?? new();
                        var today = DateTime.UtcNow.DayOfWeek.ToString().ToUpper();
                        TodayClasses = all
                            .Where(s => string.Equals(s.DayOfWeek, today, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(s => s.StartTime)
                            .ToList();
                    }
                }

                // Notifications
                var notifRes = await client.GetAsync($"{baseUrl}/notification/unread");
                if (notifRes.IsSuccessStatusCode)
                {
                    var content = await notifRes.Content.ReadAsStringAsync();
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        UnreadNotifications = doc.RootElement.ValueKind == JsonValueKind.Array
                            ? doc.RootElement.GetArrayLength()
                            : 0;
                    }
                    catch { }
                }

                // Announcements — reuse notification feed for now
                var annRes = await client.GetAsync($"{baseUrl}/notification");
                if (annRes.IsSuccessStatusCode)
                {
                    var content = await annRes.Content.ReadAsStringAsync();
                    try
                    {
                        Announcements = JsonSerializer.Deserialize<List<AnnouncementDto>>(content, opts) ?? new();
                        Announcements = Announcements.Take(5).ToList();
                    }
                    catch { }
                }
            }
            catch
            {
                // Friendly empty state on failure
            }

            return Page();
        }
    }

    public class TodayClassDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? RoomNumber { get; set; }
        public string LecturerName { get; set; } = string.Empty;
    }

    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
