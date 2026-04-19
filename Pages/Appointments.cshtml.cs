using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    public class AppointmentsModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsModel> _logger;

        public AppointmentsModel(IAppointmentService appointmentService, ILogger<AppointmentsModel> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public List<dynamic> Appointments { get; set; } = new();
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

                // For staff - show appointments they received
                if (userRole == "STAFF")
                {
                    var appointments = await _appointmentService.GetAppointmentsByStaffAsync(userId);
                    Appointments = appointments.Cast<dynamic>().ToList();
                }
                // For students - show appointments they requested
                else if (userRole == "STUDENT")
                {
                    var appointments = await _appointmentService.GetAppointmentsByStudentAsync(userId);
                    Appointments = appointments.Cast<dynamic>().ToList();
                }
                else
                {
                    Response.Redirect("/AccessDenied");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading appointments: {ex.Message}");
                ErrorMessage = "Failed to load appointments.";
            }
        }

        public async Task OnPostApproveAsync(int appointmentId)
        {
            try
            {
                var request = new DTOs.Request.UpdateAppointmentStatusDto { Status = "APPROVED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request);
                SuccessMessage = "Appointment approved!";
                await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving appointment: {ex.Message}");
                ErrorMessage = "Failed to approve appointment.";
            }
        }

        public async Task OnPostRejectAsync(int appointmentId)
        {
            try
            {
                var request = new DTOs.Request.UpdateAppointmentStatusDto { Status = "REJECTED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request);
                SuccessMessage = "Appointment rejected!";
                await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting appointment: {ex.Message}");
                ErrorMessage = "Failed to reject appointment.";
            }
        }
    }
}

