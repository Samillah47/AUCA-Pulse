using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class LecturerDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LecturerDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public LecturerSummary? Lecturer { get; set; }
        public List<LecturerAssignmentRow> Assignments { get; set; } = new();
        public List<LecturerScheduleRow> Schedule { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var api = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5204/api";
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. Lecturer identity
                var userRes = await client.GetAsync($"{api}/User/{id}");
                if (userRes.IsSuccessStatusCode)
                {
                    Lecturer = JsonSerializer.Deserialize<LecturerSummary>(
                        await userRes.Content.ReadAsStringAsync(), opts);
                }

                if (Lecturer == null)
                {
                    ErrorMessage = "We couldn't find that lecturer.";
                    return Page();
                }

                // 2. Course assignments (each row is one group of a course)
                var caRes = await client.GetAsync($"{api}/courseassignments/lecturer/{id}");
                if (caRes.IsSuccessStatusCode)
                {
                    Assignments = JsonSerializer.Deserialize<List<LecturerAssignmentRow>>(
                        await caRes.Content.ReadAsStringAsync(), opts) ?? new();
                }

                // 3. Weekly schedule (from generated timetable)
                var schedRes = await client.GetAsync($"{api}/LectureSchedule/lecturer/{id}");
                if (schedRes.IsSuccessStatusCode)
                {
                    Schedule = JsonSerializer.Deserialize<List<LecturerScheduleRow>>(
                        await schedRes.Content.ReadAsStringAsync(), opts) ?? new();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }

            return Page();
        }
    }

    public class LecturerSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class LecturerAssignmentRow
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class LecturerScheduleRow
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? RoomNumber { get; set; }
        public string? GroupName { get; set; }
        public string? SemesterName { get; set; }
    }
}
