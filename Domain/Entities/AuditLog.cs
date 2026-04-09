namespace TeknoSOS.WebApp.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Entity { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
