using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class MyOfficeModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public MyOfficeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public OfficeDto? Office { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
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

                var response = await client.GetAsync($"{apiUrl}/Office/my-office");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Office = JsonSerializer.Deserialize<OfficeDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "You don't have an office assigned yet. Please contact the administrator.";
                }
                else
                {
                    ErrorMessage = "Failed to load office information.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string status)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // First get the office to know the ID
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5204/api";

                var getResponse = await client.GetAsync($"{apiUrl}/Office/my-office");
                if (!getResponse.IsSuccessStatusCode)
                {
                    ErrorMessage = "Office not found.";
                    return await OnGetAsync();
                }

                var content = await getResponse.Content.ReadAsStringAsync();
                var office = JsonSerializer.Deserialize<OfficeDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (office != null)
                {
                    var updateRequest = new { availabilityStatus = status };
                    var json = JsonSerializer.Serialize(updateRequest);
                    var putContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync($"{apiUrl}/Office/{office.Id}/availability", putContent);

                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = $"Office status updated to {status}!";
                    }
                    else
                    {
                        ErrorMessage = "Failed to update office status.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return await OnGetAsync();
        }
    }

    public class OfficeDto
    {
        public int Id { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeNumber { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string? PhoneExtension { get; set; }
        public string? Department { get; set; }
    }
}
