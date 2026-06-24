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
            // Bọc trong Task.Run để giải phóng luồng chính ngay lập tức
            _ = Task.Run(async () =>
            {
                try
                {
                    string senderName = _config["EmailSettings:SenderName"] ?? "No Reply";
                    string senderEmail = _config["EmailSettings:SenderEmail"] ?? throw new Exception("Config Error");
                    string smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                    int smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                    string senderPassword = _config["EmailSettings:SenderPassword"] ?? throw new Exception("Config Error");

                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress(senderName, senderEmail));
                    email.To.Add(MailboxAddress.Parse(toEmail));
                    email.Subject = subject;
                    email.Body = new TextPart(TextFormat.Html) { Text = body };

                    using var smtp = new SmtpClient();
                    // Timeout 10 giây để tránh treo thread
                    await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(senderEmail, senderPassword);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    // Log lỗi vào Console thay vì để App văng lỗi 500 cho khách
                    Console.WriteLine($"[EMAIL ERROR]: {ex.Message}");
                }
            });
        }
    }
}
