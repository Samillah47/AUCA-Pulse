namespace AUCAPulse.Middleware
{
    /// <summary>
    /// Session-based route protection for Razor Pages.
    ///
    /// Reads `UserRole` from the current session and, for the requested path,
    /// decides whether the user is allowed through:
    ///   - public pages always pass,
    ///   - API requests are left to JWT authorization,
    ///   - signed-out users on protected pages go to /Login,
    ///   - signed-in users requesting a page their role can't open go to /AccessDenied.
    ///
    /// This runs AFTER UseSession and BEFORE the Razor Pages endpoint is invoked,
    /// so any page-level redirects still work, but nobody can reach a page by
    /// typing a URL if their role is wrong.
    /// </summary>
    public class RoleGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleGuardMiddleware> _logger;

        public RoleGuardMiddleware(RequestDelegate next, ILogger<RoleGuardMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Paths that don't need a login at all.
        private static readonly string[] PublicPrefixes =
        {
            "/Login", "/Signup", "/VerifyOtp",
            "/ForgotPassword", "/ResetPassword",
            "/Error", "/AccessDenied", "/Privacy",
            "/Logout"
        };

        // Role rules, checked in order. First match wins.
        // A null role set means "any signed-in user".
        private static readonly (string Prefix, string[]? AllowedRoles)[] Rules =
        {
            // Student portal
            ("/Home",              new[] { "STUDENT" }),
            ("/Student",           new[] { "STUDENT" }),

            // Staff portal
            ("/StaffDashboard",    new[] { "STAFF", "ADMIN" }),
            ("/MyOffice",          new[] { "STAFF", "LECTURER", "ADMIN" }),
            ("/OfficeManagement",  new[] { "ADMIN" }),

            // Lecturer/admin operations
            ("/Schedule",          new[] { "LECTURER", "ADMIN" }),
            ("/UpdateStatus",      new[] { "LECTURER", "STAFF", "ADMIN" }),
            ("/Dashboard",         new[] { "ADMIN", "LECTURER", "STAFF" }),

            // Admin-only control-plane pages
            ("/VerificationRequests", new[] { "ADMIN" }),
            ("/UserManagement",       new[] { "ADMIN" }),
            ("/Courses",              new[] { "ADMIN" }),
            ("/CourseAssignment",     new[] { "ADMIN" }),
            ("/Groups",               new[] { "ADMIN" }),
            ("/TimetableGenerator",   new[] { "ADMIN" }),
            ("/Reports",              new[] { "ADMIN" }),
            ("/CancelledClasses",     new[] { "ADMIN" }),

            // Shared pages — anyone signed in
            ("/Appointments",      new[] { "STAFF", "LECTURER", "STUDENT", "ADMIN" }),
            ("/Lecturers",         null),   // everyone authed can browse
            ("/LecturerProfile",   null),
            ("/LecturerDetail",    null),
            ("/Rooms",             null),
            ("/RoomDetails",       null),
            ("/Notifications",     null),
            ("/Profile",           null)
        };

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? "/";

            // API: JWT middleware/[Authorize] handles it — don't touch
            if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }

            // Static files, Razor runtime, health probes — let them through
            if (path.Contains('.') ||
                path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }

            // Public pages and the root index
            if (path == "/" || IsPublic(path))
            {
                await _next(ctx);
                return;
            }

            // Everything below requires a session
            var token = ctx.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                ctx.Response.Redirect("/Login");
                return;
            }

            var role = (ctx.Session.GetString("UserRole") ?? string.Empty).ToUpperInvariant();

            // Find the first matching rule
            foreach (var (prefix, allowed) in Rules)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (allowed == null || allowed.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        await _next(ctx);
                        return;
                    }

                    _logger.LogWarning("Role guard denied {Role} access to {Path}", role, path);
                    ctx.Response.Redirect($"/AccessDenied?from={Uri.EscapeDataString(path)}");
                    return;
                }
            }

            // No rule matched — let it through rather than accidentally locking
            // a legitimate page. Protected pages should be listed above.
            await _next(ctx);
        }

        private static bool IsPublic(string path)
        {
            foreach (var p in PublicPrefixes)
            {
                if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
