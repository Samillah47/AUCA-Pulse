using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages.Lecturers
{
    public class RequestAppointmentModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILecturerStatusService _lecturerStatusService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<RequestAppointmentModel> _logger;

        public RequestAppointmentModel(
            IUserService userService,
            ILecturerStatusService lecturerStatusService,
            IAppointmentService appointmentService,
            ILogger<RequestAppointmentModel> logger)
        {
            _userService = userService;
            _lecturerStatusService = lecturerStatusService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public List<dynamic> AvailableStaff { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool ShowOnlyAvailable { get; set; } = false;
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync(bool? showOnlyAvailable = false, string? search = null)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userIdStr) || userRole != "STUDENT")
                {
                    Response.Redirect("/AccessDenied");
                    return;
                }

                // Get all STAFF users - try both "STAFF" and any staff-like role
                var staffUsers = await _userService.GetUsersByRoleNameAsync("STAFF");
                
                _logger.LogInformation($"Found {staffUsers.Count} STAFF users");
                
                AvailableStaff = staffUsers.Cast<dynamic>().ToList();

                // Get current status for each staff member
                foreach (var staff in AvailableStaff)
                {
                    try
                    {
                        int staffId = (staff as dynamic)?.Id;
                        var status = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(staffId);
                        (staff as dynamic).CurrentStatus = status?.Status ?? "AVAILABLE";
                        _logger.LogInformation($"Staff {staffId} status: {(staff as dynamic).CurrentStatus}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to get status for staff: {ex.Message}");
                        (staff as dynamic).CurrentStatus = "AVAILABLE"; // Default to AVAILABLE
                    }
                }

                // Apply filter if requested
                ShowOnlyAvailable = showOnlyAvailable ?? false;
                if (ShowOnlyAvailable)
                {
                    AvailableStaff = AvailableStaff
                        .Where(s => (s as dynamic)?.CurrentStatus?.ToString() == "AVAILABLE")
                        .ToList();
                    _logger.LogInformation($"After AVAILABLE filter: {AvailableStaff.Count} staff");
                }

                // Apply search filter
                SearchQuery = search;
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    AvailableStaff = AvailableStaff
                        .Where(s => ((s as dynamic)?.Name?.ToLower() ?? "").Contains(searchLower) ||
                                   ((s as dynamic)?.Email?.ToLower() ?? "").Contains(searchLower) ||
                                   ((s as dynamic)?.Department?.ToLower() ?? "").Contains(searchLower))
                        .ToList();
                    _logger.LogInformation($"After search '{search}': {AvailableStaff.Count} staff");
                }

                _logger.LogInformation($"Staff list loaded for student with {AvailableStaff.Count} staff members");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading staff list: {ex.Message}\n{ex.StackTrace}");
                ErrorMessage = $"Failed to load staff list: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostRequestAsync(int staffId, string appointmentDate, string appointmentTime, string reason)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var studentId))
                {
                    return RedirectToPage("/Login");
                }

                // Parse date and time
                if (!DateTime.TryParse($"{appointmentDate} {appointmentTime}", out var appointmentDateTime))
                {
                    ErrorMessage = "Invalid date or time format.";
                    await OnGetAsync();
                    return Page();
                }

                // Validate appointment is in future and at least 1 hour away
                var now = DateTime.Now;
                if (appointmentDateTime <= now)
                {
                    ErrorMessage = "Appointment date must be in the future.";
                    await OnGetAsync();
                    return Page();
                }

                if ((appointmentDateTime - now).TotalHours < 1)
                {
                    ErrorMessage = "Appointment must be at least 1 hour from now.";
                    await OnGetAsync();
                    return Page();
                }

                // Check staff current status
                var staffStatus = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(staffId);
                if (staffStatus?.Status != "AVAILABLE")
                {
                    ErrorMessage = "Staff member is no longer available. Please choose another staff member.";
                    await OnGetAsync();
                    return Page();
                }

                // Create appointment
                var createDto = new DTOs.Request.CreateAppointmentDto
                {
                    StaffUserId = staffId,
                    AppointmentDate = appointmentDateTime,
                    Reason = reason
                };

                var result = await _appointmentService.CreateAppointmentAsync(studentId, createDto);
                
                if (result != null)
                {
                    _logger.LogInformation($"Student {studentId} requested appointment with staff {staffId}");
                    SuccessMessage = "Appointment request submitted successfully!";
                    return RedirectToPage("/Student/MyAppointments");
                }
                else
                {
                    ErrorMessage = "Failed to create appointment request.";
                    await OnGetAsync();
                    return Page();
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate appointment prevented for student {StudentId}", HttpContext.Session.GetString("UserId"));
                ErrorMessage = ex.Message;
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating appointment request: {ex.Message}");
                ErrorMessage = "Failed to create appointment request.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}






