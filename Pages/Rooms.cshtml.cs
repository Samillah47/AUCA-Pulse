using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class RoomsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RoomsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<RoomDto> Rooms { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? SelectedStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string UserRole { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? search, string? status)
        {
            // Check authentication
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            SearchQuery = search;
            SelectedStatus = status;
            UserRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var response = await client.GetAsync($"{apiUrl}/Room");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Rooms = JsonSerializer.Deserialize<List<RoomDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<RoomDto>();

                    // Apply filters
                    if (!string.IsNullOrEmpty(search))
                    {
                        Rooms = Rooms.Where(r =>
                            (r.RoomNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (r.RoomName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (r.Building?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        Rooms = Rooms.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }
                else
                {
                    ErrorMessage = "Failed to load rooms. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAutoAssignAsync(string? roomTypeFilter, int? durationMinutes)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var body = new
                {
                    roomType = string.IsNullOrWhiteSpace(roomTypeFilter) ? null : roomTypeFilter,
                    durationMinutes = durationMinutes ?? 120
                };
                var json = JsonSerializer.Serialize(body);
                var response = await client.PostAsync($"{apiUrl}/rooms/auto-assign",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(content);
                    var roomNumber = doc.RootElement.TryGetProperty("roomNumber", out var rn) ? rn.GetString() : "?";
                    var building = doc.RootElement.TryGetProperty("building", out var b) ? b.GetString() : "";
                    var until = doc.RootElement.TryGetProperty("occupiedUntil", out var u) && u.ValueKind != JsonValueKind.Null
                        ? u.GetDateTime().ToLocalTime().ToString("HH:mm")
                        : "";
                    TempData["AutoAssignSuccess"] = $"Round Robin assigned you room {roomNumber} ({building}) until {until}.";
                }
                else
                {
                    TempData["AutoAssignError"] = $"Auto-assign failed: {content}";
                }
            }
            catch (Exception ex)
            {
                TempData["AutoAssignError"] = $"Error: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
