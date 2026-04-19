using AUCAPulse.Models;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class StaffDashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IOfficeService _officeService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<StaffDashboardModel> _logger;

        public StaffDashboardModel(
            IUserService userService,
            IOfficeService officeService,
            IAppointmentService appointmentService,
            ILogger<StaffDashboardModel> logger)
        {
            _userService = userService;
            _officeService = officeService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public dynamic? StaffUser { get; set; }
        public dynamic? Office { get; set; }
        public List<dynamic> PendingAppointments { get; set; } = new();
        public string? AvailabilityStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Response.Redirect("/Login");
                    return;
                }

                if (userRole != "STAFF")
                {
                    Response.Redirect("/AccessDenied");
                    return;
                }

                // Load staff user info
                StaffUser = await _userService.GetUserByIdAsync(userId);
                if (StaffUser != null)
                {
                    AvailabilityStatus = (StaffUser as dynamic)?.AvailabilityStatus?.ToString() ?? "AWAY";
                }

                // Load office info
                Office = await _officeService.GetOfficeByUserIdAsync(userId);

                // Load pending appointments
                var allAppointments = await _appointmentService.GetAppointmentsByStaffAsync(userId);
                PendingAppointments = allAppointments
                    .Where(a => (a as dynamic)?.Status?.ToString() == "PENDING")
                    .Cast<dynamic>()
                    .ToList();

                _logger.LogInformation($"Staff dashboard loaded for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading staff dashboard: {ex.Message}");
                ErrorMessage = "Failed to load dashboard.";
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string newStatus)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return RedirectToPage("/Login");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToPage("/Login");
                }

                // Parse the new status
                if (Enum.TryParse<AvailabilityStatus>(newStatus, true, out var status))
                {
                    var result = await _userService.UpdateUserAvailabilityStatusAsync(userId, status);
                    if (result != null)
                    {
                        SuccessMessage = $"Status changed to {status}";
                        _logger.LogInformation($"Staff {userId} changed availability status to {status}");
                    }
                    else
                    {
                        ErrorMessage = "Failed to update status.";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid status value.";
                }

                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating status: {ex.Message}");
                ErrorMessage = "Failed to update status.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}


