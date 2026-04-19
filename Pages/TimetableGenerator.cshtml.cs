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
        public List<TimetableUserDto> LecturersForFilter { get; set; } = new();
        public List<SavedScheduleDto> SavedSchedules { get; set; } = new();
        public TimetableResultDto? LastGenerationResult { get; set; }
        public string? ErrorMessage { get; set; }

        // Filters
        [BindProperty(SupportsGet = true)] public int? SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public int? SelectedLecturerId { get; set; }

        // Generate form parameters
        [BindProperty] public int GenSemesterId { get; set; }
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

            await LoadReferenceData(token);

            // Default selected semester = current if not set
            if (!SelectedSemesterId.HasValue)
            {
                var current = Semesters.FirstOrDefault(s => s.IsCurrent) ?? Semesters.FirstOrDefault();
                SelectedSemesterId = current?.Id;
            }

            if (SelectedSemesterId.HasValue)
            {
                await LoadSavedSchedules(token, SelectedSemesterId.Value);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new
            {
                semesterId = GenSemesterId,
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
                var res = JsonSerializer.Deserialize<TimetableResultDto>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TempData["SuccessMessage"] = $"Timetable generated: {res?.ScheduledCount ?? 0} scheduled, {res?.UnscheduledCount ?? 0} unscheduled.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractFriendlyError(content);
            }

            return RedirectToPage(new { SelectedSemesterId = GenSemesterId });
        }

        private static string ExtractFriendlyError(string raw)
        {
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
            return "We couldn't generate the timetable. Please check the parameters and try again.";
        }

        private async Task LoadReferenceData(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var semRes = await client.GetAsync($"{baseUrl}/semester");
                if (semRes.IsSuccessStatusCode)
                {
                    var content = await semRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        Semesters = JsonSerializer.Deserialize<List<SemesterDto>>(dataEl.GetRawText(), opts) ?? new();
                    }
                }

                var lecRes = await client.GetAsync($"{baseUrl}/User/lecturers");
                if (lecRes.IsSuccessStatusCode)
                {
                    var content = await lecRes.Content.ReadAsStringAsync();
                    LecturersForFilter = JsonSerializer.Deserialize<List<TimetableUserDto>>(content, opts) ?? new();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private async Task LoadSavedSchedules(string token, int semesterId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                var res = await client.GetAsync($"{baseUrl}/LectureSchedule/semester/{semesterId}");
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    SavedSchedules = JsonSerializer.Deserialize<List<SavedScheduleDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading schedules: {ex.Message}";
            }
        }
    }

    public class TimetableUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class SavedScheduleDto
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
        public int? SemesterId { get; set; }
        public string? SemesterName { get; set; }
    }

    public class TimetableResultDto
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int ScheduledCount { get; set; }
        public int UnscheduledCount { get; set; }
    }
}
