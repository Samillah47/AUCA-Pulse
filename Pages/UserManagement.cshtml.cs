using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class UserManagementModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserManagementModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<UserDto> Users { get; set; } = new();

        [BindProperty] public CreateUserInput NewUser { get; set; } = new();

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

            var response = await client.GetAsync($"{baseUrl}/User");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var payload = new
            {
                name = NewUser.Name,
                email = NewUser.Email,
                password = NewUser.Password,
                identificationNumber = NewUser.IdentificationNumber,
                roleType = NewUser.RoleType,
                phoneNumber = NewUser.PhoneNumber,
                department = NewUser.Department
            };
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PostAsync($"{baseUrl}/User",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"New {NewUser.RoleType.ToUpper()} account created for {NewUser.Email}.";
            }
            else
            {
                TempData["ErrorMessage"] = ExtractMessage(content)
                    ?? "We couldn't create the user. Please check the form and try again.";
            }

            return RedirectToPage();
        }

        private static string? ExtractMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var m))
                {
                    var msg = m.GetString();
                    if (!string.IsNullOrWhiteSpace(msg)) return msg;
                }
            }
            catch { }
            return null;
        }

        public async Task<IActionResult> OnPostActivateAsync(int userId)
        {
            var token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var response = await client.PutAsync($"{baseUrl}/User/{userId}/status", new StringContent(
                JsonSerializer.Serialize(new { status = "APPROVED" }, jsonOptions), 
                System.Text.Encoding.UTF8, 
                "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User activated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int userId)
        {
            var token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var response = await client.PutAsync($"{baseUrl}/User/{userId}/status", new StringContent(
                JsonSerializer.Serialize(new { status = "REJECTED" }, jsonOptions), 
                System.Text.Encoding.UTF8, 
                "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User deactivated successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int userId)
        {
            var token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var response = await client.DeleteAsync($"{baseUrl}/User/{userId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToPage();
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreateUserInput
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string RoleType { get; set; } = "STAFF";
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
    }
}
