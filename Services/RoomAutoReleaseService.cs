using AUCAPulse.Data;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    /// <summary>
    /// Background service that periodically releases rooms whose OccupiedUntil
    /// timestamp has passed. Runs in the background for the lifetime of the app,
    /// independent of any HTTP request.
    ///
    /// Inherits from BackgroundService (IHostedService) — a standard .NET pattern
    /// for long-running background work. Uses IServiceScopeFactory to resolve a
    /// scoped ApplicationDbContext per tick, since this service itself is a Singleton.
    /// </summary>
    public class RoomAutoReleaseService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoomAutoReleaseService> _logger;
        private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

        public RoomAutoReleaseService(
            IServiceScopeFactory scopeFactory,
            ILogger<RoomAutoReleaseService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RoomAutoReleaseService started. Ticking every {Interval}.", TickInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var released = await ReleaseExpiredRoomsAsync(stoppingToken);
                    if (released > 0)
                    {
                        _logger.LogInformation("Auto-released {Count} expired room(s).", released);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected on shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during room auto-release tick.");
                }

                try
                {
                    await Task.Delay(TickInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("RoomAutoReleaseService stopping.");
        }

        private async Task<int> ReleaseExpiredRoomsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            var expired = await context.Rooms
                .Where(r => (r.Status == RoomStatus.OCCUPIED || r.Status == RoomStatus.RESERVED)
                         && r.OccupiedUntil != null
                         && r.OccupiedUntil < now)
                .ToListAsync(ct);

            if (!expired.Any()) return 0;

            foreach (var room in expired)
            {
                room.Status = RoomStatus.AVAILABLE;
                room.CurrentLecturerId = null;
                room.OccupiedAt = null;
                room.OccupiedUntil = null;
                room.UpdatedAt = now;
            }

            await context.SaveChangesAsync(ct);
            return expired.Count;
        }
    }
}
