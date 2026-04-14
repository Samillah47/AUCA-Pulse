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

        [BindProperty]
        public int LecturerId { get; set; }
        [BindProperty]
        public int CourseId { get; set; }
        [BindProperty]
        public int SemesterId { get; set; }

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

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new { lecturerId = LecturerId, courseId = CourseId, semesterId = SemesterId };
            var json = JsonSerializer.Serialize(body);
            var response = await client.PostAsync($"{baseUrl}/courseassignments",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course assigned successfully!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed: {err}";
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
            return RedirectToPage();
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
