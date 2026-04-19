using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class ChatModel : PageModel
    {
        public string WithLecturerName { get; set; } = "Lecturer";
        public int WithLecturerId { get; set; }

        public IActionResult OnGet(int with, string name)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

            WithLecturerId = with;
            WithLecturerName = name ?? "Lecturer";

            return Page();
        }
    }
}
