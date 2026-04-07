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

            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                // Check if admin user already exists
                if (!await context.Users.AnyAsync(u => u.Email == "habiyaadolphe19@gmail.com"))
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
                        Name = "System Admin",
                        Email = "habiyaadolphe19@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mugisha1234!@"),
                        IdentificationNumber = "ADMIN001",
                        Status = UserStatus.APPROVED,
                        RoleId = adminRole.Id,
                        Department = "Administration",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(admin);
                    await context.SaveChangesAsync();

                    logger.LogInformation("✅ Admin user created successfully!");
                    logger.LogInformation("   Email: habiyaadolphe19@gmail.com");
                    logger.LogInformation("   Password: Mugisha1234!@");
                }
                else
                {
                    logger.LogInformation("ℹ️  Admin user already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error initializing admin user: {ex.Message}");
            }
        }
    }
}
