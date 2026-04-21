using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AUCAPulse.Data;
using AUCAPulse.Services;
using AUCAPulse.Helpers;
using AUCAPulse.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // Optional: make JSON more readable
        options.JsonSerializerOptions.WriteIndented = true;
        // Convert enums to strings
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json. Please add JwtSettings:SecretKey.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "AUCAPulse",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "AUCAPulseUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVerificationRequestService, VerificationRequestService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ILecturerStatusService, LecturerStatusService>();
builder.Services.AddScoped<ILectureScheduleService, LectureScheduleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordResetRequestService, PasswordResetRequestService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseAssignmentService, CourseAssignmentService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ITimetableGeneratorService, TimetableGeneratorService>();
builder.Services.AddScoped<ILecturerLocationService, LecturerLocationService>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Singleton: preserves the Round Robin pointer between requests across all users
builder.Services.AddSingleton<IRoundRobinRoomService, RoundRobinRoomService>();

// Background service: auto-releases rooms whose OccupiedUntil has passed
builder.Services.AddHostedService<RoomAutoReleaseService>();
builder.Services.AddScoped<JwtHelper>();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add session support with persistent cookies
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Session lasts 7 days
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AUCAPulse.Session";
    options.Cookie.MaxAge = TimeSpan.FromDays(7); // Cookie persists for 7 days
});

// Add distributed memory cache for session
builder.Services.AddDistributedMemoryCache();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize admin user
await AdminUserInitializer.InitializeAsync(app.Services);

// Global friendly exception handler (logs full error server-side, returns
// plain-English message to the client). Must come before UseRouting.
app.UseMiddleware<FriendlyExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Temporarily disabled for development
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Route protection for Razor Pages — must run after UseSession (reads role
// from session) and before MapRazorPages (redirects before the page executes).
app.UseMiddleware<RoleGuardMiddleware>();

app.MapRazorPages();
app.MapControllers(); // Map API controllers

app.Run();
