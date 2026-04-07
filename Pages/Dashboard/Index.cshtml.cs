using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int PendingRequests { get; set; }
        public int AvailableRooms { get; set; }
        public int ActiveLecturers { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // Get user info from session
            UserName = HttpContext.Session.GetString("UserName") ?? "User";
            UserRole = HttpContext.Session.GetString("UserRole") ?? "STUDENT";

            // Mock data for dashboard stats (in real app, fetch from API)
            TotalUsers = 150;
            PendingRequests = 5;
            AvailableRooms = 12;
            ActiveLecturers = 25;

            return Page();
        }
    }
}
