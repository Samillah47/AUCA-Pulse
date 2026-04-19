using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AUCAPulse.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp);
        Task SendApprovalEmailAsync(string toEmail, string name, string role, string department);
        Task SendRejectionEmailAsync(string toEmail, string name, string reason);
        Task SendPasswordResetEmailAsync(string toEmail, string name, string token);
        Task SendPasswordResetRejectionEmailAsync(string toEmail, string name, string reason);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var senderName = emailSettings["SenderName"];

                var allowFallback = bool.TryParse(emailSettings["AllowDevConsoleOtpFallback"], out var parsed)
                    ? parsed
                    : true;

                var hasPlaceholderValues =
                    string.IsNullOrWhiteSpace(smtpServer) ||
                    string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(senderPassword) ||
                    smtpServer.Contains("your-", StringComparison.OrdinalIgnoreCase) ||
                    senderEmail.Contains("your-", StringComparison.OrdinalIgnoreCase) ||
                    senderPassword.Contains("your-", StringComparison.OrdinalIgnoreCase);

                // In local/dev setups, allow login flow to continue and rely on console OTP output.
                if (hasPlaceholderValues && allowFallback)
                {
                    _logger.LogWarning("EmailSettings are not fully configured. Skipping SMTP send and using console OTP fallback.");
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            // Log OTP to console for development/testing
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("📧 OTP EMAIL");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"OTP Code: {otp}");
            Console.WriteLine("Expires: 5 minutes");
            Console.WriteLine(new string('=', 50) + "\n");

            var subject = "Your Login OTP Code - AUCA Pulse";
            var body = $@"Dear User,

Your OTP code for login is: {otp}

This code will expire in 5 minutes.

If you did not request this code, please ignore this email.

Best regards,
AUCA Pulse Team
Adventist University of Central Africa";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendApprovalEmailAsync(string toEmail, string name, string role, string department)
        {
            var subject = "ACPulse Account Approved - Welcome to the System";
            var body = $@"Dear {name},

We are pleased to inform you that your ACPulse account has been successfully approved.

Account Details:
- Name: {name}
- Email: {toEmail}
- Role: {role}
- Department: {department ?? "Not specified"}

Next Steps:
1. Visit the ACPulse portal
2. Log in using your registered email and password
3. Complete your profile setup if required

If you have any questions or need assistance, please contact our support team.

Best regards,
ACPulse System Administration
Adventist University of Central Africa";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendRejectionEmailAsync(string toEmail, string name, string reason)
        {
            var subject = "ACPulse Registration - Action Required";
            var body = $@"Dear {name},

Thank you for your interest in ACPulse.

Unfortunately, we could not approve your registration at this time.

Reason: {reason}

Please contact the admin office for assistance or re-submit your registration with the correct information.

Contact: admin@auca.ac.rw

Best regards,
ACPulse Team
Adventist University of Central Africa";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string name, string token)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5204/api";
            // Strip "/api" suffix if present to get the site root
            var siteRoot = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? baseUrl[..^4]
                : baseUrl;
            var resetUrl = $"{siteRoot}/ResetPassword?token={Uri.EscapeDataString(token)}";
            var subject = "AUCA Pulse - Reset your password";
            var body = $@"Hi {name},

We received a request to reset your AUCA Pulse password.

Click the link below to choose a new password:
{resetUrl}

This link will expire in 30 minutes and can only be used once.

If you didn't request a password reset, you can safely ignore this email — your password will not be changed.

Best regards,
The AUCA Pulse Team";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetRejectionEmailAsync(string toEmail, string name, string reason)
        {
            var subject = "ACPulse - Password Reset Request Update";
            var body = $@"Dear {name},

We regret to inform you that your recent password reset request has been reviewed and cannot be processed at this time.

Reason: {reason}

If you believe this is an error or need further assistance, please contact our support team or visit the help desk.

For security reasons, you may submit a new password reset request if needed.

Best regards,
ACPulse System Administration
Adventist University of Central Africa";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
