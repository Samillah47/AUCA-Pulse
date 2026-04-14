using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages.Dashboard
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

        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int PendingRequests { get; set; }
        public int AvailableRooms { get; set; }
        public int ActiveLecturers { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            UserName = HttpContext.Session.GetString("UserName") ?? "User";
            UserRole = HttpContext.Session.GetString("UserRole") ?? "STUDENT";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            try
            {
                if (UserRole == "ADMIN")
                {
                    var usersResponse = await client.GetAsync($"{baseUrl}/users");
                    if (usersResponse.IsSuccessStatusCode)
                    {
                        var usersContent = await usersResponse.Content.ReadAsStringAsync();
                        using var usersDoc = JsonDocument.Parse(usersContent);
                        TotalUsers = usersDoc.RootElement.GetArrayLength();
                    }

                    var verificationResponse = await client.GetAsync($"{baseUrl}/verificationrequests/status/PENDING");
                    if (verificationResponse.IsSuccessStatusCode)
                    {
                        var verificationContent = await verificationResponse.Content.ReadAsStringAsync();
                        using var verificationDoc = JsonDocument.Parse(verificationContent);
                        PendingRequests = verificationDoc.RootElement.GetArrayLength();
                    }
                }

                var roomsResponse = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsResponse.IsSuccessStatusCode)
                {
                    var roomsContent = await roomsResponse.Content.ReadAsStringAsync();
                    using var roomsDoc = JsonDocument.Parse(roomsContent);
                    AvailableRooms = roomsDoc.RootElement.EnumerateArray()
                        .Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "AVAILABLE") 
                                 || (r.TryGetProperty("Status", out var s2) && s2.GetString()?.ToUpper() == "AVAILABLE"));
                }

                var lecturersResponse = await client.GetAsync($"{baseUrl}/users");
                if (lecturersResponse.IsSuccessStatusCode)
                {
                    var lecturersContent = await lecturersResponse.Content.ReadAsStringAsync();
                    using var allUsersDoc = JsonDocument.Parse(lecturersContent);
                    ActiveLecturers = allUsersDoc.RootElement.EnumerateArray()
                        .Count(u => (u.TryGetProperty("role", out var r) && r.GetString()?.ToUpper() == "LECTURER")
                                 || (u.TryGetProperty("Role", out var r2) && r2.GetString()?.ToUpper() == "LECTURER"));
                }
            }
            catch
            {
                // Silently fail and show 0 for stats
            }

            return Page();
        }
    }
}
