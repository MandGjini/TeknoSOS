using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Services
{
    public class MonthlyBillingHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyBillingHostedService> _logger;

        public MonthlyBillingHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyBillingHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMonthlyBillingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gabim gjate faturimit mujor.");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task ProcessMonthlyBillingAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var settings = scope.ServiceProvider.GetRequiredService<ISiteSettingsService>();
            var smtpSender = scope.ServiceProvider.GetService<SmtpEmailSender>();
            var emailSender = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            var pdfService = scope.ServiceProvider.GetRequiredService<IInvoicePdfService>();

            var now = DateTime.UtcNow;

            // Dergimi kryesor ne diten 1 te muajit; nese serveri ishte offline, dergo ne 3 ditet e para me dedupe.
            if (now.Day > 3)
                return;

            var paymentSettings = await settings.GetGroupAsync("payments");
            var technicianAmount = ParseAmount(paymentSettings, "TechnicianMonthlyAmountALL", 1999m);
            var businessAmount = ParseAmount(paymentSettings, "BusinessMonthlyAmountALL", 3999m);

            await SendTechnicianInvoicesAsync(db, smtpSender, emailSender, pdfService, userManager, now, technicianAmount, cancellationToken);
            await SendBusinessInvoicesAsync(db, smtpSender, emailSender, pdfService, now, businessAmount, cancellationToken);
        }

        private async Task SendTechnicianInvoicesAsync(
            ApplicationDbContext db,
            SmtpEmailSender? smtpSender,
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
            IInvoicePdfService pdfService,
            UserManager<ApplicationUser> userManager,
            DateTime now,
            decimal amount,
            CancellationToken cancellationToken)
        {
            var professionals = await db.Users
                .Where(u => u.Role == UserRole.Professional && u.IsActive && !string.IsNullOrEmpty(u.Email))
                .ToListAsync(cancellationToken);

            foreach (var tech in professionals)
            {
                var dedupeKey = $"INVOICE:TECH:{tech.Id}:{now:yyyy-MM}";
                var alreadySent = await db.AuditLogs.AnyAsync(a => a.Action == "MONTHLY_INVOICE_EMAIL_SENT" && a.Entity == dedupeKey, cancellationToken);
                if (alreadySent)
                    continue;

                var invoiceNumber = $"INV-TECH-{now:yyyyMM}-{tech.Id[..Math.Min(6, tech.Id.Length)].ToUpperInvariant()}";
                var dueDate = new DateTime(now.Year, now.Month, 10);

                var pdf = pdfService.BuildTechnicianInvoicePdf(tech, invoiceNumber, now, dueDate, amount);
                var subject = $"Fatura mujore TeknoSOS - {invoiceNumber}";
                var body = BuildMailBody(tech.GetFullName(), invoiceNumber, amount, dueDate, true);

                try
                {
                    if (smtpSender != null)
                    {
                        await smtpSender.SendEmailWithAttachmentAsync(tech.Email!, subject, body, pdf, $"{invoiceNumber}.pdf", "application/pdf", true);
                    }
                    else
                    {
                        await emailSender.SendEmailAsync(tech.Email!, subject, body);
                    }

                    db.AuditLogs.Add(new AuditLog
                    {
                        UserId = tech.Id,
                        Action = "MONTHLY_INVOICE_EMAIL_SENT",
                        Entity = dedupeKey,
                        NewValue = $"AmountALL={amount:N0}; Invoice={invoiceNumber}",
                        CreatedDate = DateTime.UtcNow
                    });

                    _logger.LogInformation("Fatura mujore per teknikun {UserId} u dergua.", tech.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Deshtoi dergimi i fatures mujore per teknikun {UserId}", tech.Id);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task SendBusinessInvoicesAsync(
            ApplicationDbContext db,
            SmtpEmailSender? smtpSender,
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
            IInvoicePdfService pdfService,
            DateTime now,
            decimal amount,
            CancellationToken cancellationToken)
        {
            var businesses = await db.Businesses
                .Where(b => b.IsActive && b.VerificationStatus == BusinessVerificationStatus.Verified && !string.IsNullOrEmpty(b.ContactEmail))
                .ToListAsync(cancellationToken);

            foreach (var business in businesses)
            {
                var dedupeKey = $"INVOICE:BIZ:{business.Id}:{now:yyyy-MM}";
                var alreadySent = await db.AuditLogs.AnyAsync(a => a.Action == "MONTHLY_INVOICE_EMAIL_SENT" && a.Entity == dedupeKey, cancellationToken);
                if (alreadySent)
                    continue;

                var invoiceNumber = $"INV-BIZ-{now:yyyyMM}-{business.Id:D5}";
                var dueDate = new DateTime(now.Year, now.Month, 10);

                var pdf = pdfService.BuildBusinessInvoicePdf(business, invoiceNumber, now, dueDate, amount);
                var subject = $"Fatura mujore TeknoSOS - {invoiceNumber}";
                var body = BuildMailBody(business.Name, invoiceNumber, amount, dueDate, false);

                try
                {
                    if (smtpSender != null)
                    {
                        await smtpSender.SendEmailWithAttachmentAsync(business.ContactEmail!, subject, body, pdf, $"{invoiceNumber}.pdf", "application/pdf", true);
                    }
                    else
                    {
                        await emailSender.SendEmailAsync(business.ContactEmail!, subject, body);
                    }

                    db.AuditLogs.Add(new AuditLog
                    {
                        UserId = business.OwnerId,
                        Action = "MONTHLY_INVOICE_EMAIL_SENT",
                        Entity = dedupeKey,
                        NewValue = $"AmountALL={amount:N0}; Invoice={invoiceNumber}",
                        CreatedDate = DateTime.UtcNow
                    });

                    _logger.LogInformation("Fatura mujore per biznesin {BusinessId} u dergua.", business.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Deshtoi dergimi i fatures mujore per biznesin {BusinessId}", business.Id);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static string BuildMailBody(string recipientName, string invoiceNumber, decimal amountAll, DateTime dueDate, bool isTechnician)
        {
            var roleText = isTechnician ? "teknik" : "biznes";
            return $@"<html><body style='font-family:Arial,sans-serif;'>
<p>Pershendetje {recipientName},</p>
<p>Kjo eshte fatura mujore e abonimit tuaj si {roleText} ne platformen TeknoSOS.</p>
<ul>
<li><strong>Nr. fatures:</strong> {invoiceNumber}</li>
<li><strong>Shuma:</strong> {amountAll:N0} ALL</li>
<li><strong>Afati i pageses:</strong> {dueDate:dd/MM/yyyy}</li>
</ul>
<p>Faturen e plote ne PDF e gjeni bashkengjitur ne kete email.</p>
<p>Per version pa limit preventivash, kontaktoni administratorin e platformes.</p>
<p>Faleminderit,<br/>TeknoSOS</p>
</body></html>";
        }

        private static decimal ParseAmount(Dictionary<string, string> settings, string key, decimal defaultValue)
        {
            if (settings.TryGetValue(key, out var raw) && decimal.TryParse(raw, out var parsed) && parsed > 0)
                return parsed;

            return defaultValue;
        }
    }
}
