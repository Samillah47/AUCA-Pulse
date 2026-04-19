using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public MessagesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration => _configuration;

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");
            
            return Page();
        }
    }
}
