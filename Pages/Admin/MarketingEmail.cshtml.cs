using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class MarketingEmailModel : PageModel
    {
        private const int MaxRecipientsPerSend = 500;

        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketingEmailModel(
            IEmailSender emailSender,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _emailSender = emailSender;
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "Subjekti është i detyrueshëm.")]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Përmbajtja është e detyrueshme.")]
        public string Body { get; set; } = string.Empty;

        [BindProperty]
        public bool IsBodyHtml { get; set; } = true;

        [BindProperty]
        public string RecipientsRaw { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? CsvFile { get; set; }

        public int ParsedCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> FailedRecipients { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostParseCsvAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ErrorMessage = "Ngarko një skedar CSV për të vazhduar.";
                return Page();
            }

            var csvContent = await ReadFileTextAsync(CsvFile);
            var emails = ParseEmails(csvContent);

            if (emails.Count == 0)
            {
                ErrorMessage = "Nuk u gjet asnjë email valid në CSV.";
                return Page();
            }

            if (emails.Count > MaxRecipientsPerSend)
            {
                ErrorMessage = $"Lista ka {emails.Count} email-e. Kufiri për një dërgim është {MaxRecipientsPerSend}.";
                return Page();
            }

            RecipientsRaw = string.Join(Environment.NewLine, emails);
            ParsedCount = emails.Count;
            SuccessMessage = $"CSV u lexua me sukses. U gjetën {emails.Count} email-e valide.";
            return Page();
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var recipients = ParseEmails(RecipientsRaw);

            if (!recipients.Any() && CsvFile != null && CsvFile.Length > 0)
            {
                var csvContent = await ReadFileTextAsync(CsvFile);
                recipients = ParseEmails(csvContent);
                RecipientsRaw = string.Join(Environment.NewLine, recipients);
            }

            if (!recipients.Any())
            {
                ErrorMessage = "Vendos listën e email-eve ose ngarko CSV me adresa valide.";
                return Page();
            }

            if (recipients.Count > MaxRecipientsPerSend)
            {
                ErrorMessage = $"Lista ka {recipients.Count} email-e. Kufiri për një dërgim është {MaxRecipientsPerSend}.";
                return Page();
            }

            var failures = new List<string>();
            var sent = 0;

            foreach (var email in recipients)
            {
                try
                {
                    if (_emailSender is SmtpEmailSender smtpSender)
                    {
                        await smtpSender.SendEmailAsync(email, Subject, Body, IsBodyHtml);
                    }
                    else
                    {
                        await _emailSender.SendEmailAsync(email, Subject, Body);
                    }

                    sent++;
                }
                catch (Exception ex)
                {
                    failures.Add($"{email} ({ex.Message})");
                }
            }

            SentCount = sent;
            FailedCount = failures.Count;
            FailedRecipients = failures.Take(20).ToList();

            if (sent > 0)
            {
                SuccessMessage = $"Fushata u dërgua: {sent} sukses, {failures.Count} dështime.";
            }

            if (failures.Count > 0)
            {
                ErrorMessage = "Disa email-e dështuan. Shiko listën e dështimeve më poshtë.";
            }

            await LogAuditAsync(recipients.Count, sent, failures.Count);
            return Page();
        }

        private static List<string> ParseEmails(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            var tokens = input
                .Split(new[] { '\n', '\r', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().Trim('"', '\''))
                .Where(t => !string.IsNullOrWhiteSpace(t));

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in tokens)
            {
                if (IsValidEmail(token))
                {
                    result.Add(token);
                }
            }

            return result.OrderBy(e => e).ToList();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> ReadFileTextAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync();
        }

        private async Task LogAuditAsync(int total, int sent, int failed)
        {
            var user = await _userManager.GetUserAsync(User);
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = user?.Id,
                Action = "MarketingEmailSend",
                Entity = "EmailCampaign",
                EntityId = null,
                NewValue = $"Subject={Subject}; Total={total}; Sent={sent}; Failed={failed}; IsHtml={IsBodyHtml}",
                CreatedDate = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });

            await _db.SaveChangesAsync();
        }
    }
}
