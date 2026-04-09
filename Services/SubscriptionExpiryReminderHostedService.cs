using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Services;

public class SubscriptionExpiryReminderHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpiryReminderHostedService> _logger;
    private readonly HashSet<string> _sentToday = new(StringComparer.OrdinalIgnoreCase);

    public SubscriptionExpiryReminderHostedService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpiryReminderHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay to avoid interfering with app startup.
        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while processing subscription reminder job.");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var settings = scope.ServiceProvider.GetRequiredService<ISiteSettingsService>();

        var paymentSettings = await settings.GetGroupAsync("payments");
        var reminderEnabled = !paymentSettings.TryGetValue("SubscriptionReminderEnabled", out var enabledRaw)
            || !string.Equals(enabledRaw, "false", StringComparison.OrdinalIgnoreCase);

        if (!reminderEnabled)
        {
            return;
        }

        var renewalUrl = "https://teknosos.app/Account/SubscriptionRenewal";
        var supportEmail = paymentSettings.TryGetValue("BillingSupportEmail", out var support)
            ? support
            : "billing@teknosos.app";

        var today = DateTime.UtcNow.Date;
        var professionals = await db.Users
            .Where(u => u.Role == UserRole.Professional)
            .Where(u => u.IsActive)
            .Where(u => u.EmailNotificationsEnabled)
            .Where(u => !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync(stoppingToken);

        foreach (var user in professionals)
        {
            try
            {
                var endDate = (user.SubscriptionEndDate ?? user.RegistrationDate.AddDays(90)).Date;
                var daysRemaining = (endDate - today).Days;

                // Countdown reminders: 3, 2, 1 and day 0 (expired now).
                if (daysRemaining is < 0 or > 3)
                {
                    continue;
                }

                var dedupeKey = $"{today:yyyy-MM-dd}:{user.Id}:{daysRemaining}";
                if (_sentToday.Contains(dedupeKey))
                {
                    continue;
                }

                var subject = daysRemaining switch
                {
                    3 => "Rikujtim miqësor: Abonimi juaj skadon pas 3 ditësh",
                    2 => "Rikujtim: Abonimi juaj skadon pas 2 ditësh",
                    1 => "Urgjente: Abonimi juaj skadon nesër",
                    _ => "Shërbimi u ndërpre: Abonimi juaj ka skaduar"
                };

                var firstName = string.IsNullOrWhiteSpace(user.FirstName) ? "Përdorues" : user.FirstName;
                var statusLine = daysRemaining > 0
                    ? $"Abonimi juaj do të skadojë pas <strong>{daysRemaining}</strong> ditësh."
                    : "Abonimi juaj ka skaduar dhe shërbimi është ndërprerë automatikisht.";

                var body = $@"
<html>
<body style='font-family:Arial,sans-serif;line-height:1.6;color:#111827;'>
    <h2 style='color:#1d4ed8;'>Përshëndetje {firstName},</h2>
    <p>{statusLine}</p>
    <p>Për të vazhduar përdorimin e shërbimit pa ndërprerje, ju lutem kryeni pagesën në një nga format e pagesës në platformë.</p>

    <p>
        <a href='{renewalUrl}' style='display:inline-block;background:#1d4ed8;color:#fff;text-decoration:none;padding:10px 16px;border-radius:6px;'>
            Kryeji pagesën tani
        </a>
    </p>

    <p style='margin-top:12px;color:#6b7280;'>Nëse keni pyetje, na shkruani te {supportEmail}.</p>
    <p style='color:#6b7280;'>— Ekipi TeknoSOS</p>
</body>
</html>";

                await emailSender.SendEmailAsync(user.Email!, subject, body);
                _sentToday.Add(dedupeKey);
                _logger.LogInformation(
                    "Subscription reminder sent to user {UserId} ({Email}) with {DaysRemaining} days remaining.",
                    user.Id,
                    user.Email,
                    daysRemaining);

                // Keep a short trace in audit logs for operational visibility.
                db.AuditLogs.Add(new AuditLog
                {
                    UserId = user.Id,
                    Action = "SUBSCRIPTION_REMINDER_SENT",
                    Entity = "Subscription",
                    NewValue = $"Countdown={daysRemaining} day(s)",
                    CreatedDate = DateTime.UtcNow,
                    IpAddress = "system",
                    UserAgent = "background-service"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed sending subscription reminder for user {UserId}", user.Id);
            }
        }

        await db.SaveChangesAsync(stoppingToken);

        // Purge old in-memory markers every day to avoid growth.
        if (_sentToday.Count > 5000)
        {
            _sentToday.Clear();
        }
    }
}
