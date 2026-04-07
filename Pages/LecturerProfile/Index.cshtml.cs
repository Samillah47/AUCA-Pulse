using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages.LecturerProfile
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

        public LecturerDetailDto? Lecturer { get; set; }
        public string? CurrentStatus { get; set; }
        public OfficeDto? Office { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
        public List<StatusHistoryDto> StatusHistory { get; set; } = new();

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

                // Get lecturer details
                var userResponse = await client.GetAsync($"{apiUrl}/User/{id}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var content = await userResponse.Content.ReadAsStringAsync();
                    Lecturer = JsonSerializer.Deserialize<LecturerDetailDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                // Get current status
                try
                {
                    var statusResponse = await client.GetAsync($"{apiUrl}/LecturerStatus/lecturer/{id}/current");
                    if (statusResponse.IsSuccessStatusCode)
                    {
                        var statusContent = await statusResponse.Content.ReadAsStringAsync();
                        var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent);
                        CurrentStatus = statusData.GetProperty("status").GetString();
                    }
                }
                catch { }

                // Get office information
                try
                {
                    var officeResponse = await client.GetAsync($"{apiUrl}/Office/staff/{id}");
                    if (officeResponse.IsSuccessStatusCode)
                    {
                        var officeContent = await officeResponse.Content.ReadAsStringAsync();
                        Office = JsonSerializer.Deserialize<OfficeDto>(officeContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch { }

                // Get lecture schedule
                try
                {
                    var scheduleResponse = await client.GetAsync($"{apiUrl}/LectureSchedule/lecturer/{id}");
                    if (scheduleResponse.IsSuccessStatusCode)
                    {
                        var scheduleContent = await scheduleResponse.Content.ReadAsStringAsync();
                        Schedules = JsonSerializer.Deserialize<List<ScheduleDto>>(scheduleContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<ScheduleDto>();
                    }
                }
                catch { }

                // Get status history
                try
                {
                    var historyResponse = await client.GetAsync($"{apiUrl}/LecturerStatus/lecturer/{id}");
                    if (historyResponse.IsSuccessStatusCode)
                    {
                        var historyContent = await historyResponse.Content.ReadAsStringAsync();
                        StatusHistory = JsonSerializer.Deserialize<List<StatusHistoryDto>>(historyContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<StatusHistoryDto>();
                    }
                }
                catch { }
            }
            catch (Exception)
            {
                Lecturer = null;
            }

            return Page();
        }
    }

    public class LecturerDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
    }

    public class OfficeDto
    {
        public string OfficeNumber { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string? RegularHours { get; set; }
    }

    public class ScheduleDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
    }

    public class StatusHistoryDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
