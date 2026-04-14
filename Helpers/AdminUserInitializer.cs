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

                // Check if admin user already exists
                if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
                {
                    // Get ADMIN role
                    var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
                    if (adminRole == null)
                    {
                        logger.LogError("ADMIN role not found. Make sure roles are seeded first.");
                        return;
                    }

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
                    logger.LogInformation("Admin user already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error initializing admin user: {ex.Message}");
            }
        }
    }
}
