using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages.Student
{
    public class TimetableModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TimetableModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<SavedScheduleDto> Schedules { get; set; } = new();
        public string SemesterName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public int? LecturerId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Resolve current semester
            var semRes = await client.GetAsync($"{baseUrl}/semester/current");
            int? semesterId = null;
            if (semRes.IsSuccessStatusCode)
            {
                var content = await semRes.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    if (dataEl.TryGetProperty("id", out var idEl)) semesterId = idEl.GetInt32();
                    if (dataEl.TryGetProperty("name", out var nameEl)) SemesterName = nameEl.GetString() ?? "";
                }
            }

            if (!semesterId.HasValue) return Page();

            var res = await client.GetAsync($"{baseUrl}/LectureSchedule/semester/{semesterId.Value}");
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                Schedules = JsonSerializer.Deserialize<List<SavedScheduleDto>>(content, opts) ?? new();
                if (LecturerId.HasValue)
                {
                    Schedules = Schedules.Where(s => s.LecturerId == LecturerId.Value).ToList();
                }
            }

            return Page();
        }
    }
}
