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

            logger.LogInformation("=== AdminUserInitializer starting ===");

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
                    // Self-heal: keep the seeded admin reachable across redeploys.
                    // BCrypt.Verify can throw on a malformed hash (e.g. empty string,
                    // or a hash created by a different library). Wrap it so a bad
                    // hash counts as "doesn't match" rather than a silent crash that
                    // leaves the admin unable to log in.
                    bool passwordOk;
                    try
                    {
                        passwordOk = !string.IsNullOrEmpty(existing.PasswordHash)
                                     && BCrypt.Net.BCrypt.Verify(adminPassword, existing.PasswordHash);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Admin password hash could not be verified; treating as mismatch.");
                        passwordOk = false;
                    }

                    var changed = false;
                    if (!passwordOk)
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
                        logger.LogWarning("=== Admin user REPAIRED for email: {Email} ===", adminEmail);
                    }
                    else
                    {
                        logger.LogInformation("=== Admin user OK for email: {Email} ===", adminEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "=== Error initializing admin user: {Message} ===", ex.Message);
            }
        }
    }
}
