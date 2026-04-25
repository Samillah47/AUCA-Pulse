using AUCAPulse.Data;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Helpers
{
    public class AdminUserInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminUserInitializer>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                var adminEmail = configuration["AdminUser:Email"];
                var adminPassword = configuration["AdminUser:Password"];
                var adminName = configuration["AdminUser:Name"] ?? "System Admin";
                var adminIdentification = configuration["AdminUser:IdentificationNumber"] ?? "ADMIN001";

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                {
                    logger.LogError("AdminUser:Email or AdminUser:Password not configured in appsettings. Skipping admin initialization.");
                    return;
                }

                // Get ADMIN role (always — we may need it whether we create or repair)
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
                if (adminRole == null)
                {
                    logger.LogError("ADMIN role not found. Make sure roles are seeded first.");
                    return;
                }

                var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

                if (existing == null)
                {
                    // Create admin user with BCrypt hashed password
                    var admin = new User
                    {
                        Name = adminName,
                        Email = adminEmail,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                        IdentificationNumber = adminIdentification,
                        Status = UserStatus.APPROVED,
                        RoleId = adminRole.Id,
                        Department = "Administration",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(admin);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Admin user created successfully for email: {Email}", adminEmail);
                }
                else
                {
                    // Self-heal: keep the seeded admin reachable across redeploys
                    // even if migrations or earlier startup attempts left them in
                    // a half-configured state. Cheap to verify, costs nothing if
                    // it's already correct.
                    var changed = false;
                    if (!BCrypt.Net.BCrypt.Verify(adminPassword, existing.PasswordHash))
                    {
                        existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                        changed = true;
                        logger.LogWarning("Admin password did not match configuration; resetting it.");
                    }
                    if (existing.Status != UserStatus.APPROVED)
                    {
                        existing.Status = UserStatus.APPROVED;
                        changed = true;
                        logger.LogWarning("Admin status was not APPROVED; setting it.");
                    }
                    if (existing.RoleId != adminRole.Id)
                    {
                        existing.RoleId = adminRole.Id;
                        changed = true;
                        logger.LogWarning("Admin role was not ADMIN; setting it.");
                    }
                    if (changed)
                    {
                        existing.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                        logger.LogInformation("Admin user repaired for email: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogInformation("Admin user already exists and matches configuration.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error initializing admin user: {ex.Message}");
            }
        }
    }
}
