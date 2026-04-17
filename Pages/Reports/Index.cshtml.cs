using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public ReportData Data { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "ADMIN")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                // Get Users for Role distribution
                var usersResponse = await client.GetAsync($"{baseUrl}/users");
                if (usersResponse.IsSuccessStatusCode)
                {
                    var content = await usersResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var users = doc.RootElement.EnumerateArray().ToList();
                    
                    Data.TotalUsers = users.Count;
                    Data.StudentsCount = users.Count(u => (u.TryGetProperty("role", out var r) && r.GetString()?.ToUpper() == "STUDENT")
                                                       || (u.TryGetProperty("Role", out var r2) && r2.GetString()?.ToUpper() == "STUDENT"));
                    Data.LecturersCount = users.Count(u => (u.TryGetProperty("role", out var r) && r.GetString()?.ToUpper() == "LECTURER")
                                                        || (u.TryGetProperty("Role", out var r2) && r2.GetString()?.ToUpper() == "LECTURER"));
                    Data.StaffCount = users.Count(u => (u.TryGetProperty("role", out var r) && r.GetString()?.ToUpper() == "STAFF")
                                                     || (u.TryGetProperty("Role", out var r2) && r2.GetString()?.ToUpper() == "STAFF"));
                }

                // Get Rooms for Status distribution
                var roomsResponse = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsResponse.IsSuccessStatusCode)
                {
                    var content = await roomsResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var rooms = doc.RootElement.EnumerateArray().ToList();

                    Data.TotalRooms = rooms.Count;
                    Data.AvailableRooms = rooms.Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "AVAILABLE")
                                                        || (r.TryGetProperty("Status", out var s2) && s2.GetString()?.ToUpper() == "AVAILABLE"));
                    Data.OccupiedRooms = rooms.Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "OCCUPIED")
                                                       || (r.TryGetProperty("Status", out var s2) && s2.GetString()?.ToUpper() == "OCCUPIED"));
                    Data.MaintenanceRooms = rooms.Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "MAINTENANCE")
                                                          || (r.TryGetProperty("Status", out var s2) && s2.GetString()?.ToUpper() == "MAINTENANCE"));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading report data: {ex.Message}";
            }

            return Page();
        }
    }

    public class ReportData
    {
        public int TotalUsers { get; set; }
        public int StudentsCount { get; set; }
        public int LecturersCount { get; set; }
        public int StaffCount { get; set; }
        
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
    }
}
