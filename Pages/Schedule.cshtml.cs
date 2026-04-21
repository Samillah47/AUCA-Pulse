using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class ScheduleModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ScheduleModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<ScheduleItemDto> Schedules { get; set; } = new();
        public string? CurrentStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? AccessToken { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }
            AccessToken = token;

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "LECTURER")
            {
                return RedirectToPage("/Dashboard/Index");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                // Get lecturer's schedule
                var scheduleResponse = await client.GetAsync($"{apiUrl}/LectureSchedule/lecturer/{userId}");
                if (scheduleResponse.IsSuccessStatusCode)
                {
                    var content = await scheduleResponse.Content.ReadAsStringAsync();
                    Schedules = JsonSerializer.Deserialize<List<ScheduleItemDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ScheduleItemDto>();
                }

                // Get current status
                try
                {
                    var statusResponse = await client.GetAsync($"{apiUrl}/LecturerStatus/lecturer/{userId}/current");
                    if (statusResponse.IsSuccessStatusCode)
                    {
                        var statusContent = await statusResponse.Content.ReadAsStringAsync();
                        var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent);
                        if (statusData.TryGetProperty("status", out var statusProp))
                        {
                            CurrentStatus = statusProp.GetString();
                        }
                        else if (statusData.TryGetProperty("Status", out var statusPropCap))
                        {
                            CurrentStatus = statusPropCap.GetString();
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }
    }

    public class ScheduleItemDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public DateTime? CancelledOn { get; set; }
        public string? CancellationReason { get; set; }
    }
}
