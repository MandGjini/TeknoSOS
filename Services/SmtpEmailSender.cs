using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TeknoSOS.WebApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailAsync(email, subject, htmlMessage, true);
        }

        public async Task SendEmailAsync(string email, string subject, string message, bool isBodyHtml = true)
        {
            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var from = smtpSection["From"] ?? username;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };
            var mail = new MailMessage(from!, email, subject, message)
            {
                IsBodyHtml = isBodyHtml
            };
            await client.SendMailAsync(mail);
        }

        public async Task SendEmailWithAttachmentAsync(
            string email,
            string subject,
            string message,
            byte[] attachmentContent,
            string attachmentFileName,
            string contentType = "application/pdf",
            bool isBodyHtml = true)
        {
            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var from = smtpSection["From"] ?? username;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            using var mail = new MailMessage(from!, email, subject, message)
            {
                IsBodyHtml = isBodyHtml
            };

            using var stream = new MemoryStream(attachmentContent);
            var attachment = new Attachment(stream, attachmentFileName, contentType);
            mail.Attachments.Add(attachment);

            await client.SendMailAsync(mail);
        }

        // Temporary method for testing SMTP email sending
        public async Task SendTestEmailAsync()
        {
            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var from = smtpSection["From"] ?? username;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };
            var mail = new MailMessage(from!, "armandogjini95@gmail.com", "Test Email from TeknoSOS", "This is a test email sent from TeknoSOS SMTP integration.")
            {
                IsBodyHtml = false
            };
            await client.SendMailAsync(mail);
        }
    }
}
