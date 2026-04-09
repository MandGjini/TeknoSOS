using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class TechnicianSubscription
    {
        public int Id { get; set; }

        [Required]
        public string TechnicianId { get; set; } = string.Empty;

        /// <summary>Period start date</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Period end date</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Amount paid or due (ALL)</summary>
        public decimal Amount { get; set; }

        /// <summary>Payment method: Manual, Stripe, PayPal, Paysera, BankTransfer</summary>
        public string PaymentMethod { get; set; } = "Manual";

        /// <summary>External transaction reference</summary>
        public string? TransactionId { get; set; }

        /// <summary>Date admin confirmed or payment gateway processed</summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>True for the initial 30-day free trial period</summary>
        public bool IsTrialPeriod { get; set; }

        /// <summary>True if admin has confirmed this payment</summary>
        public bool IsConfirmed { get; set; }

        /// <summary>Admin notes about this subscription/payment</summary>
        public string? AdminNotes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser Technician { get; set; } = null!;
    }
}
