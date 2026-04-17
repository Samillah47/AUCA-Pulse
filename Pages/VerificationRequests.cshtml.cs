using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class VerificationRequestsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VerificationRequestsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<VerificationRequestDto> Requests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN")
            {
                return RedirectToPage("/Dashboard/Index");
            }

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/verificationrequests/status/PENDING");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Requests = JsonSerializer.Deserialize<List<VerificationRequestDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int requestId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var updateDto = new { status = "APPROVED" };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var content = new StringContent(JsonSerializer.Serialize(updateDto, jsonOptions), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/verificationrequests/{requestId}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User approved successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to approve user. API says: {response.StatusCode}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int requestId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var updateDto = new { status = "REJECTED", rejectionReason = "User does not meet criteria." };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var content = new StringContent(JsonSerializer.Serialize(updateDto, jsonOptions), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/verificationrequests/{requestId}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User rejected successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to reject user. API says: {response.StatusCode}";
            }

            return RedirectToPage();
        }
    }

    public class VerificationRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string SubmittedId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
