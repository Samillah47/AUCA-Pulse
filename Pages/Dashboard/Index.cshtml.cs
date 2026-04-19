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
        public List<LecturerLocationDto> LecturerLocations { get; set; } = new();
        public List<RoomDto> MyRooms { get; set; } = new();
        public int UnreadNotificationsCount { get; set; }
        public string MyCurrentStatus { get; set; } = "AVAILABLE";

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            int userId = int.Parse(userIdStr);
            UserName = HttpContext.Session.GetString("UserName") ?? "User";
            UserRole = HttpContext.Session.GetString("UserRole") ?? "STUDENT";

            // Students never see the admin-style dashboard
            if (UserRole == "STUDENT") return RedirectToPage("/Home");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            try
            {
                // Unread notifications for everyone
                var notifResponse = await client.GetAsync($"{baseUrl}/Notification/user/{userId}/unread-count");
                if (notifResponse.IsSuccessStatusCode)
                {
                    var notifContent = await notifResponse.Content.ReadAsStringAsync();
                    using var notifDoc = JsonDocument.Parse(notifContent);
                    UnreadNotificationsCount = notifDoc.RootElement.GetProperty("count").GetInt32();
                }

                if (UserRole == "LECTURER" || UserRole == "STAFF")
                {
                    var statusResponse = await client.GetAsync($"{baseUrl}/LecturerStatus/lecturer/{userId}/current");
                    if (statusResponse.IsSuccessStatusCode)
                    {
                        var statusContent = await statusResponse.Content.ReadAsStringAsync();
                        var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent);
                        // Check both camelCase and PascalCase
                        if (statusData.TryGetProperty("status", out var sProp)) {
                            MyCurrentStatus = sProp.GetString() ?? "AVAILABLE";
                        } else if (statusData.TryGetProperty("Status", out var sPropCap)) {
                            MyCurrentStatus = sPropCap.GetString() ?? "AVAILABLE";
                        }
                    }
                }

                if (UserRole == "LECTURER")
                {
                    var myRoomsResponse = await client.GetAsync($"{baseUrl}/Room");
                    if (myRoomsResponse.IsSuccessStatusCode)
                    {
                        var roomsContent = await myRoomsResponse.Content.ReadAsStringAsync();
                        var allRooms = JsonSerializer.Deserialize<List<RoomDto>>(roomsContent, 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                        MyRooms = allRooms.Where(r => r.CurrentLecturerId == userId).ToList();
                    }
                }
                // Single /users call — parse TotalUsers + ActiveLecturers from the same response
                var usersResponse = await client.GetAsync($"{baseUrl}/users");
                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersContent = await usersResponse.Content.ReadAsStringAsync();
                    using var usersDoc = JsonDocument.Parse(usersContent);

                    if (UserRole == "ADMIN")
                    {
                        TotalUsers = usersDoc.RootElement.GetArrayLength();
                    }

                    ActiveLecturers = usersDoc.RootElement.EnumerateArray()
                        .Count(u => (u.TryGetProperty("role", out var r) && r.GetString()?.ToUpper() == "LECTURER")
                                 || (u.TryGetProperty("Role", out var r2) && r2.GetString()?.ToUpper() == "LECTURER"));
                }

                if (UserRole == "ADMIN")
                {
                    var verificationResponse = await client.GetAsync($"{baseUrl}/verificationrequests/status/PENDING");
                    if (verificationResponse.IsSuccessStatusCode)
                    {
                        var verificationContent = await verificationResponse.Content.ReadAsStringAsync();
                        using var verificationDoc = JsonDocument.Parse(verificationContent);
                        PendingRequests = verificationDoc.RootElement.GetArrayLength();
                    }
                }

                // Load live lecturer locations for student dashboard
                if (UserRole == "STUDENT" || UserRole == "ADMIN")
                {
                    var locResponse = await client.GetAsync($"{baseUrl}/lecturer-locations");
                    if (locResponse.IsSuccessStatusCode)
                    {
                        var locContent = await locResponse.Content.ReadAsStringAsync();
                        LecturerLocations = JsonSerializer.Deserialize<List<LecturerLocationDto>>(locContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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
            }
            catch
            {
                // Silently fail and show 0 for stats
            }

            return Page();
        }
    }

    public class LecturerLocationDto
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LocationLabel { get; set; } = string.Empty;
        public string? CourseInfo { get; set; }
        public string? UntilTime { get; set; }
        public string? Source { get; set; }
    }
}
