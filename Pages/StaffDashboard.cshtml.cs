using AUCAPulse.Models;
using AUCAPulse.Services;
using AUCAPulse.DTOs.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class StaffDashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IOfficeService _officeService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILecturerStatusService _lecturerStatusService;
        private readonly ILogger<StaffDashboardModel> _logger;

        public StaffDashboardModel(
            IUserService userService,
            IOfficeService officeService,
            IAppointmentService appointmentService,
            ILecturerStatusService lecturerStatusService,
            ILogger<StaffDashboardModel> logger)
        {
            _userService = userService;
            _officeService = officeService;
            _appointmentService = appointmentService;
            _lecturerStatusService = lecturerStatusService;
            _logger = logger;
        }

        public dynamic? StaffUser { get; set; }
        public dynamic? Office { get; set; }
        public List<dynamic> PendingAppointments { get; set; } = new();
        public List<dynamic> ApprovedUpcomingAppointments { get; set; } = new();
        public List<dynamic> AllAppointments { get; set; } = new();
        public string? AvailabilityStatus { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
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

                // Load current lecturer status
                var lecturerStatus = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(userId);
                AvailabilityStatus = lecturerStatus?.Status ?? "AVAILABLE";

                // Load office info
                Office = await _officeService.GetOfficeByUserIdAsync(userId);

                // Load pending appointments
                var allAppointments = await _appointmentService.GetAppointmentsByStaffAsync(userId);
                AllAppointments = allAppointments.Cast<dynamic>().ToList();
                
                PendingAppointments = AllAppointments
                    .Where(a => (a as dynamic)?.Status?.ToString() == "PENDING")
                    .Cast<dynamic>()
                    .ToList();
                
                ApprovedUpcomingAppointments = AllAppointments
                    .Where(a => (a as dynamic)?.Status?.ToString() == "APPROVED" && 
                           (a as dynamic)?.AppointmentDate >= DateTime.Now)
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

        public async Task<IActionResult> OnPostApproveAsync(int appointmentId)
        {
            try
            {
                if (!TryGetStaffId(out var staffId))
                {
                    return RedirectToPage("/AccessDenied");
                }

                var request = new UpdateAppointmentStatusDto { Status = "APPROVED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request, staffId);
                SuccessMessage = "Appointment approved.";
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You can only manage appointments assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving appointment from dashboard");
                ErrorMessage = "Failed to approve appointment.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int appointmentId)
        {
            try
            {
                if (!TryGetStaffId(out var staffId))
                {
                    return RedirectToPage("/AccessDenied");
                }

                var request = new UpdateAppointmentStatusDto { Status = "REJECTED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request, staffId);
                SuccessMessage = "Appointment rejected.";
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You can only manage appointments assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting appointment from dashboard");
                ErrorMessage = "Failed to reject appointment.";
            }

            return RedirectToPage();
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
                if (Enum.TryParse<Status>(newStatus, true, out var status))
                {
                    // Create a new lecturer status record
                    var statusDto = new CreateLecturerStatusDto { Status = status };
                    var result = await _lecturerStatusService.CreateStatusAsync(userId, statusDto);
                    
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

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating status: {ex.Message}");
                ErrorMessage = "Failed to update status.";
                return RedirectToPage();
            }
        }

        private bool TryGetStaffId(out int staffId)
        {
            staffId = 0;
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            return userRole == "STAFF"
                && !string.IsNullOrEmpty(userIdStr)
                && int.TryParse(userIdStr, out staffId);
        }
    }
}

