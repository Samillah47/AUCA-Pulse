using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
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
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? search, string? status)
        {
            // Check authentication
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            SearchQuery = search;

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
                            r.RoomNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            r.Building.Contains(search, StringComparison.OrdinalIgnoreCase)
                        ).ToList();
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        Rooms = Rooms.Where(r => r.Status == status).ToList();
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
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
