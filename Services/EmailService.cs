using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TeknoSOS.WebApp.Models;

namespace TeknoSOS.WebApp.Services
{
    public class EmailService
    {
        private readonly SmtpClient _client;
        private readonly string _from = "noreply@teknosos.app";

        public EmailService()
        {
            _client = new SmtpClient("business177.web-hosting.com", 587)
            {
                Credentials = new NetworkCredential("noreply@teknosos.app", "Celine@021021!"),
                EnableSsl = true
            };
        }

        public async Task SendWelcomeEmail(User user)
            => await SendCustomEmail(user.Email ?? string.Empty, "Mirë se vini në TeknoSOS", EmailTemplates.WelcomeEmail(user));

        public async Task SendTechnicianCertificateRequest(Technician tech)
            => await SendCustomEmail(tech.Email ?? string.Empty, "Ngarko certifikatat për verifikim", EmailTemplates.TechnicianCertificateRequest(tech));

        public async Task SendTechnicianActivated(Technician tech)
            => await SendCustomEmail(tech.Email ?? string.Empty, "Llogaria u aktivizua", EmailTemplates.TechnicianActivated(tech));

        public async Task SendTechnicianDeactivated(Technician tech)
            => await SendCustomEmail(tech.Email ?? string.Empty, "Llogaria u çaktivizua", EmailTemplates.TechnicianDeactivated(tech));

        public async Task SendSubscriptionExpired(User user)
            => await SendCustomEmail(user.Email ?? string.Empty, "Abonimi ka skaduar", EmailTemplates.SubscriptionExpired(user));

        public async Task SendToTechniciansByProfession(Defect defect, List<Technician> technicians)
        {
            string subject = $"[TeknoSOS] Defekt i ri: {defect.Title}";
            string body = EmailTemplates.DefectCreated(defect);
            foreach (var tech in technicians)
            {
                await SendCustomEmail(tech.Email ?? string.Empty, subject, body);
            }
        }

        public async Task SendActionNotification(string to, string actionType, object data)
        {
            string subject = $"[TeknoSOS] {actionType}";
            string body = EmailTemplates.ActionNotification(actionType, data);
            await SendCustomEmail(to, subject, body);
        }

        public async Task SendCustomEmail(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                // Do not attempt to send email without a recipient
                return;
            }

            var mail = new MailMessage(_from, to, subject, htmlBody) { IsBodyHtml = true };
            await _client.SendMailAsync(mail);
        }
    }
}
