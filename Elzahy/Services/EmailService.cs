using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Elzahy.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendTwoFactorCodeAsync(string email, string code, string purpose);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendPasswordChangeNotificationAsync(string email, string name);
        Task SendAdminRequestNotificationAsync(string email, string name, bool approved, string? adminNotes);
        Task SendAdminCreatedUserEmailAsync(string email, string name, string tempPassword);
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

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var fromEmail = _configuration["Email:From"] ?? "noreply@elzahy.com";
                var fromName = _configuration["Email:FromName"] ?? "Elzahy Portfolio";

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                email.To.Add(new MailboxAddress("", to));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                    bodyBuilder.HtmlBody = body;
                else
                    bodyBuilder.TextBody = body;

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "";
                var smtpPass = _configuration["Email:SmtpPassword"] ?? "";

                _logger.LogInformation("Attempting to connect to SMTP server {Host}:{Port}", smtpHost, smtpPort);

                // Connect to Gmail SMTP with explicit TLS
                await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                
                if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogInformation("Authenticating with SMTP user: {User}", smtpUser);
                    await smtp.AuthenticateAsync(smtpUser, smtpPass);
                    _logger.LogInformation("SMTP authentication successful");
                }
                else
                {
                    _logger.LogWarning("SMTP credentials not provided, attempting to send without authentication");
                }
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} with subject: {Subject}. Error: {Error}", to, subject, ex.Message);
                throw;
            }
        }

        public async Task SendTwoFactorCodeAsync(string email, string code, string purpose)
        {
            var subject = $"Your {purpose} verification code";
            var body = $@"
                <html>
                <body>
                    <h2>Elzahy Portfolio - Verification Code</h2>
                    <p>Your verification code for {purpose.ToLower()} is:</p>
                    <h1 style='color: #2563eb; font-family: monospace; letter-spacing: 2px;'>{code}</h1>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't request this code, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Elzahy Portfolio - Password Reset</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Click the link below to reset your password:</p>
                    <a href='{resetLink}' style='background-color: #2563eb; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Email Address";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Elzahy Portfolio!</h2>
                    <p>Thank you for registering with us.</p>
                    <p>Please confirm your email address by clicking the link below:</p>
                    <a href='{confirmationLink}' style='background-color: #16a34a; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't create this account, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Welcome to Elzahy Portfolio!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome {name}!</h2>
                    <p>Your account has been successfully created and verified.</p>
                    <p>You can now access all features of the Elzahy Portfolio platform.</p>
                    <p>If you have any questions or need assistance, please don't hesitate to contact us.</p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordChangeNotificationAsync(string email, string name)
        {
            var subject = "Password Changed Successfully";
            var body = $@"
                <html>
                <body>
                    <h2>Password Changed</h2>
                    <p>Hello {name},</p>
                    <p>Your password has been successfully changed.</p>
                    <p>If you did not make this change, please contact us immediately and consider changing your password again.</p>
                    <p>For security reasons, this change was made on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.</p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAdminRequestNotificationAsync(string email, string name, bool approved, string? adminNotes)
        {
            var subject = approved ? "Admin Request Approved" : "Admin Request Denied";
            var statusText = approved ? "approved" : "denied";
            var statusColor = approved ? "#16a34a" : "#dc2626";
            
            var body = $@"
                <html>
                <body>
                    <h2 style='color: {statusColor};'>Admin Request {statusText.ToUpper()}</h2>
                    <p>Hello {name},</p>
                    <p>Your request for admin privileges has been <strong style='color: {statusColor};'>{statusText}</strong>.</p>";

            if (!string.IsNullOrEmpty(adminNotes))
            {
                body += $@"
                    <div style='background-color: #f3f4f6; padding: 15px; border-left: 4px solid {statusColor}; margin: 20px 0;'>
                        <h4>Admin Notes:</h4>
                        <p>{adminNotes}</p>
                    </div>";
            }

            if (approved)
            {
                body += @"
                    <p>You now have admin privileges and can access the admin panel.</p>
                    <p>Please use these privileges responsibly.</p>";
            }
            else
            {
                body += @"
                    <p>If you have any questions about this decision, please contact us.</p>";
            }

            body += @"
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAdminCreatedUserEmailAsync(string email, string name, string tempPassword)
        {
            var subject = "Your Account Has Been Created";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Elzahy Portfolio!</h2>
                    <p>Hello {name},</p>
                    <p>An admin has created an account for you. Here are your login credentials:</p>
                    <div style='background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Email:</strong> {email}</p>
                        <p><strong>Temporary Password:</strong> <code style='background-color: #e5e7eb; padding: 2px 6px; border-radius: 3px;'>{tempPassword}</code></p>
                    </div>
                    <p><strong>Important:</strong> Please change your password after your first login for security reasons.</p>
                    <p>You can log in at: <a href='{_configuration["App:FrontendUrl"]}/login'>Login Page</a></p>
                    <br>
                    <p>Best regards,<br>Elzahy Portfolio Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }
    }
}