using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class TimetableGeneratorModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TimetableGeneratorModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<SemesterDto> Semesters { get; set; } = new();
        public TimetableResultDto? Result { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public int SemesterId { get; set; }
        [BindProperty] public int StartHour { get; set; } = 8;
        [BindProperty] public int EndHour { get; set; } = 18;
        [BindProperty] public int SlotDurationMinutes { get; set; } = 120;
        [BindProperty] public bool ReplaceExisting { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            await LoadSemesters(token);
            return Page();
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            await LoadSemesters(token);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new
            {
                semesterId = SemesterId,
                startHour = StartHour,
                endHour = EndHour,
                slotDurationMinutes = SlotDurationMinutes,
                replaceExisting = ReplaceExisting
            };

            var json = JsonSerializer.Serialize(body);
            var response = await client.PostAsync($"{baseUrl}/timetable/generate",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Result = JsonSerializer.Deserialize<TimetableResultDto>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                ErrorMessage = $"Generation failed: {content}";
            }

            return Page();
        }

        private async Task LoadSemesters(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                var res = await client.GetAsync($"{baseUrl}/semester");
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        Semesters = JsonSerializer.Deserialize<List<SemesterDto>>(dataEl.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading semesters: {ex.Message}";
            }
        }
    }

    public class TimetableResultDto
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int ScheduledCount { get; set; }
        public int UnscheduledCount { get; set; }
        public List<GeneratedScheduleEntryDto> Scheduled { get; set; } = new();
        public List<UnscheduledAssignmentDto> Unscheduled { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class GeneratedScheduleEntryDto
    {
        public int ScheduleId { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }

    public class UnscheduledAssignmentDto
    {
        public int AssignmentId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
