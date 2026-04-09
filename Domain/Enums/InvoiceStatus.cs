namespace TeknoSOS.WebApp.Domain.Enums
{
    /// <summary>
    /// Status of an invoice
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Invoice created, awaiting payment</summary>
        Pending = 0,

        /// <summary>Payment received and confirmed</summary>
        Paid = 1,

        /// <summary>Invoice overdue (past due date)</summary>
        Overdue = 2,

        /// <summary>Invoice cancelled by admin</summary>
        Cancelled = 3,

        /// <summary>Invoice refunded</summary>
        Refunded = 4
    }
}
