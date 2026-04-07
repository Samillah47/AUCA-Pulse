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
                        var users = JsonSerializer.Deserialize<List<JsonElement>>(usersContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        TotalUsers = users?.Count ?? 0;
                    }

                    var verificationResponse = await client.GetAsync($"{baseUrl}/verificationrequests/pending");
                    if (verificationResponse.IsSuccessStatusCode)
                    {
                        var verificationContent = await verificationResponse.Content.ReadAsStringAsync();
                        var requests = JsonSerializer.Deserialize<List<JsonElement>>(verificationContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        PendingRequests = requests?.Count ?? 0;
                    }
                }

                var roomsResponse = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsResponse.IsSuccessStatusCode)
                {
                    var roomsContent = await roomsResponse.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<JsonElement>>(roomsContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    AvailableRooms = rooms?.Count(r => r.GetProperty("status").GetString() == "AVAILABLE") ?? 0;
                }

                var lecturersResponse = await client.GetAsync($"{baseUrl}/users");
                if (lecturersResponse.IsSuccessStatusCode)
                {
                    var lecturersContent = await lecturersResponse.Content.ReadAsStringAsync();
                    var allUsers = JsonSerializer.Deserialize<List<JsonElement>>(lecturersContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ActiveLecturers = allUsers?.Count(u => u.GetProperty("roleName").GetString() == "LECTURER") ?? 0;
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
