using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using System.Text;

namespace TeknoSOS.WebApp.Services
{
    /// <summary>
    /// Service for generating and managing technician invoices.
    /// Automatically generates invoices after the 3-month trial period ends.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>Generate invoice for a technician</summary>
        Task<Invoice> GenerateInvoiceAsync(string technicianId, decimal amount, string? description = null);

        /// <summary>Get all invoices for a technician</summary>
        Task<List<Invoice>> GetTechnicianInvoicesAsync(string technicianId);

        /// <summary>Get invoice by ID</summary>
        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);

        /// <summary>Mark invoice as paid</summary>
        Task<bool> MarkAsPaidAsync(int invoiceId, string paymentMethod, string? paymentReference = null);

        /// <summary>Send invoice email to technician</summary>
        Task<bool> SendInvoiceEmailAsync(int invoiceId);

        /// <summary>Check and generate invoices for technicians whose trial has ended</summary>
        Task<int> ProcessTrialExpirationInvoicesAsync();

        /// <summary>Get all pending invoices</summary>
        Task<List<Invoice>> GetPendingInvoicesAsync();

        /// <summary>Get overdue invoices</summary>
        Task<List<Invoice>> GetOverdueInvoicesAsync();
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly ILogger<InvoiceService> _logger;

        // Default monthly subscription amount in Albanian Lek (ALL)
        private const decimal DefaultSubscriptionAmount = 1999m;
        private const decimal TaxRate = 0m;

        public InvoiceService(
            ApplicationDbContext db,
            IEmailSender emailSender,
            IInvoicePdfService invoicePdfService,
            ILogger<InvoiceService> logger)
        {
            _db = db;
            _emailSender = emailSender;
            _invoicePdfService = invoicePdfService;
            _logger = logger;
        }

        public async Task<Invoice> GenerateInvoiceAsync(string technicianId, decimal amount, string? description = null)
        {
            var technician = await _db.Users.FindAsync(technicianId);
            if (technician == null || technician.Role != UserRole.Professional)
            {
                throw new InvalidOperationException("Invalid technician ID or user is not a professional");
            }

            // Generate unique invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync();

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                TechnicianId = technicianId,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(10),
                BillingPeriodStart = DateTime.UtcNow,
                BillingPeriodEnd = DateTime.UtcNow.AddMonths(1),
                Amount = amount,
                TaxAmount = amount * TaxRate,
                TotalAmount = amount * (1 + TaxRate),
                Status = InvoiceStatus.Pending,
                Description = description ?? "Abonimi vjetor TeknoSOS për teknikë profesionistë",
                CreatedDate = DateTime.UtcNow
            };

            _db.Set<Invoice>().Add(invoice);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Invoice {InvoiceNumber} generated for technician {TechnicianId}", 
                invoiceNumber, technicianId);

            return invoice;
        }

        public async Task<List<Invoice>> GetTechnicianInvoicesAsync(string technicianId)
        {
            return await _db.Set<Invoice>()
                .Where(i => i.TechnicianId == technicianId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _db.Set<Invoice>()
                .Include(i => i.Technician)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<bool> MarkAsPaidAsync(int invoiceId, string paymentMethod, string? paymentReference = null)
        {
            // Use transaction to prevent race conditions
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _db.Set<Invoice>().FindAsync(invoiceId);
                if (invoice == null) return false;

                // Prevent double payment
                if (invoice.Status == InvoiceStatus.Paid)
                {
                    _logger.LogWarning("Attempted to mark already paid invoice {InvoiceId} as paid again", invoiceId);
                    return false;
                }

                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
                invoice.PaymentMethod = paymentMethod;
                invoice.PaymentReference = paymentReference;
                invoice.ModifiedDate = DateTime.UtcNow;

                // Activate technician subscription
                var technician = await _db.Users.FindAsync(invoice.TechnicianId);
                if (technician != null)
                {
                    technician.SubscriptionEndDate = invoice.BillingPeriodEnd;
                    technician.IsActive = technician.IsProfileVerified; // Only activate if verified
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Invoice {InvoiceId} marked as paid via {PaymentMethod}", 
                    invoiceId, paymentMethod);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
                throw;
            }
        }

        public async Task<bool> SendInvoiceEmailAsync(int invoiceId)
        {
            var invoice = await _db.Set<Invoice>()
                .Include(i => i.Technician)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null || string.IsNullOrEmpty(invoice.Technician?.Email))
                return false;

            var emailBody = BuildInvoiceEmailBody(invoice);

            try
            {
                var pdf = _invoicePdfService.BuildTechnicianInvoicePdf(
                    invoice.Technician,
                    invoice.InvoiceNumber,
                    invoice.InvoiceDate,
                    invoice.DueDate,
                    invoice.TotalAmount);

                if (_emailSender is SmtpEmailSender smtpSender)
                {
                    await smtpSender.SendEmailWithAttachmentAsync(
                        invoice.Technician.Email,
                        $"Fatura TeknoSOS #{invoice.InvoiceNumber}",
                        emailBody,
                        pdf,
                        $"{invoice.InvoiceNumber}.pdf",
                        "application/pdf",
                        true);
                }
                else
                {
                    await _emailSender.SendEmailAsync(
                        invoice.Technician.Email,
                        $"Fatura TeknoSOS #{invoice.InvoiceNumber}",
                        emailBody
                    );
                }

                invoice.EmailSent = true;
                invoice.EmailSentDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Invoice email sent for {InvoiceNumber} to {Email}", 
                    invoice.InvoiceNumber, invoice.Technician.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email for {InvoiceNumber}", invoice.InvoiceNumber);
                return false;
            }
        }

        public async Task<int> ProcessTrialExpirationInvoicesAsync()
        {
            // Find technicians whose trial has expired and don't have an active subscription or pending invoice
            var expiredTrialTechnicians = await _db.Users
                .Where(u => u.Role == UserRole.Professional &&
                           !u.SubscriptionEndDate.HasValue && // No active subscription
                           u.RegistrationDate.AddDays(90) < DateTime.UtcNow) // Trial expired (3 months)
                .ToListAsync();

            int invoicesGenerated = 0;

            foreach (var technician in expiredTrialTechnicians)
            {
                // Use transaction to prevent race condition
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Re-check if there's already a pending invoice for this technician
                    var hasPendingInvoice = await _db.Set<Invoice>()
                        .AnyAsync(i => i.TechnicianId == technician.Id && 
                                      (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue));

                    if (!hasPendingInvoice)
                    {
                        var invoice = await GenerateInvoiceAsync(
                            technician.Id, 
                            DefaultSubscriptionAmount,
                            "Fatura mujore pas përfundimit të periudhës provë 3-mujore"
                        );

                        // Send invoice email
                        await SendInvoiceEmailAsync(invoice.Id);

                        // Restrict technician access without hiding the account from admin listings.
                        technician.IsActive = true;
                        technician.IsAvailableForWork = false;
                        await _db.SaveChangesAsync();

                        invoicesGenerated++;
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error processing invoice for technician {TechnicianId}", technician.Id);
                }
            }

            if (invoicesGenerated > 0)
            {
                _logger.LogInformation("Generated {Count} invoices for expired trial technicians", invoicesGenerated);
            }

            return invoicesGenerated;
        }

        public async Task<List<Invoice>> GetPendingInvoicesAsync()
        {
            return await _db.Set<Invoice>()
                .Include(i => i.Technician)
                .Where(i => i.Status == InvoiceStatus.Pending)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        {
            var now = DateTime.UtcNow;

            // Update status to overdue for any pending invoices past due date
            var overdueInvoices = await _db.Set<Invoice>()
                .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate < now)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.ModifiedDate = now;
            }

            if (overdueInvoices.Any())
            {
                await _db.SaveChangesAsync();
            }

            return await _db.Set<Invoice>()
                .Include(i => i.Technician)
                .Where(i => i.Status == InvoiceStatus.Overdue)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastInvoice = await _db.Set<Invoice>()
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}"))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"INV-{year}-{nextNumber:D5}";
        }

        private string BuildInvoiceEmailBody(Invoice invoice)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body style='font-family: Arial, sans-serif;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            
            // Header
            sb.AppendLine("<div style='background: linear-gradient(135deg, #2563eb, #764ba2); padding: 20px; border-radius: 10px 10px 0 0;'>");
            sb.AppendLine("<h1 style='color: white; margin: 0;'>TeknoSOS</h1>");
            sb.AppendLine("<p style='color: rgba(255,255,255,0.9); margin: 5px 0 0 0;'>Fatura mujore për abonim</p>");
            sb.AppendLine("</div>");
            
            // Body
            sb.AppendLine("<div style='background: #f8fafc; padding: 20px; border: 1px solid #e2e8f0;'>");
            sb.AppendLine($"<p>I/E nderuar/a <strong>{invoice.Technician.GetFullName()}</strong>,</p>");
            sb.AppendLine("<p>Kjo është fatura juaj mujore. Për të vazhduar përdorimin e platformës TeknoSOS, ju lutem kryeni pagesën sipas detajeve më poshtë:</p>");
            
            // Invoice Details
            sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>Numri Faturës:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.InvoiceNumber}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>Data:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.InvoiceDate:dd/MM/yyyy}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>Afati Pagesës:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.DueDate:dd/MM/yyyy}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>Periudha:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.BillingPeriodStart:dd/MM/yyyy} - {invoice.BillingPeriodEnd:dd/MM/yyyy}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>Shuma:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.Amount:N0} ALL</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'><strong>TVSH:</strong></td><td style='padding: 10px; border-bottom: 1px solid #e2e8f0;'>{invoice.TaxAmount:N0} ALL</td></tr>");
            sb.AppendLine($"<tr style='background: #2563eb; color: white;'><td style='padding: 10px;'><strong>TOTALI:</strong></td><td style='padding: 10px;'><strong>{invoice.TotalAmount:N0} ALL</strong></td></tr>");
            sb.AppendLine("</table>");
            
            // Payment Info
            sb.AppendLine("<div style='background: #fff; padding: 15px; border-radius: 8px; border-left: 4px solid #2563eb;'>");
            sb.AppendLine("<h3 style='margin-top: 0; color: #2563eb;'>Mënyrat e Pagesës</h3>");
            sb.AppendLine("<p><strong>Transfer Bankare:</strong><br>Emri: TeknoSOS SH.P.K<br>IBAN: AL00 0000 0000 0000 0000 0000 0000<br>Referenca: " + invoice.InvoiceNumber + "</p>");
            sb.AppendLine("<p><strong>Numri Kontaktit:</strong> 00355693446516</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<p style='color: #64748b; font-size: 14px; margin-top: 20px;'><em>Pas kryerjes së pagesës, llogaria juaj do të aktivizohet brenda 24 orëve.</em></p>");
            sb.AppendLine("</div>");
            
            // Footer
            sb.AppendLine("<div style='background: #1e293b; color: white; padding: 20px; border-radius: 0 0 10px 10px; text-align: center;'>");
            sb.AppendLine("<p style='margin: 0;'>TeknoSOS - Platforma e Shërbimeve Teknike</p>");
            sb.AppendLine("<p style='margin: 5px 0 0 0; font-size: 12px; color: rgba(255,255,255,0.7);'>© 2026 TeknoSOS. Të gjitha të drejtat e rezervuara.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div></body></html>");
            
            return sb.ToString();
        }
    }
}
