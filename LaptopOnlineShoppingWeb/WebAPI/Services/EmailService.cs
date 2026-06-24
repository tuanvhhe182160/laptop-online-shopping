using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace WebAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            string senderName = _config["EmailSettings:SenderName"] ?? "No Reply";
            string senderEmail = _config["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("Sender Email is not configured.");
            string smtpHost = _config["EmailSettings:SmtpHost"] ?? throw new InvalidOperationException("SMTP Host is not configured.");
            string smtpPortStr = _config["EmailSettings:SmtpPort"] ?? "587";
            string senderPassword = _config["EmailSettings:SenderPassword"] ?? throw new InvalidOperationException("Sender Password is not configured.");

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            // Kết nối SMTP Gmail
            await smtp.ConnectAsync(smtpHost, int.Parse(smtpPortStr), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, senderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
