using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AUCAPulse.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        // KPI cards
        public int TotalUsers { get; set; }
        public int PendingRequests { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalRooms { get; set; }
        public int ActiveLecturers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int ClassesThisWeek { get; set; }
        public int ClassesToday { get; set; }
        public string CurrentSemesterName { get; set; } = "No active semester";
        public DateTime? CurrentSemesterStart { get; set; }
        public DateTime? CurrentSemesterEnd { get; set; }
        public bool IsSemesterActiveToday { get; set; }

        // Chart data
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public Dictionary<string, int> RoomsByStatus { get; set; } = new();
        public Dictionary<string, int> ClassesByDay { get; set; } = new();

        public List<LecturerLocationDto> LecturerLocations { get; set; } = new();

        // For the lecturer dashboard
        public int MyCoursesCount { get; set; }
        public int MyClassesThisWeek { get; set; }
        public List<TodayClassDto> MyTodayClasses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            UserName = HttpContext.Session.GetString("UserName") ?? "User";
            UserRole = HttpContext.Session.GetString("UserRole") ?? "STUDENT";

            // Students never see the admin-style dashboard
            if (UserRole == "STUDENT") return RedirectToPage("/Home");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // Users: counts by role + totals
                var usersRes = await client.GetAsync($"{baseUrl}/users");
                if (usersRes.IsSuccessStatusCode)
                {
                    var content = await usersRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var arr = doc.RootElement;
                    TotalUsers = arr.GetArrayLength();
                    foreach (var u in arr.EnumerateArray())
                    {
                        var role = u.TryGetProperty("role", out var r)
                            ? r.GetString()
                            : (u.TryGetProperty("Role", out var r2) ? r2.GetString() : null);
                        role = (role ?? "UNKNOWN").ToUpperInvariant();
                        UsersByRole.TryGetValue(role, out var existing);
                        UsersByRole[role] = existing + 1;
                    }
                    ActiveLecturers = UsersByRole.GetValueOrDefault("LECTURER");
                }

                // Rooms: counts by status + totals
                var roomsRes = await client.GetAsync($"{baseUrl}/rooms");
                if (roomsRes.IsSuccessStatusCode)
                {
                    var content = await roomsRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var arr = doc.RootElement;
                    TotalRooms = arr.GetArrayLength();
                    foreach (var room in arr.EnumerateArray())
                    {
                        var status = room.TryGetProperty("status", out var s)
                            ? s.GetString()
                            : (room.TryGetProperty("Status", out var s2) ? s2.GetString() : null);
                        status = (status ?? "UNKNOWN").ToUpperInvariant();
                        RoomsByStatus.TryGetValue(status, out var existing);
                        RoomsByStatus[status] = existing + 1;
                    }
                    AvailableRooms = RoomsByStatus.GetValueOrDefault("AVAILABLE");
                }

                // Pending verifications (admin)
                if (UserRole == "ADMIN")
                {
                    var verRes = await client.GetAsync($"{baseUrl}/verificationrequests/status/PENDING");
                    if (verRes.IsSuccessStatusCode)
                    {
                        var content = await verRes.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);
                        PendingRequests = doc.RootElement.GetArrayLength();
                    }
                }

                // Courses
                var courseRes = await client.GetAsync($"{baseUrl}/courses");
                if (courseRes.IsSuccessStatusCode)
                {
                    var content = await courseRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    TotalCourses = doc.RootElement.GetArrayLength();
                }

                // Course assignments (one per course+semester+group)
                var caRes = await client.GetAsync($"{baseUrl}/courseassignments");
                if (caRes.IsSuccessStatusCode)
                {
                    var content = await caRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    TotalAssignments = doc.RootElement.GetArrayLength();
                }

                // Current semester name + date window
                var semRes = await client.GetAsync($"{baseUrl}/semester/current");
                int? currentSemesterId = null;
                if (semRes.IsSuccessStatusCode)
                {
                    var content = await semRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var d))
                    {
                        if (d.TryGetProperty("name", out var n)) CurrentSemesterName = n.GetString() ?? CurrentSemesterName;
                        if (d.TryGetProperty("id", out var i)) currentSemesterId = i.GetInt32();
                        if (d.TryGetProperty("startDate", out var sd) && sd.TryGetDateTime(out var s)) CurrentSemesterStart = s;
                        if (d.TryGetProperty("endDate", out var ed) && ed.TryGetDateTime(out var e)) CurrentSemesterEnd = e;
                    }
                }
                var todayDate = DateTime.UtcNow.Date;
                IsSemesterActiveToday = currentSemesterId.HasValue
                    && (!CurrentSemesterStart.HasValue || CurrentSemesterStart.Value.Date <= todayDate)
                    && (!CurrentSemesterEnd.HasValue || CurrentSemesterEnd.Value.Date >= todayDate);

                // Classes by day of week (from saved LectureSchedule of the current semester)
                var weekDays = new[] { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" };
                foreach (var d in weekDays) ClassesByDay[d] = 0;

                if (currentSemesterId.HasValue)
                {
                    var schedRes = await client.GetAsync($"{baseUrl}/LectureSchedule/semester/{currentSemesterId.Value}");
                    if (schedRes.IsSuccessStatusCode)
                    {
                        var content = await schedRes.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);
                        var arr = doc.RootElement;
                        ClassesThisWeek = arr.GetArrayLength();
                        var today = DateTime.UtcNow.DayOfWeek.ToString().ToUpperInvariant();
                        foreach (var s in arr.EnumerateArray())
                        {
                            var day = s.TryGetProperty("dayOfWeek", out var dw) ? dw.GetString() : null;
                            day = (day ?? string.Empty).ToUpperInvariant();
                            if (ClassesByDay.ContainsKey(day)) ClassesByDay[day]++;
                            if (day == today && IsSemesterActiveToday) ClassesToday++;
                        }
                    }
                }

                // Live lecturer locations (admin)
                if (UserRole == "ADMIN")
                {
                    var locRes = await client.GetAsync($"{baseUrl}/lecturer-locations");
                    if (locRes.IsSuccessStatusCode)
                    {
                        LecturerLocations = JsonSerializer.Deserialize<List<LecturerLocationDto>>(
                            await locRes.Content.ReadAsStringAsync(), opts) ?? new();
                    }
                }

                // Lecturer-specific: own courses + schedule
                if (UserRole == "LECTURER")
                {
                    var myId = HttpContext.Session.GetString("UserId");
                    if (!string.IsNullOrEmpty(myId))
                    {
                        var myCoursesRes = await client.GetAsync($"{baseUrl}/courseassignments/lecturer/{myId}");
                        if (myCoursesRes.IsSuccessStatusCode)
                        {
                            using var doc = JsonDocument.Parse(await myCoursesRes.Content.ReadAsStringAsync());
                            MyCoursesCount = doc.RootElement.GetArrayLength();
                        }

                        var mySchedRes = await client.GetAsync($"{baseUrl}/LectureSchedule/lecturer/{myId}");
                        if (mySchedRes.IsSuccessStatusCode)
                        {
                            var mySched = JsonSerializer.Deserialize<List<TodayClassDto>>(
                                await mySchedRes.Content.ReadAsStringAsync(), opts) ?? new();
                            MyClassesThisWeek = mySched.Count;
                            var today = DateTime.UtcNow.DayOfWeek.ToString().ToUpperInvariant();
                            MyTodayClasses = mySched
                                .Where(s => string.Equals(s.DayOfWeek, today, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(s => s.StartTime)
                                .ToList();
                        }
                    }
                }
            }
            catch
            {
                // Fail silently; widgets just show 0
            }

            return Page();
        }
    }

    public class LecturerLocationDto
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LocationLabel { get; set; } = string.Empty;
        public string? CourseInfo { get; set; }
        public string? UntilTime { get; set; }
        public string? Source { get; set; }
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
    }
}
