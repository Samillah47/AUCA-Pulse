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
        public List<GroupDto> Groups { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty] public int CourseId { get; set; }
        [BindProperty] public int SemesterId { get; set; }
        [BindProperty] public int LecturerId { get; set; }
        [BindProperty] public int GroupId { get; set; }

        [BindProperty] public int CopySourceSemesterId { get; set; }
        [BindProperty] public int CopyTargetSemesterId { get; set; }

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

            if (CourseId <= 0 || SemesterId <= 0 || LecturerId <= 0 || GroupId <= 0)
            {
                TempData["ErrorMessage"] = "Please choose a course, semester, lecturer, and group.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new
            {
                lecturerId = LecturerId,
                courseId = CourseId,
                semesterId = SemesterId,
                groupId = GroupId
            };
            var json = JsonSerializer.Serialize(body);
            var response = await client.PostAsync($"{baseUrl}/courseassignments",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course assigned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content)
                    ?? "We couldn't save the assignment. Please check the form and try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCopyAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            if (CopySourceSemesterId <= 0 || CopyTargetSemesterId <= 0 || CopySourceSemesterId == CopyTargetSemesterId)
            {
                TempData["ErrorMessage"] = "Please pick two different semesters to copy between.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = JsonSerializer.Serialize(new { sourceSemesterId = CopySourceSemesterId, targetSemesterId = CopyTargetSemesterId });
            var res = await client.PostAsync($"{baseUrl}/courseassignments/copy-from-semester",
                new StringContent(body, Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = ExtractMessage(content) ?? "Assignments copied.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content) ?? "We couldn't copy the assignments. Please try again.";
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
                TempData["SuccessMessage"] = "Assignment removed.";
            else
                TempData["ErrorMessage"] = "Could not remove the assignment. Please try again.";

            return RedirectToPage();
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
                    Lecturers = JsonSerializer.Deserialize<List<UserDto>>(await lecRes.Content.ReadAsStringAsync(), opts) ?? new();

                var courseRes = await client.GetAsync($"{baseUrl}/courses");
                if (courseRes.IsSuccessStatusCode)
                    Courses = JsonSerializer.Deserialize<List<CourseDto>>(await courseRes.Content.ReadAsStringAsync(), opts) ?? new();

                var semRes = await client.GetAsync($"{baseUrl}/semester");
                if (semRes.IsSuccessStatusCode)
                {
                    var content = await semRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                        Semesters = JsonSerializer.Deserialize<List<SemesterDto>>(dataEl.GetRawText(), opts) ?? new();
                }

                var grpRes = await client.GetAsync($"{baseUrl}/groups");
                if (grpRes.IsSuccessStatusCode)
                    Groups = JsonSerializer.Deserialize<List<GroupDto>>(await grpRes.Content.ReadAsStringAsync(), opts) ?? new();

                var assignRes = await client.GetAsync($"{baseUrl}/courseassignments");
                if (assignRes.IsSuccessStatusCode)
                    Assignments = JsonSerializer.Deserialize<List<AssignmentDto>>(await assignRes.Content.ReadAsStringAsync(), opts) ?? new();
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
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class SemesterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }

    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
