using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages.Student
{
    public class MyAppointmentsModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IOfficeService _officeService;
        private readonly ILecturerStatusService _lecturerStatusService;
        private readonly IUserService _userService;
        private readonly ILogger<MyAppointmentsModel> _logger;

        public MyAppointmentsModel(
            IAppointmentService appointmentService,
            IOfficeService officeService,
            ILecturerStatusService lecturerStatusService,
            IUserService userService,
            ILogger<MyAppointmentsModel> logger)
        {
            _appointmentService = appointmentService;
            _officeService = officeService;
            _lecturerStatusService = lecturerStatusService;
            _userService = userService;
            _logger = logger;
        }

        public List<AppointmentItem> PendingAppointments { get; set; } = new();
        public List<AppointmentItem> ApprovedAppointments { get; set; } = new();
        public List<AppointmentItem> RejectedAppointments { get; set; } = new();
        public List<AppointmentItem> CompletedAppointments { get; set; } = new();
        public List<AppointmentItem> PaginatedAppointments { get; set; } = new();
        public List<OfficeBookingOption> OfficeOptions { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public int SelectedOfficeId { get; set; }

        [BindProperty]
        public string? AppointmentDate { get; set; }

        [BindProperty]
        public string? AppointmentTime { get; set; }

        [BindProperty]
        public string? AppointmentReason { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalAppointments { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalAppointments / (double)PageSize);
        public string FilterTab { get; set; } = "pending";

        public string MinBookingDate => DateTime.Today.ToString("yyyy-MM-dd");
        public string MaxBookingDate => DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");

        public async Task OnGetAsync(int? pageNumber, string? tab)
        {
            if (!TryGetStudentId(out var studentId))
            {
                Response.Redirect("/Login");
                return;
            }

            await LoadPageAsync(studentId, pageNumber ?? 1, tab);
        }

        public async Task<IActionResult> OnPostRequestNewAsync(int? pageNumber, string? tab)
        {
            if (!TryGetStudentId(out var studentId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                if (SelectedOfficeId <= 0)
                {
                    ErrorMessage = "Please choose an office to visit.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(AppointmentDate) || string.IsNullOrWhiteSpace(AppointmentTime))
                {
                    ErrorMessage = "Please provide both appointment date and time.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                if (!DateTime.TryParse($"{AppointmentDate} {AppointmentTime}", out var appointmentDateTime))
                {
                    ErrorMessage = "Invalid date or time format.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                var now = DateTime.Now;
                if (appointmentDateTime <= now.AddHours(1))
                {
                    ErrorMessage = "Appointment must be at least 1 hour from now.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                if (appointmentDateTime > now.AddDays(30))
                {
                    ErrorMessage = "Appointment must be within the next 30 days.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                var appointmentTimeOfDay = appointmentDateTime.TimeOfDay;
                if (appointmentTimeOfDay < new TimeSpan(8, 0, 0) || appointmentTimeOfDay > new TimeSpan(17, 0, 0))
                {
                    ErrorMessage = "Appointment time must be between 08:00 and 17:00.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                var officeOptions = await BuildOfficeOptionsAsync();
                var selectedOffice = officeOptions.FirstOrDefault(o => o.OfficeId == SelectedOfficeId);
                if (selectedOffice == null)
                {
                    ErrorMessage = "The selected office no longer exists.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                if (!selectedOffice.StaffUserId.HasValue)
                {
                    ErrorMessage = "This office has no assigned staff yet.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                if (!string.Equals(selectedOffice.CurrentStatus, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Staff is not currently available. Please choose another office or try later.";
                    await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                    return Page();
                }

                var createDto = new CreateAppointmentDto
                {
                    StaffUserId = selectedOffice.StaffUserId.Value,
                    AppointmentDate = appointmentDateTime,
                    Reason = string.IsNullOrWhiteSpace(AppointmentReason) ? null : AppointmentReason.Trim()
                };

                await _appointmentService.CreateAppointmentAsync(studentId, createDto);
                SuccessMessage = "Appointment request sent! Waiting for staff approval.";
                return RedirectToPage(new { tab = "pending" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate appointment prevented for student {StudentId}", studentId);
                ErrorMessage = ex.Message;
                await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment request from MyAppointments");
                ErrorMessage = "Failed to create appointment request.";
                await LoadPageAsync(studentId, pageNumber ?? 1, tab);
                return Page();
            }
        }

        private async Task LoadPageAsync(int studentId, int pageNumber, string? tab)
        {
            FilterTab = NormalizeTab(tab);
            OfficeOptions = await BuildOfficeOptionsAsync();

            var appointments = await _appointmentService.GetAppointmentsByStudentAsync(studentId);
            var appointmentItems = await BuildAppointmentItemsAsync(appointments);

            PendingAppointments = appointmentItems.Where(a => a.Status == "PENDING").ToList();
            ApprovedAppointments = appointmentItems.Where(a => a.Status == "APPROVED").ToList();
            RejectedAppointments = appointmentItems.Where(a => a.Status == "REJECTED").ToList();
            CompletedAppointments = appointmentItems.Where(a => a.Status == "COMPLETED").ToList();

            var filteredAppointments = FilterTab switch
            {
                "approved" => ApprovedAppointments,
                "rejected" => RejectedAppointments,
                "completed" => CompletedAppointments,
                _ => PendingAppointments
            };

            TotalAppointments = filteredAppointments.Count;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            if (PageNumber > TotalPages && TotalPages > 0)
            {
                PageNumber = TotalPages;
            }

            PaginatedAppointments = filteredAppointments
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private async Task<List<AppointmentItem>> BuildAppointmentItemsAsync(List<DTOs.Response.AppointmentResponse> appointments)
        {
            var officeByStaffId = new Dictionary<int, string>();
            var uniqueStaffIds = appointments.Select(a => a.StaffUserId).Distinct().ToList();

            foreach (var staffId in uniqueStaffIds)
            {
                var office = await _officeService.GetOfficeByUserIdAsync(staffId);
                officeByStaffId[staffId] = office == null
                    ? "No office assigned"
                    : $"{office.OfficeName} ({office.OfficeNumber})";
            }

            return appointments
                .Select(a => new AppointmentItem
                {
                    Id = a.Id,
                    StaffName = a.StaffName,
                    StaffEmail = a.StaffEmail,
                    OfficeName = officeByStaffId.TryGetValue(a.StaffUserId, out var officeName) ? officeName : "No office assigned",
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = (a.Status ?? "PENDING").ToUpperInvariant(),
                    CreatedAt = a.CreatedAt
                })
                .OrderBy(a => a.AppointmentDate)
                .ToList();
        }

        private async Task<List<OfficeBookingOption>> BuildOfficeOptionsAsync()
        {
            var offices = await _officeService.GetAllOfficesAsync();
            var officeOptions = new List<OfficeBookingOption>();

            foreach (var office in offices)
            {
                var option = new OfficeBookingOption
                {
                    OfficeId = office.Id,
                    OfficeName = office.OfficeName,
                    OfficeNumber = office.OfficeNumber,
                    Building = office.Building,
                    StaffUserId = office.StaffUserId,
                    StaffName = office.StaffUserName,
                    StaffEmail = office.StaffUserEmail,
                    Department = office.Department,
                    CurrentStatus = "UNAVAILABLE"
                };

                if (office.StaffUserId.HasValue)
                {
                    var staff = await _userService.GetUserByIdAsync(office.StaffUserId.Value);
                    if (!string.IsNullOrWhiteSpace(staff?.Department))
                    {
                        option.Department = staff.Department;
                    }

                    var currentStatus = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(office.StaffUserId.Value);
                    option.CurrentStatus = (currentStatus?.Status ?? "AVAILABLE").ToUpperInvariant();
                }

                option.IsBookable = option.StaffUserId.HasValue && option.CurrentStatus == "AVAILABLE";
                officeOptions.Add(option);
            }

            return officeOptions
                .OrderBy(o => o.OfficeName)
                .ThenBy(o => o.OfficeNumber)
                .ToList();
        }

        private bool TryGetStudentId(out int studentId)
        {
            studentId = 0;
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            return userRole == "STUDENT"
                && !string.IsNullOrEmpty(userIdStr)
                && int.TryParse(userIdStr, out studentId);
        }

        private static string NormalizeTab(string? tab)
        {
            var normalized = (tab ?? "pending").Trim().ToLowerInvariant();
            return normalized is "approved" or "rejected" or "completed" ? normalized : "pending";
        }

        public class AppointmentItem
        {
            public int Id { get; set; }
            public string StaffName { get; set; } = string.Empty;
            public string StaffEmail { get; set; } = string.Empty;
            public string OfficeName { get; set; } = string.Empty;
            public DateTime AppointmentDate { get; set; }
            public string? Reason { get; set; }
            public string Status { get; set; } = "PENDING";
            public DateTime CreatedAt { get; set; }
        }

        public class OfficeBookingOption
        {
            public int OfficeId { get; set; }
            public string OfficeName { get; set; } = string.Empty;
            public string OfficeNumber { get; set; } = string.Empty;
            public string? Building { get; set; }
            public int? StaffUserId { get; set; }
            public string? StaffName { get; set; }
            public string? StaffEmail { get; set; }
            public string? Department { get; set; }
            public string CurrentStatus { get; set; } = "UNAVAILABLE";
            public bool IsBookable { get; set; }
        }
    }
}
