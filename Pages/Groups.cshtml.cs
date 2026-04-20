using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AUCAPulse.Pages
{
    public class GroupsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GroupsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<GroupListDto> Groups { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty] public string NewName { get; set; } = string.Empty;
        [BindProperty] public string? NewDescription { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            await LoadGroups(token);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            if (string.IsNullOrWhiteSpace(NewName))
            {
                TempData["ErrorMessage"] = "Group name is required.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var body = new { name = NewName.Trim(), description = NewDescription };
            var json = JsonSerializer.Serialize(body);
            var response = await client.PostAsync($"{baseUrl}/groups",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Group '{NewName.Trim()}' created.";
            else
                TempData["ErrorMessage"] = ExtractMessage(content) ?? "We couldn't create the group. Please try again.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int groupId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN") return RedirectToPage("/Dashboard/Index");

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var response = await client.DeleteAsync($"{baseUrl}/groups/{groupId}");
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Group deleted.";
            else
                TempData["ErrorMessage"] = ExtractMessage(content) ?? "We couldn't delete the group.";

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

        private async Task LoadGroups(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var baseUrl = _configuration["ApiSettings:BaseUrl"];

                var res = await client.GetAsync($"{baseUrl}/groups");
                if (res.IsSuccessStatusCode)
                {
                    Groups = JsonSerializer.Deserialize<List<GroupListDto>>(
                        await res.Content.ReadAsStringAsync(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                else
                {
                    ErrorMessage = "We couldn't load the group list.";
                }
            }
            catch
            {
                ErrorMessage = "We couldn't load the group list.";
            }
        }
    }

    public class GroupListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
