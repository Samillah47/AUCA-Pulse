using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages.RoomDetails
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public RoomDetailDto? Room { get; set; }
        public bool CanRelease { get; set; }
        public bool CanOccupy { get; set; }
        public List<RoomScheduleEntry> Schedule { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var response = await client.GetAsync($"{apiUrl}/Room/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Room = JsonSerializer.Deserialize<RoomDetailDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Check if current user can release the room
                    var userId = HttpContext.Session.GetString("UserId");
                    var userRole = HttpContext.Session.GetString("UserRole");
                    CanRelease = Room?.CurrentLecturerId?.ToString() == userId;
                    // Only lecturers can claim a room for a class session
                    CanOccupy = string.Equals(userRole, "LECTURER", StringComparison.OrdinalIgnoreCase);

                    // Load the weekly schedule for this room
                    if (!string.IsNullOrWhiteSpace(Room?.RoomNumber))
                    {
                        var schedRes = await client.GetAsync($"{apiUrl}/LectureSchedule/room/{Uri.EscapeDataString(Room.RoomNumber)}");
                        if (schedRes.IsSuccessStatusCode)
                        {
                            var schedContent = await schedRes.Content.ReadAsStringAsync();
                            Schedule = JsonSerializer.Deserialize<List<RoomScheduleEntry>>(schedContent,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                        }
                    }
                }
            }
            catch (Exception)
            {
                Room = null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostOccupyAsync(int roomId, DateTime occupyUntil)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                // datetime-local input arrives as Kind=Unspecified — mark it UTC
                // before sending so it round-trips cleanly through PostgreSQL.
                var occupiedUntilUtc = DateTime.SpecifyKind(occupyUntil, DateTimeKind.Utc);

                var occupyRequest = new { occupiedUntil = occupiedUntilUtc };
                var json = JsonSerializer.Serialize(occupyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Room/{roomId}/occupy", content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Room occupied successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = ExtractMessage(body)
                        ?? "We couldn't occupy this room. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Something went wrong: {ex.Message}";
            }

            return RedirectToPage(new { id = roomId });
        }

        public async Task<IActionResult> OnPostReleaseAsync(int roomId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var response = await client.PostAsync($"{apiUrl}/Room/{roomId}/release", null);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Room released successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = ExtractMessage(body)
                        ?? "We couldn't release this room. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Something went wrong: {ex.Message}";
            }

            return RedirectToPage(new { id = roomId });
        }

        private static string? ExtractMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
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
            return null;
        }
    }

    public class RoomDetailDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public int? Capacity { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Match the API's field name (currentLecturerId) — previously this was
        // called OccupiedBy and never populated because the JSON key didn't match.
        public int? CurrentLecturerId { get; set; }
        public string? CurrentLecturerName { get; set; }
        public DateTime? OccupiedAt { get; set; }
        public DateTime? OccupiedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoomScheduleEntry
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? RoomNumber { get; set; }
        public string? SemesterName { get; set; }
    }
}
