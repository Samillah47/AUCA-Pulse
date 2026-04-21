using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class CoursesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CoursesModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<CourseDto> Courses { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty] public CreateCourseInput NewCourse { get; set; } = new();
        [BindProperty] public EditCourseInput EditCourse { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            await LoadCourses(token);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(NewCourse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = await client.PostAsync($"{baseUrl}/courses",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content)
                    ?? "We couldn't create the course. Please check the form and try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            if (EditCourse.Id <= 0)
            {
                TempData["ErrorMessage"] = "Could not find the course to update.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var payload = new
            {
                courseCode = EditCourse.CourseCode,
                courseName = EditCourse.CourseName,
                credits = EditCourse.Credits,
                department = EditCourse.Department,
                description = EditCourse.Description
            };
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PutAsync($"{baseUrl}/courses/{EditCourse.Id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content)
                    ?? "We couldn't update the course. Please check the form and try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int courseId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var response = await client.DeleteAsync($"{baseUrl}/courses/{courseId}");
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content)
                    ?? "We couldn't delete the course. Please try again.";
            }

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

        private async Task LoadCourses(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                var response = await client.GetAsync($"{baseUrl}/courses");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Courses = JsonSerializer.Deserialize<List<CourseDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                else
                {
                    ErrorMessage = "We couldn't load the course list. Please refresh the page.";
                }
            }
            catch
            {
                ErrorMessage = "We couldn't load the course list. Please refresh the page.";
            }
        }
    }

    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Department { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCourseInput
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; } = 3;
        public string? Department { get; set; }
        public string? Description { get; set; }
    }

    public class EditCourseInput
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; } = 3;
        public string? Department { get; set; }
        public string? Description { get; set; }
    }
}
