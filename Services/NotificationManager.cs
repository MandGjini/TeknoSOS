using System.Threading.Tasks;
using TeknoSOS.WebApp.Models;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Services
{
    public class NotificationManager
    {
        private readonly EmailService _emailService;

        public NotificationManager(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task OnUserRegistered(User user)
            => await _emailService.SendWelcomeEmail(user);

        public async Task OnTechnicianRegistered(Technician tech)
            => await _emailService.SendTechnicianCertificateRequest(tech);

        public async Task OnTechnicianVerified(Technician tech)
            => await _emailService.SendTechnicianActivated(tech);

        public async Task OnTechnicianDeactivated(Technician tech)
            => await _emailService.SendTechnicianDeactivated(tech);

        public async Task OnSubscriptionExpired(User user)
            => await _emailService.SendSubscriptionExpired(user);
    }
}
