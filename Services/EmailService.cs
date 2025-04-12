using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace dagstore.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmail(string email, string verificationLink)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer ayarı bulunamadı");
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException("SmtpPort ayarı bulunamadı"));
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? throw new ArgumentNullException("SmtpUsername ayarı bulunamadı");
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new ArgumentNullException("SmtpPassword ayarı bulunamadı");
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? throw new ArgumentNullException("FromEmail ayarı bulunamadı");
            var fromName = _configuration["EmailSettings:FromName"] ?? throw new ArgumentNullException("FromName ayarı bulunamadı");

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "E-posta Doğrulama",
                Body = $"Lütfen e-posta adresinizi doğrulamak için aşağıdaki bağlantıya tıklayın: {verificationLink}",
                IsBodyHtml = true
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
} 