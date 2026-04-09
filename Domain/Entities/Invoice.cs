using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Invoice entity for technician subscriptions.
    /// Generated automatically after the 3-month trial period ends.
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }

        /// <summary>Unique invoice number (e.g., INV-2026-00001)</summary>
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>Technician ID this invoice belongs to</summary>
        [Required]
        public string TechnicianId { get; set; } = string.Empty;

        /// <summary>Invoice generation date</summary>
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        /// <summary>Payment due date (typically 30 days from invoice date)</summary>
        public DateTime DueDate { get; set; }

        /// <summary>Billing period start date</summary>
        public DateTime BillingPeriodStart { get; set; }

        /// <summary>Billing period end date</summary>
        public DateTime BillingPeriodEnd { get; set; }

        /// <summary>Subscription amount (ALL)</summary>
        public decimal Amount { get; set; }

        /// <summary>Tax amount if applicable</summary>
        public decimal TaxAmount { get; set; }

        /// <summary>Total amount including tax</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Invoice status</summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        /// <summary>Date payment was received</summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>Payment method used</summary>
        public string? PaymentMethod { get; set; }

        /// <summary>Payment transaction reference</summary>
        public string? PaymentReference { get; set; }

        /// <summary>Whether email notification was sent</summary>
        public bool EmailSent { get; set; }

        /// <summary>Date email was sent</summary>
        public DateTime? EmailSentDate { get; set; }

        /// <summary>Invoice description/notes</summary>
        public string? Description { get; set; }

        /// <summary>Admin notes</summary>
        public string? AdminNotes { get; set; }

        /// <summary>Creation timestamp</summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>Last modification timestamp</summary>
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ApplicationUser Technician { get; set; } = null!;
    }
}
