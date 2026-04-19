using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AUCAPulse.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions. Logs the full error server-side and returns
    /// a friendly, non-technical response — JSON for API calls, redirect for UI.
    /// Well-known exception types (duplicate key, foreign key violation, not found)
    /// are translated to plain-English messages before being sent to the client.
    /// </summary>
    public class FriendlyExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FriendlyExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public FriendlyExceptionMiddleware(
            RequestDelegate next,
            ILogger<FriendlyExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.Response.HasStarted)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var (status, friendlyMessage) = TranslateException(ex);
            var isApi = context.Request.Path.StartsWithSegments("/api");

            if (isApi)
            {
                context.Response.StatusCode = (int)status;
                context.Response.ContentType = "application/json";
                var payload = new { message = friendlyMessage };
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            else
            {
                context.Response.Redirect("/Error");
            }
        }

        private static (HttpStatusCode status, string message) TranslateException(Exception ex)
        {
            // Walk the inner exception chain for a PostgresException
            var pg = FindInner<PostgresException>(ex);
            if (pg != null)
            {
                switch (pg.SqlState)
                {
                    case "23505": // unique_violation
                        return (HttpStatusCode.Conflict,
                            "This record already exists. Please check your input and try again.");
                    case "23503": // foreign_key_violation
                        return (HttpStatusCode.Conflict,
                            "This item is still being used elsewhere. Remove the related records first.");
                    case "23502": // not_null_violation
                        return (HttpStatusCode.BadRequest,
                            "A required field is missing. Please fill out all required fields.");
                }
            }

            if (FindInner<DbUpdateException>(ex) != null)
                return (HttpStatusCode.BadRequest,
                    "We couldn't save your changes. Please check the form and try again.");

            if (ex is UnauthorizedAccessException)
                return (HttpStatusCode.Unauthorized, "You're not signed in, or your session has expired.");

            if (ex is KeyNotFoundException)
                return (HttpStatusCode.NotFound, "We couldn't find what you were looking for.");

            if (ex is TimeoutException)
                return (HttpStatusCode.RequestTimeout,
                    "The server took too long to respond. Please try again in a moment.");

            return (HttpStatusCode.InternalServerError,
                "Something went wrong on our end. Please try again, and contact support if it continues.");
        }

        private static T? FindInner<T>(Exception? ex) where T : Exception
        {
            while (ex != null)
            {
                if (ex is T match) return match;
                ex = ex.InnerException;
            }
            return null;
        }
    }
}
