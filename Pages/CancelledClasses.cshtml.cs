using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class CancelledClassesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CancelledClassesModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<CancelledClassRow> Items { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public DateTime EffectiveDate => (Date ?? DateTime.UtcNow).Date;

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");
            if (role != "ADMIN" && role != "STAFF") return RedirectToPage("/AccessDenied");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var api = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var url = $"{api}/LectureSchedule/cancelled?date={EffectiveDate:yyyy-MM-dd}";
                var res = await client.GetAsync(url);
                if (res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync();
                    Items = JsonSerializer.Deserialize<List<CancelledClassRow>>(body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                else
                {
                    ErrorMessage = "We couldn't load cancelled classes right now.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }

            return Page();
        }

        public class CancelledClassRow
        {
            public int Id { get; set; }
            public int LecturerId { get; set; }
            public string LecturerName { get; set; } = string.Empty;
            public string? LecturerEmail { get; set; }
            public string DayOfWeek { get; set; } = string.Empty;
            public string StartTime { get; set; } = string.Empty;
            public string EndTime { get; set; } = string.Empty;
            public string? CourseCode { get; set; }
            public string? CourseName { get; set; }
            public string? RoomNumber { get; set; }
            public string? GroupName { get; set; }
            public string? SemesterName { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string? CancellationReason { get; set; }
        }
    }
}
