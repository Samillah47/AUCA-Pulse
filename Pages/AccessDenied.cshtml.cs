using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class AccessDeniedModel : PageModel
    {
        public string? UserRole { get; set; }
        public string? AttemptedPath { get; set; }

        public void OnGet(string? from)
        {
            UserRole = HttpContext.Session.GetString("UserRole");
            AttemptedPath = from;
        }
    }
}
