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

        [BindProperty]
        public CreateCourseInput NewCourse { get; set; } = new();

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

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course created successfully!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to create course: {err}";
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
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to delete course: {err}";
            }

            return RedirectToPage();
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
                    ErrorMessage = "Failed to load courses.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
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
}
