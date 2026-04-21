using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using AUCAPulse.Services;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Pages
{
    public class HomeModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IAppointmentService _appointmentService;
        private readonly ILecturerStatusService _lecturerStatusService;
        private readonly INotificationService _notificationService;

        public HomeModel(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IAppointmentService appointmentService,
            ILecturerStatusService lecturerStatusService,
            INotificationService notificationService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _appointmentService = appointmentService;
            _lecturerStatusService = lecturerStatusService;
            _notificationService = notificationService;
        }

        public string FirstName { get; set; } = "there";
        public int AvailableRoomCount { get; set; }
        public int UnreadNotifications { get; set; }
        public int AvailableLecturerCount { get; set; }
        public List<TodayClassDto> TodayClasses { get; set; } = new();
        public List<AnnouncementDto> Announcements { get; set; } = new();
        public List<AppointmentResponse> UpcomingAppointments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            var userIdStr = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) 
                return RedirectToPage("/Login");

            var fullName = HttpContext.Session.GetString("UserName") ?? "there";
            FirstName = fullName.Split(' ').FirstOrDefault() ?? fullName;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // Available rooms count
                var roomsRes = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsRes.IsSuccessStatusCode)
                {
                    var content = await roomsRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    AvailableRoomCount = doc.RootElement.EnumerateArray()
                        .Count(r => (r.TryGetProperty("status", out var s) && s.GetString()?.ToUpper() == "AVAILABLE"));
                }

                // Available Lecturers count
                var lecturers = await _lecturerStatusService.GetAllStatusesAsync();
                AvailableLecturerCount = lecturers.Count(l => l.Status == "AVAILABLE");

                // Upcoming Appointments
                var allAppointments = await _appointmentService.GetAppointmentsByStudentAsync(userId);
                UpcomingAppointments = allAppointments
                    .Where(a => a.AppointmentDate >= DateTime.UtcNow && a.Status != "CANCELLED" && a.Status != "REJECTED")
                    .OrderBy(a => a.AppointmentDate)
                    .Take(3)
                    .ToList();

                // Today's timetable
                var semRes = await client.GetAsync($"{baseUrl}/semester/current");
                int? semesterId = null;
                DateTime? semStart = null, semEnd = null;
                if (semRes.IsSuccessStatusCode)
                {
                    var content = await semRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataEl))
                    {
                        if (dataEl.TryGetProperty("id", out var idEl)) semesterId = idEl.GetInt32();
                        if (dataEl.TryGetProperty("startDate", out var sd) && sd.TryGetDateTime(out var s)) semStart = s;
                        if (dataEl.TryGetProperty("endDate", out var ed) && ed.TryGetDateTime(out var e)) semEnd = e;
                    }
                }

                var todayUtc = DateTime.UtcNow.Date;
                var semesterActiveToday = semesterId.HasValue
                    && (!semStart.HasValue || semStart.Value.Date <= todayUtc)
                    && (!semEnd.HasValue || semEnd.Value.Date >= todayUtc);

                if (semesterId.HasValue && semesterActiveToday)
                {
                    var schedRes = await client.GetAsync($"{baseUrl}/LectureSchedule/semester/{semesterId.Value}");
                    if (schedRes.IsSuccessStatusCode)
                    {
                        var content = await schedRes.Content.ReadAsStringAsync();
                        var all = JsonSerializer.Deserialize<List<TodayClassDto>>(content, opts) ?? new();
                        var todayName = DateTime.UtcNow.DayOfWeek.ToString().ToUpper();
                        TodayClasses = all
                            .Where(s => string.Equals(s.DayOfWeek, todayName, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(s => s.StartTime)
                            .ToList();
                    }
                }

                // Notifications
                UnreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

                // Announcements - Add dummy data
                var notifs = await _notificationService.GetNotificationsByUserIdAsync(userId);
                var dbAnnouncements = notifs
                    .Where(n => n.Type == Models.NotificationType.INFO || n.Type == Models.NotificationType.WARNING)
                    .Take(5)
                    .Select(n => new AnnouncementDto 
                    { 
                        Id = n.Id, 
                        Title = n.Title, 
                        Message = n.Message, 
                        Type = n.Type.ToString(), 
                        CreatedAt = n.CreatedAt 
                    })
                    .ToList();

                // Add dummy announcements if none exist
                if (!dbAnnouncements.Any())
                {
                    Announcements = new List<AnnouncementDto>
                    {
                        new AnnouncementDto
                        {
                            Id = -1,
                            Title = "Final Exams Schedule",
                            Message = "Final examinations will begin on May 3rd, 2025. Please check your timetable for specific dates and times.",
                            Type = "INFO",
                            CreatedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new AnnouncementDto
                        {
                            Id = -2,
                            Title = "Library Extended Hours",
                            Message = "The library will be open 24/7 during exam period from April 28th to May 15th.",
                            Type = "INFO",
                            CreatedAt = DateTime.UtcNow.AddDays(-5)
                        },
                        new AnnouncementDto
                        {
                            Id = -3,
                            Title = "Registration Deadline",
                            Message = "Course registration for next semester closes on May 20th. Don't miss the deadline!",
                            Type = "WARNING",
                            CreatedAt = DateTime.UtcNow.AddDays(-7)
                        },
                        new AnnouncementDto
                        {
                            Id = -4,
                            Title = "Campus Maintenance",
                            Message = "Scheduled maintenance in Building A on April 30th from 8 AM to 12 PM. Classes will be relocated.",
                            Type = "WARNING",
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        },
                        new AnnouncementDto
                        {
                            Id = -5,
                            Title = "Student Awards Ceremony",
                            Message = "Join us for the annual Student Excellence Awards on May 25th at 3 PM in the Main Auditorium.",
                            Type = "INFO",
                            CreatedAt = DateTime.UtcNow.AddDays(-12)
                        }
                    };
                }
                else
                {
                    Announcements = dbAnnouncements;
                }
            }
            catch
            {
                // Friendly empty state on failure
            }

            return Page();
        }
    }

    public class TodayClassDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? RoomNumber { get; set; }
        public string? GroupName { get; set; }
        public string LecturerName { get; set; } = string.Empty;
    }

    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
