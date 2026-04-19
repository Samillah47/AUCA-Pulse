using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class CourseAssignmentModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CourseAssignmentModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<AssignmentDto> Assignments { get; set; } = new();
        public List<UserDto> Lecturers { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<SemesterDto> Semesters { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty] public int CourseId { get; set; }
        [BindProperty] public int SemesterId { get; set; }
        [BindProperty] public List<int> LecturerIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            await LoadData(token);
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            if (LecturerIds == null || LecturerIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one lecturer.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new { courseId = CourseId, semesterId = SemesterId, lecturerIds = LecturerIds };
            var json = JsonSerializer.Serialize(body);
            var response = await client.PostAsync($"{baseUrl}/courseassignments/bulk",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var created = doc.RootElement.TryGetProperty("created", out var c) ? c.GetInt32() : 0;
                    var skipped = doc.RootElement.TryGetProperty("skippedDuplicates", out var s) ? s.GetInt32() : 0;
                    var invalid = doc.RootElement.TryGetProperty("invalidLecturers", out var i) ? i.GetInt32() : 0;

                    var parts = new List<string>();
                    if (created > 0) parts.Add($"{created} new assignment(s) created");
                    if (skipped > 0) parts.Add($"{skipped} already existed");
                    if (invalid > 0) parts.Add($"{invalid} invalid");

                    if (created > 0)
                        TempData["SuccessMessage"] = string.Join(", ", parts) + ".";
                    else
                        TempData["WarningMessage"] = parts.Count > 0
                            ? string.Join(", ", parts) + "."
                            : "No changes were made.";
                }
                catch
                {
                    TempData["SuccessMessage"] = "Assignments saved.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = ExtractFriendlyError(content);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int assignmentId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var response = await client.DeleteAsync($"{baseUrl}/courseassignments/{assignmentId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Assignment removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove the assignment. Please try again.";
            }
            return RedirectToPage();
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
            return "We couldn't save the assignments. Please check the form and try again.";
        }

        private async Task LoadData(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var lecRes = await client.GetAsync($"{baseUrl}/User/lecturers");
                if (lecRes.IsSuccessStatusCode)
                {
                    var content = await lecRes.Content.ReadAsStringAsync();
                    Lecturers = JsonSerializer.Deserialize<List<UserDto>>(content, opts) ?? new();
                }

                var courseRes = await client.GetAsync($"{baseUrl}/courses");
                if (courseRes.IsSuccessStatusCode)
                {
                    var content = await courseRes.Content.ReadAsStringAsync();
                    Courses = JsonSerializer.Deserialize<List<CourseDto>>(content, opts) ?? new();
                }

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

                var assignRes = await client.GetAsync($"{baseUrl}/courseassignments");
                if (assignRes.IsSuccessStatusCode)
                {
                    var content = await assignRes.Content.ReadAsStringAsync();
                    Assignments = JsonSerializer.Deserialize<List<AssignmentDto>>(content, opts) ?? new();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class SemesterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }
}
