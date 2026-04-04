using Microsoft.EntityFrameworkCore;
using AUCAPulse.Models;

namespace AUCAPulse.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<LecturerStatus> LecturerStatuses { get; set; }
        public DbSet<LectureSchedule> LectureSchedules { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<VerificationRequest> VerificationRequests { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IdentificationNumber).IsUnique();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Users)
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Office)
                    .WithOne(o => o.StaffUser)
                    .HasForeignKey<Office>(o => o.StaffUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // Configure Location entity
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                
                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.HasOne(e => e.Parent)
                    .WithMany(l => l.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => e.RoomNumber).IsUnique();
                
                entity.Property(e => e.RoomType)
                    .HasConversion<string>();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.CurrentLecturer)
                    .WithMany(u => u.OccupiedRooms)
                    .HasForeignKey(e => e.CurrentLecturerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Office entity
            modelBuilder.Entity<Office>(entity =>
            {
                entity.HasIndex(e => e.OfficeNumber).IsUnique();
                
                entity.Property(e => e.AvailabilityStatus)
                    .HasConversion<string>();
            });

            // Configure LecturerStatus entity
            modelBuilder.Entity<LecturerStatus>(entity =>
            {
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Lecturer)
                    .WithMany(u => u.LecturerStatuses)
                    .HasForeignKey(e => e.LecturerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LectureSchedule entity
            modelBuilder.Entity<LectureSchedule>(entity =>
            {
                entity.HasOne(e => e.Lecturer)
                    .WithMany(u => u.LectureSchedules)
                    .HasForeignKey(e => e.LecturerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Semester)
                    .WithMany(s => s.LectureSchedules)
                    .HasForeignKey(e => e.SemesterId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure VerificationRequest entity
            modelBuilder.Entity<VerificationRequest>(entity =>
            {
                entity.Property(e => e.RequestType)
                    .HasConversion<string>();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.VerificationRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PasswordResetRequest entity
            modelBuilder.Entity<PasswordResetRequest>(entity =>
            {
                entity.HasIndex(e => e.Token).IsUnique();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.PasswordResetRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "STUDENT", Description = "Student user with search and view permissions" },
                new Role { Id = 2, RoleName = "LECTURER", Description = "Lecturer with room occupation and status update permissions" },
                new Role { Id = 3, RoleName = "STAFF", Description = "Staff member with office management permissions" },
                new Role { Id = 4, RoleName = "ADMIN", Description = "Administrator with full system access" }
            );

            // Seed Locations (Rwanda provinces)
            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, Name = "Kigali", Code = "KGL", Type = LocationType.PROVINCE, CreatedAt = seedDate },
                new Location { Id = 2, Name = "Eastern Province", Code = "EST", Type = LocationType.PROVINCE, CreatedAt = seedDate },
                new Location { Id = 3, Name = "Northern Province", Code = "NTH", Type = LocationType.PROVINCE, CreatedAt = seedDate },
                new Location { Id = 4, Name = "Southern Province", Code = "STH", Type = LocationType.PROVINCE, CreatedAt = seedDate },
                new Location { Id = 5, Name = "Western Province", Code = "WST", Type = LocationType.PROVINCE, CreatedAt = seedDate },
                // Kigali districts
                new Location { Id = 6, Name = "Gasabo", Code = "GSB", Type = LocationType.DISTRICT, ParentId = 1, CreatedAt = seedDate },
                new Location { Id = 7, Name = "Kicukiro", Code = "KCK", Type = LocationType.DISTRICT, ParentId = 1, CreatedAt = seedDate },
                new Location { Id = 8, Name = "Nyarugenge", Code = "NYR", Type = LocationType.DISTRICT, ParentId = 1, CreatedAt = seedDate }
            );

            // Seed Rooms
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, RoomNumber = "A-101", RoomName = "Lecture Hall 1", Capacity = 100, Building = "Academic Block A", Floor = "1st Floor", RoomType = RoomType.LECTURE_HALL, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate },
                new Room { Id = 2, RoomNumber = "A-102", RoomName = "Lecture Hall 2", Capacity = 100, Building = "Academic Block A", Floor = "1st Floor", RoomType = RoomType.LECTURE_HALL, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate },
                new Room { Id = 3, RoomNumber = "A-204", RoomName = "Computer Lab 1", Capacity = 40, Building = "Academic Block A", Floor = "2nd Floor", RoomType = RoomType.LAB, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate },
                new Room { Id = 4, RoomNumber = "A-205", RoomName = "Computer Lab 2", Capacity = 40, Building = "Academic Block A", Floor = "2nd Floor", RoomType = RoomType.LAB, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate },
                new Room { Id = 5, RoomNumber = "B-101", RoomName = "Science Lab", Capacity = 30, Building = "Academic Block B", Floor = "1st Floor", RoomType = RoomType.LAB, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate },
                new Room { Id = 6, RoomNumber = "B-201", RoomName = "Conference Room", Capacity = 50, Building = "Academic Block B", Floor = "2nd Floor", RoomType = RoomType.MEETING_ROOM, Status = RoomStatus.AVAILABLE, CreatedAt = seedDate }
            );

            // Seed Offices
            modelBuilder.Entity<Office>().HasData(
                new Office { Id = 1, OfficeNumber = "Admin-101", OfficeName = "Registrar Office", Building = "Admin Building", Floor = "1st Floor", AvailabilityStatus = AvailabilityStatus.CLOSED, RegularOpenTime = new TimeSpan(8, 0, 0), RegularCloseTime = new TimeSpan(17, 0, 0), CreatedAt = seedDate },
                new Office { Id = 2, OfficeNumber = "Admin-102", OfficeName = "Finance Office", Building = "Admin Building", Floor = "1st Floor", AvailabilityStatus = AvailabilityStatus.CLOSED, RegularOpenTime = new TimeSpan(8, 0, 0), RegularCloseTime = new TimeSpan(17, 0, 0), CreatedAt = seedDate },
                new Office { Id = 3, OfficeNumber = "Admin-201", OfficeName = "Student Affairs", Building = "Admin Building", Floor = "2nd Floor", AvailabilityStatus = AvailabilityStatus.CLOSED, RegularOpenTime = new TimeSpan(8, 0, 0), RegularCloseTime = new TimeSpan(17, 0, 0), CreatedAt = seedDate },
                new Office { Id = 4, OfficeNumber = "Admin-202", OfficeName = "Academic Affairs", Building = "Admin Building", Floor = "2nd Floor", AvailabilityStatus = AvailabilityStatus.CLOSED, RegularOpenTime = new TimeSpan(8, 0, 0), RegularCloseTime = new TimeSpan(17, 0, 0), CreatedAt = seedDate }
            );

            // Seed Semester
            modelBuilder.Entity<Semester>().HasData(
                new Semester { Id = 1, Name = "Fall 2024/2025", StartDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc), IsCurrent = true, CreatedAt = seedDate }
            );
        }
    }
}
