using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Hubs;

namespace TeknoSOS.WebApp.Services;

public interface IProfessionalOpportunityNotifier
{
    Task<int> NotifyNewOpportunityAsync(ServiceRequest serviceRequest, string source, CancellationToken cancellationToken = default);
}

public class ProfessionalOpportunityNotifier : IProfessionalOpportunityNotifier
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<ProfessionalOpportunityNotifier> _logger;

    public ProfessionalOpportunityNotifier(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushNotificationSender pushSender,
        IHubContext<NotificationHub> notificationHub,
        ILogger<ProfessionalOpportunityNotifier> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<int> NotifyNewOpportunityAsync(ServiceRequest serviceRequest, string source, CancellationToken cancellationToken = default)
    {
        var professionals = await _context.Users
            .Include(u => u.Specialties)
            .Where(u => u.IsActive && u.IsAvailableForWork && u.NotificationsEnabled)
            .Where(u => u.Specialties.Any(s => s.Category == serviceRequest.Category))
            .ToListAsync(cancellationToken);

        if (professionals.Count == 0)
        {
            var fallback = await _userManager.GetUsersInRoleAsync("Professional");
            professionals = fallback
                .Where(u => u.IsActive && u.NotificationsEnabled)
                .ToList();
        }

        if (professionals.Count == 0)
        {
            return 0;
        }

        var categoryText = GetCategoryText(serviceRequest.Category);
        var detailUrl = $"/DefectDetails?id={serviceRequest.Id}";
        var title = $"Punë e re te profesioni juaj: {categoryText}";

        var notifications = new List<Notification>();

        foreach (var professional in professionals)
        {
            var message =
                $"U publikua një punë e re ({serviceRequest.UniqueCode}) në kategorinë {categoryText}. " +
                "Ju ftojmë të dërgoni ofertën tuaj.";

            notifications.Add(new Notification
            {
                RecipientId = professional.Id,
                ServiceRequestId = serviceRequest.Id,
                Type = NotificationType.CaseCreated,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        for (var i = 0; i < professionals.Count; i++)
        {
            var professional = professionals[i];
            var dbNotification = notifications[i];
            var message = dbNotification.Message;

            try
            {
                await _notificationHub.Clients.Group($"user-{professional.Id}").SendAsync("ReceiveNotification", new
                {
                    id = dbNotification.Id,
                    title,
                    message,
                    type = (int)NotificationType.CaseCreated,
                    serviceRequestId = serviceRequest.Id,
                    category = serviceRequest.Category.ToString(),
                    linkUrl = detailUrl,
                    source,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed sending real-time notification to professional {ProfessionalId}", professional.Id);
            }

            if (professional.EmailNotificationsEnabled && !string.IsNullOrWhiteSpace(professional.Email))
            {
                var body = BuildOpportunityEmailBody(professional, serviceRequest, categoryText, detailUrl);
                try
                {
                    await _emailSender.SendEmailAsync(professional.Email!, title, body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed sending opportunity email to {Email}", professional.Email);
                }
            }

            if (!string.IsNullOrWhiteSpace(professional.PhoneNumber))
            {
                var sms = $"TeknoSOS: Pune e re {serviceRequest.UniqueCode} ({categoryText}). Hape app dhe dergo oferten tende.";
                await _smsSender.SendSmsAsync(professional.PhoneNumber!, sms, cancellationToken);
            }

            if (professional.PushNotificationsEnabled)
            {
                await _pushSender.SendToUserAsync(
                    professional,
                    title,
                    message,
                    new Dictionary<string, string>
                    {
                        ["serviceRequestId"] = serviceRequest.Id.ToString(),
                        ["uniqueCode"] = serviceRequest.UniqueCode,
                        ["category"] = serviceRequest.Category.ToString(),
                        ["linkUrl"] = detailUrl,
                        ["source"] = source
                    },
                    cancellationToken);
            }
        }

        return professionals.Count;
    }

    private static string BuildOpportunityEmailBody(
        ApplicationUser professional,
        ServiceRequest serviceRequest,
        string categoryText,
        string detailUrl)
    {
        var fullName = string.IsNullOrWhiteSpace(professional.FirstName)
            ? "Profesional"
            : professional.FirstName;

        return $@"<html><body style='font-family:Arial,sans-serif;line-height:1.6;'>
<h2 style='color:#2563eb;'>Punë e re në profesionin tuaj</h2>
<p>Përshëndetje {fullName},</p>
<p>U publikua një punë e re që përputhet me profesionin tuaj:</p>
<ul>
<li><strong>Kodi:</strong> {serviceRequest.UniqueCode}</li>
<li><strong>Kategoria:</strong> {categoryText}</li>
<li><strong>Titulli:</strong> {serviceRequest.Title}</li>
<li><strong>Lokacioni:</strong> {serviceRequest.Location}</li>
</ul>
<p>Ju ftojmë të dërgoni ofertën tuaj sa më shpejt.</p>
<p><a href='https://teknosos.app{detailUrl}' style='display:inline-block;background:#2563eb;color:#fff;padding:10px 16px;border-radius:6px;text-decoration:none;'>Shiko punën dhe dërgo ofertë</a></p>
<p style='color:#6b7280;'>— Ekipi TeknoSOS</p>
</body></html>";
    }

    private static string GetCategoryText(ServiceCategory category)
        => category switch
        {
            ServiceCategory.Plumbing => "Hidraulikë",
            ServiceCategory.Electrical => "Elektrike",
            ServiceCategory.HVAC => "Ngrohje/Ftohje",
            ServiceCategory.Carpentry => "Zdrukthtari",
            ServiceCategory.Appliance => "Pajisje",
            ServiceCategory.Mechanical => "Mekanike",
            ServiceCategory.ITTechnology => "IT",
            ServiceCategory.Gypsum => "Gips",
            ServiceCategory.Tiles => "Pllaka",
            ServiceCategory.Parquet => "Parket",
            ServiceCategory.TerraceInsulation => "Izolim Tarace",
            ServiceCategory.Architect => "Arkitekt",
            ServiceCategory.Engineer => "Inxhinier",
            _ => "Të përgjithshme"
        };
}
