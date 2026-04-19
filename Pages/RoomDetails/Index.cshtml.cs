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
                    CanRelease = Room?.OccupiedBy?.ToString() == userId;

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
                var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                // Token is already retrieved at line 62

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var occupyRequest = new
                {
                    occupiedUntil = occupyUntil
                };

                var json = JsonSerializer.Serialize(occupyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiUrl}/Room/{roomId}/occupy", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Room occupied successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to occupy room. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
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

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Room released successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to release room. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToPage(new { id = roomId });
        }
    }

    public class RoomDetailDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? OccupiedBy { get; set; }
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
