using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc;
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
        public List<dynamic> PaginatedAppointments { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalAppointments { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalAppointments / (double)PageSize);

        public string? StatusFilter { get; set; }
        public string? DateFilter { get; set; }

        public async Task OnGetAsync(int? pageNumber, string? status, string? filter)
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

                if (userRole == "STAFF")
                {
                    var appointments = await _appointmentService.GetAppointmentsByStaffAsync(userId);
                    Appointments = appointments.Cast<dynamic>().ToList();
                }
                else if (userRole == "STUDENT")
                {
                    var appointments = await _appointmentService.GetAppointmentsByStudentAsync(userId);
                    Appointments = appointments.Cast<dynamic>().ToList();
                }
                else
                {
                    Response.Redirect("/AccessDenied");
                    return;
                }

                StatusFilter = status;
                DateFilter = filter;

                if (!string.IsNullOrEmpty(status))
                {
                    Appointments = Appointments
                        .Where(a => string.Equals((a as dynamic)?.Status?.ToString(), status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (string.Equals(filter, "upcoming", StringComparison.OrdinalIgnoreCase))
                {
                    Appointments = Appointments
                        .Where(a => (a as dynamic)?.AppointmentDate >= DateTime.Now)
                        .ToList();
                }

                Appointments = string.Equals(filter, "upcoming", StringComparison.OrdinalIgnoreCase)
                    ? Appointments.OrderBy(a => (a as dynamic)?.AppointmentDate).ToList()
                    : Appointments.OrderByDescending(a => (a as dynamic)?.AppointmentDate).ToList();

                TotalAppointments = Appointments.Count;
                PageNumber = pageNumber ?? 1;

                if (PageNumber < 1)
                    PageNumber = 1;
                if (PageNumber > TotalPages && TotalPages > 0)
                    PageNumber = TotalPages;

                PaginatedAppointments = Appointments
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading appointments: {ex.Message}");
                ErrorMessage = "Failed to load appointments.";
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int appointmentId, int pageNumber, string? statusFilter, string? dateFilter)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (userRole != "STAFF" || string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var staffId))
                {
                    return RedirectToPage("/AccessDenied");
                }

                var request = new DTOs.Request.UpdateAppointmentStatusDto { Status = "APPROVED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request, staffId);
                SuccessMessage = "Appointment approved.";
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You can only manage appointments assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving appointment: {ex.Message}");
                ErrorMessage = "Failed to approve appointment.";
            }

            return RedirectToPage(new { pageNumber, status = statusFilter, filter = dateFilter });
        }

        public async Task<IActionResult> OnPostRejectAsync(int appointmentId, string? rejectionReason, int pageNumber, string? statusFilter, string? dateFilter)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (userRole != "STAFF" || string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var staffId))
                {
                    return RedirectToPage("/AccessDenied");
                }

                var request = new DTOs.Request.UpdateAppointmentStatusDto
                {
                    Status = "REJECTED",
                    Reason = rejectionReason
                };

                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request, staffId);
                SuccessMessage = "Appointment rejected.";
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You can only manage appointments assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting appointment: {ex.Message}");
                ErrorMessage = "Failed to reject appointment.";
            }

            return RedirectToPage(new { pageNumber, status = statusFilter, filter = dateFilter });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int appointmentId, int pageNumber, string? statusFilter, string? dateFilter)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (userRole != "STAFF" || string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var staffId))
                {
                    return RedirectToPage("/AccessDenied");
                }

                var request = new DTOs.Request.UpdateAppointmentStatusDto { Status = "COMPLETED" };
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request, staffId);
                SuccessMessage = "Appointment marked as completed.";
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You can only manage appointments assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing appointment: {ex.Message}");
                ErrorMessage = "Failed to mark appointment as completed.";
            }

            return RedirectToPage(new { pageNumber, status = statusFilter, filter = dateFilter });
        }
    }
}
